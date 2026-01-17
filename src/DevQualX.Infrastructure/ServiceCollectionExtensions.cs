using DevQualX.Domain.Infrastructure;
using DevQualX.Infrastructure.Adapters;
using DevQualX.Infrastructure.Adapters.GitHub;
using Microsoft.Extensions.DependencyInjection;

namespace DevQualX.Infrastructure;

/// <summary>
/// Extension methods for registering infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers infrastructure layer services (adapters).
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Register Azure infrastructure adapters
        services.AddSingleton<IBlobStorageService, BlobStorageService>();
        services.AddSingleton<IMessageQueueService, ServiceBusMessageQueueService>();

        // Register GitHub adapters (scoped for per-request lifecycle)
        services.AddScoped<IGitHubOAuthService, GitHubOAuthService>();
        services.AddScoped<IGitHubApiService, GitHubApiService>();
        services.AddScoped<IGitHubAppService, GitHubAppService>();

        return services;
    }
}
