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
    private OLTHostService _db;

    public OLTHostController(OLTHostService db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<Guid> Add(OLT_Host model) => await _db.AddOLTHost(model);

}