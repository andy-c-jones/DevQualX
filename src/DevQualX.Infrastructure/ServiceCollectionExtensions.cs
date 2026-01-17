using DevQualX.Domain.Infrastructure;
using DevQualX.Infrastructure.Adapters;
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
        // Register infrastructure adapters
        services.AddSingleton<IBlobStorageService, BlobStorageService>();
        services.AddSingleton<IMessageQueueService, ServiceBusMessageQueueService>();

        return services;
    }
}
