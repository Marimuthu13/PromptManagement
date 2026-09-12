using Microsoft.Extensions.DependencyInjection;

namespace SmartPrompt.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register Application layer services, MediatR, FluentValidation, etc. here in future phases.
        
        return services;
    }
}
