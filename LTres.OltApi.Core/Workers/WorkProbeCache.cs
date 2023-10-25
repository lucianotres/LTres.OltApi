using System.Collections.Concurrent;
using LTres.OltApi.Common;

namespace LTres.OltApi.Core.Workers;

public class WorkProbeCache : IWorkProbeCache
{
    private List<Guid> _cacheLst = new List<Guid>();
    private object _cacheLock = new object();

    public WorkProbeCache(ILogCounter logCounter)
    {
        logCounter.RegisterHookOnPrintResetAction<WorkProbeCache>(HookLogCounter);
    }

    private void HookLogCounter(ILogCounter logCounter)
    {
        var cacheCount = _cacheLst.Count;
        if (cacheCount > 0)
            logCounter.AddCount("probe cache", cacheCount);
    }

    public async Task<bool> TryToPutIntoCache(Guid idWork)
    {
        return await Task.Run(() =>
        {
            lock (_cacheLock)
            {
                if (_cacheLst.Any(w => w == idWork))
                    return false;

                _cacheLst.Add(idWork);
                return true;
            }
        });
    }

    public async Task<bool> TryToRemoveFromCache(Guid idWork)
    {
        return await Task.Run(() =>
        {
            lock (_cacheLock)
                return _cacheLst.Remove(idWork);
        });
    }
}