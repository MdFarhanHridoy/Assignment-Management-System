using AssignmentManagement.Application.Common.DTOs.Assignments;
using AssignmentManagement.Application.Services;
using AssignmentManagement.Domain;
using AssignmentManagement.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace AssignmentManagement.UnitTests.Rules;

public class AssignmentRulesTests
{
    private static CreateAssignmentRequest ValidCreateRequest() => new()
    {
        Title = "T",
        Description = "D",
        DeadlineUtc = DateTime.UtcNow.AddDays(7),
        MaxMarks = 100,
        ClassId = TestDbHelper.ClassId,
        SubjectId = TestDbHelper.SubjectId
    };

    [Fact]
    public async Task TS_ASGN_01_CreateForUnassignedClassSubject_ThrowsForbidden()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateAssignmentService(db);
        var request = ValidCreateRequest();
        request.ClassId = TestDbHelper.Class2Id;
        request.SubjectId = TestDbHelper.Subject2Id;

        Func<Task> act = () => service.CreateAsync(request, TestDbHelper.TeacherId);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task TS_ASGN_01_CreateForAssignedClassSubject_Succeeds()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateAssignmentService(db);
        var request = ValidCreateRequest();

        var result = await service.CreateAsync(request, TestDbHelper.TeacherId);

        result.Id.Should().NotBeEmpty();
        result.Status.Should().Be(AssignmentStatus.Draft);
        result.Title.Should().Be("T");
        result.MaxMarks.Should().Be(100);
        result.TeacherId.Should().Be(TestDbHelper.TeacherId);
    }

    [Fact]
    public async Task TS_ASGN_02_DraftInvisibleToStudent()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateAssignmentService(db);

        var result = await service.GetPublishedForStudentAsync(TestDbHelper.StudentId);

        result.Should().NotContain(a => a.Id == TestDbHelper.DraftAssignmentId);
    }

    [Fact]
    public async Task TS_ASGN_02_DraftDetailReturns404()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateAssignmentService(db);

        Func<Task> act = () => service.GetPublishedDetailForStudentAsync(
            TestDbHelper.DraftAssignmentId, TestDbHelper.StudentId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task TS_ASGN_03_PublishedVisibleToEnrolledStudent()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateAssignmentService(db);

        var result = await service.GetPublishedForStudentAsync(TestDbHelper.StudentId);

        result.Should().Contain(a => a.Id == TestDbHelper.PublishedAssignmentId);
    }

    [Fact]
    public async Task TS_ASGN_03_PublishedInvisibleToUnenrolledStudent()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateAssignmentService(db);

        var result = await service.GetPublishedForStudentAsync(TestDbHelper.Student2Id);

        result.Should().NotContain(a => a.Id == TestDbHelper.PublishedAssignmentId);
    }

    [Fact]
    public async Task TS_ASGN_04_MaxMarksZero_ThrowsDomainException()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateAssignmentService(db);
        var request = ValidCreateRequest();
        request.MaxMarks = 0;

        Func<Task> act = () => service.CreateAsync(request, TestDbHelper.TeacherId);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task TS_ASGN_04_MaxMarksPositive_Succeeds()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateAssignmentService(db);
        var request = ValidCreateRequest();
        request.MaxMarks = 100;

        var result = await service.CreateAsync(request, TestDbHelper.TeacherId);

        result.MaxMarks.Should().Be(100);
        result.Status.Should().Be(AssignmentStatus.Draft);
    }

    [Fact]
    public async Task TS_ASGN_05_TeacherUpdatesOwnAssignment()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateAssignmentService(db);

        var result = await service.UpdateAsync(
            TestDbHelper.DraftAssignmentId,
            new UpdateAssignmentRequest { Title = "Updated" },
            TestDbHelper.TeacherId);

        result.Title.Should().Be("Updated");
        result.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task TS_ASGN_06_OtherTeacherCannotUpdate()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateAssignmentService(db);

        Func<Task> act = () => service.UpdateAsync(
            TestDbHelper.DraftAssignmentId,
            new UpdateAssignmentRequest { Title = "Hijacked" },
            TestDbHelper.Teacher2Id);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task TS_ASGN_06_OtherTeacherCannotDelete()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateAssignmentService(db);

        Func<Task> act = () => service.DeleteAsync(TestDbHelper.DraftAssignmentId, TestDbHelper.Teacher2Id);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task TS_ASGN_08_DeadlineStoredInUtc()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateAssignmentService(db);
        var deadline = DateTime.UtcNow.AddDays(14);
        var request = ValidCreateRequest();
        request.DeadlineUtc = deadline;

        var created = await service.CreateAsync(request, TestDbHelper.TeacherId);
        var readBack = await service.GetMyAssignmentAsync(created.Id, TestDbHelper.TeacherId);

        readBack.DeadlineUtc.Should().Be(deadline);
        readBack.DeadlineUtc.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task TS_ASGN_09_PublishDraftTransitionsToPublished()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var service = TestDbHelper.CreateAssignmentService(db);

        var result = await service.PublishAsync(TestDbHelper.DraftAssignmentId, TestDbHelper.TeacherId);

        result.Status.Should().Be(AssignmentStatus.Published);
        result.UpdatedAt.Should().NotBeNull();
    }
}
