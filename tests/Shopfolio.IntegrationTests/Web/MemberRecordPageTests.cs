using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the member record screen's tests (T533): one database copied from the template; every test sets its rows up again.</summary>
[CollectionDefinition(MemberRecordPageTestsDefinition.Name)]
public sealed class MemberRecordPageTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "MemberRecordPages";
}

/// <summary>
/// The member record screen (E-40) on the opened application and a real database (T533 criteria 1-9, KK1-KK9; 07 UC-95, UC-157; 04 9.12): the search by e-mail, the three results, the bound orders'
/// order and pages, the wrong e-mail, the open revert link, the confirmation, the deletion and its aftermath, the refusals and the repeated submit.
/// </summary>
[Collection(MemberRecordPageTestsDefinition.Name)]
public sealed partial class MemberRecordPageTests : IAsyncLifetime
{
    private const string PanelPath = "/panel/uyeler";

    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    private static readonly TimeSpan PatternTimeout = TimeSpan.FromSeconds(5);

    private readonly CollectionDatabaseFixture _database;

    private string _adminKey = string.Empty;

    private Guid _provinceId;

    public MemberRecordPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [GeneratedRegex(@"<tr class=""panel-table__row"">", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex OrderRow();

    [GeneratedRegex(@"href=""(/panel/siparisler/[^""]+)""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex OrderLink();

    [GeneratedRegex(@"<input type=""hidden"" name=""([^""]+)"" value=""([^""]*)""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex HiddenField();

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE customers, admins, orders, audit_logs, dispatch_records, disposal_records, link_records, carts, address_books, sessions, provinces, companies, company_settings, legal_text_versions CASCADE");
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
            await db.SaveChangesAsync(Ct);
        }

        _provinceId = province.Id;
        _adminKey = (await TestSessions.AdminAsync(_database.Dsn)).Key;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the field alone; exact match without case; administrators are not searched ----

    [Fact]
    public async Task WithoutASearch_OnlyTheSearchFieldShows_KK1()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, PanelPath);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("name=\"search\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("member-record", html, StringComparison.Ordinal);
        Assert.DoesNotContain(MembersModel.NotFoundText, html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheSearch_FindsTheCustomerAccountByTheExactAddress_WithoutCase_AndNeverAnAdministrator_KK1()
    {
        await SeedCustomerAsync("ali@example.com", "Ali Veli", verified: true);
        string adminEmail = await ScalarAsync<string>("SELECT email FROM admins LIMIT 1");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string exact) = await GetAsync(client, $"{PanelPath}?search=ali@example.com");
        (_, string upper) = await GetAsync(client, $"{PanelPath}?search=ALI@Example.COM");
        (_, string partial) = await GetAsync(client, $"{PanelPath}?search=ali@example");
        (_, string admin) = await GetAsync(client, $"{PanelPath}?search={Uri.EscapeDataString(adminEmail)}");

        Assert.Contains("Ali Veli", exact, StringComparison.Ordinal);
        Assert.Contains("Ali Veli", upper, StringComparison.Ordinal);
        Assert.DoesNotContain("Ali Veli", partial, StringComparison.Ordinal);
        Assert.DoesNotContain("Ali Veli", admin, StringComparison.Ordinal);
        Assert.Contains(MembersModel.NotFoundText, admin, StringComparison.Ordinal);
    }

    // ---- KK2: the three results ----

    [Fact]
    public async Task TheResult_IsNoAccount_AnUnverifiedRegistration_OrAVerifiedAccount_KK2()
    {
        await SeedCustomerAsync("bekleyen@example.com", "Bekleyen", verified: false);
        await SeedCustomerAsync("ali@example.com", "Ali Veli", verified: true);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode noneStatus, string none) = await GetAsync(client, $"{PanelPath}?search=yok@example.com");
        (_, string pending) = await GetAsync(client, $"{PanelPath}?search=bekleyen@example.com");
        (_, string account) = await GetAsync(client, $"{PanelPath}?search=ali@example.com");

        Assert.Equal(HttpStatusCode.OK, noneStatus);
        Assert.Contains(MembersModel.NotFoundText, none, StringComparison.Ordinal);
        Assert.Contains(MembersModel.UnverifiedText, pending, StringComparison.Ordinal);
        Assert.Contains("bekleyen@example.com", pending, StringComparison.Ordinal);
        Assert.DoesNotContain("member-delete-button", pending, StringComparison.Ordinal);
        Assert.DoesNotContain(MembersModel.NotFoundText, pending, StringComparison.Ordinal);
        Assert.DoesNotContain(MembersModel.UnverifiedText, account, StringComparison.Ordinal);
        Assert.Contains("Ali Veli", account, StringComparison.Ordinal);
        Assert.Contains("id=\"member-delete-button\"", account, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheAccount_ShowsTheLoginPaths_AndTheAddressBook_OrTheOneLineWhenThereIsNone_KK2_KK3()
    {
        Guid withEverything = await SeedCustomerAsync("ali@example.com", "Ali Veli", verified: true, withPassword: true, googleLinked: true);
        await SeedCustomerAsync("google@example.com", "Google Kisi", verified: true, withPassword: false, googleLinked: false);
        await SeedAddressAsync(withEverything, "Ev", "Ali Veli", "Çankaya", "Atatürk Bulvarı 1", "5321234567");
        await SeedAddressAsync(withEverything, "İş", "Ayşe Kaya", "Kızılay", "Kızılay Sokak 2", null);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string full) = await GetAsync(client, $"{PanelPath}?search=ali@example.com");
        (_, string bare) = await GetAsync(client, $"{PanelPath}?search=google@example.com");

        Assert.Contains(MembersModel.EmailPathPresentText, full, StringComparison.Ordinal);
        Assert.Contains(MembersModel.GoogleLinkedText, full, StringComparison.Ordinal);
        Assert.Contains("Ayşe Kaya", full, StringComparison.Ordinal);
        Assert.Contains("Atatürk Bulvarı 1", full, StringComparison.Ordinal);
        Assert.Contains("Ankara", full, StringComparison.Ordinal);
        Assert.Contains(MembersModel.EmailPathAbsentText, bare, StringComparison.Ordinal);
        Assert.Contains(MembersModel.GoogleNotLinkedText, bare, StringComparison.Ordinal);
        Assert.Contains(MembersModel.NoAddressText, bare, StringComparison.Ordinal);
        Assert.Contains(MembersModel.NoOrderText, bare, StringComparison.Ordinal);
    }

    // ---- KK3: the bound orders, newest first, 25 to a page, the row opens the order ----

    [Fact]
    public async Task TheBoundOrders_AreNewestFirst_ThePageBoundaryHoldsTies_AndTheRowLinkReturnsToThePage_KK3()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", "Ali Veli", verified: true);
        Guid other = await SeedCustomerAsync("veli@example.com", "Veli", verified: true);
        List<Guid> orders = [];
        for (int i = 0; i < 30; i++)
        {
            orders.Add((await TestSessions.OrderAsync(_database.Dsn, id)).OrderId);
        }

        string foreignNumber = (await TestSessions.OrderAsync(_database.Dsn, other)).Number;
        await TestSessions.OrderAsync(_database.Dsn, null);

        // Three newest orders, then 27 orders created at one and the same moment: the page boundary (25 | 5) falls inside the tie.
        DateTimeOffset tied = Now.AddDays(-5);
        await ExecuteAsync($"UPDATE orders SET created_at = '{tied:O}' WHERE customer_id = '{id}'");
        for (int i = 0; i < 3; i++)
        {
            await ExecuteAsync($"UPDATE orders SET created_at = '{Now.AddDays(-1).AddMinutes(i):O}' WHERE id = '{orders[i]}'");
        }

        List<string> expected = await ColumnAsync($"SELECT order_number FROM orders WHERE customer_id = '{id}' ORDER BY created_at DESC, id DESC");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string first) = await GetAsync(client, $"{PanelPath}?search=ali@example.com");
        (_, string second) = await GetAsync(client, $"{PanelPath}?search=ali@example.com&page=2");
        (_, string past) = await GetAsync(client, $"{PanelPath}?search=ali@example.com&page=99");

        Assert.Equal(25, OrderRow().Count(first));
        Assert.Equal(5, OrderRow().Count(second));
        Assert.Equal(5, OrderRow().Count(past));
        Assert.Equal(30, expected.Count);
        List<string> shown = [.. NumbersOf(first), .. NumbersOf(second)];
        Assert.Equal(expected, shown);
        Assert.Equal(expected.Count, shown.Distinct(StringComparer.Ordinal).Count());
        Assert.DoesNotContain(foreignNumber, first + second, StringComparison.Ordinal);
        Assert.Contains("rel=\"next\"", first, StringComparison.Ordinal);
        Assert.Contains("rel=\"prev\"", second, StringComparison.Ordinal);

        // The newest order is first; its link carries the address of the page it was on.
        string firstLink = WebUtility.HtmlDecode(OrderLink().Match(second).Groups[1].Value);
        Assert.StartsWith($"{PanelFramePathsOrders}/", firstLink, StringComparison.Ordinal);
        Assert.Contains("returnUrl=", firstLink, StringComparison.Ordinal);
        Assert.Contains(Uri.EscapeDataString("&page=2"), firstLink, StringComparison.Ordinal);
        Assert.Contains("badge-pair", first, StringComparison.Ordinal);
    }

    private const string PanelFramePathsOrders = "/panel/siparisler";

    // ---- KK4: a wrong e-mail is a field message; nothing is searched; the trail is untouched ----

    [Fact]
    public async Task AWrongEmail_IsAFieldMessage_With200_NoSearchIsMade_AndTheTrailIsNotWritten_KK4()
    {
        await SeedCustomerAsync("ali@example.com", "Ali Veli", verified: true);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode wrong, string html) = await GetAsync(client, $"{PanelPath}?search=ali");
        await GetAsync(client, $"{PanelPath}?search=ali@example.com");
        await GetAsync(client, $"{PanelPath}?search=yok@example.com");

        Assert.Equal(HttpStatusCode.OK, wrong);
        Assert.Contains("id=\"list-search-field-message\"", html, StringComparison.Ordinal);
        Assert.Contains("aria-describedby=\"list-search-field-message\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("member-record", html, StringComparison.Ordinal);
        Assert.DoesNotContain(MembersModel.NotFoundText, html, StringComparison.Ordinal);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
    }

    // ---- KK5: an open revert link closes the button; the reason and the end date stay in its place ----

    [Fact]
    public async Task WhileARevertLinkIsOpen_TheDeleteButtonIsAbsent_AndTheReasonAndTheEndDateStand_KK5()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", "Ali Veli", verified: true);
        await using WebApplicationFactory<Program> factory = Open();
        DateTimeOffset ends = await SeedOpenRevertLinkAsync(factory, id);
        using HttpClient client = ClientOf(factory);

        (_, string html) = await GetAsync(client, $"{PanelPath}?search=ali@example.com");

        Assert.DoesNotContain("member-delete-button", html, StringComparison.Ordinal);
        Assert.Contains($"Bu hesabın önceki e-posta adresine gönderilen geri alma bağlantısı {DisplayFormat.Date(ends)} tarihine kadar açık. Hesap bu tarihe kadar silinemez.", html, StringComparison.Ordinal);
    }

    // ---- KK6: the confirmation state; the result code returns to the empty search field with a lasting message ----

    [Fact]
    public async Task WithoutTheConfirmationMark_TheAnswerIsTheConfirmationState_AndNothingIsDeleted_KK6()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", "Ali Veli", verified: true);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.PostAsync(DeletePath, Form(id, "ali@example.com", confirmed: false), Ct);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("id=\"confirm-heading\"", html, StringComparison.Ordinal);
        Assert.Contains("Hesap ve hesaptaki ad, giriş bilgileri, adres defteri ve sepet hemen silinecek; siparişler sürer.", html, StringComparison.Ordinal);
        Assert.Contains("Bu işlem geri alınamaz.", html, StringComparison.Ordinal);
        Assert.Contains("Vazgeç", html, StringComparison.Ordinal);
        Dictionary<string, string> hidden = HiddenFields(Region(html, "<dialog", "</dialog>"));
        Assert.Equal("true", hidden["Confirmed"]);
        Assert.Equal(id.ToString(), hidden["CustomerId"]);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM customers WHERE email_verified_at IS NOT NULL"));
    }

    [Fact]
    public async Task TheConfirmedDeletion_Answers303ToTheSearch_AndTheScreenShowsTheLastingMessage_WithAnEmptyField_KK6()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", "Ali Veli", verified: true);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.PostAsync(DeletePath, Form(id, "ali@example.com", confirmed: true), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        string location = response.Headers.Location!.OriginalString;
        Assert.StartsWith(PanelPath, location, StringComparison.Ordinal);
        Assert.Contains("notice=", location, StringComparison.Ordinal);
        (HttpStatusCode status, string html) = await GetAsync(client, location);
        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains(MembersModel.DeletedText, html, StringComparison.Ordinal);
        Assert.Contains("message--in-place", Region(html, "id=\"member-deleted\"", "</div>") + "message--in-place", StringComparison.Ordinal);
        Assert.DoesNotContain("message--short-lived", html, StringComparison.Ordinal);
        Assert.Contains("name=\"search\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("value=\"ali@example.com\"", html, StringComparison.Ordinal);
    }

    // ---- KK7: 422 while a revert link is open; 404 for an account that is not there ----

    [Fact]
    public async Task ADeletionWhileARevertLinkIsOpen_Answers422_AndDeletesNothing_KK7()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", "Ali Veli", verified: true);
        await using WebApplicationFactory<Program> factory = Open();
        await SeedOpenRevertLinkAsync(factory, id);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.PostAsync(DeletePath, Form(id, "ali@example.com", confirmed: true), Ct);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains("Hesap bu tarihe kadar silinemez.", html, StringComparison.Ordinal);
        Assert.DoesNotContain("member-delete-button", html, StringComparison.Ordinal);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM customers"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM disposal_records"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records WHERE dispatch_type = 'AccountDeletedNotice'"));
    }

    [Fact]
    public async Task ADeletionOfAnAccountThatIsNotThere_Answers404_InBothSteps_AndAnUnverifiedRegistrationIsNotDeleted_KK7()
    {
        Guid pending = await SeedCustomerAsync("bekleyen@example.com", "Bekleyen", verified: false);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage confirmStep = await client.PostAsync(DeletePath, Form(Guid.NewGuid(), "yok@example.com", confirmed: false), Ct);
        using HttpResponseMessage confirmed = await client.PostAsync(DeletePath, Form(Guid.NewGuid(), "yok@example.com", confirmed: true), Ct);
        using HttpResponseMessage unverified = await client.PostAsync(DeletePath, Form(pending, "bekleyen@example.com", confirmed: true), Ct);

        Assert.Equal(HttpStatusCode.NotFound, confirmStep.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, confirmed.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, unverified.StatusCode);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM customers"));
    }

    // ---- KK8: what goes, what stays, the disposal record, the e-mail, the trail row ----

    [Fact]
    public async Task TheDeletion_RemovesTheAccountAndWhatHangsOnIt_KeepsTheOrders_WritesTheRecords_AndTheTrailNamesNotTheAccount_KK8()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", "Ali Veli", verified: true);
        Guid other = await SeedCustomerAsync("veli@example.com", "Veli", verified: true);
        await SeedAddressAsync(id, "Ev", "Ali Veli", "Çankaya", "Atatürk Bulvarı 1", "5321234567");
        await using (AppDbContext carts = NewContext())
        {
            carts.Carts.Add(Cart.ForCustomer(id, Now));
            await carts.SaveChangesAsync(Ct);
        }

        (Guid orderId, _, _) = await TestSessions.OrderAsync(_database.Dsn, id);
        await TestSessions.CustomerWithSessionAsync(_database.Dsn);
        Guid adminId = await ScalarAsync<Guid>("SELECT id FROM admins LIMIT 1");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.PostAsync(DeletePath, Form(id, "ali@example.com", confirmed: true), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM customers WHERE id = '{id}'"));
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM customers WHERE id = '{other}'"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM address_books"));
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM carts WHERE customer_id = '{id}'"));
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM orders WHERE id = '{orderId}' AND customer_id IS NULL AND is_account_deleted"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM disposal_records WHERE disposal_record_type = 'Customer'"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records WHERE dispatch_type = 'AccountDeletedNotice' AND recipient_email = 'ali@example.com'"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
        Assert.Equal("DeleteCustomerAccount", await ScalarAsync<string>("SELECT operation FROM audit_logs"));
        Assert.Equal(adminId, await ScalarAsync<Guid>("SELECT admin_id FROM audit_logs"));
        string row = await ScalarAsync<string>("SELECT to_jsonb(a)::text FROM audit_logs a");
        Assert.DoesNotContain("ali@example.com", row, StringComparison.Ordinal);
        Assert.DoesNotContain("Ali Veli", row, StringComparison.Ordinal);

        // Searching again: no account (the orders stay in the order list, bound to nothing).
        (_, string again) = await GetAsync(client, $"{PanelPath}?search=ali@example.com");
        Assert.Contains(MembersModel.NotFoundText, again, StringComparison.Ordinal);
    }

    // ---- KK9: the second submit is 404; an uncertain outcome returns to the search ----

    [Fact]
    public async Task TheSecondSubmit_Answers404_AndSendsOneEmailOnly_KK9()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", "Ali Veli", verified: true);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage first = await client.PostAsync(DeletePath, Form(id, "ali@example.com", confirmed: true), Ct);
        using HttpResponseMessage second = await client.PostAsync(DeletePath, Form(id, "ali@example.com", confirmed: true), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records WHERE dispatch_type = 'AccountDeletedNotice'"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
    }

    [Fact]
    public async Task AnUncertainOutcome_Answers303ToTheSearchOfTheEmail_AndSaysNotDone_Never_KK9()
    {
        Guid id = await SeedCustomerAsync("ali@example.com", "Ali Veli", verified: true);
        await using WebApplicationFactory<Program> factory = Open().WithWebHostBuilder(b => b.ConfigureServices(s => s.AddScoped<ITransactionRunner, FailingTransactionRunner>()));
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.PostAsync(DeletePath, Form(id, "Ali@Example.com", confirmed: true), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal($"{PanelPath}?search=Ali%40Example.com", response.Headers.Location!.OriginalString);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM customers"));
    }

    // ---- helpers ----

    private static string DeletePath => $"{PanelPath}?handler={MembersModel.DeleteHandler}";

    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn, withJobRunner: false)
            .WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(ForgeryTokenSampleModel).Assembly)));

    private HttpClient ClientOf(WebApplicationFactory<Program> factory)
    {
        HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        client.DefaultRequestHeaders.Add(HeaderNames.Cookie, $"{CookieNames.AdminSession}={_adminKey}");
        return client;
    }

    private static FormUrlEncodedContent Form(Guid customerId, string searchEmail, bool confirmed) =>
        new(
        [
            new KeyValuePair<string, string>("CustomerId", customerId.ToString()),
            new KeyValuePair<string, string>("SearchEmail", searchEmail),
            new KeyValuePair<string, string>("Confirmed", confirmed ? "true" : "false"),
        ]);

    private static async Task<(HttpStatusCode Status, string Html)> GetAsync(HttpClient client, string path)
    {
        using HttpResponseMessage response = await client.GetAsync(new Uri(path, UriKind.Relative), Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static string Region(string html, string startMarker, string endMarker)
    {
        int start = html.IndexOf(startMarker, StringComparison.Ordinal);
        Assert.True(start >= 0, $"No '{startMarker}' in the response.");
        int end = html.IndexOf(endMarker, start, StringComparison.Ordinal);
        Assert.True(end > start, $"No '{endMarker}' after '{startMarker}'.");
        return html[start..end];
    }

    private static Dictionary<string, string> HiddenFields(string html) =>
        HiddenField().Matches(html).ToDictionary(m => m.Groups[1].Value, m => WebUtility.HtmlDecode(m.Groups[2].Value), StringComparer.Ordinal);

    private static List<string> NumbersOf(string html) =>
        [.. Regex.Matches(html, @"<a href=""/panel/siparisler/[^""]+"">([^<]+)</a>", RegexOptions.CultureInvariant, PatternTimeout).Select(m => m.Groups[1].Value)];

    private async Task<Guid> SeedCustomerAsync(string email, string name, bool verified, bool withPassword = true, bool googleLinked = false)
    {
        Customer customer = Customer.Create(name, email, withPassword ? "digest-placeholder" : null, verified ? Now : null, googleLinked ? Now : null, Now);
        await using AppDbContext db = NewContext();
        db.Customers.Add(customer);
        await db.SaveChangesAsync(Ct);
        return customer.Id;
    }

    private async Task SeedAddressAsync(Guid customerId, string name, string recipient, string district, string line, string? phone)
    {
        await using AppDbContext db = NewContext();
        db.AddressBooks.Add(AddressBook.Create(customerId, name, recipient, _provinceId, district, line, phone, Now));
        await db.SaveChangesAsync(Ct);
    }

    private async Task<DateTimeOffset> SeedOpenRevertLinkAsync(WebApplicationFactory<Program> factory, Guid customerId)
    {
        DateTimeOffset ends = DateTimeOffset.UtcNow.AddDays(ProductConstants.EmailChangeRevertLinkDays);
        LinkKeys keys = factory.Services.GetRequiredService<LinkKeys>();
        await using AppDbContext db = NewContext();
        db.LinkRecords.Add(LinkRecord.ForCustomer(keys, LinkType.EmailChangeRevert, customerId, ends, DateTimeOffset.UtcNow));
        await db.SaveChangesAsync(Ct);
        return ends;
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
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

    private async Task<List<string>> ColumnAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(Ct);
        List<string> values = [];
        while (await reader.ReadAsync(Ct))
        {
            values.Add(reader.GetString(0));
        }

        return values;
    }

    /// <summary>A runner whose transaction always fails: the deletion's outcome is unknown to the browser (09 8.1 item 4).</summary>
    private sealed class FailingTransactionRunner : ITransactionRunner
    {
        public Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken ct)
            where T : IOperationResult =>
            throw new InvalidOperationException("The test transaction fails.");

        public Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> work, IReadOnlyCollection<string> retryOnUniqueIndexes, CancellationToken ct)
            where T : IOperationResult =>
            throw new InvalidOperationException("The test transaction fails.");
    }
}
