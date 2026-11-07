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

namespace Shopfolio.UnitTests.Notifications;

/// <summary>
/// The bodies of the customer notices B-9…B-16 (T307 criteria 1-10, KK1-KK10; 08 2.2.6; 09 11.4 item 7): the four headings and the date forms of B-9, its items and its return
/// address, the new address of B-10 alone, the item of B-11 without an amount, the corrected shipping block of B-12, the status names and the paid-mark section of B-13, the six
/// sentences and the missing IBAN of B-14, the button-less B-15, the reason and the dispute ways of B-16, the firm's contact, the re-send sentences and the event values' round
/// trip. Every notice goes through the real composer. No network and no database.
/// </summary>
public sealed class CustomerNoticeAfterOrderTemplateTests
{
    private const string Domain = "magaza.example.com";
    private const string Recipient = "musteri@example.com";
    private const string Number = "2026-ABC123";
    private const string AccessKey = "erisim-anahtari-1";
    private const string Iban = "TR330006100519786457841326";
    private const string FirmEmail = "destek@magaza.example.com";
    private const string FirmPhone = "3121234567";
    private const string Legal = "METIN";
    private const string FirmContactEmailLine = "Firma e-postası: " + FirmEmail;
    private const string FirmContactPhoneLine = "Firma telefonu: 0312 123 45 67";
    private const string OrderPageButton = "Siparişi görüntüle";

    private static readonly Guid OrderId = Guid.Parse("018f0000-0000-7000-8000-0000000000a1");
    private static readonly Guid RecordId = Guid.Parse("018f0000-0000-7000-8000-0000000000b2");
    private static readonly Guid Ankara = Guid.Parse("018f0000-0000-7000-8000-0000000000c3");
    private static readonly Guid Izmir = Guid.Parse("018f0000-0000-7000-8000-0000000000c4");
    private static readonly DateTimeOffset FirstAt = new(2026, 10, 4, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly DeclaredOn = new(2026, 10, 5);
    private static readonly DateTimeOffset ClaimAt = new(2026, 10, 6, 11, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset WindowEnd = new(2026, 11, 10, 21, 0, 0, TimeSpan.Zero);

    private static readonly NoticeItem Table = new("Ahşap Masa", "Meşe", "120 cm", 2);
    private static readonly NoticeItem Lamp = new("Masa Lambası", null, null, 1);
    private static readonly NoticeAddress ReturnAddress = new("İade Merkezi", "Depo Cad. No: 5", "Çankaya", Ankara, "3129876543");
    private static readonly NoticeAddress NewShipping = new("Ali Veli", "Yeni Sok. No: 7 Daire: 3", "Konak", Izmir, "5321234567");

    private static readonly ServiceProvider _services = new ServiceCollection().AddLogging().BuildServiceProvider();
    private static readonly EmailFirm _firm = new("Örnek Ticaret Anonim Şirketi", FirmEmail);
    private static readonly EmailBranding _brand = new("Örnek Mağaza", null, null);
    private static readonly Dictionary<Guid, string> Provinces = new() { [Ankara] = "Ankara", [Izmir] = "İzmir" };

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static IOptions<SiteOptions> Site() => Options.Create(new SiteOptions { Domain = Domain });

    private static EmailLinks Links() => new(new SiteAddress(Site()));

    private static CustomerNoticeTemplates Templates() => new(Links(), CarrierCatalog.Default);

    private static OrderNoticeContext Guest(DateTimeOffset? resendOf = null) =>
        new(Number, AccessKey, true, false, Iban, false, Legal, Legal, FirmEmail, FirmPhone, resendOf, Provinces);

    private static OrderNoticeContext Member() =>
        new(Number, null, false, false, Iban, false, Legal, Legal, FirmEmail, FirmPhone, null, Provinces);

    private static Task<EmailMessage> Compose(OrderNoticeContext order, CustomerNotice notice)
    {
        EmailComposer composer = new(Links(), Site(), _services, _services.GetRequiredService<ILoggerFactory>());
        return composer.ComposeAsync(Recipient, _brand, _firm, Templates().Compose(order, notice), Ct);
    }

    private static string Decoded(string html) => WebUtility.HtmlDecode(html);

    private static DeclarationReceivedNotice Withdrawal(NoticeAddress? address = null, params NoticeItem[] items) =>
        new(OrderId, DispatchRelatedRecordType.Withdrawal, RecordId, false, DeclaredOn, null, items.Length == 0 ? [Table] : items, address);

    private static DeclarationReceivedNotice Termination() =>
        new(OrderId, DispatchRelatedRecordType.LateDeliveryTermination, RecordId, false, DeclaredOn, null, [Table], null);

    private static DeclarationReceivedNotice Claim(NoticeAddress? address = null) =>
        new(OrderId, DispatchRelatedRecordType.DefectClaim, RecordId, false, null, ClaimAt, [Lamp], address);

    private static DeclarationReceivedNotice Reopened() =>
        new(OrderId, DispatchRelatedRecordType.DefectClaim, RecordId, true, null, ClaimAt, [Lamp], null);

    private static IEnumerable<CustomerNotice> AllEight()
    {
        yield return Withdrawal(ReturnAddress);
        yield return new AddressCorrectedNotice(OrderId, AddressType.ShippingAddress, NewShipping);
        yield return new ItemRemovedNotice(OrderId, Table);
        yield return new CarrierCorrectedNotice(OrderId, "YurticiKargo", null, "YT999", false);
        yield return new StatusCorrectedNotice(OrderId, OrderStatus.Preparing, null);
        yield return new RefundIbanRequestedNotice(OrderId, RefundIbanRequestReason.AdminRequest);
        yield return new OrderEmailChangedNotice(OrderId, FirstAt);
        yield return new ReturnRejectedNotice(OrderId, Lamp, WithdrawalExemptionReason.SealedMedia);
    }

    public static TheoryData<int> EightIndexes() => [0, 1, 2, 3, 4, 5, 6, 7];

    // ---------- the eight types; the skeleton ----------

    [Fact]
    public void TheEightNotices_AreTheClosedTypesB9ToB16_InOrder()
    {
        DispatchType[] types = [.. AllEight().Select(n => CustomerNoticeValues.For(n).Type)];

        Assert.Equal(
            [DispatchType.B9, DispatchType.B10, DispatchType.B11, DispatchType.B12, DispatchType.B13, DispatchType.B14, DispatchType.B15, DispatchType.B16],
            types);
    }

    [Theory]
    [MemberData(nameof(EightIndexes))]
    public async Task EveryNotice_StartsWithMerhaba_WithoutAName_AndTheOrderNumberIsInTheSubjectAndTheSentence(int index)
    {
        EmailMessage message = await Compose(Guest(), AllEight().ElementAt(index));

        Assert.EndsWith($" (Sipariş {Number})", message.Subject, StringComparison.Ordinal);
        string[] lines = message.TextBody.Split('\n');
        Assert.Equal("Merhaba,", lines[2]);

        // The order number is in the event's sentence; B-14's event sentence is the cause and the number follows in the request's sentence (08 2.2.6 B-14).
        string sentences = string.Join('\n', lines.Skip(4).Take(index == 5 ? 4 : 1));
        Assert.Contains(Number, sentences, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheSubjects_AreTheTitlesOfTheSpec()
    {
        string[] expected =
        [
            "Örnek Mağaza: Cayma beyanınız alındı (Sipariş 2026-ABC123)",
            "Örnek Mağaza: Siparişinizin adresi düzeltildi (Sipariş 2026-ABC123)",
            "Örnek Mağaza: Siparişinizdeki bir kalem değişti (Sipariş 2026-ABC123)",
            "Örnek Mağaza: Kargo bilginiz düzeltildi (Sipariş 2026-ABC123)",
            "Örnek Mağaza: Siparişinizin durumu düzeltildi (Sipariş 2026-ABC123)",
            "Örnek Mağaza: Geri ödemeniz için IBAN bilgisi gerekli (Sipariş 2026-ABC123)",
            "Örnek Mağaza: Siparişinizin e-posta adresi değiştirildi (Sipariş 2026-ABC123)",
            "Örnek Mağaza: İade ettiğiniz ürün kabul edilmedi (Sipariş 2026-ABC123)",
        ];
        List<string> actual = [];
        foreach (CustomerNotice notice in AllEight())
        {
            actual.Add((await Compose(Guest(), notice)).Subject);
        }

        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(EightIndexes))]
    public async Task ThePageButton_IsTheAccessKeyAddressOfAGuestOrder_AndTheAccountAddressOfAMembers_ExceptB15(int index)
    {
        CustomerNotice notice = AllEight().ElementAt(index);
        EmailMessage guest = await Compose(Guest(), notice);
        EmailMessage member = await Compose(Member(), notice);

        if (notice is OrderEmailChangedNotice)
        {
            // B-15 has no button and no address of the order, whoever the order belongs to (T307 criterion 8).
            Assert.DoesNotContain("https://", guest.TextBody, StringComparison.Ordinal);
            Assert.DoesNotContain(OrderPageButton, guest.TextBody, StringComparison.Ordinal);
            Assert.DoesNotContain(AccessKey, guest.HtmlBody, StringComparison.Ordinal);
            Assert.DoesNotContain("https://", member.TextBody, StringComparison.Ordinal);
            return;
        }

        Assert.Contains($"https://{Domain}/siparis/erisim/{AccessKey}", guest.TextBody, StringComparison.Ordinal);
        Assert.Contains($"https://{Domain}/siparis/{Number}", member.TextBody, StringComparison.Ordinal);
    }

    // ---------- KK1: B-9 ----------

    [Theory]
    [InlineData(0, "Cayma beyanınız alındı")]
    [InlineData(1, "Gecikme nedeniyle fesih beyanınız alındı")]
    [InlineData(2, "Ayıp talebiniz alındı")]
    [InlineData(3, "Ayıp talebiniz yeniden açıldı")]
    public async Task B9_HasFourHeadings_EachWithTheOrderNumberAtTheEnd(int index, string heading)
    {
        CustomerNotice[] four = [Withdrawal(ReturnAddress), Termination(), Claim(ReturnAddress), Reopened()];
        EmailMessage message = await Compose(Guest(), four[index]);

        Assert.Equal($"Örnek Mağaza: {heading} (Sipariş {Number})", message.Subject);
    }

    [Fact]
    public async Task B9_OfAWithdrawalAndATermination_WritesTheDateAlone_NoTime()
    {
        EmailMessage withdrawal = await Compose(Guest(), Withdrawal(ReturnAddress));
        EmailMessage termination = await Compose(Guest(), Termination());

        Assert.Contains($"{Number} numaralı siparişinizde cayma beyanınız 5 Ekim 2026 tarihinde kaydedildi.", withdrawal.TextBody, StringComparison.Ordinal);
        Assert.Contains($"{Number} numaralı siparişinizde gecikme nedeniyle fesih beyanınız 5 Ekim 2026 tarihinde kaydedildi.", termination.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("5 Ekim 2026,", withdrawal.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("5 Ekim 2026,", termination.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task B9_OfADefectClaimAndItsReopening_WritesTheDateAndTheTime_InTheTurkeyClock()
    {
        EmailMessage claim = await Compose(Guest(), Claim());
        EmailMessage reopened = await Compose(Guest(), Reopened());

        Assert.Contains($"{Number} numaralı siparişinizde ayıp talebiniz 6 Ekim 2026, 14:30 tarihinde kaydedildi.", claim.TextBody, StringComparison.Ordinal);
        Assert.Contains($"{Number} numaralı siparişinizdeki ayıp talebiniz 6 Ekim 2026, 14:30 tarihinde yeniden açıldı.", reopened.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task B9_ListsTheItemsAndTheQuantities_WithTheOptionValuesInBrackets()
    {
        EmailMessage message = await Compose(Guest(), Withdrawal(null, Table, Lamp));

        Assert.Contains("Kalem: Ahşap Masa (Meşe, 120 cm), 2 adet", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Kalem: Masa Lambası, 1 adet", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Ahşap Masa (Meşe, 120 cm), 2 adet", Decoded(message.HtmlBody), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public async Task B9_OfAWithdrawalAndOfAClaim_WritesTheReturnAddress_NameAddressDistrictProvinceAndTelephone(int index)
    {
        CustomerNotice[] two = [Withdrawal(ReturnAddress), Claim(ReturnAddress), Claim(ReturnAddress)];
        EmailMessage message = await Compose(Guest(), two[index]);

        Assert.Contains("İade adresi:", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Alıcı adı: İade Merkezi", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Adres: Depo Cad. No: 5", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("İlçe: Çankaya", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("İl: Ankara", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Telefon: 0312 987 65 43", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task B9_OfATermination_HasNoReturnAddress_AndOneThatCarriesItIsRefused()
    {
        EmailMessage message = await Compose(Guest(), Termination());

        Assert.DoesNotContain("İade adresi", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("Depo Cad.", message.TextBody, StringComparison.Ordinal);
        Assert.Throws<ArgumentException>(() => CustomerNoticeValues.For(Termination() with { ReturnAddress = ReturnAddress }));
    }

    [Fact]
    public async Task B9_OfADigitalOnlyDeclaration_WithoutAReturnAddress_WritesNone()
    {
        EmailMessage message = await Compose(Guest(), Withdrawal(null, Lamp));

        Assert.DoesNotContain("İade adresi", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public void B9_NamesItsRelatedRecord_ByTheKindOfTheDeclaration_AndTheReopeningIsTheClaimsRecord()
    {
        CustomerNoticeRow withdrawal = CustomerNoticeValues.For(Withdrawal(ReturnAddress));
        CustomerNoticeRow termination = CustomerNoticeValues.For(Termination());
        CustomerNoticeRow claim = CustomerNoticeValues.For(Claim());
        CustomerNoticeRow reopened = CustomerNoticeValues.For(Reopened());

        Assert.Equal((DispatchRelatedRecordType.Withdrawal, RecordId), (withdrawal.RelatedRecordType!.Value, withdrawal.RelatedRecordId!.Value));
        Assert.Equal((DispatchRelatedRecordType.LateDeliveryTermination, RecordId), (termination.RelatedRecordType!.Value, termination.RelatedRecordId!.Value));
        Assert.Equal((DispatchRelatedRecordType.DefectClaim, RecordId), (claim.RelatedRecordType!.Value, claim.RelatedRecordId!.Value));
        Assert.Equal((DispatchRelatedRecordType.DefectClaim, RecordId), (reopened.RelatedRecordType!.Value, reopened.RelatedRecordId!.Value));
        Assert.All(
            [withdrawal, termination, claim, reopened],
            row => Assert.Equal(DispatchType.B9, row.Type));

        // The reopening is told apart by the event's values, not by the row's group (08 2.2.6 B-9).
        Assert.Contains("Reopened", reopened.EventValues, StringComparison.Ordinal);
        Assert.DoesNotContain("Reopened", claim.EventValues, StringComparison.Ordinal);
    }

    [Fact]
    public void TheOtherNotices_HaveNoRelatedRecord()
    {
        Assert.All(
            AllEight().Skip(1),
            notice =>
            {
                CustomerNoticeRow row = CustomerNoticeValues.For(notice);
                Assert.Null(row.RelatedRecordType);
                Assert.Null(row.RelatedRecordId);
            });
    }

    [Fact]
    public async Task B9_WritesNoLegalPeriods_ThePeriodsAreOnTheOrderPage()
    {
        EmailMessage message = await Compose(Guest(), Withdrawal(ReturnAddress));

        Assert.DoesNotContain("gün içinde", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("on dört", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task B9_AMarkupInAnItemOrAnAddress_IsTextInTheHtml()
    {
        NoticeAddress hostile = new("<i>Ali</i>", "Sok. <script>x</script>", "Çankaya", Ankara, null);
        EmailMessage message = await Compose(Guest(), Withdrawal(hostile, new NoticeItem("Masa <b>özel</b>", null, null, 1)));

        Assert.DoesNotContain("<script>", message.HtmlBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<b>özel", message.HtmlBody, StringComparison.Ordinal);
        Assert.DoesNotContain("<i>Ali", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("Masa <b>özel</b>, 1 adet", message.TextBody, StringComparison.Ordinal);
    }

    // ---------- KK2: B-10 ----------

    [Fact]
    public async Task B10_WritesTheNewAddressOfTheDelivery_WithTheTelephone_AndNoOldAddress()
    {
        EmailMessage message = await Compose(Guest(), new AddressCorrectedNotice(OrderId, AddressType.ShippingAddress, NewShipping));

        Assert.Contains($"{Number} numaralı siparişinizin teslimat adresi düzeltildi. Yeni adres:", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Alıcı adı: Ali Veli", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Adres: Yeni Sok. No: 7 Daire: 3", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("İlçe: Konak", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("İl: İzmir", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Telefon: 0532 123 45 67", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("Eski", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("eski adres", message.TextBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task B10_OfTheBilling_WritesNoTelephone_AndTheValuesHoldNoOldAddress()
    {
        NoticeAddress billing = new("Ali Veli", "Fatura Sok. 1", "Konak", Izmir, null);
        AddressCorrectedNotice notice = new(OrderId, AddressType.BillingAddress, billing);
        EmailMessage message = await Compose(Guest(), notice);

        Assert.Contains($"{Number} numaralı siparişinizin fatura adresi düzeltildi. Yeni adres:", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("Telefon:", message.TextBody, StringComparison.Ordinal);
        string stored = CustomerNoticeValues.For(notice).EventValues!;
        Assert.DoesNotContain("Old", stored, StringComparison.Ordinal);
        Assert.All(["NewRecipientName", "NewAddressLine", "NewDistrict", "NewProvinceId"], key => Assert.Contains(key, stored, StringComparison.Ordinal));
    }

    [Fact]
    public void B10_ADeliveryAddressWithoutATelephone_AndABillingAddressWithOne_AreRefused()
    {
        Assert.Throws<ArgumentException>(() => CustomerNoticeValues.For(new AddressCorrectedNotice(OrderId, AddressType.ShippingAddress, NewShipping with { Phone = null })));
        Assert.Throws<ArgumentException>(() => CustomerNoticeValues.For(new AddressCorrectedNotice(OrderId, AddressType.BillingAddress, NewShipping)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    public async Task B10AndB15_WriteTheFirmsEmailAndTelephone_AndTheNotYouSentence(int index)
    {
        EmailMessage message = await Compose(Guest(), AllEight().ElementAt(index));

        Assert.Contains(FirmContactEmailLine, message.TextBody, StringComparison.Ordinal);
        Assert.Contains(FirmContactPhoneLine, message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Bu değişikliği siz istemediyseniz lütfen firmayla iletişime geçin.", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task B16_WritesTheFirmsContact_ButNotTheNotYouSentence()
    {
        EmailMessage message = await Compose(Guest(), AllEight().ElementAt(7));

        Assert.Contains(FirmContactEmailLine, message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("istemediyseniz", message.TextBody, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("", FirmPhone, 1)]
    [InlineData(FirmEmail, "", 1)]
    [InlineData(" ", FirmPhone, 6)]
    [InlineData(FirmEmail, " ", 7)]
    public void B10AndB15AndB16_WithoutTheFirmsEmailOrTelephone_AreRefused(string email, string phone, int index)
    {
        OrderNoticeContext order = new(Number, AccessKey, true, false, Iban, false, Legal, Legal, email, phone, null, Provinces);

        Assert.Throws<ArgumentException>(() => Templates().Compose(order, AllEight().ElementAt(index)));
    }

    [Fact]
    public void AnAddressWithoutTheProvincesName_IsRefused_WhenTheBodyIsMade()
    {
        OrderNoticeContext order = new(Number, AccessKey, true, false, Iban, false, Legal, Legal, FirmEmail, FirmPhone, null);

        Assert.Throws<ArgumentException>(() => Templates().Compose(order, AllEight().ElementAt(1)));
        Assert.Throws<ArgumentException>(() => Templates().Compose(order, Withdrawal(ReturnAddress)));
    }

    // ---------- KK3: B-11 ----------

    [Fact]
    public async Task B11_WritesTheItemAndTheQuantity_AndNoAmount()
    {
        EmailMessage message = await Compose(Guest(), new ItemRemovedNotice(OrderId, Table));

        Assert.Contains($"{Number} numaralı siparişinizden Ahşap Masa (Meşe, 120 cm), 2 adet çıkarıldı.", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("TL", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("geri ödeme", message.TextBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IBAN", message.TextBody, StringComparison.Ordinal);
    }

    // ---------- KK4: B-12 ----------

    [Fact]
    public async Task B12_WritesTheCorrectedShippingBlock_OfAListedCompany_WithTheTrackingLink()
    {
        EmailMessage message = await Compose(Guest(), new CarrierCorrectedNotice(OrderId, "YurticiKargo", null, "YT999", false));

        Assert.Contains($"{Number} numaralı siparişinizin kargo bilgisi düzeltildi.", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Takip numarası: YT999", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Takip et", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("rel=\"noreferrer", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("YT999", message.HtmlBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task B12_OfAnotherCompany_WritesTheNameAndTheNumber_AndOfOwnVehicleTheDeclarationAlone()
    {
        EmailMessage other = await Compose(Guest(), new CarrierCorrectedNotice(OrderId, null, "Yerel Kurye", "K-7", false));
        EmailMessage own = await Compose(Guest(), new CarrierCorrectedNotice(OrderId, null, null, null, true));

        Assert.Contains("Kargo şirketi: Yerel Kurye", other.TextBody, StringComparison.Ordinal);
        Assert.Contains("Takip numarası: K-7", other.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("Takip et", other.TextBody, StringComparison.Ordinal);
        Assert.Contains("Siparişiniz firmamızın kendi aracıyla teslim edilecektir.", own.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("Kargo şirketi", own.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("Takip numarası", own.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task B12_IsTheShippedBlockOfB5_WithTheValuesOfTheCorrection()
    {
        EmailMessage shipped = await Compose(Guest(), new ShippedNotice(OrderId, "YurticiKargo", null, "YT123", false));
        EmailMessage corrected = await Compose(Guest(), new CarrierCorrectedNotice(OrderId, "YurticiKargo", null, "YT999", false));

        Assert.Contains("YT123", shipped.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("YT123", corrected.TextBody, StringComparison.Ordinal);
        Assert.Contains("YT999", corrected.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public void B12_BreakingTheRuleOfB5_IsRefused()
    {
        CustomerNotice[] refused =
        [
            new CarrierCorrectedNotice(OrderId, "YurticiKargo", "Yerel Kurye", "A", false),
            new CarrierCorrectedNotice(OrderId, null, null, "A", false),
            new CarrierCorrectedNotice(OrderId, "YurticiKargo", null, "A", true),
        ];

        Assert.All(refused, notice => Assert.ThrowsAny<ArgumentException>(() => CustomerNoticeValues.For(notice)));
    }

    // ---------- KK5 and KK6: B-13 ----------

    [Theory]
    [InlineData(OrderStatus.Placed, "Alındı")]
    [InlineData(OrderStatus.Preparing, "Hazırlanıyor")]
    [InlineData(OrderStatus.Shipped, "Kargoya verildi")]
    [InlineData(OrderStatus.Delivered, "Teslim edildi")]
    [InlineData(OrderStatus.DeliveryFailed, "Teslim edilemedi")]
    [InlineData(OrderStatus.Cancelled, "İptal edildi")]
    public async Task B13_WritesTheStatusNameOfTheClosedList_AndNoReason(OrderStatus status, string name)
    {
        EmailMessage message = await Compose(Guest(), new StatusCorrectedNotice(OrderId, status, null));

        Assert.Contains($"{Number} numaralı siparişinizin durumu yanlışlıkla güncellenmişti ve düzeltildi. Siparişinizin durumu: {name}.", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("Sebep", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("IBAN", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("Ödemeniz alınmış görünmüyor", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task B13_OfThePaidMarksCorrection_WritesTheNewLastPaymentDay_AndTheFrozenIbanBlock()
    {
        EmailMessage message = await Compose(Guest(), new StatusCorrectedNotice(OrderId, OrderStatus.Placed, WindowEnd));

        Assert.Contains("Siparişinizin durumu: Alındı.", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Ödemeniz alınmış görünmüyor. Ödemeyi 10 Kasım 2026 tarihine kadar yapmanız gerekir.", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("IBAN: TR33 0006 1005 1978 6457 8413 26", message.TextBody, StringComparison.Ordinal);
        Assert.Contains($"Havale açıklamasına yazılacak sipariş numarası: {Number}", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public void B13_OfThePaidMarksCorrection_WithoutAFrozenIban_IsRefused()
    {
        OrderNoticeContext order = new(Number, AccessKey, true, false, null, false, Legal, Legal, FirmEmail, FirmPhone, null);

        Assert.Throws<ArgumentException>(() => Templates().Compose(order, new StatusCorrectedNotice(OrderId, OrderStatus.Placed, WindowEnd)));
    }

    [Fact]
    public void B13_AWindowEndWithAnotherStatusThanPlaced_AndAnUnknownStatus_AreRefused()
    {
        Assert.Throws<ArgumentException>(() => CustomerNoticeValues.For(new StatusCorrectedNotice(OrderId, OrderStatus.Shipped, WindowEnd)));
        Assert.Throws<ArgumentOutOfRangeException>(() => CustomerNoticeValues.For(new StatusCorrectedNotice(OrderId, (OrderStatus)99, null)));
    }

    // ---------- KK7: B-14 ----------

    [Theory]
    [InlineData(RefundIbanRequestReason.ReturnAfterIbanDeleted, "İade ettiğiniz ürün firmamıza ulaştı.")]
    [InlineData(RefundIbanRequestReason.TransferFailed, "Geri ödeme havalesi girdiğiniz IBAN'a gerçekleşmedi.")]
    [InlineData(RefundIbanRequestReason.CardRefundFailed, "Kartınıza yapılan iade gerçekleşmedi. Geri ödemeyi IBAN'ınıza havale yoluyla almayı seçebilirsiniz; IBAN girmek sizin seçiminizdir.")]
    [InlineData(RefundIbanRequestReason.NoticeWithoutIban, "Firmaya başka bir yoldan ilettiğiniz cayma ya da fesih bildirimi kaydedildi; bildirim IBAN içermiyordu.")]
    [InlineData(RefundIbanRequestReason.CancellationOrRemoval, "Ödenmiş bir kalem iptal edildi ya da siparişten çıkarıldı.")]
    [InlineData(RefundIbanRequestReason.AdminRequest, "Firma, bir geri ödeme için IBAN'ınızı istiyor.")]
    public async Task B14_IsOneNotice_WithTheSentenceOfTheEvent(RefundIbanRequestReason reason, string sentence)
    {
        EmailMessage message = await Compose(Guest(), new RefundIbanRequestedNotice(OrderId, reason));

        Assert.Equal($"Örnek Mağaza: Geri ödemeniz için IBAN bilgisi gerekli (Sipariş {Number})", message.Subject);
        Assert.Contains(sentence, message.TextBody, StringComparison.Ordinal);
        Assert.Contains($"{Number} numaralı siparişinizin geri ödemesini yapabilmemiz için IBAN'ınızı sipariş sayfanızdan girmeniz gerekiyor.", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task B14_HasItsOwnButtonLabel_CarriesNoIban_AndSaysNotToReplyWithAnIban()
    {
        // The order holds an IBAN; the e-mail never writes it (K-1251).
        EmailMessage message = await Compose(Guest(), new RefundIbanRequestedNotice(OrderId, RefundIbanRequestReason.TransferFailed));

        Assert.Contains("IBAN'ı sipariş sayfasından gir", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain(OrderPageButton, message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("TR33", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("TR33", message.HtmlBody, StringComparison.Ordinal);
        Assert.DoesNotContain("0006 1005", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("IBAN'ınızı bu e-postayı yanıtlayarak göndermeyin; yalnız sipariş sayfasından girin.", message.TextBody, StringComparison.Ordinal);
        Assert.Contains($"https://{Domain}/siparis/erisim/{AccessKey}", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("IBAN'ı sipariş sayfasından gir", Decoded(message.HtmlBody), StringComparison.Ordinal);
    }

    [Fact]
    public async Task B14_TheSecurityLine_FollowsTheButton()
    {
        EmailMessage message = await Compose(Guest(), new RefundIbanRequestedNotice(OrderId, RefundIbanRequestReason.AdminRequest));

        Assert.True(
            message.TextBody.IndexOf("IBAN'ı sipariş sayfasından gir", StringComparison.Ordinal)
            < message.TextBody.IndexOf("yanıtlayarak göndermeyin", StringComparison.Ordinal));
    }

    [Fact]
    public void B14_AnUnknownReason_IsRefused()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CustomerNoticeValues.For(new RefundIbanRequestedNotice(OrderId, (RefundIbanRequestReason)99)));
    }

    // ---------- KK8: B-15 ----------

    [Fact]
    public async Task B15_WritesTheDateOfTheChange_AndNoNewAddressNoNameNoAddressNoLinkAndNoButton()
    {
        EmailMessage message = await Compose(Guest(), new OrderEmailChangedNotice(OrderId, FirstAt));

        Assert.Contains($"{Number} numaralı siparişin e-posta adresi 4 Ekim 2026 tarihinde değiştirildi.", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Bu değişikliği siz istemediyseniz lütfen firmayla iletişime geçin.", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("Yeni", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("https://", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain(OrderPageButton, message.HtmlBody, StringComparison.Ordinal);
        Assert.DoesNotContain(AccessKey, message.HtmlBody, StringComparison.Ordinal);
        Assert.DoesNotContain(Recipient, message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task B15_OfAMembersOrder_HasNoAccountLinkEither()
    {
        EmailMessage message = await Compose(Member(), new OrderEmailChangedNotice(OrderId, FirstAt));

        Assert.DoesNotContain($"/siparis/{Number}", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain($"/siparis/{Number}", message.HtmlBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task B15_WorksForAnOrderWithNoAccessKeyAtAll_BecauseItWritesNoLink()
    {
        OrderNoticeContext order = new(Number, null, true, false, null, false, null, null, FirmEmail, FirmPhone, null);

        EmailMessage message = await Compose(order, new OrderEmailChangedNotice(OrderId, FirstAt));

        Assert.Contains("değiştirildi", message.TextBody, StringComparison.Ordinal);
    }

    // ---------- KK9: B-16 ----------

    [Theory]
    [InlineData(WithdrawalExemptionReason.HygieneSensitive)]
    [InlineData(WithdrawalExemptionReason.SealedMedia)]
    public async Task B16_WritesTheItem_TheReason_NoRefund_TheReturnShipment_TheDisputeWays_AndTheFirmsContact(WithdrawalExemptionReason reason)
    {
        EmailMessage message = await Compose(Guest(), new ReturnRejectedNotice(OrderId, Table, reason));

        Assert.Contains($"{Number} numaralı siparişinizde iade ettiğiniz Ahşap Masa (Meşe, 120 cm), 2 adet kabul edilmedi.", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Ürünün koruyucu ambalajı açılmıştı; bu ürün için cayma hakkı ambalajın açılmasıyla sona erer.", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Bu nedenle geri ödeme yapılmayacaktır.", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("Ürün, siparişteki teslimat adresinize firmamızın bedeliyle geri gönderilecektir.", message.TextBody, StringComparison.Ordinal);
        Assert.Contains(
            "Bu karara itirazınız varsa tüketici hakem heyetine ya da — dava açmadan önce arabulucuya başvurmanız şartıyla — tüketici mahkemesine başvurabilirsiniz.",
            message.TextBody,
            StringComparison.Ordinal);
        Assert.Contains(FirmContactEmailLine, message.TextBody, StringComparison.Ordinal);
        Assert.Contains(FirmContactPhoneLine, message.TextBody, StringComparison.Ordinal);
        Assert.Contains($"https://{Domain}/siparis/erisim/{AccessKey}", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task B16_WritesNoMonetaryLimit_AsAFigure()
    {
        EmailMessage message = await Compose(Guest(), new ReturnRejectedNotice(OrderId, Lamp, WithdrawalExemptionReason.SealedMedia));

        string body = message.TextBody;
        Assert.DoesNotContain("TL", body, StringComparison.Ordinal);
        Assert.DoesNotContain("₺", body, StringComparison.Ordinal);
        Assert.DoesNotContain("lira", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("bin ", body, StringComparison.Ordinal);

        // The only digits in the body are the order number, the telephone, the dates and the address of the page; none of them is a limit.
        string withoutKnown = body.Replace(Number, string.Empty, StringComparison.Ordinal)
            .Replace("0312 123 45 67", string.Empty, StringComparison.Ordinal)
            .Replace("1 adet", string.Empty, StringComparison.Ordinal);
        Assert.DoesNotContain("000", withoutKnown, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(WithdrawalExemptionReason.MadeToOrder)]
    [InlineData(WithdrawalExemptionReason.Perishable)]
    [InlineData(WithdrawalExemptionReason.ExpiryDated)]
    public void B16_ForAnyOtherExemption_IsRefused(WithdrawalExemptionReason reason)
    {
        Assert.Throws<ArgumentException>(() => CustomerNoticeValues.For(new ReturnRejectedNotice(OrderId, Lamp, reason)));
        Assert.Throws<ArgumentException>(() => Templates().Compose(Guest(), new ReturnRejectedNotice(OrderId, Lamp, reason)));
    }

    // ---------- KK10: the re-send ----------

    [Theory]
    [MemberData(nameof(EightIndexes))]
    public async Task AResend_CarriesTheSentenceOfTheFirstEmail_AndOnlyB10B12B13CarryTheCurrentInfoSentence(int index)
    {
        bool carriesNote = index is 1 or 3 or 4;
        EmailMessage message = await Compose(Guest(FirstAt), AllEight().ElementAt(index));

        string sentence = "Bu e-posta 4 Ekim 2026 tarihli bildirimin yeniden gönderilmesidir.";
        Assert.Contains(sentence, message.TextBody, StringComparison.Ordinal);
        Assert.Equal(carriesNote, message.TextBody.Contains(EmailResend.CurrentInfoSentence, StringComparison.Ordinal));
        Assert.Equal(carriesNote, Decoded(message.HtmlBody).Contains(EmailResend.CurrentInfoSentence, StringComparison.Ordinal));
        Assert.True(message.TextBody.IndexOf(sentence, StringComparison.Ordinal) < message.TextBody.IndexOf("Merhaba,", StringComparison.Ordinal));
        Assert.Equal((await Compose(Guest(), AllEight().ElementAt(index))).Subject, message.Subject);
    }

    [Theory]
    [MemberData(nameof(EightIndexes))]
    public async Task ANewEventOfTheSameType_CarriesNeitherSentence_B12Included(int index)
    {
        EmailMessage message = await Compose(Guest(), AllEight().ElementAt(index));

        Assert.DoesNotContain("yeniden gönderilmesidir", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain(EmailResend.CurrentInfoSentence, message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AResentB12_CarriesTheValuesOfTheFirstEvent_NotALaterCorrection()
    {
        // The stored values of the first correction are read back and written again; a later correction is a new row (and a new B-12).
        CarrierCorrectedNotice first = new(OrderId, "YurticiKargo", null, "YT999", false);
        CarrierCorrectedNotice later = new(OrderId, "ArasKargo", null, "AR555", false);
        CustomerNoticeRow row = CustomerNoticeValues.For(first);
        CustomerNotice stored = CustomerNoticeValues.Read(row.Type, OrderId, row.EventValues)!;

        EmailMessage resent = await Compose(Guest(FirstAt), stored);
        EmailMessage fresh = await Compose(Guest(), later);

        Assert.Contains("YT999", resent.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("AR555", resent.TextBody, StringComparison.Ordinal);
        Assert.Contains(EmailResend.CurrentInfoSentence, resent.TextBody, StringComparison.Ordinal);
        Assert.Contains("AR555", fresh.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("yeniden gönderilmesidir", fresh.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AResentB13_CarriesTheStatusAndTheWindowOfTheFirstEvent()
    {
        StatusCorrectedNotice first = new(OrderId, OrderStatus.Placed, WindowEnd);
        CustomerNoticeRow row = CustomerNoticeValues.For(first);
        EmailMessage resent = await Compose(Guest(FirstAt), CustomerNoticeValues.Read(row.Type, OrderId, row.EventValues)!);

        Assert.Contains("10 Kasım 2026 tarihine kadar", resent.TextBody, StringComparison.Ordinal);
        Assert.Contains(EmailResend.CurrentInfoSentence, resent.TextBody, StringComparison.Ordinal);
    }

    // ---------- the event's values ----------

    [Fact]
    public void TheEventsValues_RoundTrip_ForEveryShapeOfTheEightNotices()
    {
        CustomerNotice[] notices =
        [
            new AddressCorrectedNotice(OrderId, AddressType.ShippingAddress, NewShipping),
            new AddressCorrectedNotice(OrderId, AddressType.BillingAddress, NewShipping with { Phone = null }),
            new ItemRemovedNotice(OrderId, Table),
            new ItemRemovedNotice(OrderId, Lamp),
            new CarrierCorrectedNotice(OrderId, "ArasKargo", null, "A-1", false),
            new CarrierCorrectedNotice(OrderId, "ArasKargo", null, null, false),
            new CarrierCorrectedNotice(OrderId, null, "Yerel Kurye", "K-7", false),
            new CarrierCorrectedNotice(OrderId, null, null, null, true),
            new StatusCorrectedNotice(OrderId, OrderStatus.Delivered, null),
            new StatusCorrectedNotice(OrderId, OrderStatus.Placed, WindowEnd),
            new RefundIbanRequestedNotice(OrderId, RefundIbanRequestReason.CardRefundFailed),
            new OrderEmailChangedNotice(OrderId, FirstAt),
            new ReturnRejectedNotice(OrderId, Table, WithdrawalExemptionReason.HygieneSensitive),
        ];

        foreach (CustomerNotice notice in notices)
        {
            CustomerNoticeRow row = CustomerNoticeValues.For(notice);
            Assert.Equal(notice, CustomerNoticeValues.Read(row.Type, OrderId, row.EventValues));
        }
    }

    [Fact]
    public void TheEventsValues_OfB9_RoundTrip_WithTheRelatedRecordId()
    {
        DeclarationReceivedNotice[] notices =
        [
            Withdrawal(ReturnAddress, Table, Lamp),
            Withdrawal(null, Lamp),
            Termination(),
            Claim(ReturnAddress),
            Claim(),
            Reopened(),
        ];

        foreach (DeclarationReceivedNotice notice in notices)
        {
            CustomerNoticeRow row = CustomerNoticeValues.For(notice);
            DeclarationReceivedNotice read = Assert.IsType<DeclarationReceivedNotice>(CustomerNoticeValues.Read(row.Type, OrderId, row.EventValues, row.RelatedRecordId));

            Assert.Equal(notice with { Items = [] }, read with { Items = [] });
            Assert.Equal(notice.Items, read.Items);
        }
    }

    [Fact]
    public void TheEventsValues_OfB9_WithoutTheRelatedRecordId_AreNotANotice()
    {
        CustomerNoticeRow row = CustomerNoticeValues.For(Withdrawal(ReturnAddress));

        Assert.Null(CustomerNoticeValues.Read(row.Type, OrderId, row.EventValues));
        Assert.Null(CustomerNoticeValues.Read(row.Type, OrderId, row.EventValues, Guid.Empty));
    }

    [Fact]
    public void TheEventsValues_HoldOnlyWhatTheEventHolds_NoIbanNoOrderNumberNoEmailNoOldAddress()
    {
        string[] stored =
        [
            CustomerNoticeValues.For(Withdrawal(ReturnAddress)).EventValues!,
            CustomerNoticeValues.For(new StatusCorrectedNotice(OrderId, OrderStatus.Placed, WindowEnd)).EventValues!,
            CustomerNoticeValues.For(new RefundIbanRequestedNotice(OrderId, RefundIbanRequestReason.TransferFailed)).EventValues!,
            CustomerNoticeValues.For(new OrderEmailChangedNotice(OrderId, FirstAt)).EventValues!,
            CustomerNoticeValues.For(new ReturnRejectedNotice(OrderId, Lamp, WithdrawalExemptionReason.SealedMedia)).EventValues!,
        ];

        Assert.All(stored, values =>
        {
            Assert.DoesNotContain("TR33", values, StringComparison.Ordinal);
            Assert.DoesNotContain(Number, values, StringComparison.Ordinal);
            Assert.DoesNotContain(Recipient, values, StringComparison.Ordinal);
            Assert.DoesNotContain("Old", values, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void ANoticeThatBreaksTheRuleOfItsType_IsRefused_WhenTheRowIsMade()
    {
        CustomerNotice[] refused =
        [
            Withdrawal() with { DeclarationDate = null },
            Withdrawal() with { At = ClaimAt },
            Claim() with { At = null },
            Claim() with { DeclarationDate = DeclaredOn },
            Reopened() with { Items = [] },
            Withdrawal() with { RecordId = Guid.Empty },
            Withdrawal() with { RecordType = DispatchRelatedRecordType.Refund },
            Withdrawal() with { RecordType = (DispatchRelatedRecordType)99 },
            Withdrawal() with { IsReopening = true },
            Termination() with { IsReopening = true },
            Withdrawal(null, new NoticeItem("Masa", null, null, 0)),
            Withdrawal(null, new NoticeItem(" ", null, null, 1)),
            Withdrawal(ReturnAddress with { District = " " }),
            Withdrawal(ReturnAddress with { ProvinceId = Guid.Empty }),
            new AddressCorrectedNotice(OrderId, (AddressType)99, NewShipping),
            new AddressCorrectedNotice(OrderId, AddressType.ShippingAddress, NewShipping with { RecipientName = "" }),
            new ItemRemovedNotice(OrderId, new NoticeItem("Masa", null, null, 0)),
            new ItemRemovedNotice(Guid.Empty, Table),
        ];

        Assert.All(refused, notice => Assert.ThrowsAny<ArgumentException>(() => CustomerNoticeValues.For(notice)));
    }

    [Theory]
    [InlineData(DispatchType.B10, "[]")]
    [InlineData(DispatchType.B10, "[{\"Name\":\"AddressType\",\"Value\":\"ShippingAddress\"}]")]
    [InlineData(DispatchType.B11, "[{\"Name\":\"Item1Name\",\"Value\":\"Masa\"}]")]
    [InlineData(DispatchType.B11, "[{\"Name\":\"Item1Name\",\"Value\":\"Masa\"},{\"Name\":\"Item1Quantity\",\"Value\":\"0\"}]")]
    [InlineData(DispatchType.B12, "[]")]
    [InlineData(DispatchType.B13, "[{\"Name\":\"Status\",\"Value\":\"Gone\"}]")]
    [InlineData(DispatchType.B13, "[{\"Name\":\"Status\",\"Value\":\"Shipped\"},{\"Name\":\"NewWindowEndsAt\",\"Value\":\"2026-11-10T21:00:00.0000000+00:00\"}]")]
    [InlineData(DispatchType.B14, "[{\"Name\":\"IbanRequestReason\",\"Value\":\"Nope\"}]")]
    [InlineData(DispatchType.B15, "[{\"Name\":\"ChangedAt\",\"Value\":\"dün\"}]")]
    [InlineData(DispatchType.B16, "[{\"Name\":\"Item1Name\",\"Value\":\"Masa\"},{\"Name\":\"Item1Quantity\",\"Value\":\"1\"},{\"Name\":\"ExemptionReason\",\"Value\":\"Perishable\"}]")]
    [InlineData(DispatchType.B9, "[{\"Name\":\"Kind\",\"Value\":\"Withdrawal\"}]")]
    [InlineData(DispatchType.B9, null)]
    [InlineData(DispatchType.B15, null)]
    public void AStoredListThatIsMissingOrMalformed_IsNotANotice_AndTheReaderDoesNotThrow(DispatchType type, string? values)
    {
        Assert.Null(CustomerNoticeValues.Read(type, OrderId, values, RecordId));
    }

    // ---------- the body is HTML of the common frame ----------

    [Theory]
    [MemberData(nameof(EightIndexes))]
    public async Task TheTwoParts_CarryTheSameAddresses_AndTheHtmlHasNoScript(int index)
    {
        EmailMessage message = await Compose(Guest(), AllEight().ElementAt(index));

        foreach (string address in message.TextBody.Split([' ', '\n'], StringSplitOptions.RemoveEmptyEntries).Where(w => w.StartsWith("https://", StringComparison.Ordinal)))
        {
            Assert.Contains(address, message.HtmlBody, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("<script", message.HtmlBody, StringComparison.OrdinalIgnoreCase);
    }
}
