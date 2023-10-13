using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    public async Task<WorkProbeResponse> Execute(WorkProbeInfo probeInfo, WorkProbeResponse? initialResponse = null)
    {
        _log.LogInformation($"Work probe received: {probeInfo.Id} -> {probeInfo.Action}");
        var workProbeResponse = new WorkProbeResponse() { Id = probeInfo.Id };
        
        if (probeInfo.Action == "ping")
        {
            var pingWorker = _serviceProvider.GetRequiredService<IWorkerActionPing>();
            workProbeResponse = await pingWorker.Execute(probeInfo, workProbeResponse);
        }
        else if (probeInfo.Action == "snmpget")
        {
            var pingWorker = _serviceProvider.GetRequiredService<IWorkerActionSnmpGet>();
            workProbeResponse = await pingWorker.Execute(probeInfo, workProbeResponse);
        }
        else
            _log.LogWarning("Action not found to perform.");

        workProbeResponse.ProbedAt = DateTime.Now;
        return workProbeResponse;
    }
}