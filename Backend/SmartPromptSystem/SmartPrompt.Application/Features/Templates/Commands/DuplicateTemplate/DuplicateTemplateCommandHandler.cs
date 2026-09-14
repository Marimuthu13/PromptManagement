using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Application.Common.Exceptions;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Application.Common.Utils;
using SmartPrompt.Domain.Entities;

namespace SmartPrompt.Application.Features.Templates.Commands.DuplicateTemplate;

public class DuplicateTemplateCommandHandler : IRequestHandler<DuplicateTemplateCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public DuplicateTemplateCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(DuplicateTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.PromptTemplates
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken);

        if (template == null)
        {
            throw new NotFoundException(nameof(PromptTemplate), request.TemplateId);
        }

        var currentUserId = _currentUser.UserId 
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var newPrompt = new Prompt
        {
            Title = $"{template.Title} (Copy)",
            Description = template.Description,
            Content = template.Content,
            CategoryId = template.CategoryId,
            UserId = currentUserId
        };

        // Extract variables automatically
        var variableNames = PromptVariableParser.ExtractVariables(newPrompt.Content);
        foreach (var variableName in variableNames)
        {
            // By default, make them required
            newPrompt.Variables.Add(new PromptVariable
            {
                Name = variableName,
                IsRequired = true
            });
        }

        _context.Prompts.Add(newPrompt);
        await _context.SaveChangesAsync(cancellationToken);

        return newPrompt.Id;
    }
}
