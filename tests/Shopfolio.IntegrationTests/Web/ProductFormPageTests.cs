using System.Globalization;
using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Panel;
using Shopfolio.UnitTests.Imaging;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The product form (E-33) on the opened application and a real database (T525 criteria 2, 7, 8, 9; 07 UC-88, UC-115, UC-116; 04 9.5): the new and the existing form, the shown stock and the reserved
/// units, the gate's unmet conditions, the absolute publish moves, the last Published variant's refusal, the deletion's confirmation state and the deletion itself.
/// </summary>
[Collection(ProductListPageTestsDefinition.Name)]
public sealed class ProductFormPageTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Base = new(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _database;

    private string _adminKey = string.Empty;

    public ProductFormPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ListSeed.ClearAsync(_database.Dsn);
        _adminKey = (await TestSessions.AdminAsync(_database.Dsn)).Key;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task TheNewForm_IsEmpty_AndAnUnknownKeyIs404_KK2()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode created, string html) = await GetAsync(client, ProductFormModel.NewPath);
        (HttpStatusCode missing, _) = await GetAsync(client, ProductFormModel.PathOf(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.OK, created);
        Assert.Contains($"<h1>{ProductFormModel.NewTitle}</h1>", html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.NotFound, missing);
    }

    [Fact]
    public async Task TheExistingForm_ShowsTheStockTheReservedUnitsAndTheGatesUnmetConditions_KK2()
    {
        (Guid productId, Guid[] variants) = await AddProductAsync("Kupa", PublishStatus.Draft, PublishStatus.Draft, (10m, 7));
        await ReserveAsync(variants[0], 3);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, ProductFormModel.PathOf(productId));

        Assert.Equal(HttpStatusCode.OK, status);
        string row = RowOf(html, variants[0]);
        Assert.Contains("<td>7</td>", row, StringComparison.Ordinal);
        Assert.Contains("<td>3</td>", row, StringComparison.Ordinal);
        Assert.Contains($"<li>{ProductFormModel.ConditionName(PublishCondition.MainCategory)}</li>", html, StringComparison.Ordinal);
        Assert.Contains($"<li>{ProductFormModel.ConditionName(PublishCondition.PublishedVariant)}</li>", html, StringComparison.Ordinal);
        Assert.DoesNotContain($"<li>{ProductFormModel.ConditionName(PublishCondition.Name)}</li>", html, StringComparison.Ordinal);
        Assert.DoesNotContain("style=", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task APublishMove_IsAbsolute_AndARepeatWritesNothing_KK7()
    {
        (Guid productId, _) = await AddProductAsync("Kupa", PublishStatus.Published, PublishStatus.Published, (10m, 5));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage moved = await PostAsync(client, productId, "ChangeProductPublishStatus", ("Input.PublishStatus", nameof(PublishStatus.Draft)));
        int versionAfterMove = await VersionOfAsync(productId);
        using HttpResponseMessage repeated = await PostAsync(client, productId, "ChangeProductPublishStatus", ("Input.PublishStatus", nameof(PublishStatus.Draft)));

        Assert.Equal(HttpStatusCode.SeeOther, moved.StatusCode);
        Assert.Equal(ProductFormModel.PathOf(productId), moved.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.SeeOther, repeated.StatusCode);
        Assert.Equal(PublishStatus.Draft, await StatusOfAsync(productId));
        Assert.Equal(versionAfterMove, await VersionOfAsync(productId));
    }

    [Fact]
    public async Task APublishMoveRefusedByTheGate_Is422_AndNamesTheUnmetConditions_KK8()
    {
        (Guid productId, _) = await AddProductAsync("Kupa", PublishStatus.Draft, PublishStatus.Draft, (10m, 5));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage refused = await PostAsync(client, productId, "ChangeProductPublishStatus", ("Input.PublishStatus", nameof(PublishStatus.Published)));
        string html = WebUtility.HtmlDecode(await refused.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
        string expected = ProductFormModel.GateRefusal(
            string.Join(", ", new[] { PublishCondition.MainCategory, PublishCondition.PublishedVariant }.Select(ProductFormModel.ConditionName)));
        Assert.Contains(expected, html, StringComparison.Ordinal);
        Assert.Equal(PublishStatus.Draft, await StatusOfAsync(productId));
    }

    [Fact]
    public async Task TheLastPublishedVariant_OfAPublishedProduct_IsKeptByTheMoveAndTheDeletion_KK8_KK9()
    {
        (Guid productId, Guid[] variants) = await AddProductAsync("Kupa", PublishStatus.Published, PublishStatus.Published, (10m, 5));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage moved = await PostAsync(
            client, productId, "ChangeVariantPublishStatus", ("Input.VariantId", variants[0].ToString("D")), ("Input.PublishStatus", nameof(PublishStatus.Archived)));
        using HttpResponseMessage deleted = await PostAsync(
            client, productId, "DeleteVariant", ("Input.VariantId", variants[0].ToString("D")), ("Input.Confirmed", "true"));
        string movedHtml = WebUtility.HtmlDecode(await moved.Content.ReadAsStringAsync(Ct));
        string deletedHtml = WebUtility.HtmlDecode(await deleted.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, moved.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, deleted.StatusCode);
        Assert.Contains(ProductFormModel.LastPublishedVariantText, movedHtml, StringComparison.Ordinal);
        Assert.Contains(ProductFormModel.LastPublishedVariantText, deletedHtml, StringComparison.Ordinal);
        await using AppDbContext db = ListSeed.Open(_database.Dsn);
        Assert.Equal(PublishStatus.Published, await db.Variants.AsNoTracking().Where(v => v.Id == variants[0]).Select(v => v.PublishStatus).SingleAsync(Ct));
    }

    [Fact]
    public async Task AVariantOfAnotherProduct_IsNotFound_ForTheMoveAndTheDeletion_KK7_KK9()
    {
        (Guid productId, _) = await AddProductAsync("Kupa", PublishStatus.Draft, PublishStatus.Draft, (10m, 5));
        (_, Guid[] others) = await AddProductAsync("Tabak", PublishStatus.Draft, PublishStatus.Draft, (10m, 5));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage moved = await PostAsync(
            client, productId, "ChangeVariantPublishStatus", ("Input.VariantId", others[0].ToString("D")), ("Input.PublishStatus", nameof(PublishStatus.Archived)));
        using HttpResponseMessage deleted = await PostAsync(client, productId, "DeleteVariant", ("Input.VariantId", others[0].ToString("D")), ("Input.Confirmed", "true"));

        Assert.Equal(HttpStatusCode.NotFound, moved.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deleted.StatusCode);
        Assert.Equal(PublishStatus.Draft, await StatusOfAsync(productId));
    }

    [Fact]
    public async Task TheDeletionWithoutTheMark_IsTheConfirmationState_AndWithItTheProductGoesAndTheListOpens_KK9()
    {
        (Guid productId, _) = await AddProductAsync("Kupa", PublishStatus.Draft, PublishStatus.Draft, (10m, 5));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage asked = await PostAsync(client, productId, "DeleteProduct");
        string html = WebUtility.HtmlDecode(await asked.Content.ReadAsStringAsync(Ct));
        bool keptAfterAsking = await ExistsAsync(productId);
        using HttpResponseMessage confirmed = await PostAsync(client, productId, "DeleteProduct", ("Input.Confirmed", "true"));
        using HttpResponseMessage again = await PostAsync(client, productId, "DeleteProduct", ("Input.Confirmed", "true"));

        Assert.Equal(HttpStatusCode.OK, asked.StatusCode);
        Assert.Contains(ProductFormModel.DeletionSentence("Kupa", 0), html, StringComparison.Ordinal);
        Assert.Contains(IrreversibleConfirmations.IrreversibleSentence, html, StringComparison.Ordinal);
        Assert.True(keptAfterAsking);
        Assert.Equal(HttpStatusCode.SeeOther, confirmed.StatusCode);
        Assert.Equal("/panel/urunler", confirmed.Headers.Location?.OriginalString);
        Assert.False(await ExistsAsync(productId));
        Assert.Equal(HttpStatusCode.NotFound, again.StatusCode);
    }

    [Fact]
    public void TheDeletionSentence_NamesTheOpenOrders_OnlyWhenThereIsOne_KK9()
    {
        Assert.Equal("Kupa kalıcı olarak silinecek.", ProductFormModel.DeletionSentence("Kupa", 0));
        Assert.Equal("Kupa kalıcı olarak silinecek. Ona bağlı 2 açık sipariş donmuş bilgisiyle sürer.", ProductFormModel.DeletionSentence("Kupa", 2));
    }

    [Fact]
    public async Task TheForms_CarryTheEntries_TheVersionAndTheShownStock_AndTheNewFormStartsFromTheDefaultVatRate_KK2()
    {
        (Guid productId, Guid[] variants) = await AddProductAsync("Kupa", PublishStatus.Draft, PublishStatus.Draft, (10m, 7));
        int version = await VersionOfAsync(productId);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string existing) = await GetAsync(client, ProductFormModel.PathOf(productId));
        (_, string created) = await GetAsync(client, ProductFormModel.NewPath);

        Assert.Contains($"name=\"Input.Version\" type=\"hidden\" value=\"{version.ToString(CultureInfo.InvariantCulture)}\"", existing, StringComparison.Ordinal);
        Assert.Contains($"name=\"Input.Variants[0].VariantId\" value=\"{variants[0]:D}\"", existing, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.Variants[0].OriginalStockQuantity\" value=\"7\"", existing, StringComparison.Ordinal);
        Assert.Contains("?handler=SaveProduct", existing, StringComparison.Ordinal);
        Assert.Contains("value=\"20\"", created, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"Input.Version\"", created, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UploadImage_ToTheGallery_Answers200WithTheSentEntriesAndTheNewImage_AndWritesNothingOfTheRecord_KK10()
    {
        (Guid productId, _) = await AddProductAsync("Kupa", PublishStatus.Draft, PublishStatus.Draft, (10m, 7));
        int version = await VersionOfAsync(productId);
        long imagesBefore = await ImageCountAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await UploadAsync(client, ProductFormModel.PathOf(productId), ImageFixtures.Png(120, 80), ProductFormModel.GalleryTarget, ("Input.Name", "Kupa yeni ad"), ("Input.Version", version.ToString(CultureInfo.InvariantCulture)));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(imagesBefore + 1, await ImageCountAsync());
        Guid imageId = await NewestImageAsync();
        Assert.Contains($"name=\"Input.Images[0].ImageId\" value=\"{imageId:D}\"", html, StringComparison.Ordinal);
        Assert.Contains("value=\"Kupa yeni ad\"", html, StringComparison.Ordinal);
        Assert.Contains($"name=\"Input.Version\" type=\"hidden\" value=\"{version.ToString(CultureInfo.InvariantCulture)}\"", html, StringComparison.Ordinal);
        Assert.Equal(version, await VersionOfAsync(productId));
        Assert.Equal("Kupa", await NameOfAsync(productId));
        Assert.Equal(0, await GalleryCountAsync(productId));
    }

    [Fact]
    public async Task UploadImage_ToAVariantRow_PutsTheNewImageInThatRowsField_KK10()
    {
        (Guid productId, Guid[] variants) = await AddProductAsync("Kupa", PublishStatus.Draft, PublishStatus.Draft, (10m, 7));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await UploadAsync(client, ProductFormModel.PathOf(productId), ImageFixtures.Jpeg(120, 80), "Variants[0].ImageId", ("Input.Variants[0].VariantId", variants[0].ToString("D")));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"name=\"Input.Variants[0].ImageId\" value=\"{await NewestImageAsync():D}\"", html, StringComparison.Ordinal);
        Assert.Null(await VariantImageOfAsync(variants[0]));
    }

    [Fact]
    public async Task UploadImage_OfAContentThatIsNoImage_Is422AtTheField_AndWritesNoImage_KK10()
    {
        (Guid productId, _) = await AddProductAsync("Kupa", PublishStatus.Draft, PublishStatus.Draft, (10m, 7));
        long imagesBefore = await ImageCountAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await UploadAsync(client, ProductFormModel.PathOf(productId), "bu bir görsel değil"u8.ToArray(), ProductFormModel.GalleryTarget, ("Input.Name", "Kupa yeni ad"));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains(UploadChecks.FormatMessage(), html, StringComparison.Ordinal);
        Assert.Contains("value=\"Kupa yeni ad\"", html, StringComparison.Ordinal);
        Assert.Equal(imagesBefore, await ImageCountAsync());
    }

    [Fact]
    public async Task UploadImage_ToAFullGallery_Is422_AndWritesNoImage_KK10()
    {
        (Guid productId, _) = await AddProductAsync("Kupa", PublishStatus.Draft, PublishStatus.Draft, (10m, 7));
        long imagesBefore = await ImageCountAsync();
        List<(string, string)> fields = [];
        for (int i = 0; i < ProductConstants.ImagesPerProductMax; i++)
        {
            fields.Add(($"Input.Images[{i}].ImageId", Guid.NewGuid().ToString("D")));
            fields.Add(($"Input.Images[{i}].SortOrder", (i + 1).ToString(CultureInfo.InvariantCulture)));
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await UploadAsync(client, ProductFormModel.PathOf(productId), ImageFixtures.Png(60, 40), ProductFormModel.GalleryTarget, [.. fields]);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains(UploadChecks.CountMessage(UploadSurface.ProductGallery), html, StringComparison.Ordinal);
        Assert.Equal(imagesBefore, await ImageCountAsync());
    }

    [Fact]
    public async Task UploadImage_ToAFilledVariantImageField_Is422_AndWritesNoImage_KK10()
    {
        (Guid productId, Guid[] variants) = await AddProductAsync("Kupa", PublishStatus.Draft, PublishStatus.Draft, (10m, 7));
        long imagesBefore = await ImageCountAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await UploadAsync(
            client,
            ProductFormModel.PathOf(productId),
            ImageFixtures.Png(60, 40),
            "Variants[0].ImageId",
            ("Input.Variants[0].VariantId", variants[0].ToString("D")),
            ("Input.Variants[0].ImageId", Guid.NewGuid().ToString("D")));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains(UploadChecks.CountMessage(UploadSurface.Logo), html, StringComparison.Ordinal);
        Assert.Equal(imagesBefore, await ImageCountAsync());
    }

    [Fact]
    public async Task UploadImage_OnTheNewForm_Answers200_AndOnAGoneRecord_Is404_KK10()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage created = await UploadAsync(client, ProductFormModel.NewPath, ImageFixtures.Png(60, 40), ProductFormModel.GalleryTarget, ("Input.Name", "Yeni kupa"));
        using HttpResponseMessage gone = await UploadAsync(client, ProductFormModel.PathOf(Guid.NewGuid()), ImageFixtures.Png(60, 40), ProductFormModel.GalleryTarget);

        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, gone.StatusCode);
    }

    [Fact]
    public async Task UploadImage_AboveTheImageBodyCeiling_Is413_KK10()
    {
        (Guid productId, _) = await AddProductAsync("Kupa", PublishStatus.Draft, PublishStatus.Draft, (10m, 7));
        long imagesBefore = await ImageCountAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await UploadAsync(client, ProductFormModel.PathOf(productId), new byte[RequestBodyLimits.ImageUploadMaxBytes + 1], ProductFormModel.GalleryTarget);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Equal(imagesBefore, await ImageCountAsync());
    }

    private static Task<HttpResponseMessage> UploadAsync(HttpClient client, string formPath, byte[] file, string target, params (string Name, string Value)[] fields)
    {
        MultipartFormDataContent content = new();
        foreach ((string name, string value) in fields)
        {
            content.Add(new StringContent(value), name);
        }

        content.Add(new StringContent(target), "Input.ImageTarget");
        content.Add(new ByteArrayContent(file), "Input.ImageFile", "gorsel.png");
        return client.PostAsync($"{formPath}?handler=UploadImage", content, Ct);
    }

    private async Task<long> ImageCountAsync()
    {
        await using AppDbContext db = ListSeed.Open(_database.Dsn);
        return await db.Images.LongCountAsync(Ct);
    }

    private async Task<Guid> NewestImageAsync()
    {
        await using AppDbContext db = ListSeed.Open(_database.Dsn);
        return await db.Images.AsNoTracking().OrderByDescending(i => i.CreatedAt).Select(i => i.Id).FirstAsync(Ct);
    }

    private async Task<int> GalleryCountAsync(Guid productId)
    {
        await using AppDbContext db = ListSeed.Open(_database.Dsn);
        return await db.ProductImages.CountAsync(i => i.ProductId == productId, Ct);
    }

    private async Task<string> NameOfAsync(Guid productId)
    {
        await using AppDbContext db = ListSeed.Open(_database.Dsn);
        return await db.Products.AsNoTracking().Where(p => p.Id == productId).Select(p => p.Name).SingleAsync(Ct);
    }

    private async Task<Guid?> VariantImageOfAsync(Guid variantId)
    {
        await using AppDbContext db = ListSeed.Open(_database.Dsn);
        return await db.Variants.AsNoTracking().Where(v => v.Id == variantId).Select(v => v.ImageId).SingleAsync(Ct);
    }

    private static string RowOf(string html, Guid variantId)
    {
        int at = html.IndexOf($"data-variant=\"{variantId:D}\"", StringComparison.Ordinal);
        Assert.True(at >= 0, $"The form has no row for {variantId}.");
        int end = html.IndexOf("</tr>", at, StringComparison.Ordinal);
        return html[at..end];
    }

    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(ForgeryTokenSampleModel).Assembly)));

    private HttpClient ClientOf(WebApplicationFactory<Program> factory)
    {
        HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        client.DefaultRequestHeaders.Add(HeaderNames.Cookie, $"{CookieNames.AdminSession}={_adminKey}");
        return client;
    }

    private static async Task<(HttpStatusCode Status, string Html)> GetAsync(HttpClient client, string path)
    {
        using HttpResponseMessage response = await client.GetAsync(new Uri(path, UriKind.Relative), Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, Guid productId, string handler, params (string Name, string Value)[] fields) =>
        client.PostAsync($"{ProductFormModel.PathOf(productId)}?handler={handler}", new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value))), Ct);

    private async Task<(Guid ProductId, Guid[] Variants)> AddProductAsync(string name, PublishStatus status, PublishStatus variantStatus, params (decimal Price, int? Stock)[] variants)
    {
        await using AppDbContext db = ListSeed.Open(_database.Dsn);
        Product product = Product.Create(name, $"p-{Guid.NewGuid():N}", ProductType.Physical, 20m, Base).WithSeededOrigin();
        product.PublishStatus = status;
        db.Products.Add(product);
        List<Guid> ids = [];
        for (int i = 0; i < variants.Length; i++)
        {
            Variant variant = Variant.Create(product.Id, $"S-{Guid.NewGuid():N}"[..20], variants[i].Price, Base.AddSeconds(i));
            variant.StockQuantity = variants[i].Stock;
            variant.PublishStatus = variantStatus;
            db.Variants.Add(variant);
            ids.Add(variant.Id);
        }

        await db.SaveChangesAsync(Ct);
        return (product.Id, [.. ids]);
    }

    private async Task ReserveAsync(Guid variantId, int units)
    {
        await using AppDbContext db = ListSeed.Open(_database.Dsn);
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE variants SET reserved_quantity = {units} WHERE id = {variantId}", Ct);
    }

    private async Task<PublishStatus> StatusOfAsync(Guid productId)
    {
        await using AppDbContext db = ListSeed.Open(_database.Dsn);
        return await db.Products.AsNoTracking().Where(p => p.Id == productId).Select(p => p.PublishStatus).SingleAsync(Ct);
    }

    private async Task<int> VersionOfAsync(Guid productId)
    {
        await using AppDbContext db = ListSeed.Open(_database.Dsn);
        return await db.Products.AsNoTracking().Where(p => p.Id == productId).Select(p => p.Version).SingleAsync(Ct);
    }

    private async Task<bool> ExistsAsync(Guid productId)
    {
        await using AppDbContext db = ListSeed.Open(_database.Dsn);
        return await db.Products.AsNoTracking().AnyAsync(p => p.Id == productId, Ct);
    }
}
