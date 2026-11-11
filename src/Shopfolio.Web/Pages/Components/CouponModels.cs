using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts.Coupons;

namespace Shopfolio.Pages.Components;

/// <summary>The four states of the coupon field (04 2.12.6, 6.2.13.9): a closed row, the open field, the applied code, and a field the attempt limit (L-6) has closed.</summary>
public enum CouponFieldMode
{
    /// <summary>04 2.12.6.1: the row the step opens with - "Kupon kodum var".</summary>
    Closed,

    /// <summary>04 2.12.6.2: the code is typed; a refusal shows under the field.</summary>
    Open,

    /// <summary>04 2.12.6.2: the applied code with its name and the amount it takes off, "Değiştir" and "Kaldır".</summary>
    Applied,

    /// <summary>04 2.12.6.3: L-6 passed; the field is closed and the neutral message stays in its place (04 2.3.5).</summary>
    LimitClosed,
}

/// <summary>
/// The coupon field of the payment step (T544; 04 OB-17, 2.12.6): its hâl, the field and its message when open, the applied code and the deduction it makes when applied. The order has one code at
/// most (04 2.12.6.2): the applied hâl holds one code and a new code replaces it. The view is built by <see cref="CouponFields"/> only.
/// </summary>
public sealed class CouponFieldView
{
    internal CouponFieldView(CouponFieldMode mode)
    {
        Mode = mode;
    }

    public CouponFieldMode Mode { get; }

    /// <summary>The code field of the open hâl, with its refusal when there is one; null in the other hâls.</summary>
    public FieldView? CodeField { get; internal init; }

    /// <summary>The applied code as the coupon defines it (the firm's spelling); null unless applied.</summary>
    public string? AppliedCode { get; internal init; }

    /// <summary>The amount the code takes off, written as a deduction ("-50 TL", 04 8.2.4); null unless applied.</summary>
    public string? DiscountText { get; internal init; }

    /// <summary>The neutral message of a closed field (04 2.3.5); null otherwise.</summary>
    public MessageView? LimitMessage { get; internal init; }

    /// <summary>True when the row is the closed one: it offers only the opening link.</summary>
    public bool IsClosedRow => Mode == CouponFieldMode.Closed;

    /// <summary>True when the applied hâl offers its two buttons.</summary>
    public bool OffersChangeAndRemove => Mode == CouponFieldMode.Applied;

    /// <summary>True when the field takes a code: only the open hâl does (04 2.12.6.3: the limit closes the field).</summary>
    public bool TakesACode => Mode != CouponFieldMode.LimitClosed;
}

/// <summary>The coupon field's words, its rules and its only constructors (T544; 04 OB-17, 2.12.6; 02 3.10).</summary>
public static class CouponFields
{
    /// <summary>The closed row's text (04 2.12.6.1; K-757).</summary>
    public const string ClosedRowText = "Kupon kodum var";

    public const string CodeLabel = "Kupon kodu";

    /// <summary>The page handler that applies a code (07 UC-23); each action of the field is a form of its own that names its handler (07 2.2.1).</summary>
    public const string ApplyHandler = "ApplyCoupon";

    /// <summary>The page handler that removes the applied code (07 UC-24).</summary>
    public const string RemoveHandler = "RemoveCoupon";

    public const string ApplyLabel = "Uygula";

    public const string ChangeLabel = "Değiştir";

    public const string RemoveLabel = "Kaldır";

    /// <summary>The one refusal of every reason that is not the minimum amount - no code, outside its dates, rights used up, the total taken to zero; the message does not tell them apart (04 5.13.15; K-845).</summary>
    public const string RejectedMessage = "Bu kupon kodu geçerli değil.";

    /// <summary>
    /// True when two codes are the same code: the comparison ignores the case and the Turkish i letters the way the coupon's own uniqueness does - the one normalization of the product (04 2.12.6.2; K-806;
    /// 09 9.4 item 1).
    /// </summary>
    public static bool SameCode(string? first, string? second) =>
        !string.IsNullOrWhiteSpace(first) && !string.IsNullOrWhiteSpace(second) && TextNormalizer.NormalizeIdentity(first) == TextNormalizer.NormalizeIdentity(second);

    /// <summary>The closed row (04 2.12.6.1): how the step opens when no code is applied.</summary>
    public static CouponFieldView Closed(string name) => new(CouponFieldMode.Closed) { CodeField = BlankField(name) };

    /// <summary>The open field (04 2.12.6.2): the code as typed, kept after a refusal, and the refusal under the field when there is one.</summary>
    /// <param name="name">The form's name of the code field.</param>
    /// <param name="typed">The code as typed; the field shows it as typed, case and all.</param>
    /// <param name="refusal">The refused outcome; null for a field that has not been refused.</param>
    /// <param name="minimumBasketAmount">The minimum basket amount, for <see cref="CouponOutcome.BelowMinimumBasket"/>.</param>
    public static CouponFieldView Open(string name, string? typed = null, CouponOutcome? refusal = null, decimal? minimumBasketAmount = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        FieldView field = BlankField(name, typed);
        if (refusal is { } outcome && outcome is CouponOutcome.Rejected or CouponOutcome.BelowMinimumBasket)
        {
            field = Fields.Bind(field, new FormErrorSummary([new FormFieldError(field.Id, RefusalText(outcome, minimumBasketAmount))]));
        }

        return new CouponFieldView(CouponFieldMode.Open) { CodeField = field };
    }

    /// <summary>The applied code (04 2.12.6.2): its name and the amount it takes off, with "Değiştir" and "Kaldır"; the amount is shown as a deduction.</summary>
    public static CouponFieldView Applied(string code, decimal discount, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(discount);
        return new CouponFieldView(CouponFieldMode.Applied) { AppliedCode = code.Trim(), DiscountText = DisplayFormat.Amount(-discount), CodeField = BlankField(name) };
    }

    /// <summary>The field L-6 closed (04 2.12.6.3): closed, with the neutral message in its place and nothing else - no remaining attempts, no length of the block.</summary>
    public static CouponFieldView LimitClosed(string? messageId = "coupon-limit-message") =>
        new(CouponFieldMode.LimitClosed) { LimitMessage = Messages.NeutralLimit(messageId) };

    /// <summary>
    /// The field the module's answer leaves (04 2.12.6.2, 2.12.6.3; 07 UC-23): accepted is the applied hâl with its deduction; a refusal is the open field with its message - the minimum amount's own, or the one
    /// message of the other reasons; the attempt limit closes the field.
    /// </summary>
    public static CouponFieldView From(CouponApplication application, string name, string? typed)
    {
        ArgumentNullException.ThrowIfNull(application);
        return application.Outcome switch
        {
            CouponOutcome.Accepted => Applied(application.Code ?? string.Empty, application.Discount, name),
            CouponOutcome.AttemptLimitExceeded => LimitClosed(),
            _ => Open(name, typed, application.Outcome, application.MinimumBasketAmount),
        };
    }

    /// <summary>
    /// The field of a customer who comes back from the sign-in with a typed code (04 2.12.6.4; 03 3.6.2): the code is applied again and, when it is no longer valid, the refusal is told; a customer who typed
    /// no code sees the closed row. <paramref name="reapplied"/> is the module's answer to the saved code, null when there was no code.
    /// </summary>
    public static CouponFieldView AfterSignIn(string name, string? savedCode, CouponApplication? reapplied)
    {
        if (string.IsNullOrWhiteSpace(savedCode) || reapplied is null)
        {
            return Closed(name);
        }

        return From(reapplied, name, savedCode);
    }

    /// <summary>The text at the field of a refused code (04 2.12.6.3, 5.13.15): the minimum amount's message names the amount; every other refusal is the one message (K-845).</summary>
    public static string RefusalText(CouponOutcome outcome, decimal? minimumBasketAmount = null) => outcome switch
    {
        CouponOutcome.BelowMinimumBasket => BelowMinimumText(minimumBasketAmount ?? throw new ArgumentException("The minimum amount's message names the amount.", nameof(minimumBasketAmount))),
        CouponOutcome.Rejected => RejectedMessage,
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Only the two refusals are told at the field; an accepted code is applied and the limit closes the field."),
    };

    // The code field every state draws: required, no autofill (a code is not a stored value).
    private static FieldView BlankField(string name, string? typed = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return Fields.Text(name, CodeLabel, FieldRequirement.Required, typed, autocomplete: "off");
    }

    /// <summary>The message of a valid code under its minimum basket amount (04 5.13.15; 02 3.10.4): it says the amount.</summary>
    public static string BelowMinimumText(decimal minimumBasketAmount) =>
        $"Bu kupon, {DisplayFormat.Amount(minimumBasketAmount)} ve üzeri sepet tutarında kullanılabilir.";
}
