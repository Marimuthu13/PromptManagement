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

        var parts = new List<object>
        {
            new { text = request.UserPrompt }
        };

        if (!string.IsNullOrWhiteSpace(request.ImageData) && !string.IsNullOrWhiteSpace(request.ImageMimeType))
        {
            var cleanBase64 = request.ImageData;
            if (cleanBase64.Contains(","))
            {
                cleanBase64 = cleanBase64.Substring(cleanBase64.IndexOf(",") + 1);
            }

            parts.Add(new
            {
                inlineData = new
                {
                    mimeType = request.ImageMimeType,
                    data = cleanBase64
                }
            });
        }

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
                    role = "user",
                    parts = parts.ToArray()
                }
            },
            generationConfig = new
            {
                temperature = request.Temperature,
                responseMimeType = request.RequireJson ? "application/json" : null,
                responseSchema = request.RequireJson && request.JsonSchema != null ? request.JsonSchema : null
            }
        };

        var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        
        var rawJson = JsonSerializer.Serialize(payload, options);
        Console.WriteLine("\n=== OUTGOING GEMINI PAYLOAD ===");
        Console.WriteLine(rawJson);
        Console.WriteLine("===============================\n");
        
        var modelsToTry = new List<string> { model };
        if (model == "gemini-1.5-flash")
        {
            modelsToTry.Add("gemini-1.5-pro");
        }
        else if (model == "gemini-3.6-flash" || model == "gemini-3.8-flash")
        {
            modelsToTry.Add("gemini-3.1-pro-preview");
        }

        HttpResponseMessage response = null!;
        int maxRetries = 3;
        
        foreach (var currentModel in modelsToTry)
        {
            int delayMs = 1000;
            var tryUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{currentModel}:generateContent?key={_apiKey}";
            
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    response = await _httpClient.PostAsJsonAsync(tryUrl, payload, options, cancellationToken);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        break;
                    }

                    var statusCode = (int)response.StatusCode;
                    if (statusCode == 503 || statusCode == 429 || statusCode >= 500)
                    {
                        if (i == maxRetries - 1)
                        {
                            break;
                        }
                        
                        await Task.Delay(delayMs, cancellationToken);
                        delayMs *= 2; // Exponential backoff
                    }
                    else
                    {
                        break; // Don't retry on other errors like 400 Bad Request
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"[GeminiProvider] Network error with {currentModel}: {ex.Message}");
                    if (i == maxRetries - 1)
                    {
                        break;
                    }
                    await Task.Delay(delayMs, cancellationToken);
                    delayMs *= 2;
                }
            }

            if (response != null && response.IsSuccessStatusCode)
            {
                model = currentModel;
                break;
            }
            else
            {
                Console.WriteLine($"[GeminiProvider] Model {currentModel} failed. Attempting fallback if available...");
            }
        }
        
        if (response == null || !response.IsSuccessStatusCode)
        {
            var errorBody = response != null ? await response.Content.ReadAsStringAsync(cancellationToken) : "No response or network failure.";
            var safeUrl = requestUrl.Replace(_apiKey, "HIDDEN_KEY");
            throw new Exception($"Gemini API Failed after retries and fallbacks. URL: {safeUrl} | Status: {response?.StatusCode} | Details: {errorBody}");
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
