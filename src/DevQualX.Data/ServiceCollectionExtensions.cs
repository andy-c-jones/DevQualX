using DevQualX.Data.Repositories;
using DevQualX.Domain.Data;
using Microsoft.Extensions.DependencyInjection;

namespace DevQualX.Data;

/// <summary>
/// Extension methods for configuring Data layer services in the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Data layer repositories with the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        // Register all repository implementations as scoped (one instance per request)
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IInstallationRepository, InstallationRepository>();
        services.AddScoped<IRepositoryRepository, RepositoryRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<IRoleAssignmentRepository, RoleAssignmentRepository>();
        services.AddScoped<ISolutionRepository, SolutionRepository>();
        services.AddScoped<ICSharpProjectRepository, CSharpProjectRepository>();
        services.AddScoped<IGitHubProjectRepository, GitHubProjectRepository>();

        return services;
    }
}
