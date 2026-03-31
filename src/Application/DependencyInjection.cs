using AuthenticationSystem.Application.Abstractions;
using AuthenticationSystem.Application.Auth;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AuthenticationSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        return services;
    }
}
