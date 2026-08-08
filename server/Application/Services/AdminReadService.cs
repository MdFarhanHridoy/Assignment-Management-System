using AssignmentManagement.Application.Common.DTOs.Assignments;
using AssignmentManagement.Application.Common.DTOs.Submissions;
using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Application.Common.Mappings;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Application.Services;

public class AdminReadService : IAdminReadService
{
    private readonly IAppDbContext _db;
    public AdminReadService(IAppDbContext db) { _db = db; }

    public async Task<List<AssignmentSummaryDto>> GetAllAssignmentsAsync(CancellationToken ct = default)
    {
        return await _db.Assignments
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => a.ToSummaryDto())
            .ToListAsync(ct);
    }

    public async Task<List<SubmissionSummaryDto>> GetAllSubmissionsAsync(CancellationToken ct = default)
    {
        return await _db.Submissions
            .OrderByDescending(s => s.SubmittedAtUtc)
            .Select(s => s.ToSummaryDto())
            .ToListAsync(ct);
    }
}
