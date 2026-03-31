using System.Text;
using AuthenticationSystem.Api.Middleware;
using AuthenticationSystem.Application;
using AuthenticationSystem.Application.Options;
using AuthenticationSystem.Infrastructure;
using AuthenticationSystem.Infrastructure.Data;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddFluentValidationAutoValidation();

var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
    {
        if (corsOrigins is { Length: > 0 })
        {
            policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
    });
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problem = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed"
        };
        problem.Extensions["code"] = "validation_error";
        return new BadRequestObjectResult(problem);
    };
});

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("WebClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
