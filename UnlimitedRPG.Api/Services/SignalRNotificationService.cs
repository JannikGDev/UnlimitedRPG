using Microsoft.AspNetCore.SignalR;
using UnlimitedRPG.Api.Hubs;
using UnlimitedRPG.Core.Interfaces;

namespace UnlimitedRPG.Api.Services;

public class SignalRNotificationService(IHubContext<ContentHub> hub) : INotificationService
{
    public Task SendNarrationAsync(Guid sessionId, int round, string narration, CancellationToken ct = default) =>
        hub.Clients.Group(sessionId.ToString())
            .SendAsync("NarrationReady", sessionId, round, narration, ct);
}
