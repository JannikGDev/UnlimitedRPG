using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnlimitedRPG.Core.Model;
using UnlimitedRPG.Database;

namespace UnlimitedRPG.Api.Controllers;

/// <summary>Manage worlds. A world defines the setting and enemy pool for a session.</summary>
[ApiController]
[Route("api/worlds")]
[Produces("application/json")]
public class WorldsController(IDbContextFactory<RPGContext> db) : ControllerBase
{
    /// <summary>Returns all available worlds.</summary>
    /// <response code="200">List of worlds.</response>
    [HttpGet]
    [ProducesResponseType(typeof(WorldDto[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorlds()
    {
        await using var ctx = await db.CreateDbContextAsync();
        var worlds = await ctx.Worlds
            .Select(w => new WorldDto(w.Id, w.Name))
            .ToArrayAsync();
        return Ok(worlds);
    }

    /// <summary>Creates a new world with the given name.</summary>
    /// <response code="201">The newly created world.</response>
    [HttpPost]
    [ProducesResponseType(typeof(WorldDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateWorld([FromBody] CreateWorldRequest request)
    {
        await using var ctx = await db.CreateDbContextAsync();
        var world = new World { Name = request.Name };
        ctx.Worlds.Add(world);
        await ctx.SaveChangesAsync();
        return CreatedAtAction(nameof(GetWorlds), new { id = world.Id }, new WorldDto(world.Id, world.Name));
    }
}

/// <summary>A game world that sessions can be set in.</summary>
/// <param name="Id">Unique identifier of the world.</param>
/// <param name="Name">Display name of the world.</param>
public record WorldDto(Guid Id, string Name);

/// <summary>Request body for creating a world.</summary>
/// <param name="Name">Display name for the new world.</param>
public record CreateWorldRequest(string Name);
