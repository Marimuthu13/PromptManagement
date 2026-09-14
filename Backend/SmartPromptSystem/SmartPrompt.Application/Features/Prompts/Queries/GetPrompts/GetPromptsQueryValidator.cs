using FluentValidation;

namespace SmartPrompt.Application.Features.Prompts.Queries.GetPrompts;

public class GetPromptsQueryValidator : AbstractValidator<GetPromptsQuery>
{
    public GetPromptsQueryValidator()
    {
        RuleFor(v => v.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page at least greater than or equal to 1.");

        RuleFor(v => v.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("PageSize at least greater than or equal to 1.")
            .LessThanOrEqualTo(100).WithMessage("PageSize must not exceed 100 to prevent unbounded queries.");
    }
}
