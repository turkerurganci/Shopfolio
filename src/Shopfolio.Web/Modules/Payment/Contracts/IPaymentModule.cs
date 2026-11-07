namespace Shopfolio.Modules.Payment.Contracts;

/// <summary>
/// The open surface of the Payment module (09 6 item 1; 05 3.4.1): what other modules, the pages and the commands may call. The module owns the payment records and the refund records (05 3.3.4).
/// The members arrive with the tasks that own the module's rules, as methods that take and give immutable records and join the caller's
/// database transaction (09 6 item 3).
/// </summary>
public interface IPaymentModule
{
    /// <summary>
    /// The payment records of the given orders, as the values a caller needs to tell whether an order's payment was taken (06 4.32; 06 9.1 Z-28, Z-41, Z-43): the
    /// order and the transition (Ö1) that confirmed the payment, or null on a payment that came late to a cancelled order. A plain read; the caller reads the
    /// "corrected" mark from the transition records it owns - the payment record itself carries no such mark.
    /// </summary>
    Task<IReadOnlyList<PaymentSource>> FindPaymentSourcesAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken ct);

    /// <summary>
    /// The order's disposal (06 9.2.1; K-1059): empties the administrator link and name of every refund of the given orders and returns the ids of all their
    /// refunds. It writes through the caller's change tracker - the caller's transaction and order row lock hold, and the caller raises the order's version and
    /// saves. The payment records, the refund records and the inbound notification records are not touched (06 9.2.2).
    /// </summary>
    Task<IReadOnlyList<Guid>> ErasePersonalDataAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken ct);
}

/// <summary>One payment record seen from another module (06 4.32): the order, and the transition that confirmed the payment - null on a late card payment.</summary>
public sealed record PaymentSource(Guid OrderId, Guid? TransitionRecordId);
