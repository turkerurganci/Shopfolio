namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// Why the download was refused (07 4.16.3, the four 422 reasons): the right is used up, the payment is not confirmed (a transfer without the "ödendi" mark), the item has no file, a record
/// closed the item (K-1629; K-1624 (1)).
/// </summary>
public enum DownloadRefusal
{
    AllowanceUsed,
    PaymentNotConfirmed,
    NoFile,
    ItemClosed,
}
