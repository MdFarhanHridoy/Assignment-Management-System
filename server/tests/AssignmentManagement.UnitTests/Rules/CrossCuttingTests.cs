using AssignmentManagement.Application.Common.DTOs.Assignments;
using AssignmentManagement.Application.Common.DTOs.Auth;
using AssignmentManagement.Application.Common.DTOs.Users;
using AssignmentManagement.Domain;
using AssignmentManagement.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace AssignmentManagement.UnitTests.Rules;

public class CrossCuttingTests
{
    [Fact]
    public async Task DeadlineUtc_IsStoredAsUtc()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var assignments = TestDbHelper.CreateAssignmentService(db);
        var deadline = new DateTime(2026, 12, 31, 23, 59, 0, DateTimeKind.Utc);

        var created = await assignments.CreateAsync(
            new CreateAssignmentRequest
            {
                Title = "Deadline Test",
                Description = "Verifies UTC deadline storage",
                DeadlineUtc = deadline,
                MaxMarks = 50,
                ClassId = TestDbHelper.ClassId,
                SubjectId = TestDbHelper.SubjectId,
                AllowResubmission = true
            },
            TestDbHelper.TeacherId);

        var readBack = await assignments.GetMyAssignmentAsync(created.Id, TestDbHelper.TeacherId);

        readBack.DeadlineUtc.Should().Be(deadline);
        readBack.DeadlineUtc.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void PasswordHash_NotInUserDto()
    {
        typeof(UserDto).GetProperty("PasswordHash").Should().BeNull();
    }

    [Fact]
    public void PasswordHash_NotInAuthResponseUser()
    {
        typeof(AuthResponse).GetProperty("PasswordHash").Should().BeNull();

        var userProperty = typeof(AuthResponse).GetProperty("User");
        userProperty.Should().NotBeNull();
        userProperty!.PropertyType.GetProperty("PasswordHash").Should().BeNull();
    }

    [Theory]
    [InlineData("admin@example.com", UserRole.Admin)]
    [InlineData("teacher@example.com", UserRole.Teacher)]
    [InlineData("student@example.com", UserRole.Student)]
    public async Task Login_AllRoles_ReturnsNonEmptyToken(string email, UserRole role)
    {
        var db = await TestDbHelper.CreateDbAsync();
        var auth = TestDbHelper.CreateAuthService(db);

        var result = await auth.LoginAsync(new LoginRequest { Email = email, Password = email });

        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.User.Role.Should().Be(role);
    }
}
