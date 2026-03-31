using AuthenticationSystem.Application.Abstractions;
using AuthenticationSystem.Application.DTOs;
using AuthenticationSystem.Application.Results;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationSystem.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        if (result.Success)
        {
            return Accepted(result.Value);
        }

        return ToProblem(result);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        if (result.Success)
        {
            return Ok(result.Value);
        }

        return ToProblem(result);
    }

    [HttpPost("verify-otp")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyOtp(VerifyOtpRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.VerifyOtpAsync(request, cancellationToken);
        if (result.Success)
        {
            return Ok(result.Value);
        }

        return ToProblem(result);
    }

    [HttpPost("resend-otp")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ResendOtp(ResendOtpRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.ResendOtpAsync(request, cancellationToken);
        if (result.Success)
        {
            return Accepted(new MessageResponse("If the account exists, a new OTP has been sent."));
        }

        return ToProblem(result);
    }

    private IActionResult ToProblem<T>(Result<T> result)
    {
        var status = MapStatusCode(result.ErrorCode ?? AuthErrorCode.InvalidCredentials);
        var problem = new ProblemDetails
        {
            Title = "Request failed",
            Detail = result.ErrorMessage,
            Status = status
        };

        problem.Extensions["code"] = ToErrorCode(result.ErrorCode ?? AuthErrorCode.InvalidCredentials);
        return StatusCode(status, problem);
    }

    private static int MapStatusCode(AuthErrorCode errorCode)
    {
        return errorCode switch
        {
            AuthErrorCode.EmailAlreadyExists => StatusCodes.Status409Conflict,
            AuthErrorCode.InvalidCredentials => StatusCodes.Status401Unauthorized,
            AuthErrorCode.EmailNotVerified => StatusCodes.Status403Forbidden,
            AuthErrorCode.OtpExpired => StatusCodes.Status410Gone,
            AuthErrorCode.OtpInvalid => StatusCodes.Status400BadRequest,
            AuthErrorCode.OtpRateLimited => StatusCodes.Status429TooManyRequests,
            AuthErrorCode.UserNotFound => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status400BadRequest
        };
    }

    private static string ToErrorCode(AuthErrorCode errorCode)
    {
        return errorCode switch
        {
            AuthErrorCode.EmailAlreadyExists => "email_already_exists",
            AuthErrorCode.InvalidCredentials => "invalid_credentials",
            AuthErrorCode.EmailNotVerified => "email_not_verified",
            AuthErrorCode.OtpExpired => "otp_expired",
            AuthErrorCode.OtpInvalid => "otp_invalid",
            AuthErrorCode.OtpRateLimited => "otp_rate_limited",
            AuthErrorCode.UserNotFound => "user_not_found",
            _ => "auth_error"
        };
    }
}
