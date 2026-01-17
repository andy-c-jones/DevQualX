using DevQualX.Application.Authentication;
using DevQualX.Application.Authorization;
using DevQualX.Application.Installation;
using DevQualX.Application.Reports;
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
        // Authentication services
        services.AddScoped<IInitiateOAuth, InitiateOAuth>();
        services.AddScoped<ICompleteOAuth, CompleteOAuth>();
        services.AddScoped<ISignOutUser, SignOutUser>();
        
        // Installation services
        services.AddScoped<IGetUserInstallations, GetUserInstallations>();
        services.AddScoped<ISyncInstallationData, SyncInstallationData>();
        
        // Authorization services
        services.AddScoped<ICheckUserRole, CheckUserRole>();
        services.AddScoped<IGetUserPermissions, GetUserPermissions>();
        
        // Weather services (example)
        services.AddScoped<IGetWeatherForecast, GetWeatherForecast>();
        
        // Report services
        services.AddScoped<IUploadReport, UploadReport>();
        services.AddScoped<IProcessReport, ProcessReport>();

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
