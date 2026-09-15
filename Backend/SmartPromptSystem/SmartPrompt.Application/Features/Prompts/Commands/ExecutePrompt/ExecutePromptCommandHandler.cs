using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Application.Common.Exceptions;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Application.Common.Models.AI;
using SmartPrompt.Application.Common.Utils;
using SmartPrompt.Application.Features.Prompts.Queries.GetPromptExecutions;
using SmartPrompt.Domain.Entities;

namespace SmartPrompt.Application.Features.Prompts.Commands.ExecutePrompt;

public class ExecutePromptCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IEnumerable<IAIProvider> providers) : IRequestHandler<ExecutePromptCommand, PromptExecutionDto>
{
    public async Task<PromptExecutionDto> Handle(ExecutePromptCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId == null)
        {
            throw new UnauthorizedAccessException();
        }

        var userId = currentUser.UserId.Value;

        var prompt = await context.Prompts
            .Include(p => p.Variables)
            .FirstOrDefaultAsync(p => p.Id == request.PromptId && p.UserId == userId, cancellationToken);

        if (prompt == null)
        {
            throw new NotFoundException(nameof(Prompt), request.PromptId);
        }

        // Validate variables
        foreach (var variable in prompt.Variables)
        {
            if (variable.IsRequired && (!request.Variables.ContainsKey(variable.Name) || string.IsNullOrWhiteSpace(request.Variables[variable.Name])))
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { variable.Name, new[] { $"The variable '{variable.Name}' is required." } }
                });
            }
        }

        // Substitute variables
        var finalContent = PromptVariableParser.SubstituteVariables(prompt.Content, request.Variables);

        // Find provider
        var provider = providers.FirstOrDefault(p => p.ProviderName.Equals(request.ProviderName, StringComparison.OrdinalIgnoreCase));
        if (provider == null)
        {
            throw new NotFoundException(nameof(IAIProvider), request.ProviderName);
        }

        var aiRequest = new AIRequest
        {
            Model = request.ModelName,
            SystemPrompt = "You are a helpful AI assistant.",
            UserPrompt = finalContent,
            Temperature = 0.7f
        };

        var execution = new PromptExecution
        {
            PromptId = prompt.Id,
            Provider = request.ProviderName,
            Model = request.ModelName
        };

        try
        {
            var response = await provider.ExecutePromptAsync(aiRequest, cancellationToken);
            
            execution.IsSuccessful = true;
            execution.ResultContent = response.Content;
            execution.TokensUsed = response.TotalTokens;
            execution.DurationMs = (long)response.Duration.TotalMilliseconds;
        }
        catch (Exception ex)
        {
            execution.IsSuccessful = false;
            execution.ErrorMessage = ex.Message;
            execution.ResultContent = string.Empty;
        }

        context.PromptExecutions.Add(execution);
        await context.SaveChangesAsync(cancellationToken);

        return new PromptExecutionDto
        {
            Id = execution.Id,
            PromptId = execution.PromptId,
            Provider = execution.Provider,
            Model = execution.Model,
            ResultContent = execution.ResultContent,
            TokensUsed = execution.TokensUsed,
            DurationMs = execution.DurationMs,
            IsSuccessful = execution.IsSuccessful,
            ErrorMessage = execution.ErrorMessage,
            CreatedAtUtc = execution.CreatedAtUtc
        };
    }
}
