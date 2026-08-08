using AssignmentManagement.Application.Common.DTOs.Enrollments;

namespace AssignmentManagement.Application.Services;

public interface IEnrollmentService
{
    Task<List<EnrollmentDto>> GetAllAsync(CancellationToken ct = default);
    Task<EnrollmentDto> CreateAsync(CreateEnrollmentRequest request, CancellationToken ct = default);
}
