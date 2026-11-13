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
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
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
        string recalculatedHtml = await BodyOf(recalculated);
        Assert.Contains("misafir@example.com", recalculatedHtml, StringComparison.Ordinal);
        Assert.Contains("class=\"checkout__email-line\">", recalculatedHtml, StringComparison.Ordinal);
        Assert.Contains("href=\"#ContactEmail\"", recalculatedHtml, StringComparison.Ordinal); // D-729: the summary's e-mail line links to the field it is corrected in
    }

    // 04 5.13.9 (b)-(d), (h), 5.13.32 (K-756): the two legal texts are open in a scrolling box, never folded; the guest's e-mail line is in the summary, not between the boxes and the button.
    [Fact]
    public async Task TheConfirmation_ShowsBothLegalTextsOpenInAScrollingBox_AndTheEmailLineInTheSummary_T509_KK1()
    {
        string guest = await SeedGuestCartAsync(ProductType.Physical);
        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);
        (_, string page, _) = await GetAsync(factory, GuestCart(guest));

        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage recalculated = await PostAsync(client, "Recalculate", [.. Fields(page, guest: true, email: "misafir@example.com")], GuestCart(guest));
        string html = await BodyOf(recalculated);

        Assert.DoesNotContain("<details", html, StringComparison.Ordinal);
        Assert.DoesNotContain("<summary", html, StringComparison.Ordinal);
        string form = LegalBox(html, "checkout-pre-information");
        string contract = LegalBox(html, "checkout-distance-sales");
        Assert.False(string.IsNullOrWhiteSpace(form), "the form's whole text is in its box");
        Assert.False(string.IsNullOrWhiteSpace(contract), "the contract's whole text is in its box");
        Assert.NotEqual(form, contract);

        int summaryText = html.IndexOf("class=\"checkout__summary-text\"", StringComparison.Ordinal);
        int emailLine = html.IndexOf("class=\"checkout__email-line\"", StringComparison.Ordinal);
        int firstBox = html.IndexOf("class=\"checkout__legal-box\"", StringComparison.Ordinal);
        int consents = html.IndexOf("class=\"consents\"", StringComparison.Ordinal);
        int consentsEnd = html.IndexOf("</fieldset>", consents, StringComparison.Ordinal);
        int total = html.IndexOf("class=\"checkout__total\"", consentsEnd, StringComparison.Ordinal);
        Assert.True(summaryText >= 0 && summaryText < emailLine && emailLine < firstBox && firstBox < consents, "the e-mail line follows the summary and comes before the legal texts and the boxes");
        Assert.Equal(1, Regex.Count(html, "class=\"checkout__email-line\""));
        Assert.DoesNotContain("checkout__email-line", html[consentsEnd..total], StringComparison.Ordinal);
    }

    // ---- KK2: three reasons, four ends ----

    [Fact]
    public async Task TheStepThatCannotOpen_SendsTheGetAndTheThreeSubmissionsToTheCart_T509_KK2()
    {
        string closedSale = await SeedGuestCartAsync(ProductType.Physical);
        string soldOut = await SeedGuestCartAsync(ProductType.Physical, stock: 0);
        string underMinimum = await SeedGuestCartAsync(ProductType.Physical, price: 10m);

        await using WebApplicationFactory<Program> closed = Open(salesOpen: false);
        Assert.Equal(AllToTheCart(), await AnswersOfTheStepAsync(closed, closedSale));

        await using WebApplicationFactory<Program> open = Open(salesOpen: true);
        Assert.Equal(AllToTheCart(), await AnswersOfTheStepAsync(open, soldOut));
        await SetMinimumAsync(50m);
        Assert.Equal(AllToTheCart(), await AnswersOfTheStepAsync(open, underMinimum));
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

    // ---- VAL-E061 (D-1077, D-1080; 04 5.13.8; 07 4.13.1; 08 2.1.8): the card the provider could not be reached for stays in the list, unselectable, with its reason ----

    [Fact]
    public async Task TheCardUnavailableNow_StaysInTheListUnselectableWithItsReason_AndItsConfirmationIsRefusedWith422_VAL_E061()
    {
        Regex disabledCard = new("<input type=\"radio\" id=\"method-card\"[^>]*disabled=\"disabled\"");
        Regex checkedCard = new("<input type=\"radio\" id=\"method-card\"[^>]*checked");
        string reason = "<span class=\"field__reason\">" + CheckoutTexts.CardUnavailableNow + "</span>";
        string guest = await SeedGuestCartAsync(ProductType.Physical);
        await using WebApplicationFactory<Program> factory = Open(salesOpen: true, cardOpen: true);

        // The open card is chosen first and has no reason beside it.
        string usable = (await GetAsync(factory, GuestCart(guest))).Html;
        Assert.Matches(checkedCard, usable);
        Assert.DoesNotMatch(disabledCard, usable);
        Assert.DoesNotContain(reason, usable, StringComparison.Ordinal);

        // A token request's connection error: for two minutes the card stays in the list, cannot be chosen and says why; the transfer is chosen.
        factory.Services.GetRequiredService<CardUnavailability>().Mark();
        string page = (await GetAsync(factory, GuestCart(guest))).Html;
        Assert.Matches(disabledCard, page);
        Assert.DoesNotMatch(checkedCard, page);
        Assert.Contains(reason, page, StringComparison.Ordinal);
        Assert.Matches(new Regex("<input type=\"radio\" id=\"method-banktransfer\"[^>]*checked=\"checked\""), page);
        Assert.DoesNotContain(CheckoutTexts.NoMethod, page, StringComparison.Ordinal);

        // A forged card confirmation is refused with 422 without an order or a notice; the page draws the card unselectable again.
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage card = await PostAsync(client, "PlaceOrder", [.. Complete(page, method: "Card")], GuestCart(guest));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, card.StatusCode);
        string refused = await BodyOf(card);
        Assert.Contains(CheckoutTexts.MethodUnavailable, refused, StringComparison.Ordinal);
        Assert.Matches(disabledCard, refused);
        Assert.Equal(0, await ScalarAsync("SELECT count(*) FROM orders"));
        Assert.Equal(0, await ScalarAsync("SELECT count(*) FROM dispatch_records"));

        // With the transfer off the unavailable card is the only one listed: no method can be used, the cart's way back stands instead (04 5.13.21).
        await ExecuteAsync("UPDATE company_settings SET is_bank_transfer_enabled = false");
        await using WebApplicationFactory<Program> cardOnly = Open(salesOpen: true, cardOpen: true);
        cardOnly.Services.GetRequiredService<CardUnavailability>().Mark();
        string alone = (await GetAsync(cardOnly, GuestCart(guest))).Html;
        Assert.Matches(disabledCard, alone);
        Assert.Contains(reason, alone, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"method-banktransfer\"", alone, StringComparison.Ordinal);
        Assert.Contains(CheckoutTexts.NoMethod, alone, StringComparison.Ordinal);
    }

    // ---- VAL-A044 (D-627; 04 5.13.10, 5.13.26; 02 3.19.5) and its neighbour VAL-A043: the threshold's line under the fee; no shipping line without a physical item ----

    [Fact]
    public async Task TheSummary_TellsTheRemainingAmountToFreeShippingUnderTheFee_SaysFreeShippingOnce_AndADigitalCartHasNoShippingLine_VAL_A044()
    {
        const decimal fee = 30m;
        const decimal threshold = 500m;
        await ExecuteAsync(FormattableString.Invariant($"UPDATE company_settings SET shipping_fee = {fee}, free_shipping_threshold = {threshold}"));
        string under = await SeedGuestCartAsync(ProductType.Physical, price: 100m);
        string reached = await SeedGuestCartAsync(ProductType.Physical, price: 600m);
        string digital = await SeedGuestCartAsync(ProductType.Digital);
        await using WebApplicationFactory<Program> factory = Open(salesOpen: true);

        // Under the threshold: the fee, and right under it the module's own sentence ("Ücretsiz kargoya 400 TL kaldı").
        ShippingFeeSection terms = new(fee, threshold, null, 1);
        string remaining = ThresholdMessages.FreeShipping(ShippingCharge.Assess(terms, 100m, hasPhysicalItem: true))!.Text;
        string underHtml = (await GetAsync(factory, GuestCart(under))).Html;
        Assert.Matches(
            new Regex("<p class=\"checkout__shipping\">Kargo: " + Regex.Escape(DisplayFormat.Amount(fee)) + "</p>\\s*<p class=\"threshold-message threshold-message--free-shipping\">"
                + Regex.Escape(remaining) + "</p>"),
            underHtml);

        // At the threshold: the shipping line says "Kargo ücretsiz" once - no second threshold line, no "Kargo: Kargo ücretsiz".
        string reachedHtml = (await GetAsync(factory, GuestCart(reached))).Html;
        Assert.Contains("<p class=\"checkout__shipping\">" + ShippingAssessment.FreeShippingText + "</p>", reachedHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("threshold-message", reachedHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("Kargo: " + ShippingAssessment.FreeShippingText, reachedHtml, StringComparison.Ordinal);

        // Without a physical item there is no shipping line and no threshold line (04 5.13.26).
        string digitalHtml = (await GetAsync(factory, GuestCart(digital))).Html;
        Assert.DoesNotContain("checkout__shipping", digitalHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("threshold-message", digitalHtml, StringComparison.Ordinal);
    }

    // ---- D-853 (07 4.13.1; 04 5.13.8, 5.13.21; 03 3.2.1.21; UC-26): a full L-8 keeps the transfer in the list, unselectable, with the neutral message ----

    [Fact]
    public async Task AFullOpenUnpaidCeiling_KeepsTheTransferListedUnselectable_TheCardOpen_AndWithTheCardUnavailableLeavesNoMethod_T509_KK1()
    {
        Regex disabledTransfer = new("<input type=\"radio\" id=\"method-banktransfer\"[^>]*disabled=\"disabled\"");
        Regex checkedCard = new("<input type=\"radio\" id=\"method-card\"[^>]*checked");
        string reason = "<span class=\"field__reason\">" + NeutralMessages.LimitExceeded + "</span>";
        await using WebApplicationFactory<Program> transfer = Open(salesOpen: true);
        using HttpClient transferClient = ForgeryTokenHandler.CreateClient(transfer);
        string last = string.Empty;
        for (int i = 0; i < ProductConstants.OpenUnpaidOrdersMax; i++)
        {
            last = await SeedGuestCartAsync(ProductType.Physical);
            string open = (await GetAsync(transfer, GuestCart(last))).Html;
            using HttpResponseMessage placed = await PostAsync(transferClient, "PlaceOrder", [.. Complete(open)], GuestCart(last));
            Assert.Equal(HttpStatusCode.SeeOther, placed.StatusCode);
        }

        // The ceiling is full on this IP: the transfer stays in the list, cannot be chosen and says the neutral message; the open card is chosen.
        string guest = await SeedGuestCartAsync(ProductType.Physical);
        await using WebApplicationFactory<Program> factory = Open(salesOpen: true, cardOpen: true);
        string page = (await GetAsync(factory, GuestCart(guest))).Html;
        Assert.Matches(disabledTransfer, page);
        Assert.Contains(reason, page, StringComparison.Ordinal);
        Assert.Matches(checkedCard, page);
        Assert.DoesNotContain(CheckoutTexts.NoMethod, page, StringComparison.Ordinal);

        // The cart whose own unpaid order is one of them is not closed by it: the confirmation cancels that order before it counts (K-837).
        Assert.DoesNotMatch(disabledTransfer, (await GetAsync(factory, GuestCart(last))).Html);

        // A forged transfer confirmation is still the neutral 429 and opens no order.
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage forged = await PostAsync(client, "PlaceOrder", [.. Complete(page)], GuestCart(guest));
        Assert.Equal(HttpStatusCode.TooManyRequests, forged.StatusCode);
        Assert.Matches(disabledTransfer, await BodyOf(forged));
        Assert.Equal(ProductConstants.OpenUnpaidOrdersMax, await ScalarAsync("SELECT count(*) FROM orders"));

        // L-8 full and the card unreachable: no usable method - the cart's way back stands in the button's place, and the return marks the cart (04 5.13.21; UC-26).
        factory.Services.GetRequiredService<CardUnavailability>().Mark();
        string none = (await GetAsync(factory, GuestCart(guest))).Html;
        Assert.Matches(disabledTransfer, none);
        Assert.Contains(CheckoutTexts.NoMethod, none, StringComparison.Ordinal);
        Assert.Equal(1, CartReturns().Count(page));
        Assert.Equal(2, CartReturns().Count(none)); // the way back in the button's place, besides the page's own return link
        using HttpResponseMessage back = await PostAsync(client, "Leave", [("LeaveTarget", "cart")], GuestCart(guest));
        Assert.Equal(HttpStatusCode.SeeOther, back.StatusCode);
        await using AppDbContext db = NewContext();
        Assert.True(await db.Carts.Where(c => c.GuestCartKeyHash == SecretKey.Hash(guest)).Select(c => c.IsPaymentUnavailableNoticePending).SingleAsync(Ct));
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

    [Fact]
    public async Task AConfirmationWithoutAClientAddress_IsRefusedWithTheNeutral429_AndNoOrder_T509_KK8()
    {
        string guest = await SeedGuestCartAsync(ProductType.Physical);
        await using WebApplicationFactory<Program> factory = Open(salesOpen: true, hasAddress: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string page = (await GetAsync(factory, GuestCart(guest))).Html;

        // D-805: the limits count the address; a request without one is not a 500 and not an order.
        using HttpResponseMessage response = await PostAsync(client, "PlaceOrder", [.. Complete(page)], GuestCart(guest));

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        Assert.Equal(0, await ScalarAsync("SELECT count(*) FROM orders"));
    }

    [GeneratedRegex("type=\"checkbox\"[^>]*name=\"Accept[A-Za-z]+\"[^>]*checked")]
    private static partial Regex TickedBox();

    [GeneratedRegex("name=\"LeaveTarget\" value=\"cart\"")]
    private static partial Regex CartReturns();
}
