using AssignmentManagement.Application.Common.DTOs.Assignments;
using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Application.Common.Mappings;
using AssignmentManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Application.Services;

public class AssignmentService : IAssignmentService
{
    private readonly IAppDbContext _db;
    public AssignmentService(IAppDbContext db) { _db = db; }

    // ===== Teacher scope =====

    public async Task<List<AssignmentDto>> GetMyAssignmentsAsync(Guid teacherId, CancellationToken ct = default)
    {
        return await _db.Assignments
            .Where(a => a.TeacherId == teacherId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
    }

    public async Task<AssignmentDto> GetMyAssignmentAsync(Guid id, Guid teacherId, CancellationToken ct = default)
    {
        return (await LoadAndCheckOwnershipAsync(id, teacherId, ct)).ToDto();
    }

    public async Task<AssignmentDto> CreateAsync(CreateAssignmentRequest request, Guid teacherId, CancellationToken ct = default)
    {
        var assigned = await _db.TeacherClassSubjects.AnyAsync(
            t => t.TeacherId == teacherId && t.ClassId == request.ClassId && t.SubjectId == request.SubjectId, ct);
        if (!assigned)
            throw new ForbiddenException("You are not assigned to this class and subject.");

        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description,
            DeadlineUtc = DateTime.SpecifyKind(request.DeadlineUtc, DateTimeKind.Utc),
            MaxMarks = request.MaxMarks,
            Status = AssignmentStatus.Draft,
            TeacherId = teacherId,
            ClassId = request.ClassId,
            SubjectId = request.SubjectId,
            AllowResubmission = request.AllowResubmission ?? true,
            CreatedAt = DateTime.UtcNow
        };
        _db.Assignments.Add(assignment);
        await _db.SaveChangesAsync(ct);
        return assignment.ToDto();
    }

    public async Task<AssignmentDto> UpdateAsync(Guid id, UpdateAssignmentRequest request, Guid teacherId, CancellationToken ct = default)
    {
        var assignment = await LoadAndCheckOwnershipAsync(id, teacherId, ct);

        if (request.Title is not null) assignment.Title = request.Title.Trim();
        if (request.Description is not null) assignment.Description = request.Description;
        if (request.DeadlineUtc.HasValue) assignment.DeadlineUtc = DateTime.SpecifyKind(request.DeadlineUtc.Value, DateTimeKind.Utc);
        if (request.MaxMarks.HasValue) assignment.MaxMarks = request.MaxMarks.Value;
        if (request.AllowResubmission.HasValue) assignment.AllowResubmission = request.AllowResubmission.Value;

        if (request.ClassId.HasValue || request.SubjectId.HasValue)
        {
            var classId = request.ClassId ?? assignment.ClassId;
            var subjectId = request.SubjectId ?? assignment.SubjectId;
            var assigned = await _db.TeacherClassSubjects.AnyAsync(
                t => t.TeacherId == teacherId && t.ClassId == classId && t.SubjectId == subjectId, ct);
            if (!assigned)
                throw new ForbiddenException("You are not assigned to this class and subject.");
            assignment.ClassId = classId;
            assignment.SubjectId = subjectId;
        }

        assignment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return assignment.ToDto();
    }

    public async Task DeleteAsync(Guid id, Guid teacherId, CancellationToken ct = default)
    {
        var assignment = await LoadAndCheckOwnershipAsync(id, teacherId, ct);
        _db.Assignments.Remove(assignment);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<AssignmentDto> PublishAsync(Guid id, Guid teacherId, CancellationToken ct = default)
    {
        var assignment = await LoadAndCheckOwnershipAsync(id, teacherId, ct);
        assignment.Status = AssignmentStatus.Published;
        assignment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return assignment.ToDto();
    }

    // ===== Student scope =====

    public async Task<List<AssignmentDto>> GetPublishedForStudentAsync(Guid studentId, CancellationToken ct = default)
    {
        var enrolledClassIds = await _db.Enrollments
            .Where(e => e.StudentId == studentId)
            .Select(e => e.ClassId)
            .ToListAsync(ct);

        return await _db.Assignments
            .Where(a => a.Status == AssignmentStatus.Published && enrolledClassIds.Contains(a.ClassId))
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
    }

    public async Task<AssignmentDto> GetPublishedDetailForStudentAsync(Guid id, Guid studentId, CancellationToken ct = default)
    {
        var assignment = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (assignment is null || assignment.Status != AssignmentStatus.Published)
            throw new NotFoundException($"Assignment with id '{id}' was not found.");

        var enrolled = await _db.Enrollments
            .AnyAsync(e => e.StudentId == studentId && e.ClassId == assignment.ClassId, ct);
        if (!enrolled)
            throw new NotFoundException($"Assignment with id '{id}' was not found.");

        return assignment.ToDto();
    }

    // ===== Private helpers =====

    private async Task<Assignment> LoadAndCheckOwnershipAsync(Guid id, Guid teacherId, CancellationToken ct)
    {
        var assignment = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new NotFoundException($"Assignment with id '{id}' was not found.");

        if (assignment.TeacherId != teacherId)
            throw new ForbiddenException("You do not own this assignment.");

        return assignment;
    }
}
