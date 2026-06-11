using InfoTrack.SolicitorFinder.Domain.Solicitors;

namespace InfoTrack.SolicitorFinder.Infrastructure.Scraping;

/// <summary>
/// Supplies the raw results-page HTML for a location. Separating "where the HTML comes from"
/// from "how it's parsed" lets the live HTTP source and the offline fixture source be swapped
/// or chained (see <see cref="FallbackHtmlSource"/>) without the parser or scraper knowing.
/// </summary>
internal interface IHtmlSource
{
    Task<string?> GetHtmlAsync(Location location, CancellationToken cancellationToken = default);
}
