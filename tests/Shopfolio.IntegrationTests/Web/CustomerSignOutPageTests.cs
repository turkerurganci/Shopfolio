using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Pages.Frame;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the member's sign-out tests (T519): one database copied from the template.</summary>
[CollectionDefinition(CustomerSignOutPageTestsDefinition.Name)]
public sealed class CustomerSignOutPageTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "CustomerSignOutPageTests";
}

/// <summary>
/// The member's sign-out on the opened application (T519 KK9, KK10; 07 4.34.1 UC-77; 04 3.6.4): the session row and the session cookie go, the cart stays on the account, the known-browser mark stays,
/// the answer is 303 to a page that needs no session or else to the home page, a value that is not the site's own path is the home page, and a request without a session gets the same answer and
/// writes nothing. The page has no GET.
/// </summary>
[Collection(CustomerSignOutPageTestsDefinition.Name)]
public sealed class CustomerSignOutPageTests : IAsyncLifetime
{
    private readonly CollectionDatabaseFixture _database;

    public CustomerSignOutPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await using AppDbContext db = NewContext();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE customers, link_records, dispatch_records, attempt_records, orders, sessions, recognized_browser_marks, carts CASCADE", Ct);
        // The storefront writes no cookie until the cookie policy has been published (05 6.7.10): the cookie is what the sign-out and the revert delete.
        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO legal_text_versions (id, created_at, legal_text_type, version_number, published_at, text) "
            + "VALUES ('01000000-0000-7000-8000-000000519001', now(), 'CookiePolicy', 1, now(), 'policy') ON CONFLICT DO NOTHING",
            Ct);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task TheSignOut_DeletesTheSessionRowAndTheCookie_KeepsTheCartTheMarkAndTheOtherSessions_AndAnswers303_T519_KK9()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (string key, Guid customerId, Guid sessionId) = await TestSessions.CustomerWithSessionAsync(_database.Dsn);
        (_, Guid otherSessionId) = await AddSessionAsync(customerId);
        Guid cartId = await SeedCartAsync(customerId);
        await SeedMarkAsync(customerId);

        using HttpResponseMessage response = await PostAsync(client, $"{FramePaths.SignOut}?handler=SignOut", $"{CookieNames.Session}={key}", ("ReturnUrl", "/sss"));

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal("/sss", response.Headers.Location!.OriginalString);
        Assert.Contains(SetCookies(response), c => c.StartsWith(CookieNames.Session + "=", StringComparison.Ordinal) && c.Contains("expires=", StringComparison.OrdinalIgnoreCase));
        await using AppDbContext db = NewContext();
        Assert.False(await db.Sessions.AnyAsync(s => s.Id == sessionId, Ct));
        Assert.True(await db.Sessions.AnyAsync(s => s.Id == otherSessionId, Ct));
        Assert.True(await db.Carts.AnyAsync(c => c.Id == cartId && c.CustomerId == customerId, Ct));
        Assert.Equal(1, await db.RecognizedBrowserMarks.CountAsync(m => m.CustomerId == customerId && m.ExpiresAt > DateTimeOffset.UtcNow, Ct));
    }

    [Theory]
    [InlineData("/sss", "/sss")]
    [InlineData("/kategori/ornek?page=2", "/kategori/ornek?page=2")]
    [InlineData("/siparis/erisim/abc-DEF_123", "/siparis/erisim/abc-DEF_123")]
    [InlineData("/hesap/silindi", "/hesap/silindi")]
    [InlineData("/hesap", "/")]
    [InlineData("/hesap/siparisler", "/")]
    [InlineData("/siparis/2026-ABC123", "/")]
    [InlineData("/panel", "/")]
    [InlineData("https://evil.example/", "/")]
    [InlineData("//evil.example/", "/")]
    [InlineData("/\\evil.example", "/")]
    [InlineData("", "/")]
    public async Task TheAnswer_IsThePageThatNeedsNoSession_OrTheHomePage_T519_KK10(string returnUrl, string expected)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (string key, _, _) = await TestSessions.CustomerWithSessionAsync(_database.Dsn);

        using HttpResponseMessage response = await PostAsync(client, $"{FramePaths.SignOut}?handler=SignOut", $"{CookieNames.Session}={key}", ("ReturnUrl", returnUrl));

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal(expected, response.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task ARequestWithoutASession_GetsTheSameRedirect_WritesNothing_AndARepeatedPostChangesNothing_T519_KK9()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (string key, Guid customerId, _) = await TestSessions.CustomerWithSessionAsync(_database.Dsn);
        (_, _) = await AddSessionAsync(customerId);

        using HttpResponseMessage first = await PostAsync(client, $"{FramePaths.SignOut}?handler=SignOut", $"{CookieNames.Session}={key}", ("ReturnUrl", "/sss"));
        int after = await SessionCountAsync();
        using HttpResponseMessage again = await PostAsync(client, $"{FramePaths.SignOut}?handler=SignOut", $"{CookieNames.Session}={key}", ("ReturnUrl", "/sss"));
        using HttpResponseMessage none = await PostAsync(client, $"{FramePaths.SignOut}?handler=SignOut", null, ("ReturnUrl", "/hesap/adresler"));
        using HttpResponseMessage stranger = await PostAsync(client, $"{FramePaths.SignOut}?handler=SignOut", null, ("ReturnUrl", "https://evil.example/"));

        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.Equal(HttpStatusCode.SeeOther, again.StatusCode);
        Assert.Equal("/sss", again.Headers.Location!.OriginalString);
        Assert.Equal(HttpStatusCode.SeeOther, none.StatusCode);
        Assert.Equal("/", none.Headers.Location!.OriginalString);
        Assert.Equal("/", stranger.Headers.Location!.OriginalString);
        Assert.Equal(after, await SessionCountAsync());
        Assert.Equal(1, after);
    }

    [Fact]
    public async Task TheSignOutPage_HasNoGet_T519_KK9()
    {
        await using WebApplicationFactory<Program> factory = Open();
        (string key, _, _) = await TestSessions.CustomerWithSessionAsync(_database.Dsn);
        using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false, HandleCookies = false });
        using HttpRequestMessage request = new(HttpMethod.Get, FramePaths.SignOut);
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        request.Headers.TryAddWithoutValidation("Cookie", $"{CookieNames.Session}={key}");

        using HttpResponseMessage response = await client.SendAsync(request, Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(1, await SessionCountAsync());
    }

    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(FrameSampleModel).Assembly)));

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task<int> SessionCountAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.Sessions.CountAsync(Ct);
    }

    private async Task<(string Key, Guid SessionId)> AddSessionAsync(Guid customerId)
    {
        string key = SecretKey.Create();
        Session session = Session.ForCustomer(customerId, SecretKey.Hash(key), false, DateTimeOffset.UtcNow);
        await using AppDbContext db = NewContext();
        db.Sessions.Add(session);
        await db.SaveChangesAsync(Ct);
        return (key, session.Id);
    }

    private async Task<Guid> SeedCartAsync(Guid customerId)
    {
        Cart cart = Cart.ForCustomer(customerId, DateTimeOffset.UtcNow);
        await using AppDbContext db = NewContext();
        db.Carts.Add(cart);
        await db.SaveChangesAsync(Ct);
        return cart.Id;
    }

    private async Task SeedMarkAsync(Guid customerId)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        await using AppDbContext db = NewContext();
        db.RecognizedBrowserMarks.Add(RecognizedBrowserMark.ForCustomer(customerId, SecretKey.Hash(SecretKey.Create()), now.AddDays(30), now));
        await db.SaveChangesAsync(Ct);
    }

    private static async Task<HttpResponseMessage> PostAsync(HttpClient client, string path, string? cookie, (string Name, string Value) field)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, path);
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        if (cookie is not null)
        {
            request.Headers.TryAddWithoutValidation("Cookie", cookie);
        }

        request.Content = new FormUrlEncodedContent([new KeyValuePair<string, string>(field.Name, field.Value)]);
        return await client.SendAsync(request, Ct);
    }

    private static List<string> SetCookies(HttpResponseMessage response) => response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? values) ? [.. values] : [];
}
