using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the start-up list, company and legal text tests (T103): one database copied from the template, made by the real migration.</summary>
[CollectionDefinition(Name)]
public sealed class SettingsTablesDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "SettingsTables";
}

/// <summary>
/// The structure of the start-up lists, the company, its settings and the legal text versions on a real PostgreSQL (T103 criteria 1-7;
/// 06 4.1-4.4, 4.56; 09 10.1, 10.6, 10.8): the migration made the six tables, the database itself refuses what the rules refuse, the permanent rows
/// are never deleted through the context and the migration history table is closed to the application and read-only users.
/// </summary>
[Collection(SettingsTablesDefinition.Name)]
public sealed class SettingsTablesTests : IAsyncLifetime
{
    private const string CheckViolation = PostgresErrorCodes.CheckViolation;
    private const string UniqueViolation = PostgresErrorCodes.UniqueViolation;

    private static readonly string[] SixTables = ["provinces", "countries", "companies", "company_settings", "company_delivery_provinces", "legal_text_versions"];

    private static readonly DateTimeOffset Now = new(2026, 11, 4, 9, 0, 0, TimeSpan.Zero);

    private readonly TemplateDatabaseFixture _template;
    private readonly CollectionDatabaseFixture _collection;

    public SettingsTablesTests(TemplateDatabaseFixture template, CollectionDatabaseFixture collection)
    {
        _template = template;
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE company_delivery_provinces, companies, company_settings, legal_text_versions, provinces, countries CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Migration_MakesTheSixTables()
    {
        List<string> tables = await QueryAsync(
            _collection.Dsn,
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name = ANY(@names) ORDER BY 1",
            ("names", SixTables));

        Assert.Equal(["companies", "company_delivery_provinces", "company_settings", "countries", "legal_text_versions", "provinces"], tables);
    }

    [Fact]
    public async Task SecondCompanyRow_IsRefusedByTheSingleRowIndex()
    {
        await SaveAsync(c => c.Companies.Add(Company.Create(Now)));

        await AssertViolationAsync(c => c.Companies.Add(Company.Create(Now)), UniqueViolation, "ix_companies_single_row");
    }

    [Fact]
    public async Task SecondCompanySettingsRow_IsRefusedByTheSingleRowIndex()
    {
        await SaveAsync(c => c.CompanySettings.Add(ValidSettings()));

        await AssertViolationAsync(c => c.CompanySettings.Add(ValidSettings()), UniqueViolation, "ix_company_settings_single_row");
    }

    [Fact]
    public async Task ValidSettingsRow_IsAccepted()
    {
        await SaveAsync(c => c.CompanySettings.Add(ValidSettings()));

        Assert.Equal(1, await ScalarAsync<long>("SELECT count(*) FROM company_settings"));
    }

    [Fact]
    public async Task OpenBankTransferWithoutAnIban_IsRefused()
    {
        CompanySettings settings = ValidSettings();
        settings.IsBankTransferEnabled = true;

        await AssertViolationAsync(c => c.CompanySettings.Add(settings), CheckViolation, "ck_company_settings_bank_transfer_iban_required");
    }

    [Fact]
    public async Task HalfFilledReturnAddress_IsRefused()
    {
        CompanySettings settings = ValidSettings();
        settings.ReturnRecipientName = "Ayşe Yılmaz";

        await AssertViolationAsync(c => c.CompanySettings.Add(settings), CheckViolation, "ck_company_settings_return_address_all_or_none");
    }

    [Fact]
    public async Task NegativeAmount_IsRefused()
    {
        CompanySettings settings = ValidSettings();
        settings.ShippingFee = -1m;

        await AssertViolationAsync(c => c.CompanySettings.Add(settings), CheckViolation, "ck_company_settings_amounts_not_negative");
    }

    [Fact]
    public async Task PhoneWithNineDigits_IsRefused()
    {
        Company company = Company.Create(Now);
        company.Phone = "532123456";

        await AssertViolationAsync(c => c.Companies.Add(company), CheckViolation, "ck_companies_phone_ten_digits");
    }

    [Fact]
    public async Task ProvinceCodeWithThreeDigits_IsRefused()
    {
        await AssertViolationAsync(c => c.Provinces.Add(Province.Create("123", "Deneme", Now)), CheckViolation, "ck_provinces_code_two_digits");
    }

    [Fact]
    public async Task CountryCodeInLowerCase_IsRefused()
    {
        await AssertViolationAsync(c => c.Countries.Add(Country.Create("tr", "Türkiye", Now)), CheckViolation, "ck_countries_code_two_capital_letters");
    }

    [Fact]
    public async Task SameProvinceTwiceInTheDeliveryList_IsRefused()
    {
        Province province = Province.Create("06", "Ankara", Now);
        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.CompanyDeliveryProvinces.Add(CompanyDeliveryProvince.Create(province.Id, Now));
        });

        await AssertViolationAsync(c => c.CompanyDeliveryProvinces.Add(CompanyDeliveryProvince.Create(province.Id, Now)), UniqueViolation, "ix_company_delivery_provinces_province_id");
    }

    [Fact]
    public async Task LegalTextVersion_SameTypeAndNumber_IsRefused()
    {
        await SaveAsync(c => c.LegalTextVersions.Add(LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, 1, Now, "metin", Now).WithHosting()));

        await AssertViolationAsync(
            c => c.LegalTextVersions.Add(LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, 1, Now, "metin", Now).WithHosting()),
            UniqueViolation,
            "ix_legal_text_versions_legal_text_type_version_number");
    }

    [Fact]
    public async Task LegalTextVersion_PublishedMomentWithoutANumber_IsRefused()
    {
        LegalTextVersion version = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, 1, Now, "metin", Now).WithHosting();
        version.VersionNumber = null;

        await AssertViolationAsync(c => c.LegalTextVersions.Add(version), CheckViolation, "ck_legal_text_versions_number_and_published_at_together");
    }

    [Fact]
    public async Task LegalTextVersion_DraftOfAGeneratedText_IsRefused()
    {
        await AssertViolationAsync(
            c => c.LegalTextVersions.Add(LegalTextVersion.CreateDraft(LegalTextType.PreInformationForm, "metin", Now)),
            CheckViolation,
            "ck_legal_text_versions_draft_only_for_firm_edited_text");
    }

    [Fact]
    public async Task LegalTextVersion_SecondDraftOfTheSameType_IsRefused()
    {
        await SaveAsync(c => c.LegalTextVersions.Add(LegalTextVersion.CreateDraft(LegalTextType.CookiePolicy, "bir", Now)));

        await AssertViolationAsync(c => c.LegalTextVersions.Add(LegalTextVersion.CreateDraft(LegalTextType.CookiePolicy, "iki", Now)), UniqueViolation, "ix_legal_text_versions_legal_text_type");
    }

    [Fact]
    public async Task LegalTextVersion_TemplateVersionOnAFirmEditedText_IsRefused()
    {
        LegalTextVersion version = LegalTextVersion.CreatePublished(LegalTextType.CookiePolicy, 1, Now, "metin", Now);
        version.TemplateVersion = "t1";

        await AssertViolationAsync(c => c.LegalTextVersions.Add(version), CheckViolation, "ck_legal_text_versions_template_version_only_generated");
    }

    [Fact]
    public async Task LegalTextVersion_TemplateVersionOnAGeneratedPublishedText_IsAccepted()
    {
        LegalTextVersion version = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, 1, Now, "metin", Now);
        version.TemplateVersion = "t1";

        await SaveAsync(c => c.LegalTextVersions.Add(version));

        Assert.Equal(1, await ScalarAsync<long>("SELECT count(*) FROM legal_text_versions"));
    }

    [Fact]
    public async Task LegalTextVersion_PublishedPrivacyNoticeWithoutHostingLocation_IsRefused()
    {
        await AssertViolationAsync(
            c => c.LegalTextVersions.Add(LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, 1, Now, "metin", Now)),
            CheckViolation,
            "ck_legal_text_versions_hosting_location_only_on_privacy_notice");
    }

    [Fact]
    public async Task ProvinceInUse_CannotBeDeletedFromTheDatabase()
    {
        Province province = Province.Create("34", "İstanbul", Now);
        Company company = Company.Create(Now);
        company.ProvinceId = province.Id;
        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.Companies.Add(company);
        });

        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM provinces WHERE id = '{province.Id}'"));

        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, error.SqlState);
        Assert.Equal("fk_companies_provinces_province_id", error.ConstraintName);
    }

    [Fact]
    public async Task PermanentRows_AreNeverDeletedThroughTheContext()
    {
        Province province = Province.Create("06", "Ankara", Now);
        Country country = Country.Create("TR", "Türkiye", Now);
        Company company = Company.Create(Now);
        CompanySettings settings = ValidSettings();
        LegalTextVersion published = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, 1, Now, "metin", Now).WithHosting();
        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.Countries.Add(country);
            c.Companies.Add(company);
            c.CompanySettings.Add(settings);
            c.LegalTextVersions.Add(published);
        });

        await AssertDeleteRefusedAsync(c => c.Provinces.Remove(c.Provinces.Single(p => p.Id == province.Id)));
        await AssertDeleteRefusedAsync(c => c.Countries.Remove(c.Countries.Single(x => x.Id == country.Id)));
        await AssertDeleteRefusedAsync(c => c.Companies.Remove(c.Companies.Single(x => x.Id == company.Id)));
        await AssertDeleteRefusedAsync(c => c.CompanySettings.Remove(c.CompanySettings.Single(x => x.Id == settings.Id)));
        await AssertDeleteRefusedAsync(c => c.LegalTextVersions.Remove(c.LegalTextVersions.Single(x => x.Id == published.Id)));

        Assert.Equal(5L, await ScalarAsync<long>("SELECT (SELECT count(*) FROM provinces) + (SELECT count(*) FROM countries) + (SELECT count(*) FROM companies) + (SELECT count(*) FROM company_settings) + (SELECT count(*) FROM legal_text_versions)"));
    }

    [Fact]
    public async Task PublishedLegalTextVersion_IsNeverUpdated_ButADraftIs()
    {
        LegalTextVersion published = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, 1, Now, "metin", Now).WithHosting();
        LegalTextVersion draft = LegalTextVersion.CreateDraft(LegalTextType.CookiePolicy, "taslak", Now);
        await SaveAsync(c =>
        {
            c.LegalTextVersions.Add(published);
            c.LegalTextVersions.Add(draft);
        });

        await using (AppDbContext context = NewContext())
        {
            LegalTextVersion loaded = await context.LegalTextVersions.SingleAsync(l => l.Id == published.Id, Ct);
            loaded.Text = "değişti";
            await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));
        }

        await using (AppDbContext context = NewContext())
        {
            LegalTextVersion loaded = await context.LegalTextVersions.SingleAsync(l => l.Id == draft.Id, Ct);
            loaded.Text = "yeni taslak";
            await context.SaveChangesAsync(Ct);
        }

        Assert.Equal("metin", await ScalarAsync<string>($"SELECT text FROM legal_text_versions WHERE id = '{published.Id}'"));
        Assert.Equal("yeni taslak", await ScalarAsync<string>($"SELECT text FROM legal_text_versions WHERE id = '{draft.Id}'"));
    }

    [Fact]
    public async Task DeliveryProvinceRow_CanBeDeleted()
    {
        Province province = Province.Create("06", "Ankara", Now);
        CompanyDeliveryProvince row = CompanyDeliveryProvince.Create(province.Id, Now);
        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.CompanyDeliveryProvinces.Add(row);
        });

        await SaveAsync(c => c.CompanyDeliveryProvinces.Remove(c.CompanyDeliveryProvinces.Single(x => x.Id == row.Id)));

        Assert.Equal(0, await ScalarAsync<long>("SELECT count(*) FROM company_delivery_provinces"));
    }

    [Fact]
    public async Task MigrationHistoryTable_IsClosedToTheApplicationAndReadOnlyUsers()
    {
        string appDsn = _template.DsnFor(_collection.DatabaseName, _template.Users.App, _template.Users.AppPassword, pooling: false);
        string readerDsn = _template.DsnFor(_collection.DatabaseName, _template.Users.Reader, _template.Users.ReaderPassword, pooling: false);

        foreach (string dsn in new[] { appDsn, readerDsn })
        {
            PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => QueryAsync(dsn, "SELECT count(*) FROM \"__EFMigrationsHistory\""));
            Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, error.SqlState);
        }
    }

    private static CompanySettings ValidSettings()
    {
        CompanySettings settings = CompanySettings.Create(Now);
        settings.BankTransferPaymentWindow = 1;
        settings.DownloadAllowance = 1;
        settings.HomepageLayout = HomepageLayout.StoreFirst;
        settings.MenuNameProducts = "Ürünler";
        settings.MenuNameOfferings = "Hizmetler";
        settings.MenuNamePortfolioItems = "Çalışmalar";
        settings.MenuNameAboutPage = "Hakkımızda";
        settings.MenuNameFaqItems = "Sıkça Sorulanlar";
        settings.MenuNameContact = "İletişim";
        return settings;
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task SaveAsync(Action<AppDbContext> change)
    {
        await using AppDbContext context = NewContext();
        change(context);
        await context.SaveChangesAsync(Ct);
    }

    private async Task AssertViolationAsync(Action<AppDbContext> change, string sqlState, string constraint)
    {
        await using AppDbContext context = NewContext();
        change(context);

        DbUpdateException error = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(Ct));

        PostgresException inner = Assert.IsType<PostgresException>(error.InnerException);
        Assert.Equal(sqlState, inner.SqlState);
        Assert.Equal(constraint, inner.ConstraintName);
    }

    private async Task AssertDeleteRefusedAsync(Action<AppDbContext> change)
    {
        await using AppDbContext context = NewContext();
        change(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));
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

    private static async Task<List<string>> QueryAsync(string dsn, string sql, params (string Name, object Value)[] parameters)
    {
        await using NpgsqlConnection connection = new(dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        foreach ((string name, object value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        List<string> rows = [];
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(Ct);
        while (await reader.ReadAsync(Ct))
        {
            rows.Add(Convert.ToString(reader.GetValue(0), System.Globalization.CultureInfo.InvariantCulture)!);
        }

        return rows;
    }
}

internal static class LegalTextVersionTestExtensions
{
    /// <summary>A published privacy notice carries its hosting location (06 4.56).</summary>
    public static LegalTextVersion WithHosting(this LegalTextVersion version)
    {
        version.HostingLocation = "Türkiye";
        return version;
    }
}
