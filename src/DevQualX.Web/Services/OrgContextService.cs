using System.Security.Claims;

namespace DevQualX.Web.Services;

/// <summary>
/// Service for managing the currently selected organization context.
/// Handles storing and retrieving the selected installation ID from session.
/// </summary>
public class OrgContextService(IHttpContextAccessor httpContextAccessor)
{
    private const string SelectedInstallationIdKey = "SelectedInstallationId";
    
    /// <summary>
    /// Gets the currently authenticated user's ID from claims.
    /// </summary>
    public long? GetCurrentUserId()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }
        
        var userIdClaim = httpContext.User.FindFirst("github_user_id")?.Value;
        if (long.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets the currently authenticated user's username from claims.
    /// </summary>
    public string? GetCurrentUsername()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }
        
        return httpContext.User.FindFirst(ClaimTypes.Name)?.Value;
    }
    
    /// <summary>
    /// Gets the currently authenticated user's email from claims.
    /// </summary>
    public string? GetCurrentUserEmail()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }
        
        return httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
    }
    
    /// <summary>
    /// Gets the currently authenticated user's avatar URL from claims.
    /// </summary>
    public string? GetCurrentUserAvatarUrl()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }
        
        return httpContext.User.FindFirst("avatar_url")?.Value;
    }
    
    /// <summary>
    /// Gets the access token for the currently authenticated user from claims.
    /// </summary>
    public string? GetAccessToken()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }
        
        return httpContext.User.FindFirst("access_token")?.Value;
    }
    
    /// <summary>
    /// Gets the currently selected installation (organization) ID from session.
    /// </summary>
    public int? GetSelectedInstallationId()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.Session == null)
        {
            return null;
        }
        
        return httpContext.Session.GetInt32(SelectedInstallationIdKey);
    }
    
    /// <summary>
    /// Sets the selected installation (organization) ID in session.
    /// </summary>
    public void SetSelectedInstallationId(int installationId)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.Session == null)
        {
            throw new InvalidOperationException("Session is not available.");
        }
        
        httpContext.Session.SetInt32(SelectedInstallationIdKey, installationId);
    }
    
    /// <summary>
    /// Clears the selected installation (organization) ID from session.
    /// </summary>
    public void ClearSelectedInstallation()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.Session == null)
        {
            return;
        }
        
        httpContext.Session.Remove(SelectedInstallationIdKey);
    }
    
    /// <summary>
    /// Checks if the user is authenticated.
    /// </summary>
    public bool IsAuthenticated()
    {
        var httpContext = httpContextAccessor.HttpContext;
        return httpContext?.User?.Identity?.IsAuthenticated == true;
    }
    
    /// <summary>
    /// Checks if the user has selected an organization.
    /// </summary>
    public bool HasSelectedOrganization()
    {
        return GetSelectedInstallationId() != null;
    }
}
