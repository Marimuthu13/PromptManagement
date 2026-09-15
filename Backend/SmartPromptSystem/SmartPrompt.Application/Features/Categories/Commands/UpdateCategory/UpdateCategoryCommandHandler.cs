using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Application.Common.Exceptions;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Application.Features.Categories.Queries.GetCategories;
using SmartPrompt.Domain.Entities;

namespace SmartPrompt.Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler(
    IApplicationDbContext context,
    ICacheService cacheService) : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException(nameof(Category), request.Id);
        }

        if (category.Name != request.Name)
        {
            var exists = await context.Categories
                .AnyAsync(c => c.Name == request.Name, cancellationToken);

            if (exists)
            {
                throw new ConflictException("A category with this name already exists.");
            }
        }

        category.Name = request.Name;
        category.Description = request.Description;

        await context.SaveChangesAsync(cancellationToken);

        await cacheService.RemoveAsync("Categories_All", cancellationToken);

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            CreatedAtUtc = category.CreatedAtUtc,
            UpdatedAtUtc = category.UpdatedAtUtc
        };
    }
}
