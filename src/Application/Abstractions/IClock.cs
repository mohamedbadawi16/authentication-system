namespace AuthenticationSystem.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
