namespace DevQualX.Domain.Models;

/// <summary>
/// Represents a GitHub App installation on an organization or user account.
/// Installations grant DevQualX access to repositories.
/// </summary>
public record Installation(
    int Id,
    long GitHubInstallationId,
    long GitHubAccountId,
    AccountType AccountType,
    string AccountLogin,
    int InstalledBy,
    DateTimeOffset InstalledAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? SuspendedAt,
    bool IsActive);
