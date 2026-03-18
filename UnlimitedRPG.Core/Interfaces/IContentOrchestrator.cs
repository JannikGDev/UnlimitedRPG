using RpgFramework.Core.Model;

namespace RpgFramework.Core.Interfaces;

public interface IContentOrchestrator
{
    Task EnqueueNarrationAsync(Guid sessionId, CombatEvent combatEvent, CancellationToken ct = default);
}
