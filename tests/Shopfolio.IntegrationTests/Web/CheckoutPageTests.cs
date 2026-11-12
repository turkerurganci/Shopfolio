using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Coupons;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Storefront;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the payment step's page tests (T509): one database copied from the template.</summary>
[CollectionDefinition(CheckoutPageTestsDefinition.Name)]
public sealed class CheckoutPageTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "CheckoutPageTests";
}

/// <summary>
/// The payment step on the opened application (T509 KK1-KK10; 07 UC-22…UC-27; 04 E-13): the GET's fresh key and carried values, the three reasons the step does not open on the four ends, the
/// cookie of the leaving, the coupon's three answers, the recalculation, the confirmation's 303s and 422s, and the confirmation key's repeat. The firm, the texts, the province, the products and
/// the carts are real rows; the sales gate is opened by a proxy except where a test keeps it closed. The bank transfer is the open method (the test installation has no card keys).
/// </summary>
[Collection(CheckoutPageTestsDefinition.Name)]
public sealed partial class CheckoutPageTests : IAsyncLifetime
{
    private const string Iban = "TR330006100519786457841326";

    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow.AddDays(-3);

    private readonly CollectionDatabaseFixture _database;

    private int _counter;
    private Guid _provinceId;
    private Guid _unservedProvinceId;

    public CheckoutPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE dispatch_records, stock_reservations, transition_records, cart_items, carts, order_items, orders, coupons, variants, products, "
            + "legal_text_versions, companies, company_settings, provinces CASCADE");
        Province province = Province.Create("06", "Ankara", Now);
        Province unserved = Province.Create("34", "İstanbul", Now);
        CompanySettings settings = CompanySettings.Create(Now);
        settings.BankTransferPaymentWindow = 2;
        settings.DefaultShippingPeriod = 2;
        settings.DownloadAllowance = 1;
        settings.IsBankTransferEnabled = true;
        settings.BankTransferIban = Iban;
        settings.HomepageLayout = HomepageLayout.StoreFirst;
        settings.MenuNameProducts = "Ürünler";
        settings.MenuNameOfferings = "Hizmetler";
        settings.MenuNamePortfolioItems = "Çalışmalar";
        settings.MenuNameAboutPage = "Hakkımızda";
        settings.MenuNameFaqItems = "Sıkça Sorulanlar";
        settings.MenuNameContact = "İletişim";
        Company company = Company.Create(Now);
        company.CompanyType = CompanyType.Tradesman;
        company.LegalName = "Örnek Ticaret";
        company.TaxIdNumber = "1234567890";
        company.ProvinceId = province.Id;
        company.District = "Çankaya";
        company.AddressLine = "Kızılay 2";
        company.Phone = "3121234567";
        company.Email = "firma@example.com";
        company.KepAddress = "firma@hs01.kep.tr";
        company.ChamberName = "Ankara Esnaf Odası";
        company.ProfessionalConductRules = "Meslek kuralları";
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, 1, Now, "ön bilgilendirme metni", Now);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, 1, Now, "mesafeli satış metni", Now);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, 1, Now, "aydınlatma metni", Now).WithHosting();
        LegalTextVersion cookies = LegalTextVersion.CreatePublished(LegalTextType.CookiePolicy, 1, Now, "çerez metni", Now);
        await using (AppDbContext db = NewContext())
        {
            db.Provinces.AddRange(province, unserved);
            db.CompanyDeliveryProvinces.Add(CompanyDeliveryProvince.Create(province.Id, Now));
            db.CompanySettings.Add(settings);
            db.Companies.Add(company);
            db.LegalTextVersions.AddRange(form, contract, notice, cookies);
            await db.SaveChangesAsync(Ct);
        }

        _provinceId = province.Id;
        _unservedProvinceId = unserved.Id;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the sections by the cart, a fresh key at every showing, the carried values; the guest's e-mail in the summary ----

    [Fact]
    public async Task TheGet_DrawsTheSectionsOfTheCart_WithAFreshKey_TheFingerprintAndTheVersions_T509_KK1()
    {
        string guest = await SeedGuestCartAsync(ProductType.Physical);
        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);

        (HttpStatusCode status, string first, _) = await GetAsync(factory, GuestCart(guest));
        (_, string second, _) = await GetAsync(factory, GuestCart(guest));

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("id=\"checkout-shipping\"", first, StringComparison.Ordinal);
        Assert.Contains("id=\"checkout-billing\"", first, StringComparison.Ordinal);
        Assert.Contains("name=\"ContactEmail\"", first, StringComparison.Ordinal);
        Assert.Contains("noindex", first, StringComparison.Ordinal);
        Assert.NotEqual(Hidden(first, "ConfirmationKey"), Hidden(second, "ConfirmationKey"));
        Assert.False(string.IsNullOrEmpty(Hidden(first, nameof(PlaceOrderInput.SummaryFingerprint))));
        Assert.NotNull(CheckoutForms.ReadFingerprint(Hidden(first, nameof(PlaceOrderInput.SummaryFingerprint))));
        Assert.True(Guid.TryParse(Hidden(first, nameof(PlaceOrderInput.PreInformationFormVersionId)), out _));
        Assert.True(Guid.TryParse(Hidden(first, nameof(PlaceOrderInput.DistanceSalesContractVersionId)), out _));

        // A digital-only cart has no shipping section.
        string digital = await SeedGuestCartAsync(ProductType.Digital);
        (_, string digitalHtml, _) = await GetAsync(factory, GuestCart(digital));
        Assert.DoesNotContain("id=\"checkout-shipping\"", digitalHtml, StringComparison.Ordinal);

        // The guest's e-mail is in the confirmation summary and is not sent a code (02 3.24.5).
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage recalculated = await PostAsync(client, "Recalculate", [.. Fields(first, guest: true, email: "misafir@example.com")], GuestCart(guest));
        Assert.Equal(HttpStatusCode.OK, recalculated.StatusCode);
        Assert.Contains("misafir@example.com", await BodyOf(recalculated), StringComparison.Ordinal);
    }

    // ---- KK2: three reasons, four ends ----

    [Fact]
    public async Task TheStepThatCannotOpen_SendsTheGetAndTheThreeSubmissionsToTheCart_T509_KK2()
    {
        string closedSale = await SeedGuestCartAsync(ProductType.Physical);
        string soldOut = await SeedGuestCartAsync(ProductType.Physical, stock: 0);
        string underMinimum = await SeedGuestCartAsync(ProductType.Physical, price: 10m);

        await using WebApplicationFactory<Program> closed = Open(salesOpen: false);
        await AssertAllToCartAsync(closed, closedSale);

        await using WebApplicationFactory<Program> open = Open(salesOpen: true);
        await AssertAllToCartAsync(open, soldOut);
        await SetMinimumAsync(50m);
        await AssertAllToCartAsync(open, underMinimum);
    }

    // ---- KK3 and KK6: the leaving writes the cookie with the 05 6.7.9 fields; three targets, an empty form too; the cookie fills the fields ----

    [Fact]
    public async Task TheLeaving_WritesTheCookieWithoutValidating_AndGoesToItsTarget_AndTheGetFillsTheFields_T509_KK3_KK6()
    {
        string guest = await SeedGuestCartAsync(ProductType.Physical);
        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage login = await PostAsync(client, "Leave", [("ContactEmail", "yarim@"), ("ShippingDistrict", "Çankaya"), ("AcceptPreInformation", "true"), ("LeaveTarget", "login")], GuestCart(guest));
        using HttpResponseMessage cart = await PostAsync(client, "Leave", [("LeaveTarget", "cart")], GuestCart(guest));
        using HttpResponseMessage other = await PostAsync(client, "Leave", [("LeaveTarget", "https://example.org/")], GuestCart(guest));
        using HttpResponseMessage empty = await PostAsync(client, "Leave", [], GuestCart(guest));

        Assert.Equal(HttpStatusCode.SeeOther, login.StatusCode);
        Assert.Equal(CheckoutModel.SignInPath, login.Headers.Location?.OriginalString);
        Assert.Equal("/giris?returnUrl=/odeme", CheckoutModel.SignInPath);
        foreach (HttpResponseMessage response in new[] { cart, other, empty })
        {
            Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
            Assert.Equal(FramePaths.Cart, response.Headers.Location?.OriginalString);
        }

        string written = CheckoutCookieOf(login);
        (HttpStatusCode status, string html, _) = await GetAsync(factory, GuestCart(guest), written);
        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("value=\"yarim@\"", html, StringComparison.Ordinal);
        Assert.Contains("value=\"Çankaya\"", html, StringComparison.Ordinal);

        // The boxes are not kept: no box comes ticked from the cookie (K-756).
        Assert.DoesNotMatch(TickedBox(), html);
    }

    [Fact]
    public async Task TheReturnToTheCart_WithNoUsableMethod_MarksTheCart_T509_KK6()
    {
        string guest = await SeedGuestCartAsync(ProductType.Physical);
        await ExecuteAsync("UPDATE company_settings SET is_bank_transfer_enabled = false");
        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage cart = await PostAsync(client, "Leave", [("LeaveTarget", "cart")], GuestCart(guest));

        Assert.Equal(HttpStatusCode.SeeOther, cart.StatusCode);
        await using AppDbContext db = NewContext();
        Assert.True(await db.Carts.Where(c => c.GuestCartKeyHash == SecretKey.Hash(guest)).Select(c => c.IsPaymentUnavailableNoticePending).SingleAsync(Ct));
    }

    // ---- KK4 and KK5: the coupon's three answers, the removal, the recalculation ----

    [Fact]
    public async Task TheCoupon_IsAppliedWithoutWriting_RefusedWith422_AndRemoved_T509_KK4_KK5()
    {
        string guest = await SeedGuestCartAsync(ProductType.Physical, price: 100m);
        await SeedCouponAsync("T509K", minimumBasket: null);
        await SeedCouponAsync("T509MIN", minimumBasket: 500m);
        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string page = (await GetAsync(factory, GuestCart(guest))).Html;

        using HttpResponseMessage applied = await PostAsync(client, "ApplyCoupon", [.. Fields(page, guest: true), ("CouponCode", "t509k")], GuestCart(guest));
        string appliedHtml = await BodyOf(applied);
        using HttpResponseMessage rejected = await PostAsync(client, "ApplyCoupon", [.. Fields(page, guest: true), ("CouponCode", "YOKBOYLE")], GuestCart(guest));
        using HttpResponseMessage below = await PostAsync(client, "ApplyCoupon", [.. Fields(page, guest: true), ("CouponCode", "T509MIN")], GuestCart(guest));
        using HttpResponseMessage removed = await PostAsync(client, "RemoveCoupon", [.. Fields(page, guest: true), ("CouponCode", "T509K")], GuestCart(guest));

        Assert.Equal(HttpStatusCode.OK, applied.StatusCode);
        Assert.Contains("T509K", appliedHtml, StringComparison.Ordinal);
        Assert.Contains(DisplayFormat.Amount(-10m), appliedHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rejected.StatusCode);
        Assert.Contains(CouponFields.RejectedMessage, await BodyOf(rejected), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, below.StatusCode);
        Assert.Contains(CouponFields.BelowMinimumText(500m), await BodyOf(below), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, removed.StatusCode);
        Assert.DoesNotContain("coupon__applied", await BodyOf(removed), StringComparison.Ordinal);

        // Nothing is reserved and no order is born by the coupon's submissions (07 UC-23).
        Assert.Equal(0, await ScalarAsync("SELECT count(*) FROM orders"));
        Assert.Equal(0, await ScalarAsync("SELECT coalesce(sum(used_count + reserved_count), 0) FROM coupons"));
    }

    [Fact]
    public async Task TheCouponAttempts_AreClosedByL6With429_T509_KK4()
    {
        string guest = await SeedGuestCartAsync(ProductType.Physical);
        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string page = (await GetAsync(factory, GuestCart(guest))).Html;

        HttpStatusCode last = HttpStatusCode.OK;
        for (int i = 0; i < 30 && last != HttpStatusCode.TooManyRequests; i++)
        {
            using HttpResponseMessage response = await PostAsync(client, "ApplyCoupon", [.. Fields(page, guest: true), ("CouponCode", $"YOK{i}")], GuestCart(guest));
            last = response.StatusCode;
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, last);
    }

    [Fact]
    public async Task TheRecalculation_ChecksNoRequiredField_TellsAnUnservedProvince_AndTheRegisteredEmailReminder_T509_KK5()
    {
        string guest = await SeedGuestCartAsync(ProductType.Physical);
        (_, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        string registered = await ScalarTextAsync($"SELECT email FROM customers WHERE id = '{customerId}'");
        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string page = (await GetAsync(factory, GuestCart(guest))).Html;

        using HttpResponseMessage response = await PostAsync(
            client, "Recalculate", [.. Fields(page, guest: true, email: registered), ("ShippingProvinceId", _unservedProvinceId.ToString())], GuestCart(guest));
        string html = await BodyOf(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(AddressMessages.NotDelivered("İstanbul"), html, StringComparison.Ordinal);
        Assert.Contains(CheckoutTexts.RegisteredEmailReminder, html, StringComparison.Ordinal);
        // Only the province's message counts; the empty required fields of the form are not checked.
        Assert.Contains("<span>1 alanda hata var</span>", html, StringComparison.Ordinal);
    }

    // ---- KK7 and KK10: one transaction, the cookie deleted; the same key opens no second order ----

    [Fact]
    public async Task TheConfirmation_PlacesTheOrder_GoesToItsConfirmation_DeletesTheCookie_AndTheRepeatOpensNoSecondOrder_T509_KK7_KK10()
    {
        string guest = await SeedGuestCartAsync(ProductType.Physical);
        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string page = (await GetAsync(factory, GuestCart(guest))).Html;
        (string, string)[] form = [.. Complete(page)];

        using HttpResponseMessage placed = await PostAsync(client, "PlaceOrder", form, GuestCart(guest));
        using HttpResponseMessage repeat = await PostAsync(client, "PlaceOrder", form, GuestCart(guest));

        Assert.Equal(HttpStatusCode.SeeOther, placed.StatusCode);
        string location = placed.Headers.Location?.OriginalString ?? string.Empty;
        Assert.StartsWith("/siparis/erisim/", location, StringComparison.Ordinal);
        Assert.EndsWith("/teyit", location, StringComparison.Ordinal);
        Assert.Contains(SetCookies(placed), c => c.StartsWith(CookieNames.Checkout + "=", StringComparison.Ordinal) && IsDeletion(c));
        Assert.Equal(HttpStatusCode.SeeOther, repeat.StatusCode);
        Assert.Equal(location, repeat.Headers.Location?.OriginalString);
        Assert.Equal(1, await ScalarAsync("SELECT count(*) FROM orders"));
    }

    [Fact]
    public async Task TwoTabs_WithTwoKeys_AreQueuedByTheCartsLock_AndTheSecondCancelsTheFirstsUnpaidOrder_T509_KK10()
    {
        string guest = await SeedGuestCartAsync(ProductType.Physical);
        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string first = (await GetAsync(factory, GuestCart(guest))).Html;
        string second = (await GetAsync(factory, GuestCart(guest))).Html;

        Task<HttpResponseMessage> one = PostAsync(client, "PlaceOrder", [.. Complete(first)], GuestCart(guest));
        Task<HttpResponseMessage> two = PostAsync(client, "PlaceOrder", [.. Complete(second)], GuestCart(guest));
        HttpResponseMessage[] responses = await Task.WhenAll(one, two);

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.SeeOther, r.StatusCode));
        Assert.Equal(2, await ScalarAsync("SELECT count(*) FROM orders"));
        Assert.Equal(1, await ScalarAsync("SELECT count(*) FROM orders WHERE order_status = 'Cancelled'"));
        foreach (HttpResponseMessage response in responses)
        {
            response.Dispose();
        }
    }

    // ---- KK8: the 422s and the 404 ----

    [Fact]
    public async Task TheConfirmation_RefusesMissingFieldsAndBoxesTogether_With422_AndNoOrder_T509_KK8()
    {
        string guest = await SeedGuestCartAsync(ProductType.Physical);
        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string page = (await GetAsync(factory, GuestCart(guest))).Html;

        using HttpResponseMessage response = await PostAsync(client, "PlaceOrder", [.. Fields(page, guest: true), ("PaymentMethod", "BankTransfer")], GuestCart(guest));
        string html = await BodyOf(response);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains("id=\"form-error-summary\"", html, StringComparison.Ordinal);
        Assert.Contains("aria-invalid=\"true\"", html, StringComparison.Ordinal);
        Assert.Matches(new Regex("id=\"AcceptPreInformation\"[^>]*aria-invalid=\"true\""), html);
        Assert.Equal(0, await ScalarAsync("SELECT count(*) FROM orders"));
    }

    [Fact]
    public async Task TheConfirmation_RefusesAnUnservedProvince_AChangedSummary_AndAnUnusableMethod_With422_T509_KK8()
    {
        string guest = await SeedGuestCartAsync(ProductType.Physical);
        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string page = (await GetAsync(factory, GuestCart(guest))).Html;

        using HttpResponseMessage province = await PostAsync(client, "PlaceOrder", [.. Complete(page, province: _unservedProvinceId)], GuestCart(guest));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, province.StatusCode);
        Assert.Contains(AddressMessages.NotDelivered("İstanbul"), await BodyOf(province), StringComparison.Ordinal);

        await ExecuteAsync("UPDATE variants SET price = price + 1");
        using HttpResponseMessage changed = await PostAsync(client, "PlaceOrder", [.. Complete(page)], GuestCart(guest));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, changed.StatusCode);
        Assert.Contains(CheckoutTexts.SummaryChangedGeneral, await BodyOf(changed), StringComparison.Ordinal);

        string fresh = (await GetAsync(factory, GuestCart(guest))).Html;
        using HttpResponseMessage card = await PostAsync(client, "PlaceOrder", [.. Complete(fresh, method: "Card")], GuestCart(guest));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, card.StatusCode);
        Assert.Equal(0, await ScalarAsync("SELECT count(*) FROM orders"));
    }

    [Fact]
    public async Task TheConfirmation_RefusesTheClosedSale_With422_AndAForeignBookEntry_With404_T509_KK8()
    {
        string guest = await SeedGuestCartAsync(ProductType.Physical);
        await using WebApplicationFactory<Program> open = Open(salesOpen: true);
        using HttpClient openClient = ForgeryTokenHandler.CreateClient(open);
        string page = (await GetAsync(open, GuestCart(guest))).Html;

        await using WebApplicationFactory<Program> closing = Open(salesOpen: false);
        using HttpClient closingClient = ForgeryTokenHandler.CreateClient(closing);
        using HttpResponseMessage closed = await PostAsync(closingClient, "PlaceOrder", [.. Complete(page)], GuestCart(guest));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, closed.StatusCode);
        Assert.Contains(SalesClosedMessages.StorefrontText, await BodyOf(closed), StringComparison.Ordinal);

        (string memberKey, Guid memberId) = await TestSessions.CustomerAsync(_database.Dsn);
        await SeedMemberCartAsync(memberId);
        string memberPage = (await GetAsync(open, Customer(memberKey))).Html;
        using HttpResponseMessage foreign = await PostAsync(
            openClient, "PlaceOrder", [.. Complete(memberPage, guest: false), ("AddressBookId", Guid.NewGuid().ToString())], Customer(memberKey));
        Assert.Equal(HttpStatusCode.NotFound, foreign.StatusCode);
        Assert.Equal(0, await ScalarAsync("SELECT count(*) FROM orders"));
    }

    // ---- KK9: the 303s ----

    [Fact]
    public async Task TheConfirmation_SendsTheCartsLostLinesToTheCart_AndAClosedMemberSessionToTheSignIn_T509_KK9()
    {
        string guest = await SeedGuestCartAsync(ProductType.Physical);
        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string page = (await GetAsync(factory, GuestCart(guest))).Html;

        await ExecuteAsync("UPDATE variants SET stock_quantity = 0");
        using HttpResponseMessage noItem = await PostAsync(client, "PlaceOrder", [.. Complete(page)], GuestCart(guest));
        Assert.Equal(HttpStatusCode.SeeOther, noItem.StatusCode);
        Assert.Equal(FramePaths.Cart, noItem.Headers.Location?.OriginalString);

        // A member's form (no e-mail field) without the session: the form is kept and the sign-in is next (07 4.13; K-1181 (4)).
        using HttpResponseMessage closed = await PostAsync(client, "PlaceOrder", [("ShippingDistrict", "Çankaya"), ("PaymentMethod", "BankTransfer")], GuestCart(guest));
        Assert.Equal(HttpStatusCode.SeeOther, closed.StatusCode);
        Assert.Equal(CheckoutModel.SignInPath, closed.Headers.Location?.OriginalString);
        Assert.Contains(SetCookies(closed), c => c.StartsWith(CookieNames.Checkout + "=", StringComparison.Ordinal) && !IsDeletion(c));
        Assert.Equal(0, await ScalarAsync("SELECT count(*) FROM orders"));
    }

    [Fact]
    public async Task TheConfirmation_UnderTheMinimumAmount_GoesToTheCart_T509_KK9()
    {
        string guest = await SeedGuestCartAsync(ProductType.Physical, price: 100m);
        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string page = (await GetAsync(factory, GuestCart(guest))).Html;

        await SetMinimumAsync(500m);
        using HttpResponseMessage response = await PostAsync(client, "PlaceOrder", [.. Complete(page)], GuestCart(guest));

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal(FramePaths.Cart, response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task AnUnexpectedError_InTheConfirmation_SendsTheGuestToTheOrderTracking_AndTheMemberToTheirOrders_T509_KK9()
    {
        string guest = await SeedGuestCartAsync(ProductType.Physical);
        (string memberKey, Guid memberId) = await TestSessions.CustomerAsync(_database.Dsn);
        await SeedMemberCartAsync(memberId);
        await using WebApplicationFactory<Program> factory = Open(salesOpen: true, confirmationFails: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string guestPage = (await GetAsync(factory, GuestCart(guest))).Html;
        string memberPage = (await GetAsync(factory, Customer(memberKey))).Html;

        // 09 8.1 item 4; 07 6.4.2: the marked handler's error is the layer's 303 to the result screen, never "nothing was done"; the open transaction is rolled back.
        using HttpResponseMessage guestAnswer = await PostAsync(client, "PlaceOrder", [.. Complete(guestPage)], GuestCart(guest));
        using HttpResponseMessage memberAnswer = await PostAsync(client, "PlaceOrder", [.. Complete(memberPage, guest: false)], Customer(memberKey));

        Assert.Equal(HttpStatusCode.SeeOther, guestAnswer.StatusCode);
        Assert.Equal(FramePaths.OrderTracking, guestAnswer.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.SeeOther, memberAnswer.StatusCode);
        Assert.Equal(FramePaths.AccountOrders, memberAnswer.Headers.Location?.OriginalString);
        Assert.Equal(0, await ScalarAsync("SELECT count(*) FROM orders"));
    }

    [Fact]
    public async Task AFullOpenUnpaidCeiling_RefusesTheNextConfirmationWith429_T509_KK8()
    {
        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        HttpStatusCode last = HttpStatusCode.OK;
        for (int i = 0; i <= ProductConstants.OpenUnpaidOrdersMax; i++)
        {
            string guest = await SeedGuestCartAsync(ProductType.Physical);
            string page = (await GetAsync(factory, GuestCart(guest))).Html;
            using HttpResponseMessage response = await PostAsync(client, "PlaceOrder", [.. Complete(page)], GuestCart(guest));
            last = response.StatusCode;
            Assert.Equal(i < ProductConstants.OpenUnpaidOrdersMax ? HttpStatusCode.SeeOther : HttpStatusCode.TooManyRequests, last);
        }

        // L-8 (02 8.2.3): the bank transfers of one IP stop at the ceiling; the refused confirmation opens no order.
        Assert.Equal(HttpStatusCode.TooManyRequests, last);
        Assert.Equal(ProductConstants.OpenUnpaidOrdersMax, await ScalarAsync("SELECT count(*) FROM orders"));
    }

    [GeneratedRegex("type=\"checkbox\"[^>]*name=\"Accept[A-Za-z]+\"[^>]*checked")]
    private static partial Regex TickedBox();
}
