namespace DevQualX.Domain.Models;

/// <summary>
/// Represents a GitHub team within an organization.
/// Teams are synced from GitHub and can be assigned roles in DevQualX.
/// </summary>
public record Team(
    int Id,
    long GitHubTeamId,
    int InstallationId,
    string Slug,
    string Name,
    string? Description,
    DateTimeOffset SyncedAt,
    bool IsActive);
