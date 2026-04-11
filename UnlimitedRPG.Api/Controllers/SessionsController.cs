using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnlimitedRPG.Core.Model;
using UnlimitedRPG.Database;

namespace UnlimitedRPG.Api.Controllers;

[ApiController]
[Route("api/sessions")]
public class SessionsController(IDbContextFactory<RPGContext> dbFactory) : ControllerBase
{
    /// <summary>Creates a new session for the given character.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSessionRequest request)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var character = await db.PlayerCharacters.FindAsync(request.CharacterId);
        if (character is null) return NotFound();

        var session = new Session
        {
            PlayerCharacterId = request.CharacterId
        };

        db.Sessions.Add(session);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = session.Id },
            new SessionDto(session.Id, session.PlayerCharacterId, session.StartedAt, session.Status));
    }

    /// <summary>Returns a session by id.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var session = await db.Sessions.FindAsync(id);
        if (session is null) return NotFound();
        return Ok(new SessionDto(session.Id, session.PlayerCharacterId, session.StartedAt, session.Status));
    }
}

public record CreateSessionRequest(Guid CharacterId);
public record SessionDto(Guid Id, Guid CharacterId, DateTime StartedAt, SessionStatus Status);
