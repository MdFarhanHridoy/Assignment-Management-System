using AssignmentManagement.Application.Common.DTOs.Users;
using AssignmentManagement.Domain;
using AssignmentManagement.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace AssignmentManagement.UnitTests.Services;

public class UserServiceTests
{
    private static readonly Guid MissingId = Guid.Parse("99999999-9999-9999-9999-999999999999");

    [Fact]
    public async Task CreateUser_ReturnsUserDto()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var users = TestDbHelper.CreateUserService(db);

        var result = await users.CreateAsync(new CreateUserRequest
        {
            Name = "Jane Doe",
            Email = "jane@example.com",
            Password = "secret123",
            Role = UserRole.Student
        });

        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Jane Doe");
        result.Email.Should().Be("jane@example.com");
        result.Role.Should().Be(UserRole.Student);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateUser_DuplicateEmail_ThrowsConflictException()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var users = TestDbHelper.CreateUserService(db);

        var act = () => users.CreateAsync(new CreateUserRequest
        {
            Name = "Dup User",
            Email = "admin@example.com",
            Password = "secret123",
            Role = UserRole.Teacher
        });

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task GetById_Existing_ReturnsUser()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var users = TestDbHelper.CreateUserService(db);

        var result = await users.GetByIdAsync(TestDbHelper.AdminId);

        result.Id.Should().Be(TestDbHelper.AdminId);
        result.Name.Should().Be("Admin User");
        result.Email.Should().Be("admin@example.com");
        result.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task GetById_Missing_ThrowsNotFoundException()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var users = TestDbHelper.CreateUserService(db);

        var act = () => users.GetByIdAsync(MissingId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAll_ReturnsSeededUsers()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var users = TestDbHelper.CreateUserService(db);

        var result = await users.GetAllAsync();

        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThanOrEqualTo(6);
        result.Should().Contain(u => u.Email == "admin@example.com")
              .Which.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task UpdateUser_ChangesName()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var users = TestDbHelper.CreateUserService(db);

        var updated = await users.UpdateAsync(
            TestDbHelper.Student2Id,
            new UpdateUserRequest { Name = "Renamed Two" });

        updated.Id.Should().Be(TestDbHelper.Student2Id);
        updated.Name.Should().Be("Renamed Two");
    }

    [Fact]
    public async Task UpdateUser_Missing_ThrowsNotFoundException()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var users = TestDbHelper.CreateUserService(db);

        var act = () => users.UpdateAsync(MissingId, new UpdateUserRequest { Name = "Ghost" });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteUser_RemovesUser()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var users = TestDbHelper.CreateUserService(db);

        var created = await users.CreateAsync(new CreateUserRequest
        {
            Name = "Ephemeral User",
            Email = "ephemeral@example.com",
            Password = "secret123",
            Role = UserRole.Student
        });

        await users.DeleteAsync(created.Id);

        var act = () => users.GetByIdAsync(created.Id);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteUser_Missing_ThrowsNotFoundException()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var users = TestDbHelper.CreateUserService(db);

        var act = () => users.DeleteAsync(MissingId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateUser_HashesPassword()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var users = TestDbHelper.CreateUserService(db);
        const string password = "plaintext-secret";

        await users.CreateAsync(new CreateUserRequest
        {
            Name = "Hash Check",
            Email = "hashcheck@example.com",
            Password = password,
            Role = UserRole.Student
        });

        var stored = db.Users.FirstOrDefault(u => u.Email == "hashcheck@example.com");

        stored.Should().NotBeNull();
        stored!.PasswordHash.Should().NotBe(password);
        stored.PasswordHash.Should().Be("hash-" + password);
    }

    [Fact]
    public async Task GetAll_UserDto_HasNoPasswordHashProperty()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var users = TestDbHelper.CreateUserService(db);

        var result = await users.GetAllAsync();

        result.Should().NotBeEmpty();
        typeof(UserDto).GetProperty("PasswordHash").Should().BeNull();
    }
}
