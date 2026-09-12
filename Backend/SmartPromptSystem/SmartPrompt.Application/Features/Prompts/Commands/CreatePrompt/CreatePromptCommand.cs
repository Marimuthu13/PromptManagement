using MediatR;

namespace SmartPrompt.Application.Features.Prompts.Commands.CreatePrompt;

public record CreatePromptCommand : IRequest<Guid>
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    
    // Note: UserId is provided in the command temporarily.
    // In future phases, this should be resolved from the authenticated user's JWT claim.
    public Guid UserId { get; init; }
}
