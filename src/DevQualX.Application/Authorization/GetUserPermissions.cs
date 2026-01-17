using DevQualX.Domain.Data;
using DevQualX.Domain.Models;
using DevQualX.Functional;

namespace DevQualX.Application.Authorization;

/// <summary>
/// Gets all effective permissions for a user in an installation.
/// </summary>
public class GetUserPermissions(IRoleAssignmentRepository roleAssignmentRepository) : IGetUserPermissions
{
    /// <inheritdoc />
    public async Task<Result<UserPermissions, Error>> ExecuteAsync(
        int userId,
        int installationId,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        if (userId <= 0)
        {
            return new ValidationError
            {
                Message = "Invalid user ID",
                Code = "AUTH007"
            };
        }

        if (installationId <= 0)
        {
            return new ValidationError
            {
                Message = "Invalid installation ID",
                Code = "AUTH008"
            };
        }

        // Get direct user role assignments
        var userRolesResult = await roleAssignmentRepository.GetUserRolesAsync(
            userId,
            installationId,
            cancellationToken);

        if (userRolesResult is Failure<RoleAssignment[], Error> userRolesFailure)
        {
            return userRolesFailure.Error;
        }

        var directRoles = ((Success<RoleAssignment[], Error>)userRolesResult).Value;

        // Get team role assignments
        var teamRolesResult = await roleAssignmentRepository.GetTeamRolesForUserAsync(
            userId,
            installationId,
            cancellationToken);

        if (teamRolesResult is Failure<RoleAssignment[], Error> teamRolesFailure)
        {
            return teamRolesFailure.Error;
        }

        var teamRoles = ((Success<RoleAssignment[], Error>)teamRolesResult).Value;

        // Find highest organization-level role
        var orgRoles = directRoles.Concat(teamRoles)
            .Where(r => r.Scope == RoleScope.Organization)
            .Select(r => r.Role)
            .ToList();

        Role? highestOrgRole = orgRoles.Any()
            ? orgRoles.Min() // Enum order: Owner=0, Admin=1, Maintainer=2, Reader=3, so Min = highest
            : null;

        return new UserPermissions(
            UserId: userId,
            InstallationId: installationId,
            DirectRoles: directRoles,
            TeamRoles: teamRoles,
            HighestOrganizationRole: highestOrgRole);
    }
}
