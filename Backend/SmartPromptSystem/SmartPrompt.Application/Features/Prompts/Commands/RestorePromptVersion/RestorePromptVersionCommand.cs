using MediatR;
using SmartPrompt.Application.Features.Prompts.Queries.GetPrompts;

namespace SmartPrompt.Application.Features.Prompts.Commands.RestorePromptVersion;

public class RestorePromptVersionCommand : IRequest<PromptDto>
{
    public Guid PromptId { get; set; }
    public Guid VersionId { get; set; }
}
