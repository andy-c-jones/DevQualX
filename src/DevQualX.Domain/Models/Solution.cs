namespace DevQualX.Domain.Models;

/// <summary>
/// Represents a C# solution (.sln or .slnx) discovered within a repository.
/// Solutions are auto-discovered from uploaded reports.
/// </summary>
public record Solution(
    int Id,
    int RepositoryId,
    string Name,
    string RelativePath,
    DateTimeOffset DiscoveredAt,
    DateTimeOffset? LastReportAt);
