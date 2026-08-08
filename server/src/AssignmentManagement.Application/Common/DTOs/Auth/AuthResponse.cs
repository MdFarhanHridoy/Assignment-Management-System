using AssignmentManagement.Application.Common.DTOs.Users;

namespace AssignmentManagement.Application.Common.DTOs.Auth;

public class AuthResponse
{
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}
