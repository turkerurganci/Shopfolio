using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the payment methods and the shipping, periods and defaults screens' tests (T536): one database copied from the template; every test sets the rows up again.</summary>
[CollectionDefinition(PaymentAndShippingPagesTestsDefinition.Name)]
public sealed class PaymentAndShippingPagesTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "PaymentAndShippingPages";
}

/// <summary>
/// E-45 and E-46 on the opened application and a real database (T536 criteria 1-10, KK1-KK10; 07 UC-100, UC-101, UC-167...UC-172; 04 9.17, 9.18): the reads, the confirmations with their numbers, the
/// 422 and 409 answers, the trail rows, F-5 to every administrator and the new versions of the two generated texts.
/// </summary>
[Collection(PaymentAndShippingPagesTestsDefinition.Name)]
public sealed partial class PaymentAndShippingPagesTests : IAsyncLifetime
{
    private const string PaymentPath = "/panel/odeme-yontemleri";

    private const string ShippingPath = "/panel/kargo-sureler-ve-varsayilanlar";

    private const string Iban = "TR330006100519786457841326";

    private const string OtherIban = "TR320010009999901234567890";

    private static readonly DateTimeOffset Now = new(2026, 11, 13, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _database;

    private string _adminKey = string.Empty;

    private Guid _ankara;

    private Guid _izmir;

    public PaymentAndShippingPagesTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [GeneratedRegex(@"<input[^>]*>", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex InputTag();

    [GeneratedRegex(@"value=""([^""]*)""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ValueAttribute();

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE provinces, companies, company_settings, legal_text_versions, audit_logs, dispatch_records, orders, admins CASCADE");
        Province ankara = Province.Create("06", "Ankara", Now);
        Province izmir = Province.Create("35", "İzmir", Now);
        _ankara = ankara.Id;
        _izmir = izmir.Id;
        CompanySettings settings = CompanySettings.Create(Now);
        settings.BankTransferPaymentWindow = 2;
        settings.DefaultShippingPeriod = 2;
        settings.DownloadAllowance = 5;
        settings.DefaultVatRate = 20m;
        settings.HomepageLayout = HomepageLayout.ContentFirst;
        settings.MenuNameProducts = MenuNameDefaults.Products;
        settings.MenuNameOfferings = MenuNameDefaults.Offerings;
        settings.MenuNamePortfolioItems = MenuNameDefaults.PortfolioItems;
        settings.MenuNameAboutPage = MenuNameDefaults.AboutPage;
        settings.MenuNameFaqItems = MenuNameDefaults.FaqItems;
        settings.MenuNameContact = MenuNameDefaults.Contact;
        Company company = Company.Create(Now);
        company.CompanyType = CompanyType.Tradesman;
        company.LegalName = "Yılmaz Ticaret";
        company.TaxIdNumber = "1234567890";
        company.District = "Çankaya";
        company.AddressLine = "Atatürk Bulvarı 1";
        company.Phone = "3121234567";
        company.Email = "iletisim@firma.com.tr";
        company.KepAddress = "firma@hs01.kep.tr";
        company.ChamberName = "Ankara Esnaf ve Sanatkarlar Odası";
        company.ProfessionalConductRules = "https://www.tobb.org.tr/etik-kurallar";
        company.ProvinceId = ankara.Id;
        await using (AppDbContext db = NewContext())
        {
            db.Provinces.AddRange(ankara, izmir);
            db.Companies.Add(company);
            db.CompanySettings.Add(settings);
            db.CompanyDeliveryProvinces.AddRange(CompanyDeliveryProvince.Create(ankara.Id, Now), CompanyDeliveryProvince.Create(izmir.Id, Now));
            await db.SaveChangesAsync(Ct);
        }

        _adminKey = (await TestSessions.AdminAsync(_database.Dsn)).Key;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the card line from the installation, the transfer switch, the IBAN in groups of four ----

    [Fact]
    public async Task ThePaymentScreen_ReadsTheCardLineFromTheInstallation_AndShowsTheIbanInGroupsOfFour_KK1()
    {
        await ExecuteAsync($"UPDATE company_settings SET is_bank_transfer_enabled = true, bank_transfer_iban = '{Iban}'");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, PaymentPath);

        Assert.Equal(HttpStatusCode.OK, status);
        bool cardOpen = await CardOpenAsync(factory);
        Assert.Contains(cardOpen ? PaymentMethodsModel.CardOpenText : PaymentMethodsModel.CardUndefinedText, html, StringComparison.Ordinal);
        Assert.Equal("TR33 0006 1005 1978 6457 8413 26", FieldValue(html, "Input.BankTransferIban"));
        Assert.Equal("1", FieldValue(html, "Input.PaymentMethodsVersion"));
        Assert.Matches(new Regex("id=\"transfer-switch\"[^>]*checked", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)), html);
        if (!cardOpen)
        {
            Assert.Contains(PaymentMethodsModel.OnlyTransferText, html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task ThePaymentScreen_WithBothMethodsClosed_SaysTheGateConditionIsNotMet_KK1()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string html) = await GetAsync(client, PaymentPath);

        if (!await CardOpenAsync(factory))
        {
            Assert.Contains(PaymentMethodsModel.NoMethodText, html, StringComparison.Ordinal);
        }

        Assert.DoesNotMatch(new Regex("id=\"transfer-switch\"[^>]*checked", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)), html);
        Assert.Equal(string.Empty, FieldValue(html, "Input.BankTransferIban"));
    }

    // ---- KK1: the four states of 04 9.17.11 with the card line bound both ways, so neither half depends on the test installation ----

    [Theory]
    [InlineData(true, false, null)]
    [InlineData(true, true, null)]
    [InlineData(false, true, PaymentMethodsModel.OnlyTransferText)]
    [InlineData(false, false, PaymentMethodsModel.NoMethodText)]
    public async Task ThePaymentScreen_ShowsEachOfTheFourStates_KK1(bool cardOpen, bool transferOpen, string? stateLine)
    {
        if (transferOpen)
        {
            await ExecuteAsync($"UPDATE company_settings SET is_bank_transfer_enabled = true, bank_transfer_iban = '{Iban}'");
        }

        await using WebApplicationFactory<Program> factory = Open().WithWebHostBuilder(b => b.ConfigureTestServices(s => s.AddScoped<ICardPaymentCondition>(_ => new FixedCardCondition(cardOpen))));
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, PaymentPath);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains(cardOpen ? PaymentMethodsModel.CardOpenText : PaymentMethodsModel.CardUndefinedText, html, StringComparison.Ordinal);
        Assert.DoesNotContain(cardOpen ? PaymentMethodsModel.CardUndefinedText : PaymentMethodsModel.CardOpenText, html, StringComparison.Ordinal);
        Assert.Equal(transferOpen, new Regex("id=\"transfer-switch\"[^>]*checked", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)).IsMatch(html));
        if (stateLine is null)
        {
            Assert.DoesNotContain("id=\"payment-state\"", html, StringComparison.Ordinal);
        }
        else
        {
            Assert.Contains(stateLine, html, StringComparison.Ordinal);
        }
    }

    // ---- KK2, KK4: opening and the first IBAN without a confirmation; F-5 to every administrator; the trail ----

    [Fact]
    public async Task OpeningTheTransferWithTheFirstIban_IsSavedWithoutAConfirmation_TellsEveryAdministrator_AndWritesTheTrail_KK2_KK4()
    {
        await AddAdminAsync("ikinci@example.com");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage saved = await client.PostAsync(PaymentPath + "?handler=SavePaymentMethods", PaymentForm(true, "TR33 0006 1005 1978 6457 8413 26", 1), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, saved.StatusCode);
        Assert.Equal(Iban, await ScalarAsync<string>("SELECT bank_transfer_iban FROM company_settings"));
        Assert.Equal(2, await ScalarAsync<int>("SELECT payment_methods_version FROM company_settings"));
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records"));
        Assert.Equal(await ScalarAsync<long>("SELECT count(*) FROM admins"), await ScalarAsync<long>("SELECT count(DISTINCT recipient_email) FROM dispatch_records"));
        string details = await ScalarAsync<string>("SELECT details::text FROM audit_logs WHERE target_record_type = 'CompanySettings'");
        Assert.Contains("BankTransferIban", details, StringComparison.Ordinal);
        Assert.Contains(Iban, details, StringComparison.Ordinal);
        Assert.NotEqual(Guid.Empty, await ScalarAsync<Guid>("SELECT admin_id FROM audit_logs WHERE target_record_type = 'CompanySettings'"));
    }

    // ---- KK2, KK4: a filled IBAN's change asks for the confirmation (200, nothing written) with the number of waiting transfer orders; the confirmed send saves ----

    [Fact]
    public async Task ChangingAFilledIban_AnswersTheConfirmationWithTheWaitingOrders_WritesNothing_AndTheConfirmedSendSaves_KK2_KK4()
    {
        await ExecuteAsync($"UPDATE company_settings SET is_bank_transfer_enabled = true, bank_transfer_iban = '{Iban}'");
        await using WebApplicationFactory<Program> factory = Open();
        await SeedAwaitingTransferOrderAsync(factory);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage first = await client.PostAsync(PaymentPath + "?handler=SavePaymentMethods", PaymentForm(true, OtherIban, 1), Ct);
        string html = WebUtility.HtmlDecode(await first.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Contains(PaymentMethodsModel.IbanChangeSentence(1), html, StringComparison.Ordinal);
        Assert.Contains("ödemesi beklenen 1 havale siparişi", html, StringComparison.Ordinal);
        Assert.Contains(PaymentMethodsModel.ChangeIbanLabel, html, StringComparison.Ordinal);
        Assert.Equal(Iban, await ScalarAsync<string>("SELECT bank_transfer_iban FROM company_settings"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records"));

        using HttpResponseMessage confirmed = await client.PostAsync(PaymentPath + "?handler=SavePaymentMethods", PaymentForm(true, OtherIban, 1, confirmed: true), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, confirmed.StatusCode);
        Assert.Equal(OtherIban, await ScalarAsync<string>("SELECT bank_transfer_iban FROM company_settings"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records"));
        string details = await ScalarAsync<string>("SELECT details::text FROM audit_logs WHERE target_record_type = 'CompanySettings'");
        Assert.Contains(Iban, details, StringComparison.Ordinal);
        Assert.Contains(OtherIban, details, StringComparison.Ordinal);
        Assert.Equal(Iban, await ScalarAsync<string>("SELECT bank_transfer_iban FROM orders"));
    }

    [Fact]
    public async Task ClosingTheTransfer_AnswersTheConfirmationWithTheWaitingOrders_AndTheConfirmedSendCloses_KK2()
    {
        await ExecuteAsync($"UPDATE company_settings SET is_bank_transfer_enabled = true, bank_transfer_iban = '{Iban}'");
        await using WebApplicationFactory<Program> factory = Open();
        await SeedAwaitingTransferOrderAsync(factory);
        await SeedAwaitingTransferOrderAsync(factory);
        using HttpClient client = ClientOf(factory);
        bool cardOpen = await CardOpenAsync(factory);

        using HttpResponseMessage first = await client.PostAsync(PaymentPath + "?handler=SavePaymentMethods", PaymentForm(false, Iban, 1), Ct);
        string html = WebUtility.HtmlDecode(await first.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Contains(PaymentMethodsModel.CloseSentence(2, cardOpen), html, StringComparison.Ordinal);
        Assert.Contains("Ödemesi beklenen 2 havale siparişi etkilenmez", html, StringComparison.Ordinal);
        Assert.Contains(PaymentMethodsModel.CloseTransferLabel, html, StringComparison.Ordinal);
        Assert.True(await ScalarAsync<bool>("SELECT is_bank_transfer_enabled FROM company_settings"));

        using HttpResponseMessage confirmed = await client.PostAsync(PaymentPath + "?handler=SavePaymentMethods", PaymentForm(false, Iban, 1, confirmed: true), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, confirmed.StatusCode);
        Assert.False(await ScalarAsync<bool>("SELECT is_bank_transfer_enabled FROM company_settings"));
    }

    // ---- VAL-A752 (D-1480): the IBAN's change and the transfer's closing together carry both sentences, each with the waiting order; the confirmed send closes the sale ----

    [Fact]
    public async Task ChangingTheIbanAndClosingTheTransferTogether_AsksBothSentencesWithTheWaitingOrder_AndTheConfirmedSendClosesTheSale_A752()
    {
        await ExecuteAsync($"UPDATE company_settings SET is_bank_transfer_enabled = true, bank_transfer_iban = '{Iban}'");
        await using WebApplicationFactory<Program> factory = Open().WithWebHostBuilder(b => b.ConfigureTestServices(s => s.AddScoped<ICardPaymentCondition>(_ => new FixedCardCondition(false))));
        await SeedAwaitingTransferOrderAsync(factory);
        using HttpClient client = ClientOf(factory);
        Assert.DoesNotContain(SalesGateCondition.PaymentMethod, (await SalesGateOfAsync(factory)).UnmetConditions);

        using HttpResponseMessage first = await client.PostAsync(PaymentPath + "?handler=SavePaymentMethods", PaymentForm(false, OtherIban, 1), Ct);
        string html = WebUtility.HtmlDecode(await first.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Contains(PaymentMethodsModel.IbanChangeSentence(1), html, StringComparison.Ordinal);
        Assert.Contains(PaymentMethodsModel.CloseSentence(1, otherMethodOpen: false), html, StringComparison.Ordinal);
        Assert.Contains("ödemesi beklenen 1 havale siparişi eski IBAN'ı göstermeye devam edecek", html, StringComparison.Ordinal);
        Assert.Contains("Ödemesi beklenen 1 havale siparişi etkilenmez; donmuş IBAN'larıyla sürer.", html, StringComparison.Ordinal);
        Assert.Contains("Açık başka ödeme yöntemi olmadığı için satış da kapanacak.", html, StringComparison.Ordinal);
        Assert.Contains(PaymentMethodsModel.CloseTransferLabel, html, StringComparison.Ordinal);
        Assert.True(await ScalarAsync<bool>("SELECT is_bank_transfer_enabled FROM company_settings"));
        Assert.Equal(Iban, await ScalarAsync<string>("SELECT bank_transfer_iban FROM company_settings"));
        Assert.Equal(1, await ScalarAsync<int>("SELECT payment_methods_version FROM company_settings"));

        using HttpResponseMessage confirmed = await client.PostAsync(PaymentPath + "?handler=SavePaymentMethods", PaymentForm(false, OtherIban, 1, confirmed: true), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, confirmed.StatusCode);
        Assert.False(await ScalarAsync<bool>("SELECT is_bank_transfer_enabled FROM company_settings"));
        Assert.Equal(OtherIban, await ScalarAsync<string>("SELECT bank_transfer_iban FROM company_settings"));
        Assert.Equal(Iban, await ScalarAsync<string>("SELECT bank_transfer_iban FROM orders"));
        Assert.Contains(SalesGateCondition.PaymentMethod, (await SalesGateOfAsync(factory)).UnmetConditions);
    }

    [Fact]
    public async Task EmptyingAFilledIbanWhileTheTransferIsOff_AsksTheConfirmation_WritesNothing_AndTheConfirmedSendEmptiesIt_KK2()
    {
        await ExecuteAsync($"UPDATE company_settings SET is_bank_transfer_enabled = false, bank_transfer_iban = '{Iban}'");
        await using WebApplicationFactory<Program> factory = Open();
        await SeedAwaitingTransferOrderAsync(factory);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage first = await client.PostAsync(PaymentPath + "?handler=SavePaymentMethods", PaymentForm(false, string.Empty, 1), Ct);
        string html = WebUtility.HtmlDecode(await first.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Contains(PaymentMethodsModel.IbanClearSentence(1), html, StringComparison.Ordinal);
        Assert.Contains(PaymentMethodsModel.ChangeIbanLabel, html, StringComparison.Ordinal);
        Assert.Equal(Iban, await ScalarAsync<string>("SELECT bank_transfer_iban FROM company_settings"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs WHERE target_record_type = 'CompanySettings'"));

        using HttpResponseMessage confirmed = await client.PostAsync(PaymentPath + "?handler=SavePaymentMethods", PaymentForm(false, string.Empty, 1, confirmed: true), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, confirmed.StatusCode);
        Assert.True(await ScalarAsync<bool>("SELECT bank_transfer_iban IS NULL FROM company_settings"));
        Assert.Equal(Iban, await ScalarAsync<string>("SELECT bank_transfer_iban FROM orders"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records"));
    }

    // ---- KK3: three 422s and the 409; a new IBAN reaches only new orders (the waiting order keeps its frozen IBAN, above) ----

    [Fact]
    public async Task ThePaymentSave_RefusesTransferWithoutIban_AnEmptiedIbanAndABadIban_AndAStaleVersionIs409_KK3()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage withoutIban = await client.PostAsync(PaymentPath + "?handler=SavePaymentMethods", PaymentForm(true, string.Empty, 1), Ct);
        string withoutIbanHtml = WebUtility.HtmlDecode(await withoutIban.Content.ReadAsStringAsync(Ct));
        using HttpResponseMessage badIban = await client.PostAsync(PaymentPath + "?handler=SavePaymentMethods", PaymentForm(true, "TR330006100519786457841399", 1), Ct);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, withoutIban.StatusCode);
        Assert.Contains(PaymentMethodsModel.IbanRequiredText, withoutIbanHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, badIban.StatusCode);

        await ExecuteAsync($"UPDATE company_settings SET is_bank_transfer_enabled = true, bank_transfer_iban = '{Iban}'");
        using HttpResponseMessage emptied = await client.PostAsync(PaymentPath + "?handler=SavePaymentMethods", PaymentForm(true, string.Empty, 1), Ct);
        string emptiedHtml = WebUtility.HtmlDecode(await emptied.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, emptied.StatusCode);
        Assert.Contains("boşaltılamaz", emptiedHtml, StringComparison.Ordinal);
        Assert.Equal(Iban, await ScalarAsync<string>("SELECT bank_transfer_iban FROM company_settings"));

        await ExecuteAsync("UPDATE company_settings SET payment_methods_version = 2");
        using HttpResponseMessage stale = await client.PostAsync(PaymentPath + "?handler=SavePaymentMethods", PaymentForm(true, OtherIban, 1, confirmed: true), Ct);

        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal(Iban, await ScalarAsync<string>("SELECT bank_transfer_iban FROM company_settings"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
    }

    // ---- KK3: both IBAN refusals log 07 6.2.8's one code (VAL-A539), the emptied one with the field's "boşaltılamaz" text ----

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BothIbanRefusals_LogBankTransferIbanRequired_KK3(bool filled)
    {
        if (filled)
        {
            await ExecuteAsync($"UPDATE company_settings SET is_bank_transfer_enabled = true, bank_transfer_iban = '{Iban}'");
        }

        CollectingLoggerProvider logs = new();
        await using WebApplicationFactory<Program> factory = new ShopfolioWebApplicationFactory(_database.Dsn, loggerProvider: logs)
            .WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(ForgeryTokenSampleModel).Assembly)));
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage refused = await client.PostAsync(PaymentPath + "?handler=SavePaymentMethods", PaymentForm(true, string.Empty, 1), Ct);
        string html = WebUtility.HtmlDecode(await refused.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
        Assert.Contains(filled ? PaymentMethodsModel.IbanClearedText : PaymentMethodsModel.IbanRequiredText, html, StringComparison.Ordinal);
        List<string> lines = [.. logs.Lines.Where(l => l.Contains("RejectionLog|", StringComparison.Ordinal) && l.Contains(LogEvents.ValidationRejected, StringComparison.Ordinal))];
        Assert.EndsWith(" " + ErrorCode.BankTransferIbanRequired, Assert.Single(lines), StringComparison.Ordinal);
    }

    // ---- KK5: five sections with their own versions; the empty states; a stale version is 409 on that section only ----

    [Fact]
    public async Task TheShippingScreen_CarriesFiveVersions_AndSaysWhenNoProvinceAndNoThreshold_KK5()
    {
        await ExecuteAsync("UPDATE company_settings SET shipping_fee_version = 3, delivery_provinces_version = 4, periods_version = 5, product_defaults_version = 6, return_address_version = 7");
        await ExecuteAsync("DELETE FROM company_delivery_provinces");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, ShippingPath);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal("3", FieldValue(html, "Input.ShippingFeeVersion"));
        Assert.Equal("4", FieldValue(html, "Input.DeliveryProvincesVersion"));
        Assert.Equal("5", FieldValue(html, "Input.PeriodsVersion"));
        Assert.Equal("6", FieldValue(html, "Input.ProductDefaultsVersion"));
        Assert.Equal("7", FieldValue(html, "Input.ReturnAddressVersion"));
        Assert.Contains(ShippingAndDefaultsModel.NoProvinceText, html, StringComparison.Ordinal);
        Assert.Contains(ShippingAndDefaultsModel.ThresholdOffText, html, StringComparison.Ordinal);
        Assert.Contains(ShippingAndDefaultsModel.MinimumOffText, html, StringComparison.Ordinal);
        string[] order = ["id=\"kargo-ucreti\"", "id=\"teslimat-illeri\"", "id=\"sureler\"", "id=\"urun-varsayilanlari\"", "id=\"iade-adresi\""];
        int[] places = [.. order.Select(id => html.IndexOf(id, StringComparison.Ordinal))];
        Assert.All(places, p => Assert.True(p >= 0));
        Assert.Equal(places.Order(), places);
    }

    [Theory]
    [InlineData("SaveShippingFee")]
    [InlineData("SaveDeliveryProvinces")]
    [InlineData("SavePeriods")]
    [InlineData("SaveProductDefaults")]
    [InlineData("SaveReturnAddress")]
    public async Task EachSection_WithAStaleVersion_Is409_WritesNothing_AndShowsTheConflictOnItsOwnSectionOnly_KK5(string handler)
    {
        await ExecuteAsync("UPDATE company_settings SET shipping_fee_version = 2, delivery_provinces_version = 2, periods_version = 2, product_defaults_version = 2, return_address_version = 2");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        long textsBefore = await ScalarAsync<long>("SELECT count(*) FROM legal_text_versions");
        using HttpResponseMessage stale = await client.PostAsync(ShippingPath + "?handler=" + handler, SectionForm(handler, 1), Ct);
        string html = WebUtility.HtmlDecode(await stale.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal(1, Regex.Count(html, "Yine de kaydet", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
        Assert.Equal(textsBefore, await ScalarAsync<long>("SELECT count(*) FROM legal_text_versions"));
    }

    // ---- KK5: the fee, the provinces, the shipping period and the return address raise the two texts' versions; the threshold, the window and the defaults do not ----

    [Theory]
    [InlineData("SaveShippingFee", true)]
    [InlineData("SaveDeliveryProvinces", true)]
    [InlineData("SavePeriods", true)]
    [InlineData("SaveReturnAddress", true)]
    [InlineData("SaveProductDefaults", false)]
    public async Task ASavedSection_WritesItsTrailRow_AndOnlyAFeedingValueRaisesTheTwoTexts_KK5(string handler, bool feeds)
    {
        await using WebApplicationFactory<Program> factory = Open();
        await SynchronizeTextsAsync(factory);
        long before = await ScalarAsync<long>("SELECT count(*) FROM legal_text_versions");
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage saved = await client.PostAsync(ShippingPath + "?handler=" + handler, SectionForm(handler, 1), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, saved.StatusCode);
        long after = await ScalarAsync<long>("SELECT count(*) FROM legal_text_versions");
        Assert.Equal(feeds ? before + 2 : before, after);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs WHERE target_record_type = 'CompanySettings'"));
        Assert.Equal(feeds ? 2L : 0L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs WHERE target_record_type = 'LegalTextVersion'"));
    }

    [Fact]
    public async Task OnlyTheThresholdAndTheWindowChanged_RaiseNoTextVersion_KK5()
    {
        await using WebApplicationFactory<Program> factory = Open();
        await SynchronizeTextsAsync(factory);
        long before = await ScalarAsync<long>("SELECT count(*) FROM legal_text_versions");
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage fee = await client.PostAsync(ShippingPath + "?handler=SaveShippingFee", Form(("Input.ShippingFee", "0"), ("Input.FreeShippingThreshold", "500"), ("Input.MinimumOrderAmount", string.Empty), ("Input.ShippingFeeVersion", "1")), Ct);
        using HttpResponseMessage window = await client.PostAsync(ShippingPath + "?handler=SavePeriods", Form(("Input.DefaultShippingPeriod", "2"), ("Input.BankTransferPaymentWindow", "3"), ("Input.PeriodsVersion", "1")), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, fee.StatusCode);
        Assert.Equal(HttpStatusCode.SeeOther, window.StatusCode);
        Assert.Equal(before, await ScalarAsync<long>("SELECT count(*) FROM legal_text_versions"));
    }

    // ---- KK6: four amount 422s ----

    [Theory]
    [InlineData("-1", "")]
    [InlineData("10,123", "")]
    [InlineData("99999999999", "")]
    [InlineData("", "")]
    [InlineData("10", "-5")]
    public async Task TheFeeSave_RefusesANegativeAThreeDecimalAnOverLimitAndAnEmptyFee_KK6(string fee, string threshold)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage refused = await client.PostAsync(ShippingPath + "?handler=SaveShippingFee", Form(("Input.ShippingFee", fee), ("Input.FreeShippingThreshold", threshold), ("Input.MinimumOrderAmount", string.Empty), ("Input.ShippingFeeVersion", "1")), Ct);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
        Assert.Equal(1, await ScalarAsync<int>("SELECT shipping_fee_version FROM company_settings"));
    }

    // ---- KK7: a province off the list is 422; no selection is saved; no "Tümünü seç" on the path without script ----

    [Fact]
    public async Task TheProvincesSave_RefusesAProvinceOffTheList_SavesAnEmptySelection_AndHasNoSelectAllWithoutScript_KK7()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string page) = await GetAsync(client, ShippingPath);
        Assert.DoesNotContain("Tümünü seç", page, StringComparison.Ordinal);
        Assert.DoesNotContain("Seçimi kaldır", page, StringComparison.Ordinal);

        // T539, D-1443: the two buttons belong to the script - it puts them above the boxes it finds by this hook; the markup carries the boxes, the hook and the page's own module, never the buttons.
        Assert.Contains("data-js=\"province-boxes\"", page, StringComparison.Ordinal);
        Assert.Contains("js/pages/panel/shipping-and-defaults", page, StringComparison.Ordinal);

        using HttpResponseMessage offList = await client.PostAsync(ShippingPath + "?handler=SaveDeliveryProvinces", Form(("Input.ProvinceIds", _ankara.ToString()), ("Input.ProvinceIds", Guid.NewGuid().ToString()), ("Input.DeliveryProvincesVersion", "1")), Ct);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, offList.StatusCode);
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM company_delivery_provinces"));

        using HttpResponseMessage none = await client.PostAsync(ShippingPath + "?handler=SaveDeliveryProvinces", Form(("Input.DeliveryProvincesVersion", "1")), Ct);
        Assert.Equal(HttpStatusCode.SeeOther, none.StatusCode);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM company_delivery_provinces"));
        (_, string after) = await GetAsync(client, ShippingPath);
        Assert.Contains(ShippingAndDefaultsModel.NoProvinceText, after, StringComparison.Ordinal);
    }

    // ---- KK8: the two fences of the periods ----

    [Theory]
    [InlineData("999", "2", "DefaultShippingPeriod")]
    [InlineData("2", "0", "BankTransferPaymentWindow")]
    [InlineData("2", "999", "BankTransferPaymentWindow")]
    public async Task ThePeriodsSave_RefusesAValueOutsideItsFence_WithK817sText_KK8(string shipping, string window, string field)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage refused = await client.PostAsync(ShippingPath + "?handler=SavePeriods", Form(("Input.DefaultShippingPeriod", shipping), ("Input.BankTransferPaymentWindow", window), ("Input.PeriodsVersion", "1")), Ct);
        string html = WebUtility.HtmlDecode(await refused.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
        Assert.Contains(ShippingAndDefaultsModel.FenceText(field), html, StringComparison.Ordinal);
        Assert.Equal(1, await ScalarAsync<int>("SELECT periods_version FROM company_settings"));
    }

    // ---- KK9: the defaults' fence; the rate of an existing product does not change ----

    [Theory]
    [InlineData("0", "20")]
    [InlineData("5", "100")]
    [InlineData("5", "-1")]
    public async Task TheDefaultsSave_RefusesAValueOutsideTheFence_KK9(string allowance, string rate)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage refused = await client.PostAsync(ShippingPath + "?handler=SaveProductDefaults", Form(("Input.DownloadAllowance", allowance), ("Input.DefaultVatRate", rate), ("Input.ProductDefaultsVersion", "1")), Ct);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
        Assert.Equal(1, await ScalarAsync<int>("SELECT product_defaults_version FROM company_settings"));
    }

    [Fact]
    public async Task TheDefaultsSave_WritesTheDefault_AndAnExistingProductKeepsItsRate_KK9()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        Guid productId = await SeedProductAsync(20m);

        using HttpResponseMessage saved = await client.PostAsync(ShippingPath + "?handler=SaveProductDefaults", Form(("Input.DownloadAllowance", "7"), ("Input.DefaultVatRate", "10"), ("Input.ProductDefaultsVersion", "1")), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, saved.StatusCode);
        Assert.Equal(10m, await ScalarAsync<decimal>("SELECT default_vat_rate FROM company_settings"));
        Assert.Equal(7, await ScalarAsync<int>("SELECT download_allowance FROM company_settings"));
        Assert.Equal(20m, await ScalarAsync<decimal>($"SELECT vat_rate FROM products WHERE id = '{productId}'"));
    }

    // ---- KK10: the first entry without a confirmation; the change asks it with the awaited items; required fields, province, phone 422 ----

    [Fact]
    public async Task TheReturnAddress_FirstEntryIsSavedWithoutAConfirmation_AChangeAsksItWithTheAwaitedItems_AndTheConfirmedSendSaves_KK10()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage first = await client.PostAsync(ShippingPath + "?handler=SaveReturnAddress", ReturnForm("Depo Sorumlusu", _ankara.ToString(), "Çankaya", 1), Ct);
        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.Equal("Depo Sorumlusu", await ScalarAsync<string>("SELECT return_recipient_name FROM company_settings"));

        using HttpResponseMessage change = await client.PostAsync(ShippingPath + "?handler=SaveReturnAddress", ReturnForm("Depo Sorumlusu", _izmir.ToString(), "Bornova", 2), Ct);
        string html = WebUtility.HtmlDecode(await change.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.OK, change.StatusCode);
        Assert.Contains(ShippingAndDefaultsModel.ReturnAddressChangeSentence(0), html, StringComparison.Ordinal);
        Assert.Contains(ShippingAndDefaultsModel.ChangeReturnAddressLabel, html, StringComparison.Ordinal);
        Assert.Equal("Çankaya", await ScalarAsync<string>("SELECT return_district FROM company_settings"));

        using HttpResponseMessage confirmed = await client.PostAsync(ShippingPath + "?handler=SaveReturnAddress", ReturnForm("Depo Sorumlusu", _izmir.ToString(), "Bornova", 2, confirmed: true), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, confirmed.StatusCode);
        Assert.Equal("Bornova", await ScalarAsync<string>("SELECT return_district FROM company_settings"));
        string details = await ScalarAsync<string>("SELECT string_agg(details::text, ' ') FROM audit_logs WHERE target_record_type = 'CompanySettings'");
        Assert.Contains("ReturnDistrict", details, StringComparison.Ordinal);
        Assert.Contains("Bornova", details, StringComparison.Ordinal);
        Assert.Contains("\"35\"", details, StringComparison.Ordinal);
    }

    // ---- D-176: the first panel request that writes the trail; the actor is the session's administrator, and a removed account's session has none ----

    [Fact]
    public async Task TheTrailActor_IsTheSessionsAdministratorWithItsNameAndEmail_AndARemovedAccountHasNone_D176()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage saved = await client.PostAsync(ShippingPath + "?handler=SaveReturnAddress", ReturnForm("Depo Sorumlusu", _ankara.ToString(), "Çankaya", 1), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, saved.StatusCode);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs WHERE target_record_type = 'CompanySettings'"));
        Assert.Equal(1L, await ScalarAsync<long>(
            "SELECT count(*) FROM audit_logs l JOIN admins a ON a.id = l.admin_id AND a.name = l.admin_name AND a.email = l.admin_email WHERE l.target_record_type = 'CompanySettings'"));

        Guid adminId = await ScalarAsync<Guid>("SELECT id FROM admins");
        Assert.Equal(adminId, (await ActorOfTheSessionAsync(factory))?.AdminId);
        await ExecuteAsync($"DELETE FROM admins WHERE id = '{adminId}'");
        Assert.Null(await ActorOfTheSessionAsync(factory));
    }

    [Fact]
    public async Task TheReturnAddressConfirmation_CountsTheItemsMarkedAwaitingTheReturnGoods_NotTheOtherAwaitedOnes_KK10()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        IReadOnlyList<(Guid Id, DateOnly Delivered)> items = await SeedAwaitedReturnsAsync(factory, marked: 2);

        // The mark and the panel home's counter read the same rows: two items carry the mark, the third (declared on the delivery day, the firm not asked) awaits its goods unmarked.
        using (IServiceScope scope = factory.Services.CreateScope())
        {
            IAfterSalesModule afterSales = scope.ServiceProvider.GetRequiredService<IAfterSalesModule>();
            IReadOnlyDictionary<Guid, ItemMarks> marks = await afterSales.ItemMarksAsync(
                [.. items.Select(i => new ItemDelivery(i.Id, Shopfolio.Modules.Catalog.Contracts.ProductType.Physical, i.Delivered))], Ct);
            Assert.Equal([true, true, false], items.Select(i => marks[i.Id].AwaitingReturnGoods));
            Assert.Single(await afterSales.ListOrdersAwaitingReturnGoodsAsync(Ct));
        }

        using HttpResponseMessage first = await client.PostAsync(ShippingPath + "?handler=SaveReturnAddress", ReturnForm("Depo Sorumlusu", _ankara.ToString(), "Çankaya", 1), Ct);
        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        using HttpResponseMessage change = await client.PostAsync(ShippingPath + "?handler=SaveReturnAddress", ReturnForm("Depo Sorumlusu", _izmir.ToString(), "Bornova", 2), Ct);
        string html = WebUtility.HtmlDecode(await change.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.OK, change.StatusCode);
        Assert.Contains(ShippingAndDefaultsModel.ReturnAddressChangeSentence(2), html, StringComparison.Ordinal);
        Assert.Equal("Çankaya", await ScalarAsync<string>("SELECT return_district FROM company_settings"));
    }

    [Theory]
    [InlineData("", "province", "Çankaya", "")]
    [InlineData("Depo", "off-list", "Çankaya", "")]
    [InlineData("Depo", "province", "Çankaya", "12")]
    public async Task TheReturnAddressSave_RefusesAnEmptyRequiredPartAProvinceOffTheListAndABadPhone_KK10(string recipient, string province, string district, string phone)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        string provinceValue = province == "province" ? _ankara.ToString() : Guid.NewGuid().ToString();

        using HttpResponseMessage refused = await client.PostAsync(ShippingPath + "?handler=SaveReturnAddress", ReturnForm(recipient, provinceValue, district, 1, phone: phone), Ct);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
        Assert.Equal(1, await ScalarAsync<int>("SELECT return_address_version FROM company_settings"));
    }

    // ---- KK10 (düzeltme 2; 07 2.2.4, VAL-A597): an invalid change of the entered address is refused by field on its first send, before any confirmation; a valid one is asked ----

    [Theory]
    [InlineData("", "province", "", nameof(SaveReturnAddressInput.ReturnRecipientName), FieldCodes.FilledFieldCleared)]
    [InlineData("Depo Sorumlusu", "off-list", "", nameof(SaveReturnAddressInput.ReturnProvinceId), FieldCodes.InvalidProvince)]
    [InlineData("Depo Sorumlusu", "province", "12", nameof(SaveReturnAddressInput.ReturnPhone), FieldCodes.InvalidPhone)]
    public async Task AnInvalidChangeOfTheEnteredReturnAddress_Is422WithTheFieldMessageOnItsFirstSend_NotTheConfirmation_AndAValidChangeIsAsked_KK10(
        string recipient, string province, string phone, string field, string code)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        using HttpResponseMessage entered = await client.PostAsync(ShippingPath + "?handler=SaveReturnAddress", ReturnForm("Depo Sorumlusu", _ankara.ToString(), "Çankaya", 1), Ct);
        Assert.Equal(HttpStatusCode.SeeOther, entered.StatusCode);
        string provinceValue = province == "province" ? _izmir.ToString() : Guid.NewGuid().ToString();
        string label = typeof(SaveReturnAddressInput).GetProperty(field)!.GetCustomAttribute<DisplayAttribute>()!.Name!;

        using HttpResponseMessage refused = await client.PostAsync(ShippingPath + "?handler=SaveReturnAddress", ReturnForm(recipient, provinceValue, "Bornova", 2, phone: phone), Ct);
        string html = WebUtility.HtmlDecode(await refused.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
        Assert.Contains(FieldMessageCatalog.Build(code, label, null), html, StringComparison.Ordinal);
        Assert.DoesNotContain(ShippingAndDefaultsModel.ReturnAddressChangeSentence(0), html, StringComparison.Ordinal);
        Assert.Equal(2, await ScalarAsync<int>("SELECT return_address_version FROM company_settings"));
        Assert.Equal("Çankaya", await ScalarAsync<string>("SELECT return_district FROM company_settings"));

        // The reverse: the same change with valid fields is asked to be confirmed (200, nothing written).
        using HttpResponseMessage valid = await client.PostAsync(ShippingPath + "?handler=SaveReturnAddress", ReturnForm("Depo Sorumlusu", _izmir.ToString(), "Bornova", 2), Ct);
        string validHtml = WebUtility.HtmlDecode(await valid.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.OK, valid.StatusCode);
        Assert.Contains(ShippingAndDefaultsModel.ReturnAddressChangeSentence(0), validHtml, StringComparison.Ordinal);
        Assert.Equal(2, await ScalarAsync<int>("SELECT return_address_version FROM company_settings"));
    }

    // ---- helpers ----

    private static FormUrlEncodedContent PaymentForm(bool enabled, string iban, int version, bool confirmed = false)
    {
        List<(string, string)> fields = [];
        if (enabled)
        {
            fields.Add(("Input.IsBankTransferEnabled", "true"));
        }

        fields.Add(("Input.IsBankTransferEnabled", "false"));
        fields.Add(("Input.BankTransferIban", iban));
        fields.Add(("Input.PaymentMethodsVersion", version.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        if (confirmed)
        {
            fields.Add(("Input.Confirmed", "true"));
        }

        return Form([.. fields]);
    }

    private static FormUrlEncodedContent ReturnForm(string recipient, string province, string district, int version, bool confirmed = false, string phone = "")
    {
        List<(string, string)> fields =
        [
            ("Input.ReturnRecipientName", recipient),
            ("Input.ReturnProvinceId", province),
            ("Input.ReturnDistrict", district),
            ("Input.ReturnAddressLine", "Depo Sk. 4"),
            ("Input.ReturnPhone", phone),
            ("Input.ReturnAddressVersion", version.ToString(System.Globalization.CultureInfo.InvariantCulture)),
        ];
        if (confirmed)
        {
            fields.Add(("Input.Confirmed", "true"));
        }

        return Form([.. fields]);
    }

    private FormUrlEncodedContent SectionForm(string handler, int version)
    {
        string v = version.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return handler switch
        {
            "SaveShippingFee" => Form(("Input.ShippingFee", "49,90"), ("Input.FreeShippingThreshold", string.Empty), ("Input.MinimumOrderAmount", string.Empty), ("Input.ShippingFeeVersion", v)),
            "SaveDeliveryProvinces" => Form(("Input.ProvinceIds", _ankara.ToString()), ("Input.DeliveryProvincesVersion", v)),
            "SavePeriods" => Form(("Input.DefaultShippingPeriod", "4"), ("Input.BankTransferPaymentWindow", "2"), ("Input.PeriodsVersion", v)),
            "SaveProductDefaults" => Form(("Input.DownloadAllowance", "8"), ("Input.DefaultVatRate", "10"), ("Input.ProductDefaultsVersion", v)),
            "SaveReturnAddress" => ReturnForm("Depo Sorumlusu", _ankara.ToString(), "Çankaya", version),
            _ => throw new ArgumentOutOfRangeException(nameof(handler)),
        };
    }

    private async Task AddAdminAsync(string email)
    {
        await using AppDbContext db = NewContext();
        db.Admins.Add(Admin.Create("İkinci Yönetici", email, "hash", Now));
        await db.SaveChangesAsync(Ct);
    }

    private static async Task<bool> CardOpenAsync(WebApplicationFactory<Program> factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ICardPaymentCondition>().IsOpenAsync(Ct);
    }

    private static async Task<SalesGate> SalesGateOfAsync(WebApplicationFactory<Program> factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ISettingsModule>().GetSalesGateAsync(Ct);
    }

    private sealed class FixedCardCondition(bool open) : ICardPaymentCondition
    {
        public Task<bool> IsOpenAsync(CancellationToken ct) => Task.FromResult(open);
    }

    private static async Task SynchronizeTextsAsync(WebApplicationFactory<Program> factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ITransactionRunner runner = scope.ServiceProvider.GetRequiredService<ITransactionRunner>();
        ISettingsModule settings = scope.ServiceProvider.GetRequiredService<ISettingsModule>();
        await runner.RunAsync(async token =>
        {
            await settings.SynchronizeGeneratedLegalTextsAsync(token);
            return SettingsSaveResult.Saved;
        }, Ct);
    }

    private async Task SeedAwaitingTransferOrderAsync(WebApplicationFactory<Program> factory) => await SeedOrderAsync(factory, PaymentMethod.BankTransfer, (_, _) => { });

    /// <summary>
    /// A paid card order shipped (S5) and delivered, whose items' withdrawals await the goods: <paramref name="marked"/> declared after the delivery carry "iade malı bekleniyor";
    /// one declared on the delivery day without the firm's answer awaits the goods for the panel home's counter but carries no mark (03 1.7.3.5; 02 7.4.1). The item ids, marked ones first.
    /// </summary>
    private async Task<IReadOnlyList<(Guid Id, DateOnly Delivered)>> SeedAwaitedReturnsAsync(WebApplicationFactory<Program> factory, int marked)
    {
        DateOnly delivered = DateOnly.FromDateTime(Now.UtcDateTime).AddDays(-3);
        List<(Guid, DateOnly)> items = [];
        Guid orderId = await SeedOrderAsync(factory, PaymentMethod.Card, (order, db) =>
        {
            for (int i = 0; i <= marked; i++)
            {
                OrderItem item = OrderItem.Create(order.Id, null, null, Shopfolio.Modules.Catalog.Contracts.ProductType.Physical, $"Ürün {i}", 2, 100m, 20m, 16.67m, Now);
                item.DeliveryMarkedAt = Now.AddDays(-3);
                item.DeliveryDate = delivered;
                DateOnly declared = i < marked ? delivered.AddDays(1) : delivered;
                db.OrderItems.Add(item);
                db.Withdrawals.Add(Shopfolio.Modules.AfterSales.Withdrawal.ByBuyer(item.Id, 2, declared, null, new Shopfolio.Modules.AfterSales.ItemAmounts(100m, 0m, 16.67m), null, Now));
                items.Add((item.Id, delivered));
            }

            db.TransitionRecords.Add(TransitionRecord.Create(order.Id, TransitionCode.S5, ActorKind.System, null, null, null, null, null, null, null, Now.AddDays(-4)));
        });
        await ExecuteAsync($"UPDATE orders SET order_status = 'Delivered', payment_status = 'Paid' WHERE id = '{orderId}'");
        return items;
    }

    // The trail's actor source of the application, asked inside a request that carries this class's administrator session cookie.
    private async Task<AuditActor?> ActorOfTheSessionAsync(WebApplicationFactory<Program> factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        DefaultHttpContext context = new() { RequestServices = scope.ServiceProvider };
        context.Request.Headers.Cookie = $"{CookieNames.AdminSession}={_adminKey}";
        IHttpContextAccessor accessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        accessor.HttpContext = context;
        try
        {
            return await scope.ServiceProvider.GetRequiredService<IAuditActorSource>().CurrentAsync(Ct);
        }
        finally
        {
            accessor.HttpContext = null;
        }
    }

    private async Task<Guid> SeedOrderAsync(WebApplicationFactory<Program> factory, PaymentMethod method, Action<Order, AppDbContext> rows)
    {
        CompanyIdentity identity;
        using (IServiceScope scope = factory.Services.CreateScope())
        {
            identity = await scope.ServiceProvider.GetRequiredService<ISettingsModule>().GetCompanyIdentityAsync(Ct);
        }

        Order order = Order.Create(
            $"2026-H{Guid.NewGuid().ToString("N")[..8]}", $"access-{Guid.NewGuid():N}", $"confirm-{Guid.NewGuid():N}", method, true, "ali@example.com", 100m, 0m, Now.AddDays(2), Now);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = Guid.NewGuid();
        order.FreezeAddresses(("Ali Veli", "5321234567", _ankara, "Çankaya", "Atatürk Bulvarı 1"), ("Ali Veli", _ankara, "Çankaya", "Atatürk Bulvarı 1"));
        OrderFreeze.FreezeLegalIdentity(order, identity, new LegalTextVersionIds(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), method == PaymentMethod.BankTransfer ? Iban : null);
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
        order.FreezeShippingFeeVatBreakdown([]);
        order.FreezeShippingPromise(2);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, 90 + Random.Shared.Next(1000), Now, "metin", Now);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, 90 + Random.Shared.Next(1000), Now, "metin", Now);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, 90 + Random.Shared.Next(1000), Now, "metin", Now).WithHosting();
        order.PreInformationFormVersionId = form.Id;
        order.DistanceSalesContractVersionId = contract.Id;
        order.PrivacyNoticeVersionId = notice.Id;
        await using AppDbContext db = NewContext();
        db.LegalTextVersions.AddRange(form, contract, notice);
        db.Orders.Add(order);
        rows(order, db);
        await db.SaveChangesAsync(Ct);
        return order.Id;
    }

    private async Task<Guid> SeedProductAsync(decimal rate)
    {
        Shopfolio.Modules.Catalog.Product product = Shopfolio.Modules.Catalog.Product.Create("Var olan ürün", $"var-olan-{Guid.NewGuid():N}", Shopfolio.Modules.Catalog.Contracts.ProductType.Physical, rate, Now);
        product.WithSeededOrigin();
        await using AppDbContext db = NewContext();
        db.Products.Add(product);
        await db.SaveChangesAsync(Ct);
        return product.Id;
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
    }

    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(ForgeryTokenSampleModel).Assembly)));

    private HttpClient ClientOf(WebApplicationFactory<Program> factory)
    {
        HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        client.DefaultRequestHeaders.Add(HeaderNames.Cookie, $"{CookieNames.AdminSession}={_adminKey}");
        return client;
    }

    private static FormUrlEncodedContent Form(params (string Name, string Value)[] fields) =>
        new(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value)));

    private static async Task<(HttpStatusCode Status, string Html)> GetAsync(HttpClient client, string path)
    {
        using HttpResponseMessage response = await client.GetAsync(new Uri(path, UriKind.Relative), Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static string? FieldValue(string html, string name)
    {
        foreach (Match tag in InputTag().Matches(html))
        {
            if (tag.Value.Contains($"name=\"{name}\"", StringComparison.Ordinal))
            {
                Match value = ValueAttribute().Match(tag.Value);
                return value.Success ? WebUtility.HtmlDecode(value.Groups[1].Value) : string.Empty;
            }
        }

        return null;
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<T> ScalarAsync<T>(string sql)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return (T)(await command.ExecuteScalarAsync(Ct))!;
    }
}
