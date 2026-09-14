using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Application.Common.Exceptions;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Application.Features.Prompts.Queries.GetPrompts;
using SmartPrompt.Domain.Entities;

namespace SmartPrompt.Application.Features.Prompts.Queries.GetPromptById;

public class GetPromptByIdQueryHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser) : IRequestHandler<GetPromptByIdQuery, PromptDto>
{
    public async Task<PromptDto> Handle(GetPromptByIdQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId == null)
        {
            throw new UnauthorizedAccessException();
        }

        var userId = currentUser.UserId.Value;

        var prompt = await context.Prompts
            .AsNoTracking()
            .Where(p => p.Id == request.Id && p.UserId == userId)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (prompt == null)
        {
            throw new NotFoundException(nameof(Prompt), request.Id);
        }

        return prompt;
    }
}
