using InfoTrack.SolicitorFinder.Domain.Solicitors;

namespace InfoTrack.SolicitorFinder.Infrastructure.Scraping;

/// <summary>
/// Tries the primary source (live HTTP) and, if it yields nothing, the fallback (fixtures).
/// Keeps the live scrape as the real behaviour while guaranteeing the demo still works when
/// the site is slow, blocked, or the machine is offline.
/// </summary>
internal sealed class FallbackHtmlSource(IHtmlSource primary, IHtmlSource fallback) : IHtmlSource
{
    public async Task<string?> GetHtmlAsync(Location location, CancellationToken cancellationToken = default)
    {
        var html = await primary.GetHtmlAsync(location, cancellationToken);
        return !string.IsNullOrEmpty(html)
            ? html
            : await fallback.GetHtmlAsync(location, cancellationToken);
    }
}
