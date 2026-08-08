using AssignmentManagement.Application.Common.Interfaces;

namespace AssignmentManagement.Infrastructure.Identity;

public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 11;

    public string Hash(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Password must not be null or empty.", nameof(password));
        }

        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
    }

    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
    }
}
