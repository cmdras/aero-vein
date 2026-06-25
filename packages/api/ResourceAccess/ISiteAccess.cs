using Api.Domain;

namespace Api.ResourceAccess;

public interface ISiteAccess
{
    Task<IReadOnlyList<Site>> ListAsync(CancellationToken ct = default);
}
