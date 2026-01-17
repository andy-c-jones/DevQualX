using DevQualX.Functional;
using DevQualX.Domain.Models;

namespace DevQualX.Domain.Data;

/// <summary>
/// Repository for user data operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets or creates a user by their GitHub user ID.
    /// Updates user info if they already exist.
    /// </summary>
    Task<Result<User, Error>> GetOrCreateAsync(
        long gitHubUserId,
        string username,
        string? email,
        string? avatarUrl,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a user by their internal ID.
    /// </summary>
    Task<Result<User, Error>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a user by their GitHub user ID.
    /// </summary>
    Task<Result<User, Error>> GetByGitHubIdAsync(long gitHubUserId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates a user's information.
    /// </summary>
    Task<Result<User, Error>> UpdateAsync(User user, CancellationToken cancellationToken = default);
}
