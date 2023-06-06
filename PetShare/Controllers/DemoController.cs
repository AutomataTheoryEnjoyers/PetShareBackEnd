using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShare.Services;

namespace PetShare.Controllers;

[Route("_demo")]
[ApiController]
public sealed class DemoController : ControllerBase
{
    private readonly DemoDatabasePopulator _populator;

    public DemoController(DemoDatabasePopulator populator)
    {
        _populator = populator;
    }

    /// <summary>
    ///     Adds some data to database
    /// </summary>
    [HttpPost]
    [Route("populate")]
    [AllowAnonymous]
    public async Task<ActionResult> Populate()
    {
        var result = await _populator.PopulateAsync();
        return result.HasValue ? Ok() : result.State.ToActionResult();
    }
}
