namespace DevQualX.Domain.Models;

/// <summary>
/// Represents a role assignment to a user or team.
/// Roles can be scoped to organization, repository, or GitHub project level.
/// </summary>
public record RoleAssignment(
    int Id,
    int InstallationId,
    int? UserId,
    int? TeamId,
    Role Role,
    RoleScope Scope,
    int? ResourceId,
    int GrantedBy,
    DateTimeOffset GrantedAt);
