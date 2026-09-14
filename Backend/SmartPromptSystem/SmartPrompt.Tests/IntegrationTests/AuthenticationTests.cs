using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SmartPrompt.Tests.Infrastructure;
using Xunit;

namespace SmartPrompt.Tests.IntegrationTests;

[Collection("IntegrationTests")]
public class AuthenticationTests(SmartPromptTestFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_WithValidData_ReturnsCreated()
    {
        // Arrange
        var request = new
        {
            Username = "AuthTestUser",
            Email = $"authtest_{Guid.NewGuid()}@test.com",
            Password = "SecurePassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain("SecurePassword123!"); // Ensure password is not leaked
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflictOrBadRequest()
    {
        // Arrange
        var email = $"dup_{Guid.NewGuid()}@test.com";
        var request = new { Username = "U1", Email = email, Password = "P1" };
        await _client.PostAsJsonAsync("/api/auth/register", request); // First time works

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request); // Second time fails

        // Assert
        response.StatusCode.Should().Match(s => s == HttpStatusCode.BadRequest || s == HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkAndToken()
    {
        // Arrange
        var email = $"login_{Guid.NewGuid()}@test.com";
        await AuthHelper.RegisterAndLoginAsync(_client, "LoginUser", email, "Pass123!");

        var request = new
        {
            Email = email,
            Password = "Pass123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResult>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrWhiteSpace();
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain("Pass123!");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var email = $"invalidpwd_{Guid.NewGuid()}@test.com";
        await AuthHelper.RegisterAndLoginAsync(_client, "User", email, "Pass123!");

        var request = new
        {
            Email = email,
            Password = "WrongPassword!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Unauthenticated_AccessToProtectedEndpoint_ReturnsUnauthorized()
    {
        // Arrange
        // (No JWT token applied to client)

        // Act
        var response = await _client.PostAsJsonAsync("/api/prompts", new { Title = "Test", Content = "Test" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private class LoginResult
    {
        public string AccessToken { get; set; } = string.Empty;
    }
}
