using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
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
        List<CouponProblem> problems = await ProblemsOfAsync(definition, null, 0, ct);
        if (problems.Count > 0)
        {
            return CouponWriteResult.Refused(problems);
        }

        DateTimeOffset now = _time.GetUtcNow();
        string code = definition.Code!; // non-null: CheckDefinition refuses an empty code (FieldRequired), refused above
        Coupon coupon = definition.Kind == CouponKind.Percentage
            ? Coupon.CreatePercentage(code, definition.Value!.Value, definition.StartDate, definition.EndDate, definition.TotalUsageLimit, now) // non-null: CheckDefinition refuses a percentage coupon without a value, refused above
            : Coupon.CreateAmount(code, definition.Value!.Value, definition.MinimumBasketAmount!.Value, definition.StartDate, definition.EndDate, definition.TotalUsageLimit, now); // non-null: CheckFixedAmount refuses an amount coupon without a value or a minimum, refused above
        if (definition.Kind == CouponKind.Percentage)
        {
            coupon.MinimumBasketAmount = definition.MinimumBasketAmount;
        }

        _db.Coupons.Add(coupon);
        return CouponWriteResult.Written(coupon.Id);
    }

    /// <summary>
    /// Changes a coupon (07 UC-122; 02 3.10.5, 3.10.6; 04 9.7.6; K-806): every field may change, and the change reaches only new orders - an order's coupon share is frozen
    /// on its items (02 3.10.3). The order of the refusals is 07 2.4.8's: a missing coupon, then the version the form carried - a changed version is refused before the
    /// field rules are looked at -, then every field problem together, the total usage count measured against the used and reserved rights and the code against every
    /// other coupon. The record save raises the version (09 10.2 item 2); the rights are not touched. Moving the end day earlier stops the campaign: a new order no longer
    /// takes the code (<see cref="CouponRules.ValidityOf"/>), and the reserved rights stay on their orders. No trail row (02 10.3.1; K-310). Joins the caller's transaction.
    /// </summary>
    public async Task<CouponWriteResult> UpdateAsync(Guid couponId, CouponDefinition definition, int formVersion, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("A coupon is changed only inside the caller's transaction (09 9.1 item 3).");
        }

        // The coupon row lock (05 7.3.5), the one the counters take: an order that reserves a right in between waits, so the floor below is read after it and holds
        // until the save is committed - the usage count never goes under the used and reserved rights (02 3.10.6).
        LockSequence.Enter(_db, LockKind.Coupon, couponId);
        List<Guid> locked = await _db.Database
            .SqlQuery<Guid>($"SELECT id AS \"Value\" FROM coupons WHERE id = {couponId} FOR UPDATE")
            .ToListAsync(ct);
        if (locked.Count == 0)
        {
            return CouponWriteResult.NotFound;
        }

        Coupon coupon = await _db.Coupons.SingleAsync(c => c.Id == couponId, ct);
        if (coupon.Version != formVersion)
        {
            return CouponWriteResult.VersionConflict(couponId);
        }

        int usedAndReserved = coupon.UsedCount + coupon.ReservedCount;
        List<CouponProblem> problems = await ProblemsOfAsync(definition, couponId, usedAndReserved, ct);
        if (problems.Count > 0)
        {
            return CouponWriteResult.Refused(problems, usedAndReserved);
        }

        RecordSaveResult saved = await RecordSave.SaveAsync(_db, coupon, formVersion, c => Apply(c, definition), ct);
        return saved.IsSaved ? CouponWriteResult.Written(couponId) : CouponWriteResult.VersionConflict(couponId);
    }

    /// <summary>The coupon list, newest first and then by inner key (07 UC-90, 2.6.5; K-1228), at the panel's page size; each row with its validity for now (03 1.10.2). A plain read.</summary>
    public async Task<CouponListPage> ListAsync(int page, CancellationToken ct)
    {
        PagedList<Coupon> paged = await _db.Coupons.AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ThenByDescending(c => c.Id)
            .ToPageAsync(page, PageSizes.Panel, ct);
        DateTimeOffset now = _time.GetUtcNow();
        return new CouponListPage([.. paged.Items.Select(c => RecordOf(c, now))], paged.PageNumber, paged.PageCount);
    }

    /// <summary>One coupon for its form (07 UC-90): null when no coupon has <paramref name="couponId"/>. A plain read.</summary>
    public async Task<CouponRecord?> GetAsync(Guid couponId, CancellationToken ct)
    {
        Coupon? coupon = await _db.Coupons.AsNoTracking().SingleOrDefaultAsync(c => c.Id == couponId, ct);
        return coupon is null ? null : RecordOf(coupon, _time.GetUtcNow());
    }

    private static CouponRecord RecordOf(Coupon coupon, DateTimeOffset now)
    {
        CouponKind kind = coupon.Percentage is not null ? CouponKind.Percentage : CouponKind.Amount;
        decimal value = coupon.Percentage ?? coupon.Amount ?? throw new InvalidOperationException($"Coupon {coupon.Id} has neither a percentage nor an amount (06 4.13).");
        CouponValidity validity = CouponRules.ValidityOf(coupon.StartDate, coupon.EndDate, coupon.TotalUsageLimit, coupon.UsedCount, coupon.ReservedCount, now);
        return new CouponRecord(
            coupon.Id,
            coupon.Code,
            kind,
            value,
            coupon.MinimumBasketAmount,
            coupon.StartDate,
            coupon.EndDate,
            coupon.TotalUsageLimit,
            coupon.UsedCount,
            coupon.ReservedCount,
            validity,
            coupon.Version);
    }

    /// <summary>
    /// Every problem of a definition (<see cref="CouponRules.CheckDefinition(CouponDefinition, int)"/>) and a code another coupon holds in the identity form
    /// (<c>DuplicateCouponCode</c>; K-806) - the coupon being changed (<paramref name="selfId"/>) is not another one.
    /// </summary>
    private async Task<List<CouponProblem>> ProblemsOfAsync(CouponDefinition definition, Guid? selfId, int usedAndReserved, CancellationToken ct)
    {
        List<CouponProblem> problems = [.. CouponRules.CheckDefinition(definition, usedAndReserved)];
        if (!string.IsNullOrWhiteSpace(definition.Code))
        {
            string normalized = TextNormalizer.NormalizeIdentity(definition.Code);
            if (await _db.Coupons.AsNoTracking().AnyAsync(c => c.NormalizedCode == normalized && c.Id != selfId, ct))
            {
                problems.Add(new CouponProblem(nameof(CouponDefinition.Code), ErrorCode.DuplicateCouponCode));
            }
        }

        return problems;
    }

    /// <summary>Writes a checked definition onto the tracked coupon: the code with its identity form, the one value its kind fills, the minimum, the dates and the count.</summary>
    private static void Apply(Coupon coupon, CouponDefinition definition)
    {
        coupon.SetCode(definition.Code!); // non-null: CheckDefinition refuses an empty code, refused before the save
        decimal value = definition.Value!.Value; // non-null: CheckDefinition refuses a coupon without a value, refused before the save
        coupon.Percentage = definition.Kind == CouponKind.Percentage ? value : null;
        coupon.Amount = definition.Kind == CouponKind.Amount ? value : null;
        coupon.MinimumBasketAmount = definition.MinimumBasketAmount;
        coupon.StartDate = definition.StartDate;
        coupon.EndDate = definition.EndDate;
        coupon.TotalUsageLimit = definition.TotalUsageLimit;
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
