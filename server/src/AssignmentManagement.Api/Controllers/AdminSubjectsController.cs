using AssignmentManagement.Application.Common.DTOs.Subjects;
using AssignmentManagement.Application.Common.Validators;
using AssignmentManagement.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/subjects")]
public class AdminSubjectsController : ControllerBase
{
    private readonly ISubjectService _service;
    private readonly CreateSubjectRequestValidator _createValidator;
    private readonly UpdateSubjectRequestValidator _updateValidator;

    public AdminSubjectsController(
        ISubjectService service,
        CreateSubjectRequestValidator createValidator,
        UpdateSubjectRequestValidator updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSubjectRequest request, CancellationToken ct)
    {
        _createValidator.ValidateAndThrow(request);
        var result = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), null, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSubjectRequest request, CancellationToken ct)
    {
        _updateValidator.ValidateAndThrow(request);
        return Ok(await _service.UpdateAsync(id, request, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
