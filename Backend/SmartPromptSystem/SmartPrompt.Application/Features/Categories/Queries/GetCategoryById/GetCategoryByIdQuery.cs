using MediatR;
using SmartPrompt.Application.Features.Categories.Queries.GetCategories;

namespace SmartPrompt.Application.Features.Categories.Queries.GetCategoryById;

public record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto>;
