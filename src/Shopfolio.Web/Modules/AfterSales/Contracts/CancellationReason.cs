namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>The closed values of CancellationReason (06 5.3.6). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum CancellationReason
{
    OutOfStock,
    DamagedOrUnsellable,
    CannotShipToAddress,
    UndeliveredBuyerCaused,
    AgreedWithBuyer,
    Other,
}
