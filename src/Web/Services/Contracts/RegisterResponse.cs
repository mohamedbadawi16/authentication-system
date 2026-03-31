namespace AuthenticationSystem.Web.Services.Contracts;

public sealed record RegisterResponse(string Email, bool VerificationRequired);
