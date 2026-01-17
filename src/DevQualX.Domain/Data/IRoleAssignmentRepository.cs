using DevQualX.Functional;
using DevQualX.Domain.Models;

namespace DevQualX.Domain.Data;

/// <summary>
/// Repository for role assignment data operations.
/// </summary>
public interface IRoleAssignmentRepository
{
    /// <summary>
    /// Grants a role to a user for a specific scope and resource.
    /// </summary>
    Task<Result<RoleAssignment, Error>> GrantRoleToUserAsync(
        int installationId,
        int userId,
        Role role,
        RoleScope scope,
        int? resourceId,
        int grantedBy,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Grants a role to a team for a specific scope and resource.
    /// </summary>
    Task<Result<RoleAssignment, Error>> GrantRoleToTeamAsync(
        int installationId,
        int teamId,
        Role role,
        RoleScope scope,
        int? resourceId,
        int grantedBy,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Revokes a role assignment.
    /// </summary>
    Task<Result<Unit, Error>> RevokeAsync(int roleAssignmentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all role assignments for a user in an installation.
    /// </summary>
    Task<Result<RoleAssignment[], Error>> GetUserRolesAsync(
        int userId,
        int installationId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all role assignments for teams a user belongs to.
    /// </summary>
    Task<Result<RoleAssignment[], Error>> GetTeamRolesForUserAsync(
        int userId,
        int installationId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a user has a specific role for a resource.
    /// Considers both direct user assignments and team assignments.
    /// </summary>
    Task<Result<bool, Error>> HasRoleAsync(
        int userId,
        int installationId,
        Role minimumRole,
        RoleScope scope,
        int? resourceId,
        CancellationToken cancellationToken = default);
}
