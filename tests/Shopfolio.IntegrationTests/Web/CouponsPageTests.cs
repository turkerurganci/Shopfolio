using System.Globalization;
using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Catalog;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the coupons screen's tests (T526): one database copied from the template; every test empties the coupons.</summary>
[CollectionDefinition(CouponsPageTestsDefinition.Name)]
public sealed class CouponsPageTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "CouponsPages";
}

/// <summary>
/// E-35, the coupons screen, and its SaveCoupon handler on the opened application and a real database (T526 criteria 8-10, KK8-KK10; 07 UC-90, UC-122, 2.4.8; K-654, K-799,
/// K-800, K-806): the list newest first with the validity line read for today, a foreign key 404, a new coupon 303, every 422 reason in its own case, and the version's 409
/// before the field rules. The module's rules (the floor under a race, the end day, the frozen share) are the model tests' (CouponTests).
/// </summary>
[Collection(CouponsPageTestsDefinition.Name)]
public sealed class CouponsPageTests : IAsyncLifetime
{
    private const string Path = "/panel/kuponlar";

    // The dates are read in Turkey time against the real clock: the application under test runs on the system time.
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3));

    private static readonly string[] NewestFirst = ["YENIKOD", "ORTAKOD", "ESKIKOD", "DOLUKOD"];

    private readonly CollectionDatabaseFixture _database;

    private string _key = string.Empty;

    public CouponsPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE coupons, admins, sessions CASCADE");
        (_key, _) = await TestSessions.AdminAsync(_database.Dsn);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK8: the list, its order and the four validity lines; a foreign key ----

    [Fact]
    public async Task TheList_IsNewestFirst_EachRowWithTheValidityLineReadFromItsDatesAndRights_T526_KK8()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        await SeedAsync(Coupon.CreatePercentage("DOLUKOD", 10m, Today.AddDays(-10), Today.AddDays(30), 1, now.AddHours(-3)), reserved: 1);
        await SeedAsync(Coupon.CreatePercentage("ESKIKOD", 10m, Today.AddDays(-30), Today.AddDays(-5), 5, now.AddHours(-2)));
        await SeedAsync(Coupon.CreatePercentage("ORTAKOD", 10m, Today.AddDays(5), Today.AddDays(30), 5, now.AddHours(-1)));
        await SeedAsync(Coupon.CreateAmount("YENIKOD", 50m, 300m, Today.AddDays(-1), Today.AddDays(1), 5, now));
        using HttpClient client = ClientOf(Open());

        (HttpStatusCode status, string html) = await GetAsync(client, Path);

        Assert.Equal(HttpStatusCode.OK, status);
        int[] positions = [.. NewestFirst.Select(code => html.IndexOf(code, StringComparison.Ordinal))];
        Assert.All(positions, p => Assert.True(p > 0));
        Assert.Equal(positions.Order(), positions);
        Assert.Contains(CouponsModel.ValidText, html, StringComparison.Ordinal);
        Assert.Contains(CouponsModel.NotStartedText, html, StringComparison.Ordinal);
        Assert.Contains(CouponsModel.ExpiredText, html, StringComparison.Ordinal);
        Assert.Contains(CouponsModel.RightsUsedUpText, html, StringComparison.Ordinal);
        Assert.Contains(CouponsModel.AmountKindText, html, StringComparison.Ordinal);
        Assert.DoesNotContain(CouponsModel.EmptySentence, html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WithoutCoupons_TheListIsTheEmptyRow_AndTheNewCouponLinkStands_T526_KK8()
    {
        using HttpClient client = ClientOf(Open());

        (HttpStatusCode status, string html) = await GetAsync(client, Path);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains(CouponsModel.EmptySentence, html, StringComparison.Ordinal);
        Assert.Contains(CouponsModel.NewCouponLabel, html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ACouponsForm_ShowsItsRightsReadOnly_AndItsVersion_T526_KK8()
    {
        Coupon coupon = await SeedAsync(Coupon.CreatePercentage("FORMKOD", 10m, Today.AddDays(-1), Today.AddDays(10), 5, DateTimeOffset.UtcNow), used: 2, reserved: 1);
        using HttpClient client = ClientOf(Open());

        (HttpStatusCode status, string html) = await GetAsync(client, $"{Path}/{coupon.Id:D}");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("Kullanılmış: 2 · Ayrılmış: 1", html, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"Input.UsedCount\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"Input.ReservedCount\"", html, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.Version\"", html, StringComparison.Ordinal);
        Assert.Contains(CouponsModel.StopNote, html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("00000000-0000-0000-0000-000000000001")]
    public async Task AForeignOrMalformedKey_Is404_OnTheFormAndOnTheSave_T526_KK8(string key)
    {
        using HttpClient client = ClientOf(Open());

        (HttpStatusCode read, _) = await GetAsync(client, $"{Path}/{key}");
        using HttpResponseMessage saved = await SaveAsync(client, key, Form());

        Assert.Equal(HttpStatusCode.NotFound, read);
        Assert.Equal(HttpStatusCode.NotFound, saved.StatusCode);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM coupons"));
    }

    // ---- KK9: a new coupon and a change are written, 303 ----

    [Fact]
    public async Task ANewCoupon_IsWritten_And303GoesToItsOwnAddressWithTheSavedNotice_T526_KK9()
    {
        using HttpClient client = ClientOf(Open());

        using HttpResponseMessage response = await SaveAsync(client, CouponsModel.NewKey, Form());

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Guid id = await ScalarAsync<Guid>("SELECT id FROM coupons WHERE code = 'YAZ10'");
        string location = response.Headers.Location?.OriginalString ?? string.Empty;
        Assert.Contains($"{Path}/{id:D}", location, StringComparison.Ordinal);
        Assert.Contains("notice=Saved", location, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AChange_IsWritten_TheRightsStay_AndNoTrailRowIsWritten_T526_KK9()
    {
        Coupon coupon = await SeedAsync(Coupon.CreatePercentage("DEGISKOD", 10m, Today.AddDays(-1), Today.AddDays(10), 5, DateTimeOffset.UtcNow), used: 1, reserved: 1);
        long trail = await ScalarAsync<long>("SELECT count(*) FROM audit_logs");
        using HttpClient client = ClientOf(Open());

        using HttpResponseMessage response = await SaveAsync(client, coupon.Id.ToString("D"), Form(("Input.Code", "DEGISKOD"), ("Input.Percentage", "25"), ("Input.Version", "1")));

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal("25.00|1|1|2", await ScalarAsync<string>($"SELECT percentage::text || '|' || used_count || '|' || reserved_count || '|' || version FROM coupons WHERE id = '{coupon.Id}'"));
        Assert.Equal(trail, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
    }

    // ---- KK10: every 422 reason in its own case; the floor; 409 before 422 ----

    [Theory]
    [InlineData("Input.Code", "", "Kod boş olamaz.")]
    [InlineData("Input.Code", "yaz10", CouponsModel.DuplicateCodeText)]
    [InlineData("Input.Code", "YAZ10", CouponsModel.DuplicateCodeText)]
    [InlineData("Input.Percentage", "0", "Yüzde geçerli bir yüzde olmalıdır")]
    [InlineData("Input.Percentage", "100", CouponsModel.PercentageReachesHundredText)]
    [InlineData("Input.EndDate", "past", "Bitiş başlangıç tarihinden önce olamaz.")]
    public async Task APercentageCouponsFieldProblem_Is422WithItsOwnMessage_AndWritesNothing_T526_KK10(string field, string value, string message)
    {
        await SeedAsync(Coupon.CreatePercentage("YAZ10", 10m, Today, Today.AddDays(5), 5, DateTimeOffset.UtcNow));
        string sent = value == "past" ? Date(Today.AddDays(-1)) : value;
        using HttpClient client = ClientOf(Open());

        using HttpResponseMessage response = await SaveAsync(client, CouponsModel.NewKey, Form(("Input.Code", "KISKOD"), (field, sent)));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains(message, html, StringComparison.Ordinal);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM coupons"));
    }

    [Theory]
    [InlineData("0", "300", CouponsModel.AmountNotAboveZeroText)]
    [InlineData("50", "50", CouponsModel.MinimumNotAboveAmountText)]
    [InlineData("50", "40", CouponsModel.MinimumNotAboveAmountText)]
    [InlineData("50", "", "Asgari sepet tutarı boş olamaz.")]
    public async Task AFixedAmountCouponsValueAndMinimumRules_Are422WithTheirOwnMessage_T526_KK10(string amount, string minimum, string message)
    {
        using HttpClient client = ClientOf(Open());

        using HttpResponseMessage response = await SaveAsync(
            client,
            CouponsModel.NewKey,
            Form(("Input.CouponType", "Amount"), ("Input.Percentage", string.Empty), ("Input.Amount", amount), ("Input.MinimumBasketAmount", minimum)));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains(message, html, StringComparison.Ordinal);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM coupons"));
    }

    [Fact]
    public async Task AUsageCountUnderTheUsedAndReservedRights_Is422WithTheirNumber_AndTheFloorItselfIsWritten_T526_KK10()
    {
        Coupon coupon = await SeedAsync(Coupon.CreatePercentage("TABANKOD", 10m, Today.AddDays(-1), Today.AddDays(10), 5, DateTimeOffset.UtcNow), used: 2, reserved: 1);
        using HttpClient client = ClientOf(Open());

        using HttpResponseMessage below = await SaveAsync(client, coupon.Id.ToString("D"), Form(("Input.Code", "TABANKOD"), ("Input.TotalUsageLimit", "2"), ("Input.Version", "1")));
        string html = WebUtility.HtmlDecode(await below.Content.ReadAsStringAsync(Ct));
        using HttpResponseMessage equal = await SaveAsync(client, coupon.Id.ToString("D"), Form(("Input.Code", "TABANKOD"), ("Input.TotalUsageLimit", "3"), ("Input.Version", "1")));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, below.StatusCode);
        Assert.Contains(CouponsModel.UsageBelowRightsText(3), html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, equal.StatusCode);
        Assert.Equal(3, await ScalarAsync<int>($"SELECT total_usage_limit FROM coupons WHERE id = '{coupon.Id}'"));
    }

    [Fact]
    public async Task AStaleVersion_Is409EvenWithAFieldProblem_AndOverwritingWithTheCurrentVersionIs303_T526_KK10()
    {
        Coupon coupon = await SeedAsync(Coupon.CreatePercentage("SURUMKOD", 10m, Today.AddDays(-1), Today.AddDays(10), 5, DateTimeOffset.UtcNow));
        await ExecuteAsync($"UPDATE coupons SET version = 2 WHERE id = '{coupon.Id}'");
        using HttpClient client = ClientOf(Open());

        using HttpResponseMessage conflict = await SaveAsync(client, coupon.Id.ToString("D"), Form(("Input.Code", string.Empty), ("Input.Percentage", "100"), ("Input.Version", "1")));
        string html = WebUtility.HtmlDecode(await conflict.Content.ReadAsStringAsync(Ct));
        using HttpResponseMessage overwritten = await SaveAsync(client, coupon.Id.ToString("D"), Form(("Input.Code", "SURUMKOD"), ("Input.Percentage", "15"), ("Input.Version", "2")));

        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        Assert.Contains(ConflictControls.OverwriteLabel, html, StringComparison.Ordinal);
        Assert.DoesNotContain("Kod boş olamaz.", html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, overwritten.StatusCode);
        Assert.Equal("15.00|3", await ScalarAsync<string>($"SELECT percentage::text || '|' || version FROM coupons WHERE id = '{coupon.Id}'"));
    }

    // ---- helpers ----

    private static string Date(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>A valid percentage coupon's form, with <paramref name="overrides"/> put in place of the named fields.</summary>
    private static (string Name, string Value)[] Form(params (string Name, string Value)[] overrides)
    {
        Dictionary<string, string> fields = new(StringComparer.Ordinal)
        {
            ["Input.Code"] = "YAZ10",
            ["Input.CouponType"] = "Percentage",
            ["Input.Percentage"] = "10",
            ["Input.Amount"] = string.Empty,
            ["Input.MinimumBasketAmount"] = string.Empty,
            ["Input.StartDate"] = Date(Today),
            ["Input.EndDate"] = Date(Today.AddDays(10)),
            ["Input.TotalUsageLimit"] = "100",
        };
        foreach ((string name, string value) in overrides)
        {
            fields[name] = value;
        }

        return [.. fields.Select(f => (f.Key, f.Value))];
    }

    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(ForgeryTokenSampleModel).Assembly)));

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

    private static Task<HttpResponseMessage> SaveAsync(HttpClient client, string key, (string Name, string Value)[] fields) =>
        client.PostAsync($"{Path}/{key}?handler=SaveCoupon", new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value))), Ct);

    private async Task<Coupon> SeedAsync(Coupon coupon, int used = 0, int reserved = 0)
    {
        await using (AppDbContext db = NewContext())
        {
            db.Coupons.Add(coupon);
            await db.SaveChangesAsync(Ct);
        }

        // The rights are the counters' (reserve, commit); a seed writes them straight, the version untouched (06 4.13).
        await ExecuteAsync($"UPDATE coupons SET used_count = {used}, reserved_count = {reserved} WHERE id = '{coupon.Id}'");
        return coupon;
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
