using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// Whose cart a call is about (02 3.16.1; 06 4.15): a member's account or a guest browser's cart key. The member's cart is the account's and is the same on every device; the guest's cart
/// is the browser's, found by the key its cookie carries. The key is the raw random value of the cookie (05 6.7.5); the module stores only its digest and nothing prints it.
/// </summary>
public sealed class CartOwner
{
    private CartOwner(Guid? customerId, string? guestCartKey)
    {
        CustomerId = customerId;
        GuestCartKey = guestCartKey;
    }

    /// <summary>The member's account; empty for a guest.</summary>
    public Guid? CustomerId { get; }

    /// <summary>The raw key of the guest browser's cart cookie; empty for a member.</summary>
    public string? GuestCartKey { get; }

    /// <summary>The cart of a signed-in member.</summary>
    public static CartOwner ForMember(Guid customerId)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("A member's cart needs the account's id.", nameof(customerId));
        }

        return new CartOwner(customerId, null);
    }

    /// <summary>The cart of a guest browser.</summary>
    public static CartOwner ForGuest(string guestCartKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(guestCartKey);
        return new CartOwner(null, guestCartKey);
    }

    /// <summary>The key is a secret of the browser: it is never printed.</summary>
    public override string ToString() => CustomerId is null ? "CartOwner(guest)" : "CartOwner(member)";
}

/// <summary>
/// One line of the cart as the customer sees it (02 3.16.5, 3.16.6). The line reads its price and its availability from the current product. <see cref="PriceChanged"/> is true when the
/// price that applies now differs from the one at the first add - in either direction; the old price and the direction are not carried (02 3.16.5). <see cref="EntersOrder"/> is false for a
/// sold-out line, which stays in the cart marked and does not enter the order or the amounts (02 3.16.6, 3.16.7). <see cref="ProductType"/> tells whether the line carries the shipping fee and
/// the minimum order amount (02 3.18.3, 3.19.1). <see cref="QuantityProblem"/> says why a line stays out for its quantity (T347; 02 3.16.8-3.16.10): above the stock ("Bu adette stok yok", no
/// number) or above the product's per-order limit (<see cref="OrderLimit"/>, told as a number). Such a line stays in the cart, its quantity is never lowered by itself, and it does not enter the
/// order, the amounts or the limit's total.
/// </summary>
public sealed record CartLine(
    Guid ItemId,
    Guid VariantId,
    Guid ProductId,
    string ProductName,
    ProductType ProductType,
    int Quantity,
    decimal UnitPrice,
    bool PriceChanged,
    bool IsSoldOut,
    bool EntersOrder,
    CartQuantityProblem QuantityProblem = CartQuantityProblem.None,
    int? OrderLimit = null);

/// <summary>
/// The cart as the cart page shows it (T346). <see cref="RemovedProductNames"/> are the items taken off the showcase since the last view, told once and deleted by the call that gave them;
/// <see cref="MergeAddedProductNames"/> are the products the sign-in merge added to what the customer saw, told once (02 3.16.4). <see cref="OrderableSubtotal"/> is the sum of unit price times
/// quantity of the lines that enter the order - the only basis the coupon's minimum, the shipping fee and the thresholds may read (02 3.16.7). <see cref="Shipping"/> is the shipping fee, the
/// free shipping message and the minimum order shortfall on that basis (T349; 02 3.18, 3.19); a shortfall closes the payment step (<see cref="CartPaymentBlock.MinimumOrderAmountNotMet"/>).
/// </summary>
public sealed record CartView(
    Guid? CartId,
    IReadOnlyList<CartLine> Lines,
    IReadOnlyList<string> RemovedProductNames,
    IReadOnlyList<string> MergeAddedProductNames,
    CartPaymentBlock PaymentBlock,
    decimal OrderableSubtotal,
    ShippingAssessment Shipping)
{
    /// <summary>True when the payment step may open: at least one line enters the order.</summary>
    public bool CanOpenPayment => PaymentBlock == CartPaymentBlock.None;

    /// <summary>The lines that enter the order and the amounts, in the cart's order.</summary>
    public IEnumerable<CartLine> OrderingLines => Lines.Where(l => l.EntersOrder);
}

/// <summary>
/// The result of an add: the outcome, and the cart the line was written to when it was added. <see cref="OrderLimit"/> is the product's per-order limit and is filled exactly when the outcome
/// is <see cref="CartAddOutcome.OverOrderLimit"/> - the limit is told as a number, the stock never (02 3.16.8, 3.16.9).
/// </summary>
public sealed record CartAddResult(CartAddOutcome Outcome, Guid? CartId, int? OrderLimit = null);

/// <summary>
/// The result of the sign-in merge (02 3.16.3, 3.16.4): the member's cart, whether a guest cart was merged, and the number of lines the customer did not see in the guest cart - the ones the
/// next cart view tells once.
/// </summary>
public sealed record CartMergeResult(Guid? MemberCartId, bool WasMerged, int UnseenLines);
