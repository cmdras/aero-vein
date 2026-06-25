namespace Api.ResourceAccess;

// Seam for M1+ event streams: MissionRequested, MissionCancelled, etc.
// M2+ will add methods here (AppendAsync, WaitingListAsync) backed by IDocumentSession.Events.
public interface IMissionStore;
