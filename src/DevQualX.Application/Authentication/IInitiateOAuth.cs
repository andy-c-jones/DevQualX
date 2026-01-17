using DevQualX.Functional;

namespace DevQualX.Application.Authentication;

/// <summary>
/// Initiates the OAuth flow with GitHub using PKCE.
/// Returns the authorization URL and PKCE code verifier for the client.
/// </summary>
public interface IInitiateOAuth
{
    /// <summary>
    /// Generates PKCE values and creates the GitHub OAuth authorization URL.
    /// </summary>
    /// <param name="redirectUri">The URI to redirect to after OAuth authorization.</param>
    /// <param name="state">Optional state parameter for CSRF protection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success with OAuth initiation result, or failure with error.</returns>
    Task<Result<OAuthInitiationResult, Error>> ExecuteAsync(
        string redirectUri,
        string? state = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of initiating OAuth flow.
/// Contains authorization URL and PKCE code verifier to store in session.
/// </summary>
public record OAuthInitiationResult(
    string AuthorizationUrl,
    string CodeVerifier,
    string State);
