using DevQualX.Functional;

namespace DevQualX.Application.Authentication;

/// <summary>
/// Completes the OAuth flow by exchanging authorization code for access token
/// and storing the authenticated user in the database.
/// </summary>
public interface ICompleteOAuth
{
    /// <summary>
    /// Exchanges OAuth code for token, retrieves user info, and stores in database.
    /// </summary>
    /// <param name="code">Authorization code from GitHub callback.</param>
    /// <param name="codeVerifier">PKCE code verifier from session.</param>
    /// <param name="redirectUri">OAuth callback URI (must match authorization request).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success with authenticated user result, or failure with error.</returns>
    Task<Result<AuthenticatedUserResult, Error>> ExecuteAsync(
        string code,
        string codeVerifier,
        string redirectUri,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of completing OAuth flow.
/// Contains user information and access token.
/// </summary>
public record AuthenticatedUserResult(
    long GitHubUserId,
    string Login,
    string? Email,
    string? Name,
    string AvatarUrl,
    string AccessToken);
