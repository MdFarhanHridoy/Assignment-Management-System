using AssignmentManagement.Application.Common.DTOs.Users;
using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Application.Common.Mappings;
using AssignmentManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Application.Services;

public class UserService : IUserService
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IAppDbContext db, IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<UserDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Users.OrderBy(u => u.Name).Select(u => u.ToDto()).ToListAsync(ct);
    }

    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NotFoundException($"User with id '{id}' was not found.");
        return user.ToDto();
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email, ct))
            throw new ConflictException($"A user with email '{email}' already exists.");

        var now = DateTime.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = request.Role,
            IsActive = true,
            CreatedAt = now
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return user.ToDto();
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NotFoundException($"User with id '{id}' was not found.");

        if (request.Name is not null) user.Name = request.Name.Trim();
        if (request.Role is not null) user.Role = request.Role.Value;
        if (request.IsActive is not null) user.IsActive = request.IsActive.Value;

        if (request.Email is not null)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            if (await _db.Users.AnyAsync(u => u.Email == email && u.Id != id, ct))
                throw new ConflictException($"A user with email '{email}' already exists.");
            user.Email = email;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return user.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NotFoundException($"User with id '{id}' was not found.");

        // Restrict FKs to Users.Id — block with a clear error instead of letting
        // Postgres raise an FK violation that surfaces as an opaque 500.
        // (Submissions.ReviewedByTeacherId is SetNull and intentionally not checked.)
        var blockers = new List<string>();
        if (await _db.Enrollments.AnyAsync(e => e.StudentId == id, ct)) blockers.Add("enrollment(s)");
        if (await _db.Submissions.AnyAsync(s => s.StudentId == id, ct)) blockers.Add("submission(s)");
        if (await _db.Assignments.AnyAsync(a => a.TeacherId == id, ct)) blockers.Add("assignment(s)");
        if (await _db.TeacherClassSubjects.AnyAsync(t => t.TeacherId == id, ct)) blockers.Add("class/subject assignment(s)");

        if (blockers.Count > 0)
        {
            throw new ConflictException(
                $"Cannot delete this user because they still have related data " +
                $"({string.Join(", ", blockers)}). Remove those first, or disable " +
                $"the account instead via Edit > Active account.");
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync(ct);
    }
}
