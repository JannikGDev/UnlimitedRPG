using System.Net;
using System.Net.Http.Json;
using UnlimitedRPG.Api.Controllers;

namespace UnlimitedRPG.Tests;

public class WorldsTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public WorldsTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetWorlds_ReturnsBothWorldsWithCorrectNames()
    {
        var worlds = await _client.GetFromJsonAsync<WorldDto[]>("/api/worlds");

        Assert.NotNull(worlds);
        Assert.Equal(2, worlds.Length);
        Assert.Contains(worlds, w => w.Name == "The Darklands");
        Assert.Contains(worlds, w => w.Name == "Sunken Vale");
    }

    [Fact]
    public async Task CreateWorld_ReturnsCreatedWorldWithGivenName()
    {
        var response = await _client.PostAsJsonAsync("/api/worlds", new CreateWorldRequest("The Ashen Wastes"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var world = await response.Content.ReadFromJsonAsync<WorldDto>();
        Assert.NotNull(world);
        Assert.Equal("The Ashen Wastes", world.Name);
    }
}
