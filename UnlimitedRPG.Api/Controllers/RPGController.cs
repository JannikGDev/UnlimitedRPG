using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RpgFramework.Core.Model;
using UnlimitedRPG.Database;

namespace RpgFramework.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RPGController(IDbContextFactory<RPGContext> contextFactory) : ControllerBase
{
    [HttpGet("worlds")]
    public async Task<IActionResult> GetWorlds()
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        var worlds = await db.Worlds.ToListAsync();
        return Ok(worlds);
    }

    [HttpPost("worlds")]
    public async Task<IActionResult> CreateWorld([FromBody] CreateWorldRequest request)
    {
        var world = new World { Name = request.Name };

        await using var db = await contextFactory.CreateDbContextAsync();
        db.Worlds.Add(world);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWorlds), new { id = world.Id }, world);
    }
}

public record CreateWorldRequest(string Name);
