using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using SmartPrompt.Application.Common.Exceptions;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Application.Common.Models.AI;

namespace SmartPrompt.Infrastructure.AI;

public class OpenAIProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly OpenAIConfiguration _configuration;

    public string ProviderName => "OpenAI";

    public OpenAIProvider(HttpClient httpClient, IOptions<OpenAIConfiguration> configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration.Value;

        _httpClient.BaseAddress = new Uri(_configuration.BaseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.ApiKey);
    }

    public async Task<AIResponse> ExecutePromptAsync(AIRequest request, CancellationToken cancellationToken)
    {
        var model = string.IsNullOrWhiteSpace(request.Model) ? _configuration.DefaultModel : request.Model;

        var payload = new
        {
            model = model,
            messages = new[]
            {
                new { role = "system", content = request.SystemPrompt },
                new { role = "user", content = request.UserPrompt }
            },
            temperature = request.Temperature
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await _httpClient.PostAsJsonAsync("chat/completions", payload, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new AIProviderException(ProviderName, $"OpenAI API Error: {response.StatusCode}. Details: {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>(cancellationToken: cancellationToken);
            stopwatch.Stop();

            if (result == null || result.Choices == null || result.Choices.Count == 0)
            {
                throw new AIProviderException(ProviderName, "Invalid response received from OpenAI API.");
            }

            return new AIResponse
            {
                Content = result.Choices[0].Message.Content,
                Provider = ProviderName,
                Model = result.Model,
                PromptTokens = result.Usage?.PromptTokens ?? 0,
                CompletionTokens = result.Usage?.CompletionTokens ?? 0,
                TotalTokens = result.Usage?.TotalTokens ?? 0,
                Duration = stopwatch.Elapsed
            };
        }
        catch (HttpRequestException ex)
        {
            throw new AIProviderException(ProviderName, "Failed to connect to OpenAI API.", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new AIProviderException(ProviderName, "OpenAI API request timed out.", ex);
        }
    }

    private class OpenAIResponse
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("choices")]
        public List<OpenAIChoice> Choices { get; set; } = new();

        [JsonPropertyName("usage")]
        public OpenAIUsage? Usage { get; set; }
    }

    private class OpenAIChoice
    {
        [JsonPropertyName("message")]
        public OpenAIMessage Message { get; set; } = new();
    }

    private class OpenAIMessage
    {
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    private class OpenAIUsage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
}
