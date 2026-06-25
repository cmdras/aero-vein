using Api.Domain;
using Marten;

namespace Api.ResourceAccess;

public sealed class SiteAccess(IQuerySession session) : ISiteAccess
{
    public async Task<IReadOnlyList<Site>> ListAsync(CancellationToken cancellationToken = default)
        => await session.Query<Site>().OrderBy(s => s.Name).ToListAsync(cancellationToken);
}
