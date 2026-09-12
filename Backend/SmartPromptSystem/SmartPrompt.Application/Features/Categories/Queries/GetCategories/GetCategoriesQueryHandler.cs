using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Application.Common.Interfaces;

namespace SmartPrompt.Application.Features.Categories.Queries.GetCategories;

public class GetCategoriesQueryHandler(IApplicationDbContext context) : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await context.Categories
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
    }
}
