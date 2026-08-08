using AssignmentManagement.Application.Common.DTOs.Assignments;

namespace AssignmentManagement.Application.Services;

public interface IAssignmentService
{
    // Teacher scope
    Task<List<AssignmentDto>> GetMyAssignmentsAsync(Guid teacherId, CancellationToken ct = default);
    Task<AssignmentDto> GetMyAssignmentAsync(Guid id, Guid teacherId, CancellationToken ct = default);
    Task<AssignmentDto> CreateAsync(CreateAssignmentRequest request, Guid teacherId, CancellationToken ct = default);
    Task<AssignmentDto> UpdateAsync(Guid id, UpdateAssignmentRequest request, Guid teacherId, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid teacherId, CancellationToken ct = default);
    Task<AssignmentDto> PublishAsync(Guid id, Guid teacherId, CancellationToken ct = default);

    // Student scope
    Task<List<AssignmentDto>> GetPublishedForStudentAsync(Guid studentId, CancellationToken ct = default);
    Task<AssignmentDto> GetPublishedDetailForStudentAsync(Guid id, Guid studentId, CancellationToken ct = default);
}
