using MediatR;

namespace SmartPrompt.Application.Features.Categories.Commands.CreateCategory;

public record CreateCategoryCommand : IRequest<Guid>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
