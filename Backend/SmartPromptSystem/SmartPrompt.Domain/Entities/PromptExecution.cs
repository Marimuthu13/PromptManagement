using SmartPrompt.Domain.Common;

namespace SmartPrompt.Domain.Entities;

public class PromptExecution : AuditableEntity
{
    public Guid PromptId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string ResultContent { get; set; } = string.Empty;
    
    public int TokensUsed { get; set; }
    public long DurationMs { get; set; }
    
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }

    // Navigation property
    public Prompt Prompt { get; set; } = null!;
}
