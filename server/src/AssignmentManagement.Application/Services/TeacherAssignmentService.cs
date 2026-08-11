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
    {        var teacher = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.TeacherId, ct);
        if (teacher is null || teacher.Role != UserRole.Teacher)
            throw new NotFoundException($"Teacher with id '{request.TeacherId}' was not found.");

        var classExists = await _db.Classes.AnyAsync(c => c.Id == request.ClassId, ct);
        if (!classExists)
            throw new NotFoundException($"Class with id '{request.ClassId}' was not found.");

        var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == request.SubjectId, ct)
            ?? throw new NotFoundException($"Subject with id '{request.SubjectId}' was not found.");

        if (subject.ClassId != request.ClassId)
            throw new DomainException("The selected subject does not belong to the selected class.");

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

    public async Task<List<TeacherAssignmentViewDto>> GetMineAsync(Guid teacherId, CancellationToken ct = default)
    {
        return await (
            from t in _db.TeacherClassSubjects
            where t.TeacherId == teacherId
            join c in _db.Classes on t.ClassId equals c.Id
            join s in _db.Subjects on t.SubjectId equals s.Id
            orderby c.Name, s.Name
            select new TeacherAssignmentViewDto
            {
                Id = t.Id,
                ClassId = c.Id,
                ClassName = c.Name,
                SubjectId = s.Id,
                SubjectName = s.Name
            }
        ).ToListAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.TeacherClassSubjects.FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new NotFoundException($"Teacher assignment with id '{id}' was not found.");

        _db.TeacherClassSubjects.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }
}
