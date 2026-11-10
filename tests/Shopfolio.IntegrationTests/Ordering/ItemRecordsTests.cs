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
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
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
/// The item records on a real PostgreSQL (T320 KK1-KK10; 03 1.7.1, 1.7.3, 1.7.4, 1.9.1; 02 Z-18): the defect claim's opening, "resolved" mark and reopening and
/// the return rejection through the transition unit's open face in the transaction runner, the withdrawal and the receipt through the AfterSales writers under the
/// order's lock, and the panel marks read from the records. Every expected count comes from <see cref="ItemRecordTable"/>, the claim window or the contract.
/// </summary>
[Collection(OrderTransitionsDefinition.Name)]
public sealed partial class ItemRecordsTests : IAsyncLifetime
{
    private const string CompanyEmail = "firma-iletisim@example.com";

    private static readonly DateTimeOffset Start = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    /// <summary>The delivery five days before the tests' clock: the declarations and arrival dates after it are not in the future.</summary>
    private static readonly DateTimeOffset DeliveredAt = Start.AddDays(-5);

    private static readonly DateOnly DeliveryDay = DateOnly.FromDateTime(DeliveredAt.UtcDateTime);

    private static readonly ItemAmounts Amounts = new(100m, 0m, 16.67m);

    private readonly CollectionDatabaseFixture _collection;

    private readonly ManualTimeProvider _clock = new(Start);

    private Admin? _admin;

    private int _counter;

    public ItemRecordsTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private TransitionAdmin AdminCaller => new(_admin!.Id, _admin.Name);

    private ItemRecordAdmin RecordAdmin => new(_admin!.Id, _admin.Name);

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE dispatch_records, cancellations, internal_notes, transition_records, order_items, orders, admins, company_settings, legal_text_versions, provinces CASCADE");
        _admin = Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", Start);

        // The claim and the withdrawal copy the firm's return address from the settings row (06 4.25, 4.30): the row exists, the address is not entered.
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
            c.CompanySettings.Add(settings);
        });
        _clock.SetUtcNow(Start);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the item carries no status; the record's own row carries the actor, the moment and the units ----

    [Fact]
    public void OrderItem_CarriesNoStatusOfItsOwn()
    {
        string[] statusLike = [.. typeof(OrderItem).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(p => p.Name)
            .Where(n => n.Contains("Status", StringComparison.Ordinal) || n.Contains("State", StringComparison.Ordinal))];
        Assert.Empty(statusLike);
    }

    [Fact]
    public async Task Records_CarryTheActorTheMomentAndTheUnits_OnTheirOwnRows()
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(3, WithdrawalExemptionReason.HygieneSensitive);
        ItemRecordWriteResult withdrawal = await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(Declaration(order, item, 2, DeliveryDay.AddDays(1)), t));
        _clock.SetUtcNow(Start.AddHours(1));
        ItemRecordWriteResult receipt = await WriteAsync(
            order.Id, (m, t) => m.AddReturnReceiptAsync(new ReturnReceiptEntry(Subject(order, item), withdrawal.RecordId!.Value, 1, DeliveryDay.AddDays(2), RecordAdmin, _clock.GetUtcNow()), t));
        _clock.SetUtcNow(Start.AddHours(2));
        ItemRecordResult rejected = await RunAsync((u, t) => u.RejectReturnAsync(Rejection(order.Id, item.Id, withdrawal.RecordId!.Value, 3, 1), t));
        _clock.SetUtcNow(Start.AddHours(3));
        ItemRecordResult opened = await RunAsync((u, t) => u.OpenDefectClaimAsync(new OpenDefectClaimRequest(order.Id, item.Id, 1, "Kırık geldi"), t));
        _clock.SetUtcNow(Start.AddHours(4));
        ItemRecordResult resolved = await RunAsync((u, t) => u.ResolveDefectClaimAsync(new ResolveDefectClaimRequest(order.Id, item.Id, AdminCaller, 5), t));
        Assert.All(new[] { rejected.Refusal, opened.Refusal, resolved.Refusal }, r => Assert.Null(r));
        Assert.False(receipt.IsRejected);

        await using AppDbContext reader = NewContext();
        Withdrawal w = await reader.Withdrawals.SingleAsync(x => x.OrderItemId == item.Id, Ct);
        Assert.Equal((ActorKind.Buyer, Start, 2), (w.ActorKind, w.CreatedAt, w.Quantity));
        Return r = await reader.Returns.SingleAsync(x => x.OrderItemId == item.Id, Ct);
        Assert.Equal((_admin!.Id, _admin.Name, Start.AddHours(1), 1), (r.AdminId, r.AdminName, r.CreatedAt, r.Quantity));
        ReturnRejection rr = await reader.ReturnRejections.SingleAsync(x => x.OrderItemId == item.Id, Ct);
        Assert.Equal((_admin.Id, _admin.Name, Start.AddHours(2), 1), (rr.AdminId, rr.AdminName, rr.CreatedAt, rr.Quantity));
        DefectClaim claim = await reader.DefectClaims.SingleAsync(x => x.OrderItemId == item.Id, Ct);
        Assert.Equal((Start.AddHours(3), 1), (claim.CreatedAt, claim.Quantity));
        DefectClaimHistoryEntry entry = Assert.Single(claim.StatusHistory);
        Assert.Equal(new DefectClaimHistoryEntry(Start.AddHours(4), DefectClaimStatus.Resolved, ActorKind.Admin, null), entry);
    }

    // ---- KK2: each record's effect on the line is the table's ----

    /// <summary>Every record of the table, so a new row of 03 1.7.1 cannot go untested.</summary>
    public static TheoryData<string> RecordKinds => new(Enum.GetNames<ItemRecordKind>());

    [Theory]
    [MemberData(nameof(RecordKinds))]
    public async Task ClosedQuantity_MovesByTheRecord_AsTheTableSays(string kindName)
    {
        ItemRecordKind kind = Enum.Parse<ItemRecordKind>(kindName);
        // The download counter is a digital item's (one unit); every other record is written on a physical item of three units.
        (ProductType type, int quantity) = kind == ItemRecordKind.DownloadCounter ? (ProductType.Digital, 1) : (ProductType.Physical, 3);
        (_, OrderItem item) = await SeedAsync(OrderStatus.Delivered, PaymentStatus.Paid, type, quantity, delivered: false, exemption: null);
        Withdrawal? earlier = null;
        if (kind is ItemRecordKind.ReturnReceipt or ItemRecordKind.ReturnRejection or ItemRecordKind.NotReturnedClosure)
        {
            earlier = Withdrawal.ByBuyer(item.Id, 1, DeliveryDay, null, Amounts, null, Start);
            await SaveAsync(c => c.Withdrawals.Add(earlier));
        }

        int before = await ClosedAsync(item.Id);
        Guid admin = _admin!.Id;
        string name = _admin.Name;
        switch (kind)
        {
            case ItemRecordKind.DeliveryMark:
                await ExecuteAsync($"UPDATE order_items SET delivery_marked_at = now(), delivery_date = current_date WHERE id = '{item.Id}'");
                break;
            case ItemRecordKind.DownloadCounter:
                await ExecuteAsync($"UPDATE order_items SET download_allowance = 5, download_count = 1 WHERE id = '{item.Id}'");
                break;
            case ItemRecordKind.Cancellation:
                await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(item.Id, 1, Amounts, null, Start)));
                break;
            case ItemRecordKind.ItemRemoval:
                await SaveAsync(c => c.ItemRemovals.Add(ItemRemoval.Create(item.Id, 1, admin, name, Amounts, Start)));
                break;
            case ItemRecordKind.LateDeliveryTermination:
                await SaveAsync(c => c.LateDeliveryTerminations.Add(LateDeliveryTermination.ByBuyer(item.Id, 1, DeliveryDay, Amounts, null, Start)));
                break;
            case ItemRecordKind.Withdrawal:
                await SaveAsync(c => c.Withdrawals.Add(Withdrawal.ByBuyer(item.Id, 1, DeliveryDay, null, Amounts, null, Start)));
                break;
            case ItemRecordKind.ReturnReceipt:
                await SaveAsync(c => c.Returns.Add(Return.ForWithdrawal(item.Id, earlier!.Id, 1, DeliveryDay, admin, name, Start)));
                break;
            case ItemRecordKind.ReturnRejection:
                await SaveAsync(c => c.ReturnRejections.Add(ReturnRejection.Create(item.Id, earlier!.Id, 1, DeliveryDay, admin, name, Start)));
                break;
            case ItemRecordKind.NotReturnedClosure:
                await SaveAsync(c => c.NotReturnedClosures.Add(NotReturnedClosure.Create(item.Id, earlier!.Id, 1, admin, name, Start)));
                break;
            case ItemRecordKind.RefundIbanRequest:
                await SaveAsync(c => c.RefundIbanRequests.Add(RefundIbanRequest.ForItem(item.Id, RefundIbanRequestReason.NoticeWithoutIban, null, admin, name, Start)));
                break;
            case ItemRecordKind.DefectClaim:
                await SaveAsync(c => c.DefectClaims.Add(DefectClaim.Create(item.Id, 1, "Kusurlu", null, Start)));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(kindName));
        }

        int expected = ItemRecordTable.ClosesOpenQuantity(kind) ? 1 : 0;
        Assert.Equal(expected, await ClosedAsync(item.Id) - before);
    }

    // ---- KK3: a quantity above the open units is refused ----

    [Fact]
    public async Task OpenDefectClaim_RefusesZero_AndMoreThanTheOpenUnits()
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(3, null);
        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(item.Id, 2, Amounts, null, Start)));

        foreach (int quantity in new[] { 0, 2, 4 })
        {
            ItemRecordResult refused = await RunAsync((u, t) => u.OpenDefectClaimAsync(new OpenDefectClaimRequest(order.Id, item.Id, quantity, "Kusur"), t));
            Assert.Equal(TransitionRefusal.InvalidInput, refused.Refusal);
        }

        Assert.Equal(0, await CountAsync(c => c.DefectClaims.Where(d => d.OrderItemId == item.Id)));
        await AssertOrderAsync(order.Id, 1);
        ItemRecordResult opened = await RunAsync((u, t) => u.OpenDefectClaimAsync(new OpenDefectClaimRequest(order.Id, item.Id, 1, "Kusur"), t));
        Assert.Null(opened.Refusal);
    }

    [Fact]
    public async Task AddWithdrawal_RefusesMoreThanTheOpenUnits_TheUnsavedOnesIncluded()
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(3, null);
        Assert.Equal(ItemRecordRefusal.QuantityExceedsOpen, (await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(Declaration(order, item, 4, DeliveryDay), t))).Refusal);
        Assert.Equal(ItemRecordRefusal.QuantityExceedsOpen, (await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(Declaration(order, item, 0, DeliveryDay), t))).Refusal);

        // Two declarations in one transaction: the first one's units are no longer open to the second.
        ItemRecordWriteResult second = await WriteAsync(order.Id, async (m, t) =>
        {
            ItemRecordWriteResult first = await m.AddWithdrawalAsync(Declaration(order, item, 2, DeliveryDay), t);
            Assert.False(first.IsRejected);
            return await m.AddWithdrawalAsync(Declaration(order, item, 2, DeliveryDay), t);
        });
        Assert.Equal(ItemRecordRefusal.QuantityExceedsOpen, second.Refusal);
        Assert.Equal(0, await CountAsync(c => c.Withdrawals.Where(w => w.OrderItemId == item.Id)));
    }

    // ---- KK4: two declarations are two rows, two receipts two rows ----

    [Fact]
    public async Task SuccessiveDeclarations_AndPartialReceipts_AreRowsOfTheirOwn()
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(3, null);
        ItemRecordWriteResult first = await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(Declaration(order, item, 1, DeliveryDay.AddDays(1)), t));
        ItemRecordWriteResult second = await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(Declaration(order, item, 2, DeliveryDay.AddDays(1)), t));
        Assert.False(first.IsRejected);
        Assert.False(second.IsRejected);
        Assert.Equal(2, await CountAsync(c => c.Withdrawals.Where(w => w.OrderItemId == item.Id)));
        Assert.Equal(3, await ClosedAsync(item.Id));
        Assert.Equal(ItemRecordRefusal.QuantityExceedsOpen, (await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(Declaration(order, item, 1, DeliveryDay), t))).Refusal);

        Guid withdrawalId = second.RecordId!.Value;
        foreach (int day in new[] { 2, 3 })
        {
            ItemRecordWriteResult receipt = await WriteAsync(
                order.Id, (m, t) => m.AddReturnReceiptAsync(new ReturnReceiptEntry(Subject(order, item), withdrawalId, 1, DeliveryDay.AddDays(day), RecordAdmin, Start), t));
            Assert.False(receipt.IsRejected);
        }

        Assert.Equal(2, await CountAsync(c => c.Returns.Where(r => r.WithdrawalId == withdrawalId)));
        ItemRecordWriteResult third = await WriteAsync(
            order.Id, (m, t) => m.AddReturnReceiptAsync(new ReturnReceiptEntry(Subject(order, item), withdrawalId, 1, DeliveryDay.AddDays(4), RecordAdmin, Start), t));
        Assert.Equal(ItemRecordRefusal.WithdrawalNotAwaiting, third.Refusal);
    }

    [Fact]
    public async Task Writers_RequireTheOrdersLock_InTheCallersTransaction()
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(1, null);
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IAfterSalesModule module = scope.ServiceProvider.GetRequiredService<IAfterSalesModule>();
        await Assert.ThrowsAsync<InvalidOperationException>(() => module.AddWithdrawalAsync(Declaration(order, item, 1, DeliveryDay), Ct));
    }

    // ---- KK5: the return rejection only on a conditional exemption ----

    [Theory]
    [InlineData(null)]
    [InlineData(WithdrawalExemptionReason.MadeToOrder)]
    [InlineData(WithdrawalExemptionReason.Perishable)]
    [InlineData(WithdrawalExemptionReason.ExpiryDated)]
    public async Task RejectReturn_OnAnItemWithoutAConditionalExemption_IsRefused_AndWritesNothing(WithdrawalExemptionReason? exemption)
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(2, exemption);
        Withdrawal withdrawal = Withdrawal.ByBuyer(item.Id, 2, DeliveryDay.AddDays(1), null, Amounts, null, Start);
        await SaveAsync(c => c.Withdrawals.Add(withdrawal));

        ItemRecordResult result = await RunAsync((u, t) => u.RejectReturnAsync(Rejection(order.Id, item.Id, withdrawal.Id, 1, 1), t));

        Assert.Equal(TransitionRefusal.ConditionNotMet, result.Refusal);
        Assert.Equal(0, await CountAsync(c => c.ReturnRejections.Where(r => r.OrderItemId == item.Id)));
        Assert.Equal(0, await CountAsync(c => c.DispatchRecords.Where(d => d.OrderId == order.Id)));
        await AssertOrderAsync(order.Id, 1);
    }

    [Theory]
    [InlineData(WithdrawalExemptionReason.HygieneSensitive)]
    [InlineData(WithdrawalExemptionReason.SealedMedia)]
    public async Task RejectReturn_OnAConditionalExemption_WritesTheRow_AndQueuesB16(WithdrawalExemptionReason exemption)
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(2, exemption);
        Withdrawal withdrawal = Withdrawal.ByBuyer(item.Id, 2, DeliveryDay.AddDays(1), null, Amounts, null, Start);
        await SaveAsync(c => c.Withdrawals.Add(withdrawal));

        ItemRecordResult unconfirmed = await RunAsync((u, t) => u.RejectReturnAsync(Rejection(order.Id, item.Id, withdrawal.Id, 1, 1) with { Confirmed = false }, t));
        Assert.Equal(TransitionRefusal.ConfirmationRequired, unconfirmed.Refusal);
        ItemRecordResult tooMany = await RunAsync((u, t) => u.RejectReturnAsync(Rejection(order.Id, item.Id, withdrawal.Id, 1, 3), t));
        Assert.Equal(TransitionRefusal.InvalidInput, tooMany.Refusal);

        ItemRecordResult result = await RunAsync((u, t) => u.RejectReturnAsync(Rejection(order.Id, item.Id, withdrawal.Id, 1, 2), t));

        Assert.Null(result.Refusal);
        await using AppDbContext reader = NewContext();
        ReturnRejection row = await reader.ReturnRejections.SingleAsync(r => r.OrderItemId == item.Id, Ct);
        Assert.Equal((result.RecordId, withdrawal.Id, 2), ((Guid?)row.Id, row.WithdrawalId, row.Quantity));
        DispatchRecord dispatch = await reader.DispatchRecords.SingleAsync(d => d.OrderId == order.Id, Ct);
        Assert.Equal((DispatchType.B16, order.ContactEmail), (dispatch.DispatchType, dispatch.RecipientEmail));
        await AssertOrderAsync(order.Id, 2);
    }

    // ---- KK6: one claim per item; a resolved claim is reopened ----

    [Fact]
    public async Task SecondClaim_IsNotOpened_AndTheResolvedClaimIsReopened_OnTheSameRow()
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(3, null);
        ItemRecordResult opened = await RunAsync((u, t) => u.OpenDefectClaimAsync(new OpenDefectClaimRequest(order.Id, item.Id, 1, "Kusur"), t));
        Assert.Null(opened.Refusal);

        ItemRecordResult second = await RunAsync((u, t) => u.OpenDefectClaimAsync(new OpenDefectClaimRequest(order.Id, item.Id, 1, "Yine kusur"), t));
        Assert.Equal(TransitionRefusal.ConditionNotMet, second.Refusal);
        ItemRecordResult reopenOpen = await RunAsync((u, t) => u.ReopenDefectClaimAsync(ReopenDefectClaimRequest.ByCustomer(order.Id, item.Id), t));
        Assert.Equal(TransitionRefusal.ConditionNotMet, reopenOpen.Refusal);

        Assert.Null((await RunAsync((u, t) => u.ResolveDefectClaimAsync(new ResolveDefectClaimRequest(order.Id, item.Id, AdminCaller, 2), t))).Refusal);
        ItemRecordResult afterResolve = await RunAsync((u, t) => u.OpenDefectClaimAsync(new OpenDefectClaimRequest(order.Id, item.Id, 1, "Yeni"), t));
        Assert.Equal(TransitionRefusal.ConditionNotMet, afterResolve.Refusal);

        ItemRecordResult reopened = await RunAsync((u, t) => u.ReopenDefectClaimAsync(ReopenDefectClaimRequest.ByCustomer(order.Id, item.Id, 2), t));

        Assert.Null(reopened.Refusal);
        Assert.Equal(opened.RecordId, reopened.RecordId);
        await using AppDbContext reader = NewContext();
        DefectClaim claim = await reader.DefectClaims.SingleAsync(c => c.OrderItemId == item.Id, Ct);
        Assert.Equal((DefectClaimStatus.Open, 2), (claim.DefectClaimStatus, claim.Quantity));
        (DefectClaimStatus, ActorKind, int?)[] expected = [(DefectClaimStatus.Resolved, ActorKind.Admin, null), (DefectClaimStatus.Open, ActorKind.Buyer, 2)];
        Assert.Equal(expected, claim.StatusHistory.Select(e => (e.Status, e.ActorKind, e.Quantity)));
    }

    // ---- KK7: only the administrator resolves; the reopening stays inside the two years ----

    [Fact]
    public async Task Resolve_IsTheAdministratorsOnly()
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(1, null);
        Assert.Null((await RunAsync((u, t) => u.OpenDefectClaimAsync(new OpenDefectClaimRequest(order.Id, item.Id, 1, "Kusur"), t))).Refusal);

        ItemRecordResult noAdmin = await RunAsync((u, t) => u.ResolveDefectClaimAsync(new ResolveDefectClaimRequest(order.Id, item.Id, new TransitionAdmin(Guid.Empty, " "), 2), t));
        Assert.Equal(TransitionRefusal.ActorNotAllowed, noAdmin.Refusal);
        ItemRecordResult staleScreen = await RunAsync((u, t) => u.ResolveDefectClaimAsync(new ResolveDefectClaimRequest(order.Id, item.Id, AdminCaller, 1), t));
        Assert.Equal(TransitionRefusal.VersionConflict, staleScreen.Refusal);
        ItemRecordResult adminRaises = await RunAsync(
            (u, t) => u.ReopenDefectClaimAsync(new ReopenDefectClaimRequest(order.Id, item.Id, ActorKind.Admin, AdminCaller, 2, 1), t));
        Assert.Equal(TransitionRefusal.ActorNotAllowed, adminRaises.Refusal);
        Assert.Equal(DefectClaimStatus.Open, (await FindClaimAsync(item.Id))!.Status);
    }

    /// <summary>
    /// The writer reads the one channel rule (AfterSalesPeriodRules, Z-18): a physical item with no delivery date takes a claim once the order reached "Kargoya verildi" -
    /// in "Teslim edilemedi" too (03 1.11.9; 1.11.5 reads the same words) - and not before it (T320 B1).
    /// </summary>
    [Theory]
    [InlineData(OrderStatus.Preparing, false)]
    [InlineData(OrderStatus.Shipped, true)]
    [InlineData(OrderStatus.DeliveryFailed, true)]
    [InlineData(OrderStatus.Delivered, true)]
    public async Task OpenDefectClaim_OnAnUndatedPhysicalItem_FollowsTheChannelRule_DeliveryFailedIncluded(OrderStatus status, bool opens)
    {
        (Order order, OrderItem item) = await SeedAsync(status, PaymentStatus.Paid, ProductType.Physical, 1, delivered: false, exemption: null);

        ItemRecordResult result = await RunAsync((u, t) => u.OpenDefectClaimAsync(new OpenDefectClaimRequest(order.Id, item.Id, 1, "Kusur"), t));

        Assert.Equal(opens, result.Refusal is null);
        Assert.Equal(opens ? 1 : 0, await CountAsync(c => c.DefectClaims.Where(d => d.OrderItemId == item.Id)));
        Assert.Equal(AfterSalesPeriodRules.IsDefectClaimChannelOpen(ProductType.Physical, status, null, _clock.GetUtcNow()), opens);
    }

    [Theory]
    [InlineData(ActorKind.Buyer)]
    [InlineData(ActorKind.Admin)]
    public async Task Reopening_OnTheLastDay_IsAllowed_AndTheDayAfter_IsRefused(ActorKind actor)
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(1, null);
        (Order other, OrderItem otherItem) = await SeedDeliveredAsync(1, null);
        Assert.Null((await RunAsync((u, t) => u.OpenDefectClaimAsync(new OpenDefectClaimRequest(order.Id, item.Id, 1, "Kusur"), t))).Refusal);
        Assert.Null((await RunAsync((u, t) => u.ResolveDefectClaimAsync(new ResolveDefectClaimRequest(order.Id, item.Id, AdminCaller, 2), t))).Refusal);

        // The last day is the delivery date two years on (Z-18); the day after it the channel is closed for the customer and the administrator alike.
        _clock.SetUtcNow(DeliveredAt.AddYears(AfterSalesPeriodRules.DefectClaimYears).AddDays(1));
        ItemRecordResult late = await RunAsync((u, t) => u.ReopenDefectClaimAsync(Reopen(actor, order.Id, item.Id, 3), t));
        Assert.Equal(TransitionRefusal.ConditionNotMet, late.Refusal);
        ItemRecordResult lateNew = await RunAsync((u, t) => u.OpenDefectClaimAsync(new OpenDefectClaimRequest(other.Id, otherItem.Id, 1, "Kusur"), t));
        Assert.Equal(TransitionRefusal.ConditionNotMet, lateNew.Refusal);
        Assert.Equal(DefectClaimStatus.Resolved, (await FindClaimAsync(item.Id))!.Status);

        _clock.SetUtcNow(DeliveredAt.AddYears(AfterSalesPeriodRules.DefectClaimYears));
        ItemRecordResult onTime = await RunAsync((u, t) => u.ReopenDefectClaimAsync(Reopen(actor, order.Id, item.Id, 3), t));
        Assert.Null(onTime.Refusal);
        Assert.Equal(DefectClaimStatus.Open, (await FindClaimAsync(item.Id))!.Status);
    }

    // ---- KK8: B-9 to the customer and F-3 to the firm on the birth and the customer's reopening; nothing else ----

    [Fact]
    public async Task ClaimNotices_GoToTheRightRecipients_OnlyOnTheBirthAndTheCustomersReopening()
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(2, null);
        ItemRecordResult opened = await RunAsync((u, t) => u.OpenDefectClaimAsync(new OpenDefectClaimRequest(order.Id, item.Id, 1, "Kusur"), t));
        Guid claimId = opened.RecordId!.Value;
        (DispatchType, string?)[] pair = [(DispatchType.B9, order.ContactEmail), (DispatchType.F3, CompanyEmail)];
        Assert.Equal(pair, await ClaimDispatchesAsync(claimId));

        _clock.SetUtcNow(Start.AddHours(1));
        Assert.Null((await RunAsync((u, t) => u.ResolveDefectClaimAsync(new ResolveDefectClaimRequest(order.Id, item.Id, AdminCaller, 2), t))).Refusal);
        Assert.Null((await RunAsync((u, t) => u.ReopenDefectClaimAsync(ReopenDefectClaimRequest.ByAdministrator(order.Id, item.Id, AdminCaller, 3), t))).Refusal);
        Assert.Null((await RunAsync((u, t) => u.ResolveDefectClaimAsync(new ResolveDefectClaimRequest(order.Id, item.Id, AdminCaller, 4), t))).Refusal);
        Assert.Equal(pair, await ClaimDispatchesAsync(claimId));

        _clock.SetUtcNow(Start.AddHours(2));
        Assert.Null((await RunAsync((u, t) => u.ReopenDefectClaimAsync(ReopenDefectClaimRequest.ByCustomer(order.Id, item.Id), t))).Refusal);
        (DispatchType, string?)[] twice = [.. pair, .. pair];
        Assert.Equal(twice, await ClaimDispatchesAsync(claimId));
    }

    // ---- KK9: the claim does not change the order's statuses ----

    [Fact]
    public async Task ClaimOperations_LeaveTheOrdersStatuses_AndWriteNoTransition()
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(1, null);
        Assert.Null((await RunAsync((u, t) => u.OpenDefectClaimAsync(new OpenDefectClaimRequest(order.Id, item.Id, 1, "Kusur"), t))).Refusal);
        await AssertOrderAsync(order.Id, 2);
        Assert.Null((await RunAsync((u, t) => u.ResolveDefectClaimAsync(new ResolveDefectClaimRequest(order.Id, item.Id, AdminCaller, 2), t))).Refusal);
        await AssertOrderAsync(order.Id, 3);
        Assert.Null((await RunAsync((u, t) => u.ReopenDefectClaimAsync(ReopenDefectClaimRequest.ByCustomer(order.Id, item.Id), t))).Refusal);
        await AssertOrderAsync(order.Id, 4);
    }

    // ---- KK10: the panel marks show while the condition lasts ----

    [Fact]
    public async Task AwaitingReturnGoods_ShowsWhileUnitsAreOnTheWay_AndGoesWithTheLastOne()
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(3, WithdrawalExemptionReason.HygieneSensitive);
        Assert.False((await MarksAsync(item)).AwaitingReturnGoods);

        // A declaration on the delivery day without the firm's answer does not start the return; one after the delivery does.
        Withdrawal sameDay = Withdrawal.ByBuyer(item.Id, 1, DeliveryDay, null, Amounts, null, Start);
        await SaveAsync(c => c.Withdrawals.Add(sameDay));
        Assert.False((await MarksAsync(item)).AwaitingReturnGoods);

        ItemRecordWriteResult declared = await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(Declaration(order, item, 2, DeliveryDay.AddDays(1)), t));
        Guid withdrawalId = declared.RecordId!.Value;
        Assert.True((await MarksAsync(item)).AwaitingReturnGoods);

        await WriteAsync(order.Id, (m, t) => m.AddReturnReceiptAsync(new ReturnReceiptEntry(Subject(order, item), withdrawalId, 1, DeliveryDay.AddDays(2), RecordAdmin, Start), t));
        Assert.True((await MarksAsync(item)).AwaitingReturnGoods);

        NotReturnedClosure closure = NotReturnedClosure.Create(item.Id, withdrawalId, 1, _admin!.Id, _admin.Name, Start);
        await SaveAsync(c => c.NotReturnedClosures.Add(closure));
        Assert.False((await MarksAsync(item)).AwaitingReturnGoods);

        // A reopened closure no longer counts: the unit is on the way again, then the rejection settles it.
        await ExecuteAsync($"UPDATE not_returned_closures SET reopened_at = now() WHERE id = '{closure.Id}'");
        Assert.True((await MarksAsync(item)).AwaitingReturnGoods);
        Assert.Null((await RunAsync((u, t) => u.RejectReturnAsync(Rejection(order.Id, item.Id, withdrawalId, 3, 1), t))).Refusal);
        Assert.False((await MarksAsync(item)).AwaitingReturnGoods);
    }

    [Fact]
    public async Task AwaitingIban_ShowsWhileTheRequestIsOpen_AndGoesWhenFulfilledOrClosed()
    {
        (_, OrderItem item) = await SeedDeliveredAsync(1, null);
        Assert.False((await MarksAsync(item)).AwaitingIban);

        RefundIbanRequest request = RefundIbanRequest.ForItem(item.Id, RefundIbanRequestReason.NoticeWithoutIban, null, _admin!.Id, _admin.Name, Start);
        await SaveAsync(c => c.RefundIbanRequests.Add(request));
        Assert.True((await MarksAsync(item)).AwaitingIban);

        await ExecuteAsync($"UPDATE refund_iban_requests SET fulfilled_at = now() WHERE id = '{request.Id}'");
        Assert.False((await MarksAsync(item)).AwaitingIban);

        RefundIbanRequest another = RefundIbanRequest.ForItem(item.Id, RefundIbanRequestReason.NoticeWithoutIban, null, _admin.Id, _admin.Name, Start);
        await SaveAsync(c => c.RefundIbanRequests.Add(another));
        Assert.True((await MarksAsync(item)).AwaitingIban);
        await ExecuteAsync($"UPDATE refund_iban_requests SET closed_at = now() WHERE id = '{another.Id}'");
        Assert.False((await MarksAsync(item)).AwaitingIban);
    }

    // ---- helpers ----

    private ReopenDefectClaimRequest Reopen(ActorKind actor, Guid orderId, Guid itemId, int version) => actor == ActorKind.Buyer
        ? ReopenDefectClaimRequest.ByCustomer(orderId, itemId)
        : ReopenDefectClaimRequest.ByAdministrator(orderId, itemId, AdminCaller, version);

    private RejectReturnRequest Rejection(Guid orderId, Guid itemId, Guid withdrawalId, int version, int quantity) =>
        new(orderId, itemId, withdrawalId, AdminCaller, version, true, quantity, DeliveryDay.AddDays(2));

    private static ItemRecordSubject Subject(Order order, OrderItem item) =>
        new(order.Id, item.Id, item.Quantity, item.ProductType, OrderStatus.Delivered, item.DeliveryDate, item.WithdrawalExemptionReason);

    private static WithdrawalDeclaration Declaration(Order order, OrderItem item, int quantity, DateOnly declared) =>
        new(Subject(order, item), quantity, null, declared, null, new ItemFrozenAmounts(100m, 0m, 16.67m * item.Quantity), WithdrawalIban.None, Start);

    private async Task<int> ClosedAsync(Guid itemId)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IReadOnlyDictionary<Guid, int> closed = await scope.ServiceProvider.GetRequiredService<IAfterSalesModule>().ClosedQuantitiesAsync([itemId], Ct);
        return closed.GetValueOrDefault(itemId);
    }

    private async Task<ItemMarks> MarksAsync(OrderItem item)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IReadOnlyDictionary<Guid, ItemMarks> marks = await scope.ServiceProvider.GetRequiredService<IAfterSalesModule>()
            .ItemMarksAsync([new ItemDelivery(item.Id, item.ProductType, item.DeliveryDate)], Ct);
        return marks[item.Id];
    }

    private async Task<DefectClaimInfo?> FindClaimAsync(Guid itemId)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IAfterSalesModule>().FindDefectClaimAsync(itemId, Ct);
    }

    private async Task<List<(DispatchType, string?)>> ClaimDispatchesAsync(Guid claimId)
    {
        await using AppDbContext reader = NewContext();
        List<DispatchRecord> rows = await reader.DispatchRecords
            .Where(d => d.RelatedRecordType == DispatchRelatedRecordType.DefectClaim && d.RelatedRecordId == claimId)
            .OrderBy(d => d.CreatedAt).ThenBy(d => d.DispatchType).ToListAsync(Ct);
        return [.. rows.Select(d => (d.DispatchType, d.RecipientEmail))];
    }

    private async Task<int> CountAsync<TRow>(Func<AppDbContext, IQueryable<TRow>> query)
    {
        await using AppDbContext reader = NewContext();
        return await query(reader).CountAsync(Ct);
    }

    /// <summary>The order's statuses are the seeded ones, no transition record is written, and the version is <paramref name="version"/>.</summary>
    private async Task AssertOrderAsync(Guid orderId, int version)
    {
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal((OrderStatus.Delivered, PaymentStatus.Paid, version), (saved.OrderStatus, saved.PaymentStatus, saved.Version));
        Assert.Empty(await reader.TransitionRecords.Where(t => t.OrderId == orderId).ToListAsync(Ct));
    }

    private async Task<T> RunAsync<T>(Func<IOrderTransitions, CancellationToken, Task<T>> work)
        where T : IOperationResult
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IOrderTransitions unit = scope.ServiceProvider.GetRequiredService<IOrderTransitions>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(unit, token), Ct);
    }

    /// <summary>
    /// An AfterSales writer called like the transition unit calls it: in the runner's transaction, after the order's row lock, the order's version raised once
    /// with the written record (06 2.6.4).
    /// </summary>
    private async Task<ItemRecordWriteResult> WriteAsync(Guid orderId, Func<IAfterSalesModule, CancellationToken, Task<ItemRecordWriteResult>> work)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        OrderLocks locks = scope.ServiceProvider.GetRequiredService<OrderLocks>();
        IAfterSalesModule module = scope.ServiceProvider.GetRequiredService<IAfterSalesModule>();
        Written written = await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
            async token =>
            {
                await locks.LockAsync([orderId], token);
                Order order = await db.Orders.SingleAsync(o => o.Id == orderId, token);

                // The transition unit loads the items under the lock before it writes their records: a record whose root is not tracked is refused (D-95; 09 10.2 item 3).
                await db.OrderItems.Where(i => i.OrderId == orderId).LoadAsync(token);
                ItemRecordWriteResult result = await work(module, token);
                if (!result.IsRejected)
                {
                    OrderTransitions.RaiseVersionOnce(db, order);
                }

                return new Written(result);
            },
            Ct);
        return written.Result;
    }

    private Task<(Order Order, OrderItem Item)> SeedDeliveredAsync(int quantity, WithdrawalExemptionReason? exemption) =>
        SeedAsync(OrderStatus.Delivered, PaymentStatus.Paid, ProductType.Physical, quantity, delivered: true, exemption);

    private async Task<(Order Order, OrderItem Item)> SeedAsync(
        OrderStatus orderStatus, PaymentStatus paymentStatus, ProductType type, int quantity, bool delivered, WithdrawalExemptionReason? exemption)
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n:D2}", $"İl {n}", Start);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Start, "metin", Start);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Start, "metin", Start);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Start, "metin", Start).WithHosting();
        Order order = Order.Create(
            $"2026-{n:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", PaymentMethod.Card, true, "Ali@Example.com", 120m, 0m, Start.AddHours(1), Start);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.BillingRecipientName = "Ali Veli";
        order.BillingProvinceId = province.Id;
        order.BillingDistrict = "Çankaya";
        order.BillingAddressLine = "Atatürk Bulvarı 1";
        order.CompanyType = CompanyType.Tradesman;
        order.CompanyLegalName = "Örnek Ticaret";
        order.CompanyTaxIdNumber = "1234567890";
        order.CompanyProvinceId = province.Id;
        order.CompanyDistrict = "Çankaya";
        order.CompanyAddressLine = "Kızılay 2";
        order.CompanyPhone = "3121234567";
        order.CompanyEmail = "firma@example.com";
        order.CompanyKepAddress = "firma@hs01.kep.tr";
        order.PreInformationFormVersionId = form.Id;
        order.DistanceSalesContractVersionId = contract.Id;
        order.PrivacyNoticeVersionId = notice.Id;
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");

        OrderItem item = OrderItem.Create(order.Id, null, null, type, "Ürün", quantity, 100m, 20m, 16.67m, Start);
        item.WithdrawalExemptionReason = exemption;
        if (type == ProductType.Digital)
        {
            item.DownloadAllowance = 5;
        }

        if (delivered)
        {
            item.DeliveryMarkedAt = DeliveredAt;
            item.DeliveryDate = DeliveryDay;
        }

        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.LegalTextVersions.AddRange(form, contract, notice);
            c.Orders.Add(order);
            c.OrderItems.Add(item);
        });

        // The states are set around the entity (it takes them only through the unit), the pair rule of the database still applies.
        await ExecuteAsync($"UPDATE orders SET order_status = '{orderStatus}', payment_status = '{paymentStatus}' WHERE id = '{order.Id}'");
        return (order, item);
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

        // F-3 goes to the firm's contact e-mail (08 2.2.7): a fixed address here, so the firm notice is written without the Settings module's legal identity.
        services.AddScoped<ICompanyContactEmail, FixedCompanyContactEmail>();
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

    /// <summary>The writer's result as the runner reads it: a refusal rolls the transaction back.</summary>
    private sealed record Written(ItemRecordWriteResult Result) : IOperationResult
    {
        public bool IsRejected => Result.IsRejected;
    }

    private sealed class FixedCompanyContactEmail : ICompanyContactEmail
    {
        public Task<string?> GetAsync(CancellationToken ct) => Task.FromResult<string?>(CompanyEmail);
    }
}
