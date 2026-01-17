using DevQualX.Domain.Data;
using DevQualX.Domain.Infrastructure;
using DevQualX.Domain.Models;
using DevQualX.Functional;

namespace DevQualX.Application.Authentication;

/// <summary>
/// Completes the OAuth flow by exchanging authorization code for access token
/// and storing the authenticated user in the database.
/// </summary>
public class CompleteOAuth(
    IGitHubOAuthService gitHubOAuthService,
    IGitHubApiService gitHubApiService,
    IUserRepository userRepository) : ICompleteOAuth
{
    /// <inheritdoc />
    public async Task<Result<AuthenticatedUserResult, Error>> ExecuteAsync(
        string code,
        string codeVerifier,
        string redirectUri,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(code))
        {
            return new ValidationError
            {
                Message = "Authorization code is required",
                Code = "AUTH003",
                Errors = new Dictionary<string, string[]>
                {
                    [nameof(code)] = ["Authorization code cannot be empty"]
                }
            };
        }

        if (string.IsNullOrWhiteSpace(codeVerifier))
        {
            return new ValidationError
            {
                Message = "Code verifier is required",
                Code = "AUTH004",
                Errors = new Dictionary<string, string[]>
                {
                    [nameof(codeVerifier)] = ["Code verifier cannot be empty"]
                }
            };
        }

        // Step 1: Exchange authorization code for access token
        var tokenResult = await gitHubOAuthService.ExchangeCodeForTokenAsync(
            code,
            codeVerifier,
            redirectUri,
            cancellationToken);

        if (tokenResult is Failure<string, Error> tokenFailure)
        {
            return tokenFailure.Error;
        }

        var accessToken = ((Success<string, Error>)tokenResult).Value;

        // Step 2: Get user information from GitHub
        var userResult = await gitHubApiService.GetUserAsync(accessToken, cancellationToken);

        if (userResult is Failure<GitHubUser, Error> userFailure)
        {
            return userFailure.Error;
        }

        var gitHubUser = ((Success<GitHubUser, Error>)userResult).Value;

        // Step 3: Get or create user in database
        // NOTE: AccessToken is not persisted in database for security reasons
        // It should be stored in encrypted session/cookie instead
        var createResult = await userRepository.GetOrCreateAsync(
            gitHubUserId: gitHubUser.Id,
            username: gitHubUser.Login,
            email: gitHubUser.Email,
            avatarUrl: gitHubUser.AvatarUrl,
            cancellationToken);

        if (createResult is Failure<User, Error> createFailure)
        {
            return createFailure.Error;
        }

        var savedUser = ((Success<User, Error>)createResult).Value;

        // Step 4: Return authenticated user result
        // Access token is returned for storage in encrypted cookie/session
        return new AuthenticatedUserResult(
            GitHubUserId: savedUser.GitHubUserId,
            Login: savedUser.Username,
            Email: savedUser.Email,
            Name: gitHubUser.Name, // From GitHub API (not stored in DB)
            AvatarUrl: savedUser.AvatarUrl ?? string.Empty,
            AccessToken: accessToken); // For encrypted cookie/session storage
    }
}

