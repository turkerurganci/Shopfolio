namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The one definition of the "payment completed" set (02 10.6.2; 03 1.11.2-1.11.3 "ödeme onaylandıktan sonra"; D-847, D-896): the payment was confirmed at some
/// moment, so Paid and the refunded statuses after it. Every reader of the set calls this type - the sales summary, the order file, the operation facts and the
/// panel's pending work - so the set cannot drift between them.
/// </summary>
public static class PaymentApproval
{
    /// <summary>The set as a fresh array for a database query (<c>PaymentApproval.Statuses.Contains(o.PaymentStatus)</c>); a new array each call, so no caller can change the set.</summary>
    public static PaymentStatus[] Statuses => [.. Enum.GetValues<PaymentStatus>().Where(IsApproved)];

    /// <summary>Whether <paramref name="status"/> is in the set.</summary>
    public static bool IsApproved(PaymentStatus status) => status is PaymentStatus.Paid or PaymentStatus.PartiallyRefunded or PaymentStatus.Refunded;
}
