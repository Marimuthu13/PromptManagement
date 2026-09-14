using SmartPrompt.Domain.Common;

namespace SmartPrompt.Domain.Entities;

public class Prompt : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    // Foreign Keys
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<PromptVariable> Variables { get; private set; } = new List<PromptVariable>();
}
