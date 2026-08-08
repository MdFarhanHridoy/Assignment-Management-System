namespace AssignmentManagement.Domain;

public class Submission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }
    public string AnswerText { get; set; } = null!;
    public DateTime SubmittedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;
    public int? Marks { get; set; }
    public string? Feedback { get; set; }
    public Guid? ReviewedByTeacherId { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
}
