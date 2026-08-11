using AssignmentManagement.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.Api.Controllers;

[ApiController]
[Authorize(Roles = "Teacher")]
[Route("api/teacher/teacher-assignments")]
public class TeacherAssignmentLinksController : ControllerBase
{
    private readonly ITeacherAssignmentService _service;

    public TeacherAssignmentLinksController(ITeacherAssignmentService service)
    {
        _service = service;
    }

    private Guid UserId => Guid.Parse(User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken ct)
        => Ok(await _service.GetMineAsync(UserId, ct));
}
