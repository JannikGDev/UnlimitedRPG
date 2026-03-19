using System.Net;

namespace UnlimitedRPG.Tests;

public class SmokeTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public SmokeTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetWorlds_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/worlds");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
