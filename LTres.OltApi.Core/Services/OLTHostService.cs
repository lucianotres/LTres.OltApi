using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LTres.OltApi.Common.DbServices;
using LTres.OltApi.Common.Models;
using Microsoft.Extensions.Logging;

namespace LTres.OltApi.Core.Services;

public class OLTHostService
{
    private readonly IDbOLTHost _db;
    private readonly ILogger _log;

    public OLTHostService(IDbOLTHost dbOLTHost, ILogger<OLTHostService> logger)
    {
        _db = dbOLTHost;
        _log = logger;
    }

    /// <summary>
    /// Validate and include an OLT Host to the database.
    /// Throw exceptions with invalid data
    /// </summary>
    public async Task<Guid> AddOLTHost(OLT_Host olt_host)
    {
        if (olt_host == null)
            throw new ArgumentNullException("olt_host");
        if (string.IsNullOrWhiteSpace(olt_host.Name))
            throw new ArgumentException("OLT Host should have a name!");
        if (string.IsNullOrWhiteSpace(olt_host.Host))
            throw new ArithmeticException("OLT Host should have a host informed!");

        var ret = await _db.AddOLTHost(olt_host);
        _log.LogInformation($"New OLT_Host included: {ret}");
        return ret;
    }


    /// <summary>
    /// Validate and do a query for OLT's hosts
    /// </summary>
    public async Task<IEnumerable<OLT_Host>> ListOLTHosts(int take = 1000, int skip = 0,
        Guid? filterId = null, 
        string? filterName = null, 
        string? filterHost = null, 
        string[]? filterTag = null)
    {
        if (take < 0 || take > 999999)
            throw new ArgumentOutOfRangeException("take");
        if (skip < 0)
            throw new ArgumentOutOfRangeException("skip");
        
        return await _db.ListOLTHosts(take, skip, filterId, filterName, filterHost, filterTag);
    }

}