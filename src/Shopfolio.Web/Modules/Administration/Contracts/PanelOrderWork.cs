namespace Shopfolio.Modules.Administration.Contracts;

/// <summary>
/// The pending work and mark filter of the panel's order list (T527; 07 5.3.2 <c>OrderQuery.pendingWork</c>; 04 9.8.4): one value at a time. The first five filter the scope of the home's counters of
/// the same name (02 10.6.1) - the counter and the list share one rule -; the last two are the order's marks. The names are the parameter's values (07 5.3.2).
/// </summary>
public enum PanelOrderWork
{
    AwaitingPayment,
    AwaitingShipment,
    AwaitingDeliveryMark,
    AwaitingServiceCompletion,
    AwaitingReturnOrRefund,
    IbanPendingMark,
    EmailUndeliveredMark,
}
