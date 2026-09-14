using MediatR;
using SmartPrompt.Application.Common.Models;

namespace SmartPrompt.Application.Features.Templates.Queries.GetTemplates;

public record GetTemplatesQuery : PagedRequest, IRequest<PagedResult<TemplateDto>>
{
    public Guid? CategoryId { get; set; }
    public bool? IsSystemCurated { get; set; }
    public string? SearchText { get; set; }
}
