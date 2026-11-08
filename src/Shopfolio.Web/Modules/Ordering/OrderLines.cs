using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>The seven lines of 03 1.5 (02 5.7): the types have no state set of their own; a line is the order's path through the two axes.</summary>
internal enum OrderLine
{
    /// <summary>1.5.1: physical, paid by card.</summary>
    PhysicalCard,

    /// <summary>1.5.2: physical, paid by bank transfer.</summary>
    PhysicalTransfer,

    /// <summary>1.5.3: digital only.</summary>
    DigitalOnly,

    /// <summary>1.5.4: service only.</summary>
    ServiceOnly,

    /// <summary>1.5.5: digital and service.</summary>
    DigitalAndService,

    /// <summary>1.5.6: mixed, with a physical item.</summary>
    MixedWithPhysical,

    /// <summary>1.5.7: mixed whose physical items are closed - it ends where it stands by the closure rule.</summary>
    MixedPhysicalClosed,
}

/// <summary>The administrator's mandatory manual steps of an order's line (03 8.5; 02 10.5.1): the rows of 8.2 they are.</summary>
internal enum ManualStep
{
    /// <summary>8.2.2: the transfer's "paid" mark - once per order, whatever its types.</summary>
    TransferPaidMark,

    /// <summary>8.2.5: shipping.</summary>
    Ship,

    /// <summary>8.2.6: the delivery mark and date.</summary>
    MarkDelivered,

    /// <summary>8.2.9: the service item's "completed" mark.</summary>
    CompleteService,
}

/// <summary>A row of 03 1.5: the line, its ordinary sequence - the transitions in order, with who takes each - and the open types and method that make it.</summary>
internal sealed record OrderLineRow(string Number, OrderLine Line, IReadOnlyList<ProductType> Types, PaymentMethod? Method, IReadOnlyList<LineStep> Sequence);

/// <summary>One step of a line's ordinary sequence: the transitions it writes and who takes it (the system or the administrator).</summary>
internal sealed record LineStep(IReadOnlyList<Contracts.TransitionCode> Codes, Contracts.ActorKind Actor);

/// <summary>
/// The lines by type and the manual step budget (03 1.5, 8.5; 02 5.7, 10.5): the one place that says which line an order follows and how many
/// mandatory manual steps the administrator takes on it. A mixed order adds the lines' steps and the transfer's "paid" mark counts once per order
/// (8.5.5); an order whose physical items are closed counts the remaining lines' steps (1.5.7). The conditional steps and the work outside the budget
/// (8.5.6-8.5.8) are not counted. The equality with the document's two tables is <c>OrderLinesConsistencyTests</c>.
/// </summary>
internal static class OrderLines
{
    private static readonly Contracts.ActorKind System = Contracts.ActorKind.System;
    private static readonly Contracts.ActorKind Admin = Contracts.ActorKind.Admin;

    /// <summary>The ordinary sequences of 03 1.5.1-1.5.5 on the card; the transfer's "paid" mark makes Ö1 the administrator's (1.5.2, 1.5.3-1.5.5).</summary>
    public static IReadOnlyList<OrderLineRow> Rows { get; } =
    [
        new("1.5.1", OrderLine.PhysicalCard, [ProductType.Physical], PaymentMethod.Card,
            [new([Contracts.TransitionCode.O1, Contracts.TransitionCode.S1], System), new([Contracts.TransitionCode.S5], Admin), new([Contracts.TransitionCode.S6], Admin)]),
        new("1.5.2", OrderLine.PhysicalTransfer, [ProductType.Physical], PaymentMethod.BankTransfer,
            [new([Contracts.TransitionCode.O1, Contracts.TransitionCode.S1], Admin), new([Contracts.TransitionCode.S5], Admin), new([Contracts.TransitionCode.S6], Admin)]),
        new("1.5.3", OrderLine.DigitalOnly, [ProductType.Digital], null,
            [new([Contracts.TransitionCode.O1, Contracts.TransitionCode.S2], System)]),
        new("1.5.4", OrderLine.ServiceOnly, [ProductType.Service], null,
            [new([Contracts.TransitionCode.O1], System), new([Contracts.TransitionCode.S2], Admin)]),
        new("1.5.5", OrderLine.DigitalAndService, [ProductType.Digital, ProductType.Service], null,
            [new([Contracts.TransitionCode.O1], System), new([Contracts.TransitionCode.S2], Admin)]),
        new("1.5.6", OrderLine.MixedWithPhysical, [ProductType.Physical, ProductType.Digital, ProductType.Service], null,
            [new([Contracts.TransitionCode.O1, Contracts.TransitionCode.S1], System), new([Contracts.TransitionCode.S5], Admin), new([Contracts.TransitionCode.S6], Admin)]),
        new("1.5.7", OrderLine.MixedPhysicalClosed, [ProductType.Digital, ProductType.Service], null, []),
    ];

    /// <summary>
    /// The line of an order: <paramref name="types"/> are the types of its items, <paramref name="physicalClosed"/> tells that a physical item it carried
    /// has no open quantity left. A single physical type splits by the method (1.5.1, 1.5.2); the other single types and digital with service do not.
    /// </summary>
    public static OrderLine LineOf(IReadOnlyCollection<ProductType> types, PaymentMethod method, bool physicalClosed)
    {
        ArgumentNullException.ThrowIfNull(types);
        HashSet<ProductType> set = [.. types];
        if (set.Count == 0)
        {
            throw new ArgumentException("An order carries at least one item.", nameof(types));
        }

        if (physicalClosed)
        {
            return OrderLine.MixedPhysicalClosed;
        }

        if (set.Contains(ProductType.Physical))
        {
            return set.Count > 1 ? OrderLine.MixedWithPhysical : method == PaymentMethod.Card ? OrderLine.PhysicalCard : OrderLine.PhysicalTransfer;
        }

        return set.SetEquals([ProductType.Digital]) ? OrderLine.DigitalOnly
            : set.SetEquals([ProductType.Service]) ? OrderLine.ServiceOnly
            : OrderLine.DigitalAndService;
    }

    /// <summary>
    /// The mandatory manual steps of an order whose open items are of <paramref name="openTypes"/> (8.5.1-8.5.5): shipping and the delivery mark for the
    /// physical items, the "completed" mark for the service items - the budget counts the service line's mark once (8.5.3) - nothing for the digital items, and the "paid" mark once when the order is paid by transfer. For an order whose physical items are closed (1.5.7) the remaining
    /// open types are given.
    /// </summary>
    public static IReadOnlyList<ManualStep> StepsOf(IReadOnlyCollection<ProductType> openTypes, PaymentMethod method)
    {
        ArgumentNullException.ThrowIfNull(openTypes);
        List<ManualStep> steps = [];
        if (method == PaymentMethod.BankTransfer)
        {
            steps.Add(ManualStep.TransferPaidMark);
        }

        if (openTypes.Contains(ProductType.Physical))
        {
            steps.Add(ManualStep.Ship);
            steps.Add(ManualStep.MarkDelivered);
        }

        if (openTypes.Contains(ProductType.Service))
        {
            steps.Add(ManualStep.CompleteService);
        }

        return steps;
    }
}
