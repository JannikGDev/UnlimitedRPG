using Microsoft.AspNetCore.Mvc;

namespace RpgFramework.Api.Controllers;

[ApiController]
[Route("api/sessions")]
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
            new(Round: 1, Hit: true,  Damage: 3, Narration: "Thorin swings his axe and lands a solid blow on the goblin's shoulder.", Provider: "stub"),
        ]
    );

    [HttpPost]
    public IActionResult CreateSession([FromBody] CreateSessionRequest request) =>
        CreatedAtAction(nameof(GetSession), new { id = _sessionId }, _sessionState with
        {
            Round = 1,
            CombatLog = []
        });

    [HttpGet("{id}")]
    public IActionResult GetSession(Guid id) => Ok(_sessionState);

    [HttpPost("{id}/actions")]
    public IActionResult ExecuteAction(Guid id, [FromBody] ActionRequest request) =>
        Ok(_sessionState);
}

public record CreateSessionRequest(Guid WorldId, string PlayerName);
public record ActionRequest(string Type);

public record SessionStateDto(
    Guid                  SessionId,
    string                Status,
    int                   Round,
    PlayerDto             Player,
    EnemyDto              Enemy,
    CombatLogEntryDto[]   CombatLog
);

public record PlayerDto(string Name, int CurrentHp, int MaxHp, int AttackBonus, int DamageBonus, int ArmorClass);
public record EnemyDto(string Name, int CurrentHp, int MaxHp, int AttackBonus, int DamageBonus, int ArmorClass, string Status);
public record CombatLogEntryDto(int Round, bool Hit, int Damage, string Narration, string Provider);
