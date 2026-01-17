namespace DevQualX.Domain.Models;

/// <summary>
/// DTO representing GitHub team information.
/// Maps to GitHub's team API responses.
/// </summary>
public record GitHubTeam(
    long Id,
    string Slug,
    string Name,
    string? Description,
    string Privacy); // "secret" or "closed"
