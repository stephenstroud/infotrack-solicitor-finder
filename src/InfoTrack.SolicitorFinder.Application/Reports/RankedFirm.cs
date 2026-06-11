namespace InfoTrack.SolicitorFinder.Application.Reports;

/// <summary>
/// A firm in the national top-rated ranking. The rating is firm-wide, so a firm with offices
/// across several of the searched locations appears once with a single rating, listing the
/// regions it was found in (rather than once per region).
/// </summary>
public sealed record RankedFirm(
    string Name,
    double Stars,
    int ReviewCount,
    IReadOnlyList<string> Locations)
{
    public int RegionCount => Locations.Count;
}
