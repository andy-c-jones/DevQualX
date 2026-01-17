using DevQualX.Functional;
using DevQualX.Domain.Models;

namespace DevQualX.Domain.Data;

/// <summary>
/// Repository for GitHub team data operations.
/// </summary>
public interface ITeamRepository
{
    /// <summary>
    /// Gets all teams for an installation.
    /// </summary>
    Task<Result<Team[], Error>> GetByInstallationIdAsync(int installationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a team by its GitHub team ID.
    /// </summary>
    Task<Result<Team, Error>> GetByGitHubIdAsync(long gitHubTeamId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Syncs teams from GitHub for an installation.
    /// Creates new ones, updates existing ones, and deactivates removed ones.
    /// </summary>
    Task<Result<Unit, Error>> SyncTeamsAsync(
        int installationId,
        GitHubTeam[] teams,
        CancellationToken cancellationToken = default);
}
