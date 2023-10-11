using LTres.OltApi.Common;
using LTres.OltApi.Common.Models;

namespace LTres.OltApi.Core.Workers;

public class WorkDoneManager : IWorkResponseController
{
    private readonly IWorkProbeCache _workProbeCache;

    public WorkDoneManager(IWorkProbeCache workProbeCache)
    {
        _workProbeCache = workProbeCache;
    }

    public async Task ResponseReceived(WorkProbeResponse workProbeResponse)
    {
        await _workProbeCache.TryToRemoveFromCache(workProbeResponse.Id);
    }
}
