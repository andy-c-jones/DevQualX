namespace DevQualX.Domain.Models;

/// <summary>
/// DTO representing GitHub App installation access token.
/// Generated for making API calls on behalf of an installation.
/// </summary>
public record InstallationAccessToken(
    string Token,
    DateTimeOffset ExpiresAt);
