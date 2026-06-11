namespace InfoTrack.SolicitorFinder.Application.Reports;

/// <summary>
/// A firm in the national top-rated ranking. A firm with offices across several of the searched
/// locations appears once here, listing all the regions it was found in (rather than once per
/// region), so the ranking reflects firms nationally rather than location-by-location.
/// </summary>
public sealed record RankedFirm(
    string Name,
    double Stars,
    int ReviewCount,
    IReadOnlyList<string> Locations)
{
    public int RegionCount => Locations.Count;
}
