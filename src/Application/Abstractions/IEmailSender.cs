namespace AuthenticationSystem.Application.Abstractions;

public interface IEmailSender
{
    Task SendOtpAsync(string email, string code, CancellationToken cancellationToken);
}
