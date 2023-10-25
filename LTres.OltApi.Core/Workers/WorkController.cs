using LTres.OltApi.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LTres.OltApi.Core.Workers;

public class WorkController: IHostedService
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

        _workResponseReceiver.OnResponseReceived += DoOnResponseReceived;
    }

    public async Task StartAsync(CancellationToken cancellationToken) => await Task.Run(() => Start());

    public async Task StopAsync(CancellationToken cancellationToken) => await Task.Run(() => Stop());

    private void Start(bool autoRestart = true)
    {
        _log.LogDebug("Starting work controller..");
        if (autoRestart && _cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            Stop();

        _cancellationTokenSource = new CancellationTokenSource();
        _loopTask = Task.Run(ExecuteLoop, _cancellationTokenSource.Token);
        _loopTask.ContinueWith(k =>
        {
            if (k.IsFaulted)
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
            var workToBeDone = await _workListController.ToBeDone();
            if (workToBeDone.Any())
            {
                var quantity = workToBeDone.Count();

                _log.LogDebug($"Working to be done: {quantity}");
                _logCounter.AddCount("work sent", quantity);
            }

            foreach (var work in workToBeDone)
                _workExecutionDispatcher.Dispatch(work);

            await Task.Delay(1000);

            if (--toCleanUpCountdown <= 0)
            {
                toCleanUpCountdown = cleanUpInterval;

                if (lastCleanUpTask.IsCompleted || lastCleanUpTask.IsCanceled)
                {
                    lastCleanUpTask = Task.Run(async () =>
                    {
                        var removeStartedAt = DateTime.Now;
                        var removedCount = await _workCleanUp.CleanUpExecute();
                        var removedTimespan = DateTime.Now.Subtract(removeStartedAt);

                        if (removedCount > 0)
                        {
                            _log.LogDebug($"Removed {removedCount} history items");
                            _logCounter.AddCount("CleanUp Rem", removedCount > int.MaxValue ? int.MaxValue : (int)removedCount, removedTimespan);
                        }
                    });
                }
            }
        }
    }

    private void DoOnResponseReceived(object? sender, WorkerResponseReceivedEventArgs e)
    {
        Task.Run(async () =>
        {
            var startedTime = DateTime.Now;
            try
            {
                await _workResponseController.ResponseReceived(e.ProbeResponse);

                var elapsedTime = DateTime.Now.Subtract(startedTime);
                _log.LogDebug($"RESPONSE: {e.ProbeResponse}, saved in {elapsedTime}");
                _logCounter.AddSuccess(e.ProbeResponse.Id, "response", elapsedTime);
            }
            catch (Exception error)
            { 
                _logCounter.AddSuccess(e.ProbeResponse.Id, "response", DateTime.Now.Subtract(startedTime));
                _log.LogError(error.ToString()); 
            }
        });
    }
}
