using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UnlimitedRPG.Api.Hubs;
using UnlimitedRPG.Core.Interfaces;
using UnlimitedRPG.Database;

namespace UnlimitedRPG.Api.Services;

public class SignalRNotificationService(IHubContext<ContentHub> hub, IDbContextFactory<RPGContext> db) : INotificationService
{
    public async Task SendNarrationAsync(Guid sessionId, int round, string narration, CancellationToken ct = default)
    {
        await using var ctx = await db.CreateDbContextAsync(ct);

        var entry = await ctx.CombatLogs
            .FirstOrDefaultAsync(l => l.SessionId == sessionId && l.Round == round, ct);

        if (entry is not null)
        {
            entry.Narration = narration;
            entry.Provider  = "stub";
            await ctx.SaveChangesAsync(ct);
        }

        await hub.Clients.Group(sessionId.ToString())
            .SendAsync("NarrationReady", sessionId, round, narration, ct);
    }
}
