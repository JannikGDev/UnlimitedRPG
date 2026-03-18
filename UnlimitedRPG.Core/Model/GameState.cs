using RpgFramework.Core.Model;

namespace RpgFramework.Core.Model;

public record GameState(
    int          Round,
    PlayerState  Player,
    EnemyState   Enemy
);

public record PlayerState(
    int CurrentHp,
    int MaxHp,
    int AttackBonus,
    int DamageBonus,
    int ArmorClass
);

public record EnemyState(
    int         CurrentHp,
    int         MaxHp,
    int         AttackBonus,
    int         DamageBonus,
    int         ArmorClass,
    EnemyStatus Status
);
