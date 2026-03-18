using RpgFramework.Core.Model;

namespace RpgFramework.Core.Interfaces;

public interface IGameEngine
{
    GameState Process(GameState state, IInput input);
}
