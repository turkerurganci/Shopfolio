using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Backup;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.StartupDatabase;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Settings;

using DsnBuilder = Npgsql.NpgsqlConnectionStringBuilder;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The database side of the start-up check on the opened application (T205; 06 8.2.1-8.2.3; 09 18.1 item 1): an empty province or country list
/// or a missing single-row record of the start data stops the start and the failure names the record and no value; what the check leaves outside
/// (06 8.2.2, 8.2.3) does not stop it. Every test works on its own copy of the template database (the tests delete start data).
/// </summary>
public sealed class StartupDatabaseCheckTests : IAsyncLifetime
{
    private readonly TemplateDatabaseFixture _template;
    private string _databaseName = string.Empty;
    private string _dsn = string.Empty;

    public StartupDatabaseCheckTests(TemplateDatabaseFixture template)
    {
        _template = template;
    }

    /// <summary>The two lists the check asks to be filled (06 8.2.1 items 1 and 2).</summary>
    private static readonly Type[] ListTypes = [typeof(Province), typeof(Country)];

    /// <summary>The six single-row records the check asks to exist (06 8.2.1 item 3).</summary>
    private static readonly Type[] SingleRowTypes =
    [
        typeof(Company),
        typeof(CompanySettings),
        typeof(Branding),
        typeof(ChannelStateRecord),
        typeof(HeartbeatRecord),
        typeof(BackupStatusRecord),
    ];

    public static TheoryData<Type> Lists => [.. ListTypes];

    public static TheoryData<Type> SingleRows => [.. SingleRowTypes];

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
    public async Task Presence_OnTheStartData_ListsTheTwoListsAndTheSixRowsAllPresent()
    {
        await using AppDbContext context = CreateContext();

        IReadOnlyList<StartupRecordPresence> presence = await context.ReadStartupRecordPresenceAsync(Ct);

        Assert.Equal(ListTypes.Select(t => t.Name), presence.Where(p => p.IsList).Select(p => p.Record));
        Assert.Equal(SingleRowTypes.Select(t => t.Name), presence.Where(p => !p.IsList).Select(p => p.Record));
        Assert.All(presence, p => Assert.True(p.IsPresent, p.Record));
    }

    [Fact]
    public async Task Start_OnTheStartData_OpensAndWritesNoRefusal()
    {
        CollectingLoggerProvider logs = new();
        await using ShopfolioWebApplicationFactory factory = new(_dsn, loggerProvider: logs);

        using HttpClient client = factory.CreateClient();

        Assert.DoesNotContain(logs.Lines, l => l.Contains(LogEvents.StartupDatabaseChecked, StringComparison.Ordinal));
    }

    [Theory]
    [MemberData(nameof(Lists))]
    public async Task Start_WithAnEmptyList_RefusesAndNamesTheList(Type list)
    {
        await EmptyTableAsync(list);

        (Exception failure, CollectingLoggerProvider logs) = await StartExpectingRefusalAsync();

        string expected = DatabaseFailures.EmptyList(list.Name);
        Assert.Contains(expected, failure.ToString(), StringComparison.Ordinal);
        AssertRefusalLineNames(logs, expected);
        AssertNoOtherRecordNamed(failure.ToString(), list);
    }

    [Theory]
    [MemberData(nameof(SingleRows))]
    public async Task Start_WithASingleRowRecordMissing_RefusesAndNamesTheRecord(Type record)
    {
        await EmptyTableAsync(record);

        (Exception failure, CollectingLoggerProvider logs) = await StartExpectingRefusalAsync();

        string expected = DatabaseFailures.MissingRow(record.Name);
        Assert.Contains(expected, failure.ToString(), StringComparison.Ordinal);
        AssertRefusalLineNames(logs, expected);
        AssertNoOtherRecordNamed(failure.ToString(), record);
    }

    [Fact]
    public async Task Start_WithARecordMissingAndAnotherRecordHoldingAValue_NeverPrintsTheValue()
    {
        const string ValueMarker = "marker-value-that-must-not-be-printed";
        await ExecuteAsync($"UPDATE {TableOf(typeof(Company))} SET legal_name = '{ValueMarker}'");
        await EmptyTableAsync(typeof(Branding));

        (Exception failure, CollectingLoggerProvider logs) = await StartExpectingRefusalAsync();

        Assert.Contains(nameof(Branding), failure.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(ValueMarker, failure.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(logs.Lines, l => l.Contains(ValueMarker, StringComparison.Ordinal));
    }

    [Fact]
    public async Task Start_WithTheTableOfARecordMissing_RefusesAndNamesTheRecord()
    {
        // A migration that stopped before it made the table: the table is absent, not only empty (06 8.2.1: a half-done migration).
        await ExecuteAsync($"DROP TABLE {TableOf(typeof(BackupStatusRecord))} CASCADE");

        (Exception failure, _) = await StartExpectingRefusalAsync();

        Assert.Contains(nameof(BackupStatusRecord), failure.ToString(), StringComparison.Ordinal);
        AssertNoOtherRecordNamed(failure.ToString(), typeof(BackupStatusRecord));
    }

    [Fact]
    public async Task Start_WithSeveralRecordsMissing_NamesEachOfThemInOneRefusal()
    {
        await EmptyTableAsync(typeof(Branding));
        await EmptyTableAsync(typeof(Country));

        (Exception failure, CollectingLoggerProvider logs) = await StartExpectingRefusalAsync();

        AssertRefusalLineNames(logs, DatabaseFailures.MissingRow(nameof(Branding)));
        AssertRefusalLineNames(logs, DatabaseFailures.EmptyList(nameof(Country)));
        Assert.Contains(nameof(Branding), failure.ToString(), StringComparison.Ordinal);
        Assert.Contains(nameof(Country), failure.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Start_WithARecordWhoseValueIsEmptyButWhoseRowExists_Opens()
    {
        // The backup status row is made empty at setup: both moments are empty (06 8.1); the row is what the check asks for (06 8.2.1).
        Assert.Equal(
            1L,
            await ScalarAsync<long>($"SELECT count(*) FROM {TableOf(typeof(BackupStatusRecord))} WHERE last_succeeded_at IS NULL AND last_failed_at IS NULL"));
        CollectingLoggerProvider logs = new();
        await using ShopfolioWebApplicationFactory factory = new(_dsn, loggerProvider: logs);

        using HttpClient client = factory.CreateClient();

        Assert.DoesNotContain(logs.Lines, l => l.Contains("|Critical|", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Start_WithoutTheAboutPageAnnouncementDeliveryProvincesAndDrafts_Opens()
    {
        // 06 8.2.2: the about page and the announcement are born with their first save; the delivery provinces and the drafts are not checked.
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM {TableOf(typeof(AboutPage))}"));
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM {TableOf(typeof(Announcement))}"));
        await EmptyTableAsync(typeof(CompanyDeliveryProvince));
        await EmptyTableAsync(typeof(LegalTextVersion));
        CollectingLoggerProvider logs = new();
        await using ShopfolioWebApplicationFactory factory = new(_dsn, loggerProvider: logs);

        using HttpClient client = factory.CreateClient();

        Assert.DoesNotContain(logs.Lines, l => l.Contains("|Critical|", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Start_WithADatabaseFailureAndASettingMissing_RefusesOnTheSettingAndNeverAsksTheDatabase()
    {
        await EmptyTableAsync(typeof(Company));
        CollectingLoggerProvider logs = new();
        await using ShopfolioWebApplicationFactory factory = new(
            _dsn,
            new Dictionary<string, string> { [SiteOptions.DomainKey] = string.Empty },
            logs);

        Exception failure = Assert.ThrowsAny<Exception>(() => factory.CreateClient());

        Assert.Contains(SiteOptions.DomainKey, failure.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(Company), failure.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(logs.Lines, l => l.Contains(LogEvents.StartupDatabaseChecked, StringComparison.Ordinal));
    }

    [Fact]
    public async Task Start_WithADatabaseThatCannotBeReached_OpensWithOneWarningThatTheCheckDidNotRun()
    {
        DsnBuilder unreachable = new(_dsn) { Port = 1 };
        CollectingLoggerProvider logs = new();
        await using ShopfolioWebApplicationFactory factory = new(unreachable.ConnectionString, loggerProvider: logs);

        using HttpClient client = factory.CreateClient();

        string line = Assert.Single(logs.Lines, l => l.Contains(LogEvents.StartupDatabaseChecked, StringComparison.Ordinal));
        Assert.Contains("|Warning|", line, StringComparison.Ordinal);
        Assert.DoesNotContain(logs.Lines, l => l.Contains("|Critical|", StringComparison.Ordinal));
        // No line of the application's log carries the server's address or the database's name (the context's own failed-open line would).
        Assert.DoesNotContain(logs.Lines, l => l.Contains(unreachable.Host ?? "-", StringComparison.Ordinal));
        Assert.DoesNotContain(logs.Lines, l => l.Contains(unreachable.Database ?? "-", StringComparison.Ordinal));
    }

    private static void AssertRefusalLineNames(CollectingLoggerProvider logs, string failureText)
    {
        Assert.Contains(
            logs.Lines,
            l => l.Contains("|Critical|", StringComparison.Ordinal)
                && l.Contains(LogEvents.StartupDatabaseChecked, StringComparison.Ordinal)
                && l.Contains(failureText, StringComparison.Ordinal));
    }

    // KK4 and KK7: the failure names the record and nothing else of the database or the settings: no other record, no setting key, no setting value.
    private static void AssertNoOtherRecordNamed(string failureText, Type missing)
    {
        foreach (Type other in AllRecords().Where(t => t != missing))
        {
            // Whole word: "Company" is a part of the name "CompanySettings".
            Assert.DoesNotMatch($@"\b{other.Name}\b", failureText);
        }

        foreach (string settingKey in ShopfolioWebApplicationFactory.RequiredSettings.Keys)
        {
            Assert.DoesNotContain(settingKey, failureText, StringComparison.Ordinal);
        }

        foreach (string settingValue in ShopfolioWebApplicationFactory.RequiredSettings.Values)
        {
            Assert.DoesNotContain(settingValue, failureText, StringComparison.Ordinal);
        }

        Assert.DoesNotContain(LinkKeyOptions.DerivationSecretKey, failureText, StringComparison.Ordinal);
    }

    private static IEnumerable<Type> AllRecords() => ListTypes.Concat(SingleRowTypes);

    private async Task<(Exception Failure, CollectingLoggerProvider Logs)> StartExpectingRefusalAsync()
    {
        CollectingLoggerProvider logs = new();
        await using ShopfolioWebApplicationFactory factory = new(_dsn, loggerProvider: logs);
        Exception failure = Assert.ThrowsAny<Exception>(() => factory.CreateClient());
        return (failure, logs);
    }

    private AppDbContext CreateContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _dsn);
        return new AppDbContext(builder.Options);
    }

    // The table name is the model's own (09 10.1), so no table name is written by hand in a test.
    private string TableOf(Type entity)
    {
        using AppDbContext context = CreateContext();
        string name = context.Model.FindEntityType(entity)?.GetTableName() ?? throw new InvalidOperationException(entity.Name + " is not in the model.");
        return "\"" + name + "\"";
    }

    // Deletes the rows without the foreign key checks (the test administrator may set the replication role), so removing one record never
    // removes another one with it (a truncate cascade would).
    private Task EmptyTableAsync(Type entity) => ExecuteAsync($"SET session_replication_role = replica; DELETE FROM {TableOf(entity)}");

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<T> ScalarAsync<T>(string sql)
    {
        await using NpgsqlConnection connection = new(_dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return (T)(await command.ExecuteScalarAsync(Ct))!;
    }
}
