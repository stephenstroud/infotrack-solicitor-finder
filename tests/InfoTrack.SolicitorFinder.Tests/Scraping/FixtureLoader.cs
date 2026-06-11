using System.Reflection;
using InfoTrack.SolicitorFinder.Infrastructure.Scraping;

namespace InfoTrack.SolicitorFinder.Tests.Scraping;

/// <summary>Loads the results-page fixtures embedded in the Infrastructure assembly.</summary>
internal static class FixtureLoader
{
    private static readonly Assembly InfrastructureAssembly = typeof(SolicitorListingParser).Assembly;

    public static string Load(string slug)
    {
        var suffix = $".Fixtures.{slug}-solicitors.html";
        var resource = InfrastructureAssembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"No embedded fixture for '{slug}'.");

        using var stream = InfrastructureAssembly.GetManifestResourceStream(resource)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
