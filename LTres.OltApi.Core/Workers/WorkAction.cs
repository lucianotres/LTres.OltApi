using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LTres.OltApi.Common;
using LTres.OltApi.Common.Models;
using Microsoft.Extensions.Logging;

namespace LTres.OltApi.Core.Workers;

public class WorkAction : IWorkerAction
{
    private readonly ILogger _log;

    public WorkAction(ILogger<WorkAction> logger)
    {
        _log = logger;
    }

    public WorkProbeResponse Execute(WorkProbeInfo probeInfo)
    {
        _log.LogInformation($"Work probe received: {probeInfo.Id} -> {probeInfo.Action}");
        var workProbeResponse = new WorkProbeResponse() { Id = probeInfo.Id };
        
        if (probeInfo.Action == "ping")
        {
            workProbeResponse.ValueInt = 1;

        }
        else
            _log.LogWarning("Action not found to perform.");

        workProbeResponse.ProbedAt = DateTime.Now;
        return workProbeResponse;
    }
}