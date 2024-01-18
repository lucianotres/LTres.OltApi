using Microsoft.AspNetCore.Mvc;
using LTres.OltApi.Common.Models;
using LTres.OltApi.Common;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using LTres.OltApi.Core;

namespace LTres.OltApi.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
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



    [HttpGet("teststart")]
    public async Task<Guid> TestStart([FromServices] IMemoryCache cache, [FromServices] IOLTHostCLIScriptService scriptService)
    {
        var guid = Guid.NewGuid();
        await scriptService.StartScript(Guid.Parse("6a2c3da4-a027-4807-8639-318d9116dace"), Guid.NewGuid(), null);
        cache.Set(guid, scriptService, TimeSpan.FromMinutes(5));

        return guid;
    }

    [HttpGet("testGet/{id}")]
    public string TestGet([FromServices] IMemoryCache cache, Guid id)
    {
        var entry = cache.Get<OLTHostCLIScriptService>(id);
        return entry == null ? "N/A" : entry.ScriptResult;
    }


}