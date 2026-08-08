using AssignmentManagement.Application.Common.DTOs.TeacherAssignments;
using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Application.Common.Mappings;
using AssignmentManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Application.Services;

public class TeacherAssignmentService : ITeacherAssignmentService
{
    private readonly IAppDbContext _db;
    public TeacherAssignmentService(IAppDbContext db) { _db = db; }

    public async Task<List<TeacherAssignmentDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.TeacherClassSubjects
            .OrderBy(t => t.CreatedAt)
            .Select(t => t.ToDto())
            .ToListAsync(ct);
    }

    public async Task<TeacherAssignmentDto> CreateAsync(CreateTeacherAssignmentRequest request, CancellationToken ct = default)
    {
        var teacher = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.TeacherId, ct);
        if (teacher is null || teacher.Role != UserRole.Teacher)
            throw new NotFoundException($"Teacher with id '{request.TeacherId}' was not found.");

        var classExists = await _db.Classes.AnyAsync(c => c.Id == request.ClassId, ct);
        if (!classExists)
            throw new NotFoundException($"Class with id '{request.ClassId}' was not found.");

        var subjectExists = await _db.Subjects.AnyAsync(s => s.Id == request.SubjectId, ct);
        if (!subjectExists)
            throw new NotFoundException($"Subject with id '{request.SubjectId}' was not found.");

        var dup = await _db.TeacherClassSubjects.AnyAsync(
            t => t.TeacherId == request.TeacherId && t.ClassId == request.ClassId && t.SubjectId == request.SubjectId, ct);
        if (dup)
            throw new ConflictException("This teacher is already assigned to this class and subject.");

        var tcs = new TeacherClassSubject
        {
            Id = Guid.NewGuid(),
            TeacherId = request.TeacherId,
            ClassId = request.ClassId,
            SubjectId = request.SubjectId,
            CreatedAt = DateTime.UtcNow
        };
        _db.TeacherClassSubjects.Add(tcs);
        await _db.SaveChangesAsync(ct);
        return tcs.ToDto();
    }
}
