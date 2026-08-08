using AssignmentManagement.Application.Common.DTOs.Classes;
using AssignmentManagement.Application.Common.Validators;
using AssignmentManagement.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/classes")]
public class AdminClassesController : ControllerBase
{
    private readonly IClassService _service;
    private readonly CreateClassRequestValidator _createValidator;
    private readonly UpdateClassRequestValidator _updateValidator;

    public AdminClassesController(
        IClassService service,
        CreateClassRequestValidator createValidator,
        UpdateClassRequestValidator updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClassRequest request, CancellationToken ct)
    {
        _createValidator.ValidateAndThrow(request);
        var result = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), null, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClassRequest request, CancellationToken ct)
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
