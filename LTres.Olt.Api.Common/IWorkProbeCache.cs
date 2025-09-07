using LTres.Olt.Api.Common.Models;

namespace LTres.Olt.Api.Common;

/// <summary>
/// Describes the interface for a work probe cache which is used to manage the state of work probes by certain amount of time.
/// </summary>
public interface IWorkProbeCache
{
    Task<bool> TryToPutIntoCache(Guid idWork, DateTime requestedIn);

    Task<bool> TryToRemoveFromCache(Guid idWork);
}
