using DevQualX.Domain.Models;
using DevQualX.Functional;

namespace DevQualX.Application.Authorization;

/// <summary>
/// Gets all effective permissions for a user in an installation.
/// Returns both direct user role assignments and inherited team role assignments.
/// </summary>
public interface IGetUserPermissions
{
    /// <summary>
    /// Retrieves all role assignments for a user (direct + team-based).
    /// </summary>
    /// <param name="userId">Internal user ID.</param>
    /// <param name="installationId">Installation ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success with user permissions summary, or failure with error.</returns>
    Task<Result<UserPermissions, Error>> ExecuteAsync(
        int userId,
        int installationId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Summary of all permissions for a user in an installation.
/// </summary>
public record UserPermissions(
    int UserId,
    int InstallationId,
    IReadOnlyList<RoleAssignment> DirectRoles,
    IReadOnlyList<RoleAssignment> TeamRoles,
    Role? HighestOrganizationRole);
