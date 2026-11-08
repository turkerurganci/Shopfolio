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
/// sold-out line, which stays in the cart marked and does not enter the order or the amounts (02 3.16.6, 3.16.7).
/// </summary>
public sealed record CartLine(Guid ItemId, Guid VariantId, Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, bool PriceChanged, bool IsSoldOut, bool EntersOrder);

/// <summary>
/// The cart as the cart page shows it (T346). <see cref="RemovedProductNames"/> are the items taken off the showcase since the last view, told once and deleted by the call that gave them;
/// <see cref="MergeAddedProductNames"/> are the products the sign-in merge added to what the customer saw, told once (02 3.16.4). <see cref="OrderableSubtotal"/> is the sum of unit price times
/// quantity of the lines that enter the order - the only basis the coupon's minimum, the shipping fee and the thresholds may read (02 3.16.7).
/// </summary>
public sealed record CartView(
    Guid? CartId,
    IReadOnlyList<CartLine> Lines,
    IReadOnlyList<string> RemovedProductNames,
    IReadOnlyList<string> MergeAddedProductNames,
    CartPaymentBlock PaymentBlock,
    decimal OrderableSubtotal)
{
    /// <summary>True when the payment step may open: at least one line enters the order.</summary>
    public bool CanOpenPayment => PaymentBlock == CartPaymentBlock.None;

    /// <summary>The lines that enter the order and the amounts, in the cart's order.</summary>
    public IEnumerable<CartLine> OrderingLines => Lines.Where(l => l.EntersOrder);
}

/// <summary>The result of an add: the outcome, and the cart the line was written to when it was added.</summary>
public sealed record CartAddResult(CartAddOutcome Outcome, Guid? CartId);

/// <summary>
/// The result of the sign-in merge (02 3.16.3, 3.16.4): the member's cart, whether a guest cart was merged, and the number of lines the customer did not see in the guest cart - the ones the
/// next cart view tells once.
/// </summary>
public sealed record CartMergeResult(Guid? MemberCartId, bool WasMerged, int UnseenLines);
