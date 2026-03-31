namespace AuthenticationSystem.Web.Services.Contracts;

public sealed record VerifyOtpRequest(string Email, string Code);
