using UnlimitedRPG.Core.Inputs;
using UnlimitedRPG.Core.Interfaces;
using UnlimitedRPG.Core.Model;

namespace UnlimitedRPG.Api.Engine;

public class GameEngine : IGameEngine
{
    public ProcessResult Process(GameState state, IInput input)
    {
        var rng    = Random.Shared;
        var roll   = rng.Next(1, 21);
        var hit    = roll + state.Player.AttackBonus >= state.Enemy.ArmorClass;
        var damage = hit ? Math.Max(1, rng.Next(1, 7) + state.Player.DamageBonus) : 0;

        var newHp     = Math.Max(0, state.Enemy.CurrentHp - damage);
        var newStatus = newHp <= 0 ? EnemyStatus.Dead
            : newHp <= 3 ? EnemyStatus.Staggered
            : EnemyStatus.Alive;

        var newState = state with
        {
            Round  = state.Round + 1,
            Enemy  = state.Enemy with { CurrentHp = newHp, Status = newStatus }
        };

        return new ProcessResult(newState, new CombatEvent(state.Round, hit, damage));
    }
}
