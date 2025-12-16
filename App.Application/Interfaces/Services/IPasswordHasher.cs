namespace App.Application.Interfaces.Services;

/// <summary>
/// Service for hashing and verifying passwords
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hash a password using a secure hashing algorithm
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Hashed password</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verify that a password matches a hash
    /// </summary>
    /// <param name="hashedPassword">The hashed password</param>
    /// <param name="providedPassword">The password to verify</param>
    /// <returns>True if password matches, false otherwise</returns>
    bool VerifyPassword(string hashedPassword, string providedPassword);
}
