namespace DevQualX.Domain.Models;

/// <summary>
/// Represents a GitHub Project (planning/tracking tool with issues).
/// This is distinct from C# Projects (csproj files).
/// </summary>
public record GitHubProject(
    int Id,
    long GitHubProjectId,
    int InstallationId,
    string Title,
    int Number,
    bool IsActive,
    DateTimeOffset SyncedAt);
