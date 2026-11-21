using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// T604 D-1843 (07 2.5.4; 04 2.7.3.1): the forgery check's 400 on the product form shows the record of the route's key and the sent entries through the one hook
/// (<see cref="IReadsOnForgeryRefusal"/>) and writes nothing; a key that is no product's is the one 404 (K-800).
/// </summary>
public sealed partial class ProductFormPageTests
{
    [Fact]
    public async Task TheForgeryRefusal_ShowsTheRecordAndTheSentName_WritesNothing_AndAForeignKeyIs404_D1843()
    {
        (Guid productId, Guid[] variants) = await AddProductAsync("Kupa", PublishStatus.Draft, PublishStatus.Draft, (100m, 5));
        string sku = await SkuOfAsync(variants[0]);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string html) = await ForgedPosts.SendAsync(client, $"{ProductFormModel.PathOf(productId)}?handler=SaveProduct", [("Input.Name", "Kupa sahte ad"), ("Input.Version", "1")]);
        (HttpStatusCode foreign, _) = await ForgedPosts.SendAsync(client, $"{ProductFormModel.PathOf(Guid.NewGuid())}?handler=SaveProduct", [("Input.Name", "Kupa sahte ad")]);

        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.Contains("value=\"Kupa sahte ad\"", html, StringComparison.Ordinal);
        Assert.Contains(sku, html, StringComparison.Ordinal);
        Assert.Contains(ForgeryProtectionFilter.Message, html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.NotFound, foreign);
        Assert.Equal("Kupa", await NameOfAsync(productId));
        Assert.Equal(5, await StockOfAsync(variants[0]));
    }
}
