using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// T604's money items on the item records (D-906, D-1588): the defect claim's price reduction counts the transfer refunds an item's records owe but have not born yet, and the
/// payee read sees the IBAN request the caller closed in its own transaction as closed.
/// </summary>
public sealed partial class ItemRecordsTests
{
    // ---- D-906: the caps count the refunds owed but not born yet ----

    [Fact]
    public async Task PriceReduction_OnTheTransfer_CountsTheItemsRecordWhoseRefundIsNotBornYet_D906()
    {
        (Order order, OrderItem item) = await SeedClaimedAsync(3, PaymentMethod.BankTransfer, returned: true);
        OrderTransitionResult cancelled = await RunAsync((u, t) => u.CancelItemByFirmAsync(
            new CancelItemByFirmRequest(order.Id, item.Id, 1, AdminCaller, 2, true, CancellationReason.UndeliveredBuyerCaused, null, false), t));
        Assert.False(cancelled.IsRejected);

        // On the transfer the cancellation's refund line is born with the firm's panel step (T370): no refund row yet, the record owes it.
        Assert.Equal(0, await CountAsync(c => c.Refunds.Where(r => r.OrderItemId == item.Id)));
        ItemAmounts owed = ItemShares.SetAside(new ItemFrozenAmounts(100m, 20m, 16.67m), 3, 1, false, 0m, 0m);
        decimal left = PaidTotal - (owed.ProductAmount - owed.CouponShare);

        Assert.Equal(TransitionRefusal.InvalidInput, (await RunAsync((u, t) => u.ReduceDefectClaimPriceAsync(new ReduceDefectClaimPriceRequest(order.Id, item.Id, AdminCaller, 3, left + 0.01m), t))).Refusal);
        Assert.Null((await RunAsync((u, t) => u.ReduceDefectClaimPriceAsync(new ReduceDefectClaimPriceRequest(order.Id, item.Id, AdminCaller, 3, left), t))).Refusal);
    }

    // ---- D-1588: the payee read sees the request closed in the same transaction ----

    [Fact]
    public async Task ThePayeeRead_SeesAnItemRequestClosedInTheSameTransaction_AsClosed_D1588()
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(1, null);
        RefundIban onFile = RefundIban.Create(order.Id, FirmIban, Start);
        RefundIbanRequest request = RefundIbanRequest.ForItem(item.Id, RefundIbanRequestReason.NoticeWithoutIban, null, _admin!.Id, _admin.Name, Start);
        await SaveAsync(c =>
        {
            c.RefundIbans.Add(onFile);
            c.RefundIbanRequests.Add(request);
        });

        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IAfterSalesModule afterSales = scope.ServiceProvider.GetRequiredService<IAfterSalesModule>();
        RefundPayeeSubject subject = new(null, null, item.Id);

        // The item's open request waits for the customer's IBAN: no payee (02 7.4.5).
        Assert.Null((await afterSales.ResolveRefundPayeesAsync(order.Id, [subject], Ct))[0].Iban);

        // The caller closes the request in its transaction and reads again before saving (the correction's S3/S4/S9): the item waits no longer.
        RefundIbanRequest tracked = await db.RefundIbanRequests.SingleAsync(r => r.Id == request.Id, Ct);
        tracked.ClosedAt = Start.AddHours(1);
        RefundPayee payee = (await afterSales.ResolveRefundPayeesAsync(order.Id, [subject], Ct))[0];

        Assert.Equal(((Guid?)onFile.Id, (string?)FirmIban), (payee.RefundIbanId, payee.Iban));
    }
}
