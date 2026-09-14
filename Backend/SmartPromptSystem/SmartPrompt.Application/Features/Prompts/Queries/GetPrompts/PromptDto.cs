namespace SmartPrompt.Application.Features.Prompts.Queries.GetPrompts;

public record PromptDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public Guid UserId { get; init; }
    public List<PromptVariableDto> Variables { get; init; } = new();
}
