namespace AuthenticationSystem.Web.Services.Contracts;

public sealed record AuthResponse(string Token, DateTimeOffset ExpiresAt, Guid UserId, string Email);
