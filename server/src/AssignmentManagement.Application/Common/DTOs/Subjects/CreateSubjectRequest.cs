namespace AssignmentManagement.Application.Common.DTOs.Subjects;

public class CreateSubjectRequest
{
    public string Name { get; set; } = null!;
    public Guid ClassId { get; set; }
}
