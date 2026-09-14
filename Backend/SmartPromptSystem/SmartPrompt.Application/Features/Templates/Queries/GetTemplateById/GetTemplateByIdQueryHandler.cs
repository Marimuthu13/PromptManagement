using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Application.Common.Exceptions;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Application.Features.Templates.Queries.GetTemplates;
using SmartPrompt.Domain.Entities;

namespace SmartPrompt.Application.Features.Templates.Queries.GetTemplateById;

public class GetTemplateByIdQueryHandler : IRequestHandler<GetTemplateByIdQuery, TemplateDto>
{
    private readonly IApplicationDbContext _context;

    public GetTemplateByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TemplateDto> Handle(GetTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var template = await _context.PromptTemplates
            .Include(t => t.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (template == null)
        {
            throw new NotFoundException(nameof(PromptTemplate), request.Id);
        }

        return new TemplateDto
        {
            Id = template.Id,
            Title = template.Title,
            Description = template.Description,
            Content = template.Content,
            CategoryId = template.CategoryId,
            CategoryName = template.Category.Name,
            IsSystemCurated = template.IsSystemCurated
        };
    }
}
