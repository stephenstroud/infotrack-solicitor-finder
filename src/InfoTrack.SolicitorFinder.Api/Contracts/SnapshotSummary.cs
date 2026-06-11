namespace InfoTrack.SolicitorFinder.Api.Contracts;

/// <summary>Lightweight history row: one past search run, without the full firm list.</summary>
public sealed record SnapshotSummary(
    Guid Id,
    DateTimeOffset CapturedAt,
    int LocationsSearched,
    int FirmsFound);
