namespace AuthenticationSystem.Web.Services;

public sealed class ApiResult<T>
{
    private ApiResult(bool success, int statusCode, T? data, string? errorCode, string? errorMessage, IDictionary<string, string[]>? validationErrors)
    {
        Success = success;
        StatusCode = statusCode;
        Data = data;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        ValidationErrors = validationErrors;
    }

    public bool Success { get; }
    public int StatusCode { get; }
    public T? Data { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }
    public IDictionary<string, string[]>? ValidationErrors { get; }

    public static ApiResult<T> Ok(int statusCode, T? data) => new(true, statusCode, data, null, null, null);

    public static ApiResult<T> Fail(int statusCode, string? errorCode, string? errorMessage, IDictionary<string, string[]>? validationErrors)
        => new(false, statusCode, default, errorCode, errorMessage, validationErrors);
}
