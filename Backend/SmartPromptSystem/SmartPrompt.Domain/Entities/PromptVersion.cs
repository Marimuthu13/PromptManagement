using SmartPrompt.Domain.Common;

namespace SmartPrompt.Domain.Entities;

public class PromptVersion : AuditableEntity
{
    public Guid PromptId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int VersionNumber { get; set; }

    // Navigation property
    public Prompt Prompt { get; set; } = null!;
}
