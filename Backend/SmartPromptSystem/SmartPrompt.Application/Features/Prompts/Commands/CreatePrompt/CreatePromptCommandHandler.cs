using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Domain.Entities;
using NotFoundException = SmartPrompt.Application.Common.Exceptions.NotFoundException;

namespace SmartPrompt.Application.Features.Prompts.Commands.CreatePrompt;

public class CreatePromptCommandHandler(IApplicationDbContext context) : IRequestHandler<CreatePromptCommand, Guid>
{
    public async Task<Guid> Handle(CreatePromptCommand request, CancellationToken cancellationToken)
    {
        // Check User existence using AnyAsync per guidelines (maps to 404)
        var userExists = await context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        // Check Category existence using AnyAsync per guidelines (maps to 404)
        var categoryExists = await context.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            throw new NotFoundException(nameof(Category), request.CategoryId);
        }

        var entity = new Prompt
        {
            Title = request.Title,
            Description = request.Description,
            Content = request.Content,
            CategoryId = request.CategoryId,
            UserId = request.UserId
        };

        context.Prompts.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
