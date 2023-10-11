using LTres.OltApi.Common;
using LTres.OltApi.Common.DbServices;
using LTres.OltApi.Common.Models;

namespace LTres.OltApi.Core.Workers;

public class WorkDoneManager : IWorkResponseController
{
    private readonly IWorkProbeCache _workProbeCache;
    private readonly IDbWorkProbeResponse _dbWorkProbeResponse;

    public WorkDoneManager(IWorkProbeCache workProbeCache, IDbWorkProbeResponse dbWorkProbeResponse)
    {
        _workProbeCache = workProbeCache;
        _dbWorkProbeResponse = dbWorkProbeResponse;
    }

    public async Task ResponseReceived(WorkProbeResponse workProbeResponse)
    {
        await _workProbeCache.TryToRemoveFromCache(workProbeResponse.Id);

        await _dbWorkProbeResponse.SaveWorkProbeResponse(workProbeResponse);
    }
}
