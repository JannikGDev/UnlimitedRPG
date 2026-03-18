using Microsoft.AspNetCore.Mvc;

namespace RpgFramework.Api.Controllers;

/// <summary>Manage combat sessions. A session ties a player character to a world and an enemy encounter.</summary>
[ApiController]
[Route("api/sessions")]
[Produces("application/json")]
public class SessionsController : ControllerBase
{
    static readonly Guid _sessionId = Guid.Parse("00000000-0000-0000-0000-000000000010");

    static readonly SessionStateDto _sessionState = new(
        SessionId:  _sessionId,
        Status:     "Active",
        Round:      2,
        Player:     new("Thorin", CurrentHp: 18, MaxHp: 20, AttackBonus: 3, DamageBonus: 2, ArmorClass: 15),
        Enemy:      new("Goblin Scout", CurrentHp: 5, MaxHp: 8, AttackBonus: 1, DamageBonus: 0, ArmorClass: 12, Status: "Staggered"),
        CombatLog:
        [
            new(Round: 1, Hit: true, Damage: 3, Narration: "Thorin swings his axe and lands a solid blow on the goblin's shoulder.", Provider: "stub"),
        ]
    );

    /// <summary>Starts a new session in the given world for the given player.</summary>
    /// <remarks>
    /// The session begins at round 1 with a fresh enemy spawned from the world's enemy pool.
    /// The combat log is empty until the first action is executed.
    /// </remarks>
    /// <response code="201">The newly created session state.</response>
    /// <response code="400">Invalid request body.</response>
    [HttpPost]
    [ProducesResponseType(typeof(SessionStateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CreateSession([FromBody] CreateSessionRequest request) =>
        CreatedAtAction(nameof(GetSession), new { id = _sessionId }, _sessionState with
        {
            Round = 1,
            CombatLog = []
        });

    /// <summary>Returns the current state of a session.</summary>
    /// <param name="id">The session ID returned when the session was created.</param>
    /// <response code="200">The current session state including player, enemy, and full combat log.</response>
    /// <response code="404">No session found with that ID.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SessionStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetSession(Guid id) => Ok(_sessionState);

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
    public IActionResult ExecuteAction(Guid id, [FromBody] ActionRequest request) =>
        Ok(_sessionState);
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
    Guid                  SessionId,
    string                Status,
    int                   Round,
    PlayerDto             Player,
    EnemyDto              Enemy,
    CombatLogEntryDto[]   CombatLog
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
/// <param name="Narration">AI-generated description of the round. Empty string while pending.</param>
/// <param name="Provider">Who generated the narration: <c>stub</c>, <c>claude</c>, or <c>pending</c>.</param>
public record CombatLogEntryDto(int Round, bool Hit, int Damage, string Narration, string Provider);
