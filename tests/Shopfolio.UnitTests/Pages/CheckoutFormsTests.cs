using System.Buffers.Text;
using System.Text;

using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Pages.Storefront;

namespace Shopfolio.UnitTests.Pages;

/// <summary>
/// The payment step's two carried values (T509; 05 6.7.9; 04 5.13.20): the cookie keeps only the fields 05 6.7.9 lists and reads a foreign value as empty; the fingerprint carries the shown summary
/// without the e-mail and the addresses and reads a broken value as no summary.
/// </summary>
public sealed class CheckoutFormsTests
{
    /// <summary>05 6.7.9: the contact e-mail, the shipping and the billing address fields with the "same" box, the telephone, the coupon code and the payment method (the book entry is the shipping address's field).</summary>
    private static readonly string[] DocumentedCookieFields =
    [
        "ContactEmail", "AddressBookId", "ShippingRecipientName", "ShippingProvinceId", "ShippingDistrict", "ShippingAddressLine", "ShippingPhone", "IsBillingSameAsShipping",
        "BillingRecipientName", "BillingProvinceId", "BillingDistrict", "BillingAddressLine", "CouponCode", "PaymentMethod",
    ];

    private static readonly CheckoutCookieValues Values = new(
        "ali@example.com", Guid.CreateVersion7(), "Ali Veli", Guid.CreateVersion7(), "Çankaya", "Kızılay Mah. 1", "5321234567", false, "Ayşe Veli", Guid.CreateVersion7(), "Kadıköy",
        "Moda Cad. 2", "YAZ10", PaymentMethod.BankTransfer);

    [Fact]
    public void TheCookie_KeepsOnlyTheDocumentedFields_AndNoBoxKeyFingerprintOrVersion()
    {
        string[] fields = [.. typeof(CheckoutCookieValues).GetProperties().Select(p => p.Name).Where(n => n != "EqualityContract").Order(StringComparer.Ordinal)];
        Assert.Equal(DocumentedCookieFields.Order(StringComparer.Ordinal), fields);
        Assert.DoesNotContain(fields, f => f.StartsWith("Accept", StringComparison.Ordinal) || f.Contains("Key", StringComparison.Ordinal)
            || f.Contains("Fingerprint", StringComparison.Ordinal) || f.Contains("Version", StringComparison.Ordinal));
    }

    [Fact]
    public void TheCookiePayload_RoundTrips_AndAForeignOrBrokenValueReadsAsEmpty()
    {
        Assert.Equal(Values, CheckoutForms.ReadCookiePayload(CheckoutForms.CookiePayload(Values)));
        Assert.Null(CheckoutForms.ReadCookiePayload(null));
        Assert.Null(CheckoutForms.ReadCookiePayload(string.Empty));
        Assert.Null(CheckoutForms.ReadCookiePayload("{not json"));
        Assert.Null(CheckoutForms.ReadCookiePayload("{\"PaymentMethod\":\"Barter\"}"));
    }

    [Fact]
    public void TheFingerprint_CarriesTheShownSummary_WithoutTheEmailAndTheAddresses()
    {
        ConfirmationSummaryLine line = new("Lamba", "Kırmızı", null, ProductType.Physical, 2, 150m, 200m, 25m, 0m, 20m, 50m, 300m, WithdrawalExemptionReason.Perishable, null);
        PartyAddress address = new("Ali Veli", "5321234567", Guid.CreateVersion7(), "Çankaya", "Kızılay Mah. 1");
        ConfirmationSummary summary = new([line], 49.9m, 349.9m, PaymentMethod.Card, 3, "ali@example.com", address, address);

        string fingerprint = CheckoutForms.Fingerprint(summary);
        ConfirmationSummary? read = CheckoutForms.ReadFingerprint(fingerprint);

        Assert.NotNull(read);
        Assert.Equal([line], read.Lines);
        Assert.Equal(summary with { Lines = read.Lines, ContactEmail = null, Shipping = null, Billing = null }, read);
        Assert.DoesNotContain("ali@example.com", Encoding.UTF8.GetString(Base64Url.DecodeFromChars(fingerprint)), StringComparison.Ordinal);
        Assert.DoesNotContain("Kızılay", Encoding.UTF8.GetString(Base64Url.DecodeFromChars(fingerprint)), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not*base64")]
    [InlineData("e30")]
    [InlineData("bm90IGpzb24")]
    public void ABrokenFingerprint_ReadsAsNoSummary(string? fingerprint)
    {
        Assert.Null(CheckoutForms.ReadFingerprint(fingerprint));
    }
}
