using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
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
        await ExecuteAsync("TRUNCATE audit_logs, dispatch_records, admins CASCADE");
        await ExecuteAsync("UPDATE company_settings SET is_bank_transfer_enabled = false, bank_transfer_iban = NULL, payment_methods_version = 1");
        await ExecuteAsync(
            "UPDATE company_settings SET shipping_fee = 0, free_shipping_threshold = NULL, minimum_order_amount = NULL, default_shipping_period = 2, "
            + "bank_transfer_payment_window = 2, download_allowance = 5, default_vat_rate = 20, shipping_fee_version = 1, periods_version = 1, product_defaults_version = 1");
        await ExecuteAsync(
            "UPDATE company_settings SET delivery_provinces_version = 1; DELETE FROM company_delivery_provinces; "
            + "INSERT INTO company_delivery_provinces (id, created_at, province_id) SELECT gen_random_uuid(), now(), id FROM provinces");
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
    [InlineData(11, 2, nameof(PeriodsInput.DefaultShippingPeriod), FenceSide.AboveUpper, 10)]
    [InlineData(-1, 2, nameof(PeriodsInput.DefaultShippingPeriod), FenceSide.BelowLower, 0)]
    [InlineData(2, 0, nameof(PeriodsInput.BankTransferPaymentWindow), FenceSide.BelowLower, 1)]
    [InlineData(2, 4, nameof(PeriodsInput.BankTransferPaymentWindow), FenceSide.AboveUpper, 3)]
    public async Task APeriodOutsideItsFence_IsRefusedWithTheSideAndTheFence_AndNothingIsStored(int shipping, int window, string field, FenceSide side, int fence)
    {
        SettingsSaveResult result = await RunAsync(m => m.SavePeriodsAsync(new PeriodsInput(shipping, window), 1, Ct));

        Assert.False(result.IsSaved);
        Assert.False(result.IsVersionConflict);
        Assert.Equal(new Shopfolio.Modules.Settings.Contracts.FieldRejection(field, nameof(ErrorCode.ValueOutOfBounds), side, fence), Assert.Single(result.Rejections));
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
                new Shopfolio.Modules.Settings.Contracts.FieldRejection(nameof(ProductDefaultsInput.DownloadAllowance), nameof(ErrorCode.ValueOutOfBounds), FenceSide.BelowLower, 1),
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

    // ---- T350 criterion 1: the delivery provinces section ----

    [Fact]
    public async Task TheDeliveryProvinces_StartAsAllEightyOne_AndTheAreaIsNotRestricted()
    {
        DeliveryProvincesSection section = await RunAsync(m => m.GetDeliveryProvincesAsync(Ct));
        DeliveryArea area = await RunAsync(m => m.GetDeliveryAreaAsync(Ct));

        Assert.Equal(81, section.ProvinceIds.Count);
        Assert.Equal(1, section.Version);
        Assert.False(area.IsRestricted);
        Assert.Null(area.RestrictionLine());
    }

    [Fact]
    public async Task SaveDeliveryProvinces_WritesTheSelection_StepsOnlyItsVersion_AndWritesOneTrailRowWithPlateCodes()
    {
        List<ProvinceInfo> all = [.. await RunAsync(m => m.GetProvincesAsync(Ct))];
        Guid[] chosen = [.. all.Where(p => p.Code is "06" or "34" or "35").Select(p => p.Id)];

        SettingsSaveResult result = await RunAsync(m => m.SaveDeliveryProvincesAsync(chosen, 1, Ct));

        Assert.True(result.IsSaved);
        DeliveryProvincesSection section = await RunAsync(m => m.GetDeliveryProvincesAsync(Ct));
        Assert.Equal(2, section.Version);
        Assert.Equal(chosen.Order(), section.ProvinceIds.Order());
        Assert.Equal(1, await ScalarAsync<int>("SELECT periods_version + shipping_fee_version - 1 FROM company_settings"));
        Assert.Equal(3, await ScalarAsync<int>("SELECT count(*)::int FROM company_delivery_provinces"));

        DeliveryArea area = await RunAsync(m => m.GetDeliveryAreaAsync(Ct));
        Assert.True(area.IsRestricted);
        Assert.Equal(78, area.NotServed.Count);
        Assert.Equal("Teslimat yapılan iller: Ankara, İstanbul, İzmir", area.RestrictionLine()!.Text);

        AuditLog row = Assert.Single(await ReadTrailAsync());
        Assert.Equal("UpdateSettings", row.Operation);
        Assert.Equal(await ScalarAsync<Guid>("SELECT id FROM company_settings"), row.TargetRecordId);
        Assert.Equal(_admin.Id, row.AdminId);
        Assert.Equal("Teslimat illeri", row.Details!.DisplayName);
        AuditValueChange change = Assert.Single(row.Details.Changes);
        Assert.Equal("DeliveryProvinces", change.Field);
        Assert.Equal("06,34,35", change.NewValue);
        Assert.StartsWith("01,02,03", change.OldValue, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SaveDeliveryProvinces_WithTheSameSelection_WritesNothing_AndAStaleVersionIsAConflict()
    {
        Guid[] all = [.. (await RunAsync(m => m.GetDeliveryProvincesAsync(Ct))).ProvinceIds];

        SettingsSaveResult same = await RunAsync(m => m.SaveDeliveryProvincesAsync(all, 1, Ct));
        await RunAsync(m => m.SaveDeliveryProvincesAsync(all.Take(10).ToArray(), 1, Ct));
        SettingsSaveResult stale = await RunAsync(m => m.SaveDeliveryProvincesAsync(all.Take(20).ToArray(), 1, Ct));
        SettingsSaveResult staleSame = await RunAsync(m => m.SaveDeliveryProvincesAsync(all.Take(10).ToArray(), 1, Ct));

        Assert.True(same.IsSaved);
        Assert.True(stale.IsVersionConflict);
        Assert.True(staleSame.IsVersionConflict);
        Assert.Equal(10, await ScalarAsync<int>("SELECT count(*)::int FROM company_delivery_provinces"));
        Assert.Equal(2, await ScalarAsync<int>("SELECT delivery_provinces_version FROM company_settings"));
        Assert.Single(await ReadTrailAsync());
    }

    [Fact]
    public async Task SaveDeliveryProvinces_RefusesAnIdThatIsNotOneOfTheEightyOne_AndAnEmptySelectionIsValid()
    {
        SettingsSaveResult unknown = await RunAsync(m => m.SaveDeliveryProvincesAsync([Guid.NewGuid()], 1, Ct));
        SettingsSaveResult none = await RunAsync(m => m.SaveDeliveryProvincesAsync([], 1, Ct));

        Assert.False(unknown.IsSaved);
        Assert.Equal(FieldCodes.InvalidProvince, Assert.Single(unknown.Rejections).Code);
        Assert.True(none.IsSaved);
        DeliveryArea area = await RunAsync(m => m.GetDeliveryAreaAsync(Ct));
        Assert.True(area.DeliversNowhere);
        Assert.Equal("Şu an hiçbir ile teslimat yapılmıyor.", area.RestrictionLine()!.Text);
        Assert.Equal(0, await ScalarAsync<int>("SELECT count(*)::int FROM company_delivery_provinces"));
    }

    // ---- T353 KK1, KK3: the bank transfer switch and its IBAN (02 3.21.5, 9.3.3) ----

    private const string Iban = "TR330006100519786457841326";

    private const string OtherIban = "TR320010009999901234567890";

    [Fact]
    public async Task SwitchingTheTransferOnWithoutAnIban_IsRefused_AndNothingIsStoredNorTold_T353_KK1()
    {
        SettingsSaveResult empty = await RunPaymentMethodsAsync(m => m.SaveAsync(new PaymentMethodsInput(true, null), 1, Ct));
        SettingsSaveResult blank = await RunPaymentMethodsAsync(m => m.SaveAsync(new PaymentMethodsInput(true, "   "), 1, Ct));
        SettingsSaveResult invalid = await RunPaymentMethodsAsync(m => m.SaveAsync(new PaymentMethodsInput(true, "TR33 0006 1005 1978 6457 8413 27"), 1, Ct));

        Assert.Equal(
            new Shopfolio.Modules.Settings.Contracts.FieldRejection(nameof(PaymentMethodsInput.BankTransferIban), FieldCodes.FieldRequired),
            Assert.Single(empty.Rejections));
        Assert.Equal(empty.Rejections, blank.Rejections);
        Assert.Equal(
            new Shopfolio.Modules.Settings.Contracts.FieldRejection(nameof(PaymentMethodsInput.BankTransferIban), FieldCodes.InvalidIban),
            Assert.Single(invalid.Rejections));
        Assert.Equal(new PaymentMethodsSection(false, null, 1), await RunPaymentMethodsAsync(m => m.GetAsync(Ct)));
        Assert.Empty(await ReadTrailAsync());
        Assert.Empty(await ReadDispatchesAsync());
        Assert.False(await ConditionAsync());
    }

    [Fact]
    public async Task TheTransferWithItsIban_OpensTheLine_AndSwitchedOffClosesIt_KeepingTheIban_T353_KK1()
    {
        SettingsSaveResult on = await RunPaymentMethodsAsync(m => m.SaveAsync(new PaymentMethodsInput(true, "tr33 0006 1005 1978 6457 8413 26"), 1, Ct));
        bool openAfterOn = await ConditionAsync();
        SettingsSaveResult off = await RunPaymentMethodsAsync(m => m.SaveAsync(new PaymentMethodsInput(false, Iban), 2, Ct));
        bool openAfterOff = await ConditionAsync();

        Assert.True(on.IsSaved);
        Assert.True(openAfterOn);
        Assert.True(off.IsSaved);
        Assert.False(openAfterOff);
        Assert.Equal(new PaymentMethodsSection(false, Iban, 3), await RunPaymentMethodsAsync(m => m.GetAsync(Ct)));

        List<AuditLog> rows = await ReadTrailAsync();
        Assert.Equal(2, rows.Count);
        Assert.All(rows, r => Assert.Equal(PaymentMethodSettings.DisplayName, r.Details!.DisplayName));
        Assert.Equal(
            [new AuditValueChange("IsBankTransferEnabled", "false", "true"), new AuditValueChange("BankTransferIban", null, Iban)],
            rows[0].Details!.Changes);
        Assert.Equal([new AuditValueChange("IsBankTransferEnabled", "true", "false")], rows[1].Details!.Changes);
    }

    [Fact]
    public async Task TheIbanEnteredAndChanged_TellsEveryAdministrator_WithTheLastFourDigitsOnly_T353_KK3()
    {
        Admin other = Admin.Create("Ayşe Demir", "ayse@example.com", "hash", Now);
        await using (AppDbContext context = NewContext())
        {
            context.Admins.Add(other);
            await context.SaveChangesAsync(Ct);
        }

        await RunPaymentMethodsAsync(m => m.SaveAsync(new PaymentMethodsInput(true, Iban), 1, Ct));
        List<DispatchRecord> entered = await ReadDispatchesAsync();
        await RunPaymentMethodsAsync(m => m.SaveAsync(new PaymentMethodsInput(true, OtherIban), 2, Ct));
        List<DispatchRecord> all = await ReadDispatchesAsync();

        Assert.Equal(["ali@example.com", "ayse@example.com"], entered.Select(d => d.RecipientEmail));
        Assert.Equal(4, all.Count);
        Assert.All(all, d => Assert.Equal(Shopfolio.Modules.Notifications.Contracts.DispatchType.F5, d.DispatchType));
        Assert.DoesNotContain(all, d => d.RecipientEmail == "firma@example.com");
        Assert.DoesNotContain(all, d => (d.EventValues ?? string.Empty).Contains(Iban, StringComparison.Ordinal)
            || (d.EventValues ?? string.Empty).Contains(OtherIban, StringComparison.Ordinal));
        AuditLog change = (await ReadTrailAsync())[^1];
        Assert.Equal([new AuditValueChange("BankTransferIban", Iban, OtherIban)], change.Details!.Changes);
    }

    [Fact]
    public async Task ASaveThatLeavesTheIbanAsItIs_TellsNoOne_T353_KK3()
    {
        await RunPaymentMethodsAsync(m => m.SaveAsync(new PaymentMethodsInput(false, Iban), 1, Ct));
        await RunPaymentMethodsAsync(m => m.SaveAsync(new PaymentMethodsInput(true, "TR33 0006 1005 1978 6457 8413 26"), 2, Ct));
        SettingsSaveResult same = await RunPaymentMethodsAsync(m => m.SaveAsync(new PaymentMethodsInput(true, Iban), 3, Ct));

        Assert.True(same.IsSaved);
        Assert.Single(await ReadDispatchesAsync());
        Assert.Equal(2, (await ReadTrailAsync()).Count);
        Assert.Equal(new PaymentMethodsSection(true, Iban, 3), await RunPaymentMethodsAsync(m => m.GetAsync(Ct)));
    }

    [Fact]
    public async Task APaymentMethodsSaveOnAStaleVersion_IsAConflict_AndTellsNoOne()
    {
        await RunPaymentMethodsAsync(m => m.SaveAsync(new PaymentMethodsInput(true, Iban), 1, Ct));

        SettingsSaveResult stale = await RunPaymentMethodsAsync(m => m.SaveAsync(new PaymentMethodsInput(true, OtherIban), 1, Ct));

        Assert.True(stale.IsVersionConflict);
        Assert.Single(await ReadDispatchesAsync());
        Assert.Equal(new PaymentMethodsSection(true, Iban, 2), await RunPaymentMethodsAsync(m => m.GetAsync(Ct)));
    }

    private async Task<bool> ConditionAsync()
    {
        await using ServiceProvider provider = BuildProvider(_admin);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IBankTransferCondition>().IsOpenAsync(Ct);
    }

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

        // The payment methods save tells every administrator about a new IBAN (T353; 02 9.3.3 F-5): the Administration side and the Notifications queue it writes through.
        services.AddScoped<IAdminNotices, AdminNotices>();
        services.AddScoped<INotificationsModule, NotificationsModule>();
        services.AddScoped<CustomerNoticeQueue>();
        services.AddScoped<CompanyNoticeQueue>();
        services.AddScoped<AccountEmailQueue>();
        services.AddScoped<ICompanyContactEmail>(_ => new FixedContactEmail());
        services.AddOptions<LinkKeyOptions>();
        services.AddSingleton<LinkKeys>();
        services.AddSettingsModule();
        return services.BuildServiceProvider();
    }

    private sealed class FixedContactEmail : ICompanyContactEmail
    {
        public Task<string?> GetAsync(CancellationToken ct) => Task.FromResult<string?>("firma@example.com");
    }

    private async Task<T> RunPaymentMethodsAsync<T>(Func<IPaymentMethodSettings, Task<T>> call, bool withAdmin = true)
    {
        await using ServiceProvider provider = BuildProvider(withAdmin ? _admin : null);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(Ct);
        T result = await call(scope.ServiceProvider.GetRequiredService<IPaymentMethodSettings>());
        await context.SaveChangesAsync(Ct);
        await transaction.CommitAsync(Ct);
        return result;
    }

    private async Task<List<DispatchRecord>> ReadDispatchesAsync()
    {
        await using AppDbContext context = NewContext();
        return await context.DispatchRecords.AsNoTracking().OrderBy(d => d.RecipientEmail).ToListAsync(Ct);
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
