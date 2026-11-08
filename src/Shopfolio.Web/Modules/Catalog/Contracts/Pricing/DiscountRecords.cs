using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Catalog.Contracts.Pricing;

/// <summary>
/// The discount declaration where it is shown - the product page's price block and the storefront card (02 3.9.2, 12.1.6; 04 2.11.2, 5.4.4): the discounted price
/// rounded to the kuruş, the reference price the system computed (02 3.9.3), the rate, and the discount's start and end dates, which are shown wherever the
/// declaration is (Ticari Reklam ve Haksız Ticari Uygulamalar Yönetmeliği m.14/1; K-545). Present only while the discount runs.
/// </summary>
public sealed record DiscountLine(decimal DiscountedPrice, decimal ReferencePrice, decimal Percentage, DateOnly StartDate, DateOnly EndDate);

/// <summary>
/// A product's discount as the product form shows it (04 9.5.8): the rate, the two dates, the state, and for each variant its own price, the reference price the
/// system computed and the discounted price - read only; the reference is never entered (02 3.9.3).
/// </summary>
public sealed record DiscountView(decimal Percentage, DateOnly StartDate, DateOnly EndDate, DiscountState State, IReadOnlyList<DiscountVariantLine> Variants);

/// <summary>One variant's line of the discount section (04 9.5.8): its price, its reference price and its discounted price.</summary>
public sealed record DiscountVariantLine(Guid VariantId, decimal Price, decimal ReferencePrice, decimal DiscountedPrice);

/// <summary>
/// The result of saving a product's discount (02 3.9.1, 3.9.5; 02 6.6.1, 6.6.13; 04 9.5.17): saved, or a named refusal that writes nothing. A refusal about a
/// variant names the variant, its rounded discounted price and its reference price, so the form can say the reason in place (K-799).
/// </summary>
public sealed class DiscountSaveResult : IOperationResult
{
    private DiscountSaveResult(string? reason, Guid? variantId = null, decimal? discountedPrice = null, decimal? referencePrice = null)
    {
        Reason = reason;
        VariantId = variantId;
        DiscountedPrice = discountedPrice;
        ReferencePrice = referencePrice;
    }

    public static DiscountSaveResult Saved { get; } = new(null);

    public static DiscountSaveResult ProductNotFound { get; } = new("ProductNotFound");

    /// <summary>The rate is zero or below, one hundred or above, or has more than two decimals (04 7.3.2.9; 02 3.9.1: %100 cannot be entered).</summary>
    public static DiscountSaveResult InvalidPercentage { get; } = new("InvalidPercentage");

    /// <summary>The end date is before the start date (04 7.3.2.12).</summary>
    public static DiscountSaveResult EndBeforeStart { get; } = new("EndBeforeStart");

    /// <summary>Why the discount was refused; null when it was saved.</summary>
    public string? Reason { get; }

    /// <summary>The variant the refusal is about; null when it is not about a variant.</summary>
    public Guid? VariantId { get; }

    /// <summary>The variant's discounted price, rounded to the kuruş.</summary>
    public decimal? DiscountedPrice { get; }

    /// <summary>The variant's reference price (02 3.9.3).</summary>
    public decimal? ReferencePrice { get; }

    public bool IsRejected => Reason is not null;

    /// <summary>The variant's discounted price, rounded to the kuruş, would be 0,00 TL (02 3.9.1; K-600).</summary>
    public static DiscountSaveResult DiscountedPriceZero(Guid variantId, decimal referencePrice) => new("DiscountedPriceZero", variantId, 0m, referencePrice);

    /// <summary>The variant's discounted price is not below its reference price (02 3.9.5; K-68).</summary>
    public static DiscountSaveResult NotBelowReference(Guid variantId, decimal discountedPrice, decimal referencePrice)
        => new("NotBelowReference", variantId, discountedPrice, referencePrice);
}

/// <summary>The result of removing a product's discount (06 4.12): removed, or there was none.</summary>
public sealed class DiscountRemoveResult : IOperationResult
{
    private DiscountRemoveResult(string? reason)
    {
        Reason = reason;
    }

    public static DiscountRemoveResult Removed { get; } = new(null);

    public static DiscountRemoveResult NotFound { get; } = new("NotFound");

    /// <summary>Why nothing was removed; null when the discount was removed.</summary>
    public string? Reason { get; }

    public bool IsRejected => Reason is not null;
}

/// <summary>The result of changing a variant's price (02 3.8.4, 3.9.1, 3.9.3): changed, unchanged (the same price), or a named refusal that writes nothing.</summary>
public sealed class VariantPriceResult : IOperationResult
{
    private VariantPriceResult(string? reason)
    {
        Reason = reason;
    }

    public static VariantPriceResult Changed { get; } = new(null);

    /// <summary>The price is the one the variant already has: nothing is written, not even a price history row (06 4.11).</summary>
    public static VariantPriceResult Unchanged { get; } = new(null);

    public static VariantPriceResult VariantNotFound { get; } = new("VariantNotFound");

    /// <summary>The price is not above zero, has more than two decimals or exceeds the money type (02 3.8.4; K-62, K-600).</summary>
    public static VariantPriceResult InvalidPrice { get; } = new("InvalidPrice");

    /// <summary>The product's discount has not ended and would bring this price down to 0,00 TL (02 3.9.1; 02 6.6.13; K-600).</summary>
    public static VariantPriceResult DiscountedPriceZero { get; } = new("DiscountedPriceZero");

    /// <summary>Why the price was refused; null when it was changed or unchanged.</summary>
    public string? Reason { get; }

    public bool IsRejected => Reason is not null;
}
