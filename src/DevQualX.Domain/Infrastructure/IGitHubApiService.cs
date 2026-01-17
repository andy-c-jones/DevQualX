using DevQualX.Domain.Models;
using DevQualX.Functional;

namespace DevQualX.Domain.Infrastructure;

/// <summary>
/// Service for interacting with the GitHub API.
/// Handles user info, installations, repositories, and teams.
/// </summary>
public interface IGitHubApiService
{
    /// <summary>
    /// Gets the authenticated user's information from GitHub.
    /// </summary>
    /// <param name="accessToken">GitHub OAuth access token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success with GitHubUser, or failure with error.</returns>
    Task<Result<GitHubUser, Error>> GetUserAsync(
        string accessToken, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all installations accessible to the authenticated user.
    /// Includes both user installations and organization installations where user has access.
    /// </summary>
    /// <param name="accessToken">GitHub OAuth access token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success with list of installations, or failure with error.</returns>
    Task<Result<IReadOnlyList<GitHubInstallation>, Error>> GetUserInstallationsAsync(
        string accessToken, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all repositories accessible via a specific installation.
    /// </summary>
    /// <param name="installationToken">GitHub App installation token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success with list of repositories, or failure with error.</returns>
    Task<Result<IReadOnlyList<GitHubRepository>, Error>> GetInstallationRepositoriesAsync(
        string installationToken, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all teams in an organization.
    /// </summary>
    /// <param name="installationToken">GitHub App installation token.</param>
    /// <param name="organizationLogin">Organization login (username).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success with list of teams, or failure with error.</returns>
    Task<Result<IReadOnlyList<GitHubTeam>, Error>> GetOrganizationTeamsAsync(
        string installationToken, 
        string organizationLogin, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets members of a specific team.
    /// </summary>
    /// <param name="installationToken">GitHub App installation token.</param>
    /// <param name="organizationLogin">Organization login (username).</param>
    /// <param name="teamSlug">Team slug (URL-friendly identifier).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success with list of team members, or failure with error.</returns>
    Task<Result<IReadOnlyList<GitHubUser>, Error>> GetTeamMembersAsync(
        string installationToken, 
        string organizationLogin, 
        string teamSlug, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a GitHub access token is still valid.
    /// </summary>
    /// <param name="accessToken">GitHub OAuth access token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success with true if valid, failure with error if invalid or expired.</returns>
    Task<Result<bool, Error>> ValidateTokenAsync(
        string accessToken, 
        CancellationToken cancellationToken = default);
}
