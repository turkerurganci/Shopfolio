namespace Shopfolio.Modules.Catalog.Contracts.Coupons;

/// <summary>
/// The validity of a coupon as a line of text (03 1.10.2; 04 9.7.3; Z-22): not started - today is before the start day -, valid, expired - the end day has passed -,
/// or its rights used up - the used and reserved rights fill the total. A coupon has no state machine; the value is read from the date range and the rights left,
/// never stored. Only the code names live here.
/// </summary>
public enum CouponValidity
{
    NotStarted,
    Valid,
    Expired,
    RightsUsedUp,
}
