using System.Data.Common;
using LTres.Olt.Api.Common;
using LTres.Olt.Api.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace LTres.Olt.Api.WebApi;

[ApiController]
[Route("api/[controller]")]
public class OLTHostItemController : ControllerBase
{
    private readonly IOLTHostItemService _service;

    public OLTHostItemController(IOLTHostItemService service)
    {
        _service = service;
    }

    /// <summary>
    /// Include an OLT Host Item to the database
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<Guid> Add(OLT_Host_Item model) => await _service.AddOLTHostItem(model);

    /// <summary>
    /// Change an OLT Host Item on the database
    /// </summary>
    [HttpPut]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<Guid> Change(OLT_Host_Item model)
    {
        var changedCount = await _service.ChangeOLTHostItem(model);
        return changedCount > 0 ? model.Id : Guid.Empty;
    }

    /// <summary>
    /// Return info of a specific OLT Host Item
    /// </summary>
    /// <param name="id">Guid ID of the OLT Host Item</param>
    [HttpGet("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(OLT_Host_Item), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<OLT_Host_Item?> Get(Guid id) => (await _service.ListOLTHostItems(1, 0, null, id)).FirstOrDefault();


    /// <summary>
    /// Return user defined items of an OLT Host
    /// </summary>
    /// <param name="id">Guid ID of the OLT Host</param>
    /// <param name="limit">Limit result range</param>
    /// <param name="skip">Amount to skip before mount the result</param>
    [HttpGet("ByOLT/{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<OLT_Host_Item>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<OLT_Host_Item>> GetByOLT(Guid id, int limit = 9999, int skip = 0) => 
        await _service.ListOLTHostItems(limit, skip, id, null, null, null, Guid.Empty);

    /// <summary>
    /// Return items of an OLT Host by related Key
    /// </summary>
    /// <param name="id">Guid ID of the OLT Host</param>
    /// <param name="key">Key to find OLT Host Items</param>
    /// <param name="limit">Limit result range</param>
    /// <param name="skip">Amount to skip before mount the result</param>
    [HttpGet("ByKey/{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<OLT_Host_Item>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<OLT_Host_Item>> GetByKey(Guid id, string key, bool activeOnly = true, int limit = 9999, int skip = 0)
    {
        if (id == Guid.Empty)
            throw new ArgumentNullException("id", "An OLT Host id is mandatory!");
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException("key", "An OLT Host Item key is mandatory!");

        var itemsList = await _service.ListOLTHostItems(limit, skip, id, null, activeOnly ? true : null, null, Guid.Empty, key);
        
        var templateItems = itemsList
            .Where(w => w.Template.GetValueOrDefault() || w.Action == "snmpwalk")
            .ToList();

        if (templateItems.Any())
        {
            itemsList = itemsList.Where(w => !templateItems.Exists(k => k.Id == w.Id)).ToList();

            foreach(var i in templateItems)
            {
                var relatedItems = await _service.ListOLTHostItems(limit, skip, id, null, null, null, i.Id);
                itemsList = itemsList.Concat(relatedItems);
            }
        }
        
        return itemsList;
    }

    [HttpGet("OnuList/{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<ONU_Info>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ONU_Info>> ListONUInfo(Guid id, bool full = true)
    {
        if (id == Guid.Empty)
            throw new ArgumentNullException("id", "An OLT Host id is mandatory!");

        return await _service.ListONUInfo(id, full);
    }


}
