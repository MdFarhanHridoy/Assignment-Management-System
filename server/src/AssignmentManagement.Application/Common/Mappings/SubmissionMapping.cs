using AssignmentManagement.Application.Common.DTOs.Submissions;
using AssignmentManagement.Domain;

namespace AssignmentManagement.Application.Common.Mappings;

public static class SubmissionMapping
{
    public static SubmissionDto ToDto(this Submission s) => new()
    {
        Id = s.Id, AssignmentId = s.AssignmentId, StudentId = s.StudentId,
        AnswerText = s.AnswerText, SubmittedAtUtc = s.SubmittedAtUtc,
        UpdatedAtUtc = s.UpdatedAtUtc, Status = s.Status, Marks = s.Marks,
        Feedback = s.Feedback, ReviewedByTeacherId = s.ReviewedByTeacherId,
        ReviewedAtUtc = s.ReviewedAtUtc
    };

    public static SubmissionSummaryDto ToSummaryDto(this Submission s) => new()
    {
        Id = s.Id, AssignmentId = s.AssignmentId, StudentId = s.StudentId,
        Status = s.Status, Marks = s.Marks, SubmittedAtUtc = s.SubmittedAtUtc,
        ReviewedAtUtc = s.ReviewedAtUtc
    };
}
