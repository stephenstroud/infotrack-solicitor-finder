using InfoTrack.SolicitorFinder.Application.Abstractions;
using InfoTrack.SolicitorFinder.Domain.Solicitors;
using Microsoft.Extensions.Logging;

namespace InfoTrack.SolicitorFinder.Infrastructure.Scraping;

/// <summary>
/// The <see cref="ISolicitorSource"/> adapter for solicitors.com: gets a location's results
/// HTML (from whichever <see cref="IHtmlSource"/> is configured) and hands it to the parser.
/// Holds no parsing or transport detail itself — those are its two collaborators.
/// </summary>
internal sealed class SolicitorScraper(
    IHtmlSource htmlSource,
    SolicitorListingParser parser,
    ILogger<SolicitorScraper> logger) : ISolicitorSource
{
    public async Task<IReadOnlyList<Solicitor>> GetByLocationAsync(
        Location location,
        CancellationToken cancellationToken = default)
    {
        var html = await htmlSource.GetHtmlAsync(location, cancellationToken);
        if (string.IsNullOrEmpty(html))
        {
            logger.LogWarning("No results HTML available for {Location}.", location);
            return [];
        }

        var firms = parser.Parse(html, location);
        logger.LogInformation("Parsed {Count} firms for {Location}.", firms.Count, location);
        return firms;
    }
}
