using AssignmentManagement.Application.Common.DTOs.Auth;

namespace AssignmentManagement.Application.Services;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
