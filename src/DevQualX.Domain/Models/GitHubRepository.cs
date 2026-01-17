namespace DevQualX.Domain.Models;

/// <summary>
/// DTO representing GitHub repository information.
/// Maps to GitHub's repository API responses.
/// </summary>
public record GitHubRepository(
    long Id,
    string Name,
    string FullName,
    bool Private,
    string? DefaultBranch,
    GitHubAccount Owner);
