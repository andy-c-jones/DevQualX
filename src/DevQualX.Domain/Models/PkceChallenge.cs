namespace DevQualX.Domain.Models;

/// <summary>
/// PKCE (Proof Key for Code Exchange) challenge for OAuth flow.
/// Used to prevent authorization code interception attacks.
/// </summary>
public record PkceChallenge(
    string CodeVerifier,
    string CodeChallenge);
