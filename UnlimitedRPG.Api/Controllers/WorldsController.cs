using Microsoft.AspNetCore.Mvc;

namespace UnlimitedRPG.Api.Controllers;

/// <summary>Manage worlds. A world defines the setting and enemy pool for a session.</summary>
[ApiController]
[Route("api/worlds")]
[Produces("application/json")]
public class WorldsController : ControllerBase
{
    static readonly WorldDto[] _worlds =
    [
        new(Guid.Parse("00000000-0000-0000-0000-000000000001"), "The Darklands"),
        new(Guid.Parse("00000000-0000-0000-0000-000000000002"), "Sunken Vale"),
    ];

    /// <summary>Returns all available worlds.</summary>
    /// <response code="200">List of worlds.</response>
    [HttpGet]
    [ProducesResponseType(typeof(WorldDto[]), StatusCodes.Status200OK)]
    public IActionResult GetWorlds() => Ok(_worlds);

    /// <summary>Creates a new world with the given name.</summary>
    /// <response code="201">The newly created world.</response>
    [HttpPost]
    [ProducesResponseType(typeof(WorldDto), StatusCodes.Status201Created)]
    public IActionResult CreateWorld([FromBody] CreateWorldRequest request)
    {
        var world = new WorldDto(Guid.Parse("00000000-0000-0000-0000-000000000001"), request.Name);
        return CreatedAtAction(nameof(GetWorlds), new { id = world.Id }, world);
    }
}

/// <summary>A game world that sessions can be set in.</summary>
/// <param name="Id">Unique identifier of the world.</param>
/// <param name="Name">Display name of the world.</param>
public record WorldDto(Guid Id, string Name);

/// <summary>Request body for creating a world.</summary>
/// <param name="Name">Display name for the new world.</param>
public record CreateWorldRequest(string Name);
