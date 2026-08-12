using AssignmentManagement.Application.Common.DTOs.Enrollments;
using AssignmentManagement.Application.Services;
using AssignmentManagement.Domain;
using AssignmentManagement.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace AssignmentManagement.UnitTests.Services;

// TS-CLASS-04 / TS-CLASS-05 : Admin student enrollment
public class EnrollmentServiceTests
{
    [Fact]
    public async Task GetAll_ReturnsSeededEnrollments()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateEnrollmentService(db);

        var result = await svc.GetAllAsync();

        result.Should().HaveCount(1);
        result.Should().Contain(e => e.Id == TestDbHelper.EnrollmentId);
        result[0].StudentId.Should().Be(TestDbHelper.StudentId);
        result[0].ClassId.Should().Be(TestDbHelper.ClassId);
    }

    [Fact]
    public async Task Create_EnrollsStudentInClass()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateEnrollmentService(db);

        var created = await svc.CreateAsync(new CreateEnrollmentRequest
        {
            ClassId = TestDbHelper.ClassId,
            StudentId = TestDbHelper.Student2Id
        });

        var all = await svc.GetAllAsync();

        created.ClassId.Should().Be(TestDbHelper.ClassId);
        created.StudentId.Should().Be(TestDbHelper.Student2Id);
        all.Should().HaveCount(2);
        all.Should().Contain(e => e.Id == created.Id);
    }

    [Fact]
    public async Task Create_DuplicateEnrollment_ThrowsConflict()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateEnrollmentService(db);

        Func<Task> act = () => svc.CreateAsync(new CreateEnrollmentRequest
        {
            ClassId = TestDbHelper.ClassId,
            StudentId = TestDbHelper.StudentId
        });

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Create_NonStudentRole_ThrowsNotFound()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateEnrollmentService(db);

        Func<Task> act = () => svc.CreateAsync(new CreateEnrollmentRequest
        {
            ClassId = TestDbHelper.ClassId,
            StudentId = TestDbHelper.TeacherId
        });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Create_MissingClass_ThrowsNotFound()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateEnrollmentService(db);

        Func<Task> act = () => svc.CreateAsync(new CreateEnrollmentRequest
        {
            ClassId = Guid.NewGuid(),
            StudentId = TestDbHelper.StudentId
        });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_RemovesEnrollment()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateEnrollmentService(db);

        var before = await svc.GetAllAsync();
        before.Should().HaveCount(1);

        await svc.DeleteAsync(TestDbHelper.EnrollmentId);

        var after = await svc.GetAllAsync();
        after.Should().BeEmpty();
    }

    [Fact]
    public async Task Delete_Missing_ThrowsNotFound()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateEnrollmentService(db);

        Func<Task> act = () => svc.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
