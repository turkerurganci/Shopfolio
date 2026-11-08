using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// The rules of the payment window (T324 criteria 4-7; 03 4.1.7-4.1.9; 06 2.7.3; D-100) on orders built in memory: the edges of the card and the transfer
/// windows, the reminder that goes only while the window runs, the "payment outcome unknown" mark read from the order's fields, and which source waits
/// for a fresh heartbeat (05 5.4.3). The database's side and the statuses other than "received, payment pending" are in the integration tests.
/// </summary>
public sealed class PaymentWindowRulesTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);

    private static Order NewOrder(PaymentMethod method, DateTimeOffset windowEndsAt, DateTimeOffset? reminderAt = null)
    {
        Order order = Order.Create("SF-2026-000001", "access", "confirm", method, isGuestOrder: true, "a@example.com", 100m, 0m, windowEndsAt, Now.AddDays(-3));
        order.PaymentReminderAt = reminderAt;
        return order;
    }

    private static bool Holds(System.Linq.Expressions.Expression<Func<Order, bool>> rule, Order order) => rule.Compile()(order);

    [Fact]
    public void CardWindow_EndsAtItsMoment_AndOnlyForACardOrder()
    {
        Assert.False(Holds(PaymentWindowRules.CardWindowEnded(Now), NewOrder(PaymentMethod.Card, Now.AddTicks(1))));
        Assert.True(Holds(PaymentWindowRules.CardWindowEnded(Now), NewOrder(PaymentMethod.Card, Now)));
        Assert.True(Holds(PaymentWindowRules.CardWindowEnded(Now), NewOrder(PaymentMethod.Card, Now.AddMinutes(-1))));
        Assert.False(Holds(PaymentWindowRules.CardWindowEnded(Now), NewOrder(PaymentMethod.BankTransfer, Now.AddMinutes(-1))));
    }

    [Fact]
    public void TransferWindow_EndsAtItsMoment_AndOnlyForATransferOrder()
    {
        Assert.False(Holds(PaymentWindowRules.TransferWindowEnded(Now), NewOrder(PaymentMethod.BankTransfer, Now.AddTicks(1))));
        Assert.True(Holds(PaymentWindowRules.TransferWindowEnded(Now), NewOrder(PaymentMethod.BankTransfer, Now)));
        Assert.False(Holds(PaymentWindowRules.TransferWindowEnded(Now), NewOrder(PaymentMethod.Card, Now.AddMinutes(-1))));
    }

    [Fact]
    public void Awaiting_IsAReceivedOrderWhosePaymentIsPending_ByTheGivenMethod()
    {
        Assert.True(Holds(PaymentWindowRules.Awaiting(PaymentMethod.Card), NewOrder(PaymentMethod.Card, Now)));
        Assert.False(Holds(PaymentWindowRules.Awaiting(PaymentMethod.BankTransfer), NewOrder(PaymentMethod.Card, Now)));
    }

    [Fact]
    public void Reminder_IsDueFromItsMoment_WhileTheWindowRuns_KK7()
    {
        DateTimeOffset end = Now.AddHours(10);

        Assert.False(Holds(PaymentWindowRules.TransferReminderDue(Now), NewOrder(PaymentMethod.BankTransfer, end, Now.AddTicks(1))));
        Assert.True(Holds(PaymentWindowRules.TransferReminderDue(Now), NewOrder(PaymentMethod.BankTransfer, end, Now)));
        Assert.True(Holds(PaymentWindowRules.TransferReminderDue(Now), NewOrder(PaymentMethod.BankTransfer, end, Now.AddDays(-1))));
    }

    [Fact]
    public void Reminder_MissedInAnOutage_IsNotDueOnceTheWindowEnded_OrWhenItWasSent_KK7_D100()
    {
        Assert.False(Holds(PaymentWindowRules.TransferReminderDue(Now), NewOrder(PaymentMethod.BankTransfer, Now, Now.AddDays(-1))));
        Assert.False(Holds(PaymentWindowRules.TransferReminderDue(Now), NewOrder(PaymentMethod.BankTransfer, Now.AddMinutes(-1), Now.AddDays(-1))));
        Assert.False(Holds(PaymentWindowRules.TransferReminderDue(Now), NewOrder(PaymentMethod.BankTransfer, Now.AddHours(10), reminderAt: null)));
        Assert.False(Holds(PaymentWindowRules.TransferReminderDue(Now), NewOrder(PaymentMethod.Card, Now.AddHours(10), Now)));
    }

    [Fact]
    public void UnknownMark_IsTheEndedCardWindowOfAWaitingOrder_ReadFromItsFields_KK5()
    {
        Assert.True(PaymentWindowRules.IsPaymentOutcomeUnknown(NewOrder(PaymentMethod.Card, Now), Now));
        Assert.False(PaymentWindowRules.IsPaymentOutcomeUnknown(NewOrder(PaymentMethod.Card, Now.AddMinutes(1)), Now));
        Assert.False(PaymentWindowRules.IsPaymentOutcomeUnknown(NewOrder(PaymentMethod.BankTransfer, Now.AddMinutes(-1)), Now));
    }

    [Fact]
    public void OnlyTheTransferCancellation_WaitsForAFreshHeartbeat()
    {
        IDueRecordSource[] sources = [new TransferPaymentExpiry(), new TransferPaymentReminder(), new CardPaymentExpiry()];

        Assert.Equal([true, false, false], sources.Select(s => s.WaitsForFreshHeartbeat));
        Assert.Equal(sources.Length, sources.Select(s => s.Name).Distinct(StringComparer.Ordinal).Count());
    }
}
