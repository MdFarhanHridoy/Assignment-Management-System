using AssignmentManagement.Domain;

namespace AssignmentManagement.Application.Common.DTOs.Users;

public class UpdateUserRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
}
