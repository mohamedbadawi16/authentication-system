using AuthenticationSystem.Application.Abstractions;
using AuthenticationSystem.Application.DTOs;
using AuthenticationSystem.Application.Options;
using AuthenticationSystem.Application.Results;
using AuthenticationSystem.Domain.Entities;
using Microsoft.Extensions.Options;

namespace AuthenticationSystem.Application.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IOtpRepository _otps;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IOtpGenerator _otpGenerator;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IEmailSender _emailSender;
    private readonly IClock _clock;
    private readonly OtpOptions _otpOptions;

    public AuthService(
        IUserRepository users,
        IOtpRepository otps,
        IPasswordHasher passwordHasher,
        IOtpGenerator otpGenerator,
        IJwtTokenGenerator jwt,
        IEmailSender emailSender,
        IClock clock,
        IOptions<OtpOptions> otpOptions)
    {
        _users = users;
        _otps = otps;
        _passwordHasher = passwordHasher;
        _otpGenerator = otpGenerator;
        _jwt = jwt;
        _emailSender = emailSender;
        _clock = clock;
        _otpOptions = otpOptions.Value;
    }

    public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var now = _clock.UtcNow;

        var existing = await _users.GetByEmailAsync(email, cancellationToken);
        if (existing is not null)
        {
            if (existing.IsVerified)
            {
                return Result<RegisterResponse>.Fail(AuthErrorCode.EmailAlreadyExists, "Email is already registered.");
            }

            var resend = await TrySendOtpAsync(existing, now, cancellationToken);
            if (!resend.Success)
            {
                return Result<RegisterResponse>.Fail(resend.ErrorCode!.Value, resend.ErrorMessage!);
            }

            return Result<RegisterResponse>.Ok(new RegisterResponse(existing.Email, true));
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            IsVerified = false,
            CreatedAt = now
        };

        await _users.AddAsync(user, cancellationToken);

        var otpResult = await TrySendOtpAsync(user, now, cancellationToken);
        if (!otpResult.Success)
        {
            return Result<RegisterResponse>.Fail(otpResult.ErrorCode!.Value, otpResult.ErrorMessage!);
        }

        return Result<RegisterResponse>.Ok(new RegisterResponse(user.Email, true));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _users.GetByEmailAsync(email, cancellationToken);

        if (user is null || !_passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            return Result<AuthResponse>.Fail(AuthErrorCode.InvalidCredentials, "Invalid email or password.");
        }

        if (!user.IsVerified)
        {
            return Result<AuthResponse>.Fail(AuthErrorCode.EmailNotVerified, "Email verification required.");
        }

        var (token, expiresAt) = _jwt.GenerateToken(user.Id, user.Email);
        return Result<AuthResponse>.Ok(new AuthResponse(token, expiresAt, user.Id, user.Email));
    }

    public async Task<Result<AuthResponse>> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _users.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            return Result<AuthResponse>.Fail(AuthErrorCode.OtpInvalid, "Invalid OTP.");
        }

        var now = _clock.UtcNow;
        var latest = await _otps.GetLatestAsync(user.Id, cancellationToken);
        if (latest is null)
        {
            return Result<AuthResponse>.Fail(AuthErrorCode.OtpInvalid, "Invalid OTP.");
        }

        if (latest.UsedAt is not null)
        {
            return Result<AuthResponse>.Fail(AuthErrorCode.OtpInvalid, "OTP has already been used.");
        }

        if (latest.ExpiresAt <= now)
        {
            return Result<AuthResponse>.Fail(AuthErrorCode.OtpExpired, "OTP has expired.");
        }

        if (!_otpGenerator.Verify(latest.CodeHash, request.Code))
        {
            return Result<AuthResponse>.Fail(AuthErrorCode.OtpInvalid, "Invalid OTP.");
        }

        latest.UsedAt = now;
        await _otps.UpdateAsync(latest, cancellationToken);

        if (!user.IsVerified)
        {
            user.IsVerified = true;
            await _users.UpdateAsync(user, cancellationToken);
        }

        var (token, expiresAt) = _jwt.GenerateToken(user.Id, user.Email);
        return Result<AuthResponse>.Ok(new AuthResponse(token, expiresAt, user.Id, user.Email));
    }

    public async Task<Result<bool>> ResendOtpAsync(ResendOtpRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _users.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            return Result<bool>.Ok(true);
        }

        if (user.IsVerified)
        {
            return Result<bool>.Ok(true);
        }

        var now = _clock.UtcNow;
        var resend = await TrySendOtpAsync(user, now, cancellationToken);
        if (!resend.Success)
        {
            return Result<bool>.Fail(resend.ErrorCode!.Value, resend.ErrorMessage!);
        }

        return Result<bool>.Ok(true);
    }

    private async Task<Result<bool>> TrySendOtpAsync(User user, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var latest = await _otps.GetLatestAsync(user.Id, cancellationToken);
        if (latest is not null)
        {
            var minInterval = TimeSpan.FromSeconds(_otpOptions.MinResendIntervalSeconds);
            if (now - latest.CreatedAt < minInterval)
            {
                return Result<bool>.Fail(AuthErrorCode.OtpRateLimited, "Please wait before requesting another OTP.");
            }
        }

        var windowStart = now.AddHours(-1);
        var recentCount = await _otps.CountSinceAsync(user.Id, windowStart, cancellationToken);
        if (recentCount >= _otpOptions.MaxPerHour)
        {
            return Result<bool>.Fail(AuthErrorCode.OtpRateLimited, "OTP request limit reached. Try again later.");
        }

        var code = _otpGenerator.GenerateCode(_otpOptions.CodeLength);
        var otp = new OtpCode
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CodeHash = _otpGenerator.Hash(code),
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(_otpOptions.ExpiryMinutes)
        };

        await _otps.AddAsync(otp, cancellationToken);
        await _emailSender.SendOtpAsync(user.Email, code, cancellationToken);

        return Result<bool>.Ok(true);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
