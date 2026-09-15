using MediatR;

namespace SmartPrompt.Application.Features.Prompts.Queries.GetPromptVersions;

public class GetPromptVersionsQuery : IRequest<List<PromptVersionDto>>
{
    public Guid PromptId { get; set; }
}
