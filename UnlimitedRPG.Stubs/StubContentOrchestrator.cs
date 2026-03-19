using UnlimitedRPG.Core.Interfaces;
using UnlimitedRPG.Core.Model;

namespace UnlimitedRPG.Stubs;

public class StubContentOrchestrator(ILlmAdapter llm, INotificationService notifications) : IContentOrchestrator
{
    public Task EnqueueNarrationAsync(Guid sessionId, CombatEvent combatEvent, CancellationToken ct = default)
    {
        _ = Task.Run(async () =>
        {
            var narration = await llm.GenerateTextAsync(string.Empty, CancellationToken.None);
            await notifications.SendNarrationAsync(sessionId, combatEvent.Round, narration, CancellationToken.None);
        });

        return Task.CompletedTask;
    }
}
