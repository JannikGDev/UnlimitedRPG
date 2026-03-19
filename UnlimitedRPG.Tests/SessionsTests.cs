using System.Net;
using System.Net.Http.Json;
using UnlimitedRPG.Api.Controllers;

namespace UnlimitedRPG.Tests;

public class SessionsTests : IClassFixture<ApiFactory>
{
    // The Darklands — known world ID with a fixed enemy (Shadow Wraith)
    static readonly Guid TheDarklandsId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private readonly HttpClient _client;

    public SessionsTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateSession_InKnownWorld_ReturnsCorrectInitialState()
    {
        var response = await _client.PostAsJsonAsync("/api/sessions",
            new CreateSessionRequest(TheDarklandsId, "Aldric"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var session = await response.Content.ReadFromJsonAsync<SessionStateDto>();
        Assert.NotNull(session);
        Assert.Equal("Active", session.Status);
        Assert.Equal(1, session.Round);
        Assert.Equal("Aldric", session.Player.Name);
        Assert.Equal(20, session.Player.CurrentHp);
        Assert.Equal("Shadow Wraith", session.Enemy.Name);
        Assert.Equal("Alive", session.Enemy.Status);
        Assert.Empty(session.CombatLog);
    }

    [Fact]
    public async Task GetSession_AfterCreating_ReturnsSameState()
    {
        // Create a session
        var createResponse = await _client.PostAsJsonAsync("/api/sessions",
            new CreateSessionRequest(TheDarklandsId, "Brynn"));
        var created = await createResponse.Content.ReadFromJsonAsync<SessionStateDto>();
        Assert.NotNull(created);

        // Fetch it back
        var getResponse = await _client.GetAsync($"/api/sessions/{created.SessionId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = await getResponse.Content.ReadFromJsonAsync<SessionStateDto>();
        Assert.NotNull(fetched);
        Assert.Equal(created.SessionId, fetched.SessionId);
        Assert.Equal(created.Status, fetched.Status);
        Assert.Equal(created.Enemy.Name, fetched.Enemy.Name);
    }

    [Fact]
    public async Task ExecuteAction_Attack_AddsRoundToCombatLog()
    {
        // Create a fresh session
        var createResponse = await _client.PostAsJsonAsync("/api/sessions",
            new CreateSessionRequest(TheDarklandsId, "Cedric"));
        var session = await createResponse.Content.ReadFromJsonAsync<SessionStateDto>();
        Assert.NotNull(session);

        // Execute one attack
        var actionResponse = await _client.PostAsJsonAsync(
            $"/api/sessions/{session.SessionId}/actions",
            new ActionRequest("attack"));

        Assert.Equal(HttpStatusCode.OK, actionResponse.StatusCode);

        var updated = await actionResponse.Content.ReadFromJsonAsync<SessionStateDto>();
        Assert.NotNull(updated);
        Assert.Single(updated.CombatLog);
        Assert.Equal(1, updated.CombatLog[0].Round);
        Assert.Equal("pending", updated.CombatLog[0].Provider);
    }
}
