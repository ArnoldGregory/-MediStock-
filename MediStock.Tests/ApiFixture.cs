using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MediStock.Tests;

[CollectionDefinition("api")]
public class ApiCollection : ICollectionFixture<ApiFixture>
{
}

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
    }
}

public sealed class ApiFixture : IAsyncLifetime
{
    public ApiFactory Factory { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;
    public string AdminToken { get; private set; } = null!;
    public string SuperToken { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        TestDatabase.Provision();
        Environment.SetEnvironmentVariable("MEDISTOCK_DBCONN", TestDatabase.TestConnectionString);

        Factory = new ApiFactory();
        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("http://localhost") });

        AdminToken = await LoginAsync(TestDatabase.AdminEmail);
        SuperToken = await LoginAsync(TestDatabase.SuperEmail);
    }

    public Task DisposeAsync()
    {
        Factory?.Dispose();
        Client?.Dispose();
        Environment.SetEnvironmentVariable("MEDISTOCK_DBCONN", null);
        return Task.CompletedTask;
    }

    public async Task<string> LoginAsync(string email)
    {
        var (status, doc) = await SendAsync(HttpMethod.Post, "api/auth/clientlogin",
            new { username = email, password = TestDatabase.Password });
        if (status != 200) throw new Exception($"Login failed ({status}) for {email}: {doc?.RootElement.GetRawText()}");
        var token = doc!.RootElement.GetProperty("data").GetProperty("accessToken").GetString();
        if (string.IsNullOrEmpty(token)) throw new Exception("Login response had no accessToken");
        return token;
    }

    public HttpRequestMessage Build(HttpMethod method, string url, object? body = null, string? token = null)
    {
        var req = new HttpRequestMessage(method, url);
        if (body != null)
            req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        if (token != null)
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return req;
    }

    public async Task<(int status, JsonDocument? doc)> SendAsync(HttpMethod method, string url, object? body = null, string? token = null)
    {
        using var req = Build(method, url, body, token);
        using var resp = await Client.SendAsync(req);
        int status = (int)resp.StatusCode;
        var raw = await resp.Content.ReadAsStringAsync();
        JsonDocument? doc = null;
        if (!string.IsNullOrWhiteSpace(raw))
        {
            try { doc = JsonDocument.Parse(raw); }
            catch (JsonException) { doc = null; }
        }
        return (status, doc);
    }

    public JsonElement? GetData(JsonDocument? doc)
    {
        if (doc == null) return null;
        return doc.RootElement.TryGetProperty("data", out var d) ? d : null;
    }
}