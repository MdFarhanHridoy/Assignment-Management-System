using AssignmentManagement.Application.Common.DTOs.TeacherAssignments;
using AssignmentManagement.Application.Services;
using AssignmentManagement.Domain;
using AssignmentManagement.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace AssignmentManagement.UnitTests.Services;

// TS-CLASS-02 / TS-CLASS-03 : Admin teacher-class-subject assignment
public class TeacherAssignmentServiceTests
{
    [Fact]
    public async Task GetAll_ReturnsSeededAssignments()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateTeacherAssignmentService(db);

        var result = await svc.GetAllAsync();

        result.Should().HaveCount(1);
        result.Should().Contain(t => t.Id == TestDbHelper.TeacherClassSubjectId);
        result[0].TeacherId.Should().Be(TestDbHelper.TeacherId);
        result[0].ClassId.Should().Be(TestDbHelper.ClassId);
        result[0].SubjectId.Should().Be(TestDbHelper.SubjectId);
    }

    [Fact]
    public async Task Create_AssignsTeacherToClassSubject()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateTeacherAssignmentService(db);

        // Subject2 (Science) is in Class1 (= ClassId) -> new Teacher/Class/Subject combo.
        var created = await svc.CreateAsync(new CreateTeacherAssignmentRequest
        {
            TeacherId = TestDbHelper.TeacherId,
            ClassId = TestDbHelper.ClassId,
            SubjectId = TestDbHelper.Subject2Id
        });

        var all = await svc.GetAllAsync();

        created.TeacherId.Should().Be(TestDbHelper.TeacherId);
        created.ClassId.Should().Be(TestDbHelper.ClassId);
        created.SubjectId.Should().Be(TestDbHelper.Subject2Id);
        all.Should().HaveCount(2);
        all.Should().Contain(t => t.Id == created.Id);
    }

    [Fact]
    public async Task Create_DuplicateAssignment_ThrowsConflict()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateTeacherAssignmentService(db);

        Func<Task> act = () => svc.CreateAsync(new CreateTeacherAssignmentRequest
        {
            TeacherId = TestDbHelper.TeacherId,
            ClassId = TestDbHelper.ClassId,
            SubjectId = TestDbHelper.SubjectId
        });

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Create_NonTeacherRole_ThrowsNotFound()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateTeacherAssignmentService(db);

        Func<Task> act = () => svc.CreateAsync(new CreateTeacherAssignmentRequest
        {
            TeacherId = TestDbHelper.StudentId,
            ClassId = TestDbHelper.ClassId,
            SubjectId = TestDbHelper.SubjectId
        });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Create_MissingClass_ThrowsNotFound()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateTeacherAssignmentService(db);

        Func<Task> act = () => svc.CreateAsync(new CreateTeacherAssignmentRequest
        {
            TeacherId = TestDbHelper.TeacherId,
            ClassId = Guid.NewGuid(),
            SubjectId = TestDbHelper.SubjectId
        });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_RemovesAssignment()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateTeacherAssignmentService(db);

        var before = await svc.GetAllAsync();
        before.Should().HaveCount(1);

        await svc.DeleteAsync(TestDbHelper.TeacherClassSubjectId);

        var after = await svc.GetAllAsync();
        after.Should().BeEmpty();
    }

    [Fact]
    public async Task Delete_Missing_ThrowsNotFound()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateTeacherAssignmentService(db);

        Func<Task> act = () => svc.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
