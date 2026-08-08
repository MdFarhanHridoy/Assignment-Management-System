namespace AssignmentManagement.Application.Common.DTOs.Subjects;

public class SubjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public Guid ClassId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
