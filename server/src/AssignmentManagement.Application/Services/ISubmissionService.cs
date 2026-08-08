using AssignmentManagement.Application.Common.DTOs.Submissions;

namespace AssignmentManagement.Application.Services;

public interface ISubmissionService
{
    // Teacher scope
    Task<List<SubmissionDto>> GetSubmissionsForAssignmentAsync(Guid assignmentId, Guid teacherId, CancellationToken ct = default);
    Task<SubmissionDto> ReviewAsync(Guid submissionId, ReviewSubmissionRequest request, Guid teacherId, CancellationToken ct = default);

    // Student scope
    Task<SubmissionDto> SubmitAsync(Guid assignmentId, SubmitRequest request, Guid studentId, CancellationToken ct = default);
    Task<SubmissionDto> UpdateSubmissionAsync(Guid submissionId, UpdateSubmissionRequest request, Guid studentId, CancellationToken ct = default);
    Task<List<SubmissionDto>> GetMySubmissionsAsync(Guid studentId, CancellationToken ct = default);
    Task<SubmissionDto> GetMySubmissionAsync(Guid submissionId, Guid studentId, CancellationToken ct = default);
}
