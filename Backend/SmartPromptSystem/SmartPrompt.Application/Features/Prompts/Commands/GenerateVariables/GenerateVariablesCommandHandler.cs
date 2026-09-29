using System.Text.Json;
using MediatR;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Application.Common.Models.AI;
using SmartPrompt.Domain.Entities;

namespace SmartPrompt.Application.Features.Prompts.Commands.GenerateVariables;

public class GenerateVariablesCommandHandler : IRequestHandler<GenerateVariablesCommand, Dictionary<string, string>>
{
    private readonly IAIProvider _aiProvider;
    private readonly IApplicationDbContext _context;

    public GenerateVariablesCommandHandler(IAIProvider aiProvider, IApplicationDbContext context)
    {
        _aiProvider = aiProvider;
        _context = context;
    }

    public async Task<Dictionary<string, string>> Handle(GenerateVariablesCommand request, CancellationToken cancellationToken)
    {
        if (request.VariableNames.Count == 0)
        {
            return new Dictionary<string, string>();
        }

        var properties = new Dictionary<string, GeminiSchemaProperty>();
        foreach (var varName in request.VariableNames)
        {
            properties[varName] = new GeminiSchemaProperty { Type = "STRING" };
        }

        var schema = new GeminiSchema
        {
            Type = "OBJECT",
            Properties = properties,
            Required = request.VariableNames.ToArray()
        };

        var aiRequest = new AIRequest
        {
            Model = "gemini-3.6-flash",
            SystemPrompt = "You are a test-data generator. Given the prompt template and the list of variable names, generate realistic, highly contextual, and cohesive mock data for each variable. Return a JSON object where keys match the variable names and values are strings.",
            UserPrompt = $"Template: {request.PromptContent}",
            Temperature = 0.6f,
            RequireJson = true,
            JsonSchema = schema
        };

        var response = await _aiProvider.ExecutePromptAsync(aiRequest, cancellationToken);

        // Log token usage (Standalone, no PromptId)
        var log = new PromptExecutionLog
        {
            PromptId = null,
            ExecutedAt = DateTime.UtcNow,
            PromptTokens = response.PromptTokens,
            CompletionTokens = response.CompletionTokens,
            TotalTokens = response.TotalTokens,
            FeatureType = "Variable Autofill",
            DisplayTitle = "Autofill: Dynamic Variables",
            Status = "Success",
            IsFallback = false,
            ModelUsed = "gemini-3.6-flash",
            LatencyMs = (long)response.Duration.TotalMilliseconds
        };

        _context.PromptExecutionLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);

        var result = JsonSerializer.Deserialize<Dictionary<string, string>>(response.Content);
        return result ?? new Dictionary<string, string>();
    }
}
