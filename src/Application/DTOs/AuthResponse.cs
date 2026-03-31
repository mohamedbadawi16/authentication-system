namespace AuthenticationSystem.Application.DTOs;

public sealed record AuthResponse(string Token, DateTimeOffset ExpiresAt, Guid UserId, string Email);
