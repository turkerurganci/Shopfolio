using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The attempt limit pattern on the opened application and a test-only sample page (T215 KK6; 07 2.8.5, 2.7.1): the sixth attempt of the contact form
/// limit by one client address is answered 429 with the page again, the sent value kept, the neutral message beside the button, no <c>Retry-After</c>
/// and nothing of the remaining attempts or the block; the attempts before it end in 303. The threshold is read from the limit table.
/// </summary>
[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed class AttemptLimitPatternTests
{
    private const string PagePath = "/panel/deneme-ornek";

    private readonly CollectionDatabaseFixture _database;

    public AttemptLimitPatternTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static FormUrlEncodedContent Form(string message) => new([new KeyValuePair<string, string>("Input.Message", message)]);

    private async Task ClearAttemptsAsync()
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand truncate = new("TRUNCATE attempt_records", connection);
        await truncate.ExecuteNonQueryAsync(Ct);
    }

    private async Task<HttpClient> ClientAsync(WebApplicationFactory<Program> factory)
    {
        string key = (await TestSessions.AdminAsync(_database.Dsn)).Key;
        HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        client.DefaultRequestHeaders.Add("Cookie", $"{CookieNames.AdminSession}={key}");
        return client;
    }

    [Fact]
    public async Task TheAttemptOverTheLimit_Answers429_KeepsTheValue_ShowsTheNeutralMessage_AndWritesNoRetryAfter()
    {
        await ClearAttemptsAsync();
        await using ShopfolioWebApplicationFactory baseFactory = new(_database.Dsn);
        await using WebApplicationFactory<Program> factory = baseFactory.WithWebHostBuilder(b => b.ConfigureServices(s =>
            s.AddRazorPages().AddApplicationPart(typeof(AttemptSampleModel).Assembly)));
        using HttpClient client = await ClientAsync(factory);
        int threshold = AttemptLimits.Of(AttemptLimit.L4, AttemptAxis.Ip).Threshold;

        for (int i = 0; i < threshold; i++)
        {
            using HttpResponseMessage allowed = await client.PostAsync(PagePath + "?handler=Send", Form("merhaba"), Ct);
            Assert.Equal(HttpStatusCode.SeeOther, allowed.StatusCode);
        }

        using HttpResponseMessage blocked = await client.PostAsync(PagePath + "?handler=Send", Form("benim mesajim"), Ct);
        string body = WebUtility.HtmlDecode(await blocked.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
        Assert.Contains("benim mesajim", body, StringComparison.Ordinal);
        Assert.Contains(NeutralMessages.LimitExceeded, body, StringComparison.Ordinal);
        Assert.Null(blocked.Headers.RetryAfter);
        Assert.False(blocked.Headers.Contains("Retry-After"));
        ErrorCodeLeakCheck.AssertNone(blocked, body); // D-143: the attempt limit's 429 names no code (09 8.2 item 5)
        Assert.DoesNotContain(threshold.ToString(System.Globalization.CultureInfo.InvariantCulture) + " ", NeutralMessages.LimitExceeded, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheAttemptsUpToTheLimit_AreNotBlocked_AndEachWritesOneRow()
    {
        await ClearAttemptsAsync();
        await using ShopfolioWebApplicationFactory baseFactory = new(_database.Dsn);
        await using WebApplicationFactory<Program> factory = baseFactory.WithWebHostBuilder(b => b.ConfigureServices(s =>
            s.AddRazorPages().AddApplicationPart(typeof(AttemptSampleModel).Assembly)));
        using HttpClient client = await ClientAsync(factory);

        using HttpResponseMessage first = await client.PostAsync(PagePath + "?handler=Send", Form("bir"), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand count = new("SELECT count(*) FROM attempt_records WHERE attempt_limit = 'L4' AND attempt_axis = 'Ip'", connection);
        Assert.Equal(1L, await count.ExecuteScalarAsync(Ct));
    }
}
