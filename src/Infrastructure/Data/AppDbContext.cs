using AuthenticationSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationSystem.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<OtpCode>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CodeHash).IsRequired();
            entity.HasIndex(x => new { x.UserId, x.CreatedAt });
        });
    }
}
