using FluentValidation;

namespace SmartPrompt.Application.Features.Prompts.Commands.CreatePrompt;

public class CreatePromptCommandValidator : AbstractValidator<CreatePromptCommand>
{
    public CreatePromptCommandValidator()
    {
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(v => v.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(v => v.Content)
            .NotEmpty().WithMessage("Content is required.");

        RuleFor(v => v.CategoryId)
            .NotEmpty().WithMessage("CategoryId is required.");
    }
}
