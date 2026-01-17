namespace DevQualX.Domain.Models;

/// <summary>
/// Scope at which a role assignment applies.
/// </summary>
public enum RoleScope
{
    /// <summary>
    /// Role applies to the entire organization (all repos, projects, etc).
    /// </summary>
    Organization,
    
    /// <summary>
    /// Role applies to a specific repository.
    /// </summary>
    Repository,
    
    /// <summary>
    /// Role applies to a specific GitHub Project (planning/tracking).
    /// </summary>
    GitHubProject
}
