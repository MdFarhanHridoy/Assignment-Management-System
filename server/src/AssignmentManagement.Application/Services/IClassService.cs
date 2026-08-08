using AssignmentManagement.Application.Common.DTOs.Classes;

namespace AssignmentManagement.Application.Services;

public interface IClassService
{
    Task<List<ClassDto>> GetAllAsync(CancellationToken ct = default);
    Task<ClassDto> CreateAsync(CreateClassRequest request, CancellationToken ct = default);
    Task<ClassDto> UpdateAsync(Guid id, UpdateClassRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
