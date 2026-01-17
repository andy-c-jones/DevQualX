using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using DevQualX.Domain.Infrastructure;
using DevQualX.Functional;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DevQualX.Infrastructure.Adapters.GitHub;

/// <summary>
/// Handles GitHub App operations (JWT generation, installation tokens).
/// </summary>
public class GitHubAppService(IConfiguration configuration, HttpClient httpClient) : IGitHubAppService
{
    private readonly string _appId = configuration["GitHub:AppId"] 
        ?? throw new InvalidOperationException("GitHub:AppId configuration is missing");
    private readonly string _privateKeyPem = configuration["GitHub:PrivateKey"] 
        ?? throw new InvalidOperationException("GitHub:PrivateKey configuration is missing");

    /// <inheritdoc />
    public Result<string, Error> GenerateAppJwt()
    {
        try
        {
            var now = DateTimeOffset.UtcNow;
            var expires = now.AddMinutes(10); // GitHub recommends 10 minutes max

            var rsa = RSA.Create();
            rsa.ImportFromPem(_privateKeyPem);

            var signingCredentials = new SigningCredentials(
                new RsaSecurityKey(rsa), 
                SecurityAlgorithms.RsaSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Exp, expires.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Iss, _appId)
            };

            var token = new JwtSecurityToken(
                issuer: _appId,
                claims: claims,
                notBefore: now.DateTime,
                expires: expires.DateTime,
                signingCredentials: signingCredentials);

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.WriteToken(token);

            return jwt;
        }
        catch (CryptographicException ex)
        {
            return new InternalError
            {
                Message = "Failed to parse GitHub App private key",
                ExceptionType = ex.GetType().Name,
                Metadata = new Dictionary<string, object> { ["ExceptionMessage"] = ex.Message }
            };
        }
        catch (Exception ex)
        {
            return new InternalError
            {
                Message = "Failed to generate GitHub App JWT",
                ExceptionType = ex.GetType().Name,
                Metadata = new Dictionary<string, object> { ["ExceptionMessage"] = ex.Message }
            };
        }
    }

    /// <inheritdoc />
    public async Task<Result<string, Error>> CreateInstallationTokenAsync(
        long installationId, 
        CancellationToken cancellationToken = default)
    {
        var jwtResult = GenerateAppJwt();
        
        // Check if JWT generation succeeded using Match
        return await jwtResult.Match(
            success: async jwt => await CreateTokenWithJwt(jwt, installationId, cancellationToken),
            failure: error => Task.FromResult<Result<string, Error>>(error));
    }

    private async Task<Result<string, Error>> CreateTokenWithJwt(
        string jwt, 
        long installationId, 
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post, 
                $"https://api.github.com/app/installations/{installationId}/access_tokens");

            request.Headers.Add("Accept", "application/vnd.github+json");
            request.Headers.Add("Authorization", $"Bearer {jwt}");
            request.Headers.Add("User-Agent", "DevQualX");
            request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");

            var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new NotFoundError
                    {
                        Message = $"GitHub installation {installationId} not found",
                        ResourceType = "Installation",
                        ResourceId = installationId.ToString()
                    };
                }

                return new ExternalServiceError
                {
                    Message = "Failed to create installation token",
                    ServiceName = "GitHub App",
                    InnerMessage = $"Status: {(int)response.StatusCode}, Body: {errorContent}"
                };
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<InstallationTokenResponse>(content);

            if (tokenResponse?.Token is null)
            {
                return new ExternalServiceError
                {
                    Message = "GitHub installation token response did not contain token",
                    ServiceName = "GitHub App"
                };
            }

            return tokenResponse.Token;
        }
        catch (HttpRequestException ex)
        {
            return new ExternalServiceError
            {
                Message = "Network error while creating installation token",
                ServiceName = "GitHub App",
                InnerMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            return new InternalError
            {
                Message = "Unexpected error creating installation token",
                ExceptionType = ex.GetType().Name,
                Metadata = new Dictionary<string, object> { ["ExceptionMessage"] = ex.Message }
            };
        }
    }

    /// <inheritdoc />
    public async Task<Result<long, Error>> ValidateInstallationTokenAsync(
        string installationToken, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Call GitHub API to get token metadata
            var request = new HttpRequestMessage(
                HttpMethod.Get, 
                "https://api.github.com/installation/repositories");

            request.Headers.Add("Accept", "application/vnd.github+json");
            request.Headers.Add("Authorization", $"Bearer {installationToken}");
            request.Headers.Add("User-Agent", "DevQualX");
            request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");

            var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return new UnauthorizedError
                {
                    Message = "Installation token is invalid or expired"
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return new ExternalServiceError
                {
                    Message = "Failed to validate installation token",
                    ServiceName = "GitHub App",
                    InnerMessage = $"Status: {(int)response.StatusCode}, Body: {errorContent}"
                };
            }

            // Token is valid - return a placeholder installation ID
            // In production, this should parse installation ID from response or use different validation approach
            return new InternalError
            {
                Message = "Installation token validation not fully implemented - requires installation ID tracking",
                Code = "NOT_IMPLEMENTED"
            };
        }
        catch (HttpRequestException ex)
        {
            return new ExternalServiceError
            {
                Message = "Network error while validating installation token",
                ServiceName = "GitHub App",
                InnerMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            return new InternalError
            {
                Message = "Unexpected error validating installation token",
                ExceptionType = ex.GetType().Name,
                Metadata = new Dictionary<string, object> { ["ExceptionMessage"] = ex.Message }
            };
        }
    }

    /// <summary>
    /// GitHub installation token response.
    /// </summary>
    private sealed record InstallationTokenResponse(string? Token = null, string? ExpiresAt = null);
}
