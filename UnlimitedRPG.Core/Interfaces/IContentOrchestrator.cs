using UnlimitedRPG.Core.Model;

namespace UnlimitedRPG.Core.Interfaces;

public interface IContentOrchestrator
{
    Task EnqueueNarrationAsync(Guid sessionId, CombatEvent combatEvent, CancellationToken ct = default);
}
