using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Application.Common.Interfaces;

namespace SmartPrompt.Application.Features.Categories.Queries.GetCategories;

public class GetCategoriesQueryHandler(
    IApplicationDbContext context,
    ICacheService cacheService) : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private const string CacheKey = "Categories_All";

    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var cachedCategories = await cacheService.GetAsync<List<CategoryDto>>(CacheKey, cancellationToken);
        if (cachedCategories != null)
        {
            return cachedCategories;
        }

        var categories = await context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CreatedAtUtc = c.CreatedAtUtc,
                UpdatedAtUtc = c.UpdatedAtUtc
            })
            .ToListAsync(cancellationToken);

        await cacheService.SetAsync(CacheKey, categories, TimeSpan.FromHours(1), cancellationToken);

        return categories;
    }
}
