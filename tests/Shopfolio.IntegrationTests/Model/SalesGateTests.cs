using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the sales gate tests (T331): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(SalesGateDefinition.Name)]
public sealed class SalesGateDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "SalesGate";
}

/// <summary>
/// The sales gate and the temporary closing of sales through the real Settings module on a real PostgreSQL (T331 criteria 1-8; KK1-KK8): the gate is read from the
/// database on every call and is open only when the four conditions hold together; each broken condition is named; the data-collecting entries have their own door and the
/// sign-in of an existing account is not behind it; a condition nobody has bound keeps the gate closed; the switch is written against the shown value with its trail row in the
/// same transaction; and nothing that closes the gate - the switch, a broken identity, an emptied return address, a closed transfer - touches an open order or a published legal
/// text.
/// </summary>
[Collection(SalesGateDefinition.Name)]
public sealed class SalesGateTests : IAsyncLifetime
{
    private const string TenDigitPhone = "3121234567";

    private const string FrozenIban = "TR330006100519786457841326";

    private static readonly DateTimeOffset Now = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    private Guid _provinceId;

    private Admin _admin = null!;

    private int _orderCounter;

    public SalesGateTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public static TheoryData<SalesGateCondition> EveryCondition => [.. Enum.GetValues<SalesGateCondition>()];

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE provinces, companies, company_settings, legal_text_versions, countries, admins, audit_logs CASCADE");
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
        });
        _provinceId = province.Id;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the four conditions together; the unbound ones are not met ----

    [Fact]
    public async Task Gate_IsOpen_WhenTheFourConditionsHoldTogether()
    {
        await ArrangeAsync(broken: null);

        SalesGate gate = await ReadGateAsync(Bindings.AllOpen);

        Assert.True(gate.IsOpen);
        Assert.Empty(gate.UnmetConditions);
        Assert.Empty(gate.MissingIdentityFields);
        Assert.True(gate.IsDataCollectionOpen);
        Assert.All(Enum.GetValues<SalesGateOperation>(), operation => Assert.True(gate.Permits(operation)));
    }

    [Theory]
    [MemberData(nameof(EveryCondition))]
    public async Task Gate_IsClosedWithEachConditionBroken_AndNamesIt(SalesGateCondition broken)
    {
        await ArrangeAsync(broken);

        SalesGate gate = await ReadGateAsync(BindingsWithout(broken));

        Assert.False(gate.IsOpen);
        Assert.Contains(broken, gate.UnmetConditions);
        Assert.All(new[] { SalesGateOperation.AddToCart, SalesGateOperation.ProceedToCheckout, SalesGateOperation.ConfirmOrder }, operation => Assert.False(gate.Permits(operation)));
        Assert.Equal(broken == SalesGateCondition.CompanyIdentity, gate.MissingIdentityFields.Count > 0);
    }

    [Fact]
    public async Task Gate_NamesTheIdentityFieldsThatStillWait_AndOpensWhenTheyAreFilled()
    {
        await ArrangeAsync(SalesGateCondition.CompanyIdentity);

        SalesGate waiting = await ReadGateAsync(Bindings.AllOpen);
        await RunAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(CompanyType.Tradesman), 1, Ct), admin: _admin);
        SalesGate filled = await ReadGateAsync(Bindings.AllOpen);

        Assert.Equal(["CompanyType", "ProvinceId", "District", "AddressLine", "Phone", "Email", "KepAddress", "ChamberName", "ProfessionalConductRules"], waiting.MissingIdentityFields);
        Assert.Equal([SalesGateCondition.CompanyIdentity, SalesGateCondition.PrivacyNotice], waiting.UnmetConditions);
        Assert.True(filled.IsOpen);
    }

    [Fact]
    public async Task UnboundConditions_KeepTheGateClosed_UntilTheirTasksBindThem()
    {
        // The transfer line is the module's own binding (T353): switched off, only the unbound card line is left for the payment condition.
        await ArrangeAsync(broken: null);
        await ExecuteAsync("UPDATE company_settings SET is_bank_transfer_enabled = false");

        SalesGate gate = await ReadGateAsync(Bindings.None);

        Assert.False(gate.IsOpen);
        Assert.Equal([SalesGateCondition.PaymentMethod, SalesGateCondition.PrivacyNotice], gate.UnmetConditions);
        Assert.False(gate.IsDataCollectionOpen);
    }

    [Fact]
    public async Task Gate_IsReadFromTheDatabaseOnEveryCall_NotStoredNorCached()
    {
        await ArrangeAsync(broken: null);
        await using ServiceProvider provider = BuildProvider(Bindings.AllOpen);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        ISettingsModule module = scope.ServiceProvider.GetRequiredService<ISettingsModule>();

        SalesGate before = await module.GetSalesGateAsync(Ct);
        await ExecuteAsync("UPDATE company_settings SET is_sales_temporarily_closed = true");
        SalesGate afterTheSwitch = await module.GetSalesGateAsync(Ct);
        await ExecuteAsync("UPDATE company_settings SET is_sales_temporarily_closed = false, return_recipient_name = NULL, return_province_id = NULL, return_district = NULL, return_address_line = NULL");
        SalesGate afterTheAddress = await module.GetSalesGateAsync(Ct);

        Assert.True(before.IsOpen);
        Assert.Equal([SalesGateCondition.TemporaryClosure], afterTheSwitch.UnmetConditions);
        Assert.Equal([SalesGateCondition.ReturnAddress], afterTheAddress.UnmetConditions);
    }

    // ---- KK2: closed gate - the adding, the payment step and the confirmation are refused with the missing condition named; the showcase and the cart stay ----

    [Fact]
    public async Task ClosedGate_RefusesTheAddingTheCheckoutAndTheConfirmation_NamingTheMissingCondition_AndKeepsTheShowcaseTheCartAndTheOpenOrders()
    {
        await ArrangeAsync(SalesGateCondition.ReturnAddress);

        SalesGate gate = await ReadGateAsync(Bindings.AllOpen);

        foreach (SalesGateOperation operation in new[] { SalesGateOperation.AddToCart, SalesGateOperation.ProceedToCheckout, SalesGateOperation.ConfirmOrder })
        {
            GateRefusal refusal = Assert.IsType<GateRefusal>(gate.RefusalOf(operation));
            Assert.Equal(ErrorCode.SalesClosed, refusal.Code);
            Assert.Equal([SalesGateCondition.ReturnAddress], refusal.UnmetConditions);
        }

        Assert.True(gate.ShowsClosedNotice);
        Assert.Null(gate.RefusalOf(SalesGateOperation.BrowseShowcase));
        Assert.Null(gate.RefusalOf(SalesGateOperation.KeepExistingCart));
        Assert.Null(gate.RefusalOf(SalesGateOperation.ContinueOpenOrder));
    }

    // ---- KK3: the data-collecting entries ----

    [Fact]
    public async Task DataCollectingEntries_AreRefusedUntilThePrivacyNoticeAndTheCookiePolicyArePublished_AndTheSignInOfAnExistingAccountIsNot()
    {
        await ArrangeAsync(SalesGateCondition.CookiePolicy);

        SalesGate noCookie = await ReadGateAsync(Bindings.AllOpen);
        SalesGate noNotice = await ReadGateAsync(BindingsWithout(SalesGateCondition.PrivacyNotice));
        await PublishCookiePolicyAsync();
        // before the cookie policy is published, both are named; after, only the notice
        Assert.Equal([SalesGateCondition.PrivacyNotice, SalesGateCondition.CookiePolicy], noNotice.RefusalOf(DataCollectionEntry.AccountRegistration)!.UnmetConditions);
        SalesGate noNoticeButCookie = await ReadGateAsync(BindingsWithout(SalesGateCondition.PrivacyNotice));
        SalesGate both = await ReadGateAsync(Bindings.AllOpen);

        foreach (DataCollectionEntry entry in new[] { DataCollectionEntry.AccountRegistration, DataCollectionEntry.NewAccountGoogleSignIn, DataCollectionEntry.ContactForm })
        {
            Assert.Equal(ErrorCode.DataCollectionClosed, noCookie.RefusalOf(entry)!.Code);
            Assert.Equal([SalesGateCondition.CookiePolicy], noCookie.RefusalOf(entry)!.UnmetConditions);
            Assert.Equal([SalesGateCondition.PrivacyNotice, SalesGateCondition.CookiePolicy], noNotice.RefusalOf(entry)!.UnmetConditions);
            Assert.Equal([SalesGateCondition.PrivacyNotice], noNoticeButCookie.RefusalOf(entry)!.UnmetConditions);
            Assert.Null(both.RefusalOf(entry));
        }

        Assert.All(new[] { noCookie, noNotice, noNoticeButCookie, both }, gate => Assert.True(gate.Permits(DataCollectionEntry.ExistingAccountSignIn)));
    }

    [Fact]
    public async Task DataCollectingEntries_AreClosedWhileTheIdentityIsIncomplete_BecauseThePrivacyNoticeIsNotComplete()
    {
        await ArrangeAsync(SalesGateCondition.CompanyIdentity);

        SalesGate gate = await ReadGateAsync(Bindings.AllOpen);

        Assert.False(gate.IsDataCollectionOpen);
        Assert.Equal([SalesGateCondition.PrivacyNotice], gate.RefusalOf(DataCollectionEntry.AccountRegistration)!.UnmetConditions);
        Assert.NotEmpty(gate.RefusalOf(DataCollectionEntry.AccountRegistration)!.MissingIdentityFields);
    }

    [Fact]
    public async Task APaymentMethodOrAReturnAddressMissing_ClosesSalesButNotTheDataCollectingEntries()
    {
        await ArrangeAsync(SalesGateCondition.ReturnAddress);

        SalesGate noAddress = await ReadGateAsync(Bindings.AllOpen);
        SalesGate noPayment = await ReadGateAsync(BindingsWithout(SalesGateCondition.PaymentMethod));

        Assert.False(noAddress.IsOpen);
        Assert.True(noAddress.IsDataCollectionOpen);
        Assert.False(noPayment.IsOpen);
        Assert.True(noPayment.IsDataCollectionOpen);
    }

    // ---- KK4: the switch and its trail row ----

    [Fact]
    public async Task CloseSales_WritesTheSwitchAndOneTrailRow_AndReopeningWritesAnother()
    {
        await ArrangeAsync(broken: null);

        SalesSwitchResult closed = await RunAsync(m => m.CloseSalesTemporarilyAsync(false, Ct), Bindings.AllOpen, _admin);
        SalesGate whileClosed = await ReadGateAsync(Bindings.AllOpen);
        SalesSwitchResult reopened = await RunAsync(m => m.ReopenSalesAsync(true, Ct), Bindings.AllOpen, _admin);

        Assert.True(closed.IsWritten);
        Assert.True(closed.IsClosedNow);
        Assert.True(closed.Gate.IsTemporarilyClosed);
        Assert.Equal([SalesGateCondition.TemporaryClosure], whileClosed.UnmetConditions);
        Assert.False(whileClosed.Permits(SalesGateOperation.AddToCart));
        Assert.True(reopened.IsWritten);
        Assert.False(reopened.IsClosedNow);
        Assert.True(reopened.Gate.IsOpen);

        List<AuditLog> rows = await ReadTrailAsync();
        Assert.Equal(2, rows.Count);
        Assert.All(rows, row =>
        {
            Assert.Equal("UpdateSettings", row.Operation);
            Assert.Equal(AuditArea.Settings, row.AuditArea);
            Assert.Equal("CompanySettings", row.TargetRecordType);
            Assert.Equal(_admin.Id, row.AdminId);
            Assert.Equal(SalesSwitches.SwitchDisplayName, row.Details!.DisplayName);
        });
        Assert.Equal(new AuditValueChange("IsSalesTemporarilyClosed", "false", "true"), Assert.Single(rows[0].Details!.Changes));
        Assert.Equal(new AuditValueChange("IsSalesTemporarilyClosed", "true", "false"), Assert.Single(rows[1].Details!.Changes));
        Assert.Equal(await ScalarAsync<Guid>("SELECT id FROM company_settings"), rows[0].TargetRecordId);
    }

    [Fact]
    public async Task TheSwitch_WithAStaleShownValue_OrAlreadyInTheAskedState_WritesNothing_AndLeavesNoTrailRow()
    {
        await ArrangeAsync(broken: null);
        await RunAsync(m => m.CloseSalesTemporarilyAsync(false, Ct), Bindings.AllOpen, _admin);

        SalesSwitchResult staleClose = await RunAsync(m => m.CloseSalesTemporarilyAsync(false, Ct), Bindings.AllOpen, _admin);
        SalesSwitchResult repeatedClose = await RunAsync(m => m.CloseSalesTemporarilyAsync(true, Ct), Bindings.AllOpen, _admin);
        await RunAsync(m => m.ReopenSalesAsync(true, Ct), Bindings.AllOpen, _admin);
        SalesSwitchResult staleReopen = await RunAsync(m => m.ReopenSalesAsync(true, Ct), Bindings.AllOpen, _admin);
        SalesSwitchResult repeatedReopen = await RunAsync(m => m.ReopenSalesAsync(false, Ct), Bindings.AllOpen, _admin);

        Assert.All(new[] { staleClose, repeatedClose, staleReopen, repeatedReopen }, result =>
        {
            Assert.True(result.IsSwitchChanged);
            Assert.True(result.IsRejected);
            Assert.False(result.IsWritten);
        });
        Assert.True(staleClose.IsClosedNow);
        Assert.True(repeatedClose.IsClosedNow);
        Assert.False(staleReopen.IsClosedNow);
        Assert.Equal(2, (await ReadTrailAsync()).Count); // the one close and the one reopen; the four refused calls left no row
        Assert.False(await ScalarAsync<bool>("SELECT is_sales_temporarily_closed FROM company_settings"));
    }

    [Fact]
    public async Task Reopen_WhileAnotherConditionIsUnmet_TurnsTheSwitchOff_ButSalesStayClosedAndTheGateNamesThem()
    {
        await ArrangeAsync(SalesGateCondition.PaymentMethod);
        await RunAsync(m => m.CloseSalesTemporarilyAsync(false, Ct), BindingsWithout(SalesGateCondition.PaymentMethod), _admin);

        SalesSwitchResult reopened = await RunAsync(m => m.ReopenSalesAsync(true, Ct), BindingsWithout(SalesGateCondition.PaymentMethod), _admin);

        Assert.True(reopened.IsWritten);
        Assert.False(reopened.IsClosedNow);
        Assert.False(reopened.Gate.IsOpen);
        Assert.Equal([SalesGateCondition.PaymentMethod], reopened.Gate.UnmetConditions);
        Assert.False(await ScalarAsync<bool>("SELECT is_sales_temporarily_closed FROM company_settings"));
    }

    [Fact]
    public async Task TheSwitch_WithoutAnAdministrator_OrOutsideATransaction_IsRefused_AndNothingIsLeft()
    {
        await ArrangeAsync(broken: null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => RunAsync(m => m.CloseSalesTemporarilyAsync(false, Ct), Bindings.AllOpen, admin: null, commit: false));
        await using ServiceProvider provider = BuildProvider(Bindings.AllOpen, _admin);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        await Assert.ThrowsAsync<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<ISettingsModule>().CloseSalesTemporarilyAsync(false, Ct));

        Assert.False(await ScalarAsync<bool>("SELECT is_sales_temporarily_closed FROM company_settings"));
        Assert.Empty(await ReadTrailAsync());
    }

    [Fact]
    public async Task TheSwitch_RolledBackWithItsTransaction_LeavesNeitherTheSwitchNorTheTrailRow()
    {
        await ArrangeAsync(broken: null);

        SalesSwitchResult written = await RunAsync(m => m.CloseSalesTemporarilyAsync(false, Ct), Bindings.AllOpen, _admin, commit: false);

        Assert.True(written.IsWritten);
        Assert.False(await ScalarAsync<bool>("SELECT is_sales_temporarily_closed FROM company_settings"));
        Assert.Empty(await ReadTrailAsync());
    }

    [Fact]
    public async Task TheSwitch_ChangesNoOtherColumnOfTheSettingsRow_AndTakesNoRowLockOrVersion()
    {
        await ArrangeAsync(broken: null);
        string before = await ScalarAsync<string>("SELECT (to_jsonb(s) - 'is_sales_temporarily_closed')::text FROM company_settings s");

        await RunAsync(m => m.CloseSalesTemporarilyAsync(false, Ct), Bindings.AllOpen, _admin);

        string after = await ScalarAsync<string>("SELECT (to_jsonb(s) - 'is_sales_temporarily_closed')::text FROM company_settings s");
        Assert.Equal(before, after);
        Assert.True(await ScalarAsync<bool>("SELECT is_sales_temporarily_closed FROM company_settings"));
    }

    // ---- KK5, KK6, KK7: nothing that closes the gate touches an open order or a published legal text ----

    [Fact]
    public async Task OpenOrders_AreNotTouched_WhenTheSwitchOrAConditionClosesTheGate_AndTheirIbanAndTextsStayAsFrozen()
    {
        await ArrangeAsync(broken: null);
        await SeedOrdersAsync();
        await PublishCookiePolicyAsync();
        await ExecuteAsync($"UPDATE company_settings SET is_bank_transfer_enabled = true, bank_transfer_iban = '{FrozenIban}'");
        Assert.True((await ReadGateAsync(Bindings.Transfer)).IsOpen);
        string orders = await OrderStateAsync();
        string texts = await LegalTextStateAsync();
        string knownIds = await ScalarAsync<string>("SELECT string_agg(id::text, ',') FROM legal_text_versions");
        Assert.Contains(FrozenIban, orders, StringComparison.Ordinal);

        // The switch is turned on; the identity changes type and loses its required fields; the return address is emptied; the transfer is closed and its IBAN replaced.
        await RunAsync(m => m.CloseSalesTemporarilyAsync(false, Ct), Bindings.Transfer, _admin);
        await RunAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(CompanyType.LegalEntity) with { MersisNumber = null, TaxIdNumber = "1234567890" }, 2, Ct), admin: _admin);
        await ExecuteAsync("UPDATE company_settings SET return_recipient_name = NULL, return_province_id = NULL, return_district = NULL, return_address_line = NULL");
        await ExecuteAsync("UPDATE company_settings SET is_bank_transfer_enabled = false, bank_transfer_iban = 'TR000000000000000000000001'");
        SalesGate gate = await ReadGateAsync(Bindings.Transfer);

        Assert.Equal(
            [SalesGateCondition.CompanyIdentity, SalesGateCondition.PaymentMethod, SalesGateCondition.TemporaryClosure, SalesGateCondition.PrivacyNotice, SalesGateCondition.ReturnAddress],
            gate.UnmetConditions);
        Assert.Equal(orders, await OrderStateAsync());
        Assert.Equal(texts, await LegalTextStateAsync(knownIds));
        Assert.Contains(FrozenIban, await OrderStateAsync(), StringComparison.Ordinal);
        Assert.True(gate.Permits(SalesGateOperation.ContinueOpenOrder));
        Assert.True(gate.Permits(SalesGateOperation.KeepExistingCart));
        Assert.True(gate.Permits(SalesGateOperation.BrowseShowcase));
        Assert.Null(gate.RefusalOf(SalesGateOperation.ContinueOpenOrder));
    }

    [Fact]
    public async Task APublishedLegalText_IsNeverWithdrawn_ByTheGateClosing_AndTheStoreRefusesItsUnpublishing()
    {
        await ArrangeAsync(broken: null);
        await PublishCookiePolicyAsync();
        await RunAsync(m => m.CloseSalesTemporarilyAsync(false, Ct), Bindings.AllOpen, _admin);
        string texts = await LegalTextStateAsync();

        await using AppDbContext context = NewContext();
        LegalTextVersion published = await context.LegalTextVersions.SingleAsync(l => l.LegalTextType == LegalTextType.CookiePolicy, Ct);
        published.PublishedAt = null;
        published.VersionNumber = null;
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));

        Assert.Equal(texts, await LegalTextStateAsync());
        Assert.True((await ReadGateAsync(Bindings.AllOpen)).IsDataCollectionOpen);
    }

    // ---- helpers ----

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

    private ReturnAddressInput Address() => new("Depo Sorumlusu", _provinceId, "Çankaya", "Depo Sk. 4", TenDigitPhone);

    /// <summary>Puts the installation in the state where every condition holds, except the broken one; the outside conditions are the test's bindings.</summary>
    private async Task ArrangeAsync(SalesGateCondition? broken)
    {
        if (broken != SalesGateCondition.CompanyIdentity)
        {
            await RunAsync(m => m.SaveCompanyIdentityAsync(CompleteInput(CompanyType.Tradesman), 1, Ct), admin: _admin);
        }

        if (broken != SalesGateCondition.ReturnAddress)
        {
            await RunAsync(m => m.SaveReturnAddressAsync(Address(), 1, Ct));
        }

        if (broken != SalesGateCondition.CookiePolicy)
        {
            await PublishCookiePolicyAsync();
        }

        if (broken == SalesGateCondition.TemporaryClosure)
        {
            await RunAsync(m => m.CloseSalesTemporarilyAsync(false, Ct), Bindings.AllOpen, _admin);
        }
    }

    private static Bindings BindingsWithout(SalesGateCondition broken) => new(
        Bound: true,
        Card: broken != SalesGateCondition.PaymentMethod,
        PrivacyNotice: broken != SalesGateCondition.PrivacyNotice);

    private Task<SalesGate> ReadGateAsync(Bindings bindings) => RunAsync(m => m.GetSalesGateAsync(Ct), bindings);

    private async Task PublishCookiePolicyAsync()
    {
        if (await ScalarAsync<long>("SELECT count(*) FROM legal_text_versions WHERE legal_text_type = 'CookiePolicy'") == 0)
        {
            await SaveAsync(c => c.LegalTextVersions.Add(LegalTextVersion.CreatePublished(LegalTextType.CookiePolicy, 1, Now, "çerez politikası", Now)));
        }
    }

    private async Task SeedOrdersAsync()
    {
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, 90, Now, "metin", Now);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, 90, Now, "metin", Now);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, 1, Now, "metin", Now).WithHosting();
        await SaveAsync(c => c.LegalTextVersions.AddRange(form, contract, notice));
        foreach (PaymentMethod method in new[] { PaymentMethod.Card, PaymentMethod.BankTransfer })
        {
            int n = Interlocked.Increment(ref _orderCounter);
            Order order = Order.Create(
                $"2026-{n:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", method, true, "Ali@Example.com", 120m, 0m, Now.AddHours(1), Now);
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
            order.BankTransferIban = method == PaymentMethod.BankTransfer ? FrozenIban : null;
            order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
            OrderItem item = OrderItem.Create(order.Id, null, null, ProductType.Physical, "Kapı", 3, 100m, 20m, 50m, Now);
            await SaveAsync(c =>
            {
                c.Orders.Add(order);
                c.OrderItems.Add(item);
            });
        }
    }

    private Task<string> OrderStateAsync() =>
        ScalarAsync<string>("SELECT coalesce(string_agg(to_jsonb(o)::text, '|' ORDER BY o.order_number), '') || '#' || coalesce((SELECT string_agg(to_jsonb(i)::text, '|' ORDER BY i.id) FROM order_items i), '') FROM orders o");

    /// <summary>The rows of the legal texts as stored; with the ids, only the rows that existed then (a changed identity may add a version of a generated text, never change a published row).</summary>
    private Task<string> LegalTextStateAsync(string? onlyIds = null) =>
        ScalarAsync<string>(
            "SELECT coalesce(string_agg(to_jsonb(l)::text, '|' ORDER BY l.legal_text_type, l.version_number), '') FROM legal_text_versions l "
            + (onlyIds is null ? string.Empty : $"WHERE l.id = ANY(string_to_array('{onlyIds}', ',')::uuid[])"));

    /// <summary>The switch rows only: the identity saves that set a test up write their own UpdateCompany rows.</summary>
    private async Task<List<AuditLog>> ReadTrailAsync()
    {
        await using AppDbContext context = NewContext();
        return await context.AuditLogs.AsNoTracking().Where(l => l.Operation == "UpdateSettings").OrderBy(l => l.CreatedAt).ThenBy(l => l.Id).ToListAsync(Ct);
    }

    private ServiceProvider BuildProvider(Bindings? bindings, Admin? admin = null)
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddScoped<CacheInvalidations>();
        services.AddSingleton<TimeProvider>(new FixedTime(Now));
        services.AddScoped<IAuditActorSource>(_ => new FixedActor(admin));
        services.AddScoped<IAuditTrail, AuditTrail>();

        // The outside conditions are bound before the module registers its defaults, the way the tasks that own them will bind them; no binding keeps the default (unbound).
        if (bindings is { Bound: true } bound)
        {
            services.AddScoped<ICardPaymentCondition>(_ => new FixedCardCondition(bound.Card));
            services.AddScoped<IPrivacyNoticeCondition>(_ => new FixedPrivacyNoticeCondition(bound.PrivacyNotice));
        }

        services.AddSettingsModule();
        return services.BuildServiceProvider();
    }

    /// <summary>Calls the module the way a command does: in one scope with one context, inside the caller's transaction, committed or rolled back by the caller.</summary>
    private async Task<T> RunAsync<T>(Func<ISettingsModule, Task<T>> call, Bindings? bindings = null, Admin? admin = null, bool commit = true)
    {
        await using ServiceProvider provider = BuildProvider(bindings ?? Bindings.AllOpen, admin);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(Ct);
        T result = await call(scope.ServiceProvider.GetRequiredService<ISettingsModule>());
        if (commit)
        {
            // The command pipeline saves the unit of work (the trail row is added to the context) before it commits.
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

    /// <summary>Which outside conditions the test binds (the module's defaults are the unbound ones): the card line and the privacy notice; the transfer line is the module's own (T353).</summary>
    private sealed record Bindings(bool Bound, bool Card, bool PrivacyNotice)
    {
        public static Bindings AllOpen { get; } = new(true, true, true);

        /// <summary>Only the transfer line can be open (read from the settings row, the way the transfer task will read it); the card line is closed.</summary>
        public static Bindings Transfer { get; } = new(true, false, true);

        /// <summary>Nothing bound: the module's own defaults (unbound, therefore unmet) stay.</summary>
        public static Bindings None { get; } = new(false, false, false);
    }

    private sealed class FixedCardCondition(bool open) : ICardPaymentCondition
    {
        public Task<bool> IsOpenAsync(CancellationToken ct) => Task.FromResult(open);
    }

    private sealed class FixedPrivacyNoticeCondition(bool published) : IPrivacyNoticeCondition
    {
        public Task<bool> IsPublishedWithHostingLocationAsync(CancellationToken ct) => Task.FromResult(published);
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
