using MediatR;

namespace SmartPrompt.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand : IRequest<Guid>
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
