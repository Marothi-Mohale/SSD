using SSD.Application.Contracts.Auth;

namespace SSD.Application.Abstractions;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, AuthRequestContext context, CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginAsync(LoginRequest request, AuthRequestContext context, CancellationToken cancellationToken = default);

    Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, AuthRequestContext context, CancellationToken cancellationToken = default);

    Task LogoutAsync(LogoutRequest request, AuthRequestContext context, CancellationToken cancellationToken = default);
}
