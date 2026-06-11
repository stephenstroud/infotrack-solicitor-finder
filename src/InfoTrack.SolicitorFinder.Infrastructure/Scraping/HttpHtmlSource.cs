using InfoTrack.SolicitorFinder.Domain.Solicitors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InfoTrack.SolicitorFinder.Infrastructure.Scraping;

/// <summary>Fetches the live results page over HTTP. Returns null (rather than throwing) on any
/// failure, so a fallback source can take over and one bad request never sinks the search.</summary>
internal sealed class HttpHtmlSource(
    HttpClient http,
    IOptions<ScraperOptions> options,
    ILogger<HttpHtmlSource> logger) : IHtmlSource
{
    private readonly ScraperOptions _options = options.Value;

    public async Task<string?> GetHtmlAsync(Location location, CancellationToken cancellationToken = default)
    {
        var url = _options.ResultsUrl(location.Slug);
        try
        {
            using var response = await http.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Live fetch for {Location} returned {Status}.", location, (int)response.StatusCode);
                return null;
            }

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Live fetch for {Location} failed ({Url}).", location, url);
            return null;
        }
    }
}
