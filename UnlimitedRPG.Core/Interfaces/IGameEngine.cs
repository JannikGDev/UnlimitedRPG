using RpgFramework.Core.Model;

namespace RpgFramework.Core.Interfaces;

public interface IGameEngine
{
    AttackResult ResolveAttack(PlayerCharacter attacker, Enemy target);
}

public record AttackResult(bool Hit, int Damage, EnemyStatus ResultingStatus);
