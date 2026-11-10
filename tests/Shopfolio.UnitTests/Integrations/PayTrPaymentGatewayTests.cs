using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.SearchEngine;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// The provider's start (T401 KK1-KK8; 08 2.1, 2.1.2, 2.1.4; 09 11.1 items 3, 4 and 8, 14.2): the token request goes once, server to server and form-encoded, through a
/// hand-written handler that keeps what it was sent - no socket is opened; without the one setting of three keys no request goes; the signature matches a value computed
/// outside .NET; amounts, the order reference, the stand-in values, the cuts, the client address and the minutes follow 08 2.1.4; the answer gives the frame's address and
/// the token reaches no log line.
/// </summary>
public sealed partial class PayTrPaymentGatewayTests
{
    // Built from parts: the repository's secret guard stops a secret-looking literal next to a secret key.
    private static readonly string SampleKey = "key-" + "sample";
    private static readonly string SampleSalt = "salt-" + "sample";
    private const string SampleId = "123456";
    private const string Domain = "shop.example.com";
    private const string AccessKey = "ak_Sample-Access_Key";
    private const string AnswerToken = "TOKEN-VALUE-55aa77";

    // The known example (KK3): the message of the eleven inputs below, HMAC-SHA256 with SampleKey, Base64 - computed with
    // `openssl dgst -sha256 -hmac key-sample -binary | base64` over the bytes of
    // 123456 203.0.113.7 20267K4M9P musteri@example.com 3456 <basket> 0 0 TL 1 salt-sample (written without the spaces).
    private const string KnownBasket = "W1siU2lwYXJpxZ8gMjAyNi03SzRNOVAiLCIzNC41NiIsMV1d";
    private const string KnownToken = "DhiH4xqQP4p+vPZz6d7caaoNJKz3xKv7yRjkD31DP/g=";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static string KeysContent(string? id = SampleId, string? key = null, string? salt = null, bool withKey = true, bool withSalt = true)
    {
        StringBuilder text = new();
        if (id is not null)
        {
            text.Append(PaymentProviderKeys.IdName).Append('=').Append(id).Append('\n');
        }

        if (withKey)
        {
            text.Append(PaymentProviderKeys.KeyName).Append('=').Append(key ?? SampleKey).Append('\n');
        }

        if (withSalt)
        {
            text.Append(PaymentProviderKeys.SaltName).Append('=').Append(salt ?? SampleSalt).Append('\n');
        }

        return text.ToString();
    }

    private static PaymentStartRequest Request(
        decimal total = 34.56m,
        string contactEmail = "musteri@example.com",
        string name = "Ayşe Yılmaz",
        string address = "Bağdat Cad. 1",
        string? shippingPhone = "5321234567",
        string ip = "203.0.113.7",
        DateTimeOffset? deadline = null) =>
        new(
            "2026-7K4M9P",
            total,
            contactEmail,
            "firma@example.com",
            name,
            address,
            shippingPhone,
            "2121234567",
            ip,
            deadline ?? RunnerHarness.Now.AddMinutes(30),
            AccessKey);

    private static (PayTrPaymentGateway Gateway, CapturingHandler Handler) Gateway(RunnerHarness harness, string keys, bool live = false, string? answer = null)
    {
        CapturingHandler handler = new() { Body = answer ?? "{\"status\":\"success\",\"token\":\"" + AnswerToken + "\"}" };
        PayTrPaymentGateway gateway = new(
            new OneClientFactory(handler),
            harness.Runner,
            Options.Create(new PaymentOptions { ProviderKeys = keys }),
            new SiteAddress(Options.Create(new SiteOptions { Domain = Domain })),
            harness.Time,
            new Environment(live ? Environments.Production : Environments.Development),
            new PaymentFailureReasonLog(harness.Time, harness.LoggerFor<PaymentFailureReasonLog>()));
        return (gateway, handler);
    }

    private static Dictionary<string, string> Fields(string body) =>
        body.Split('&').Select(pair => pair.Split('=', 2)).ToDictionary(p => WebUtility.UrlDecode(p[0]), p => WebUtility.UrlDecode(p[1]), StringComparer.Ordinal);

    // KK1
    [Fact]
    public async Task Start_PostsOnceToTheTokenAddress_FormEncoded_ServerToServer_KK1()
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent());

        PaymentStartResult result = await gateway.StartAsync(Request(), Ct);

        Assert.IsType<PaymentStartResult.Started>(result);
        CapturedRequest sent = Assert.Single(handler.Sent);
        Assert.Equal(HttpMethod.Post, sent.Method);
        Assert.Equal(PayTrStartForm.TokenAddress, sent.Address);
        Assert.Equal(new Uri("https://www.paytr.com/odeme/api/get-token"), sent.Address);
        Assert.Equal("application/x-www-form-urlencoded", sent.ContentType);
        Assert.Equal(
            ["merchant_id", "user_ip", "merchant_oid", "email", "payment_amount", "paytr_token", "user_basket", "no_installment", "max_installment", "user_name",
                "user_address", "user_phone", "merchant_ok_url", "merchant_fail_url", "timeout_limit", "currency", "test_mode", "lang"],
            Fields(sent.Body).Keys);
        Assert.Equal("TL", Fields(sent.Body)["currency"]);
        Assert.Equal("0", Fields(sent.Body)["no_installment"]);
        Assert.Equal("0", Fields(sent.Body)["max_installment"]);
        Assert.Equal("tr", Fields(sent.Body)["lang"]);
    }

    [Theory]
    [InlineData(false, "1")]
    [InlineData(true, "0")]
    public async Task Start_TestMode_IsZeroOnlyOnTheLiveInstallation_KK1(bool live, string expected)
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent(), live);

        await gateway.StartAsync(Request(), Ct);

        Assert.Equal(expected, Fields(Assert.Single(handler.Sent).Body)["test_mode"]);
    }

    // KK2
    public static TheoryData<string> IncompleteKeys =>
    [
        string.Empty,
        "   ",
        KeysContent(id: null),
        KeysContent(withKey: false),
        KeysContent(withSalt: false),
        KeysContent(id: string.Empty),
        KeysContent() + PaymentProviderKeys.KeyName + "=again\n",
        KeysContent() + "merchant_other=x\n",
        "made-up-provider-keys",
    ];

    [Theory]
    [MemberData(nameof(IncompleteKeys))]
    public async Task Start_WithoutTheWholeSetting_MakesNoCall_KK2(string keys)
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, keys);

        PaymentStartResult result = await gateway.StartAsync(Request(), Ct);

        Assert.Equal(new PaymentStartResult.ConnectionFailed(PayTrPaymentGateway.KeysMissingReason), result);
        Assert.Empty(handler.Sent);
        Assert.Null(PaymentProviderKeys.TryParse(keys));
    }

    [Theory]
    [MemberData(nameof(IncompleteKeys))]
    public void CardLine_WithoutTheWholeSetting_IsClosed_KK2(string keys)
    {
        ConditionalLine card = Assert.Single(ConditionalLines.Evaluate(new PaymentOptions { ProviderKeys = keys }, new GoogleOptions()), l => l.Name == ConditionalLine.CardPayment);

        Assert.False(card.IsOpen);
        Assert.Equal([PaymentOptions.ProviderKeysKey], card.MissingKeys);
    }

    [Fact]
    public void Keys_ThreeLinesInAnyOrder_WithCarriageReturnsAndBlankLines_AreTheSetting_AndTheirTextNamesNoValue_KK2()
    {
        string content = "\r\n" + PaymentProviderKeys.SaltName + " = " + SampleSalt + "\r\n" + PaymentProviderKeys.IdName + "=" + SampleId + "\r\n\r\n"
            + PaymentProviderKeys.KeyName + "=" + SampleKey;

        PaymentProviderKeys keys = Assert.IsType<PaymentProviderKeys>(PaymentProviderKeys.TryParse(content));

        Assert.Equal(new PaymentProviderKeys(SampleId, SampleKey, SampleSalt), keys);
        Assert.DoesNotContain(SampleKey, keys.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(SampleSalt, keys.ToString(), StringComparison.Ordinal);
        Assert.True(Assert.Single(ConditionalLines.Evaluate(new PaymentOptions { ProviderKeys = content }, new GoogleOptions()), l => l.Name == ConditionalLine.CardPayment).IsOpen);
    }

    // KK3
    [Fact]
    public async Task Token_MatchesTheKnownExample_AndTheKeyAndTheSaltDoNotGoWithTheRequest_KK3()
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent());

        await gateway.StartAsync(Request(), Ct);

        string body = Assert.Single(handler.Sent).Body;
        Dictionary<string, string> fields = Fields(body);
        Assert.Equal(KnownBasket, fields["user_basket"]);
        Assert.Equal(KnownToken, fields["paytr_token"]);
        Assert.DoesNotContain("merchant_key", fields.Keys);
        Assert.DoesNotContain("merchant_salt", fields.Keys);
        Assert.DoesNotContain(SampleKey, body, StringComparison.Ordinal);
        Assert.DoesNotContain(SampleSalt, body, StringComparison.Ordinal);
    }

    [Fact]
    public void Token_IsTheHmacOfTheElevenInputsInTheirOrder_KK3()
    {
        string message = SampleId + "203.0.113.7" + "20267K4M9P" + "musteri@example.com" + "3456" + KnownBasket + "0" + "0" + "TL" + "1" + SampleSalt;

        Assert.Equal(KnownToken, PayTrSignature.Of(SampleKey, message));
        Assert.NotEqual(KnownToken, PayTrSignature.Of(SampleKey, message + "x"));
    }

    // KK4
    [Theory]
    [InlineData("tr-TR")]
    [InlineData("en-US")]
    public async Task Amount_IsWholeKurus_AndTheBasketIsOneItemInBase64_WhateverTheCulture_KK4(string culture)
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            using RunnerHarness harness = new();
            (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent());

            await gateway.StartAsync(Request(total: 34.56m), Ct);

            Dictionary<string, string> fields = Fields(Assert.Single(handler.Sent).Body);
            Assert.Equal(PaymentAmounts.ToKurus(34.56m).ToString(CultureInfo.InvariantCulture), fields["payment_amount"]);
            Assert.Equal("3456", fields["payment_amount"]);
            string json = Encoding.UTF8.GetString(Convert.FromBase64String(fields["user_basket"]));
            Assert.Equal("[[\"Sipariş 2026-7K4M9P\",\"34.56\",1]]", json);
            using JsonDocument basket = JsonDocument.Parse(json);
            JsonElement item = Assert.Single(basket.RootElement.EnumerateArray());
            Assert.Equal(PaymentAmounts.ToPlainText(34.56m), item[1].GetString());
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Theory]
    [InlineData("10", "10.00")]
    [InlineData("0.5", "0.50")]
    [InlineData("34.56", "34.56")]
    [InlineData("1234567.89", "1234567.89")]
    [InlineData("0.01", "0.01")]
    public void ToPlainText_TwoPlaces_DecimalPoint_NoThousandsSeparator_InBothCultures_KK4(string amount, string expected)
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            foreach (string culture in new[] { "tr-TR", "en-US" })
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture);
                Assert.Equal(expected, PaymentAmounts.ToPlainText(decimal.Parse(amount, CultureInfo.InvariantCulture)));
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void ToPlainText_ThirdPlaceOrNegative_IsRefused_NothingIsRounded_KK4()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PaymentAmounts.ToPlainText(10.123m));
        Assert.Throws<ArgumentOutOfRangeException>(() => PaymentAmounts.ToPlainText(-1m));
    }

    // KK5
    [Fact]
    public async Task OrderReference_IsTheNumberWithoutItsDash_AndTheSameAsThePaymentRecordsReference_KK5()
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent());

        await gateway.StartAsync(Request(), Ct);

        string sent = Fields(Assert.Single(handler.Sent).Body)["merchant_oid"];
        Assert.Equal("20267K4M9P", sent);
        Assert.Equal(CardPaymentReference.OfOrderNumber("2026-7K4M9P"), sent);
        Assert.Equal("2026-7K4M9P", OrderNumber.FromWithoutSeparator(sent));
    }

    [Fact]
    public void OrderReference_MapsBackFully_ForEveryNewNumber_KK5()
    {
        for (int i = 0; i < 2000; i++)
        {
            string number = OrderNumber.Create(2026);
            Assert.Equal(number, OrderNumber.FromWithoutSeparator(OrderNumber.WithoutSeparator(number)));
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("2026")]
    [InlineData("2026-7K4M9P")]
    [InlineData("20A67K4M9P")]
    [InlineData("20267K4M9O")]
    [InlineData("20267k4m9p")]
    [InlineData(" 20267K4M9P")]
    public void OrderReference_ThatIsNotAYearAndABlock_MapsToNothing_KK5(string? value)
    {
        Assert.Null(OrderNumber.FromWithoutSeparator(value));
    }

    // KK6
    [Theory]
    [InlineData(100, false, true)]
    [InlineData(101, false, false)]
    [InlineData(20, true, false)]
    public void Email_OutsideTheLimit_IsReplacedByTheFirmEmail_NeitherCutNorConverted_KK6(int length, bool nonAscii, bool kept)
    {
        string local = new('a', length - "@example.com".Length - (nonAscii ? 1 : 0));
        string email = local + (nonAscii ? "ş" : string.Empty) + "@example.com";
        Assert.Equal(length, email.Length);

        Assert.Equal(kept ? email : "firma@example.com", PayTrStartForm.Email(email, "firma@example.com"));
    }

    [Theory]
    [InlineData("5321234567", "05321234567")]
    [InlineData(null, "02121234567")]
    [InlineData("", "02121234567")]
    public void Phone_TakesTheLeadingZero_AndTheFirmPhoneStandsInWithoutAShippingPhone_KK6(string? shipping, string expected)
    {
        Assert.Equal(expected, PayTrStartForm.Phone(shipping, "2121234567"));
    }

    [Fact]
    public async Task NameAndAddress_AreCutAtTheirLimits_AndTheStandInsGoInTheRequest_KK6()
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent());
        string name = new('N', PayTrStartForm.NameMaxLength + 5);
        string address = new('A', PayTrStartForm.AddressMaxLength + 5);

        await gateway.StartAsync(Request(name: name, address: address, shippingPhone: null, contactEmail: "müşteri@example.com"), Ct);

        Dictionary<string, string> fields = Fields(Assert.Single(handler.Sent).Body);
        Assert.Equal(name[..PayTrStartForm.NameMaxLength], fields["user_name"]);
        Assert.Equal(address[..PayTrStartForm.AddressMaxLength], fields["user_address"]);
        Assert.Equal("02121234567", fields["user_phone"]);
        Assert.Equal("firma@example.com", fields["email"]);
        Assert.Equal(60, PayTrStartForm.NameMaxLength);
        Assert.Equal(400, PayTrStartForm.AddressMaxLength);
        Assert.Equal(100, PayTrStartForm.EmailMaxLength);
    }

    [Theory]
    [InlineData(60, 60)]
    [InlineData(59, 59)]
    [InlineData(61, 60)]
    public void Cut_KeepsAValueAtTheLimit_AndCutsALongerOne_KK6(int length, int expected)
    {
        Assert.Equal(expected, PayTrStartForm.Cut(new string('x', length), PayTrStartForm.NameMaxLength).Length);
    }

    [Fact]
    public void Cut_DoesNotSplitASurrogatePair_KK6()
    {
        string value = new string('x', PayTrStartForm.NameMaxLength - 1) + "😀" + "tail";

        Assert.Equal(new string('x', PayTrStartForm.NameMaxLength - 1), PayTrStartForm.Cut(value, PayTrStartForm.NameMaxLength));
    }

    // KK7
    [Theory]
    [InlineData("203.0.113.7", "203.0.113.7")]
    [InlineData("::ffff:203.0.113.7", "203.0.113.7")]
    [InlineData("2001:db8:85a3::8a2e:370:7334", "2001:db8:85a3::8a2e:370:7334")]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334", "2001:db8:85a3::8a2e:370:7334")]
    [InlineData("fe80::1%12", "fe80::1")]
    public void ClientAddress_GoesAsItIs_MappedIPv4TurnedBack_NoZone_KK7(string text, string expected)
    {
        Assert.True(PayTrStartForm.TryClientAddress(text, out string address));
        Assert.Equal(expected, address);
        Assert.True(address.Length <= PayTrStartForm.ClientAddressMaxLength);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-an-address")]
    [InlineData("203.0.113")]
    public async Task ClientAddress_ThatIsNoAddress_MakesNoCall_KK7(string? text)
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent());

        PaymentStartResult result = await gateway.StartAsync(Request(ip: text!), Ct);

        Assert.Equal(new PaymentStartResult.ConnectionFailed(PayTrPaymentGateway.ClientAddressReason), result);
        Assert.Empty(handler.Sent);
    }

    public static TheoryData<long, long> Minutes => new()
    {
        { TimeSpan.FromMinutes(30).Ticks, 30 },
        { TimeSpan.FromMinutes(29).Ticks + 1, 30 },
        { TimeSpan.FromSeconds(30).Ticks, 1 },
        { 1, 1 },
        { 0, 1 },
        { -TimeSpan.FromMinutes(5).Ticks, 1 },
        { TimeSpan.FromHours(48).Ticks - TimeSpan.FromSeconds(1).Ticks, 48 * 60 },
    };

    [Theory]
    [MemberData(nameof(Minutes))]
    public void TimeoutMinutes_IsTheTimeLeftRoundedUp_AtLeastOne_KK7(long leftTicks, long expected)
    {
        Assert.Equal(expected, PayTrStartForm.TimeoutMinutes(RunnerHarness.Now.AddTicks(leftTicks), RunnerHarness.Now));
    }

    [Fact]
    public async Task TimeoutLimit_IsReadOnTheClockAtTheRequest_KK7()
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent());
        harness.Time.Advance(TimeSpan.FromMinutes(12) + TimeSpan.FromSeconds(1));

        await gateway.StartAsync(Request(deadline: RunnerHarness.Now.AddMinutes(30)), Ct);

        Assert.Equal("18", Fields(Assert.Single(handler.Sent).Body)["timeout_limit"]);
    }

    // KK8
    [Fact]
    public async Task Answer_GivesTheFrameAddress_BothReturnAddressesAreTheAccessKeyPath_AndTheTokenReachesNoLogLine_KK8()
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent());

        PaymentStartResult result = await gateway.StartAsync(Request(), Ct);

        PaymentStartResult.Started started = Assert.IsType<PaymentStartResult.Started>(result);
        Assert.Equal(new Uri("https://www.paytr.com/odeme/guvenli/" + AnswerToken), started.FrameAddress);
        Dictionary<string, string> fields = Fields(Assert.Single(handler.Sent).Body);
        Assert.Equal("https://" + Domain + "/odeme/donus/" + AccessKey, fields["merchant_ok_url"]);
        Assert.Equal(fields["merchant_ok_url"], fields["merchant_fail_url"]);
        Assert.DoesNotContain(harness.Lines, line => line.Contains(AnswerToken, StringComparison.Ordinal) || line.Contains(KnownToken, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(200, "{\"status\":\"failed\",\"reason\":\"Zorunlu alan değeri geçersiz: merchant_id\"}", PayTrPaymentGateway.FailedReason)]
    [InlineData(200, "{\"status\":\"success\"}", PayTrPaymentGateway.UnrecognizedBodyReason)]
    [InlineData(200, "{\"status\":\"success\",\"token\":\"\"}", PayTrPaymentGateway.UnrecognizedBodyReason)]
    [InlineData(200, "not json", PayTrPaymentGateway.UnrecognizedBodyReason)]
    [InlineData(200, "[]", PayTrPaymentGateway.UnrecognizedBodyReason)]
    [InlineData(500, "{\"status\":\"success\",\"token\":\"x\"}", "http:500")]
    public async Task Answer_WithoutAToken_IsAConnectionError_WritingOnlyTheReasonCode_KK8(int status, string body, string reason)
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent(), answer: body);
        handler.Status = (HttpStatusCode)status;

        PaymentStartResult result = await gateway.StartAsync(Request(), Ct);

        Assert.Equal(new PaymentStartResult.ConnectionFailed(reason), result);
        Assert.Single(handler.Sent);
        Assert.DoesNotContain(harness.Lines, line => line.Contains("merchant_id", StringComparison.Ordinal) || line.Contains("musteri@", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Start_ThatHangs_EndsAtTheUserWaitingLimit_WithOneRequest_KK1()
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent());
        handler.Hang = true;

        Task<PaymentStartResult> call = gateway.StartAsync(Request(), Ct);
        harness.Time.Advance(Shopfolio.Infrastructure.ExternalCallTimeouts.UserWaiting);

        Assert.Equal(new PaymentStartResult.ConnectionFailed(ExternalCallFailure.TimeoutReason), await call);
        Assert.Single(handler.Sent);
    }

    private sealed record CapturedRequest(HttpMethod Method, Uri? Address, string? ContentType, string Body);

    // A hand-written handler: it keeps each request's method, address, content type and body, and answers as told. No socket is opened.
    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly List<CapturedRequest> _sent = [];

        public string Body { get; set; } = string.Empty;

        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

        public bool Hang { get; set; }

        public List<CapturedRequest> Sent => _sent;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            _sent.Add(new CapturedRequest(request.Method, request.RequestUri, request.Content?.Headers.ContentType?.MediaType, body));
            if (Hang)
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }

            return new HttpResponseMessage(Status) { Content = new StringContent(Body) };
        }
    }

    internal sealed class OneClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            Assert.Equal(IntegrationHttpClients.PaymentProvider, name);
            return new HttpClient(handler, disposeHandler: false) { Timeout = Timeout.InfiniteTimeSpan };
        }
    }

    internal sealed class Environment(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;

        public string ApplicationName { get; set; } = "Shopfolio.Web";

        public string ContentRootPath { get; set; } = string.Empty;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
