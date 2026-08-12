using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Domain;

namespace AssignmentManagement.UnitTests.TestHelpers;

/// <summary>
/// Simple fake <see cref="IPasswordHasher"/> — hashes by prefixing "hash-"
/// and verifies by string equality.  No BCrypt cost in unit tests.
/// </summary>
public class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password) => $"hash-{password}";

    public bool Verify(string password, string hash) => hash == $"hash-{password}";
}

/// <summary>
/// Fake <see cref="IJwtTokenService"/> — returns a deterministic token string
/// and a fixed expiry.  No real JWT signing in unit tests.
/// </summary>
public class FakeJwtTokenService : IJwtTokenService
{
    public (string Token, DateTime ExpiresAtUtc) GenerateToken(User user) =>
        ($"fake-token-{user.Email}", DateTime.UtcNow.AddHours(2));
}
