using Dapper;
using DevQualX.Domain.Data;
using DevQualX.Domain.Models;
using DevQualX.Functional;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DevQualX.Data.Repositories;

/// <summary>
/// Repository implementation for user data operations using Dapper.
/// </summary>
public class UserRepository(IConfiguration configuration) : IUserRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection connection string not found");

    /// <inheritdoc />
    public async Task<Result<User, Error>> GetOrCreateAsync(
        long gitHubUserId,
        string username,
        string? email,
        string? avatarUrl,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            MERGE Users AS target
            USING (SELECT @GitHubUserId AS GitHubUserId) AS source
            ON target.GitHubUserId = source.GitHubUserId
            WHEN MATCHED THEN
                UPDATE SET
                    Username = @Username,
                    Email = @Email,
                    AvatarUrl = @AvatarUrl,
                    UpdatedAt = SYSDATETIMEOFFSET()
            WHEN NOT MATCHED THEN
                INSERT (GitHubUserId, Username, Email, AvatarUrl, CreatedAt, UpdatedAt)
                VALUES (@GitHubUserId, @Username, @Email, @AvatarUrl, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET())
            OUTPUT
                INSERTED.Id,
                INSERTED.GitHubUserId,
                INSERTED.Username,
                INSERTED.Email,
                INSERTED.AvatarUrl,
                INSERTED.CreatedAt,
                INSERTED.UpdatedAt;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var user = await connection.QuerySingleAsync<User>(sql, new
            {
                GitHubUserId = gitHubUserId,
                Username = username,
                Email = email,
                AvatarUrl = avatarUrl
            });

            return new Success<User, Error>(user);
        }
        catch (SqlException ex)
        {
            return new Failure<User, Error>(new InternalError
            {
                Message = $"Database error while getting or creating user with GitHub ID {gitHubUserId}",
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
    public async Task<Result<User, Error>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                GitHubUserId,
                Username,
                Email,
                AvatarUrl,
                CreatedAt,
                UpdatedAt
            FROM Users
            WHERE Id = @Id;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var user = await connection.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });

            return user is not null
                ? new Success<User, Error>(user)
                : new Failure<User, Error>(new NotFoundError
                {
                    Message = $"User with ID {id} not found",
                    ResourceType = nameof(User),
                    ResourceId = id.ToString()
                });
        }
        catch (SqlException ex)
        {
            return new Failure<User, Error>(new InternalError
            {
                Message = $"Database error while retrieving user with ID {id}",
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
    public async Task<Result<User, Error>> GetByGitHubIdAsync(long gitHubUserId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                GitHubUserId,
                Username,
                Email,
                AvatarUrl,
                CreatedAt,
                UpdatedAt
            FROM Users
            WHERE GitHubUserId = @GitHubUserId;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var user = await connection.QuerySingleOrDefaultAsync<User>(sql, new { GitHubUserId = gitHubUserId });

            return user is not null
                ? new Success<User, Error>(user)
                : new Failure<User, Error>(new NotFoundError
                {
                    Message = $"User with GitHub ID {gitHubUserId} not found",
                    ResourceType = nameof(User),
                    ResourceId = gitHubUserId.ToString()
                });
        }
        catch (SqlException ex)
        {
            return new Failure<User, Error>(new InternalError
            {
                Message = $"Database error while retrieving user with GitHub ID {gitHubUserId}",
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
    public async Task<Result<User, Error>> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE Users
            SET
                Username = @Username,
                Email = @Email,
                AvatarUrl = @AvatarUrl,
                UpdatedAt = SYSDATETIMEOFFSET()
            WHERE Id = @Id;
            
            SELECT
                Id,
                GitHubUserId,
                Username,
                Email,
                AvatarUrl,
                CreatedAt,
                UpdatedAt
            FROM Users
            WHERE Id = @Id;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var updatedUser = await connection.QuerySingleOrDefaultAsync<User>(sql, new
            {
                user.Id,
                user.Username,
                user.Email,
                user.AvatarUrl
            });

            return updatedUser is not null
                ? new Success<User, Error>(updatedUser)
                : new Failure<User, Error>(new NotFoundError
                {
                    Message = $"User with ID {user.Id} not found",
                    ResourceType = nameof(User),
                    ResourceId = user.Id.ToString()
                });
        }
        catch (SqlException ex)
        {
            return new Failure<User, Error>(new InternalError
            {
                Message = $"Database error while updating user with ID {user.Id}",
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
