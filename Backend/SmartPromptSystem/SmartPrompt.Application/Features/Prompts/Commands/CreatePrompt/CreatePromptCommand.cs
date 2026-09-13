using MediatR;

namespace SmartPrompt.Application.Features.Prompts.Commands.CreatePrompt;

public record CreatePromptCommand : IRequest<Guid>
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
}
