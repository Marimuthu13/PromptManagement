using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Application.Common.Interfaces;

namespace SmartPrompt.Application.Features.Prompts.Queries.GetPrompts;

public class GetPromptsQueryHandler(IApplicationDbContext context) : IRequestHandler<GetPromptsQuery, List<PromptDto>>
{
    public async Task<List<PromptDto>> Handle(GetPromptsQuery request, CancellationToken cancellationToken)
    {
        return await context.Prompts
            .AsNoTracking()
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
