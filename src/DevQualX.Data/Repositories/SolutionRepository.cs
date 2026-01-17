using Dapper;
using DevQualX.Domain.Data;
using DevQualX.Domain.Models;
using DevQualX.Functional;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DevQualX.Data.Repositories;

/// <summary>
/// Repository implementation for C# solution data operations using Dapper.
/// </summary>
public class SolutionRepository(IConfiguration configuration) : ISolutionRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection connection string not found");

    /// <inheritdoc />
    public async Task<Result<Solution, Error>> GetOrCreateAsync(
        int repositoryId,
        string name,
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            MERGE Solutions AS target
            USING (SELECT @RepositoryId AS RepositoryId, @RelativePath AS RelativePath) AS source
            ON target.RepositoryId = source.RepositoryId AND target.RelativePath = source.RelativePath
            WHEN MATCHED THEN
                UPDATE SET
                    Name = @Name
            WHEN NOT MATCHED THEN
                INSERT (RepositoryId, Name, RelativePath, DiscoveredAt)
                VALUES (@RepositoryId, @Name, @RelativePath, SYSDATETIMEOFFSET())
            OUTPUT
                INSERTED.Id,
                INSERTED.RepositoryId,
                INSERTED.Name,
                INSERTED.RelativePath,
                INSERTED.DiscoveredAt,
                INSERTED.LastReportAt;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var solution = await connection.QuerySingleAsync<Solution>(sql, new
            {
                RepositoryId = repositoryId,
                Name = name,
                RelativePath = relativePath
            });

            return new Success<Solution, Error>(solution);
        }
        catch (SqlException ex)
        {
            return new Failure<Solution, Error>(new InternalError
            {
                Message = $"Database error while getting or creating solution {name} in repository {repositoryId}",
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
    public async Task<Result<Solution, Error>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                RepositoryId,
                Name,
                RelativePath,
                DiscoveredAt,
                LastReportAt
            FROM Solutions
            WHERE Id = @Id;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var solution = await connection.QuerySingleOrDefaultAsync<Solution>(sql, new { Id = id });

            return solution is not null
                ? new Success<Solution, Error>(solution)
                : new Failure<Solution, Error>(new NotFoundError
                {
                    Message = $"Solution with ID {id} not found",
                    ResourceType = nameof(Solution),
                    ResourceId = id.ToString()
                });
        }
        catch (SqlException ex)
        {
            return new Failure<Solution, Error>(new InternalError
            {
                Message = $"Database error while retrieving solution with ID {id}",
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
    public async Task<Result<Solution[], Error>> GetByRepositoryIdAsync(int repositoryId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                RepositoryId,
                Name,
                RelativePath,
                DiscoveredAt,
                LastReportAt
            FROM Solutions
            WHERE RepositoryId = @RepositoryId
            ORDER BY Name;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var solutions = await connection.QueryAsync<Solution>(sql, new { RepositoryId = repositoryId });

            return new Success<Solution[], Error>(solutions.ToArray());
        }
        catch (SqlException ex)
        {
            return new Failure<Solution[], Error>(new InternalError
            {
                Message = $"Database error while retrieving solutions for repository {repositoryId}",
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
            UPDATE Solutions
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
                    Message = $"Solution with ID {id} not found",
                    ResourceType = nameof(Solution),
                    ResourceId = id.ToString()
                });
        }
        catch (SqlException ex)
        {
            return new Failure<Unit, Error>(new InternalError
            {
                Message = $"Database error while updating last report for solution {id}",
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
