using Microsoft.AspNetCore.SignalR;

namespace UnlimitedRPG.Api.Hubs;

public class ContentHub : Hub
{
    public Task JoinSession(Guid sessionId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, sessionId.ToString());
}
