using Dapper;
using DevQualX.Domain.Data;
using DevQualX.Domain.Models;
using DevQualX.Functional;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DevQualX.Data.Repositories;

/// <summary>
/// Repository implementation for GitHub repository data operations using Dapper.
/// </summary>
public class RepositoryRepository(IConfiguration configuration) : IRepositoryRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection connection string not found");

    /// <inheritdoc />
    public async Task<Result<Repository, Error>> CreateAsync(
        long gitHubRepositoryId,
        int installationId,
        string name,
        string fullName,
        bool isPrivate,
        string? defaultBranch,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO Repositories (
                GitHubRepositoryId,
                InstallationId,
                Name,
                FullName,
                IsPrivate,
                DefaultBranch,
                IsActive,
                CreatedAt,
                UpdatedAt
            )
            VALUES (
                @GitHubRepositoryId,
                @InstallationId,
                @Name,
                @FullName,
                @IsPrivate,
                @DefaultBranch,
                1,
                SYSDATETIMEOFFSET(),
                SYSDATETIMEOFFSET()
            );
            
            SELECT
                Id,
                GitHubRepositoryId,
                InstallationId,
                Name,
                FullName,
                IsPrivate,
                DefaultBranch,
                IsActive,
                CreatedAt,
                UpdatedAt
            FROM Repositories
            WHERE Id = SCOPE_IDENTITY();
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var repository = await connection.QuerySingleAsync<Repository>(sql, new
            {
                GitHubRepositoryId = gitHubRepositoryId,
                InstallationId = installationId,
                Name = name,
                FullName = fullName,
                IsPrivate = isPrivate,
                DefaultBranch = defaultBranch
            });

            return new Success<Repository, Error>(repository);
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            return new Failure<Repository, Error>(new ConflictError
            {
                Message = $"Repository with GitHub ID {gitHubRepositoryId} already exists",
                ConflictingResource = gitHubRepositoryId.ToString()
            });
        }
        catch (SqlException ex)
        {
            return new Failure<Repository, Error>(new InternalError
            {
                Message = $"Database error while creating repository {fullName}",
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
    public async Task<Result<Repository, Error>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                GitHubRepositoryId,
                InstallationId,
                Name,
                FullName,
                IsPrivate,
                DefaultBranch,
                IsActive,
                CreatedAt,
                UpdatedAt
            FROM Repositories
            WHERE Id = @Id;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var repository = await connection.QuerySingleOrDefaultAsync<Repository>(sql, new { Id = id });

            return repository is not null
                ? new Success<Repository, Error>(repository)
                : new Failure<Repository, Error>(new NotFoundError
                {
                    Message = $"Repository with ID {id} not found",
                    ResourceType = nameof(Repository),
                    ResourceId = id.ToString()
                });
        }
        catch (SqlException ex)
        {
            return new Failure<Repository, Error>(new InternalError
            {
                Message = $"Database error while retrieving repository with ID {id}",
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
    public async Task<Result<Repository, Error>> GetByGitHubIdAsync(long gitHubRepositoryId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                GitHubRepositoryId,
                InstallationId,
                Name,
                FullName,
                IsPrivate,
                DefaultBranch,
                IsActive,
                CreatedAt,
                UpdatedAt
            FROM Repositories
            WHERE GitHubRepositoryId = @GitHubRepositoryId;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var repository = await connection.QuerySingleOrDefaultAsync<Repository>(sql, new { GitHubRepositoryId = gitHubRepositoryId });

            return repository is not null
                ? new Success<Repository, Error>(repository)
                : new Failure<Repository, Error>(new NotFoundError
                {
                    Message = $"Repository with GitHub ID {gitHubRepositoryId} not found",
                    ResourceType = nameof(Repository),
                    ResourceId = gitHubRepositoryId.ToString()
                });
        }
        catch (SqlException ex)
        {
            return new Failure<Repository, Error>(new InternalError
            {
                Message = $"Database error while retrieving repository with GitHub ID {gitHubRepositoryId}",
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
    public async Task<Result<Repository, Error>> GetByFullNameAsync(string fullName, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                GitHubRepositoryId,
                InstallationId,
                Name,
                FullName,
                IsPrivate,
                DefaultBranch,
                IsActive,
                CreatedAt,
                UpdatedAt
            FROM Repositories
            WHERE FullName = @FullName AND IsActive = 1;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var repository = await connection.QuerySingleOrDefaultAsync<Repository>(sql, new { FullName = fullName });

            return repository is not null
                ? new Success<Repository, Error>(repository)
                : new Failure<Repository, Error>(new NotFoundError
                {
                    Message = $"Repository '{fullName}' not found",
                    ResourceType = nameof(Repository),
                    ResourceId = fullName
                });
        }
        catch (SqlException ex)
        {
            return new Failure<Repository, Error>(new InternalError
            {
                Message = $"Database error while retrieving repository '{fullName}'",
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
    public async Task<Result<Repository[], Error>> GetByInstallationIdAsync(int installationId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                GitHubRepositoryId,
                InstallationId,
                Name,
                FullName,
                IsPrivate,
                DefaultBranch,
                IsActive,
                CreatedAt,
                UpdatedAt
            FROM Repositories
            WHERE InstallationId = @InstallationId AND IsActive = 1
            ORDER BY FullName;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var repositories = await connection.QueryAsync<Repository>(sql, new { InstallationId = installationId });

            return new Success<Repository[], Error>(repositories.ToArray());
        }
        catch (SqlException ex)
        {
            return new Failure<Repository[], Error>(new InternalError
            {
                Message = $"Database error while retrieving repositories for installation {installationId}",
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
    public async Task<Result<Repository, Error>> UpdateAsync(Repository repository, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE Repositories
            SET
                Name = @Name,
                FullName = @FullName,
                IsPrivate = @IsPrivate,
                DefaultBranch = @DefaultBranch,
                IsActive = @IsActive,
                UpdatedAt = SYSDATETIMEOFFSET()
            WHERE Id = @Id;
            
            SELECT
                Id,
                GitHubRepositoryId,
                InstallationId,
                Name,
                FullName,
                IsPrivate,
                DefaultBranch,
                IsActive,
                CreatedAt,
                UpdatedAt
            FROM Repositories
            WHERE Id = @Id;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var updatedRepository = await connection.QuerySingleOrDefaultAsync<Repository>(sql, new
            {
                repository.Id,
                repository.Name,
                repository.FullName,
                repository.IsPrivate,
                repository.DefaultBranch,
                repository.IsActive
            });

            return updatedRepository is not null
                ? new Success<Repository, Error>(updatedRepository)
                : new Failure<Repository, Error>(new NotFoundError
                {
                    Message = $"Repository with ID {repository.Id} not found",
                    ResourceType = nameof(Repository),
                    ResourceId = repository.Id.ToString()
                });
        }
        catch (SqlException ex)
        {
            return new Failure<Repository, Error>(new InternalError
            {
                Message = $"Database error while updating repository with ID {repository.Id}",
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
            UPDATE Repositories
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
                    Message = $"Repository with ID {id} not found",
                    ResourceType = nameof(Repository),
                    ResourceId = id.ToString()
                });
        }
        catch (SqlException ex)
        {
            return new Failure<Unit, Error>(new InternalError
            {
                Message = $"Database error while deactivating repository with ID {id}",
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
    public async Task<Result<Unit, Error>> SyncRepositoriesAsync(
        int installationId,
        GitHubRepository[] repositories,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                // Get existing repositories for this installation
                var existingSql = """
                    SELECT GitHubRepositoryId FROM Repositories
                    WHERE InstallationId = @InstallationId;
                    """;

                var existingRepoIds = (await connection.QueryAsync<long>(existingSql, new { InstallationId = installationId }, transaction))
                    .ToHashSet();

                var incomingRepoIds = repositories.Select(r => r.Id).ToHashSet();

                // Deactivate removed repositories
                var removedIds = existingRepoIds.Except(incomingRepoIds).ToList();
                if (removedIds.Any())
                {
                    var deactivateSql = """
                        UPDATE Repositories
                        SET IsActive = 0, UpdatedAt = SYSDATETIMEOFFSET()
                        WHERE InstallationId = @InstallationId AND GitHubRepositoryId IN @RemovedIds;
                        """;

                    await connection.ExecuteAsync(deactivateSql, new { InstallationId = installationId, RemovedIds = removedIds }, transaction);
                }

                // Upsert incoming repositories
                var upsertSql = """
                    MERGE Repositories AS target
                    USING (SELECT @GitHubRepositoryId AS GitHubRepositoryId, @InstallationId AS InstallationId) AS source
                    ON target.GitHubRepositoryId = source.GitHubRepositoryId AND target.InstallationId = source.InstallationId
                    WHEN MATCHED THEN
                        UPDATE SET
                            Name = @Name,
                            FullName = @FullName,
                            IsPrivate = @IsPrivate,
                            DefaultBranch = @DefaultBranch,
                            IsActive = 1,
                            UpdatedAt = SYSDATETIMEOFFSET()
                    WHEN NOT MATCHED THEN
                        INSERT (GitHubRepositoryId, InstallationId, Name, FullName, IsPrivate, DefaultBranch, IsActive, CreatedAt, UpdatedAt)
                        VALUES (@GitHubRepositoryId, @InstallationId, @Name, @FullName, @IsPrivate, @DefaultBranch, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
                    """;

                foreach (var repo in repositories)
                {
                    await connection.ExecuteAsync(upsertSql, new
                    {
                        GitHubRepositoryId = repo.Id,
                        InstallationId = installationId,
                        Name = repo.Name,
                        FullName = repo.FullName,
                        IsPrivate = repo.Private,
                        DefaultBranch = repo.DefaultBranch
                    }, transaction);
                }

                await transaction.CommitAsync(cancellationToken);
                return new Success<Unit, Error>(Unit.Default);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (SqlException ex)
        {
            return new Failure<Unit, Error>(new InternalError
            {
                Message = $"Database error while syncing repositories for installation {installationId}",
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
