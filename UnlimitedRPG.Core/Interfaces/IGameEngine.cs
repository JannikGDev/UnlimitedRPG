using UnlimitedRPG.Core.Model;

namespace UnlimitedRPG.Core.Interfaces;

public interface IGameEngine
{
    ProcessResult Process(GameState state, IInput input);
}

public record ProcessResult(GameState NewState, CombatEvent Event);
