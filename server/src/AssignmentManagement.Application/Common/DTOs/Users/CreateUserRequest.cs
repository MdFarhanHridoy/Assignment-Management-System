using AssignmentManagement.Domain;

namespace AssignmentManagement.Application.Common.DTOs.Users;

public class CreateUserRequest
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public UserRole Role { get; set; }
}
