using System.Net.Http.Headers;
using System.Net.Http.Json;
using DressTrack.Api.Endpoints;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace DressTrack.Api.Tests;

public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string SeedPassword = "test-seed-password";

    private readonly PostgreSqlContainer _database =
        new PostgreSqlBuilder("postgres:17-alpine").Build();

    public async Task InitializeAsync() => await _database.StartAsync();

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Default", _database.GetConnectionString());
        builder.UseSetting("Seed:Password", SeedPassword);
    }

    public async Task<HttpClient> CreateClientForAsync(string login)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/auth/login",
            new LoginRequest(login, SeedPassword));
        response.EnsureSuccessStatusCode();
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResponse!.Token);
        return client;
    }
}
