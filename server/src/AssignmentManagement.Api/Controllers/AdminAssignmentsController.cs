using AssignmentManagement.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/assignments")]
public class AdminAssignmentsController : ControllerBase
{
    private readonly IAdminReadService _service;

    public AdminAssignmentsController(IAdminReadService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAssignmentsAsync(ct));
}
