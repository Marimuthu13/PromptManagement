using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Application.Common.Interfaces;

namespace SmartPrompt.Application.Features.Prompts.Queries.GetPrompts;

public class GetPromptsQueryHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser) : IRequestHandler<GetPromptsQuery, List<PromptDto>>
{
    public async Task<List<PromptDto>> Handle(GetPromptsQuery request, CancellationToken cancellationToken)
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

        return await query
            .OrderBy(p => p.Title)
            .Select(p => new PromptDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Content = p.Content,
                CategoryId = p.CategoryId,
                UserId = p.UserId
            })
            .ToListAsync(cancellationToken);
    }
}
