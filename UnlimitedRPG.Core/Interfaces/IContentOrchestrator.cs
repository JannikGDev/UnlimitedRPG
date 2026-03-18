namespace RpgFramework.Core.Interfaces;

public interface IContentOrchestrator
{
    Task EnqueueNarrationAsync(Guid sessionId, int round, CancellationToken ct = default);
}
