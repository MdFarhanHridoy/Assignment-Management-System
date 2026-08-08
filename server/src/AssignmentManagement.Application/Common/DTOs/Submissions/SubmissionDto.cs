using AssignmentManagement.Domain;

namespace AssignmentManagement.Application.Common.DTOs.Submissions;

public class SubmissionDto
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }
    public string AnswerText { get; set; } = null!;
    public DateTime SubmittedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public SubmissionStatus Status { get; set; }
    public int? Marks { get; set; }
    public string? Feedback { get; set; }
    public Guid? ReviewedByTeacherId { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
}
