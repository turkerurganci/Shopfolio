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
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The corporate content screens on the opened application and a real database (T534; 07 UC-96, UC-98, UC-158 to UC-161, UC-163; 04 9.13, 9.14): the list with its seven groups and no paging, the form that
/// is 404 outside the closed type list, the save that makes a draft, the gate's refusal, the absolute status move, the deletion's confirmation state and the deletion, the move in the manual order with its
/// stale-screen refusal, and the brand screen's read and save.
/// </summary>
[Collection(ProductListPageTestsDefinition.Name)]
public sealed class ContentPanelPageTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Base = new(2026, 11, 14, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _database;

    private string _adminKey = string.Empty;

    private int _counter;

    public ContentPanelPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ListSeed.ClearAsync(_database.Dsn);
        await ExecuteAsync("TRUNCATE about_pages, offerings, portfolio_items, faq_items, branches, custom_pages, announcements CASCADE");
        _adminKey = (await TestSessions.AdminAsync(_database.Dsn)).Key;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task TheList_ShowsTheSevenGroupsInOrder_WithoutAPager_AndDrawsNoInlineStyle()
    {
        await SeedOfferingAsync("Montaj", 1);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, PanelFramePaths.CorporateContent);

        Assert.Equal(HttpStatusCode.OK, status);
        int[] at = [.. Enum.GetValues<PanelContentType>().Select(t => html.IndexOf(ContentRoutes.GroupNameOf(t), StringComparison.Ordinal))];
        Assert.All(at, position => Assert.True(position >= 0));
        Assert.Equal(at.Order(), at);
        Assert.Contains("Montaj", html, StringComparison.Ordinal);
        Assert.DoesNotContain("style=", html, StringComparison.Ordinal);
        Assert.DoesNotContain("pager", html, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("/panel/kurumsal-icerik/urun/yeni")]
    [InlineData("/panel/kurumsal-icerik/HIZMET/yeni")]
    [InlineData("/panel/kurumsal-icerik/hizmet/00000000-0000-0000-0000-000000000001")]
    [InlineData("/panel/kurumsal-icerik/sss/00000000-0000-0000-0000-000000000001")]
    public async Task AFormOutsideTheClosedTypeList_OrOfAMissingRecord_Is404(string path)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, _) = await GetAsync(client, path);

        Assert.Equal(HttpStatusCode.NotFound, status);
    }

    [Fact]
    public async Task TheNewForm_Opens_AndASave_MakesADraft_AtTheEndOfItsGroup_AndRedirectsToTheRecord()
    {
        await SeedOfferingAsync("Eski hizmet", 1);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode opened, _) = await GetAsync(client, ContentRoutes.NewPath(PanelContentType.Offering));
        using HttpResponseMessage saved = await PostAsync(client, ContentRoutes.NewPath(PanelContentType.Offering), "SaveContent", ("Input.Name", "Yeni hizmet"));

        Assert.Equal(HttpStatusCode.OK, opened);
        Assert.Equal(HttpStatusCode.SeeOther, saved.StatusCode);
        Assert.Equal("Draft", await ScalarAsync<string>("SELECT publish_status FROM offerings WHERE name = 'Yeni hizmet'"));
        Assert.Equal(2, await ScalarAsync<int>("SELECT sort_order FROM offerings WHERE name = 'Yeni hizmet'"));
        Assert.StartsWith(ContentRoutes.Root + "/hizmet/", saved.Headers.Location!.OriginalString, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ASave_WithoutTheRequiredName_IsRefused422_AndWritesNothing()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage refused = await PostAsync(client, ContentRoutes.NewPath(PanelContentType.Offering), "SaveContent", ("Input.Name", "  "));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM offerings"));
    }

    [Fact]
    public async Task APublishMove_OfAnIncompleteRecord_Is422_AndOfACompleteOne_RedirectsAndPublishes()
    {
        Guid incomplete = await SeedFaqAsync("Boş cevap?", null, 1);
        Guid complete = await SeedFaqAsync("İade var mı?", "<p>Evet.</p>", 2);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage refused = await PostAsync(client, ContentRoutes.FormPath(PanelContentType.FaqItem, incomplete), "ChangePublishStatus", ("Input.PublishStatus", "Published"));
        using HttpResponseMessage moved = await PostAsync(client, ContentRoutes.FormPath(PanelContentType.FaqItem, complete), "ChangePublishStatus", ("Input.PublishStatus", "Published"));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
        Assert.Equal(HttpStatusCode.SeeOther, moved.StatusCode);
        Assert.Equal("Draft", await ScalarAsync<string>($"SELECT publish_status FROM faq_items WHERE id = '{incomplete}'"));
        Assert.Equal("Published", await ScalarAsync<string>($"SELECT publish_status FROM faq_items WHERE id = '{complete}'"));
    }

    [Fact]
    public async Task ADeletion_WithoutTheMark_ShowsTheConfirmation_AndWritesNothing_ThenWithIt_DeletesAndGoesToTheList()
    {
        Guid faq = await SeedFaqAsync("İade var mı?", "<p>Evet.</p>", 1);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        string path = ContentRoutes.FormPath(PanelContentType.FaqItem, faq);

        using HttpResponseMessage asked = await PostAsync(client, path, "DeleteContent");
        long afterAsk = await ScalarAsync<long>("SELECT count(*) FROM faq_items");
        using HttpResponseMessage done = await PostAsync(client, path, "DeleteContent", ("Input.Confirmed", "true"));

        Assert.Equal(HttpStatusCode.OK, asked.StatusCode);
        Assert.Equal(1L, afterAsk);
        Assert.Equal(HttpStatusCode.SeeOther, done.StatusCode);
        Assert.Equal(PanelFramePaths.CorporateContent, done.Headers.Location!.OriginalString);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM faq_items"));
    }

    [Fact]
    public async Task AMove_WithTheShownOrders_SwapsTheRecords_AndAStaleScreen_Is409_WithNothingWritten()
    {
        await SeedOfferingAsync("Birinci", 1);
        Guid second = await SeedOfferingAsync("İkinci", 2);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage stale = await PostAsync(
            client, PanelFramePaths.CorporateContent, "ReorderContent",
            ("Input.ContentKind", "hizmet"), ("Input.ContentId", second.ToString("D")), ("Input.Direction", "Up"), ("Input.OriginalSortOrder", "2"), ("Input.OriginalAdjacentSortOrder", "9"));
        int afterStale = await ScalarAsync<int>($"SELECT sort_order FROM offerings WHERE id = '{second}'");
        using HttpResponseMessage moved = await PostAsync(
            client, PanelFramePaths.CorporateContent, "ReorderContent",
            ("Input.ContentKind", "hizmet"), ("Input.ContentId", second.ToString("D")), ("Input.Direction", "Up"), ("Input.OriginalSortOrder", "2"), ("Input.OriginalAdjacentSortOrder", "1"));

        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal(2, afterStale);
        Assert.Equal(HttpStatusCode.SeeOther, moved.StatusCode);
        Assert.Equal(1, await ScalarAsync<int>($"SELECT sort_order FROM offerings WHERE id = '{second}'"));
    }

    [Fact]
    public async Task TheBrandScreen_Opens_AnAnInvalidColor_Is422_AStaleVersion_Is409_AndAGoodSave_RedirectsAndShowsTheNewName()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode opened, string html) = await GetAsync(client, PanelFramePaths.Brand);
        int version = int.Parse(System.Text.RegularExpressions.Regex.Match(System.Text.RegularExpressions.Regex.Match(html, @"<input[^>]*Input\.Version[^>]*>").Value, @"value=""(\d+)""").Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
        using HttpResponseMessage badColor = await PostAsync(client, PanelFramePaths.Brand, "SaveBranding", ("Input.BrandName", "Deneme"), ("Input.BrandColor", "ZZZ"), ("Input.Version", version.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        using HttpResponseMessage stale = await PostAsync(client, PanelFramePaths.Brand, "SaveBranding", ("Input.BrandName", "Deneme"), ("Input.Version", "999"));
        using HttpResponseMessage saved = await PostAsync(client, PanelFramePaths.Brand, "SaveBranding", ("Input.BrandName", "Deneme Markası"), ("Input.Version", version.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        (_, string after) = await GetAsync(client, PanelFramePaths.Brand);

        Assert.Equal(HttpStatusCode.OK, opened);
        Assert.Contains("Marka", html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, badColor.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal(HttpStatusCode.SeeOther, saved.StatusCode);
        Assert.Contains("Deneme Markası", after, StringComparison.Ordinal);
    }

    // ---- helpers ----

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

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, string path, string handler, params (string Name, string Value)[] fields) =>
        client.PostAsync($"{path}?handler={handler}", new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value))), Ct);

    private async Task<Guid> SeedOfferingAsync(string name, int order)
    {
        int n = Interlocked.Increment(ref _counter);
        Offering offering = Offering.Create(name, $"hizmet-{n:D3}", order, Base.AddMinutes(n));
        await SaveAsync(c => c.Offerings.Add(offering));
        return offering.Id;
    }

    private async Task<Guid> SeedFaqAsync(string question, string? answer, int order)
    {
        int n = Interlocked.Increment(ref _counter);
        FaqItem faq = FaqItem.Create(question, order, Base.AddMinutes(n));
        faq.SetAnswer(answer);
        await SaveAsync(c => c.FaqItems.Add(faq));
        return faq.Id;
    }

    private async Task SaveAsync(Action<AppDbContext> change)
    {
        await using AppDbContext db = ListSeed.Open(_database.Dsn);
        change(db);
        await db.SaveChangesAsync(Ct);
    }

    private async Task<T> ScalarAsync<T>(string sql)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return (T)(await command.ExecuteScalarAsync(Ct))!;  // non-null: every query here returns one row
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }
}
