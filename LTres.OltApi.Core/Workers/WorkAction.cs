using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using LTres.OltApi.Common;
using LTres.OltApi.Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LTres.OltApi.Core.Workers;

public class WorkAction : IWorkerAction
{
    private readonly ILogger _log;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogCounter _logCounter;

    public WorkAction(ILogger<WorkAction> logger, IServiceProvider serviceProvider, ILogCounter counter)
    {
        _log = logger;
        _serviceProvider = serviceProvider;
        _logCounter = counter;
    }

    public async Task<WorkProbeResponse> Execute(WorkProbeInfo probeInfo, CancellationToken cancellationToken, WorkProbeResponse? initialResponse = null)
    {
        _log.LogDebug($"Work probe received: {probeInfo.Id} -> {probeInfo.Action}");
        var workProbeResponse = new WorkProbeResponse() { Id = probeInfo.Id, Request = probeInfo };
        var timer = Stopwatch.StartNew();

        try
        {
            bool shouldUseMocking = probeInfo.Host.Address.Equals(IPAddress.None);
            IWorkerAction worker;

            //find the correct action to perform ------

            if (probeInfo.Action == "ping")
            {
                if (shouldUseMocking)
                    worker = _serviceProvider.GetRequiredService<MockPingAction>();
                else
                    worker = _serviceProvider.GetRequiredService<IWorkerActionPing>();
            }
            else if (probeInfo.Action == "snmpget")
            {
                if (shouldUseMocking)
                    worker = _serviceProvider.GetRequiredService<MockPingAction>();
                else
                    worker = _serviceProvider.GetRequiredService<IWorkerActionSnmpGet>();
            }
            else if (probeInfo.Action == "snmpwalk")
            {
                if (shouldUseMocking)
                    worker = _serviceProvider.GetRequiredService<MockPingAction>();
                else
                    worker = _serviceProvider.GetRequiredService<IWorkerActionSnmpWalk>();
            }
            else
                throw new Exception("Action not found to perform.");

            //execute the action ------
            workProbeResponse = await worker.Execute(probeInfo, cancellationToken, workProbeResponse);

            if (probeInfo.Calc != null)
            {
                var calc = _serviceProvider.GetRequiredService<IWorkProbeCalc>();
                await calc.UpdateProbedValuesWithCalculated(probeInfo, workProbeResponse);
            }

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
}