using FluentValidation;

namespace SmartPrompt.Application.Features.Templates.Queries.GetTemplates;

public class GetTemplatesQueryValidator : AbstractValidator<GetTemplatesQuery>
{
    public GetTemplatesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(100);
    }
}
