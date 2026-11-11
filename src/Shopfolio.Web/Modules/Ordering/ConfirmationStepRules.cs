using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Ordering.Documents;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The rules of the confirmation step (T356; 02 3.24.1-3.24.4, 3.24.7; 04 5.13.9, 2.12.7): which boxes an order needs, the text of each, the button and its payment obligation, the
/// short withdrawal summary above the boxes and the boxes still unticked. Pure: no database, no clock. There is no consent box, no age box and no note field here - the box set is closed
/// to the four <see cref="ConfirmationBox"/> values (02 3.24.1, 3.24.4, 3.24.7).
/// </summary>
internal static class ConfirmationStepRules
{
    /// <summary>The button's text (02 3.24.1; 04 5.13.18): the payment obligation stands in the button itself, for the card and the bank transfer alike (K-544).</summary>
    public const string ConfirmButtonText = "Siparişi onayla — ödeme yükümlülüğü doğar";

    /// <summary>The line next to the button (02 3.24.1): confirming the order creates the obligation to pay, whatever the method.</summary>
    public const string PaymentObligationNotice = "Siparişi onaylamanız ödeme yükümlülüğü doğurur.";

    public const string PreInformationFormBoxText = "Ön Bilgilendirme Formu'nu okudum ve onaylıyorum.";

    public const string DistanceSalesContractBoxText = "Mesafeli Satış Sözleşmesi'ni okudum ve kabul ediyorum.";

    /// <summary>The boxes an order with these lines needs, in screen order: the form and the contract always, the digital one with a digital line, the service one with a service line (02 3.24.1, 3.24.2).</summary>
    public static IReadOnlyList<ConfirmationBox> NeededBoxes(IReadOnlyList<ConfirmationSummaryLine> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        if (lines.Count == 0)
        {
            throw new ArgumentException("An order with no line is a programming error (02 3.16.7).", nameof(lines));
        }

        List<ConfirmationBox> needed = [ConfirmationBox.PreInformationFormRead, ConfirmationBox.DistanceSalesContractAccepted];
        if (lines.Any(l => l.ProductType == ProductType.Digital))
        {
            needed.Add(ConfirmationBox.DigitalWithdrawalWaiver);
        }

        if (lines.Any(l => l.ProductType == ProductType.Service))
        {
            needed.Add(ConfirmationBox.ServiceFulfilmentConsent);
        }

        return needed;
    }

    /// <summary>The boxes offered with their texts (K-771): the digital and the service box count the items they cover by name, in the plural when more than one is covered.</summary>
    public static IReadOnlyList<ConfirmationBoxOffer> Offers(IReadOnlyList<ConfirmationSummaryLine> lines, PaymentMethod method)
    {
        return [.. NeededBoxes(lines).Select(box => new ConfirmationBoxOffer(box, TextOf(box, lines, method), CoveredNames(box, lines)))];
    }

    /// <summary>The needed boxes the customer left unticked, in screen order; a ticked box that is not needed is ignored (the rule reads what the order needs, not what the client sent).</summary>
    public static IReadOnlyList<ConfirmationBox> MissingBoxes(IReadOnlyList<ConfirmationSummaryLine> lines, TickedBoxes ticked)
    {
        ArgumentNullException.ThrowIfNull(ticked);
        return [.. NeededBoxes(lines).Where(box => !IsTicked(box, ticked))];
    }

    /// <summary>The short withdrawal summary above the boxes (02 3.24.3; K-831): the terms, the period, the way and the carrier information; the exempt line's exemption and reason.</summary>
    public static string WithdrawalSummary(IReadOnlyList<ConfirmationSummaryLine> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        List<string> parts =
        [
            "Fiziksel ürünlerde teslim tarihinden itibaren on dört gün içinde, gerekçe göstermeden ve cezai şart ödemeden cayabilirsiniz; cayma hakkınızı sipariş sayfanızdan cayma beyanında bulunarak kullanırsınız. "
            + "Malı, cayma beyanınızdan itibaren on dört gün içinde göndermeniz gerekir.",
            "İade için taşıyıcı belirlenmemiştir: malı dilediğiniz taşıyıcıyla, bedeli teslim alırken satıcı tarafından ödenmek üzere (ödemeli) gönderirsiniz. İade kargo bedeli satıcıya aittir; siz önden ödeme yapmazsınız.",
        ];
        if (lines.Any(l => l.ProductType == ProductType.Service))
        {
            parts.Add("Hizmette cayma hakkınız, sizin onayınızla başlayan hizmetin ifasının tamamlanmasıyla sona erer.");
        }

        if (lines.Any(l => l.ProductType == ProductType.Digital))
        {
            parts.Add("Dijital üründe indirme başladıktan sonra cayma hakkı kullanılamaz.");
        }

        foreach (ConfirmationSummaryLine line in lines.Where(l => l.WithdrawalExemptionReason is not null))
        {
            parts.Add($"{ConfirmationSummaryText.Name(line)}: {ConfirmationSummaryText.ExemptionNote(line.WithdrawalExemptionReason!.Value)}"); // non-null: the Where above keeps only lines with an exemption reason
        }

        return string.Join("\n", parts);
    }

    private static bool IsTicked(ConfirmationBox box, TickedBoxes ticked)
    {
        if (box == ConfirmationBox.PreInformationFormRead)
        {
            return ticked.PreInformationFormRead;
        }

        if (box == ConfirmationBox.DistanceSalesContractAccepted)
        {
            return ticked.DistanceSalesContractAccepted;
        }

        if (box == ConfirmationBox.DigitalWithdrawalWaiver)
        {
            return ticked.DigitalWithdrawalWaiver;
        }

        return box == ConfirmationBox.ServiceFulfilmentConsent ? ticked.ServiceFulfilmentConsent : throw new ArgumentOutOfRangeException(nameof(box), box, "Unknown confirmation box.");
    }

    private static IReadOnlyList<string> CoveredNames(ConfirmationBox box, IReadOnlyList<ConfirmationSummaryLine> lines)
    {
        ProductType? type = box == ConfirmationBox.DigitalWithdrawalWaiver ? ProductType.Digital : box == ConfirmationBox.ServiceFulfilmentConsent ? ProductType.Service : null;
        return type is null ? [] : [.. lines.Where(l => l.ProductType == type).Select(ConfirmationSummaryText.Name).Distinct(StringComparer.Ordinal)];
    }

    private static string TextOf(ConfirmationBox box, IReadOnlyList<ConfirmationSummaryLine> lines, PaymentMethod method)
    {
        IReadOnlyList<string> names = CoveredNames(box, lines);
        bool plural = names.Count > 1;
        string quoted = string.Join(", ", names.Select(n => $"\"{n}\""));
        if (box == ConfirmationBox.PreInformationFormRead)
        {
            return PreInformationFormBoxText;
        }

        if (box == ConfirmationBox.DistanceSalesContractAccepted)
        {
            return DistanceSalesContractBoxText;
        }

        if (box == ConfirmationBox.DigitalWithdrawalWaiver)
        {
            // A bank transfer opens the download with the firm's "paid" mark, so the text does not say "immediately" (02 3.24.2): it names the firm's confirmation.
            return (plural ? "İndirmeler, " : "İndirme, ") + (method == PaymentMethod.BankTransfer ? "firma ödememi onayladığında" : "ödemem onaylandığı anda")
                + " açılacak; " + quoted + (plural ? " ürünlerinde cayma haklarımın düşeceğini kabul ediyorum." : " ürününde cayma hakkımın düşeceğini kabul ediyorum.");
        }

        if (box == ConfirmationBox.ServiceFulfilmentConsent)
        {
            return plural
                ? $"{quoted} hizmetlerinin cayma süresi dolmadan ifasına başlanmasını onaylıyorum; ifa tamamlandığında cayma haklarım düşer."
                : $"{quoted} hizmetinin cayma süresi dolmadan ifasına başlanmasını onaylıyorum; ifa tamamlandığında cayma hakkım düşer.";
        }

        throw new ArgumentOutOfRangeException(nameof(box), box, "Unknown confirmation box.");
    }
}
