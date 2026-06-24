using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Api.IntegrationTests;

// Worked example for packages/api.integration.tests — see .claude/skills/app-write-tests.
// Boots the real app with WebApplicationFactory<Program> and asserts on observable HTTP
// behavior. Endpoint tests like this belong in the integration project, not the unit one.
public class RootEndpointShould : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public RootEndpointShould(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Get_Root_ReturnsApiName()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<RootResponse>();
        Assert.Equal("AeroVein API", body?.Name);
    }

    private sealed record RootResponse(string Name, string? Version);
}
