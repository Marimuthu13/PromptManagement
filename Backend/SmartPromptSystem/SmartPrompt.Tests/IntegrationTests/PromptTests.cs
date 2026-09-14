using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SmartPrompt.Tests.Infrastructure;
using Xunit;

namespace SmartPrompt.Tests.IntegrationTests;

[Collection("IntegrationTests")]
public class PromptTests(SmartPromptTestFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetPrompt_AsDifferentUser_ReturnsNotFound()
    {
        // Arrange
        var tokenA = await AuthHelper.RegisterAndLoginAsync(_client, "UserA", $"ua_{Guid.NewGuid()}@test.com", "Pass123!");
        var tokenB = await AuthHelper.RegisterAndLoginAsync(_client, "UserB", $"ub_{Guid.NewGuid()}@test.com", "Pass123!");

        // User A creates Prompt
        var promptReq = new HttpRequestMessage(HttpMethod.Post, "/api/prompts")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", tokenA) },
            Content = JsonContent.Create(new { Title = "User A Prompt", Content = "Secret" })
        };
        var promptRes = await _client.SendAsync(promptReq);
        var promptId = (await promptRes.Content.ReadFromJsonAsync<PromptResponse>())!.Id;

        // Act - User B attempts to read it
        var readReq = new HttpRequestMessage(HttpMethod.Get, $"/api/prompts/{promptId}")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", tokenB) }
        };
        var response = await _client.SendAsync(readReq);

        // Assert - Should return 404 to hide resource existence
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPrompts_WithCategoryFilter_ReturnsOnlyMatchingUserPrompts()
    {
        // Arrange
        var tokenA = await AuthHelper.RegisterAndLoginAsync(_client, "UserAFilt", $"uaf_{Guid.NewGuid()}@test.com", "Pass123!");
        var tokenB = await AuthHelper.RegisterAndLoginAsync(_client, "UserBFilt", $"ubf_{Guid.NewGuid()}@test.com", "Pass123!");

        // Setup Category
        var catReq = new HttpRequestMessage(HttpMethod.Post, "/api/categories")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", tokenA) },
            Content = JsonContent.Create(new { Name = $"FiltCat_{Guid.NewGuid()}" })
        };
        var catId = (await (await _client.SendAsync(catReq)).Content.ReadFromJsonAsync<CategoryResponse>())!.Id;

        // User A creates prompt in category
        var pReqA = new HttpRequestMessage(HttpMethod.Post, "/api/prompts")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", tokenA) },
            Content = JsonContent.Create(new { Title = "Prompt A", Content = "C", CategoryId = catId })
        };
        await _client.SendAsync(pReqA);

        // User B creates prompt in same category
        var pReqB = new HttpRequestMessage(HttpMethod.Post, "/api/prompts")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", tokenB) },
            Content = JsonContent.Create(new { Title = "Prompt B", Content = "C", CategoryId = catId })
        };
        await _client.SendAsync(pReqB);

        // Act - User A requests all prompts in category
        var listReq = new HttpRequestMessage(HttpMethod.Get, $"/api/prompts?categoryId={catId}")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", tokenA) }
        };
        var response = await _client.SendAsync(listReq);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var prompts = await response.Content.ReadFromJsonAsync<List<PromptListResponse>>();
        prompts.Should().NotBeNull();
        prompts.Should().ContainSingle(); // Should ONLY contain User A's prompt
        prompts![0].Title.Should().Be("Prompt A");
    }

    private class PromptResponse { public Guid Id { get; set; } }
    private class PromptListResponse { public string Title { get; set; } = string.Empty; }
    private class CategoryResponse { public Guid Id { get; set; } }
}
