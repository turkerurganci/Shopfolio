using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The minimum order amount, the shipping fee and the free shipping threshold as pure rules (02 3.18.1-3.18.3, 3.19.1-3.19.5; 11 T349 criteria 1-8). The one basis of all three is the sum
/// of the items that enter the order at their discounted prices, before the coupon (02 3.18.2, 3.19.4): the cart passes <c>OrderableSubtotal</c> (sold-out lines are outside it, 02 3.16.7) and
/// the confirmation passes the coupon's <c>BasketTotal</c> (02 3.10.7), so a coupon never lowers it. The fee and both amounts are the firm's section as the caller read it (P-1, P-3, P-4);
/// nothing here reads a clock, a table or a setting.
/// </summary>
public static class ShippingCharge
{
    /// <summary>The decimals of a money amount: the kuruş (02 3.8.4; 06 2.5.1 <c>numeric(12,2)</c>).</summary>
    private const int KurusDecimals = 2;

    /// <summary>
    /// The shipping terms of a basket. <paramref name="basis"/> is the sum of the items that enter the order, discounted, before the coupon; <paramref name="hasPhysicalItem"/> is true when at
    /// least one of those items is physical - a sold-out physical line does not count (02 3.18.3). The fee is charged once when there is a physical item and the basis is under the threshold
    /// (or there is no threshold); the minimum applies only with a physical item; the threshold message shows only when the threshold is set and there is a physical item.
    /// </summary>
    public static ShippingAssessment Assess(ShippingFeeSection terms, decimal basis, bool hasPhysicalItem)
    {
        ArgumentNullException.ThrowIfNull(terms);
        CheckAmount(basis, nameof(basis));
        CheckAmount(terms.ShippingFee, nameof(terms));
        if (terms.FreeShippingThreshold is decimal setThreshold)
        {
            CheckAmount(setThreshold, nameof(terms));
        }

        if (terms.MinimumOrderAmount is decimal setMinimum)
        {
            CheckAmount(setMinimum, nameof(terms));
        }

        if (!hasPhysicalItem)
        {
            // An order of digital items and services carries no shipping fee, no minimum and no threshold message (02 3.18.3, 3.19.1, 3.19.5).
            return new ShippingAssessment(basis, false, 0m, false, null, null);
        }

        decimal? shortfall = terms.MinimumOrderAmount is decimal minimum && basis < minimum ? minimum - basis : null;
        decimal? remaining = terms.FreeShippingThreshold is decimal threshold ? Math.Max(threshold - basis, 0m) : null;
        bool isFreeByThreshold = remaining == 0m;
        decimal fee = isFreeByThreshold ? 0m : terms.ShippingFee;
        return new ShippingAssessment(basis, true, fee, isFreeByThreshold, remaining, shortfall);
    }

    private static void CheckAmount(decimal amount, string name)
    {
        if (amount < 0 || !FieldRuleAttribute.HasAtMostDecimals(amount, KurusDecimals))
        {
            throw new ArgumentOutOfRangeException(name, amount, "An amount of the shipping terms is a kuruş amount of zero or more.");
        }
    }
}

/// <summary>
/// The shipping terms of one basket (02 3.18, 3.19). <see cref="Fee"/> is what the order pays for shipping, once - zero without a physical item, at or above the threshold, or when the firm's
/// fee is zero. <see cref="RemainingToFreeShipping"/> is set exactly when the threshold message shows (threshold set, physical item present): zero means "Kargo ücretsiz".
/// <see cref="MinimumOrderShortfall"/> is set exactly when the minimum applies and the basis is under it; the order cannot be placed then.
/// </summary>
public sealed record ShippingAssessment(decimal Basis, bool HasPhysicalItem, decimal Fee, bool IsFreeByThreshold, decimal? RemainingToFreeShipping, decimal? MinimumOrderShortfall)
{
    /// <summary>The free shipping line of the summary (02 3.19.5; 04 2.6.5, 5.12.6): "Ücretsiz kargoya 80 TL kaldı" or "Kargo ücretsiz"; null when no message shows.</summary>
    public string? FreeShippingMessage => RemainingToFreeShipping switch
    {
        null => null,
        0m => "Kargo ücretsiz",
        decimal left => "Ücretsiz kargoya " + DisplayFormat.Amount(left) + " kaldı",
    };

    /// <summary>The missing amount under the minimum (02 3.18.1; 04 2.6.5): "Sipariş verebilmek için 130 TL'lik daha ürün ekleyin"; null when the order may be placed.</summary>
    public string? MinimumOrderMessage => MinimumOrderShortfall is decimal missing ? "Sipariş verebilmek için " + DisplayFormat.Amount(missing) + "'lik daha ürün ekleyin" : null;

    /// <summary>
    /// Why the order cannot be placed on these terms (02 3.18.1; 07 6.2 <see cref="ErrorCode.MinimumOrderAmountNotMet"/>, 303 to the cart): the cart's payment step and the confirmation
    /// refuse with it; null when the minimum is met or does not apply.
    /// </summary>
    public ErrorCode? Refusal => MinimumOrderShortfall is null ? null : ErrorCode.MinimumOrderAmountNotMet;
}
