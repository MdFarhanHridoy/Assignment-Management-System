namespace AssignmentManagement.Domain;

public class Assignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime DeadlineUtc { get; set; }
    public int MaxMarks { get; set; }
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;
    public Guid TeacherId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SubjectId { get; set; }
    public bool AllowResubmission { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
