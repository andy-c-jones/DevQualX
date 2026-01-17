using Dapper;
using DevQualX.Domain.Data;
using DevQualX.Domain.Models;
using DevQualX.Functional;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DevQualX.Data.Repositories;

/// <summary>
/// Repository implementation for GitHub Project (planning/tracking) data operations using Dapper.
/// </summary>
public class GitHubProjectRepository(IConfiguration configuration) : IGitHubProjectRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection connection string not found");

    /// <inheritdoc />
    public async Task<Result<GitHubProject[], Error>> GetByInstallationIdAsync(int installationId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                GitHubProjectId,
                InstallationId,
                Title,
                Number,
                IsActive,
                SyncedAt
            FROM GitHubProjects
            WHERE InstallationId = @InstallationId AND IsActive = 1
            ORDER BY Number;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var projects = await connection.QueryAsync<GitHubProject>(sql, new { InstallationId = installationId });

            return new Success<GitHubProject[], Error>(projects.ToArray());
        }
        catch (SqlException ex)
        {
            return new Failure<GitHubProject[], Error>(new InternalError
            {
                Message = $"Database error while retrieving GitHub Projects for installation {installationId}",
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
    public async Task<Result<GitHubProject, Error>> GetByGitHubIdAsync(long gitHubProjectId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                GitHubProjectId,
                InstallationId,
                Title,
                Number,
                IsActive,
                SyncedAt
            FROM GitHubProjects
            WHERE GitHubProjectId = @GitHubProjectId;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var project = await connection.QuerySingleOrDefaultAsync<GitHubProject>(sql, new { GitHubProjectId = gitHubProjectId });

            return project is not null
                ? new Success<GitHubProject, Error>(project)
                : new Failure<GitHubProject, Error>(new NotFoundError
                {
                    Message = $"GitHub Project with GitHub ID {gitHubProjectId} not found",
                    ResourceType = nameof(GitHubProject),
                    ResourceId = gitHubProjectId.ToString()
                });
        }
        catch (SqlException ex)
        {
            return new Failure<GitHubProject, Error>(new InternalError
            {
                Message = $"Database error while retrieving GitHub Project with GitHub ID {gitHubProjectId}",
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
    public async Task<Result<Unit, Error>> SyncProjectsAsync(
        int installationId,
        GitHubProject[] projects,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                // Get existing projects for this installation
                const string existingSql = """
                    SELECT GitHubProjectId FROM GitHubProjects
                    WHERE InstallationId = @InstallationId;
                    """;

                var existingProjectIds = (await connection.QueryAsync<long>(existingSql, new { InstallationId = installationId }, transaction))
                    .ToHashSet();

                var incomingProjectIds = projects.Select(p => p.GitHubProjectId).ToHashSet();

                // Deactivate removed projects
                var removedIds = existingProjectIds.Except(incomingProjectIds).ToList();
                if (removedIds.Any())
                {
                    const string deactivateSql = """
                        UPDATE GitHubProjects
                        SET IsActive = 0
                        WHERE InstallationId = @InstallationId AND GitHubProjectId IN @RemovedIds;
                        """;

                    await connection.ExecuteAsync(deactivateSql, new { InstallationId = installationId, RemovedIds = removedIds }, transaction);
                }

                // Upsert incoming projects
                const string upsertSql = """
                    MERGE GitHubProjects AS target
                    USING (SELECT @GitHubProjectId AS GitHubProjectId, @InstallationId AS InstallationId) AS source
                    ON target.GitHubProjectId = source.GitHubProjectId AND target.InstallationId = source.InstallationId
                    WHEN MATCHED THEN
                        UPDATE SET
                            Title = @Title,
                            Number = @Number,
                            SyncedAt = SYSDATETIMEOFFSET(),
                            IsActive = 1
                    WHEN NOT MATCHED THEN
                        INSERT (GitHubProjectId, InstallationId, Title, Number, SyncedAt, IsActive)
                        VALUES (@GitHubProjectId, @InstallationId, @Title, @Number, SYSDATETIMEOFFSET(), 1);
                    """;

                foreach (var project in projects)
                {
                    await connection.ExecuteAsync(upsertSql, new
                    {
                        GitHubProjectId = project.GitHubProjectId,
                        InstallationId = installationId,
                        Title = project.Title,
                        Number = project.Number
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
                Message = $"Database error while syncing GitHub Projects for installation {installationId}",
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
