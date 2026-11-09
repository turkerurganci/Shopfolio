using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The IBAN's life beside the defect remedies' refunds on a real PostgreSQL (T370 KK5; 02 7.4.5; Z-44): the IBAN goes once the last pending refund that uses it is
/// processed, whatever the refund's source - a rescission and a price reduction (T374) pending on the transfer way keep it, through the item's request or the IBAN on the
/// order's file; the last one's processing deletes it with its disposal record.
/// </summary>
public sealed partial class ItemRecordsTests
{
    private const string CustomerIban = "TR320010009999901234567890";

    [Fact]
    public async Task TheIban_OfTheItemsRequest_StaysWhileTheItemsOtherDefectRefundIsPending_AndGoesWithTheLast_KK5()
    {
        (Order order, OrderItem item) = await SeedClaimedAsync(3, PaymentMethod.BankTransfer);
        ItemRecordResult rescinded = await RunAsync((u, t) => u.RescindDefectClaimAsync(new RescindDefectClaimRequest(order.Id, item.Id, AdminCaller, 2, 1), t));
        ItemRecordResult reduced = await RunAsync((u, t) => u.ReduceDefectClaimPriceAsync(new ReduceDefectClaimPriceRequest(order.Id, item.Id, AdminCaller, 3, 10m), t));
        Assert.Null(rescinded.Refusal);
        Assert.Null(reduced.Refusal);
        Guid ibanId = await EnterItemIbanAsync(order.Id, item.Id);
        int disposals = await RefundIbanDisposalsAsync();

        // One refund processed: the other is still pending on the transfer way and is paid to the item's IBAN (02 7.4.5).
        Assert.False((await ProcessTransferAsync(order.Id, rescinded.RecordId!.Value)).IsRejected);
        Assert.True(await IbanExistsAsync(ibanId));
        Assert.Equal(disposals, await RefundIbanDisposalsAsync());

        Assert.False((await ProcessTransferAsync(order.Id, reduced.RecordId!.Value)).IsRejected);
        Assert.False(await IbanExistsAsync(ibanId));
        Assert.Equal(disposals + 1, await RefundIbanDisposalsAsync());
    }

    [Fact]
    public async Task TheIban_OnTheOrdersFile_StaysWhileAnotherItemsDefectRefundIsPending_AndGoesWithTheLast_KK5()
    {
        (Order order, OrderItem first) = await SeedClaimedAsync(3, PaymentMethod.BankTransfer);
        OrderItem second = OrderItem.Create(order.Id, null, null, ProductType.Physical, "Ürün 2", 1, 100m, 20m, 16.67m, Start);
        second.DeliveryMarkedAt = DeliveredAt;
        second.DeliveryDate = DeliveryDay;
        await SaveAsync(c => c.OrderItems.Add(second));
        Assert.Null((await RunAsync((u, t) => u.OpenDefectClaimAsync(new OpenDefectClaimRequest(order.Id, second.Id, 1, "Kusurlu"), t))).Refusal);

        int claimed = await OrderVersionAsync(order.Id);
        ItemRecordResult rescinded = await RunAsync((u, t) => u.RescindDefectClaimAsync(new RescindDefectClaimRequest(order.Id, first.Id, AdminCaller, claimed, 1), t));
        Assert.Null(rescinded.Refusal);
        Guid ibanId = await EnterItemIbanAsync(order.Id, first.Id);

        // The second item's remedy comes with the IBAN on the order's file: no request of its own, it is paid to that IBAN.
        int entered = await OrderVersionAsync(order.Id);
        ItemRecordResult reduced = await RunAsync((u, t) => u.ReduceDefectClaimPriceAsync(new ReduceDefectClaimPriceRequest(order.Id, second.Id, AdminCaller, entered, 10m), t));
        Assert.Null(reduced.Refusal);
        Assert.Equal(0, await CountAsync(c => c.RefundIbanRequests.Where(r => r.OrderItemId == second.Id)));
        int disposals = await RefundIbanDisposalsAsync();

        Assert.False((await ProcessTransferAsync(order.Id, rescinded.RecordId!.Value)).IsRejected);
        Assert.True(await IbanExistsAsync(ibanId));
        Assert.Equal(disposals, await RefundIbanDisposalsAsync());

        Assert.False((await ProcessTransferAsync(order.Id, reduced.RecordId!.Value)).IsRejected);
        Assert.False(await IbanExistsAsync(ibanId));
        Assert.Equal(disposals + 1, await RefundIbanDisposalsAsync());
    }

    /// <summary>The customer enters the IBAN in the item's open request field; the IBAN's id.</summary>
    private async Task<Guid> EnterItemIbanAsync(Guid orderId, Guid itemId)
    {
        Guid requestId;
        await using (AppDbContext reader = NewContext())
        {
            requestId = (await reader.RefundIbanRequests.SingleAsync(r => r.OrderItemId == itemId && r.FulfilledAt == null && r.ClosedAt == null, Ct)).Id;
        }

        Assert.False((await RunAsync((u, t) => u.EnterRefundIbanAsync(new EnterRefundIbanRequest(orderId, requestId, CustomerIban), t))).IsRejected);
        await using AppDbContext after = NewContext();
        return (await after.RefundIbanRequests.SingleAsync(r => r.Id == requestId, Ct)).RefundIbanId!.Value;
    }

    /// <summary>The firm processes the transfer refund on the order's current version.</summary>
    private async Task<PaymentTransitionResult> ProcessTransferAsync(Guid orderId, Guid refundId)
    {
        int version = await OrderVersionAsync(orderId);
        return await RunAsync((u, t) => u.ProcessTransferRefundAsync(new TransferRefundProcessing(orderId, refundId, _admin!.Id, _admin.Name, version, true), t));
    }

    private async Task<int> OrderVersionAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.Orders.Where(o => o.Id == orderId).Select(o => o.Version).SingleAsync(Ct);
    }

    private async Task<bool> IbanExistsAsync(Guid ibanId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.RefundIbans.AnyAsync(i => i.Id == ibanId, Ct);
    }

    private async Task<int> RefundIbanDisposalsAsync()
    {
        await using AppDbContext reader = NewContext();
        return await reader.DisposalRecords.CountAsync(d => d.DisposalRecordType == DisposalRecordType.RefundIban, Ct);
    }
}
