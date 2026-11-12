using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.IntegrationTests.Pages.Panel;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the contact page tests (T506): one database copied from the template.</summary>
[CollectionDefinition(ContactPageDefinition.Name)]
public sealed class ContactPageDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "ContactPage";
}

/// <summary>
/// The contact page and the form's send on the opened application with a real PostgreSQL (T506 criteria 3-10; KK3-KK10; 07 UC-14, UC-15; 04 5.10): the identity block, the form and the branches
/// with the map link; the member's name and e-mail that start the form; the closed form while the gate of the data-collecting entries is closed; the state after the send that the address code
/// opens and the code-free new form; the 303 that writes the request open and sends nothing to the sender; the 422 with the three reasons together and with the closed gate; the 429 of the limit L-4
/// with the attempts recorded; the trap field that leaves no trace and answers as a success; the second send that opens a second request with the firm's notice F-2.
/// </summary>
[Collection(ContactPageDefinition.Name)]
public sealed class ContactPageTests : IAsyncLifetime
{
    private const string FirmEmail = "firma@example.com";

    private const string SenderEmail = "gonderen@example.org";

    private static readonly DateTimeOffset Now = new(2026, 11, 11, 9, 0, 0, TimeSpan.Zero);

    private static readonly Regex _branchNames = new("""<h3 class="branch__name">(.*?)</h3>""", RegexOptions.Singleline | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));

    private static readonly Regex _tags = new("<[^>]+>", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));

    private readonly CollectionDatabaseFixture _database;

    public ContactPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE contact_requests, dispatch_records, attempt_records, branches, legal_text_versions CASCADE");
        await ExecuteAsync(
            "UPDATE companies SET company_type = NULL, legal_name = NULL, tax_id_number = NULL, province_id = NULL, district = NULL, address_line = NULL, phone = NULL, email = NULL, "
            + "kep_address = NULL, chamber_name = NULL, professional_conduct_rules = NULL, etbis_verification_url = NULL");
        await ExecuteAsync("UPDATE company_settings SET menu_name_contact = 'İletişim'");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private WebApplicationFactory<Program> Open() => new ShopfolioWebApplicationFactory(_database.Dsn);

    // The forgery token page of the test host lives in the test assembly (ForgeryTokenHandler): a test that posts opens the application with that assembly's pages.
    private WebApplicationFactory<Program> OpenForPost() => new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s =>
        s.AddRazorPages().AddApplicationPart(typeof(AttemptSampleModel).Assembly)));

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task ExecuteAsync(string sql)
    {
        await using AppDbContext db = NewContext();
        await db.Database.ExecuteSqlRawAsync(sql, Ct);
    }

    private async Task<long> CountAsync(string table)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new($"SELECT count(*) FROM {table}", connection);
        return (long)(await command.ExecuteScalarAsync(Ct))!;
    }

    // The identity of a firm and, with open = true, the two published legal texts that open the gate of the data-collecting entries (02 3.1.5): the privacy notice with its hosting location and the cookie policy.
    private async Task SeedFirmAsync(bool open)
    {
        Guid province;
        await using (AppDbContext db = NewContext())
        {
            province = await db.Provinces.AsNoTracking().OrderBy(p => p.Code).Select(p => p.Id).FirstAsync(Ct);
        }

        await ExecuteAsync(
            $"UPDATE companies SET company_type = 'Tradesman', legal_name = 'Örnek Ticaret', tax_id_number = '1111111111', province_id = '{province}', district = 'Çankaya', "
            + $"address_line = 'Kızılay 2', phone = '3121234567', email = '{FirmEmail}', kep_address = 'firma@hs01.kep.tr', chamber_name = 'Ankara Esnaf Odası', professional_conduct_rules = 'Meslek kuralları'");
        if (open)
        {
            await using AppDbContext db = NewContext();
            db.LegalTextVersions.Add(LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, 1, Now, "metin", Now).WithHosting());
            db.LegalTextVersions.Add(LegalTextVersion.CreatePublished(LegalTextType.CookiePolicy, 1, Now, "metin", Now));
            await db.SaveChangesAsync(Ct);
        }
    }

    private async Task SeedBranchAsync(string name, int order, PublishStatus status = PublishStatus.Published, string? mapUrl = "https://www.google.com/maps/place/Ankara")
    {
        Guid province;
        await using (AppDbContext db = NewContext())
        {
            province = await db.Provinces.AsNoTracking().OrderBy(p => p.Code).Select(p => p.Id).FirstAsync(Ct);
        }

        Branch branch = Branch.Create(name, order, Now.AddMinutes(order));
        branch.ProvinceId = province;
        branch.District = "Yenimahalle";
        branch.AddressLine = $"{name} Caddesi 5";
        branch.Phone = "3125550000";
        branch.WorkingHours = "09:00-18:00";
        branch.MapUrl = mapUrl;
        branch.PublishStatus = status;
        await using AppDbContext write = NewContext();
        write.Branches.Add(branch);
        await write.SaveChangesAsync(Ct);
    }

    private static async Task<(HttpStatusCode Status, string Body)> GetAsync(HttpClient client, string address, string? cookie = null)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, address);
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        if (cookie is not null)
        {
            request.Headers.Add("Cookie", cookie);
        }

        using HttpResponseMessage response = await client.SendAsync(request, Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static Dictionary<string, string> Fields(string? website = null, string email = SenderEmail) => new()
    {
        ["Input.Name"] = "Ayşe Yılmaz",
        ["Input.Email"] = email,
        ["Input.Phone"] = "0532 123 45 67",
        ["Input.ContactTopic"] = "AboutOrder",
        ["Input.Message"] = "Siparişim hakkında bilgi almak istiyorum.",
        ["Input.Website"] = website ?? string.Empty,
    };

    private static async Task<(HttpStatusCode Status, string Body, string? Location)> PostAsync(HttpClient client, Dictionary<string, string> fields, string? cookie = null)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, "/iletisim?handler=SendContactRequest") { Content = new FormUrlEncodedContent(fields) };
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        if (cookie is not null)
        {
            request.Headers.Add("Cookie", cookie);
        }

        using HttpResponseMessage response = await client.SendAsync(request, Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)), response.Headers.Location?.OriginalString);
    }

    private static string FormOf(string html)
    {
        int marker = html.IndexOf("class=\"form contact-form\"", StringComparison.Ordinal);
        int start = marker < 0 ? -1 : html.LastIndexOf("<form", marker, StringComparison.Ordinal);
        Assert.True(start >= 0, "The contact form is not on the page.");
        return html[start..html.IndexOf("</form>", start, StringComparison.Ordinal)];
    }

    // --- KK3: identity block, form, branches with the map link; the member's values; the guest's empty form ---

    [Fact]
    public async Task ThePage_ShowsTheIdentityBlock_TheOpenForm_AndTheBranchesInTheFirmsOrderWithTheMapLink_KK3()
    {
        await SeedFirmAsync(open: true);
        await SeedBranchAsync("Merkez şube", 20);
        await SeedBranchAsync("Birinci şube", 10);
        await SeedBranchAsync("Taslak şube", 5, PublishStatus.Draft);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        (HttpStatusCode status, string html) = await GetAsync(client, "/iletisim");

        Assert.Equal(HttpStatusCode.OK, status);
        string contact = html[html.IndexOf("""<section class="contact" """, StringComparison.Ordinal)..];
        Assert.Contains("""<h1 class="contact__title" id="contact-title">İletişim</h1>""", contact, StringComparison.Ordinal);
        Assert.Contains("Örnek Ticaret", contact, StringComparison.Ordinal);
        Assert.Contains("Ankara Esnaf Odası", contact, StringComparison.Ordinal);
        Assert.Contains("""<dl class="identity-block">""", contact, StringComparison.Ordinal);
        Assert.DoesNotContain("etbis-band", contact, StringComparison.Ordinal);

        string form = FormOf(contact);
        foreach (string field in new[] { "Input.Name", "Input.Email", "Input.Phone", "Input.ContactTopic", "Input.Message", "Input.Website" })
        {
            Assert.Contains($"name=\"{field}\"", form, StringComparison.Ordinal);
        }

        Assert.Equal(5, Regex.Count(form, """<option value="[A-Za-z]+"[^>]*>""", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));
        Assert.Contains("""<button type="submit" """, form, StringComparison.Ordinal);
        Assert.DoesNotContain("type=\"checkbox\"", form, StringComparison.Ordinal);

        // The branches stand in the firm's order, the draft one is not shown to a visitor, and the map is a link only (nothing embedded).
        Assert.Equal(["Birinci şube", "Merkez şube"], _branchNames.Matches(contact).Select(m => _tags.Replace(m.Groups[1].Value, string.Empty).Trim()));
        Assert.Contains("""href="https://www.google.com/maps/place/Ankara" target="_blank" rel="noopener noreferrer">Haritada aç</a>""", contact, StringComparison.Ordinal);
        Assert.DoesNotContain("<iframe", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Taslak şube", html, StringComparison.Ordinal);
        Assert.DoesNotContain("branch--draft", html, StringComparison.Ordinal);
        Assert.Contains("09:00-18:00", contact, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AMembersNameAndEmail_StartTheForm_AndAGuestsFormIsEmpty_KK3()
    {
        await SeedFirmAsync(open: true);
        (string key, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        string memberEmail;
        await using (AppDbContext db = NewContext())
        {
            memberEmail = await db.Customers.AsNoTracking().Where(c => c.Id == customerId).Select(c => c.Email).SingleAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        (_, string member) = await GetAsync(client, "/iletisim", $"{CookieNames.Session}={key}");
        (_, string guest) = await GetAsync(client, "/iletisim");

        Assert.Matches("""name="Input\.Name" value="Test musteri" """, FormOf(member));
        Assert.Matches($"""name="Input\.Email" value="{Regex.Escape(memberEmail)}" """, FormOf(member));
        Assert.DoesNotContain("readonly", FormOf(member), StringComparison.Ordinal);
        Assert.Matches("""name="Input\.Name" value="" """, FormOf(guest));
        Assert.Matches("""name="Input\.Email" value="" """, FormOf(guest));
    }

    [Fact]
    public async Task APanelSession_SeesTheDraftBranchMarked_AndThePageIsClosedToTheSearchEngines_KK3()
    {
        await SeedFirmAsync(open: true);
        await SeedBranchAsync("Birinci şube", 10);
        await SeedBranchAsync("Taslak şube", 15, PublishStatus.Draft);
        (string adminKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        (_, string admin) = await GetAsync(client, "/iletisim", $"{CookieNames.AdminSession}={adminKey}");
        (_, string visitor) = await GetAsync(client, "/iletisim");

        Assert.Equal(["Birinci şube", "Taslak Taslak şube"], _branchNames.Matches(admin).Select(m => Regex.Replace(_tags.Replace(m.Groups[1].Value, string.Empty), @"\s+", " ", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)).Trim()));
        Assert.Contains("""<span class="branch__draft">Taslak</span>""", admin, StringComparison.Ordinal);
        Assert.Contains("""<meta name="robots" content="noindex""", admin, StringComparison.Ordinal);
        Assert.DoesNotContain("""<meta name="robots" content="noindex""", visitor, StringComparison.Ordinal);
    }

    // --- KK4: the closed gate ---

    [Fact]
    public async Task WhileTheGateOfTheDataCollectingEntriesIsClosed_TheFormIsClosed_NoButton_AndTheIdentityAndTheBranchesStay_KK4()
    {
        await SeedFirmAsync(open: false);
        await SeedBranchAsync("Merkez şube", 10);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        (HttpStatusCode status, string html) = await GetAsync(client, "/iletisim");

        Assert.Equal(HttpStatusCode.OK, status);
        string contact = html[html.IndexOf("""<section class="contact" """, StringComparison.Ordinal)..];
        Assert.Contains("Bu form şu anda kullanıma kapalı.", contact, StringComparison.Ordinal);
        Assert.DoesNotContain("<form", contact, StringComparison.Ordinal);
        Assert.DoesNotContain("Input.Name", contact, StringComparison.Ordinal);
        Assert.DoesNotContain("""<button type="submit" """, contact, StringComparison.Ordinal);
        Assert.Contains("Örnek Ticaret", contact, StringComparison.Ordinal);
        Assert.Contains("Merkez şube", contact, StringComparison.Ordinal);
        Assert.Contains("Haritada aç", contact, StringComparison.Ordinal);
    }

    // --- KK5: the state after the send; the form without the code ---

    [Fact]
    public async Task AnAddressWithTheContactRequestSentCode_ShowsTheStateAfterTheSend_AndTheNewFormLinkIsTheCodeFreeAddress_KK5()
    {
        await SeedFirmAsync(open: true);
        await SeedBranchAsync("Merkez şube", 10);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        (HttpStatusCode status, string html) = await GetAsync(client, "/iletisim?notice=ContactRequestSent");
        (_, string plain) = await GetAsync(client, "/iletisim");

        Assert.Equal(HttpStatusCode.OK, status);
        string contact = html[html.IndexOf("""<section class="contact" """, StringComparison.Ordinal)..];
        Assert.Contains("""id="contact-sent" """.TrimEnd(), contact, StringComparison.Ordinal);
        Assert.Contains("Mesajınız firmaya iletildi. Cevap, yazdığınız e-posta adresine gelecek.", contact, StringComparison.Ordinal);
        Assert.Contains("""href="/iletisim">Yeni bir mesaj yaz</a>""", contact, StringComparison.Ordinal);
        Assert.DoesNotContain("<form", contact, StringComparison.Ordinal);
        Assert.DoesNotMatch(@"(?i)(talep|başvuru|takip)\s*(no|numara)", contact);

        // The identity block, the branches and the page's name stay; the code-free address draws the form again, with no sent state.
        Assert.Contains("Örnek Ticaret", contact, StringComparison.Ordinal);
        Assert.Contains("Merkez şube", contact, StringComparison.Ordinal);
        Assert.DoesNotContain("contact-sent", plain, StringComparison.Ordinal);
        Assert.Contains("""<button type="submit" """, plain, StringComparison.Ordinal);
    }

    // --- KK6: the request is open; nothing goes to the sender ---

    [Fact]
    public async Task ASend_Is303ToTheContactPageWithTheCode_WritesTheRequestOpen_AndQueuesNoAcknowledgementForTheSender_KK6()
    {
        await SeedFirmAsync(open: true);
        await using WebApplicationFactory<Program> factory = OpenForPost();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, _, string? location) = await PostAsync(client, Fields());

        Assert.Equal(HttpStatusCode.SeeOther, status);
        Assert.Equal("/iletisim?notice=ContactRequestSent", location);
        await using AppDbContext db = NewContext();
        Assert.Equal(1, await db.ContactRequests.CountAsync(Ct));
        Assert.Equal("Open", await ScalarAsync("SELECT contact_request_status FROM contact_requests"));
        Assert.Equal("Ayşe Yılmaz", await ScalarAsync("SELECT name FROM contact_requests"));
        Assert.Equal(SenderEmail, await ScalarAsync("SELECT email FROM contact_requests"));
        Assert.Equal("AboutOrder", await ScalarAsync("SELECT contact_topic FROM contact_requests"));
        DispatchRecord row = await db.DispatchRecords.AsNoTracking().SingleAsync(Ct);
        Assert.Equal(DispatchType.F2, row.DispatchType);
        Assert.Equal(FirmEmail, row.RecipientEmail);
        Assert.Equal(0L, await CountAsync($"dispatch_records WHERE recipient_email = '{SenderEmail}'"));
    }

    private async Task<string> ScalarAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return (string)(await command.ExecuteScalarAsync(Ct))!;
    }

    // --- KK7: the three reasons of the 422, together; the closed gate ---

    [Fact]
    public async Task EmptyRequiredFields_AnInvalidEmailAndPhone_AndATopicOutsideTheFive_AreEachMarkedTogether_In422_WithTheSentValuesKept_KK7()
    {
        await SeedFirmAsync(open: true);
        await using WebApplicationFactory<Program> factory = OpenForPost();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Dictionary<string, string> empty = new(Fields()) { ["Input.Name"] = "  ", ["Input.Message"] = "", ["Input.Email"] = "", ["Input.ContactTopic"] = "" };
        (HttpStatusCode emptyStatus, string emptyBody, _) = await PostAsync(client, empty);

        Dictionary<string, string> invalid = new(Fields()) { ["Input.Email"] = "gecersiz", ["Input.Phone"] = "123" };
        (HttpStatusCode invalidStatus, string invalidBody, _) = await PostAsync(client, invalid);

        Dictionary<string, string> topic = new(Fields()) { ["Input.ContactTopic"] = "Genel soru" };
        (HttpStatusCode topicStatus, string topicBody, _) = await PostAsync(client, topic);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, emptyStatus);
        Assert.Equal(["Input_Name", "Input_Email", "Input_ContactTopic", "Input_Message"], SummaryTargets(emptyBody));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, invalidStatus);
        Assert.Equal(["Input_Email", "Input_Phone"], SummaryTargets(invalidBody));
        Assert.Contains("value=\"gecersiz\"", invalidBody, StringComparison.Ordinal);
        Assert.Contains("value=\"123\"", invalidBody, StringComparison.Ordinal);
        Assert.Contains("Ayşe Yılmaz", invalidBody, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, topicStatus);
        Assert.Equal(["Input_ContactTopic"], SummaryTargets(topicBody));

        // None of the three wrote a request or counted an attempt.
        Assert.Equal(0L, await CountAsync("contact_requests"));
        Assert.Equal(0L, await CountAsync("attempt_records"));
    }

    [Fact]
    public async Task WhileTheGateIsClosed_ASendIs422_WithThePageInItsClosedState_NothingIsWritten_OrCounted_KK7()
    {
        await SeedFirmAsync(open: false);
        await using WebApplicationFactory<Program> factory = OpenForPost();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string body, _) = await PostAsync(client, Fields());

        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.Contains("Bu form şu anda kullanıma kapalı.", body, StringComparison.Ordinal);
        Assert.DoesNotContain("""<button type="submit" """, body[body.IndexOf("""<section class="contact" """, StringComparison.Ordinal)..], StringComparison.Ordinal);
        Assert.Equal(0L, await CountAsync("contact_requests"));
        Assert.Equal(0L, await CountAsync("attempt_records"));
        Assert.Equal(0L, await CountAsync("dispatch_records"));
    }

    [Fact]
    public async Task EachRefusalOfTheSend_WritesItsOneRejectionLine_TheClosedGateWithDataCollectionClosed_TheFieldsWithTheirNames_TheLimitWithItsCode_KK7()
    {
        await SeedFirmAsync(open: true);
        CollectingLoggerProvider logs = new();
        await using WebApplicationFactory<Program> open = new ShopfolioWebApplicationFactory(_database.Dsn, loggerProvider: logs).WithWebHostBuilder(b => b.ConfigureServices(s =>
            s.AddRazorPages().AddApplicationPart(typeof(AttemptSampleModel).Assembly)));
        using HttpClient client = ForgeryTokenHandler.CreateClient(open);

        int beforeFields = RejectionLines(logs).Count;
        await PostAsync(client, new Dictionary<string, string>(Fields()) { ["Input.Email"] = "gecersiz" });
        string fieldLine = Assert.Single(RejectionLines(logs).Skip(beforeFields));
        Assert.Contains("Email", fieldLine, StringComparison.Ordinal);
        Assert.Contains(FieldCodes.InvalidEmail, fieldLine, StringComparison.Ordinal);

        int threshold = AttemptLimits.Of(AttemptLimit.L4, AttemptAxis.Ip).Threshold;
        for (int i = 0; i < threshold; i++)
        {
            await PostAsync(client, Fields());
        }

        int beforeLimit = RejectionLines(logs).Count;
        (HttpStatusCode limited, _, _) = await PostAsync(client, Fields());
        Assert.Equal(HttpStatusCode.TooManyRequests, limited);
        Assert.EndsWith(" " + ErrorCode.ContactRequestLimitExceeded, Assert.Single(RejectionLines(logs).Skip(beforeLimit)), StringComparison.Ordinal);

        await ExecuteAsync("TRUNCATE legal_text_versions CASCADE");
        int beforeGate = RejectionLines(logs).Count;
        (HttpStatusCode closed, _, _) = await PostAsync(client, Fields());
        Assert.Equal(HttpStatusCode.UnprocessableEntity, closed);
        Assert.EndsWith(" " + ErrorCode.DataCollectionClosed, Assert.Single(RejectionLines(logs).Skip(beforeGate)), StringComparison.Ordinal);
    }

    private static List<string> RejectionLines(CollectingLoggerProvider logs) =>
        [.. logs.Lines.Where(l => l.Contains("RejectionLog|", StringComparison.Ordinal) && l.Contains(LogEvents.ValidationRejected, StringComparison.Ordinal))];

    private static string[] SummaryTargets(string html)
    {
        int start = html.IndexOf("""<ul class="error-summary__list">""", StringComparison.Ordinal);
        Assert.True(start >= 0, "The error summary is not on the page.");
        string list = html[start..html.IndexOf("</ul>", start, StringComparison.Ordinal)];
        return [.. Regex.Matches(list, "href=\"#([^\"]+)\"", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)).Select(m => m.Groups[1].Value)];
    }

    // --- KK8: the limit L-4 ---

    [Fact]
    public async Task TheSendOverTheLimitL4_Is429_WithTheNeutralMessageBesideTheButton_TheSentValuesKept_AndEveryAttemptRecorded_KK8()
    {
        await SeedFirmAsync(open: true);
        await using WebApplicationFactory<Program> factory = OpenForPost();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        int threshold = AttemptLimits.Of(AttemptLimit.L4, AttemptAxis.Ip).Threshold;

        for (int i = 0; i < threshold; i++)
        {
            (HttpStatusCode allowed, _, _) = await PostAsync(client, Fields());
            Assert.Equal(HttpStatusCode.SeeOther, allowed);
        }

        Dictionary<string, string> last = new(Fields()) { ["Input.Message"] = "Bu mesajım korunmalı." };
        (HttpStatusCode status, string body, _) = await PostAsync(client, last);

        Assert.Equal(HttpStatusCode.TooManyRequests, status);
        Assert.Contains(NeutralMessages.LimitExceeded, body, StringComparison.Ordinal);
        Assert.Contains("Bu mesajım korunmalı.", body, StringComparison.Ordinal);
        Assert.Contains("value=\"Ayşe Yılmaz\"", body, StringComparison.Ordinal);
        Assert.DoesNotContain("ContactRequestLimitExceeded", body, StringComparison.Ordinal);

        // The message stands in the actions row of the form, beside the submit button.
        string actions = body[body.IndexOf("""<div class="contact-form__actions">""", StringComparison.Ordinal)..];
        Assert.True(actions.IndexOf("""<button type="submit" """, StringComparison.Ordinal) < actions.IndexOf(NeutralMessages.LimitExceeded, StringComparison.Ordinal));
        Assert.DoesNotContain("error-summary__list", body, StringComparison.Ordinal);

        // Every attempt is a row, the refused one included; only the allowed ones wrote a request.
        Assert.Equal(threshold + 1L, await CountAsync("attempt_records WHERE attempt_limit = 'L4' AND attempt_axis = 'Ip'"));
        Assert.Equal(threshold, await CountAsync("contact_requests"));
    }

    // --- KK9: the trap ---

    [Fact]
    public async Task ASendWithTheTrapFieldFilled_AnswersAsASuccess_LeavesNoRequestNoNoticeNoAttemptRow_AndDoesNotCountForL4_KK9()
    {
        await SeedFirmAsync(open: true);
        await using WebApplicationFactory<Program> factory = OpenForPost();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        int threshold = AttemptLimits.Of(AttemptLimit.L4, AttemptAxis.Ip).Threshold;

        (HttpStatusCode genuineStatus, _, string? genuineLocation) = await PostAsync(client, Fields());
        await ExecuteAsync("TRUNCATE contact_requests, dispatch_records, attempt_records CASCADE");

        for (int i = 0; i < threshold + 2; i++)
        {
            (HttpStatusCode droppedStatus, string droppedBody, string? droppedLocation) = await PostAsync(client, Fields(website: "https://spam.example"));
            Assert.Equal(genuineStatus, droppedStatus);
            Assert.Equal(genuineLocation, droppedLocation);
            Assert.Equal(string.Empty, droppedBody);
        }

        Assert.Equal(0L, await CountAsync("contact_requests"));
        Assert.Equal(0L, await CountAsync("dispatch_records"));
        Assert.Equal(0L, await CountAsync("attempt_records"));

        // The dropped sends did not use the visitor's allowance: a genuine send is still accepted.
        (HttpStatusCode after, _, _) = await PostAsync(client, Fields());
        Assert.Equal(HttpStatusCode.SeeOther, after);
        Assert.Equal(1L, await CountAsync("contact_requests"));
    }

    // --- KK10: the second send is a second request; F-2 to the firm with the trap empty ---

    [Fact]
    public async Task ASecondSend_OpensASecondRequest_CountsForL4_AndTheFirmGetsTheNoticeF2ForEach_KK10()
    {
        await SeedFirmAsync(open: true);
        await using WebApplicationFactory<Program> factory = OpenForPost();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode first, _, _) = await PostAsync(client, Fields());
        (HttpStatusCode second, _, _) = await PostAsync(client, Fields());

        Assert.Equal(HttpStatusCode.SeeOther, first);
        Assert.Equal(HttpStatusCode.SeeOther, second);
        Assert.Equal(2L, await CountAsync("contact_requests"));
        Assert.Equal(2L, await CountAsync("attempt_records WHERE attempt_limit = 'L4'"));
        await using AppDbContext db = NewContext();
        List<DispatchRecord> rows = await db.DispatchRecords.AsNoTracking().ToListAsync(Ct);
        Assert.Equal(2, rows.Count);
        Assert.All(rows, r =>
        {
            Assert.Equal(DispatchType.F2, r.DispatchType);
            Assert.Equal(FirmEmail, r.RecipientEmail);
            Assert.Equal(DispatchRelatedRecordType.ContactRequest, r.RelatedRecordType);
        });
        Assert.Equal(2, rows.Select(r => r.RelatedRecordId).Distinct().Count());
        Assert.Equal(0L, await CountAsync($"dispatch_records WHERE recipient_email = '{SenderEmail}'"));
    }
}
