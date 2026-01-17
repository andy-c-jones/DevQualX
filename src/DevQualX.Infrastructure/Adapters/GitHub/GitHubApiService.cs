using DevQualX.Domain.Infrastructure;
using DevQualX.Domain.Models;
using DevQualX.Functional;
using Octokit;

namespace DevQualX.Infrastructure.Adapters.GitHub;

/// <summary>
/// Handles GitHub API operations using Octokit.
/// </summary>
public class GitHubApiService : IGitHubApiService
{
    /// <inheritdoc />
    public async Task<Result<GitHubUser, Error>> GetUserAsync(
        string accessToken, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = CreateClient(accessToken);
            var user = await client.User.Current();

            return new GitHubUser(
                Id: user.Id,
                Login: user.Login,
                Email: user.Email,
                Name: user.Name,
                AvatarUrl: user.AvatarUrl,
                Type: user.Type?.ToString() ?? "User");
        }
        catch (AuthorizationException)
        {
            return new UnauthorizedError
            {
                Message = "GitHub access token is invalid or expired"
            };
        }
        catch (ApiException ex)
        {
            return new ExternalServiceError
            {
                Message = $"GitHub API error: {ex.Message}",
                ServiceName = "GitHub API",
                InnerMessage = $"Status: {(int)ex.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new InternalError
            {
                Message = "Unexpected error retrieving user from GitHub",
                ExceptionType = ex.GetType().Name,
                Metadata = new Dictionary<string, object> { ["ExceptionMessage"] = ex.Message }
            };
        }
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<GitHubInstallation>, Error>> GetUserInstallationsAsync(
        string accessToken, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = CreateClient(accessToken);
            var installationsResponse = await client.GitHubApps.GetAllInstallationsForCurrent();

            var installations = installationsResponse
                .Select(i => new GitHubInstallation(
                    Id: i.Id,
                    Account: new GitHubAccount(
                        Id: i.Account.Id,
                        Login: i.Account.Login,
                        Type: i.Account.Type?.ToString() ?? "User",
                        AvatarUrl: i.Account.AvatarUrl),
                    CreatedAt: DateTimeOffset.Now, // Placeholder - Octokit Installation doesn't expose this
                    UpdatedAt: DateTimeOffset.Now, // Placeholder - Octokit Installation doesn't expose this
                    SuspendedAt: i.SuspendedAt?.ToString("O"),
                    SuspendedBy: i.SuspendedBy != null 
                        ? new GitHubUser(
                            Id: i.SuspendedBy.Id,
                            Login: i.SuspendedBy.Login,
                            Email: i.SuspendedBy.Email,
                            Name: i.SuspendedBy.Name,
                            AvatarUrl: i.SuspendedBy.AvatarUrl,
                            Type: i.SuspendedBy.Type?.ToString() ?? "User")
                        : null))
                .ToList();

            return installations;
        }
        catch (AuthorizationException)
        {
            return new UnauthorizedError
            {
                Message = "GitHub access token is invalid or expired"
            };
        }
        catch (ApiException ex)
        {
            return new ExternalServiceError
            {
                Message = $"GitHub API error: {ex.Message}",
                ServiceName = "GitHub API",
                InnerMessage = $"Status: {(int)ex.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new InternalError
            {
                Message = "Unexpected error retrieving installations from GitHub",
                ExceptionType = ex.GetType().Name,
                Metadata = new Dictionary<string, object> { ["ExceptionMessage"] = ex.Message }
            };
        }
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<GitHubRepository>, Error>> GetInstallationRepositoriesAsync(
        string installationToken, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = CreateClient(installationToken);
            var reposResponse = await client.GitHubApps.Installation.GetAllRepositoriesForCurrent();

            var repositories = reposResponse.Repositories
                .Select(r => new GitHubRepository(
                    Id: r.Id,
                    Name: r.Name,
                    FullName: r.FullName,
                    Private: r.Private,
                    DefaultBranch: r.DefaultBranch,
                    Owner: new GitHubAccount(
                        Id: r.Owner.Id,
                        Login: r.Owner.Login,
                        Type: r.Owner.Type?.ToString() ?? "User",
                        AvatarUrl: r.Owner.AvatarUrl)))
                .ToList();

            return repositories;
        }
        catch (AuthorizationException)
        {
            return new UnauthorizedError
            {
                Message = "GitHub installation token is invalid or expired"
            };
        }
        catch (ApiException ex)
        {
            return new ExternalServiceError
            {
                Message = $"GitHub API error: {ex.Message}",
                ServiceName = "GitHub API",
                InnerMessage = $"Status: {(int)ex.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new InternalError
            {
                Message = "Unexpected error retrieving repositories from GitHub",
                ExceptionType = ex.GetType().Name,
                Metadata = new Dictionary<string, object> { ["ExceptionMessage"] = ex.Message }
            };
        }
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<GitHubTeam>, Error>> GetOrganizationTeamsAsync(
        string installationToken, 
        string organizationLogin, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = CreateClient(installationToken);
            var teams = await client.Organization.Team.GetAll(organizationLogin);

            var gitHubTeams = teams
                .Select(t => new GitHubTeam(
                    Id: t.Id,
                    Name: t.Name,
                    Slug: t.Slug,
                    Description: t.Description,
                    Privacy: t.Privacy.StringValue))
                .ToList();

            return gitHubTeams;
        }
        catch (AuthorizationException)
        {
            return new UnauthorizedError
            {
                Message = "GitHub installation token is invalid or expired"
            };
        }
        catch (ApiException ex)
        {
            return new ExternalServiceError
            {
                Message = $"GitHub API error: {ex.Message}",
                ServiceName = "GitHub API",
                InnerMessage = $"Status: {(int)ex.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new InternalError
            {
                Message = "Unexpected error retrieving teams from GitHub",
                ExceptionType = ex.GetType().Name,
                Metadata = new Dictionary<string, object> { ["ExceptionMessage"] = ex.Message }
            };
        }
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<GitHubUser>, Error>> GetTeamMembersAsync(
        string installationToken, 
        string organizationLogin, 
        string teamSlug, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = CreateClient(installationToken);
            
            // First, get the team by name to get its ID
            var team = await client.Organization.Team.GetByName(organizationLogin, teamSlug);
            
            // Then get team members using the team ID
            var members = await client.Organization.Team.GetAllMembers(team.Id);

            var gitHubUsers = members
                .Select(m => new GitHubUser(
                    Id: m.Id,
                    Login: m.Login,
                    Email: m.Email,
                    Name: m.Name,
                    AvatarUrl: m.AvatarUrl,
                    Type: m.Type?.ToString() ?? "User"))
                .ToList();

            return gitHubUsers;
        }
        catch (AuthorizationException)
        {
            return new UnauthorizedError
            {
                Message = "GitHub installation token is invalid or expired"
            };
        }
        catch (NotFoundException)
        {
            return new NotFoundError
            {
                Message = $"Team '{teamSlug}' not found in organization '{organizationLogin}'",
                ResourceType = "Team",
                ResourceId = teamSlug
            };
        }
        catch (ApiException ex)
        {
            return new ExternalServiceError
            {
                Message = $"GitHub API error: {ex.Message}",
                ServiceName = "GitHub API",
                InnerMessage = $"Status: {(int)ex.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new InternalError
            {
                Message = "Unexpected error retrieving team members from GitHub",
                ExceptionType = ex.GetType().Name,
                Metadata = new Dictionary<string, object> { ["ExceptionMessage"] = ex.Message }
            };
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool, Error>> ValidateTokenAsync(
        string accessToken, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = CreateClient(accessToken);
            await client.User.Current();
            return true;
        }
        catch (AuthorizationException)
        {
            return new UnauthorizedError
            {
                Message = "GitHub access token is invalid or expired"
            };
        }
        catch (ApiException ex)
        {
            return new ExternalServiceError
            {
                Message = $"GitHub API error: {ex.Message}",
                ServiceName = "GitHub API",
                InnerMessage = $"Status: {(int)ex.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new InternalError
            {
                Message = "Unexpected error validating token with GitHub",
                ExceptionType = ex.GetType().Name,
                Metadata = new Dictionary<string, object> { ["ExceptionMessage"] = ex.Message }
            };
        }
    }

    /// <summary>
    /// Creates a GitHubClient with the specified access token.
    /// </summary>
    private static GitHubClient CreateClient(string accessToken)
    {
        return new GitHubClient(new ProductHeaderValue("DevQualX"))
        {
            Credentials = new Credentials(accessToken)
        };
    }
}
