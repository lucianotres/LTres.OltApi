using System.Collections.Concurrent;
using System.Diagnostics;
using LTres.Olt.Api.Common;
using LTres.Olt.Api.Common.DbServices;
using LTres.Olt.Api.Common.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LTres.Olt.Api.Core.Workers;

public class WorkController : IHostedService
{
    private const int cleanUpInterval = 60;

    private readonly ILogger _log;
    private readonly ILogCounter _logCounter;
    private readonly IWorkListController _workListController;
    private readonly IWorkerDispatcher _workExecutionDispatcher;
    private readonly IWorkerResponseReceiver _workResponseReceiver;
    private readonly IWorkResponseController _workResponseController;
    private readonly IDbWorkCleanUp _workCleanUp;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _loopTask = Task.CompletedTask;
    private Task lastCleanUpTask = Task.CompletedTask;
    private readonly ConcurrentQueue<WorkProbeResponse> _queueResponses;
    private Task? _lastResponseSavingProcessTask;

    public WorkController(ILogger<WorkController> logger,
        ILogCounter logCounter,
        IWorkListController workListController,
        IWorkerDispatcher workExecutionDispatcher,
        IWorkerResponseReceiver workerResponseReceiver,
        IWorkResponseController workResponseController,
        IDbWorkCleanUp workCleanUp)
    {
        _log = logger;
        _logCounter = logCounter;
        _workListController = workListController;
        _workExecutionDispatcher = workExecutionDispatcher;
        _workResponseReceiver = workerResponseReceiver;
        _workResponseController = workResponseController;
        _workCleanUp = workCleanUp;

        _queueResponses = new ConcurrentQueue<WorkProbeResponse>();
        logCounter.RegisterHookOnPrintResetAction<WorkController>(HookOnPrintResetAction);

        _workResponseReceiver.OnResponseReceived += DoOnResponseReceived;
    }

    private void HookOnPrintResetAction(ILogCounter counter)
    {
        var queueCount = _queueResponses.Count;
        if (queueCount > 0)
            counter.AddCount("to save queue", queueCount);
    }

    public Task StartAsync(CancellationToken cancellationToken) => Start(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Stop();
    
    private async Task Start(CancellationToken cancellationToken = default, bool autoRestart = true)
    {
        _log.LogDebug("Starting work controller..");
        if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            await Stop();

        if (cancellationToken.IsCancellationRequested)
            return;

        _cancellationTokenSource = new CancellationTokenSource();
        _loopTask = Task.Run(ExecuteLoop, _cancellationTokenSource.Token);
        _ = _loopTask.ContinueWith(task =>
        {
            if (task.IsFaulted && task.Exception != null)
                _log.LogError(task.Exception, "Work controller failed on execute loop.");
        }, TaskContinuationOptions.OnlyOnFaulted);

        _log.LogDebug("Work controller started.");
    }

    private Task Stop()
    {
        if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
            return Task.CompletedTask;

        _log.LogDebug("Stopping work controller..");

        try
        {
            _cancellationTokenSource.Cancel();
        }
        catch { }

        return _loopTask ?? Task.CompletedTask;
    }

    public bool IsRunning { get => _loopTask != null && !_loopTask.IsCompleted; }

    private async Task ExecuteLoop()
    {
        var toDispatchWorkCountdown = 0;
        var toCleanUpCountdown = cleanUpInterval * 100;

        while (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
        {
            if (--toDispatchWorkCountdown <= 0)
            {
                toCleanUpCountdown = 100;
                await VerifyAndDispatchWorkToBeDone();
            }

            await Task.Delay(10);

            if (--toCleanUpCountdown <= 0)
            {
                toCleanUpCountdown = cleanUpInterval;
                QueueAsyncCleanUp();
            }
        }

        _log.LogDebug("Work controller stopped.");
    }

    private async Task VerifyAndDispatchWorkToBeDone()
    {
        var workToBeDone = await _workListController.ToBeDone();
        if (!workToBeDone.Any())
            return;
        
        var quantity = workToBeDone.Count();
        _log.LogDebug($"Working to be done: {quantity}");
        _logCounter.AddCount("work sent", quantity);

        foreach (var work in workToBeDone)
            _workExecutionDispatcher.Dispatch(work);
    }

    private void QueueAsyncCleanUp()
    {
        if (!lastCleanUpTask.IsCompleted && !lastCleanUpTask.IsCanceled)
            return;
            
        lastCleanUpTask = Task.Run(async () =>
        {
            var removeTimer = Stopwatch.StartNew();
            var removedCount = await _workCleanUp.CleanUpExecute();
            removeTimer.Stop();

            if (removedCount > 0)
            {
                _log.LogDebug($"Removed {removedCount} history items");
                _logCounter.AddCount("CleanUp Rem", removedCount > int.MaxValue ? int.MaxValue : (int)removedCount, removeTimer.Elapsed);
            }
        });
    }

    private void DoOnResponseReceived(object? sender, WorkerResponseReceivedEventArgs e)
    {
        var probeResponse = e.ProbeResponse;
        if (probeResponse == null)
        {
            _log.LogWarning("Empty response received");
            return;
        }
        
        _queueResponses.Enqueue(probeResponse);
        QueueAsyncResponseSavingProcess();
    }

    private void QueueAsyncResponseSavingProcess()
    {
        if (_lastResponseSavingProcessTask != null && !_lastResponseSavingProcessTask.IsCompleted) 
            return;

        _lastResponseSavingProcessTask = Task.Run(async() =>
        {
            while (_queueResponses.TryDequeue(out var workProbeResponse))
                await SaveProbeResponse(workProbeResponse);
        });
    }

    private async Task SaveProbeResponse(WorkProbeResponse workProbeResponse)
    {
        var workResponseTimer = Stopwatch.StartNew();
        try
        {
            await _workResponseController.ResponseReceived(workProbeResponse);
            workResponseTimer.Stop();

            _log.LogDebug($"RESPONSE: {workProbeResponse}, saved in {workResponseTimer.Elapsed}");
            _logCounter.AddSuccess(workProbeResponse.Id, "response", workResponseTimer.Elapsed);
        }
        catch (Exception error)
        {
            try
            { workResponseTimer.Stop(); }
            catch
            { }
            _logCounter.AddSuccess(workProbeResponse.Id, "response", workResponseTimer.Elapsed);
            _log.LogError(error.ToString());
        }
    }
}
