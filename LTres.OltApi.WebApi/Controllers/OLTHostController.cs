using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LTres.OltApi.Common.Models;
using LTres.OltApi.Core.Services;

namespace LTres.OltApi.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OLTHostController : ControllerBase
{
    private OLTHostService _service;

    public OLTHostController(OLTHostService service)
    {
        _service = service;
    }

    /// <summary>
    /// Include an OLT Host to the database
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType<Guid>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<Guid> Add(OLT_Host model) => await _service.AddOLTHost(model);

    /// <summary>
    /// Change an OLT Host on the database
    /// </summary>
    [HttpPut]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType<Guid>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<Guid> Change(OLT_Host model) => await _service.ChangeOLTHost(model);

    /// <summary>
    /// Search or list the registered OLT Hosts
    /// </summary>
    /// <param name="take">Limit a maximum results</param>
    /// <param name="skip">Skip a certain amount of registers before list</param>
    /// <param name="filterId">Filter by ID of OLT Host</param>
    /// <param name="filterName">Filter by OLT name (contains)</param>
    /// <param name="filterHost">Filter by OLT host (contains)</param>
    /// <param name="filterTag">Filter by specifics tags</param>
    /// <returns></returns>
    [HttpGet]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType<Guid>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<OLT_Host>> List(int take = 1000, int skip = 0,
        Guid? filterId = null, 
        string? filterName = null, 
        string? filterHost = null, 
        string[]? filterTag = null)
        => await _service.ListOLTHosts(take, skip, filterId, filterName, filterHost, filterTag);

    /// <summary>
    /// Return info of a specific OLT Host
    /// </summary>
    /// <param name="id">Guid ID of the OLT Host</param>
    [HttpGet("{id}")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType<Guid>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<OLT_Host?> Get(Guid id) => (await _service.ListOLTHosts(1, 0, id)).FirstOrDefault();
}