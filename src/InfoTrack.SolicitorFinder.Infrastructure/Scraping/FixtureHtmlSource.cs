using System.Reflection;
using InfoTrack.SolicitorFinder.Domain.Solicitors;
using Microsoft.Extensions.Logging;

namespace InfoTrack.SolicitorFinder.Infrastructure.Scraping;

/// <summary>
/// Serves results-page HTML from fixtures embedded in this assembly (captured from the live
/// site). This is what makes the app run fully offline with zero configuration — the brief's
/// top submission check — and gives the parser tests a deterministic input.
/// </summary>
internal sealed class FixtureHtmlSource(ILogger<FixtureHtmlSource> logger) : IHtmlSource
{
    private static readonly Assembly Assembly = typeof(FixtureHtmlSource).Assembly;

    public async Task<string?> GetHtmlAsync(Location location, CancellationToken cancellationToken = default)
    {
        var suffix = $".Fixtures.{location.Slug}-solicitors.html";
        var resource = Assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));

        if (resource is null)
        {
            logger.LogInformation("No bundled fixture for {Location}.", location);
            return null;
        }

        await using var stream = Assembly.GetManifestResourceStream(resource)!;
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}
