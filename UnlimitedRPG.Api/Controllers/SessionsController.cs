using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnlimitedRPG.Core.Interfaces;
using UnlimitedRPG.Core.Model;
using UnlimitedRPG.Database;

namespace UnlimitedRPG.Api.Controllers;

[ApiController]
[Route("api/sessions")]
public class SessionsController(IDbContextFactory<RPGContext> dbFactory, ITextGenerator textGenerator) : ControllerBase
{
    private const string SystemPrompt =
        "You are the narrator of a pen-and-paper RPG adventure. " +
        "The player will say or do things. Respond with a short, vivid narration of what happens next. " +
        "Keep responses to 2-3 sentences.";

    /// <summary>Creates a new session for the given character.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSessionRequest request)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var character = await db.PlayerCharacters.FindAsync(request.CharacterId);
        if (character is null) return NotFound();

        var session = new Session { PlayerCharacterId = request.CharacterId };

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

    /// <summary>Returns all active sessions.</summary>
    [HttpGet]
    public async Task<IActionResult> GetActive()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var sessions = await db.Sessions
            .Where(s => s.Status == SessionStatus.Active)
            .Include(s => s.PlayerCharacter)
            .Select(s => new ActiveSessionDto(s.Id, s.PlayerCharacterId, s.PlayerCharacter.Name, s.StartedAt))
            .ToListAsync();
        return Ok(sessions);
    }

    /// <summary>Appends a player message and returns the player message + game response.</summary>
    [HttpPost("{id:guid}/messages")]
    public async Task<IActionResult> AddMessage(Guid id, [FromBody] AddMessageRequest request, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var session = await db.Sessions.FindAsync([id], ct);
        if (session is null) return NotFound();

        var playerMessage = new SessionMessage
        {
            SessionId = id,
            Role      = "player",
            Mode      = request.Mode,
            Text      = request.Text
        };
        db.SessionMessages.Add(playerMessage);
        await db.SaveChangesAsync(ct);

        var inputPrompt = $"[{request.Mode.ToUpperInvariant()}] {request.Text}";
        var narration = await textGenerator.GenerateAsync(SystemPrompt, inputPrompt, ct);

        var gameMessage = new SessionMessage
        {
            SessionId = id,
            Role      = "game",
            Mode      = string.Empty,
            Text      = narration
        };
        db.SessionMessages.Add(gameMessage);
        await db.SaveChangesAsync(ct);

        return Ok(new AddMessageResponse(
            new SessionMessageDto(playerMessage.Id, playerMessage.Role, playerMessage.Mode, playerMessage.Text, playerMessage.SentAt),
            new SessionMessageDto(gameMessage.Id, gameMessage.Role, gameMessage.Mode, gameMessage.Text, gameMessage.SentAt)
        ));
    }

    /// <summary>Returns all messages for a session ordered by time.</summary>
    [HttpGet("{id:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var session = await db.Sessions.FindAsync(id);
        if (session is null) return NotFound();

        var messages = await db.SessionMessages
            .Where(m => m.SessionId == id)
            .OrderBy(m => m.SentAt)
            .Select(m => new SessionMessageDto(m.Id, m.Role, m.Mode, m.Text, m.SentAt))
            .ToListAsync();

        return Ok(messages);
    }
}

public record CreateSessionRequest(Guid CharacterId);
public record AddMessageRequest(string Mode, string Text);
public record AddMessageResponse(SessionMessageDto PlayerMessage, SessionMessageDto GameMessage);
public record SessionDto(Guid Id, Guid CharacterId, DateTime StartedAt, SessionStatus Status);
public record ActiveSessionDto(Guid Id, Guid CharacterId, string CharacterName, DateTime StartedAt);
public record SessionMessageDto(Guid Id, string Role, string Mode, string Text, DateTime SentAt);
