using System.Net;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.EmailComposition;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.SearchEngine;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.UnitTests.Notifications;

/// <summary>
/// The bodies of the customer notices B-1…B-8 (T306 criteria 1-2, 4, 6-10, KK1-KK4, KK6-KK10; 08 2.2.6; 09 11.4 item 7): the skeleton, the order number in the subject and
/// the event's sentence, the main button's address, the two legal texts of B-1, the frozen IBAN of B-2, the digital item sentence of B-4, the three shipping forms of
/// B-5, the firm's contact in B-6 and B-7, the three wordings and the reasons of B-7, the reason, amount and method of B-8 and the event values' round trip. Every
/// notice goes through the real composer, so the HTML and the plain text part are the ones a customer gets. No network and no database.
/// </summary>
public sealed class CustomerNoticeTemplateTests
{
    private const string Domain = "magaza.example.com";
    private const string Recipient = "musteri@example.com";
    private const string Number = "2026-ABC123";
    private const string AccessKey = "erisim-anahtari-1";
    private const string Iban = "TR330006100519786457841326";
    private const string FirmEmail = "destek@magaza.example.com";
    private const string FirmPhone = "3121234567";
    private const string PreForm = "ÖN BİLGİLENDİRME FORMU METNİ\n\nMadde 1 - Satıcı: Örnek Ticaret.\nMadde 2 - Alıcı: Ali Veli.";
    private const string Contract = "MESAFELİ SATIŞ SÖZLEŞMESİ METNİ\n\nMadde 1 - Taraflar.\nMadde 2 - Konu.";

    private static readonly Guid OrderId = Guid.Parse("018f0000-0000-7000-8000-0000000000a1");
    private static readonly DateTimeOffset FirstAt = new(2026, 10, 4, 12, 0, 0, TimeSpan.Zero);

    private static readonly ServiceProvider _services = new ServiceCollection().AddLogging().BuildServiceProvider();
    private static readonly EmailFirm _firm = new("Örnek Ticaret Anonim Şirketi", FirmEmail);
    private static readonly EmailBranding _brand = new("Örnek Mağaza", null, null);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static IOptions<SiteOptions> Site() => Options.Create(new SiteOptions { Domain = Domain });

    private static EmailLinks Links() => new(new SiteAddress(Site()));

    private static CustomerNoticeTemplates Templates(CarrierCatalog? carriers = null) => new(Links(), carriers ?? CarrierCatalog.Default);

    private static OrderNoticeContext Member(DateTimeOffset? resendOf = null, bool digital = false) =>
        new(Number, null, false, false, Iban, digital, PreForm, Contract, FirmEmail, FirmPhone, resendOf);

    private static OrderNoticeContext Guest(DateTimeOffset? resendOf = null) =>
        new(Number, AccessKey, true, false, Iban, false, PreForm, Contract, FirmEmail, FirmPhone, resendOf);

    private static Task<EmailMessage> Compose(OrderNoticeContext order, CustomerNotice notice, CarrierCatalog? carriers = null)
    {
        EmailComposer composer = new(Links(), Site(), _services, _services.GetRequiredService<ILoggerFactory>());
        return composer.ComposeAsync(Recipient, _brand, _firm, Templates(carriers).Compose(order, notice), Ct);
    }

    private static string Decoded(string html) => WebUtility.HtmlDecode(html);

    private static readonly DateTimeOffset WindowEnd = new(2026, 11, 10, 21, 0, 0, TimeSpan.Zero);

    private static IEnumerable<CustomerNotice> AllEight()
    {
        yield return new OrderReceivedNotice(OrderId);
        yield return new TransferChosenNotice(OrderId, WindowEnd);
        yield return new TransferReminderNotice(OrderId, WindowEnd);
        yield return new PaymentConfirmedNotice(OrderId);
        yield return new ShippedNotice(OrderId, "YurticiKargo", null, "YT123456", false);
        yield return new DeliveryFailedNotice(OrderId);
        yield return new CancelledNotice(OrderId, ActorKind.Buyer, null, null, null, null);
        yield return new RefundProcessedNotice(OrderId, RefundType.Cancellation, 1200m, PaymentMethod.Card);
    }

    public static TheoryData<int> EightIndexes() => [0, 1, 2, 3, 4, 5, 6, 7];

    // ---------- KK1: eight types; the skeleton; no name in the greeting ----------

    [Fact]
    public void TheEightNotices_AreTheEightClosedTypes_InOrder()
    {
        DispatchType[] types = [.. AllEight().Select(n => CustomerNoticeValues.For(n).Type)];

        Assert.Equal([DispatchType.B1, DispatchType.B2, DispatchType.B3, DispatchType.B4, DispatchType.B5, DispatchType.B6, DispatchType.B7, DispatchType.B8], types);
        Assert.All(types, type => Assert.True(Enum.IsDefined(type)));
    }

    [Theory]
    [MemberData(nameof(EightIndexes))]
    public async Task EveryNotice_StartsWithMerhaba_WithoutAName_AndTheOrderNumberIsInTheSubjectAndTheSentence(int index)
    {
        CustomerNotice notice = AllEight().ElementAt(index);
        EmailMessage message = await Compose(Guest(), notice);

        Assert.EndsWith($" (Sipariş {Number})", message.Subject, StringComparison.Ordinal);
        Assert.StartsWith("Örnek Mağaza: ", message.Subject, StringComparison.Ordinal);

        // The plain text: brand, a blank line, the greeting with no name, a blank line, then the event's sentence, which holds the order number.
        string[] lines = message.TextBody.Split('\n');
        Assert.Equal("Örnek Mağaza", lines[0]);
        Assert.Equal("Merhaba,", lines[2]);
        Assert.Equal(string.Empty, lines[3]);
        Assert.Contains(Number, lines[4], StringComparison.Ordinal);
        Assert.Contains("Merhaba,", Decoded(message.HtmlBody), StringComparison.Ordinal);
        Assert.DoesNotContain("Merhaba,\n" + "Ali", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheSubjects_AreTheTitlesOfTheSpec()
    {
        string[] expected =
        [
            "Örnek Mağaza: Siparişiniz alındı (Sipariş 2026-ABC123)",
            "Örnek Mağaza: Havale/EFT bilgileriniz (Sipariş 2026-ABC123)",
            "Örnek Mağaza: Havale ödemeniz bekleniyor (Sipariş 2026-ABC123)",
            "Örnek Mağaza: Ödemeniz onaylandı (Sipariş 2026-ABC123)",
            "Örnek Mağaza: Siparişiniz kargoya verildi (Sipariş 2026-ABC123)",
            "Örnek Mağaza: Siparişiniz teslim edilemedi (Sipariş 2026-ABC123)",
            "Örnek Mağaza: Siparişiniz iptal edildi (Sipariş 2026-ABC123)",
            "Örnek Mağaza: Geri ödemeniz işlendi (Sipariş 2026-ABC123)",
        ];
        List<string> actual = [];
        foreach (CustomerNotice notice in AllEight())
        {
            actual.Add((await Compose(Guest(), notice)).Subject);
        }

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(4, true)]
    [InlineData(0, false)]
    [InlineData(3, false)]
    [InlineData(5, false)]
    [InlineData(6, false)]
    [InlineData(7, false)]
    public async Task AResend_CarriesTheSentenceOfTheFirstEmail_AndOnlyB2B3B5CarryTheCurrentInfoSentence(int index, bool carriesNote)
    {
        EmailMessage message = await Compose(Guest(FirstAt), AllEight().ElementAt(index));

        string sentence = "Bu e-posta 4 Ekim 2026 tarihli bildirimin yeniden gönderilmesidir.";
        Assert.Contains(sentence, message.TextBody, StringComparison.Ordinal);
        Assert.Equal(carriesNote, message.TextBody.Contains(EmailResend.CurrentInfoSentence, StringComparison.Ordinal));
        Assert.Equal(carriesNote, Decoded(message.HtmlBody).Contains(EmailResend.CurrentInfoSentence, StringComparison.Ordinal));

        // The sentence is the first text of the body, before the greeting; the subject is the same as the first e-mail's.
        Assert.True(message.TextBody.IndexOf(sentence, StringComparison.Ordinal) < message.TextBody.IndexOf("Merhaba,", StringComparison.Ordinal));
        Assert.Equal((await Compose(Guest(), AllEight().ElementAt(index))).Subject, message.Subject);
    }

    [Theory]
    [MemberData(nameof(EightIndexes))]
    public async Task ANewEventOfTheSameType_CarriesNeitherSentence(int index)
    {
        EmailMessage message = await Compose(Guest(), AllEight().ElementAt(index));

        Assert.DoesNotContain("yeniden gönderilmesidir", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain(EmailResend.CurrentInfoSentence, message.TextBody, StringComparison.Ordinal);
    }

    // ---------- KK2: the main button's address ----------

    [Theory]
    [MemberData(nameof(EightIndexes))]
    public async Task ThePageButton_OfAGuestOrder_IsTheAccessKeyAddress_InTheHtmlAndInThePlainText(int index)
    {
        EmailMessage message = await Compose(Guest(), AllEight().ElementAt(index));

        string address = $"https://{Domain}/siparis/erisim/{AccessKey}";
        Assert.Contains($"href=\"{address}\"", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("Siparişi görüntüle:\n" + address, message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("/siparis/" + Number, message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ThePageButton_OfAnOrderWhoseAccountIsDeleted_IsTheAccessKeyAddress_NotTheAccountAddress()
    {
        OrderNoticeContext deleted = new(Number, AccessKey, false, true, Iban, false, PreForm, Contract, FirmEmail, FirmPhone, null);

        EmailMessage message = await Compose(deleted, new PaymentConfirmedNotice(OrderId));

        Assert.Contains($"https://{Domain}/siparis/erisim/{AccessKey}", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain($"https://{Domain}/siparis/{Number}", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ThePageButton_OfAMembersOrder_IsTheAccountsOrderAddress()
    {
        EmailMessage message = await Compose(Member(), new PaymentConfirmedNotice(OrderId));

        Assert.Contains($"https://{Domain}/siparis/{Number}", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("/siparis/erisim/", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("/siparis/erisim/", message.HtmlBody, StringComparison.Ordinal);
    }

    [Fact]
    public void AGuestOrderWithoutAnAccessKey_IsRefused_NotSentToTheAccountAddress()
    {
        OrderNoticeContext broken = new(Number, null, true, false, Iban, false, PreForm, Contract, FirmEmail, FirmPhone, null);

        Assert.Throws<ArgumentException>(() => Templates().Compose(broken, new PaymentConfirmedNotice(OrderId)));
    }

    // ---------- KK3: B-1 and the two legal texts ----------

    [Fact]
    public async Task B1_CarriesTheSentenceTheButtonAndThenTheTwoTextsInFull_AsTwoSections()
    {
        EmailMessage message = await Compose(Guest(), new OrderReceivedNotice(OrderId));

        Assert.Contains($"Siparişiniz alındı. Sipariş numaranız: {Number}.", message.TextBody, StringComparison.Ordinal);
        int button = message.TextBody.IndexOf("Siparişi görüntüle:", StringComparison.Ordinal);
        int form = message.TextBody.IndexOf("Ön Bilgilendirme Formu\n", StringComparison.Ordinal);
        int contract = message.TextBody.IndexOf("Mesafeli Satış Sözleşmesi\n", StringComparison.Ordinal);
        Assert.True(button >= 0 && button < form && form < contract);
        Assert.Contains(PreForm, message.TextBody, StringComparison.Ordinal);
        Assert.Contains(Contract, message.TextBody, StringComparison.Ordinal);

        string html = Decoded(message.HtmlBody);
        Assert.True(html.IndexOf("<h2", StringComparison.Ordinal) > html.IndexOf("Siparişi görüntüle", StringComparison.Ordinal));
        Assert.Contains("Ön Bilgilendirme Formu</h2>", html, StringComparison.Ordinal);
        Assert.Contains("Mesafeli Satış Sözleşmesi</h2>", html, StringComparison.Ordinal);
        Assert.Contains("MESAFELİ SATIŞ SÖZLEŞMESİ METNİ", html, StringComparison.Ordinal);
        Assert.Contains("Madde 2 - Alıcı: Ali Veli.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheTwoTexts_AreSetAtTwelvePointsAtLeast_WithoutScrollBoxFoldOrReadMore()
    {
        EmailMessage message = await Compose(Guest(), new OrderReceivedNotice(OrderId));

        string html = message.HtmlBody;
        int firstText = html.IndexOf("Madde 1 - Sat", StringComparison.Ordinal);
        Assert.True(firstText > 0);
        string paragraph = html[html.LastIndexOf("<p", firstText, StringComparison.Ordinal)..firstText];
        Assert.Contains("font-size:12pt", paragraph, StringComparison.Ordinal);
        Assert.Contains("-webkit-text-size-adjust:100%", html, StringComparison.Ordinal);
        foreach (string forbidden in new[] { "overflow", "max-height", "<details", "<summary", "devamını oku", "display:none", "font-size:11", "font-size:10", "font-size:9", "font-size:8" })
        {
            Assert.DoesNotContain(forbidden, html.Replace("font-size:14px", string.Empty, StringComparison.Ordinal).Replace("font-size:16px", string.Empty, StringComparison.Ordinal), StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task B1_HasNoAttachment_AndNoTextIsReplacedByALink()
    {
        EmailMessage message = await Compose(Guest(), new OrderReceivedNotice(OrderId));

        Assert.DoesNotContain(typeof(EmailMessage).GetProperties(), p => p.Name.Contains("Attach", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(1, CountOf(message.HtmlBody, "<a "));
    }

    [Fact]
    public async Task ALegalTextWithMarkup_IsTextInTheHtml_AndTheB1DoesNotExpandAPlaceholderShapedValue()
    {
        OrderNoticeContext order = new(Number, AccessKey, true, false, Iban, false, "<script>alert(1)</script>\n{{ShippingRecipientName}}", Contract, FirmEmail, FirmPhone, null);

        EmailMessage message = await Compose(order, new OrderReceivedNotice(OrderId));

        Assert.DoesNotContain("<script", message.HtmlBody, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("&lt;script&gt;", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("{{ShippingRecipientName}}", message.TextBody, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null, "metin")]
    [InlineData("metin", null)]
    [InlineData(" ", "metin")]
    public void B1_WithoutOneOfTheTexts_IsRefused(string? form, string? contract)
    {
        OrderNoticeContext order = new(Number, AccessKey, true, false, Iban, false, form, contract, FirmEmail, FirmPhone, null);

        Assert.Throws<ArgumentException>(() => Templates().Compose(order, new OrderReceivedNotice(OrderId)));
    }

    // ---------- KK4: B-2 ----------

    [Fact]
    public async Task B2_CarriesTheFrozenIbanInGroupsOfFour_TheOrderNumberAndTheLastPaymentDay()
    {
        EmailMessage message = await Compose(Guest(), new TransferChosenNotice(OrderId, WindowEnd));

        Assert.Contains("Ödemeyi aşağıdaki hesaba havale/EFT ile yapabilirsiniz.", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("IBAN: TR33 0006 1005 1978 6457 8413 26", message.TextBody, StringComparison.Ordinal);
        Assert.Contains($"Havale açıklamasına yazılacak sipariş numarası: {Number}", message.TextBody, StringComparison.Ordinal);

        // The window ended at 00:00 on the 11th (Turkey time, 21:00 UTC on the 10th): the last day is the 10th.
        Assert.Contains("Ödemenizi 10 Kasım 2026 tarihine kadar yapmanız gerekir; aksi hâlde sipariş iptal edilir.", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("TR33 0006 1005 1978 6457 8413 26", Decoded(message.HtmlBody), StringComparison.Ordinal);
    }

    [Fact]
    public async Task B2_WritesTheIbanTheOrderFroze_NotAnotherOne()
    {
        OrderNoticeContext frozen = new(Number, AccessKey, true, false, "TR120006200000000000000001", false, PreForm, Contract, FirmEmail, FirmPhone, null);

        EmailMessage message = await Compose(frozen, new TransferChosenNotice(OrderId, WindowEnd));

        Assert.Contains("TR12 0006 2000 0000 0000 0000 01", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("TR33", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public void B2AndB3_WithoutAFrozenIban_AreRefused()
    {
        OrderNoticeContext none = new(Number, AccessKey, true, false, null, false, PreForm, Contract, FirmEmail, FirmPhone, null);

        Assert.Throws<ArgumentException>(() => Templates().Compose(none, new TransferChosenNotice(OrderId, WindowEnd)));
        Assert.Throws<ArgumentException>(() => Templates().Compose(none, new TransferReminderNotice(OrderId, WindowEnd)));
    }

    // ---------- KK5 (the body part): B-3 ----------

    [Fact]
    public async Task B3_CarriesTheIbanBlockTheEventsWindowEndAndTheClosingSentence()
    {
        DateTimeOffset restartedEnd = new(2026, 11, 20, 21, 0, 0, TimeSpan.Zero);

        EmailMessage message = await Compose(Guest(), new TransferReminderNotice(OrderId, restartedEnd));

        Assert.Contains("Ödemeyi 20 Kasım 2026 tarihine kadar yapmazsanız siparişiniz iptal edilir.", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("IBAN: TR33 0006 1005 1978 6457 8413 26", message.TextBody, StringComparison.Ordinal);
        Assert.Contains($"Havale açıklamasına yazılacak sipariş numarası: {Number}", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Ödemeyi yaptıysanız bu e-postayı dikkate almayın; havale firmaya ulaştığında siparişiniz güncellenir.", message.TextBody, StringComparison.Ordinal);
        Assert.True(message.TextBody.IndexOf("Siparişi görüntüle:", StringComparison.Ordinal) < message.TextBody.IndexOf("Ödemeyi yaptıysanız", StringComparison.Ordinal));
    }

    // ---------- KK6: B-4 ----------

    [Fact]
    public async Task B4_WithADigitalItem_PointsTheDownloadToTheOrderPage_AndWritesNoDownloadAddress()
    {
        EmailMessage message = await Compose(Guest(), new PaymentConfirmedNotice(OrderId));
        EmailMessage digital = await Compose(Member(digital: true), new PaymentConfirmedNotice(OrderId));

        Assert.DoesNotContain("Dijital ürününüz", message.TextBody, StringComparison.Ordinal);
        Assert.Contains($"{Number} numaralı siparişinizin ödemesi onaylandı.", digital.TextBody, StringComparison.Ordinal);
        Assert.Contains("Dijital ürününüz indirmeye hazır. Dosyayı sipariş sayfanızdan indirebilirsiniz.", digital.TextBody, StringComparison.Ordinal);

        // The only address is the order page's (the button), never a file address.
        Assert.Equal(1, CountOf(digital.TextBody, "https://"));
        Assert.DoesNotContain("/indir", digital.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("/indir", digital.HtmlBody, StringComparison.Ordinal);
    }

    // ---------- KK7: B-5 ----------

    [Fact]
    public async Task B5_OfAListedCompany_WritesTheNameTheNumberAndATrackingLinkWithNoReferrer()
    {
        EmailMessage message = await Compose(Guest(), new ShippedNotice(OrderId, "YurticiKargo", null, "YT 123/456", false));

        Assert.Contains($"{Number} numaralı siparişiniz kargoya verildi.", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Kargo şirketi: Yurtiçi Kargo", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Takip numarası: YT 123/456", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Takip et:\nhttps://www.yurticikargo.com/tr/online-servisler/gonderi-sorgula?code=YT%20123%2F456", message.TextBody, StringComparison.Ordinal);
        string link = "href=\"https://www.yurticikargo.com/tr/online-servisler/gonderi-sorgula?code=YT%20123%2F456\" rel=\"noreferrer\"";
        Assert.Contains(link, message.HtmlBody, StringComparison.Ordinal);

        // Only the carrier link carries noreferrer; the order page button is ours and has no rel.
        Assert.Equal(1, CountOf(message.HtmlBody, "rel=\"noreferrer\""));
    }

    [Fact]
    public async Task B5_OfAnotherCompany_WritesTheNameAndTheNumber_ButNoLink()
    {
        EmailMessage message = await Compose(Guest(), new ShippedNotice(OrderId, null, "Yerel Kurye", "K-77", false));

        Assert.Contains("Kargo şirketi: Yerel Kurye", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Takip numarası: K-77", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("Takip et", message.TextBody, StringComparison.Ordinal);
        Assert.Equal(1, CountOf(message.TextBody, "https://"));
        Assert.DoesNotContain("noreferrer", message.HtmlBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task B5_OfARetiredCompany_WritesTheNameByTheOldCode_AndNoLink()
    {
        CarrierCatalog catalog = new(CarrierCatalog.Default.Active, [new RetiredCarrierEntry("EskiKargo", "Eski Kargo")]);

        EmailMessage message = await Compose(Guest(), new ShippedNotice(OrderId, "EskiKargo", null, "E-1", false), catalog);

        Assert.Contains("Kargo şirketi: Eski Kargo", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Takip numarası: E-1", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("Takip et", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("noreferrer", message.HtmlBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task B5_OfAListedCompanyWithoutANumber_WritesTheNameAndNoLink()
    {
        EmailMessage message = await Compose(Guest(), new ShippedNotice(OrderId, "ArasKargo", null, null, false));

        Assert.Contains("Kargo şirketi: Aras Kargo", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("Takip numarası", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("Takip et", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task B5_OfOwnVehicleDelivery_WritesTheDeclarationAlone_NoCompanyAndNoNumber()
    {
        EmailMessage message = await Compose(Guest(), new ShippedNotice(OrderId, null, null, null, true));

        Assert.Contains("Siparişiniz firmamızın kendi aracıyla teslim edilecektir.", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("Kargo şirketi", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("Takip", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("noreferrer", message.HtmlBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task B5_ATrackingNumberWithMarkup_IsTextInTheHtml_AndOnlyTheNumberGoesIntoTheAddress()
    {
        EmailMessage message = await Compose(Guest(), new ShippedNotice(OrderId, "SuratKargo", null, "<b>1</b>&x=2", false));

        Assert.DoesNotContain("<b>", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("kargotakipno=%3Cb%3E1%3C%2Fb%3E%26x%3D2", message.TextBody, StringComparison.Ordinal);
    }

    // ---------- KK8: B-6 ----------

    [Fact]
    public async Task B6_WritesTheSentence_AndTheFirmsEmailAndTelephoneInTheText()
    {
        EmailMessage message = await Compose(Guest(), new DeliveryFailedNotice(OrderId));

        Assert.Contains($"{Number} numaralı siparişinizin gönderisi teslim edilemedi ve firmamıza geri döndü. Teslimat için firmayla iletişime geçebilirsiniz.", message.TextBody, StringComparison.Ordinal);
        Assert.Contains($"Firma e-postası: {FirmEmail}", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Firma telefonu: 0312 123 45 67", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("0312 123 45 67", Decoded(message.HtmlBody), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("", FirmPhone)]
    [InlineData(FirmEmail, "")]
    [InlineData(" ", FirmPhone)]
    public void B6AndB7_WithoutTheFirmsEmailOrTelephone_AreRefused(string email, string phone)
    {
        OrderNoticeContext order = new(Number, AccessKey, true, false, Iban, false, PreForm, Contract, email, phone, null);

        Assert.Throws<ArgumentException>(() => Templates().Compose(order, new DeliveryFailedNotice(OrderId)));
        Assert.Throws<ArgumentException>(() => Templates().Compose(order, new CancelledNotice(OrderId, ActorKind.Buyer, null, null, null, null)));
    }

    // ---------- KK8 and KK9: B-7 ----------

    [Fact]
    public async Task B7_ByTheCustomer_IsTheRecordCopy_WithNoReasonAndNoRefundParagraph_AndTheFirmsContact()
    {
        EmailMessage message = await Compose(Guest(), new CancelledNotice(OrderId, ActorKind.Buyer, null, null, null, null));

        Assert.Contains($"Talebiniz üzerine {Number} numaralı siparişiniz iptal edildi.", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("Sebep", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("on dört gün", message.TextBody, StringComparison.Ordinal);
        Assert.Contains($"Firma e-postası: {FirmEmail}", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Firma telefonu: 0312 123 45 67", message.TextBody, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(CancellationReason.OutOfStock, "stokta bulunamadı")]
    [InlineData(CancellationReason.DamagedOrUnsellable, "ürün hasarlı ya da satılamaz durumda")]
    [InlineData(CancellationReason.CannotShipToAddress, "teslimat adresine gönderim yapılamıyor")]
    [InlineData(CancellationReason.UndeliveredBuyerCaused, "gönderi teslim edilemedi — müşteri kaynaklı (adres hatalı ya da alıcı bulunamadı)")]
    [InlineData(CancellationReason.AgreedWithBuyer, "müşteriyle anlaşıldı (müşteri talebi)")]
    public async Task B7_ByTheAdministrator_WritesTheReasonAsItIs_AndTheFourteenDaySentence_EvenOnStock(CancellationReason reason, string text)
    {
        EmailMessage message = await Compose(Guest(), new CancelledNotice(OrderId, ActorKind.Admin, reason, null, null, null));

        Assert.Contains($"{Number} numaralı siparişiniz firmamız tarafından iptal edildi.", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Sebep: " + text, message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Ödediğiniz tutar, teslimat masrafları dahil, bu bildirimden itibaren en geç on dört gün içinde iade edilir.", message.TextBody, StringComparison.Ordinal);
        Assert.Contains($"Firma e-postası: {FirmEmail}", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Firma telefonu: 0312 123 45 67", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task B7_ByTheAdministratorWithOther_WritesTheNoteAsItIs_EncodedInTheHtml()
    {
        EmailMessage message = await Compose(
            Guest(),
            new CancelledNotice(OrderId, ActorKind.Admin, CancellationReason.Other, "Tedarikçi <b>kapandı</b>\nüretim durdu", null, null));

        Assert.Contains("Sebep: Tedarikçi <b>kapandı</b> üretim durdu", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("<b>", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("&lt;b&gt;kapand", message.HtmlBody, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(TransitionReason.PaymentWindowExpired, "ödeme süresi dolduğu için")]
    [InlineData(TransitionReason.CardPaymentFailed, "kart ödemeniz tamamlanamadığı için")]
    [InlineData(TransitionReason.PaymentPageNotOpened, "ödeme sayfası açılamadığı için")]
    [InlineData(TransitionReason.SameCartReorder, "aynı sepetten yeni bir sipariş verdiğiniz için")]
    public async Task B7_BySystem_WritesTheReason_AndNoFourteenDaySentence_AndTheFirmsContact(TransitionReason reason, string text)
    {
        EmailMessage message = await Compose(Guest(), new CancelledNotice(OrderId, ActorKind.System, null, null, reason, null));

        Assert.Contains($"{Number} numaralı siparişiniz kendiliğinden iptal edildi.", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Sebep: " + text, message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("on dört gün", message.TextBody, StringComparison.Ordinal);
        Assert.Contains($"Firma e-postası: {FirmEmail}", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Firma telefonu: 0312 123 45 67", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task B7_OfAnItem_HasItsOwnSubject_AndTheItemAndQuantityOnlyThere()
    {
        CancelledItem item = new("Ahşap Masa", "Meşe", "120 cm", 2);
        EmailMessage byCustomer = await Compose(Guest(), new CancelledNotice(OrderId, ActorKind.Buyer, null, null, null, item));
        EmailMessage byAdmin = await Compose(Guest(), new CancelledNotice(OrderId, ActorKind.Admin, CancellationReason.OutOfStock, null, null, item));
        EmailMessage whole = await Compose(Guest(), new CancelledNotice(OrderId, ActorKind.Admin, CancellationReason.OutOfStock, null, null, null));

        Assert.Equal($"Örnek Mağaza: Siparişinizdeki bir kalem iptal edildi (Sipariş {Number})", byCustomer.Subject);
        Assert.Equal(byCustomer.Subject, byAdmin.Subject);
        Assert.NotEqual(whole.Subject, byAdmin.Subject);
        Assert.Contains($"Talebiniz üzerine {Number} numaralı siparişinizdeki Ahşap Masa (Meşe, 120 cm), 2 adet iptal edildi.", byCustomer.TextBody, StringComparison.Ordinal);
        Assert.Contains($"{Number} numaralı siparişinizdeki Ahşap Masa (Meşe, 120 cm), 2 adet firmamız tarafından iptal edildi.", byAdmin.TextBody, StringComparison.Ordinal);
        Assert.Contains("on dört gün", byAdmin.TextBody, StringComparison.Ordinal);

        // The whole order's cancellation lists no item and no quantity.
        Assert.DoesNotContain("Ahşap", whole.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain(" adet", whole.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task B7_OfAnItemWithoutOptions_WritesTheNameAlone_AndAMarkupNameIsTextInTheHtml()
    {
        CancelledItem item = new("<i>Masa</i>", null, null, 1);

        EmailMessage message = await Compose(Guest(), new CancelledNotice(OrderId, ActorKind.Buyer, null, null, null, item));

        Assert.Contains("siparişinizdeki <i>Masa</i>, 1 adet iptal edildi.", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("<i>", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("&lt;i&gt;Masa&lt;/i&gt;", message.HtmlBody, StringComparison.Ordinal);
    }

    // ---------- KK10: B-8 ----------

    [Theory]
    [InlineData(RefundType.Cancellation, "iptal")]
    [InlineData(RefundType.ItemRemoval, "kalem çıkarma")]
    [InlineData(RefundType.LateDeliveryTermination, "gecikme nedeniyle fesih")]
    [InlineData(RefundType.Withdrawal, "cayma")]
    [InlineData(RefundType.DefectClaimRescission, "ayıp talebi")]
    [InlineData(RefundType.AmountBased, "kısmi geri ödeme")]
    public async Task B8_WritesTheReasonTheAmountAndTheCardMethod_AndNoIban(RefundType type, string reason)
    {
        EmailMessage message = await Compose(Guest(), new RefundProcessedNotice(OrderId, type, 266.67m, PaymentMethod.Card));

        Assert.Contains($"{Number} numaralı siparişiniz için {reason} nedeniyle 266,67 TL geri ödeme işlendi.", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Tutar, ödeme yaptığınız karta iade edildi.", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("IBAN", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("TR33", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task B8_OfATransfer_SaysItWentToTheIbanTheCustomerGave_AndWritesNoIban_EvenWhenTheOrderHasOne()
    {
        EmailMessage message = await Compose(Guest(), new RefundProcessedNotice(OrderId, RefundType.Withdrawal, 1200m, PaymentMethod.BankTransfer));

        Assert.Contains($"{Number} numaralı siparişiniz için cayma nedeniyle 1.200 TL geri ödeme işlendi.", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Tutar, bildirdiğiniz IBAN'a havale edildi.", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("TR33", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("0006", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("0006", message.HtmlBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task B8_OfALatePayment_HasItsOwnSentenceInsteadOfTheReasonSentence()
    {
        EmailMessage message = await Compose(Guest(), new RefundProcessedNotice(OrderId, RefundType.LatePayment, 1200m, PaymentMethod.Card));

        Assert.Contains($"İptal edilmiş {Number} numaralı siparişinize gelen ödeme, tutarın tamamı olarak iade edildi.", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("nedeniyle", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("İade edilen tutar: 1.200 TL", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Tutar, ödeme yaptığınız karta iade edildi.", message.TextBody, StringComparison.Ordinal);
    }

    // ---------- the event's values: written when the row is made, read when the body is made ----------

    [Fact]
    public void TheEventsValues_RoundTrip_ForEveryShapeOfTheEightNotices()
    {
        CustomerNotice[] notices =
        [
            new OrderReceivedNotice(OrderId),
            new TransferChosenNotice(OrderId, WindowEnd),
            new TransferReminderNotice(OrderId, WindowEnd),
            new PaymentConfirmedNotice(OrderId),
            new ShippedNotice(OrderId, "ArasKargo", null, "A-1", false),
            new ShippedNotice(OrderId, "ArasKargo", null, null, false),
            new ShippedNotice(OrderId, null, "Yerel Kurye", "K-7", false),
            new ShippedNotice(OrderId, null, null, null, true),
            new DeliveryFailedNotice(OrderId),
            new CancelledNotice(OrderId, ActorKind.Buyer, null, null, null, null),
            new CancelledNotice(OrderId, ActorKind.Buyer, null, null, null, new CancelledItem("Masa", null, "120 cm", 3)),
            new CancelledNotice(OrderId, ActorKind.Admin, CancellationReason.Other, "açıklama", null, null),
            new CancelledNotice(OrderId, ActorKind.Admin, CancellationReason.OutOfStock, null, null, new CancelledItem("Masa", "Meşe", "120 cm", 2)),
            new CancelledNotice(OrderId, ActorKind.System, null, null, TransitionReason.SameCartReorder, null),
            new RefundProcessedNotice(OrderId, RefundType.AmountBased, 266.67m, PaymentMethod.BankTransfer),
        ];

        foreach (CustomerNotice notice in notices)
        {
            CustomerNoticeRow row = CustomerNoticeValues.For(notice);
            Assert.Equal(notice, CustomerNoticeValues.Read(row.Type, OrderId, row.EventValues));
        }
    }

    [Fact]
    public void TheEventsValues_HoldOnlyWhatTheEventHolds_NoNameNoAddressNoIbanNoOrderNumber()
    {
        string[] stored =
        [
            CustomerNoticeValues.For(new TransferChosenNotice(OrderId, WindowEnd)).EventValues!,
            CustomerNoticeValues.For(new ShippedNotice(OrderId, "ArasKargo", null, "A-1", false)).EventValues!,
            CustomerNoticeValues.For(new CancelledNotice(OrderId, ActorKind.Admin, CancellationReason.OutOfStock, null, null, null)).EventValues!,
            CustomerNoticeValues.For(new RefundProcessedNotice(OrderId, RefundType.Cancellation, 10m, PaymentMethod.BankTransfer)).EventValues!,
        ];

        Assert.All(stored, values =>
        {
            Assert.DoesNotContain("TR33", values, StringComparison.Ordinal);
            Assert.DoesNotContain(Number, values, StringComparison.Ordinal);
            Assert.DoesNotContain(OrderId.ToString("D"), values, StringComparison.Ordinal);
        });
        Assert.Null(CustomerNoticeValues.For(new OrderReceivedNotice(OrderId)).EventValues);
        Assert.Null(CustomerNoticeValues.For(new PaymentConfirmedNotice(OrderId)).EventValues);
        Assert.Null(CustomerNoticeValues.For(new DeliveryFailedNotice(OrderId)).EventValues);
    }

    [Fact]
    public void TheB1AfterACorrection_CarriesTheFirstEmailsRowId()
    {
        Guid first = Guid.Parse("018f0000-0000-7000-8000-0000000000c3");

        Assert.Equal(first, CustomerNoticeValues.For(new OrderReceivedNotice(OrderId, first)).OriginalDispatchId);
        Assert.Null(CustomerNoticeValues.For(new OrderReceivedNotice(OrderId)).OriginalDispatchId);
    }

    [Fact]
    public void ANoticeThatBreaksTheRuleOfItsType_IsRefused_WhenTheRowIsMade()
    {
        CancelledItem item = new("Masa", null, null, 1);
        CustomerNotice[] refused =
        [
            new CancelledNotice(OrderId, ActorKind.Buyer, CancellationReason.OutOfStock, null, null, null),
            new CancelledNotice(OrderId, ActorKind.Buyer, null, null, TransitionReason.SameCartReorder, null),
            new CancelledNotice(OrderId, ActorKind.Admin, null, null, null, null),
            new CancelledNotice(OrderId, ActorKind.Admin, CancellationReason.Other, " ", null, null),
            new CancelledNotice(OrderId, ActorKind.Admin, CancellationReason.OutOfStock, null, TransitionReason.SameCartReorder, null),
            new CancelledNotice(OrderId, ActorKind.System, null, null, null, null),
            new CancelledNotice(OrderId, ActorKind.System, null, null, TransitionReason.Closure, null),
            new CancelledNotice(OrderId, ActorKind.System, null, null, TransitionReason.PaymentWindowExpired, item),
            new CancelledNotice(OrderId, ActorKind.Buyer, null, null, null, new CancelledItem("Masa", null, null, 0)),
            new CancelledNotice(OrderId, ActorKind.Buyer, null, null, null, new CancelledItem(" ", null, null, 1)),
            new ShippedNotice(OrderId, "ArasKargo", "Yerel Kurye", "A-1", false),
            new ShippedNotice(OrderId, null, null, "A-1", false),
            new ShippedNotice(OrderId, "ArasKargo", null, "A-1", true),
            new ShippedNotice(OrderId, null, null, "A-1", true),
            new RefundProcessedNotice(OrderId, RefundType.Cancellation, 0m, PaymentMethod.Card),
            new RefundProcessedNotice(OrderId, RefundType.Cancellation, 1.005m, PaymentMethod.Card),
            new RefundProcessedNotice(OrderId, (RefundType)99, 10m, PaymentMethod.Card),
            new RefundProcessedNotice(OrderId, RefundType.Cancellation, 10m, (PaymentMethod)99),
            new PaymentConfirmedNotice(Guid.Empty),
        ];

        Assert.All(refused, notice => Assert.ThrowsAny<ArgumentException>(() => CustomerNoticeValues.For(notice)));
    }

    [Theory]
    [InlineData(DispatchType.B2, null)]
    [InlineData(DispatchType.B2, "")]
    [InlineData(DispatchType.B2, "not json")]
    [InlineData(DispatchType.B2, "{}")]
    [InlineData(DispatchType.B2, "[{\"Name\":\"WindowEndsAt\",\"Value\":\"yarın\"}]")]
    [InlineData(DispatchType.B3, "[1,2]")]
    [InlineData(DispatchType.B5, "[]")]
    [InlineData(DispatchType.B5, "[{\"Name\":\"Carrier\",\"Value\":\"A\"},{\"Name\":\"OtherCarrierName\",\"Value\":\"B\"}]")]
    [InlineData(DispatchType.B7, "[{\"Name\":\"Actor\",\"Value\":\"Admin\"}]")]
    [InlineData(DispatchType.B7, "[{\"Name\":\"Actor\",\"Value\":\"System\"},{\"Name\":\"SystemReason\",\"Value\":\"Closure\"}]")]
    [InlineData(DispatchType.B8, "[{\"Name\":\"RefundType\",\"Value\":\"Cancellation\"},{\"Name\":\"Amount\",\"Value\":\"-5\"},{\"Name\":\"Method\",\"Value\":\"Card\"}]")]
    [InlineData(DispatchType.F1, "[]")]
    [InlineData(DispatchType.PasswordResetLink, null)]
    public void AStoredListThatIsMissingOrMalformed_IsNotANotice_AndTheReaderDoesNotThrow(DispatchType type, string? values)
    {
        Assert.Null(CustomerNoticeValues.Read(type, OrderId, values));
    }

    // ---------- the body is HTML of the common frame ----------

    [Theory]
    [MemberData(nameof(EightIndexes))]
    public async Task TheTwoParts_CarryTheSameAddresses_AndTheHtmlHasNoUnencodedOrderValue(int index)
    {
        EmailMessage message = await Compose(Guest(), AllEight().ElementAt(index));

        foreach (string address in message.TextBody.Split([' ', '\n'], StringSplitOptions.RemoveEmptyEntries).Where(w => w.StartsWith("https://", StringComparison.Ordinal)))
        {
            Assert.Contains(address, message.HtmlBody, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("<script", message.HtmlBody, StringComparison.OrdinalIgnoreCase);
    }

    private static int CountOf(string text, string part)
    {
        int count = 0;
        for (int i = text.IndexOf(part, StringComparison.Ordinal); i >= 0; i = text.IndexOf(part, i + part.Length, StringComparison.Ordinal))
        {
            count++;
        }

        return count;
    }
}
