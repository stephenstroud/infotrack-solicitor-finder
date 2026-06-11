using InfoTrack.SolicitorFinder.Application.Abstractions;
using InfoTrack.SolicitorFinder.Infrastructure.Persistence;
using InfoTrack.SolicitorFinder.Infrastructure.Scraping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace InfoTrack.SolicitorFinder.Infrastructure;

/// <summary>
/// Wires the adapters that satisfy the Application ports: the solicitors.com scraper and the
/// EF (in-memory) snapshot store. The HTML source is assembled from configuration — live HTTP
/// with fixture fallback, or fixtures only.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ScraperOptions>(configuration.GetSection(ScraperOptions.SectionName));

        // Persistence — in-memory EF; the named database lives for the app's lifetime so
        // history (and new-firm detection) works across requests within a run.
        services.AddDbContext<SolicitorFinderDbContext>(options =>
            options.UseInMemoryDatabase("solicitor-finder"));
        services.AddScoped<ISearchSnapshotStore, EfSearchSnapshotStore>();

        // Parser, configured with the base URL so it can absolutise relative links.
        services.AddSingleton(sp =>
            new SolicitorListingParser(sp.GetRequiredService<IOptions<ScraperOptions>>().Value.BaseUrl));

        // HTML sources.
        services.AddHttpClient<HttpHtmlSource>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ScraperOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0 Safari/537.36");
        });
        services.AddSingleton<FixtureHtmlSource>();

        services.AddScoped<IHtmlSource>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ScraperOptions>>().Value;
            var fixtures = sp.GetRequiredService<FixtureHtmlSource>();

            // Fixtures-only for fully offline/deterministic runs; otherwise live with fallback.
            return options.UseFixturesOnly
                ? fixtures
                : new FallbackHtmlSource(sp.GetRequiredService<HttpHtmlSource>(), fixtures);
        });

        services.AddScoped<ISolicitorSource, SolicitorScraper>();

        return services;
    }
}
