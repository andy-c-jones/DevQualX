using DevQualX.Functional;
using DevQualX.Domain.Models;

namespace DevQualX.Domain.Data;

/// <summary>
/// Repository for C# project (.csproj) data operations.
/// </summary>
public interface ICSharpProjectRepository
{
    /// <summary>
    /// Gets or creates a C# project by repository ID and relative path.
    /// Updates the project if it already exists.
    /// </summary>
    Task<Result<CSharpProject, Error>> GetOrCreateAsync(
        int repositoryId,
        int? solutionId,
        string name,
        string relativePath,
        string? targetFramework,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a C# project by its internal ID.
    /// </summary>
    Task<Result<CSharpProject, Error>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all C# projects for a repository.
    /// </summary>
    Task<Result<CSharpProject[], Error>> GetByRepositoryIdAsync(int repositoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all C# projects for a solution.
    /// </summary>
    Task<Result<CSharpProject[], Error>> GetBySolutionIdAsync(int solutionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates the last report timestamp for a C# project.
    /// </summary>
    Task<Result<Unit, Error>> UpdateLastReportAsync(int id, DateTimeOffset lastReportAt, CancellationToken cancellationToken = default);
}
