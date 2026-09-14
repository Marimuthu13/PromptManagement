using MediatR;

namespace SmartPrompt.Application.Features.Prompts.Commands.DeletePrompt;

public record DeletePromptCommand(Guid Id) : IRequest;
