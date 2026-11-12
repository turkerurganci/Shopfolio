using System.Globalization;
using System.Net;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Web;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Orders;

namespace Shopfolio.IntegrationTests.Files;

[CollectionDefinition(AfterSalesScreenTestsDefinition.Name)]
public sealed class AfterSalesScreenTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "AfterSalesScreens";
}

/// <summary>
/// E-18 and E-19 on the opened application and a real PostgreSQL (T513 criteria 1-10; KK1-KK10; 07 UC-42…UC-46): the withdrawal screen and its last step, the declaration frozen with
/// the return address of the moment, the 422s, the closed and stale answers and the 404 of an item of another order; the defect claim's two states, its three 303s and 404s, the
/// one Açık claim with its return address, and the reopening inside and outside Z-18 with the customer's history line. The module's rules themselves are the Ordering tests'.
/// </summary>
[Collection(AfterSalesScreenTestsDefinition.Name)]
public sealed class AfterSalesScreenTests : IAsyncLifetime
{
    private const string ValidIban = "TR330006100519786457841326";

    private const string FirmEmail = "destek@example.com";

    private readonly CollectionDatabaseFixture _database;

    public AfterSalesScreenTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static Uri At(string path) => new(path, UriKind.Relative);

    /// <summary>
    /// The firm's settings row with a return address and the company row with its e-mail. The startup data of the migrations already holds both rows without these values, so
    /// the rows are filled in place (and created only when a database lacks them); every test starts from the same return address.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        await using AppDbContext db = host.NewContext();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Province? province = await db.Provinces.FirstOrDefaultAsync(Ct);
        if (province is null)
        {
            province = Province.Create("06", "Ankara", now);
            db.Provinces.Add(province);
        }

        CompanySettings? settings = await db.CompanySettings.FirstOrDefaultAsync(Ct);
        if (settings is null)
        {
            settings = CompanySettings.Create(now);
            settings.BankTransferPaymentWindow = 1;
            settings.DownloadAllowance = 1;
            settings.HomepageLayout = Shopfolio.Modules.CorporateContent.Contracts.HomepageLayout.StoreFirst;
            settings.MenuNameProducts = "Ürünler";
            settings.MenuNameOfferings = "Hizmetler";
            settings.MenuNamePortfolioItems = "Çalışmalar";
            settings.MenuNameAboutPage = "Hakkımızda";
            settings.MenuNameFaqItems = "Sıkça Sorulanlar";
            settings.MenuNameContact = "İletişim";
            db.CompanySettings.Add(settings);
        }

        settings.ReturnRecipientName = "Örnek Ticaret İade";
        settings.ReturnProvinceId = province.Id;
        settings.ReturnDistrict = "Yenimahalle";
        settings.ReturnAddressLine = "Depo Sokak 3";
        settings.ReturnPhone = "3125550000";
        Company? company = await db.Companies.FirstOrDefaultAsync(Ct);
        if (company is null)
        {
            company = Company.Create(now);
            db.Companies.Add(company);
        }

        company.Email = FirmEmail;
        await db.SaveChangesAsync(Ct);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the withdrawal screen; its closed state ----

    [Fact]
    public async Task TheWithdrawalScreen_ShowsTheItemsUnitsReturnAddressAndRefund_AndAClosedOneIs303WithTheReason_KK1()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, string key, _, string name) = await SeedAsync(host, "Shipped", 2);
        (_, string closedKey, _, _) = await SeedAsync(host, "Preparing", 2);
        string unknown = OrderRoutes.AccessPath("bilinmeyen-" + Guid.NewGuid().ToString("N"));

        using HttpResponseMessage page = await host.Client.GetAsync(At(Withdrawal(key)), Ct);
        using HttpResponseMessage closed = await host.Client.GetAsync(At(Withdrawal(closedKey)), Ct);
        using HttpResponseMessage missing = await host.Client.GetAsync(At(OrderRoutes.ScreenPath(unknown, "cayma")), Ct);

        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        string html = WebUtility.HtmlDecode(await page.Content.ReadAsStringAsync(Ct));
        Assert.Contains(name, html, StringComparison.Ordinal);
        Assert.Contains(OrderScreenTexts.OpenUnits(2), html, StringComparison.Ordinal);
        Assert.Contains("Depo Sokak 3", html, StringComparison.Ordinal);
        Assert.Contains(OrderScreenTexts.CarrierReturnsText, html, StringComparison.Ordinal);
        Assert.Contains(OrderScreenTexts.SendingWayText, html, StringComparison.Ordinal);
        Assert.Contains("Geri ödeme ", html, StringComparison.Ordinal);
        Assert.Contains("name=\"Iban\"", html, StringComparison.Ordinal);
        Assert.Equal((HttpStatusCode.SeeOther, NoticeParameter.Address(OrderRoutes.AccessPath(closedKey), NoticeCode.OperationClosed)), (closed.StatusCode, closed.Headers.Location!.OriginalString));
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.DoesNotContain(OrderScreenTexts.PackagingConditionText, html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheWithdrawalScreen_ShowsTheConditionalExemptionsLine_OnlyOnTheItemThatHasIt_KK1()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, string key, Guid itemId, string name) = await SeedAsync(host, "Shipped", 1);
        await using (AppDbContext db = host.NewContext())
        {
            OrderItem item = await db.OrderItems.SingleAsync(i => i.Id == itemId, Ct);
            item.WithdrawalExemptionReason = WithdrawalExemptionReason.HygieneSensitive;
            await db.SaveChangesAsync(Ct);
        }

        using HttpResponseMessage page = await host.Client.GetAsync(At(Withdrawal(key)), Ct);

        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        string html = WebUtility.HtmlDecode(await page.Content.ReadAsStringAsync(Ct));
        Assert.Contains(name, html, StringComparison.Ordinal);
        Assert.Contains(OrderScreenTexts.PackagingConditionText, html, StringComparison.Ordinal);
    }

    // ---- KK2, KK3: the last step writes nothing; the declaration freezes the return address ----

    [Fact]
    public async Task TheUnconfirmedPost_IsTheLastStepAndWritesNothing_AndTheConfirmedOneFreezesTheReturnAddress_KK2_KK3()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, string key, Guid itemId, _) = await SeedAsync(host, "Shipped", 2);
        string screen = Withdrawal(key);

        using HttpResponseMessage lastStep = await PostAsync(host, screen, "SubmitWithdrawal", [.. Row(0, itemId, "2", 2), new("Iban", ValidIban)]);
        int afterLastStep = await WithdrawalCountAsync(host, itemId);
        using HttpResponseMessage confirmed = await PostAsync(host, screen, "SubmitWithdrawal", [.. Row(0, itemId, "2", 2), new("Iban", ValidIban), new("Confirmed", "true")]);
        await SetReturnAddressLineAsync(host, "Değişen Sokak 7");
        try
        {
            Assert.Equal(HttpStatusCode.OK, lastStep.StatusCode);
            string html = WebUtility.HtmlDecode(await lastStep.Content.ReadAsStringAsync(Ct));
            Assert.Contains("name=\"Confirmed\" value=\"true\"", html, StringComparison.Ordinal);
            Assert.Contains(OrderScreenTexts.WithdrawalConfirmLabel, html, StringComparison.Ordinal);
            Assert.Equal(0, afterLastStep);
            Assert.Equal((HttpStatusCode.SeeOther, OrderRoutes.AccessPath(key)), (confirmed.StatusCode, confirmed.Headers.Location!.OriginalString));
            await using AppDbContext db = host.NewContext();
            Withdrawal row = await db.Withdrawals.SingleAsync(w => w.OrderItemId == itemId, Ct);
            Assert.Equal((2, "Depo Sokak 3"), (row.Quantity, row.ReturnAddressLine));
            Assert.Equal(TurkeyTime.DateOf(host.Time.GetUtcNow()), row.DeclarationDate);

            // 07 UC-43, 04 5.18.10 and 5.16.5: the root shows the declaration's date and the address written into it, not the firm's changed one (K-665).
            using HttpResponseMessage root = await host.Client.GetAsync(At(OrderRoutes.AccessPath(key)), Ct);
            string rootHtml = WebUtility.HtmlDecode(await root.Content.ReadAsStringAsync(Ct));
            Assert.Contains(OrderScreenTexts.ReturnAddressTitle + ": ", rootHtml, StringComparison.Ordinal);
            Assert.Contains(", Depo Sokak 3, ", rootHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("Değişen Sokak 7", rootHtml, StringComparison.Ordinal);
            Assert.Contains(DisplayFormat.Date(row.CreatedAt), rootHtml, StringComparison.Ordinal);
        }
        finally
        {
            await SetReturnAddressLineAsync(host, "Depo Sokak 3");
        }
    }

    // ---- KK4: three 422s, the window, the 404 ----

    [Fact]
    public async Task TheWithdrawalsThree422s_TheClosedWindow_TheStaleUnits_AndTheForeignItem_WriteNothing_KK4()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, string key, Guid itemId, _) = await SeedAsync(host, "Shipped", 2);
        (_, string closedKey, Guid closedItem, _) = await SeedAsync(host, "Preparing", 2);
        (_, _, Guid foreignItem, _) = await SeedAsync(host, "Shipped", 1);
        string root = OrderRoutes.AccessPath(key);
        string screen = Withdrawal(key);

        using HttpResponseMessage noItem = await PostAsync(host, screen, "SubmitWithdrawal", [new("Items[0].OriginalOpenQuantity", "2"), new("Iban", ValidIban), new("Confirmed", "true")]);
        using HttpResponseMessage zero = await PostAsync(host, screen, "SubmitWithdrawal", [.. Row(0, itemId, "0", 2), new("Iban", ValidIban), new("Confirmed", "true")]);
        using HttpResponseMessage noIban = await PostAsync(host, screen, "SubmitWithdrawal", [.. Row(0, itemId, "1", 2), new("Confirmed", "true")]);
        using HttpResponseMessage window = await PostAsync(host, Withdrawal(closedKey), "SubmitWithdrawal", [.. Row(0, closedItem, "1", 2), new("Iban", ValidIban), new("Confirmed", "true")]);
        using HttpResponseMessage exceeds = await PostAsync(host, screen, "SubmitWithdrawal", [.. Row(0, itemId, "3", 2), new("Iban", ValidIban), new("Confirmed", "true")]);
        using HttpResponseMessage stale = await PostAsync(host, screen, "SubmitWithdrawal", [.. Row(0, itemId, "1", 1), new("Iban", ValidIban), new("Confirmed", "true")]);
        using HttpResponseMessage foreign = await PostAsync(host, screen, "SubmitWithdrawal", [.. Row(0, foreignItem, "1", 1), new("Iban", ValidIban), new("Confirmed", "true")]);

        Assert.Equal(
            [HttpStatusCode.UnprocessableEntity, HttpStatusCode.UnprocessableEntity, HttpStatusCode.UnprocessableEntity],
            new[] { noItem.StatusCode, zero.StatusCode, noIban.StatusCode });
        Assert.Equal((HttpStatusCode.SeeOther, NoticeParameter.Address(OrderRoutes.AccessPath(closedKey), NoticeCode.OperationClosed)), (window.StatusCode, window.Headers.Location!.OriginalString));
        string exceeded = NoticeParameter.Address(root, NoticeCode.QuantityExceedsOpen);
        Assert.Equal((HttpStatusCode.SeeOther, exceeded), (exceeds.StatusCode, exceeds.Headers.Location!.OriginalString));
        Assert.Equal((HttpStatusCode.SeeOther, exceeded), (stale.StatusCode, stale.Headers.Location!.OriginalString));
        Assert.Equal(HttpStatusCode.NotFound, foreign.StatusCode);
        Assert.Equal(0, await WithdrawalCountAsync(host, itemId) + await WithdrawalCountAsync(host, closedItem) + await WithdrawalCountAsync(host, foreignItem));
    }

    // ---- KK5: the repeat of a partial selection ----

    [Fact]
    public async Task TheRepeatOfAPartialSelection_IsNoSecondWithdrawal_But303WithTheReason_KK5()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, string key, Guid itemId, _) = await SeedAsync(host, "Shipped", 3);
        string screen = Withdrawal(key);
        KeyValuePair<string, string>[] form = [.. Row(0, itemId, "1", 3), new("Iban", ValidIban), new("Confirmed", "true")];

        using HttpResponseMessage first = await PostAsync(host, screen, "SubmitWithdrawal", form);
        using HttpResponseMessage again = await PostAsync(host, screen, "SubmitWithdrawal", form);

        Assert.Equal((HttpStatusCode.SeeOther, OrderRoutes.AccessPath(key)), (first.StatusCode, first.Headers.Location!.OriginalString));
        Assert.Equal((HttpStatusCode.SeeOther, NoticeParameter.Address(OrderRoutes.AccessPath(key), NoticeCode.QuantityExceedsOpen)), (again.StatusCode, again.Headers.Location!.OriginalString));
        Assert.Equal(1, await WithdrawalCountAsync(host, itemId));
    }

    // ---- KK6: the defect claim's two states ----

    [Fact]
    public async Task TheDefectClaimScreen_NewClaimHasDefaultOneAndTheLimit_AndTheReopeningShowsTheResolvedClaim_KK6()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, string key, _, string name) = await SeedAsync(host, "Shipped", 3);
        (_, string resolvedKey, Guid resolvedItem, string resolvedName) = await SeedAsync(host, "Shipped", 2);
        await SeedClaimAsync(host, resolvedItem, DefectClaimStatus.Resolved, "Sap kırık geldi");

        using HttpResponseMessage fresh = await host.Client.GetAsync(At(ClaimScreen(key)), Ct);
        using HttpResponseMessage reopening = await host.Client.GetAsync(At(ClaimScreen(resolvedKey) + "?orderItemId=" + resolvedItem), Ct);

        Assert.Equal(HttpStatusCode.OK, fresh.StatusCode);
        string html = WebUtility.HtmlDecode(await fresh.Content.ReadAsStringAsync(Ct));
        Assert.Contains(name, html, StringComparison.Ordinal);
        Assert.Contains("name=\"Quantity\"", html, StringComparison.Ordinal);
        Assert.Contains("value=\"1\"", html, StringComparison.Ordinal);
        Assert.Contains("en çok 3", html, StringComparison.Ordinal);
        Assert.Contains("name=\"Description\"", html, StringComparison.Ordinal);
        Assert.Contains("Depo Sokak 3", html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, reopening.StatusCode);
        string reopenHtml = WebUtility.HtmlDecode(await reopening.Content.ReadAsStringAsync(Ct));
        Assert.Contains(resolvedName, reopenHtml, StringComparison.Ordinal);
        Assert.Contains(OrderScreenTexts.ResolvedBadge, reopenHtml, StringComparison.Ordinal);
        Assert.Contains("Sap kırık geldi", reopenHtml, StringComparison.Ordinal);
        Assert.Contains(FirmEmail, reopenHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"Description\"", reopenHtml, StringComparison.Ordinal);
    }

    // ---- KK7: three 303s, the 404s ----

    [Fact]
    public async Task TheDefectClaimScreen_Is303WhenNotOpenNotResolvedOrPastZ18_And404ForAForeignItemOrNoClaim_KK7()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, string closedKey, _, _) = await SeedAsync(host, "Preparing", 1);
        (_, string openKey, Guid openItem, _) = await SeedAsync(host, "Shipped", 1);
        await SeedClaimAsync(host, openItem, DefectClaimStatus.Open, "Açık talep");
        DateOnly longAgo = TurkeyTime.DateOf(host.Time.GetUtcNow()).AddYears(-3);
        (_, string lateKey, Guid lateItem, _) = await SeedAsync(host, "Delivered", 1, longAgo);
        await SeedClaimAsync(host, lateItem, DefectClaimStatus.Resolved, "Eski talep");
        (_, string plainKey, Guid plainItem, _) = await SeedAsync(host, "Shipped", 1);
        (_, _, Guid foreignItem, _) = await SeedAsync(host, "Shipped", 1);

        using HttpResponseMessage notOpen = await host.Client.GetAsync(At(ClaimScreen(closedKey)), Ct);
        using HttpResponseMessage notResolved = await host.Client.GetAsync(At(ClaimScreen(openKey) + "?orderItemId=" + openItem), Ct);
        using HttpResponseMessage late = await host.Client.GetAsync(At(ClaimScreen(lateKey) + "?orderItemId=" + lateItem), Ct);
        using HttpResponseMessage foreign = await host.Client.GetAsync(At(ClaimScreen(plainKey) + "?orderItemId=" + foreignItem), Ct);
        using HttpResponseMessage noClaim = await host.Client.GetAsync(At(ClaimScreen(plainKey) + "?orderItemId=" + plainItem), Ct);
        using HttpResponseMessage notAKey = await host.Client.GetAsync(At(ClaimScreen(plainKey) + "?orderItemId=abc"), Ct);

        Assert.Equal(Closed(closedKey), (notOpen.StatusCode, notOpen.Headers.Location!.OriginalString));
        Assert.Equal(Closed(openKey), (notResolved.StatusCode, notResolved.Headers.Location!.OriginalString));
        Assert.Equal(Closed(lateKey), (late.StatusCode, late.Headers.Location!.OriginalString));
        Assert.Equal([HttpStatusCode.NotFound, HttpStatusCode.NotFound, HttpStatusCode.NotFound], new[] { foreign.StatusCode, noClaim.StatusCode, notAKey.StatusCode });
    }

    // ---- KK8: one claim, Açık, with the return address ----

    [Fact]
    public async Task SubmitDefectClaim_OpensTheOneClaimAsAcikWithTheReturnAddress_WithoutAConfirmationStep_KK8()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, string key, Guid itemId, _) = await SeedAsync(host, "Shipped", 2);

        using HttpResponseMessage submitted = await PostAsync(host, ClaimScreen(key), "SubmitDefectClaim", [new("OrderItemId", itemId.ToString()), new("Quantity", "2"), new("Description", "Çatlak")]);

        Assert.Equal((HttpStatusCode.SeeOther, OrderRoutes.AccessPath(key)), (submitted.StatusCode, submitted.Headers.Location!.OriginalString));
        await using AppDbContext db = host.NewContext();
        DefectClaim claim = await db.DefectClaims.SingleAsync(c => c.OrderItemId == itemId, Ct);
        Assert.Equal((DefectClaimStatus.Open, 2, "Çatlak", "Depo Sokak 3"), (claim.DefectClaimStatus, claim.Quantity, claim.Description, claim.ReturnAddressLine));

        // 04 5.16.5: the claim's record on the item's row carries its status badge.
        using HttpResponseMessage root = await host.Client.GetAsync(At(OrderRoutes.AccessPath(key)), Ct);
        string rootHtml = WebUtility.HtmlDecode(await root.Content.ReadAsStringAsync(Ct));
        Assert.Contains("<span class=\"badge badge--warning\">" + StatusBadges.For(DefectClaimStatus.Open).Text + "</span>", rootHtml, StringComparison.Ordinal);
    }

    // ---- KK9: three 422s, three 303s, the 404 ----

    [Fact]
    public async Task SubmitDefectClaim_Three422s_Three303s_AndTheForeignItems404_KK9()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, string key, Guid itemId, _) = await SeedAsync(host, "Shipped", 2);
        (_, string closedKey, Guid closedItem, _) = await SeedAsync(host, "Preparing", 2);
        (_, string claimedKey, Guid claimedItem, _) = await SeedAsync(host, "Shipped", 1);
        await SeedClaimAsync(host, claimedItem, DefectClaimStatus.Open, "Var olan");
        (_, _, Guid foreignItem, _) = await SeedAsync(host, "Shipped", 1);
        string screen = ClaimScreen(key);

        using HttpResponseMessage noItem = await PostAsync(host, screen, "SubmitDefectClaim", [new("Quantity", "1"), new("Description", "Kırık")]);
        using HttpResponseMessage zero = await PostAsync(host, screen, "SubmitDefectClaim", [new("OrderItemId", itemId.ToString()), new("Quantity", "0"), new("Description", "Kırık")]);
        using HttpResponseMessage noText = await PostAsync(host, screen, "SubmitDefectClaim", [new("OrderItemId", itemId.ToString()), new("Quantity", "1"), new("Description", "  ")]);
        using HttpResponseMessage exists = await PostAsync(host, ClaimScreen(claimedKey), "SubmitDefectClaim", [new("OrderItemId", claimedItem.ToString()), new("Description", "İkinci")]);
        using HttpResponseMessage notOpen = await PostAsync(host, ClaimScreen(closedKey), "SubmitDefectClaim", [new("OrderItemId", closedItem.ToString()), new("Description", "Kırık")]);
        using HttpResponseMessage exceeds = await PostAsync(host, screen, "SubmitDefectClaim", [new("OrderItemId", itemId.ToString()), new("Quantity", "5"), new("Description", "Kırık")]);
        using HttpResponseMessage foreign = await PostAsync(host, screen, "SubmitDefectClaim", [new("OrderItemId", foreignItem.ToString()), new("Description", "Kırık")]);

        Assert.Equal(
            [HttpStatusCode.UnprocessableEntity, HttpStatusCode.UnprocessableEntity, HttpStatusCode.UnprocessableEntity],
            new[] { noItem.StatusCode, zero.StatusCode, noText.StatusCode });
        Assert.Equal(Closed(claimedKey), (exists.StatusCode, exists.Headers.Location!.OriginalString));
        Assert.Equal(Closed(closedKey), (notOpen.StatusCode, notOpen.Headers.Location!.OriginalString));
        Assert.Equal((HttpStatusCode.SeeOther, NoticeParameter.Address(OrderRoutes.AccessPath(key), NoticeCode.QuantityExceedsOpen)), (exceeds.StatusCode, exceeds.Headers.Location!.OriginalString));
        Assert.Equal(HttpStatusCode.NotFound, foreign.StatusCode);
        await using AppDbContext db = host.NewContext();
        Assert.Equal(1, await db.DefectClaims.CountAsync(c => c.OrderItemId == itemId || c.OrderItemId == closedItem || c.OrderItemId == claimedItem || c.OrderItemId == foreignItem, Ct));
    }

    // ---- KK10: the reopening inside and outside Z-18 ----

    [Fact]
    public async Task ReopenDefectClaim_InsideZ18MakesItAcikWithTheCustomersLine_OutsideIs303_AndNoClaimIs404_KK10()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, string key, Guid itemId, _) = await SeedAsync(host, "Shipped", 3);
        await SeedClaimAsync(host, itemId, DefectClaimStatus.Resolved, "Kırık");
        DateOnly longAgo = TurkeyTime.DateOf(host.Time.GetUtcNow()).AddYears(-3);
        (_, string lateKey, Guid lateItem, _) = await SeedAsync(host, "Delivered", 1, longAgo);
        await SeedClaimAsync(host, lateItem, DefectClaimStatus.Resolved, "Eski");
        (_, string plainKey, Guid plainItem, _) = await SeedAsync(host, "Shipped", 1);

        using HttpResponseMessage reopened = await PostAsync(host, ClaimScreen(key), "ReopenDefectClaim", [new("OrderItemId", itemId.ToString()), new("Quantity", "2")]);
        using HttpResponseMessage again = await PostAsync(host, ClaimScreen(key), "ReopenDefectClaim", [new("OrderItemId", itemId.ToString())]);
        using HttpResponseMessage late = await PostAsync(host, ClaimScreen(lateKey), "ReopenDefectClaim", [new("OrderItemId", lateItem.ToString())]);
        using HttpResponseMessage noClaim = await PostAsync(host, ClaimScreen(plainKey), "ReopenDefectClaim", [new("OrderItemId", plainItem.ToString())]);

        Assert.Equal((HttpStatusCode.SeeOther, OrderRoutes.AccessPath(key)), (reopened.StatusCode, reopened.Headers.Location!.OriginalString));
        Assert.Equal(Closed(key), (again.StatusCode, again.Headers.Location!.OriginalString));
        Assert.Equal(Closed(lateKey), (late.StatusCode, late.Headers.Location!.OriginalString));
        Assert.Equal(HttpStatusCode.NotFound, noClaim.StatusCode);
        await using AppDbContext db = host.NewContext();
        DefectClaim claim = await db.DefectClaims.SingleAsync(c => c.OrderItemId == itemId, Ct);
        Assert.Equal((DefectClaimStatus.Open, 2), (claim.DefectClaimStatus, claim.Quantity));
        DefectClaimHistoryEntry last = claim.StatusHistory[^1];
        Assert.Equal((DefectClaimStatus.Open, ActorKind.Buyer, (int?)2), (last.Status, last.ActorKind, last.Quantity));
        Assert.Equal(DefectClaimStatus.Resolved, (await db.DefectClaims.SingleAsync(c => c.OrderItemId == lateItem, Ct)).DefectClaimStatus);
    }

    // ---- helpers ----

    private static string Withdrawal(string key) => OrderRoutes.ScreenPath(OrderRoutes.AccessPath(key), "cayma");

    private static string ClaimScreen(string key) => OrderRoutes.ScreenPath(OrderRoutes.AccessPath(key), "ayip-talebi");

    private static (HttpStatusCode, string) Closed(string key) => (HttpStatusCode.SeeOther, NoticeParameter.Address(OrderRoutes.AccessPath(key), NoticeCode.OperationClosed));

    /// <summary>A guest transfer order, paid, in <paramref name="status"/>, with one physical item of <paramref name="quantity"/> units; the delivery date when given.</summary>
    private static async Task<(Guid OrderId, string Key, Guid ItemId, string Name)> SeedAsync(FileEndpointsHost host, string status, int quantity, DateOnly? deliveryDate = null)
    {
        DateTimeOffset now = host.Time.GetUtcNow();
        string n = Guid.NewGuid().ToString("N")[..8];
        string name = "Kupa " + n;
        await using AppDbContext db = host.NewContext();
        Product product = Product.Create(name, "kupa-" + n, ProductType.Physical, 20m, now).WithSeededOrigin();
        Variant variant = Variant.Create(product.Id, "SKU-" + n, 10m, now, 5);
        db.Products.Add(product);
        db.Variants.Add(variant);
        await db.SaveChangesAsync(Ct);
        (Guid orderId, _, string key) = await TestSessions.OrderAsync(host.Dsn, null);
        OrderItem item = OrderItem.Create(orderId, product.Id, variant.Id, ProductType.Physical, name, quantity, 100m, 20m, 16.67m, now);
        if (deliveryDate is { } day)
        {
            item.DeliveryDate = day;
            item.DeliveryMarkedAt = now;
        }

        db.OrderItems.Add(item);
        await db.SaveChangesAsync(Ct);
        await db.Database.ExecuteSqlAsync($"UPDATE orders SET payment_status = 'Paid', order_status = {status} WHERE id = {orderId}", Ct);
        return (orderId, key, item.Id, name);
    }

    private static async Task SeedClaimAsync(FileEndpointsHost host, Guid itemId, DefectClaimStatus status, string description)
    {
        DateTimeOffset now = host.Time.GetUtcNow();
        await using AppDbContext db = host.NewContext();
        DefectClaim claim = DefectClaim.Create(itemId, 1, description, null, now.AddDays(-1));
        if (status == DefectClaimStatus.Resolved)
        {
            claim.ApplyStatus(DefectClaimStatus.Resolved, ActorKind.Admin, now.AddHours(-1), null);
        }

        db.DefectClaims.Add(claim);
        await db.SaveChangesAsync(Ct);
    }

    private static KeyValuePair<string, string>[] Row(int index, Guid itemId, string quantity, int originalOpen) =>
    [
        new($"Items[{index}].OrderItemId", itemId.ToString()),
        new($"Items[{index}].Quantity", quantity),
        new($"Items[{index}].OriginalOpenQuantity", originalOpen.ToString(CultureInfo.InvariantCulture)),
    ];

    private static async Task<HttpResponseMessage> PostAsync(FileEndpointsHost host, string path, string handler, KeyValuePair<string, string>[] fields)
    {
        using FormUrlEncodedContent form = new(fields);
        return await host.Client.PostAsync(At(path + "?handler=" + handler), form, Ct);
    }

    private static async Task<int> WithdrawalCountAsync(FileEndpointsHost host, Guid itemId)
    {
        await using AppDbContext db = host.NewContext();
        return await db.Withdrawals.CountAsync(w => w.OrderItemId == itemId, Ct);
    }

    private static async Task SetReturnAddressLineAsync(FileEndpointsHost host, string line)
    {
        await using AppDbContext db = host.NewContext();
        CompanySettings settings = await db.CompanySettings.SingleAsync(Ct);
        settings.ReturnAddressLine = line;
        await db.SaveChangesAsync(Ct);
    }
}
