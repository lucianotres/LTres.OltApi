using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LTres.OltApi.Common.Models;

namespace LTres.OltApi.Common.DbServices;

/// <summary>
/// Representation of actions of OLT hosts
/// </summary>
public interface IDbOLTHost
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
        Guid? filterId = null,
        string? filterName = null,
        string? filterHost = null,
        string[]? filterTag = null);
}
