namespace SmartPrompt.Application.Common.Models.AI;

public record AIRequest
{
    public string Model { get; init; } = string.Empty;
    public string SystemPrompt { get; init; } = string.Empty;
    public string UserPrompt { get; init; } = string.Empty;
    public float Temperature { get; init; } = 0.7f;
    public string? ImageData { get; init; }
    public string? ImageMimeType { get; init; }
    public bool RequireJson { get; init; } = false;
    public GeminiSchema? JsonSchema { get; init; }
}
