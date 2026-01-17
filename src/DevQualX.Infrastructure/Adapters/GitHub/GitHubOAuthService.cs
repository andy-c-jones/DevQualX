using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using DevQualX.Domain.Infrastructure;
using DevQualX.Functional;
using Microsoft.Extensions.Configuration;

namespace DevQualX.Infrastructure.Adapters.GitHub;

/// <summary>
/// Handles GitHub OAuth authentication flow with PKCE.
/// </summary>
public class GitHubOAuthService(IConfiguration configuration, HttpClient httpClient) : IGitHubOAuthService
{
    private readonly string _clientId = configuration["GitHub:ClientId"] 
        ?? throw new InvalidOperationException("GitHub:ClientId configuration is missing");
    private readonly string _clientSecret = configuration["GitHub:ClientSecret"] 
        ?? throw new InvalidOperationException("GitHub:ClientSecret configuration is missing");

    /// <inheritdoc />
    public string GetAuthorizationUrl(string codeVerifier, string state, string redirectUri)
    {
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = _clientId,
            ["redirect_uri"] = redirectUri,
            ["scope"] = "read:user user:email read:org",
            ["state"] = state,
            ["code_challenge"] = codeChallenge,
            ["code_challenge_method"] = "S256"
        };

        var queryString = string.Join("&", parameters.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
        return $"https://github.com/login/oauth/authorize?{queryString}";
    }

    /// <inheritdoc />
    public async Task<Result<string, Error>> ExchangeCodeForTokenAsync(
        string code, 
        string codeVerifier, 
        string redirectUri, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = _clientId,
                    ["client_secret"] = _clientSecret,
                    ["code"] = code,
                    ["redirect_uri"] = redirectUri,
                    ["code_verifier"] = codeVerifier
                })
            };
            request.Headers.Add("Accept", "application/json");

            var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return new ExternalServiceError
                {
                    Message = "Failed to exchange OAuth code for token",
                    ServiceName = "GitHub OAuth",
                    InnerMessage = $"Status: {(int)response.StatusCode}, Body: {errorContent}"
                };
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<GitHubTokenResponse>(cancellationToken);

            if (tokenResponse?.AccessToken is null)
            {
                return new ExternalServiceError
                {
                    Message = "GitHub OAuth token response did not contain access_token",
                    ServiceName = "GitHub OAuth"
                };
            }

            return tokenResponse.AccessToken;
        }
        catch (HttpRequestException ex)
        {
            return new ExternalServiceError
            {
                Message = "Network error while communicating with GitHub OAuth",
                ServiceName = "GitHub OAuth",
                InnerMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            return new InternalError
            {
                Message = "Unexpected error during OAuth token exchange",
                ExceptionType = ex.GetType().Name,
                Metadata = new Dictionary<string, object> { ["ExceptionMessage"] = ex.Message }
            };
        }
    }

    /// <summary>
    /// Generates SHA256 code challenge from code verifier for PKCE.
    /// </summary>
    private static string GenerateCodeChallenge(string codeVerifier)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier));
        return Convert.ToBase64String(hash)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    /// <summary>
    /// GitHub OAuth token response.
    /// </summary>
    private sealed record GitHubTokenResponse(
        string? AccessToken = null,
        string? TokenType = null,
        string? Scope = null);
}
