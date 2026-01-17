namespace DevQualX.Domain.Models;

/// <summary>
/// Roles that can be assigned to users or teams within an organization.
/// Mirrors GitHub's role system: Owner > Admin > Maintainer > Reader
/// </summary>
public enum Role
{
    /// <summary>
    /// Full control over the organization including installation/uninstallation.
    /// Organization-wide role only.
    /// </summary>
    Owner,
    
    /// <summary>
    /// Can perform most actions except organization reconfiguration/uninstallation.
    /// Can be assigned at organization, repository, or GitHub project level.
    /// </summary>
    Admin,
    
    /// <summary>
    /// Can edit repository configuration but not remove/uninstall repositories.
    /// Can be assigned at organization, repository, or GitHub project level.
    /// </summary>
    Maintainer,
    
    /// <summary>
    /// Read-only access to view reports and metrics.
    /// Can be assigned at organization, repository, or GitHub project level.
    /// </summary>
    Reader
}
