using DevQualX.Functional;
using DevQualX.Domain.Models;

namespace DevQualX.Domain.Data;

/// <summary>
/// Repository for GitHub Project (planning/tracking) data operations.
/// </summary>
public interface IGitHubProjectRepository
{
    /// <summary>
    /// Gets all GitHub Projects for an installation.
    /// </summary>
    Task<Result<GitHubProject[], Error>> GetByInstallationIdAsync(int installationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a GitHub Project by its GitHub project ID.
    /// </summary>
    Task<Result<GitHubProject, Error>> GetByGitHubIdAsync(long gitHubProjectId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Syncs GitHub Projects from GitHub for an installation.
    /// Creates new ones, updates existing ones, and deactivates removed ones.
    /// </summary>
    Task<Result<Unit, Error>> SyncProjectsAsync(
        int installationId,
        GitHubProject[] projects,
        CancellationToken cancellationToken = default);
}
