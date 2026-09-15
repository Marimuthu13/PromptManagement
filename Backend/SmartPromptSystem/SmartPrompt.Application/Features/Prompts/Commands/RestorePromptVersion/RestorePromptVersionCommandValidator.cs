using FluentValidation;

namespace SmartPrompt.Application.Features.Prompts.Commands.RestorePromptVersion;

public class RestorePromptVersionCommandValidator : AbstractValidator<RestorePromptVersionCommand>
{
    public RestorePromptVersionCommandValidator()
    {
        RuleFor(x => x.PromptId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
    }
}
