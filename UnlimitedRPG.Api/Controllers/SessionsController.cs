using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnlimitedRPG.Api.Engine;
using UnlimitedRPG.Core.Inputs;
using UnlimitedRPG.Core.Interfaces;
using UnlimitedRPG.Core.Model;
using UnlimitedRPG.Database;

namespace UnlimitedRPG.Api.Controllers;

/// <summary>Manage combat sessions. A session ties a player character to a world and an enemy encounter.</summary>
[ApiController]
[Route("api/sessions")]
[Produces("application/json")]
public class SessionsController(
    IGameEngine engine,
    IContentOrchestrator orchestrator,
    IDbContextFactory<RPGContext> db) : ControllerBase
{
    /// <summary>Starts a new session in the given world for the given player.</summary>
    /// <remarks>
    /// The session begins at round 1 with a fresh enemy spawned from the world's enemy pool.
    /// The combat log is empty until the first action is executed.
    /// </remarks>
    /// <response code="201">The newly created session state.</response>
    /// <response code="400">Invalid request body or unknown world.</response>
    [HttpPost]
    [ProducesResponseType(typeof(SessionStateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
    {
        await using var ctx = await db.CreateDbContextAsync();

        var template = await ctx.EnemyTemplates
            .Include(t => t.World)
            .FirstOrDefaultAsync(t => t.WorldId == request.WorldId);

        if (template is null)
            return BadRequest($"No enemy template found for world {request.WorldId}.");

        var user      = new User            { Username = request.PlayerName, Email = $"{request.PlayerName}@stub.local" };
        var character = new PlayerCharacter { Name = request.PlayerName, Hp = 20, AttackBonus = 3, DamageBonus = 2, ArmorClass = 15, UserId = user.Id, User = user };
        var enemy     = template.Instantiate();
        var session   = new Session
        {
            WorldId           = request.WorldId,
            UserId            = user.Id,            User            = user,
            PlayerCharacterId = character.Id,       PlayerCharacter = character,
            Enemy             = enemy,
            Round             = 1,
        };

        ctx.Sessions.Add(session);
        await ctx.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSession), new { id = session.Id }, ToDto(session));
    }

    /// <summary>Returns the current state of a session.</summary>
    /// <param name="id">The session ID returned when the session was created.</param>
    /// <response code="200">The current session state including player, enemy, and full combat log.</response>
    /// <response code="404">No session found with that ID.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SessionStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSession(Guid id)
    {
        await using var ctx = await db.CreateDbContextAsync();

        var session = await ctx.Sessions
            .Include(s => s.PlayerCharacter)
            .Include(s => s.Enemy).ThenInclude(e => e.Template)
            .Include(s => s.CombatLog)
            .FirstOrDefaultAsync(s => s.Id == id);

        return session is null ? NotFound() : Ok(ToDto(session));
    }

    /// <summary>Executes an action within a session on behalf of the player.</summary>
    /// <remarks>
    /// The engine resolves the action deterministically (dice rolls, stat checks).
    /// The updated session state is returned immediately. Narration is generated asynchronously
    /// and pushed to the client via SignalR (<c>NarrationReady</c> event on <c>/hubs/content</c>)
    /// once available — the combat log entry will initially show <c>Provider: "pending"</c>.
    /// </remarks>
    /// <param name="id">The session ID.</param>
    /// <param name="request">The action to execute.</param>
    /// <response code="200">Updated session state after the action has been resolved.</response>
    /// <response code="400">The action type is invalid or the session is no longer active.</response>
    /// <response code="404">No session found with that ID.</response>
    [HttpPost("{id}/actions")]
    [ProducesResponseType(typeof(SessionStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExecuteAction(Guid id, [FromBody] ActionRequest request)
    {
        await using var ctx = await db.CreateDbContextAsync();

        // Read pass — AsNoTracking avoids InMemory shadow-FK tracking issues
        var session = await ctx.Sessions
            .Include(s => s.PlayerCharacter)
            .Include(s => s.Enemy).ThenInclude(e => e.Template)
            .Include(s => s.CombatLog)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session is null) return NotFound();
        if (session.Status != SessionStatus.Active) return BadRequest("Session is no longer active.");

        var state = new GameState(
            Round:  session.Round,
            Player: new(session.PlayerCharacter.Hp, session.PlayerCharacter.Hp,
                        session.PlayerCharacter.AttackBonus, session.PlayerCharacter.DamageBonus,
                        session.PlayerCharacter.ArmorClass),
            Enemy:  new(session.Enemy.CurrentHp, session.Enemy.Template.BaseHp,
                        session.Enemy.Template.AttackBonus, session.Enemy.Template.DamageBonus,
                        session.Enemy.Template.ArmorClass, session.Enemy.Status)
        );

        var result = engine.Process(state, new PlayerAttackInput());

        // Write pass — load only the mutable entities by PK so EF tracks them cleanly
        var enemy   = await ctx.Set<Enemy>().FindAsync(session.Enemy.Id);
        var tracked = await ctx.Sessions.FindAsync(id);

        enemy!.CurrentHp = result.NewState.Enemy.CurrentHp;
        enemy.Status     = result.NewState.Enemy.Status;
        tracked!.Round   = result.NewState.Round;
        if (result.NewState.Enemy.Status == EnemyStatus.Dead) tracked.Complete();

        var entry = new CombatLog
        {
            Round     = result.Event.Round,
            Hit       = result.Event.Hit,
            Damage    = result.Event.Damage,
            Narration = string.Empty,
            Provider  = "pending",
            SessionId = id,
        };
        ctx.CombatLogs.Add(entry);
        await ctx.SaveChangesAsync();

        _ = orchestrator.EnqueueNarrationAsync(id, result.Event);

        // Reload for the response DTO (includes Template name, full combat log, etc.)
        var updated = await ctx.Sessions
            .Include(s => s.PlayerCharacter)
            .Include(s => s.Enemy).ThenInclude(e => e.Template)
            .Include(s => s.CombatLog)
            .AsNoTracking()
            .FirstAsync(s => s.Id == id);

        return Ok(ToDto(updated));
    }

    // ── Mapping ─────────────────────────────────────────────────────────────

    static SessionStateDto ToDto(Session s) => new(
        SessionId:  s.Id,
        Status:     s.Status.ToString(),
        Round:      s.Round,
        Player: new(
            s.PlayerCharacter.Name,
            CurrentHp:    s.PlayerCharacter.Hp,
            MaxHp:        s.PlayerCharacter.Hp,
            AttackBonus:  s.PlayerCharacter.AttackBonus,
            DamageBonus:  s.PlayerCharacter.DamageBonus,
            ArmorClass:   s.PlayerCharacter.ArmorClass
        ),
        Enemy: new(
            s.Enemy.Template.Name,
            CurrentHp:    s.Enemy.CurrentHp,
            MaxHp:        s.Enemy.Template.BaseHp,
            AttackBonus:  s.Enemy.Template.AttackBonus,
            DamageBonus:  s.Enemy.Template.DamageBonus,
            ArmorClass:   s.Enemy.Template.ArmorClass,
            Status:       s.Enemy.Status.ToString()
        ),
        CombatLog: s.CombatLog
            .OrderBy(e => e.Round)
            .Select(e => new CombatLogEntryDto(e.Round, e.Hit, e.Damage, e.Narration, e.Provider))
            .ToArray()
    );
}

/// <summary>Request body for starting a session.</summary>
/// <param name="WorldId">ID of the world the session takes place in.</param>
/// <param name="PlayerName">Display name for the player character.</param>
public record CreateSessionRequest(Guid WorldId, string PlayerName);

/// <summary>An action the player wants to execute this round.</summary>
/// <param name="Type">Action type. Currently supported: <c>attack</c>.</param>
public record ActionRequest(string Type);

/// <summary>Full state of a session at a given point in time.</summary>
/// <param name="SessionId">Unique session identifier.</param>
/// <param name="Status">Current session status: <c>Active</c>, <c>Completed</c>, or <c>Abandoned</c>.</param>
/// <param name="Round">Current round number (1-based).</param>
/// <param name="Player">Player character state.</param>
/// <param name="Enemy">Enemy state.</param>
/// <param name="CombatLog">All combat log entries for this session, ordered by round.</param>
public record SessionStateDto(
    Guid                SessionId,
    string              Status,
    int                 Round,
    PlayerDto           Player,
    EnemyDto            Enemy,
    CombatLogEntryDto[] CombatLog
);

/// <summary>Player character stats and current HP.</summary>
/// <param name="Name">Character name.</param>
/// <param name="CurrentHp">Current hit points.</param>
/// <param name="MaxHp">Maximum hit points.</param>
/// <param name="AttackBonus">Bonus added to attack rolls (d20).</param>
/// <param name="DamageBonus">Bonus added to damage rolls.</param>
/// <param name="ArmorClass">Defense value that attackers must beat to hit.</param>
public record PlayerDto(string Name, int CurrentHp, int MaxHp, int AttackBonus, int DamageBonus, int ArmorClass);

/// <summary>Enemy stats and current HP.</summary>
/// <param name="Name">Enemy name.</param>
/// <param name="CurrentHp">Current hit points.</param>
/// <param name="MaxHp">Maximum hit points.</param>
/// <param name="AttackBonus">Bonus added to attack rolls (d20).</param>
/// <param name="DamageBonus">Bonus added to damage rolls.</param>
/// <param name="ArmorClass">Defense value that attackers must beat to hit.</param>
/// <param name="Status">Current status: <c>Alive</c>, <c>Staggered</c> (HP ≤ 3), or <c>Dead</c>.</param>
public record EnemyDto(string Name, int CurrentHp, int MaxHp, int AttackBonus, int DamageBonus, int ArmorClass, string Status);

/// <summary>One round of combat as resolved by the game engine.</summary>
/// <param name="Round">The round number this entry belongs to.</param>
/// <param name="Hit">Whether the attack connected.</param>
/// <param name="Damage">Damage dealt. 0 if the attack missed.</param>
/// <param name="Narration">Description of the round. Empty while <c>Provider</c> is <c>pending</c>.</param>
/// <param name="Provider">Who generated the narration: <c>stub</c>, <c>claude</c>, or <c>pending</c>.</param>
public record CombatLogEntryDto(int Round, bool Hit, int Damage, string Narration, string Provider);
