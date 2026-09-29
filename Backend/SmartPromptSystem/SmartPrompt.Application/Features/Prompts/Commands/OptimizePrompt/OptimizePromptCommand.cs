using MediatR;

namespace SmartPrompt.Application.Features.Prompts.Commands.OptimizePrompt;

public record OptimizePromptCommand : IRequest<string>
{
    public string DraftContent { get; init; } = string.Empty;
}
