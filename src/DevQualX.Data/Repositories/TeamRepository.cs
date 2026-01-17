using Dapper;
using DevQualX.Domain.Data;
using DevQualX.Domain.Models;
using DevQualX.Functional;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DevQualX.Data.Repositories;

/// <summary>
/// Repository implementation for GitHub team data operations using Dapper.
/// </summary>
public class TeamRepository(IConfiguration configuration) : ITeamRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection connection string not found");

    /// <inheritdoc />
    public async Task<Result<Team[], Error>> GetByInstallationIdAsync(int installationId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                GitHubTeamId,
                InstallationId,
                Slug,
                Name,
                Description,
                SyncedAt,
                IsActive
            FROM Teams
            WHERE InstallationId = @InstallationId AND IsActive = 1
            ORDER BY Name;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var teams = await connection.QueryAsync<Team>(sql, new { InstallationId = installationId });

            return new Success<Team[], Error>(teams.ToArray());
        }
        catch (SqlException ex)
        {
            return new Failure<Team[], Error>(new InternalError
            {
                Message = $"Database error while retrieving teams for installation {installationId}",
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
    public async Task<Result<Team, Error>> GetByGitHubIdAsync(long gitHubTeamId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                GitHubTeamId,
                InstallationId,
                Slug,
                Name,
                Description,
                SyncedAt,
                IsActive
            FROM Teams
            WHERE GitHubTeamId = @GitHubTeamId;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var team = await connection.QuerySingleOrDefaultAsync<Team>(sql, new { GitHubTeamId = gitHubTeamId });

            return team is not null
                ? new Success<Team, Error>(team)
                : new Failure<Team, Error>(new NotFoundError
                {
                    Message = $"Team with GitHub ID {gitHubTeamId} not found",
                    ResourceType = nameof(Team),
                    ResourceId = gitHubTeamId.ToString()
                });
        }
        catch (SqlException ex)
        {
            return new Failure<Team, Error>(new InternalError
            {
                Message = $"Database error while retrieving team with GitHub ID {gitHubTeamId}",
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
    public async Task<Result<Unit, Error>> SyncTeamsAsync(
        int installationId,
        GitHubTeam[] teams,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                // Get existing teams for this installation
                var existingSql = """
                    SELECT GitHubTeamId FROM Teams
                    WHERE InstallationId = @InstallationId;
                    """;

                var existingTeamIds = (await connection.QueryAsync<long>(existingSql, new { InstallationId = installationId }, transaction))
                    .ToHashSet();

                var incomingTeamIds = teams.Select(t => t.Id).ToHashSet();

                // Deactivate removed teams
                var removedIds = existingTeamIds.Except(incomingTeamIds).ToList();
                if (removedIds.Any())
                {
                    var deactivateSql = """
                        UPDATE Teams
                        SET IsActive = 0
                        WHERE InstallationId = @InstallationId AND GitHubTeamId IN @RemovedIds;
                        """;

                    await connection.ExecuteAsync(deactivateSql, new { InstallationId = installationId, RemovedIds = removedIds }, transaction);
                }

                // Upsert incoming teams
                var upsertSql = """
                    MERGE Teams AS target
                    USING (SELECT @GitHubTeamId AS GitHubTeamId, @InstallationId AS InstallationId) AS source
                    ON target.GitHubTeamId = source.GitHubTeamId AND target.InstallationId = source.InstallationId
                    WHEN MATCHED THEN
                        UPDATE SET
                            Slug = @Slug,
                            Name = @Name,
                            Description = @Description,
                            SyncedAt = SYSDATETIMEOFFSET(),
                            IsActive = 1
                    WHEN NOT MATCHED THEN
                        INSERT (GitHubTeamId, InstallationId, Slug, Name, Description, SyncedAt, IsActive)
                        VALUES (@GitHubTeamId, @InstallationId, @Slug, @Name, @Description, SYSDATETIMEOFFSET(), 1);
                    """;

                foreach (var team in teams)
                {
                    await connection.ExecuteAsync(upsertSql, new
                    {
                        GitHubTeamId = team.Id,
                        InstallationId = installationId,
                        Slug = team.Slug,
                        Name = team.Name,
                        Description = team.Description
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
                Message = $"Database error while syncing teams for installation {installationId}",
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
