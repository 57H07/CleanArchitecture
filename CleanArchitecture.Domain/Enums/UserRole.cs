namespace CleanArchitecture.Domain.Enums;

/// <summary>
/// Represents the different roles a user can have in the system
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Regular user with basic permissions
    /// </summary>
    User = 1,
    
    /// <summary>
    /// Administrator with elevated permissions
    /// </summary>
    Admin = 2,
    
    /// <summary>
    /// Moderator with limited administrative permissions
    /// </summary>
    Moderator = 3
}
