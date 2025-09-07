using Microsoft.AspNetCore.Mvc;
using LTres.Olt.Api.Common.Models;
using LTres.Olt.Api.Common;

namespace LTres.Olt.Api.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class OLTHostController : ControllerBase
{
    private readonly IOLTHostService _service;

    public OLTHostController(IOLTHostService service)
    {
        _service = service;
    }

    /// <summary>
    /// Include an OLT Host to the database
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<Guid> Add(OLT_Host model) => await _service.AddOLTHost(model);

    /// <summary>
    /// Change an OLT Host on the database
    /// </summary>
    [HttpPut]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<Guid> Change(OLT_Host model)
    {
        var changedCount = await _service.ChangeOLTHost(model);
        return changedCount > 0 ? model.Id : Guid.Empty;
    }

    /// <summary>
    /// Search or list the registered OLT Hosts
    /// </summary>
    /// <param name="take">Limit a maximum results</param>
    /// <param name="skip">Skip a certain amount of registers before list</param>
    /// <param name="filterActive">Filter by Active OLT Hosts</param>
    /// <param name="filterId">Filter by ID of OLT Host</param>
    /// <param name="filterName">Filter by OLT name (contains)</param>
    /// <param name="filterHost">Filter by OLT host (contains)</param>
    /// <param name="filterTag">Filter by specifics tags</param>
    /// <returns></returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<OLT_Host>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<OLT_Host>> List(int take = 1000, int skip = 0,
        bool? filterActive = null,
        Guid? filterId = null,
        string? filterName = null,
        string? filterHost = null,
        string[]? filterTag = null)
        => await _service.ListOLTHosts(take, skip, filterActive, filterId, filterName, filterHost, filterTag);

    /// <summary>
    /// Return info of a specific OLT Host
    /// </summary>
    /// <param name="id">Guid ID of the OLT Host</param>
    [HttpGet("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(OLT_Host), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<OLT_Host?> Get(Guid id) => (await _service.ListOLTHosts(1, 0, null, id)).FirstOrDefault();
}
