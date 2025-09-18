using LTres.Olt.Api.Common;
using LTres.Olt.Api.Common.DbServices;
using LTres.Olt.Api.Common.Models;

namespace LTres.Olt.Api.Core.Workers;

/// <summary>
/// Get from DB a list of work to be done and verify it if is not in cache.
/// </summary>
public class WorkListManager(IDbWorkProbeInfo dbWorkProbeInfo, IWorkProbeCache workProbeCache) : IWorkListController
{
    private readonly IDbWorkProbeInfo _dbWorkProbeInfo = dbWorkProbeInfo;
    private readonly IWorkProbeCache _workProbeCache = workProbeCache;

    public async Task<IEnumerable<WorkProbeInfo>> ToBeDone()
    {
        var finalList = new List<WorkProbeInfo>();
        var listToDoFromDB = await _dbWorkProbeInfo.ToDoList();
        var requestedIn = DateTime.Now;

        //filter work which is not in cache
        foreach (var a in listToDoFromDB)
        {
            if (await _workProbeCache.TryToPutIntoCache(a.Id, requestedIn))
            {
                a.RequestedIn = requestedIn;
                finalList.Add(a);
            }
        }

        return finalList;
    }
}
