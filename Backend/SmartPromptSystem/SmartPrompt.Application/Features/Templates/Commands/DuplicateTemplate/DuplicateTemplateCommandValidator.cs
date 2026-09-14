using FluentValidation;

namespace SmartPrompt.Application.Features.Templates.Commands.DuplicateTemplate;

public class DuplicateTemplateCommandValidator : AbstractValidator<DuplicateTemplateCommand>
{
    public DuplicateTemplateCommandValidator()
    {
        RuleFor(x => x.TemplateId).NotEmpty();
    }
}
