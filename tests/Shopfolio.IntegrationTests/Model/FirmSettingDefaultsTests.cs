using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Migrations;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the firm setting defaults test (T381): one database copied from the template, made by the real migrations; the tests only read.</summary>
[CollectionDefinition(FirmSettingDefaultsDefinition.Name)]
public sealed class FirmSettingDefaultsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "FirmSettingDefaults";
}

/// <summary>
/// An empty installation through the real Settings module (T381 criterion 5; KK4, KK5; 02 10.1.3, 11.1): the firm settings come filled with the start values, every start
/// value is inside its own fence, and nothing has to be entered for the numbers to work - the shipping fee is zero, the two optional amounts are off, the periods and the allowance
/// are the table's. P-6, P-9 and P-44 are product-form fields and have no column in the settings row.
/// </summary>
[Collection(FirmSettingDefaultsDefinition.Name)]
public sealed class FirmSettingDefaultsTests
{
    private readonly CollectionDatabaseFixture _collection;

    public FirmSettingDefaultsTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task AnEmptyInstallation_HasEverySettingFilledWithItsStartValue_WithoutAnyEntry()
    {
        Dictionary<string, decimal?> start = InsertStartupData.SettingDefaults.ToDictionary(s => s.Parameter, s => s.Value, StringComparer.Ordinal);

        ShippingFeeSection fee = await ReadAsync(m => m.GetShippingFeeAsync(Ct));
        PeriodsSection periods = await ReadAsync(m => m.GetPeriodsAsync(Ct));
        ProductDefaultsSection defaults = await ReadAsync(m => m.GetProductDefaultsAsync(Ct));

        Assert.Equal(start["P-1"], fee.ShippingFee);
        Assert.Equal(start["P-3"], fee.FreeShippingThreshold);
        Assert.Equal(start["P-4"], fee.MinimumOrderAmount);
        Assert.Equal(start["P-5"], periods.ShippingPeriod);
        Assert.Equal(start["P-7"], periods.BankTransferPaymentWindow);
        Assert.Equal(start["P-8"], defaults.DownloadAllowance);
        Assert.Equal(start["P-10"], defaults.DefaultVatRate);
        Assert.All([fee.Version, periods.Version, defaults.Version], version => Assert.Equal(1, version));
    }

    [Fact]
    public async Task EveryStartValue_IsInsideItsOwnFence_SoTheFirstSaveOfTheFormNeverNeedsAChange()
    {
        PeriodsSection periods = await ReadAsync(m => m.GetPeriodsAsync(Ct));
        ProductDefaultsSection defaults = await ReadAsync(m => m.GetProductDefaultsAsync(Ct));

        Assert.True(FirmSettingFences.Check("P-5", periods.ShippingPeriod).IsWithin);
        Assert.True(FirmSettingFences.Check("P-7", periods.BankTransferPaymentWindow).IsWithin);
        Assert.True(FirmSettingFences.Check("P-8", defaults.DownloadAllowance).IsWithin);
    }

    [Fact]
    public async Task TheShippingFeeOfAnEmptyInstallation_IsZero_SoEveryOrderShipsFree()
    {
        ShippingFeeSection fee = await ReadAsync(m => m.GetShippingFeeAsync(Ct));

        Assert.Equal(0m, fee.ShippingFee);
        Assert.Null(fee.FreeShippingThreshold);
        Assert.Null(fee.MinimumOrderAmount);
    }

    [Fact]
    public async Task ThePerProductSettings_P6P9AndP44_HaveNoColumnInTheSettingsRow_AndTheInstallationSettingsHaveNone()
    {
        await using AppDbContext context = NewContext();
        string[] columns = await context.Database
            .SqlQuery<string>($"SELECT column_name AS \"Value\" FROM information_schema.columns WHERE table_name = 'company_settings'")
            .ToArrayAsync(Ct);

        Assert.DoesNotContain(columns, c => c.Contains("fulfilment", StringComparison.Ordinal) || c.Contains("max_quantity", StringComparison.Ordinal) || c.Contains("per_product", StringComparison.Ordinal));
        Assert.DoesNotContain(columns, c => c.Contains("domain", StringComparison.Ordinal) || c.Contains("sender", StringComparison.Ordinal) || c.Contains("provider_key", StringComparison.Ordinal)
            || c.Contains("client_secret", StringComparison.Ordinal) || c.Contains("client_id", StringComparison.Ordinal) || c.Contains("credential", StringComparison.Ordinal));
    }

    private async Task<T> ReadAsync<T>(Func<ISettingsModule, Task<T>> call)
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddScoped<CacheInvalidations>();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IAuditActorSource, NoActor>();
        services.AddScoped<IAuditTrail, AuditTrail>();
        services.AddSettingsModule();
        await using ServiceProvider provider = services.BuildServiceProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<ISettingsModule>());
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    private sealed class NoActor : IAuditActorSource
    {
        public Task<AuditActor?> CurrentAsync(CancellationToken ct) => Task.FromResult<AuditActor?>(null);
    }
}
