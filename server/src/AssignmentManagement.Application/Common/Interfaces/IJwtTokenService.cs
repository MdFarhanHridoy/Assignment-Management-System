using AssignmentManagement.Domain;

namespace AssignmentManagement.Application.Common.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}
