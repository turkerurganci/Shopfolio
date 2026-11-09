using System.Text;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering.Documents;

/// <summary>
/// The confirmation summary as text (T356; 02 3.24.3; 04 5.13.9 (b); 09 11.4): one function writes it for the step before the order exists and for the order's frozen items after it, so
/// "the form is the summary, word for word" (02 3.24.3) holds by construction - the line is one shape (<see cref="ConfirmationSummaryLine"/>) made from the product's source or from the
/// frozen item, and both are rendered by <see cref="Render"/>. Pure: no database, no clock. The text holds no personal data: the e-mail line is written apart (<see cref="EmailLine"/>) and the
/// addresses are the form's tokens. A product name that carries a double opening brace is made inert (<see cref="PlaceholderFiller.Inert"/>) so the order stage's filling of the form's tokens
/// cannot expand it. The folder is in the scope of the rendering scan: no raw markup, no request object (D-167).
/// </summary>
internal static class ConfirmationSummaryText
{
    public const string Heading = "SİPARİŞ ÖZETİ";

    /// <summary>The condition a conditional exemption's customer reads (02 7.3.4; K-509).</summary>
    public const string ConditionalWithdrawalNote = "koruyucu ambalajı açılmamışsa cayabilirsiniz";

    /// <summary>The line of an item before the order exists (02 3.24.3): the price that applies, the list price while a discount runs, the coupon's share from the price calculation.</summary>
    public static ConfirmationSummaryLine LineOf(OrderItemSource source, int quantity, ItemAmounts amounts)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(amounts);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        decimal applied = source.Discount?.DiscountedPrice ?? source.UnitPrice;
        if (amounts.Amount != applied * quantity)
        {
            throw new ArgumentException("The amounts are not of the price that applies and the quantity (02 3.8.3).", nameof(amounts));
        }

        return new ConfirmationSummaryLine(
            source.ProductName,
            source.FirstOptionValue,
            source.SecondOptionValue,
            source.ProductType,
            quantity,
            applied,
            source.Discount is null ? null : source.UnitPrice,
            source.Discount?.Percentage,
            amounts.CouponShare,
            source.VatRate,
            amounts.Vat,
            amounts.Payable,
            source.ProductType == ProductType.Physical ? source.WithdrawalExemptionReason : null,
            source.ProductType == ProductType.Service ? source.FulfilmentPeriod : null);
    }

    /// <summary>The line of a frozen item (02 3.23.1): read from the item's own columns, never from the product.</summary>
    public static ConfirmationSummaryLine LineOf(OrderItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        decimal applied = item.DiscountedUnitPrice ?? item.UnitPrice;
        return new ConfirmationSummaryLine(
            item.ProductName,
            item.FirstOptionValue,
            item.SecondOptionValue,
            item.ProductType,
            item.Quantity,
            applied,
            item.DiscountedUnitPrice is null ? null : item.UnitPrice,
            item.DiscountPercentage,
            item.CouponShare,
            item.VatRate,
            item.VatAmount,
            (applied * item.Quantity) - item.CouponShare,
            item.WithdrawalExemptionReason,
            item.FulfilmentPeriod);
    }

    /// <summary>The summary as plain text: the lines, the discount, the coupon's share, the VAT, the exemption and the fulfilment period of a line, the subtotal, the shipping fee or "Kargo ücretsiz", the total.</summary>
    public static string Render(ConfirmationSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);
        StringBuilder text = new();
        text.Append(Heading);
        for (int i = 0; i < summary.Lines.Count; i++)
        {
            ConfirmationSummaryLine line = summary.Lines[i];
            text.Append("\n\n").Append(i + 1).Append(". ").Append(PlaceholderFiller.Inert(Name(line)));
            text.Append("\nAdet: ").Append(DisplayFormat.Number(line.Quantity));
            text.Append("\nBirim fiyat: ").Append(DisplayFormat.Amount(line.UnitPrice));
            if (line.ListPrice is decimal list && line.DiscountPercentage is decimal percentage)
            {
                text.Append(" (indirimden önceki fiyat ").Append(DisplayFormat.Amount(list)).Append(", indirim ").Append(DisplayFormat.Percent(percentage)).Append(')');
            }

            if (line.CouponShare > 0)
            {
                text.Append("\nKupon payı: ").Append(DisplayFormat.Amount(-line.CouponShare));
            }

            text.Append("\nKDV (").Append(DisplayFormat.Percent(line.VatRate)).Append(" dahil): ").Append(DisplayFormat.Amount(line.Vat));
            text.Append("\nSatır tutarı: ").Append(DisplayFormat.Amount(line.Payable));
            if (line.WithdrawalExemptionReason is WithdrawalExemptionReason reason)
            {
                text.Append('\n').Append(ExemptionNote(reason));
            }

            if (line.FulfilmentPeriodDays is int days)
            {
                text.Append("\nİfa süresi: ").Append(DisplayFormat.Days(days)).Append(" (ödemenin onaylandığı andan itibaren)");
            }
        }

        decimal subtotal = summary.Lines.Sum(l => l.UnitPrice * l.Quantity);
        decimal couponShare = summary.Lines.Sum(l => l.CouponShare);
        text.Append("\n\nAra toplam: ").Append(DisplayFormat.Amount(subtotal));
        if (couponShare > 0)
        {
            text.Append("\nKupon indirimi: ").Append(DisplayFormat.Amount(-couponShare));
        }

        if (summary.ShippingFee > 0)
        {
            text.Append("\nKargo ücreti: ").Append(DisplayFormat.Amount(summary.ShippingFee));
        }
        else if (summary.ShippingPeriod is not null)
        {
            text.Append("\nKargo ücretsiz");
        }

        text.Append("\nÖdenecek toplam (KDV dahil): ").Append(DisplayFormat.Amount(summary.Total));
        return text.ToString();
    }

    /// <summary>The guest's e-mail line above the "Düzelt" link (04 5.13.9 (b); 02 3.24.5).</summary>
    public static string EmailLine(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        return "Siparişiniz şu adrese gönderilecek: " + email;
    }

    /// <summary>The item's name with its option values: "Lamba (Kırmızı, M)".</summary>
    public static string Name(ConfirmationSummaryLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        string[] options = [.. new[] { line.FirstOptionValue, line.SecondOptionValue }.OfType<string>()];
        return options.Length == 0 ? line.ProductName : $"{line.ProductName} ({string.Join(", ", options)})";
    }

    /// <summary>True for the two conditional reasons (02 7.3.4 (ç), (e)): the right holds while the protective packaging is unopened.</summary>
    public static bool IsConditional(WithdrawalExemptionReason reason) => reason is WithdrawalExemptionReason.HygieneSensitive or WithdrawalExemptionReason.SealedMedia;

    /// <summary>The reason's name in the words of the Regulation's article 15 (02 7.3.4).</summary>
    public static string ReasonName(WithdrawalExemptionReason reason) => reason switch
    {
        WithdrawalExemptionReason.MadeToOrder => "tüketicinin istekleri veya kişisel ihtiyaçları doğrultusunda hazırlanan mal",
        WithdrawalExemptionReason.Perishable => "çabuk bozulabilen mal",
        WithdrawalExemptionReason.ExpiryDated => "son kullanma tarihi geçebilecek mal",
        WithdrawalExemptionReason.HygieneSensitive => "teslimden sonra koruyucu unsurları açılmış, iadesi sağlık ve hijyen açısından uygun olmayan mal",
        WithdrawalExemptionReason.SealedMedia => "teslimden sonra koruyucu unsurları açılmış kitap, dijital içerik ve bilgisayar sarf malzemesi",
        _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unknown withdrawal exemption reason."),
    };

    /// <summary>
    /// The exemption of a line as the summary and the withdrawal summary say it (02 3.24.3; K-206, K-509): an absolute reason says the right does not exist and why; a conditional one
    /// keeps the right under the condition of the unopened protective packaging.
    /// </summary>
    public static string ExemptionNote(WithdrawalExemptionReason reason) => IsConditional(reason)
        ? $"Cayma hakkı istisnası ({ReasonName(reason)}): {ConditionalWithdrawalNote}."
        : $"Cayma hakkı yoktur: {ReasonName(reason)}.";
}
