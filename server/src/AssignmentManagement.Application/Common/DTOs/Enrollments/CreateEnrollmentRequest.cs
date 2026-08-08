namespace AssignmentManagement.Application.Common.DTOs.Enrollments;

public class CreateEnrollmentRequest
{
    public Guid ClassId { get; set; }
    public Guid StudentId { get; set; }
}
