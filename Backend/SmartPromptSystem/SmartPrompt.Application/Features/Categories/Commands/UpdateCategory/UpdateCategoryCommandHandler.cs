using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Application.Common.Exceptions;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Application.Features.Categories.Queries.GetCategories;
using SmartPrompt.Domain.Entities;

namespace SmartPrompt.Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Category), request.Id);
        }

        if (entity.Name != request.Name)
        {
            var exists = await context.Categories
                .AnyAsync(c => c.Name == request.Name, cancellationToken);

            if (exists)
            {
                throw new ConflictException("A category with this name already exists.");
            }
        }

        entity.Name = request.Name;
        entity.Description = request.Description;

        await context.SaveChangesAsync(cancellationToken);

        return new CategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
    }
}
