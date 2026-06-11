using InfoTrack.SolicitorFinder.Domain.Search;
using InfoTrack.SolicitorFinder.Domain.Solicitors;

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

        // National ranking by quality *and* volume of reviews (see Rating.Score). The rating is
        // firm-wide, so group by firm name and rank once: a firm qualifies if it is rated in at
        // least one location, and we list every location it appears in as its regional coverage.
        var topRated = firms
            .GroupBy(f => Solicitor.CanonicalName(f.Name))
            .Where(group => group.Any(f => f.Rating is not null))
            .Select(group => new
            {
                Representative = group
                    .Where(f => f.Rating is not null)
                    .OrderByDescending(f => f.Rating!.ReviewCount)
                    .First(),
                Locations = group.Select(f => f.Location.Name).Distinct().OrderBy(name => name).ToList()
            })
            .OrderByDescending(x => x.Representative.Rating!.Score)
            .ThenByDescending(x => x.Representative.Rating!.ReviewCount)
            .Take(10)
            .Select(x => new RankedFirm(
                x.Representative.Name,
                x.Representative.Rating!.Stars,
                x.Representative.Rating.ReviewCount,
                x.Locations))
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
