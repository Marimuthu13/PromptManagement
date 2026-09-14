using SmartPrompt.Domain.Common;

namespace SmartPrompt.Domain.Entities;

public class PromptTemplate : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsSystemCurated { get; set; } = true;

    // Foreign Keys
    public Guid CategoryId { get; set; }

    // Navigation properties
    public Category Category { get; set; } = null!;
}
