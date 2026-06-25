using Api.Domain;
using Marten;
using Marten.Schema;

namespace Api.ResourceAccess;

// Seeded catalog for the Mechelen pilot (ADR-0004). IDs are stable so future
// foreign-key references (originSite / destSite on missions) never drift.
public sealed class MechelenSiteSeeds : IInitialData
{
    private static readonly Site[] Sites =
    [
        new(Guid.Parse("a1b2c3d4-0001-0000-0000-000000000001"), "Sint-Maarten Hospital"),
        new(Guid.Parse("a1b2c3d4-0001-0000-0000-000000000002"), "AZ Rivierenland"),
        new(Guid.Parse("a1b2c3d4-0001-0000-0000-000000000003"), "Rode Kruis Blood Bank Mechelen"),
        new(Guid.Parse("a1b2c3d4-0001-0000-0000-000000000004"), "UZ Leuven"),
        new(Guid.Parse("a1b2c3d4-0001-0000-0000-000000000005"), "Imeldaziekenhuis Bonheiden"),
    ];

    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        await using var session = store.LightweightSession();
        session.Store(Sites);
        await session.SaveChangesAsync(cancellation);
    }
}
