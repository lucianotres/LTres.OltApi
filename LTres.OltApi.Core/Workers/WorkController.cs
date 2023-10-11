using LTres.OltApi.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LTres.OltApi.Core.Workers;

public class WorkController
{
    private readonly ILogger _log;
    private readonly IWorkListController _workListController;
    private readonly IWorkerDispatcher _workExecutionDispatcher;
    private readonly IWorkerResponseReceiver _workResponseReceiver;
    private readonly IWorkResponseController _workResponseController;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _loopTask = Task.CompletedTask;

    public WorkController(ILogger<WorkController> logger,
        IWorkListController workListController, 
        IWorkerDispatcher workExecutionDispatcher,
        IWorkerResponseReceiver workerResponseReceiver,
        IWorkResponseController workResponseController)
    {
        _log = logger;
        _workListController = workListController;
        _workExecutionDispatcher = workExecutionDispatcher;
        _workResponseReceiver = workerResponseReceiver;
        _workResponseController = workResponseController;

        _workResponseReceiver.OnResponseReceived += doOnResponseReceived;
    }

    public void Start(bool autoRestart = true)
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

    public void Stop()
    {
        _log.LogDebug("Stopping work controller..");
        if (_cancellationTokenSource == null)
            return;

        _cancellationTokenSource.Cancel();
        if (_loopTask != null)
            _loopTask.Wait();

        _cancellationTokenSource = null;
        _log.LogDebug("Work controller stopped.");
    }

    private async Task ExecuteLoop()
    {
        while (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
        {
            var workToBeDone = await _workListController.ToBeDone();
            _log.LogDebug($"Working to be done: {workToBeDone.Count()}");

            foreach(var work in workToBeDone)
                _workExecutionDispatcher.Dispatch(work);

            await Task.Delay(1000);
        }
    }

    private void doOnResponseReceived(object? sender, WorkerResponseReceivedEventArgs e)
    {
        _ = _workResponseController.ResponseReceived(e.ProbeResponse);
        _log.LogInformation($"RESPONSE: {e.ProbeResponse} -> {e.ProbeResponse.ValueInt}ms");
    }
}
