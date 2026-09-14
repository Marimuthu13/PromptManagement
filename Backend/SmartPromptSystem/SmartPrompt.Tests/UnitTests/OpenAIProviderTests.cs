using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using SmartPrompt.Application.Common.Exceptions;
using SmartPrompt.Application.Common.Models.AI;
using SmartPrompt.Infrastructure.AI;
using Xunit;

namespace SmartPrompt.Tests.UnitTests;

public class OpenAIProviderTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly IOptions<OpenAIConfiguration> _options;

    public OpenAIProviderTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _options = Options.Create(new OpenAIConfiguration
        {
            ApiKey = "test-key",
            BaseUrl = "https://api.openai.com/v1/",
            DefaultModel = "gpt-4"
        });
    }

    private OpenAIProvider CreateProvider()
    {
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(_options.Value.BaseUrl)
        };
        return new OpenAIProvider(httpClient, _options);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecutePromptAsync_SuccessfulResponse_ReturnsAIResponse()
    {
        // Arrange
        var provider = CreateProvider();
        var request = new AIRequest
        {
            SystemPrompt = "System",
            UserPrompt = "User"
        };

        var mockResponse = new
        {
            model = "gpt-4",
            choices = new[]
            {
                new { message = new { content = "Hello AI" } }
            },
            usage = new { prompt_tokens = 10, completion_tokens = 5, total_tokens = 15 }
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(mockResponse))
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        var result = await provider.ExecutePromptAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Hello AI", result.Content);
        Assert.Equal("OpenAI", result.Provider);
        Assert.Equal("gpt-4", result.Model);
        Assert.Equal(10, result.PromptTokens);
        Assert.Equal(5, result.CompletionTokens);
        Assert.Equal(15, result.TotalTokens);
        Assert.True(result.Duration > TimeSpan.Zero);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecutePromptAsync_HttpError_ThrowsAIProviderException()
    {
        // Arrange
        var provider = CreateProvider();
        var request = new AIRequest { UserPrompt = "Test" };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("Invalid API key")
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AIProviderException>(() => provider.ExecutePromptAsync(request, CancellationToken.None));
        Assert.Contains("Unauthorized", ex.Message);
        Assert.Contains("Invalid API key", ex.Message);
    }
}
