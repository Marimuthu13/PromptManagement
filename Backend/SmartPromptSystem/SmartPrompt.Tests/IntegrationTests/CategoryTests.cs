using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SmartPrompt.Tests.Infrastructure;
using Xunit;

namespace SmartPrompt.Tests.IntegrationTests;

[Collection("IntegrationTests")]
public class CategoryTests(SmartPromptTestFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateCategory_WithValidData_ReturnsCreated()
    {
        // Arrange
        var token = await AuthHelper.RegisterAndLoginAsync(_client, "CatUser1", $"cat1_{Guid.NewGuid()}@test.com", "Pass123!");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/categories")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = JsonContent.Create(new { Name = $"New Cat {Guid.NewGuid()}", Description = "Test" })
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateCategory_WithDuplicateName_ReturnsConflict()
    {
        // Arrange
        var token = await AuthHelper.RegisterAndLoginAsync(_client, "CatUser2", $"cat2_{Guid.NewGuid()}@test.com", "Pass123!");
        var catName = $"DupCat_{Guid.NewGuid()}";
        var payload = new { Name = catName, Description = "Test" };
        
        var request1 = new HttpRequestMessage(HttpMethod.Post, "/api/categories")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = JsonContent.Create(payload)
        };
        await _client.SendAsync(request1); // First succeeds

        var request2 = new HttpRequestMessage(HttpMethod.Post, "/api/categories")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = JsonContent.Create(payload)
        };

        // Act
        var response = await _client.SendAsync(request2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DeleteCategory_WhenReferencedByPrompt_ReturnsConflict()
    {
        // Arrange
        var token = await AuthHelper.RegisterAndLoginAsync(_client, "CatUser3", $"cat3_{Guid.NewGuid()}@test.com", "Pass123!");
        
        // 1. Create Category
        var catReq = new HttpRequestMessage(HttpMethod.Post, "/api/categories")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = JsonContent.Create(new { Name = $"RefCat_{Guid.NewGuid()}" })
        };
        var catRes = await _client.SendAsync(catReq);
        var catId = (await catRes.Content.ReadFromJsonAsync<CategoryResponse>())!.Id;

        // 2. Create Prompt referencing Category
        var promptReq = new HttpRequestMessage(HttpMethod.Post, "/api/prompts")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = JsonContent.Create(new { Title = "T", Content = "C", CategoryId = catId })
        };
        await _client.SendAsync(promptReq);

        // 3. Attempt to delete category
        var delReq = new HttpRequestMessage(HttpMethod.Delete, $"/api/categories/{catId}")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
        };

        // Act
        var response = await _client.SendAsync(delReq);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private class CategoryResponse
    {
        public Guid Id { get; set; }
    }
}
