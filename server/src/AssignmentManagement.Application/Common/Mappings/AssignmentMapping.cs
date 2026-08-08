using AssignmentManagement.Application.Common.DTOs.Assignments;
using AssignmentManagement.Domain;

namespace AssignmentManagement.Application.Common.Mappings;

public static class AssignmentMapping
{
    public static AssignmentDto ToDto(this Assignment a) => new()
    {
        Id = a.Id, Title = a.Title, Description = a.Description,
        DeadlineUtc = a.DeadlineUtc, MaxMarks = a.MaxMarks, Status = a.Status,
        TeacherId = a.TeacherId, ClassId = a.ClassId, SubjectId = a.SubjectId,
        AllowResubmission = a.AllowResubmission, CreatedAt = a.CreatedAt, UpdatedAt = a.UpdatedAt
    };

    public static AssignmentSummaryDto ToSummaryDto(this Assignment a) => new()
    {
        Id = a.Id, Title = a.Title, Status = a.Status,
        TeacherId = a.TeacherId, ClassId = a.ClassId, SubjectId = a.SubjectId,
        DeadlineUtc = a.DeadlineUtc, MaxMarks = a.MaxMarks, CreatedAt = a.CreatedAt
    };
}
