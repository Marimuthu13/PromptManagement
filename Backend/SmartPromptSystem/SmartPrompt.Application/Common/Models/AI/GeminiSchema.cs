using System.Text.Json.Serialization;

namespace SmartPrompt.Application.Common.Models.AI;

public class GeminiSchema
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "OBJECT";

    [JsonPropertyName("properties")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, GeminiSchemaProperty>? Properties { get; set; }

    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Required { get; set; }

    [JsonPropertyName("items")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GeminiSchemaProperty? Items { get; set; }
}

public class GeminiSchemaProperty
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "STRING";

    [JsonPropertyName("items")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GeminiSchemaProperty? Items { get; set; }
}
