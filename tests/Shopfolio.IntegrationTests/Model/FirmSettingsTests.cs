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

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the firm setting save tests (T381): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(FirmSettingsDefinition.Name)]
public sealed class FirmSettingsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "FirmSettings";
}

/// <summary>
/// The saves of the numeric firm settings through the real Settings module on a real PostgreSQL (T381 criteria 4, 6, 9; KK4, KK6, KK9; 02 11.1, 10.3.1; 09 10.3): each section is
/// written by the conditional update of its own version, behind the settings row lock, in the caller's transaction; a value outside a fence is refused with the side and the fence and
/// nothing is stored; every changed field reaches the trail with its old and new value in the same transaction; a save that changes nothing writes nothing.
/// </summary>
[Collection(FirmSettingsDefinition.Name)]
public sealed class FirmSettingsTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    private Admin _admin = null!;

    public FirmSettingsTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        // The arrangement is a known state of the row, not the defaults: the fee zero, both amounts empty, periods and allowance as set here, every section at version 1.
        await ExecuteAsync("TRUNCATE audit_logs, admins CASCADE");
        await ExecuteAsync(
            "UPDATE company_settings SET shipping_fee = 0, free_shipping_threshold = NULL, minimum_order_amount = NULL, default_shipping_period = 2, "
            + "bank_transfer_payment_window = 2, download_allowance = 5, default_vat_rate = 20, shipping_fee_version = 1, periods_version = 1, product_defaults_version = 1");
        _admin = Admin.Create("Ali Yılmaz", "ali@example.com", "hash", Now);
        await using AppDbContext context = NewContext();
        context.Admins.Add(_admin);
        await context.SaveChangesAsync(Ct);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK4, KK9: a section is written with its own version and its trail row ----

    [Fact]
    public async Task SaveShippingFee_WritesTheSection_StepsOnlyItsVersion_AndWritesOneTrailRowWithTheChangedFields()
    {
        SettingsSaveResult result = await RunAsync(m => m.SaveShippingFeeAsync(new ShippingFeeInput(49.90m, 500m, null), 1, Ct));

        Assert.True(result.IsSaved);
        ShippingFeeSection section = await RunAsync(m => m.GetShippingFeeAsync(Ct));
        Assert.Equal(new ShippingFeeSection(49.90m, 500m, null, 2), section);
        Assert.Equal(1, await ScalarAsync<int>("SELECT periods_version + product_defaults_version - 1 FROM company_settings"));

        AuditLog row = Assert.Single(await ReadTrailAsync());
        Assert.Equal("UpdateSettings", row.Operation);
        Assert.Equal("CompanySettings", row.TargetRecordType);
        Assert.Equal(await ScalarAsync<Guid>("SELECT id FROM company_settings"), row.TargetRecordId);
        Assert.Equal(_admin.Id, row.AdminId);
        Assert.Equal(FirmSettings.ShippingFeeDisplayName, row.Details!.DisplayName);
        Assert.Equal(
            [new AuditValueChange("ShippingFee", "0.00", "49.90"), new AuditValueChange("FreeShippingThreshold", null, "500.00")],
            row.Details.Changes);
    }

    [Fact]
    public async Task SavePeriods_AndSaveProductDefaults_WriteTheirSectionsWithOldAndNewValues()
    {
        SettingsSaveResult periods = await RunAsync(m => m.SavePeriodsAsync(new PeriodsInput(10, 3), 1, Ct));
        SettingsSaveResult defaults = await RunAsync(m => m.SaveProductDefaultsAsync(new ProductDefaultsInput(1, 18m), 1, Ct));

        Assert.True(periods.IsSaved);
        Assert.True(defaults.IsSaved);
        Assert.Equal(new PeriodsSection(10, 3, 2), await RunAsync(m => m.GetPeriodsAsync(Ct)));
        Assert.Equal(new ProductDefaultsSection(1, 18m, 2), await RunAsync(m => m.GetProductDefaultsAsync(Ct)));
        Assert.Equal(1, await ScalarAsync<int>("SELECT shipping_fee_version FROM company_settings"));

        List<AuditLog> rows = await ReadTrailAsync();
        Assert.Equal(2, rows.Count);
        Assert.Equal(FirmSettings.PeriodsDisplayName, rows[0].Details!.DisplayName);
        Assert.Equal(
            [new AuditValueChange("DefaultShippingPeriod", "2", "10"), new AuditValueChange("BankTransferPaymentWindow", "2", "3")],
            rows[0].Details!.Changes);
        Assert.Equal(FirmSettings.ProductDefaultsDisplayName, rows[1].Details!.DisplayName);
        Assert.Equal(
            [new AuditValueChange("DownloadAllowance", "5", "1"), new AuditValueChange("DefaultVatRate", "20.00", "18.00")],
            rows[1].Details!.Changes);
    }

    [Fact]
    public async Task OnlyTheChangedFields_ReachTheTrail()
    {
        await RunAsync(m => m.SavePeriodsAsync(new PeriodsInput(2, 3), 1, Ct));

        AuditLog row = Assert.Single(await ReadTrailAsync());
        Assert.Equal(new AuditValueChange("BankTransferPaymentWindow", "2", "3"), Assert.Single(row.Details!.Changes));
    }

    // ---- KK6: the fences are applied in the panel's save ----

    [Theory]
    [InlineData(11, 2, nameof(PeriodsInput.ShippingPeriod), FenceSide.AboveUpper, 10)]
    [InlineData(-1, 2, nameof(PeriodsInput.ShippingPeriod), FenceSide.BelowLower, 0)]
    [InlineData(2, 0, nameof(PeriodsInput.BankTransferPaymentWindow), FenceSide.BelowLower, 1)]
    [InlineData(2, 4, nameof(PeriodsInput.BankTransferPaymentWindow), FenceSide.AboveUpper, 3)]
    public async Task APeriodOutsideItsFence_IsRefusedWithTheSideAndTheFence_AndNothingIsStored(int shipping, int window, string field, FenceSide side, int fence)
    {
        SettingsSaveResult result = await RunAsync(m => m.SavePeriodsAsync(new PeriodsInput(shipping, window), 1, Ct));

        Assert.False(result.IsSaved);
        Assert.False(result.IsVersionConflict);
        Assert.Equal(new Shopfolio.Modules.Settings.Contracts.FieldRejection(field, FieldCodes.InvalidQuantity, side, fence), Assert.Single(result.Rejections));
        Assert.Equal(new PeriodsSection(2, 2, 1), await RunAsync(m => m.GetPeriodsAsync(Ct)));
        Assert.Empty(await ReadTrailAsync());
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(10, 3)]
    public async Task ThePeriodsAtTheirFences_AreAccepted(int shipping, int window)
    {
        SettingsSaveResult result = await RunAsync(m => m.SavePeriodsAsync(new PeriodsInput(shipping, window), 1, Ct));

        Assert.True(result.IsSaved);
        Assert.Equal(new PeriodsSection(shipping, window, 2), await RunAsync(m => m.GetPeriodsAsync(Ct)));
    }

    [Fact]
    public async Task ADownloadAllowanceUnderOne_AndAnOutOfRangeVatRate_AreRefusedTogether_AndNothingIsStored()
    {
        SettingsSaveResult result = await RunAsync(m => m.SaveProductDefaultsAsync(new ProductDefaultsInput(0, 100m), 1, Ct));

        Assert.Equal(
            [
                new Shopfolio.Modules.Settings.Contracts.FieldRejection(nameof(ProductDefaultsInput.DownloadAllowance), FieldCodes.InvalidQuantity, FenceSide.BelowLower, 1),
                new Shopfolio.Modules.Settings.Contracts.FieldRejection(nameof(ProductDefaultsInput.DefaultVatRate), FieldCodes.InvalidPercentage),
            ],
            result.Rejections);
        Assert.Equal(new ProductDefaultsSection(5, 20m, 1), await RunAsync(m => m.GetProductDefaultsAsync(Ct)));
        Assert.Empty(await ReadTrailAsync());
    }

    [Theory]
    [InlineData(-0.01, nameof(ShippingFeeInput.ShippingFee))]
    [InlineData(10.001, nameof(ShippingFeeInput.ShippingFee))]
    [InlineData(10000000000.00, nameof(ShippingFeeInput.ShippingFee))]
    public async Task AnInvalidAmount_IsRefused_AndNothingIsStored(double amount, string field)
    {
        SettingsSaveResult result = await RunAsync(m => m.SaveShippingFeeAsync(new ShippingFeeInput((decimal)amount, null, null), 1, Ct));

        Assert.Equal(new Shopfolio.Modules.Settings.Contracts.FieldRejection(field, FieldCodes.InvalidAmount), Assert.Single(result.Rejections));
        Assert.Equal(new ShippingFeeSection(0m, null, null, 1), await RunAsync(m => m.GetShippingFeeAsync(Ct)));
        Assert.Empty(await ReadTrailAsync());
    }

    [Fact]
    public async Task AnEmptyFee_AndAnInvalidThreshold_AreRefusedTogether()
    {
        SettingsSaveResult result = await RunAsync(m => m.SaveShippingFeeAsync(new ShippingFeeInput(null, -5m, 0.001m), 1, Ct));

        Assert.Equal(
            [
                new Shopfolio.Modules.Settings.Contracts.FieldRejection(nameof(ShippingFeeInput.ShippingFee), FieldCodes.FieldRequired),
                new Shopfolio.Modules.Settings.Contracts.FieldRejection(nameof(ShippingFeeInput.FreeShippingThreshold), FieldCodes.InvalidAmount),
                new Shopfolio.Modules.Settings.Contracts.FieldRejection(nameof(ShippingFeeInput.MinimumOrderAmount), FieldCodes.InvalidAmount),
            ],
            result.Rejections);
    }

    [Fact]
    public async Task AFreeFee_AndEmptyOptionalAmounts_AreValid_AndAnAmountMayBeEmptiedAgain()
    {
        await RunAsync(m => m.SaveShippingFeeAsync(new ShippingFeeInput(0m, 750.50m, 100m), 1, Ct));
        SettingsSaveResult emptied = await RunAsync(m => m.SaveShippingFeeAsync(new ShippingFeeInput(0m, null, null), 2, Ct));

        Assert.True(emptied.IsSaved);
        Assert.Equal(new ShippingFeeSection(0m, null, null, 3), await RunAsync(m => m.GetShippingFeeAsync(Ct)));
        List<AuditLog> rows = await ReadTrailAsync();
        Assert.Equal(
            [new AuditValueChange("FreeShippingThreshold", "750.50", null), new AuditValueChange("MinimumOrderAmount", "100.00", null)],
            rows[1].Details!.Changes);
    }

    // ---- Versions, unchanged saves, the transaction and the actor ----

    [Fact]
    public async Task AStaleVersion_IsAConflict_AndWritesNeitherTheValuesNorATrailRow()
    {
        await RunAsync(m => m.SavePeriodsAsync(new PeriodsInput(5, 2), 1, Ct));

        SettingsSaveResult stale = await RunAsync(m => m.SavePeriodsAsync(new PeriodsInput(8, 3), 1, Ct));

        Assert.True(stale.IsVersionConflict);
        Assert.Equal(new PeriodsSection(5, 2, 2), await RunAsync(m => m.GetPeriodsAsync(Ct)));
        Assert.Single(await ReadTrailAsync());
    }

    [Fact]
    public async Task ASaveThatChangesNothing_WritesNothing_NoVersionStepAndNoTrailRow_AndAStaleOneIsStillAConflict()
    {
        SettingsSaveResult same = await RunAsync(m => m.SavePeriodsAsync(new PeriodsInput(2, 2), 1, Ct));
        await RunAsync(m => m.SavePeriodsAsync(new PeriodsInput(3, 2), 1, Ct));
        SettingsSaveResult staleSame = await RunAsync(m => m.SavePeriodsAsync(new PeriodsInput(3, 2), 1, Ct));

        Assert.True(same.IsSaved);
        Assert.True(staleSame.IsVersionConflict);
        Assert.Equal(new PeriodsSection(3, 2, 2), await RunAsync(m => m.GetPeriodsAsync(Ct)));
        Assert.Single(await ReadTrailAsync());
    }

    [Fact]
    public async Task TheSaveAndItsTrailRow_AreOneTransaction_ARollbackLeavesNeither()
    {
        SettingsSaveResult written = await RunAsync(m => m.SaveShippingFeeAsync(new ShippingFeeInput(25m, null, null), 1, Ct), commit: false);

        Assert.True(written.IsSaved);
        Assert.Equal(new ShippingFeeSection(0m, null, null, 1), await RunAsync(m => m.GetShippingFeeAsync(Ct)));
        Assert.Empty(await ReadTrailAsync());
    }

    [Fact]
    public async Task ASave_WithoutAnAdministrator_OrOutsideATransaction_IsRefused_AndNothingIsLeft()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => RunAsync(m => m.SavePeriodsAsync(new PeriodsInput(3, 2), 1, Ct), commit: false, withAdmin: false));
        await using ServiceProvider provider = BuildProvider(_admin);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        await Assert.ThrowsAsync<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<ISettingsModule>().SavePeriodsAsync(new PeriodsInput(3, 2), 1, Ct));

        Assert.Equal(new PeriodsSection(2, 2, 1), await RunAsync(m => m.GetPeriodsAsync(Ct)));
        Assert.Empty(await ReadTrailAsync());
    }

    [Fact]
    public async Task ASave_ChangesNoOtherColumnOfTheSettingsRow()
    {
        string before = await ScalarAsync<string>(OtherColumns);

        await RunAsync(m => m.SaveProductDefaultsAsync(new ProductDefaultsInput(7, 8m), 1, Ct));

        Assert.Equal(before, await ScalarAsync<string>(OtherColumns));
    }

    private const string OtherColumns =
        "SELECT concat_ws('|', shipping_fee, free_shipping_threshold, minimum_order_amount, default_shipping_period, bank_transfer_payment_window, is_sales_temporarily_closed, "
        + "menu_name_products, homepage_layout, shipping_fee_version, periods_version, payment_methods_version, homepage_version) FROM company_settings";

    // ---- Helpers ----

    private async Task<List<AuditLog>> ReadTrailAsync()
    {
        await using AppDbContext context = NewContext();
        return await context.AuditLogs.AsNoTracking().OrderBy(l => l.CreatedAt).ThenBy(l => l.Id).ToListAsync(Ct);
    }

    private ServiceProvider BuildProvider(Admin? admin)
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddScoped<CacheInvalidations>();
        services.AddSingleton<TimeProvider>(new FixedTime(Now));
        services.AddScoped<IAuditActorSource>(_ => new FixedActor(admin));
        services.AddScoped<IAuditTrail, AuditTrail>();
        services.AddSettingsModule();
        return services.BuildServiceProvider();
    }

    /// <summary>Calls the module the way a command does: one scope, one context, inside the caller's transaction, committed (the unit of work saved first) or rolled back by the caller.</summary>
    private async Task<T> RunAsync<T>(Func<ISettingsModule, Task<T>> call, bool commit = true, bool withAdmin = true)
    {
        await using ServiceProvider provider = BuildProvider(withAdmin ? _admin : null);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(Ct);
        T result = await call(scope.ServiceProvider.GetRequiredService<ISettingsModule>());
        if (commit)
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
