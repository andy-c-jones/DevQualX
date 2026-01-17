using DevQualX.Functional;

namespace DevQualX.Domain.Infrastructure;

/// <summary>
/// Service for GitHub App operations.
/// Handles JWT generation and installation token creation.
/// </summary>
public interface IGitHubAppService
{
    /// <summary>
    /// Generates a short-lived JWT for authenticating as the GitHub App.
    /// Required for creating installation tokens.
    /// </summary>
    /// <returns>Success with JWT string, or failure with error.</returns>
    Result<string, Error> GenerateAppJwt();

    /// <summary>
    /// Creates an installation access token for a specific GitHub App installation.
    /// Installation tokens have repository-scoped permissions.
    /// </summary>
    /// <param name="installationId">GitHub installation ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success with installation token, or failure with error.</returns>
    Task<Result<string, Error>> CreateInstallationTokenAsync(
        long installationId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that an installation token is valid by checking with GitHub.
    /// Used to authenticate report upload requests from GitHub Actions.
    /// </summary>
    /// <param name="installationToken">Installation token to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success with installation ID if valid, failure with error if invalid.</returns>
    Task<Result<long, Error>> ValidateInstallationTokenAsync(
        string installationToken, 
        CancellationToken cancellationToken = default);
}
