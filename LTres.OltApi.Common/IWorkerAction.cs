using LTres.OltApi.Common.Models;

namespace LTres.OltApi.Common;

public interface IWorkerAction
{
    WorkProbeResponse Execute(WorkProbeInfo probeInfo);
}