using AssignmentManagement.Application.Common.DTOs.Users;
using AssignmentManagement.Domain;

namespace AssignmentManagement.Application.Common.Mappings;

public static class UserMapping
{
    public static UserDto ToDto(this User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        Role = user.Role,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };
}
