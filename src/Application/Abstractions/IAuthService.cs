using AuthenticationSystem.Application.DTOs;
using AuthenticationSystem.Application.Results;

namespace AuthenticationSystem.Application.Abstractions;

public interface IAuthService
{
    Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<Result<AuthResponse>> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken);
    Task<Result<bool>> ResendOtpAsync(ResendOtpRequest request, CancellationToken cancellationToken);
}
