using DevQualX.Domain.Models;
using DevQualX.Functional;

namespace DevQualX.Application.Installation;

/// <summary>
/// Gets all GitHub App installations accessible to the authenticated user.
/// Used for organization selection after sign-in.
/// </summary>
public interface IGetUserInstallations
{
    /// <summary>
    /// Retrieves all installations the user has access to from GitHub.
    /// </summary>
    /// <param name="accessToken">User's GitHub OAuth access token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success with list of installations, or failure with error.</returns>
    Task<Result<IReadOnlyList<GitHubInstallation>, Error>> ExecuteAsync(
        string accessToken,
        CancellationToken cancellationToken = default);
}
