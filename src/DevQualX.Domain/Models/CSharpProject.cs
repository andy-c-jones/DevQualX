namespace DevQualX.Domain.Models;

/// <summary>
/// Represents a C# project (.csproj) within a repository.
/// Projects can be standalone or part of a solution.
/// </summary>
public record CSharpProject(
    int Id,
    int RepositoryId,
    int? SolutionId,
    string Name,
    string RelativePath,
    string? TargetFramework,
    DateTimeOffset DiscoveredAt,
    DateTimeOffset? LastReportAt);
