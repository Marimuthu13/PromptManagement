using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Application.Common.Models;

namespace SmartPrompt.Application.Features.Templates.Queries.GetTemplates;

public class GetTemplatesQueryHandler : IRequestHandler<GetTemplatesQuery, PagedResult<TemplateDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTemplatesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<TemplateDto>> Handle(GetTemplatesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PromptTemplates.Include(t => t.Category).AsNoTracking();

        if (request.CategoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == request.CategoryId.Value);
        }

        if (request.IsSystemCurated.HasValue)
        {
            query = query.Where(t => t.IsSystemCurated == request.IsSystemCurated.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            query = query.Where(t => t.Title.Contains(request.SearchText) || 
                                     t.Description.Contains(request.SearchText));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var templates = await query
            .OrderBy(t => t.Title)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TemplateDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Content = t.Content,
                CategoryId = t.CategoryId,
                CategoryName = t.Category.Name,
                IsSystemCurated = t.IsSystemCurated
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<TemplateDto>
        {
            Items = templates,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
