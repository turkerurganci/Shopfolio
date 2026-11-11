using Shopfolio.Infrastructure.Errors;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Pages.Components;

/// <summary>
/// The error codes of the cart's refusals (07 6.2.3; D-638): the module's result names (<c>NoStock</c>, <c>OverOrderLimit</c>, <c>AlreadyInCart</c>) are the rule's words, and the code a refusal
/// writes in its rejection line (07 6.1.3; <see cref="RejectionLog"/>) is the one 07 spells. The mapping is written here once; the cart page (UC-20) and the add (UC-05) read it from here.
/// A result that is not a refusal has no code.
/// </summary>
public static class CartRefusalCodes
{
    /// <summary>The code of a line quantity's problem: the stock (<see cref="ErrorCode.StockInsufficient"/>) or the per-order limit (<see cref="ErrorCode.MaxQuantityPerOrderExceeded"/>).</summary>
    public static ErrorCode CodeOf(CartQuantityProblem problem) => problem switch
    {
        CartQuantityProblem.NoStock => ErrorCode.StockInsufficient,
        CartQuantityProblem.OverOrderLimit => ErrorCode.MaxQuantityPerOrderExceeded,
        _ => throw new ArgumentOutOfRangeException(nameof(problem), problem, "An allowed quantity is not refused."),
    };

    /// <summary>The code of a refused quantity change (07 UC-20 422, 404).</summary>
    public static ErrorCode CodeOf(CartItemUpdateOutcome outcome) => outcome switch
    {
        CartItemUpdateOutcome.NotInCart => ErrorCode.NotFound,
        CartItemUpdateOutcome.InvalidQuantity => ErrorCode.InvalidQuantity,
        CartItemUpdateOutcome.NoStock => ErrorCode.StockInsufficient,
        CartItemUpdateOutcome.OverOrderLimit => ErrorCode.MaxQuantityPerOrderExceeded,
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "A written quantity, or a line taken off the showcase that the read deletes, is not refused (K-1179 (3))."),
    };

    /// <summary>The code of a refused add (07 UC-05 422, 404): a variant not on the showcase is the 404 of 07 2.3.3.</summary>
    public static ErrorCode CodeOf(CartAddOutcome outcome) => outcome switch
    {
        CartAddOutcome.InvalidQuantity => ErrorCode.InvalidQuantity,
        CartAddOutcome.VariantNotFound or CartAddOutcome.NotOnSale => ErrorCode.NotFound,
        CartAddOutcome.SoldOut => ErrorCode.VariantUnavailable,
        CartAddOutcome.NoStock => ErrorCode.StockInsufficient,
        CartAddOutcome.OverOrderLimit => ErrorCode.MaxQuantityPerOrderExceeded,
        CartAddOutcome.AlreadyInCart => ErrorCode.DigitalVariantAlreadyInCart,
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "An added variant is not refused."),
    };
}
