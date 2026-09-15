using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Application.Common.Exceptions;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Domain.Entities;

namespace SmartPrompt.Application.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler(
    IApplicationDbContext context,
    ICacheService cacheService) : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException(nameof(Category), request.Id);
        }

        // Optional: Check if category is being used by Prompts
        var hasPrompts = await context.Prompts.AnyAsync(p => p.CategoryId == request.Id, cancellationToken);
        if (hasPrompts)
        {
            throw new ConflictException("Category cannot be deleted because it is referenced by one or more prompts.");
        }

        context.Categories.Remove(category);
        await context.SaveChangesAsync(cancellationToken);

        await cacheService.RemoveAsync("Categories_All", cancellationToken);
    }
}
