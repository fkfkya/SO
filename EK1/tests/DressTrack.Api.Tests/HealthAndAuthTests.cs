using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DressTrack.Api.Endpoints;

namespace DressTrack.Api.Tests;

public class HealthAndAuthTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Health_WithoutToken_ReturnsHealthy()
    {
        var response = await factory.CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Parcels_WithoutToken_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/parcels");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPasswordAndUnknownLogin_ReturnSameResponse()
    {
        var client = factory.CreateClient();

        var wrongPassword = await client.PostAsJsonAsync("/auth/login",
            new LoginRequest("buyer1", "wrong-password"));
        var unknownLogin = await client.PostAsJsonAsync("/auth/login",
            new LoginRequest("no-such-user", "wrong-password"));

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownLogin.StatusCode);
        Assert.Equal(await ReadVisibleErrorAsync(wrongPassword),
            await ReadVisibleErrorAsync(unknownLogin));
    }

    private static async Task<string> ReadVisibleErrorAsync(HttpResponseMessage response)
    {
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        return $"{problem.GetProperty("status")}|{problem.GetProperty("title")}|" +
               $"{problem.GetProperty("detail")}";
    }

    [Fact]
    public async Task Login_WithUnexpectedField_Returns400()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/auth/login",
            new { login = "buyer1", password = ApiFactory.SeedPassword, role = "Operator" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
