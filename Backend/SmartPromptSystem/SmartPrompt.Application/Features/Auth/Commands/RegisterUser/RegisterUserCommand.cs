using MediatR;

namespace SmartPrompt.Application.Features.Auth.Commands.RegisterUser;

public record RegisterUserCommand : IRequest<Guid>
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
