using Dapper;
using DevQualX.Domain.Data;
using DevQualX.Domain.Models;
using DevQualX.Functional;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DevQualX.Data.Repositories;

/// <summary>
/// Repository implementation for C# project data operations using Dapper.
/// </summary>
public class CSharpProjectRepository(IConfiguration configuration) : ICSharpProjectRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection connection string not found");

    /// <inheritdoc />
    public async Task<Result<CSharpProject, Error>> GetOrCreateAsync(
        int repositoryId,
        int? solutionId,
        string name,
        string relativePath,
        string? targetFramework,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            MERGE CSharpProjects AS target
            USING (SELECT @RepositoryId AS RepositoryId, @RelativePath AS RelativePath) AS source
            ON target.RepositoryId = source.RepositoryId AND target.RelativePath = source.RelativePath
            WHEN MATCHED THEN
                UPDATE SET
                    SolutionId = @SolutionId,
                    Name = @Name,
                    TargetFramework = @TargetFramework
            WHEN NOT MATCHED THEN
                INSERT (RepositoryId, SolutionId, Name, RelativePath, TargetFramework, DiscoveredAt)
                VALUES (@RepositoryId, @SolutionId, @Name, @RelativePath, @TargetFramework, SYSDATETIMEOFFSET())
            OUTPUT
                INSERTED.Id,
                INSERTED.RepositoryId,
                INSERTED.SolutionId,
                INSERTED.Name,
                INSERTED.RelativePath,
                INSERTED.TargetFramework,
                INSERTED.DiscoveredAt,
                INSERTED.LastReportAt;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var project = await connection.QuerySingleAsync<CSharpProject>(sql, new
            {
                RepositoryId = repositoryId,
                SolutionId = solutionId,
                Name = name,
                RelativePath = relativePath,
                TargetFramework = targetFramework
            });

            return new Success<CSharpProject, Error>(project);
        }
        catch (SqlException ex)
        {
            return new Failure<CSharpProject, Error>(new InternalError
            {
                Message = $"Database error while getting or creating C# project {name} in repository {repositoryId}",
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
    public async Task<Result<CSharpProject, Error>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                RepositoryId,
                SolutionId,
                Name,
                RelativePath,
                TargetFramework,
                DiscoveredAt,
                LastReportAt
            FROM CSharpProjects
            WHERE Id = @Id;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var project = await connection.QuerySingleOrDefaultAsync<CSharpProject>(sql, new { Id = id });

            return project is not null
                ? new Success<CSharpProject, Error>(project)
                : new Failure<CSharpProject, Error>(new NotFoundError
                {
                    Message = $"C# project with ID {id} not found",
                    ResourceType = nameof(CSharpProject),
                    ResourceId = id.ToString()
                });
        }
        catch (SqlException ex)
        {
            return new Failure<CSharpProject, Error>(new InternalError
            {
                Message = $"Database error while retrieving C# project with ID {id}",
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
    public async Task<Result<CSharpProject[], Error>> GetByRepositoryIdAsync(int repositoryId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                RepositoryId,
                SolutionId,
                Name,
                RelativePath,
                TargetFramework,
                DiscoveredAt,
                LastReportAt
            FROM CSharpProjects
            WHERE RepositoryId = @RepositoryId
            ORDER BY Name;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var projects = await connection.QueryAsync<CSharpProject>(sql, new { RepositoryId = repositoryId });

            return new Success<CSharpProject[], Error>(projects.ToArray());
        }
        catch (SqlException ex)
        {
            return new Failure<CSharpProject[], Error>(new InternalError
            {
                Message = $"Database error while retrieving C# projects for repository {repositoryId}",
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
    public async Task<Result<CSharpProject[], Error>> GetBySolutionIdAsync(int solutionId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                RepositoryId,
                SolutionId,
                Name,
                RelativePath,
                TargetFramework,
                DiscoveredAt,
                LastReportAt
            FROM CSharpProjects
            WHERE SolutionId = @SolutionId
            ORDER BY Name;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var projects = await connection.QueryAsync<CSharpProject>(sql, new { SolutionId = solutionId });

            return new Success<CSharpProject[], Error>(projects.ToArray());
        }
        catch (SqlException ex)
        {
            return new Failure<CSharpProject[], Error>(new InternalError
            {
                Message = $"Database error while retrieving C# projects for solution {solutionId}",
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
    public async Task<Result<Unit, Error>> UpdateLastReportAsync(int id, DateTimeOffset lastReportAt, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE CSharpProjects
            SET LastReportAt = @LastReportAt
            WHERE Id = @Id;
            
            SELECT @@ROWCOUNT;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteScalarAsync<int>(sql, new
            {
                Id = id,
                LastReportAt = lastReportAt
            });

            return rowsAffected > 0
                ? new Success<Unit, Error>(Unit.Default)
                : new Failure<Unit, Error>(new NotFoundError
                {
                    Message = $"C# project with ID {id} not found",
                    ResourceType = nameof(CSharpProject),
                    ResourceId = id.ToString()
                });
        }
        catch (SqlException ex)
        {
            return new Failure<Unit, Error>(new InternalError
            {
                Message = $"Database error while updating last report for C# project {id}",
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
