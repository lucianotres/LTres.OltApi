using Microsoft.AspNetCore.Mvc;
using LTres.OltApi.Common.Models;
using LTres.OltApi.Common;

namespace LTres.OltApi.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OLTScriptController : ControllerBase
{
    private readonly IOLTScriptService _service;

    public OLTScriptController(IOLTScriptService service)
    {
        _service = service;
    }

    /// <summary>
    /// Include an OLT Script to the database
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<Guid> Add(OLT_Script model) => await _service.AddOLTScript(model);

    /// <summary>
    /// Change an OLT Script on the database
    /// </summary>
    [HttpPut]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<Guid> Change(OLT_Script model)
    {
        var changedCount = await _service.ChangeOLTScript(model);
        return changedCount > 0 ? model.Id : Guid.Empty;
    }

    /// <summary>
    /// Search or list the registered OLT Scripts
    /// </summary>
    /// <param name="take">Limit a maximum results</param>
    /// <param name="skip">Skip a certain amount of registers before list</param>
    /// <param name="filterId">Filter by ID of OLT Script</param>
    /// <param name="filterTag">Filter by specifics tags</param>
    /// <returns></returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<OLT_Script>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<OLT_Script>> List(int take = 1000, int skip = 0,
        Guid? filterId = null,
        string[]? filterTag = null)
        => await _service.ListOLTScripts(take, skip, filterId, filterTag);

    /// <summary>
    /// Return info of a specific OLT Script
    /// </summary>
    /// <param name="id">Guid ID of the OLT Script</param>
    [HttpGet("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(OLT_Script), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<OLT_Script?> Get(Guid id) => (await _service.ListOLTScripts(1, 0, id)).FirstOrDefault();
}