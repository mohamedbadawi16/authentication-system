using AuthenticationSystem.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace AuthenticationSystem.Infrastructure.Services;

public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendOtpAsync(string email, string code, CancellationToken cancellationToken)
    {
        _logger.LogInformation("OTP for {Email}: {Code}", email, code);
        return Task.CompletedTask;
    }
}
