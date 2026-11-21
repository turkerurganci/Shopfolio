using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// Z-8's last payment day (02 4.2; 04 9.8, 9.9.5; D-1436, D-1450): a payment window ends at the 00:00 after its last day (<see cref="TurkeyTime.EndOfPeriod"/>), so the day is the one that holds the
/// instant just before the end - the day the customer's notices (B-2, B-3), the panel and the customer's screens name. The one home of the rule: the module's <c>PaymentWindowRules.LastPaymentDay</c>
/// and the contracts the screens read call it.
/// </summary>
public static class PaymentWindowDay
{
    public static DateOnly LastOf(DateTimeOffset paymentWindowEndsAt) => TurkeyTime.DateOf(paymentWindowEndsAt.AddTicks(-1));
}
