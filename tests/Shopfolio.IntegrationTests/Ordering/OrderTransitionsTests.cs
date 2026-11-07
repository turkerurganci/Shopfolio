using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>The collection of the transition unit's tests (T315): one database copied from the template.</summary>
[CollectionDefinition(OrderTransitionsDefinition.Name)]
public sealed class OrderTransitionsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "OrderTransitions";
}

/// <summary>
/// The transition unit on a real PostgreSQL (T315 KK2-KK7; 09 9.1 items 3, 4, 8; 05 4.1): the white list and the actor before the write, the row
/// lock and the condition against the order as it is now, the appended record, the closure rule at the end of the same operation, and the
/// rollback of an operation that fails after its first write. A second transaction's wait is read from <c>pg_stat_activity</c>, not timed.
/// </summary>
[Collection(OrderTransitionsDefinition.Name)]
public sealed class OrderTransitionsTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    private static readonly ItemAmounts Amounts = new(100m, 0m, 16.67m);

    private Admin? _admin;

    private readonly CollectionDatabaseFixture _collection;

    private int _counter;

    // The context and the lock taker of the scope RunAsync opened: the stand-in operations of the closure tests take the lock and write item records through them.
    private AppDbContext? _db;

    private OrderLocks? _locks;

    public OrderTransitionsTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private TransitionActor AdminActor => TransitionActor.Admin(_admin!.Id, _admin.Name);

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE cancellations, internal_notes, transition_records, order_items, orders, admins, legal_text_versions, provinces CASCADE");
        _admin = Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", Now);
        await SaveAsync(c => c.Admins.Add(_admin));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK3: the white list and the correction ----

    [Fact]
    public async Task Apply_AnAllowedTransition_WritesTheStatus_OneRecord_AndRaisesTheVersionOnce_KK2()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Paid);

        TransitionOutcome outcome = await RunAsync((unit, token) => unit.ApplyAsync(
            new TransitionRequest(order.Id, TransitionCode.S1, TransitionActor.System), null, token));

        Assert.False(outcome.IsRejected);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        TransitionRecord record = await reader.TransitionRecords.SingleAsync(t => t.OrderId == order.Id, Ct);
        Assert.Equal((OrderStatus.Preparing, PaymentStatus.Paid, 2), (saved.OrderStatus, saved.PaymentStatus, saved.Version));
        Assert.Equal((TransitionCode.S1, ActorKind.System), (record.TransitionCode, record.ActorKind));
        Assert.Equal([record.Id], outcome.TransitionRecordIds);
    }

    [Fact]
    public async Task Apply_TwoTransitionsTogether_AreOneVersionRaiseAndTwoRecords_AndTheCancellationOfAnUnpaidOrderTakesBothAxes()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);

        TransitionOutcome outcome = await RunAsync((unit, token) => unit.ApplyAsync(
            new TransitionRequest(order.Id, [TransitionCode.S3, TransitionCode.O2], TransitionActor.Buyer), null, token));

        Assert.False(outcome.IsRejected);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Cancelled, PaymentStatus.Failed, 2), (saved.OrderStatus, saved.PaymentStatus, saved.Version));
        Assert.Equal(
            new[] { TransitionCode.S3, TransitionCode.O2 }.Order(),
            (await reader.TransitionRecords.Where(t => t.OrderId == order.Id).Select(t => t.TransitionCode).ToListAsync(Ct)).Order());
    }

    [Theory]
    [InlineData(TransitionCode.Correction, OrderStatus.Placed, PaymentStatus.Pending)]
    [InlineData(TransitionCode.S5, OrderStatus.Placed, PaymentStatus.Paid)]
    [InlineData(TransitionCode.S6, OrderStatus.Preparing, PaymentStatus.Paid)]
    [InlineData(TransitionCode.S3, OrderStatus.Delivered, PaymentStatus.Paid)]
    [InlineData(TransitionCode.O1, OrderStatus.Cancelled, PaymentStatus.Failed)]
    [InlineData(TransitionCode.O5, OrderStatus.Preparing, PaymentStatus.Paid)]
    public async Task Apply_ATransitionOutsideTheTable_IsRefused_AndWritesNothing_KK3(TransitionCode code, OrderStatus orderStatus, PaymentStatus paymentStatus)
    {
        (Order order, _) = await SeedOrderAsync(orderStatus, paymentStatus);

        TransitionOutcome outcome = await RunAsync((unit, token) => unit.ApplyAsync(new TransitionRequest(order.Id, code, AdminActor), null, token));

        Assert.Equal(TransitionRejection.NotInTheTable, outcome.Rejection);
        await AssertUntouchedAsync(order.Id, orderStatus, paymentStatus);
    }

    [Fact]
    public async Task Apply_ForAnOrderThatDoesNotExist_IsRefused()
    {
        TransitionOutcome outcome = await RunAsync((unit, token) => unit.ApplyAsync(
            new TransitionRequest(EntityIds.New(), TransitionCode.S3, AdminActor), null, token));

        Assert.Equal(TransitionRejection.OrderNotFound, outcome.Rejection);
    }

    [Fact]
    public async Task Correction_FromTheBuyerOrTheSystem_IsRefused_AndOnlyTheAdministratorsCallWorks_KK3()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid);
        Guid recordId = (await RunAsync((unit, token) => unit.ApplyAsync(new TransitionRequest(order.Id, TransitionCode.S5, AdminActor), null, token))).TransitionRecordIds.Single();

        foreach (TransitionActor actor in new[] { TransitionActor.Buyer, TransitionActor.System })
        {
            TransitionOutcome refused = await RunAsync((unit, token) => unit.CorrectAsync(
                new CorrectionRequest(order.Id, recordId, CorrectionReason.WrongOrder, null, actor, 2), null, token));

            Assert.Equal(TransitionRejection.CorrectionRequiresPanel, refused.Rejection);
        }

        TransitionOutcome withoutReason = await RunAsync((unit, token) => unit.CorrectAsync(new CorrectionRequest(order.Id, recordId, null, null, AdminActor, 2), null, token));
        TransitionOutcome staleScreen = await RunAsync((unit, token) => unit.CorrectAsync(new CorrectionRequest(order.Id, recordId, CorrectionReason.WrongOrder, null, AdminActor, 1), null, token));
        Assert.Equal(TransitionRejection.CorrectionReasonRequired, withoutReason.Rejection);
        Assert.Equal(TransitionRejection.VersionConflict, staleScreen.Rejection);
        await AssertUntouchedAsync(order.Id, OrderStatus.Shipped, PaymentStatus.Paid, version: 2);

        TransitionOutcome applied = await RunAsync((unit, token) => unit.CorrectAsync(
            new CorrectionRequest(order.Id, recordId, CorrectionReason.WrongOrder, "yanlış işaret", AdminActor, 2), null, token));

        Assert.False(applied.IsRejected);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Preparing, PaymentStatus.Paid, 3), (saved.OrderStatus, saved.PaymentStatus, saved.Version));
        TransitionRecord correction = await reader.TransitionRecords.SingleAsync(t => t.TransitionCode == TransitionCode.Correction, Ct);
        Assert.Equal((recordId, CorrectionReason.WrongOrder, ActorKind.Admin), (correction.CorrectedTransitionRecordId, correction.CorrectionReason, correction.ActorKind));
        Assert.True(await reader.TransitionRecords.AnyAsync(t => t.Id == recordId, Ct));
    }

    [Fact]
    public async Task Correction_OfARecordThatWasCorrectedAlready_OrIsAForeignOne_IsRefused()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid);
        (Order other, _) = await SeedOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid);
        Guid recordId = (await RunAsync((unit, token) => unit.ApplyAsync(new TransitionRequest(order.Id, TransitionCode.S5, AdminActor), null, token))).TransitionRecordIds.Single();
        Assert.False((await RunAsync((unit, token) => unit.CorrectAsync(new CorrectionRequest(order.Id, recordId, CorrectionReason.WrongOrder, null, AdminActor, 2), null, token))).IsRejected);

        TransitionOutcome again = await RunAsync((unit, token) => unit.CorrectAsync(new CorrectionRequest(order.Id, recordId, CorrectionReason.WrongOrder, null, AdminActor, 3), null, token));
        TransitionOutcome foreign = await RunAsync((unit, token) => unit.CorrectAsync(new CorrectionRequest(other.Id, recordId, CorrectionReason.WrongOrder, null, AdminActor, 1), null, token));

        Assert.Equal(TransitionRejection.CorrectionNotPossible, again.Rejection);
        Assert.Equal(TransitionRejection.CorrectionNotPossible, foreign.Rejection);
    }

    // ---- KK4: the actor ----

    [Theory]
    [InlineData(TransitionCode.S5, ActorKind.Buyer, OrderStatus.Preparing, PaymentStatus.Paid)]
    [InlineData(TransitionCode.S5, ActorKind.System, OrderStatus.Preparing, PaymentStatus.Paid)]
    [InlineData(TransitionCode.S1, ActorKind.Admin, OrderStatus.Placed, PaymentStatus.Paid)]
    [InlineData(TransitionCode.S1, ActorKind.Buyer, OrderStatus.Placed, PaymentStatus.Paid)]
    [InlineData(TransitionCode.O3, ActorKind.Buyer, OrderStatus.Preparing, PaymentStatus.Paid)]
    public async Task Apply_ByAKindOfActorTheTableDoesNotName_IsRefused_AndWritesNothing_KK4(TransitionCode code, ActorKind kind, OrderStatus orderStatus, PaymentStatus paymentStatus)
    {
        (Order order, _) = await SeedOrderAsync(orderStatus, paymentStatus);
        TransitionActor actor = kind == ActorKind.Admin ? AdminActor : kind == ActorKind.Buyer ? TransitionActor.Buyer : TransitionActor.System;

        TransitionOutcome outcome = await RunAsync((unit, token) => unit.ApplyAsync(new TransitionRequest(order.Id, code, actor), null, token));

        Assert.Equal(TransitionRejection.ActorNotAllowed, outcome.Rejection);
        await AssertUntouchedAsync(order.Id, orderStatus, paymentStatus);
    }

    [Fact]
    public async Task Apply_ByAnAdministratorWithoutAnIdentity_IsRefused()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid);

        TransitionOutcome outcome = await RunAsync((unit, token) => unit.ApplyAsync(
            new TransitionRequest(order.Id, TransitionCode.S5, new TransitionActor(ActorKind.Admin, null, null)), null, token));

        Assert.Equal(TransitionRejection.ActorNotAllowed, outcome.Rejection);
    }

    [Fact]
    public async Task Apply_AReasonOnATransitionThatTakesNone_OrTwoReasons_IsRefused()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid);

        TransitionOutcome onShip = await RunAsync((unit, token) => unit.ApplyAsync(
            new TransitionRequest(order.Id, TransitionCode.S5, AdminActor, TransitionReasons.OfFirm(CancellationReason.Other, "not")), null, token));
        TransitionOutcome two = await RunAsync((unit, token) => unit.ApplyAsync(
            new TransitionRequest(order.Id, TransitionCode.S4, AdminActor, new TransitionReasons(TransitionReason.Closure, CancellationReason.Other, null)), null, token));

        Assert.Equal(TransitionRejection.ReasonNotAllowed, onShip.Rejection);
        Assert.Equal(TransitionRejection.ReasonNotAllowed, two.Rejection);
        await AssertUntouchedAsync(order.Id, OrderStatus.Preparing, PaymentStatus.Paid);
    }

    // ---- KK5: the row lock and the condition against the order as it is now ----

    [Fact]
    public async Task Race_TheSecondTransition_WaitsForTheFirst_AndIsCheckedAgainstTheNewState_KK5()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid);

        (TransitionOutcome first, TransitionOutcome second) = await RaceAsync(
            order.Id,
            (unit, token) => unit.ApplyAsync(new TransitionRequest(order.Id, TransitionCode.S5, AdminActor), null, token),
            (unit, token) => unit.ApplyAsync(new TransitionRequest(order.Id, TransitionCode.S4, TransitionActor.Buyer), null, token));

        Assert.False(first.IsRejected);
        Assert.Equal(TransitionRejection.NotInTheTable, second.Rejection);
        await using AppDbContext reader = NewContext();
        Assert.Equal([TransitionCode.S5], await reader.TransitionRecords.Where(t => t.OrderId == order.Id).Select(t => t.TransitionCode).ToListAsync(Ct));
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Shipped, 2), (saved.OrderStatus, saved.Version));
    }

    [Fact]
    public async Task Race_TheConditionOfTheSecondTransition_IsReadAgainstTheOrderAsItIsAfterTheFirst_AndWhenItFailsNothingIsApplied_KK5()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid);
        PaymentStatus? seen = null;

        (TransitionOutcome first, TransitionOutcome second) = await RaceAsync(
            order.Id,
            (unit, token) => unit.ApplyAsync(new TransitionRequest(order.Id, TransitionCode.O3, AdminActor), null, token),
            (unit, token) => unit.ApplyAsync(
                new TransitionRequest(order.Id, TransitionCode.S5, AdminActor),
                (current, _) =>
                {
                    seen = current.PaymentStatus;
                    return Task.FromResult(current.PaymentStatus == PaymentStatus.Paid);
                },
                token));

        Assert.False(first.IsRejected);
        Assert.Equal(TransitionRejection.ConditionNotMet, second.Rejection);
        Assert.Equal(PaymentStatus.PartiallyRefunded, seen);
        await AssertUntouchedAsync(order.Id, OrderStatus.Preparing, PaymentStatus.PartiallyRefunded, version: 2);
    }

    [Fact]
    public async Task Race_AnAdministratorWhoseScreenShowedTheOlderVersion_IsRefusedAfterWaiting_KK5()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid);

        (TransitionOutcome first, TransitionOutcome second) = await RaceAsync(
            order.Id,
            (unit, token) => unit.ApplyAsync(new TransitionRequest(order.Id, TransitionCode.O3, AdminActor, null, 1), null, token),
            (unit, token) => unit.ApplyAsync(new TransitionRequest(order.Id, TransitionCode.S5, AdminActor, null, 1), null, token));

        Assert.False(first.IsRejected);
        Assert.Equal(TransitionRejection.VersionConflict, second.Rejection);
        await AssertUntouchedAsync(order.Id, OrderStatus.Preparing, PaymentStatus.PartiallyRefunded, version: 2);
    }

    // ---- The pairs of the two axes ----

    [Fact]
    public async Task Apply_ATransitionThatWouldEndInAForbiddenPair_IsRefusedBeforeAnythingIsWritten()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);

        TransitionOutcome cancelOnly = await RunAsync((unit, token) => unit.ApplyAsync(new TransitionRequest(order.Id, TransitionCode.S3, TransitionActor.Buyer), null, token));
        TransitionOutcome failOnly = await RunAsync((unit, token) => unit.ApplyAsync(new TransitionRequest(order.Id, TransitionCode.O2, TransitionActor.Buyer), null, token));

        Assert.Equal(TransitionRejection.ForbiddenStatePair, cancelOnly.Rejection);
        Assert.Equal(TransitionRejection.ForbiddenStatePair, failOnly.Rejection);
        await AssertUntouchedAsync(order.Id, OrderStatus.Placed, PaymentStatus.Pending);
    }

    [Fact]
    public async Task IsPairAllowed_EqualsTheCombinationRuleOfTheDatabase_ForAllPairs()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        int checkedPairs = 0;
        foreach (OrderStatus orderStatus in Enum.GetValues<OrderStatus>())
        {
            foreach (PaymentStatus paymentStatus in Enum.GetValues<PaymentStatus>())
            {
                await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(Ct);
                bool accepted;
                try
                {
                    await using NpgsqlCommand command = new("UPDATE orders SET order_status = @o, payment_status = @p WHERE id = @id", connection, transaction);
                    command.Parameters.AddWithValue("o", orderStatus.ToString());
                    command.Parameters.AddWithValue("p", paymentStatus.ToString());
                    command.Parameters.AddWithValue("id", order.Id);
                    await command.ExecuteNonQueryAsync(Ct);
                    accepted = true;
                }
                catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.CheckViolation)
                {
                    accepted = false;
                }

                await transaction.RollbackAsync(Ct);
                Assert.True(accepted == TransitionTable.IsPairAllowed(orderStatus, paymentStatus), $"{orderStatus} + {paymentStatus}: database {accepted}.");
                checkedPairs++;
            }
        }

        Assert.Equal(Enum.GetValues<OrderStatus>().Length * Enum.GetValues<PaymentStatus>().Length, checkedPairs);
    }

    // ---- KK6: the records ----

    [Fact]
    public async Task TransitionRecord_CannotBeChanged_ButItsAdministratorLinkMayBeEmptied_KK6()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid);
        Assert.False((await RunAsync((unit, token) => unit.ApplyAsync(new TransitionRequest(order.Id, TransitionCode.S5, AdminActor), null, token))).IsRejected);

        await using AppDbContext editing = NewContext();
        TransitionRecord record = await editing.TransitionRecords.SingleAsync(t => t.OrderId == order.Id, Ct);
        editing.Entry(record).Property(nameof(TransitionRecord.CreatedAt)).CurrentValue = Now.AddDays(1);
        await Assert.ThrowsAsync<InvalidOperationException>(() => editing.SaveChangesAsync(Ct));

        await using AppDbContext deleting = NewContext();
        deleting.TransitionRecords.Remove(await deleting.TransitionRecords.SingleAsync(t => t.OrderId == order.Id, Ct));
        await Assert.ThrowsAsync<InvalidOperationException>(() => deleting.SaveChangesAsync(Ct));

        await using AppDbContext erasing = NewContext();
        TransitionRecord erasable = await erasing.TransitionRecords.SingleAsync(t => t.OrderId == order.Id, Ct);
        erasable.ErasePersonalData();
        await erasing.SaveChangesAsync(Ct);
        await using AppDbContext reader = NewContext();
        TransitionRecord after = await reader.TransitionRecords.SingleAsync(t => t.OrderId == order.Id, Ct);
        Assert.Equal((null, null, TransitionCode.S5), (after.AdminId, after.AdminName, after.TransitionCode));
    }

    [Fact]
    public async Task AnItemRecord_WritesNoTransitionRecordRow_ByItself_KK6()
    {
        (Order order, OrderItem item) = await SeedOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, quantity: 3);

        TransitionOutcome outcome = await RunAsync(async (unit, token) =>
        {
            await _locks!.LockAsync([order.Id], token);
            Order locked = await _db!.Orders.SingleAsync(o => o.Id == order.Id, token);
            _db.Cancellations.Add(Cancellation.ByBuyer(item.Id, 1, Amounts, null, Now));
            locked.BumpVersion();
            return new TransitionOutcome(null, [], await unit.EvaluateClosureAsync(locked, TransitionActor.Buyer, token));
        });

        Assert.False(outcome.Closure.Closed);
        await using AppDbContext reader = NewContext();
        Assert.Equal(1, await reader.Cancellations.CountAsync(c => c.OrderItemId == item.Id, Ct));
        Assert.False(await reader.TransitionRecords.AnyAsync(t => t.OrderId == order.Id, Ct));
    }

    // ---- KK7: the closure at the end of the same operation ----

    [Theory]
    [InlineData(OrderStatus.Preparing, TransitionCode.S4)]
    [InlineData(OrderStatus.Placed, TransitionCode.S3)]
    public async Task Closure_WhenTheLastOpenItemIsCancelled_CancelsTheOrderInTheSameOperation_WithOneSystemRecord_KK7(OrderStatus status, TransitionCode expected)
    {
        (Order order, OrderItem item) = await SeedOrderAsync(status, PaymentStatus.Paid, quantity: status == OrderStatus.Placed ? 1 : 2, productType: status == OrderStatus.Placed ? ProductType.Digital : ProductType.Physical);
        ClosureResult closure = ClosureResult.None;

        TransitionOutcome outcome = await RunAsync(async (unit, token) =>
        {
            AppDbContext db = _db!;
            await _locks!.LockAsync([order.Id], token);
            Order locked = await db.Orders.SingleAsync(o => o.Id == order.Id, token);
            db.Cancellations.Add(Cancellation.ByBuyer(item.Id, item.Quantity, Amounts, null, Now));
            locked.BumpVersion();
            closure = await unit.EvaluateClosureAsync(locked, TransitionActor.Buyer, token);
            return new TransitionOutcome(null, [], closure);
        });

        Assert.False(outcome.IsRejected);
        Assert.Equal(expected, closure.Code);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        TransitionRecord record = await reader.TransitionRecords.SingleAsync(t => t.OrderId == order.Id, Ct);
        Assert.Equal((OrderStatus.Cancelled, PaymentStatus.Paid, 2), (saved.OrderStatus, saved.PaymentStatus, saved.Version));
        Assert.Equal((expected, ActorKind.System, (TransitionReason?)TransitionReason.Closure), (record.TransitionCode, record.ActorKind, record.TransitionReason));
        Assert.Equal(record.Id, closure.TransitionRecordId);
    }

    [Fact]
    public async Task Closure_WhileAnOpenPhysicalItemRemains_ClosesNothing_KK7()
    {
        (Order order, OrderItem item) = await SeedOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, quantity: 2);

        TransitionOutcome outcome = await RunAsync(async (unit, token) =>
        {
            AppDbContext db = _db!;
            await _locks!.LockAsync([order.Id], token);
            Order locked = await db.Orders.SingleAsync(o => o.Id == order.Id, token);
            db.Cancellations.Add(Cancellation.ByBuyer(item.Id, 1, Amounts, null, Now));
            locked.BumpVersion();
            return new TransitionOutcome(null, [], await unit.EvaluateClosureAsync(locked, TransitionActor.Buyer, token));
        });

        Assert.False(outcome.Closure.Closed);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Preparing, 2), (saved.OrderStatus, saved.Version));
        Assert.False(await reader.TransitionRecords.AnyAsync(t => t.OrderId == order.Id, Ct));
    }

    [Fact]
    public async Task Closure_WithoutTheOrdersRowLock_Throws()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid);

        await Assert.ThrowsAsync<InvalidOperationException>(() => RunAsync(async (unit, token) =>
        {
            Order unlocked = await _db!.Orders.SingleAsync(o => o.Id == order.Id, token);
            return new TransitionOutcome(null, [], await unit.EvaluateClosureAsync(unlocked, TransitionActor.System, token));
        }));
    }

    [Fact]
    public async Task Apply_WhenTheOperationFailsAfterTheTransition_TheStatusAndTheRecordsAreRolledBackTogether()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);

        await Assert.ThrowsAsync<InvalidOperationException>(() => RunAsync<TransitionOutcome>(async (unit, token) =>
        {
            TransitionOutcome applied = await unit.ApplyAsync(new TransitionRequest(order.Id, [TransitionCode.S3, TransitionCode.O2], TransitionActor.Buyer), null, token);
            Assert.False(applied.IsRejected);
            throw new InvalidOperationException("the inner side effect failed");
        }));

        await AssertUntouchedAsync(order.Id, OrderStatus.Placed, PaymentStatus.Pending);
    }

    [Fact]
    public async Task Apply_ARefusalAfterTheFirstWrite_RollsBackWhatTheOperationWroteBefore()
    {
        (Order order, _) = await SeedOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid);

        TransitionOutcome outcome = await RunAsync(async (unit, token) =>
        {
            TransitionOutcome applied = await unit.ApplyAsync(new TransitionRequest(order.Id, TransitionCode.S5, AdminActor), null, token);
            Assert.False(applied.IsRejected);
            return TransitionOutcome.Rejected(TransitionRejection.ConditionNotMet);
        });

        Assert.True(outcome.IsRejected);
        await AssertUntouchedAsync(order.Id, OrderStatus.Preparing, PaymentStatus.Paid);
    }

    // ---- Helpers ----

    private async Task AssertUntouchedAsync(Guid orderId, OrderStatus orderStatus, PaymentStatus paymentStatus, int version = 1)
    {
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal((orderStatus, paymentStatus, version), (saved.OrderStatus, saved.PaymentStatus, saved.Version));
        int records = await reader.TransitionRecords.CountAsync(t => t.OrderId == orderId, Ct);
        Assert.Equal(version - 1, records);
    }

    private async Task<T> RunAsync<T>(Func<OrderTransitions, CancellationToken, Task<T>> work)
        where T : IOperationResult
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        OrderTransitions unit = scope.ServiceProvider.GetRequiredService<OrderTransitions>();
        _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _locks = scope.ServiceProvider.GetRequiredService<OrderLocks>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(unit, token), Ct);
    }

    /// <summary>
    /// Runs two operations on the same order in two transactions: the first applies and does not commit yet; the second starts, waits on the order's
    /// row lock (read from <c>pg_stat_activity</c>), and runs once the first commits.
    /// </summary>
    private async Task<(TransitionOutcome First, TransitionOutcome Second)> RaceAsync(
        Guid orderId,
        Func<OrderTransitions, CancellationToken, Task<TransitionOutcome>> first,
        Func<OrderTransitions, CancellationToken, Task<TransitionOutcome>> second)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope firstScope = provider.CreateAsyncScope();
        await using AsyncServiceScope secondScope = provider.CreateAsyncScope();
        AppDbContext firstDb = firstScope.ServiceProvider.GetRequiredService<AppDbContext>();
        AppDbContext secondDb = secondScope.ServiceProvider.GetRequiredService<AppDbContext>();

        await using IDbContextTransaction firstTransaction = await firstDb.Database.BeginTransactionAsync(Ct);
        TransitionOutcome firstOutcome = await first(firstScope.ServiceProvider.GetRequiredService<OrderTransitions>(), Ct);
        await firstDb.SaveChangesAsync(Ct);

        // The connection stays open, so the runner's transaction runs on the backend whose wait is watched.
        await secondDb.Database.OpenConnectionAsync(Ct);
        int pid = (await secondDb.Database.SqlQuery<int>($"SELECT pg_backend_pid() AS \"Value\"").ToListAsync(Ct)).Single();
        OrderTransitions secondUnit = secondScope.ServiceProvider.GetRequiredService<OrderTransitions>();
        Task<TransitionOutcome> waiting = secondScope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => second(secondUnit, token), Ct);
        await WaitUntilBlockedAsync(pid);
        Assert.False(waiting.IsCompleted);
        await firstTransaction.CommitAsync(Ct);

        TransitionOutcome secondOutcome = await waiting.WaitAsync(TimeSpan.FromSeconds(30), Ct);
        Assert.Equal(orderId, orderId);
        return (firstOutcome, secondOutcome);
    }

    /// <summary>Waits until the backend waits on a lock (pg_stat_activity), at most about ten seconds.</summary>
    private async Task WaitUntilBlockedAsync(int pid)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        for (int attempt = 0; attempt < 400; attempt++)
        {
            await using NpgsqlCommand command = new("SELECT wait_event_type FROM pg_stat_activity WHERE pid = @pid", connection);
            command.Parameters.AddWithValue("pid", pid);
            if (await command.ExecuteScalarAsync(Ct) is string type && type == "Lock")
            {
                return;
            }

            await Task.Delay(25, Ct);
        }

        throw new Xunit.Sdk.XunitException($"Backend {pid} never waited on a lock.");
    }

    private async Task<(Order Order, OrderItem Item)> SeedOrderAsync(
        OrderStatus orderStatus,
        PaymentStatus paymentStatus,
        int quantity = 1,
        ProductType productType = ProductType.Physical)
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n:D2}", $"İl {n}", Now);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Now, "metin", Now);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Now, "metin", Now);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Now, "metin", Now).WithHosting();
        Order order = Order.Create(
            $"2026-{n:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", PaymentMethod.Card, true, "Ali@Example.com", 120m, 0m, Now.AddHours(1), Now);
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
        OrderItem item = OrderItem.Create(order.Id, null, null, productType, "Ürün", quantity, 100m, 20m, 16.67m, Now);
        if (productType == ProductType.Digital)
        {
            item.DownloadAllowance = 5;
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
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IJobQueue, JobQueue>();
        services.AddFileStore();
        services.AddOptions<Shopfolio.Infrastructure.InstallationSettings.StatusThresholdOptions>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.IDiskUsageReader, Shopfolio.Infrastructure.Status.DriveDiskUsageReader>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.DiskUsageGuard>();
        services.AddSingleton<ImageProcessor>();
        services.AddCatalogModule();
        services.AddAfterSalesModule();
        services.AddOrderingModule();
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
}
