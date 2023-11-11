using LTres.OltApi.Common.Models;

namespace LTres.OltApi.Common;

public interface IOLTHostService
{
    /// <summary>
    /// Include an OLT host at DB.
    /// Returns: ID of the created registry
    /// </summary>
    Task<Guid> AddOLTHost(OLT_Host olt);

    /// <summary>
    /// Get a list of OLT's. Can use one or more filters to locate one of them
    /// </summary>
    /// <param name="take">Limit the result to max of records</param>
    /// <param name="skip">Skip a number of records before</param>
    /// <param name="filterId">Filter by ID</param>
    /// <param name="filterName">Filter by Name (contains)</param>
    /// <param name="filterHost">Filter by Host (contains)</param>
    /// <param name="filterTag">Filter by existing tags</param>
    /// <returns></returns>
    Task<IEnumerable<OLT_Host>> ListOLTHosts(int take = 1000, int skip = 0,
        bool? filterActive = null,
        Guid? filterId = null,
        string? filterName = null,
        string? filterHost = null,
        string[]? filterTag = null);


    /// <summary>
    /// Change an entire register of OLT Host.
    /// Id it's the key to find the existing one.
    /// </summary>
    /// <param name="olt">New OLT Host info with the original ID</param>
    /// <returns>0 to nothing changed and positive for number of affected</returns>
    Task<int> ChangeOLTHost(OLT_Host olt);
}
