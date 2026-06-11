namespace InfoTrack.SolicitorFinder.Application.Reports;

/// <summary>
/// The standard report returned for a search: headline totals, a per-location breakdown,
/// contactability insight, any newly-appeared firms versus the previous run, and the full
/// firm list for the detail table.
/// </summary>
public sealed record SearchReport(
    Guid SnapshotId,
    DateTimeOffset GeneratedAt,
    int TotalFirms,
    int LocationsSearched,
    ContactabilityStats Contactability,
    IReadOnlyList<LocationBreakdown> Breakdown,
    IReadOnlyList<SolicitorView> TopRated,
    IReadOnlyList<SolicitorView> NewFirms,
    IReadOnlyList<SolicitorView> Firms)
{
    /// <summary>Location with the most firms — the strongest single lead pool.</summary>
    public string? TopLocation => Breakdown.Count == 0 ? null : Breakdown[0].Location;

    public bool HasNewFirms => NewFirms.Count > 0;
}
