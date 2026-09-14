using MediatR;

namespace SmartPrompt.Application.Features.Prompts.Queries.GetPrompts;

public record GetPromptsQuery : IRequest<List<PromptDto>>
{
    public Guid? CategoryId { get; init; }
    public string? Search { get; init; }
}
