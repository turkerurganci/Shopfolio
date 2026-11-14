namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>Why a panel item step was refused before or after the lock (T528; 07 UC-129, UC-130, UC-138, UC-140...UC-145): the page turns each into its 422 message and code.</summary>
public enum PanelItemObstacle
{
    /// <summary>No named reason: the operation is not open (07 6.2.7 <c>OperationNotAvailable</c>).</summary>
    None,

    /// <summary>UC-129: no reason, or "Diğer" without its note, or "müşteriyle anlaşıldı" without the owner's confirmation (04 7.2.9.3).</summary>
    ReasonIncomplete,

    /// <summary>UC-129, UC-130: no item was chosen where the operation needs one.</summary>
    ItemNotSelected,

    /// <summary>A quantity under one (07 6.2.7 <c>InvalidQuantity</c>).</summary>
    QuantityInvalid,

    /// <summary>A quantity above the units open to the operation; the request is not applied at all (07 6.2.7 <c>QuantityExceedsOpen</c>; K-1188 (4)).</summary>
    QuantityExceedsOpen,

    /// <summary>The same item twice in <c>Items</c> - a hand-made request: the item choice is invalid, no row is applied (07 5.1.5).</summary>
    ItemSelectionInvalid,

    /// <summary>A date in the future, or out of the operation's range (before the delivery, before the order).</summary>
    DateInvalid,

    /// <summary>UC-130: the notice's IBAN fails the format or the check digits (OB-14).</summary>
    IbanInvalid,

    /// <summary>UC-130: the notice is outside the window or on an absolute exemption and the separate confirmation box is not ticked (K-709).</summary>
    WarningsUnconfirmed,

    /// <summary>UC-130: the date is the item's delivery day and the order of the two was not answered (K-608).</summary>
    DeliveryOrderMissing,

    /// <summary>UC-130: a physical item not handed to the carrier - the way is the "müşteriyle anlaşıldı" cancellation (K-604).</summary>
    ItemNotShipped,
}
