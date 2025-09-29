using System.Diagnostics;
using System.Net;
using LTres.Olt.Api.Common;
using LTres.Olt.Api.Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LTres.Olt.Api.Core.Workers;

/// <summary>
/// Process the requisition of work to probe data and activate the adequate action.
/// </summary>
public class WorkAction(ILogger<WorkAction> logger, IServiceProvider serviceProvider, ILogCounter counter) : IWorkerAction
{
    private readonly ILogger _log = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogCounter _logCounter = counter;

    public async Task<WorkProbeResponse> Execute(WorkProbeInfo probeInfo, CancellationToken cancellationToken, WorkProbeResponse? initialResponse = null)
    {
        _log.LogDebug($"Work probe received: {probeInfo.Id} -> {probeInfo.Action}");
        var workProbeResponse = new WorkProbeResponse() { Id = probeInfo.Id, Request = probeInfo };
        var timer = Stopwatch.StartNew();

        try
        {
            bool shouldUseMocking = probeInfo.Host.Address.Equals(IPAddress.None);
            IWorkerAction worker = (shouldUseMocking ?
                CreateMockWorkerByAction(probeInfo.Action) :
                CreateWorkerByAction(probeInfo.Action))
                ?? throw new Exception("Action not found to perform!");

            workProbeResponse = await worker.Execute(probeInfo, cancellationToken, workProbeResponse);

            await DoCalcIfNeeded(probeInfo, workProbeResponse);

            timer.Stop();
            _logCounter.AddSuccess(probeInfo.Id, probeInfo.Action, timer.Elapsed);
        }
        catch (Exception error)
        {
            timer.Stop();

            workProbeResponse.Success = false;
            workProbeResponse.FailMessage = error.Message;

            _logCounter.AddError(probeInfo.Id, probeInfo.Action, timer.Elapsed, error);
        }

        workProbeResponse.ProbedAt = DateTime.Now;
        workProbeResponse.DoHistory = probeInfo.DoHistory;

        _log.LogDebug($"Work {probeInfo.Id} done in {timer.Elapsed}");
        return workProbeResponse;
    }

    private IWorkerAction? CreateWorkerByAction(string actionName) => actionName switch
    {
        "ping" => _serviceProvider.GetRequiredService<IWorkerActionPing>(),
        "snmpget" => _serviceProvider.GetRequiredService<IWorkerActionSnmpGet>(),
        "snmpwalk" => _serviceProvider.GetRequiredService<IWorkerActionSnmpWalk>(),
        _ => null
    };

    private IWorkerAction? CreateMockWorkerByAction(string actionName) => actionName switch
    {
        "ping" => _serviceProvider.GetRequiredService<MockPingAction>(),
        "snmpget" => _serviceProvider.GetRequiredService<MockSnmpGetAction>(),
        "snmpwalk" => _serviceProvider.GetRequiredService<MockSnmpWalkAction>(),
        _ => null
    };

    private async Task DoCalcIfNeeded(WorkProbeInfo workProbeInfo, WorkProbeResponse workProbeResponse)
    {
        if (workProbeInfo.Calc == null)
            return;

        var calc = _serviceProvider.GetRequiredService<IWorkProbeCalc>();
        await calc.UpdateProbedValuesWithCalculated(workProbeInfo, workProbeResponse);
    }

}
