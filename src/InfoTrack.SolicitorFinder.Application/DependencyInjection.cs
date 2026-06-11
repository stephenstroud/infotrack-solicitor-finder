using InfoTrack.SolicitorFinder.Application.SearchSolicitors;
using Microsoft.Extensions.DependencyInjection;

namespace InfoTrack.SolicitorFinder.Application;

/// <summary>
/// Registers the application's use cases. Ports (ISolicitorSource, ISearchSnapshotStore)
/// are supplied by the Infrastructure layer, keeping this layer free of adapters.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<SearchSolicitorsHandler>();
        return services;
    }
}
