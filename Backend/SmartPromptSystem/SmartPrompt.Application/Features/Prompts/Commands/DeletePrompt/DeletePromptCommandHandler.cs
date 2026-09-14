using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Application.Common.Exceptions;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Domain.Entities;

namespace SmartPrompt.Application.Features.Prompts.Commands.DeletePrompt;

public class DeletePromptCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser) : IRequestHandler<DeletePromptCommand>
{
    public async Task Handle(DeletePromptCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId == null)
        {
            throw new UnauthorizedAccessException();
        }

        var userId = currentUser.UserId.Value;

        var entity = await context.Prompts
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.UserId == userId, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Prompt), request.Id);
        }

        context.Prompts.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
