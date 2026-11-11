using System.Net;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.IntegrationTests.Web;
using Shopfolio.Pages.Orders;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>The collection of the order tracking page tests (T511): its own database, because the attempt counts of L-5 are kept per address and per e-mail.</summary>
[CollectionDefinition(OrderTrackingPageDefinition.Name)]
public sealed class OrderTrackingPageDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "OrderTrackingPage";
}

/// <summary>
/// E-15 on the opened application and a real PostgreSQL (T511 criteria 1 to 3; KK1 to KK3; 07 UC-31, UC-32): the form with the reminder of the two other ways and noindex; a match
/// of the number in any letter case with the e-mail answers 303 to the access-key root; an empty or malformed field is 422; a miss is the same neutral sentence whichever part was
/// wrong, stays in the attempt record, and a disposed order never matches; the two axes of L-5 are counted apart, and over either one the answer is 429.
/// </summary>
[Collection(OrderTrackingPageDefinition.Name)]
public sealed class OrderTrackingPageTests : IAsyncLifetime
{
    private readonly CollectionDatabaseFixture _database;
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public OrderTrackingPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await using (AppDbContext db = NewContext())
        {
            await db.Database.ExecuteSqlRawAsync("DELETE FROM attempt_records", Ct);
        }

        _factory = new ShopfolioWebApplicationFactory(_database.Dsn)
            .WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(ForgeryTokenSampleModel).Assembly)));
        _client = ForgeryTokenHandler.CreateClient(_factory);
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task<HttpResponseMessage> FindAsync(string number, string email)
    {
        using FormUrlEncodedContent form = new(
        [
            new KeyValuePair<string, string>("Input." + nameof(FindOrderInput.OrderNumber), number),
            new KeyValuePair<string, string>("Input." + nameof(FindOrderInput.ContactEmail), email),
        ]);
        return await _client.PostAsync(new Uri(TrackingModel.Path + "?handler=FindOrder", UriKind.Relative), form, Ct);
    }

    private string Encoded(string text) => _factory.Services.GetRequiredService<HtmlEncoder>().Encode(text);

    private async Task<long> AttemptsAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.Database.SqlQueryRaw<long>("SELECT count(*) AS \"Value\" FROM attempt_records WHERE attempt_limit = 'L5'").SingleAsync(Ct);
    }

    // --- KK1 ---

    [Fact]
    public async Task TheForm_HasTheTwoFieldsAndTheReminder_AndIsClosedToSearchEngines_KK1()
    {
        using HttpResponseMessage response = await _client.GetAsync(new Uri(TrackingModel.Path, UriKind.Relative), Ct);
        string body = await response.Content.ReadAsStringAsync(Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("name=\"Input.OrderNumber\"", body, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.ContactEmail\"", body, StringComparison.Ordinal);
        Assert.Contains("id=\"order-tracking-reminder\"", body, StringComparison.Ordinal);
        Assert.Contains("name=\"robots\" content=\"noindex", body, StringComparison.Ordinal);

        // The closed screen's head (RecordPages.ClosedScreen; D-693): one title and no description.
        Assert.Equal(2, body.Split("<title>", StringSplitOptions.None).Length);
        Assert.DoesNotContain("name=\"description\"", body, StringComparison.Ordinal);
    }

    // --- KK2 ---

    [Fact]
    public async Task AMatch_InAnyLetterCase_Is303ToTheAccessKeyRoot_AndForgetsTheAttempt_KK2()
    {
        (_, string number, string key) = await TestSessions.OrderAsync(_database.Dsn, customerId: null);

        using HttpResponseMessage exact = await FindAsync(number, "alici@example.com");
        using HttpResponseMessage folded = await FindAsync(number.ToLowerInvariant(), "ALICI@example.com");

        foreach (HttpResponseMessage response in new[] { exact, folded })
        {
            Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
            Assert.Equal(OrderRoutes.AccessPath(key), response.Headers.Location?.OriginalString);
        }

        Assert.Equal(0, await AttemptsAsync());
    }

    [Fact]
    public async Task TheOrderOfADeletedAccount_MatchesItsFrozenEmail_KK2()
    {
        (_, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        (Guid orderId, string number, string key) = await TestSessions.OrderAsync(_database.Dsn, customerId);
        await using (AppDbContext db = NewContext())
        {
            await db.Database.ExecuteSqlAsync($"UPDATE orders SET is_account_deleted = true, customer_id = NULL WHERE id = {orderId}", Ct);
        }

        using HttpResponseMessage response = await FindAsync(number, "alici@example.com");

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal(OrderRoutes.AccessPath(key), response.Headers.Location?.OriginalString);
    }

    // --- KK3 ---

    [Fact]
    public async Task AnEmptyOrMalformedField_Is422_AndCountsNothing_KK3()
    {
        using HttpResponseMessage empty = await FindAsync(string.Empty, "alici@example.com");
        using HttpResponseMessage malformed = await FindAsync("2026-ABC", "not-an-email");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, empty.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, malformed.StatusCode);
        Assert.Equal(0, await AttemptsAsync());
    }

    [Fact]
    public async Task AMiss_IsTheSameNeutralSentence_WhicheverPartWasWrong_AndADisposedOrderNeverMatches_KK3()
    {
        (Guid orderId, string number, _) = await TestSessions.OrderAsync(_database.Dsn, customerId: null);
        (Guid disposedId, string disposedNumber, _) = await TestSessions.OrderAsync(_database.Dsn, customerId: null);
        await using (AppDbContext db = NewContext())
        {
            Modules.Ordering.Order disposed = await db.Orders.SingleAsync(o => o.Id == disposedId, Ct);
            disposed.ErasePersonalData();
            disposed.BumpVersion();
            await db.SaveChangesAsync(Ct);
        }

        using HttpResponseMessage wrongEmail = await FindAsync(number, "baska@example.com");
        using HttpResponseMessage noOrder = await FindAsync("2026-ZZZZZZZZ", "alici@example.com");
        using HttpResponseMessage gone = await FindAsync(disposedNumber, "alici@example.com");

        string sentence = Encoded(TrackingModel.NotMatchedText);
        foreach (HttpResponseMessage response in new[] { wrongEmail, noOrder, gone })
        {
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Contains(sentence, await response.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
        }

        // Every miss stays in the attempt record on both axes (L-5 counts failures; K-772).
        Assert.Equal(6, await AttemptsAsync());
        _ = orderId;
    }

    [Fact]
    public async Task TheEmailAxis_IsCountedApart_AndOverItsThresholdTheAnswerIs429EvenForARightQuery_KK3()
    {
        (_, string number, _) = await TestSessions.OrderAsync(_database.Dsn, customerId: null);
        for (int i = 0; i < ProductConstants.GuestOrderLookupAttemptsPerEmail; i++)
        {
            using HttpResponseMessage miss = await FindAsync("2026-ZZZZZZZZ", "alici@example.com");
            Assert.Equal(HttpStatusCode.UnprocessableEntity, miss.StatusCode);
        }

        using HttpResponseMessage limited = await FindAsync(number, "alici@example.com");
        using HttpResponseMessage otherEmail = await FindAsync("2026-ZZZZZZZZ", "baska@example.com");

        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.Null(limited.Headers.Location);

        // The limit's answer (D-801; 12 A353): the neutral limit message, no Retry-After.
        Assert.Null(limited.Headers.RetryAfter);
        Assert.Contains(Encoded(Shopfolio.Infrastructure.Security.NeutralMessages.LimitExceeded), await limited.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);

        // The address's own count is below its threshold: another e-mail from the same address is still looked up.
        Assert.Equal(HttpStatusCode.UnprocessableEntity, otherEmail.StatusCode);
    }

    [Fact]
    public async Task TheAddressAxis_IsCountedApart_AndOverItsThresholdEveryEmailIs429_KK3()
    {
        (_, string number, _) = await TestSessions.OrderAsync(_database.Dsn, customerId: null);
        for (int i = 0; i < ProductConstants.GuestOrderLookupAttemptsPerIp; i++)
        {
            using HttpResponseMessage miss = await FindAsync("2026-ZZZZZZZZ", $"kisi{i}@example.com");
            Assert.Equal(HttpStatusCode.UnprocessableEntity, miss.StatusCode);
        }

        using HttpResponseMessage limited = await FindAsync(number, "alici@example.com");

        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
    }
}
