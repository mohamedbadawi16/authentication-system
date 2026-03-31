namespace AuthenticationSystem.Application.Options;

public sealed class OtpOptions
{
    public int CodeLength { get; init; } = 6;
    public int ExpiryMinutes { get; init; } = 10;
    public int MinResendIntervalSeconds { get; init; } = 60;
    public int MaxPerHour { get; init; } = 5;
    public string HashKey { get; init; } = "change-me";
    public string? FixedCode { get; init; }
}
