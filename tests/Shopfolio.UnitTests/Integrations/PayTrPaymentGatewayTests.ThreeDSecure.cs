using System.Reflection;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Integrations;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// 3D Secure in the link layer (T406 KK1, KK2; 08 2.1.3; 02 3.21.3): the product has no 3D mark to check - the start form sends no field that could turn 3D Secure off
/// (the iFrame token request has none; <c>non_3d</c> is the Direct API's), no field name of the layer names a 3D field, and a 3D-looking field in a notification changes
/// neither the signature's result nor the notice. A failed 3D verification (<c>failed_reason_code</c> 1, 2 or 10) reads as a failed payment whatever the code; a
/// payment type other than <c>card</c>, as it came (ordinal), is not a card payment. The guarantee that every card payment goes through 3D Secure is the installation
/// condition and the live proof (DEPLOY_RUNBOOK N.1, N.3; DV-3), not a field.
/// </summary>
public sealed partial class PayTrPaymentGatewayTests
{
    private const string NoticeReference = "20267K4M9P";

    [Fact]
    public async Task Start_SendsNo3DField_TheFormCannotTurn3DSecureOff_T406_KK1()
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent());

        await gateway.StartAsync(Request(), Ct);

        Assert.DoesNotContain(Fields(Assert.Single(handler.Sent).Body).Keys, name => Names3D(name));
    }

    [Fact]
    public void TheLayersFieldNames_NameNo3DField_T406_KK1()
    {
        List<string> values = [.. typeof(PayTrPaymentGateway).Assembly.GetTypes()
            .Where(type => type.Namespace == typeof(PayTrPaymentGateway).Namespace && type.Name.StartsWith("PayTr", StringComparison.Ordinal))
            .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            .Where(field => field.IsLiteral && field.FieldType == typeof(string))
            .Select(field => (string)field.GetRawConstantValue()!)];

        Assert.Contains(PayTrNotification.PaymentTypeField, values);
        Assert.DoesNotContain(values, Names3D);
    }

    // The reflection above sees the layer's constants only; this sees every string in the application's source, wherever it is written (an indexer, a
    // verbatim or raw literal, an interpolated string), so a 3D field read inline in any type is found too.
    [Fact]
    public void TheApplicationsSource_HasNoStringNamingA3DField_T406_KK1()
    {
        string source = Path.Combine(RepositoryRoot.Find(), "src");

        List<string> found = [.. Directory.EnumerateFiles(source, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => FieldLiteral3D().IsMatch(File.ReadAllText(path)))
            .Select(path => Path.GetRelativePath(source, path))];

        Assert.Empty(found);
    }

    [Theory]
    [InlineData("fields[\"non_3d\"]")]
    [InlineData("public const string Mark = \"3D_SECURE\";")]
    [InlineData("Text(root, @\"threeds_status\")")]
    [InlineData("string raw = \"\"\"3ds\"\"\";")]
    [InlineData("fields.TryGetValue(\"three_d\" + \"\", out _) || fields.ContainsKey(\"Is3D\")")]
    [InlineData("$\"{prefix}_3d\"")]
    public void SourceScan_OnViolationSample_FindsTheString(string sample)
    {
        Assert.Matches(FieldLiteral3D(), sample);
    }

    [Theory]
    [InlineData("// non_3d is the Direct API's field, the iFrame API has none")]
    [InlineData("string text = \"3D Secure doğrulaması tamamlanmadı\";")]
    [InlineData("const string Color = \"#3d4f5a\";")]
    [InlineData("int threeDays = 3;")]
    public void SourceScan_OnCleanSample_FindsNothing(string sample)
    {
        Assert.DoesNotMatch(FieldLiteral3D(), sample);
    }

    [Theory]
    [InlineData("non_3d", "1")]
    [InlineData("non_3d", "0")]
    [InlineData("3d_secure", "0")]
    [InlineData("threeds_status", "failed")]
    public void Notification_AFieldThatLooksLikeA3DMark_ChangesNeitherTheSignatureNorTheNotice_T406_KK1(string field, string value)
    {
        Dictionary<string, string> plain = Notice(PayTrNotification.SucceededStatus, "3456");
        Dictionary<string, string> with3D = new(plain, StringComparer.Ordinal) { [field] = value };

        Assert.Equal(VerifiedNotice(plain), VerifiedNotice(with3D));
    }

    [Theory]
    [InlineData("1")]
    [InlineData("2")]
    [InlineData("10")]
    public void Notification_AFailed3DVerification_ReadsAsAFailedPayment_WhateverTheCode_T406_KK1(string reasonCode)
    {
        Dictionary<string, string> fields = Notice(PayTrNotification.FailedStatus, "0");
        fields[PayTrNotification.FailedReasonCodeField] = reasonCode;

        PaymentNotice notice = VerifiedNotice(fields);

        Assert.Equal(NotificationResult.PaymentFailed, PayTrNotification.ReadResult(notice.EventType, notice.Outcome));
        Assert.Equal(VerifiedNotice(Notice(PayTrNotification.FailedStatus, "0")), notice);
    }

    [Theory]
    [InlineData("card", true)]
    [InlineData("eft", false)]
    [InlineData("CARD", false)]
    [InlineData("Card", false)]
    [InlineData(" card", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void Notification_OnlyThePaymentTypeCard_IsACardPayment_AsItCame_T406_KK2(string? paymentType, bool expected)
    {
        Dictionary<string, string> fields = Notice(PayTrNotification.SucceededStatus, "3456");
        if (paymentType is null)
        {
            fields.Remove(PayTrNotification.PaymentTypeField);
        }
        else
        {
            fields[PayTrNotification.PaymentTypeField] = paymentType;
        }

        Assert.Equal(expected, VerifiedNotice(fields).PaidByCard);
    }

    // A 3D field name: "3d" anywhere, whatever the case (non_3d, 3d_secure, 3D), or the "threeds"/"3ds" spelling.
    private static bool Names3D(string name) =>
        name.Contains("3d", StringComparison.OrdinalIgnoreCase) || name.Contains("threeds", StringComparison.OrdinalIgnoreCase);

    // A string that is one field-like token (no space) naming 3D: "3d"/"3ds" not inside a hex run, "three_d", "threeds", whatever the case. A sentence ("3D Secure
    // ...") is text, not a field name; a colour ("#3d4f5a") is not a token of this kind.
    [GeneratedRegex(
        """
        "[{}$A-Za-z0-9_.\-]*(?:(?<![0-9a-f])3ds?(?![0-9a-f])|three_?ds?)[{}A-Za-z0-9_.\-]*"
        """,
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex FieldLiteral3D();

    private static Dictionary<string, string> Notice(string status, string total) =>
        new(StringComparer.Ordinal)
        {
            [PayTrNotification.OrderReferenceField] = NoticeReference,
            [PayTrNotification.StatusField] = status,
            [PayTrNotification.TotalAmountField] = total,
            [PayTrNotification.HashField] = PayTrSignature.Of(SampleKey, PayTrNotification.SignedMessage(NoticeReference, SampleSalt, status, total)),
            [PayTrNotification.PaymentTypeField] = PayTrNotification.CardPaymentType,
            [PayTrNotification.PaymentAmountField] = "3456",
            [PayTrNotification.CurrencyField] = PayTrStartForm.Currency,
        };

    private static PaymentNotice VerifiedNotice(Dictionary<string, string> fields) =>
        Assert.IsType<NotificationVerification.SignatureValid>(PayTrNotification.Verify(fields, PaymentProviderKeys.TryParse(KeysContent()))).Notice;
}
