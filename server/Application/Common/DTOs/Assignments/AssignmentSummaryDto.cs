using AssignmentManagement.Domain;

namespace AssignmentManagement.Application.Common.DTOs.Assignments;

public class AssignmentSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public AssignmentStatus Status { get; set; }
    public Guid TeacherId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SubjectId { get; set; }
    public DateTime DeadlineUtc { get; set; }
    public int MaxMarks { get; set; }
    public DateTime CreatedAt { get; set; }
}
