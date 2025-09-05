using System.Collections.Concurrent;
using LTres.Olt.Api.Common;

namespace LTres.Olt.Api.Core.Workers;

public class WorkProbeCache : IWorkProbeCache
{
    private List<(Guid id, DateTime requestedIn)> _cacheLst = new List<(Guid id, DateTime requestedIn)>();
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

    public async Task<bool> TryToPutIntoCache(Guid idWork, DateTime requestedIn)
    {
        return await Task.Run(() =>
        {
            lock (_cacheLock)
            {
                if (_cacheLst.Any(w => w.id == idWork))
                    return false;

                RemoveOld();

                _cacheLst.Add((idWork, requestedIn));
                return true;
            }
        });
    }

    private void RemoveOld()
    {
        var cutAt = DateTime.Now.Subtract(TimeSpan.FromMinutes(3));
        _cacheLst.RemoveAll(f => f.requestedIn < cutAt);
    }

    public async Task<bool> TryToRemoveFromCache(Guid idWork)
    {
        return await Task.Run(() =>
        {
            lock (_cacheLock)
                return _cacheLst.RemoveAll(f => f.id == idWork) > 0;
        });
    }
}
