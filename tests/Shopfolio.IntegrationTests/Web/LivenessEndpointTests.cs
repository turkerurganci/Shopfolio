using System.Diagnostics;
using System.Net;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Npgsql;

using Shopfolio.Endpoints;
using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.Database;

using Abort = System.Threading.CancellationTokenSource;
using DsnBuilder = Npgsql.NpgsqlConnectionStringBuilder;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>UC-81, the liveness endpoint (07 4.33.1; 09 10.7 item 6, 19.2 item 4).</summary>
[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed class LivenessEndpointTests
{
    private static readonly Uri _livenessUri = new(LivenessEndpoint.Path, UriKind.Relative);

    private readonly CollectionDatabaseFixture _database;

    public LivenessEndpointTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    [Fact]
    public async Task Get_WithHealthyDatabase_Returns200WithEmptyBody()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        await using ShopfolioWebApplicationFactory factory = new(_database.Dsn);
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(_livenessUri, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync(ct));
        Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task Get_WhenDatabaseIsUnreachable_Returns503WithEmptyBody()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        DsnBuilder unreachable = new(_database.Dsn) { Port = 1 };
        await using ShopfolioWebApplicationFactory factory = new(unreachable.ConnectionString);
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(_livenessUri, ct);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync(ct));
    }

    [Fact]
    public async Task Get_Answers_WithoutVersionOrDetailInHeadersOrBody()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        DsnBuilder unreachable = new(_database.Dsn) { Port = 1 };
        await using ShopfolioWebApplicationFactory healthy = new(_database.Dsn);
        await using ShopfolioWebApplicationFactory broken = new(unreachable.ConnectionString);
        using HttpClient healthyClient = healthy.CreateClient();
        using HttpClient brokenClient = broken.CreateClient();

        foreach (HttpClient client in new[] { healthyClient, brokenClient })
        {
            using HttpResponseMessage response = await client.GetAsync(_livenessUri, ct);

            Assert.Equal(0, response.Content.Headers.ContentLength ?? 0);
            Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync(ct));
            // No "Server" assertion here: TestServer writes no such header, so a check would pass without testing anything.
            // The version-free "Server: Kestrel" of a real server is checked in the validation run against Kestrel (T01 report).
            Assert.False(response.Headers.Contains("X-Powered-By"));
            Assert.False(response.Headers.Contains("X-AspNet-Version"));
        }
    }

    [Fact]
    public async Task Get_WithoutCookies_Returns200AndSetsNone()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        await using ShopfolioWebApplicationFactory factory = new(_database.Dsn);
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(_livenessUri, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(response.Headers.Contains("Set-Cookie"));
    }

    [Fact]
    public async Task Get_WhenDatabaseDoesNotAnswerInTime_Returns503AndWritesOneTimeoutLine()
    {
        // The only pooled connection is held, so the probe's connection open waits until its limit (the setting) runs out.
        CancellationToken ct = TestContext.Current.CancellationToken;
        DsnBuilder singleConnection = new(_database.Dsn) { MaxPoolSize = 1, Pooling = true };
        CollectingLoggerProvider logs = new();
        Dictionary<string, string> settings = new() { [DatabaseOptions.LivenessTimeoutKey] = "1" };
        await using ShopfolioWebApplicationFactory factory = new(singleConnection.ConnectionString, settings, logs);
        using HttpClient client = factory.CreateClient();
        NpgsqlDataSource dataSource = factory.Services.GetRequiredService<NpgsqlDataSource>();
        await using NpgsqlConnection held = await dataSource.OpenConnectionAsync(ct);

        using HttpResponseMessage response = await client.GetAsync(_livenessUri, ct);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync(ct));
        string line = Assert.Single(logs.Lines, l => l.Contains("DbTimeout", StringComparison.Ordinal));
        Assert.Contains("Warning", line, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Get_WhenTheConnectionOpenIsSlow_Returns503WithinTheLimit()
    {
        // The server accepts the connection and never answers: the physical open is slow, which Npgsql's token does not cut.
        CancellationToken ct = TestContext.Current.CancellationToken;
        DsnBuilder real = new(_database.Dsn);
        await using StallingTcpProxy proxy = new(UpstreamHost(real), real.Port, stalled: true);
        DsnBuilder slow = new(_database.Dsn) { Host = "127.0.0.1", Port = proxy.Port, Pooling = false };

        CollectingLoggerProvider logs = await AssertAnswers503WithinTheLimitAsync(slow.ConnectionString, warmPool: false, stallAfterWarmUp: null, ct);
        Assert.Single(logs.Lines, l => l.Contains("DbTimeout", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Get_WhenTheCommandIsSlow_Returns503WithinTheLimit()
    {
        // The pool holds a good connection; then the server stops answering, so the open is instant and SELECT 1 hangs.
        CancellationToken ct = TestContext.Current.CancellationToken;
        DsnBuilder real = new(_database.Dsn);
        await using StallingTcpProxy proxy = new(UpstreamHost(real), real.Port, stalled: false);
        DsnBuilder viaProxy = new(_database.Dsn) { Host = "127.0.0.1", Port = proxy.Port, Pooling = true, MaxPoolSize = 1 };

        CollectingLoggerProvider logs = await AssertAnswers503WithinTheLimitAsync(viaProxy.ConnectionString, warmPool: true, stallAfterWarmUp: proxy, ct);
        Assert.Single(logs.Lines, l => l.Contains("DbTimeout", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Get_WhenTheServerIsUnreachable_Returns503WithinTheLimit()
    {
        // 192.0.2.1 (TEST-NET-1) is never routed: the connect attempt hangs or fails; either way the limit holds.
        CancellationToken ct = TestContext.Current.CancellationToken;
        DsnBuilder unreachable = new(_database.Dsn) { Host = "192.0.2.1", Pooling = false };

        CollectingLoggerProvider logs = await AssertAnswers503WithinTheLimitAsync(unreachable.ConnectionString, warmPool: false, stallAfterWarmUp: null, ct);
        Assert.DoesNotContain(logs.Lines, l => l.Contains("192.0.2.1", StringComparison.Ordinal));
        // 09 10.7 item 7: an unreachable server writes at most one DbTimeout line (a fast refusal is a failure, not a timeout, and writes none).
        Assert.True(logs.Lines.Count(l => l.Contains("DbTimeout", StringComparison.Ordinal)) <= 1);
    }

    private static string UpstreamHost(DsnBuilder dsn) =>
        dsn.Host ?? throw new InvalidOperationException("The test database connection string has no host.");

    private static async Task<CollectingLoggerProvider> AssertAnswers503WithinTheLimitAsync(
        string connectionString, bool warmPool, StallingTcpProxy? stallAfterWarmUp, CancellationToken ct)
    {
        // Limit 1 s; the margin allows for a slow CI host. The connection string's own Timeout (15 s) must not decide.
        CollectingLoggerProvider logs = new();
        Dictionary<string, string> settings = new() { [DatabaseOptions.LivenessTimeoutKey] = "1" };
        await using ShopfolioWebApplicationFactory factory = new(connectionString, settings, logs);
        using HttpClient client = factory.CreateClient();
        if (warmPool)
        {
            NpgsqlDataSource dataSource = factory.Services.GetRequiredService<NpgsqlDataSource>();
            await using NpgsqlConnection warm = await dataSource.OpenConnectionAsync(ct);
        }

        stallAfterWarmUp?.Stall();
        Stopwatch watch = Stopwatch.StartNew();

        using HttpResponseMessage response = await client.GetAsync(_livenessUri, ct);

        watch.Stop();
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync(ct));
        Assert.True(watch.Elapsed < TimeSpan.FromSeconds(3), $"The answer took {watch.Elapsed.TotalSeconds:F1} s; the limit is 1 s.");
        return logs;
    }

    [Fact]
    public async Task Get_WhenDatabaseFailsBeforeTheLimit_Returns503AndWritesNoTimeoutLine()
    {
        // A database that does not exist fails at once: a failure is not a timeout (09 10.7 item 6).
        CancellationToken ct = TestContext.Current.CancellationToken;
        DsnBuilder missing = new(_database.Dsn) { Database = "shopfolio_missing_database" };
        CollectingLoggerProvider logs = new();
        await using ShopfolioWebApplicationFactory factory = new(missing.ConnectionString, loggerProvider: logs);
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(_livenessUri, ct);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.DoesNotContain(logs.Lines, l => l.Contains("DbTimeout", StringComparison.Ordinal));
    }

    [Fact]
    public async Task CheckDatabase_WhenTheRequestIsCancelled_LetsTheCancellationPassUpAndWritesNoTimeoutLine()
    {
        // 09 10.7 item 7: a cancelled request is not a failure; the cancellation passes up (false is not returned) and nothing is logged.
        CancellationToken ct = TestContext.Current.CancellationToken;
        CollectingLoggerProvider logs = new();
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(logs));
        ILogger logger = loggerFactory.CreateLogger("LivenessProbeTest");
        await using NpgsqlDataSource dataSource = NpgsqlDataSource.Create(_database.Dsn);
        using Abort aborted = Abort.CreateLinkedTokenSource(ct);
        await aborted.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => LivenessProbe.CheckDatabaseAsync(dataSource, TimeSpan.FromSeconds(2), logger, aborted.Token));

        Assert.DoesNotContain(logs.Lines, l => l.Contains("DbTimeout", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("20")]
    [InlineData("300")]
    public void Start_WithInvalidLivenessTimeout_StopsTheApplicationAndNamesTheSetting(string seconds)
    {
        Dictionary<string, string> settings = new() { [DatabaseOptions.LivenessTimeoutKey] = seconds };
        using ShopfolioWebApplicationFactory factory = new(_database.Dsn, settings);

        Exception failure = Assert.ThrowsAny<Exception>(() => factory.CreateClient());

        Assert.Contains(DatabaseOptions.LivenessTimeoutKey, failure.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void Start_WithoutConnectionString_StopsTheApplicationAndNamesTheSetting()
    {
        using ShopfolioWebApplicationFactory factory = new(string.Empty);

        Exception failure = Assert.ThrowsAny<Exception>(() => factory.CreateClient());

        Assert.Contains(DatabaseOptions.ConnectionStringKey, failure.ToString(), StringComparison.Ordinal);
    }
}
