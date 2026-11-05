namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>A variant and a unit count of a stock movement; the count is above zero.</summary>
public sealed record VariantQuantity(Guid VariantId, int Quantity);

/// <summary>
/// What one stock movement covers (06 4.14): the variants with their units - stock or service capacity - and at most one coupon right. Each
/// variant appears once.
/// </summary>
public sealed record StockClaim(IReadOnlyList<VariantQuantity> Variants, Guid? CouponId);
