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

        return await context.Prompts
            .AsNoTracking()
            .Where(p => p.UserId == userId)
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
