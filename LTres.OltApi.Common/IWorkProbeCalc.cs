using LTres.OltApi.Common.Models;

namespace LTres.OltApi.Common;

public interface IWorkProbeCalc
{
    Task UpdateProbedValuesWithCalculated(WorkProbeInfo workProbeInfo, WorkProbeResponse workProbeResponse);
}
