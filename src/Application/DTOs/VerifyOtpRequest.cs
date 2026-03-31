namespace AuthenticationSystem.Application.DTOs;

public sealed record VerifyOtpRequest(string Email, string Code);
