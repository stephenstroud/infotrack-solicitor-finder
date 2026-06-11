using InfoTrack.SolicitorFinder.Application.Abstractions;
using InfoTrack.SolicitorFinder.Application.Reports;
using InfoTrack.SolicitorFinder.Domain.Search;
using InfoTrack.SolicitorFinder.Domain.Solicitors;
using Microsoft.Extensions.Logging;

namespace InfoTrack.SolicitorFinder.Application.SearchSolicitors;

/// <summary>
/// The core use case: given a list of locations, gather solicitors from the configured
/// source, de-duplicate, persist a snapshot for history, and return the insight report
/// (including firms newly appeared since the previous run).
/// </summary>
public sealed class SearchSolicitorsHandler(
    ISolicitorSource source,
    ISearchSnapshotStore store,
    ILogger<SearchSolicitorsHandler> logger)
{
    public async Task<SearchReport> HandleAsync(
        IReadOnlyList<string> locationNames,
        CancellationToken cancellationToken = default)
    {
        var locations = NormaliseLocations(locationNames);
        if (locations.Count == 0)
            throw new ArgumentException("At least one location is required.", nameof(locationNames));

        var previous = await store.GetLatestAsync(cancellationToken);

        var found = new List<Solicitor>();
        foreach (var location in locations)
        {
            // Sequential by design: one polite request per location rather than hammering
            // the source. The work is I/O-bound and the location list is small.
            try
            {
                var firms = await source.GetByLocationAsync(location, cancellationToken);
                found.AddRange(firms);
                logger.LogInformation("Found {Count} firms for {Location}.", firms.Count, location);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // One bad location shouldn't sink the whole report.
                logger.LogWarning(ex, "Failed to source firms for {Location}; skipping.", location);
            }
        }

        var deduped = found
            .GroupBy(s => s.Key)
            .Select(g => g.First())
            .ToList();

        var snapshot = SearchSnapshot.Create(locations, deduped);
        await store.SaveAsync(snapshot, cancellationToken);

        return SearchReportBuilder.Build(snapshot, previous);
    }

    private static List<Location> NormaliseLocations(IReadOnlyList<string> names)
    {
        if (names is null) return [];

        return names
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => new Location(n))
            .DistinctBy(l => l.Slug)
            .ToList();
    }
}
