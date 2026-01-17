using DevQualX.Domain.Data;
using DevQualX.Domain.Models;
using DevQualX.Functional;

namespace DevQualX.Application.Authorization;

/// <summary>
/// Checks if a user has a specific role (or higher) for a resource.
/// </summary>
public class CheckUserRole(IRoleAssignmentRepository roleAssignmentRepository) : ICheckUserRole
{
    /// <inheritdoc />
    public async Task<Result<bool, Error>> ExecuteAsync(
        int userId,
        int installationId,
        Role minimumRole,
        RoleScope scope,
        int? resourceId = null,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        if (userId <= 0)
        {
            return new ValidationError
            {
                Message = "Invalid user ID",
                Code = "AUTH005"
            };
        }

        if (installationId <= 0)
        {
            return new ValidationError
            {
                Message = "Invalid installation ID",
                Code = "AUTH006"
            };
        }

        // Check if user has the required role
        // The repository method considers role hierarchy and team assignments
        return await roleAssignmentRepository.HasRoleAsync(
            userId,
            installationId,
            minimumRole,
            scope,
            resourceId,
            cancellationToken);
    }
}
