using Microsoft.AspNetCore.Mvc;

namespace RpgFramework.Api.Controllers;

[ApiController]
[Route("api/worlds")]
public class WorldsController : ControllerBase
{
    static readonly WorldDto[] _worlds =
    [
        new(Guid.Parse("00000000-0000-0000-0000-000000000001"), "The Darklands"),
        new(Guid.Parse("00000000-0000-0000-0000-000000000002"), "Sunken Vale"),
    ];

    [HttpGet]
    public IActionResult GetWorlds() => Ok(_worlds);

    [HttpPost]
    public IActionResult CreateWorld([FromBody] CreateWorldRequest request)
    {
        var world = new WorldDto(Guid.Parse("00000000-0000-0000-0000-000000000001"), request.Name);
        return CreatedAtAction(nameof(GetWorlds), new { id = world.Id }, world);
    }
}

public record WorldDto(Guid Id, string Name);
public record CreateWorldRequest(string Name);
