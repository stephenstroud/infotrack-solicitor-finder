using InfoTrack.SolicitorFinder.Domain.Search;

namespace InfoTrack.SolicitorFinder.Application.Reports;

/// <summary>
/// Projects a captured <see cref="SearchSnapshot"/> (and the previous one, for diffing)
/// into the <see cref="SearchReport"/>. Pure function of its inputs, so it's trivial to
/// unit test and carries no infrastructure dependencies.
/// </summary>
public static class SearchReportBuilder
{
    public static SearchReport Build(SearchSnapshot snapshot, SearchSnapshot? previous)
    {
        var firms = snapshot.Solicitors;

        var breakdown = firms
            .GroupBy(f => f.Location)
            .Select(g => new LocationBreakdown(
                Location: g.Key.Name,
                FirmCount: g.Count(),
                ReachableFirms: g.Count(f => f.Contact.IsReachable),
                WithPhone: g.Count(f => f.Contact.HasPhone),
                WithEmail: g.Count(f => f.Contact.HasEmail),
                WithWebsite: g.Count(f => f.Contact.HasWebsite)))
            .OrderByDescending(b => b.FirmCount)
            .ThenBy(b => b.Location)
            .ToList();

        var contactability = new ContactabilityStats(
            TotalFirms: firms.Count,
            WithPhone: firms.Count(f => f.Contact.HasPhone),
            WithEmail: firms.Count(f => f.Contact.HasEmail),
            WithWebsite: firms.Count(f => f.Contact.HasWebsite),
            Reachable: firms.Count(f => f.Contact.IsReachable));

        // National ranking by quality *and* volume of reviews (see Rating.Score).
        var topRated = firms
            .Where(f => f.Rating is not null)
            .OrderByDescending(f => f.Rating!.Score)
            .ThenByDescending(f => f.Rating!.ReviewCount)
            .Take(10)
            .Select(SolicitorView.From)
            .ToList();

        var newFirms = snapshot.NewComparedTo(previous)
            .Select(SolicitorView.From)
            .ToList();

        var views = firms
            .OrderBy(f => f.Location.Name)
            .ThenBy(f => f.Name)
            .Select(SolicitorView.From)
            .ToList();

        return new SearchReport(
            SnapshotId: snapshot.Id,
            GeneratedAt: snapshot.CapturedAt,
            TotalFirms: firms.Count,
            LocationsSearched: snapshot.Locations.Count,
            Contactability: contactability,
            Breakdown: breakdown,
            TopRated: topRated,
            NewFirms: newFirms,
            Firms: views);
    }
}
