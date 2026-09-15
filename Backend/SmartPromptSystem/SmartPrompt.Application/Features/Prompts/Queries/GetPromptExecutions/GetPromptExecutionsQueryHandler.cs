using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Application.Common.Exceptions;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Domain.Entities;

namespace SmartPrompt.Application.Features.Prompts.Queries.GetPromptExecutions;

public class GetPromptExecutionsQueryHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser) : IRequestHandler<GetPromptExecutionsQuery, List<PromptExecutionDto>>
{
    public async Task<List<PromptExecutionDto>> Handle(GetPromptExecutionsQuery request, CancellationToken cancellationToken)
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

        var executions = await context.PromptExecutions
            .AsNoTracking()
            .Where(e => e.PromptId == request.PromptId)
            .OrderByDescending(e => e.CreatedAtUtc)
            .Select(e => new PromptExecutionDto
            {
                Id = e.Id,
                PromptId = e.PromptId,
                Provider = e.Provider,
                Model = e.Model,
                ResultContent = e.ResultContent,
                TokensUsed = e.TokensUsed,
                DurationMs = e.DurationMs,
                IsSuccessful = e.IsSuccessful,
                ErrorMessage = e.ErrorMessage,
                CreatedAtUtc = e.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return executions;
    }
}
