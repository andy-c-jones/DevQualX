using DevQualX.Functional;
using DevQualX.Domain.Models;

namespace DevQualX.Domain.Data;

/// <summary>
/// Repository for GitHub App installation data operations.
/// </summary>
public interface IInstallationRepository
{
    /// <summary>
    /// Creates a new installation record.
    /// </summary>
    Task<Result<Installation, Error>> CreateAsync(
        long gitHubInstallationId,
        long gitHubAccountId,
        AccountType accountType,
        string accountLogin,
        int installedBy,
        DateTimeOffset installedAt,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets an installation by its internal ID.
    /// </summary>
    Task<Result<Installation, Error>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets an installation by its GitHub installation ID.
    /// </summary>
    Task<Result<Installation, Error>> GetByGitHubIdAsync(long gitHubInstallationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all active installations for a user.
    /// </summary>
    Task<Result<Installation[], Error>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets an installation by account login (org/user name).
    /// </summary>
    Task<Result<Installation, Error>> GetByAccountLoginAsync(string accountLogin, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an installation's information.
    /// </summary>
    Task<Result<Installation, Error>> UpdateAsync(Installation installation, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Marks an installation as inactive (soft delete).
    /// </summary>
    Task<Result<Unit, Error>> DeactivateAsync(int id, CancellationToken cancellationToken = default);
}
