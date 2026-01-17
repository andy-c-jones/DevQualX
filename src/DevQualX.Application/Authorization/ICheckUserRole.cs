using DevQualX.Domain.Models;
using DevQualX.Functional;

namespace DevQualX.Application.Authorization;

/// <summary>
/// Checks if a user has a specific role (or higher) for a resource.
/// Considers role hierarchy: Owner > Admin > Maintainer > Reader.
/// </summary>
public interface ICheckUserRole
{
    /// <summary>
    /// Checks if a user has at least the minimum required role.
    /// </summary>
    /// <param name="userId">Internal user ID.</param>
    /// <param name="installationId">Installation ID.</param>
    /// <param name="minimumRole">Minimum required role.</param>
    /// <param name="scope">Scope of the role check (Organization, Repository, GitHubProject).</param>
    /// <param name="resourceId">Optional resource ID (for Repository or GitHubProject scope).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success with true if user has role, false if not; or failure with error.</returns>
    Task<Result<bool, Error>> ExecuteAsync(
        int userId,
        int installationId,
        Role minimumRole,
        RoleScope scope,
        int? resourceId = null,
        CancellationToken cancellationToken = default);
}
