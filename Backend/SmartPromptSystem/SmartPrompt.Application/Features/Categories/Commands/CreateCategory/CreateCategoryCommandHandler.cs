using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Domain.Entities;
using SmartPrompt.Application.Common.Exceptions;

namespace SmartPrompt.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateCategoryCommand, Guid>
{
    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        // Primary check for duplicate names (Note: concurrent inserts could still hit the database unique constraint)
        var exists = await context.Categories
            .AnyAsync(c => c.Name == request.Name, cancellationToken);

        if (exists)
        {
            throw new ConflictException("A category with this name already exists.");
        }

        var entity = new Category
        {
            Name = request.Name,
            Description = request.Description
        };

        context.Categories.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
