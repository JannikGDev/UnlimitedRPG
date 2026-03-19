using System.Net.Http.Json;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using UnlimitedRPG.Api.Controllers;

namespace UnlimitedRPG.Tests;

public class NarrationTests : IClassFixture<ApiFactory>
{
    static readonly Guid TheDarklandsId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public NarrationTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ExecuteAction_NarrationArrivesViaSignalR()
    {
        // Connect to the hub through the in-process test server
        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(_factory.Server.BaseAddress, "hubs/content"), options =>
            {
                options.Transports = HttpTransportType.LongPolling;
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        var tcs = new TaskCompletionSource<(Guid SessionId, int Round, string Narration)>();
        connection.On<Guid, int, string>("NarrationReady", (sid, round, narration) =>
            tcs.TrySetResult((sid, round, narration)));

        await connection.StartAsync();

        // Create a fresh session
        var createResponse = await _client.PostAsJsonAsync("/api/sessions",
            new CreateSessionRequest(TheDarklandsId, "Ewin"));
        var session = await createResponse.Content.ReadFromJsonAsync<SessionStateDto>();
        Assert.NotNull(session);

        // Subscribe to narration for this session
        await connection.InvokeAsync("JoinSession", session.SessionId);

        // Execute an attack — narration is enqueued asynchronously (~200 ms stub delay)
        await _client.PostAsJsonAsync(
            $"/api/sessions/{session.SessionId}/actions",
            new ActionRequest("attack"));

        // Wait for the push (stub takes ~200ms; 5s ceiling)
        var winner = await Task.WhenAny(tcs.Task, Task.Delay(5_000));
        Assert.Same(tcs.Task, winner); // fails here if no push arrived in time

        var (sid, round, narration) = await tcs.Task;
        Assert.Equal(session.SessionId, sid);
        Assert.Equal(1, round);
        Assert.NotEmpty(narration);

        await connection.DisposeAsync();
    }
}
