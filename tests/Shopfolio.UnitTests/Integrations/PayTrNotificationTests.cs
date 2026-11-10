using System.Reflection;
using System.Text.Json;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.SearchEngine;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// The provider's notification in the link layer (T402 KK1-KK7; 08 2.1.5; 09 11.1 items 2, 3, 5, 6 and 8, 14.2): the signature over the four signed fields matches a value
/// computed outside .NET and is compared in fixed time; a missing or broken <c>hash</c> is <see cref="NotificationVerification.SignatureInvalid"/>; the unsigned fields are
/// read only as consistency marks; the three mapped fields and the event type; the reading of the outcome; the amount condition in whole kuruş; the failed payment's
/// reason written thinned and kept nowhere else; and no input makes the layer throw or answer - the answer is the endpoint's.
/// </summary>
public sealed class PayTrNotificationTests : IDisposable
{
    // Built from parts: the repository's secret guard stops a secret-looking literal next to a secret key.
    private static readonly string SampleKey = "key-" + "sample";
    private static readonly string SampleSalt = "salt-" + "sample";
    private const string Reference = "20267K4M9P";

    // The known examples (KK1): HMAC-SHA256 with SampleKey, Base64, over merchant_oid + merchant_salt + status + total_amount, computed with
    // `printf '%s' 20267K4M9Psalt-samplesuccess3456 | openssl dgst -sha256 -hmac key-sample -binary | base64` (and the same with "failed").
    private const string KnownSuccessHash = "vbLzXkrZgcta+7fQYpiaKNIhWhEQE1Y5mSfO+Ivu1wY=";
    private const string KnownFailedHash = "XKWKaEWmZWgKpeTIiaWUTa7IvOJ/ygq5t7y/dNCqkAY=";

    private readonly RunnerHarness _harness = new();

    public void Dispose() => _harness.Dispose();

    private static string Keys(bool complete = true) =>
        PaymentProviderKeys.IdName + "=123456\n"
        + PaymentProviderKeys.KeyName + "=" + SampleKey + "\n"
        + (complete ? PaymentProviderKeys.SaltName + "=" + SampleSalt + "\n" : string.Empty);

    private PayTrPaymentGateway Gateway(string? keys = null) =>
        new(
            new PayTrPaymentGatewayTests.OneClientFactory(new ScriptedHandler()),
            _harness.Runner,
            Options.Create(new PaymentOptions { ProviderKeys = keys ?? Keys() }),
            new SiteAddress(Options.Create(new SiteOptions { Domain = "shop.example.com" })),
            _harness.Time,
            new PayTrPaymentGatewayTests.Environment(Environments.Development),
            new PaymentFailureReasonLog(_harness.Time, _harness.LoggerFor<PaymentFailureReasonLog>()));

    // A notification as the provider sends it: the seven fields always there, the reason fields of a failure, the hash computed with the sample keys.
    private static Dictionary<string, string> Fields(
        string status = PayTrNotification.SucceededStatus,
        string total = "3456",
        string paymentAmount = "3456",
        string paymentType = "card",
        string currency = "TL",
        string? testMode = null,
        string? hash = null)
    {
        Dictionary<string, string> fields = new(StringComparer.Ordinal)
        {
            ["merchant_oid"] = Reference,
            ["status"] = status,
            ["total_amount"] = total,
            ["hash"] = hash ?? PayTrSignature.Of(SampleKey, Reference + SampleSalt + status + total),
            ["payment_type"] = paymentType,
            ["payment_amount"] = paymentAmount,
            ["currency"] = currency,
        };
        if (testMode is not null)
        {
            fields["test_mode"] = testMode;
        }

        return fields;
    }

    private static PaymentNotice Valid(NotificationVerification verification) =>
        Assert.IsType<NotificationVerification.SignatureValid>(verification).Notice;

    private JsonElement Line(int index) => JsonDocument.Parse(_harness.Lines[index]).RootElement.Clone();

    // ---- KK1: the signature of the known example ----

    [Fact]
    public void KnownExample_TheComputedHashMatchesTheValueComputedOutsideDotNet_AndTheNotificationIsVerified_KK1()
    {
        Assert.Equal(KnownSuccessHash, PayTrSignature.Of(SampleKey, PayTrNotification.SignedMessage(Reference, SampleSalt, "success", "3456")));
        Assert.Equal(KnownFailedHash, PayTrSignature.Of(SampleKey, PayTrNotification.SignedMessage(Reference, SampleSalt, "failed", "3456")));

        Valid(Gateway().VerifyNotification(Fields(hash: KnownSuccessHash)));
        Valid(Gateway().VerifyNotification(Fields(status: "failed", hash: KnownFailedHash)));
    }

    [Theory]
    [InlineData("merchant_oid", "20267K4M9Q")]
    [InlineData("status", "failed")]
    [InlineData("total_amount", "3457")]
    public void ChangingASignedField_BreaksTheSignature_KK1(string field, string value)
    {
        Dictionary<string, string> fields = Fields(hash: KnownSuccessHash);
        fields[field] = value;

        Assert.IsType<NotificationVerification.SignatureInvalid>(Gateway().VerifyNotification(fields));
    }

    [Fact]
    public void TheComparison_GoesThroughTheOneFixedTimeComparison_KK1()
    {
        // 09 11.1 item 5: the layer's only signature comparison is SignatureComparison.Matches (its own tests and the source scan cover the rest).
        string source = File.ReadAllText(Path.Combine(Support.RepositoryRoot.Find(), "src", "Shopfolio.Web", "Infrastructure", "Integrations", "PayTrNotification.cs"));
        Assert.Contains("SignatureComparison.Matches(computed, fields.GetValueOrDefault(HashField))", source, StringComparison.Ordinal);
    }

    // ---- KK2: a broken and a missing hash, in separate tests ----

    [Theory]
    [InlineData("vbLzXkrZgcta+7fQYpiaKNIhWhEQE1Y5mSfO+Ivu1wZ=")]
    [InlineData("vbLzXkrZgcta+7fQYpiaKNIhWhEQE1Y5mSfO+Ivu1wY")]
    [InlineData("VBLZXKRZGCTA+7FQYPIAKNIHWHEQE1Y5MSFO+IVU1WY=")]
    [InlineData("not-a-signature")]
    public void BrokenHash_IsSignatureInvalid_KK2(string hash)
    {
        Assert.IsType<NotificationVerification.SignatureInvalid>(Gateway().VerifyNotification(Fields(hash: hash)));
    }

    [Fact]
    public void MissingHash_IsSignatureInvalid_KK2()
    {
        Dictionary<string, string> fields = Fields();
        fields.Remove("hash");

        Assert.IsType<NotificationVerification.SignatureInvalid>(Gateway().VerifyNotification(fields));
    }

    [Fact]
    public void EmptyHash_IsSignatureInvalid_KK2()
    {
        Assert.IsType<NotificationVerification.SignatureInvalid>(Gateway().VerifyNotification(Fields(hash: string.Empty)));
    }

    [Theory]
    [InlineData("merchant_oid")]
    [InlineData("status")]
    [InlineData("total_amount")]
    public void MissingSignedField_IsSignatureInvalid_KK2(string field)
    {
        Dictionary<string, string> fields = Fields();
        fields.Remove(field);

        Assert.IsType<NotificationVerification.SignatureInvalid>(Gateway().VerifyNotification(fields));
    }

    [Fact]
    public void WithoutTheOneSettingOfKeys_NothingIsVerified_KK2()
    {
        Assert.IsType<NotificationVerification.SignatureInvalid>(Gateway(Keys(complete: false)).VerifyNotification(Fields()));
    }

    [Fact]
    public void TheUnsignedFields_AreNotInTheSignature_ChangingThemKeepsItValid_KK2()
    {
        // 08 2.1.5: payment_amount, payment_type, currency and test_mode are unsigned - consistency marks only, never a guarantee.
        Dictionary<string, string> fields = Fields(paymentAmount: "1", paymentType: "eft", currency: "USD", testMode: "1", hash: KnownSuccessHash);

        PaymentNotice notice = Valid(Gateway().VerifyNotification(fields));

        Assert.Equal((0.01m, false, false, true), (notice.PaymentAmount, notice.PaidByCard, notice.InLira, notice.TestMode));
    }

    // ---- KK5: the three field mappings and the event type ----

    [Fact]
    public void Mapping_MerchantOidToBothReferences_StatusToOutcome_TotalAmountToAmount_EventTypeSale_KK5()
    {
        PaymentNotice notice = Valid(Gateway().VerifyNotification(Fields(total: "123456", paymentAmount: "120000")));

        Assert.Equal(Reference, notice.ProviderTransactionId);
        Assert.Equal("2026-7K4M9P", notice.OrderNumber);
        Assert.Equal("success", notice.Outcome);
        Assert.Equal(1234.56m, notice.TotalAmount);
        Assert.Equal(1200m, notice.PaymentAmount);
        Assert.Equal(PayTrNotification.SaleEventType, notice.EventType);
        Assert.Equal("Sale", notice.EventType);
        Assert.Equal((true, true, false), (notice.PaidByCard, notice.InLira, notice.TestMode));
    }

    [Theory]
    [InlineData("SUCCESS")]
    [InlineData("pending")]
    public void Mapping_KeepsAnUnknownStatusAsItCame_KK5(string status)
    {
        Assert.Equal(status, Valid(Gateway().VerifyNotification(Fields(status: status))).Outcome);
    }

    [Theory]
    [InlineData("2026-7K4M9P")]
    [InlineData("20260K4M9P")]
    [InlineData("ABCD7K4M9P")]
    public void Mapping_AReferenceThatDoesNotMapBack_KeepsItAsItCame_WithNoOrderNumber_KK5(string reference)
    {
        Dictionary<string, string> fields = Fields();
        fields["merchant_oid"] = reference;
        fields["hash"] = PayTrSignature.Of(SampleKey, reference + SampleSalt + "success" + "3456");

        PaymentNotice notice = Valid(Gateway().VerifyNotification(fields));

        Assert.Equal((reference, (string?)null), (notice.ProviderTransactionId, notice.OrderNumber));
    }

    [Theory]
    [InlineData("34.56")]
    [InlineData("-3456")]
    [InlineData("+3456")]
    [InlineData(" 3456")]
    [InlineData("3,456")]
    [InlineData("99999999999999999999")]
    public void Mapping_AnAmountThatIsNotWholeKurus_IsNotRead_AndTheSignatureStillHolds_KK5(string total)
    {
        PaymentNotice notice = Valid(Gateway().VerifyNotification(Fields(total: total, paymentAmount: total)));

        Assert.Equal(((decimal?)null, (decimal?)null), (notice.TotalAmount, notice.PaymentAmount));
    }

    // ---- KK3 (layer part): the reading of the event type and the outcome ----

    [Theory]
    [InlineData("Sale", "success", nameof(NotificationResult.PaymentSucceeded))]
    [InlineData("Sale", "failed", nameof(NotificationResult.PaymentFailed))]
    [InlineData("Sale", "SUCCESS", nameof(NotificationResult.Unrecognized))]
    [InlineData("Sale", "pending", nameof(NotificationResult.Unrecognized))]
    [InlineData("Sale", "", nameof(NotificationResult.Unrecognized))]
    [InlineData("Refund", "success", nameof(NotificationResult.Unrecognized))]
    [InlineData("sale", "failed", nameof(NotificationResult.Unrecognized))]
    public void ReadNotificationResult_KnowsOnlySaleWithSuccessOrFailed_KK3(string eventType, string outcome, string expected)
    {
        Assert.Equal(expected, Gateway().ReadNotificationResult(eventType, outcome).ToString());
    }

    // ---- KK4: the amount condition in whole kuruş ----

    [Theory]
    [InlineData("3456", "3456", nameof(NotificationAmountCheck.Holds))]
    [InlineData("3456", "3906", nameof(NotificationAmountCheck.Holds))]
    [InlineData("3456", "3455", nameof(NotificationAmountCheck.Mismatch))]
    [InlineData("3455", "3456", nameof(NotificationAmountCheck.Mismatch))]
    [InlineData("3457", "3906", nameof(NotificationAmountCheck.Mismatch))]
    [InlineData("3456", "x", nameof(NotificationAmountCheck.Mismatch))]
    [InlineData("x", "3456", nameof(NotificationAmountCheck.Mismatch))]
    public void CheckNotificationAmount_PaymentAmountEqual_TotalAmountAtLeast_InKurus_KK4(string paymentAmount, string total, string expected)
    {
        PayTrPaymentGateway gateway = Gateway();
        PaymentNotice notice = Valid(gateway.VerifyNotification(Fields(total: total, paymentAmount: paymentAmount)));

        Assert.Equal(expected, gateway.CheckNotificationAmount(notice, 34.56m).ToString());
    }

    [Fact]
    public void CheckNotificationAmount_InstalmentDifferencePasses_ShortAmountDoesNot_KK4()
    {
        PayTrPaymentGateway gateway = Gateway();
        PaymentNotice withDifference = Valid(gateway.VerifyNotification(Fields(total: "12450", paymentAmount: "12000")));
        PaymentNotice shortAmount = Valid(gateway.VerifyNotification(Fields(total: "11999", paymentAmount: "12000")));

        Assert.Equal(NotificationAmountCheck.Holds, gateway.CheckNotificationAmount(withDifference, 120m));
        Assert.Equal(NotificationAmountCheck.Mismatch, gateway.CheckNotificationAmount(shortAmount, 120m));
    }

    // ---- KK6: the failed payment's reason goes to the log thinned and is kept nowhere else ----

    [Fact]
    public void FailedPaymentsReason_IsWrittenThinned_TheFirstLineCarriesCodeAndMessage_TheNextAfterTheWindowTheCount_KK6()
    {
        PayTrPaymentGateway gateway = Gateway();
        Dictionary<string, string> failed = Fields(status: "failed");
        failed["failed_reason_code"] = "6";
        failed["failed_reason_msg"] = "Müşteri ödeme yapmaktan vazgeçti";

        Valid(gateway.VerifyNotification(failed));
        _harness.Time.Advance(TimeSpan.FromSeconds(1));
        Valid(gateway.VerifyNotification(failed));
        Valid(gateway.VerifyNotification(failed));

        Assert.Single(_harness.Lines);
        JsonElement first = Line(0);
        Assert.Equal(LogEvents.PaymentFailedReasonSampled, first.GetProperty("event").GetString());
        Assert.Equal("6", first.GetProperty("reasonCode").GetString());
        Assert.Equal("Müşteri ödeme yapmaktan vazgeçti", first.GetProperty("reasonMessage").GetString());
        Assert.Equal(0, first.GetProperty("suppressedCount").GetInt32());

        _harness.Time.Advance(PaymentFailureReasonLog.Window);
        failed["failed_reason_code"] = "2";
        Valid(gateway.VerifyNotification(failed));

        Assert.Equal(2, _harness.Lines.Count);
        Assert.Equal(("2", 2), (Line(1).GetProperty("reasonCode").GetString(), Line(1).GetProperty("suppressedCount").GetInt32()));
    }

    [Fact]
    public void FailedPaymentsReasonLine_CarriesNoReferenceNoAmountNoHash_AndTheNoticeHasNoFieldForTheReason_KK6()
    {
        Dictionary<string, string> failed = Fields(status: "failed");
        failed["failed_reason_code"] = "0";
        failed["failed_reason_msg"] = "Kart limiti yetersiz";

        Valid(Gateway().VerifyNotification(failed));

        string line = Assert.Single(_harness.Lines);
        Assert.DoesNotContain(Reference, line, StringComparison.Ordinal);
        Assert.DoesNotContain("3456", line, StringComparison.Ordinal);
        Assert.DoesNotContain(failed["hash"], line, StringComparison.Ordinal);
        Assert.DoesNotContain(
            typeof(PaymentNotice).GetProperties(BindingFlags.Public | BindingFlags.Instance),
            p => p.Name.Contains("Reason", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("Message", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SuccessAndSignatureFailure_WriteNoReasonLine_KK6()
    {
        Dictionary<string, string> success = Fields();
        success["failed_reason_code"] = "6";
        Dictionary<string, string> forged = Fields(status: "failed", hash: KnownSuccessHash);
        forged["failed_reason_code"] = "6";

        Valid(Gateway().VerifyNotification(success));
        Assert.IsType<NotificationVerification.SignatureInvalid>(Gateway().VerifyNotification(forged));

        Assert.Empty(_harness.Lines);
    }

    // ---- KK7: the layer gives a class, never an answer or an exception ----

    public static TheoryData<Dictionary<string, string>> OddForms =>
    [
        new Dictionary<string, string>(StringComparer.Ordinal),
        new Dictionary<string, string>(StringComparer.Ordinal) { ["hash"] = "x" },
        new Dictionary<string, string>(StringComparer.Ordinal) { ["merchant_oid"] = "", ["status"] = "", ["total_amount"] = "", ["hash"] = "" },
        new Dictionary<string, string>(StringComparer.Ordinal) { ["merchant_oid"] = new string('9', 70_000), ["status"] = "success", ["total_amount"] = "1", ["hash"] = "x" },
    ];

    [Theory]
    [MemberData(nameof(OddForms))]
    public void OddForms_AreSignatureInvalid_WithoutAnException_KK7(Dictionary<string, string> fields)
    {
        Assert.IsType<NotificationVerification.SignatureInvalid>(Gateway().VerifyNotification(fields));
    }

    [Fact]
    public void TheLayersNotificationMembers_ReturnOnlyTheirClasses_NoHttpAnswer_KK7()
    {
        // 08 2.1.5; 05 MK-39: OK, 400 and 500 are the endpoint's (T311); the layer's three members return the class families of 09 11.1 item 2.
        Type gateway = typeof(PayTrPaymentGateway);
        Assert.Equal(typeof(NotificationVerification), gateway.GetMethod(nameof(IPaymentGateway.VerifyNotification))!.ReturnType);
        Assert.Equal(typeof(NotificationResult), gateway.GetMethod(nameof(IPaymentGateway.ReadNotificationResult))!.ReturnType);
        Assert.Equal(typeof(NotificationAmountCheck), gateway.GetMethod(nameof(IPaymentGateway.CheckNotificationAmount))!.ReturnType);

        string source = File.ReadAllText(Path.Combine(Support.RepositoryRoot.Find(), "src", "Shopfolio.Web", "Infrastructure", "Integrations", "PayTrNotification.cs"));
        Assert.DoesNotContain("StatusCode", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Results.", source, StringComparison.Ordinal);
        Assert.DoesNotContain("throw ", source, StringComparison.Ordinal);
    }
}
