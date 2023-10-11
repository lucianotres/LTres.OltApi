using LTres.OltApi.Common.Models;

namespace LTres.OltApi.Common.DbServices;

public interface IDbWorkProbeResponse
{
    Task SaveWorkProbeResponse(WorkProbeResponse workProbeResponse);
}