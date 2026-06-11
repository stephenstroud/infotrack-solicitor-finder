using Microsoft.AspNetCore.Mvc;

namespace InfoTrack.SolicitorFinder.Api.Controllers;

/// <summary>Supplies the default location list the UI pre-populates (and the user can edit).</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class LocationsController(IConfiguration configuration) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<string>> GetDefaults() =>
        Ok(configuration.GetSection("DefaultLocations").Get<string[]>() ?? []);
}
