using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the FAQ page tests (T506): one database copied from the template.</summary>
[CollectionDefinition(FaqPageDefinition.Name)]
public sealed class FaqPageDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "FaqPage";
}

/// <summary>
/// The FAQ page on the opened application with a real PostgreSQL (T506 criteria 1-2; KK1-KK2; 07 UC-13; 04 5.9): the published questions in the firm's manual order in one list, the empty-state
/// line while none is published, the draft question that only a panel session sees in its place with the "Taslak" label, and the structured data that is in the first response of a visitor and is
/// left out of the page that shows a draft (which is closed to the search engines).
/// </summary>
[Collection(FaqPageDefinition.Name)]
public sealed class FaqPageTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 11, 9, 0, 0, TimeSpan.Zero);

    private static readonly Regex _questions = new("""<h2 class="faq__question">(.*?)</h2>""", RegexOptions.Singleline | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));

    private static readonly Regex _tags = new("<[^>]+>", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));

    private readonly CollectionDatabaseFixture _database;

    private int _counter;

    public FaqPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await using AppDbContext db = NewContext();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE faq_items CASCADE", Ct);
        await db.Database.ExecuteSqlRawAsync("UPDATE company_settings SET menu_name_faq_items = 'Sık sorulan sorular'", Ct);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private WebApplicationFactory<Program> Open() => new ShopfolioWebApplicationFactory(_database.Dsn);

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
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

    private static string[] QuestionsOf(string html) =>
        [.. _questions.Matches(html).Select(m => Regex.Replace(_tags.Replace(m.Groups[1].Value, string.Empty), @"\s+", " ", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)).Trim())];

    private async Task SeedAsync(string question, int order, PublishStatus status = PublishStatus.Published, string? answer = null)
    {
        int n = Interlocked.Increment(ref _counter);
        FaqItem item = FaqItem.Create(question, order, Now.AddMinutes(n));
        item.SetAnswer(answer ?? $"<p>Cevap {n}</p>");
        item.PublishStatus = status;
        await using AppDbContext db = NewContext();
        db.FaqItems.Add(item);
        await db.SaveChangesAsync(Ct);
    }

    // --- KK1: the manual order in one list, the empty-state line ---

    [Fact]
    public async Task ThePublishedQuestions_AreInOneListInTheManualOrder_WithTheirAnswers_NoDraftAndNoSearchOrPaging_KK1()
    {
        await SeedAsync("Üçüncü soru", 30);
        await SeedAsync("Birinci soru", 10, answer: "<p>Birinci cevap <strong>kalın</strong></p>");
        await SeedAsync("İkinci soru", 20);
        await SeedAsync("Taslak soru", 15, PublishStatus.Draft);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        (HttpStatusCode status, string html) = await GetAsync(client, "/sss");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(["Birinci soru", "İkinci soru", "Üçüncü soru"], QuestionsOf(html));
        Assert.Contains("<p>Birinci cevap <strong>kalın</strong></p>", html, StringComparison.Ordinal);
        Assert.Equal(1, Regex.Count(html, """<ol class="faq__list">""", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));
        Assert.DoesNotContain("Taslak soru", html, StringComparison.Ordinal);
        Assert.DoesNotContain("faq__draft", html, StringComparison.Ordinal);
        Assert.DoesNotContain("type=\"search\"", html[html.IndexOf("class=\"faq\"", StringComparison.Ordinal)..], StringComparison.Ordinal);
        Assert.DoesNotContain("pagination", html, StringComparison.Ordinal);
        Assert.DoesNotContain("empty-state", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WhileNoQuestionIsPublished_TheEmptyStateLineShows_WithTheWayHome_KK1()
    {
        await SeedAsync("Taslak soru", 1, PublishStatus.Draft);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        (HttpStatusCode status, string html) = await GetAsync(client, "/sss");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("Henüz yayında soru yok.", html, StringComparison.Ordinal);
        Assert.Contains("Ana sayfaya dön", html, StringComparison.Ordinal);
        Assert.Contains("empty-state", html, StringComparison.Ordinal);
        Assert.Empty(QuestionsOf(html));
        Assert.DoesNotContain("faq__list", html, StringComparison.Ordinal);
        Assert.DoesNotContain("application/ld+json\">{\"@context\":\"https://schema.org\",\"@type\":\"FAQPage\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ThePageName_IsTheMenuNameOfTheFaqItem_KK1()
    {
        await SeedAsync("Soru", 1);
        await using (AppDbContext db = NewContext())
        {
            await db.Database.ExecuteSqlRawAsync("UPDATE company_settings SET menu_name_faq_items = 'Merak edilenler'", Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        (_, string html) = await GetAsync(client, "/sss");

        Assert.Contains("""<h1 class="faq__title" id="faq-title">Merak edilenler</h1>""", html, StringComparison.Ordinal);
    }

    // --- KK2: the draft label for the panel session; the structured data in the first response ---

    [Fact]
    public async Task TheStructuredData_IsInTheHeadOfTheFirstResponse_ForAVisitor_AndHoldsThePublishedQuestionsOnly_KK2()
    {
        await SeedAsync("Kargo ne kadar sürer?", 1, answer: "<p>İki gün.</p>");
        await SeedAsync("Taslak soru", 2, PublishStatus.Draft);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        (_, string html) = await GetAsync(client, "/sss");

        string compact = html.Replace(" ", string.Empty, StringComparison.Ordinal);
        Assert.Contains("""<script type="application/ld+json">""", html, StringComparison.Ordinal);
        Assert.Contains("\"@type\":\"FAQPage\"", compact, StringComparison.Ordinal);
        Assert.Contains("Kargonekadarsürer?", compact, StringComparison.Ordinal);
        Assert.True(html.IndexOf("application/ld+json", StringComparison.Ordinal) < html.IndexOf("<body", StringComparison.Ordinal), "The structured data is in the head.");
        Assert.DoesNotContain("""<meta name="robots" content="noindex""", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task APanelSession_SeesTheDraftQuestionInItsPlace_WithTheDraftLabel_AndThePageIsClosedToTheSearchEngines_KK2()
    {
        await SeedAsync("Birinci soru", 10);
        await SeedAsync("Taslak soru", 15, PublishStatus.Draft);
        await SeedAsync("Üçüncü soru", 20);
        (string adminKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        (string customerKey, _) = await TestSessions.CustomerAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        (_, string admin) = await GetAsync(client, "/sss", $"{CookieNames.AdminSession}={adminKey}");
        (_, string customer) = await GetAsync(client, "/sss", $"{CookieNames.Session}={customerKey}");

        Assert.Equal(["Birinci soru", "Taslak Taslak soru", "Üçüncü soru"], QuestionsOf(admin));
        Assert.Contains("""<li class="faq__item faq__item--draft">""", admin, StringComparison.Ordinal);
        Assert.Contains("""<span class="faq__draft">Taslak</span>""", admin, StringComparison.Ordinal);
        Assert.Contains("""<meta name="robots" content="noindex""", admin, StringComparison.Ordinal);
        Assert.DoesNotContain("\"@type\":\"FAQPage\"", admin.Replace(" ", string.Empty, StringComparison.Ordinal), StringComparison.Ordinal);

        // A member is no panel session: the list is the visitor's, with the structured data.
        Assert.Equal(["Birinci soru", "Üçüncü soru"], QuestionsOf(customer));
        Assert.DoesNotContain("faq__draft", customer, StringComparison.Ordinal);
        Assert.Contains("\"@type\":\"FAQPage\"", customer.Replace(" ", string.Empty, StringComparison.Ordinal), StringComparison.Ordinal);
    }
}
