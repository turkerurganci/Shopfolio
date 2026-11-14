using System.Globalization;
using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Net.Http.Headers;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
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

    /// <summary>This test's own file store (the uploads' images), so no test writes under the production directories; removed when the test ends.</summary>
    private readonly string _fileRoot = Path.Combine(Path.GetTempPath(), "shopfolio-t525-" + Guid.NewGuid().ToString("N"));

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

    public ValueTask DisposeAsync()
    {
        if (Directory.Exists(_fileRoot))
        {
            Directory.Delete(_fileRoot, recursive: true);
        }

        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task TheNewForm_IsEmpty_AndAnUnknownKeyIs404_KK2()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode created, string html) = await GetAsync(client, ProductFormModel.NewPath);
        (HttpStatusCode missing, _) = await GetAsync(client, ProductFormModel.PathOf(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.OK, created);
        Assert.Contains($"<h1>{ProductFormModel.NewTitle}</h1>", html, StringComparison.Ordinal);
        Assert.Contains(ProductFormModel.MeasurementUnitReminderText, html, StringComparison.Ordinal);
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
    public async Task TheReservedUnits_NameTheOrdersThatHoldThem_EachALinkToItsOrder_KK2()
    {
        (Guid productId, Guid[] variants) = await AddProductAsync("Kupa", PublishStatus.Draft, PublishStatus.Draft, (10m, 7));
        (Guid orderId, string number, _) = await TestSessions.OrderAsync(_database.Dsn, null);
        await ReserveAsync(variants[0], 2);
        await using (AppDbContext db = ListSeed.Open(_database.Dsn))
        {
            db.StockReservations.Add(StockReservation.ForVariant(orderId, variants[0], 2, Base));
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string html) = await GetAsync(client, ProductFormModel.PathOf(productId));

        string row = RowOf(html, variants[0]);
        Assert.Contains($"<a href=\"/panel/siparisler/{orderId:D}\">{number}</a> (2)", row, StringComparison.Ordinal);
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

    [Fact]
    public async Task SaveProduct_Saved_Is303ToTheProductsForm_AndWritesTheEntries_KK3()
    {
        (Guid productId, Guid[] variants) = await AddProductAsync("Kupa", PublishStatus.Draft, PublishStatus.Draft, (10m, 7));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await PostAsync(client, productId, "SaveProduct", await SaveFieldsAsync(productId, variants[0], name: "Kupa yeni ad", stock: "9"));

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal(ProductFormModel.PathOf(productId), response.Headers.Location?.OriginalString);
        Assert.Equal("Kupa yeni ad", await NameOfAsync(productId));
        Assert.Equal(9, await StockOfAsync(variants[0]));
    }

    [Fact]
    public async Task SaveProduct_OfTheFormAsItWasShown_KeepsTheCategoriesTheOptionsTheDiscountAndTheFiles_KK2_KK3()
    {
        // UC-114 writes the form whole: the form carries every part the record holds, so posting it back as shown - with only the name changed - keeps them all.
        Category category = Category.Create("Kitaplar", $"kitaplar-{Guid.NewGuid():N}"[..20], null, 1, Base);
        Product product = Product.Create("E-kitap", $"p-{Guid.NewGuid():N}", ProductType.Digital, 20m, Base);
        Option option = Option.Create(product.Id, "Biçim", 1, Base);
        OptionValue pdf = OptionValue.Create(option.Id, "PDF", 1, Base);
        OptionValue epub = OptionValue.Create(option.Id, "EPUB", 2, Base);
        Variant variant = Variant.Create(product.Id, $"S-{Guid.NewGuid():N}"[..20], 100m, Base, null);
        variant.FirstOptionValueId = pdf.Id;
        DigitalFile productFile = AttachedFile(product.Id, null);
        DigitalFile variantFile = AttachedFile(null, variant.Id);
        DateOnly start = new(2027, 1, 1);
        DateOnly end = new(2027, 1, 31);
        await using (AppDbContext db = ListSeed.Open(_database.Dsn))
        {
            db.Categories.Add(category);
            db.Products.Add(product);
            db.Options.Add(option);
            db.OptionValues.AddRange(pdf, epub);
            db.Variants.Add(variant);
            db.PriceHistories.Add(PriceHistory.Create(variant.Id, 100m, Base, Base));
            db.ProductCategories.Add(ProductCategory.Create(product.Id, category.Id, true, Base));
            db.Discounts.Add(Discount.Create(product.Id, 10m, start, end, Base));
            db.DigitalFiles.AddRange(productFile, variantFile);
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        (_, string html) = await GetAsync(client, ProductFormModel.PathOf(product.Id));
        List<(string Name, string Value)> fields = [.. SaveFormFieldsOf(html).Where(f => f.Name != "Input.Name"), ("Input.Name", "E-kitap yeni"), ("Input.ProductType", nameof(ProductType.Digital))];

        using HttpResponseMessage response = await PostAsync(client, product.Id, "SaveProduct", [.. fields]);

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        await using AppDbContext check = ListSeed.Open(_database.Dsn);
        Assert.Equal("E-kitap yeni", await NameOfAsync(product.Id));
        ProductCategory link = await check.ProductCategories.AsNoTracking().SingleAsync(l => l.ProductId == product.Id, Ct);
        Assert.Equal((category.Id, true), (link.CategoryId, link.IsPrimaryCategory));
        Assert.Equal(["PDF", "EPUB"], await check.OptionValues.AsNoTracking().Where(v => v.OptionId == option.Id).OrderBy(v => v.SortOrder).Select(v => v.Value).ToListAsync(Ct));
        Assert.Equal("Biçim", await check.Options.AsNoTracking().Where(o => o.Id == option.Id).Select(o => o.Name).SingleAsync(Ct));
        Assert.Equal(pdf.Id, await check.Variants.AsNoTracking().Where(v => v.Id == variant.Id).Select(v => v.FirstOptionValueId).SingleAsync(Ct));
        Discount kept = await check.Discounts.AsNoTracking().SingleAsync(d => d.ProductId == product.Id, Ct);
        Assert.Equal((10m, start, end), (kept.Percentage, kept.StartDate, kept.EndDate));
        Assert.Equal(
            [productFile.Id, variantFile.Id],
            await check.DigitalFiles.AsNoTracking().Where(f => f.DetachedAt == null && f.AttachedAt != null).OrderBy(f => f.VariantId != null).Select(f => f.Id).ToListAsync(Ct));
    }

    [Fact]
    public async Task TheForm_ShowsEveryPartItWrites_AsAVisibleField_AndTheDiscountsPricesAsTheServerComputedThem_KK2()
    {
        // 04 9.5.3-9.5.9: categories, options, the variant's option values, the place of origin, the unit, the exemption, the discount and its read-only prices are fields of the form.
        Category category = Category.Create("Kupalar", $"kupalar-{Guid.NewGuid():N}"[..20], null, 1, Base);
        Product product = Product.Create("Kupa", $"p-{Guid.NewGuid():N}", ProductType.Physical, 20m, Base).WithSeededOrigin();
        product.MeasurementUnit = MeasurementUnit.Gram;
        product.WithdrawalExemptionReason = Shopfolio.Modules.AfterSales.Contracts.WithdrawalExemptionReason.Perishable;
        Option option = Option.Create(product.Id, "Renk", 1, Base);
        OptionValue red = OptionValue.Create(option.Id, "Kırmızı", 1, Base);
        OptionValue blue = OptionValue.Create(option.Id, "Mavi", 2, Base);
        Variant variant = Variant.Create(product.Id, $"S-{Guid.NewGuid():N}"[..20], 100m, Base, null);
        variant.FirstOptionValueId = blue.Id;
        variant.NetQuantity = 250m;
        variant.StockQuantity = 4;
        await using (AppDbContext db = ListSeed.Open(_database.Dsn))
        {
            db.Categories.Add(category);
            db.Products.Add(product);
            db.Options.Add(option);
            db.OptionValues.AddRange(red, blue);
            db.Variants.Add(variant);
            db.PriceHistories.Add(PriceHistory.Create(variant.Id, 100m, Base, Base));
            db.ProductCategories.Add(ProductCategory.Create(product.Id, category.Id, true, Base));
            db.Discounts.Add(Discount.Create(product.Id, 10m, new DateOnly(2090, 1, 1), new DateOnly(2090, 1, 31), Base));
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        (HttpStatusCode status, string html) = await GetAsync(client, ProductFormModel.PathOf(product.Id));

        Assert.Equal(HttpStatusCode.OK, status);
        List<(string Name, string Value)> sent = [.. SaveFormFieldsOf(html)];
        Assert.Contains(sent, f => f.Name.EndsWith("].CategoryId", StringComparison.Ordinal) && f.Value == category.Id.ToString("D"));
        Assert.Contains(sent, f => f.Name.EndsWith("].IsPrimaryCategory", StringComparison.Ordinal) && f.Value == "true");
        Assert.Contains(("Input.Options[0].Name", "Renk"), sent);
        Assert.Contains(("Input.Options[0].Values[1].Value", "Mavi"), sent);
        Assert.Contains(("Input.Variants[0].FirstOptionValueId", blue.Id.ToString("D")), sent);
        Assert.Contains(("Input.OriginCountryId", SeededCountry.Turkey.ToString("D")), sent);
        Assert.Contains(("Input.MeasurementUnit", nameof(MeasurementUnit.Gram)), sent);
        Assert.Contains(("Input.WithdrawalExemptionReason", nameof(Shopfolio.Modules.AfterSales.Contracts.WithdrawalExemptionReason.Perishable)), sent);
        Assert.Contains(("Input.Discount.StartDate", "2090-01-01"), sent);
        Assert.Contains($"Net miktar ({ProductFormModel.UnitName(MeasurementUnit.Gram)})", html, StringComparison.Ordinal);
        Assert.Contains(Shopfolio.Modules.AfterSales.Contracts.WithdrawalExemptionReasonNames.Of(Shopfolio.Modules.AfterSales.Contracts.WithdrawalExemptionReason.Perishable), html, StringComparison.Ordinal);
        Assert.Contains(ProductFormModel.NewOrdersOnlyText, html, StringComparison.Ordinal);
        Assert.Contains(ProductFormModel.MeasurementUnitReminderText, html, StringComparison.Ordinal);
        string prices = html[html.IndexOf("id=\"product-discount-prices\"", StringComparison.Ordinal)..];
        prices = prices[..prices.IndexOf("</table>", StringComparison.Ordinal)];
        Assert.Contains(DisplayFormat.Amount(DiscountRules.DiscountedPrice(100m, 10m)), prices, StringComparison.Ordinal);
        Assert.Contains(DisplayFormat.Amount(100m), prices, StringComparison.Ordinal);
        Assert.Contains(ProductFormModel.DiscountStateText(Shopfolio.Modules.Catalog.Contracts.Pricing.DiscountState.NotStarted), html, StringComparison.Ordinal);
        // The existing product's form has one blank new variant row (K-1189 (2)).
        Assert.Contains("name=\"Input.Variants[1].SKU\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SaveProduct_ThroughTheVisibleFields_WritesTheCategoryTheOptionValueTheDiscountAndTheExemption_AndTheBlankNewEntriesAreNotRead_KK3()
    {
        Category category = Category.Create("Bardaklar", $"bardak-{Guid.NewGuid():N}"[..20], null, 1, Base);
        Product product = Product.Create("Bardak", $"p-{Guid.NewGuid():N}", ProductType.Physical, 20m, Base).WithSeededOrigin();
        Option option = Option.Create(product.Id, "Renk", 1, Base);
        OptionValue red = OptionValue.Create(option.Id, "Kırmızı", 1, Base);
        Variant variant = Variant.Create(product.Id, $"S-{Guid.NewGuid():N}"[..20], 100m, Base, null);
        variant.FirstOptionValueId = red.Id;
        variant.StockQuantity = 3;
        await using (AppDbContext db = ListSeed.Open(_database.Dsn))
        {
            db.Categories.Add(category);
            db.Products.Add(product);
            db.Options.Add(option);
            db.OptionValues.Add(red);
            db.Variants.Add(variant);
            db.PriceHistories.Add(PriceHistory.Create(variant.Id, 100m, Base, Base));
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        (_, string html) = await GetAsync(client, ProductFormModel.PathOf(product.Id));
        string key = System.Text.RegularExpressions.Regex.Match(html, $"name=\"Input\\.Categories\\[(?<k>\\d+)\\]\\.CategoryId\" value=\"{category.Id:D}\"").Groups["k"].Value;
        Dictionary<string, string> changed = new(StringComparer.Ordinal)
        {
            [$"Input.Categories[{key}].CategoryId"] = category.Id.ToString("D"),
            [$"Input.Categories[{key}].IsPrimaryCategory"] = "true",
            ["Input.Options[0].Values[0].Value"] = "Koyu kırmızı",
            ["Input.Discount.Percentage"] = "15",
            ["Input.Discount.StartDate"] = "2090-02-01",
            ["Input.Discount.EndDate"] = "2090-02-28",
            ["Input.WithdrawalExemptionReason"] = nameof(Shopfolio.Modules.AfterSales.Contracts.WithdrawalExemptionReason.MadeToOrder),
        };
        List<(string Name, string Value)> fields = [.. SaveFormFieldsOf(html).Where(f => !changed.ContainsKey(f.Name)), .. changed.Select(c => (c.Key, c.Value))];

        using HttpResponseMessage response = await PostAsync(client, product.Id, "SaveProduct", [.. fields]);

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        await using AppDbContext check = ListSeed.Open(_database.Dsn);
        ProductCategory link = await check.ProductCategories.AsNoTracking().SingleAsync(l => l.ProductId == product.Id, Ct);
        Assert.Equal((category.Id, true), (link.CategoryId, link.IsPrimaryCategory));
        Assert.Equal(["Koyu kırmızı"], await check.OptionValues.AsNoTracking().Where(v => v.OptionId == option.Id).Select(v => v.Value).ToListAsync(Ct));
        Assert.Equal(1, await check.Options.AsNoTracking().CountAsync(o => o.ProductId == product.Id, Ct));
        Discount saved = await check.Discounts.AsNoTracking().SingleAsync(d => d.ProductId == product.Id, Ct);
        Assert.Equal((15m, new DateOnly(2090, 2, 1), new DateOnly(2090, 2, 28)), (saved.Percentage, saved.StartDate, saved.EndDate));
        Assert.Equal(
            Shopfolio.Modules.AfterSales.Contracts.WithdrawalExemptionReason.MadeToOrder,
            await check.Products.AsNoTracking().Where(p => p.Id == product.Id).Select(p => p.WithdrawalExemptionReason).SingleAsync(Ct));
        // The blank new variant row, the blank option slot and the blank value slot are not read (07 5.1.5; K-1189 (2)).
        Assert.Equal([variant.Id], await check.Variants.AsNoTracking().Where(v => v.ProductId == product.Id).Select(v => v.Id).ToListAsync(Ct));
    }

    [Fact]
    public void TheBlankNewEntries_AreNotRead_AndAnEmptyDiscountIsNone_KK3()
    {
        Guid kept = Guid.NewGuid();
        SaveProductInput input = new()
        {
            Categories = [new SaveProductCategoryRow { CategoryId = Guid.Empty }, null!, new SaveProductCategoryRow { CategoryId = kept, IsPrimaryCategory = true }],
            Options =
            [
                new SaveProductOptionRow { Name = "Renk", Values = [new SaveProductOptionValueRow { Value = "Mavi" }, new SaveProductOptionValueRow { Value = " " }] },
                new SaveProductOptionRow { Name = string.Empty, Values = [new SaveProductOptionValueRow()] },
            ],
            Variants = [new SaveProductVariantRow { SKU = "A-1", Price = 5m }, new SaveProductVariantRow()],
            Discount = new SaveProductDiscountRow(),
        };

        ProductFormModel.OmitBlankEntries(input);

        Assert.Equal([kept], input.Categories.Select(c => c.CategoryId));
        Assert.Equal(["Mavi"], input.Options.Single().Values.Select(v => v.Value));
        Assert.Equal(["A-1"], input.Variants.Select(v => v.SKU));
        Assert.Null(input.Discount);

        // A new product's only row stays even when blank: the module says what it lacks.
        SaveProductInput blank = new() { Variants = [new SaveProductVariantRow()] };
        ProductFormModel.OmitBlankEntries(blank);
        Assert.Single(blank.Variants);
    }

    [Fact]
    public async Task ADigitalProductsForm_CarriesTheFileFields_HookedForTheChunkedUpload_KK2()
    {
        Product product = Product.Create("E-kitap", $"p-{Guid.NewGuid():N}", ProductType.Digital, 20m, Base);
        Variant variant = Variant.Create(product.Id, $"S-{Guid.NewGuid():N}"[..20], 100m, Base, null);
        DigitalFile productFile = AttachedFile(product.Id, null);
        await using (AppDbContext db = ListSeed.Open(_database.Dsn))
        {
            db.Products.Add(product);
            db.Variants.Add(variant);
            db.PriceHistories.Add(PriceHistory.Create(variant.Id, 100m, Base, Base));
            db.DigitalFiles.Add(productFile);
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        (_, string html) = await GetAsync(client, ProductFormModel.PathOf(product.Id));

        Assert.Contains($"data-js=\"{ProductFormModel.DigitalFileUploadHook}\" data-target-field=\"Input.DigitalFileId\"", html, StringComparison.Ordinal);
        Assert.Contains($"data-js=\"{ProductFormModel.DigitalFileUploadHook}\" data-target-field=\"Input.Variants[0].DigitalFileId\"", html, StringComparison.Ordinal);
        Assert.Contains(("Input.DigitalFileId", productFile.Id.ToString("D")), SaveFormFieldsOf(html));
        Assert.Contains(ProductFormModel.FileUpdateText, html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SaveProduct_OnAGoneRecord_Is404_BeforeAFieldsRefusal_AndTheSameFieldOnAnExistingRecordIs422_KK6()
    {
        (Guid productId, Guid[] variants) = await AddProductAsync("Kupa", PublishStatus.Draft, PublishStatus.Draft, (10m, 7));
        (string Name, string Value)[] broken = await SaveFieldsAsync(productId, variants[0], price: "on lira");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage existing = await PostAsync(client, productId, "SaveProduct", broken);
        using HttpResponseMessage gone = await PostAsync(client, Guid.NewGuid(), "SaveProduct", broken);
        using HttpResponseMessage goneValid = await PostAsync(client, Guid.NewGuid(), "SaveProduct", await SaveFieldsAsync(productId, variants[0]));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, existing.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, gone.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, goneValid.StatusCode);
    }

    [Fact]
    public async Task SaveProduct_AFieldsRefusal_ComesBeforeAChangedVersion_AndWritesNothing_KK6()
    {
        (Guid productId, Guid[] variants) = await AddProductAsync("Kupa", PublishStatus.Draft, PublishStatus.Draft, (10m, 7));
        int version = await VersionOfAsync(productId);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await PostAsync(client, productId, "SaveProduct", await SaveFieldsAsync(productId, variants[0], name: "Kupa yeni ad", price: "-5", version: version + 1));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("Kupa", await NameOfAsync(productId));
        Assert.Equal(version, await VersionOfAsync(productId));
    }

    [Fact]
    public async Task SaveProduct_AtAChangedVersion_Is409_WritesNothing_AndYineDeKaydetSavesWithTheCurrentVersion_KK6()
    {
        (Guid productId, Guid[] variants) = await AddProductAsync("Kupa", PublishStatus.Draft, PublishStatus.Draft, (10m, 7));
        int version = await VersionOfAsync(productId);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage stale = await PostAsync(client, productId, "SaveProduct", await SaveFieldsAsync(productId, variants[0], name: "Kupa yeni ad", version: version + 1));
        string html = WebUtility.HtmlDecode(await stale.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal("Kupa", await NameOfAsync(productId));
        Assert.Equal(version, await VersionOfAsync(productId));
        Assert.Contains(ConflictControls.RecordMessage, html, StringComparison.Ordinal);
        Assert.Contains(ConflictControls.OverwriteLabel, html, StringComparison.Ordinal);
        Assert.Contains(ConflictControls.OpenCurrentLabel, html, StringComparison.Ordinal);
        Assert.Contains("value=\"Kupa yeni ad\"", html, StringComparison.Ordinal);
        Assert.Contains($"name=\"Input.Version\" type=\"hidden\" value=\"{version.ToString(CultureInfo.InvariantCulture)}\"", html, StringComparison.Ordinal);

        using HttpResponseMessage overwrite = await PostAsync(client, productId, "SaveProduct", await SaveFieldsAsync(productId, variants[0], name: "Kupa yeni ad", version: version));

        Assert.Equal(HttpStatusCode.SeeOther, overwrite.StatusCode);
        Assert.Equal("Kupa yeni ad", await NameOfAsync(productId));
    }

    [Fact]
    public async Task SaveProduct_AChangedShownStock_Is409BeforeTheReservedQuantitys422_AndTheResentFormIsRefusedBelowTheReservedQuantity_KK4_KK6()
    {
        (Guid productId, Guid[] variants) = await AddProductAsync("Kupa", PublishStatus.Draft, PublishStatus.Draft, (10m, 7));
        await ReserveAsync(variants[0], 3);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage stale = await PostAsync(client, productId, "SaveProduct", await SaveFieldsAsync(productId, variants[0], stock: "1", shownStock: "5"));
        string html = WebUtility.HtmlDecode(await stale.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal(7, await StockOfAsync(variants[0]));
        Assert.Contains("name=\"Input.Variants[0].OriginalStockQuantity\" value=\"7\"", html, StringComparison.Ordinal);
        Assert.Contains(ConflictControls.OverwriteLabel, html, StringComparison.Ordinal);

        using HttpResponseMessage resent = await PostAsync(client, productId, "SaveProduct", await SaveFieldsAsync(productId, variants[0], stock: "1", shownStock: "7"));
        string refused = WebUtility.HtmlDecode(await resent.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resent.StatusCode);
        Assert.Contains(ProductSaveRefusals.BelowReservedText(3), refused, StringComparison.Ordinal);
        Assert.Equal(7, await StockOfAsync(variants[0]));
    }

    [Fact]
    public async Task SaveProduct_AnUnreadableShownStock_IsNoMatch_Is409_AndWritesNothing_KK6()
    {
        (Guid productId, Guid[] variants) = await AddProductAsync("Kupa", PublishStatus.Draft, PublishStatus.Draft, (10m, 7));
        int version = await VersionOfAsync(productId);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await PostAsync(client, productId, "SaveProduct", await SaveFieldsAsync(productId, variants[0], name: "Kupa yeni ad", shownStock: "yedi"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("Kupa", await NameOfAsync(productId));
        Assert.Equal(version, await VersionOfAsync(productId));
        Assert.Equal(7, await StockOfAsync(variants[0]));
    }

    [Fact]
    public async Task SaveProduct_TheModulesRefusals_AreTheirFieldsMessages_AndTheNetQuantitysTextSaysThreeDecimals_KK5()
    {
        (Guid productId, Guid[] variants) = await AddProductAsync("Kupa", PublishStatus.Draft, PublishStatus.Draft, (10m, 7));
        (_, Guid[] others) = await AddProductAsync("Tabak", PublishStatus.Draft, PublishStatus.Draft, (12m, 4));
        string otherSku = await SkuOfAsync(others[0]);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage duplicate = await PostAsync(client, productId, "SaveProduct", await SaveFieldsAsync(productId, variants[0], sku: otherSku));
        using HttpResponseMessage zero = await PostAsync(client, productId, "SaveProduct", await SaveFieldsAsync(productId, variants[0], price: "0"));
        using HttpResponseMessage netQuantity = await PostAsync(client, productId, "SaveProduct", [.. await SaveFieldsAsync(productId, variants[0]), ("Input.MeasurementUnit", "Gram"), ("Input.Variants[0].NetQuantity", "1,2345")]);
        string netQuantityHtml = WebUtility.HtmlDecode(await netQuantity.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, duplicate.StatusCode);
        Assert.Contains(ProductSaveRefusals.DuplicateStockCodeText, WebUtility.HtmlDecode(await duplicate.Content.ReadAsStringAsync(Ct)), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, zero.StatusCode);
        Assert.Contains("id=\"product-variant-0-sku-message\"", WebUtility.HtmlDecode(await zero.Content.ReadAsStringAsync(Ct)), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, netQuantity.StatusCode);
        Assert.Contains("en çok üç ondalık", netQuantityHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("iki ondalık", netQuantityHtml, StringComparison.Ordinal);
        Assert.Equal("Kupa", await NameOfAsync(productId));
    }

    [Fact]
    public async Task SaveProduct_AShippingPeriodAboveItsFence_Is422WithItsFieldsText_AndWritesNothing_D496()
    {
        (Guid productId, Guid[] variants) = await AddProductAsync("Kupa", PublishStatus.Draft, PublishStatus.Draft, (10m, 7));
        int version = await VersionOfAsync(productId);
        string above = (ParameterFences.ShippingPeriodUpper + 1).ToString(CultureInfo.InvariantCulture);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await PostAsync(client, productId, "SaveProduct", [.. await SaveFieldsAsync(productId, variants[0], name: "Kupa yeni ad"), ("Input.ShippingPeriod", above)]);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains(ProductSaveRefusals.ShippingPeriodText(), html, StringComparison.Ordinal);
        Assert.Equal("Kupa", await NameOfAsync(productId));
        Assert.Equal(version, await VersionOfAsync(productId));
    }

    [Fact]
    public async Task SaveProduct_AServicesExemption_AndAFulfilmentPeriodBelowItsFence_Are422AtTheirFields_AndWriteNothing_D246()
    {
        Guid productId;
        Guid variantId;
        await using (AppDbContext db = ListSeed.Open(_database.Dsn))
        {
            Product service = Product.Create("Ders", $"p-{Guid.NewGuid():N}", ProductType.Service, 20m, Base);
            db.Products.Add(service);
            Variant variant = Variant.Create(service.Id, $"S-{Guid.NewGuid():N}"[..20], 10m, Base);
            db.Variants.Add(variant);
            await db.SaveChangesAsync(Ct);
            (productId, variantId) = (service.Id, variant.Id);
        }

        int version = await VersionOfAsync(productId);
        string below = (ParameterFences.FulfilmentPeriodLower - 1).ToString(CultureInfo.InvariantCulture);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        (string Name, string Value)[] fields = await SaveFieldsAsync(productId, variantId, name: "Ders yeni ad", stock: string.Empty, shownStock: string.Empty);

        using HttpResponseMessage exemption = await PostAsync(client, productId, "SaveProduct", [.. fields, ("Input.WithdrawalExemptionReason", Enum.GetValues<WithdrawalExemptionReason>()[0].ToString())]);
        using HttpResponseMessage fulfilment = await PostAsync(client, productId, "SaveProduct", [.. fields, ("Input.FulfilmentPeriod", below)]);
        string fulfilmentHtml = WebUtility.HtmlDecode(await fulfilment.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, exemption.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, fulfilment.StatusCode);
        Assert.Contains(ProductSaveRefusals.OutOfBoundsText, fulfilmentHtml, StringComparison.Ordinal);
        Assert.Equal("Ders", await NameOfAsync(productId));
        Assert.Equal(version, await VersionOfAsync(productId));
        await using AppDbContext check = ListSeed.Open(_database.Dsn);
        Assert.Null(await check.Products.AsNoTracking().Where(p => p.Id == productId).Select(p => p.WithdrawalExemptionReason).SingleAsync(Ct));
    }

    [Fact]
    public async Task SaveProduct_APriceRiseUnderADiscount_ThatLiftsTheDiscountedPriceToTheReference_Is422AtThePrice_AndWritesNothing_D488()
    {
        // 02 3.9.5 on the form's price change (D-488): the form saves the discount again with the new price, so a rise that takes the discounted price to or above the reference
        // (the lowest price of the 30 days before the start) is the price field's refusal - 100 with 10% off is 90; 120 with 10% off is 108, not below 100.
        (Guid productId, Guid[] variants) = await AddProductAsync("Kupa", PublishStatus.Draft, PublishStatus.Draft, (100m, 7));
        DateOnly start = TurkeyTime.DateOf(DateTimeOffset.UtcNow).AddDays(3);
        DateOnly end = start.AddDays(10);
        await using (AppDbContext db = ListSeed.Open(_database.Dsn))
        {
            db.PriceHistories.Add(PriceHistory.Create(variants[0], 100m, Base, Base));
            db.Discounts.Add(Discount.Create(productId, 10m, start, end, Base));
            await db.SaveChangesAsync(Ct);

            // On sale since before the window (02 3.9.3): the reference is the window's lowest price, not the price immediately before the start.
            await db.Variants.Where(v => v.Id == variants[0]).ExecuteUpdateAsync(s => s.SetProperty(v => v.FirstOnSaleAt, Base), Ct);
        }

        int version = await VersionOfAsync(productId);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        (string Name, string Value)[] discount =
        [
            ("Input.Discount.Percentage", "10"),
            ("Input.Discount.StartDate", start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            ("Input.Discount.EndDate", end.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
        ];

        using HttpResponseMessage response = await PostAsync(client, productId, "SaveProduct", [.. await SaveFieldsAsync(productId, variants[0], price: "120"), .. discount]);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains($"referans fiyatın ({DisplayFormat.Amount(100m)}) altına inmiyor", html, StringComparison.Ordinal);
        Assert.Equal(version, await VersionOfAsync(productId));
        await using AppDbContext check = ListSeed.Open(_database.Dsn);
        Assert.Equal(100m, await check.Variants.AsNoTracking().Where(v => v.Id == variants[0]).Select(v => v.Price).SingleAsync(Ct));
        Assert.Equal([100m], await check.PriceHistories.AsNoTracking().Where(h => h.VariantId == variants[0]).Select(h => h.Price).ToListAsync(Ct));
        Discount kept = await check.Discounts.AsNoTracking().SingleAsync(d => d.ProductId == productId, Ct);
        Assert.Equal((10m, start, end), (kept.Percentage, kept.StartDate, kept.EndDate));
    }

    [Fact]
    public async Task SaveProduct_ABodyAboveTheDefaultCeilingAndBelowTheLongTextOne_IsNot413_KK3()
    {
        (Guid productId, Guid[] variants) = await AddProductAsync("Kupa", PublishStatus.Draft, PublishStatus.Draft, (10m, 7));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        string description = new('a', (int)RequestBodyLimits.GeneralMaxBytes + 1024);
        Assert.True(description.Length < RequestBodyLimits.LongTextMaxBytes);

        using HttpResponseMessage response = await PostAsync(client, productId, "SaveProduct", [.. await SaveFieldsAsync(productId, variants[0]), ("Input.Description", description)]);

        Assert.NotEqual(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    }

    [Fact]
    public void TheModulesRefusals_TakeK799sTexts_WithTheModulesValues_AndTheNetQuantityKeepsItsRulesText_KK5()
    {
        static string Name(string field) => field == "Variants[1].Price" ? "Kırmızı" : "?";

        Assert.Equal(ProductSaveRefusals.PriceAboveZeroText, ProductSaveRefusals.TextOf(new("Variants[0].Price", ErrorCode.InvalidAmount), Name));
        Assert.Null(ProductSaveRefusals.TextOf(new("Variants[0].NetQuantity", ErrorCode.InvalidAmount), Name));
        Assert.Equal(new InvalidNetQuantityAttribute().ParameterText, ProductSaveRefusals.ParameterOf(new("Variants[0].NetQuantity", ErrorCode.InvalidAmount)));
        Assert.Equal(ProductSaveRefusals.BelowReservedText(3), ProductSaveRefusals.TextOf(new("Variants[0].StockQuantity", ErrorCode.StockBelowReserved, 3), Name));
        Assert.Equal(
            ProductSaveRefusals.DiscountNotBelowReferenceText("Kırmızı", DisplayFormat.Amount(120m)),
            ProductSaveRefusals.TextOf(new("Variants[1].Price", ErrorCode.DiscountedPriceNotBelowReference, 130m, 120m), Name));
        Assert.Equal(ProductSaveRefusals.DiscountToZeroText, ProductSaveRefusals.TextOf(new("Variants[1].Price", ErrorCode.DiscountedPriceZero, 0m, 120m), Name));
        Assert.Equal(ProductSaveRefusals.ShippingPeriodText(), ProductSaveRefusals.TextOf(new("ShippingPeriod", ErrorCode.ValueOutOfBounds), Name));
        Assert.Null(ProductSaveRefusals.TextOf(new("Name", ErrorCode.TextTooLong, ProductConstants.ProductNameMaxLength), Name));
        Assert.Equal(1, ProductSaveRefusals.VariantRowOf("Variants[1].Price"));
        Assert.Null(ProductSaveRefusals.VariantRowOf("Images"));
    }

    private async Task<(string Name, string Value)[]> SaveFieldsAsync(
        Guid productId, Guid variantId, string name = "Kupa", string price = "10", string stock = "7", string shownStock = "7", string? sku = null, int? version = null)
    {
        await using AppDbContext db = ListSeed.Open(_database.Dsn);
        Product product = await db.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == productId, Ct) ?? throw new InvalidOperationException("No product.");
        string ownSku = await db.Variants.AsNoTracking().Where(v => v.Id == variantId).Select(v => v.SKU).SingleAsync(Ct);
        return
        [
            ("Input.Version", (version ?? product.Version).ToString(CultureInfo.InvariantCulture)),
            ("Input.ProductType", product.ProductType.ToString()),
            ("Input.Name", name),
            ("Input.VatRate", "20"),
            ("Input.OriginCountryId", product.OriginCountryId?.ToString("D") ?? string.Empty),
            ("Input.Variants[0].VariantId", variantId.ToString("D")),
            ("Input.Variants[0].SKU", sku ?? ownSku),
            ("Input.Variants[0].Price", price),
            ("Input.Variants[0].StockQuantity", stock),
            ("Input.Variants[0].OriginalStockQuantity", shownStock),
        ];
    }

    private async Task<int?> StockOfAsync(Guid variantId)
    {
        await using AppDbContext db = ListSeed.Open(_database.Dsn);
        return await db.Variants.AsNoTracking().Where(v => v.Id == variantId).Select(v => v.StockQuantity).SingleAsync(Ct);
    }

    private async Task<string> SkuOfAsync(Guid variantId)
    {
        await using AppDbContext db = ListSeed.Open(_database.Dsn);
        return await db.Variants.AsNoTracking().Where(v => v.Id == variantId).Select(v => v.SKU).SingleAsync(Ct);
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
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s =>
        {
            s.RemoveAll<FileStoreDirectories>();
            s.AddSingleton(new FileStoreDirectories(Path.Combine(_fileRoot, "images"), Path.Combine(_fileRoot, "digital")));
            s.AddRazorPages().AddApplicationPart(typeof(ForgeryTokenSampleModel).Assembly);
        }));

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

    private static DigitalFile AttachedFile(Guid? productId, Guid? variantId)
    {
        DigitalFile file = DigitalFile.Create(EntityIds.NewStorageKey(), "kitap.pdf", 100, Base);
        file.UploadCompletedAt = Base;
        file.ProductId = productId;
        file.VariantId = variantId;
        file.AttachedAt = Base;
        return file;
    }

    /// <summary>The entries the save form posts as a browser sends it: its inputs (text and hidden; a box only when ticked), each closed list's chosen option and the description's text; the buttons and the file fields are left out.</summary>
    private static IEnumerable<(string Name, string Value)> SaveFormFieldsOf(string html)
    {
        int start = html.IndexOf("id=\"product-save\"", StringComparison.Ordinal);
        string form = html[start..html.IndexOf("</form>", start, StringComparison.Ordinal)];
        // As a browser sends the form: an unticked box is not sent, a select sends its chosen option, a file input sends no text.
        static string? Attribute(string attrs, string attribute)
        {
            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(attrs, $"\\s{attribute}=\"(?<v>[^\"]*)\"");
            return match.Success ? System.Net.WebUtility.HtmlDecode(match.Groups["v"].Value) : null;
        }

        foreach (System.Text.RegularExpressions.Match input in System.Text.RegularExpressions.Regex.Matches(form, "<input(?<attrs>[^>]*)>"))
        {
            string attrs = input.Groups["attrs"].Value;
            bool box = attrs.Contains("type=\"checkbox\"", StringComparison.Ordinal) || attrs.Contains("type=\"radio\"", StringComparison.Ordinal);
            if (Attribute(attrs, "name") is { } name && !attrs.Contains("type=\"file\"", StringComparison.Ordinal) && (!box || attrs.Contains(" checked", StringComparison.Ordinal)))
            {
                yield return (name, Attribute(attrs, "value") ?? (box ? "on" : string.Empty));
            }
        }

        foreach (System.Text.RegularExpressions.Match select in System.Text.RegularExpressions.Regex.Matches(form, "<select(?<attrs>[^>]*)>(?<body>.*?)</select>", System.Text.RegularExpressions.RegexOptions.Singleline))
        {
            System.Text.RegularExpressions.Match chosen = System.Text.RegularExpressions.Regex.Match(select.Groups["body"].Value, "<option(?<attrs>[^>]*\\sselected[^>]*)>");
            if (Attribute(select.Groups["attrs"].Value, "name") is { } name)
            {
                yield return (name, chosen.Success ? Attribute(chosen.Groups["attrs"].Value, "value") ?? string.Empty : string.Empty);
            }
        }

        foreach (System.Text.RegularExpressions.Match area in System.Text.RegularExpressions.Regex.Matches(form, "<textarea(?<attrs>[^>]*)>(?<body>.*?)</textarea>", System.Text.RegularExpressions.RegexOptions.Singleline))
        {
            if (Attribute(area.Groups["attrs"].Value, "name") is { } name)
            {
                yield return (name, System.Net.WebUtility.HtmlDecode(area.Groups["body"].Value).TrimStart('\n', '\r'));
            }
        }
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
