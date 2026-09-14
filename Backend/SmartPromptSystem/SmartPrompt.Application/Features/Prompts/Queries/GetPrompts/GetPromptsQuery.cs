using MediatR;
using SmartPrompt.Application.Common.Models;

namespace SmartPrompt.Application.Features.Prompts.Queries.GetPrompts;

public record GetPromptsQuery : PagedRequest, IRequest<PagedResult<PromptDto>>
{
    public Guid? CategoryId { get; init; }
    public string? Search { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
}
