using Dapper;
using DevQualX.Domain.Data;
using DevQualX.Domain.Models;
using DevQualX.Functional;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DevQualX.Data.Repositories;

/// <summary>
/// Repository implementation for GitHub App installation data operations using Dapper.
/// </summary>
public class InstallationRepository(IConfiguration configuration) : IInstallationRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection connection string not found");

    /// <inheritdoc />
    public async Task<Result<Installation, Error>> CreateAsync(
        long gitHubInstallationId,
        long gitHubAccountId,
        AccountType accountType,
        string accountLogin,
        int installedBy,
        DateTimeOffset installedAt,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO Installations (
                GitHubInstallationId,
                GitHubAccountId,
                AccountType,
                AccountLogin,
                InstalledBy,
                InstalledAt,
                UpdatedAt,
                IsActive
            )
            VALUES (
                @GitHubInstallationId,
                @GitHubAccountId,
                @AccountType,
                @AccountLogin,
                @InstalledBy,
                @InstalledAt,
                SYSDATETIMEOFFSET(),
                1
            );
            
            SELECT
                Id,
                GitHubInstallationId,
                GitHubAccountId,
                AccountType,
                AccountLogin,
                InstalledBy,
                InstalledAt,
                UpdatedAt,
                SuspendedAt,
                IsActive
            FROM Installations
            WHERE Id = SCOPE_IDENTITY();
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var installation = await connection.QuerySingleAsync<Installation>(sql, new
            {
                GitHubInstallationId = gitHubInstallationId,
                GitHubAccountId = gitHubAccountId,
                AccountType = accountType.ToString(),
                AccountLogin = accountLogin,
                InstalledBy = installedBy,
                InstalledAt = installedAt
            });

            return new Success<Installation, Error>(installation);
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) // Unique constraint violation
        {
            return new Failure<Installation, Error>(new ConflictError
            {
                Message = $"Installation with GitHub ID {gitHubInstallationId} already exists",
                ConflictingResource = gitHubInstallationId.ToString()
            });
        }
        catch (SqlException ex)
        {
            return new Failure<Installation, Error>(new InternalError
            {
                Message = $"Database error while creating installation for account {accountLogin}",
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
    public async Task<Result<Installation, Error>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                GitHubInstallationId,
                GitHubAccountId,
                AccountType,
                AccountLogin,
                InstalledBy,
                InstalledAt,
                UpdatedAt,
                SuspendedAt,
                IsActive
            FROM Installations
            WHERE Id = @Id;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var installation = await connection.QuerySingleOrDefaultAsync<Installation>(sql, new { Id = id });

            return installation is not null
                ? new Success<Installation, Error>(installation)
                : new Failure<Installation, Error>(new NotFoundError
                {
                    Message = $"Installation with ID {id} not found",
                    ResourceType = nameof(Installation),
                    ResourceId = id.ToString()
                });
        }
        catch (SqlException ex)
        {
            return new Failure<Installation, Error>(new InternalError
            {
                Message = $"Database error while retrieving installation with ID {id}",
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
    public async Task<Result<Installation, Error>> GetByGitHubIdAsync(long gitHubInstallationId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                GitHubInstallationId,
                GitHubAccountId,
                AccountType,
                AccountLogin,
                InstalledBy,
                InstalledAt,
                UpdatedAt,
                SuspendedAt,
                IsActive
            FROM Installations
            WHERE GitHubInstallationId = @GitHubInstallationId;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var installation = await connection.QuerySingleOrDefaultAsync<Installation>(sql, new { GitHubInstallationId = gitHubInstallationId });

            return installation is not null
                ? new Success<Installation, Error>(installation)
                : new Failure<Installation, Error>(new NotFoundError
                {
                    Message = $"Installation with GitHub ID {gitHubInstallationId} not found",
                    ResourceType = nameof(Installation),
                    ResourceId = gitHubInstallationId.ToString()
                });
        }
        catch (SqlException ex)
        {
            return new Failure<Installation, Error>(new InternalError
            {
                Message = $"Database error while retrieving installation with GitHub ID {gitHubInstallationId}",
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
    public async Task<Result<Installation[], Error>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT DISTINCT
                i.Id,
                i.GitHubInstallationId,
                i.GitHubAccountId,
                i.AccountType,
                i.AccountLogin,
                i.InstalledBy,
                i.InstalledAt,
                i.UpdatedAt,
                i.SuspendedAt,
                i.IsActive
            FROM Installations i
            LEFT JOIN RoleAssignments ra ON ra.ResourceId = i.Id AND ra.Scope = 'Organization'
            WHERE i.IsActive = 1
                AND (
                    i.InstalledBy = @UserId
                    OR ra.UserId = @UserId
                    OR ra.TeamId IN (
                        SELECT tm.TeamId
                        FROM TeamMembers tm
                        WHERE tm.UserId = @UserId
                    )
                )
            ORDER BY i.AccountLogin;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var installations = await connection.QueryAsync<Installation>(sql, new { UserId = userId });

            return new Success<Installation[], Error>(installations.ToArray());
        }
        catch (SqlException ex)
        {
            return new Failure<Installation[], Error>(new InternalError
            {
                Message = $"Database error while retrieving installations for user {userId}",
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
    public async Task<Result<Installation, Error>> GetByAccountLoginAsync(string accountLogin, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                GitHubInstallationId,
                GitHubAccountId,
                AccountType,
                AccountLogin,
                InstalledBy,
                InstalledAt,
                UpdatedAt,
                SuspendedAt,
                IsActive
            FROM Installations
            WHERE AccountLogin = @AccountLogin AND IsActive = 1;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var installation = await connection.QuerySingleOrDefaultAsync<Installation>(sql, new { AccountLogin = accountLogin });

            return installation is not null
                ? new Success<Installation, Error>(installation)
                : new Failure<Installation, Error>(new NotFoundError
                {
                    Message = $"Installation for account '{accountLogin}' not found",
                    ResourceType = nameof(Installation),
                    ResourceId = accountLogin
                });
        }
        catch (SqlException ex)
        {
            return new Failure<Installation, Error>(new InternalError
            {
                Message = $"Database error while retrieving installation for account '{accountLogin}'",
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
    public async Task<Result<Installation, Error>> UpdateAsync(Installation installation, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE Installations
            SET
                AccountLogin = @AccountLogin,
                SuspendedAt = @SuspendedAt,
                IsActive = @IsActive,
                UpdatedAt = SYSDATETIMEOFFSET()
            WHERE Id = @Id;
            
            SELECT
                Id,
                GitHubInstallationId,
                GitHubAccountId,
                AccountType,
                AccountLogin,
                InstalledBy,
                InstalledAt,
                UpdatedAt,
                SuspendedAt,
                IsActive
            FROM Installations
            WHERE Id = @Id;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var updatedInstallation = await connection.QuerySingleOrDefaultAsync<Installation>(sql, new
            {
                installation.Id,
                installation.AccountLogin,
                installation.SuspendedAt,
                installation.IsActive
            });

            return updatedInstallation is not null
                ? new Success<Installation, Error>(updatedInstallation)
                : new Failure<Installation, Error>(new NotFoundError
                {
                    Message = $"Installation with ID {installation.Id} not found",
                    ResourceType = nameof(Installation),
                    ResourceId = installation.Id.ToString()
                });
        }
        catch (SqlException ex)
        {
            return new Failure<Installation, Error>(new InternalError
            {
                Message = $"Database error while updating installation with ID {installation.Id}",
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
    public async Task<Result<Unit, Error>> DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE Installations
            SET
                IsActive = 0,
                UpdatedAt = SYSDATETIMEOFFSET()
            WHERE Id = @Id;
            
            SELECT @@ROWCOUNT;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });

            return rowsAffected > 0
                ? new Success<Unit, Error>(Unit.Default)
                : new Failure<Unit, Error>(new NotFoundError
                {
                    Message = $"Installation with ID {id} not found",
                    ResourceType = nameof(Installation),
                    ResourceId = id.ToString()
                });
        }
        catch (SqlException ex)
        {
            return new Failure<Unit, Error>(new InternalError
            {
                Message = $"Database error while deactivating installation with ID {id}",
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
