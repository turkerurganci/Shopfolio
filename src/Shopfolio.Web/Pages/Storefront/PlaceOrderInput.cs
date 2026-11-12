using Shopfolio.Infrastructure.Binding;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Pages.Components;

namespace Shopfolio.Pages.Storefront;

/// <summary>
/// The payment step's one form (07 5.2 <c>PlaceOrderInput</c>; 04 7.1.5): the five handlers of UC-23…UC-27 bind it (07 5.1.5). The fields carry no rule attribute: the coupon, the recalculation and the
/// leaving do not validate, and the confirmation checks the fields itself with the project's rules (the e-mail rule, the address part, the boxes) so the errors are marked together (04 5.13.18).
/// The boxes are the confirmation section's (04 OB-18; <see cref="ConsentBoxes.NameOf"/>).
/// </summary>
public sealed class PlaceOrderInput : IFormInput
{
    public string? ContactEmail { get; set; }

    public Guid? AddressBookId { get; set; }

    public string? ShippingRecipientName { get; set; }

    public Guid? ShippingProvinceId { get; set; }

    public string? ShippingDistrict { get; set; }

    public string? ShippingAddressLine { get; set; }

    public bool SaveToAddressBook { get; set; }

    public string? ShippingPhone { get; set; }

    public bool IsBillingSameAsShipping { get; set; }

    public string? BillingRecipientName { get; set; }

    public Guid? BillingProvinceId { get; set; }

    public string? BillingDistrict { get; set; }

    public string? BillingAddressLine { get; set; }

    public string? CouponCode { get; set; }

    public PaymentMethod? PaymentMethod { get; set; }

    public string? AcceptPreInformation { get; set; }

    public string? AcceptDistanceSales { get; set; }

    public string? AcceptDigital { get; set; }

    public string? AcceptService { get; set; }

    public string? ConfirmationKey { get; set; }

    public string? SummaryFingerprint { get; set; }

    public Guid? PreInformationFormVersionId { get; set; }

    public Guid? DistanceSalesContractVersionId { get; set; }

    public string? LeaveTarget { get; set; }
}

/// <summary>The conversions of the payment step's form (T509): to the cookie's fields, back from them, to the boxes and to the address part's request.</summary>
public static class PlaceOrderInputs
{
    /// <summary>The form the cookie fills (07 UC-22): only the fields 05 6.7.9 keeps; the boxes, the key, the fingerprint and the versions stay empty.</summary>
    public static PlaceOrderInput From(CheckoutCookieValues kept)
    {
        ArgumentNullException.ThrowIfNull(kept);
        return new PlaceOrderInput
        {
            ContactEmail = kept.ContactEmail,
            AddressBookId = kept.AddressBookId,
            ShippingRecipientName = kept.ShippingRecipientName,
            ShippingProvinceId = kept.ShippingProvinceId,
            ShippingDistrict = kept.ShippingDistrict,
            ShippingAddressLine = kept.ShippingAddressLine,
            ShippingPhone = kept.ShippingPhone,
            IsBillingSameAsShipping = kept.IsBillingSameAsShipping,
            BillingRecipientName = kept.BillingRecipientName,
            BillingProvinceId = kept.BillingProvinceId,
            BillingDistrict = kept.BillingDistrict,
            BillingAddressLine = kept.BillingAddressLine,
            CouponCode = kept.CouponCode,
            PaymentMethod = kept.PaymentMethod,
        };
    }

    /// <summary>The fields the cookie keeps (05 6.7.9).</summary>
    public static CheckoutCookieValues CookieValues(PlaceOrderInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return new CheckoutCookieValues(
            input.ContactEmail,
            input.AddressBookId,
            input.ShippingRecipientName,
            input.ShippingProvinceId,
            input.ShippingDistrict,
            input.ShippingAddressLine,
            input.ShippingPhone,
            input.IsBillingSameAsShipping,
            input.BillingRecipientName,
            input.BillingProvinceId,
            input.BillingDistrict,
            input.BillingAddressLine,
            input.CouponCode,
            input.PaymentMethod);
    }

    /// <summary>The boxes the form sent.</summary>
    public static ConsentSent Boxes(PlaceOrderInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return new ConsentSent(
            ConsentSent.IsTicked(input.AcceptPreInformation),
            ConsentSent.IsTicked(input.AcceptDistanceSales),
            ConsentSent.IsTicked(input.AcceptDigital),
            ConsentSent.IsTicked(input.AcceptService));
    }

    /// <summary>The address part's request (07 UC-27; 04 7.1.5.2-7.1.5.6): a guest has no book entry.</summary>
    public static OrderAddressRequest AddressRequest(PlaceOrderInput input, Guid? customerId, IReadOnlyCollection<ProductType> itemTypes)
    {
        ArgumentNullException.ThrowIfNull(input);
        return new OrderAddressRequest(
            customerId,
            itemTypes,
            customerId is null ? null : input.AddressBookId,
            input.ShippingRecipientName,
            input.ShippingProvinceId,
            input.ShippingDistrict,
            input.ShippingAddressLine,
            input.ShippingPhone,
            customerId is not null && input.SaveToAddressBook,
            null,
            input.IsBillingSameAsShipping,
            input.BillingRecipientName,
            input.BillingProvinceId,
            input.BillingDistrict,
            input.BillingAddressLine);
    }
}

/// <summary>The payment step's own words (T509; 04 5.13).</summary>
public static class CheckoutTexts
{
    public const string ScreenName = "Ödeme";

    public const string ConfirmLabel = "Siparişi onayla — ödeme yükümlülüğü doğar";

    public const string BackToCart = "Sepete dön";

    /// <summary>The guest e-mail line's link to the e-mail field (02 3.24.5; D-729): the address is corrected before the confirmation.</summary>
    public const string CorrectEmail = "E-postayı düzelt";

    public const string SignInPrompt = "Hesabınız var mı? Giriş yapın";

    public const string RegisteredEmailReminder = "Bu e-posta kayıtlı, giriş yaparsanız adresleriniz dolu gelir.";

    public const string SignInLabel = "Giriş yap";

    public const string CardUnavailableNow = "şu an kullanılamıyor";

    public const string RecalculateLabel = "Özeti güncelle";

    /// <summary>The chosen method closed while the step was open (07 4.13.6).</summary>
    public const string MethodUnavailable = "Seçtiğiniz ödeme yöntemi şu an kullanılamıyor; başka bir yöntem seçin.";

    /// <summary>No method can be used: the cart's way back stands in the button's place (04 5.13.21).</summary>
    public const string NoMethod = "Şu anda kullanılabilecek bir ödeme yöntemi yok.";

    /// <summary>The head of the change lines (02 3.17.2).</summary>
    public const string SummaryChangedGeneral = "Siparişinizin özeti değişti; güncel özeti kontrol edip yeniden onaylayın.";

    /// <summary>A legal text's version rose while the step was open (02 3.24.1): the current text is confirmed again.</summary>
    public const string LegalTextChanged = "Ön Bilgilendirme Formu ya da Mesafeli Satış Sözleşmesi güncellendi; güncel metni okuyup yeniden onaylayın.";

    /// <summary>The change lines the page tells at the head of the confirmation section (02 3.17.2; 04 5.13.20): "Lambanın fiyatı 1.000 TL'den 1.200 TL'ye değişti".</summary>
    public static IReadOnlyList<string> ChangeLinesOf(IReadOnlyList<SummaryChange> changes)
    {
        ArgumentNullException.ThrowIfNull(changes);
        List<string> lines = [SummaryChangedGeneral];
        lines.AddRange(changes.Select(LineOf));
        return lines;
    }

    private static string LineOf(SummaryChange change) => change.Kind switch
    {
        SummaryChangeKind.ItemLeft => $"“{change.ProductName}” artık siparişe giremiyor.",
        SummaryChangeKind.ItemAdded => $"“{change.ProductName}” siparişe eklendi.",
        SummaryChangeKind.Quantity => $"“{change.ProductName}” ürününün adedi değişti: {change.Before:0} yerine {change.After:0}.",
        SummaryChangeKind.UnitPrice => $"“{change.ProductName}” ürününün fiyatı {Amount(change.Before)}'den {Amount(change.After)}'ye değişti.",
        SummaryChangeKind.Breakdown => $"“{change.ProductName}” ürününün tutarı {Amount(change.Before)}'den {Amount(change.After)}'ye değişti.",
        SummaryChangeKind.ShippingFee => $"Kargo ücreti {Amount(change.Before)}'den {Amount(change.After)}'ye değişti.",
        SummaryChangeKind.Total => $"Ödenecek toplam {Amount(change.Before)}'den {Amount(change.After)}'ye değişti.",
        SummaryChangeKind.ItemNotReserved => $"“{change.ProductName}” için yeterli stok ayrılamadı.",
        SummaryChangeKind.ShippingPeriod => "Kargoya veriliş süresi değişti.",
        SummaryChangeKind.DeliveryArea => "Teslimat yapılan iller değişti.",
        _ => SummaryChangedGeneral,
    };

    private static string Amount(decimal? value) => Infrastructure.DisplayFormat.Amount(value ?? 0m);
}
