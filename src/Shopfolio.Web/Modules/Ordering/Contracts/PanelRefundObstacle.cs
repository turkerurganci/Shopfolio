namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>The named reason of a refused refund step the screen tells apart (T529; 07 UC-131, UC-133, UC-134): the codes are the page's (07 6.2.7).</summary>
public enum PanelRefundObstacle
{
    /// <summary>The transfer line has no IBAN on file for the refund: the IBAN request comes first (K-686; <c>RefundIbanMissing</c>).</summary>
    IbanMissing,

    /// <summary>The amount based refund's amount is zero or more than the paid money not yet given back (K-1078; <c>AmountBasedRefundExceeded</c>).</summary>
    AmountExceeded,

    /// <summary>The IBAN request names no item of the order (07 UC-134; <c>FieldRequired</c>).</summary>
    ItemNotSelected,

    /// <summary>The amount based refund names a closure that is not an open "mal dönmedi" closure of the chosen item (07 UC-133; 04 7.2.13.2; K-1105; <c>InvalidListValue</c>).</summary>
    ClosureNotOpen,
}
