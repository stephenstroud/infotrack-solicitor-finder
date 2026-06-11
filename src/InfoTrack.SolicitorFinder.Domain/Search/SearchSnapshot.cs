using InfoTrack.SolicitorFinder.Domain.Solicitors;

namespace InfoTrack.SolicitorFinder.Domain.Search;

/// <summary>
/// An immutable record of one search run: the locations requested and the firms found
/// at that moment. Snapshots are the unit of history — comparing the latest against the
/// previous one is what powers "new firm" detection.
/// </summary>
public sealed class SearchSnapshot
{
    public Guid Id { get; }
    public DateTimeOffset CapturedAt { get; }
    public IReadOnlyList<Location> Locations { get; }
    public IReadOnlyList<Solicitor> Solicitors { get; }

    public SearchSnapshot(
        Guid id,
        DateTimeOffset capturedAt,
        IReadOnlyList<Location> locations,
        IReadOnlyList<Solicitor> solicitors)
    {
        Id = id;
        CapturedAt = capturedAt;
        Locations = locations ?? [];
        Solicitors = solicitors ?? [];
    }

    public static SearchSnapshot Create(
        IReadOnlyList<Location> locations,
        IReadOnlyList<Solicitor> solicitors) =>
        new(Guid.NewGuid(), DateTimeOffset.UtcNow, locations, solicitors);

    /// <summary>
    /// Firms present here but absent from <paramref name="previous"/>, by firm identity.
    /// When there is no previous snapshot we treat nothing as "new", so the very first
    /// run establishes a baseline rather than flagging every firm as an alert.
    /// </summary>
    public IReadOnlyList<Solicitor> NewComparedTo(SearchSnapshot? previous)
    {
        if (previous is null) return [];

        var seen = previous.Solicitors.Select(s => s.Key).ToHashSet();
        return Solicitors.Where(s => !seen.Contains(s.Key)).ToList();
    }
}
