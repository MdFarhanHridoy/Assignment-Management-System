using AssignmentManagement.Domain;

namespace AssignmentManagement.Application.Common.DTOs.Submissions;

public class SubmissionSummaryDto
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }
    public SubmissionStatus Status { get; set; }
    public int? Marks { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
}
