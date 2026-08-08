namespace AssignmentManagement.Application.Common.DTOs.Assignments;

public class UpdateAssignmentRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? DeadlineUtc { get; set; }
    public int? MaxMarks { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? SubjectId { get; set; }
    public bool? AllowResubmission { get; set; }
}
