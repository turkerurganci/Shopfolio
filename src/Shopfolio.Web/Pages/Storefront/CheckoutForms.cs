using System.Buffers.Text;
using System.Text;
using System.Text.Json;

using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Pages.Storefront;

/// <summary>
/// What the payment step keeps in its cookie (T509; 05 6.7.9; 07 UC-26): the contact e-mail, the shipping and the billing address fields with the "same" box, the telephone, the coupon code and the
/// payment method - and nothing else. The boxes, the confirmation key, the summary's fingerprint and the text versions are not kept: every showing makes them again (04 5.13.20; K-756). The member's
/// chosen book entry is the shipping address's field the member filled (04 7.1.5.2).
/// </summary>
public sealed record CheckoutCookieValues(
    string? ContactEmail,
    Guid? AddressBookId,
    string? ShippingRecipientName,
    Guid? ShippingProvinceId,
    string? ShippingDistrict,
    string? ShippingAddressLine,
    string? ShippingPhone,
    bool IsBillingSameAsShipping,
    string? BillingRecipientName,
    Guid? BillingProvinceId,
    string? BillingDistrict,
    string? BillingAddressLine,
    string? CouponCode,
    PaymentMethod? PaymentMethod);

/// <summary>
/// The payment step's two carried values (T509): the cookie's payload and the summary's fingerprint. The fingerprint is the summary the form showed - the lines with their VAT rates, the shipping fee,
/// the total, the method and the period - written as JSON and base64url (05 4.2.1); the confirmation compares it with the summary it makes again under the cart's lock. The server never trusts it
/// as an amount: a changed or broken value only makes a summary difference (02 3.17.2).
/// </summary>
public static class CheckoutForms
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.General);

    /// <summary>The cookie's payload: only the fields of 05 6.7.9.</summary>
    public static string CookiePayload(CheckoutCookieValues values)
    {
        ArgumentNullException.ThrowIfNull(values);
        return JsonSerializer.Serialize(values, Json);
    }

    /// <summary>The cookie's values, or null when the payload is not this record's (an old or foreign value is ignored and the fields stay empty).</summary>
    public static CheckoutCookieValues? ReadCookiePayload(string? payload)
    {
        if (string.IsNullOrEmpty(payload))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<CheckoutCookieValues>(payload, Json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>The fingerprint of the summary the form shows.</summary>
    public static string Fingerprint(ConfirmationSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);
        ConfirmationSummary shown = summary with { ContactEmail = null, Shipping = null, Billing = null };
        return Base64Url.EncodeToString(JsonSerializer.SerializeToUtf8Bytes(shown, Json));
    }

    /// <summary>The summary a fingerprint carries, or null when the value is broken or empty.</summary>
    public static ConfirmationSummary? ReadFingerprint(string? fingerprint)
    {
        if (string.IsNullOrEmpty(fingerprint))
        {
            return null;
        }

        try
        {
            byte[] bytes = Base64Url.DecodeFromChars(fingerprint);
            ConfirmationSummary? summary = JsonSerializer.Deserialize<ConfirmationSummary>(Encoding.UTF8.GetString(bytes), Json);
            return summary?.Lines is null ? null : summary;
        }
        catch (FormatException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
