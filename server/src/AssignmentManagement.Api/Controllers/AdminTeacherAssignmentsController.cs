using AssignmentManagement.Application.Common.DTOs.TeacherAssignments;
using AssignmentManagement.Application.Common.Validators;
using AssignmentManagement.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/teacher-assignments")]
public class AdminTeacherAssignmentsController : ControllerBase
{
    private readonly ITeacherAssignmentService _service;
    private readonly CreateTeacherAssignmentRequestValidator _validator;

    public AdminTeacherAssignmentsController(
        ITeacherAssignmentService service,
        CreateTeacherAssignmentRequestValidator validator)
    {
        _service = service;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTeacherAssignmentRequest request, CancellationToken ct)
    {
        _validator.ValidateAndThrow(request);
        var result = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), null, result);
    }
}
