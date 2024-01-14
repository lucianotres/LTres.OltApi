using Microsoft.AspNetCore.Mvc;
using LTres.OltApi.Common.Models;
using LTres.OltApi.Common;
using System.Text;

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
}