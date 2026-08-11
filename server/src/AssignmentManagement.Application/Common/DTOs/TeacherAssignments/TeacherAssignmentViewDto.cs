namespace AssignmentManagement.Application.Common.DTOs.TeacherAssignments;

public class TeacherAssignmentViewDto
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = null!;
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = null!;
}
