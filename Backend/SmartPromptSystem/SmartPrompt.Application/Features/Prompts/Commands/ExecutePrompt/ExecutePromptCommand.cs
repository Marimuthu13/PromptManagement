using MediatR;
using SmartPrompt.Application.Features.Prompts.Queries.GetPromptExecutions;

namespace SmartPrompt.Application.Features.Prompts.Commands.ExecutePrompt;

public class ExecutePromptCommand : IRequest<PromptExecutionDto>
{
    public Guid PromptId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public Dictionary<string, string> Variables { get; set; } = new();
}
