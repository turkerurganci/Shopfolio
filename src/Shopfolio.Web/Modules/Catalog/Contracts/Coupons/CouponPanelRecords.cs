namespace Shopfolio.Modules.Catalog.Contracts.Coupons;

/// <summary>
/// The validity of a coupon as a line of text (03 1.10.2; 04 9.7.3; Z-22): not started - today is before the start day -, valid, expired - the end day has passed -,
/// or its rights used up - the used and reserved rights fill the total. A coupon has no state machine; the value is read from the date range and the rights left,
/// never stored. Not a stored value.
/// </summary>
public enum CouponValidity
{
    NotStarted,
    Valid,
    Expired,
    RightsUsedUp,
}

/// <summary>
/// One coupon as the panel reads it (04 9.7.3, 9.7.4; 06 4.13; 07 UC-90): the definition, the used and reserved rights - read only (K-654) -, the validity the server
/// read for today and the version the form carries (06 2.6.2). <see cref="Value"/> is the percentage or the amount, as <see cref="Kind"/> says.
/// </summary>
public sealed record CouponRecord(
    Guid Id,
    string Code,
    CouponKind Kind,
    decimal Value,
    decimal? MinimumBasketAmount,
    DateOnly StartDate,
    DateOnly EndDate,
    int TotalUsageLimit,
    int UsedCount,
    int ReservedCount,
    CouponValidity Validity,
    int Version)
{
    /// <summary>The used and reserved rights together: the floor of the total usage count (02 3.10.6; K-654).</summary>
    public int UsedAndReserved => UsedCount + ReservedCount;
}

/// <summary>One page of the coupon list, newest first (07 UC-90, 2.6.5): the rows, the page shown - a page past the last shows the last - and the number of pages.</summary>
public sealed record CouponListPage(IReadOnlyList<CouponRecord> Items, int PageNumber, int PageCount);
