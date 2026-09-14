namespace SmartPrompt.Infrastructure.AI;

public class OpenAIConfiguration
{
    public const string SectionName = "OpenAI";
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.openai.com/v1/";
    public string DefaultModel { get; set; } = "gpt-4o";
}
