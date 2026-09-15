using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Application.Common.Exceptions;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Domain.Entities;

namespace SmartPrompt.Application.Features.Prompts.Queries.GetPromptVersions;

public class GetPromptVersionsQueryHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser) : IRequestHandler<GetPromptVersionsQuery, List<PromptVersionDto>>
{
    public async Task<List<PromptVersionDto>> Handle(GetPromptVersionsQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId == null)
        {
            throw new UnauthorizedAccessException();
        }

        var userId = currentUser.UserId.Value;

        var prompt = await context.Prompts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PromptId && p.UserId == userId, cancellationToken);

        if (prompt == null)
        {
            throw new NotFoundException(nameof(Prompt), request.PromptId);
        }

        var versions = await context.PromptVersions
            .AsNoTracking()
            .Where(v => v.PromptId == request.PromptId)
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => new PromptVersionDto
            {
                Id = v.Id,
                PromptId = v.PromptId,
                Content = v.Content,
                VersionNumber = v.VersionNumber,
                CreatedAtUtc = v.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return versions;
    }
}
