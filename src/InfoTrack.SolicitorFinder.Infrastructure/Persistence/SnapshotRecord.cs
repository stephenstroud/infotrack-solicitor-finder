namespace InfoTrack.SolicitorFinder.Infrastructure.Persistence;

/// <summary>
/// EF persistence shape for a search snapshot. Kept separate from the domain
/// <c>SearchSnapshot</c> so the domain model stays free of ORM concerns (keys, mutable
/// setters, parameterless construction). The store maps between the two.
/// </summary>
internal sealed class SnapshotRecord
{
    public Guid Id { get; set; }
    public DateTimeOffset CapturedAt { get; set; }

    /// <summary>Searched location names, pipe-delimited (e.g. "London|Leeds").</summary>
    public string Locations { get; set; } = string.Empty;

    public List<SolicitorRecord> Firms { get; set; } = [];
}

/// <summary>EF persistence shape for one firm within a snapshot (owned by <see cref="SnapshotRecord"/>).</summary>
internal sealed class SolicitorRecord
{
    public string Name { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string AddressRaw { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Postcode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
    public double? Stars { get; set; }
    public int? ReviewCount { get; set; }
}
