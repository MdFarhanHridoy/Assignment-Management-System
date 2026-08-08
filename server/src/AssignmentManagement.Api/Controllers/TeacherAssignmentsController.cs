using AssignmentManagement.Application.Common.DTOs.Assignments;
using AssignmentManagement.Application.Common.Validators;
using AssignmentManagement.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AssignmentManagement.Api.Controllers;

[ApiController]
[Authorize(Roles = "Teacher")]
[Route("api/teacher/assignments")]
public class TeacherAssignmentsController : ControllerBase
{
    private readonly IAssignmentService _service;
    private readonly CreateAssignmentRequestValidator _createValidator;
    private readonly UpdateAssignmentRequestValidator _updateValidator;

    public TeacherAssignmentsController(
        IAssignmentService service,
        CreateAssignmentRequestValidator createValidator,
        UpdateAssignmentRequestValidator updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    private Guid UserId => Guid.Parse(User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken ct)
        => Ok(await _service.GetMyAssignmentsAsync(UserId, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await _service.GetMyAssignmentAsync(id, UserId, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAssignmentRequest request, CancellationToken ct)
    {
        _createValidator.ValidateAndThrow(request);
        var result = await _service.CreateAsync(request, UserId, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAssignmentRequest request, CancellationToken ct)
    {
        _updateValidator.ValidateAndThrow(request);
        return Ok(await _service.UpdateAsync(id, request, UserId, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, UserId, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
        => Ok(await _service.PublishAsync(id, UserId, ct));
}
