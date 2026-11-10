using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.IntegrationTests.Web;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// Concurrent operations on one order (T364 KK3, KK5, KK6; 02 3.31.2; 05 7.3.2): two administrators who send the same operation from the same screen
/// at once - only the first completed one is applied, the second finds the order changed and applies nothing, so the transition and the refund
/// are not written twice -, and the system's own transition that comes after an administrator's: refused against the order as it is now, with the
/// one application log line of its code and the order and no notice; the customer's refusal is told on the order page, not in the log. The order is
/// never left locked.
/// </summary>
public sealed partial class PaymentTransitionsTests
{
    [Fact]
    public async Task TwoAdministrators_MarkTheSameTransferPaidAtOnce_OnlyOneIsApplied_OneO1_OnePayment_OneNotice_T364_KK3_KK5()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.BankTransfer);

        PaymentTransitionResult[] results = await Task.WhenAll(
            RunAsync((unit, token) => unit.MarkTransferPaidAsync(Mark(order.Id, 1), token)),
            RunAsync((unit, token) => unit.MarkTransferPaidAsync(Mark(order.Id, 1), token)));

        Assert.Single(results, r => !r.IsRejected && r.Applied.Contains(TransitionCode.O1));
        Assert.Single(results, r => r.IsVersionConflict);
        await AssertStateAsync(order.Id, TransitionTable.FindShipment(TransitionCode.S1)!.To, TransitionTable.FindPayment(TransitionCode.O1)!.To, 2, records: 2);
        await using AppDbContext reader = NewContext();
        Assert.Equal(1, await reader.TransitionRecords.CountAsync(t => t.OrderId == order.Id && t.TransitionCode == TransitionCode.O1, Ct));
        Assert.Equal(1, await reader.PaymentRecords.CountAsync(p => p.OrderId == order.Id, Ct));
        Assert.Equal([DispatchType.B4], await reader.DispatchRecords.Where(d => d.OrderId == order.Id).Select(d => d.DispatchType).ToListAsync(Ct));
    }

    [Fact]
    public async Task TwoAdministrators_ProcessTheSameTransferRefundAtOnce_TheRefundIsProcessedOnce_T364_KK3_KK5()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.BankTransfer);
        Guid refundId = await SeedTransferRefundAsync(order, 50m, sent: false);

        PaymentTransitionResult[] results = await Task.WhenAll(
            RunAsync((unit, token) => unit.ProcessTransferRefundAsync(Processing(order.Id, refundId, 1), token)),
            RunAsync((unit, token) => unit.ProcessTransferRefundAsync(Processing(order.Id, refundId, 1), token)));

        Assert.Single(results, r => !r.IsRejected && r.Applied.Count == 1);
        Assert.Single(results, r => r.IsVersionConflict);
        await AssertStateAsync(order.Id, OrderStatus.Preparing, PaymentStatus.PartiallyRefunded, 2, records: 1);
        await using AppDbContext reader = NewContext();
        Assert.NotNull((await reader.Refunds.SingleAsync(r => r.Id == refundId, Ct)).SentAt);
        Assert.Equal([DispatchType.B8], await reader.DispatchRecords.Where(d => d.OrderId == order.Id).Select(d => d.DispatchType).ToListAsync(Ct));
    }

    [Fact]
    public async Task SystemAndCustomerCancellation_AfterTheAdministratorsPaidMark_AreRefused_OnlyTheSystemsIsLogged_NoNotice_NoLockRemains_T364_KK6()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.BankTransfer);
        PaymentTransitionResult marked = await RunAsync((unit, token) => unit.MarkTransferPaidAsync(Mark(order.Id, 1), token));
        Assert.False(marked.IsRejected);

        CollectingLoggerProvider logs = new();
        OrderTransitionResult system = await RunAsync(
            (unit, token) => unit.CancelOrderAsync(CancelOrderRequest.BySystem(order.Id, TransitionReason.PaymentWindowExpired), token), logs);
        OrderTransitionResult customer = await RunAsync((unit, token) => unit.CancelOrderAsync(CancelOrderRequest.ByCustomer(order.Id), token), logs);

        // Both are refused with a reason; neither moved an axis, raised the version or queued a notice beside the mark's B-4.
        Assert.NotNull(system.Refusal);
        Assert.NotNull(customer.Refusal);
        await AssertStateAsync(order.Id, OrderStatus.Preparing, PaymentStatus.Paid, 2, records: 2);
        await using (AppDbContext reader = NewContext())
        {
            Assert.Equal([DispatchType.B4], await reader.DispatchRecords.Where(d => d.OrderId == order.Id).Select(d => d.DispatchType).ToListAsync(Ct));
        }

        // The system's refusal is one application log line - the event, a refusal code of the transition unit and the order's internal id (K-1559);
        // the customer's is not logged: the order page tells it.
        string line = Assert.Single(logs.Lines, l => l.Contains(LogEvents.AutomaticTransitionRefused, StringComparison.Ordinal));
        string prefix = $"{typeof(AutomaticTransitionLog).FullName}|{LogLevel.Information}|{LogEvents.AutomaticTransitionRefused} ";
        Assert.StartsWith(prefix, line, StringComparison.Ordinal);
        Assert.EndsWith($" {order.Id}", line, StringComparison.Ordinal);
        Assert.True(Enum.TryParse(line[prefix.Length..^($" {order.Id}".Length)], out TransitionRejection _));

        // The order is not locked (02 3.31.2): the row lock ended with each operation, a new one takes it at once.
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(Ct);
        await using NpgsqlCommand command = new($"SELECT id FROM orders WHERE id = '{order.Id}' FOR UPDATE NOWAIT", connection, transaction);
        Assert.Equal(order.Id, await command.ExecuteScalarAsync(Ct));
    }

    private async Task<T> RunAsync<T>(Func<IOrderTransitions, CancellationToken, Task<T>> work, CollectingLoggerProvider logs)
        where T : IOperationResult
    {
        await using ServiceProvider provider = OrderingTestServices.Build(_collection.Dsn, TimeProvider.System, cardLineOpen: true, logs);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IOrderTransitions unit = scope.ServiceProvider.GetRequiredService<IOrderTransitions>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(unit, token), Ct);
    }
}
