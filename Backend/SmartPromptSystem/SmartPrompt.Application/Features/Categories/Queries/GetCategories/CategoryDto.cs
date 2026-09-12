namespace SmartPrompt.Application.Features.Categories.Queries.GetCategories;

public record CategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? UpdatedAtUtc { get; init; }
}
