using System.Net;
using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Membership.Contracts.Results;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The administrator's order corrections on a real PostgreSQL (T375 criteria 1-9, KK1-KK9; 02 7.6.6, 7.6.7, 8.3.6, 8.3.7, 10.4.1, 10.4.2, 10.4.11; 03 1.11.23, 1.11.35): the address
/// and the guest order's e-mail are forward records that leave the amount, the items and the axes alone; the address is open by the operation decision, only with the firm's confirmation,
/// and keeps the old and the new value with B-10; the e-mail is changed only with the firm's confirmation of the buyer, renews the access key, follows the verified account and sends B-1
/// to the new address and B-15 to the old one; there is no way to take a declaration back.
/// </summary>
[Collection(OrderTransitionsDefinition.Name)]
public sealed partial class OrderCorrectionTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Start = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    private readonly ManualTimeProvider _clock = new(Start);

    private Admin? _admin;

    private Province? _ankara;

    private Province? _izmir;

    private int _counter;

    public OrderCorrectionTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private TransitionAdmin AdminCaller => new(_admin!.Id, _admin.Name);

    private static GuestIdentityConfirmation FullConfirmation => new(true, true, true, true);

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE dispatch_records, jobs, audit_logs, order_email_histories, order_address_histories, transition_records, stock_reservations, order_items, orders, customers, "
            + "admins, company_settings, legal_text_versions, provinces CASCADE");
        _admin = Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", Start);
        _ankara = Province.Create("06", "Ankara", Start);
        _izmir = Province.Create("35", "İzmir", Start);
        CompanySettings settings = CompanySettings.Create(Start);
        settings.BankTransferPaymentWindow = 1;
        settings.DownloadAllowance = 1;
        settings.HomepageLayout = Shopfolio.Modules.CorporateContent.Contracts.HomepageLayout.StoreFirst;
        settings.MenuNameProducts = "Ürünler";
        settings.MenuNameOfferings = "Hizmetler";
        settings.MenuNamePortfolioItems = "Çalışmalar";
        settings.MenuNameAboutPage = "Hakkımızda";
        settings.MenuNameFaqItems = "Sıkça Sorulanlar";
        settings.MenuNameContact = "İletişim";
        await SaveAsync(c =>
        {
            c.Admins.Add(_admin);
            c.Provinces.AddRange(_ankara, _izmir);
            c.CompanyDeliveryProvinces.AddRange(CompanyDeliveryProvince.Create(_ankara.Id, Start), CompanyDeliveryProvince.Create(_izmir.Id, Start));
            c.CompanySettings.Add(settings);
        });
        _clock.SetUtcNow(Start);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the amount stays, nothing is added ----

    [Fact]
    public async Task BothCorrections_LeaveTheAmountTheItemsAndTheAxesAlone_AndAskNothingOfThem_KK1()
    {
        Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical, ProductType.Digital);
        (decimal total, decimal fee, string items) = await AmountsAsync(order.Id);

        OrderCorrectionResult address = await RunAsync(c => c.CorrectAddressAsync(AddressRequest(order, AddressType.ShippingAddress, "Yeni Mahalle 5"), Ct));
        OrderCorrectionResult email = await RunAsync(c => c.CorrectGuestEmailAsync(EmailRequest(order, "yeni@example.com"), Ct));

        Assert.Null(address.Refusal);
        Assert.Null(email.Refusal);
        Assert.Equal((total, fee, items), await AmountsAsync(order.Id));
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.AsNoTracking().SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Preparing, PaymentStatus.Paid, order.Version + 2), (saved.OrderStatus, saved.PaymentStatus, saved.Version));
        Assert.Empty(await reader.TransitionRecords.Where(t => t.OrderId == order.Id).ToListAsync(Ct));

        // The contract has no way to add an item or to touch a price: the two contact corrections carry no amount, quantity, price or item; the interventions of T386 carry
        // no amount, price or total either - the removal names an existing item and the units it takes away, never a new one (02 10.4.3).
        Assert.Equal(
            ["CorrectAddressAsync", "CorrectDeliveryDateAsync", "CorrectGuestEmailAsync", "CorrectReturnArrivalDateAsync", "CorrectShipmentAsync", "RemoveItemAsync", "SaveInternalNoteAsync"],
            typeof(IOrderCorrections).GetMethods().Select(m => m.Name).Order(StringComparer.Ordinal).ToArray());
        string[] carried = [.. new[] { typeof(CorrectAddressRequest), typeof(CorrectGuestEmailRequest) }.SelectMany(t => t.GetProperties()).Select(p => p.Name)];
        Assert.DoesNotContain(carried, n => n.Contains("Amount", StringComparison.Ordinal) && n != nameof(GuestIdentityConfirmation.Amount)
            || n.Contains("Quantity", StringComparison.Ordinal) || n.Contains("Price", StringComparison.Ordinal) || n.Contains("Item", StringComparison.Ordinal) || n.Contains("Total", StringComparison.Ordinal));
        string[] interventions = [.. new[] { typeof(RemoveItemRequest), typeof(CorrectShipmentRequest), typeof(CorrectDeliveryDateRequest), typeof(CorrectReturnArrivalDateRequest), typeof(SaveInternalNoteRequest) }
            .SelectMany(t => t.GetProperties()).Select(p => p.Name)];
        Assert.DoesNotContain(interventions, n => n.Contains("Amount", StringComparison.Ordinal) || n.Contains("Price", StringComparison.Ordinal) || n.Contains("Total", StringComparison.Ordinal)
            || n.Contains("Product", StringComparison.Ordinal) || n.Contains("Variant", StringComparison.Ordinal));
    }

    // ---- KK2: nine interventions, each on the trail ----

    [Fact]
    public void TheInterventions_AreNine_EachCarriedByAnOperationAndAWrittenTrailOperation_AndNoOneCreatesAnOrder_KK2()
    {
        Assert.Equal(9, OrderInterventions.All.Count);
        Assert.Equal([1, 2, 3, 4, 5, 6, 7, 8, 9], OrderInterventions.All.Select(i => i.Number));
        Assert.All(OrderInterventions.All, i =>
        {
            Assert.NotEmpty(i.Operations);
            Assert.NotEmpty(i.Trail);
            Assert.All(i.Operations, o => Assert.True(OrderInterventions.IsIntervention(o)));
        });
        Assert.All(OrderInterventions.All.SelectMany(i => i.Trail), t => Assert.True(Enum.IsDefined(t)));

        // No manual order: no method of any module contract creates an order for the administrator (02 10.4.1; KD-13).
        string[] creating = [.. ContractMethods().Where(m => m.Name.Contains("Order", StringComparison.Ordinal) && (m.Name.StartsWith("Create", StringComparison.Ordinal) || m.Name.StartsWith("Place", StringComparison.Ordinal) || m.Name.StartsWith("Manual", StringComparison.Ordinal)))
            .Select(m => $"{m.DeclaringType!.Name}.{m.Name}")];
        Assert.DoesNotContain(creating, n => n.Contains("Manual", StringComparison.Ordinal) || n.Contains("Admin", StringComparison.Ordinal));
    }

    [Fact]
    public async Task EachCorrection_WritesItsTrailRow_WithTheAdministrator_AndNoValue_KK2()
    {
        Order order = await SeedAsync(OrderStatus.Placed, PaymentStatus.Paid, ProductType.Physical);

        await RunAsync(c => c.CorrectAddressAsync(AddressRequest(order, AddressType.BillingAddress, "Başka Cadde 9"), Ct));
        await RunAsync(c => c.CorrectGuestEmailAsync(EmailRequest(order, "yeni@example.com"), Ct));

        await using AppDbContext reader = NewContext();
        List<AuditLog> rows = await reader.AuditLogs.AsNoTracking().Where(a => a.TargetRecordId == order.Id).OrderBy(a => a.CreatedAt).ToListAsync(Ct);
        Assert.Equal([nameof(AuditOperation.CorrectAddress), nameof(AuditOperation.CorrectContactEmail)], rows.Select(r => r.Operation).Order(StringComparer.Ordinal));
        Assert.All(rows, r => Assert.Equal((_admin!.Id, nameof(Order)), (r.AdminId, r.TargetRecordType)));
        Assert.All(rows, r =>
        {
            Assert.Empty(r.Details!.Changes);
            Assert.DoesNotContain(r.Details.Values, v => (v.Value ?? string.Empty).Contains("Başka", StringComparison.Ordinal) || (v.Value ?? string.Empty).Contains("yeni@", StringComparison.OrdinalIgnoreCase));
        });
    }

    // ---- KK3: the address boundary in two cases ----

    [Theory]
    [InlineData(OrderStatus.Placed, true)]
    [InlineData(OrderStatus.Preparing, true)]
    [InlineData(OrderStatus.Shipped, false)]
    [InlineData(OrderStatus.Delivered, false)]
    public async Task AnOrderWithAPhysicalItem_TakesBothAddressesUntilShipped_KK3(OrderStatus status, bool open)
    {
        Order order = await SeedAsync(status, PaymentStatus.Paid, ProductType.Physical);

        OrderCorrectionResult shipping = await RunAsync(c => c.CorrectAddressAsync(AddressRequest(order, AddressType.ShippingAddress, "Yeni Mahalle 5"), Ct));
        OrderCorrectionResult billing = await RunAsync(c => c.CorrectAddressAsync(AddressRequest(order, AddressType.BillingAddress, "Yeni Fatura 7"), Ct));

        TransitionRefusal? expected = open ? null : TransitionRefusal.ConditionNotMet;
        Assert.Equal((expected, expected), (shipping.Refusal, billing.Refusal));
        Assert.Equal(open ? 2 : 0, await CountAsync(c => c.OrderAddressHistories.Where(h => h.OrderId == order.Id)));
    }

    [Theory]
    [InlineData(OrderStatus.Placed, true)]
    [InlineData(OrderStatus.Shipped, true)]
    [InlineData(OrderStatus.Delivered, false)]
    public async Task AnOrderWithNoPhysicalItem_TakesTheBillingAddressUntilDelivered_AndHasNoShippingAddress_KK3(OrderStatus status, bool open)
    {
        Order order = await SeedAsync(status, PaymentStatus.Paid, ProductType.Digital);

        OrderCorrectionResult billing = await RunAsync(c => c.CorrectAddressAsync(AddressRequest(order, AddressType.BillingAddress, "Yeni Fatura 7"), Ct));
        OrderCorrectionResult shipping = await RunAsync(c => c.CorrectAddressAsync(AddressRequest(order, AddressType.ShippingAddress, "Yeni Mahalle 5"), Ct));

        Assert.Equal(open ? null : TransitionRefusal.ConditionNotMet, billing.Refusal);
        Assert.Equal(TransitionRefusal.ConditionNotMet, shipping.Refusal);
        Assert.Equal(open ? 1 : 0, await CountAsync(c => c.OrderAddressHistories.Where(h => h.OrderId == order.Id)));
    }

    // ---- KK4: no correction without the firm's confirmation of the owner ----

    [Fact]
    public async Task AnAddressRequestThatTheFirmDidNotConfirm_ChangesNothing_AndAnyOfTheThreeConfirmationsIsEnough_KK4()
    {
        Order order = await SeedAsync(OrderStatus.Placed, PaymentStatus.Paid, ProductType.Physical);

        OrderCorrectionResult none = await RunAsync(c => c.CorrectAddressAsync(AddressRequest(order, AddressType.ShippingAddress, "Yeni Mahalle 5") with { Confirmation = AddressRequestConfirmation.None }, Ct));

        Assert.Equal(TransitionRefusal.ConfirmationRequired, none.Refusal);
        Assert.Equal(0, await CountAsync(c => c.OrderAddressHistories.Where(h => h.OrderId == order.Id)));
        Assert.Equal(0, await CountAsync(c => c.AuditLogs.Where(a => a.TargetRecordId == order.Id)));
        Assert.Empty(await DispatchesAsync(order.Id));
        Assert.Equal(order.Version, (await ReloadAsync(order.Id)).Version);

        AddressRequestConfirmation[] confirmations = [AddressRequestConfirmation.FromOrderEmail, AddressRequestConfirmation.ConfirmedByEmailToOrder, AddressRequestConfirmation.ConfirmedByCallToOrderPhone];
        for (int i = 0; i < confirmations.Length; i++)
        {
            OrderCorrectionResult done = await RunAsync(c => c.CorrectAddressAsync(AddressRequest(order, AddressType.ShippingAddress, $"Yeni Mahalle {i + 10}") with { Confirmation = confirmations[i] }, Ct));
            Assert.Null(done.Refusal);
        }

        Assert.Equal(3, await CountAsync(c => c.OrderAddressHistories.Where(h => h.OrderId == order.Id)));
    }

    [Fact]
    public async Task ARefusedCorrection_WritesNothing_ForTheVersionTheCallerTheValueAndTheMissingOrder_KK4()
    {
        Order order = await SeedAsync(OrderStatus.Placed, PaymentStatus.Paid, ProductType.Physical);

        OrderCorrectionResult stale = await RunAsync(c => c.CorrectAddressAsync(AddressRequest(order, AddressType.ShippingAddress, "Yeni Mahalle 5") with { ExpectedVersion = order.Version + 5 }, Ct));
        OrderCorrectionResult noAdmin = await RunAsync(c => c.CorrectAddressAsync(AddressRequest(order, AddressType.ShippingAddress, "Yeni Mahalle 5") with { Admin = null }, Ct));
        OrderCorrectionResult unchanged = await RunAsync(c => c.CorrectAddressAsync(AddressRequest(order, AddressType.ShippingAddress, "Eski Sokak 1") with { ProvinceId = _ankara!.Id, District = "Çankaya", Phone = "5321234567" }, Ct));
        OrderCorrectionResult noLine = await RunAsync(c => c.CorrectAddressAsync(AddressRequest(order, AddressType.ShippingAddress, "") with { AddressLine = null }, Ct));
        OrderCorrectionResult missing = await RunAsync(c => c.CorrectAddressAsync(AddressRequest(order, AddressType.ShippingAddress, "Yeni Mahalle 5") with { OrderId = Guid.NewGuid() }, Ct));

        Assert.Equal(
            [TransitionRefusal.VersionConflict, TransitionRefusal.ActorNotAllowed, TransitionRefusal.InvalidInput, TransitionRefusal.InvalidInput, TransitionRefusal.OrderNotFound],
            [stale.Refusal!.Value, noAdmin.Refusal!.Value, unchanged.Refusal!.Value, noLine.Refusal!.Value, missing.Refusal!.Value]);
        Assert.NotEmpty(noLine.Fields);
        Assert.Equal(0, await CountAsync(c => c.OrderAddressHistories.Where(h => h.OrderId == order.Id)));
        Assert.Equal(order.Version, (await ReloadAsync(order.Id)).Version);
    }

    // ---- KK5: B-10 goes out and the history stays ----

    [Fact]
    public async Task AnAddressCorrection_SendsB10ToTheOrdersEmail_AndKeepsTheOldAndTheNewValueOnTheOrder_KK5()
    {
        Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);

        OrderCorrectionResult result = await RunAsync(c => c.CorrectAddressAsync(AddressRequest(order, AddressType.ShippingAddress, "Yeni Mahalle 5"), Ct));
        Assert.True(result.Refusal is null, string.Join(",", result.Fields.Select(f => f.Field + ":" + f.Code)));

        Assert.Equal([(DispatchType.B10, "ali@example.com")], await DispatchesAsync(order.Id));
        await using AppDbContext reader = NewContext();
        OrderAddressHistory history = await reader.OrderAddressHistories.AsNoTracking().SingleAsync(h => h.OrderId == order.Id, Ct);
        Assert.Equal(result.CorrectionRecordId, history.Id);
        Assert.Equal((AddressType.ShippingAddress, "Eski Sokak 1", "Yeni Mahalle 5", _admin!.Id, _admin.Name, Start), (history.AddressType, history.OldAddressLine, history.NewAddressLine, history.AdminId, history.AdminName, history.CreatedAt));
        Assert.Equal(("5321234567", "5329998877", _ankara!.Id, _izmir!.Id), (history.OldPhone, history.NewPhone, history.OldProvinceId, history.NewProvinceId));
        Order saved = await reader.Orders.AsNoTracking().SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal(("Yeni Mahalle 5", "5329998877", _izmir.Id), (saved.ShippingAddressLine, saved.ShippingPhone, saved.ShippingProvinceId));
        Assert.Equal(("Atatürk Bulvarı 1", _ankara.Id), (saved.BillingAddressLine, saved.BillingProvinceId));

        // A second correction keeps the first row: the history only grows.
        await RunAsync(c => c.CorrectAddressAsync(AddressRequest(order, AddressType.ShippingAddress, "Üçüncü Sokak 3"), Ct));
        Assert.Equal(["Eski Sokak 1", "Yeni Mahalle 5"], (await reader.OrderAddressHistories.AsNoTracking().Where(h => h.OrderId == order.Id).OrderBy(h => h.CreatedAt).ThenBy(h => h.NewAddressLine).ToListAsync(Ct)).Select(h => h.OldAddressLine).Order(StringComparer.Ordinal));
    }

    // ---- KK6: the buyer has no field ----

    [Fact]
    public void TheCustomersSurfaces_HaveNoWayToChangeTheEmailOrTheAddressOfAnOrder_KK6()
    {
        // The corrections are the administrator's: the requests need a TransitionAdmin and no module contract but IOrderCorrections has such a method (02 8.3.6, 8.3.7).
        Assert.Equal(typeof(TransitionAdmin), typeof(CorrectAddressRequest).GetProperty(nameof(CorrectAddressRequest.Admin))!.PropertyType.GetGenericArguments().FirstOrDefault() ?? typeof(CorrectAddressRequest).GetProperty(nameof(CorrectAddressRequest.Admin))!.PropertyType);
        Assert.DoesNotContain(typeof(CorrectGuestEmailRequest).GetProperties(), p => p.Name.Contains("Customer", StringComparison.Ordinal) || p.Name.Contains("AccessKey", StringComparison.Ordinal) || p.Name.Contains("OrderNumber", StringComparison.Ordinal));
        string[] reaching = [.. ContractMethods()
            .Where(m => m.DeclaringType != typeof(IOrderCorrections))
            .Where(m => m.GetParameters().Any(p => p.ParameterType == typeof(CorrectAddressRequest) || p.ParameterType == typeof(CorrectGuestEmailRequest)))
            .Select(m => $"{m.DeclaringType!.Name}.{m.Name}")];
        Assert.Empty(reaching);
        string[] changing = [.. ContractMethods()
            .Where(m => m.DeclaringType != typeof(IOrderCorrections) && m.DeclaringType != typeof(IMembershipModule))
            .Where(m => m.Name.Contains("Order", StringComparison.Ordinal) && (m.Name.Contains("Email", StringComparison.Ordinal) || m.Name.Contains("Address", StringComparison.Ordinal))
                && (m.Name.StartsWith("Change", StringComparison.Ordinal) || m.Name.StartsWith("Update", StringComparison.Ordinal) || m.Name.StartsWith("Correct", StringComparison.Ordinal) || m.Name.StartsWith("Set", StringComparison.Ordinal)))
            .Select(m => $"{m.DeclaringType!.Name}.{m.Name}")];
        Assert.Empty(changing);
    }

    [Fact]
    public async Task TheEmailOfAnAccountsOrder_IsNotCorrected_AndNeitherIsADisposedOne_KK6()
    {
        Order account = await SeedAsync(OrderStatus.Placed, PaymentStatus.Paid, ProductType.Physical, guest: false);
        Order disposed = await SeedAsync(OrderStatus.Placed, PaymentStatus.Paid, ProductType.Physical);
        await using (AppDbContext context = NewContext())
        {
            Order tracked = await context.Orders.SingleAsync(o => o.Id == disposed.Id, Ct);
            tracked.ErasePersonalData();
            await context.SaveChangesAsync(Ct);
        }

        OrderCorrectionResult byAccount = await RunAsync(c => c.CorrectGuestEmailAsync(EmailRequest(account, "yeni@example.com"), Ct));
        OrderCorrectionResult byDisposed = await RunAsync(c => c.CorrectGuestEmailAsync(EmailRequest(disposed, "yeni@example.com"), Ct));

        Assert.Equal((TransitionRefusal.ConditionNotMet, TransitionRefusal.ConditionNotMet), (byAccount.Refusal, byDisposed.Refusal));
        Assert.Equal(0, await CountAsync(c => c.OrderEmailHistories));
    }

    // ---- KK7: the firm's confirmation, the old key, B-1 and B-15 ----

    [Theory]
    [InlineData(false, true, true, true)]
    [InlineData(true, false, true, true)]
    [InlineData(true, true, false, true)]
    [InlineData(true, true, true, false)]
    public async Task TheEmailCorrection_NeedsTheNameThePhoneTheItemsAndTheAmount_AndTheNumberAloneIsNoConfirmation_KK7(bool name, bool phone, bool items, bool amount)
    {
        Order order = await SeedAsync(OrderStatus.Placed, PaymentStatus.Paid, ProductType.Physical);

        OrderCorrectionResult partial = await RunAsync(c => c.CorrectGuestEmailAsync(EmailRequest(order, "yeni@example.com") with { Confirmation = new GuestIdentityConfirmation(name, phone, items, amount) }, Ct));
        OrderCorrectionResult none = await RunAsync(c => c.CorrectGuestEmailAsync(EmailRequest(order, "yeni@example.com") with { Confirmation = null }, Ct));

        Assert.Equal((TransitionRefusal.ConfirmationRequired, TransitionRefusal.ConfirmationRequired), (partial.Refusal, none.Refusal));
        Assert.Equal(0, await CountAsync(c => c.OrderEmailHistories));
        Order saved = await ReloadAsync(order.Id);
        Assert.Equal(("Ali@Example.com", order.OrderAccessKey, order.Version), (saved.ContactEmail, saved.OrderAccessKey, saved.Version));
        Assert.Empty(await DispatchesAsync(order.Id));
    }

    [Fact]
    public async Task AnOrderWithNoPhysicalItem_NeedsNoShippingPhone_ForTheEmailCorrection_KK7()
    {
        Order order = await SeedAsync(OrderStatus.Placed, PaymentStatus.Paid, ProductType.Digital);

        OrderCorrectionResult done = await RunAsync(c => c.CorrectGuestEmailAsync(EmailRequest(order, "yeni@example.com") with { Confirmation = new GuestIdentityConfirmation(true, false, true, true) }, Ct));

        Assert.Null(done.Refusal);
    }

    [Fact]
    public async Task TheEmailCorrection_KillsTheOldKey_OpensTheOrderByTheNewAddress_SendsB1ToTheNewAndB15ToTheOld_AndKeepsBothInTheHistory_KK7()
    {
        Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);
        string oldKey = order.OrderAccessKey!;

        // An unsent notice of the old address (B-10 of an earlier address correction) goes to the new address with the correction.
        await RunAsync(c => c.CorrectAddressAsync(AddressRequest(order, AddressType.BillingAddress, "Yeni Fatura 7"), Ct));
        _clock.SetUtcNow(Start.AddMinutes(5));
        OrderCorrectionResult result = await RunAsync(c => c.CorrectGuestEmailAsync(EmailRequest(order, "  Yeni@Example.com ") with { ExpectedVersion = order.Version + 1 }, Ct));
        Assert.Null(result.Refusal);
        Assert.Null(result.LinkedCustomerId);

        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IOrderingModule ordering = scope.ServiceProvider.GetRequiredService<IOrderingModule>();
        Order saved = await ReloadAsync(order.Id);
        Assert.NotEqual(oldKey, saved.OrderAccessKey);
        Assert.False(string.IsNullOrEmpty(saved.OrderAccessKey));
        Assert.Null(await ordering.FindOrderByAccessKeyAsync(oldKey, Ct));
        Assert.Equal(order.Id, await ordering.FindOrderByAccessKeyAsync(saved.OrderAccessKey!, Ct));
        Assert.Null(await ordering.FindOrderByNumberAndEmailAsync(order.OrderNumber, "Ali@Example.com", Ct));
        Assert.NotNull(await ordering.FindOrderByNumberAndEmailAsync(order.OrderNumber, "yeni@example.com", Ct));
        Assert.Equal("Yeni@Example.com", saved.ContactEmail);

        List<(DispatchType Type, string? To)> sent = await DispatchesAsync(order.Id);
        Assert.Contains((DispatchType.B1, "yeni@example.com"), sent);
        Assert.Contains((DispatchType.B15, "ali@example.com"), sent);
        Assert.DoesNotContain(sent, d => d.Type == DispatchType.B15 && d.To != "ali@example.com");
        Assert.Contains((DispatchType.B10, "yeni@example.com"), sent);
        Assert.DoesNotContain((DispatchType.B10, "ali@example.com"), sent);

        await using AppDbContext reader = NewContext();
        OrderEmailHistory history = await reader.OrderEmailHistories.AsNoTracking().SingleAsync(h => h.OrderId == order.Id, Ct);
        Assert.Equal((result.CorrectionRecordId, "Ali@Example.com", "Yeni@Example.com", (Guid?)_admin!.Id, Start.AddMinutes(5)), ((Guid?)history.Id, history.OldEmail, history.NewEmail, history.AdminId, history.CreatedAt));
    }

    [Fact]
    public async Task TheSameAddressInAnotherCaseOrAnInvalidOne_IsRefusedWithoutAChange_KK7()
    {
        Order order = await SeedAsync(OrderStatus.Placed, PaymentStatus.Paid, ProductType.Physical);

        OrderCorrectionResult same = await RunAsync(c => c.CorrectGuestEmailAsync(EmailRequest(order, "ALI@example.com"), Ct));
        OrderCorrectionResult invalid = await RunAsync(c => c.CorrectGuestEmailAsync(EmailRequest(order, "bu bir adres degil"), Ct));
        OrderCorrectionResult empty = await RunAsync(c => c.CorrectGuestEmailAsync(EmailRequest(order, "  "), Ct));
        OrderCorrectionResult noAdmin = await RunAsync(c => c.CorrectGuestEmailAsync(EmailRequest(order, "yeni@example.com") with { Admin = null }, Ct));

        Assert.Equal([TransitionRefusal.InvalidInput, TransitionRefusal.InvalidInput, TransitionRefusal.InvalidInput, TransitionRefusal.ActorNotAllowed], [same.Refusal!.Value, invalid.Refusal!.Value, empty.Refusal!.Value, noAdmin.Refusal!.Value]);
        Assert.Equal(0, await CountAsync(c => c.OrderEmailHistories));
        Assert.Equal(order.OrderAccessKey, (await ReloadAsync(order.Id)).OrderAccessKey);
    }

    // ---- KK8: the account link follows the new address ----

    [Fact]
    public async Task AVerifiedAccountOfTheNewAddress_TakesTheOrder_AndTheOldLinkIsCut_KK8()
    {
        Customer oldOwner = Customer.Create("Eski Hesap", "ali@example.com", null, Start, null, Start);
        Customer newOwner = Customer.Create("Yeni Hesap", "yeni@example.com", null, Start, null, Start);
        await SaveAsync(c => c.Customers.AddRange(oldOwner, newOwner));
        Order order = await SeedAsync(OrderStatus.Placed, PaymentStatus.Paid, ProductType.Physical);
        await ExecuteAsync($"UPDATE orders SET customer_id = '{oldOwner.Id}' WHERE id = '{order.Id}'");

        OrderCorrectionResult result = await RunAsync(c => c.CorrectGuestEmailAsync(EmailRequest(order, "yeni@example.com"), Ct));

        Assert.Null(result.Refusal);
        Assert.Equal(newOwner.Id, result.LinkedCustomerId);
        Assert.Equal(newOwner.Id, (await ReloadAsync(order.Id)).CustomerId);
    }

    [Fact]
    public async Task AnAddressWithNoVerifiedAccount_LeavesAGuestOrder_AndTheOldLinkIsCut_KK8()
    {
        Customer oldOwner = Customer.Create("Eski Hesap", "ali@example.com", null, Start, null, Start);
        Customer pending = Customer.Create("Bekleyen", "yeni@example.com", "hash-placeholder", null, null, Start);
        await SaveAsync(c => c.Customers.AddRange(oldOwner, pending));
        Order order = await SeedAsync(OrderStatus.Placed, PaymentStatus.Paid, ProductType.Physical);
        await ExecuteAsync($"UPDATE orders SET customer_id = '{oldOwner.Id}' WHERE id = '{order.Id}'");

        OrderCorrectionResult result = await RunAsync(c => c.CorrectGuestEmailAsync(EmailRequest(order, "yeni@example.com"), Ct));

        Assert.Null(result.Refusal);
        Assert.Null(result.LinkedCustomerId);
        Assert.Null((await ReloadAsync(order.Id)).CustomerId);
    }

    [Fact]
    public async Task AnOrderWhoseAccountWasDeleted_IsNotLinkedToTheNewAddressesAccount_KK8()
    {
        Customer newOwner = Customer.Create("Yeni Hesap", "yeni@example.com", null, Start, null, Start);
        await SaveAsync(c => c.Customers.Add(newOwner));
        Order order = await SeedAsync(OrderStatus.Placed, PaymentStatus.Paid, ProductType.Physical);
        await ExecuteAsync($"UPDATE orders SET is_account_deleted = true WHERE id = '{order.Id}'");

        OrderCorrectionResult result = await RunAsync(c => c.CorrectGuestEmailAsync(EmailRequest(order, "yeni@example.com"), Ct));

        Assert.Null(result.Refusal);
        Assert.Null(result.LinkedCustomerId);
        Assert.Null((await ReloadAsync(order.Id)).CustomerId);
    }

    // ---- KK9: a declaration is not taken back ----

    [Fact]
    public void NoModuleSurface_TakesBackOrCorrectsAWithdrawalDeclaration_KK9()
    {
        // 02 7.6.7: no method of any module contract changes, corrects, undoes or removes a withdrawal; the corrections of T375 name no withdrawal either.
        string[] changing = [.. ContractMethods()
            .Where(m => m.Name.Contains("Withdrawal", StringComparison.Ordinal) || m.Name.Contains("Declaration", StringComparison.Ordinal))
            .Where(m => System.Text.RegularExpressions.Regex.IsMatch(m.Name, "^(Update|Change|Correct|Edit|Delete|Remove|Cancel|Undo|Revert|Reset|TakeBack)", System.Text.RegularExpressions.RegexOptions.CultureInvariant))
            .Select(m => $"{m.DeclaringType!.Name}.{m.Name}")];
        Assert.Empty(changing);
        Assert.DoesNotContain(typeof(IOrderCorrections).GetMethods(), m => m.Name.Contains("Withdrawal", StringComparison.Ordinal));
        Assert.DoesNotContain(OrderInterventions.All.SelectMany(i => i.Trail), t => t.ToString().Contains("Undo", StringComparison.Ordinal));
    }

    // ---- helpers ----

    private static IEnumerable<MethodInfo> ContractMethods() =>
        typeof(IOrderCorrections).Assembly.GetTypes()
            .Where(t => t.IsInterface && t.IsPublic && t.Namespace is { } ns && ns.StartsWith("Shopfolio.Modules.", StringComparison.Ordinal) && ns.EndsWith(".Contracts", StringComparison.Ordinal))
            .SelectMany(t => t.GetMethods());

    private CorrectAddressRequest AddressRequest(Order order, AddressType type, string line)
    {
        bool shipping = type == AddressType.ShippingAddress;
        return new CorrectAddressRequest(
            order.Id, AdminCaller, type, shipping ? "Ali Veli" : "Ali Veli Fatura", _izmir!.Id, "Konak", line, shipping ? "5329998877" : null, AddressRequestConfirmation.FromOrderEmail, null);
    }

    private CorrectGuestEmailRequest EmailRequest(Order order, string newEmail) => new(order.Id, AdminCaller, newEmail, FullConfirmation, null);

    private async Task<Order> ReloadAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
    }

    private async Task<(decimal Total, decimal Fee, string Items)> AmountsAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
        List<OrderItem> items = await reader.OrderItems.AsNoTracking().Where(i => i.OrderId == orderId).OrderBy(i => i.Id).ToListAsync(Ct);
        return (saved.Total, saved.ShippingFee, string.Join('|', items.Select(i => $"{i.Id}:{i.Quantity}:{i.UnitPrice}:{i.VatAmount}:{i.CouponShare}")));
    }

    private async Task<List<(DispatchType Type, string? To)>> DispatchesAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        List<DispatchRecord> rows = await reader.DispatchRecords.AsNoTracking().Where(d => d.OrderId == orderId).OrderBy(d => d.DispatchType).ToListAsync(Ct);
        return [.. rows.Select(d => (d.DispatchType, d.RecipientEmail?.ToLowerInvariant()))];
    }

    private async Task<int> CountAsync<TRow>(Func<AppDbContext, IQueryable<TRow>> query)
    {
        await using AppDbContext reader = NewContext();
        return await query(reader).CountAsync(Ct);
    }

    private async Task<T> RunAsync<T>(Func<IOrderCorrections, Task<T>> work)
        where T : IOperationResult
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IOrderCorrections unit = scope.ServiceProvider.GetRequiredService<IOrderCorrections>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(_ => work(unit), Ct);
    }

    private async Task<Order> SeedAsync(OrderStatus orderStatus, PaymentStatus paymentStatus, ProductType first, ProductType? second = null, bool guest = true)
    {
        int n = Interlocked.Increment(ref _counter);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Start, "metin", Start);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Start, "metin", Start);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Start, "metin", Start).WithHosting();
        DateTimeOffset orderedAt = Start.AddDays(-7);
        Order order = Order.Create($"2026-{n + 375000:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", PaymentMethod.Card, guest, "Ali@Example.com", 120m, 0m, orderedAt.AddHours(1), orderedAt);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.FreezeShippingPromise(2);
        order.BillingRecipientName = "Ali Veli";
        order.BillingProvinceId = _ankara!.Id;
        order.BillingDistrict = "Çankaya";
        order.BillingAddressLine = "Atatürk Bulvarı 1";
        if (first == ProductType.Physical || second == ProductType.Physical)
        {
            order.ShippingRecipientName = "Ali Veli";
            order.ShippingPhone = "5321234567";
            order.ShippingProvinceId = _ankara.Id;
            order.ShippingDistrict = "Çankaya";
            order.ShippingAddressLine = "Eski Sokak 1";
        }

        order.CompanyType = CompanyType.Tradesman;
        order.CompanyLegalName = "Örnek Ticaret";
        order.CompanyTaxIdNumber = "1234567890";
        order.CompanyProvinceId = _ankara.Id;
        order.CompanyDistrict = "Çankaya";
        order.CompanyAddressLine = "Kızılay 2";
        order.CompanyPhone = "3121234567";
        order.CompanyEmail = "firma@example.com";
        order.CompanyKepAddress = "firma@hs01.kep.tr";
        order.PreInformationFormVersionId = form.Id;
        order.DistanceSalesContractVersionId = contract.Id;
        order.PrivacyNoticeVersionId = notice.Id;
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");

        List<OrderItem> items = [];
        foreach (ProductType type in new[] { first, second }.OfType<ProductType>())
        {
            int quantity = type == ProductType.Digital ? 1 : 2;
            OrderItem item = OrderItem.Create(order.Id, null, null, type, "Ürün", quantity, 300m, 20m, 50m * quantity, orderedAt);
            item.CouponShare = 10m * quantity;
            item.DownloadAllowance = type == ProductType.Digital ? 5 : null;
            items.Add(item);
        }

        await SaveAsync(c =>
        {
            c.LegalTextVersions.AddRange(form, contract, notice);
            c.Orders.Add(order);
            c.OrderItems.AddRange(items);
        });

        // The states are set around the entity (it takes them only through the unit); the pair rule of the database still applies.
        await ExecuteAsync($"UPDATE orders SET order_status = '{orderStatus}', payment_status = '{paymentStatus}' WHERE id = '{order.Id}'");
        return await ReloadAsync(order.Id);
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
        services.AddSingleton<TimeProvider>(_clock);
        services.AddScoped<IJobQueue, JobQueue>();
        services.AddFileStore();
        services.AddOptions<Shopfolio.Infrastructure.InstallationSettings.StatusThresholdOptions>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.IDiskUsageReader, Shopfolio.Infrastructure.Status.DriveDiskUsageReader>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.DiskUsageGuard>();
        services.AddSingleton<ImageProcessor>();
        services.AddCatalogModule();
        services.AddAfterSalesModule();
        services.AddNoticeDependencies();
        services.AddOrderingModule();
        services.AddPaymentModule();
        services.AddScoped<ICompanyContactEmail, FixedCompanyContactEmail>();

        // The trail names the administrator; the Membership module's two operations the corrections use are the real address rules and the real account lookup.
        services.AddScoped<IAuditActorSource>(_ => new FixedActor(_admin!));
        services.AddScoped<AddressBooks>();
        services.AddScoped(provider => CorrectionMembership.Create(provider.GetRequiredService<AppDbContext>(), provider.GetRequiredService<AddressBooks>()));
        return services.BuildServiceProvider();
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

    private sealed class FixedCompanyContactEmail : ICompanyContactEmail
    {
        public Task<string?> GetAsync(CancellationToken ct) => Task.FromResult<string?>("firma-iletisim@example.com");
    }

    private sealed class FixedActor : IAuditActorSource
    {
        private readonly Admin _admin;

        public FixedActor(Admin admin)
        {
            _admin = admin;
        }

        public Task<AuditActor?> CurrentAsync(CancellationToken ct) => Task.FromResult<AuditActor?>(new AuditActor(_admin.Id, _admin.Name, _admin.Email));
    }

    /// <summary>The Membership module as the corrections use it: the address rules of the address book and the account lookup of the module itself; nothing else is called.</summary>
#pragma warning disable CA1852 // DispatchProxy derives the proxy from this class: it is not sealed.
    internal class CorrectionMembership : DispatchProxy
#pragma warning restore CA1852
    {
        private AppDbContext _db = null!;

        private AddressBooks _addresses = null!;

        public static IMembershipModule Create(AppDbContext db, AddressBooks addresses)
        {
            IMembershipModule proxy = Create<IMembershipModule, CorrectionMembership>();
            CorrectionMembership self = (CorrectionMembership)(object)proxy;
            self._db = db;
            self._addresses = addresses;
            return proxy;
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            return targetMethod!.Name switch
            {
                nameof(IMembershipModule.ResolveOrderAddressesAsync) => _addresses.ResolveAsync((OrderAddressRequest)args![0]!, (CancellationToken)args[1]!),
                nameof(IMembershipModule.ResolveOrderAccountAsync) => ResolveAccountAsync((string)args![1]!, (CancellationToken)args[2]!),
                _ => throw new NotSupportedException(targetMethod.Name),
            };
        }

        private async Task<OrderAccountResolution> ResolveAccountAsync(string email, CancellationToken ct)
        {
            string normalized = TextNormalizer.NormalizeIdentity(email);
            Guid? bound = await _db.Customers.AsNoTracking().Where(c => c.NormalizedEmail == normalized && c.EmailVerifiedAt != null).Select(c => (Guid?)c.Id).SingleOrDefaultAsync(ct);
            return bound is { } id ? new OrderAccountResolution(OrderAccountKind.GuestBoundToAccount, id) : new OrderAccountResolution(OrderAccountKind.Guest, null);
        }
    }
}
