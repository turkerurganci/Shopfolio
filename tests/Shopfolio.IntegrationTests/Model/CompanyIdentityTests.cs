using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

using FieldRejection = Shopfolio.Modules.Settings.Contracts.FieldRejection;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the company identity tests (T330): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(CompanyIdentityDefinition.Name)]
public sealed class CompanyIdentityDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "CompanyIdentity";
}

/// <summary>
/// The company identity and the return address through the real Settings module on a real PostgreSQL (T330 criteria 1-8; KK1, KK3-KK5, KK8): the
/// save joins the caller's transaction, takes the settings row lock first, writes with the version the form carried and leaves nothing when the
/// transaction is rolled back; the empty required field does not stop a save, the filled one cannot be emptied, the type may change; the
/// visitor's read needs no session and shows the fields of the type; the return address has its own section version and a made declaration keeps
/// the address it copied.
/// </summary>
[Collection(CompanyIdentityDefinition.Name)]
public sealed class CompanyIdentityTests : IAsyncLifetime
{
    private const string TenDigitPhone = "3121234567";

    private static readonly DateTimeOffset Now = new(2026, 11, 6, 9, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly Today = new(2026, 11, 6);

    private readonly CollectionDatabaseFixture _collection;

    private Guid _provinceId;

    private Admin _admin = null!;

    private int _orderCounter;

    public CompanyIdentityTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE provinces, companies, company_settings, legal_text_versions, countries, admins, audit_logs CASCADE");
        _admin = Admin.Create("Ali Yılmaz", "ali@example.com", "hash", Now);
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
        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.Companies.Add(Company.Create(Now));
            c.Admins.Add(_admin);
            c.CompanySettings.Add(settings);
        });
        _provinceId = province.Id;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task NewInstallation_HasOneCompanyWithoutAType_AndEveryRequiredFieldIsWaitedFor()
    {
        CompanyIdentity identity = await InTransactionAsync(m => m.GetCompanyIdentityAsync(Ct));
        CompanyIdentityCompleteness completeness = await InTransactionAsync(m => m.GetCompanyIdentityCompletenessAsync(Ct));

        Assert.Null(identity.CompanyType);
        Assert.Equal(1, identity.Version);
        Assert.False(completeness.IsComplete);
        Assert.Equal(
            ["CompanyType", "ProvinceId", "District", "AddressLine", "Phone", "Email", "KepAddress", "ChamberName", "ProfessionalConductRules"],
            completeness.MissingFields);
    }

    [Fact]
    public async Task Save_StoresTheValuesNormalized_RaisesTheVersion_AndReadBackGivesThem()
    {
        SettingsSaveResult saved = await InTransactionAsync(
            m => m.SaveCompanyIdentityAsync(CompleteInput(CompanyType.Tradesman) with { Phone = " +90 (312) 123-45-67 ", District = "  Çankaya " }, 1, Ct));

        CompanyIdentity identity = await InTransactionAsync(m => m.GetCompanyIdentityAsync(Ct));

        Assert.True(saved.IsSaved);
        Assert.Equal(2, identity.Version);
        Assert.Equal(CompanyType.Tradesman, identity.CompanyType);
        Assert.Equal(TenDigitPhone, identity.Phone);
        Assert.Equal("Çankaya", identity.District);
        Assert.Equal(_provinceId, identity.ProvinceId);
        Assert.Equal("firma@hs01.kep.tr", identity.KepAddress);
    }

    [Theory]
    [InlineData(CompanyType.Tradesman)]
    [InlineData(CompanyType.NaturalPersonMerchant)]
    [InlineData(CompanyType.LegalEntity)]
    public async Task Save_OfACompleteIdentityOfAnyType_LeavesNothingWaiting_WithoutTheOptionalFields(CompanyType type)
    {
        SettingsSaveResult saved = await InTransactionAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(type), 1, Ct));

        CompanyIdentityCompleteness completeness = await InTransactionAsync(m => m.GetCompanyIdentityCompletenessAsync(Ct));
        CompanyIdentity identity = await InTransactionAsync(m => m.GetCompanyIdentityAsync(Ct));

        Assert.True(saved.IsSaved);
        Assert.True(completeness.IsComplete);
        Assert.Null(identity.BusinessName);
        Assert.Null(identity.EtbisVerificationUrl);
    }

    [Fact]
    public async Task Save_WithAnEmptyKepAddress_IsAccepted_AndTheGateWaitsForIt()
    {
        SettingsSaveResult saved = await InTransactionAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(CompanyType.Tradesman) with { KepAddress = null }, 1, Ct));

        CompanyIdentityCompleteness completeness = await InTransactionAsync(m => m.GetCompanyIdentityCompletenessAsync(Ct));

        Assert.True(saved.IsSaved);
        Assert.Equal(["KepAddress"], completeness.MissingFields);
    }

    [Fact]
    public async Task Save_EmptyingAFilledKepAddress_IsRefused_AndNothingIsWritten()
    {
        await InTransactionAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(CompanyType.Tradesman), 1, Ct));

        SettingsSaveResult refused = await InTransactionAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(CompanyType.Tradesman) with { KepAddress = " " }, 2, Ct));
        CompanyIdentity identity = await InTransactionAsync(m => m.GetCompanyIdentityAsync(Ct));

        Assert.False(refused.IsSaved);
        Assert.False(refused.IsVersionConflict);
        Assert.Equal([new FieldRejection("KepAddress", FieldCodes.FilledFieldCleared)], refused.Rejections);
        Assert.Equal("firma@hs01.kep.tr", identity.KepAddress);
        Assert.Equal(2, identity.Version);
    }

    [Fact]
    public async Task Save_EmptyingAnyFilledRequiredField_IsRefusedWithItsName()
    {
        await InTransactionAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(CompanyType.LegalEntity), 1, Ct));

        SettingsSaveResult refused = await InTransactionAsync(
            m => m.SaveCompanyIdentityAsync(CompleteInput(CompanyType.LegalEntity) with { MersisNumber = null, ChamberName = null, ProvinceId = null }, 2, Ct));

        Assert.Equal(["MersisNumber", "ProvinceId", "ChamberName"], refused.Rejections.Select(r => r.Field));
        Assert.All(refused.Rejections, r => Assert.Equal(FieldCodes.FilledFieldCleared, r.Code));
    }

    [Fact]
    public async Task Save_ChangingTheType_IsNeverRefused_AndAChosenTypeCannotBeCleared()
    {
        await InTransactionAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(CompanyType.Tradesman), 1, Ct));

        SettingsSaveResult changed = await InTransactionAsync(
            m => m.SaveCompanyIdentityAsync(CompleteInput(CompanyType.LegalEntity) with { TaxIdNumber = "1234567890" }, 2, Ct));
        SettingsSaveResult cleared = await InTransactionAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(CompanyType.LegalEntity) with { CompanyType = null }, 3, Ct));
        CompanyIdentityCompleteness completeness = await InTransactionAsync(m => m.GetCompanyIdentityCompletenessAsync(Ct));

        Assert.True(changed.IsSaved);
        Assert.Equal([new FieldRejection("CompanyType", FieldCodes.FilledFieldCleared)], cleared.Rejections);
        Assert.True(completeness.IsComplete);
    }

    [Fact]
    public async Task Save_WithAnOldVersion_IsAVersionConflict_AndWritesNothing()
    {
        await InTransactionAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(CompanyType.Tradesman), 1, Ct));

        SettingsSaveResult stale = await InTransactionAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(CompanyType.Tradesman) with { LegalName = "Başkası" }, 1, Ct));
        CompanyIdentity identity = await InTransactionAsync(m => m.GetCompanyIdentityAsync(Ct));

        Assert.True(stale.IsVersionConflict);
        Assert.Empty(stale.Rejections);
        Assert.Equal("Yılmaz Ticaret", identity.LegalName);
        Assert.Equal(2, identity.Version);
    }

    [Fact]
    public async Task Save_RolledBackWithTheCallersTransaction_LeavesNothing()
    {
        await InTransactionAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(CompanyType.Tradesman), 1, Ct), commit: false);

        CompanyIdentity identity = await InTransactionAsync(m => m.GetCompanyIdentityAsync(Ct));

        Assert.Null(identity.CompanyType);
        Assert.Equal(1, identity.Version);
    }

    [Fact]
    public async Task Save_WithWrongForms_IsRefusedWithEveryCode()
    {
        SettingsSaveResult refused = await InTransactionAsync(
            m => m.SaveCompanyIdentityAsync(
                CompleteInput(CompanyType.Tradesman) with
                {
                    Email = "yok",
                    KepAddress = "yok",
                    Phone = "123",
                    EtbisVerificationUrl = "yok",
                    ProfessionalConductRules = "https://",
                    ProvinceId = Guid.NewGuid(),
                },
                1,
                Ct));

        Assert.Equal(
            [
                ("Email", FieldCodes.InvalidEmail), ("KepAddress", FieldCodes.InvalidEmail), ("Phone", FieldCodes.InvalidPhone), ("EtbisVerificationUrl", FieldCodes.InvalidUrl),
                ("ProfessionalConductRules", FieldCodes.InvalidUrl), ("ProvinceId", FieldCodes.InvalidProvince),
            ],
            refused.Rejections.Select(r => (r.Field, r.Code)));
        Assert.Equal(1, (await InTransactionAsync(m => m.GetCompanyIdentityAsync(Ct))).Version);
    }

    [Fact]
    public async Task LegalIdentity_NeedsNoSession_ShowsTheFieldsOfTheType_AndTheEtbisLinkOnlyWhenFilled()
    {
        // A merchant's numbers and then a change to tradesman: the numbers stay in the row but the visitor sees only the tradesman's fields.
        await InTransactionAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(CompanyType.LegalEntity) with { BusinessName = "Marka" }, 1, Ct));
        await InTransactionAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(CompanyType.Tradesman) with { MersisNumber = "0123456789012345", TradeRegistryNumber = "123456" }, 2, Ct));

        LegalIdentity withoutLink = await InTransactionAsync(m => m.GetLegalIdentityAsync(Ct));
        await InTransactionAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(CompanyType.Tradesman) with { EtbisVerificationUrl = "https://etbis.example.com/d/1" }, 3, Ct));
        LegalIdentity withLink = await InTransactionAsync(m => m.GetLegalIdentityAsync(Ct));

        Assert.Equal(CompanyType.Tradesman, withoutLink.CompanyType);
        Assert.Equal("1234567890", withoutLink.TaxIdNumber);
        Assert.Null(withoutLink.MersisNumber);
        Assert.Null(withoutLink.TradeRegistryNumber);
        Assert.Equal("Ankara", withoutLink.ProvinceName);
        Assert.Equal("İstanbul Esnaf ve Sanatkarlar Odası", withoutLink.ChamberName);
        Assert.Equal("https://www.tobb.org.tr/etik-kurallar", withoutLink.ProfessionalConductRules);
        Assert.Null(withoutLink.EtbisVerificationUrl);
        Assert.Null(withoutLink.BusinessName);
        Assert.Equal("https://etbis.example.com/d/1", withLink.EtbisVerificationUrl);
    }

    [Fact]
    public async Task LegalIdentity_OfAMerchant_ShowsTheBusinessNameWhenFilled_AndNotTheTaxNumber()
    {
        await InTransactionAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(CompanyType.NaturalPersonMerchant) with { BusinessName = "Marka", TaxIdNumber = "1234567890" }, 1, Ct));

        LegalIdentity legal = await InTransactionAsync(m => m.GetLegalIdentityAsync(Ct));

        Assert.Equal("Marka", legal.BusinessName);
        Assert.Equal("0123456789012345", legal.MersisNumber);
        Assert.Equal("123456", legal.TradeRegistryNumber);
        Assert.Null(legal.TaxIdNumber);
    }

    [Fact]
    public async Task SecondCompanyRow_AndDeletingTheCompany_AreRefused()
    {
        string state = await ViolationOfAsync(c => c.Companies.Add(Company.Create(Now)));

        await using AppDbContext context = NewContext();
        context.Companies.Remove(await context.Companies.SingleAsync(Ct));

        Assert.Equal(PostgresErrorCodes.UniqueViolation, state);
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM companies"));
    }

    [Fact]
    public async Task ReturnAddress_IsEmptyAtFirst_AndTheFourPartsComeTogether()
    {
        ReturnAddressSection empty = await InTransactionAsync(m => m.GetReturnAddressAsync(Ct));
        SettingsSaveResult partial = await InTransactionAsync(m => m.SaveReturnAddressAsync(new ReturnAddressInput("Depo", _provinceId, null, null, null), 1, Ct));
        SettingsSaveResult phoneAlone = await InTransactionAsync(m => m.SaveReturnAddressAsync(new ReturnAddressInput(null, null, null, null, "0312 123 45 67"), 1, Ct));
        SettingsSaveResult nothing = await InTransactionAsync(m => m.SaveReturnAddressAsync(new ReturnAddressInput(null, null, null, null, null), 1, Ct));

        Assert.False(empty.IsComplete);
        Assert.Equal(1, empty.Version);
        Assert.Equal(["District", "AddressLine"], partial.Rejections.Select(r => r.Field));
        Assert.All(partial.Rejections, r => Assert.Equal(FieldCodes.FieldRequired, r.Code));
        Assert.Equal(["RecipientName", "ProvinceId", "District", "AddressLine"], phoneAlone.Rejections.Select(r => r.Field));
        Assert.True(nothing.IsSaved);
        Assert.False((await InTransactionAsync(m => m.GetReturnAddressAsync(Ct))).IsComplete);
    }

    [Fact]
    public async Task ReturnAddress_Saved_IsReadBack_WithTheTelephoneAsTenDigits_AndCannotBeEmptiedAfterwards()
    {
        SettingsSaveResult saved = await InTransactionAsync(m => m.SaveReturnAddressAsync(Address("0312 123 45 67"), 1, Ct));
        ReturnAddressSection section = await InTransactionAsync(m => m.GetReturnAddressAsync(Ct));
        SettingsSaveResult emptied = await InTransactionAsync(m => m.SaveReturnAddressAsync(new ReturnAddressInput(null, null, null, null, null), 2, Ct));
        SettingsSaveResult withoutPhone = await InTransactionAsync(m => m.SaveReturnAddressAsync(Address(null), 2, Ct));

        Assert.True(saved.IsSaved);
        Assert.Equal(new ReturnAddressSnapshot("Depo Sorumlusu", _provinceId, "Çankaya", "Depo Sk. 4", TenDigitPhone), section.Address);
        Assert.Equal(2, section.Version);
        Assert.Equal(["RecipientName", "ProvinceId", "District", "AddressLine"], emptied.Rejections.Select(r => r.Field));
        Assert.All(emptied.Rejections, r => Assert.Equal(FieldCodes.FilledFieldCleared, r.Code));
        Assert.True(withoutPhone.IsSaved);
        Assert.Null((await InTransactionAsync(m => m.GetReturnAddressAsync(Ct))).Address!.Phone);
    }

    [Fact]
    public async Task ReturnAddress_WithAnUnknownProvinceOrWrongPhone_IsRefused()
    {
        SettingsSaveResult refused = await InTransactionAsync(m => m.SaveReturnAddressAsync(Address("12") with { ProvinceId = Guid.NewGuid() }, 1, Ct));

        Assert.Equal(["Phone", "ProvinceId"], refused.Rejections.Select(r => r.Field).Order(StringComparer.Ordinal));
        Assert.False((await InTransactionAsync(m => m.GetReturnAddressAsync(Ct))).IsComplete);
    }

    [Fact]
    public async Task ReturnAddress_SavesBySectionVersion_AnotherSectionsNewerVersionDoesNotStopIt_ItsOwnOldVersionDoes()
    {
        await ExecuteAsync("UPDATE company_settings SET homepage_version = 5, shipping_fee_version = 7");

        SettingsSaveResult saved = await InTransactionAsync(m => m.SaveReturnAddressAsync(Address(null), 1, Ct));
        SettingsSaveResult stale = await InTransactionAsync(m => m.SaveReturnAddressAsync(Address(null) with { District = "Mamak" }, 1, Ct));
        ReturnAddressSection section = await InTransactionAsync(m => m.GetReturnAddressAsync(Ct));

        Assert.True(saved.IsSaved);
        Assert.True(stale.IsVersionConflict);
        Assert.Equal("Çankaya", section.Address!.District);
        Assert.Equal(2, section.Version);
        Assert.Equal(5, await ScalarAsync<int>("SELECT homepage_version FROM company_settings"));
        Assert.Equal(7, await ScalarAsync<int>("SELECT shipping_fee_version FROM company_settings"));
    }

    [Fact]
    public async Task ReturnAddress_RolledBack_LeavesNothing()
    {
        await InTransactionAsync(m => m.SaveReturnAddressAsync(Address(null), 1, Ct), commit: false);

        ReturnAddressSection section = await InTransactionAsync(m => m.GetReturnAddressAsync(Ct));

        Assert.False(section.IsComplete);
        Assert.Equal(1, section.Version);
    }

    [Fact]
    public async Task SettingsRowLock_IsTakenByBothSaves_SoASecondSaveWaitsForTheFirstTransaction()
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope first = provider.CreateAsyncScope();
        AppDbContext firstContext = first.ServiceProvider.GetRequiredService<AppDbContext>();
        await using IDbContextTransaction firstTransaction = await firstContext.Database.BeginTransactionAsync(Ct);
        Assert.True((await first.ServiceProvider.GetRequiredService<ISettingsModule>().SaveReturnAddressAsync(Address(null), 1, Ct)).IsSaved);

        Task<SettingsSaveResult> second = Task.Run(
            async () =>
            {
                await using AsyncServiceScope scope = provider.CreateAsyncScope();
                AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(Ct);
                SettingsSaveResult result = await scope.ServiceProvider.GetRequiredService<ISettingsModule>().SaveCompanyIdentityAsync(CompleteInput(CompanyType.Tradesman), 1, Ct);
                await transaction.CommitAsync(Ct);
                return result;
            },
            Ct);

        Task finished = await Task.WhenAny(second, Task.Delay(TimeSpan.FromSeconds(1.5), Ct));
        Assert.NotSame(second, finished);

        await firstTransaction.CommitAsync(Ct);
        SettingsSaveResult secondResult = await second.WaitAsync(TimeSpan.FromSeconds(30), Ct);

        Assert.True(secondResult.IsSaved);
    }

    [Fact]
    public async Task SettingsRowLock_OutsideATransaction_IsRefused()
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();

        await Assert.ThrowsAsync<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<ISettingsModule>().SaveReturnAddressAsync(Address(null), 1, Ct));
        await Assert.ThrowsAsync<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<ISettingsModule>().SaveCompanyIdentityAsync(CompleteInput(CompanyType.Tradesman), 1, Ct));
    }

    [Fact]
    public async Task Withdrawal_KeepsTheReturnAddressItCopied_WhenTheSettingChangesLater()
    {
        await InTransactionAsync(m => m.SaveReturnAddressAsync(Address("0312 123 45 67"), 1, Ct));
        ReturnAddressSnapshot copied = (await InTransactionAsync(m => m.GetReturnAddressAsync(Ct))).Address!;
        Guid itemId = await SeedItemAsync();
        Withdrawal withdrawal = Withdrawal.ByBuyer(
            itemId, 1, Today, new ReturnAddress(copied.RecipientName, copied.ProvinceId, copied.District, copied.AddressLine, copied.Phone), new ItemAmounts(100m, 10m, 15m), null, Now);
        await SaveAsync(c => c.Withdrawals.Add(withdrawal));

        SettingsSaveResult changed = await InTransactionAsync(m => m.SaveReturnAddressAsync(Address(null) with { RecipientName = "Yeni Depo", District = "Mamak", AddressLine = "Yeni Sk. 9" }, 2, Ct));

        await using AppDbContext reader = NewContext();
        Withdrawal stored = await reader.Withdrawals.SingleAsync(w => w.Id == withdrawal.Id, Ct);
        ReturnAddressSnapshot now = (await InTransactionAsync(m => m.GetReturnAddressAsync(Ct))).Address!;
        Assert.True(changed.IsSaved);
        Assert.Equal("Yeni Depo", now.RecipientName);
        Assert.Equal("Depo Sorumlusu", stored.ReturnRecipientName);
        Assert.Equal(_provinceId, stored.ReturnProvinceId);
        Assert.Equal("Çankaya", stored.ReturnDistrict);
        Assert.Equal("Depo Sk. 4", stored.ReturnAddressLine);
        Assert.Equal(TenDigitPhone, stored.ReturnPhone);
    }

    [Fact]
    public void Model_TheSevenSectionVersionsOfTheSettingsRow_AreNotConcurrencyTokens_AndTheCompanyVersionIs()
    {
        using AppDbContext context = NewContext();
        string[] sectionVersions =
        [
            nameof(CompanySettings.PaymentMethodsVersion), nameof(CompanySettings.ShippingFeeVersion), nameof(CompanySettings.DeliveryProvincesVersion),
            nameof(CompanySettings.PeriodsVersion), nameof(CompanySettings.ProductDefaultsVersion), nameof(CompanySettings.ReturnAddressVersion), nameof(CompanySettings.HomepageVersion),
        ];
        Microsoft.EntityFrameworkCore.Metadata.IEntityType settings = context.Model.FindEntityType(typeof(CompanySettings))!;

        Assert.Equal(7, sectionVersions.Length);
        Assert.All(sectionVersions, name => Assert.False(settings.FindProperty(name)!.IsConcurrencyToken, name));
        Assert.True(context.Model.FindEntityType(typeof(Company))!.FindProperty(nameof(Company.Version))!.IsConcurrencyToken);
    }

    private CompanyIdentityInput CompleteInput(CompanyType type)
    {
        bool tradesman = type == CompanyType.Tradesman;
        return new CompanyIdentityInput(
            type,
            "Yılmaz Ticaret",
            tradesman ? "1234567890" : null,
            tradesman ? null : "0123456789012345",
            tradesman ? null : "123456",
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
    }

    private ReturnAddressInput Address(string? phone) => new("Depo Sorumlusu", _provinceId, "Çankaya", "Depo Sk. 4", phone);

    private ServiceProvider BuildProvider()
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddScoped<CacheInvalidations>();
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddScoped<IAuditActorSource>(_ => new FixedActor(_admin));
        services.AddScoped<IAuditTrail, AuditTrail>();
        services.AddSettingsModule();
        return services.BuildServiceProvider();
    }

    /// <summary>Calls the module the way a command does: in one scope with one context, inside the caller's transaction, committed or rolled back by the caller.</summary>
    private async Task<T> InTransactionAsync<T>(Func<ISettingsModule, Task<T>> call, bool commit = true)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(Ct);
        T result = await call(scope.ServiceProvider.GetRequiredService<ISettingsModule>());
        if (commit)
        {
            await transaction.CommitAsync(Ct);
        }
        else
        {
            await transaction.RollbackAsync(Ct);
        }

        return result;
    }

    /// <summary>Seeds an order with one item for the declaration and returns the item's id.</summary>
    private async Task<Guid> SeedItemAsync()
    {
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, 90, Now, "metin", Now);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, 90, Now, "metin", Now);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, 1, Now, "metin", Now).WithHosting();
        await SaveAsync(c => c.LegalTextVersions.AddRange(form, contract, notice));
        int n = Interlocked.Increment(ref _orderCounter);
        Order order = Order.Create(
            $"2026-{n:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", PaymentMethod.Card, true, "Ali@Example.com", 120m, 0m, Now.AddHours(1), Now);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.BillingRecipientName = "Ali Veli";
        order.BillingProvinceId = _provinceId;
        order.BillingDistrict = "Çankaya";
        order.BillingAddressLine = "Atatürk Bulvarı 1";
        order.CompanyType = CompanyType.Tradesman;
        order.CompanyLegalName = "Örnek Ticaret";
        order.CompanyTaxIdNumber = "1234567890";
        order.CompanyProvinceId = _provinceId;
        order.CompanyDistrict = "Çankaya";
        order.CompanyAddressLine = "Kızılay 2";
        order.CompanyPhone = TenDigitPhone;
        order.CompanyEmail = "firma@example.com";
        order.CompanyKepAddress = "firma@hs01.kep.tr";
        order.PreInformationFormVersionId = form.Id;
        order.DistanceSalesContractVersionId = contract.Id;
        order.PrivacyNoticeVersionId = notice.Id;
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
        OrderItem item = OrderItem.Create(order.Id, null, null, ProductType.Physical, "Kapı", 3, 100m, 20m, 50m, Now);
        await SaveAsync(c =>
        {
            c.Orders.Add(order);
            c.OrderItems.Add(item);
        });
        return item.Id;
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

    private async Task<string> ViolationOfAsync(Action<AppDbContext> change)
    {
        await using AppDbContext context = NewContext();
        change(context);
        DbUpdateException error = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(Ct));
        return Assert.IsType<PostgresException>(error.InnerException).SqlState;
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

    /// <summary>The identity save writes a trail row (UpdateCompany), so a signed-in administrator is needed.</summary>
    private sealed class FixedActor(Admin admin) : IAuditActorSource
    {
        public Task<AuditActor?> CurrentAsync(CancellationToken ct) => Task.FromResult<AuditActor?>(new AuditActor(admin.Id, admin.Name, admin.Email));
    }
}
