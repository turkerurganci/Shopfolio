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

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the brand identity tests (T362): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(BrandingDefinition.Name)]
public sealed class BrandingDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "Branding";
}

/// <summary>
/// The brand identity through the real CorporateContent module on a real PostgreSQL (T362 criteria 1-6; KK1-KK6; 02 3.29; 06 4.54; 09 10.2 item 2): the four settings and the platform signature
/// switch are saved by the version of the form, behind the uploaded-image check, in the caller's transaction; the name cannot be emptied; the colour is a stored shape; the identity read gives
/// the displayed name, the icon letter and the text colour the system chose; every changed value reaches the trail in the same transaction and a save that changes nothing writes nothing.
/// </summary>
[Collection(BrandingDefinition.Name)]
public sealed class BrandingTests : IAsyncLifetime
{
    private const string Domain = "shop.example.com";

    private static readonly DateTimeOffset Now = new(2026, 11, 8, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    private Admin _admin = null!;

    private Guid _brandingId;

    public BrandingTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        // The arrangement is the row as it is born: no name, no logo, no icon, no colour, the signature on, version 1.
        await ExecuteAsync("TRUNCATE audit_logs, admins, brandings, images CASCADE");
        _admin = Admin.Create("Ali Yılmaz", "ali@example.com", "hash", Now);
        Branding branding = Branding.Create(Now);
        _brandingId = branding.Id;
        await using AppDbContext context = NewContext();
        context.Admins.Add(_admin);
        context.Brandings.Add(branding);
        await context.SaveChangesAsync(Ct);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the four settings and the signature switch, no field for code ----

    [Fact]
    public async Task TheBrandTable_HoldsTheFourSettingsAndTheSignatureSwitchOnly_NoColumnForAScriptAStyleATrackingCodeATheme_OrAPanelColour()
    {
        string columns = await ScalarAsync<string>(
            "SELECT string_agg(column_name, ',' ORDER BY column_name) FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'brandings'");

        Assert.Equal("brand_color,brand_name,created_at,favicon_id,id,is_platform_signature_enabled,logo_id,version", columns);
    }

    [Fact]
    public async Task TheFormIsRead_AsTheRowIsBorn_NothingEntered_TheSignatureOn()
    {
        BrandingSection section = await RunAsync(m => m.GetBrandingAsync(Ct));

        Assert.Equal(new BrandingSection(null, null, null, null, true, 1), section);
    }

    [Fact]
    public async Task ASave_WritesTheFourSettings_NormalisesTheColour_StepsTheVersion_AndWritesOneBrandingTrailRowWithTheChangedFields()
    {
        List<Image> images = await SeedImagesAsync(2);

        BrandingSaveResult result = await RunAsync(m => m.SaveBrandingAsync(new BrandingInput("  Ayşe Butik ", images[0].Id, images[1].Id, "#1a56db", true), 1, Ct));

        Assert.True(result.IsSaved);
        Assert.Equal(new BrandingSection("Ayşe Butik", images[0].Id, images[1].Id, "#1A56DB", true, 2), await RunAsync(m => m.GetBrandingAsync(Ct)));
        AuditLog row = Assert.Single(await ReadTrailAsync());
        Assert.Equal("UpdateBranding", row.Operation);
        Assert.Equal("Branding", row.TargetRecordType);
        Assert.Equal(_brandingId, row.TargetRecordId);
        Assert.Equal(_admin.Id, row.AdminId);
        Assert.Equal(BrandingSettings.BrandingDisplayName, row.Details!.DisplayName);
        Assert.Equal(
            [
                new AuditValueChange("BrandName", null, "Ayşe Butik"),
                new AuditValueChange("LogoId", null, images[0].Id.ToString("D")),
                new AuditValueChange("FaviconId", null, images[1].Id.ToString("D")),
                new AuditValueChange("BrandColor", null, "#1A56DB"),
            ],
            row.Details.Changes);
    }

    [Fact]
    public async Task OnlyTheChangedFields_ReachTheTrail_AndALogoCanBeRemovedAgain()
    {
        List<Image> images = await SeedImagesAsync(1);
        await RunAsync(m => m.SaveBrandingAsync(new BrandingInput("Ayşe Butik", images[0].Id, null, null, true), 1, Ct));

        BrandingSaveResult removed = await RunAsync(m => m.SaveBrandingAsync(new BrandingInput("Ayşe Butik", null, null, null, true), 2, Ct));

        Assert.True(removed.IsSaved);
        List<AuditLog> rows = await ReadTrailAsync();
        Assert.Equal(2, rows.Count);
        Assert.Equal([new AuditValueChange("LogoId", images[0].Id.ToString("D"), null)], rows[1].Details!.Changes);
        Assert.Null((await RunAsync(m => m.GetBrandingAsync(Ct))).LogoId);
    }

    // ---- KK2: no logo shows the name as text; an image that is not an uploaded one is refused ----

    [Fact]
    public async Task WithoutALogo_TheIdentityShowsTheNameAsText_AndNoLogoId()
    {
        await RunAsync(m => m.SaveBrandingAsync(new BrandingInput("Ayşe Butik", null, null, null, true), 1, Ct));

        BrandIdentity identity = await RunAsync(m => m.GetBrandIdentityAsync(Ct));

        Assert.Equal("Ayşe Butik", identity.DisplayName);
        Assert.True(identity.HasBrandName);
        Assert.Null(identity.LogoId);
    }

    [Fact]
    public async Task ALogoOrAnIconThatNamesNoUploadedImage_IsRefused_AndNothingIsStored()
    {
        Guid unknown = EntityIds.New();

        BrandingSaveResult result = await RunAsync(m => m.SaveBrandingAsync(new BrandingInput("Ayşe Butik", unknown, Guid.Empty, "#1A56DB", false), 1, Ct));

        Assert.False(result.IsSaved);
        Assert.False(result.IsVersionConflict);
        Assert.Equal(
            [
                new BrandingRejection(nameof(BrandingInput.LogoId), FieldCodes.InvalidImage),
                new BrandingRejection(nameof(BrandingInput.FaviconId), FieldCodes.InvalidImage),
            ],
            result.Rejections);
        Assert.Equal(new BrandingSection(null, null, null, null, true, 1), await RunAsync(m => m.GetBrandingAsync(Ct)));
        Assert.Empty(await ReadTrailAsync());
    }

    [Fact]
    public async Task TheImageAlreadyLinked_IsKept_WithoutBeingCheckedAgain()
    {
        List<Image> images = await SeedImagesAsync(1);
        await RunAsync(m => m.SaveBrandingAsync(new BrandingInput("Ayşe Butik", images[0].Id, null, null, true), 1, Ct));

        BrandingSaveResult again = await RunAsync(m => m.SaveBrandingAsync(new BrandingInput("Ayşe Butik Yeni", images[0].Id, null, null, true), 2, Ct));

        Assert.True(again.IsSaved);
        Assert.Equal(images[0].Id, (await RunAsync(m => m.GetBrandingAsync(Ct))).LogoId);
    }

    // ---- KK3: the name is required once entered; until then the domain shows ----

    [Fact]
    public async Task TheBrandName_CannotBeEmptied_OnceEntered_AndNothingIsStored()
    {
        await RunAsync(m => m.SaveBrandingAsync(new BrandingInput("Ayşe Butik", null, null, null, true), 1, Ct));

        BrandingSaveResult blank = await RunAsync(m => m.SaveBrandingAsync(new BrandingInput("   ", null, null, "#1A56DB", true), 2, Ct));
        BrandingSaveResult none = await RunAsync(m => m.SaveBrandingAsync(new BrandingInput(null, null, null, null, true), 2, Ct));

        Assert.Equal(new BrandingRejection(nameof(BrandingInput.BrandName), FieldCodes.FilledFieldCleared), Assert.Single(blank.Rejections));
        Assert.Equal(new BrandingRejection(nameof(BrandingInput.BrandName), FieldCodes.FilledFieldCleared), Assert.Single(none.Rejections));
        Assert.Equal(new BrandingSection("Ayşe Butik", null, null, null, true, 2), await RunAsync(m => m.GetBrandingAsync(Ct)));
        Assert.Single(await ReadTrailAsync());
    }

    [Fact]
    public async Task WhileTheNameIsEmpty_AnEmptyNameIsAccepted_AndTheDomainIsTheDisplayedName()
    {
        BrandingSaveResult colourOnly = await RunAsync(m => m.SaveBrandingAsync(new BrandingInput(null, null, null, "#000080", true), 1, Ct));
        BrandIdentity identity = await RunAsync(m => m.GetBrandIdentityAsync(Ct));

        Assert.True(colourOnly.IsSaved);
        Assert.Equal(Domain, identity.DisplayName);
        Assert.False(identity.HasBrandName);
    }

    [Fact]
    public async Task ABrandNameBeyondTheTrailsValueLimit_IsStoredWhole_AndShownCutAtTheLimitInTheTrail()
    {
        string longName = new('a', AuditEntryRules.MaxValueLength + 500);

        BrandingSaveResult result = await RunAsync(m => m.SaveBrandingAsync(new BrandingInput(longName, null, null, null, true), 1, Ct));

        Assert.True(result.IsSaved);
        Assert.Equal(longName, (await RunAsync(m => m.GetBrandingAsync(Ct))).BrandName);
        AuditLog row = Assert.Single(await ReadTrailAsync());
        Assert.Equal(new string('a', AuditEntryRules.MaxValueLength), Assert.Single(row.Details!.Changes).NewValue);
    }

    // ---- KK4: no icon shows the first letter on the brand colour; the icon of the product is never the site's ----

    [Theory]
    [InlineData("istanbul Saat", "İ")]
    [InlineData("ayşe butik", "A")]
    public async Task WithoutAnIcon_TheIdentityCarriesTheFirstLetterOfTheName_InCapitals(string name, string initial)
    {
        await RunAsync(m => m.SaveBrandingAsync(new BrandingInput(name, null, null, null, true), 1, Ct));

        BrandIdentity identity = await RunAsync(m => m.GetBrandIdentityAsync(Ct));

        Assert.Null(identity.FaviconId);
        Assert.Equal(initial, identity.FaviconInitial);
    }

    [Fact]
    public async Task WithoutAName_TheLetterIsTheFirstLetterOfTheDomain_AndAnIconIsTheUploadedOne()
    {
        Assert.Equal("S", (await RunAsync(m => m.GetBrandIdentityAsync(Ct))).FaviconInitial);
        List<Image> images = await SeedImagesAsync(1);
        await RunAsync(m => m.SaveBrandingAsync(new BrandingInput(null, null, images[0].Id, null, true), 1, Ct));

        Assert.Equal(images[0].Id, (await RunAsync(m => m.GetBrandIdentityAsync(Ct))).FaviconId);
    }

    // ---- KK5: the colour, the text on it and the default ----

    [Theory]
    [InlineData("#FFFF00", "#000000")]
    [InlineData("#000080", "#FFFFFF")]
    [InlineData("#777777", "#000000")]
    public async Task TheIdentity_GivesTheBrandColour_AndTheTextColourTheSystemChose(string color, string text)
    {
        await RunAsync(m => m.SaveBrandingAsync(new BrandingInput("Ayşe Butik", null, null, color, true), 1, Ct));

        BrandIdentity identity = await RunAsync(m => m.GetBrandIdentityAsync(Ct));

        Assert.Equal(color, identity.Color);
        Assert.Equal(text, identity.TextColor);
        Assert.False(identity.IsDefaultColor);
    }

    [Fact]
    public async Task WithoutAColour_TheIdentityTakesTheProductsDefault_AndNoTextColourIsStored()
    {
        BrandIdentity identity = await RunAsync(m => m.GetBrandIdentityAsync(Ct));

        Assert.True(identity.IsDefaultColor);
        Assert.Equal(EmailColors.DefaultButtonBackground, identity.Color);
        Assert.Equal(EmailColors.White, identity.TextColor);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM information_schema.columns WHERE table_name = 'brandings' AND column_name LIKE '%text%'"));
    }

    [Theory]
    [InlineData("#FFF")]
    [InlineData("#GGGGGG")]
    [InlineData("#123456;background:url(x)")]
    [InlineData("blue")]
    public async Task AColourThatIsNotTheStoredShape_IsRefused_AndNothingIsStored(string color)
    {
        BrandingSaveResult result = await RunAsync(m => m.SaveBrandingAsync(new BrandingInput("Ayşe Butik", null, null, color, true), 1, Ct));

        Assert.Equal(new BrandingRejection(nameof(BrandingInput.BrandColor), FieldCodes.InvalidColor), Assert.Single(result.Rejections));
        Assert.Equal(new BrandingSection(null, null, null, null, true, 1), await RunAsync(m => m.GetBrandingAsync(Ct)));
        Assert.Empty(await ReadTrailAsync());
    }

    [Fact]
    public async Task AColourChosenBefore_CanBeClearedAgain_AndTheDefaultReturns()
    {
        await RunAsync(m => m.SaveBrandingAsync(new BrandingInput("Ayşe Butik", null, null, "#FFFF00", true), 1, Ct));

        BrandingSaveResult cleared = await RunAsync(m => m.SaveBrandingAsync(new BrandingInput("Ayşe Butik", null, null, "  ", true), 2, Ct));

        Assert.True(cleared.IsSaved);
        Assert.True((await RunAsync(m => m.GetBrandIdentityAsync(Ct))).IsDefaultColor);
        Assert.Equal([new AuditValueChange("BrandColor", "#FFFF00", null)], (await ReadTrailAsync())[1].Details!.Changes);
    }

    // ---- KK6: the platform signature ----

    [Fact]
    public async Task TheSignature_IsOnByDefault_AndEachSwitchIsItsOwnSettingsTrailRowWithTheOldAndNewValue()
    {
        Assert.True((await RunAsync(m => m.GetBrandIdentityAsync(Ct))).ShowsPlatformSignature);

        BrandingSaveResult off = await RunAsync(m => m.SaveBrandingAsync(new BrandingInput(null, null, null, null, false), 1, Ct));
        BrandingSaveResult on = await RunAsync(m => m.SaveBrandingAsync(new BrandingInput(null, null, null, null, true), 2, Ct));

        Assert.True(off.IsSaved);
        Assert.True(on.IsSaved);
        List<AuditLog> rows = await ReadTrailAsync();
        Assert.Equal(2, rows.Count);
        Assert.All(rows, r =>
        {
            Assert.Equal("UpdateSettings", r.Operation);
            Assert.Equal("Branding", r.TargetRecordType);
            Assert.Equal(_brandingId, r.TargetRecordId);
            Assert.Equal(BrandingSettings.PlatformSignatureDisplayName, r.Details!.DisplayName);
        });
        Assert.Equal([new AuditValueChange("IsPlatformSignatureEnabled", "true", "false")], rows[0].Details!.Changes);
        Assert.Equal([new AuditValueChange("IsPlatformSignatureEnabled", "false", "true")], rows[1].Details!.Changes);
        Assert.Equal(3, (await RunAsync(m => m.GetBrandingAsync(Ct))).Version);
    }

    [Fact]
    public async Task ASaveThatChangesTheFieldsAndTheSignature_WritesTwoRows_TheBrandingAndTheSettingsOne()
    {
        await RunAsync(m => m.SaveBrandingAsync(new BrandingInput("Ayşe Butik", null, null, null, false), 1, Ct));

        List<AuditLog> rows = await ReadTrailAsync();
        Assert.Equal(2, rows.Count);
        Assert.Single(rows, r => r.Operation == "UpdateBranding");
        Assert.Single(rows, r => r.Operation == "UpdateSettings");
        Assert.False((await RunAsync(m => m.GetBrandIdentityAsync(Ct))).ShowsPlatformSignature);
    }

    // ---- Versions, unchanged saves, the transaction and the actor ----

    [Fact]
    public async Task AStaleVersion_IsAConflict_AndWritesNeitherTheValuesNorATrailRow()
    {
        await RunAsync(m => m.SaveBrandingAsync(new BrandingInput("Ayşe Butik", null, null, null, true), 1, Ct));

        BrandingSaveResult stale = await RunAsync(m => m.SaveBrandingAsync(new BrandingInput("Başka Ad", null, null, "#FFFF00", false), 1, Ct));

        Assert.True(stale.IsVersionConflict);
        Assert.Equal(new BrandingSection("Ayşe Butik", null, null, null, true, 2), await RunAsync(m => m.GetBrandingAsync(Ct)));
        Assert.Single(await ReadTrailAsync());
    }

    [Fact]
    public async Task ASaveThatChangesNothing_WritesNothing_NoVersionStepAndNoTrailRow_AndAStaleOneIsStillAConflict()
    {
        await RunAsync(m => m.SaveBrandingAsync(new BrandingInput("Ayşe Butik", null, null, "#FFFF00", true), 1, Ct));
        BrandingSaveResult same = await RunAsync(m => m.SaveBrandingAsync(new BrandingInput("Ayşe Butik", null, null, "#ffff00", true), 2, Ct));
        BrandingSaveResult staleSame = await RunAsync(m => m.SaveBrandingAsync(new BrandingInput("Ayşe Butik", null, null, "#FFFF00", true), 1, Ct));

        Assert.True(same.IsSaved);
        Assert.True(staleSame.IsVersionConflict);
        Assert.Equal(2, (await RunAsync(m => m.GetBrandingAsync(Ct))).Version);
        Assert.Single(await ReadTrailAsync());
    }

    [Fact]
    public async Task TheSaveAndItsTrailRows_AreOneTransaction_ARollbackLeavesNeither()
    {
        BrandingSaveResult written = await RunAsync(m => m.SaveBrandingAsync(new BrandingInput("Ayşe Butik", null, null, null, false), 1, Ct), commit: false);

        Assert.True(written.IsSaved);
        Assert.Equal(new BrandingSection(null, null, null, null, true, 1), await RunAsync(m => m.GetBrandingAsync(Ct)));
        Assert.Empty(await ReadTrailAsync());
    }

    [Fact]
    public async Task ASave_WithoutAnAdministrator_OrOutsideATransaction_IsRefused_AndNothingIsLeft()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => RunAsync(m => m.SaveBrandingAsync(new BrandingInput("Ayşe Butik", null, null, null, true), 1, Ct), commit: false, withAdmin: false));
        await using ServiceProvider provider = BuildProvider(_admin);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => scope.ServiceProvider.GetRequiredService<ICorporateContentModule>().SaveBrandingAsync(new BrandingInput("Ayşe Butik", null, null, null, true), 1, Ct));

        Assert.Equal(new BrandingSection(null, null, null, null, true, 1), await RunAsync(m => m.GetBrandingAsync(Ct)));
        Assert.Empty(await ReadTrailAsync());
    }

    [Fact]
    public async Task ASave_ChangesNoOtherRow_ThanTheBrandingOne()
    {
        string before = await ScalarAsync<string>("SELECT count(*) || '|' || (SELECT count(*) FROM images) FROM admins");

        await RunAsync(m => m.SaveBrandingAsync(new BrandingInput("Ayşe Butik", null, null, "#FFFF00", false), 1, Ct));

        Assert.Equal(before, await ScalarAsync<string>("SELECT count(*) || '|' || (SELECT count(*) FROM images) FROM admins"));
        Assert.Equal(1, await ScalarAsync<long>("SELECT count(*) FROM brandings"));
    }

    // ---- Helpers ----

    private async Task<List<Image>> SeedImagesAsync(int count)
    {
        List<Image> images = [.. Enumerable.Range(0, count).Select(i => Image.Create($"images/{EntityIds.New():N}", ImageFormat.Jpeg, 800, 600, Now.AddSeconds(i)))];
        await using AppDbContext context = NewContext();
        context.Images.AddRange(images);
        await context.SaveChangesAsync(Ct);
        return images;
    }

    private async Task<List<AuditLog>> ReadTrailAsync()
    {
        await using AppDbContext context = NewContext();
        List<AuditLog> rows = await context.AuditLogs.AsNoTracking().ToListAsync(Ct);

        // The rows of one transaction share the fixed time; the order of writing is the order of the identifiers (time-ordered), and the branding row is written first.
        return [.. rows.OrderBy(l => l.CreatedAt).ThenBy(l => l.Id)];
    }

    private ServiceProvider BuildProvider(Admin? admin)
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
        services.AddScoped<IAuditActorSource>(_ => new FixedActor(admin));
        services.AddScoped<IAuditTrail, AuditTrail>();
        services.AddOptions<SiteOptions>().Configure(o => o.Domain = Domain);
        services.AddCatalogModule();
        services.AddSettingsModule();
        services.AddScoped<global::Shopfolio.Infrastructure.Security.AttemptCounter>();
        services.AddCorporateContentModule();
        return services.BuildServiceProvider();
    }

    /// <summary>Calls the module the way a command does: one scope, one context, inside the caller's transaction, committed (the unit of work saved first) or rolled back by the caller.</summary>
    private async Task<T> RunAsync<T>(Func<ICorporateContentModule, Task<T>> call, bool commit = true, bool withAdmin = true)
    {
        await using ServiceProvider provider = BuildProvider(withAdmin ? _admin : null);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(Ct);
        T result = await call(scope.ServiceProvider.GetRequiredService<ICorporateContentModule>());
        if (commit && result is not BrandingSaveResult { IsSaved: false })
        {
            await context.SaveChangesAsync(Ct);
            await transaction.CommitAsync(Ct);
        }
        else
        {
            await transaction.RollbackAsync(Ct);
        }

        return result;
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
