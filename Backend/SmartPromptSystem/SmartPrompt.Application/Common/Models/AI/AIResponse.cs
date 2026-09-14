namespace SmartPrompt.Application.Common.Models.AI;

public record AIResponse
{
    public string Content { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    
    public int PromptTokens { get; init; }
    public int CompletionTokens { get; init; }
    public int TotalTokens { get; init; }
    
    public TimeSpan Duration { get; init; }
}
