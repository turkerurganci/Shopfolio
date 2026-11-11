using Microsoft.AspNetCore.Mvc.ModelBinding;

using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Pages.Components;

/// <summary>The two reason lists (04 2.12.4.2): the firm's cancellation and the status correction. A list is the product's; the firm does not edit it (04 2.12.4.1).</summary>
public enum ReasonKind
{
    /// <summary>03 8.3.1.1 and 02 7.2.4.</summary>
    FirmCancellation,

    /// <summary>03 8.4.5.</summary>
    StatusCorrection,
}

/// <summary>A refusal of the reason fields: the member the message belongs to and its closed code (the page filter turns the code into the text, 09 3.4 item 3).</summary>
public sealed record ReasonFieldError(string Member, string Code);

/// <summary>What the reason fields of a sent form say (04 2.12.4.1): the refusals - none when the reason is right - and whether the step that follows (the confirmation, the correction's sending) may open.</summary>
public sealed record ReasonCheck(IReadOnlyList<ReasonFieldError> Errors)
{
    /// <summary>True when the reason is chosen from the list and, for "diğer", the explanation is written (04 2.12.4.1).</summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// A firm cancellation's confirmation opens only for a valid reason, and a status correction's form is sent only with one (04 2.12.4.1): without a reason the answer is the field message and no
    /// confirmation window, no correction.
    /// </summary>
    public bool MayContinue => IsValid;

    /// <summary>Puts the refusals into the model state by their codes under <paramref name="namePrefix"/>; the page filter writes the texts.</summary>
    public void AddTo(ModelStateDictionary modelState, string namePrefix)
    {
        ArgumentNullException.ThrowIfNull(modelState);
        ArgumentNullException.ThrowIfNull(namePrefix);
        foreach (ReasonFieldError error in Errors)
        {
            modelState.AddFieldError(namePrefix + error.Member, error.Code);
        }
    }
}

/// <summary>
/// The reason selection (T543; 04 OB-15, 2.12.4): the closed list as a select and the explanation field beside it, which "diğer" makes required. The members are <c>Reason</c> and <c>Explanation</c> after the
/// screen's prefix. The list is read from the enum of the variant, so a reason added to <c>06 5.3.6</c> or <c>5.3.9</c> reaches the form with its text or fails the test that keeps the texts complete.
/// </summary>
public sealed class ReasonSelectView
{
    internal ReasonSelectView(ReasonKind kind, FieldView reason, FieldView explanation, bool legalWarning)
    {
        Kind = kind;
        Reason = reason;
        Explanation = explanation;
        LegalWarningBeforeConfirmation = legalWarning;
    }

    public ReasonKind Kind { get; }

    /// <summary>The closed list.</summary>
    public FieldView Reason { get; }

    /// <summary>The explanation; required when "diğer" is chosen, optional otherwise.</summary>
    public FieldView Explanation { get; }

    /// <summary>True when the chosen reason is "stokta bulunamadı": the legal warning shows before the confirmation (04 2.12.4.2, 2.4.3).</summary>
    public bool LegalWarningBeforeConfirmation { get; }

    /// <summary>The names of the two fields in the order they are drawn.</summary>
    public IReadOnlyList<string> FieldOrder => [Reason.Name, Explanation.Name];

    /// <summary>The same selection with the summary bound to its fields.</summary>
    public ReasonSelectView Bind(FormErrorSummary? summary) => new(Kind, Fields.Bind(Reason, summary), Fields.Bind(Explanation, summary), LegalWarningBeforeConfirmation);
}

/// <summary>The only constructors of a <see cref="ReasonSelectView"/>, the texts of the lists and the rule of the reason fields (T543; 04 2.12.4).</summary>
public static class Reasons
{
    /// <summary>The member of the reason's closed list.</summary>
    public const string ReasonMember = "Reason";

    /// <summary>The member of the explanation.</summary>
    public const string ExplanationMember = "Explanation";

    /// <summary>The value "diğer": both lists name it <c>Other</c> (06 5.3.6, 5.3.9).</summary>
    public const string OtherValue = nameof(CancellationReason.Other);

    /// <summary>The visible label of the list.</summary>
    public const string ReasonLabel = "Sebep";

    /// <summary>The visible label of the explanation.</summary>
    public const string ExplanationLabel = "Açıklama";

    /// <summary>The line beside the explanation field (04 2.12.4.1).</summary>
    public const string ExplanationHint = "\"Diğer\" seçilirse açıklama zorunludur.";

    /// <summary>The texts of the firm's cancellation list in the order 03 8.3.1.1 lists them.</summary>
    public static IReadOnlyList<FieldOption> CancellationOptions { get; } =
    [
        new(nameof(CancellationReason.OutOfStock), "Stokta bulunamadı"),
        new(nameof(CancellationReason.DamagedOrUnsellable), "Ürün hasarlı ya da satılamaz durumda"),
        new(nameof(CancellationReason.CannotShipToAddress), "Teslimat adresine gönderim yapılamıyor"),
        new(nameof(CancellationReason.UndeliveredBuyerCaused), "Gönderi teslim edilemedi — müşteri kaynaklı"),
        new(nameof(CancellationReason.AgreedWithBuyer), "Müşteriyle anlaşıldı (müşteri talebi)"),
        new(nameof(CancellationReason.Other), "Diğer"),
    ];

    /// <summary>The texts of the status correction list in the order 03 8.4.5 lists them.</summary>
    public static IReadOnlyList<FieldOption> CorrectionOptions { get; } =
    [
        new(nameof(CorrectionReason.WrongOrder), "Yanlış siparişte işlem yapıldı"),
        new(nameof(CorrectionReason.MarkedBeforeOccurred), "İşlem gerçekleşmeden işaretlendi"),
        new(nameof(CorrectionReason.Other), "Diğer"),
    ];

    /// <summary>The list of a kind.</summary>
    public static IReadOnlyList<FieldOption> OptionsOf(ReasonKind kind) => kind switch
    {
        ReasonKind.FirmCancellation => CancellationOptions,
        ReasonKind.StatusCorrection => CorrectionOptions,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Not a reason list."),
    };

    /// <summary>The form of a kind with the names under <paramref name="namePrefix"/> (<c>Input.</c> gives <c>Input.Reason</c> and <c>Input.Explanation</c>); <paramref name="selected"/> and <paramref name="explanation"/> are the sent or stored values.</summary>
    public static ReasonSelectView Create(ReasonKind kind, string namePrefix, string? selected = null, string? explanation = null)
    {
        ArgumentNullException.ThrowIfNull(namePrefix);
        IReadOnlyList<FieldOption> options = OptionsOf(kind);
        bool other = string.Equals(selected, OtherValue, StringComparison.Ordinal);
        FieldView reason = Fields.ClosedList(namePrefix + ReasonMember, ReasonLabel, FieldRequirement.Required, options, selected);
        FieldView note = Fields.TextArea(
            namePrefix + ExplanationMember,
            ExplanationLabel,
            other ? FieldRequirement.Required : FieldRequirement.Optional,
            explanation,
            hint: ExplanationHint);
        bool legalWarning = kind == ReasonKind.FirmCancellation && string.Equals(selected, nameof(CancellationReason.OutOfStock), StringComparison.Ordinal);
        return new ReasonSelectView(kind, reason, note, legalWarning);
    }

    /// <summary>
    /// What the reason fields say of a sent form (04 2.12.4.1): no reason is <c>FieldRequired</c> at the list; a value outside the list is <c>InvalidListValue</c> - free text is not a reason; "diğer"
    /// without an explanation (a text of white space only is none, 04 7.3.1) is <c>FieldRequired</c> at the explanation.
    /// </summary>
    public static ReasonCheck Check(ReasonKind kind, string? reason, string? explanation)
    {
        List<ReasonFieldError> errors = [];
        string? chosen = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        if (chosen is null)
        {
            errors.Add(new ReasonFieldError(ReasonMember, FieldCodes.FieldRequired));
        }
        else if (!ClosedLists.Accepts(OptionsOf(kind), chosen))
        {
            errors.Add(new ReasonFieldError(ReasonMember, FieldCodes.InvalidListValue));
        }
        else if (chosen == OtherValue && string.IsNullOrWhiteSpace(explanation))
        {
            errors.Add(new ReasonFieldError(ExplanationMember, FieldCodes.FieldRequired));
        }

        return new ReasonCheck(errors);
    }
}
