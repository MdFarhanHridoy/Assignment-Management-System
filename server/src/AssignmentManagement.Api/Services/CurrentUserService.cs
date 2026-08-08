using System.Security.Claims;
using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Domain;

namespace AssignmentManagement.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _contextAccessor;

    public CurrentUserService(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var sub = _contextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? _contextAccessor.HttpContext?.User?.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public UserRole? Role
    {
        get
        {
            var role = _contextAccessor.HttpContext?.User?.FindFirstValue("role");
            return Enum.TryParse<UserRole>(role, out var r) ? r : null;
        }
    }

    public bool IsAuthenticated =>
        _contextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
}
