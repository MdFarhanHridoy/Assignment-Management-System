using AssignmentManagement.Domain;

namespace AssignmentManagement.Application.Common.DTOs.Submissions;

public class ReviewSubmissionRequest
{
    public int Marks { get; set; }
    public string? Feedback { get; set; }
    public SubmissionStatus? Status { get; set; }
}
