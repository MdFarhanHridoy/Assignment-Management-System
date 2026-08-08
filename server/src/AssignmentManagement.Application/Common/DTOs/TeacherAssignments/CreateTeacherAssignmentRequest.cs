namespace AssignmentManagement.Application.Common.DTOs.TeacherAssignments;

public class CreateTeacherAssignmentRequest
{
    public Guid TeacherId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SubjectId { get; set; }
}
