using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Domain.Entities;
using NotFoundException = SmartPrompt.Application.Common.Exceptions.NotFoundException;

namespace SmartPrompt.Application.Features.Prompts.Commands.CreatePrompt;

public class CreatePromptCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser) : IRequestHandler<CreatePromptCommand, Guid>
{
    public async Task<Guid> Handle(CreatePromptCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId == null)
        {
            throw new UnauthorizedAccessException();
        }

        var userId = currentUser.UserId.Value;

        // Check User existence using AnyAsync per guidelines (maps to 404)
        var userExists = await context.Users.AnyAsync(u => u.Id == userId, cancellationToken);
        if (!userExists)
        {
            throw new NotFoundException(nameof(User), userId);
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
            UserId = userId
        };

        context.Prompts.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
