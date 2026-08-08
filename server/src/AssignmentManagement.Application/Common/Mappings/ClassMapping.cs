using AssignmentManagement.Application.Common.DTOs.Classes;
using AssignmentManagement.Domain;

namespace AssignmentManagement.Application.Common.Mappings;

public static class ClassMapping
{
    public static ClassDto ToDto(this Class c) => new()
    {
        Id = c.Id, Name = c.Name, Description = c.Description,
        CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt
    };
}
