using MediatR;
using SmartPrompt.Application.Features.Templates.Queries.GetTemplates;

namespace SmartPrompt.Application.Features.Templates.Queries.GetTemplateById;

public class GetTemplateByIdQuery : IRequest<TemplateDto>
{
    public Guid Id { get; set; }
}
