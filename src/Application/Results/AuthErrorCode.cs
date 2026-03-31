namespace AuthenticationSystem.Application.Results;

public enum AuthErrorCode
{
    EmailAlreadyExists,
    InvalidCredentials,
    EmailNotVerified,
    OtpExpired,
    OtpInvalid,
    OtpRateLimited,
    UserNotFound
}
