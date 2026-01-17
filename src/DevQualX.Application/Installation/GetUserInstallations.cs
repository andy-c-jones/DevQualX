using DevQualX.Domain.Infrastructure;
using DevQualX.Domain.Models;
using DevQualX.Functional;

namespace DevQualX.Application.Installation;

/// <summary>
/// Gets all GitHub App installations accessible to the authenticated user.
/// </summary>
public class GetUserInstallations(IGitHubApiService gitHubApiService) : IGetUserInstallations
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<GitHubInstallation>, Error>> ExecuteAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        // Validate access token
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return new UnauthorizedError
            {
                Message = "Access token is required"
            };
        }

        // Get installations from GitHub
        return await gitHubApiService.GetUserInstallationsAsync(accessToken, cancellationToken);
    }
}
