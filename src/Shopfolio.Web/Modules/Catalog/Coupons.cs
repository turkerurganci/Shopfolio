using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Catalog.Contracts.Coupons;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The coupon's reads and its definition (02 3.10; 07 UC-23, UC-122): a new coupon added to the caller's unit of work, the verdict on a code for a basket, and the payment step's
/// application that counts the invalid attempts of the cart (L-6; 02 8.2, P-36). The code is matched in its identity form (<see cref="TextNormalizer.NormalizeIdentity"/>) -
/// the column of the unique index -, so "YAZ10", "yaz10" and "Yaz10" are one code and "I", "İ", "ı", "i" one letter (02 3.10.5; K-1016). The rules are
/// <see cref="CouponRules"/>; the used and reserved rights are written only by <see cref="StockCounters"/> (09 10.2 item 4). No method opens a transaction or saves (09 9.1
/// item 3): the definition joins the caller's unit of work, the reads are plain reads, and the attempt counter keeps its own short transactions (K-972).
/// </summary>
internal sealed class Coupons
{
    private readonly AppDbContext _db;
    private readonly AttemptCounter _attempts;
    private readonly TimeProvider _time;

    public Coupons(AppDbContext db, AttemptCounter attempts, TimeProvider time)
    {
        _db = db;
        _attempts = attempts;
        _time = time;
    }

    /// <summary>
    /// Adds a new coupon to the caller's unit of work (02 3.10.2, 3.10.4, 3.10.5; 07 UC-122): every field problem of <see cref="CouponRules.CheckDefinition"/>, and a code another
    /// coupon holds in the identity form (<c>DuplicateCouponCode</c>; K-806), refuse it and nothing is added. The code is kept as typed. The unique index is the last defense
    /// of two saves that race for one code. The caller's transaction saves it.
    /// </summary>
    public async Task<CouponWriteResult> CreateAsync(CouponDefinition definition, CancellationToken ct)
    {
        List<CouponProblem> problems = [.. CouponRules.CheckDefinition(definition)];
        if (!string.IsNullOrWhiteSpace(definition.Code))
        {
            string normalized = TextNormalizer.NormalizeIdentity(definition.Code);
            if (await _db.Coupons.AsNoTracking().AnyAsync(c => c.NormalizedCode == normalized, ct))
            {
                problems.Add(new CouponProblem(nameof(CouponDefinition.Code), ErrorCode.DuplicateCouponCode));
            }
        }

        if (problems.Count > 0)
        {
            return CouponWriteResult.Refused(problems);
        }

        DateTimeOffset now = _time.GetUtcNow();
        string code = definition.Code!;
        Coupon coupon = definition.Kind == CouponKind.Percentage
            ? Coupon.CreatePercentage(code, definition.Value!.Value, definition.StartDate, definition.EndDate, definition.TotalUsageLimit, now)
            : Coupon.CreateAmount(code, definition.Value!.Value, definition.MinimumBasketAmount!.Value, definition.StartDate, definition.EndDate, definition.TotalUsageLimit, now);
        if (definition.Kind == CouponKind.Percentage)
        {
            coupon.MinimumBasketAmount = definition.MinimumBasketAmount;
        }

        _db.Coupons.Add(coupon);
        return CouponWriteResult.Written(coupon.Id);
    }

    /// <summary>
    /// The verdict on <paramref name="code"/> for the basket now (02 3.10.1-3.10.5): an unknown or blank code is not accepted; a known one is judged by
    /// <see cref="CouponRules.Evaluate"/> on its stored values. A plain read; it counts no attempt and reserves no right - the confirmation reserves the right with a
    /// conditional update, which is what keeps two orders from taking the last one (02 3.10.6; T348).
    /// </summary>
    public async Task<CouponApplication> EvaluateAsync(string? code, IReadOnlyList<CouponLine> lines, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(lines);
        CouponTerms? terms = null;
        if (!string.IsNullOrWhiteSpace(code))
        {
            string normalized = TextNormalizer.NormalizeIdentity(code);
            terms = await _db.Coupons.AsNoTracking()
                .Where(c => c.NormalizedCode == normalized)
                .Select(c => new CouponTerms(
                    c.Id, c.Code, c.Percentage, c.Amount, c.MinimumBasketAmount, c.StartDate, c.EndDate, c.TotalUsageLimit, c.UsedCount, c.ReservedCount))
                .SingleOrDefaultAsync(ct);
        }

        return terms is null ? CouponRules.RejectUnknown(lines) : CouponRules.Evaluate(terms, lines, _time.GetUtcNow());
    }

    /// <summary>
    /// The payment step's application of a code on a cart (07 UC-23; 02 3.10.5, 6.2.18, 8.2 L-6; P-36), in the counter's order for the limits counted on a failure
    /// (<see cref="AttemptCounter"/>): the attempt is recorded on the cart, and when the cart's invalid attempts pass the limit the field closes - the code is not looked
    /// at and the attempt does not count. Otherwise the code is judged (<see cref="EvaluateAsync"/>); a code that is not accepted keeps its attempt, an accepted code and a
    /// valid code under its minimum basket amount give theirs back. The thresholds and the window are the product constants', read by the counter.
    /// </summary>
    public async Task<CouponApplication> ApplyAsync(Guid cartId, string? code, IReadOnlyList<CouponLine> lines, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(lines);
        AttemptSubject cart = AttemptSubject.Cart(cartId);
        Guid attempt = await _attempts.RecordAsync(AttemptLimit.L6, cart, ct);
        if (await _attempts.IsExceededAsync(AttemptLimit.L6, cart, ct))
        {
            await _attempts.ForgetAsync(attempt, ct);
            return CouponApplication.LimitExceeded();
        }

        CouponApplication application = await EvaluateAsync(code, lines, ct);
        if (application.Outcome != CouponOutcome.Rejected)
        {
            await _attempts.ForgetAsync(attempt, ct);
        }

        return application;
    }
}
