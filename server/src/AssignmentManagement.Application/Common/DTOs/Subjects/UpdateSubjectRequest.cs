namespace AssignmentManagement.Application.Common.DTOs.Subjects;

public class UpdateSubjectRequest
{
    public string? Name { get; set; }
    public Guid? ClassId { get; set; }
}
