using AssignmentManagement.Application.Common.DTOs.Subjects;
using AssignmentManagement.Application.Services;
using AssignmentManagement.Domain;
using AssignmentManagement.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace AssignmentManagement.UnitTests.Services;

// TS-CLASS-01 (cont.) : Admin Subject CRUD
public class SubjectServiceTests
{
    [Fact]
    public async Task GetAll_ReturnsSeededSubjects()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateSubjectService(db);

        var result = await svc.GetAllAsync();

        result.Should().HaveCount(2);
        result.Should().Contain(s => s.Id == TestDbHelper.SubjectId);
        result.Should().Contain(s => s.Id == TestDbHelper.Subject2Id);
    }

    [Fact]
    public async Task Create_AddsSubjectToClass()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateSubjectService(db);

        var created = await svc.CreateAsync(new CreateSubjectRequest
        {
            Name = "Physics",
            ClassId = TestDbHelper.ClassId
        });

        var all = await svc.GetAllAsync();

        created.Name.Should().Be("Physics");
        created.ClassId.Should().Be(TestDbHelper.ClassId);
        all.Should().HaveCount(3);
        all.Should().Contain(s => s.Id == created.Id && s.Name == "Physics");
    }

    [Fact]
    public async Task Create_BadClassId_ThrowsNotFound()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateSubjectService(db);

        Func<Task> act = () => svc.CreateAsync(new CreateSubjectRequest
        {
            Name = "Physics",
            ClassId = Guid.NewGuid()
        });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Create_DuplicateNameInSameClass_ThrowsConflict()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateSubjectService(db);

        Func<Task> act = () => svc.CreateAsync(new CreateSubjectRequest
        {
            Name = "Math",
            ClassId = TestDbHelper.ClassId
        });

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Update_ChangesSubjectName()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateSubjectService(db);

        var updated = await svc.UpdateAsync(
            TestDbHelper.SubjectId,
            new UpdateSubjectRequest { Name = "Renamed" });

        var all = await svc.GetAllAsync();

        updated.Name.Should().Be("Renamed");
        all.Should().Contain(s => s.Id == TestDbHelper.SubjectId && s.Name == "Renamed");
    }

    [Fact]
    public async Task Delete_RemovesSubject()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateSubjectService(db);

        var before = await svc.GetAllAsync();
        before.Should().HaveCount(2);

        await svc.DeleteAsync(TestDbHelper.Subject2Id);

        var after = await svc.GetAllAsync();
        after.Should().HaveCount(1);
        after.Should().NotContain(s => s.Id == TestDbHelper.Subject2Id);
    }
}
