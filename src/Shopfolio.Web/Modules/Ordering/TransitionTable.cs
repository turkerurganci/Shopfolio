using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>A shipment-axis row of the white list: the transition's identity and the two states it joins (09 9.1 item 2; 02 5.4).</summary>
internal sealed record ShipmentTransition(TransitionCode Code, OrderStatus From, OrderStatus To);

/// <summary>A payment-axis row of the white list: the transition's identity and the two states it joins (09 9.1 item 2; 02 5.5).</summary>
internal sealed record PaymentTransition(TransitionCode Code, PaymentStatus From, PaymentStatus To);

/// <summary>
/// The white list of the order's transitions (09 9.1 item 2; 05 4.1.1, MK-35): one table of sixteen rows - S1...S11 on the shipment axis, O1...O5 on
/// the payment axis - and the one list of who may trigger each (03 1.2, 1.3). A transition that is not in the table is refused: the table is read
/// first, the condition of the transition second (05 4.1.2). The correction is not in the table: it changes the state outside it and is a named
/// operation of the same unit, called only from the panel (<see cref="OrderTransitions"/>; 02 5.4). The table is equal to 02 5.4 and 5.5 (the
/// equality check of 09 21.4 item 1: <c>TransitionTableConsistencyTests</c>).
/// </summary>
internal static class TransitionTable
{
    private static readonly ActorKind[] CustomerFirmSystem = [ActorKind.Buyer, ActorKind.Admin, ActorKind.System];
    private static readonly ActorKind[] FirmAndSystem = [ActorKind.Admin, ActorKind.System];
    private static readonly ActorKind[] FirmOnly = [ActorKind.Admin];
    private static readonly ActorKind[] SystemOnly = [ActorKind.System];

    /// <summary>The shipment axis: the eleven rows S1...S11 (02 5.4).</summary>
    public static IReadOnlyList<ShipmentTransition> Shipment { get; } =
    [
        new(TransitionCode.S1, OrderStatus.Placed, OrderStatus.Preparing),
        new(TransitionCode.S2, OrderStatus.Placed, OrderStatus.Delivered),
        new(TransitionCode.S3, OrderStatus.Placed, OrderStatus.Cancelled),
        new(TransitionCode.S4, OrderStatus.Preparing, OrderStatus.Cancelled),
        new(TransitionCode.S5, OrderStatus.Preparing, OrderStatus.Shipped),
        new(TransitionCode.S6, OrderStatus.Shipped, OrderStatus.Delivered),
        new(TransitionCode.S7, OrderStatus.Shipped, OrderStatus.DeliveryFailed),
        new(TransitionCode.S8, OrderStatus.DeliveryFailed, OrderStatus.Shipped),
        new(TransitionCode.S9, OrderStatus.DeliveryFailed, OrderStatus.Cancelled),
        new(TransitionCode.S10, OrderStatus.Preparing, OrderStatus.Delivered),
        new(TransitionCode.S11, OrderStatus.DeliveryFailed, OrderStatus.Delivered),
    ];

    /// <summary>The payment axis: the five rows O1...O5 (02 5.5).</summary>
    public static IReadOnlyList<PaymentTransition> Payment { get; } =
    [
        new(TransitionCode.O1, PaymentStatus.Pending, PaymentStatus.Paid),
        new(TransitionCode.O2, PaymentStatus.Pending, PaymentStatus.Failed),
        new(TransitionCode.O3, PaymentStatus.Paid, PaymentStatus.PartiallyRefunded),
        new(TransitionCode.O4, PaymentStatus.Paid, PaymentStatus.Refunded),
        new(TransitionCode.O5, PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded),
    ];

    /// <summary>
    /// The kinds of actor each transition may be requested by (03 1.2, 1.3; 11 T315 criterion 4). S2, S10 and S11 have no trigger of their own: they
    /// run with the last event that completes the closure rule, so the actor is that event's (the customer's, the firm's or the system's). S4 by the
    /// system is the closure only; S9 is the firm's - its closure is the firm's reasonless closing after S7. Which of them a given call may use
    /// beyond the kind (the closure-only cases, the reason) is the condition of the transition's own operation, not of this table.
    /// </summary>
    private static readonly IReadOnlyDictionary<TransitionCode, ActorKind[]> Actors = new Dictionary<TransitionCode, ActorKind[]>
    {
        [TransitionCode.S1] = SystemOnly,
        [TransitionCode.S2] = CustomerFirmSystem,
        [TransitionCode.S3] = CustomerFirmSystem,
        [TransitionCode.S4] = CustomerFirmSystem,
        [TransitionCode.S5] = FirmOnly,
        [TransitionCode.S6] = FirmOnly,
        [TransitionCode.S7] = FirmOnly,
        [TransitionCode.S8] = FirmOnly,
        [TransitionCode.S9] = FirmOnly,
        [TransitionCode.S10] = CustomerFirmSystem,
        [TransitionCode.S11] = CustomerFirmSystem,
        [TransitionCode.O1] = FirmAndSystem,
        [TransitionCode.O2] = CustomerFirmSystem,
        [TransitionCode.O3] = FirmAndSystem,
        [TransitionCode.O4] = FirmAndSystem,
        [TransitionCode.O5] = FirmAndSystem,
    };

    /// <summary>The codes of the table, in table order: the shipment axis, then the payment axis.</summary>
    public static IReadOnlyList<TransitionCode> Codes { get; } = [.. Shipment.Select(t => t.Code), .. Payment.Select(t => t.Code)];

    public static ShipmentTransition? FindShipment(TransitionCode code) => Shipment.FirstOrDefault(t => t.Code == code);

    public static PaymentTransition? FindPayment(TransitionCode code) => Payment.FirstOrDefault(t => t.Code == code);

    /// <summary>The shipment row that joins the two states, or null (the closure rule reads the row it needs from here, not from a second list).</summary>
    public static ShipmentTransition? FindShipment(OrderStatus from, OrderStatus to) => Shipment.FirstOrDefault(t => t.From == from && t.To == to);

    /// <summary>The payment row that joins the two states, or null (the refund's sent money reads Ö3, Ö4 or Ö5 from here, not from a second list).</summary>
    public static PaymentTransition? FindPayment(PaymentStatus from, PaymentStatus to) => Payment.FirstOrDefault(t => t.From == from && t.To == to);

    /// <summary>The actor kinds the transition may be requested by; null for a code that is not in the table (the correction included).</summary>
    public static IReadOnlyList<ActorKind>? ActorsOf(TransitionCode code) => Actors.GetValueOrDefault(code);

    /// <summary>True when the code is in the table and the order is in the row's source state on that axis (09 9.1 item 4: the white list is the second step).</summary>
    public static bool Allows(TransitionCode code, Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        if (FindShipment(code) is { } shipment)
        {
            return order.OrderStatus == shipment.From;
        }

        return FindPayment(code) is { } payment && order.PaymentStatus == payment.From;
    }

    /// <summary>
    /// The two axes' combination table (03 1.4.1; 02 5.3.1, 5.6.1), cell by cell: a shipment state's row lists the payment states it may stand
    /// with. The crossed cells are (c) a pending payment on an order past Placed - Preparing is reached only with the payment's approval and the
    /// payment returns to Pending only with the transfer mark's correction, together with the shipment (1.4.2.4); (d) Delivered with Pending or
    /// Failed; (e) a failed payment on an order that is not Cancelled - it is born only with the unpaid cancellation; (g) Cancelled with Pending -
    /// the unpaid cancellation makes the payment Failed. The database's <c>status_pair_allowed</c> rule keeps the same pairs (06 4.17); the
    /// equality with the document is <c>CombinationTableConsistencyTests</c>.
    /// </summary>
    public static IReadOnlyDictionary<OrderStatus, IReadOnlySet<PaymentStatus>> Combinations { get; } = new Dictionary<OrderStatus, IReadOnlySet<PaymentStatus>>
    {
        [OrderStatus.Placed] = new HashSet<PaymentStatus> { PaymentStatus.Pending, PaymentStatus.Paid, PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded },
        [OrderStatus.Preparing] = new HashSet<PaymentStatus> { PaymentStatus.Paid, PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded },
        [OrderStatus.Shipped] = new HashSet<PaymentStatus> { PaymentStatus.Paid, PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded },
        [OrderStatus.DeliveryFailed] = new HashSet<PaymentStatus> { PaymentStatus.Paid, PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded },
        [OrderStatus.Delivered] = new HashSet<PaymentStatus> { PaymentStatus.Paid, PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded },
        [OrderStatus.Cancelled] = new HashSet<PaymentStatus> { PaymentStatus.Paid, PaymentStatus.Failed, PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded },
    };

    /// <summary>
    /// False for a crossed cell of the combination table (<see cref="Combinations"/>; 03 1.4.1). A transition or a correction that would end in one
    /// is refused before anything is written.
    /// </summary>
    public static bool IsPairAllowed(OrderStatus orderStatus, PaymentStatus paymentStatus)
    {
        return Combinations.TryGetValue(orderStatus, out IReadOnlySet<PaymentStatus>? allowed) && allowed.Contains(paymentStatus);
    }

    /// <summary>True for a terminal state of the shipment axis (02 5.4): the closing states are left only by the firm's correction.</summary>
    public static bool IsTerminal(OrderStatus status) => TerminalStatuses.Contains(status);

    /// <summary>The terminal states of the shipment axis (02 5.4); an order in any other state is an open order (02 3.7.7), and a query reads this list, not a copy of it.</summary>
    public static IReadOnlyList<OrderStatus> TerminalStatuses { get; } = [OrderStatus.Delivered, OrderStatus.Cancelled];

    /// <summary>True for a terminal state of the payment axis (02 5.5; 03 1.3): Başarısız and Geri ödendi; no row of the table leaves them.</summary>
    public static bool IsTerminal(PaymentStatus status) => status is PaymentStatus.Failed or PaymentStatus.Refunded;
}
