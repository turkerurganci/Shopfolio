using System.Linq.Expressions;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The rules of the payment window (03 4.1.7-4.1.9; 02 4.2 Z-7, Z-8, Z-9; 06 2.7.3): when an order still waits for its payment, when its window has
/// ended and when its transfer reminder is due. One home for the conditions the scheduled sources, the final query's handler and the panel's
/// "payment outcome unknown" mark read - the mark is not stored, it is this rule over the order's own fields (06 2.7.3). Each rule is an expression,
/// so the source's query and its second look on the locked order are the same text.
/// </summary>
internal static class PaymentWindowRules
{
    /// <summary>
    /// Z-8's last payment day as the panel shows it (04 9.8, 9.9.5; D-1436): the window ends at the 00:00 after its last day (<see cref="TurkeyTime.EndOfPeriod"/>), so the day is the one that holds the
    /// instant just before the end - the day the customer's notices (B-2, B-3) name. The list and the order's screen read it here.
    /// </summary>
    public static DateOnly LastPaymentDay(DateTimeOffset paymentWindowEndsAt) => TurkeyTime.DateOf(paymentWindowEndsAt.AddTicks(-1));

    /// <summary>An order that waits for its payment: received, payment pending, by the given method.</summary>
    public static Expression<Func<Order, bool>> Awaiting(PaymentMethod method) =>
        o => o.PaymentMethod == method && o.PaymentStatus == PaymentStatus.Pending && o.OrderStatus == OrderStatus.Placed;

    /// <summary>
    /// A card order whose window (Z-7) has ended and that still waits: the final query is due, and while it has no answer the order carries the
    /// "payment outcome unknown" mark (03 4.1.7; 06 2.7.3). The order is not cancelled by the window alone.
    /// </summary>
    public static Expression<Func<Order, bool>> CardWindowEnded(DateTimeOffset now) =>
        o => o.PaymentMethod == PaymentMethod.Card && o.PaymentStatus == PaymentStatus.Pending && o.OrderStatus == OrderStatus.Placed
            && o.PaymentWindowEndsAt <= now;

    /// <summary>A bank transfer order whose window (Z-8) has ended and that still waits: it is cancelled by the system (03 4.1.8).</summary>
    public static Expression<Func<Order, bool>> TransferWindowEnded(DateTimeOffset now) =>
        o => o.PaymentMethod == PaymentMethod.BankTransfer && o.PaymentStatus == PaymentStatus.Pending && o.OrderStatus == OrderStatus.Placed
            && o.PaymentWindowEndsAt <= now;

    /// <summary>
    /// A bank transfer order whose reminder (Z-9, B-3) is due: the moment has come and the window still runs (03 4.1.9; D-100). A reminder missed in
    /// an outage goes on the return only if the window has not ended; the field is emptied once the reminder is queued, so it goes once per window.
    /// </summary>
    public static Expression<Func<Order, bool>> TransferReminderDue(DateTimeOffset now) =>
        o => o.PaymentMethod == PaymentMethod.BankTransfer && o.PaymentStatus == PaymentStatus.Pending && o.OrderStatus == OrderStatus.Placed
            && o.PaymentReminderAt != null && o.PaymentReminderAt <= now && o.PaymentWindowEndsAt > now;

    /// <summary>Whether the card order carries the "payment outcome unknown" mark at <paramref name="now"/> (06 2.7.3): its window has ended and it still waits.</summary>
    public static bool IsPaymentOutcomeUnknown(Order order, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(order);
        return CardWindowEnded(now).Compile()(order);
    }
}

/// <summary>
/// The end of an order's payment window and the moment of its transfer reminder (02 4.2 Z-7, Z-8, Z-9; P-7, P-18; 09 3.5): the card window is the
/// product constant <see cref="ProductConstants.CardPaymentMinutes"/> from the start; the transfer window is the firm's setting
/// (<c>BankTransferPaymentWindow</c>, P-7), read when the window starts, counted in business days (02 4.1.3). The start is the order's confirmation, or
/// the moment the transfer's "paid" mark is corrected (03 4.1.8, 4.1.9; K-685). The order's confirmation and the correction write what this returns on
/// the order's own fields; the scheduled sources read those fields, never this class.
/// </summary>
internal sealed class PaymentWindows
{
    private readonly ISettingsModule _settings;

    public PaymentWindows(ISettingsModule settings)
    {
        _settings = settings;
    }

    /// <summary>The window's end and, for a bank transfer, the reminder's moment (the last business day's start; <see cref="Infrastructure.Jobs.TransferSchedule.ReminderAt"/>).</summary>
    public async Task<PaymentWindow> StartAsync(PaymentMethod method, DateTimeOffset start, CancellationToken ct)
    {
        switch (method)
        {
            case PaymentMethod.Card:
                return new PaymentWindow(PeriodCounting.SpanEnd(start, TimeSpan.FromMinutes(ProductConstants.CardPaymentMinutes)), null);
            case PaymentMethod.BankTransfer:
                int businessDays = (await _settings.GetPeriodsAsync(ct)).BankTransferPaymentWindow;
                DateTimeOffset end = PeriodCounting.EndOf(CountedPeriod.BankTransferPaymentWindow, start, businessDays, BusinessCalendar.Product);
                return new PaymentWindow(end, Infrastructure.Jobs.TransferSchedule.ReminderAt(end));
            default:
                throw new ArgumentOutOfRangeException(nameof(method));
        }
    }
}

/// <summary>The end of a payment window and, for a bank transfer, the reminder's moment (empty for a card).</summary>
internal sealed record PaymentWindow(DateTimeOffset EndsAt, DateTimeOffset? ReminderAt);
