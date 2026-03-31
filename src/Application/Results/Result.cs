namespace AuthenticationSystem.Application.Results;

public sealed class Result<T>
{
    private Result(bool success, T? value, AuthErrorCode? errorCode, string? errorMessage)
    {
        Success = success;
        Value = value;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public bool Success { get; }
    public T? Value { get; }
    public AuthErrorCode? ErrorCode { get; }
    public string? ErrorMessage { get; }

    public static Result<T> Ok(T value) => new(true, value, null, null);

    public static Result<T> Fail(AuthErrorCode errorCode, string message) => new(false, default, errorCode, message);
}
