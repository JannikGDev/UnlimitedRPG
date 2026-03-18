namespace RpgFramework.Core.Interfaces;

public interface INotificationService
{
    Task SendNarrationAsync(Guid sessionId, int round, string narration, CancellationToken ct = default);
}
