using AssignmentManagement.Application.Common.DTOs.Enrollments;
using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Application.Common.Mappings;
using AssignmentManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Application.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IAppDbContext _db;
    public EnrollmentService(IAppDbContext db) { _db = db; }

    public async Task<List<EnrollmentDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Enrollments
            .OrderBy(e => e.EnrolledAt)
            .Select(e => e.ToDto())
            .ToListAsync(ct);
    }

    public async Task<EnrollmentDto> CreateAsync(CreateEnrollmentRequest request, CancellationToken ct = default)
    {
        var classExists = await _db.Classes.AnyAsync(c => c.Id == request.ClassId, ct);
        if (!classExists)
            throw new NotFoundException($"Class with id '{request.ClassId}' was not found.");

        var student = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.StudentId, ct);
        if (student is null || student.Role != UserRole.Student)
            throw new NotFoundException($"Student with id '{request.StudentId}' was not found.");

        var dup = await _db.Enrollments.AnyAsync(
            e => e.ClassId == request.ClassId && e.StudentId == request.StudentId, ct);
        if (dup)
            throw new ConflictException("This student is already enrolled in this class.");

        var now = DateTime.UtcNow;
        var enrollment = new Enrollment
        {
            Id = Guid.NewGuid(),
            ClassId = request.ClassId,
            StudentId = request.StudentId,
            EnrolledAt = now,
            CreatedAt = now
        };
        _db.Enrollments.Add(enrollment);
        await _db.SaveChangesAsync(ct);
        return enrollment.ToDto();
    }
}
