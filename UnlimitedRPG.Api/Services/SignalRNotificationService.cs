using Microsoft.AspNetCore.SignalR;
using UnlimitedRPG.Api.Hubs;
using UnlimitedRPG.Core.Interfaces;

namespace UnlimitedRPG.Api.Services;

public class SignalRNotificationService(IHubContext<ContentHub> hub, SessionStore sessions) : INotificationService
{
    public async Task SendNarrationAsync(Guid sessionId, int round, string narration, CancellationToken ct = default)
    {
        sessions.UpdateNarration(sessionId, round, narration);
        await hub.Clients.Group(sessionId.ToString())
            .SendAsync("NarrationReady", sessionId, round, narration, ct);
    }
}
