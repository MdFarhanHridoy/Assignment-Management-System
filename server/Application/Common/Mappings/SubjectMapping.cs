using AssignmentManagement.Application.Common.DTOs.Subjects;
using AssignmentManagement.Domain;

namespace AssignmentManagement.Application.Common.Mappings;

public static class SubjectMapping
{
    public static SubjectDto ToDto(this Subject s) => new()
    {
        Id = s.Id, Name = s.Name, ClassId = s.ClassId,
        CreatedAt = s.CreatedAt, UpdatedAt = s.UpdatedAt
    };
}
