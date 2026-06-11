namespace InfoTrack.SolicitorFinder.Application.Reports;

/// <summary>
/// A firm in the national top-rated ranking. A firm with offices across several of the searched
/// locations appears once here, with all the regions it was found in — each carrying that
/// office's own rating (the site lists some offices without one), so the expanded view shows
/// where the firm is rated and where it isn't.
/// </summary>
public sealed record RankedFirm(
    string Name,
    double Stars,
    int ReviewCount,
    IReadOnlyList<RankedFirmRegion> Regions)
{
    public int RegionCount => Regions.Count;
}

/// <summary>One office of a ranked firm, in a single location, with that listing's rating.</summary>
public sealed record RankedFirmRegion(string Location, double? Stars, int? ReviewCount);
