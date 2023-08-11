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
    /// <param name="olt_host"></param>
    /// <returns></returns>
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
}
