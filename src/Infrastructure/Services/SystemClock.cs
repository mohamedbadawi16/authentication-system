using AuthenticationSystem.Application.Abstractions;

namespace AuthenticationSystem.Infrastructure.Services;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
