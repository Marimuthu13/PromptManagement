using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Application.Common.Models;

namespace SmartPrompt.Application.Features.Prompts.Queries.GetPrompts;

public class GetPromptsQueryHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser) : IRequestHandler<GetPromptsQuery, PagedResult<PromptDto>>
{
    public async Task<PagedResult<PromptDto>> Handle(GetPromptsQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId == null)
        {
            throw new UnauthorizedAccessException();
        }

        var userId = currentUser.UserId.Value;

        var query = context.Prompts
            .AsNoTracking()
            .Where(p => p.UserId == userId);

        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(p => p.Title.Contains(request.Search) || p.Content.Contains(request.Search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Sorting
        query = request.SortBy?.ToLowerInvariant() switch
        {
            "category" => request.SortDescending ? query.OrderByDescending(p => p.CategoryId) : query.OrderBy(p => p.CategoryId),
            _ => request.SortDescending ? query.OrderByDescending(p => p.Title) : query.OrderBy(p => p.Title)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PromptDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Content = p.Content,
                CategoryId = p.CategoryId,
                UserId = p.UserId,
                Variables = p.Variables.Select(v => new PromptVariableDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    IsRequired = v.IsRequired
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<PromptDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
