using MediatR;

namespace SmartPrompt.Application.Features.Prompts.Queries.GetPrompts;

public record GetPromptsQuery : IRequest<List<PromptDto>>
{
}
