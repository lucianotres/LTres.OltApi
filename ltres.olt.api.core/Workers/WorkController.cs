using System.Collections.Concurrent;
using System.Diagnostics;
using LTres.Olt.Api.Common;
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
    private Task? _queueResponseSavingProccessorTask;

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

    public async Task StartAsync(CancellationToken cancellationToken) => await Task.Run(() => Start()).ConfigureAwait(false);

    public async Task StopAsync(CancellationToken cancellationToken) => await Task.Run(() => Stop()).ConfigureAwait(false);

    private void Start(bool autoRestart = true)
    {
        _log.LogDebug("Starting work controller..");
        if (autoRestart && _cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            Stop();

        _cancellationTokenSource = new CancellationTokenSource();
        _loopTask = Task.Run(ExecuteLoop, _cancellationTokenSource.Token);
        _loopTask.ContinueWith(k =>
        {
            if (k.IsFaulted && (k.Exception != null))
                throw k.Exception;

            Stop();
        });

        _log.LogDebug("Work controller started.");
    }

    private void Stop()
    {
        if (_cancellationTokenSource == null)
            return;

        _log.LogDebug("Stopping work controller..");

        _cancellationTokenSource.Cancel();
        if (_loopTask != null)
            _loopTask.Wait();

        _cancellationTokenSource = null;
        _log.LogDebug("Work controller stopped.");
    }

    private async Task ExecuteLoop()
    {
        var toCleanUpCountdown = cleanUpInterval;

        while (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
        {
            var workToBeDone = await _workListController.ToBeDone().ConfigureAwait(false);
            if (workToBeDone.Any())
            {
                var quantity = workToBeDone.Count();

                _log.LogDebug($"Working to be done: {quantity}");
                _logCounter.AddCount("work sent", quantity);
            }

            foreach (var work in workToBeDone)
                _workExecutionDispatcher.Dispatch(work);

            await Task.Delay(1000).ConfigureAwait(false);

            if (--toCleanUpCountdown <= 0)
            {
                toCleanUpCountdown = cleanUpInterval;

                if (lastCleanUpTask.IsCompleted || lastCleanUpTask.IsCanceled)
                {
                    lastCleanUpTask = Task.Run(async () =>
                    {
                        var removeTimer = Stopwatch.StartNew();
                        var removedCount = await _workCleanUp.CleanUpExecute().ConfigureAwait(false);
                        removeTimer.Stop();

                        if (removedCount > 0)
                        {
                            _log.LogDebug($"Removed {removedCount} history items");
                            _logCounter.AddCount("CleanUp Rem", removedCount > int.MaxValue ? int.MaxValue : (int)removedCount, removeTimer.Elapsed);
                        }
                    });
                }
            }
        }
    }

    private void DoOnResponseReceived(object? sender, WorkerResponseReceivedEventArgs e)
    {
        var probeResponse = e.ProbeResponse;

        if (probeResponse == null)
            _log.LogWarning("Empty response received");
        else 
        {
            _queueResponses.Enqueue(probeResponse);
            StartQueueResponseSavingProccessor();
        }
    }

    private void StartQueueResponseSavingProccessor()
    {
        if (_queueResponseSavingProccessorTask != null && !_queueResponseSavingProccessorTask.IsCompleted)
            return;

        _queueResponseSavingProccessorTask = Task.Run(async() =>
        {
            while (_queueResponses.TryDequeue(out var workProbeResponse))
                await SaveProbeResponse(workProbeResponse).ConfigureAwait(false);
        });
    }

    private async Task SaveProbeResponse(WorkProbeResponse workProbeResponse)
    {
        var workResponseTimer = Stopwatch.StartNew();
        try
        {
            await _workResponseController.ResponseReceived(workProbeResponse).ConfigureAwait(false);
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
