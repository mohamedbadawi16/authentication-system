using System.Net.Http.Json;
using AuthenticationSystem.Web.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationSystem.Web.Services;

public sealed class AuthApiClient
{
    private readonly HttpClient _client;

    public AuthApiClient(HttpClient client)
    {
        _client = client;
    }

    public Task<ApiResult<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
        => PostAsync<RegisterResponse>("api/auth/register", request, cancellationToken);

    public Task<ApiResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
        => PostAsync<AuthResponse>("api/auth/login", request, cancellationToken);

    public Task<ApiResult<AuthResponse>> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken)
        => PostAsync<AuthResponse>("api/auth/verify-otp", request, cancellationToken);

    public Task<ApiResult<MessageResponse>> ResendOtpAsync(ResendOtpRequest request, CancellationToken cancellationToken)
        => PostAsync<MessageResponse>("api/auth/resend-otp", request, cancellationToken);

    private async Task<ApiResult<T>> PostAsync<T>(string url, object payload, CancellationToken cancellationToken)
    {
        using var response = await _client.PostAsJsonAsync(url, payload, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
            return ApiResult<T>.Ok((int)response.StatusCode, data);
        }

        return await ReadFailure<T>(response, cancellationToken);
    }

    private static async Task<ApiResult<T>> ReadFailure<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string? errorCode = null;
        string? message = null;
        IDictionary<string, string[]>? validationErrors = null;

        if (response.Content.Headers.ContentType?.MediaType == "application/problem+json")
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: cancellationToken);

            if (problem is ValidationProblemDetails validation && validation.Errors.Count > 0)
            {
                validationErrors = validation.Errors;
            }

            if (problem?.Extensions.TryGetValue("code", out var codeObj) == true)
            {
                errorCode = codeObj?.ToString();
            }

            message = problem?.Detail ?? problem?.Title;
        }

        message ??= "Request failed.";
        return ApiResult<T>.Fail((int)response.StatusCode, errorCode, message, validationErrors);
    }
}
