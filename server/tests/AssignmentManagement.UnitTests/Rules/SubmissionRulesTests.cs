using AssignmentManagement.Application.Common.DTOs.Submissions;
using AssignmentManagement.Application.Services;
using AssignmentManagement.Domain;
using AssignmentManagement.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace AssignmentManagement.UnitTests.Rules;

public class SubmissionRulesTests
{
    [Fact]
    public async Task TS_SUB_01_StudentSubmitsBeforeDeadline_Succeeds()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateSubmissionService(db);

        var result = await service.SubmitAsync(
            TestDbHelper.PublishedAssignmentId,
            new SubmitRequest { AnswerText = "My answer" },
            TestDbHelper.StudentId);

        result.Status.Should().Be(SubmissionStatus.Submitted);
        result.AnswerText.Should().Be("My answer");
        result.StudentId.Should().Be(TestDbHelper.StudentId);
    }

    [Fact]
    public async Task TS_SUB_02_StudentCannotSubmitAfterDeadline()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateSubmissionService(db);

        Func<Task> act = () => service.SubmitAsync(
            TestDbHelper.PastDeadlineAssignmentId,
            new SubmitRequest { AnswerText = "Late attempt" },
            TestDbHelper.StudentId);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task TS_SUB_03_StudentUpdatesBeforeDeadline_Succeeds()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateSubmissionService(db);

        var result = await service.UpdateSubmissionAsync(
            TestDbHelper.SubmissionId,
            new UpdateSubmissionRequest { AnswerText = "Updated answer" },
            TestDbHelper.StudentId);

        result.AnswerText.Should().Be("Updated answer");
        result.Status.Should().Be(SubmissionStatus.Submitted);
    }

    [Fact]
    public async Task TS_SUB_04_StudentCannotUpdateAfterDeadline()
    {
        var db = TestDbHelper.CreateEmptyDb();
        var assignmentId = Guid.NewGuid();
        db.Assignments.Add(new Assignment
        {
            Id = assignmentId,
            Title = "Past",
            Description = "Deadline already passed",
            DeadlineUtc = DateTime.UtcNow.AddDays(-1),
            MaxMarks = 100,
            Status = AssignmentStatus.Published,
            TeacherId = TestDbHelper.TeacherId,
            ClassId = TestDbHelper.ClassId,
            SubjectId = TestDbHelper.SubjectId,
            AllowResubmission = true
        });
        var submissionId = Guid.NewGuid();
        db.Submissions.Add(new Submission
        {
            Id = submissionId,
            AssignmentId = assignmentId,
            StudentId = TestDbHelper.StudentId,
            AnswerText = "Original answer",
            SubmittedAtUtc = DateTime.UtcNow.AddDays(-2),
            Status = SubmissionStatus.Submitted
        });
        await db.SaveChangesAsync();

        var service = TestDbHelper.CreateSubmissionService(db);

        Func<Task> act = () => service.UpdateSubmissionAsync(
            submissionId,
            new UpdateSubmissionRequest { AnswerText = "Try to update after deadline" },
            TestDbHelper.StudentId);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task TS_SUB_05_StudentCannotSubmitToDraft()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateSubmissionService(db);

        Func<Task> act = () => service.SubmitAsync(
            TestDbHelper.DraftAssignmentId,
            new SubmitRequest { AnswerText = "Should not work" },
            TestDbHelper.StudentId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task TS_SUB_06_StudentCannotViewOthersSubmission()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateSubmissionService(db);

        Func<Task> act = () => service.GetMySubmissionAsync(TestDbHelper.SubmissionId, TestDbHelper.Student2Id);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task TS_SUB_08_DuplicateSubmissionWithResubmission_Upserts()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateSubmissionService(db);

        var result = await service.SubmitAsync(
            TestDbHelper.PublishedAssignmentId,
            new SubmitRequest { AnswerText = "Second attempt" },
            TestDbHelper.StudentId);

        result.Id.Should().Be(TestDbHelper.SubmissionId);
        result.AnswerText.Should().Be("Second attempt");
        result.Status.Should().Be(SubmissionStatus.Submitted);
    }

    [Fact]
    public async Task TS_SUB_09_NotEnrolledStudentCannotSeeAssignment()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var assignmentService = TestDbHelper.CreateAssignmentService(db);

        Func<Task> act = () => assignmentService.GetPublishedDetailForStudentAsync(
            TestDbHelper.PublishedAssignmentId, TestDbHelper.Student2Id);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task TS_SUB_09_NotEnrolledStudentCannotSubmit()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateSubmissionService(db);

        Func<Task> act = () => service.SubmitAsync(
            TestDbHelper.PublishedAssignmentId,
            new SubmitRequest { AnswerText = "Not enrolled" },
            TestDbHelper.Student2Id);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task TS_SUB_10_UpdateBlockedWhenResubmissionFalse()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var submissionId = Guid.NewGuid();
        db.Submissions.Add(new Submission
        {
            Id = submissionId,
            AssignmentId = TestDbHelper.NoResubmissionAssignmentId,
            StudentId = TestDbHelper.StudentId,
            AnswerText = "First and only",
            SubmittedAtUtc = DateTime.UtcNow,
            Status = SubmissionStatus.Submitted
        });
        await db.SaveChangesAsync();

        var service = TestDbHelper.CreateSubmissionService(db);

        Func<Task> act = () => service.UpdateSubmissionAsync(
            submissionId,
            new UpdateSubmissionRequest { AnswerText = "Try to update" },
            TestDbHelper.StudentId);

        await act.Should().ThrowAsync<DomainException>();
    }
}
