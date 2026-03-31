using AuthenticationSystem.Application.Abstractions;
using AuthenticationSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationSystem.Infrastructure.Services;

public sealed class PasswordHasherService : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(string password)
    {
        return _hasher.HashPassword(new User(), password);
    }

    public bool Verify(string hash, string password)
    {
        var result = _hasher.VerifyHashedPassword(new User(), hash, password);
        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}
