namespace DevQualX.Domain.Models;

/// <summary>
/// DTO representing GitHub OAuth access token response.
/// Returned from OAuth token exchange.
/// </summary>
public record GitHubAccessToken(
    string AccessToken,
    string TokenType,
    string Scope);
