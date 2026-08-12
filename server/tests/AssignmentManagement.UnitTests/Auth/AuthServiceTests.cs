using AssignmentManagement.Application.Common.DTOs.Auth;
using AssignmentManagement.Application.Common.DTOs.Users;
using AssignmentManagement.Domain;
using AssignmentManagement.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace AssignmentManagement.UnitTests.Auth;

public class AuthServiceTests
{
    private static LoginRequest Login(string email, string password) =>
        new() { Email = email, Password = password };

    [Fact]
    public async Task ValidLogin_ReturnsAuthResponseWithToken()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var auth = TestDbHelper.CreateAuthService(db);

        var result = await auth.LoginAsync(Login("teacher@example.com", "teacher@example.com"));

        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be("teacher@example.com");
        result.User.Role.Should().Be(UserRole.Teacher);
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task ValidLogin_Admin_ReturnsAdminRole()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var auth = TestDbHelper.CreateAuthService(db);

        var result = await auth.LoginAsync(Login("admin@example.com", "admin@example.com"));

        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.User.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task InvalidLogin_WrongPassword_ReturnsNull()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var auth = TestDbHelper.CreateAuthService(db);

        var result = await auth.LoginAsync(Login("teacher@example.com", "definitely-not-the-password"));

        result.Should().BeNull();
    }

    [Fact]
    public async Task InvalidLogin_UnknownEmail_ReturnsNull()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var auth = TestDbHelper.CreateAuthService(db);

        var result = await auth.LoginAsync(Login("nobody@example.com", "nobody@example.com"));

        result.Should().BeNull();
    }

    [Fact]
    public async Task Login_DisabledUser_ReturnsNull()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var users = TestDbHelper.CreateUserService(db);

        var created = await users.CreateAsync(new CreateUserRequest
        {
            Name = "Disabled User",
            Email = "disabled@example.com",
            Password = "disabled@example.com",
            Role = UserRole.Teacher
        });
        await users.UpdateAsync(created.Id, new UpdateUserRequest { IsActive = false });

        var auth = TestDbHelper.CreateAuthService(db);
        var result = await auth.LoginAsync(Login("disabled@example.com", "disabled@example.com"));

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginResponse_UserDto_HasNoPasswordHash()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var auth = TestDbHelper.CreateAuthService(db);

        var result = await auth.LoginAsync(Login("teacher@example.com", "teacher@example.com"));

        result.Should().NotBeNull();
        result!.User.Should().NotBeNull();
        typeof(UserDto).GetProperty("PasswordHash").Should().BeNull();
    }
}
