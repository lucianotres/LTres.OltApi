using LTres.OltApi.Common.Models;

namespace LTres.OltApi.Common;

public interface IWorkerAction
{
    Task<WorkProbeResponse> Execute(WorkProbeInfo probeInfo, WorkProbeResponse? initialResponse = null);
}


public interface IWorkerActionPing : IWorkerAction { };
public interface IWorkerActionSnmpGet : IWorkerAction { };
public interface IWorkerActionSnmpWalk : IWorkerAction { };