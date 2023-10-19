using LTres.OltApi.Common;
using LTres.OltApi.Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LTres.OltApi.Core.Workers;

public class WorkAction : IWorkerAction
{
    private readonly ILogger _log;
    private readonly IServiceProvider _serviceProvider;

    public WorkAction(ILogger<WorkAction> logger, IServiceProvider serviceProvider)
    {
        _log = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<WorkProbeResponse> Execute(WorkProbeInfo probeInfo, CancellationToken cancellationToken, WorkProbeResponse? initialResponse = null)
    {
        _log.LogInformation($"Work probe received: {probeInfo.Id} -> {probeInfo.Action}");
        var workProbeResponse = new WorkProbeResponse() { Id = probeInfo.Id };

        if (probeInfo.Action == "ping")
        {
            var pingWorker = _serviceProvider.GetRequiredService<IWorkerActionPing>();
            workProbeResponse = await pingWorker.Execute(probeInfo, cancellationToken, workProbeResponse);
        }
        else if (probeInfo.Action == "snmpget")
        {
            var snmpGetWorker = _serviceProvider.GetRequiredService<IWorkerActionSnmpGet>();
            workProbeResponse = await snmpGetWorker.Execute(probeInfo, cancellationToken, workProbeResponse);
        }
        else if (probeInfo.Action == "snmpwalk")
        {
            var snmpWalkWorker = _serviceProvider.GetRequiredService<IWorkerActionSnmpWalk>();
            workProbeResponse = await snmpWalkWorker.Execute(probeInfo, cancellationToken, workProbeResponse);
        }
        else
            _log.LogWarning("Action not found to perform.");

        workProbeResponse.ProbedAt = DateTime.Now;
        return workProbeResponse;
    }
}