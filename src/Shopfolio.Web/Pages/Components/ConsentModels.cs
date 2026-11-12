using Microsoft.AspNetCore.Mvc.ModelBinding;

using Shopfolio.Infrastructure.Validation;

namespace Shopfolio.Pages.Components;

/// <summary>The four boxes of the order confirmation (04 2.12.7.1; 02 3.24.1, 3.24.2): two are fixed, the third and the fourth come with the basket.</summary>
public enum ConsentKind
{
    /// <summary>The Ön Bilgilendirme Formu's reading is confirmed; fixed.</summary>
    PreInformation,

    /// <summary>The Mesafeli Satış Sözleşmesi is approved; fixed.</summary>
    DistanceSales,

    /// <summary>The digital item's box: the download opens when the payment is confirmed and the right of withdrawal falls (02 3.24.2); only with a digital item in the basket.</summary>
    Digital,

    /// <summary>The service item's box: the performance starts before the withdrawal period ends and the right falls when it is done (02 3.24.2); only with a service item in the basket.</summary>
    Service,
}

/// <summary>Why the legal boxes are emptied when the confirmation is shown again (04 2.12.7.3; K-756, K-833).</summary>
public enum ConsentReset
{
    None,

    /// <summary>A legal text's version changed while the step was open: the marks of the boxes are taken off (04 2.12.7.3; K-756).</summary>
    TextVersionChanged,

    /// <summary>The order summary changed in a way that changes the Ön Bilgilendirme Formu's content: the two legal boxes are emptied (04 2.12.7.3; 02 3.24.3; K-833).</summary>
    SummaryChanged,
}

/// <summary>What the confirmation step was shown with: the identifiers of the two legal texts' versions and the fingerprint of the summary (07 UC-22; T509 k1). A form carries them; the next answer is compared with the current ones.</summary>
public sealed record ConsentState(string TextVersions, string SummaryFingerprint);

/// <summary>What the basket brings to the boxes: the digital and the service items the extra boxes name, and the payment line (the digital text does not say "hemen" on the transfer line, 02 3.24.2).</summary>
public sealed record ConsentBasket(IReadOnlyList<string> DigitalItemNames, IReadOnlyList<string> ServiceItemNames, bool BankTransfer = false)
{
    /// <summary>No digital and no service item: only the two fixed boxes.</summary>
    public static ConsentBasket Physical { get; } = new([], []);
}

/// <summary>The boxes as they were sent: a box not sent is not ticked.</summary>
public sealed record ConsentSent(bool PreInformation, bool DistanceSales, bool Digital, bool Service)
{
    /// <summary>The value a ticked box sends.</summary>
    public const string TickedValue = "true";

    /// <summary>True when the posted value is a tick: <c>true</c> (what the box sends) or <c>on</c> (a box with no value of its own).</summary>
    public static bool IsTicked(string? posted) =>
        string.Equals(posted, TickedValue, StringComparison.OrdinalIgnoreCase) || string.Equals(posted, "on", StringComparison.OrdinalIgnoreCase);

    public bool Of(ConsentKind kind) => kind switch
    {
        ConsentKind.PreInformation => PreInformation,
        ConsentKind.DistanceSales => DistanceSales,
        ConsentKind.Digital => Digital,
        ConsentKind.Service => Service,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "There are four boxes."),
    };
}

/// <summary>One box as the view draws it: its member, its text and whether it is ticked; a box that is wrong carries its message.</summary>
/// <param name="Kind">Which box.</param>
/// <param name="Name">The form's name of the box.</param>
/// <param name="Text">The visible text; for the extra boxes it names the items.</param>
/// <param name="IsTicked">True only for a box the customer ticked and no reset emptied; no box is ticked when the step opens (04 2.12.7.1; K-756).</param>
/// <param name="Error">The box's own message, drawn at the box; null when it is right.</param>
public sealed record ConsentBoxView(ConsentKind Kind, string Name, string Text, bool IsTicked, MessageView? Error = null)
{
    public string Id => FormErrorSummary.FieldIdOf(Name);

    public string MessageId => Messages.FieldMessageId(Id);

    public bool IsInvalid => Error is not null;
}

/// <summary>The confirmation's boxes (04 OB-18) and the place they stand in the section.</summary>
public sealed class ConsentBoxesView
{
    internal ConsentBoxesView(IReadOnlyList<ConsentBoxView> boxes)
    {
        Boxes = boxes;
    }

    public IReadOnlyList<ConsentBoxView> Boxes { get; }

    /// <summary>The heading of the group.</summary>
    public string Legend => ConsentBoxes.Legend;
}

/// <summary>The parts of the confirmation section, in the order they stand (04 2.12.7.2, 5.13.9; K-756, K-831).</summary>
public enum ConfirmPart
{
    LegalTexts,
    WithdrawalSummary,
    Boxes,
    ConfirmButton,
}

/// <summary>The boxes' words, rules and constructors (T544; 04 OB-18, 2.12.7; 02 3.24).</summary>
public static class ConsentBoxes
{
    public const string Legend = "Onaylar";

    public const string PreInformationText = "Ön Bilgilendirme Formu'nu okuduğumu teyit ediyorum.";

    public const string DistanceSalesText = "Mesafeli Satış Sözleşmesi'ni onaylıyorum.";

    /// <summary>The order the parts of the confirmation section stand in: the legal texts and the withdrawal summary above, then the boxes, then the button - nothing between the boxes and the button (04 2.12.7.2).</summary>
    public static IReadOnlyList<ConfirmPart> SectionOrder { get; } =
        [ConfirmPart.LegalTexts, ConfirmPart.WithdrawalSummary, ConfirmPart.Boxes, ConfirmPart.ConfirmButton];

    /// <summary>True when the boxes stand under the legal texts and the withdrawal summary and the button follows them at once (04 2.12.7.2).</summary>
    public static bool IsPlacedRight(IReadOnlyList<ConfirmPart> order)
    {
        ArgumentNullException.ThrowIfNull(order);
        int boxes = order.ToList().IndexOf(ConfirmPart.Boxes);
        int button = order.ToList().IndexOf(ConfirmPart.ConfirmButton);
        int legal = order.ToList().IndexOf(ConfirmPart.LegalTexts);
        int summary = order.ToList().IndexOf(ConfirmPart.WithdrawalSummary);
        return boxes >= 0 && button == boxes + 1 && legal >= 0 && legal < boxes && summary >= 0 && summary < boxes;
    }

    /// <summary>The boxes the basket needs, in the order they are drawn: the two fixed ones, the digital one when the basket has a digital item and the service one when it has a service item (04 2.12.7.1).</summary>
    public static IReadOnlyList<ConsentKind> RequiredFor(ConsentBasket basket)
    {
        ArgumentNullException.ThrowIfNull(basket);
        List<ConsentKind> kinds = [ConsentKind.PreInformation, ConsentKind.DistanceSales];
        if (basket.DigitalItemNames.Count > 0)
        {
            kinds.Add(ConsentKind.Digital);
        }

        if (basket.ServiceItemNames.Count > 0)
        {
            kinds.Add(ConsentKind.Service);
        }

        return kinds;
    }

    /// <summary>
    /// The text of a box (04 2.12.7.6; 02 3.24.1, 3.24.2; K-771): the two fixed texts; the digital and the service box name the items they cover, and with several items the text is plural. The digital text on the
    /// transfer line says the download opens when the company marks the payment, not at once.
    /// </summary>
    public static string TextOf(ConsentKind kind, ConsentBasket basket)
    {
        ArgumentNullException.ThrowIfNull(basket);
        return kind switch
        {
            ConsentKind.PreInformation => PreInformationText,
            ConsentKind.DistanceSales => DistanceSalesText,
            ConsentKind.Digital => DigitalText(basket),
            ConsentKind.Service => ServiceText(basket),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "There are four boxes."),
        };
    }

    private static string DigitalText(ConsentBasket basket)
    {
        bool many = basket.DigitalItemNames.Count > 1;
        string opens = basket.BankTransfer ? "ödemem firmaca onaylandığı anda" : "ödemem onaylandığı anda";
        return $"{Names(basket.DigitalItemNames)} için indirme, {opens} açılacak; {(many ? "bu ürünlerde" : "bu üründe")} cayma hakkımın düşeceğini kabul ediyorum.";
    }

    private static string ServiceText(ConsentBasket basket)
    {
        bool many = basket.ServiceItemNames.Count > 1;
        return $"{Names(basket.ServiceItemNames)} için {(many ? "hizmetlerin" : "hizmetin")} cayma süresi dolmadan ifasına başlanmasını onaylıyorum; ifa tamamlandığında cayma hakkım düşer.";
    }

    private static string Names(IReadOnlyList<string> names) => string.Join(", ", names.Select(n => "“" + n + "”"));

    /// <summary>The name of a box in a form with the screen's prefix.</summary>
    public static string NameOf(ConsentKind kind, string namePrefix)
    {
        ArgumentNullException.ThrowIfNull(namePrefix);
        return namePrefix + kind switch
        {
            ConsentKind.PreInformation => "AcceptPreInformation",
            ConsentKind.DistanceSales => "AcceptDistanceSales",
            ConsentKind.Digital => "AcceptDigital",
            ConsentKind.Service => "AcceptService",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "There are four boxes."),
        };
    }

    /// <summary>
    /// What a sent form brings back to a shown confirmation (04 2.12.7.3; K-756, K-833): a changed legal text's version takes the marks off every box; a changed summary fingerprint takes them off the two legal boxes.
    /// The text of the version is compared as the ids the form carried and the ids now.
    /// </summary>
    public static ConsentReset ResetOf(ConsentState? sent, ConsentState current)
    {
        ArgumentNullException.ThrowIfNull(current);
        if (sent is null)
        {
            return ConsentReset.None;
        }

        if (!string.Equals(sent.TextVersions, current.TextVersions, StringComparison.Ordinal))
        {
            return ConsentReset.TextVersionChanged;
        }

        return string.Equals(sent.SummaryFingerprint, current.SummaryFingerprint, StringComparison.Ordinal) ? ConsentReset.None : ConsentReset.SummaryChanged;
    }

    /// <summary>The ticks the form keeps after the reset: none when the text version changed, only the digital and the service ticks when only the summary changed (the two legal boxes are emptied), all of them when nothing changed.</summary>
    public static ConsentSent AfterReset(ConsentSent sent, ConsentReset reset)
    {
        ArgumentNullException.ThrowIfNull(sent);
        return reset switch
        {
            ConsentReset.None => sent,
            ConsentReset.SummaryChanged => sent with { PreInformation = false, DistanceSales = false },
            ConsentReset.TextVersionChanged => new ConsentSent(false, false, false, false),
            _ => throw new ArgumentOutOfRangeException(nameof(reset), reset, "Not a known reset."),
        };
    }

    /// <summary>The kinds that are required and not ticked, in the order the boxes are drawn (04 2.12.7.3): a form sent with one of them unticked does not make the order.</summary>
    public static IReadOnlyList<ConsentKind> Missing(ConsentBasket basket, ConsentSent sent)
    {
        ArgumentNullException.ThrowIfNull(basket);
        ArgumentNullException.ThrowIfNull(sent);
        return [.. RequiredFor(basket).Where(kind => !sent.Of(kind))];
    }

    /// <summary>Puts the missing boxes into the model state with the catalog's code (<c>CheckboxNotAccepted</c>); the page filter writes the text. The first of them is the box the screen goes to.</summary>
    public static void AddRefusals(ModelStateDictionary modelState, string namePrefix, IReadOnlyList<ConsentKind> missing)
    {
        ArgumentNullException.ThrowIfNull(modelState);
        ArgumentNullException.ThrowIfNull(missing);
        foreach (ConsentKind kind in missing)
        {
            modelState.AddFieldError(NameOf(kind, namePrefix), FieldCodes.CheckboxNotAccepted);
        }
    }

    /// <summary>The names of the boxes in the order they are drawn: what <see cref="FormErrorSummary.From"/> takes to find the first wrong box.</summary>
    public static IReadOnlyList<string> FormOrder(string namePrefix, ConsentBasket basket) =>
        [.. RequiredFor(basket).Select(kind => NameOf(kind, namePrefix))];

    /// <summary>
    /// The boxes as the step draws them (04 2.12.7.1, 2.12.7.3): the boxes the basket needs; none ticked when the step opens (<paramref name="sent"/> is null); a sent form keeps its ticks except what the reset
    /// emptied; a box that is wrong carries its message from <paramref name="summary"/>.
    /// </summary>
    public static ConsentBoxesView Create(string namePrefix, ConsentBasket basket, ConsentSent? sent = null, ConsentReset reset = ConsentReset.None, FormErrorSummary? summary = null)
    {
        ArgumentNullException.ThrowIfNull(namePrefix);
        ArgumentNullException.ThrowIfNull(basket);
        ConsentSent kept = sent is null ? new ConsentSent(false, false, false, false) : AfterReset(sent, reset);
        List<ConsentBoxView> boxes = [];
        foreach (ConsentKind kind in RequiredFor(basket))
        {
            string name = NameOf(kind, namePrefix);
            ConsentBoxView box = new(kind, name, TextOf(kind, basket), kept.Of(kind));
            FormFieldError? error = summary?.Fields.FirstOrDefault(f => string.Equals(f.FieldId, box.Id, StringComparison.OrdinalIgnoreCase));
            boxes.Add(error is null ? box : box with { Error = Messages.ForField(box.Id, error.Text) });
        }

        return new ConsentBoxesView(boxes);
    }
}
