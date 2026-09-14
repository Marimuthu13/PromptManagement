using MediatR;
using SmartPrompt.Application.Features.Prompts.Queries.GetPrompts;

namespace SmartPrompt.Application.Features.Prompts.Queries.GetPromptById;

public record GetPromptByIdQuery(Guid Id) : IRequest<PromptDto>;
