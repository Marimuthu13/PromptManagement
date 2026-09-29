using MediatR;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Application.Common.Models.AI;
using SmartPrompt.Domain.Entities;

namespace SmartPrompt.Application.Features.Prompts.Commands.OptimizePrompt;

public class OptimizePromptCommandHandler : IRequestHandler<OptimizePromptCommand, string>
{
    private readonly IAIProvider _aiProvider;
    private readonly IApplicationDbContext _context;

    public OptimizePromptCommandHandler(IAIProvider aiProvider, IApplicationDbContext context)
    {
        _aiProvider = aiProvider;
        _context = context;
    }

    public async Task<string> Handle(OptimizePromptCommand request, CancellationToken cancellationToken)
    {
        var schema = new GeminiSchema
        {
            Type = "OBJECT",
            Properties = new Dictionary<string, GeminiSchemaProperty>
            {
                { "score", new GeminiSchemaProperty { Type = "INTEGER" } },
                { "grade", new GeminiSchemaProperty { Type = "STRING" } },
                { "suggestions", new GeminiSchemaProperty 
                    { 
                        Type = "ARRAY", 
                        Items = new GeminiSchemaProperty { Type = "STRING" } 
                    } 
                },
                { "optimizedPrompt", new GeminiSchemaProperty { Type = "STRING" } }
            },
            Required = new[] { "score", "grade", "suggestions", "optimizedPrompt" }
        };

        var aiRequest = new AIRequest
        {
            Model = "gemini-3.6-flash",
            SystemPrompt = "You are an expert Prompt Engineer evaluator. Analyze the user's draft prompt for clarity, variable usage, and token efficiency. Return your evaluation strictly following the provided JSON schema.",
            UserPrompt = request.DraftContent,
            Temperature = 0.2f, // Lower temperature for more analytical/consistent output
            RequireJson = true,
            JsonSchema = schema
        };

        try
        {
            var response = await _aiProvider.ExecutePromptAsync(aiRequest, cancellationToken);
            
            var log = new PromptExecutionLog
            {
                PromptId = null,
                ExecutedAt = DateTime.UtcNow,
                PromptTokens = response.PromptTokens,
                CompletionTokens = response.CompletionTokens,
                TotalTokens = response.TotalTokens,
                FeatureType = "Prompt Doctor",
                DisplayTitle = "Prompt Doctor: Optimization",
                Status = "Success",
                IsFallback = false,
                ModelUsed = "gemini-3.6-flash",
                LatencyMs = (long)response.Duration.TotalMilliseconds
            };

            _context.PromptExecutionLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);

            return response.Content; // Will be a valid JSON string adhering to the schema
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OptimizePrompt] AI Provider failed: {ex.Message}. Using offline fallback.");
            
            var log = new PromptExecutionLog
            {
                PromptId = null,
                ExecutedAt = DateTime.UtcNow,
                PromptTokens = 0,
                CompletionTokens = 0,
                TotalTokens = 0,
                FeatureType = "Prompt Doctor",
                DisplayTitle = "Prompt Doctor: Optimization",
                Status = "Fallback",
                IsFallback = true,
                ModelUsed = "local-linter",
                LatencyMs = 0
            };

            _context.PromptExecutionLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);

            return GenerateOfflineEvaluation(request.DraftContent);
        }
    }

    private string GenerateOfflineEvaluation(string draftContent)
    {
        int score = 50;
        var suggestions = new List<string> { "AI service temporarily unreachable. Evaluated via local offline linter." };
        
        var lowerContent = draftContent.ToLowerInvariant();
        if (lowerContent.Contains("act as") || lowerContent.Contains("you are"))
        {
            score += 20;
        }
        else
        {
            suggestions.Add("Consider assigning a specific persona or role (e.g., 'Act as an expert...').");
        }

        if (System.Text.RegularExpressions.Regex.IsMatch(draftContent, @"\{\{(.+?)\}\}"))
        {
            score += 10;
        }
        
        var wordCount = draftContent.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        if (wordCount > 10)
        {
            score += 20;
        }
        else
        {
            suggestions.Add("Prompt is very short. Add more context or specific constraints.");
        }

        string grade = score >= 90 ? "A" : score >= 80 ? "B" : score >= 70 ? "C" : score >= 60 ? "D" : "F";

        var fallbackResult = new
        {
            score = score,
            grade = grade,
            suggestions = suggestions,
            optimizedPrompt = draftContent,
            isFallback = true
        };

        return System.Text.Json.JsonSerializer.Serialize(fallbackResult);
    }
}
