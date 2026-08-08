using AssignmentManagement.Application.Common.DTOs.Enrollments;
using AssignmentManagement.Application.Common.Validators;
using AssignmentManagement.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/enrollments")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _service;
    private readonly CreateEnrollmentRequestValidator _validator;

    public EnrollmentsController(
        IEnrollmentService service,
        CreateEnrollmentRequestValidator validator)
    {
        _service = service;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request, CancellationToken ct)
    {
        _validator.ValidateAndThrow(request);
        var result = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), null, result);
    }
}
