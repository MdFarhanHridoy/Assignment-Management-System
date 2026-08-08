namespace AssignmentManagement.Domain;

public class TeacherClassSubject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TeacherId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SubjectId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
