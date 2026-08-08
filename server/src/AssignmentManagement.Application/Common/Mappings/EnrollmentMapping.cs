using AssignmentManagement.Application.Common.DTOs.Enrollments;
using AssignmentManagement.Domain;

namespace AssignmentManagement.Application.Common.Mappings;

public static class EnrollmentMapping
{
    public static EnrollmentDto ToDto(this Enrollment e) => new()
    {
        Id = e.Id, ClassId = e.ClassId, StudentId = e.StudentId, EnrolledAt = e.EnrolledAt
    };
}
