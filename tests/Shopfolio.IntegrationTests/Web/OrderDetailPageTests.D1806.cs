using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>T604 (U-H, UH4) D-1806 (para; K-1075, K-1633 (1); 04 1.11.11): the itemless open IBAN request of a late payment's refund is in the decision's fact, so the decision and the write ask one rule.</summary>
public sealed partial class OrderDetailPageTests
{
    [Fact]
    public async Task TheItemlessIbanRequest_IsInTheDecisionsFact_BothStatesAreSeen_AndTheCustomersEntryIsAskedTheDecision_KK2_D1806()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        TurnedRefunds seeded = await SeedTurnedRefundsAsync(adminId, latePayment: true, withCancellation: true);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        Guid orderId = seeded.Order.OrderId;
        Guid[] itemIds;
        await using (AppDbContext db = NewContext())
        {
            itemIds = await db.OrderItems.AsNoTracking().Where(i => i.OrderId == orderId).Select(i => i.Id).ToArrayAsync(Ct);
        }

        // Closed: no open request of the order - the fact is false and the decision refuses the entry.
        RefundIbanState closedFact = await StateOfAsync(factory, orderId, itemIds);
        bool closedDecision = await EnterIsAllowedAsync(factory, orderId);

        // The firm reports the transfer of the late payment's refund failed: the itemless request opens (K-1131 (2)).
        (HttpStatusCode reported, _) = await PostForLocationAsync(
            client, Path(seeded.Order, "MarkBankTransferRefundFailed"), [("Input.PaymentRecordId", seeded.ItemlessKey.ToString("D")), ("Input.Version", "1")], key);

        RefundIbanState openFact = await StateOfAsync(factory, orderId, itemIds);
        bool openDecision = await EnterIsAllowedAsync(factory, orderId);
        RefundIbanState foreignFact = await StateOfAsync(factory, Guid.NewGuid(), []);

        Assert.Equal(HttpStatusCode.SeeOther, reported);
        Assert.False(closedFact.HasOpenRequest);
        Assert.False(closedDecision);
        Assert.True(openFact.HasOpenRequest);
        Assert.True(openDecision);
        Assert.False(foreignFact.HasOpenRequest);

        // The customer's entry on the itemless place is applied through the decision; the request is fulfilled and the fact closes again.
        CustomerOperationResult entered = await OperateAsync(factory, (ops, token) => ops.SaveRefundIbanAsync(orderId, null, ValidRefundIban, token));
        RefundIbanState afterFact = await StateOfAsync(factory, orderId, itemIds);

        Assert.Equal(CustomerOperationOutcome.Applied, entered.Outcome);
        Assert.False(afterFact.HasOpenRequest);
        await using AppDbContext reader = NewContext();
        Assert.True(await reader.RefundIbanRequests.AsNoTracking().AnyAsync(r => r.RefundId == seeded.ItemlessRefundId && r.OrderItemId == null && r.FulfilledAt != null, Ct));
    }

    private static async Task<RefundIbanState> StateOfAsync(WebApplicationFactory<Program> factory, Guid orderId, Guid[] itemIds)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        Guid[] refundIds = [.. (await scope.ServiceProvider.GetRequiredService<IPaymentModule>().ListPendingRefundsAsync(orderId, Ct)).Select(r => r.RefundId)];
        return await scope.ServiceProvider.GetRequiredService<IAfterSalesModule>().RefundIbanStateAsync(orderId, itemIds, refundIds, Ct);
    }

    private static async Task<bool> EnterIsAllowedAsync(WebApplicationFactory<Program> factory, Guid orderId)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        return (await scope.ServiceProvider.GetRequiredService<IOrderOperations>().CheckAsync(orderId, OrderOperation.EnterRefundIban, null, null, Ct)).IsAllowed;
    }

    private static async Task<CustomerOperationResult> OperateAsync(WebApplicationFactory<Program> factory, Func<ICustomerOperations, CancellationToken, Task<CustomerOperationResult>> work)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ICustomerOperations operations = scope.ServiceProvider.GetRequiredService<ICustomerOperations>();
        CustomerOperationResult? outcome = null;
        await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
            async token =>
            {
                outcome = await work(operations, token);
                return outcome.Gate;
            },
            Ct);
        return outcome!;
    }
}
