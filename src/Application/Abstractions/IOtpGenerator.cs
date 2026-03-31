namespace AuthenticationSystem.Application.Abstractions;

public interface IOtpGenerator
{
    string GenerateCode(int length);
    string Hash(string code);
    bool Verify(string hash, string code);
}
