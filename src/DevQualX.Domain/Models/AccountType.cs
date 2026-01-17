namespace DevQualX.Domain.Models;

/// <summary>
/// Type of GitHub account where the app is installed.
/// </summary>
public enum AccountType
{
    /// <summary>
    /// GitHub organization account.
    /// </summary>
    Organization,
    
    /// <summary>
    /// Individual user account (personal repositories).
    /// </summary>
    User
}
