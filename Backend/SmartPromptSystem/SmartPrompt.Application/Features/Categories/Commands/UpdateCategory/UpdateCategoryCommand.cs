using MediatR;
using SmartPrompt.Application.Features.Categories.Queries.GetCategories;

namespace SmartPrompt.Application.Features.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand : IRequest<CategoryDto>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
