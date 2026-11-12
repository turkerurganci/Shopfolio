using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Modules.Catalog.Contracts.Pricing;

namespace Shopfolio.Modules.Catalog.Contracts.Coupons;

/// <summary>
/// A new coupon as the firm defines it (02 3.10.2, 3.10.4, 3.10.5; 04 9.7.4 (2)): the code, the kind and its value - a percentage above zero and under one hundred, or an
/// amount above zero -, the minimum basket amount - required on a fixed amount and larger than it, optional on a percentage -, the date range (both days included,
/// 02 4.1.4; Z-22) and the total usage count (a personal code is a code with a count of 1).
/// </summary>
public sealed record CouponDefinition(
    string? Code,
    CouponKind Kind,
    decimal? Value,
    decimal? MinimumBasketAmount,
    DateOnly StartDate,
    DateOnly EndDate,
    int TotalUsageLimit);

/// <summary>One field of a coupon definition that cannot be saved, with the code of its message (07 6.2): <paramref name="Field"/> is the name of the definition's property.</summary>
public sealed record CouponProblem(string Field, ErrorCode Code);

/// <summary>
/// The result of saving a new coupon: the coupon written, or the fields that block it - every problem the definition has, so the form shows them together. A refusal
/// is a value, not an exception; the transaction runner rolls the work back on it (09 9.1 item 3).
/// </summary>
public sealed class CouponWriteResult : IOperationResult
{
    private CouponWriteResult(Guid? couponId, IReadOnlyList<CouponProblem> problems, bool isNotFound = false, bool isVersionConflict = false)
    {
        CouponId = couponId;
        Problems = problems;
        IsNotFound = isNotFound;
        IsVersionConflict = isVersionConflict;
    }

    /// <summary>The coupon written; empty on a refusal.</summary>
    public Guid? CouponId { get; }

    /// <summary>The fields that block the save; empty when the coupon was written, and on a missing coupon or a version conflict.</summary>
    public IReadOnlyList<CouponProblem> Problems { get; }

    /// <summary>The coupon to change does not exist (07 UC-122: 404).</summary>
    public bool IsNotFound { get; }

    /// <summary>The coupon's version is no longer the one the form carried (07 UC-122: 409, decided before the field rules - 07 2.4.8).</summary>
    public bool IsVersionConflict { get; }

    public bool IsRejected => Problems.Count > 0 || IsNotFound || IsVersionConflict;

    public static CouponWriteResult NotFound { get; } = new(null, [], isNotFound: true);

    public static CouponWriteResult VersionConflict(Guid couponId) => new(couponId, [], isVersionConflict: true);

    public static CouponWriteResult Written(Guid couponId) => new(couponId, []);

    public static CouponWriteResult Refused(IReadOnlyList<CouponProblem> problems)
    {
        ArgumentNullException.ThrowIfNull(problems);
        if (problems.Count == 0)
        {
            throw new ArgumentException("A refusal names at least one problem.", nameof(problems));
        }

        return new CouponWriteResult(null, problems);
    }
}

/// <summary>
/// One item of the basket as the coupon reads it (02 3.10.1, 3.8.3): <paramref name="UnitPrice"/> is the VAT-included unit price that applies - the discounted one already
/// rounded to the kuruş when a discount runs (the coupon comes down from the discounted price) -, <paramref name="Quantity"/> the units and <paramref name="VatRate"/> the
/// product's rate in percent. Only the items that go into the order are listed (K-130).
/// </summary>
public sealed record CouponLine(decimal UnitPrice, int Quantity, decimal VatRate);

/// <summary>
/// The verdict on a coupon code for a basket (02 3.10.1-3.10.7). On <see cref="CouponOutcome.Accepted"/> it carries the coupon, its code as defined and every item's amounts
/// in the fixed order of 02 3.8.3 - the coupon share set for the item, the VAT separated after it -, in the order of the lines; the confirmation freezes them to the order
/// items (02 3.10.3, 5.11.4). <see cref="BasketTotal"/> is the basket without the coupon: the base the free shipping threshold and the minimum order amount are compared with,
/// which the coupon never lowers (02 3.10.7).
/// </summary>
public sealed class CouponApplication : IOperationResult
{
    private CouponApplication(CouponOutcome outcome, Guid? couponId, string? code, decimal basketTotal, decimal? minimumBasketAmount, IReadOnlyList<ItemAmounts> items)
    {
        Outcome = outcome;
        CouponId = couponId;
        Code = code;
        BasketTotal = basketTotal;
        MinimumBasketAmount = minimumBasketAmount;
        Items = items;
    }

    public CouponOutcome Outcome { get; }

    /// <summary>The coupon applied; empty unless accepted.</summary>
    public Guid? CouponId { get; }

    /// <summary>The coupon's code as the firm wrote it, which the summary shows and the order keeps (06 4.17 <c>CouponCode</c>); empty unless accepted.</summary>
    public string? Code { get; }

    /// <summary>The sum of the items' amounts before the coupon (unit price times quantity): the base of the minimum basket amount, the free shipping threshold and the minimum order amount (02 3.10.4, 3.10.7).</summary>
    public decimal BasketTotal { get; }

    /// <summary>The minimum basket amount the message names on <see cref="CouponOutcome.BelowMinimumBasket"/>; empty otherwise.</summary>
    public decimal? MinimumBasketAmount { get; }

    /// <summary>The items' amounts with their coupon shares, in the order of the lines; empty unless accepted.</summary>
    public IReadOnlyList<ItemAmounts> Items { get; }

    /// <summary>The coupon's whole reduction: the sum of the items' shares.</summary>
    public decimal Discount => Items.Sum(i => i.CouponShare);

    /// <summary>The items' payable amounts added up; above zero when accepted (02 3.10.4). The shipping fee is not in it (<c>SumOrderTotal</c> adds it).</summary>
    public decimal PayableTotal => Items.Sum(i => i.Payable);

    public bool IsRejected => Outcome != CouponOutcome.Accepted;

    internal static CouponApplication Accepted(Guid couponId, string code, decimal basketTotal, IReadOnlyList<ItemAmounts> items)
        => new(CouponOutcome.Accepted, couponId, code, basketTotal, null, items);

    internal static CouponApplication Rejected(decimal basketTotal) => new(CouponOutcome.Rejected, null, null, basketTotal, null, []);

    internal static CouponApplication BelowMinimum(decimal basketTotal, decimal minimumBasketAmount)
        => new(CouponOutcome.BelowMinimumBasket, null, null, basketTotal, minimumBasketAmount, []);

    internal static CouponApplication LimitExceeded() => new(CouponOutcome.AttemptLimitExceeded, null, null, 0m, null, []);
}

/// <summary>
/// One item of an order as the return of the coupon right reads it (02 5.11.3, 7.2.6, 7.4.8): its units and how many of them a cancellation, a removal, a late delivery
/// termination, a withdrawal or a return has closed - a unit closed by late delivery termination counts as cancelled.
/// </summary>
public sealed record ItemClosure(int Quantity, int ClosedQuantity);
