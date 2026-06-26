using System.Net.Http.Json;
using Xunit;

namespace Api.IntegrationTests;

public sealed class SitesCatalogShould : IClassFixture<PostgresApiFactory>
{
    private readonly HttpClient _client;

    public SitesCatalogShould(PostgresApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task GetSites_ReturnsFiveSitesOrderedByName()
    {
        // Arrange — factory seeds the catalog via MechelenSiteSeeds (IInitialData).

        // Act
        var response = await _client.GetAsync("/api/sites");

        // Assert
        response.EnsureSuccessStatusCode();
        var sites = await response.Content.ReadFromJsonAsync<SiteResponse[]>();
        Assert.NotNull(sites);
        Assert.Equal(5, sites.Length);
        Assert.Contains(sites, s => s.Name == "Sint-Maarten Hospital");
        var names = sites.Select(s => s.Name).ToList();
        Assert.Equal(names.OrderBy(n => n), names);
    }

    private sealed record SiteResponse(Guid Id, string Name);
}
