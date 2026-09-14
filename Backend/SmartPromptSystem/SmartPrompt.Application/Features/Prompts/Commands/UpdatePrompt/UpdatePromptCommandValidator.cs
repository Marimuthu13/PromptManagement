using FluentValidation;

namespace SmartPrompt.Application.Features.Prompts.Commands.UpdatePrompt;

public class UpdatePromptCommandValidator : AbstractValidator<UpdatePromptCommand>
{
    public UpdatePromptCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(v => v.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(4000).WithMessage("Content must not exceed 4000 characters.");

        RuleFor(v => v.CategoryId)
            .NotEmpty().WithMessage("Category ID is required.");
    }
}
