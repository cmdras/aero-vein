using Marten;

namespace Api.ResourceAccess;

public sealed class MissionStore(IDocumentSession session) : IMissionStore
{
    private readonly IDocumentSession _session = session;
    // M2+ slices will add methods here: AppendAsync, WaitingListAsync, etc.
}
