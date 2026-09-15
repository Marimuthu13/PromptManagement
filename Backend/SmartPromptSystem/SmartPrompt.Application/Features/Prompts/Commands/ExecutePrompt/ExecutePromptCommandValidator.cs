using FluentValidation;

namespace SmartPrompt.Application.Features.Prompts.Commands.ExecutePrompt;

public class ExecutePromptCommandValidator : AbstractValidator<ExecutePromptCommand>
{
    public ExecutePromptCommandValidator()
    {
        RuleFor(x => x.PromptId).NotEmpty();
        RuleFor(x => x.ProviderName).NotEmpty();
        RuleFor(x => x.ModelName).NotEmpty();
    }
}
