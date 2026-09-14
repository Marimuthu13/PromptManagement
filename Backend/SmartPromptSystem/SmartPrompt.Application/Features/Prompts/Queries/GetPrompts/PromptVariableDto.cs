namespace SmartPrompt.Application.Features.Prompts.Queries.GetPrompts;

public record PromptVariableDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsRequired { get; init; }
}
