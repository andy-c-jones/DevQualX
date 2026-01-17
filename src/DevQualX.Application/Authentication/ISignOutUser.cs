using DevQualX.Functional;

namespace DevQualX.Application.Authentication;

/// <summary>
/// Signs out a user by clearing their authentication session.
/// </summary>
public interface ISignOutUser
{
    /// <summary>
    /// Clears the user's authentication session.
    /// This is typically called before the Web/API clears cookies/sessions.
    /// </summary>
    /// <param name="userId">The internal user ID to sign out.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success with Unit, or failure with error.</returns>
    Task<Result<Unit, Error>> ExecuteAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
