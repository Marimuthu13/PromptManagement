using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Application.Common.Models.AI;

namespace SmartPrompt.Infrastructure.AI;

public class GeminiProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public string ProviderName => "Gemini";

    public GeminiProvider(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new ArgumentException("Gemini API key is not configured.");
    }

    public async Task<AIResponse> ExecutePromptAsync(AIRequest request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var model = string.IsNullOrWhiteSpace(request.Model) ? "gemini-3.6-flash" : request.Model.Trim();
        var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";

        var payload = new
        {
            systemInstruction = string.IsNullOrWhiteSpace(request.SystemPrompt) ? null : new
            {
                parts = new[] { new { text = request.SystemPrompt } }
            },
            contents = new[]
            {
                new
                {
                    parts = new[] { new { text = request.UserPrompt } }
                }
            },
            generationConfig = new
            {
                temperature = request.Temperature
            }
        };

        var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        var response = await _httpClient.PostAsJsonAsync(requestUrl, payload, options, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var safeUrl = requestUrl.Replace(_apiKey, "HIDDEN_KEY");
            throw new Exception($"Gemini API Failed. URL: {safeUrl} | Status: {response.StatusCode} | Details: {errorBody}");
        }

        var result = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: cancellationToken);
        stopwatch.Stop();

        var generatedText = result?.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text").GetString() ?? string.Empty;

        int promptTokens = 0;
        int completionTokens = 0;
        int totalTokens = 0;

        if (result != null && result.RootElement.TryGetProperty("usageMetadata", out var usageMetadata))
        {
            if (usageMetadata.TryGetProperty("promptTokenCount", out var pt)) promptTokens = pt.GetInt32();
            if (usageMetadata.TryGetProperty("candidatesTokenCount", out var ct)) completionTokens = ct.GetInt32();
            if (usageMetadata.TryGetProperty("totalTokenCount", out var tt)) totalTokens = tt.GetInt32();
        }

        return new AIResponse
        {
            Content = generatedText,
            Provider = ProviderName,
            Model = model,
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens,
            TotalTokens = totalTokens,
            Duration = stopwatch.Elapsed
        };
    }
}
