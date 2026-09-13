using MediatR;

namespace SmartPrompt.Application.Features.Auth.Commands.Login;

public record LoginResponseDto
{
    public string AccessToken { get; init; } = string.Empty;
    public int ExpiresInMinutes { get; init; }
}

public record LoginCommand : IRequest<LoginResponseDto>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
