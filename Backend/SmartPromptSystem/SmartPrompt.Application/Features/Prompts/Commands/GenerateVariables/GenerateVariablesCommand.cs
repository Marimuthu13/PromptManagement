using MediatR;

namespace SmartPrompt.Application.Features.Prompts.Commands.GenerateVariables;

public record GenerateVariablesCommand : IRequest<Dictionary<string, string>>
{
    public string PromptContent { get; init; } = string.Empty;
    public List<string> VariableNames { get; init; } = new();
}
