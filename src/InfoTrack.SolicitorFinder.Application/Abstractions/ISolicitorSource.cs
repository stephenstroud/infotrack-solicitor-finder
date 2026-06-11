using InfoTrack.SolicitorFinder.Domain.Solicitors;

namespace InfoTrack.SolicitorFinder.Application.Abstractions;

/// <summary>
/// A source of solicitor listings for a location. The live solicitors.com scraper is one
/// implementation; a fixture-backed source is another. Adding "scrape another site" later
/// is just a new adapter behind this same port — no change to the orchestration.
/// </summary>
public interface ISolicitorSource
{
    Task<IReadOnlyList<Solicitor>> GetByLocationAsync(
        Location location,
        CancellationToken cancellationToken = default);
}
