using AssignmentManagement.Application.Common.DTOs.TeacherAssignments;

namespace AssignmentManagement.Application.Services;

public interface ITeacherAssignmentService
{
    Task<List<TeacherAssignmentDto>> GetAllAsync(CancellationToken ct = default);
    Task<TeacherAssignmentDto> CreateAsync(CreateTeacherAssignmentRequest request, CancellationToken ct = default);
    Task<List<TeacherAssignmentViewDto>> GetMineAsync(Guid teacherId, CancellationToken ct = default);
}
