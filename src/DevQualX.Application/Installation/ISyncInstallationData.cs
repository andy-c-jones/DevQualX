using DevQualX.Functional;

namespace DevQualX.Application.Installation;

/// <summary>
/// Syncs installation data (repositories and teams) from GitHub to the database.
/// Called when installation webhooks are received (installed, repositories added/removed).
/// </summary>
public interface ISyncInstallationData
{
    /// <summary>
    /// Syncs all repositories and teams for an installation to the database.
    /// </summary>
    /// <param name="installationId">GitHub installation ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success with sync summary, or failure with error.</returns>
    Task<Result<SyncResult, Error>> ExecuteAsync(
        long installationId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of syncing installation data.
/// </summary>
public record SyncResult(
    long InstallationId,
    int RepositoriesSynced,
    int TeamsSynced);
