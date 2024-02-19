using LTres.OltApi.Common;
using LTres.OltApi.Common.DbServices;
using LTres.OltApi.Common.Models;
using Microsoft.Extensions.Logging;

namespace LTres.OltApi.Core.Services;

public class OLTScriptService : IOLTScriptService
{
    private readonly IDbOLTScript _db;
    private readonly ILogger _log;

    public OLTScriptService(IDbOLTScript dbOLTScript, ILogger<OLTHostService> logger)
    {
        _db = dbOLTScript;
        _log = logger;
    }

    private void ValidateOltScript(OLT_Script olt_script)
    {
        if (olt_script == null)
            throw new ArgumentNullException("olt_script");

        if (string.IsNullOrWhiteSpace(olt_script.Script))
            throw new ArgumentException("OLT Script should have a script lines!");
    }

    public async Task<Guid> AddOLTScript(OLT_Script oltScript)
    {
        ValidateOltScript(oltScript);
        if (oltScript.Id != Guid.Empty)
            throw new ArgumentOutOfRangeException("Id", "Id should be empty when adding!");

        var registeredGuid = await _db.AddOLTScript(oltScript);
        _log.LogInformation($"New OLT_Script included: {registeredGuid}");
        return registeredGuid;
    }

    public async Task<int> ChangeOLTScript(OLT_Script oltScript)
    {
        ValidateOltScript(oltScript);
        if (oltScript.Id == Guid.Empty)
            throw new ArgumentOutOfRangeException("Id", "Id should not be empty when changing!");

        return await _db.ChangeOLTScript(oltScript);
    }

    public async Task<IEnumerable<OLT_Script>> ListOLTScripts(int take = 1000, int skip = 0, Guid? filterId = null, string[]? filterTag = null)
    {
        if (take < 0 || take > 999999)
            throw new ArgumentOutOfRangeException("take");
        if (skip < 0)
            throw new ArgumentOutOfRangeException("skip");

        return await _db.ListOLTScripts(take, skip, filterId, filterTag);
    }

}
