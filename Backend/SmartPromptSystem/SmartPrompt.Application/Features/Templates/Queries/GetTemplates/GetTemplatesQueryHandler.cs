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

        var query = context.PromptTemplates.Include(t => t.Category).AsNoTracking();

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
