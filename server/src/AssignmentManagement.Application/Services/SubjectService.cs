using AssignmentManagement.Application.Common.DTOs.Subjects;
using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Application.Common.Mappings;
using AssignmentManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Application.Services;

public class SubjectService : ISubjectService
{
    private readonly IAppDbContext _db;

    public SubjectService(IAppDbContext db) { _db = db; }

    public async Task<List<SubjectDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Subjects.OrderBy(s => s.Name).Select(s => s.ToDto()).ToListAsync(ct);
    }

    public async Task<SubjectDto> CreateAsync(CreateSubjectRequest request, CancellationToken ct = default)
    {
        var classExists = await _db.Classes.AnyAsync(c => c.Id == request.ClassId, ct);
        if (!classExists)
            throw new NotFoundException($"Class with id '{request.ClassId}' was not found.");

        var name = request.Name.Trim();
        var dup = await _db.Subjects.AnyAsync(s => s.ClassId == request.ClassId && s.Name == name, ct);
        if (dup)
            throw new ConflictException($"A subject named '{name}' already exists in this class.");

        var subject = new Subject
        {
            Id = Guid.NewGuid(),
            Name = name,
            ClassId = request.ClassId,
            CreatedAt = DateTime.UtcNow
        };
        _db.Subjects.Add(subject);
        await _db.SaveChangesAsync(ct);
        return subject.ToDto();
    }

    public async Task<SubjectDto> UpdateAsync(Guid id, UpdateSubjectRequest request, CancellationToken ct = default)
    {
        var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException($"Subject with id '{id}' was not found.");

        if (request.Name is not null)
        {
            var name = request.Name.Trim();
            var dup = await _db.Subjects.AnyAsync(s => s.ClassId == subject.ClassId && s.Name == name && s.Id != id, ct);
            if (dup)
                throw new ConflictException($"A subject named '{name}' already exists in this class.");
            subject.Name = name;
        }
        if (request.ClassId is not null)
        {
            var classExists = await _db.Classes.AnyAsync(c => c.Id == request.ClassId.Value, ct);
            if (!classExists)
                throw new NotFoundException($"Class with id '{request.ClassId}' was not found.");
            subject.ClassId = request.ClassId.Value;
        }
        subject.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return subject.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException($"Subject with id '{id}' was not found.");
        _db.Subjects.Remove(subject);
        await _db.SaveChangesAsync(ct);
    }
}
