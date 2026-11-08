using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The outcome of taking one download from an order item's right (05 7.7.4; 02 3.12.5; 09 10.2 item 4): taken; refused because the item's right is used up (the
/// order page's button makes no link, Z-19: no time limit, only a count); or refused because the item cannot be downloaded at all - no such item, not a digital
/// item, or its payment is not confirmed (02 3.12.1). Fixed instances; the refusal is a value.
/// </summary>
public sealed class DownloadTakeResult : IOperationResult
{
    private DownloadTakeResult(string? reason)
    {
        Reason = reason;
    }

    public static DownloadTakeResult Taken { get; } = new(null);

    /// <summary>The counter has reached the frozen allowance: the buyer asks the firm to renew it (02 3.12.6).</summary>
    public static DownloadTakeResult AllowanceUsed { get; } = new("AllowanceUsed");

    public static DownloadTakeResult NotAvailable { get; } = new("NotAvailable");

    /// <summary>Why nothing was taken, for the log; null when a download was taken.</summary>
    public string? Reason { get; }

    public bool IsTaken => Reason is null;

    public bool IsRejected => Reason is not null;
}

/// <summary>
/// The outcome of the firm's renewal of an item's download right (07 UC-143; 02 3.12.6; 06 4.18): the counter is back at zero and the order's version rose by one;
/// or refused - no such item (404), an item that is not a digital item (422), or a version other than the one the screen showed (409).
/// </summary>
public sealed class DownloadRenewalResult : IOperationResult
{
    private DownloadRenewalResult(string? reason)
    {
        Reason = reason;
    }

    public static DownloadRenewalResult Renewed { get; } = new(null);

    public static DownloadRenewalResult NotFound { get; } = new("NotFound");

    public static DownloadRenewalResult NotAvailable { get; } = new("NotAvailable");

    public static DownloadRenewalResult VersionConflict { get; } = new("VersionConflict");

    public string? Reason { get; }

    public bool IsRenewed => Reason is null;

    public bool IsRejected => Reason is not null;
}

/// <summary>
/// What a product page or a cart asks before a variant is added to a cart (02 3.12.4, 3.12.9): the variant's product type as the caller read it from the catalog,
/// the quantity asked for, the cart (null when the visitor has none yet) and the signed-in customer (null for a guest).
/// </summary>
public sealed record DigitalAddRequest(Guid VariantId, ProductType ProductType, int Quantity, Guid? CartId, Guid? CustomerId);

/// <summary>
/// The answer to <see cref="DigitalAddRequest"/>. A digital variant is always one unit (<see cref="IsQuantityAllowed"/>) and is not added a second time to the
/// same cart (<see cref="IsAlreadyInCart"/>, "Bu ürün zaten sepetinde"); a member who bought it before is warned, not stopped (<see cref="ShowsRepurchaseWarning"/>);
/// a guest is never warned, because a guest's past is not known. For any other product type every field is the neutral one.
/// </summary>
public sealed record DigitalAddCheck(bool IsQuantityAllowed, bool IsAlreadyInCart, bool WasBoughtBefore)
{
    /// <summary>The add is refused: a quantity above one or a variant already in the cart.</summary>
    public bool IsBlocked => !IsQuantityAllowed || IsAlreadyInCart;

    /// <summary>The add goes through and the member is told "Bu ürünü daha önce aldınız — sipariş sayfanızdan indirebilirsiniz".</summary>
    public bool ShowsRepurchaseWarning => !IsBlocked && WasBoughtBefore;
}
