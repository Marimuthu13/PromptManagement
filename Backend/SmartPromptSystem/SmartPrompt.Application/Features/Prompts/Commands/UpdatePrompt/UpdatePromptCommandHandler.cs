using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Application.Common.Exceptions;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Application.Features.Prompts.Queries.GetPrompts;
using SmartPrompt.Domain.Entities;

namespace SmartPrompt.Application.Features.Prompts.Commands.UpdatePrompt;

public class UpdatePromptCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser) : IRequestHandler<UpdatePromptCommand, PromptDto>
{
    public async Task<PromptDto> Handle(UpdatePromptCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId == null)
        {
            throw new UnauthorizedAccessException();
        }

        var userId = currentUser.UserId.Value;

        var entity = await context.Prompts
            .Include(p => p.Variables)
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.UserId == userId, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Prompt), request.Id);
        }

        if (entity.CategoryId != request.CategoryId)
        {
            var categoryExists = await context.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
            if (!categoryExists)
            {
                throw new NotFoundException(nameof(Category), request.CategoryId);
            }
        }

        entity.Title = request.Title;
        entity.Description = request.Description;
        entity.Content = request.Content;
        entity.CategoryId = request.CategoryId;

        var variableNames = SmartPrompt.Application.Common.Utils.PromptVariableParser.ExtractVariables(request.Content).ToList();
        
        // Remove variables that are no longer in the content
        var toRemove = entity.Variables.Where(v => !variableNames.Contains(v.Name)).ToList();
        foreach (var variable in toRemove)
        {
            entity.Variables.Remove(variable);
        }

        // Add new variables
        var existingNames = entity.Variables.Select(v => v.Name).ToList();
        var toAdd = variableNames.Where(n => !existingNames.Contains(n)).ToList();
        foreach (var name in toAdd)
        {
            entity.Variables.Add(new PromptVariable
            {
                Name = name,
                IsRequired = true
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        return new PromptDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            Content = entity.Content,
            CategoryId = entity.CategoryId,
            UserId = entity.UserId,
            Variables = entity.Variables.Select(v => new PromptVariableDto
            {
                Id = v.Id,
                Name = v.Name,
                IsRequired = v.IsRequired
            }).ToList()
        };
    }
}
