using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The form of the PayTR token request (08 2.1.4, 2.1 "Kimlik doğrulama"; 09 11.1 item 3): a pure function from the product's start request to the provider's fields, and
/// the <c>paytr_token</c> over the request's eleven inputs. Every value goes into the signature as the same text the request sends (<c>user_basket</c> as its Base64 text,
/// <c>payment_amount</c> as whole kuruş text). <c>merchant_key</c> and <c>merchant_salt</c> are not fields of the form: they only sign it.
/// </summary>
internal static class PayTrStartForm
{
    /// <summary>The token request's address: server to server, form-encoded (08 2.1.4).</summary>
    public static readonly Uri TokenAddress = new("https://www.paytr.com/odeme/api/get-token");

    /// <summary>The frame's address root: the payment page opens <c>{root}{token}</c> in its frame (08 2.1.4).</summary>
    public const string FrameAddressRoot = "https://www.paytr.com/odeme/guvenli/";

    /// <summary>The fixed currency (08 2.1.4 <c>currency</c>).</summary>
    public const string Currency = "TL";

    /// <summary>No instalment limit is set by the product: the instalments are the provider's form (02 3.21.9; 08 2.1.4).</summary>
    public const string ProviderFormDefault = "0";

    /// <summary>The form's language (08 2.1.4 <c>lang</c>).</summary>
    public const string Language = "tr";

    /// <summary>The provider's limit on <c>email</c>: at most 100 characters, ASCII only (08 2.1.4); a value outside it is replaced by the firm's e-mail.</summary>
    public const int EmailMaxLength = 100;

    /// <summary>The provider's limit on <c>user_name</c> (08 2.1.4): a longer value is cut at this character.</summary>
    public const int NameMaxLength = 60;

    /// <summary>The provider's limit on <c>user_address</c> (08 2.1.4): a longer value is cut at this character.</summary>
    public const int AddressMaxLength = 400;

    /// <summary>The provider's limit on <c>user_ip</c> (08 2.1.4): 39 characters, a full IPv6 address.</summary>
    public const int ClientAddressMaxLength = 39;

    /// <summary>The prefix the stored ten digits of a phone take (08 2.1.4 <c>user_phone</c>: <c>5321234567</c> → <c>05321234567</c>).</summary>
    public const string PhonePrefix = "0";

    /// <summary>The return address's path root (UC-30; K-1185): both return addresses are <c>/odeme/donus/{access key}</c>.</summary>
    public const string ReturnPathRoot = "/odeme/donus/";

    // The basket's JSON keeps a letter outside ASCII ("ş" of "Sipariş") as its UTF-8 bytes, not as a \u escape (08 2.1.4: the JSON's UTF-8 bytes; DV-14). The
    // encoder still escapes the characters that end a string or open markup.
    private static readonly JsonWriterOptions BasketWriting = new() { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };

    /// <summary>
    /// The fields of the token request, in the provider's names, and its signature. <paramref name="clientAddress"/> is the address already in the provider's form
    /// (<see cref="TryClientAddress"/>); <paramref name="returnAddress"/> is the full return address; <paramref name="testMode"/> is <c>1</c> outside the live installation.
    /// </summary>
    public static IReadOnlyList<KeyValuePair<string, string>> Build(
        PaymentStartRequest request, PaymentProviderKeys keys, string clientAddress, string returnAddress, DateTimeOffset now, bool testMode)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(keys);

        string merchantOid = OrderNumber.WithoutSeparator(request.OrderNumber);
        string email = Email(request.ContactEmail, request.CompanyEmail);
        string paymentAmount = PaymentAmounts.ToKurus(request.Total).ToString(CultureInfo.InvariantCulture);
        string userBasket = Basket(request.OrderNumber, request.Total);
        string test = testMode ? "1" : "0";
        string signature = PayTrSignature.Of(
            keys.MerchantKey,
            keys.MerchantId + clientAddress + merchantOid + email + paymentAmount + userBasket + ProviderFormDefault + ProviderFormDefault + Currency + test + keys.MerchantSalt);

        return
        [
            new("merchant_id", keys.MerchantId),
            new("user_ip", clientAddress),
            new("merchant_oid", merchantOid),
            new("email", email),
            new("payment_amount", paymentAmount),
            new("paytr_token", signature),
            new("user_basket", userBasket),
            new("no_installment", ProviderFormDefault),
            new("max_installment", ProviderFormDefault),
            new("user_name", Cut(request.BillingRecipientName, NameMaxLength)),
            new("user_address", Cut(request.BillingAddressLine, AddressMaxLength)),
            new("user_phone", Phone(request.ShippingPhone, request.CompanyPhone)),
            new("merchant_ok_url", returnAddress),
            new("merchant_fail_url", returnAddress),
            new("timeout_limit", TimeoutMinutes(request.PaymentDeadline, now).ToString(CultureInfo.InvariantCulture)),
            new("currency", Currency),
            new("test_mode", test),
            new("lang", Language),
        ];
    }

    /// <summary>The return address's path for an order's access key (08 2.1.4 <c>merchant_ok_url</c>, <c>merchant_fail_url</c>).</summary>
    public static string ReturnPath(string orderAccessKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderAccessKey);
        return ReturnPathRoot + Uri.EscapeDataString(orderAccessKey);
    }

    /// <summary>
    /// <c>user_basket</c> (08 2.1.4): one item "Sipariş {number}", the price as plain text (<see cref="PaymentAmounts.ToPlainText"/>), quantity 1 - the JSON array
    /// <c>[["Sipariş 2026-7K4M9P","34.56",1]]</c>, its UTF-8 bytes in Base64. The item list does not go to the provider (02 3.21.9).
    /// </summary>
    public static string Basket(string orderNumber, decimal total)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderNumber);
        using MemoryStream buffer = new();
        using (Utf8JsonWriter writer = new(buffer, BasketWriting))
        {
            writer.WriteStartArray();
            writer.WriteStartArray();
            writer.WriteStringValue("Sipariş " + orderNumber);
            writer.WriteStringValue(PaymentAmounts.ToPlainText(total));
            writer.WriteNumberValue(1);
            writer.WriteEndArray();
            writer.WriteEndArray();
        }

        return Convert.ToBase64String(buffer.ToArray());
    }

    /// <summary>
    /// <c>email</c> (08 2.1.4; K-1261): the contact e-mail as it was entered; one longer than <see cref="EmailMaxLength"/> or with a character outside ASCII is neither
    /// converted nor cut - the order's frozen firm e-mail goes in its place. The order's record does not change.
    /// </summary>
    public static string Email(string contactEmail, string companyEmail)
    {
        ArgumentNullException.ThrowIfNull(contactEmail);
        ArgumentNullException.ThrowIfNull(companyEmail);
        return contactEmail.Length <= EmailMaxLength && Ascii.IsValid(contactEmail) ? contactEmail : companyEmail;
    }

    /// <summary>
    /// <c>user_phone</c> (08 2.1.4; K-1261): the shipping phone's ten digits with <see cref="PhonePrefix"/> before them; an order without a physical item has no shipping
    /// phone and the order's frozen firm phone goes in its place.
    /// </summary>
    public static string Phone(string? shippingPhone, string companyPhone)
    {
        ArgumentNullException.ThrowIfNull(companyPhone);
        return PhonePrefix + (string.IsNullOrWhiteSpace(shippingPhone) ? companyPhone : shippingPhone);
    }

    /// <summary>A value cut at <paramref name="maxLength"/> characters for the provider (08 2.1.4: the order's record does not change); a surrogate pair is not split.</summary>
    public static string Cut(string value, int maxLength)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Length <= maxLength)
        {
            return value;
        }

        int length = char.IsHighSurrogate(value[maxLength - 1]) ? maxLength - 1 : maxLength;
        return value[..length];
    }

    /// <summary>
    /// <c>user_ip</c> (08 2.1.4): the request's client address as it is - a full IPv6 address, an IPv4 address mapped into IPv6 (<c>::ffff:</c>) turned back into IPv4,
    /// no zone. False when the text is no address or its form is longer than <see cref="ClientAddressMaxLength"/>: a code error, the call is not made.
    /// </summary>
    public static bool TryClientAddress(string? text, out string address)
    {
        address = string.Empty;
        // The parser also takes the short IPv4 forms ("203.0.113" is 203.0.0.113); a request's address is never one, so only the four-part form is an address here.
        if (!IPAddress.TryParse(text, out IPAddress? parsed)
            || (parsed.AddressFamily == AddressFamily.InterNetwork && text!.Count(c => c == '.') != 3))
        {
            return false;
        }

        if (parsed.IsIPv4MappedToIPv6)
        {
            parsed = parsed.MapToIPv4();
        }
        else if (parsed.AddressFamily == AddressFamily.InterNetworkV6 && parsed.ScopeId != 0)
        {
            parsed = new IPAddress(parsed.GetAddressBytes());
        }

        string form = parsed.ToString();
        if (form.Length > ClientAddressMaxLength)
        {
            return false;
        }

        address = form;
        return true;
    }

    /// <summary>
    /// <c>timeout_limit</c> (08 2.1.4): the minutes left to the end of the order's payment period, rounded up, at least 1 - never the whole period (each token opens a new
    /// window at the provider) and never 0 (0 is "no limit" for the provider).
    /// </summary>
    public static long TimeoutMinutes(DateTimeOffset paymentDeadline, DateTimeOffset now)
    {
        long left = (paymentDeadline - now).Ticks;
        if (left <= 0)
        {
            return 1;
        }

        long minutes = (left + TimeSpan.TicksPerMinute - 1) / TimeSpan.TicksPerMinute;
        return Math.Max(1, minutes);
    }
}
