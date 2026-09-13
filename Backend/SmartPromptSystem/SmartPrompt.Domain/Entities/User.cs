using SmartPrompt.Domain.Common;

namespace SmartPrompt.Domain.Entities;

public class User : AuditableEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Prompt> Prompts { get; set; } = new List<Prompt>();
}
