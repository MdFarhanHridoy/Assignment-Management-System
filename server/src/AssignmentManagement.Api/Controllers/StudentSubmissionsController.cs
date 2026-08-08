using AssignmentManagement.Application.Common.DTOs.Submissions;
using AssignmentManagement.Application.Common.Validators;
using AssignmentManagement.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.Api.Controllers;

[ApiController]
[Authorize(Roles = "Student")]
[Route("api/student")]
public class StudentSubmissionsController : ControllerBase
{
    private readonly ISubmissionService _service;
    private readonly SubmitRequestValidator _submitValidator;
    private readonly UpdateSubmissionRequestValidator _updateValidator;

    public StudentSubmissionsController(
        ISubmissionService service,
        SubmitRequestValidator submitValidator,
        UpdateSubmissionRequestValidator updateValidator)
    {
        _service = service;
        _submitValidator = submitValidator;
        _updateValidator = updateValidator;
    }

    private Guid UserId => Guid.Parse(User.FindFirst("sub")!.Value);

    [HttpPost("assignments/{assignmentId:guid}/submit")]
    public async Task<IActionResult> Submit(Guid assignmentId, [FromBody] SubmitRequest request, CancellationToken ct)
    {
        _submitValidator.ValidateAndThrow(request);
        var result = await _service.SubmitAsync(assignmentId, request, UserId, ct);
        return Ok(result);
    }

    [HttpGet("submissions")]
    public async Task<IActionResult> GetMine(CancellationToken ct)
        => Ok(await _service.GetMySubmissionsAsync(UserId, ct));

    [HttpGet("submissions/{submissionId:guid}")]
    public async Task<IActionResult> GetById(Guid submissionId, CancellationToken ct)
        => Ok(await _service.GetMySubmissionAsync(submissionId, UserId, ct));

    [HttpPut("submissions/{submissionId:guid}")]
    public async Task<IActionResult> Update(Guid submissionId, [FromBody] UpdateSubmissionRequest request, CancellationToken ct)
    {
        _updateValidator.ValidateAndThrow(request);
        return Ok(await _service.UpdateSubmissionAsync(submissionId, request, UserId, ct));
    }
}
