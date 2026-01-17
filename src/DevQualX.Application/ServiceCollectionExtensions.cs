using DevQualX.Application.Weather;
using DevQualX.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DevQualX.Application;

/// <summary>
/// Extension methods for registering application services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers application layer services (IDD application services).
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register application services (IDD pattern - transient or scoped)
        services.AddScoped<IGetWeatherForecast, GetWeatherForecast>();

        return services;
    }

    /// <summary>
    /// Registers domain layer services.
    /// </summary>
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // Register domain services
        services.AddSingleton<IWeatherService, WeatherService>();

        return services;
    }
}
