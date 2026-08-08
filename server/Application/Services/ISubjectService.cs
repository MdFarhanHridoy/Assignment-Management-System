using AssignmentManagement.Application.Common.DTOs.Subjects;

namespace AssignmentManagement.Application.Services;

public interface ISubjectService
{
    Task<List<SubjectDto>> GetAllAsync(CancellationToken ct = default);
    Task<SubjectDto> CreateAsync(CreateSubjectRequest request, CancellationToken ct = default);
    Task<SubjectDto> UpdateAsync(Guid id, UpdateSubjectRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
