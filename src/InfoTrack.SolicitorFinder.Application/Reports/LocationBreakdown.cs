namespace InfoTrack.SolicitorFinder.Application.Reports;

/// <summary>
/// Per-location slice of the report: how many firms, and how reachable they are.
/// This is the row a CEO scans to decide which city has the richest set of leads.
/// </summary>
public sealed record LocationBreakdown(
    string Location,
    int FirmCount,
    int ReachableFirms,
    int WithPhone,
    int WithEmail,
    int WithWebsite)
{
    /// <summary>Share of firms in this location we can actually contact (0–100).</summary>
    public int ReachablePercent => FirmCount == 0 ? 0 : (int)Math.Round(100.0 * ReachableFirms / FirmCount);
}
