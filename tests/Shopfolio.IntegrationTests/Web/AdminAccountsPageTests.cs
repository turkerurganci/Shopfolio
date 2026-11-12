using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the administrator accounts screen's tests (T522): one database copied from the template; every test sets the administrators up again.</summary>
[CollectionDefinition(AdminAccountsPageTestsDefinition.Name)]
public sealed class AdminAccountsPageTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "AdminAccountsPages";
}

/// <summary>
/// The administrator accounts screen (E-49) and its three handlers on the opened application and a real database (T522 criteria 1-8, KK1-KK8; 07 UC-103, UC-175, UC-176, UC-177; 04 9.20).
/// </summary>
[Collection(AdminAccountsPageTestsDefinition.Name)]
public sealed class AdminAccountsPageTests : IAsyncLifetime
{
    private const string Path = "/panel/yonetici-hesaplari";

    private readonly CollectionDatabaseFixture _database;

    private string _key = string.Empty;
    private Guid _adminId;

    public AdminAccountsPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE admins, admin_invitations, link_records, audit_logs, dispatch_records, sessions, disposal_records CASCADE");
        (_key, _adminId) = await TestSessions.AdminAsync(_database.Dsn);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task TheList_IsInTurkishAlphabeticalOrder_SaysSizOnTheOwnRow_AndHasNoRemovalOnTheOwnRow_KK1()
    {
        await AddAdminAsync("Zeynep");
        await AddAdminAsync("Çağlar");
        await AddAdminAsync("Ahmet");
        await AddAdminAsync("Ayşe");
        using HttpClient client = ClientOf(Open());

        (HttpStatusCode status, string html) = await GetAsync(client, Path);

        Assert.Equal(HttpStatusCode.OK, status);
        int[] positions = [.. new[] { "Ahmet", "Ayşe", "Çağlar", "Zeynep" }.Select(name => html.IndexOf(name, StringComparison.Ordinal))];
        Assert.All(positions, p => Assert.True(p > 0));
        Assert.Equal(positions.Order(), positions);
        Assert.Contains("(siz)", html, StringComparison.Ordinal);
        Assert.Equal(4, Count(html, "handler=RemoveAdmin"));
    }

    [Fact]
    public async Task WithOneAdministrator_NoRowHasARemovalButton_AndTheInvitationsListIsNotDrawn_KK1_KK2()
    {
        using HttpClient client = ClientOf(Open());

        (_, string html) = await GetAsync(client, Path);

        Assert.DoesNotContain("handler=RemoveAdmin", html, StringComparison.Ordinal);
        Assert.DoesNotContain("admin-invitation-list", html, StringComparison.Ordinal);
        Assert.Contains("Yönetici davet et", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SendingAnInvitation_WritesTheRowTheLinkTheEmailTheTrailAndTheNotice_AndTheRowShowsInTheList_KK2_KK3()
    {
        await AddAdminAsync("Diğer");
        using HttpClient client = ClientOf(Open());

        using HttpResponseMessage sent = await PostAsync(client, "SendAdminInvitation", ("Input.Email", "yeni@example.com"));
        (_, string html) = await GetAsync(client, Path);

        Assert.Equal(HttpStatusCode.SeeOther, sent.StatusCode);
        Assert.Contains("notice=InvitationSent", sent.Headers.Location?.OriginalString, StringComparison.Ordinal);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM admin_invitations WHERE email = 'yeni@example.com'"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM link_records WHERE admin_invitation_id IS NOT NULL"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs WHERE operation = 'SendAdminInvitation'"));
        Assert.True(await ScalarAsync<long>("SELECT count(*) FROM dispatch_records") >= 3, "The invitation e-mail and the F-6 notice of both administrators are queued.");
        Assert.Contains("yeni@example.com", html, StringComparison.Ordinal);
        Assert.Contains("Test yonetici", html, StringComparison.Ordinal);
        Assert.Contains("Son gün:", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Yeniden gönder", html, StringComparison.Ordinal);
        Assert.Contains("Daveti geri çek", html, StringComparison.Ordinal);
        Assert.Contains("Yeni davet gönder", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ANewInvitationToTheSameAddress_EndsTheEarlierOne_AndOnlyTheNewestIsListed_KK4()
    {
        using HttpClient client = ClientOf(Open());

        using HttpResponseMessage first = await PostAsync(client, "SendAdminInvitation", ("Input.Email", "ayni@example.com"));
        using HttpResponseMessage second = await PostAsync(client, "SendAdminInvitation", ("Input.Email", "ayni@example.com"));
        (_, string html) = await GetAsync(client, Path);

        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.Equal(HttpStatusCode.SeeOther, second.StatusCode);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM admin_invitations WHERE invalidated_at IS NULL AND withdrawn_at IS NULL AND accepted_at IS NULL"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM admin_invitations WHERE invalidated_at IS NOT NULL"));
        Assert.Equal(1, Count(html, "handler=RevokeAdminInvitation"));
    }

    [Fact]
    public async Task AnAdministratorsAddress_AndABadAddress_Are422WithTheFieldMessage_AndWriteNothing_KK4()
    {
        (string Name, string Email) other = (await AddAdminAsync("Diğer"), "diger@example.com");
        using HttpClient client = ClientOf(Open());

        using HttpResponseMessage taken = await PostAsync(client, "SendAdminInvitation", ("Input.Email", other.Email));
        string takenHtml = WebUtility.HtmlDecode(await taken.Content.ReadAsStringAsync(Ct));
        using HttpResponseMessage bad = await PostAsync(client, "SendAdminInvitation", ("Input.Email", "bu-bir-adres-degil"));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, taken.StatusCode);
        Assert.Contains("Bu adres zaten bir yönetici hesabına ait.", takenHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, bad.StatusCode);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM admin_invitations"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
    }

    [Fact]
    public async Task AMarkedAddressThatHasAValidInvitation_TellsTheNewInvitationEndsIt_OnTheRedisplay_KK4()
    {
        using HttpClient client = ClientOf(Open());
        using HttpResponseMessage sent = await PostAsync(client, "SendAdminInvitation", ("Input.Email", "var@example.com"));
        await AddAdminAsync("Diğer");

        // The same address typed with the wrong form of the second try is not the case; the redisplay of a refusal carries the hint only for the address that has a valid invitation.
        using HttpResponseMessage refused = await PostAsync(client, "SendAdminInvitation", ("Input.Email", "diger@example.com"));
        string html = WebUtility.HtmlDecode(await refused.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.SeeOther, sent.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
        Assert.DoesNotContain("yeni davet onu geçersiz kılar", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Revoking_EndsTheLink_WritesTheTrail_RedirectsWithoutACode_AndASecondRevokeWritesNothing_Or404ForAMissingRow_KK5()
    {
        using HttpClient client = ClientOf(Open());
        await PostAsync(client, "SendAdminInvitation", ("Input.Email", "geri@example.com")).ContinueWith(t => t.Result.Dispose(), Ct);
        Guid id = await ScalarAsync<Guid>("SELECT id FROM admin_invitations");

        using HttpResponseMessage first = await PostAsync(client, "RevokeAdminInvitation", ("Input.AdminInvitationId", id.ToString("D")));
        long trailAfterFirst = await ScalarAsync<long>("SELECT count(*) FROM audit_logs WHERE operation = 'WithdrawAdminInvitation'");
        long mailsAfterFirst = await ScalarAsync<long>("SELECT count(*) FROM dispatch_records");
        using HttpResponseMessage second = await PostAsync(client, "RevokeAdminInvitation", ("Input.AdminInvitationId", id.ToString("D")));
        using HttpResponseMessage missing = await PostAsync(client, "RevokeAdminInvitation", ("Input.AdminInvitationId", Guid.NewGuid().ToString("D")));
        (_, string html) = await GetAsync(client, Path);

        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.Equal(Path, first.Headers.Location?.OriginalString);
        Assert.Equal(1L, trailAfterFirst);
        Assert.NotNull(await ScalarAsync<DateTimeOffset?>("SELECT withdrawn_at FROM admin_invitations"));
        Assert.Equal(HttpStatusCode.SeeOther, second.StatusCode);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs WHERE operation = 'WithdrawAdminInvitation'"));
        Assert.Equal(mailsAfterFirst, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records"));
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.DoesNotContain("geri@example.com", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheListHidesInvitationsThatAreNotValid_UsedWithdrawnInvalidatedAndRunOut_KK2()
    {
        using HttpClient client = ClientOf(Open());
        foreach (string address in new[] { "kullanilmis", "geri", "gecersiz", "dolmus", "gecerli" })
        {
            (await PostAsync(client, "SendAdminInvitation", ("Input.Email", address + "@example.com"))).Dispose();
        }

        await ExecuteAsync("UPDATE link_records SET used_at = now() WHERE admin_invitation_id = (SELECT id FROM admin_invitations WHERE email = 'kullanilmis@example.com')");
        await ExecuteAsync("UPDATE admin_invitations SET accepted_at = now() WHERE email = 'kullanilmis@example.com'");
        await ExecuteAsync("UPDATE admin_invitations SET withdrawn_at = now() WHERE email = 'geri@example.com'");
        await ExecuteAsync("UPDATE admin_invitations SET invalidated_at = now() WHERE email = 'gecersiz@example.com'");
        await ExecuteAsync("UPDATE link_records SET expires_at = now() - interval '1 hour' WHERE admin_invitation_id = (SELECT id FROM admin_invitations WHERE email = 'dolmus@example.com')");

        (_, string html) = await GetAsync(client, Path);

        Assert.Contains("gecerli@example.com", html, StringComparison.Ordinal);
        foreach (string hidden in new[] { "kullanilmis", "geri@", "gecersiz@", "dolmus" })
        {
            Assert.DoesNotContain(hidden, html, StringComparison.Ordinal);
        }

        Assert.Equal(1, Count(html, "handler=RevokeAdminInvitation"));
    }

    [Fact]
    public async Task RemovingWithoutTheMark_IsTheConfirmationState_AndWritesNothing_WithTheNameTheAddressAndTheInvitationCount_KK6()
    {
        Guid target = await AddAdminIdAsync("Hedef", "hedef@example.com");
        using HttpClient client = ClientOf(Open());

        using HttpResponseMessage response = await PostAsync(client, "RemoveAdmin", ("Input.AdminId", target.ToString("D")));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Hedef (hedef@example.com) yönetici olarak kaldırılacak ve açık oturumları hemen kapanacak.", html, StringComparison.Ordinal);
        Assert.Contains("Bu işlem geri alınamaz.", html, StringComparison.Ordinal);
        Assert.Contains("Kaldırma bütün yöneticilere bildirilecek.", html, StringComparison.Ordinal);
        Assert.DoesNotContain("davet geçersizleşecek", html, StringComparison.Ordinal);
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM admins"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
    }

    [Fact]
    public async Task TheConfirmationCountsTheTargetsUnusedInvitations_KK6()
    {
        Guid target = await AddAdminIdAsync("Hedef", "hedef@example.com");
        (string targetKey, Guid sender) = await TestSessions.AdminAsync(_database.Dsn);
        target = sender;
        using WebApplicationFactory<Program> factory = Open();
        using HttpClient asTarget = ClientOf(factory, targetKey);
        (await PostAsync(asTarget, "SendAdminInvitation", ("Input.Email", "davetli@example.com"))).Dispose();

        using HttpClient client = ClientOf(factory);
        using HttpResponseMessage response = await PostAsync(client, "RemoveAdmin", ("Input.AdminId", target.ToString("D")));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Contains("kullanılmamış 1 davet geçersizleşecek.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RemovingWithTheMark_DeletesTheAccountItsSessionsAndLinks_EndsItsInvitations_AndWritesTheTrailTheDisposalRecordAndTheNotice_KK8()
    {
        (string targetKey, Guid target) = await TestSessions.AdminAsync(_database.Dsn);
        using HttpClient client = ClientOf(Open());

        using HttpResponseMessage response = await PostAsync(client, "RemoveAdmin", ("Input.AdminId", target.ToString("D")), ("Input.Confirmed", "true"));

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal(Path, response.Headers.Location?.OriginalString);
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM admins WHERE id = '{target}'"));
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM sessions WHERE admin_id = '{target}'"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs WHERE operation = 'RemoveAdminAccount'"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM disposal_records"));
        Assert.True(await ScalarAsync<long>("SELECT count(*) FROM dispatch_records") >= 2, "The F-6 notice reaches every administrator, the removed one included.");
        Assert.False(string.IsNullOrEmpty(targetKey));
    }

    [Fact]
    public async Task RemovingTheOwnRow_AndTheLastAdministrator_Are422WithThePermanentMessage_AndAMissingRowIs404_KK6()
    {
        using HttpClient client = ClientOf(Open());

        using HttpResponseMessage self = await PostAsync(client, "RemoveAdmin", ("Input.AdminId", _adminId.ToString("D")), ("Input.Confirmed", "true"));
        string html = WebUtility.HtmlDecode(await self.Content.ReadAsStringAsync(Ct));
        using HttpResponseMessage missing = await PostAsync(client, "RemoveAdmin", ("Input.AdminId", Guid.NewGuid().ToString("D")), ("Input.Confirmed", "true"));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, self.StatusCode);
        Assert.Contains("Son yönetici kaldırılamaz.", html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM admins"));
    }

    [Fact]
    public async Task TwoAdministratorsRemovingEachOtherAtOnce_LeaveExactlyOne_KK7()
    {
        (string otherKey, Guid other) = await TestSessions.AdminAsync(_database.Dsn);
        WebApplicationFactory<Program> factory = Open();
        using HttpClient first = ClientOf(factory);
        using HttpClient second = ClientOf(factory, otherKey);

        Task<HttpResponseMessage> a = PostAsync(first, "RemoveAdmin", ("Input.AdminId", other.ToString("D")), ("Input.Confirmed", "true"));
        Task<HttpResponseMessage> b = PostAsync(second, "RemoveAdmin", ("Input.AdminId", _adminId.ToString("D")), ("Input.Confirmed", "true"));
        using HttpResponseMessage[] responses = await Task.WhenAll(a, b);

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM admins"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs WHERE operation = 'RemoveAdminAccount'"));
        Assert.All(responses, r => Assert.NotEqual(HttpStatusCode.InternalServerError, r.StatusCode));
    }

    [Fact]
    public async Task TheListReadsAreFewAndDoNotGrowWithTheRows_ThroughTheService()
    {
        for (int i = 0; i < 12; i++)
        {
            await AddAdminAsync("Yönetici " + i);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient warm = ClientOf(factory);
        using HttpClient client = ClientOf(factory);
        await GetAsync(warm, Path);
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();

        AdminAccountList list = await scope.ServiceProvider.GetRequiredService<IAdminAccountList>().ListAsync(Ct);

        Assert.Equal(13, list.Admins.Count);
        Assert.Empty(list.Invitations);
    }

    // ---- helpers ----

    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(ForgeryTokenSampleModel).Assembly)));

    private HttpClient ClientOf(WebApplicationFactory<Program> factory, string? key = null)
    {
        HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        client.DefaultRequestHeaders.Add(HeaderNames.Cookie, $"{CookieNames.AdminSession}={key ?? _key}");
        return client;
    }

    private static async Task<(HttpStatusCode Status, string Html)> GetAsync(HttpClient client, string path)
    {
        using HttpResponseMessage response = await client.GetAsync(new Uri(path, UriKind.Relative), Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, string handler, params (string Name, string Value)[] fields) =>
        client.PostAsync($"{Path}?handler={handler}", new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value))), Ct);

    private static int Count(string html, string needle) => (html.Length - html.Replace(needle, string.Empty, StringComparison.Ordinal).Length) / needle.Length;

    private async Task<string> AddAdminAsync(string name)
    {
        await AddAdminIdAsync(name, name.ToLowerInvariant().Replace('ğ', 'g').Replace('ş', 's').Replace('ç', 'c').Replace('ö', 'o').Replace('ü', 'u').Replace('ı', 'i') + "@example.com");
        return name;
    }

    private async Task<Guid> AddAdminIdAsync(string name, string email)
    {
        Admin admin = Admin.Create(name, email, "digest-placeholder", DateTimeOffset.UtcNow);
        await using AppDbContext db = NewContext();
        db.Admins.Add(admin);
        await db.SaveChangesAsync(Ct);
        return admin.Id;
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
        object? value = await command.ExecuteScalarAsync(Ct);
        return value is null or DBNull ? default! : (T)value;
    }
}
