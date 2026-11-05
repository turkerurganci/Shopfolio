using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;

using DsnBuilder = Npgsql.NpgsqlConnectionStringBuilder;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The data protection ring on the opened application and a real database (T232 KK1, KK5; 09 15.5 items 1 and 3): the keys are rows of
/// <c>data_protection_keys</c> and are not encrypted, a second application instance on the same database reads what the first protected, and a
/// value whose key is no longer in the ring is invalid. Every test works on its own copy of the template database (a test deletes the keys).
/// </summary>
public sealed class DataProtectionRingTests : IAsyncLifetime
{
    private const string KeyTable = "data_protection_keys";

    private readonly TemplateDatabaseFixture _template;
    private string _databaseName = string.Empty;
    private string _dsn = string.Empty;

    public DataProtectionRingTests(TemplateDatabaseFixture template)
    {
        _template = template;
    }

    public static TheoryData<string> Purposes => [.. DataProtectionPurposes.All];

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        _databaseName = await _template.CreateDatabaseFromTemplateAsync(Ct);
        _dsn = _template.DsnFor(_databaseName);
    }

    public async ValueTask DisposeAsync()
    {
        using NpgsqlConnection pooled = new(_dsn);
        NpgsqlConnection.ClearPool(pooled);
        await _template.DropDatabaseAsync(_databaseName, CancellationToken.None);
    }

    [Fact]
    public async Task Start_WritesTheKeyAsARowOfTheKeyTable_InClear()
    {
        await using ShopfolioWebApplicationFactory factory = new(_dsn);
        using HttpClient client = factory.CreateClient();

        factory.Services.GetRequiredService<ProtectedValues>().Protect(DataProtectionPurposes.DownloadLink, "payload");

        List<string> keys = await ReadKeyXmlAsync();
        string xml = Assert.Single(keys);
        Assert.Contains("<key ", xml, StringComparison.Ordinal);
        Assert.Contains("<masterKey", xml, StringComparison.Ordinal);
        Assert.DoesNotContain("encryptedSecret", xml, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(Purposes))]
    public async Task SecondInstance_OnTheSameDatabase_ReadsWhatTheFirstProtected(string purpose)
    {
        string value;
        await using (ShopfolioWebApplicationFactory first = new(_dsn))
        {
            using HttpClient firstClient = first.CreateClient();
            value = first.Services.GetRequiredService<ProtectedValues>().Protect(purpose, "payload-" + purpose);
        }

        await using ShopfolioWebApplicationFactory second = new(_dsn);
        using HttpClient secondClient = second.CreateClient();
        ProtectedValueResult result = second.Services.GetRequiredService<ProtectedValues>().Unprotect(purpose, value);

        Assert.True(result.IsValid);
        Assert.Equal("payload-" + purpose, result.Payload);
        Assert.Equal(1, await CountKeysAsync());
    }

    [Fact]
    public async Task TwoInstancesRunningTogether_ShareOneRing()
    {
        await using ShopfolioWebApplicationFactory first = new(_dsn);
        using HttpClient firstClient = first.CreateClient();
        string value = first.Services.GetRequiredService<ProtectedValues>().Protect(DataProtectionPurposes.CheckoutCookie, "payload");

        await using ShopfolioWebApplicationFactory second = new(_dsn);
        using HttpClient secondClient = second.CreateClient();
        ProtectedValueResult result = second.Services.GetRequiredService<ProtectedValues>().Unprotect(DataProtectionPurposes.CheckoutCookie, value);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Value_WhoseKeyIsNoLongerInTheRing_IsInvalid()
    {
        string value;
        await using (ShopfolioWebApplicationFactory first = new(_dsn))
        {
            using HttpClient firstClient = first.CreateClient();
            value = first.Services.GetRequiredService<ProtectedValues>().Protect(DataProtectionPurposes.PendingRegistration, "payload");
        }

        await DeleteKeysAsync();
        await using ShopfolioWebApplicationFactory second = new(_dsn);
        using HttpClient secondClient = second.CreateClient();
        ProtectedValues values = second.Services.GetRequiredService<ProtectedValues>();

        Assert.False(values.Unprotect(DataProtectionPurposes.PendingRegistration, value).IsValid);
        Assert.True(values.Unprotect(DataProtectionPurposes.PendingRegistration, values.Protect(DataProtectionPurposes.PendingRegistration, "again")).IsValid);
    }

    [Fact]
    public async Task Start_WithADatabaseThatCannotBeReached_Opens_AndTheFileCarriesNeitherTheServerNorTheDatabaseName()
    {
        // The ring reads its keys at start; the failure is written without the data access layer's own lines, which carry the address and the name.
        DsnBuilder unreachable = new(_dsn) { Port = 1 };
        await using ShopfolioWebApplicationFactory factory = new(unreachable.ConnectionString);

        using HttpClient client = factory.CreateClient();

        string file = Assert.Single(Directory.GetFiles(factory.LogDirectory, "app-*.jsonl"));
        using FileStream stream = new(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using StreamReader reader = new(stream);
        string text = await reader.ReadToEndAsync(Ct);
        Assert.Contains("KeyRingProvider", text, StringComparison.Ordinal);
        Assert.DoesNotContain(unreachable.Database ?? "-", text, StringComparison.Ordinal);
        Assert.DoesNotContain(unreachable.Host ?? "-", text, StringComparison.Ordinal);
    }

    private async Task<int> CountKeysAsync()
    {
        await using NpgsqlConnection connection = new(_dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new($"SELECT count(*) FROM {KeyTable}", connection);
        return Convert.ToInt32(await command.ExecuteScalarAsync(Ct), System.Globalization.CultureInfo.InvariantCulture);
    }

    private async Task<List<string>> ReadKeyXmlAsync()
    {
        await using NpgsqlConnection connection = new(_dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new($"SELECT xml FROM {KeyTable} ORDER BY id", connection);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(Ct);
        List<string> xml = [];
        while (await reader.ReadAsync(Ct))
        {
            xml.Add(reader.GetString(0));
        }

        return xml;
    }

    private async Task DeleteKeysAsync()
    {
        await using NpgsqlConnection connection = new(_dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new($"DELETE FROM {KeyTable}", connection);
        await command.ExecuteNonQueryAsync(Ct);
    }
}
