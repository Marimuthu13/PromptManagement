using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Application.Common.Exceptions;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Application.Common.Utils;
using SmartPrompt.Application.Features.Prompts.Queries.GetPrompts;
using SmartPrompt.Domain.Entities;

namespace SmartPrompt.Application.Features.Prompts.Commands.RestorePromptVersion;

public class RestorePromptVersionCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser) : IRequestHandler<RestorePromptVersionCommand, PromptDto>
{
    public async Task<PromptDto> Handle(RestorePromptVersionCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId == null)
        {
            throw new UnauthorizedAccessException();
        }

        var userId = currentUser.UserId.Value;

        var prompt = await context.Prompts
            .Include(p => p.Variables)
            .FirstOrDefaultAsync(p => p.Id == request.PromptId && p.UserId == userId, cancellationToken);

        if (prompt == null)
        {
            throw new NotFoundException(nameof(Prompt), request.PromptId);
        }

        var version = await context.PromptVersions
            .FirstOrDefaultAsync(v => v.Id == request.VersionId && v.PromptId == request.PromptId, cancellationToken);

        if (version == null)
        {
            throw new NotFoundException(nameof(PromptVersion), request.VersionId);
        }

        if (prompt.Content != version.Content)
        {
            var lastVersionNumber = await context.PromptVersions
                .Where(v => v.PromptId == prompt.Id)
                .MaxAsync(v => (int?)v.VersionNumber, cancellationToken) ?? 0;

            // Save current content as a version before restoring
            context.PromptVersions.Add(new PromptVersion
            {
                PromptId = prompt.Id,
                Content = prompt.Content,
                VersionNumber = lastVersionNumber + 1
            });

            prompt.Content = version.Content;

            // Update variables
            var variableNames = PromptVariableParser.ExtractVariables(prompt.Content).ToList();
            
            var toRemove = prompt.Variables.Where(v => !variableNames.Contains(v.Name)).ToList();
            foreach (var v in toRemove)
            {
                prompt.Variables.Remove(v);
            }

            var existingNames = prompt.Variables.Select(v => v.Name).ToList();
            var toAdd = variableNames.Where(n => !existingNames.Contains(n)).ToList();
            foreach (var name in toAdd)
            {
                prompt.Variables.Add(new PromptVariable
                {
                    Name = name,
                    IsRequired = true
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return new PromptDto
        {
            Id = prompt.Id,
            Title = prompt.Title,
            Description = prompt.Description,
            Content = prompt.Content,
            CategoryId = prompt.CategoryId,
            UserId = prompt.UserId,
            Variables = prompt.Variables.Select(v => new PromptVariableDto
            {
                Id = v.Id,
                Name = v.Name,
                IsRequired = v.IsRequired
            }).ToList()
        };
    }
}
