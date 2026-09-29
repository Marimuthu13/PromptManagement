using SmartPrompt.Domain.Common;

namespace SmartPrompt.Domain.Entities;

public class PromptExecutionLog : AuditableEntity
{
    public Guid? PromptId { get; set; }
    public DateTime ExecutedAt { get; set; }
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }

    public string FeatureType { get; set; } = "Prompt Execution";
    public string? DisplayTitle { get; set; }
    public string Status { get; set; } = "Success";
    public bool IsFallback { get; set; } = false;
    public string? ModelUsed { get; set; }
    public long LatencyMs { get; set; }

    public Prompt? Prompt { get; set; }
}
