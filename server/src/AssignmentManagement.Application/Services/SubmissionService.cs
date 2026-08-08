using AssignmentManagement.Application.Common.DTOs.Submissions;
using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Application.Common.Mappings;
using AssignmentManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Application.Services;

public class SubmissionService : ISubmissionService
{
    private readonly IAppDbContext _db;
    public SubmissionService(IAppDbContext db) { _db = db; }

    // ===== Teacher scope =====

    public async Task<List<SubmissionDto>> GetSubmissionsForAssignmentAsync(Guid assignmentId, Guid teacherId, CancellationToken ct = default)
    {
        var assignment = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == assignmentId, ct)
            ?? throw new NotFoundException($"Assignment with id '{assignmentId}' was not found.");

        if (assignment.TeacherId != teacherId)
            throw new ForbiddenException("You do not own this assignment.");

        return await _db.Submissions
            .Where(s => s.AssignmentId == assignmentId)
            .OrderByDescending(s => s.SubmittedAtUtc)
            .Select(s => s.ToDto())
            .ToListAsync(ct);
    }

    public async Task<SubmissionDto> ReviewAsync(Guid submissionId, ReviewSubmissionRequest request, Guid teacherId, CancellationToken ct = default)
    {
        var submission = await _db.Submissions.FirstOrDefaultAsync(s => s.Id == submissionId, ct)
            ?? throw new NotFoundException($"Submission with id '{submissionId}' was not found.");

        var assignment = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == submission.AssignmentId, ct)
            ?? throw new NotFoundException("The assignment for this submission was not found.");

        if (assignment.TeacherId != teacherId)
            throw new ForbiddenException("You do not own the assignment for this submission.");

        if (request.Marks < 0 || request.Marks > assignment.MaxMarks)
            throw new DomainException($"Marks must be between 0 and {assignment.MaxMarks}.");

        var now = DateTime.UtcNow;
        submission.Marks = request.Marks;
        submission.Feedback = request.Feedback;
        submission.ReviewedByTeacherId = teacherId;
        submission.ReviewedAtUtc = now;
        submission.Status = request.Status ?? SubmissionStatus.Reviewed;

        await _db.SaveChangesAsync(ct);
        return submission.ToDto();
    }

    // ===== Student scope =====

    public async Task<SubmissionDto> SubmitAsync(Guid assignmentId, SubmitRequest request, Guid studentId, CancellationToken ct = default)
    {
        var assignment = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == assignmentId, ct)
            ?? throw new NotFoundException($"Assignment with id '{assignmentId}' was not found.");

        if (assignment.Status != AssignmentStatus.Published)
            throw new NotFoundException($"Assignment with id '{assignmentId}' was not found.");

        var enrolled = await _db.Enrollments
            .AnyAsync(e => e.StudentId == studentId && e.ClassId == assignment.ClassId, ct);
        if (!enrolled)
            throw new ForbiddenException("You are not enrolled in this class.");

        if (DateTime.UtcNow > assignment.DeadlineUtc)
            throw new DomainException("The assignment deadline has passed.");

        var existing = await _db.Submissions
            .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId, ct);

        var now = DateTime.UtcNow;

        if (existing is not null)
        {
            if (!assignment.AllowResubmission)
                throw new ConflictException("You have already submitted and resubmission is not allowed.");

            existing.AnswerText = request.AnswerText;
            existing.UpdatedAtUtc = now;
            existing.Status = SubmissionStatus.Submitted;
            await _db.SaveChangesAsync(ct);
            return existing.ToDto();
        }

        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            AssignmentId = assignmentId,
            StudentId = studentId,
            AnswerText = request.AnswerText,
            SubmittedAtUtc = now,
            Status = SubmissionStatus.Submitted
        };
        _db.Submissions.Add(submission);
        await _db.SaveChangesAsync(ct);
        return submission.ToDto();
    }

    public async Task<SubmissionDto> UpdateSubmissionAsync(Guid submissionId, UpdateSubmissionRequest request, Guid studentId, CancellationToken ct = default)
    {
        var submission = await _db.Submissions.FirstOrDefaultAsync(s => s.Id == submissionId, ct)
            ?? throw new NotFoundException($"Submission with id '{submissionId}' was not found.");

        if (submission.StudentId != studentId)
            throw new ForbiddenException("You do not own this submission.");

        var assignment = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == submission.AssignmentId, ct);
        if (assignment is not null)
        {
            if (DateTime.UtcNow > assignment.DeadlineUtc)
                throw new DomainException("The assignment deadline has passed.");

            if (!assignment.AllowResubmission)
                throw new DomainException("Resubmission is not allowed for this assignment.");
        }

        submission.AnswerText = request.AnswerText;
        submission.UpdatedAtUtc = DateTime.UtcNow;
        submission.Status = SubmissionStatus.Submitted;
        await _db.SaveChangesAsync(ct);
        return submission.ToDto();
    }

    public async Task<List<SubmissionDto>> GetMySubmissionsAsync(Guid studentId, CancellationToken ct = default)
    {
        return await _db.Submissions
            .Where(s => s.StudentId == studentId)
            .OrderByDescending(s => s.SubmittedAtUtc)
            .Select(s => s.ToDto())
            .ToListAsync(ct);
    }

    public async Task<SubmissionDto> GetMySubmissionAsync(Guid submissionId, Guid studentId, CancellationToken ct = default)
    {
        var submission = await _db.Submissions.FirstOrDefaultAsync(s => s.Id == submissionId, ct)
            ?? throw new NotFoundException($"Submission with id '{submissionId}' was not found.");

        if (submission.StudentId != studentId)
            throw new ForbiddenException("You do not own this submission.");

        return submission.ToDto();
    }
}
