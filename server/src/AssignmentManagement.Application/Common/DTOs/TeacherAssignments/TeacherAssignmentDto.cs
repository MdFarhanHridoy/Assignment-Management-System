namespace AssignmentManagement.Application.Common.DTOs.TeacherAssignments;

public class TeacherAssignmentDto
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SubjectId { get; set; }
    public DateTime CreatedAt { get; set; }
}
