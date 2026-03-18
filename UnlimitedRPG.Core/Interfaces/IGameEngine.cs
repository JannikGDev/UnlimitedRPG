using RpgFramework.Core.Model;

namespace RpgFramework.Core.Interfaces;

public interface IGameEngine
{
    ProcessResult Process(GameState state, IInput input);
}

public record ProcessResult(GameState NewState, CombatEvent Event);
