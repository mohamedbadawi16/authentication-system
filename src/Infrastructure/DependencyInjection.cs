using AuthenticationSystem.Application.Abstractions;
using AuthenticationSystem.Application.Options;
using AuthenticationSystem.Infrastructure.Data;
using AuthenticationSystem.Infrastructure.Repositories;
using AuthenticationSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthenticationSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? "Server=(localdb)\\\\MSSQLLocalDB;Database=AuthenticationSystemDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<OtpOptions>(configuration.GetSection("Otp"));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOtpRepository, OtpRepository>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPasswordHasher, PasswordHasherService>();
        services.AddSingleton<IOtpGenerator, OtpGenerator>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IEmailSender, LoggingEmailSender>();

        return services;
    }
}
