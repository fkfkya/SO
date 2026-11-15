using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DressTrack.Api.Tests;

public class ParcelAccessTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Buyer1Parcel = "DT-000001";

    [Theory]
    [InlineData("buyer1", new[] { "DT-000001" })]
    [InlineData("buyer2", new[] { "DT-000002", "DT-000003" })]
    [InlineData("seller1", new[] { "DT-000001", "DT-000003" })]
    [InlineData("seller2", new[] { "DT-000002" })]
    [InlineData("operator1", new[] { "DT-000001" })]
    [InlineData("operator2", new[] { "DT-000002", "DT-000003" })]
    public async Task ParcelList_ContainsOnlyParcelsOfUserBinding(string login,
        string[] expectedTrackingNumbers)
    {
        var client = await factory.CreateClientForAsync(login);

        var parcels = await client.GetFromJsonAsync<JsonElement[]>("/parcels");

        var trackingNumbers = parcels!.Select(p => p.GetProperty("trackingNumber").GetString());
        Assert.Equal(expectedTrackingNumbers, trackingNumbers);
    }

    [Theory]
    [InlineData("")]
    [InlineData("/history")]
    public async Task ForeignParcel_ReturnsSameResponseAsMissingParcel(string suffix)
    {
        var parcelId = await FindParcelIdAsync("buyer1", Buyer1Parcel);
        var foreignClient = await factory.CreateClientForAsync("buyer2");

        var foreign = await foreignClient.GetAsync($"/parcels/{parcelId}{suffix}");
        var missing = await foreignClient.GetAsync($"/parcels/{Guid.NewGuid()}{suffix}");

        Assert.Equal(HttpStatusCode.NotFound, foreign.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal(await missing.Content.ReadAsStringAsync(),
            await foreign.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task OwnParcel_IsReturnedWithHistory()
    {
        var parcelId = await FindParcelIdAsync("buyer1", Buyer1Parcel);
        var client = await factory.CreateClientForAsync("buyer1");

        var history = await client.GetFromJsonAsync<JsonElement[]>($"/parcels/{parcelId}/history");

        var statuses = history!.Select(c => c.GetProperty("to").GetString());
        Assert.Equal(["Created", "Shipped", "AwaitingPickup"], statuses);
    }

    [Fact]
    public async Task OperatorView_HasNoPrices_SellerViewHasPrices()
    {
        var parcelId = await FindParcelIdAsync("seller1", Buyer1Parcel);
        var operatorClient = await factory.CreateClientForAsync("operator1");
        var sellerClient = await factory.CreateClientForAsync("seller1");

        var operatorView = await operatorClient.GetStringAsync($"/parcels/{parcelId}");
        var sellerView = await sellerClient.GetStringAsync($"/parcels/{parcelId}");

        Assert.DoesNotContain("\"price\"", operatorView);
        Assert.Contains("\"price\"", sellerView);
    }

    private async Task<string> FindParcelIdAsync(string ownerLogin, string trackingNumber)
    {
        var owner = await factory.CreateClientForAsync(ownerLogin);
        var parcels = await owner.GetFromJsonAsync<JsonElement[]>("/parcels");
        return parcels!
            .Single(p => p.GetProperty("trackingNumber").GetString() == trackingNumber)
            .GetProperty("id").GetString()!;
    }
}
