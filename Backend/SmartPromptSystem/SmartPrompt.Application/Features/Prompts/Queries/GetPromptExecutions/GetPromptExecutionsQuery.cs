using MediatR;

namespace SmartPrompt.Application.Features.Prompts.Queries.GetPromptExecutions;

public class GetPromptExecutionsQuery : IRequest<List<PromptExecutionDto>>
{
    public Guid PromptId { get; set; }
}
