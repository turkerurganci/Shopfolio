using System.Globalization;
using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the operation trail screen's tests (T522): one database copied from the template; every test seeds the rows again.</summary>
[CollectionDefinition(AuditTrailPageTestsDefinition.Name)]
public sealed class AuditTrailPageTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "AuditTrailPages";
}

/// <summary>The operation trail screen (E-52) on the opened application and a real database (T522 criteria 9-10, KK9-KK10; 07 UC-106; 04 9.23).</summary>
[Collection(AuditTrailPageTestsDefinition.Name)]
public sealed class AuditTrailPageTests : IAsyncLifetime
{
    private const string Path = "/panel/islem-izi";

    private readonly CollectionDatabaseFixture _database;

    private string _key = string.Empty;
    private Guid _adminId;

    public AuditTrailPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE admins, audit_logs, sessions CASCADE");
        (_key, _adminId) = await TestSessions.AdminAsync(_database.Dsn);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task TheRowsAreNewestFirst_TwentyFiveAPage_AndTheSecondPageHoldsTheRest_KK9()
    {
        DateTimeOffset start = new(2026, 3, 1, 9, 0, 0, TimeSpan.Zero);
        for (int i = 0; i < 26; i++)
        {
            await SeedAsync(_adminId, "Ad " + i.ToString("D2", CultureInfo.InvariantCulture), start.AddMinutes(i), "kayit-" + i.ToString("D2", CultureInfo.InvariantCulture));
        }

        using HttpClient client = ClientOf(Open());

        (HttpStatusCode firstStatus, string first) = await GetAsync(client, Path);
        (HttpStatusCode secondStatus, string second) = await GetAsync(client, Path + "?page=2");

        Assert.Equal(HttpStatusCode.OK, firstStatus);
        Assert.Equal(HttpStatusCode.OK, secondStatus);
        Assert.Equal(25, Count(first, "kayit-"));
        Assert.True(first.IndexOf("kayit-25", StringComparison.Ordinal) < first.IndexOf("kayit-24", StringComparison.Ordinal));
        Assert.DoesNotContain("kayit-00", first, StringComparison.Ordinal);
        Assert.Equal(1, Count(second, "kayit-"));
        Assert.Contains("kayit-00", second, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ARowCarriesTheNameAndTheAddressOfTheMomentTheArea_TheOperationAndTheValues_AndNoCustomerData_KK9()
    {
        await SeedAsync(_adminId, "Eski Ad", new DateTimeOffset(2026, 3, 1, 9, 0, 0, TimeSpan.Zero), "davetli@example.com", "eski-ad@example.com");
        using HttpClient client = ClientOf(Open());

        (_, string html) = await GetAsync(client, Path);

        Assert.Contains("Eski Ad · eski-ad@example.com", html, StringComparison.Ordinal);
        Assert.Contains(AuditTexts.OperationOf(AuditOperation.SendAdminInvitation), html, StringComparison.Ordinal);
        Assert.Contains(AuditTexts.AreaOf(AuditArea.AdminAccounts), html, StringComparison.Ordinal);
        Assert.Contains("davetli@example.com", html, StringComparison.Ordinal);
        Assert.DoesNotContain("<a ", html[html.IndexOf("<table", StringComparison.Ordinal)..html.IndexOf("</table>", StringComparison.Ordinal)], StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheDateAndTheAdministratorFiltersApplyTogether_AndEachHasARemovalChip_KK10()
    {
        Guid other = await AddAdminAsync("Diğer", "diger@example.com");
        await SeedAsync(_adminId, "Test yonetici", new DateTimeOffset(2026, 3, 1, 9, 0, 0, TimeSpan.Zero), "bu-satir-eski");
        await SeedAsync(_adminId, "Test yonetici", new DateTimeOffset(2026, 3, 10, 9, 0, 0, TimeSpan.Zero), "bu-satir-gelir");
        await SeedAsync(other, "Diğer", new DateTimeOffset(2026, 3, 10, 9, 0, 0, TimeSpan.Zero), "bu-satir-baskasinin");
        using HttpClient client = ClientOf(Open());

        (HttpStatusCode status, string html) = await GetAsync(client, $"{Path}?startDate=2026-03-05&endDate=2026-03-15&adminId={_adminId:D}");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("bu-satir-gelir", html, StringComparison.Ordinal);
        Assert.DoesNotContain("bu-satir-eski", html, StringComparison.Ordinal);
        Assert.DoesNotContain("bu-satir-baskasinin", html, StringComparison.Ordinal);
        Assert.Equal(3, Count(html, "request-filter__chip-remove"));
    }

    [Theory]
    [InlineData("startDate=2026-03-15&endDate=2026-03-05", "başlangıç tarihinden önce olamaz.")]
    [InlineData("startDate=2999-01-01", "İleri tarih seçilemez.")]
    public async Task ARangeThatDoesNotHold_Is200WithTheFieldMessage_AndAppliesNoFilter_KK10(string query, string message)
    {
        await SeedAsync(_adminId, "Test yonetici", new DateTimeOffset(2026, 3, 1, 9, 0, 0, TimeSpan.Zero), "unfiltered-row");
        using HttpClient client = ClientOf(Open());

        (HttpStatusCode status, string html) = await GetAsync(client, $"{Path}?{query}&adminId={_adminId:D}");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains(message, html, StringComparison.Ordinal);
        Assert.Contains("unfiltered-row", html, StringComparison.Ordinal);
        Assert.DoesNotContain("request-filter__chip-remove", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ARemovedAdministratorIsSelectable_WithTheSuffix_AndItsRowsStay_KK9()
    {
        Guid gone = Guid.NewGuid();
        await SeedAsync(gone, "Giden", new DateTimeOffset(2026, 3, 1, 9, 0, 0, TimeSpan.Zero), "giden-satiri", "giden@example.com");
        using HttpClient client = ClientOf(Open());

        (_, string all) = await GetAsync(client, Path);
        (_, string filtered) = await GetAsync(client, $"{Path}?adminId={gone:D}");

        Assert.Contains("Giden (giden@example.com) (kaldırıldı)", all, StringComparison.Ordinal);
        Assert.Contains("giden-satiri", filtered, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WithNoRowsTheEmptyStateGoesHome_AndWithAFilterThatMatchesNothingItCarriesTheRemovalPath_KK10()
    {
        using HttpClient client = ClientOf(Open());

        (_, string none) = await GetAsync(client, Path);
        (_, string noMatch) = await GetAsync(client, Path + "?startDate=2026-03-05&endDate=2026-03-15");

        Assert.Contains(AuditTrailModel.NoRowsText, none, StringComparison.Ordinal);
        Assert.Contains(AuditTrailModel.HomeLabel, none, StringComparison.Ordinal);
        Assert.Contains(AuditTrailModel.NoMatchText, noMatch, StringComparison.Ordinal);
        Assert.Contains(AuditTrailModel.RemoveFilterLabel, noMatch, StringComparison.Ordinal);
    }

    // ---- helpers ----

    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn);

    private HttpClient ClientOf(WebApplicationFactory<Program> factory)
    {
        HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        client.DefaultRequestHeaders.Add(HeaderNames.Cookie, $"{CookieNames.AdminSession}={_key}");
        return client;
    }

    private static async Task<(HttpStatusCode Status, string Html)> GetAsync(HttpClient client, string path)
    {
        using HttpResponseMessage response = await client.GetAsync(new Uri(path, UriKind.Relative), Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static int Count(string html, string needle) => (html.Length - html.Replace(needle, string.Empty, StringComparison.Ordinal).Length) / needle.Length;

    private async Task SeedAsync(Guid adminId, string name, DateTimeOffset at, string displayName, string? email = null)
    {
        AuditLog row = AuditLog.Create(
            AuditOperation.SendAdminInvitation,
            adminId,
            name,
            email ?? "seed@example.com",
            "AdminInvitation",
            Guid.NewGuid(),
            new AuditDetails(displayName),
            at);
        await using AppDbContext db = NewContext();
        db.AuditLogs.Add(row);
        await db.SaveChangesAsync(Ct);
    }

    private async Task<Guid> AddAdminAsync(string name, string email)
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
}
