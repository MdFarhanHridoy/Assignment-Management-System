using AssignmentManagement.Application.Common.DTOs.Assignments;
using AssignmentManagement.Application.Common.DTOs.Submissions;
using AssignmentManagement.Application.Services;
using AssignmentManagement.Domain;
using AssignmentManagement.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace AssignmentManagement.UnitTests.Rules;

// TS-ADM-01 / TS-ADM-02 / TS-ADM-03 : Admin read visibility rules
public class AdminVisibilityTests
{
    // TS-ADM-01 : Admin sees every assignment regardless of status.
    [Fact]
    public async Task TS_ADM_01_AdminSeesAllAssignments()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateAdminReadService(db);

        var result = await svc.GetAllAssignmentsAsync();

        result.Should().HaveCount(4);
        result.Should().Contain(a => a.Id == TestDbHelper.DraftAssignmentId);
        result.Should().Contain(a => a.Id == TestDbHelper.PublishedAssignmentId);
        result.Should().Contain(a => a.Id == TestDbHelper.PastDeadlineAssignmentId);
        result.Should().Contain(a => a.Id == TestDbHelper.NoResubmissionAssignmentId);
    }

    // TS-ADM-01 : Draft assignments are NOT filtered out for admin.
    [Fact]
    public async Task TS_ADM_01_IncludesDraftAssignments()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateAdminReadService(db);

        var result = await svc.GetAllAssignmentsAsync();

        var draft = result.SingleOrDefault(a => a.Id == TestDbHelper.DraftAssignmentId);
        draft.Should().NotBeNull();
        draft!.Status.Should().Be(AssignmentStatus.Draft);
        result.Select(a => a.Status).Should().Contain(AssignmentStatus.Draft);
    }

    // TS-ADM-02 : Admin sees every submission.
    [Fact]
    public async Task TS_ADM_02_AdminSeesAllSubmissions()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateAdminReadService(db);

        var result = await svc.GetAllSubmissionsAsync();

        result.Should().HaveCount(1);
        result.Should().Contain(s => s.Id == TestDbHelper.SubmissionId);
        result[0].AssignmentId.Should().Be(TestDbHelper.PublishedAssignmentId);
        result[0].StudentId.Should().Be(TestDbHelper.StudentId);
    }

    // TS-ADM-03 : Admin visibility is global and not limited by teacher or status.
    [Fact]
    public async Task TS_ADM_03_AdminVisibilityNotLimitedByTeacher()
    {
        var db = await TestDbHelper.CreateDbAsync();
        var svc = TestDbHelper.CreateAdminReadService(db);

        var assignments = await svc.GetAllAssignmentsAsync();

        // No status filtering: every status present in seed data is visible.
        assignments.Select(a => a.Status).Distinct().Should().BeEquivalentTo(new[]
        {
            AssignmentStatus.Draft,
            AssignmentStatus.Published
        });

        // All assignments belong to the seeded teacher and remain visible to admin.
        assignments.Should().AllSatisfy(a =>
            a.TeacherId.Should().Be(TestDbHelper.TeacherId));

        // Published + past-deadline + no-resubmission items are all returned (not filtered).
        assignments.Should().Contain(a => a.Id == TestDbHelper.PublishedAssignmentId);
        assignments.Should().Contain(a => a.Id == TestDbHelper.PastDeadlineAssignmentId);
        assignments.Should().Contain(a => a.Id == TestDbHelper.NoResubmissionAssignmentId);
    }
}
