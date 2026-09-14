using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Infrastructure.Authentication;
using SmartPrompt.Infrastructure.Persistence;
using SmartPrompt.Infrastructure.Persistence.Interceptors;

namespace SmartPrompt.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<SmartPromptDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<AuditableEntityInterceptor>();

            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                   .UseSnakeCaseNamingConvention()
                   .AddInterceptors(interceptor);
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<SmartPromptDbContext>());

        // Authentication & Authorization
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        // AI Provider
        services.Configure<SmartPrompt.Infrastructure.AI.OpenAIConfiguration>(configuration.GetSection(SmartPrompt.Infrastructure.AI.OpenAIConfiguration.SectionName));
        services.AddHttpClient<IAIProvider, SmartPrompt.Infrastructure.AI.OpenAIProvider>();

        return services;
    }
}
