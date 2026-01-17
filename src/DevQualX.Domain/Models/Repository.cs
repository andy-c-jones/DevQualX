namespace DevQualX.Domain.Models;

/// <summary>
/// Represents a GitHub repository that DevQualX has access to via an installation.
/// Repositories contain solutions and C# projects that are analyzed.
/// </summary>
public record Repository(
    int Id,
    long GitHubRepositoryId,
    int InstallationId,
    string Name,
    string FullName,
    bool IsPrivate,
    string? DefaultBranch,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
