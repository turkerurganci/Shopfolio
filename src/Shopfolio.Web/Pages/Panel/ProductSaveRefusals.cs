using System.Globalization;

using Microsoft.AspNetCore.Mvc.ModelBinding;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// The field messages of the product form's refused save (07 UC-114 422; 04 9.5.21, 2.12.1.5). A code of the field catalog (07 6.2.2) goes through the validation filter with its
/// label (09 3.4 item 2); a refusal the catalog has no text for - a product rule of K-799, the stock code's uniqueness, the parameter's fence - is written in place as its own text. The
/// values a message names (the reserved quantity, the reference price) are the module's: the page computes no amount.
/// </summary>
internal static class ProductSaveRefusals
{
    /// <summary>K-799: the price.</summary>
    public const string PriceAboveZeroText = "Fiyat sıfırdan büyük olmalı.";

    /// <summary>K-799: the discount that brings a price to zero (02 3.9.1; 03 3.5.1.13).</summary>
    public const string DiscountToZeroText = "İndirim hiçbir seçeneğin fiyatını sıfıra indiremez.";

    /// <summary>04 9.5.21 (K-801; 07 6.2.2 <c>DuplicateSku</c>): the stock code is unique, compared without case (K-1016). The form follows K-799's coupon code text.</summary>
    public const string DuplicateStockCodeText = "Bu stok kodu başka bir seçenekte kullanılıyor.";

    /// <summary>04 2.12.1.5, 9.5.12 (07 6.2.2 <c>PublishedRecordGateBroken</c>): the edit that empties a field the published product's gate needs; the way out is the draft (K-799's form).</summary>
    public const string PublishedGateText = "Yayındaki ürünün bu alanı yayın için gerekli; boşaltılamaz. Önce ürünü taslağa alın.";

    /// <summary>07 6.2.2 <c>ValueOutOfBounds</c>: a value outside its parameter's fence that no field rule caught (P-44's lower fence, the order quantity's, the option count P-21).</summary>
    public const string OutOfBoundsText = "Bu değer izin verilen sınırın dışında.";

    /// <summary>02 3.6.1, 3.6.2 (04 9.5.21): the stock is a whole number that never goes below zero; a digital product has no stock field.</summary>
    public const string StockText = "Stok sıfır ya da daha büyük bir tam sayı olmalı; dijital üründe stok girilmez.";

    /// <summary>K-799: the reserved quantity (02 3.6.7; K-653).</summary>
    public static string BelowReservedText(int reservedQuantity) =>
        $"Stok, ödemesi beklenen siparişlerin ayırdığı {DisplayFormat.Number(reservedQuantity)} adedin altına indirilemez.";

    /// <summary>K-799: the discounted price that is not below the reference price; the amount is the module's reference price.</summary>
    public static string DiscountNotBelowReferenceText(string variant, string referencePrice) =>
        $"{variant} için indirimli fiyat referans fiyatın ({referencePrice}) altına inmiyor; indirim kaydedilmedi.";

    /// <summary>K-799: the file whose removal leaves a published variant without one (02 3.7.4; 03 3.5.1.6).</summary>
    public static string DigitalFileText(string variant) =>
        $"Bu dosya silinirse yayındaki {variant} dosyasız kalır. Önce yeni dosyayı yükleyin ya da seçeneği yayından çekin.";

    /// <summary>K-817: the shipping period's upper fence (P-5).</summary>
    public static string ShippingPeriodText() =>
        $"Kargoya verme süresi {ParameterFences.ShippingPeriodUpper.ToString(CultureInfo.InvariantCulture)} iş gününü aşamaz.";

    /// <summary>
    /// Writes each refusal to its field (<c>Input.</c> and the module's field name). <paramref name="variantName"/> gives a variant row's name for the messages that name the variant.
    /// </summary>
    public static void AddTo(ModelStateDictionary modelState, IEnumerable<ProductSaveFieldError> errors, Func<string, string> variantName)
    {
        ArgumentNullException.ThrowIfNull(modelState);
        ArgumentNullException.ThrowIfNull(errors);
        ArgumentNullException.ThrowIfNull(variantName);
        foreach (ProductSaveFieldError error in errors)
        {
            string key = "Input." + error.Field;
            if (TextOf(error, variantName) is { } text)
            {
                modelState.AddModelError(key, text);
            }
            else
            {
                modelState.AddFieldError(key, error.Code.ToString(), ParameterOf(error));
            }
        }
    }

    /// <summary>The refusal's own text, or null when the field catalog's text with the field's label is the message.</summary>
    public static string? TextOf(ProductSaveFieldError error, Func<string, string> variantName)
    {
        ArgumentNullException.ThrowIfNull(error);
        ArgumentNullException.ThrowIfNull(variantName);
        return error.Code switch
        {
            // The net quantity's refusal is its field rule's text (InvalidNetQuantityAttribute, through the filter); the price's is K-799's.
            ErrorCode.InvalidAmount when IsNetQuantity(error) => null,
            ErrorCode.InvalidAmount => PriceAboveZeroText,
            ErrorCode.DuplicateSku => DuplicateStockCodeText,
            ErrorCode.StockBelowReserved => BelowReservedText(decimal.ToInt32(error.Amount ?? 0m)),
            ErrorCode.DiscountedPriceZero => DiscountToZeroText,
            ErrorCode.DiscountedPriceNotBelowReference => DiscountNotBelowReferenceText(variantName(error.Field), DisplayFormat.Amount(error.ReferenceAmount ?? 0m)),
            ErrorCode.DigitalFileRequired => DigitalFileText(variantName(error.Field)),
            ErrorCode.PublishedRecordGateBroken => PublishedGateText,
            ErrorCode.ValueOutOfBounds when error.Field == nameof(ProductSaveCommand.ShippingPeriod) => ShippingPeriodText(),
            ErrorCode.ValueOutOfBounds => OutOfBoundsText,
            ErrorCode.InvalidQuantity => StockText,
            _ => null,
        };
    }

    /// <summary>The limit a catalog text names (<c>{1}</c>): the module's value when it gave one, else the field rule's own limit.</summary>
    public static string? ParameterOf(ProductSaveFieldError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        if (error.Amount is { } amount)
        {
            return DisplayFormat.Number(amount);
        }

        return error.Code switch
        {
            ErrorCode.InvalidPercentage => new InvalidPercentageAttribute().ParameterText,
            ErrorCode.InvalidAmount when IsNetQuantity(error) => new InvalidNetQuantityAttribute().ParameterText,
            _ => null,
        };
    }

    private static bool IsNetQuantity(ProductSaveFieldError error) => error.Field.EndsWith("." + nameof(ProductSaveVariant.NetQuantity), StringComparison.Ordinal);

    /// <summary>The row of a variant field <c>Variants[n].X</c>; null when the field is no variant row's.</summary>
    public static int? VariantRowOf(string field)
    {
        ArgumentNullException.ThrowIfNull(field);
        const string Prefix = "Variants[";
        int close = field.IndexOf(']', StringComparison.Ordinal);
        if (!field.StartsWith(Prefix, StringComparison.Ordinal) || close <= Prefix.Length)
        {
            return null;
        }

        string index = field[Prefix.Length..close];
        return index.Length <= 4 && index.All(char.IsAsciiDigit) ? int.Parse(index, CultureInfo.InvariantCulture) : null;
    }
}
