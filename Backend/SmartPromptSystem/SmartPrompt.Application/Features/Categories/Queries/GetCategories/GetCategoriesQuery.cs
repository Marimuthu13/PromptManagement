using MediatR;

namespace SmartPrompt.Application.Features.Categories.Queries.GetCategories;

public record GetCategoriesQuery : IRequest<List<CategoryDto>>
{
}
