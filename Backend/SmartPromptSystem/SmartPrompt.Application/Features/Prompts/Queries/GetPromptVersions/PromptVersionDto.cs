namespace SmartPrompt.Application.Features.Prompts.Queries.GetPromptVersions;

public class PromptVersionDto
{
    public Guid Id { get; set; }
    public Guid PromptId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int VersionNumber { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
