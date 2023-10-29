using LTres.OltApi.Common;
using LTres.OltApi.Common.DbServices;
using LTres.OltApi.Common.Models;

namespace LTres.OltApi.Core.Workers;

public class WorkListManager : IWorkListController
{
    private readonly IDbWorkProbeInfo _dbWorkProbeInfo;
    private readonly IWorkProbeCache _workProbeCache;

    public WorkListManager(IDbWorkProbeInfo dbWorkProbeInfo, IWorkProbeCache workProbeCache)
    {
        _dbWorkProbeInfo = dbWorkProbeInfo;
        _workProbeCache = workProbeCache;
    }

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
