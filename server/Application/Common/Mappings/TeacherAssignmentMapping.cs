using AssignmentManagement.Application.Common.DTOs.TeacherAssignments;
using AssignmentManagement.Domain;

namespace AssignmentManagement.Application.Common.Mappings;

public static class TeacherAssignmentMapping
{
    public static TeacherAssignmentDto ToDto(this TeacherClassSubject t) => new()
    {
        Id = t.Id, TeacherId = t.TeacherId, ClassId = t.ClassId,
        SubjectId = t.SubjectId, CreatedAt = t.CreatedAt
    };
}
