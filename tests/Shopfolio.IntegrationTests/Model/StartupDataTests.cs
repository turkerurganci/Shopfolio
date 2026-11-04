using System.Reflection;
using System.Text.RegularExpressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Migrations;
using Shopfolio.IntegrationTests.Database;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the start-up data tests (T119): one database copied from the template, made by the real migration bundle.</summary>
[CollectionDefinition(StartupDataDefinition.Name)]
public sealed class StartupDataDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "StartupData";
}

/// <summary>
/// The start-up data on a real PostgreSQL (T119 criteria 2-6 and 8; 06 8.1.1, 8.4.2; D-52): the database the migrations made - an empty
/// installation - holds the 81 provinces under their plate codes, the country list, the six single rows with their start values, the 81
/// delivery provinces and the two unpublished drafts, and nothing the framework or the first start writes. The tests only read.
/// </summary>
[Collection(StartupDataDefinition.Name)]
public sealed partial class StartupDataTests
{
    private const string Separator = "|";

    private static readonly string[] TwoDrafts = ["CookiePolicy|t|t|t|t|1|t", "PrivacyNotice|t|t|t|t|1|t"];

    private readonly CollectionDatabaseFixture _collection;

    public StartupDataTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    // ---- KK1, criterion 2 and 5: provinces and delivery provinces ----

    [Fact]
    public async Task EmptyInstallation_HasThe81ProvincesUnderTheirPlateCodes()
    {
        List<string> rows = await QueryAsync("SELECT code || '|' || name FROM provinces ORDER BY code");

        Assert.Equal(81, rows.Count);
        Assert.Equal(Enumerable.Range(1, 81).Select(n => n.ToString("00", System.Globalization.CultureInfo.InvariantCulture)), rows.Select(r => r.Split(Separator)[0]));
        Assert.Contains("01|Adana", rows);
        Assert.Contains("06|Ankara", rows);
        Assert.Contains("33|Mersin", rows);
        Assert.Contains("34|İstanbul", rows);
        Assert.Contains("35|İzmir", rows);
        Assert.Contains("46|Kahramanmaraş", rows);
        Assert.Contains("63|Şanlıurfa", rows);
        Assert.Contains("81|Düzce", rows);
        Assert.Equal(81, rows.Select(r => r.Split(Separator)[1]).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public async Task EmptyInstallation_DeliversToAll81Provinces()
    {
        Assert.Equal(81L, await ScalarAsync<long>("SELECT count(*) FROM company_delivery_provinces"));
        Assert.Equal(
            81L,
            await ScalarAsync<long>("SELECT count(DISTINCT d.province_id) FROM company_delivery_provinces AS d JOIN provinces AS p ON p.id = d.province_id"));
    }

    [Fact]
    public async Task EmptyInstallation_HasTheCountryListWithTurkishNames()
    {
        List<string> rows = await QueryAsync("SELECT code || '|' || name FROM countries ORDER BY code");

        Assert.Equal(InsertStartupData.Countries.Length, rows.Count);
        Assert.Equal(249, rows.Count);
        Assert.Contains("TR|Türkiye", rows);
        Assert.Contains("CY|Kıbrıs", rows);
        Assert.Contains("DE|Almanya", rows);
        Assert.All(rows, r => Assert.Matches("^[A-Z]{2}\\|.+$", r));
    }

    // ---- KK2, criterion 3 and 4: the six single rows ----

    [Fact]
    public async Task EmptyInstallation_HasEachSingleRowOnce()
    {
        foreach (string table in new[] { "companies", "company_settings", "brandings", "channel_state_records", "heartbeat_records", "backup_status_records" })
        {
            Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM {table}"));
        }
    }

    [Fact]
    public async Task Company_StartsEmptyAtVersion1()
    {
        Assert.Equal(
            "1|true",
            await ScalarAsync<string>(
                "SELECT version || '|' || (company_type IS NULL AND legal_name IS NULL AND tax_id_number IS NULL AND mersis_number IS NULL "
                + "AND trade_registry_number IS NULL AND province_id IS NULL AND district IS NULL AND address_line IS NULL AND phone IS NULL "
                + "AND email IS NULL AND kep_address IS NULL AND chamber_name IS NULL AND business_name IS NULL "
                + "AND professional_conduct_rules IS NULL AND etbis_verification_url IS NULL)::text FROM companies"));
    }

    [Fact]
    public async Task CompanySettings_TakesTheDefaultsOf02_11_1()
    {
        foreach (InsertStartupData.SettingDefault setting in InsertStartupData.SettingDefaults)
        {
            object? value = await NullableScalarAsync($"SELECT {setting.Column} FROM company_settings");

            if (setting.Value is null)
            {
                Assert.Null(value);
            }
            else
            {
                Assert.Equal(setting.Value.Value, Convert.ToDecimal(value, System.Globalization.CultureInfo.InvariantCulture));
            }
        }
    }

    [Fact]
    public async Task CompanySettings_SwitchesOff_SectionsAtVersion1_LayoutContentFirst_MenuNamesOfTheProduct()
    {
        Assert.Equal(
            "f|f|t|1|1|1|1|1|1|1|ContentFirst|Ürünler|Hizmetlerimiz|Referanslarımız|Hakkımızda|SSS|İletişim|t",
            await ScalarAsync<string>(
                "SELECT concat_ws('|', is_bank_transfer_enabled, is_sales_temporarily_closed, bank_transfer_iban IS NULL, payment_methods_version, "
                + "shipping_fee_version, delivery_provinces_version, periods_version, product_defaults_version, return_address_version, "
                + "homepage_version, homepage_layout, menu_name_products, menu_name_offerings, menu_name_portfolio_items, menu_name_about_page, "
                + "menu_name_faq_items, menu_name_contact, (return_recipient_name IS NULL AND return_province_id IS NULL AND return_district IS NULL "
                + "AND return_address_line IS NULL AND return_phone IS NULL AND whats_app_number IS NULL)) FROM company_settings"));
    }

    [Fact]
    public async Task Branding_ChannelState_Heartbeat_BackupStatus_StartEmpty()
    {
        Assert.Equal(
            "t|t|1",
            await ScalarAsync<string>(
                "SELECT concat_ws('|', brand_name IS NULL AND logo_id IS NULL AND favicon_id IS NULL AND brand_color IS NULL, "
                + "is_platform_signature_enabled, version) FROM brandings"));
        Assert.Equal(
            "0|t",
            await ScalarAsync<string>(
                "SELECT concat_ws('|', consecutive_channel_error_count, last_succeeded_at IS NULL AND first_channel_error_at IS NULL) FROM channel_state_records"));
        Assert.True(
            await ScalarAsync<bool>(
                "SELECT last_heartbeat_at IS NULL AND nightly_job_last_succeeded_at IS NULL AND nightly_job_last_attempted_at IS NULL FROM heartbeat_records"));
        Assert.True(await ScalarAsync<bool>("SELECT last_succeeded_at IS NULL AND last_failed_at IS NULL FROM backup_status_records"));
    }

    [Fact]
    public async Task SeedRows_CarryTheMigrationsMomentAndVersion7Ids()
    {
        string[] tables = ["provinces", "countries", "company_delivery_provinces", "companies", "company_settings", "brandings", "channel_state_records", "heartbeat_records", "backup_status_records", "legal_text_versions"];
        foreach (string table in tables)
        {
            Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM {table} WHERE created_at <> @moment OR uuid_extract_version(id) <> 7", ("moment", InsertStartupData.Moment)));
        }
    }

    // ---- KK3, criterion 6: the two drafts ----

    [Fact]
    public async Task LegalTexts_AreTheTwoUnpublishedDraftsOfTheFirmEditedTexts()
    {
        List<string> rows = await QueryAsync(
            "SELECT concat_ws('|', legal_text_type, version_number IS NULL, published_at IS NULL, hosting_location IS NULL, template_version IS NULL, version, length(text) > 0) "
            + "FROM legal_text_versions ORDER BY legal_text_type");

        Assert.Equal(TwoDrafts, rows);
    }

    [Fact]
    public async Task LegalTextDrafts_UseOnlyTheClosedTextFormatSet()
    {
        string[] allowed = ["p", "br", "strong", "em", "ul", "li", "a", "h3"];
        foreach (string text in await QueryAsync("SELECT text FROM legal_text_versions"))
        {
            string[] tags = HtmlTag().Matches(text).Select(m => m.Groups[1].Value.ToLowerInvariant()).Distinct().ToArray();

            Assert.NotEmpty(tags);
            Assert.All(tags, tag => Assert.Contains(tag, allowed));
        }
    }

    // ---- KK8, criterion 8: what the migrations do not write ----

    [Fact]
    public async Task EmptyInstallation_HasNoKeyRingRowAndNoGeneratedTextVersion()
    {
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM data_protection_keys"));
        Assert.Equal(
            0L,
            await ScalarAsync<long>("SELECT count(*) FROM legal_text_versions WHERE legal_text_type IN ('PreInformationForm', 'DistanceSalesContract')"));
    }

    // ---- D-52: the defaults are the database's ----

    [Theory]
    [InlineData("companies", "version", "1")]
    [InlineData("company_settings", "is_bank_transfer_enabled", "false")]
    [InlineData("company_settings", "is_sales_temporarily_closed", "false")]
    [InlineData("company_settings", "payment_methods_version", "1")]
    [InlineData("company_settings", "shipping_fee_version", "1")]
    [InlineData("company_settings", "delivery_provinces_version", "1")]
    [InlineData("company_settings", "periods_version", "1")]
    [InlineData("company_settings", "product_defaults_version", "1")]
    [InlineData("company_settings", "return_address_version", "1")]
    [InlineData("company_settings", "homepage_version", "1")]
    [InlineData("legal_text_versions", "version", "1")]
    public async Task SettingsColumns_CarryTheDatabaseDefaultOf06(string table, string column, string expected)
    {
        Assert.Equal(
            expected,
            await ScalarAsync<string>(
                "SELECT column_default FROM information_schema.columns WHERE table_schema = 'public' AND table_name = @table AND column_name = @column",
                ("table", table),
                ("column", column)));
    }

    // ---- criterion 7: forward only ----

    [Theory]
    [InlineData(typeof(SetSettingsColumnDefaults))]
    [InlineData(typeof(InsertStartupData))]
    public void Migration_HasNoDownMethod(Type migration)
    {
        Assert.Null(migration.GetMethod("Down", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly));
    }

    // ---- helpers ----

    private async Task<List<string>> QueryAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(Ct);
        List<string> rows = [];
        while (await reader.ReadAsync(Ct))
        {
            rows.Add(reader.GetString(0));
        }

        return rows;
    }

    private async Task<T> ScalarAsync<T>(string sql, params (string Name, object Value)[] parameters)
    {
        return (T)(await NullableScalarAsync(sql, parameters))!;
    }

    private async Task<object?> NullableScalarAsync(string sql, params (string Name, object Value)[] parameters)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        foreach ((string name, object value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        object? result = await command.ExecuteScalarAsync(Ct);
        return result is DBNull ? null : result;
    }

    [GeneratedRegex(@"<\s*/?\s*([a-zA-Z][a-zA-Z0-9]*)")]
    private static partial Regex HtmlTag();
}

/// <summary>The collection of the second application test (T119): its own database, since the test changes the company's values.</summary>
[CollectionDefinition(StartupDataReapplyDefinition.Name)]
public sealed class StartupDataReapplyDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "StartupDataReapply";
}

/// <summary>
/// KK4 and criterion 7 (06 8.1.2): the start-up data applied a second time - its history row taken away so that the migrator runs it again as
/// the schema owner - changes no row count and none of the company's own values: a changed setting, a deselected delivery province, a filled
/// identity field, a renamed list row and an edited draft stay as the company left them.
/// </summary>
[Collection(StartupDataReapplyDefinition.Name)]
public sealed class StartupDataReapplyTests
{
    private static readonly string[] CountedTables =
        ["provinces", "countries", "company_delivery_provinces", "companies", "company_settings", "brandings", "channel_state_records", "heartbeat_records", "backup_status_records", "legal_text_versions", "data_protection_keys"];

    private readonly TemplateDatabaseFixture _template;
    private readonly CollectionDatabaseFixture _collection;

    public StartupDataReapplyTests(TemplateDatabaseFixture template, CollectionDatabaseFixture collection)
    {
        _template = template;
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task SecondApplication_ChangesNoRowCountAndNoValueTheCompanyChanged()
    {
        string migrationId = typeof(InsertStartupData).GetCustomAttribute<MigrationAttribute>()!.Id;
        await ExecuteAsync(
            "UPDATE company_settings SET shipping_fee = 49.90, homepage_layout = 'StoreFirst', menu_name_products = 'Mağaza', shipping_fee_version = 2; "
            + "DELETE FROM company_delivery_provinces WHERE province_id = (SELECT id FROM provinces WHERE code = '34'); "
            + "UPDATE companies SET legal_name = 'Örnek Ticaret A.Ş.', version = 2; "
            + "UPDATE countries SET name = 'Türkiye Cumhuriyeti' WHERE code = 'TR'; "
            + "UPDATE legal_text_versions SET text = '<p>Firmanın metni</p>' WHERE legal_text_type = 'PrivacyNotice'; "
            + "UPDATE brandings SET brand_name = 'Örnek';");
        string before = await SnapshotAsync();
        Assert.Equal(80L, await ScalarAsync<long>("SELECT count(*) FROM company_delivery_provinces"));

        await ExecuteAsync($"DELETE FROM \"__EFMigrationsHistory\" WHERE migration_id = '{migrationId}'");
        await MigrateAsync(_template.DsnFor(_collection.DatabaseName, _template.Users.Migrator, _template.Users.MigratorPassword, pooling: false));

        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM \"__EFMigrationsHistory\" WHERE migration_id = '{migrationId}'"));
        Assert.Equal(before, await SnapshotAsync());
        Assert.Equal(
            "49.90|StoreFirst|Mağaza|2|Örnek Ticaret A.Ş.|Türkiye Cumhuriyeti|<p>Firmanın metni</p>|Örnek|0",
            await ScalarAsync<string>(
                "SELECT concat_ws('|', s.shipping_fee, s.homepage_layout, s.menu_name_products, s.shipping_fee_version, c.legal_name, "
                + "(SELECT name FROM countries WHERE code = 'TR'), (SELECT text FROM legal_text_versions WHERE legal_text_type = 'PrivacyNotice'), "
                + "(SELECT brand_name FROM brandings), "
                + "(SELECT count(*) FROM company_delivery_provinces AS d JOIN provinces AS p ON p.id = d.province_id WHERE p.code = '34')) "
                + "FROM company_settings AS s CROSS JOIN companies AS c"));
    }

    /// <summary>The row count of every start-up data table and the whole text of the settings row: equal before and after.</summary>
    private async Task<string> SnapshotAsync()
    {
        List<string> parts = [];
        foreach (string table in CountedTables)
        {
            parts.Add($"{table}={await ScalarAsync<long>($"SELECT count(*) FROM {table}")}");
        }

        parts.Add(await ScalarAsync<string>("SELECT row_to_json(s)::text FROM company_settings AS s"));
        parts.Add(await ScalarAsync<string>("SELECT row_to_json(c)::text FROM companies AS c"));
        return string.Join("\n", parts);
    }

    private static async Task MigrateAsync(string dsn)
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, dsn);
        await using AppDbContext context = new(builder.Options);
        IMigrator migrator = ((IInfrastructure<IServiceProvider>)context).Instance.GetService(typeof(IMigrator)) as IMigrator
            ?? throw new InvalidOperationException("The migrator service is not available.");
        await migrator.MigrateAsync(null, Ct);
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<T> ScalarAsync<T>(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return (T)(await command.ExecuteScalarAsync(Ct))!;
    }
}
