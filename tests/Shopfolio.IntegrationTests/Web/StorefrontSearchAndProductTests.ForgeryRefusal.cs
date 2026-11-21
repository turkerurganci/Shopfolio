using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

using Shopfolio.Infrastructure.Security;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// T604 D-1843 (07 2.5.4; 04 2.7.3.1; K-1627 (2)): the product page the forgery check's 400 shows again reads the product in the page's own wrapper with the variant the form named and the typed
/// quantity, closed to the search engines, and no cart is opened; an address that holds no product is the GET's 404, not an error.
/// </summary>
public sealed partial class StorefrontSearchAndProductTests
{
    [Fact]
    public async Task TheForgeryRefusal_ShowsTheProductWithTheNamedVariantAndTheTypedQuantity_AndOpensNoCart_D1843()
    {
        SeededProduct product = await SeedAsync(
            "Masa sahte",
            variants:
            [
                new VariantSpec(First: 1, Second: 1),
                new VariantSpec(First: 2, Second: 3),
            ]);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply refused = await PostWithoutTokenAsync(client, AddPath(product.Slug), ("VariantId", product.VariantIds[1].ToString("D")), ("Quantity", "7"));
        Reply missing = await PostWithoutTokenAsync(client, AddPath("no-such-product"), ("VariantId", product.VariantIds[1].ToString("D")), ("Quantity", "1"));

        Assert.Equal(HttpStatusCode.BadRequest, refused.Status);
        Assert.Contains("Masa sahte", refused.Text, StringComparison.Ordinal);
        Assert.Contains($"name=\"VariantId\" value=\"{product.VariantIds[1]:D}\"", refused.Body, StringComparison.Ordinal);
        Assert.Contains("value=\"7\"", refused.Body, StringComparison.Ordinal);
        Assert.Contains(ForgeryProtectionFilter.Message, refused.Text, StringComparison.Ordinal);
        Assert.True(IsClosed(refused));
        Assert.Equal(HttpStatusCode.NotFound, missing.Status);
        Assert.Equal(0, await CartCountAsync());
        refused.Message.Dispose();
        missing.Message.Dispose();
    }

    private static async Task<Reply> PostWithoutTokenAsync(HttpClient client, string path, params (string Name, string Value)[] fields)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, path);
        request.Headers.Add(ForgeryTokenHandler.SkipHeader, "1");
        request.Content = new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value)));
        HttpResponseMessage response = await client.SendAsync(request, Ct);
        return new Reply(response.StatusCode, await response.Content.ReadAsStringAsync(Ct), response);
    }
}
