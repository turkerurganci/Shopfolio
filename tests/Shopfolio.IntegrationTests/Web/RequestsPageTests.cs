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
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the requests screens' tests (T532): one database copied from the template; every test sets the contact requests up again.</summary>
[CollectionDefinition(RequestsPageTestsDefinition.Name)]
public sealed class RequestsPageTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "RequestsPages";
}

/// <summary>
/// The requests list (E-38) and the contact request's screen (E-39) with its two handlers on the opened application and a real database (T532 criteria 1-10, KK1-KK10; 07 UC-93, UC-94, UC-155, UC-156;
/// 04 9.10, 9.11). The contact request half; the defect claim half of the list is in the next worker's hands (hand-over note).
/// </summary>
[Collection(RequestsPageTestsDefinition.Name)]
public sealed partial class RequestsPageTests : IAsyncLifetime
{
    private const string ListPath = "/panel/talepler";

    private static readonly TimeSpan PatternTimeout = TimeSpan.FromSeconds(5);

    private readonly CollectionDatabaseFixture _database;

    private string _adminKey = string.Empty;

    public RequestsPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [GeneratedRegex(@"<tr\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RowStart();

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE contact_requests, dispatch_records CASCADE");
        _adminKey = (await TestSessions.AdminAsync(_database.Dsn)).Key;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1, KK5: the list, the filters, the empty state (criteria 1, 5) ----

    [Fact]
    public async Task TheList_ShowsTheContactRequestsNewestFirst_WithBothFilters_AndNoSearch_KK1_KK2()
    {
        ContactRequest older = await AddAsync("Ayşe", "ayse@example.com", ContactTopic.GeneralQuestion, "Eski", DateTimeOffset.UtcNow.AddDays(-5));
        ContactRequest newer = await AddAsync("Ali", "ali@example.com", ContactTopic.AboutOrder, "Yeni", DateTimeOffset.UtcNow.AddDays(-1));
        await SetStatusAsync(older.Id, "Closed", DateTimeOffset.UtcNow.AddDays(-4));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, ListPath);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.True(html.IndexOf(newer.Id.ToString("D"), StringComparison.Ordinal) < html.IndexOf(older.Id.ToString("D"), StringComparison.Ordinal), "The newest request stands first.");
        Assert.Contains("name=\"requestType\"", html, StringComparison.Ordinal);
        Assert.Contains("name=\"requestStatus\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"search\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"topic\"", html, StringComparison.Ordinal);
        Assert.Contains("İletişim talebi", html, StringComparison.Ordinal);
        Assert.Contains("Sipariş hakkında", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheStatusFilter_TakesSeveralValues_AndTheOpenRowsEqualTheHomeCounter_KK1_KK3()
    {
        ContactRequest open1 = await AddAsync("A", "a@example.com", ContactTopic.Other, "1", DateTimeOffset.UtcNow.AddDays(-3));
        ContactRequest open2 = await AddAsync("B", "b@example.com", ContactTopic.Other, "2", DateTimeOffset.UtcNow.AddDays(-2));
        ContactRequest closed = await AddAsync("C", "c@example.com", ContactTopic.Other, "3", DateTimeOffset.UtcNow.AddDays(-1));
        await SetStatusAsync(closed.Id, "Closed", DateTimeOffset.UtcNow.AddHours(-3));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string onlyOpen) = await GetAsync(client, ListPath + "?requestStatus=Open");
        (_, string both) = await GetAsync(client, ListPath + "?requestStatus=Open&requestStatus=Closed");

        Assert.Contains(open1.Id.ToString("D"), onlyOpen, StringComparison.Ordinal);
        Assert.Contains(open2.Id.ToString("D"), onlyOpen, StringComparison.Ordinal);
        Assert.DoesNotContain(closed.Id.ToString("D"), onlyOpen, StringComparison.Ordinal);
        Assert.Contains(closed.Id.ToString("D"), both, StringComparison.Ordinal);
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        int counter = await scope.ServiceProvider.GetRequiredService<ICorporateContentModule>().CountOpenContactRequestsAsync(Ct);
        Assert.Equal(2, counter);
        Assert.Equal(counter, RecordLinks(onlyOpen).Count);
    }

    [Fact]
    public async Task ARequestPastItsRetention_IsNotInTheList_AndAClosedOneStaysUntilTheCutFromItsLastClosing_KK3()
    {
        ContactRequest expired = await AddAsync("Eski", "eski@example.com", ContactTopic.Other, "Süresi dolmuş", DateTimeOffset.UtcNow.AddYears(-ProductConstants.ContactRequestRetentionYears - 1));
        ContactRequest keptOpen = await AddAsync("Yeni", "yeni@example.com", ContactTopic.Other, "Açık", DateTimeOffset.UtcNow.AddDays(-2));
        ContactRequest longAgoClosedRecently = await AddAsync("Kapalı", "kapali@example.com", ContactTopic.Other, "Yakında kapatıldı", DateTimeOffset.UtcNow.AddYears(-ProductConstants.ContactRequestRetentionYears - 1));
        await SetStatusAsync(longAgoClosedRecently.Id, "Closed", DateTimeOffset.UtcNow.AddDays(-1));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string html) = await GetAsync(client, ListPath);

        Assert.DoesNotContain(expired.Id.ToString("D"), html, StringComparison.Ordinal);
        Assert.Contains(keptOpen.Id.ToString("D"), html, StringComparison.Ordinal);
        Assert.Contains(longAgoClosedRecently.Id.ToString("D"), html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ARowOpensTheRequest_WithTheListAddressInReturnUrl_AndTheContactRowHasNoMark_KK4()
    {
        ContactRequest request = await AddAsync("Ayşe", "ayse@example.com", ContactTopic.GeneralQuestion, "Merhaba", DateTimeOffset.UtcNow.AddDays(-1));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string html) = await GetAsync(client, ListPath + "?requestStatus=Open");

        string expected = $"/panel/talepler/{request.Id:D}?returnUrl={Uri.EscapeDataString("/panel/talepler?requestStatus=Open")}";
        Assert.Contains(expected, html, StringComparison.Ordinal);
        Assert.DoesNotContain("ulaşmadı", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AnInvalidFilterValue_AppliesNoFilter_AndAnEmptyResultIsTheEmptyStateRow_KK5()
    {
        ContactRequest request = await AddAsync("Ayşe", "ayse@example.com", ContactTopic.Other, "Merhaba", DateTimeOffset.UtcNow.AddDays(-1));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode invalidStatus, string invalid) = await GetAsync(client, ListPath + "?requestType=Nope&requestStatus=Nope&page=abc");
        (HttpStatusCode resolvedStatus, string resolved) = await GetAsync(client, ListPath + "?requestStatus=Resolved");
        (HttpStatusCode hugeStatus, string huge) = await GetAsync(client, ListPath + "?page=99999999999");

        Assert.Equal(HttpStatusCode.OK, invalidStatus);
        Assert.Contains(request.Id.ToString("D"), invalid, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, resolvedStatus);
        Assert.Contains(RequestsModel.NoMatchText, resolved, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Id.ToString("D"), resolved, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, hugeStatus);
        Assert.Contains(request.Id.ToString("D"), huge, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AnEmptyList_SaysThereIsNoRequestYet_KK5()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, ListPath);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains(RequestsModel.NoRequestsText, html, StringComparison.Ordinal);
    }

    // ---- KK6, KK7: the detail screen (criteria 6, 7) ----

    [Fact]
    public async Task TheDetail_ShowsTopicStatusSenderMessageAndTheDates_KK6()
    {
        DateTimeOffset reached = new(2026, 11, 3, 10, 30, 0, TimeSpan.Zero);
        ContactRequest request = await AddAsync("Ayşe Yılmaz", "ayse@example.com", ContactTopic.PersonalDataRequest, "Birinci satır\nİkinci satır", reached, "5321234567");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, $"{ListPath}/{request.Id:D}");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("KVKK talebi", html, StringComparison.Ordinal);
        Assert.Contains("Ayşe Yılmaz", html, StringComparison.Ordinal);
        Assert.Contains("5321234567", html, StringComparison.Ordinal);
        Assert.Contains("Birinci satır\nİkinci satır", html.ReplaceLineEndings("\n"), StringComparison.Ordinal);
        Assert.Contains("request-detail__message", html, StringComparison.Ordinal);
        Assert.Contains(RequestDetailModel.ReceivedLabel, html, StringComparison.Ordinal);
        Assert.Contains(Shopfolio.Infrastructure.DisplayFormat.DateAndTime(reached), html, StringComparison.Ordinal);
        Assert.DoesNotContain(RequestDetailModel.LastClosedLabel, html, StringComparison.Ordinal);
        Assert.DoesNotContain("style=", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheDetail_OfAClosedRequest_ShowsTheLastClosing_AndOfAReopenedOneThatTheRetentionKeepsRunning_KK6_KK10()
    {
        ContactRequest request = await AddAsync("Ali", "ali@example.com", ContactTopic.Other, "Merhaba", DateTimeOffset.UtcNow.AddDays(-10));
        DateTimeOffset closedAt = new(2026, 11, 5, 9, 15, 0, TimeSpan.Zero);
        await SetStatusAsync(request.Id, "Closed", closedAt);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string closed) = await GetAsync(client, $"{ListPath}/{request.Id:D}");
        await SetStatusAsync(request.Id, "Open", closedAt);
        (_, string reopened) = await GetAsync(client, $"{ListPath}/{request.Id:D}");

        Assert.Contains(RequestDetailModel.LastClosedLabel, closed, StringComparison.Ordinal);
        Assert.Contains(Shopfolio.Infrastructure.DisplayFormat.DateAndTime(closedAt), closed, StringComparison.Ordinal);
        Assert.Contains(RequestDetailModel.RetentionRunsText, closed, StringComparison.Ordinal);
        Assert.Contains(RequestDetailModel.RetentionKeepsRunningText, reopened, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheDetail_HasOnePrimaryButtonByStatus_AndTwoSearchesWithTheEmailInTheQuery_AndTheBackLinkFollowsReturnUrl_KK7()
    {
        ContactRequest request = await AddAsync("Ali", "ali+x@example.com", ContactTopic.Other, "Merhaba", DateTimeOffset.UtcNow.AddDays(-1));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        string path = $"{ListPath}/{request.Id:D}";

        (_, string open) = await GetAsync(client, path + "?returnUrl=" + Uri.EscapeDataString("/panel/talepler?page=2"));
        await SetStatusAsync(request.Id, "Closed", DateTimeOffset.UtcNow);
        (_, string closed) = await GetAsync(client, path + "?returnUrl=" + Uri.EscapeDataString("https://evil.example/x"));

        Assert.Contains(">Talebi kapat<", open, StringComparison.Ordinal);
        Assert.DoesNotContain(">Yeniden aç<", open, StringComparison.Ordinal);
        Assert.Contains("handler=CloseContactRequest", open, StringComparison.Ordinal);
        Assert.Contains(">Yeniden aç<", closed, StringComparison.Ordinal);
        Assert.Contains("handler=ReopenContactRequest", closed, StringComparison.Ordinal);
        string encoded = Uri.EscapeDataString("ali+x@example.com");
        Assert.Contains($"/panel/siparisler?search={encoded}", open, StringComparison.Ordinal);
        Assert.Contains($"/panel/uyeler?search={encoded}", open, StringComparison.Ordinal);
        Assert.Contains("href=\"/panel/talepler?page=2\"", open, StringComparison.Ordinal);
        Assert.Contains("href=\"/panel/talepler\"", closed, StringComparison.Ordinal);
    }

    // ---- KK8: 404 on the three handlers (criterion 8) ----

    [Fact]
    public async Task AMissingOrDestroyedRequest_Answers404_OnTheGet_AndOnBothPosts_KK8()
    {
        ContactRequest expired = await AddAsync("Eski", "eski@example.com", ContactTopic.Other, "Süresi dolmuş", DateTimeOffset.UtcNow.AddYears(-ProductConstants.ContactRequestRetentionYears - 1));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        foreach (Guid id in new[] { Guid.NewGuid(), expired.Id })
        {
            string path = $"{ListPath}/{id:D}";
            using HttpResponseMessage get = await client.GetAsync(new Uri(path, UriKind.Relative), Ct);
            using HttpResponseMessage close = await client.PostAsync(path + "?handler=CloseContactRequest", new FormUrlEncodedContent([]), Ct);
            using HttpResponseMessage reopen = await client.PostAsync(path + "?handler=ReopenContactRequest", new FormUrlEncodedContent([]), Ct);

            Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, close.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, reopen.StatusCode);
        }

        Assert.Equal("Open", await StatusOfAsync(expired.Id));
    }

    // ---- KK9, KK10: close and reopen (criteria 9, 10) ----

    [Fact]
    public async Task Close_SetsClosedAndTheClosingMoment_SendsNoNotice_AndASecondPostIs303WithoutANotice_KK9()
    {
        ContactRequest request = await AddAsync("Ali", "ali@example.com", ContactTopic.Other, "Merhaba", DateTimeOffset.UtcNow.AddDays(-1));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        string path = $"{ListPath}/{request.Id:D}";

        using HttpResponseMessage first = await client.PostAsync(path + "?handler=CloseContactRequest", new FormUrlEncodedContent([]), Ct);
        DateTimeOffset? closedAt = await LastClosedAtAsync(request.Id);
        using HttpResponseMessage second = await client.PostAsync(path + "?handler=CloseContactRequest", new FormUrlEncodedContent([]), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.StartsWith(path, first.Headers.Location?.OriginalString, StringComparison.Ordinal);
        Assert.Contains("notice=ContactRequestClosed", first.Headers.Location?.OriginalString, StringComparison.Ordinal);
        Assert.Equal("Closed", await StatusOfAsync(request.Id));
        Assert.NotNull(closedAt);
        Assert.Equal(HttpStatusCode.SeeOther, second.StatusCode);
        Assert.Equal(path, second.Headers.Location?.OriginalString);
        Assert.Equal(closedAt, await LastClosedAtAsync(request.Id));
        Assert.Equal(0, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records"));
        (_, string html) = await GetAsync(client, first.Headers.Location!.OriginalString);
        Assert.Contains("Talep kapatıldı", html, StringComparison.Ordinal);
        Assert.Contains(">Yeniden aç<", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Reopen_SetsOpenAndKeepsTheLastClosing_SoTheRetentionDoesNotStop_AndASecondPostIs303WithoutANotice_KK10()
    {
        ContactRequest request = await AddAsync("Ali", "ali@example.com", ContactTopic.Other, "Merhaba", DateTimeOffset.UtcNow.AddDays(-30));
        DateTimeOffset closedAt = DateTimeOffset.UtcNow.AddDays(-20);
        closedAt = closedAt.AddTicks(-(closedAt.Ticks % TimeSpan.TicksPerSecond));
        await SetStatusAsync(request.Id, "Closed", closedAt);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        string path = $"{ListPath}/{request.Id:D}";

        using HttpResponseMessage first = await client.PostAsync(path + "?handler=ReopenContactRequest", new FormUrlEncodedContent([]), Ct);
        DateTimeOffset? afterReopen = await LastClosedAtAsync(request.Id);
        using HttpResponseMessage second = await client.PostAsync(path + "?handler=ReopenContactRequest", new FormUrlEncodedContent([]), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.Contains("notice=ContactRequestReopened", first.Headers.Location?.OriginalString, StringComparison.Ordinal);
        Assert.Equal("Open", await StatusOfAsync(request.Id));
        Assert.Equal(closedAt.ToUniversalTime().ToString("O"), afterReopen?.ToUniversalTime().ToString("O"));
        Assert.Equal(HttpStatusCode.SeeOther, second.StatusCode);
        Assert.Equal(path, second.Headers.Location?.OriginalString);
        Assert.Equal(0, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records"));
    }

    [Fact]
    public async Task TheScreens_RefuseAGuest_KK1()
    {
        ContactRequest request = await AddAsync("Ali", "ali@example.com", ContactTopic.Other, "Merhaba", DateTimeOffset.UtcNow.AddDays(-1));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient guest = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage list = await guest.GetAsync(new Uri(ListPath, UriKind.Relative), Ct);
        using HttpResponseMessage detail = await guest.GetAsync(new Uri($"{ListPath}/{request.Id:D}", UriKind.Relative), Ct);
        using HttpResponseMessage close = await guest.PostAsync($"{ListPath}/{request.Id:D}?handler=CloseContactRequest", new FormUrlEncodedContent([]), Ct);

        Assert.NotEqual(HttpStatusCode.OK, list.StatusCode);
        Assert.NotEqual(HttpStatusCode.OK, detail.StatusCode);
        Assert.Equal("Open", await StatusOfAsync(request.Id));
    }

    // ---- helpers ----

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

    private static async Task<(HttpStatusCode Status, string Html)> GetAsync(HttpClient client, string path)
    {
        using HttpResponseMessage response = await client.GetAsync(new Uri(path, UriKind.Relative), Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static List<string> RecordLinks(string html) =>
        [.. Regex.Matches(html, @"href=""(/panel/talepler/[0-9a-f-]{36}[^""]*)""", RegexOptions.CultureInvariant, PatternTimeout).Select(m => m.Groups[1].Value)];

    private async Task<ContactRequest> AddAsync(string name, string email, ContactTopic topic, string message, DateTimeOffset at, string? phone = null)
    {
        ContactRequest request = ContactRequest.Create(name, email, topic, message, at, phone);
        await using AppDbContext db = NewContext();
        db.ContactRequests.Add(request);
        await db.SaveChangesAsync(Ct);
        return request;
    }

    private async Task SetStatusAsync(Guid id, string status, DateTimeOffset closedAt)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new("UPDATE contact_requests SET contact_request_status = @status, last_closed_at = @closed WHERE id = @id", connection);
        command.Parameters.AddWithValue("status", status);
        command.Parameters.AddWithValue("closed", closedAt);
        command.Parameters.AddWithValue("id", id);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private Task<string> StatusOfAsync(Guid id) => ScalarAsync<string>($"SELECT contact_request_status FROM contact_requests WHERE id = '{id}'");

    private async Task<DateTimeOffset?> LastClosedAtAsync(Guid id)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new($"SELECT last_closed_at FROM contact_requests WHERE id = '{id}'", connection);
        object? value = await command.ExecuteScalarAsync(Ct);
        return value is DateTime or DateTimeOffset ? new DateTimeOffset(Convert.ToDateTime(value, System.Globalization.CultureInfo.InvariantCulture), TimeSpan.Zero) : null;
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
