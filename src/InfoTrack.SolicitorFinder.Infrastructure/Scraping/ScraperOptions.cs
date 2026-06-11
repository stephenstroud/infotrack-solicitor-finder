namespace InfoTrack.SolicitorFinder.Infrastructure.Scraping;

/// <summary>
/// Configuration for where listings are fetched from. Bound from the "Scraper" section of
/// appsettings, so the assessor can flip between live scraping and the bundled offline
/// fixtures without touching code.
/// </summary>
public sealed class ScraperOptions
{
    public const string SectionName = "Scraper";

    public string BaseUrl { get; set; } = "https://www.solicitors.com";

    /// <summary>Results path; <c>{slug}</c> is replaced with the location slug (e.g. "london").</summary>
    public string ResultsPathTemplate { get; set; } = "/{slug}-solicitors.html";

    /// <summary>
    /// "live" fetches over HTTP and falls back to a bundled fixture if the request fails;
    /// "fixture" uses only the bundled fixtures (fully offline, deterministic).
    /// </summary>
    public string Mode { get; set; } = "live";

    public int TimeoutSeconds { get; set; } = 30;

    public bool UseFixturesOnly => string.Equals(Mode, "fixture", StringComparison.OrdinalIgnoreCase);

    public string ResultsUrl(string slug) =>
        $"{BaseUrl.TrimEnd('/')}{ResultsPathTemplate.Replace("{slug}", slug)}";
}
