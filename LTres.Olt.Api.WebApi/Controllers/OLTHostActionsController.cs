using Microsoft.AspNetCore.Mvc;
using LTres.Olt.Api.Common;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using LTres.Olt.Api.Core;

namespace LTres.Olt.Api.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class OLTHostActionsController : ControllerBase
{
    private readonly IOLTHostCLIActionsService _service;

    public OLTHostActionsController(IOLTHostCLIActionsService service)
    {
        _service = service;
    }

    private string LinesToStr(IEnumerable<string> lines) =>
        lines.Aggregate(new StringBuilder(), (sb, l) =>
        {
            sb.AppendLine(l);
            return sb;
        }).ToString();


    [HttpGet("onuinfo/{oltId}/{olt}/{slot}/{port}/{id}")]
    [Produces("text/plain")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<string> GetONUInfo(Guid oltId, int olt, int slot, int port, int id) =>
        LinesToStr(await _service.GetONUInfo(oltId, olt, slot, port, id));

    [HttpGet("onuinterfaces/{oltId}/{olt}/{slot}/{port}/{id}")]
    [Produces("text/plain")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<string> GetONUInterfaces(Guid oltId, int olt, int slot, int port, int id) =>
        LinesToStr(await _service.GetONUInterfaces(oltId, olt, slot, port, id));

    [HttpGet("onuversion/{oltId}/{olt}/{slot}/{port}/{id}")]
    [Produces("text/plain")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<string> GetONUVersion(Guid oltId, int olt, int slot, int port, int id) =>
        LinesToStr(await _service.GetONUVersion(oltId, olt, slot, port, id));

    [HttpGet("onumac/{oltId}/{olt}/{slot}/{port}/{id}")]
    [Produces("text/plain")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<string> GetONUmac(Guid oltId, int olt, int slot, int port, int id) =>
        LinesToStr(await _service.GetONUmac(oltId, olt, slot, port, id));



    [HttpGet("oltscript/run/{oltId}/{scriptId}")]
    public async Task<Guid> GetOltScriptRun([FromServices] IMemoryCache cache, [FromServices] IOLTHostCLIScriptService scriptService, Guid oltId, Guid scriptId)
    {
        var guid = Guid.NewGuid();

        var variablesFromQueryString = HttpContext.Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());

        await scriptService.StartScript(oltId, scriptId, variablesFromQueryString);
        cache.Set(guid, scriptService, TimeSpan.FromMinutes(5));

        return guid;
    }

    [HttpGet("oltscript/result/{id}")]
    public string GetOltScriptResult([FromServices] IMemoryCache cache, Guid id)
    {
        var entry = cache.Get<OLTHostCLIScriptService>(id);
        return entry == null ? "N/A" : entry.ScriptResult;
    }


}
