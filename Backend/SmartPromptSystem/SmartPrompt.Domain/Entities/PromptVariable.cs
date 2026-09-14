using SmartPrompt.Domain.Common;

namespace SmartPrompt.Domain.Entities;

public class PromptVariable : AuditableEntity
{
    public Guid PromptId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsRequired { get; set; }

    public Prompt Prompt { get; set; } = null!;
}
