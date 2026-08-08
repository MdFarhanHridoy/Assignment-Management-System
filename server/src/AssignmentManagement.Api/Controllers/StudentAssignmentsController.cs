using AssignmentManagement.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.Api.Controllers;

[ApiController]
[Authorize(Roles = "Student")]
[Route("api/student/assignments")]
public class StudentAssignmentsController : ControllerBase
{
    private readonly IAssignmentService _service;

    public StudentAssignmentsController(IAssignmentService service)
    {
        _service = service;
    }

    private Guid UserId => Guid.Parse(User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetPublished(CancellationToken ct)
        => Ok(await _service.GetPublishedForStudentAsync(UserId, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await _service.GetPublishedDetailForStudentAsync(id, UserId, ct));
}
