namespace Shopfolio.Modules.AfterSales;

/// <summary>The records that tell an item's path (03 1.7.1): the item carries no status of its own (K-179), these records are its path.</summary>
internal enum ItemRecordKind
{
    DeliveryMark,
    Cancellation,
    ItemRemoval,
    LateDeliveryTermination,
    Withdrawal,
    ReturnReceipt,
    ReturnRejection,
    NotReturnedClosure,
    RefundIbanRequest,
    DefectClaim,
    DownloadCounter,
}

/// <summary>
/// How a record counts on the shipment line (03 1.7.1 "Hatta sayılışı"; the closure rule of 03 1.4.3): the units it covers are delivered, closed, counted as
/// cancelled, closed or not by the delivery mark (a withdrawal closes the units of an item without the mark and starts the return on a delivered one), or untouched.
/// </summary>
internal enum LineEffect
{
    Delivered,
    Closed,
    CountedAsCancelled,
    DependsOnDeliveryMark,
    None,
}

/// <summary>
/// One row of 03 1.7.1: the row's number, the record, its effect on the line, whether it carries the units it covers and whether its own row carries the actor and the
/// moment (03 1.7.4: every record but the delivery mark and the download counter carries both, and the quantity when it covers units).
/// </summary>
internal sealed record ItemRecordRow(string Number, ItemRecordKind Kind, LineEffect LineEffect, bool CarriesQuantity, bool CarriesActorAndMoment);

/// <summary>
/// The table of the item's records (03 1.7.1, 1.7.4; 02 5.8; 05 4.1.2): the one place that says how each record counts on the line. The closed quantity of an item
/// (<see cref="AfterSalesModule.ClosedQuantitiesAsync"/>) sums exactly the records whose units leave the open quantity here; the tests read the expected rows from
/// 03 1.7.1 itself.
/// </summary>
internal static class ItemRecordTable
{
    public static readonly IReadOnlyList<ItemRecordRow> Rows =
    [
        new("1.7.1.1", ItemRecordKind.DeliveryMark, LineEffect.Delivered, CarriesQuantity: false, CarriesActorAndMoment: false),
        new("1.7.1.2", ItemRecordKind.Cancellation, LineEffect.Closed, CarriesQuantity: true, CarriesActorAndMoment: true),
        new("1.7.1.3", ItemRecordKind.ItemRemoval, LineEffect.CountedAsCancelled, CarriesQuantity: true, CarriesActorAndMoment: true),
        new("1.7.1.4", ItemRecordKind.LateDeliveryTermination, LineEffect.Closed, CarriesQuantity: true, CarriesActorAndMoment: true),
        new("1.7.1.5", ItemRecordKind.Withdrawal, LineEffect.DependsOnDeliveryMark, CarriesQuantity: true, CarriesActorAndMoment: true),
        new("1.7.1.6", ItemRecordKind.ReturnReceipt, LineEffect.None, CarriesQuantity: true, CarriesActorAndMoment: true),
        new("1.7.1.7", ItemRecordKind.ReturnRejection, LineEffect.None, CarriesQuantity: true, CarriesActorAndMoment: true),
        new("1.7.1.8", ItemRecordKind.NotReturnedClosure, LineEffect.None, CarriesQuantity: true, CarriesActorAndMoment: true),
        new("1.7.1.9", ItemRecordKind.RefundIbanRequest, LineEffect.None, CarriesQuantity: false, CarriesActorAndMoment: true),
        new("1.7.1.10", ItemRecordKind.DefectClaim, LineEffect.None, CarriesQuantity: true, CarriesActorAndMoment: true),
        new("1.7.1.11", ItemRecordKind.DownloadCounter, LineEffect.None, CarriesQuantity: false, CarriesActorAndMoment: false),
    ];

    public static ItemRecordRow Of(ItemRecordKind kind) => Rows.Single(r => r.Kind == kind);

    /// <summary>
    /// Whether the record's units leave the item's open quantity (03 1.7.4): the closed, the counted-as-cancelled and the withdrawn units - a withdrawal closes its
    /// units whether or not the item carries the delivery mark; on a delivered item the mark decides only that the return starts.
    /// </summary>
    public static bool ClosesOpenQuantity(ItemRecordKind kind) =>
        Of(kind).LineEffect is LineEffect.Closed or LineEffect.CountedAsCancelled or LineEffect.DependsOnDeliveryMark;
}

/// <summary>
/// The unit rules of an item (03 1.7.4; K-787, K-790): the unit of an operation is the unit; the selectable quantity is the quantity open to the operation and a value
/// above it is refused. The item's open quantity is its quantity minus the units its cancellation, removal, termination and withdrawal records closed.
/// </summary>
internal static class ItemQuantities
{
    /// <summary>The item's open quantity: its own quantity minus the closed units (never below zero).</summary>
    public static int Open(int itemQuantity, int closed) => Math.Max(0, itemQuantity - closed);

    /// <summary>A selected quantity is at least one and at most the quantity open to the operation.</summary>
    public static bool IsSelectable(int selected, int open) => selected >= 1 && selected <= open;

    /// <summary>
    /// The units of one withdrawal declaration still on their way back (03 1.7.3.5; K-794): the declared units minus those received, rejected or closed as not come
    /// back (a reopened closure no longer counts).
    /// </summary>
    public static int AwaitingReceipt(int declared, int received, int rejected, int notReturned) => Math.Max(0, declared - received - rejected - notReturned);
}
