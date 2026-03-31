namespace AuthenticationSystem.Application.Abstractions;

public interface IJwtTokenGenerator
{
    (string Token, DateTimeOffset ExpiresAt) GenerateToken(Guid userId, string email);
}
