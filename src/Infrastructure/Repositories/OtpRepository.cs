using AuthenticationSystem.Application.Abstractions;
using AuthenticationSystem.Domain.Entities;
using AuthenticationSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationSystem.Infrastructure.Repositories;

public sealed class OtpRepository : IOtpRepository
{
    private readonly AppDbContext _db;

    public OtpRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<OtpCode?> GetLatestAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _db.OtpCodes
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<OtpCode?> GetLatestActiveAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        return _db.OtpCodes
            .Where(x => x.UserId == userId && x.UsedAt == null && x.ExpiresAt > now)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<int> CountSinceAsync(Guid userId, DateTimeOffset since, CancellationToken cancellationToken)
    {
        return _db.OtpCodes.CountAsync(x => x.UserId == userId && x.CreatedAt >= since, cancellationToken);
    }

    public async Task AddAsync(OtpCode otp, CancellationToken cancellationToken)
    {
        _db.OtpCodes.Add(otp);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(OtpCode otp, CancellationToken cancellationToken)
    {
        _db.OtpCodes.Update(otp);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
