using MediatR;

namespace SmartPrompt.Application.Features.Templates.Commands.DuplicateTemplate;

public class DuplicateTemplateCommand : IRequest<Guid>
{
    public Guid TemplateId { get; set; }
}
