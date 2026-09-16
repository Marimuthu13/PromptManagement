using SmartPrompt.Domain.Common;

namespace SmartPrompt.Domain.Entities;

public class PromptExecutionLog : AuditableEntity
{
    public Guid PromptId { get; set; }
    public DateTime ExecutedAt { get; set; }
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }

    public Prompt Prompt { get; set; } = null!;
}
