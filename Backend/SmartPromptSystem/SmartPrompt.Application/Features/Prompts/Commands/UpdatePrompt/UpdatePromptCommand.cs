using MediatR;
using SmartPrompt.Application.Features.Prompts.Queries.GetPrompts;

namespace SmartPrompt.Application.Features.Prompts.Commands.UpdatePrompt;

public record UpdatePromptCommand : IRequest<PromptDto>
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
}
