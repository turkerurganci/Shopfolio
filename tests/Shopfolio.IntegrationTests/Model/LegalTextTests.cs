using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;

using FieldRejection = Shopfolio.Modules.Settings.Contracts.FieldRejection;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the legal text tests (T366): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(LegalTextDefinition.Name)]
public sealed class LegalTextDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "LegalText";
}

/// <summary>
/// The four legal texts through the real Settings module on a real PostgreSQL (T366 criteria 1-10; KK1-KK10): the firm-edited texts have a draft and a publication, the two
/// generated texts get a version only when their text or template changes, the identity fields feed the privacy notice and the sales gate, a version once published stays
/// readable, and nothing in the schema records a consent or a "seen" mark.
/// </summary>
[Collection(LegalTextDefinition.Name)]
public sealed class LegalTextTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    private Guid _provinceId;

    private Admin _admin = null!;

    public LegalTextTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE provinces, companies, company_settings, legal_text_versions, countries, admins, audit_logs, dispatch_records CASCADE");
        Province province = Province.Create("06", "Ankara", Now);
        CompanySettings settings = CompanySettings.Create(Now);
        settings.BankTransferPaymentWindow = 1;
        settings.DownloadAllowance = 1;
        settings.HomepageLayout = Modules.CorporateContent.Contracts.HomepageLayout.StoreFirst;
        settings.MenuNameProducts = "Ürünler";
        settings.MenuNameOfferings = "Hizmetler";
        settings.MenuNamePortfolioItems = "Çalışmalar";
        settings.MenuNameAboutPage = "Hakkımızda";
        settings.MenuNameFaqItems = "Sıkça Sorulanlar";
        settings.MenuNameContact = "İletişim";
        _admin = Admin.Create("Ali Yılmaz", "ali@example.com", "hash", Now);
        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.Companies.Add(Company.Create(Now));
            c.CompanySettings.Add(settings);
            c.Admins.Add(_admin);
            c.LegalTextVersions.Add(LegalTextVersion.CreateDraft(LegalTextType.PrivacyNotice, "<p>{{LegalName}} aydınlatma metni</p>", Now));
            c.LegalTextVersions.Add(LegalTextVersion.CreateDraft(LegalTextType.CookiePolicy, "<p>çerez politikası</p>", Now));
        });
        _provinceId = province.Id;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: four separate records ----

    [Fact]
    public async Task TheFourTexts_AreFourSeparateRecords_EachWithItsOwnVersionNumbers()
    {
        await RunAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(), 1, Ct));
        await PublishAsync(LegalTextType.PrivacyNotice, "<p>gizlilik</p>", "Avrupa");
        await PublishAsync(LegalTextType.CookiePolicy, "<p>çerez</p>", null);

        IReadOnlyList<LegalTextSummary> summaries = await RunAsync(m => m.GetLegalTextsAsync(Ct));

        Assert.Equal(Enum.GetValues<LegalTextType>().Length, summaries.Count);
        Assert.All(summaries, s => Assert.Equal(1, s.LastVersionNumber));
        Assert.Equal(4, await ScalarAsync<long>("SELECT count(DISTINCT legal_text_type) FROM legal_text_versions WHERE published_at IS NOT NULL"));
        Assert.Equal([LegalTextType.PreInformationForm, LegalTextType.DistanceSalesContract], summaries.Where(s => !s.IsEditedByFirm).Select(s => s.Type));
    }

    // ---- KK2: draft and publication rules ----

    [Fact]
    public async Task AnEmptyText_IsRefusedAtTheSave_AndTheDraftStaysAsItWas()
    {
        LegalTextDraft draft = await RunAsync(m => m.GetLegalTextDraftAsync(LegalTextType.CookiePolicy, Ct));

        SettingsSaveResult refused = await RunAsync(m => m.SaveLegalTextDraftAsync(LegalTextType.CookiePolicy, "  ", null, draft.FormVersion, Ct));

        Assert.Equal([new FieldRejection("Text", FieldCodes.FilledFieldCleared)], refused.Rejections);
        Assert.Equal(draft, await RunAsync(m => m.GetLegalTextDraftAsync(LegalTextType.CookiePolicy, Ct)));
    }

    [Fact]
    public async Task ThePrivacyNotice_IsNotPublishedWithoutItsHostingLocation_AndTheLocationCannotBeEmptied()
    {
        LegalTextDraft draft = await RunAsync(m => m.GetLegalTextDraftAsync(LegalTextType.PrivacyNotice, Ct));

        await RunAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(), 1, Ct));
        LegalTextPublishResult refused = await RunAsync(m => m.PublishLegalTextAsync(LegalTextType.PrivacyNotice, draft.FormVersion, Ct));
        Assert.Equal([new FieldRejection("HostingLocation", FieldCodes.FieldRequired)], refused.Rejections);

        Assert.True((await RunAsync(m => m.SaveLegalTextDraftAsync(LegalTextType.PrivacyNotice, "<p>metin</p>", "Avrupa", draft.FormVersion, Ct))).IsSaved);
        LegalTextDraft saved = await RunAsync(m => m.GetLegalTextDraftAsync(LegalTextType.PrivacyNotice, Ct));
        SettingsSaveResult cleared = await RunAsync(m => m.SaveLegalTextDraftAsync(LegalTextType.PrivacyNotice, "<p>metin</p>", null, saved.FormVersion, Ct));
        Assert.Equal([new FieldRejection("HostingLocation", FieldCodes.FilledFieldCleared)], cleared.Rejections);
    }

    [Fact]
    public async Task TheList_NamesWhatIsMissing_ByPart()
    {
        IReadOnlyList<LegalTextSummary> before = await RunAsync(m => m.GetLegalTextsAsync(Ct));
        LegalTextSummary notice = before.Single(s => s.Type == LegalTextType.PrivacyNotice);

        Assert.Equal([LegalTextMissingPart.NotPublished, LegalTextMissingPart.HostingLocation, LegalTextMissingPart.CompanyIdentity], notice.Missing);

        await RunAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(), 1, Ct));
        await PublishAsync(LegalTextType.PrivacyNotice, "<p>gizlilik</p>", "Avrupa");
        LegalTextSummary after = (await RunAsync(m => m.GetLegalTextsAsync(Ct))).Single(s => s.Type == LegalTextType.PrivacyNotice);
        Assert.True(after.IsComplete);
    }

    [Fact]
    public async Task APublication_FillsTheIdentityPlaceholders_AndWritesOneTrailRow()
    {
        await RunAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(), 1, Ct));
        long before = await TrailCountAsync();

        LegalTextPublishResult result = await PublishAsync(LegalTextType.PrivacyNotice, "<p>{{LegalName}} & {{Order.OrderNumber}}</p>", "Avrupa");

        Assert.True(result.IsPublished);
        LegalTextVersionInfo version = (await RunAsync(m => m.GetLegalTextVersionAsync(result.VersionId!.Value, Ct)))!;
        Assert.Contains("Yılmaz Ticaret", version.Text, StringComparison.Ordinal);
        Assert.Equal(before + 1, await TrailCountAsync());
    }

    [Fact]
    public async Task ThePrivacyNotice_IsNotPublishedWhileAnIdentityField_IsMissing_AndEachMissingFieldIsNamed()
    {
        await RunAsync(m => m.SaveCompanyIdentityAsync(CompleteInput() with { KepAddress = null }, 1, Ct));
        LegalTextDraft draft = await RunAsync(m => m.GetLegalTextDraftAsync(LegalTextType.PrivacyNotice, Ct));
        Assert.True((await RunAsync(m => m.SaveLegalTextDraftAsync(LegalTextType.PrivacyNotice, "<p>{{LegalName}} {{KepAddress}}</p>", "Avrupa", draft.FormVersion, Ct))).IsSaved);
        LegalTextDraft saved = await RunAsync(m => m.GetLegalTextDraftAsync(LegalTextType.PrivacyNotice, Ct));

        LegalTextPublishResult refused = await RunAsync(m => m.PublishLegalTextAsync(LegalTextType.PrivacyNotice, saved.FormVersion, Ct));

        Assert.True(refused.IsRejected);
        Assert.Equal([new FieldRejection("KepAddress", FieldCodes.FieldRequired)], refused.Rejections);
        Assert.Equal(0, await VersionCountAsync(LegalTextType.PrivacyNotice));
        Assert.Equal(0, await TrailCountAsync());
    }

    [Fact]
    public async Task ThePublishedNotice_ShowsTheCurrentIdentity_AfterAnIdentityFieldChanges_WithoutANewVersion()
    {
        await RunAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(), 1, Ct));
        LegalTextPublishResult result = await PublishAsync(LegalTextType.PrivacyNotice, "<p>{{LegalName}} - {{KepAddress}}</p>", "Avrupa");
        Assert.True(result.IsPublished);

        LegalTextVersionInfo first = (await RunAsync(m => m.GetLegalTextVersionAsync(result.VersionId!.Value, Ct)))!;
        Assert.Equal("<p>Yılmaz Ticaret - firma@hs01.kep.tr</p>", first.Text);

        await RunAsync(m => m.SaveCompanyIdentityAsync(CompleteInput() with { LegalName = "Yeni & Ticaret", KepAddress = "yeni@hs01.kep.tr" }, 2, Ct));

        LegalTextVersionInfo byId = (await RunAsync(m => m.GetLegalTextVersionAsync(result.VersionId!.Value, Ct)))!;
        LegalTextVersionInfo latest = (await RunAsync(m => m.GetLatestLegalTextVersionAsync(LegalTextType.PrivacyNotice, Ct)))!;
        Assert.Equal("<p>Yeni &amp; Ticaret - yeni@hs01.kep.tr</p>", byId.Text);
        Assert.Equal(byId.Text, latest.Text);
        Assert.Equal(1, await VersionCountAsync(LegalTextType.PrivacyNotice));
        Assert.Contains("{{LegalName}}", await ScalarAsync<string>("SELECT text FROM legal_text_versions WHERE published_at IS NOT NULL AND legal_text_type = 'PrivacyNotice'"), StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheGate_KeepsThePrivacyNoticeClosed_WhileTheIdentityIsMissing_AndTheListNamesTheIdentity()
    {
        await RunAsync(m => m.SaveReturnAddressAsync(new ReturnAddressInput("Depo Sorumlusu", _provinceId, "Çankaya", "Depo Sk. 4", "3121234567"), 1, Ct));
        LegalTextDraft draft = await RunAsync(m => m.GetLegalTextDraftAsync(LegalTextType.PrivacyNotice, Ct));
        Assert.True((await RunAsync(m => m.SaveLegalTextDraftAsync(LegalTextType.PrivacyNotice, "<p>{{LegalName}}</p>", "Avrupa", draft.FormVersion, Ct))).IsSaved);
        LegalTextDraft saved = await RunAsync(m => m.GetLegalTextDraftAsync(LegalTextType.PrivacyNotice, Ct));

        Assert.True((await RunAsync(m => m.PublishLegalTextAsync(LegalTextType.PrivacyNotice, saved.FormVersion, Ct))).IsRejected);

        SalesGate gate = await RunAsync(m => m.GetSalesGateAsync(Ct));
        Assert.Contains(SalesGateCondition.PrivacyNotice, gate.UnmetConditions);
        LegalTextSummary notice = (await RunAsync(m => m.GetLegalTextsAsync(Ct))).Single(s => s.Type == LegalTextType.PrivacyNotice);
        Assert.Contains(LegalTextMissingPart.CompanyIdentity, notice.Missing);
        Assert.Contains(LegalTextMissingPart.NotPublished, notice.Missing);
    }

    // ---- KK3: generated texts and the version rule ----

    [Fact]
    public async Task ASaveThatChangesNoValue_WritesNoVersion_AndAChangedValueWritesOne()
    {
        await RunAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(), 1, Ct));
        long first = await VersionCountAsync(LegalTextType.PreInformationForm);

        await RunAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(), 2, Ct));
        long same = await VersionCountAsync(LegalTextType.PreInformationForm);
        await RunAsync(m => m.SaveCompanyIdentityAsync(CompleteInput() with { Phone = "0312 999 99 99" }, 3, Ct));
        long changed = await VersionCountAsync(LegalTextType.PreInformationForm);

        Assert.Equal(1, first);
        Assert.Equal(first, same);
        Assert.Equal(first + 1, changed);
    }

    [Fact]
    public async Task ATemplateChange_WritesANewVersionAsASystemEvent_WithNoTrailRow()
    {
        await RunAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(), 1, Ct));
        await ExecuteAsync("UPDATE legal_text_versions SET template_version = 'eski' WHERE legal_text_type = 'DistanceSalesContract'");
        long trail = await TrailCountAsync();

        await RunAsync(m => m.SynchronizeGeneratedLegalTextsAsync(Ct));

        Assert.Equal(2, await VersionCountAsync(LegalTextType.DistanceSalesContract));
        Assert.Equal(trail, await TrailCountAsync());
    }

    [Fact]
    public async Task TheFirstVersionsOfTheGeneratedTexts_WriteNoTrailRow()
    {
        await RunAsync(m => m.SynchronizeGeneratedLegalTextsAsync(Ct));

        Assert.Equal(1, await VersionCountAsync(LegalTextType.PreInformationForm));
        Assert.Equal(1, await VersionCountAsync(LegalTextType.DistanceSalesContract));
        Assert.Equal(0, await TrailCountAsync());
    }

    [Fact]
    public async Task ASecondSynchronization_WritesNothing()
    {
        await RunAsync(m => m.SynchronizeGeneratedLegalTextsAsync(Ct));
        await RunAsync(m => m.SynchronizeGeneratedLegalTextsAsync(Ct));

        Assert.Equal(1, await VersionCountAsync(LegalTextType.PreInformationForm));
    }

    [Fact]
    public async Task ConcurrentIdentityAndReturnAddressSaves_LeaveALastVersionCarryingBoth()
    {
        await RunAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(), 1, Ct));
        await RunAsync(m => m.SaveReturnAddressAsync(new ReturnAddressInput("Depo Sorumlusu", _provinceId, "Çankaya", "Depo Sk. 4", "3121234567"), 1, Ct));

        await Task.WhenAll(
            RunAsync(m => m.SaveCompanyIdentityAsync(CompleteInput() with { AddressLine = "Yeni Cad. 7" }, 2, Ct)),
            RunAsync(m => m.SaveReturnAddressAsync(new ReturnAddressInput("Yeni Sorumlu", _provinceId, "Çankaya", "Depo Sk. 9", "3121234567"), 2, Ct)));

        LegalTextVersionInfo last = (await RunAsync(m => m.GetLatestLegalTextVersionAsync(LegalTextType.PreInformationForm, Ct)))!;
        Assert.Contains("Yeni Cad. 7", last.Text, StringComparison.Ordinal);
        Assert.Contains("Depo Sk. 9", last.Text, StringComparison.Ordinal);
    }

    // ---- KK4: shown versions, old versions, no notice ----

    [Fact]
    public async Task ANewVersion_MakesTheShownIdsStale_ButTheOldVersionStaysReadable_AndNobodyIsNotified()
    {
        await RunAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(), 1, Ct));
        LegalTextVersionIds shown = await RunAsync(m => m.GetCurrentLegalTextVersionsAsync(Ct));
        Assert.True(await RunAsync(m => m.AreShownLegalTextVersionsCurrentAsync(shown.PreInformationForm!.Value, shown.DistanceSalesContract!.Value, Ct)));

        await RunAsync(m => m.SaveCompanyIdentityAsync(CompleteInput() with { Phone = "0312 777 77 77" }, 2, Ct));

        Assert.False(await RunAsync(m => m.AreShownLegalTextVersionsCurrentAsync(shown.PreInformationForm!.Value, shown.DistanceSalesContract!.Value, Ct)));
        LegalTextVersionInfo old = (await RunAsync(m => m.GetLegalTextVersionAsync(shown.PreInformationForm!.Value, Ct)))!;
        Assert.Equal(1, old.VersionNumber);
        Assert.Equal(0, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records"));
    }

    // ---- KK5: no consent and no "seen" record ----

    [Fact]
    public async Task NoTableOrColumn_RecordsAConsentOrASeenMark()
    {
        long found = await ScalarAsync<long>(
            "SELECT count(*) FROM information_schema.columns WHERE table_schema = 'public' "
            + "AND (column_name ~* '(consent|seen|accepted_privacy|privacy_accepted)' OR table_name ~* '(consent|seen)')");

        Assert.Equal(0, found);
    }

    // ---- KK9: the gate's privacy notice condition ----

    [Fact]
    public async Task TheGate_HasThePrivacyNoticeOpen_OnlyAfterTheNoticeIsPublishedWithItsHostingLocation()
    {
        await RunAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(), 1, Ct));
        await RunAsync(m => m.SaveReturnAddressAsync(new ReturnAddressInput("Depo Sorumlusu", _provinceId, "Çankaya", "Depo Sk. 4", "3121234567"), 1, Ct));

        SalesGate before = await RunAsync(m => m.GetSalesGateAsync(Ct));
        Assert.Contains(SalesGateCondition.PrivacyNotice, before.UnmetConditions);

        await PublishAsync(LegalTextType.PrivacyNotice, "<p>gizlilik</p>", "Avrupa");

        SalesGate after = await RunAsync(m => m.GetSalesGateAsync(Ct));
        Assert.DoesNotContain(SalesGateCondition.PrivacyNotice, after.UnmetConditions);
    }

    // ---- T507: the draft preview and the cleaning of the draft ----

    [Fact]
    public async Task TheDraftPreview_FillsThePlaceholdersWithTheCurrentIdentity_AndDiffersOnlyAfterAPublicationWhenTheDraftChanged()
    {
        await RunAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(), 1, Ct));

        LegalTextDraftPreview unpublished = await RunAsync(m => m.GetLegalTextDraftPreviewAsync(LegalTextType.PrivacyNotice, Ct));
        Assert.Equal("<p>Yılmaz Ticaret aydınlatma metni</p>", unpublished.FilledText);
        Assert.False(unpublished.DiffersFromPublished);

        await PublishAsync(LegalTextType.PrivacyNotice, "<p>{{LegalName}} aydınlatma metni</p>", "Avrupa");
        Assert.False((await RunAsync(m => m.GetLegalTextDraftPreviewAsync(LegalTextType.PrivacyNotice, Ct))).DiffersFromPublished);

        // An identity change alone is no difference of the draft: the tokens stay in both rows.
        await ExecuteAsync("UPDATE companies SET legal_name = 'Başka Unvan'");
        LegalTextDraftPreview renamed = await RunAsync(m => m.GetLegalTextDraftPreviewAsync(LegalTextType.PrivacyNotice, Ct));
        Assert.Equal("<p>Başka Unvan aydınlatma metni</p>", renamed.FilledText);
        Assert.False(renamed.DiffersFromPublished);

        LegalTextDraft draft = await RunAsync(m => m.GetLegalTextDraftAsync(LegalTextType.PrivacyNotice, Ct));
        Assert.True((await RunAsync(m => m.SaveLegalTextDraftAsync(LegalTextType.PrivacyNotice, "<p>değişti</p>", "Avrupa", draft.FormVersion, Ct))).IsSaved);
        Assert.True((await RunAsync(m => m.GetLegalTextDraftPreviewAsync(LegalTextType.PrivacyNotice, Ct))).DiffersFromPublished);
    }

    [Fact]
    public async Task TheDraftPreview_IsRefusedForAGeneratedText()
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(() => RunAsync(m => m.GetLegalTextDraftPreviewAsync(LegalTextType.PreInformationForm, Ct)));
    }

    [Fact]
    public async Task TheDraftSave_CleansTheTextWithTheClosedFormatSet_KeepsTheIdentityTokens_AndAnEmptiedByCleaningTextIsRefused()
    {
        LegalTextDraft draft = await RunAsync(m => m.GetLegalTextDraftAsync(LegalTextType.CookiePolicy, Ct));

        SettingsSaveResult saved = await RunAsync(m => m.SaveLegalTextDraftAsync(LegalTextType.CookiePolicy, "<p onclick=\"x()\">{{LegalName}} <b>kalın</b></p><script>alert(1)</script>", null, draft.FormVersion, Ct));

        Assert.True(saved.IsSaved);
        LegalTextDraft after = await RunAsync(m => m.GetLegalTextDraftAsync(LegalTextType.CookiePolicy, Ct));
        Assert.DoesNotContain("script", after.Text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onclick", after.Text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("{{LegalName}}", after.Text, StringComparison.Ordinal);
        Assert.Equal(draft.FormVersion + 1, after.FormVersion);

        SettingsSaveResult emptied = await RunAsync(m => m.SaveLegalTextDraftAsync(LegalTextType.CookiePolicy, "<script>alert(1)</script>", null, after.FormVersion, Ct));
        Assert.Equal([new FieldRejection("Text", FieldCodes.FilledFieldCleared)], emptied.Rejections);
        Assert.Equal(after, await RunAsync(m => m.GetLegalTextDraftAsync(LegalTextType.CookiePolicy, Ct)));
    }

    // ---- helpers ----

    private CompanyIdentityInput CompleteInput() =>
        new(
            CompanyType.Tradesman,
            "Yılmaz Ticaret",
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

    private async Task<LegalTextPublishResult> PublishAsync(LegalTextType type, string text, string? hosting)
    {
        LegalTextDraft draft = await RunAsync(m => m.GetLegalTextDraftAsync(type, Ct));
        Assert.True((await RunAsync(m => m.SaveLegalTextDraftAsync(type, text, hosting, draft.FormVersion, Ct))).IsSaved);
        LegalTextDraft saved = await RunAsync(m => m.GetLegalTextDraftAsync(type, Ct));
        return await RunAsync(m => m.PublishLegalTextAsync(type, saved.FormVersion, Ct), _admin);
    }

    private Task<long> VersionCountAsync(LegalTextType type) =>
        ScalarAsync<long>($"SELECT count(*) FROM legal_text_versions WHERE legal_text_type = '{type}' AND published_at IS NOT NULL");

    private Task<long> TrailCountAsync() => ScalarAsync<long>("SELECT count(*) FROM audit_logs WHERE target_record_type = 'LegalTextVersion'");

    private ServiceProvider BuildProvider(Admin? admin)
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddScoped<CacheInvalidations>();
        services.AddSingleton<TimeProvider>(new FixedTime(Now));
        services.AddScoped<IAuditActorSource>(_ => new FixedActor(admin));
        services.AddScoped<IAuditTrail, AuditTrail>();
        services.AddScoped<ICardPaymentCondition>(_ => new OpenCardCondition());
        services.AddScoped<IBankTransferCondition>(_ => new OpenTransferCondition());
        services.AddSettingsModule();
        return services.BuildServiceProvider();
    }

    private async Task RunAsync(Func<ISettingsModule, Task> call)
    {
        await RunAsync(async m =>
        {
            await call(m);
            return 0;
        });
    }

    private async Task<T> RunAsync<T>(Func<ISettingsModule, Task<T>> call, Admin? admin = null)
    {
        await using ServiceProvider provider = BuildProvider(admin ?? _admin);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(Ct);
        T result = await call(scope.ServiceProvider.GetRequiredService<ISettingsModule>());
        await context.SaveChangesAsync(Ct);
        await transaction.CommitAsync(Ct);
        return result;
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

    private sealed class OpenCardCondition : ICardPaymentCondition
    {
        public Task<bool> IsOpenAsync(CancellationToken ct) => Task.FromResult(true);
    }

    private sealed class OpenTransferCondition : IBankTransferCondition
    {
        public Task<bool> IsOpenAsync(CancellationToken ct) => Task.FromResult(true);
    }

    private sealed class FixedTime(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FixedActor : IAuditActorSource
    {
        private readonly AuditActor? _actor;

        public FixedActor(Admin? admin)
        {
            _actor = admin is null ? null : new AuditActor(admin.Id, admin.Name, admin.Email);
        }

        public Task<AuditActor?> CurrentAsync(CancellationToken ct) => Task.FromResult(_actor);
    }
}
