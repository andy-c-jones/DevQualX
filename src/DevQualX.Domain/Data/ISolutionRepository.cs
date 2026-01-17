using DevQualX.Functional;
using DevQualX.Domain.Models;

namespace DevQualX.Domain.Data;

/// <summary>
/// Repository for C# solution data operations.
/// </summary>
public interface ISolutionRepository
{
    /// <summary>
    /// Gets or creates a solution by repository ID and relative path.
    /// Updates the solution if it already exists.
    /// </summary>
    Task<Result<Solution, Error>> GetOrCreateAsync(
        int repositoryId,
        string name,
        string relativePath,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a solution by its internal ID.
    /// </summary>
    Task<Result<Solution, Error>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all solutions for a repository.
    /// </summary>
    Task<Result<Solution[], Error>> GetByRepositoryIdAsync(int repositoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates the last report timestamp for a solution.
    /// </summary>
    Task<Result<Unit, Error>> UpdateLastReportAsync(int id, DateTimeOffset lastReportAt, CancellationToken cancellationToken = default);
}
