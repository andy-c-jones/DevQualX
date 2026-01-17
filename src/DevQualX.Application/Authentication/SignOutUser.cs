using DevQualX.Functional;

namespace DevQualX.Application.Authentication;

/// <summary>
/// Signs out a user by clearing their authentication session.
/// </summary>
public class SignOutUser : ISignOutUser
{
    /// <inheritdoc />
    public Task<Result<Unit, Error>> ExecuteAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        // In a cookie-based authentication system, sign-out is handled by the Web/API layer
        // by clearing the authentication cookie. This service can be used for:
        // - Logging sign-out events
        // - Invalidating refresh tokens (if implemented)
        // - Clearing cached permissions
        
        // For now, just return success
        // Future: Add audit logging, token revocation, etc.
        return Task.FromResult<Result<Unit, Error>>(Unit.Default);
    }
}
