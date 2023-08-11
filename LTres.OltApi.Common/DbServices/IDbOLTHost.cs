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
}
