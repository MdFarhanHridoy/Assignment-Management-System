using AssignmentManagement.Application.Common.DTOs.Classes;
using AssignmentManagement.Application.Services;
using AssignmentManagement.Domain;
using AssignmentManagement.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace AssignmentManagement.UnitTests.Services;

// TS-CLASS-01 : Admin Class CRUD
public class ClassServiceTests
{
    [Fact]
    public async Task GetAll_ReturnsSeededClasses()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateClassService(db);

        var result = await svc.GetAllAsync();

        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Id == TestDbHelper.ClassId);
        result.Should().Contain(c => c.Id == TestDbHelper.Class2Id);
    }

    [Fact]
    public async Task Create_AddsClass()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateClassService(db);

        var created = await svc.CreateAsync(new CreateClassRequest
        {
            Name = "New Class",
            Description = "Desc"
        });

        var all = await svc.GetAllAsync();

        created.Name.Should().Be("New Class");
        all.Should().HaveCount(3);
        all.Should().Contain(c => c.Id == created.Id && c.Name == "New Class");
    }

    [Fact]
    public async Task Update_ChangesName()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateClassService(db);

        var updated = await svc.UpdateAsync(
            TestDbHelper.ClassId,
            new UpdateClassRequest { Name = "Updated" });

        var all = await svc.GetAllAsync();

        updated.Name.Should().Be("Updated");
        all.Should().Contain(c => c.Id == TestDbHelper.ClassId && c.Name == "Updated");
    }

    [Fact]
    public async Task Update_Missing_ThrowsNotFound()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateClassService(db);

        Func<Task> act = () => svc.UpdateAsync(
            Guid.NewGuid(),
            new UpdateClassRequest { Name = "Updated" });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_RemovesClass()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateClassService(db);

        var before = await svc.GetAllAsync();
        before.Should().HaveCount(2);

        await svc.DeleteAsync(TestDbHelper.Class2Id);

        var after = await svc.GetAllAsync();
        after.Should().HaveCount(1);
        after.Should().NotContain(c => c.Id == TestDbHelper.Class2Id);
    }

    [Fact]
    public async Task Delete_Missing_ThrowsNotFound()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateClassService(db);

        Func<Task> act = () => svc.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
