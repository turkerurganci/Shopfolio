using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Storefront;

using CatalogImage = Shopfolio.Modules.Catalog.Image;
using ProductImageLink = Shopfolio.Modules.Catalog.ProductImage;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The archived product's page E-05 on the opened application (T549 KK1-KK8; 07 UC-04, 4.5.1; 04 5.5): the same address as the product page answers 200 with the name, the main image, the line that the
/// product is not sold any more and the ways out - and nothing that sells; the page is in no list, no category, no search result and no site map, and is closed to the search engines; a visitor and an
/// administrator see the same page; an add to the cart is the 404; a product published again gets E-04 at the same address and a deleted one the 404.
/// </summary>
public sealed partial class StorefrontSearchAndProductTests
{
    private const decimal ArchivedPrice = 7351.25m;

    private static string MainOf(Reply reply)
    {
        int start = reply.Body.IndexOf("<main", StringComparison.Ordinal);
        int end = reply.Body.IndexOf("</main>", StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start, "The page has no main part.");
        return reply.Body[start..end];
    }

    private async Task<Guid> AddImageAsync(Guid productId, int order, bool isMain, string? altText)
    {
        int n = Interlocked.Increment(ref _counter);
        CatalogImage image = CatalogImage.Create($"arsiv-{n:D4}", ImageFormat.WebP, 800, 600, Now);
        ProductImageLink link = ProductImageLink.Create(productId, image.Id, order, isMain, Now);
        link.AltText = altText;
        await using AppDbContext db = NewContext();
        db.Images.Add(image);
        db.ProductImages.Add(link);
        await db.SaveChangesAsync(Ct);
        return image.Id;
    }

    private async Task SetStatusAsync(Guid productId, PublishStatus status)
    {
        await using AppDbContext db = NewContext();
        await db.Products.Where(p => p.Id == productId).ExecuteUpdateAsync(s => s.SetProperty(p => p.PublishStatus, status), Ct);
    }

    private async Task DeleteProductAsync(Guid productId)
    {
        await using AppDbContext db = NewContext();
        await db.ProductImages.Where(p => p.ProductId == productId).ExecuteDeleteAsync(Ct);
        await db.Variants.Where(v => v.ProductId == productId).ExecuteDeleteAsync(Ct);
        await db.Products.Where(p => p.Id == productId).ExecuteDeleteAsync(Ct);
    }

    private async Task<string> SeedCategoryOfAsync(string name, params Guid[] productIds)
    {
        int n = Interlocked.Increment(ref _counter);
        Category category = Category.Create(name, $"arsiv-kategori-{n:D3}", null, 1, Now.AddMinutes(n));
        await using AppDbContext db = NewContext();
        db.Categories.Add(category);
        await db.SaveChangesAsync(Ct);
        foreach (Guid productId in productIds)
        {
            db.ProductCategories.Add(ProductCategory.Create(productId, category.Id, true, Now));
        }

        await db.SaveChangesAsync(Ct);
        return category.Slug;
    }

    // ---- KK1: 200 and E-05 at the product's own address ----

    [Fact]
    public async Task AnArchivedProduct_IsE05_AtTheSameAddress_With200_AndThereIsNoOtherAddress_T549_KK1()
    {
        SeededProduct archived = await SeedAsync("Eski masa", PublishStatus.Archived, variants: new VariantSpec(Price: ArchivedPrice));
        SeededProduct live = await SeedAsync("Yeni masa", variants: new VariantSpec(Price: ArchivedPrice));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply page = await GetAsync(client, $"/urun/{archived.Slug}");
        Reply published = await GetAsync(client, $"/urun/{live.Slug}");
        Reply viaSelection = await GetAsync(client, $"/urun/{archived.Slug}?firstOptionValue=0&secondOptionValue=0");

        Assert.Equal(HttpStatusCode.OK, page.Status);
        Assert.Null(page.Location);
        Assert.Contains("<h1 class=\"product__name\">Eski masa</h1>", page.Body, StringComparison.Ordinal);
        Assert.Contains($"<p class=\"product__retired\">{ProductView.NoLongerSoldText}</p>", WebUtility.HtmlDecode(page.Body), StringComparison.Ordinal);

        // The product page of a published product is E-04 (the contrast): the line is not on it.
        Assert.Contains("Sepete ekle", published.Text, StringComparison.Ordinal);
        Assert.DoesNotContain(ProductView.NoLongerSoldText, published.Text, StringComparison.Ordinal);

        // A selection address of the archived page is the same page.
        Assert.Equal(HttpStatusCode.OK, viaSelection.Status);
        Assert.Contains(ProductView.NoLongerSoldText, viaSelection.Text, StringComparison.Ordinal);
        Assert.True(IsClosed(viaSelection));
    }

    // ---- KK2: the three pieces of information and the two ways out ----

    [Fact]
    public async Task TheArchivedPage_HasTheName_TheMainImage_TheLine_AndTheWaysOut_AndNothingElse_T549_KK2()
    {
        SeededProduct archived = await SeedAsync("Eski masa", PublishStatus.Archived);
        SeededProduct live = await SeedAsync("Yeni masa");
        Guid mainImage = await AddImageAsync(archived.ProductId, 1, true, "Eski masanın görseli");
        Guid otherImage = await AddImageAsync(archived.ProductId, 2, false, "Eski masanın yan görünüşü");
        await using (AppDbContext db = NewContext())
        {
            await db.Database.ExecuteSqlRawAsync("UPDATE products SET description = {0} WHERE id = {1}", ["Eski masanın uzun açıklaması", archived.ProductId], Ct);
        }

        string categorySlug = await SeedCategoryOfAsync("Mobilya", live.ProductId, archived.ProductId);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply page = await GetAsync(client, $"/urun/{archived.Slug}");
        string content = MainOf(page);

        Assert.Equal(HttpStatusCode.OK, page.Status);

        // The three pieces of information: the name, the main image with its alternative text, the line.
        Assert.Contains("<h1 class=\"product__name\">Eski masa</h1>", content, StringComparison.Ordinal);
        Assert.Contains($"src=\"{FrameStyle.ImagePath(mainImage)}\"", content, StringComparison.Ordinal);
        Assert.Contains("Eski masanın görseli", WebUtility.HtmlDecode(content), StringComparison.Ordinal);
        Assert.Contains(ProductView.NoLongerSoldText, WebUtility.HtmlDecode(content), StringComparison.Ordinal);

        // The two ways out: home, and the home page's own first-level category.
        Assert.Contains($"<a href=\"{FramePaths.Home}\">{NotFoundTexts.HomeLabel}</a>", WebUtility.HtmlDecode(content), StringComparison.Ordinal);
        Assert.Contains($"<a href=\"/kategori/{categorySlug}\">Mobilya</a>", content, StringComparison.Ordinal);

        // Nothing else: no other image, no description, no path from the root, no delivery lines, no share button, no script.
        Assert.DoesNotContain(FrameStyle.ImagePath(otherImage), content, StringComparison.Ordinal);
        Assert.DoesNotContain("yan görünüşü", WebUtility.HtmlDecode(content), StringComparison.Ordinal);
        Assert.DoesNotContain("uzun açıklaması", WebUtility.HtmlDecode(content), StringComparison.Ordinal);
        Assert.DoesNotContain("product__description", content, StringComparison.Ordinal);
        Assert.DoesNotContain("breadcrumb", content, StringComparison.Ordinal);
        Assert.DoesNotContain("product__thumbs", content, StringComparison.Ordinal);
        Assert.DoesNotContain("class=\"delivery", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Paylaş", WebUtility.HtmlDecode(content), StringComparison.Ordinal);
        Assert.DoesNotContain("product.js", page.Body, StringComparison.Ordinal);
    }

    // ---- KK3: no price, no variant data, no form, no primary button ----

    [Fact]
    public async Task TheArchivedPage_HasNoPrice_NoVariantData_NoForm_AndNoPrimaryButton_T549_KK3()
    {
        SeededProduct archived = await SeedAsync(
            "Eski masa",
            PublishStatus.Archived,
            variants: [new VariantSpec(Price: ArchivedPrice, First: 1, Second: 1), new VariantSpec(Price: ArchivedPrice + 1m, First: 2, Second: 1)]);
        await AddImageAsync(archived.ProductId, 1, true, "Eski masa");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply page = await GetAsync(client, $"/urun/{archived.Slug}");

        Assert.Equal(HttpStatusCode.OK, page.Status);
        foreach (decimal amount in new[] { ArchivedPrice, ArchivedPrice + 1m })
        {
            Assert.DoesNotContain(DisplayFormat.Amount(amount), page.Text, StringComparison.Ordinal);
            Assert.DoesNotContain(amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), page.Body, StringComparison.Ordinal);
        }

        foreach (string variantSku in new[] { $"SKU-{archived.Slug[^4..]}-0", $"SKU-{archived.Slug[^4..]}-1" })
        {
            Assert.DoesNotContain(variantSku, page.Body, StringComparison.Ordinal);
        }

        foreach (Guid variantId in archived.VariantIds)
        {
            Assert.DoesNotContain(variantId.ToString("D"), page.Body, StringComparison.Ordinal);
        }

        // No variant form, no add-to-cart form, no button of any kind in the product part, no data for the script, no structured data (price and stock state).
        string content = MainOf(page);
        Assert.DoesNotContain("<form", content, StringComparison.Ordinal);
        Assert.DoesNotContain("<button", content, StringComparison.Ordinal);
        Assert.DoesNotContain("button--primary", content, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex("data-variants=\"[^\"]+\""), page.Body);
        Assert.DoesNotContain("price-block", page.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("AddCartItem", page.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("Sepete ekle", page.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("application/ld+json", page.Body, StringComparison.Ordinal);
    }

    // ---- KK4: in no list, no category, no search result and no site map; noindex ----

    [Fact]
    public async Task TheArchivedPage_IsInNoCategoryNoSearchNoSiteMap_AndIsNoindex_T549_KK4()
    {
        SeededProduct archived = await SeedAsync("Arsivli sandalye", PublishStatus.Archived);
        SeededProduct live = await SeedAsync("Yayinda sandalye");
        string categorySlug = await SeedCategoryOfAsync("Sandalyeler", live.ProductId, archived.ProductId);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply page = await GetAsync(client, $"/urun/{archived.Slug}");
        Reply category = await GetAsync(client, $"/kategori/{categorySlug}");
        Reply search = await GetAsync(client, "/arama?search=sandalye");
        Reply home = await GetAsync(client, "/");
        Reply map = await GetAsync(client, "/sitemap.xml");

        Assert.Equal(HttpStatusCode.OK, page.Status);
        Assert.True(IsClosed(page));
        Assert.DoesNotContain("rel=\"canonical\"", page.Body, StringComparison.Ordinal);

        // The published product of the same category is listed everywhere; the archived one nowhere (the contrast shows the lists were read).
        foreach (Reply list in new[] { category, search })
        {
            Assert.Equal(HttpStatusCode.OK, list.Status);
            Assert.Contains($"/urun/{live.Slug}\"", list.Body, StringComparison.Ordinal);
            Assert.DoesNotContain($"/urun/{archived.Slug}\"", list.Body, StringComparison.Ordinal);
        }

        Assert.DoesNotContain($"/urun/{archived.Slug}\"", home.Body, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, map.Status);
        Assert.DoesNotContain($"/urun/{archived.Slug}", map.Body, StringComparison.Ordinal);
        Assert.Contains($"/urun/{live.Slug}", map.Body, StringComparison.Ordinal);
    }

    // ---- KK5: the same for a visitor and an administrator; the strip is the frame's ----

    [Fact]
    public async Task TheArchivedPage_IsTheSameForAVisitorAndAnAdministrator_AndTheStripIsTheFrames_T549_KK5()
    {
        SeededProduct archived = await SeedAsync("Eski masa", PublishStatus.Archived);
        await AddImageAsync(archived.ProductId, 1, true, "Eski masa");
        (string adminKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply visitor = await GetAsync(client, $"/urun/{archived.Slug}");
        Reply admin = await GetAsync(client, $"/urun/{archived.Slug}", $"{CookieNames.AdminSession}={adminKey}");

        Assert.Equal(HttpStatusCode.OK, visitor.Status);
        Assert.Equal(HttpStatusCode.OK, admin.Status);
        Assert.Equal(WithoutNonce(MainOf(visitor)), WithoutNonce(MainOf(admin)));
        Assert.DoesNotContain(ProductView.PreviewText, admin.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("Taslak", MainOf(admin), StringComparison.Ordinal);
        Assert.True(IsClosed(visitor));
        Assert.True(IsClosed(admin));

        // The administrator strip is a part of the frame, not of the page: it stands over the administrator's answer only.
        Assert.Contains("Yönetici şeridi", admin.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("Yönetici şeridi", visitor.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("Yönetici şeridi", MainOf(admin), StringComparison.Ordinal);
    }

    // ---- KK6: an add to the cart is the 404 of UC-05 ----

    [Fact]
    public async Task AnAddToTheCart_OfAnArchivedProduct_IsThe404_AndNothingIsWritten_T549_KK6()
    {
        SeededProduct archived = await SeedAsync("Eski masa", PublishStatus.Archived);
        SeededProduct unknown = await SeedAsync("Baska masa");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply refused = await PostAsync(client, AddPath(archived.Slug), null, ("VariantId", archived.VariantIds[0].ToString("D")), ("Quantity", "1"));
        Reply withoutVariant = await PostAsync(client, AddPath(archived.Slug), null, ("Quantity", "1"));
        Reply missing = await PostAsync(client, AddPath("yok-boyle-bir-urun"), null, ("VariantId", unknown.VariantIds[0].ToString("D")), ("Quantity", "1"));

        Assert.Equal(HttpStatusCode.NotFound, refused.Status);
        Assert.Equal(HttpStatusCode.NotFound, withoutVariant.Status);
        Assert.Equal(WithoutNonce(missing.Body), WithoutNonce(refused.Body));
        Assert.Equal(0, await CartCountAsync());
        Assert.Equal(0, await CartQuantityOfAsync(archived.VariantIds[0]));
    }

    // ---- KK7: published again it is E-04 at the same address; deleted for good it is the 404 ----

    [Fact]
    public async Task AProductPublishedAgain_IsE04AtTheSameAddress_AndOneDeletedForGood_IsThe404_T549_KK7()
    {
        SeededProduct product = await SeedAsync("Donen masa", PublishStatus.Archived);
        SeededProduct gone = await SeedAsync("Silinen masa", PublishStatus.Archived);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply archived = await GetAsync(client, $"/urun/{product.Slug}");
        Assert.Contains(ProductView.NoLongerSoldText, archived.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("Sepete ekle", archived.Text, StringComparison.Ordinal);

        await SetStatusAsync(product.ProductId, PublishStatus.Published);
        Reply again = await GetAsync(client, $"/urun/{product.Slug}");
        Assert.Equal(HttpStatusCode.OK, again.Status);
        Assert.Contains("Sepete ekle", again.Text, StringComparison.Ordinal);
        Assert.DoesNotContain(ProductView.NoLongerSoldText, again.Text, StringComparison.Ordinal);
        Assert.False(IsClosed(again));

        Reply beforeDelete = await GetAsync(client, $"/urun/{gone.Slug}");
        Assert.Equal(HttpStatusCode.OK, beforeDelete.Status);
        await DeleteProductAsync(gone.ProductId);
        Reply afterDelete = await GetAsync(client, $"/urun/{gone.Slug}");
        Reply unknown = await GetAsync(client, "/urun/yok-boyle-bir-urun");
        Assert.Equal(HttpStatusCode.NotFound, afterDelete.Status);
        Assert.Equal(WithoutNonce(unknown.Body), WithoutNonce(afterDelete.Body));
    }

    // ---- KK8: no image - the name alone; the page needs no script and no wait ----

    [Fact]
    public async Task AnArchivedProductWithoutAnImage_ShowsTheNameAlone_WithNoImageAndNoGap_T549_KK8()
    {
        SeededProduct archived = await SeedAsync("Gorselsiz masa", PublishStatus.Archived);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply page = await GetAsync(client, $"/urun/{archived.Slug}");
        string content = MainOf(page);

        Assert.Equal(HttpStatusCode.OK, page.Status);
        Assert.Contains("<h1 class=\"product__name\">Gorselsiz masa</h1>", content, StringComparison.Ordinal);
        Assert.Contains(ProductView.NoLongerSoldText, WebUtility.HtmlDecode(content), StringComparison.Ordinal);
        Assert.DoesNotContain("<img", content, StringComparison.Ordinal);
        Assert.DoesNotContain("product__image", content, StringComparison.Ordinal);
        Assert.True(IsClosed(page));
    }
}
