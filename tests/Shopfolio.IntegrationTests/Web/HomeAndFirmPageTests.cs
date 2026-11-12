using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the home page and menu and the firm identity screens' tests (T535): one database copied from the template; every test sets the company, the settings and the provinces up again.</summary>
[CollectionDefinition(HomeAndFirmPageTestsDefinition.Name)]
public sealed class HomeAndFirmPageTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "HomeAndFirmPages";
}

/// <summary>
/// The home page and menu screen (E-42) and the firm identity and sales screen (E-44) on the opened application and a real database (T535 criteria 1-10, KK1-KK10; 07 UC-97, UC-99, UC-162,
/// UC-164...UC-166; 04 9.14, 9.16).
/// </summary>
[Collection(HomeAndFirmPageTestsDefinition.Name)]
public sealed partial class HomeAndFirmPageTests : IAsyncLifetime
{
    private const string HomePath = "/panel/ana-sayfa-ve-menu";

    private const string FirmPath = "/panel/firma-kimligi-ve-satis";

    private static readonly DateTimeOffset Now = new(2026, 11, 12, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _database;

    private string _adminKey = string.Empty;

    private Guid _provinceId;

    public HomeAndFirmPageTests(CollectionDatabaseFixture database)
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
        await ExecuteAsync("TRUNCATE provinces, companies, company_settings, legal_text_versions, audit_logs, social_links, dispatch_records, custom_pages CASCADE");
        Province province = Province.Create("06", "Ankara", Now);
        _provinceId = province.Id;
        CompanySettings settings = CompanySettings.Create(Now);
        settings.BankTransferPaymentWindow = 1;
        settings.DownloadAllowance = 1;
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
        company.ProvinceId = province.Id;
        company.District = "Çankaya";
        company.AddressLine = "Atatürk Bulvarı 1";
        company.Phone = "3121234567";
        company.Email = "iletisim@firma.com.tr";
        company.KepAddress = "firma@hs01.kep.tr";
        company.ChamberName = "Ankara Esnaf ve Sanatkarlar Odası";
        company.ProfessionalConductRules = "https://www.tobb.org.tr/etik-kurallar";
        await using (AppDbContext db = NewContext())
        {
            db.Provinces.Add(province);
            db.Companies.Add(company);
            db.CompanySettings.Add(settings);
            await db.SaveChangesAsync(Ct);
        }

        _adminKey = (await TestSessions.AdminAsync(_database.Dsn)).Key;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the screen reads the layout, the names, the links and the reasons; the first setup has the defaults (criterion 1) ----

    [Fact]
    public async Task TheHomeScreen_ShowsLayoutNamesDefaultsAndTheReasonsItemsAreHidden_AndCarriesTheSectionVersion_KK1()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, HomePath);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(MenuNameDefaults.Products, FieldValue(html, "Input.MenuNameProducts"));
        Assert.Equal(MenuNameDefaults.Contact, FieldValue(html, "Input.MenuNameContact"));
        Assert.Contains(HomeAndMenuModel.DefaultNamePrefix + MenuNameDefaults.Offerings, html, StringComparison.Ordinal);
        Assert.Contains(HomeAndMenuModel.NoPublishedRecordText, html, StringComparison.Ordinal);
        Assert.Contains(HomeAndMenuModel.NoPublishedProductText, html, StringComparison.Ordinal);
        Assert.Contains("Menüde görünmüyor — yayında kaydı yok.", html, StringComparison.Ordinal);
        Assert.Contains("Menüde görünmüyor — yayında ürünü olan kategori yok.", html, StringComparison.Ordinal);
        Assert.Equal("1", FieldValue(html, "Input.Version"));
        Assert.Matches(new Regex("id=\"layout-content-first\"[^>]*checked", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)), html);
        foreach (SocialPlatform platform in Enum.GetValues<SocialPlatform>())
        {
            Assert.Equal(string.Empty, FieldValue(html, HomeAndMenuModel.SocialFieldName(platform)));
            Assert.Equal(platform.ToString(), FieldValue(html, HomeAndMenuModel.SocialPlatformFieldName(platform)));
        }
    }

    [Theory]
    [InlineData("ContentFirst", new[] { "AboutPage", "Offerings", "PortfolioItems", "FaqItems", "Products" })]
    [InlineData("StoreFirst", new[] { "Products", "Offerings", "PortfolioItems", "AboutPage", "FaqItems" })]
    public async Task TheMenuNameFields_FollowTheSelectedLayoutsMenuOrder_AndTheGeneralPagesComeBeforeContact_KK1(string layout, string[] order)
    {
        ArgumentNullException.ThrowIfNull(order);
        await ExecuteAsync($"UPDATE company_settings SET homepage_layout = '{layout}'");
        await using (AppDbContext db = NewContext())
        {
            CustomPage page = CustomPage.Create("Teslimat bilgisi", "teslimat-bilgisi", 1, Now);
            page.PublishStatus = PublishStatus.Published;
            page.IsShownInMenu = true;
            db.CustomPages.Add(page);
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, HomePath);

        Assert.Equal(HttpStatusCode.OK, status);
        List<int> positions = [.. order.Select(kind => html.IndexOf($"name=\"Input.MenuName{kind}\"", StringComparison.Ordinal))];
        positions.Add(html.IndexOf("id=\"menu-pages\"", StringComparison.Ordinal));
        positions.Add(html.IndexOf("name=\"Input.MenuNameContact\"", StringComparison.Ordinal));
        Assert.All(positions, p => Assert.True(p >= 0));
        Assert.Equal(positions.Order().ToList(), positions);
        Assert.Contains("Teslimat bilgisi", html[positions[^2]..positions[^1]], StringComparison.Ordinal);
    }

    // ---- KK2: the save writes the section and the links, an emptied link deletes the row, the changes go to the trail (criterion 2) ----

    [Fact]
    public async Task TheSave_WritesLayoutNamesNumberAndLinks_AnEmptiedLinkDeletesItsRow_AndTheChangesGoToTheTrail_KK2()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage first = await client.PostAsync(HomePath + "?handler=SaveHomepage", HomeForm(layout: "StoreFirst", offerings: "Çözümlerimiz", instagram: "https://instagram.com/firma", whatsApp: "0532 123 45 67", version: 1), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.Equal("StoreFirst", await ScalarAsync<string>("SELECT homepage_layout FROM company_settings"));
        Assert.Equal("Çözümlerimiz", await ScalarAsync<string>("SELECT menu_name_offerings FROM company_settings"));
        Assert.Equal("5321234567", await ScalarAsync<string>("SELECT whats_app_number FROM company_settings"));
        Assert.Equal(2, await ScalarAsync<int>("SELECT homepage_version FROM company_settings"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM social_links"));
        string trail = await ScalarAsync<string>("SELECT details::text FROM audit_logs WHERE operation = 'UpdateSettings'");
        Assert.Contains("Ana sayfa ve menü", trail, StringComparison.Ordinal);
        Assert.Contains("SocialLink.Instagram", trail, StringComparison.Ordinal);
        Assert.Contains("Çözümlerimiz", trail, StringComparison.Ordinal);
        Assert.Contains(MenuNameDefaults.Offerings, trail, StringComparison.Ordinal);

        using HttpResponseMessage second = await client.PostAsync(HomePath + "?handler=SaveHomepage", HomeForm(layout: "StoreFirst", offerings: "Çözümlerimiz", instagram: string.Empty, whatsApp: "5321234567", version: 2), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, second.StatusCode);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM social_links"));
        Assert.Equal(3, await ScalarAsync<int>("SELECT homepage_version FROM company_settings"));
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs WHERE operation = 'UpdateSettings'"));

        using HttpResponseMessage unchanged = await client.PostAsync(HomePath + "?handler=SaveHomepage", HomeForm(layout: "StoreFirst", offerings: "Çözümlerimiz", instagram: string.Empty, whatsApp: "5321234567", version: 3), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, unchanged.StatusCode);
        Assert.Equal(3, await ScalarAsync<int>("SELECT homepage_version FROM company_settings"));
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs WHERE operation = 'UpdateSettings'"));
    }

    // ---- KK3: the 422 reasons write nothing; a stale version is 409 (criterion 3) ----

    [Fact]
    public async Task TheSave_RefusesAnEmptyNameABadLinkAndABadNumberTogether_WritesNothing_AndAStaleVersionIs409_KK3()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage refused = await client.PostAsync(HomePath + "?handler=SaveHomepage", HomeForm(layout: "StoreFirst", offerings: "  ", instagram: "bağlantı değil", whatsApp: "12", version: 1), Ct);
        string html = WebUtility.HtmlDecode(await refused.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
        Assert.Contains(HomeAndMenuModel.DefaultNamePrefix + MenuNameDefaults.Offerings, html, StringComparison.Ordinal);
        Assert.Contains("Menü adı boş olamaz; varsayılan ad: " + MenuNameDefaults.Offerings + ".", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Hizmetler menü adı boş olamaz", html, StringComparison.Ordinal);
        Assert.DoesNotContain("FieldRequired", html, StringComparison.Ordinal);
        Assert.DoesNotContain("InvalidUrl", html, StringComparison.Ordinal);
        Assert.DoesNotContain("InvalidPhone", html, StringComparison.Ordinal);
        Assert.Contains("field--invalid", html, StringComparison.Ordinal);
        Assert.Equal("ContentFirst", await ScalarAsync<string>("SELECT homepage_layout FROM company_settings"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM social_links"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
        Assert.Equal(1, await ScalarAsync<int>("SELECT homepage_version FROM company_settings"));

        using HttpResponseMessage ok = await client.PostAsync(HomePath + "?handler=SaveHomepage", HomeForm(layout: "StoreFirst", offerings: "Hizmetler", instagram: string.Empty, whatsApp: string.Empty, version: 1), Ct);
        Assert.Equal(HttpStatusCode.SeeOther, ok.StatusCode);
        using HttpResponseMessage stale = await client.PostAsync(HomePath + "?handler=SaveHomepage", HomeForm(layout: "ContentFirst", offerings: "Başka", instagram: "https://instagram.com/x", whatsApp: string.Empty, version: 1), Ct);
        string staleHtml = WebUtility.HtmlDecode(await stale.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal("2", FieldValue(staleHtml, "Input.Version"));
        Assert.Equal("Hizmetler", await ScalarAsync<string>("SELECT menu_name_offerings FROM company_settings"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM social_links"));
    }

    [Fact]
    public async Task TheEmptyMenuNameRefusal_WritesTheRejectionLine_WithTheFieldAndTheFieldRequiredCode_AndNoValue_KK3()
    {
        await using ShopfolioWebApplicationFactory root = new(_database.Dsn);
        await using WebApplicationFactory<Program> factory = root.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(ForgeryTokenSampleModel).Assembly)));
        using HttpClient client = ClientOf(factory);
        const string requestId = "t535-empty-menu-name";
        using HttpRequestMessage request = new(HttpMethod.Post, new Uri(HomePath + "?handler=SaveHomepage", UriKind.Relative))
        {
            Content = HomeForm(layout: "StoreFirst", offerings: "  ", instagram: string.Empty, whatsApp: string.Empty, version: 1),
        };
        request.Headers.TryAddWithoutValidation("X-Request-Id", requestId);

        using HttpResponseMessage refused = await client.SendAsync(request, Ct);
        string html = WebUtility.HtmlDecode(await refused.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
        Assert.Contains("Menü adı boş olamaz; varsayılan ad: " + MenuNameDefaults.Offerings + ".", html, StringComparison.Ordinal);
        Assert.DoesNotContain("FieldRequired", html, StringComparison.Ordinal);
        List<JsonElement> lines = [];
        foreach (string path in Directory.GetFiles(root.LogDirectory, "app-*.jsonl"))
        {
            using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using StreamReader reader = new(stream);
            while (reader.ReadLine() is { } text)
            {
                JsonElement line = JsonDocument.Parse(text).RootElement.Clone();
                if (line.TryGetProperty("requestId", out JsonElement id) && id.GetString() == requestId)
                {
                    lines.Add(line);
                }
            }
        }

        JsonElement rejection = Assert.Single(lines);
        Assert.Equal(LogEvents.ValidationRejected, rejection.GetProperty("event").GetString());
        JsonElement field = Assert.Single(rejection.GetProperty("fields").EnumerateArray());
        Assert.Equal("FieldRequired", field.GetProperty("code").GetString());
        Assert.Contains("MenuNameOfferings", field.GetProperty("field").GetString(), StringComparison.Ordinal);
        Assert.DoesNotContain("varsayılan", rejection.GetRawText(), StringComparison.Ordinal);
    }

    // ---- KK4, KK5: the conditions, their links, the e-mail line, the two labels (criteria 4, 5) ----

    [Fact]
    public async Task TheFirmScreen_ReadsTheSalesStateAndEachCondition_LinksTheUnmetOnes_AndLabelsTheSwitchByItsValue_KK4_KK5()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, FirmPath);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains(FirmIdentityModel.SalesClosedText, html, StringComparison.Ordinal);
        foreach (string name in new[] { FirmIdentityModel.IdentityConditionName, FirmIdentityModel.PaymentConditionName, FirmIdentityModel.LegalTextsConditionName, FirmIdentityModel.ReturnAddressConditionName, FirmIdentityModel.ClosureConditionName })
        {
            Assert.Contains(name, html, StringComparison.Ordinal);
        }

        Assert.Contains("href=\"/panel/odeme-yontemleri\"", html, StringComparison.Ordinal);
        Assert.Contains("href=\"/panel/yasal-metinler\"", html, StringComparison.Ordinal);
        Assert.Contains("href=\"/panel/kargo-sureler-ve-varsayilanlar\"", html, StringComparison.Ordinal);
        Assert.Contains(FirmIdentityModel.EmailHint, html, StringComparison.Ordinal);
        Assert.Contains(FirmIdentityModel.EtbisReminder, html, StringComparison.Ordinal);
        Assert.Contains(FirmIdentityModel.CloseLabel, html, StringComparison.Ordinal);
        Assert.DoesNotContain(FirmIdentityModel.ReopenLabel, html, StringComparison.Ordinal);
        Assert.Equal("1", FieldValue(html, "Input.Version"));
        Assert.Equal("false", FieldValue(html, "Input.Original"));

        await ExecuteAsync("UPDATE company_settings SET is_sales_temporarily_closed = true");
        (_, string closed) = await GetAsync(client, FirmPath);

        Assert.Contains(FirmIdentityModel.ReopenLabel, closed, StringComparison.Ordinal);
        Assert.DoesNotContain(FirmIdentityModel.CloseLabel, closed, StringComparison.Ordinal);
        Assert.Equal("true", FieldValue(closed, "Input.Original"));
    }

    [Fact]
    public async Task TheEmailField_WarnsWhenTheLastSettledFirmNoticeWentUndeliveredToTheCurrentEmail_AndTheWarningLiftsWithTheNextSentNotice_KK4()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        (_, string quiet) = await GetAsync(client, FirmPath);

        DispatchRecord undelivered = DispatchRecord.Queue(DispatchType.F2, "Iletisim@Firma.com.tr", null, DispatchRelatedRecordType.ContactRequest, Guid.NewGuid(), Now.AddHours(-2));
        undelivered.DispatchState = DispatchState.Undelivered;
        undelivered.LastOutcome = DispatchOutcome.RecipientError;
        undelivered.AttemptCount = 3;
        await using (AppDbContext db = NewContext())
        {
            db.DispatchRecords.Add(undelivered);
            await db.SaveChangesAsync(Ct);
        }

        (_, string warned) = await GetAsync(client, FirmPath);
        DispatchRecord sent = DispatchRecord.Queue(DispatchType.F4, "iletisim@firma.com.tr", null, DispatchRelatedRecordType.Refund, Guid.NewGuid(), Now.AddHours(-1));
        sent.DispatchState = DispatchState.Sent;
        sent.SentAt = Now.AddHours(-1);
        sent.LastOutcome = DispatchOutcome.Sent;
        sent.AttemptCount = 1;
        await using (AppDbContext db = NewContext())
        {
            db.DispatchRecords.Add(sent);
            await db.SaveChangesAsync(Ct);
        }

        (_, string lifted) = await GetAsync(client, FirmPath);

        Assert.DoesNotContain("firm-notices-warning", quiet, StringComparison.Ordinal);
        Assert.Contains("id=\"firm-notices-warning\"", warned, StringComparison.Ordinal);
        Assert.Contains("Firma bildirimleri ulaşmıyor", warned, StringComparison.Ordinal);
        Assert.Contains("iletisim@firma.com.tr", warned, StringComparison.Ordinal);
        Assert.DoesNotContain("firm-notices-warning", lifted, StringComparison.Ordinal);
    }

    // ---- KK6, KK7: the identity save - 422 reasons, the type change, the trail, the two new text versions, 409 (criteria 6, 7) ----

    [Fact]
    public async Task TheIdentitySave_RefusesAnEmptiedFieldAndABadEmail_AcceptsATypeChange_WritesTheTrailAndTheTwoTexts_AndAStaleVersionIs409_KK6_KK7()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        (_, string page) = await GetAsync(client, FirmPath); // the first request starts the host, which opens the first versions of the two generated texts
        long before = await ScalarAsync<long>("SELECT count(*) FROM legal_text_versions WHERE legal_text_type IN ('PreInformationForm', 'DistanceSalesContract')");
        Assert.True(before >= 2);

        using HttpResponseMessage emptied = await client.PostAsync(FirmPath + "?handler=SaveCompany", CompanyForm(legalName: string.Empty, email: "iletisim@firma.com.tr", version: 1), Ct);
        using HttpResponseMessage badEmail = await client.PostAsync(FirmPath + "?handler=SaveCompany", CompanyForm(legalName: "Yılmaz Ticaret", email: "bozuk", version: 1), Ct);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, emptied.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, badEmail.StatusCode);
        string badEmailHtml = WebUtility.HtmlDecode(await badEmail.Content.ReadAsStringAsync(Ct));
        Assert.Contains("E-posta geçerli bir e-posta adresi olmalıdır.", badEmailHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("InvalidEmail", badEmailHtml, StringComparison.Ordinal);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));

        using HttpResponseMessage saved = await client.PostAsync(FirmPath + "?handler=SaveCompany", CompanyForm(legalName: "Yılmaz Ticaret Ltd", email: "iletisim@firma.com.tr", version: 1, type: "LegalEntity"), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, saved.StatusCode);
        Assert.Equal("LegalEntity", await ScalarAsync<string>("SELECT company_type FROM companies"));
        string companyTrail = await ScalarAsync<string>("SELECT details::text FROM audit_logs WHERE operation = 'UpdateCompany'");
        Assert.Contains("LegalName", companyTrail, StringComparison.Ordinal);
        Assert.Contains("Yılmaz Ticaret Ltd", companyTrail, StringComparison.Ordinal);
        Assert.Equal(before + 2, await ScalarAsync<long>("SELECT count(*) FROM legal_text_versions WHERE legal_text_type IN ('PreInformationForm', 'DistanceSalesContract')"));
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs WHERE operation = 'PublishLegalText'"));

        using HttpResponseMessage stale = await client.PostAsync(FirmPath + "?handler=SaveCompany", CompanyForm(legalName: "Başka Ad", email: "iletisim@firma.com.tr", version: 1, type: "LegalEntity"), Ct);

        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal(before + 2, await ScalarAsync<long>("SELECT count(*) FROM legal_text_versions WHERE legal_text_type IN ('PreInformationForm', 'DistanceSalesContract')"));
    }

    // ---- KK8, KK9: closing - the confirmation, the closed operations, the trail; the two 303s (criteria 8, 9) ----

    [Fact]
    public async Task Closing_AsksFirst_ThenWritesAgainstTheShownValue_AndAChangedOrAlreadyAskedSwitchIs303WithNothingWritten_KK8_KK9()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage asked = await client.PostAsync(FirmPath + "?handler=SuspendSales", Form(("Input.Original", "false")), Ct);
        string askedHtml = WebUtility.HtmlDecode(await asked.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.OK, asked.StatusCode);
        Assert.Contains(FirmIdentityModel.ConfirmSentence, askedHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("geri alınamaz", askedHtml, StringComparison.OrdinalIgnoreCase);
        Assert.False(await ScalarAsync<bool>("SELECT is_sales_temporarily_closed FROM company_settings"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));

        using HttpResponseMessage closed = await client.PostAsync(FirmPath + "?handler=SuspendSales", Form(("Input.Original", "false"), ("Input.Confirmed", "true")), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, closed.StatusCode);
        Assert.True(await ScalarAsync<bool>("SELECT is_sales_temporarily_closed FROM company_settings"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs WHERE operation = 'UpdateSettings'"));
        await using (AsyncServiceScope scope = factory.Services.CreateAsyncScope())
        {
            SalesGate gate = await scope.ServiceProvider.GetRequiredService<ISettingsModule>().GetSalesGateAsync(Ct);
            Assert.False(gate.Permits(SalesGateOperation.AddToCart));
            Assert.True(gate.Permits(SalesGateOperation.BrowseShowcase));
            Assert.True(gate.Permits(SalesGateOperation.ContinueOpenOrder));
        }

        using HttpResponseMessage again = await client.PostAsync(FirmPath + "?handler=SuspendSales", Form(("Input.Original", "false"), ("Input.Confirmed", "true")), Ct);
        using HttpResponseMessage alreadyOpen = await client.PostAsync(FirmPath + "?handler=ResumeSales", Form(("Input.Original", "false")), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, again.StatusCode);
        Assert.Equal(HttpStatusCode.SeeOther, alreadyOpen.StatusCode);
        Assert.True(await ScalarAsync<bool>("SELECT is_sales_temporarily_closed FROM company_settings"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
    }

    // ---- KK10: reopening never opens the sales alone (criterion 10) ----

    [Fact]
    public async Task Reopening_ClosesTheSwitchButLeavesSalesClosedWhileAnotherConditionIsUnmet_NamesIt_AndWritesTheTrail_KK10()
    {
        await ExecuteAsync("UPDATE company_settings SET is_sales_temporarily_closed = true");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage reopened = await client.PostAsync(FirmPath + "?handler=ResumeSales", Form(("Input.Original", "true")), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, reopened.StatusCode);
        Assert.False(await ScalarAsync<bool>("SELECT is_sales_temporarily_closed FROM company_settings"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs WHERE operation = 'UpdateSettings'"));
        (_, string html) = await GetAsync(client, FirmPath);
        Assert.Contains(FirmIdentityModel.SalesClosedText, html, StringComparison.Ordinal);
        Assert.Contains("Eksik: açık ödeme yöntemi yok", html, StringComparison.Ordinal);
        Assert.Contains(FirmIdentityModel.CloseLabel, html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ThePanelScreens_RefuseAGuest()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient guest = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage home = await guest.GetAsync(HomePath, Ct);
        using HttpResponseMessage firm = await guest.GetAsync(FirmPath, Ct);

        Assert.NotEqual(HttpStatusCode.OK, home.StatusCode);
        Assert.NotEqual(HttpStatusCode.OK, firm.StatusCode);
    }

    // ---- helpers ----

    private static FormUrlEncodedContent HomeForm(string layout, string offerings, string instagram, string whatsApp, int version) =>
        Form(
            ("Input.HomepageLayout", layout),
            ("Input.MenuNameProducts", MenuNameDefaults.Products),
            ("Input.MenuNameOfferings", offerings),
            ("Input.MenuNamePortfolioItems", MenuNameDefaults.PortfolioItems),
            ("Input.MenuNameAboutPage", MenuNameDefaults.AboutPage),
            ("Input.MenuNameFaqItems", MenuNameDefaults.FaqItems),
            ("Input.MenuNameContact", MenuNameDefaults.Contact),
            ("Input.WhatsAppNumber", whatsApp),
            ("Input.SocialLinks[0].SocialPlatform", "Instagram"),
            ("Input.SocialLinks[0].Url", instagram),
            ("Input.SocialLinks[1].SocialPlatform", "Facebook"),
            ("Input.SocialLinks[1].Url", string.Empty),
            ("Input.SocialLinks[2].SocialPlatform", "X"),
            ("Input.SocialLinks[2].Url", string.Empty),
            ("Input.SocialLinks[3].SocialPlatform", "LinkedIn"),
            ("Input.SocialLinks[3].Url", string.Empty),
            ("Input.SocialLinks[4].SocialPlatform", "YouTube"),
            ("Input.SocialLinks[4].Url", string.Empty),
            ("Input.SocialLinks[5].SocialPlatform", "TikTok"),
            ("Input.SocialLinks[5].Url", string.Empty),
            ("Input.Version", version.ToString(System.Globalization.CultureInfo.InvariantCulture)));

    private FormUrlEncodedContent CompanyForm(string legalName, string email, int version, string type = "Tradesman") =>
        Form(
            ("Input.CompanyType", type),
            ("Input.LegalName", legalName),
            ("Input.TaxIdNumber", "1234567890"),
            ("Input.MersisNumber", "0123456789012345"),
            ("Input.TradeRegistryNumber", "12345"),
            ("Input.ProvinceId", _provinceId.ToString()),
            ("Input.District", "Çankaya"),
            ("Input.AddressLine", "Atatürk Bulvarı 1"),
            ("Input.Phone", "3121234567"),
            ("Input.Email", email),
            ("Input.KepAddress", "firma@hs01.kep.tr"),
            ("Input.ChamberName", "Ankara Esnaf ve Sanatkarlar Odası"),
            ("Input.ProfessionalConductRules", "https://www.tobb.org.tr/etik-kurallar"),
            ("Input.BusinessName", string.Empty),
            ("Input.EtbisVerificationUrl", string.Empty),
            ("Input.Version", version.ToString(System.Globalization.CultureInfo.InvariantCulture)));

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
