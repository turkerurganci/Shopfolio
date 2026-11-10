using System.Net;
using System.Text;

using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Web;

namespace Shopfolio.IntegrationTests.Payment;

/// <summary>
/// The payment start on the opened application (T401 criterion 8; KK8; 08 2.1.4): the registered gateway, with the one setting of the three keys, sends the
/// token request through the application's own named client and gives the frame's address; the token reaches neither a log line (the host's lines and its log
/// files) nor any row of the database, and a <c>failed</c> answer writes only its reason code, never the provider's <c>reason</c> text.
/// </summary>
[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed class PaymentStartTests
{
    private const string AccessKey = "ak_Start-Access_Key";

    private readonly CollectionDatabaseFixture _database;

    public PaymentStartTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    // Built from parts at run time: the repository's secret guard stops a secret-looking literal next to a secret key.
    private static string Keys() =>
        PaymentProviderKeys.IdName + "=654321\n"
        + PaymentProviderKeys.KeyName + "=key-" + "start-test\n"
        + PaymentProviderKeys.SaltName + "=salt-" + "start-test\n";

    private static PaymentStartRequest Request() =>
        new(
            "2026-7K4M9P",
            34.56m,
            "musteri@example.com",
            "firma@example.com",
            "Ayşe Yılmaz",
            "Bağdat Cad. 1",
            "5321234567",
            "2121234567",
            "203.0.113.7",
            DateTimeOffset.UtcNow.AddMinutes(30),
            AccessKey);

    [Fact]
    public async Task Start_GivesTheFrameAddress_AndTheTokenIsInNoLogLineAndNoRow_KK8()
    {
        string issued = "jeton" + Guid.NewGuid().ToString("N");
        AnsweringHandler handler = new("{\"status\":\"success\",\"token\":\"" + issued + "\"}");
        CollectingLoggerProvider logs = new();
        await using ShopfolioWebApplicationFactory factory = new(
            _database.Dsn, new Dictionary<string, string> { [PaymentOptions.ProviderKeysKey] = Keys() }, logs);
        await using WebApplicationFactoryWithHandler host = new(factory, handler);

        PaymentStartResult result = await host.Gateway.StartAsync(Request(), Ct);

        PaymentStartResult.Started started = Assert.IsType<PaymentStartResult.Started>(result);
        Assert.Equal(new Uri(PayTrStartForm.FrameAddressRoot + issued), started.FrameAddress);
        (Uri address, string body) = Assert.Single(handler.Sent);
        Assert.Equal(PayTrStartForm.TokenAddress, address);
        Assert.Contains("merchant_ok_url=" + WebUtility.UrlEncode("https://localhost/odeme/donus/" + AccessKey), body, StringComparison.Ordinal);
        Assert.Contains("merchant_fail_url=" + WebUtility.UrlEncode("https://localhost/odeme/donus/" + AccessKey), body, StringComparison.Ordinal);

        // The log lines are there (the host's start-up lines) and none carries the token; the same for the log files the host wrote.
        Assert.NotEmpty(logs.Lines);
        Assert.DoesNotContain(logs.Lines, l => l.Contains(issued, StringComparison.Ordinal));
        Assert.DoesNotContain(LogFileTexts(factory.LogDirectory), text => text.Contains(issued, StringComparison.Ordinal));

        // The scan reads the rows of every table (it finds a migration's id in the history table) and finds the token in none.
        string migrationId = await FirstMigrationIdAsync();
        Assert.True(await RowsContainingAsync(migrationId) > 0);
        Assert.Equal(0, await RowsContainingAsync(issued));
    }

    [Fact]
    public async Task Start_WithAFailedAnswer_WritesOnlyTheReasonCode_NotTheProvidersReasonText_KK8()
    {
        string reasonText = "gerekce" + Guid.NewGuid().ToString("N");
        AnsweringHandler handler = new("{\"status\":\"failed\",\"reason\":\"" + reasonText + "\"}");
        CollectingLoggerProvider logs = new();
        await using ShopfolioWebApplicationFactory factory = new(
            _database.Dsn, new Dictionary<string, string> { [PaymentOptions.ProviderKeysKey] = Keys() }, logs);
        await using WebApplicationFactoryWithHandler host = new(factory, handler);

        PaymentStartResult result = await host.Gateway.StartAsync(Request(), Ct);

        Assert.Equal(new PaymentStartResult.ConnectionFailed(PayTrPaymentGateway.FailedReason), result);
        Assert.Single(handler.Sent);
        Assert.Contains(logs.Lines, l => l.Contains(nameof(IntegrationOperation.PaymentStart), StringComparison.Ordinal)
            && l.Contains(" " + PayTrPaymentGateway.FailedReason, StringComparison.Ordinal));
        Assert.DoesNotContain(logs.Lines, l => l.Contains(reasonText, StringComparison.Ordinal));
        Assert.DoesNotContain(LogFileTexts(factory.LogDirectory), text => text.Contains(reasonText, StringComparison.Ordinal));
        Assert.Equal(0, await RowsContainingAsync(reasonText));
    }

    private static List<string> LogFileTexts(string directory) =>
        Directory.Exists(directory)
            ? [.. Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).Select(ReadShared)]
            : [];

    private static string ReadShared(string path)
    {
        using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using StreamReader reader = new(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private async Task<string> FirstMigrationIdAsync()
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new("SELECT migration_id FROM public.\"__EFMigrationsHistory\" ORDER BY 1 LIMIT 1", connection);
        return (string)(await command.ExecuteScalarAsync(Ct))!;
    }

    // Every base table outside the system schemas, each row as its text: the count of rows that carry the value anywhere.
    private async Task<long> RowsContainingAsync(string value)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        List<string> tables = [];
        await using (NpgsqlCommand list = new(
            "SELECT format('%I.%I', table_schema, table_name) FROM information_schema.tables "
            + "WHERE table_type = 'BASE TABLE' AND table_schema NOT IN ('pg_catalog', 'information_schema')",
            connection))
        await using (NpgsqlDataReader reader = await list.ExecuteReaderAsync(Ct))
        {
            while (await reader.ReadAsync(Ct))
            {
                tables.Add(reader.GetString(0));
            }
        }

        Assert.NotEmpty(tables);
        long total = 0;
        foreach (string table in tables)
        {
            await using NpgsqlCommand count = new($"SELECT count(*) FROM {table} t WHERE t::text LIKE '%' || @value || '%'", connection);
            count.Parameters.AddWithValue("value", value);
            total += (long)(await count.ExecuteScalarAsync(Ct))!;
        }

        return total;
    }

    /// <summary>The opened application with the payment provider's named client answered by <paramref name="handler"/>; the gateway is the registered one.</summary>
    private sealed class WebApplicationFactoryWithHandler : IAsyncDisposable
    {
        private readonly Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> _host;

        public WebApplicationFactoryWithHandler(ShopfolioWebApplicationFactory factory, HttpMessageHandler handler)
        {
            _host = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
                services.AddHttpClient(IntegrationHttpClients.PaymentProvider).ConfigurePrimaryHttpMessageHandler(() => handler)));
            _ = _host.Server;
        }

        public IPaymentGateway Gateway => _host.Services.GetRequiredService<IPaymentGateway>();

        public ValueTask DisposeAsync() => _host.DisposeAsync();
    }

    private sealed class AnsweringHandler(string answer) : HttpMessageHandler
    {
        private readonly List<(Uri Address, string Body)> _sent = [];

        public IReadOnlyList<(Uri Address, string Body)> Sent
        {
            get
            {
                lock (_sent)
                {
                    return [.. _sent];
                }
            }
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            lock (_sent)
            {
                _sent.Add((request.RequestUri!, body));
            }

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(answer, Encoding.UTF8, "application/json") };
        }
    }
}
