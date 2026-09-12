using SmartPrompt.Domain.Common;

namespace SmartPrompt.Domain.Entities;

public class Category : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Prompt> Prompts { get; set; } = new List<Prompt>();
}
