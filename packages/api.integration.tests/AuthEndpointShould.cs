using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Api.IntegrationTests;

// Pins the documented behavior of /api/auth/me: 401 until Microsoft OAuth is wired
// (Program.cs, ARCHITECTURE §6.2). When auth lands, this is the contract to update.
public class AuthEndpointShould : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthEndpointShould(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task GetMe_WhenUnauthenticated_Returns401()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
