using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Components;

/// <summary>Where the IBAN field is drawn (04 2.12.3.2): in the declaration of a cancellation, a late-delivery termination or a withdrawal, on the order page when an IBAN is asked, or read-only in the panel.</summary>
public enum IbanFieldPlace
{
    /// <summary>E-17, E-18: a field of the form, empty when it arrives.</summary>
    Declaration,

    /// <summary>E-16: opened on the order page when an IBAN request arises (04 2.12.3.2; K-1075).</summary>
    OrderPage,

    /// <summary>E-31, E-36, E-37: the administrator who processes the refund sees the customer's IBAN and cannot enter one (04 2.12.3.2; 03 8.3.3.2).</summary>
    Panel,
}

/// <summary>The two exceptions that open the field on the card line (04 2.12.3.1): a card refund that did not happen at the provider and the firm opened the transfer way, and the refund of a payment that came late for a cancelled order.</summary>
public enum IbanOpening
{
    Standard,

    /// <summary>The card refund did not happen at the provider; the firm opened the bank transfer way for the item (K-569).</summary>
    CardRefundNotRealized,

    /// <summary>The refund of a payment that arrived after the order was cancelled (K-1075).</summary>
    LatePayment,
}

/// <summary>
/// The IBAN field with its place (T543; 04 OB-14): the field itself, whether it can still be changed, and the link of the privacy notice beside it - on the customer's side only (04 2.12.3.1, 7.3.2.7).
/// </summary>
/// <param name="Field">The field; the same <see cref="FieldView"/> every field is.</param>
/// <param name="Place">Where it is drawn.</param>
/// <param name="Editable">False once the refund has been processed (04 2.12.3.3) and always in the panel.</param>
/// <param name="NoticeHref">The privacy notice's address beside the field; null in the panel.</param>
/// <param name="NoticeText">The link's text; null in the panel.</param>
public sealed record IbanFieldView(FieldView Field, IbanFieldPlace Place, bool Editable, string? NoticeHref, string? NoticeText)
{
    /// <summary>True when the field is optional - the exception of the card line (04 2.12.3.1).</summary>
    public bool IsOptional => Field.Requirement == FieldRequirement.Optional;
}

/// <summary>The only constructors of an <see cref="IbanFieldView"/> and the rule of when the field is there (T543; 04 2.12.3).</summary>
public static class IbanFields
{
    /// <summary>The visible label of the field.</summary>
    public const string Label = "IBAN";

    /// <summary>The line under the field once the refund is processed (04 2.12.3.3).</summary>
    public const string ProcessedHint = "Geri ödeme işlendi; IBAN artık değiştirilemez.";

    /// <summary>
    /// 04 2.12.3.1: the field is on the bank transfer line; the card line has none and asks none - except the two exceptions of <see cref="IbanOpening"/>, where the field opens on the order page and is optional.
    /// </summary>
    public static bool IsShown(PaymentMethod method, IbanOpening opening) =>
        method == PaymentMethod.BankTransfer || opening != IbanOpening.Standard;

    /// <summary>
    /// The field of a place, or null where there is none (04 2.12.3.1). A declaration asks the IBAN of a paid bank transfer order and is required; the order page opens it when an IBAN is asked and
    /// it is required, except on the card line's two exceptions, where it is optional; the panel shows the stored value read-only. A processed refund closes the field to changes (04 2.12.3.3).
    /// </summary>
    /// <param name="place">Where the field is drawn.</param>
    /// <param name="method">The line the order was paid on.</param>
    /// <param name="opening">The card line's exception, when there is one.</param>
    /// <param name="name">The form's name of the field.</param>
    /// <param name="value">The IBAN as typed or stored.</param>
    /// <param name="processed">True once the refund that needs this IBAN has been processed.</param>
    public static IbanFieldView? Create(IbanFieldPlace place, PaymentMethod method, IbanOpening opening, string name, string? value = null, bool processed = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (!Enum.IsDefined(place) || !Enum.IsDefined(method) || !Enum.IsDefined(opening))
        {
            throw new ArgumentOutOfRangeException(nameof(place), place, "Not a known place, line or exception.");
        }

        bool shown = place == IbanFieldPlace.Declaration ? method == PaymentMethod.BankTransfer : IsShown(method, opening);
        if (!shown)
        {
            return null;
        }

        FieldRequirement requirement = place == IbanFieldPlace.OrderPage && opening != IbanOpening.Standard ? FieldRequirement.Optional : FieldRequirement.Required;
        bool editable = place != IbanFieldPlace.Panel && !processed;
        FieldView field = Fields.Text(name, Label, requirement, value, autocomplete: "off", hint: processed && place != IbanFieldPlace.Panel ? ProcessedHint : null);
        if (!editable)
        {
            field = Fields.AsReadOnly(field);
        }

        bool customerSide = place != IbanFieldPlace.Panel;
        return new IbanFieldView(
            field,
            place,
            editable,
            customerSide ? FramePaths.PrivacyNotice : null,
            customerSide ? FramePaths.PrivacyNoticeTitle : null);
    }
}
