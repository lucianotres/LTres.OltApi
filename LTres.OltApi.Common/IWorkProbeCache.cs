using LTres.OltApi.Common.Models;

namespace LTres.OltApi.Common;

public interface IWorkProbeCache
{
    Task<bool> TryToPutIntoCache(Guid idWork, DateTime requestedIn);

    Task<bool> TryToRemoveFromCache(Guid idWork);
}