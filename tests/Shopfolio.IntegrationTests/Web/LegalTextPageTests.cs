using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Legal;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the legal text pages' tests (T507): one database copied from the template; every test sets the legal text rows, the company and the provinces up again.</summary>
[CollectionDefinition(LegalTextPageTestsDefinition.Name)]
public sealed class LegalTextPageTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "LegalTextPages";
}

/// <summary>
/// The legal text pages (E-11) and the legal texts screen of the panel (E-47) on the opened application and a real database (T507 criteria 1-10, KK1-KK10; 07 UC-16...UC-18, UC-102,
/// UC-173, UC-174; 04 5.11, 9.19). The public pages read the last published version; the screen shows the four texts, the two firm-edited sections and the generated texts, and its two
/// handlers save the draft and publish it.
/// </summary>
[Collection(LegalTextPageTestsDefinition.Name)]
public sealed partial class LegalTextPageTests : IAsyncLifetime
{
    private const string PanelPath = "/panel/yasal-metinler";

    private static readonly DateTimeOffset Now = new(2026, 11, 11, 9, 0, 0, TimeSpan.Zero);

    private static readonly TimeSpan PatternTimeout = TimeSpan.FromSeconds(5);

    private readonly CollectionDatabaseFixture _database;

    private string _adminKey = string.Empty;

    public LegalTextPageTests(CollectionDatabaseFixture database)
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
        await ExecuteAsync("TRUNCATE provinces, companies, company_settings, legal_text_versions, audit_logs CASCADE");
        Province province = Province.Create("06", "Ankara", Now);
        CompanySettings settings = CompanySettings.Create(Now);
        settings.BankTransferPaymentWindow = 1;
        settings.DownloadAllowance = 1;
        settings.HomepageLayout = Modules.CorporateContent.Contracts.HomepageLayout.StoreFirst;
        settings.MenuNameProducts = "Ürünler";
        settings.MenuNameOfferings = "Hizmetler";
        settings.MenuNamePortfolioItems = "Çalışmalar";
        settings.MenuNameAboutPage = "Hakkımızda";
        settings.MenuNameFaqItems = "Sıkça Sorulanlar";
        settings.MenuNameContact = "İletişim";
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
        company.ChamberName = "İstanbul Esnaf ve Sanatkarlar Odası";
        company.ProfessionalConductRules = "https://www.tobb.org.tr/etik-kurallar";
        await using (AppDbContext db = NewContext())
        {
            db.Provinces.Add(province);
            db.Companies.Add(company);
            db.CompanySettings.Add(settings);
            db.LegalTextVersions.Add(LegalTextVersion.CreateDraft(LegalTextType.PrivacyNotice, "<p>{{LegalName}} aydınlatma metni</p>", Now));
            db.LegalTextVersions.Add(LegalTextVersion.CreateDraft(LegalTextType.CookiePolicy, "<p>çerez politikası taslağı</p>", Now));
            await db.SaveChangesAsync(Ct);
        }

        _adminKey = (await TestSessions.AdminAsync(_database.Dsn)).Key;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1, KK2: the three addresses (criteria 1, 2) ----

    [Fact]
    public async Task ThePublishedTexts_ShowTheLastVersionWhole_AndTheProcessGuideAnswersWhileSalesAreClosed_KK1()
    {
        await InsertPublishedAsync("PrivacyNotice", 1, "<p>eski aydınlatma</p>", "Avrupa");
        await InsertPublishedAsync("PrivacyNotice", 2, "<p>son <strong>aydınlatma</strong> metni</p>", "Avrupa");
        await InsertPublishedAsync("CookiePolicy", 1, "<p>cerez politikasi yayinda</p>", null);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory, panel: false);

        (HttpStatusCode noticeStatus, string notice) = await GetAsync(client, "/aydinlatma-metni");
        (HttpStatusCode cookieStatus, string cookie) = await GetAsync(client, "/cerez-politikasi");
        (HttpStatusCode guideStatus, string guide) = await GetAsync(client, "/islem-rehberi");

        Assert.Equal(HttpStatusCode.OK, noticeStatus);
        Assert.Contains("son <strong>aydınlatma</strong> metni", notice, StringComparison.Ordinal);
        Assert.DoesNotContain("eski aydınlatma", notice, StringComparison.Ordinal);
        Assert.Contains($"<h1 class=\"legal-text__title\">{LegalTextPageTexts.PrivacyNoticeTitle}</h1>", notice, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, cookieStatus);
        Assert.Contains("cerez politikasi yayinda", cookie, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, guideStatus);
        foreach (ProcessGuideSection section in ProcessGuide.Sections)
        {
            Assert.Contains(section.Title, guide, StringComparison.Ordinal);
        }

        // The sales are closed here (no return address, no payment method), and the guide is still there.
        SalesGate gate = await ReadGateAsync(factory);
        Assert.False(gate.IsOpen);
    }

    [Fact]
    public async Task AnUnpublishedNoticeAndPolicy_Answer404_WhileTheProcessGuideAnswers200_KK2()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory, panel: false);

        (HttpStatusCode noticeStatus, string notice) = await GetAsync(client, "/aydinlatma-metni");
        (HttpStatusCode cookieStatus, string cookie) = await GetAsync(client, "/cerez-politikasi");
        (HttpStatusCode guideStatus, _) = await GetAsync(client, "/islem-rehberi");

        Assert.Equal(HttpStatusCode.NotFound, noticeStatus);
        Assert.Equal(HttpStatusCode.NotFound, cookieStatus);
        Assert.Equal(HttpStatusCode.OK, guideStatus);

        // A draft is not a publication: the draft's text is never on the page.
        Assert.DoesNotContain("aydınlatma metni", notice, StringComparison.Ordinal);
        Assert.DoesNotContain("çerez politikası taslağı", cookie, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ThePanelScreen_RefusesAGuest_KK3()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient guest = ClientOf(factory, panel: false);

        using HttpResponseMessage response = await guest.GetAsync(PanelPath, Ct);

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    // ---- KK3: four lines, two sections, the version of the form (criterion 3) ----

    [Fact]
    public async Task TheScreen_ShowsFourLines_TwoSections_TheGeneratedTexts_AndEachFormCarriesTheDraftVersion_KK3()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory, panel: true);

        (HttpStatusCode status, string html) = await GetAsync(client, PanelPath);

        Assert.Equal(HttpStatusCode.OK, status);
        string rows = Region(html, "id=\"legal-text-rows\"", "</ul>");
        Assert.Equal(4, Regex.Count(rows, "<li class=\"legal-texts__row\">", RegexOptions.CultureInvariant, PatternTimeout));
        foreach (string title in new[] { LegalTextsModel.PrivacyNoticeTitle, LegalTextsModel.CookiePolicyTitle, LegalTextsModel.PreInformationFormTitle, LegalTextsModel.DistanceSalesContractTitle })
        {
            Assert.Contains(title, rows, StringComparison.Ordinal);
        }

        Assert.Equal(2, Regex.Count(html, "<section class=\"legal-section\" id=\"(privacy-notice|cookie-policy)\"", RegexOptions.CultureInvariant, PatternTimeout));
        string privacy = SectionOf(html, "privacy-notice");
        string cookie = SectionOf(html, "cookie-policy");
        Assert.Equal("1", FieldValue(privacy, "Input.Version"));
        Assert.Equal("1", FieldValue(cookie, "Input.Version"));
        Assert.Equal("PrivacyNotice", FieldValue(privacy, "Input.LegalTextType"));
        Assert.Equal("CookiePolicy", FieldValue(cookie, "Input.LegalTextType"));
        Assert.Contains("name=\"Input.HostingLocation\"", privacy, StringComparison.Ordinal);
        Assert.DoesNotContain("Input.HostingLocation", cookie, StringComparison.Ordinal);

        // The generated texts are read only: their current state, no form.
        string generated = html[html.IndexOf("Ayarlardan üretilen metinler", StringComparison.Ordinal)..];
        foreach (string type in new[] { "PreInformationForm", "DistanceSalesContract" })
        {
            string text = await ScalarAsync<string>($"SELECT text FROM legal_text_versions WHERE legal_text_type = '{type}' ORDER BY version_number DESC LIMIT 1");
            int number = await ScalarAsync<int>($"SELECT version_number FROM legal_text_versions WHERE legal_text_type = '{type}' ORDER BY version_number DESC LIMIT 1");
            Assert.False(string.IsNullOrWhiteSpace(text));
            Assert.Contains(text.ReplaceLineEndings("\n"), generated.ReplaceLineEndings("\n"), StringComparison.Ordinal);
            Assert.Contains($"Yayındaki sürüm: {number} · ", generated, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("<form", generated, StringComparison.Ordinal);
        foreach (string setting in LegalTextsModel.FeedingSettings)
        {
            Assert.Contains(setting, generated, StringComparison.Ordinal);
        }
    }

    // ---- KK4: version, difference, completion, gates (criterion 4) ----

    [Fact]
    public async Task TheSection_SaysTheVersionInForceItsDate_TheDifference_TheCompletion_AndTheClosedGates_KK4()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory, panel: true);

        (_, string before) = await GetAsync(client, PanelPath);
        string privacyBefore = SectionOf(before, "privacy-notice");
        Assert.Contains(LegalTextsModel.NotPublishedText, privacyBefore, StringComparison.Ordinal);
        Assert.Contains(LegalTextsModel.HostingLocationMissingText, privacyBefore, StringComparison.Ordinal);
        Assert.DoesNotContain(LegalTextsModel.CompleteText, privacyBefore, StringComparison.Ordinal);
        Assert.Contains(LegalTextsModel.PrivacyNoticeGateNote, privacyBefore, StringComparison.Ordinal);
        Assert.Contains(LegalTextsModel.CookiePolicyGateNote, SectionOf(before, "cookie-policy"), StringComparison.Ordinal);
        Assert.Contains(LegalTextsModel.SalesAndEntriesClosedText, before, StringComparison.Ordinal);

        // A published notice with the same text: the number and the date show, the draft does not differ and the notice is complete.
        await InsertPublishedAsync("PrivacyNotice", 1, "<p>{{LegalName}} aydınlatma metni</p>", "Avrupa");
        await ExecuteAsync("UPDATE legal_text_versions SET hosting_location = 'Avrupa' WHERE legal_text_type = 'PrivacyNotice' AND published_at IS NULL");
        (_, string same) = await GetAsync(client, PanelPath);
        string privacySame = SectionOf(same, "privacy-notice");
        Assert.Contains("Yayındaki sürüm: 1 · ", privacySame, StringComparison.Ordinal);
        Assert.DoesNotContain(LegalTextsModel.DiffersText, privacySame, StringComparison.Ordinal);
        Assert.Contains(LegalTextsModel.CompleteText, privacySame, StringComparison.Ordinal);
        // The note stays while the cookie policy waits too: the entries open when both texts are published.
        Assert.Contains(LegalTextsModel.PrivacyNoticeGateNote, privacySame, StringComparison.Ordinal);
        Assert.Contains(LegalTextsModel.SalesAndEntriesClosedText, same, StringComparison.Ordinal);

        // A draft that differs from the version in force is said to differ.
        await ExecuteAsync("UPDATE legal_text_versions SET text = '<p>başka metin</p>' WHERE legal_text_type = 'PrivacyNotice' AND published_at IS NULL");
        (_, string differs) = await GetAsync(client, PanelPath);
        Assert.Contains(LegalTextsModel.DiffersText, SectionOf(differs, "privacy-notice"), StringComparison.Ordinal);

        // The cookie policy published: the note of its section and the line of the closed gates both go.
        await InsertPublishedAsync("CookiePolicy", 1, "<p>çerez</p>", null);
        (_, string opened) = await GetAsync(client, PanelPath);
        Assert.DoesNotContain(LegalTextsModel.SalesAndEntriesClosedText, opened, StringComparison.Ordinal);
        Assert.DoesNotContain(LegalTextsModel.CookiePolicyGateNote, opened, StringComparison.Ordinal);
        Assert.DoesNotContain(LegalTextsModel.PrivacyNoticeGateNote, opened, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheCompletionLine_NamesEachMissingIdentityField_WithALinkToTheIdentityScreen_KK4()
    {
        await ExecuteAsync("UPDATE companies SET legal_name = NULL, kep_address = NULL");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory, panel: true);

        (_, string html) = await GetAsync(client, PanelPath);

        string privacy = SectionOf(html, "privacy-notice");
        Assert.Contains(LegalTextsModel.IdentityMissingPrefix + "Unvan veya ad soyad", privacy, StringComparison.Ordinal);
        Assert.Contains(LegalTextsModel.IdentityMissingPrefix + "KEP adresi", privacy, StringComparison.Ordinal);
        Assert.Contains($"href=\"{PanelFramePaths.FirmIdentityAndSales}\"", privacy, StringComparison.Ordinal);
    }

    // ---- KK5: the placeholders with the current values (criterion 5) ----

    [Fact]
    public async Task ThePreview_FillsThePlaceholdersWithTheCurrentIdentity_AndFollowsAnIdentityChangeWithoutANewDraft_KK5()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory, panel: true);

        (_, string html) = await GetAsync(client, PanelPath);
        string section = SectionOf(html, "privacy-notice");
        string preview = section[section.IndexOf("id=\"privacy-notice-preview\"", StringComparison.Ordinal)..];
        string textarea = Region(SectionOf(html, "privacy-notice"), "<textarea class=\"field__control\" id=\"privacy-notice-text\"", "</textarea>");

        Assert.Contains("Yılmaz Ticaret aydınlatma metni", preview, StringComparison.Ordinal);
        Assert.DoesNotContain("{{LegalName}}", preview, StringComparison.Ordinal);
        Assert.Contains("{{LegalName}}", textarea, StringComparison.Ordinal);

        await ExecuteAsync("UPDATE companies SET legal_name = 'Yeni Unvan Ltd'");
        (_, string changed) = await GetAsync(client, PanelPath);
        Assert.Contains("Yeni Unvan Ltd aydınlatma metni", SectionOf(changed, "privacy-notice"), StringComparison.Ordinal);
        Assert.Equal("1", FieldValue(SectionOf(changed, "privacy-notice"), "Input.Version"));
    }

    // ---- KK6: the draft save (criterion 6) ----

    [Fact]
    public async Task TheDraftSave_CleansTheText_KeepsThePublishedVersionAndTheTrail_RaisesTheDraftVersion_AndAnswers303WithTheNotice_KK6()
    {
        await InsertPublishedAsync("CookiePolicy", 1, "<p>yayındaki çerez</p>", null);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory, panel: true);

        using HttpResponseMessage response = await client.PostAsync(
            PanelPath + "?handler=SaveLegalTextDraft",
            Form(("Input.LegalTextType", "CookiePolicy"), ("Input.Text", "<p>yeni <strong>taslak</strong></p><script>alert(1)</script>"), ("Input.Version", "1")),
            Ct);

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        string location = response.Headers.Location?.OriginalString ?? string.Empty;
        Assert.StartsWith(PanelPath, location, StringComparison.Ordinal);
        Assert.Contains("notice=Saved", location, StringComparison.Ordinal);
        string draft = await ScalarAsync<string>("SELECT text FROM legal_text_versions WHERE legal_text_type = 'CookiePolicy' AND published_at IS NULL");
        Assert.Contains("<strong>taslak</strong>", draft, StringComparison.Ordinal);
        Assert.DoesNotContain("script", draft, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2, await ScalarAsync<int>("SELECT version FROM legal_text_versions WHERE legal_text_type = 'CookiePolicy' AND published_at IS NULL"));
        Assert.Equal("<p>yayındaki çerez</p>", await ScalarAsync<string>("SELECT text FROM legal_text_versions WHERE legal_text_type = 'CookiePolicy' AND published_at IS NOT NULL"));
        Assert.Equal(1, await ScalarAsync<int>("SELECT count(*)::int FROM legal_text_versions WHERE legal_text_type = 'CookiePolicy' AND published_at IS NOT NULL"));
        Assert.Equal(0, await ScalarAsync<int>("SELECT count(*)::int FROM audit_logs WHERE target_record_type = 'LegalTextVersion'"));

        // The page the redirect leads to says "Kaydedildi" and carries the new version; the same page without the parameter does not.
        (_, string saved) = await GetAsync(client, location);
        Assert.Contains("Kaydedildi", saved, StringComparison.Ordinal);
        Assert.Equal("2", FieldValue(SectionOf(saved, "cookie-policy"), "Input.Version"));
        (_, string plain) = await GetAsync(client, PanelPath);
        Assert.DoesNotContain("Kaydedildi", plain, StringComparison.Ordinal);
        Assert.Contains(LegalTextsModel.DiffersText, SectionOf(plain, "cookie-policy"), StringComparison.Ordinal);
    }

    // ---- KK7: the four 422 reasons, the 409 buttons (criterion 7) ----

    [Fact]
    public async Task AnEmptyText_Is422_InThatSectionOnly_AndNothingIsWritten_KK7()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory, panel: true);

        using HttpResponseMessage response = await client.PostAsync(
            PanelPath + "?handler=SaveLegalTextDraft",
            Form(("Input.LegalTextType", "CookiePolicy"), ("Input.Text", "   "), ("Input.Version", "1")),
            Ct);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains("aria-invalid=\"true\"", SectionOf(html, "cookie-policy"), StringComparison.Ordinal);
        Assert.DoesNotContain("aria-invalid=\"true\"", SectionOf(html, "privacy-notice"), StringComparison.Ordinal);
        Assert.Equal("<p>çerez politikası taslağı</p>", await ScalarAsync<string>("SELECT text FROM legal_text_versions WHERE legal_text_type = 'CookiePolicy' AND published_at IS NULL"));
        Assert.Equal(1, await ScalarAsync<int>("SELECT version FROM legal_text_versions WHERE legal_text_type = 'CookiePolicy' AND published_at IS NULL"));
        Assert.Equal("1", FieldValue(SectionOf(html, "cookie-policy"), "Input.Version"));
    }

    [Fact]
    public async Task EmptyingAFilledHostingLocation_Is422_AndTheLocationStays_KK7()
    {
        await ExecuteAsync("UPDATE legal_text_versions SET hosting_location = 'Avrupa' WHERE legal_text_type = 'PrivacyNotice' AND published_at IS NULL");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory, panel: true);

        using HttpResponseMessage response = await client.PostAsync(
            PanelPath + "?handler=SaveLegalTextDraft",
            Form(("Input.LegalTextType", "PrivacyNotice"), ("Input.Text", "<p>metin</p>"), ("Input.HostingLocation", ""), ("Input.Version", "1")),
            Ct);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains("aria-invalid=\"true\"", Region(SectionOf(html, "privacy-notice"), "id=\"privacy-notice-hosting-location\"", ">"), StringComparison.Ordinal);
        Assert.Equal("Avrupa", await ScalarAsync<string>("SELECT hosting_location FROM legal_text_versions WHERE legal_text_type = 'PrivacyNotice' AND published_at IS NULL"));
    }

    [Theory]
    [InlineData("Imprint")]
    [InlineData("")]
    [InlineData("PreInformationForm")]
    [InlineData("DistanceSalesContract")]
    public async Task ATextTypeOutsideTheTwoEditedOnes_Is422_AndNothingIsWritten_KK7(string type)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory, panel: true);

        foreach (string handler in new[] { "SaveLegalTextDraft", "PublishLegalText" })
        {
            using HttpResponseMessage response = await client.PostAsync(
                PanelPath + "?handler=" + handler,
                Form(("Input.LegalTextType", type), ("Input.Text", "<p>metin</p>"), ("Input.Version", "1")),
                Ct);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        // The application writes the first versions of the two generated texts at its start; the two firm-edited texts stay unpublished drafts at version 1.
        Assert.Equal(2, await ScalarAsync<int>("SELECT count(*)::int FROM legal_text_versions WHERE legal_text_type IN ('PrivacyNotice', 'CookiePolicy')"));
        Assert.Equal(0, await ScalarAsync<int>("SELECT count(*)::int FROM legal_text_versions WHERE legal_text_type IN ('PrivacyNotice', 'CookiePolicy') AND published_at IS NOT NULL"));
        Assert.Equal(2, await ScalarAsync<int>("SELECT count(*)::int FROM legal_text_versions WHERE legal_text_type IN ('PrivacyNotice', 'CookiePolicy') AND version = 1"));
        Assert.Equal(0, await ScalarAsync<int>("SELECT count(*)::int FROM audit_logs WHERE target_record_type = 'LegalTextVersion'"));
    }

    [Fact]
    public async Task AStaleVersion_Is409_WithTheTwoButtonsInThatSectionOnly_AndTheCurrentVersionInTheForm_KK7()
    {
        await ExecuteAsync("UPDATE legal_text_versions SET text = '<p>başkasının</p>', version = 2 WHERE legal_text_type = 'CookiePolicy' AND published_at IS NULL");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory, panel: true);

        using HttpResponseMessage response = await client.PostAsync(
            PanelPath + "?handler=SaveLegalTextDraft",
            Form(("Input.LegalTextType", "CookiePolicy"), ("Input.Text", "<p>benim</p>"), ("Input.Version", "1")),
            Ct);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ErrorCodeLeakCheck.AssertNone(response, html);
        string cookie = SectionOf(html, "cookie-policy");
        string privacy = SectionOf(html, "privacy-notice");
        Assert.Contains("Güncel hâli aç", cookie, StringComparison.Ordinal);
        Assert.Contains("Yine de kaydet", cookie, StringComparison.Ordinal);
        Assert.DoesNotContain("Güncel hâli aç", privacy, StringComparison.Ordinal);
        Assert.DoesNotContain("Yine de kaydet", privacy, StringComparison.Ordinal);
        Assert.Contains("Taslağı kaydet", privacy, StringComparison.Ordinal);
        Assert.Equal("2", FieldValue(cookie, "Input.Version"));
        Assert.Contains("benim", cookie, StringComparison.Ordinal);
        Assert.Equal("<p>başkasının</p>", await ScalarAsync<string>("SELECT text FROM legal_text_versions WHERE legal_text_type = 'CookiePolicy' AND published_at IS NULL"));
    }

    // ---- KK8: the publication opens a new version, also for the same text; no confirmation, no notice (criterion 8) ----

    [Fact]
    public async Task APublication_OfTheSameText_OpensANewNumber_WritesATrailRow_AndHasNoConfirmationNoticeOrReconsent_KK8()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory, panel: true);
        (_, string page) = await GetAsync(client, PanelPath);
        Assert.DoesNotContain("data-js=\"confirm", page, StringComparison.Ordinal);

        using HttpResponseMessage first = await client.PostAsync(
            PanelPath + "?handler=PublishLegalText",
            Form(("Input.LegalTextType", "CookiePolicy"), ("Input.Text", "<p>aynı metin</p>"), ("Input.Version", "1")),
            Ct);
        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.Equal(PanelPath, first.Headers.Location?.OriginalString);
        (_, string afterFirst) = await GetAsync(client, PanelPath);
        string version = FieldValue(SectionOf(afterFirst, "cookie-policy"), "Input.Version")!;

        using HttpResponseMessage second = await client.PostAsync(
            PanelPath + "?handler=PublishLegalText",
            Form(("Input.LegalTextType", "CookiePolicy"), ("Input.Text", "<p>aynı metin</p>"), ("Input.Version", version)),
            Ct);

        Assert.Equal(HttpStatusCode.SeeOther, second.StatusCode);
        Assert.Equal(PanelPath, second.Headers.Location?.OriginalString);
        Assert.Equal([1, 2], await NumbersAsync("CookiePolicy"));
        Assert.Equal(2, await ScalarAsync<int>("SELECT count(*)::int FROM audit_logs WHERE target_record_type = 'LegalTextVersion'"));
        (_, string after) = await GetAsync(client, PanelPath);
        Assert.Contains("Yayındaki sürüm: 2", SectionOf(after, "cookie-policy"), StringComparison.Ordinal);
        Assert.DoesNotContain("Kaydedildi", after, StringComparison.Ordinal);
        Assert.DoesNotContain("data-js=\"confirm", after, StringComparison.Ordinal);
        (HttpStatusCode publicStatus, string publicHtml) = await GetAsync(ClientOf(factory, panel: false), "/cerez-politikasi");
        Assert.Equal(HttpStatusCode.OK, publicStatus);
        Assert.Contains("aynı metin", publicHtml, StringComparison.Ordinal);
    }

    // ---- KK9: an incomplete notice writes no draft; a second publication gets 409 (criterion 9) ----

    [Fact]
    public async Task AnIncompleteNotice_Is422_WritesNoDraft_AndNamesTheMissingFields_KK9()
    {
        await ExecuteAsync("UPDATE companies SET legal_name = NULL");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory, panel: true);

        using HttpResponseMessage response = await client.PostAsync(
            PanelPath + "?handler=PublishLegalText",
            Form(("Input.LegalTextType", "PrivacyNotice"), ("Input.Text", "<p>yeni metin</p>"), ("Input.HostingLocation", "Avrupa"), ("Input.Version", "1")),
            Ct);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string privacy = SectionOf(html, "privacy-notice");
        Assert.Contains(LegalTextsModel.PublicationIncompleteText, privacy, StringComparison.Ordinal);
        Assert.Contains(LegalTextsModel.IdentityMissingPrefix + "Unvan veya ad soyad", privacy, StringComparison.Ordinal);
        Assert.Equal("<p>{{LegalName}} aydınlatma metni</p>", await ScalarAsync<string>("SELECT text FROM legal_text_versions WHERE legal_text_type = 'PrivacyNotice' AND published_at IS NULL"));
        Assert.Equal(1, await ScalarAsync<int>("SELECT version FROM legal_text_versions WHERE legal_text_type = 'PrivacyNotice' AND published_at IS NULL"));
        Assert.Empty(await NumbersAsync("PrivacyNotice"));
        Assert.Equal(0, await ScalarAsync<int>("SELECT count(*)::int FROM audit_logs WHERE target_record_type = 'LegalTextVersion'"));
    }

    [Fact]
    public async Task ASecondPublicationWithTheOldDraftVersion_Is409_AndOpensNoSecondVersion_KK9()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory, panel: true);
        static FormUrlEncodedContent Publish() => Form(("Input.LegalTextType", "CookiePolicy"), ("Input.Text", "<p>yayın</p>"), ("Input.Version", "1"));

        using HttpResponseMessage first = await client.PostAsync(PanelPath + "?handler=PublishLegalText", Publish(), Ct);
        using HttpResponseMessage second = await client.PostAsync(PanelPath + "?handler=PublishLegalText", Publish(), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Equal([1], await NumbersAsync("CookiePolicy"));
    }

    [Fact]
    public async Task TwoPublicationsAtOnce_WithTheSameDraftVersion_OpenOneVersion_AndTheOtherGets409_KK9()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient first = ClientOf(factory, panel: true);
        using HttpClient second = ClientOf(factory, panel: true);
        static FormUrlEncodedContent Publish(string text) => Form(("Input.LegalTextType", "CookiePolicy"), ("Input.Text", text), ("Input.Version", "1"));

        Task<HttpResponseMessage> a = first.PostAsync(PanelPath + "?handler=PublishLegalText", Publish("<p>a</p>"), Ct);
        Task<HttpResponseMessage> b = second.PostAsync(PanelPath + "?handler=PublishLegalText", Publish("<p>b</p>"), Ct);
        HttpResponseMessage[] responses = await Task.WhenAll(a, b);
        try
        {
            Assert.Equal([HttpStatusCode.SeeOther, HttpStatusCode.Conflict], responses.Select(r => r.StatusCode).Order());
            Assert.Equal([1], await NumbersAsync("CookiePolicy"));
        }
        finally
        {
            foreach (HttpResponseMessage response in responses)
            {
                response.Dispose();
            }
        }
    }

    // ---- KK10: the gates and the cookie (criterion 10) ----

    [Fact]
    public async Task TheFirstPublicationsOfTheTwoTexts_OpenTheDataCollectingGates_AndTheCookiePolicyOpensTheStorefrontCookie_KK10()
    {
        await using ShopfolioWebApplicationFactory baseFactory = new(_database.Dsn);
        using WebApplicationFactory<Program> factory = WithSamples(baseFactory).WithWebHostBuilder(b => b.ConfigureServices(s => s.AddSingleton<Microsoft.AspNetCore.Hosting.IStartupFilter, CookieProbeStartupFilter>()));
        using HttpClient panel = ClientOf(factory, panel: true);
        using HttpClient storefront = ClientOf(factory, panel: false);
        SalesGate closed = await ReadGateAsync(factory);
        Assert.Contains(SalesGateCondition.PrivacyNotice, closed.UnmetConditions);
        Assert.Contains(SalesGateCondition.CookiePolicy, closed.UnmetConditions);
        using (HttpResponseMessage silent = await storefront.GetAsync(CookieProbeStartupFilter.CartPath, Ct))
        {
            Assert.Empty(CookieHeaders.Read(silent));
        }

        using HttpResponseMessage notice = await panel.PostAsync(
            PanelPath + "?handler=PublishLegalText",
            Form(("Input.LegalTextType", "PrivacyNotice"), ("Input.Text", "<p>{{LegalName}} aydınlatma</p>"), ("Input.HostingLocation", "Avrupa"), ("Input.Version", "1")),
            Ct);
        Assert.Equal(HttpStatusCode.SeeOther, notice.StatusCode);
        SalesGate afterNotice = await ReadGateAsync(factory);
        Assert.DoesNotContain(SalesGateCondition.PrivacyNotice, afterNotice.UnmetConditions);
        Assert.Contains(SalesGateCondition.CookiePolicy, afterNotice.UnmetConditions);
        Assert.False(afterNotice.IsDataCollectionOpen);

        using HttpResponseMessage policy = await panel.PostAsync(
            PanelPath + "?handler=PublishLegalText",
            Form(("Input.LegalTextType", "CookiePolicy"), ("Input.Text", "<p>çerez</p>"), ("Input.Version", "1")),
            Ct);
        Assert.Equal(HttpStatusCode.SeeOther, policy.StatusCode);
        SalesGate opened = await ReadGateAsync(factory);
        Assert.DoesNotContain(SalesGateCondition.PrivacyNotice, opened.UnmetConditions);
        Assert.DoesNotContain(SalesGateCondition.CookiePolicy, opened.UnmetConditions);
        Assert.True(opened.IsDataCollectionOpen);
        (_, string html) = await GetAsync(panel, PanelPath);
        Assert.DoesNotContain(LegalTextsModel.SalesAndEntriesClosedText, html, StringComparison.Ordinal);

        using HttpResponseMessage cookie = await storefront.GetAsync(CookieProbeStartupFilter.CartPath, Ct);
        Assert.NotEmpty(CookieHeaders.Read(cookie));
    }

    // ---- helpers ----

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
    }

    private WebApplicationFactory<Program> Open() => WithSamples(new ShopfolioWebApplicationFactory(_database.Dsn));

    private static WebApplicationFactory<Program> WithSamples(ShopfolioWebApplicationFactory factory) =>
        factory.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(ForgeryTokenSampleModel).Assembly)));

    private HttpClient ClientOf(WebApplicationFactory<Program> factory, bool panel)
    {
        HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        if (panel)
        {
            client.DefaultRequestHeaders.Add(HeaderNames.Cookie, $"{CookieNames.AdminSession}={_adminKey}");
        }

        return client;
    }

    private static FormUrlEncodedContent Form(params (string Name, string Value)[] fields) =>
        new(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value)));

    private static async Task<(HttpStatusCode Status, string Html)> GetAsync(HttpClient client, string path)
    {
        using HttpResponseMessage response = await client.GetAsync(new Uri(path, UriKind.Relative), Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static async Task<SalesGate> ReadGateAsync(WebApplicationFactory<Program> factory)
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ISettingsModule>().GetSalesGateAsync(Ct);
    }

    private static string Region(string html, string startMarker, string endMarker)
    {
        int start = html.IndexOf(startMarker, StringComparison.Ordinal);
        Assert.True(start >= 0, $"No '{startMarker}' in the response.");
        int end = html.IndexOf(endMarker, start, StringComparison.Ordinal);
        Assert.True(end > start, $"No '{endMarker}' after '{startMarker}'.");
        return html[start..end];
    }

    private static string SectionOf(string html, string id) => Region(html, $"<section class=\"legal-section\" id=\"{id}\"", "</section>");

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

    private async Task InsertPublishedAsync(string type, int number, string text, string? hostingLocation)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(
            "INSERT INTO legal_text_versions (id, created_at, legal_text_type, version_number, published_at, text, hosting_location, version) "
            + "VALUES (@id, now(), @type, @number, now(), @text, @hosting, 1)",
            connection);
        command.Parameters.AddWithValue("id", EntityIds.New());
        command.Parameters.AddWithValue("type", type);
        command.Parameters.AddWithValue("number", number);
        command.Parameters.AddWithValue("text", text);
        command.Parameters.AddWithValue("hosting", (object?)hostingLocation ?? DBNull.Value);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<List<int>> NumbersAsync(string type)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new($"SELECT version_number FROM legal_text_versions WHERE legal_text_type = '{type}' AND published_at IS NOT NULL ORDER BY version_number", connection);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(Ct);
        List<int> numbers = [];
        while (await reader.ReadAsync(Ct))
        {
            numbers.Add(reader.GetInt32(0));
        }

        return numbers;
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
