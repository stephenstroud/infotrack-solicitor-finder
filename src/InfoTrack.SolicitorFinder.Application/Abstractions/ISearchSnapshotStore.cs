using InfoTrack.SolicitorFinder.Domain.Search;

namespace InfoTrack.SolicitorFinder.Application.Abstractions;

/// <summary>
/// Persists search snapshots and retrieves prior ones. Backed by EF Core (in-memory) here,
/// but the orchestration only depends on this port, so swapping to SQL/Postgres is a
/// configuration change rather than a code change.
/// </summary>
public interface ISearchSnapshotStore
{
    Task SaveAsync(SearchSnapshot snapshot, CancellationToken cancellationToken = default);

    /// <summary>The most recent snapshot, or null if none has been captured yet.</summary>
    Task<SearchSnapshot?> GetLatestAsync(CancellationToken cancellationToken = default);

    /// <summary>Recent snapshots, newest first, for the history view.</summary>
    Task<IReadOnlyList<SearchSnapshot>> GetHistoryAsync(
        int limit = 20,
        CancellationToken cancellationToken = default);
}
