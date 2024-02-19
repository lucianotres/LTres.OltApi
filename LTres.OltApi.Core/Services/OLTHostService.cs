using LTres.OltApi.Common;
using LTres.OltApi.Common.DbServices;
using LTres.OltApi.Common.Models;
using Microsoft.Extensions.Logging;

namespace LTres.OltApi.Core.Services;

public class OLTHostService : IOLTHostService
{
    private readonly IDbOLTHost _db;
    private readonly ILogger _log;

    public OLTHostService(IDbOLTHost dbOLTHost, ILogger<OLTHostService> logger)
    {
        _db = dbOLTHost;
        _log = logger;
    }

    private void ValidateOltHost(OLT_Host olt_host)
    {
        if (olt_host == null)
            throw new ArgumentNullException("olt_host");

        if (string.IsNullOrWhiteSpace(olt_host.Name))
            throw new ArgumentException("OLT Host should have a name!");
        if (string.IsNullOrWhiteSpace(olt_host.Host))
            throw new ArithmeticException("OLT Host should have a host informed!");
    }

    public async Task<Guid> AddOLTHost(OLT_Host olt_host)
    {
        ValidateOltHost(olt_host);
        if (olt_host.Id != Guid.Empty)
            throw new ArgumentOutOfRangeException("Id", "Id should be empty when adding!");

        var registeredGuid = await _db.AddOLTHost(olt_host);
        _log.LogInformation($"New OLT_Host included: {registeredGuid}");
        return registeredGuid;
    }

    public async Task<int> ChangeOLTHost(OLT_Host olt_host)
    {
        ValidateOltHost(olt_host);
        if (olt_host.Id == Guid.Empty)
            throw new ArgumentOutOfRangeException("Id", "Id should not be empty when changing!");

        return await _db.ChangeOLTHost(olt_host);
    }

    public async Task<IEnumerable<OLT_Host>> ListOLTHosts(int take = 1000, int skip = 0,
        bool? filterActive = null,
        Guid? filterId = null,
        string? filterName = null,
        string? filterHost = null,
        string[]? filterTag = null)
    {
        if (take < 0 || take > 999999)
            throw new ArgumentOutOfRangeException("take");
        if (skip < 0)
            throw new ArgumentOutOfRangeException("skip");

        return await _db.ListOLTHosts(take, skip, filterActive, filterId, filterName, filterHost, filterTag);
    }

}
