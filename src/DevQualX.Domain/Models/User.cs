namespace DevQualX.Domain.Models;

/// <summary>
/// Represents a user in the DevQualX system.
/// Users are created when they sign in with GitHub OAuth.
/// </summary>
public record User(
    int Id,
    long GitHubUserId,
    string Username,
    string? Email,
    string? AvatarUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
