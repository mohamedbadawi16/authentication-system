using AuthenticationSystem.Domain.Entities;

namespace AuthenticationSystem.Application.Abstractions;

public interface IOtpRepository
{
    Task<OtpCode?> GetLatestAsync(Guid userId, CancellationToken cancellationToken);
    Task<OtpCode?> GetLatestActiveAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken);
    Task<int> CountSinceAsync(Guid userId, DateTimeOffset since, CancellationToken cancellationToken);
    Task AddAsync(OtpCode otp, CancellationToken cancellationToken);
    Task UpdateAsync(OtpCode otp, CancellationToken cancellationToken);
}
