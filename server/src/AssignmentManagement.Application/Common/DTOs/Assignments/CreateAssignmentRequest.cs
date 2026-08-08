namespace AssignmentManagement.Application.Common.DTOs.Assignments;

public class CreateAssignmentRequest
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime DeadlineUtc { get; set; }
    public int MaxMarks { get; set; }
    public Guid ClassId { get; set; }
    public Guid SubjectId { get; set; }
    public bool? AllowResubmission { get; set; }
}
