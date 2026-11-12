using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.EmailComposition;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

using FieldRejection = Shopfolio.Modules.Settings.Contracts.FieldRejection;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the home page, social links and identity trail tests (T535): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(HomepageAndIdentityTrailDefinition.Name)]
public sealed class HomepageAndIdentityTrailDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "HomepageAndIdentityTrail";
}

/// <summary>
/// The home page and menu save, the social links save, the identity trail and the generated legal text trail through the real Settings and CorporateContent modules on a real PostgreSQL
/// (T535; 02 3.27.12, 3.28.4, 10.3.1 (1) (6); 07 UC-162, UC-164): the social links are written in the section's transaction and reach the section's trail row with their old and new address; a
/// refused value writes nothing; a save that changes nothing writes no row and does not step the version; a stale version is a conflict that writes nothing; the identity save writes one
/// row of the changed fields, and the versions a changed identity causes each get their publication row.
/// </summary>
[Collection(HomepageAndIdentityTrailDefinition.Name)]
public sealed class HomepageAndIdentityTrailTests : IAsyncLifetime
{
    private const string HomepageDisplayName = "Ana sayfa ve menü";

    private const string InstagramUrl = "https://www.instagram.com/firma";

    private const string FacebookUrl = "https://www.facebook.com/firma";

    private static readonly DateTimeOffset Now = new(2026, 11, 12, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    private Guid _provinceId;

    private Admin _admin = null!;

    public HomepageAndIdentityTrailTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE provinces, companies, company_settings, legal_text_versions, social_links, countries, admins, audit_logs CASCADE");
        Province province = Province.Create("06", "Ankara", Now);
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
        _admin = Admin.Create("Ali Yılmaz", "ali@example.com", "hash", Now);
        await using (AppDbContext context = NewContext())
        {
            context.Provinces.Add(province);
            context.Companies.Add(Company.Create(Now));
            context.CompanySettings.Add(settings);
            context.Admins.Add(_admin);
            await context.SaveChangesAsync(Ct);
        }

        _provinceId = province.Id;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- the home page and menu save: the section's trail row carries the changed values and the social links ----

    [Fact]
    public async Task SaveHomepage_WritesOneTrailRow_WithTheChangedValuesAndTheSocialLinksWithTheirOldAndNewAddress()
    {
        HomepageInput input = Input(HomepageLayout.ContentFirst, products: "Mağaza", whatsApp: "0312 123 45 67");

        HomepageOutcome outcome = await SaveHomepageAsync(input, new Dictionary<string, string?> { ["Instagram"] = InstagramUrl, ["Facebook"] = FacebookUrl }, 1);

        Assert.True(outcome.IsSaved);
        List<AuditLog> rows = await ReadTrailAsync("UpdateSettings");
        AuditLog row = Assert.Single(rows);
        Assert.Equal(AuditArea.Settings, row.AuditArea);
        Assert.Equal("CompanySettings", row.TargetRecordType);
        Assert.Equal(_admin.Id, row.AdminId);
        Assert.Equal(HomepageDisplayName, row.Details!.DisplayName);
        Assert.Equal(
            [
                new AuditValueChange("HomepageLayout", "StoreFirst", "ContentFirst"),
                new AuditValueChange("MenuNameProducts", "Ürünler", "Mağaza"),
                new AuditValueChange("WhatsAppNumber", null, "3121234567"),
                new AuditValueChange("SocialLink.Instagram", null, InstagramUrl),
                new AuditValueChange("SocialLink.Facebook", null, FacebookUrl),
            ],
            row.Details.Changes);
        Assert.Equal(2, await ScalarAsync<int>("SELECT homepage_version FROM company_settings"));
        Assert.Equal("3121234567", await ScalarAsync<string>("SELECT whats_app_number FROM company_settings"));
        Assert.Equal(2, await ScalarAsync<long>("SELECT count(*) FROM social_links"));
    }

    [Fact]
    public async Task SaveHomepage_WithOnlyASocialLinkChanged_StepsTheVersion_AndTheRowCarriesOnlyTheLink()
    {
        await SaveHomepageAsync(Input(), new Dictionary<string, string?> { ["Instagram"] = InstagramUrl }, 1);

        HomepageOutcome outcome = await SaveHomepageAsync(Input(), new Dictionary<string, string?> { ["Instagram"] = FacebookUrl }, 2);

        Assert.True(outcome.IsSaved);
        List<AuditLog> rows = await ReadTrailAsync("UpdateSettings");
        Assert.Equal(2, rows.Count);
        Assert.Equal(new AuditValueChange("SocialLink.Instagram", InstagramUrl, FacebookUrl), Assert.Single(rows[1].Details!.Changes));
        Assert.Equal(3, await ScalarAsync<int>("SELECT homepage_version FROM company_settings"));
    }

    [Fact]
    public async Task SaveHomepage_WithNothingChanged_WritesNoRow_AndKeepsTheVersion()
    {
        HomepageOutcome outcome = await SaveHomepageAsync(Input(), new Dictionary<string, string?>(), 1);

        Assert.True(outcome.IsSaved);
        Assert.Empty(await ReadTrailAsync("UpdateSettings"));
        Assert.Equal(1, await ScalarAsync<int>("SELECT homepage_version FROM company_settings"));
    }

    [Fact]
    public async Task SaveHomepage_WithAStaleVersion_IsAConflict_AndWritesNeitherTheSettingsNorTheLinksNorARow()
    {
        await SaveHomepageAsync(Input(HomepageLayout.ContentFirst), new Dictionary<string, string?>(), 1);

        HomepageOutcome outcome = await SaveHomepageAsync(Input(HomepageLayout.StoreFirst, products: "Başka"), new Dictionary<string, string?> { ["Instagram"] = InstagramUrl }, 1);

        Assert.False(outcome.IsSaved);
        Assert.True(outcome.Settings!.IsVersionConflict);
        Assert.Single(await ReadTrailAsync("UpdateSettings"));
        Assert.Equal("ContentFirst", await ScalarAsync<string>("SELECT homepage_layout FROM company_settings"));
        Assert.Equal("Ürünler", await ScalarAsync<string>("SELECT menu_name_products FROM company_settings"));
        Assert.Equal(0, await ScalarAsync<long>("SELECT count(*) FROM social_links"));
    }

    [Theory]
    [InlineData("MenuNameProducts")]
    [InlineData("MenuNameOfferings")]
    [InlineData("MenuNamePortfolioItems")]
    [InlineData("MenuNameAboutPage")]
    [InlineData("MenuNameFaqItems")]
    [InlineData("MenuNameContact")]
    public async Task SaveHomepage_WithAnEmptyMenuName_RefusesTheFieldRequired_AndWritesNothing(string field)
    {
        HomepageInput input = field switch
        {
            "MenuNameProducts" => Input(products: "   "),
            "MenuNameOfferings" => Input(offerings: "   "),
            "MenuNamePortfolioItems" => Input(portfolio: "   "),
            "MenuNameAboutPage" => Input(about: "   "),
            "MenuNameFaqItems" => Input(faq: "   "),
            _ => Input(contact: "   "),
        };

        HomepageOutcome outcome = await SaveHomepageAsync(input, new Dictionary<string, string?> { ["Instagram"] = InstagramUrl }, 1);

        Assert.False(outcome.IsSaved);
        FieldRejection rejection = Assert.Single(outcome.Settings!.Rejections);
        Assert.Equal(field, rejection.Field);
        Assert.Equal(FieldCodes.FieldRequired, rejection.Code);
        Assert.Empty(await ReadTrailAsync("UpdateSettings"));
        Assert.Equal(0, await ScalarAsync<long>("SELECT count(*) FROM social_links"));
        Assert.Equal(1, await ScalarAsync<int>("SELECT homepage_version FROM company_settings"));
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("abcdefghij")]
    [InlineData("03121234567890")]
    public async Task SaveHomepage_WithAWhatsAppNumberThatIsNotATelephoneNumber_RefusesInvalidPhone(string typed)
    {
        HomepageOutcome outcome = await SaveHomepageAsync(Input(whatsApp: typed), new Dictionary<string, string?>(), 1);

        Assert.False(outcome.IsSaved);
        FieldRejection rejection = Assert.Single(outcome.Settings!.Rejections);
        Assert.Equal("WhatsAppNumber", rejection.Field);
        Assert.Equal(FieldCodes.InvalidPhone, rejection.Code);
        Assert.Null(await ScalarOrNullAsync("SELECT whats_app_number FROM company_settings"));
    }

    [Fact]
    public async Task SaveHomepage_WithAnEmptiedWhatsAppNumber_StoresNothing_AndTheRowShowsTheOldNumber()
    {
        await SaveHomepageAsync(Input(whatsApp: "3121234567"), new Dictionary<string, string?>(), 1);

        HomepageOutcome outcome = await SaveHomepageAsync(Input(whatsApp: " "), new Dictionary<string, string?>(), 2);

        Assert.True(outcome.IsSaved);
        List<AuditLog> rows = await ReadTrailAsync("UpdateSettings");
        Assert.Equal(new AuditValueChange("WhatsAppNumber", "3121234567", null), Assert.Single(rows[1].Details!.Changes));
        Assert.Null(await ScalarOrNullAsync("SELECT whats_app_number FROM company_settings"));
    }

    [Fact]
    public async Task SaveHomepage_WithAMissingOrUndefinedLayout_RefusesTheLayout()
    {
        HomepageOutcome missing = await SaveHomepageAsync(Input() with { HomepageLayout = null }, new Dictionary<string, string?>(), 1);
        HomepageOutcome undefined = await SaveHomepageAsync(Input() with { HomepageLayout = (HomepageLayout)99 }, new Dictionary<string, string?>(), 1);

        Assert.Equal(new FieldRejection("HomepageLayout", FieldCodes.FieldRequired), Assert.Single(missing.Settings!.Rejections), FieldRejectionComparer.Instance);
        Assert.Equal(new FieldRejection("HomepageLayout", FieldCodes.InvalidListValue), Assert.Single(undefined.Settings!.Rejections), FieldRejectionComparer.Instance);
        Assert.Empty(await ReadTrailAsync("UpdateSettings"));
    }

    [Fact]
    public async Task SaveHomepage_RolledBack_LeavesNeitherTheValuesNorTheLinksNorTheRow()
    {
        HomepageOutcome outcome = await SaveHomepageAsync(Input(HomepageLayout.ContentFirst), new Dictionary<string, string?> { ["Instagram"] = InstagramUrl }, 1, commit: false);

        Assert.True(outcome.IsSaved);
        Assert.Empty(await ReadTrailAsync("UpdateSettings"));
        Assert.Equal("StoreFirst", await ScalarAsync<string>("SELECT homepage_layout FROM company_settings"));
        Assert.Equal(0, await ScalarAsync<long>("SELECT count(*) FROM social_links"));
    }

    // ---- the social links save ----

    [Fact]
    public async Task SaveSocialLinks_AddsChangesAndDeletes_AndALeftOutPlatformStays()
    {
        SocialLinksSaveResult first = await RunContentAsync(m => m.SaveSocialLinksAsync(new Dictionary<string, string?> { ["Instagram"] = InstagramUrl, ["Facebook"] = FacebookUrl }, Ct));
        SocialLinksSaveResult second = await RunContentAsync(m => m.SaveSocialLinksAsync(new Dictionary<string, string?> { ["Instagram"] = "https://www.instagram.com/yeni", ["Facebook"] = " " }, Ct));
        SocialLinksSaveResult third = await RunContentAsync(m => m.SaveSocialLinksAsync(new Dictionary<string, string?> { ["TikTok"] = "https://www.tiktok.com/@firma" }, Ct));

        Assert.Equal(
            [new SocialLinkChange("Instagram", null, InstagramUrl), new SocialLinkChange("Facebook", null, FacebookUrl)],
            first.Changes);
        Assert.Equal(
            [new SocialLinkChange("Instagram", InstagramUrl, "https://www.instagram.com/yeni"), new SocialLinkChange("Facebook", FacebookUrl, null)],
            second.Changes);
        Assert.Equal([new SocialLinkChange("TikTok", null, "https://www.tiktok.com/@firma")], third.Changes);
        Assert.Equal("Instagram|TikTok", await ScalarAsync<string>("SELECT string_agg(social_platform, '|' ORDER BY social_platform) FROM social_links"));
    }

    [Fact]
    public async Task SaveSocialLinks_WithNothingChanged_ReturnsNoChange()
    {
        await RunContentAsync(m => m.SaveSocialLinksAsync(new Dictionary<string, string?> { ["Instagram"] = InstagramUrl }, Ct));

        SocialLinksSaveResult again = await RunContentAsync(m => m.SaveSocialLinksAsync(new Dictionary<string, string?> { ["Instagram"] = InstagramUrl, ["X"] = "" }, Ct));

        Assert.False(again.IsRejected);
        Assert.Empty(again.Changes);
    }

    [Theory]
    [InlineData("Instagram", "not a link", "InvalidUrl")]
    [InlineData("Instagram", "javascript:alert(1)", "InvalidUrl")]
    [InlineData("Myspace", "https://www.example.com/x", "InvalidListValue")]
    [InlineData("instagram", "https://www.example.com/x", "InvalidListValue")]
    [InlineData("0", "https://www.example.com/x", "InvalidListValue")]
    public async Task SaveSocialLinks_WithAWrongValueOrAPlatformOutsideTheList_RefusesTheWholeSave_AndWritesNothing(string platform, string typed, string code)
    {
        SocialLinksSaveResult result = await RunContentAsync(m => m.SaveSocialLinksAsync(
            new Dictionary<string, string?> { ["Facebook"] = FacebookUrl, [platform] = typed },
            Ct));

        Assert.True(result.IsRejected);
        Assert.Equal(new SocialLinkRejection(platform, code), Assert.Single(result.Rejections));
        Assert.Empty(result.Changes);
        Assert.Equal(0, await ScalarAsync<long>("SELECT count(*) FROM social_links"));
    }

    // ---- the identity trail (02 10.3.1 (1)) and the trail of the versions a changed identity causes (02 10.3.1 (6)) ----

    [Fact]
    public async Task SaveCompanyIdentity_WritesOneRowOfTheChangedFields_TheProvinceByItsName_AndASecondIdenticalSaveWritesNone()
    {
        SettingsSaveResult first = await RunSettingsAsync(m => m.SaveCompanyIdentityAsync(Identity("Yılmaz Ticaret"), 1, Ct));
        SettingsSaveResult same = await RunSettingsAsync(m => m.SaveCompanyIdentityAsync(Identity("Yılmaz Ticaret"), 2, Ct));
        SettingsSaveResult renamed = await RunSettingsAsync(m => m.SaveCompanyIdentityAsync(Identity("Yılmaz Ltd."), 3, Ct));

        Assert.True(first.IsSaved);
        Assert.True(same.IsSaved);
        Assert.True(renamed.IsSaved);
        List<AuditLog> rows = await ReadTrailAsync("UpdateCompany");
        Assert.Equal(2, rows.Count);
        Assert.All(rows, row =>
        {
            Assert.Equal(AuditArea.CompanyAndBranding, row.AuditArea);
            Assert.Equal("Company", row.TargetRecordType);
            Assert.Equal(_admin.Id, row.AdminId);
            Assert.Equal("Firma kimliği", row.Details!.DisplayName);
        });
        Assert.Contains(new AuditValueChange("LegalName", null, "Yılmaz Ticaret"), rows[0].Details!.Changes);
        Assert.Contains(new AuditValueChange("ProvinceId", null, "Ankara"), rows[0].Details!.Changes);
        Assert.Contains(new AuditValueChange("CompanyType", null, "Tradesman"), rows[0].Details!.Changes);
        Assert.Equal(new AuditValueChange("LegalName", "Yılmaz Ticaret", "Yılmaz Ltd."), Assert.Single(rows[1].Details!.Changes));
        Assert.Equal(await ScalarAsync<Guid>("SELECT id FROM companies"), rows[0].TargetRecordId);
    }

    [Fact]
    public async Task SaveCompanyIdentity_WithAStaleVersionOrARefusedValue_WritesNoRow()
    {
        await RunSettingsAsync(m => m.SaveCompanyIdentityAsync(Identity("Yılmaz Ticaret"), 1, Ct));

        SettingsSaveResult stale = await RunSettingsAsync(m => m.SaveCompanyIdentityAsync(Identity("Başka Ad"), 1, Ct));
        SettingsSaveResult refused = await RunSettingsAsync(m => m.SaveCompanyIdentityAsync(Identity("Başka Ad") with { Email = "not-an-email" }, 2, Ct));

        Assert.True(stale.IsVersionConflict);
        Assert.True(refused.IsRejected);
        Assert.Single(await ReadTrailAsync("UpdateCompany"));
        Assert.Equal("Yılmaz Ticaret", await ScalarAsync<string>("SELECT legal_name FROM companies"));
    }

    [Fact]
    public async Task SaveCompanyIdentity_ThatCausesGeneratedVersions_WritesAPublicationRowForEachOfThem_WithTheVersionNumber()
    {
        await RunSettingsAsync(m => m.SaveCompanyIdentityAsync(Identity("Yılmaz Ticaret"), 1, Ct));
        await RunSettingsAsync(m => m.SaveCompanyIdentityAsync(Identity("Yılmaz Ltd."), 2, Ct));

        List<AuditLog> rows = await ReadTrailAsync("PublishLegalText");
        string secondGeneration = await ScalarAsync<string>("SELECT coalesce(string_agg(id::text, '|' ORDER BY legal_text_type), '') FROM legal_text_versions WHERE version_number = 2");
        long firstGeneration = await ScalarAsync<long>("SELECT count(*) FROM legal_text_versions WHERE version_number = 1");

        // The first versions the templates make are a system event (the template changes with them) and get no row; the versions the changed identity causes each get one.
        Assert.Equal(2, firstGeneration);
        Assert.Equal(2, secondGeneration.Split('|').Length);
        Assert.Equal(2, rows.Count);
        Assert.All(rows, row =>
        {
            Assert.Equal("LegalTextVersion", row.TargetRecordType);
            Assert.Equal(_admin.Id, row.AdminId);
            Assert.Contains(row.TargetRecordId!.Value.ToString(), secondGeneration);
            AuditValue number = Assert.Single(row.Details!.Values);
            Assert.Equal("versionNumber", number.Name);
            Assert.Equal("2", number.Value);
        });
        Assert.Contains(rows, row => row.Details!.DisplayName == "Ön Bilgilendirme Formu");
        Assert.Contains(rows, row => row.Details!.DisplayName == "Mesafeli Satış Sözleşmesi");
    }

    // ---- helpers ----

    private static HomepageInput Input(
        HomepageLayout layout = HomepageLayout.StoreFirst,
        string products = "Ürünler",
        string offerings = "Hizmetler",
        string portfolio = "Çalışmalar",
        string about = "Hakkımızda",
        string faq = "Sıkça Sorulanlar",
        string contact = "İletişim",
        string? whatsApp = null)
    {
        return new HomepageInput(layout, products, offerings, portfolio, about, faq, contact, whatsApp);
    }

    private CompanyIdentityInput Identity(string legalName) => new(
        CompanyType.Tradesman,
        legalName,
        "1234567890",
        null,
        null,
        _provinceId,
        "Çankaya",
        "Atatürk Bulvarı 1",
        "0312 123 45 67",
        "iletisim@firma.com.tr",
        "firma@hs01.kep.tr",
        "İstanbul Esnaf ve Sanatkarlar Odası",
        null,
        "https://www.tobb.org.tr/etik-kurallar",
        null);

    /// <summary>
    /// Runs the home page save the way the page's runner does: one scope and one transaction; the social links first (tracked rows), the section's save with their changes after; the unit of work is
    /// saved and the transaction committed when both are saved, rolled back otherwise.
    /// </summary>
    private async Task<HomepageOutcome> SaveHomepageAsync(HomepageInput input, IReadOnlyDictionary<string, string?> links, int formVersion, bool commit = true)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(Ct);
        SocialLinksSaveResult social = await scope.ServiceProvider.GetRequiredService<ICorporateContentModule>().SaveSocialLinksAsync(links, Ct);
        if (social.IsRejected)
        {
            await transaction.RollbackAsync(Ct);
            return new HomepageOutcome(social, null);
        }

        SettingsSaveResult saved = await scope.ServiceProvider.GetRequiredService<ISettingsModule>().SaveHomepageAsync(input with { SocialLinkChanges = social.Changes }, formVersion, Ct);
        if (saved.IsSaved && commit)
        {
            await context.SaveChangesAsync(Ct);
            await transaction.CommitAsync(Ct);
        }
        else
        {
            await transaction.RollbackAsync(Ct);
        }

        return new HomepageOutcome(social, saved);
    }

    private Task<T> RunContentAsync<T>(Func<ICorporateContentModule, Task<T>> call) => RunAsync(call);

    private Task<T> RunSettingsAsync<T>(Func<ISettingsModule, Task<T>> call) => RunAsync(call);

    /// <summary>Calls a module the way a command does: one scope, one context, inside the caller's transaction; the unit of work is saved and the transaction committed.</summary>
    private async Task<T> RunAsync<TModule, T>(Func<TModule, Task<T>> call)
        where TModule : notnull
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(Ct);
        T result = await call(scope.ServiceProvider.GetRequiredService<TModule>());
        if (result is SettingsSaveResult { IsSaved: false })
        {
            await transaction.RollbackAsync(Ct);
        }
        else
        {
            await context.SaveChangesAsync(Ct);
            await transaction.CommitAsync(Ct);
        }

        return result;
    }

    private async Task<List<AuditLog>> ReadTrailAsync(string operation)
    {
        await using AppDbContext context = NewContext();
        List<AuditLog> rows = await context.AuditLogs.AsNoTracking().Where(l => l.Operation == operation).ToListAsync(Ct);

        // The rows of one transaction share the fixed time; the order of writing is the order of the identifiers (time-ordered).
        return [.. rows.OrderBy(l => l.CreatedAt).ThenBy(l => l.Id)];
    }

    private ServiceProvider BuildProvider()
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddAppCache();
        services.AddScoped<AdvisoryLocks>();
        services.AddSingleton<TimeProvider>(new FixedTime(Now));
        services.AddScoped<IJobQueue, JobQueue>();
        services.AddFileStore();
        services.AddOptions<StatusThresholdOptions>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.IDiskUsageReader, Shopfolio.Infrastructure.Status.DriveDiskUsageReader>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.DiskUsageGuard>();
        services.AddSingleton<ImageProcessor>();
        services.AddScoped<CacheInvalidations>();
        services.AddScoped<IAuditActorSource>(_ => new FixedActor(_admin));
        services.AddScoped<IAuditTrail, AuditTrail>();
        services.AddOptions<SiteOptions>().Configure(o => o.Domain = "shop.example.com");
        services.AddCatalogModule();
        services.AddSettingsModule();
        services.AddScoped<global::Shopfolio.Infrastructure.Security.AttemptCounter>();
        services.AddScoped<Shopfolio.Modules.Notifications.Contracts.INotificationsModule, Shopfolio.IntegrationTests.CorporateContent.UnusedNotifications>();
        services.AddCorporateContentModule();
        return services.BuildServiceProvider();
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
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

    private async Task<object?> ScalarOrNullAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        object? value = await command.ExecuteScalarAsync(Ct);
        return value is DBNull ? null : value;
    }

    /// <summary>The two results of the page runner's save: the social links' and (when they were accepted) the section's.</summary>
    private sealed record HomepageOutcome(SocialLinksSaveResult Social, SettingsSaveResult? Settings)
    {
        public bool IsSaved => Settings is { IsSaved: true };
    }

    private sealed class FieldRejectionComparer : IEqualityComparer<FieldRejection>
    {
        public static FieldRejectionComparer Instance { get; } = new();

        public bool Equals(FieldRejection? x, FieldRejection? y) => x is not null && y is not null && x.Field == y.Field && x.Code == y.Code;

        public int GetHashCode(FieldRejection obj) => HashCode.Combine(obj.Field, obj.Code);
    }

    private sealed class FixedTime(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FixedActor(Admin admin) : IAuditActorSource
    {
        public Task<AuditActor?> CurrentAsync(CancellationToken ct) => Task.FromResult<AuditActor?>(new AuditActor(admin.Id, admin.Name, admin.Email));
    }
}
