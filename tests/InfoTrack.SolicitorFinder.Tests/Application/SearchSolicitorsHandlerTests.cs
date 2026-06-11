using InfoTrack.SolicitorFinder.Application.Abstractions;
using InfoTrack.SolicitorFinder.Application.SearchSolicitors;
using InfoTrack.SolicitorFinder.Domain.Search;
using InfoTrack.SolicitorFinder.Domain.Solicitors;
using Microsoft.Extensions.Logging.Abstractions;

namespace InfoTrack.SolicitorFinder.Tests.Application;

public class SearchSolicitorsHandlerTests
{
    private static Solicitor Firm(string name, string location, bool phone = true, double? stars = null) =>
        new(name,
            new Location(location),
            Address.FromRaw($"1 High Street, {location}, AB1 2CD"),
            new ContactDetails(phone: phone ? "0123" : null, website: "http://x"),
            description: null,
            rating: stars is { } s ? new Rating(s, 100) : null);

    private static SearchSolicitorsHandler HandlerFor(ISolicitorSource source, ISearchSnapshotStore store) =>
        new(source, store, NullLogger<SearchSolicitorsHandler>.Instance);

    [Fact]
    public async Task Deduplicates_the_same_firm_returned_more_than_once()
    {
        var source = new StubSource
        {
            ["london"] = [Firm("Acme Law", "London"), Firm("Acme Law", "London"), Firm("Beta Legal", "London")]
        };
        var report = await HandlerFor(source, new InMemorySnapshotStore()).HandleAsync(["London"]);

        Assert.Equal(2, report.TotalFirms); // the duplicate Acme Law collapses to one
    }

    [Fact]
    public async Task First_run_reports_no_new_firms_then_only_genuinely_new_ones_appear()
    {
        var store = new InMemorySnapshotStore();
        var source = new StubSource { ["london"] = [Firm("Acme Law", "London")] };

        var first = await HandlerFor(source, store).HandleAsync(["London"]);
        Assert.Empty(first.NewFirms); // baseline run

        source["london"] = [Firm("Acme Law", "London"), Firm("New Firm", "London")];
        var second = await HandlerFor(source, store).HandleAsync(["London"]);

        var newFirm = Assert.Single(second.NewFirms);
        Assert.Equal("New Firm", newFirm.Name);
    }

    [Fact]
    public async Task Aggregates_locations_and_ranks_top_rated_by_quality_and_volume()
    {
        var source = new StubSource
        {
            ["london"] = [Firm("Five Star Few", "London", stars: 5.0)],
            ["leeds"] = [Firm("Strong And Popular", "Leeds", stars: 4.5)]
        };
        // Give the popular firm far more reviews so its weighted score wins.
        source["leeds"] = [new("Strong And Popular", new Location("Leeds"),
            Address.Empty, new ContactDetails(phone: "1"), null, new Rating(4.5, 10_000))];
        source["london"] = [new("Five Star Few", new Location("London"),
            Address.Empty, new ContactDetails(phone: "1"), null, new Rating(5.0, 2))];

        var report = await HandlerFor(source, new InMemorySnapshotStore()).HandleAsync(["London", "Leeds"]);

        Assert.Equal(2, report.LocationsSearched);
        Assert.Equal("Strong And Popular", report.TopRated[0].Name); // volume beats a lone 5★
    }

    [Fact]
    public async Task Rejects_an_empty_location_list()
    {
        var handler = HandlerFor(new StubSource(), new InMemorySnapshotStore());
        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync([]));
    }
}

/// <summary>Returns preset firms per location slug; missing locations yield nothing.</summary>
file sealed class StubSource : Dictionary<string, IReadOnlyList<Solicitor>>, ISolicitorSource
{
    public Task<IReadOnlyList<Solicitor>> GetByLocationAsync(Location location, CancellationToken cancellationToken = default) =>
        Task.FromResult(TryGetValue(location.Slug, out var firms) ? firms : []);
}

/// <summary>In-memory snapshot store standing in for the EF adapter.</summary>
file sealed class InMemorySnapshotStore : ISearchSnapshotStore
{
    private readonly List<SearchSnapshot> _snapshots = [];

    public Task SaveAsync(SearchSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        _snapshots.Add(snapshot);
        return Task.CompletedTask;
    }

    public Task<SearchSnapshot?> GetLatestAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_snapshots.OrderByDescending(s => s.CapturedAt).FirstOrDefault());

    public Task<IReadOnlyList<SearchSnapshot>> GetHistoryAsync(int limit = 20, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<SearchSnapshot>>(
            _snapshots.OrderByDescending(s => s.CapturedAt).Take(limit).ToList());
}
