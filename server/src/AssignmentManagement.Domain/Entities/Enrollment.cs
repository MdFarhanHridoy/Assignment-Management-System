namespace AssignmentManagement.Domain;

public class Enrollment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClassId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
