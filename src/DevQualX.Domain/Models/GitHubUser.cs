namespace DevQualX.Domain.Models;

/// <summary>
/// DTO representing GitHub user information from OAuth flow.
/// Maps to GitHub's /user endpoint response.
/// </summary>
public record GitHubUser(
    long Id,
    string Login,
    string? Email,
    string? Name,
    string AvatarUrl,
    string Type); // "User" or "Organization"
