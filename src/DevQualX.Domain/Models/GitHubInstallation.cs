namespace DevQualX.Domain.Models;

/// <summary>
/// DTO representing GitHub App installation information.
/// Maps to GitHub's installation webhook payloads and API responses.
/// </summary>
public record GitHubInstallation(
    long Id,
    GitHubAccount Account,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string? SuspendedAt,
    GitHubUser? SuspendedBy);

/// <summary>
/// Account (organization or user) where the app is installed.
/// </summary>
public record GitHubAccount(
    long Id,
    string Login,
    string Type, // "Organization" or "User"
    string AvatarUrl);
