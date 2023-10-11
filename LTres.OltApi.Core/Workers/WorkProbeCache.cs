using System.Collections.Concurrent;
using LTres.OltApi.Common;

namespace LTres.OltApi.Core.Workers;

public class WorkProbeCache : IWorkProbeCache
{
    private List<Guid> _cacheLst = new List<Guid>();
    private object _cacheLock = new object();

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