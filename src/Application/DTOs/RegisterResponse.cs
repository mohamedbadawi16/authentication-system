namespace AuthenticationSystem.Application.DTOs;

public sealed record RegisterResponse(string Email, bool VerificationRequired);
