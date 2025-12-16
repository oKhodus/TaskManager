namespace App.Domain.Enums;

/// <summary>
/// User role for access control
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Regular worker - can view and edit own tasks
    /// </summary>
    Worker = 0,

    /// <summary>
    /// Administrator - can view and edit all tasks, manage users
    /// </summary>
    Admin = 1
}
