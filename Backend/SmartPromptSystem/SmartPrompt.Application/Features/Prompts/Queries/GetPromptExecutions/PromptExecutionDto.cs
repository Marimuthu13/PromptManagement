namespace SmartPrompt.Application.Features.Prompts.Queries.GetPromptExecutions;

public class PromptExecutionDto
{
    public Guid Id { get; set; }
    public Guid PromptId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string ResultContent { get; set; } = string.Empty;
    public int TokensUsed { get; set; }
    public long DurationMs { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
