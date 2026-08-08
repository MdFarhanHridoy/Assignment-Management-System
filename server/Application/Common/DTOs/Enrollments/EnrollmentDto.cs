namespace AssignmentManagement.Application.Common.DTOs.Enrollments;

public class EnrollmentDto
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime EnrolledAt { get; set; }
}
