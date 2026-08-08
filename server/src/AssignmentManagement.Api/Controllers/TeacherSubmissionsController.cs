using AssignmentManagement.Application.Common.DTOs.Submissions;
using AssignmentManagement.Application.Common.Validators;
using AssignmentManagement.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.Api.Controllers;

[ApiController]
[Authorize(Roles = "Teacher")]
[Route("api/teacher")]
public class TeacherSubmissionsController : ControllerBase
{
    private readonly ISubmissionService _service;
    private readonly ReviewSubmissionRequestValidator _reviewValidator;

    public TeacherSubmissionsController(ISubmissionService service, ReviewSubmissionRequestValidator reviewValidator)
    {
        _service = service;
        _reviewValidator = reviewValidator;
    }

    private Guid UserId => Guid.Parse(User.FindFirst("sub")!.Value);

    [HttpGet("assignments/{assignmentId:guid}/submissions")]
    public async Task<IActionResult> GetSubmissionsForAssignment(Guid assignmentId, CancellationToken ct)
        => Ok(await _service.GetSubmissionsForAssignmentAsync(assignmentId, UserId, ct));

    [HttpPut("submissions/{submissionId:guid}/review")]
    public async Task<IActionResult> Review(Guid submissionId, [FromBody] ReviewSubmissionRequest request, CancellationToken ct)
    {
        _reviewValidator.ValidateAndThrow(request);
        return Ok(await _service.ReviewAsync(submissionId, request, UserId, ct));
    }
}
