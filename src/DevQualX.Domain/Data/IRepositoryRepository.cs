using DevQualX.Functional;
using DevQualX.Domain.Models;

namespace DevQualX.Domain.Data;

/// <summary>
/// Repository for repository (GitHub repo) data operations.
/// </summary>
public interface IRepositoryRepository
{
    /// <summary>
    /// Creates a new repository record.
    /// </summary>
    Task<Result<Repository, Error>> CreateAsync(
        long gitHubRepositoryId,
        int installationId,
        string name,
        string fullName,
        bool isPrivate,
        string? defaultBranch,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a repository by its internal ID.
    /// </summary>
    Task<Result<Repository, Error>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a repository by its GitHub repository ID.
    /// </summary>
    Task<Result<Repository, Error>> GetByGitHubIdAsync(long gitHubRepositoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a repository by its full name (org/repo).
    /// </summary>
    Task<Result<Repository, Error>> GetByFullNameAsync(string fullName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all repositories for an installation.
    /// </summary>
    Task<Result<Repository[], Error>> GetByInstallationIdAsync(int installationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates a repository's information.
    /// </summary>
    Task<Result<Repository, Error>> UpdateAsync(Repository repository, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Marks a repository as inactive (soft delete).
    /// </summary>
    Task<Result<Unit, Error>> DeactivateAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Bulk syncs repositories for an installation.
    /// Creates new ones, updates existing ones, and deactivates removed ones.
    /// </summary>
    Task<Result<Unit, Error>> SyncRepositoriesAsync(
        int installationId,
        GitHubRepository[] repositories,
        CancellationToken cancellationToken = default);
}
