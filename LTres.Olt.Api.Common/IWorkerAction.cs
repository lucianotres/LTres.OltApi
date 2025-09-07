using LTres.Olt.Api.Common.Models;

namespace LTres.Olt.Api.Common;

public interface IWorkerAction
{
    Task<WorkProbeResponse> Execute(WorkProbeInfo probeInfo, CancellationToken cancellationToken, WorkProbeResponse? initialResponse = null);
}


public interface IWorkerActionPing : IWorkerAction { };
public interface IWorkerActionSnmpGet : IWorkerAction { };
public interface IWorkerActionSnmpWalk : IWorkerAction { };
