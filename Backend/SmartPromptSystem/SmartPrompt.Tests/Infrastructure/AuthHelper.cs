using System.Net.Http.Json;

namespace SmartPrompt.Tests.Infrastructure;

public static class AuthHelper
{
    public static async Task<string> RegisterAndLoginAsync(HttpClient client, string username, string email, string password)
    {
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = username,
            Email = email,
            Password = password
        });

        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = email,
            Password = password
        });

        loginResponse.EnsureSuccessStatusCode();

        var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthResult>();
        return authResult!.AccessToken;
    }

    private class AuthResult
    {
        public string AccessToken { get; set; } = string.Empty;
    }
}
