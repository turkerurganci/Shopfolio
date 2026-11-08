namespace Shopfolio.Modules.Catalog.Contracts.Coupons;

/// <summary>The two kinds of coupon (02 3.10.2): a percentage of every item, or a fixed amount spread over the items. Not stored; the filled value tells the kind (06 4.13).</summary>
public enum CouponKind
{
    Percentage,
    Amount,
}
