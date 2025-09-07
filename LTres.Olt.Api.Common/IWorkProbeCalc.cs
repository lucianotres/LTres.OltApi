using LTres.Olt.Api.Common.Models;

namespace LTres.Olt.Api.Common;

public interface IWorkProbeCalc
{
    Task UpdateProbedValuesWithCalculated(WorkProbeInfo workProbeInfo, WorkProbeResponse workProbeResponse);
}
