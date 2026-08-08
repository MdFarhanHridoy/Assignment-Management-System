using AssignmentManagement.Application.Common.DTOs.Classes;
using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Application.Common.Mappings;
using AssignmentManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Application.Services;

public class ClassService : IClassService
{
    private readonly IAppDbContext _db;

    public ClassService(IAppDbContext db) { _db = db; }

    public async Task<List<ClassDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Classes.OrderBy(c => c.Name).Select(c => c.ToDto()).ToListAsync(ct);
    }

    public async Task<ClassDto> CreateAsync(CreateClassRequest request, CancellationToken ct = default)
    {
        var cls = new Class
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };
        _db.Classes.Add(cls);
        await _db.SaveChangesAsync(ct);
        return cls.ToDto();
    }

    public async Task<ClassDto> UpdateAsync(Guid id, UpdateClassRequest request, CancellationToken ct = default)
    {
        var cls = await _db.Classes.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException($"Class with id '{id}' was not found.");
        if (request.Name is not null) cls.Name = request.Name.Trim();
        if (request.Description is not null) cls.Description = request.Description;
        cls.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return cls.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var cls = await _db.Classes.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException($"Class with id '{id}' was not found.");
        _db.Classes.Remove(cls);
        await _db.SaveChangesAsync(ct);
    }
}
