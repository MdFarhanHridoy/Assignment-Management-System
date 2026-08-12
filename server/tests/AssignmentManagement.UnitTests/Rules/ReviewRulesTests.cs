using AssignmentManagement.Application.Common.DTOs.Submissions;
using AssignmentManagement.Application.Services;
using AssignmentManagement.Domain;
using AssignmentManagement.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace AssignmentManagement.UnitTests.Rules;

public class ReviewRulesTests
{
    private static ReviewSubmissionRequest Review(int marks, string? feedback = null, SubmissionStatus? status = null) =>
        new()
        {
            Marks = marks,
            Feedback = feedback,
            Status = status
        };

    [Fact]
    public async Task TS_REV_01_TeacherReviewsOwnAssignmentSubmission()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateSubmissionService(db);

        var result = await service.ReviewAsync(
            TestDbHelper.SubmissionId,
            Review(80, "Good"),
            TestDbHelper.TeacherId);

        result.Marks.Should().Be(80);
        result.Feedback.Should().Be("Good");
        result.ReviewedByTeacherId.Should().Be(TestDbHelper.TeacherId);
        result.ReviewedAtUtc.Should().NotBeNull();
        result.Status.Should().Be(SubmissionStatus.Reviewed);
    }

    [Fact]
    public async Task TS_REV_02_NegativeMarks_ThrowsDomainException()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateSubmissionService(db);

        Func<Task> act = () => service.ReviewAsync(
            TestDbHelper.SubmissionId,
            Review(-1),
            TestDbHelper.TeacherId);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task TS_REV_03_MarksExceedingMax_ThrowsDomainException()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateSubmissionService(db);

        Func<Task> act = () => service.ReviewAsync(
            TestDbHelper.SubmissionId,
            Review(101),
            TestDbHelper.TeacherId);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task TS_REV_03_MarksAtBoundary_Succeeds()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateSubmissionService(db);

        var result = await service.ReviewAsync(
            TestDbHelper.SubmissionId,
            Review(100),
            TestDbHelper.TeacherId);

        result.Marks.Should().Be(100);
        result.Status.Should().Be(SubmissionStatus.Reviewed);
    }

    [Fact]
    public async Task TS_REV_04_OtherTeacherCannotReview()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateSubmissionService(db);

        Func<Task> act = () => service.ReviewAsync(
            TestDbHelper.SubmissionId,
            Review(80),
            TestDbHelper.Teacher2Id);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task TS_REV_05_FeedbackOptional()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateSubmissionService(db);

        var result = await service.ReviewAsync(
            TestDbHelper.SubmissionId,
            Review(70),
            TestDbHelper.TeacherId);

        result.Marks.Should().Be(70);
        result.Feedback.Should().BeNull();
        result.Status.Should().Be(SubmissionStatus.Reviewed);
    }

    [Fact]
    public async Task TS_REV_06_StatusTransitionsToReviewed()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateSubmissionService(db);

        var result = await service.ReviewAsync(
            TestDbHelper.SubmissionId,
            Review(85, status: SubmissionStatus.Reviewed),
            TestDbHelper.TeacherId);

        result.Status.Should().Be(SubmissionStatus.Reviewed);
        result.Marks.Should().Be(85);
    }
}
