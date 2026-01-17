using Dapper;
using DevQualX.Domain.Data;
using DevQualX.Domain.Models;
using DevQualX.Functional;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DevQualX.Data.Repositories;

/// <summary>
/// Repository implementation for role assignment data operations using Dapper.
/// </summary>
public class RoleAssignmentRepository(IConfiguration configuration) : IRoleAssignmentRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection connection string not found");

    /// <inheritdoc />
    public async Task<Result<RoleAssignment, Error>> GrantRoleToUserAsync(
        int installationId,
        int userId,
        Role role,
        RoleScope scope,
        int? resourceId,
        int grantedBy,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO RoleAssignments (
                InstallationId,
                UserId,
                TeamId,
                Role,
                Scope,
                ResourceId,
                GrantedBy,
                GrantedAt
            )
            VALUES (
                @InstallationId,
                @UserId,
                NULL,
                @Role,
                @Scope,
                @ResourceId,
                @GrantedBy,
                SYSDATETIMEOFFSET()
            );
            
            SELECT
                Id,
                InstallationId,
                UserId,
                TeamId,
                Role,
                Scope,
                ResourceId,
                GrantedBy,
                GrantedAt
            FROM RoleAssignments
            WHERE Id = SCOPE_IDENTITY();
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var assignment = await connection.QuerySingleAsync<RoleAssignment>(sql, new
            {
                InstallationId = installationId,
                UserId = userId,
                Role = role.ToString(),
                Scope = scope.ToString(),
                ResourceId = resourceId,
                GrantedBy = grantedBy
            });

            return new Success<RoleAssignment, Error>(assignment);
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            return new Failure<RoleAssignment, Error>(new ConflictError
            {
                Message = $"Role {role} already assigned to user {userId} for this scope and resource",
                ConflictingResource = $"User:{userId},Role:{role},Scope:{scope}"
            });
        }
        catch (SqlException ex)
        {
            return new Failure<RoleAssignment, Error>(new InternalError
            {
                Message = $"Database error while granting role {role} to user {userId}",
                Code = "DB_ERROR",
                Metadata = new Dictionary<string, object>
                {
                    ["SqlErrorNumber"] = ex.Number,
                    ["SqlErrorMessage"] = ex.Message
                }
            });
        }
    }

    /// <inheritdoc />
    public async Task<Result<RoleAssignment, Error>> GrantRoleToTeamAsync(
        int installationId,
        int teamId,
        Role role,
        RoleScope scope,
        int? resourceId,
        int grantedBy,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO RoleAssignments (
                InstallationId,
                UserId,
                TeamId,
                Role,
                Scope,
                ResourceId,
                GrantedBy,
                GrantedAt
            )
            VALUES (
                @InstallationId,
                NULL,
                @TeamId,
                @Role,
                @Scope,
                @ResourceId,
                @GrantedBy,
                SYSDATETIMEOFFSET()
            );
            
            SELECT
                Id,
                InstallationId,
                UserId,
                TeamId,
                Role,
                Scope,
                ResourceId,
                GrantedBy,
                GrantedAt
            FROM RoleAssignments
            WHERE Id = SCOPE_IDENTITY();
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var assignment = await connection.QuerySingleAsync<RoleAssignment>(sql, new
            {
                InstallationId = installationId,
                TeamId = teamId,
                Role = role.ToString(),
                Scope = scope.ToString(),
                ResourceId = resourceId,
                GrantedBy = grantedBy
            });

            return new Success<RoleAssignment, Error>(assignment);
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            return new Failure<RoleAssignment, Error>(new ConflictError
            {
                Message = $"Role {role} already assigned to team {teamId} for this scope and resource",
                ConflictingResource = $"Team:{teamId},Role:{role},Scope:{scope}"
            });
        }
        catch (SqlException ex)
        {
            return new Failure<RoleAssignment, Error>(new InternalError
            {
                Message = $"Database error while granting role {role} to team {teamId}",
                Code = "DB_ERROR",
                Metadata = new Dictionary<string, object>
                {
                    ["SqlErrorNumber"] = ex.Number,
                    ["SqlErrorMessage"] = ex.Message
                }
            });
        }
    }

    /// <inheritdoc />
    public async Task<Result<Unit, Error>> RevokeAsync(int roleAssignmentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            DELETE FROM RoleAssignments
            WHERE Id = @Id;
            
            SELECT @@ROWCOUNT;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteScalarAsync<int>(sql, new { Id = roleAssignmentId });

            return rowsAffected > 0
                ? new Success<Unit, Error>(Unit.Default)
                : new Failure<Unit, Error>(new NotFoundError
                {
                    Message = $"Role assignment with ID {roleAssignmentId} not found",
                    ResourceType = nameof(RoleAssignment),
                    ResourceId = roleAssignmentId.ToString()
                });
        }
        catch (SqlException ex)
        {
            return new Failure<Unit, Error>(new InternalError
            {
                Message = $"Database error while revoking role assignment {roleAssignmentId}",
                Code = "DB_ERROR",
                Metadata = new Dictionary<string, object>
                {
                    ["SqlErrorNumber"] = ex.Number,
                    ["SqlErrorMessage"] = ex.Message
                }
            });
        }
    }

    /// <inheritdoc />
    public async Task<Result<RoleAssignment[], Error>> GetUserRolesAsync(
        int userId,
        int installationId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                InstallationId,
                UserId,
                TeamId,
                Role,
                Scope,
                ResourceId,
                GrantedBy,
                GrantedAt
            FROM RoleAssignments
            WHERE UserId = @UserId AND InstallationId = @InstallationId
            ORDER BY GrantedAt DESC;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var assignments = await connection.QueryAsync<RoleAssignment>(sql, new
            {
                UserId = userId,
                InstallationId = installationId
            });

            return new Success<RoleAssignment[], Error>(assignments.ToArray());
        }
        catch (SqlException ex)
        {
            return new Failure<RoleAssignment[], Error>(new InternalError
            {
                Message = $"Database error while retrieving roles for user {userId}",
                Code = "DB_ERROR",
                Metadata = new Dictionary<string, object>
                {
                    ["SqlErrorNumber"] = ex.Number,
                    ["SqlErrorMessage"] = ex.Message
                }
            });
        }
    }

    /// <inheritdoc />
    public async Task<Result<RoleAssignment[], Error>> GetTeamRolesForUserAsync(
        int userId,
        int installationId,
        CancellationToken cancellationToken = default)
    {
        // Note: This query assumes there's a Teams table but NO TeamMembers table yet
        // When TeamMembers table is created, update this query to join through it
        const string sql = """
            SELECT
                ra.Id,
                ra.InstallationId,
                ra.UserId,
                ra.TeamId,
                ra.Role,
                ra.Scope,
                ra.ResourceId,
                ra.GrantedBy,
                ra.GrantedAt
            FROM RoleAssignments ra
            WHERE ra.TeamId IS NOT NULL
                AND ra.InstallationId = @InstallationId
            ORDER BY ra.GrantedAt DESC;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var assignments = await connection.QueryAsync<RoleAssignment>(sql, new
            {
                UserId = userId,
                InstallationId = installationId
            });

            return new Success<RoleAssignment[], Error>(assignments.ToArray());
        }
        catch (SqlException ex)
        {
            return new Failure<RoleAssignment[], Error>(new InternalError
            {
                Message = $"Database error while retrieving team roles for user {userId}",
                Code = "DB_ERROR",
                Metadata = new Dictionary<string, object>
                {
                    ["SqlErrorNumber"] = ex.Number,
                    ["SqlErrorMessage"] = ex.Message
                }
            });
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool, Error>> HasRoleAsync(
        int userId,
        int installationId,
        Role minimumRole,
        RoleScope scope,
        int? resourceId,
        CancellationToken cancellationToken = default)
    {
        // Role hierarchy: Owner > Admin > Maintainer > Reader
        var roleHierarchy = new Dictionary<Role, int>
        {
            [Role.Owner] = 4,
            [Role.Admin] = 3,
            [Role.Maintainer] = 2,
            [Role.Reader] = 1
        };

        var minimumRoleLevel = roleHierarchy[minimumRole];

        // Check both user and team assignments
        // Note: Team membership check is simplified until TeamMembers table exists
        const string sql = """
            SELECT COUNT(*)
            FROM RoleAssignments
            WHERE InstallationId = @InstallationId
                AND (UserId = @UserId OR TeamId IS NOT NULL)
                AND Scope = @Scope
                AND (ResourceId = @ResourceId OR (ResourceId IS NULL AND @ResourceId IS NULL))
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var assignments = await connection.QueryAsync<RoleAssignment>(
                sql.Replace("SELECT COUNT(*)", "SELECT Id, InstallationId, UserId, TeamId, Role, Scope, ResourceId, GrantedBy, GrantedAt"),
                new
                {
                    UserId = userId,
                    InstallationId = installationId,
                    Scope = scope.ToString(),
                    ResourceId = resourceId
                });

            // Check if any assignment meets or exceeds the minimum role level
            var hasRole = assignments.Any(a => roleHierarchy.TryGetValue(a.Role, out var level) && level >= minimumRoleLevel);

            return new Success<bool, Error>(hasRole);
        }
        catch (SqlException ex)
        {
            return new Failure<bool, Error>(new InternalError
            {
                Message = $"Database error while checking role for user {userId}",
                Code = "DB_ERROR",
                Metadata = new Dictionary<string, object>
                {
                    ["SqlErrorNumber"] = ex.Number,
                    ["SqlErrorMessage"] = ex.Message
                }
            });
        }
    }
}
