using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Application.Common.Models;

namespace SmartPrompt.Application.Features.Templates.Queries.GetTemplates;

public class GetTemplatesQueryHandler(
    IApplicationDbContext context,
    ICacheService cacheService) : IRequestHandler<GetTemplatesQuery, PagedResult<TemplateDto>>
{
    public async Task<PagedResult<TemplateDto>> Handle(GetTemplatesQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"Templates_{request.CategoryId}_{request.IsSystemCurated}_{request.SearchText}_{request.Page}_{request.PageSize}";

        var cachedResult = await cacheService.GetAsync<PagedResult<TemplateDto>>(cacheKey, cancellationToken);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        var query = context.Prompts.Include(p => p.Category).AsNoTracking().Where(p => p.IsTemplate);

        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            query = query.Where(p => p.Title.Contains(request.SearchText) || 
                                     p.Description.Contains(request.SearchText));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var templates = await query
            .OrderBy(p => p.Title)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new TemplateDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Content = p.Content,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                IsSystemCurated = true
            })
            .ToListAsync(cancellationToken);

        var result = new PagedResult<TemplateDto>
        {
            Items = templates,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };

        await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15), cancellationToken);

        return result;
    }
}
