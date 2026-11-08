namespace Shopfolio.Modules.Catalog.Contracts.Coupons;

/// <summary>
/// What the payment step's coupon field gets back (02 3.10.4, 3.10.5, 6.2.12, 6.2.18; 07 UC-23). Accepted: the code is applied, the shares set item by item. Rejected: unknown,
/// outside its dates, its rights used up, equal to or above the basket total, or rounded to a total of zero - one message that does not tell the reasons apart
/// (CouponRejected; K-845), counted toward L-6. BelowMinimumBasket: a valid code under its minimum amount - its own message with the amount, not counted toward L-6.
/// AttemptLimitExceeded: the cart's invalid attempts passed L-6 - the field closes for a while with a neutral message and the code is not looked at.
/// </summary>
public enum CouponOutcome
{
    Accepted,
    Rejected,
    BelowMinimumBasket,
    AttemptLimitExceeded,
}
