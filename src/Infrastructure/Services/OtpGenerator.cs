using System.Security.Cryptography;
using System.Text;
using AuthenticationSystem.Application.Abstractions;
using AuthenticationSystem.Application.Options;
using Microsoft.Extensions.Options;

namespace AuthenticationSystem.Infrastructure.Services;

public sealed class OtpGenerator : IOtpGenerator
{
    private readonly byte[] _keyBytes;
    private readonly string? _fixedCode;

    public OtpGenerator(IOptions<OtpOptions> options)
    {
        var config = options.Value;
        _keyBytes = Encoding.UTF8.GetBytes(config.HashKey);
        _fixedCode = string.IsNullOrWhiteSpace(config.FixedCode) ? null : config.FixedCode;
    }

    public string GenerateCode(int length)
    {
        if (!string.IsNullOrWhiteSpace(_fixedCode))
        {
            if (_fixedCode.Length != length)
            {
                throw new InvalidOperationException("Fixed OTP length does not match configuration.");
            }

            return _fixedCode;
        }

        var max = (int)Math.Pow(10, length);
        var value = RandomNumberGenerator.GetInt32(0, max);
        return value.ToString($"D{length}");
    }

    public string Hash(string code)
    {
        using var hmac = new HMACSHA256(_keyBytes);
        var bytes = Encoding.UTF8.GetBytes(code);
        var hash = hmac.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public bool Verify(string hash, string code)
    {
        var expected = Hash(code);
        return FixedTimeEquals(hash, expected);
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        var leftBytes = Convert.FromBase64String(left);
        var rightBytes = Convert.FromBase64String(right);
        return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }
}
