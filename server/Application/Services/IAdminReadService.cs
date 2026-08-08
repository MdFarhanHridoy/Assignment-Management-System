using AssignmentManagement.Application.Common.DTOs.Assignments;
using AssignmentManagement.Application.Common.DTOs.Submissions;

namespace AssignmentManagement.Application.Services;

public interface IAdminReadService
{
    Task<List<AssignmentSummaryDto>> GetAllAssignmentsAsync(CancellationToken ct = default);
    Task<List<SubmissionSummaryDto>> GetAllSubmissionsAsync(CancellationToken ct = default);
}
