using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.EmailComposition;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>
/// The templates of the customer notices B-9…B-16 (T307; 08 2.2.6): the same skeleton as B-1…B-8 - the re-send sentence, "Merhaba," without a name, the event's sentence
/// with the order number, the sections, the main button - from the order's values and the event's values handed in. B-15 is the one notice without a button and without
/// the order's address: it goes to an address that may belong to someone else.
/// </summary>
internal sealed partial class CustomerNoticeTemplates
{
    /// <summary>The label of the main button of B-14 (08 2.2.6 item 3).</summary>
    public const string IbanButtonLabel = "IBAN'ı sipariş sayfasından gir";

    private const string NotYouSentence = "Bu değişikliği siz istemediyseniz lütfen firmayla iletişime geçin.";

    /// <summary>The headline of B-9, one of the four of 08 2.2.6 B-9; a record that is not a declaration's, or a reopening of anything but a claim, has none.</summary>
    private static string DeclarationTitle(DeclarationReceivedNotice notice) => (notice.RecordType, notice.IsReopening) switch
    {
        (DispatchRelatedRecordType.Withdrawal, false) => "Cayma beyanınız alındı",
        (DispatchRelatedRecordType.LateDeliveryTermination, false) => "Gecikme nedeniyle fesih beyanınız alındı",
        (DispatchRelatedRecordType.DefectClaim, false) => "Ayıp talebiniz alındı",
        (DispatchRelatedRecordType.DefectClaim, true) => "Ayıp talebiniz yeniden açıldı",
        _ => throw new ArgumentException("A declaration notice is a withdrawal, a termination, a defect claim or the reopening of a claim (08 2.2.6 B-9).", nameof(notice)),
    };

    private List<EmailBlock> Declaration(OrderNoticeContext order, string number, DeclarationReceivedNotice notice)
    {
        string sentence = (notice.RecordType, notice.IsReopening) switch
        {
            (DispatchRelatedRecordType.Withdrawal, false) => $"{number} numaralı siparişinizde cayma beyanınız {DeclaredDate(notice)} tarihinde kaydedildi.",
            (DispatchRelatedRecordType.LateDeliveryTermination, false) =>
                $"{number} numaralı siparişinizde gecikme nedeniyle fesih beyanınız {DeclaredDate(notice)} tarihinde kaydedildi.",
            (DispatchRelatedRecordType.DefectClaim, false) => $"{number} numaralı siparişinizde ayıp talebiniz {DeclaredMoment(notice)} tarihinde kaydedildi.",
            (DispatchRelatedRecordType.DefectClaim, true) => $"{number} numaralı siparişinizdeki ayıp talebiniz {DeclaredMoment(notice)} tarihinde yeniden açıldı.",
            _ => throw new ArgumentException("A declaration notice is a withdrawal, a termination, a defect claim or the reopening of a claim (08 2.2.6 B-9).", nameof(notice)),
        };

        List<EmailBlock> blocks = [new EmailParagraph(Greeting), new EmailParagraph(sentence)];
        blocks.AddRange(notice.Items.Select(ItemRow));
        if (notice.ReturnAddress is { } address)
        {
            // The address the goods are sent to: the one the declaration froze (08 2.2.6 B-9).
            blocks.Add(new EmailParagraph("İade adresi:"));
            blocks.AddRange(AddressRows(order, address));
        }

        blocks.Add(Button(order));
        return blocks;
    }

    private static string DeclaredDate(DeclarationReceivedNotice notice) =>
        DisplayFormat.Date(notice.DeclarationDate ?? throw new ArgumentException("A withdrawal and a termination carry the declaration's date.", nameof(notice)));

    private static string DeclaredMoment(DeclarationReceivedNotice notice) =>
        DisplayFormat.DateAndTime(notice.At ?? throw new ArgumentException("A defect claim and its reopening carry the moment.", nameof(notice)));

    private List<EmailBlock> AddressCorrected(OrderNoticeContext order, string number, AddressCorrectedNotice notice)
    {
        string kind = notice.AddressType switch
        {
            AddressType.ShippingAddress => "teslimat",
            AddressType.BillingAddress => "fatura",
            _ => throw new ArgumentOutOfRangeException(nameof(notice), notice.AddressType, "Unknown address type."),
        };

        // Only the new address is written; the old one never is (08 2.2.6 B-10).
        List<EmailBlock> blocks = [new EmailParagraph(Greeting), new EmailParagraph($"{number} numaralı siparişinizin {kind} adresi düzeltildi. Yeni adres:")];
        blocks.AddRange(AddressRows(order, notice.NewAddress));
        blocks.Add(new EmailParagraph(NotYouSentence));
        blocks.AddRange(FirmContact(order));
        blocks.Add(Button(order));
        return blocks;
    }

    private List<EmailBlock> ItemRemoved(OrderNoticeContext order, string number, ItemRemovedNotice notice)
    {
        // The item and the quantity, and no amount: a paid item's refund has its own notice (B-8 for a card, B-14 for a transfer; 08 2.2.6 B-11).
        return
        [
            new EmailParagraph(Greeting),
            new EmailParagraph($"{number} numaralı siparişinizden {ItemText(notice.Item)}, {DisplayFormat.Number(notice.Item.Quantity)} adet çıkarıldı."),
            Button(order),
        ];
    }

    private List<EmailBlock> CarrierCorrected(OrderNoticeContext order, string number, CarrierCorrectedNotice notice)
    {
        List<EmailBlock> blocks = [new EmailParagraph(Greeting), new EmailParagraph($"{number} numaralı siparişinizin kargo bilgisi düzeltildi.")];
        blocks.AddRange(ShippingBlocks(new ShippedNotice(notice.OrderId, notice.Carrier, notice.OtherCarrierName, notice.TrackingNumber, notice.IsOwnVehicleDelivery)));
        blocks.Add(Button(order));
        return blocks;
    }

    private List<EmailBlock> StatusCorrected(OrderNoticeContext order, string number, StatusCorrectedNotice notice)
    {
        // The correction's reason is never written (08 2.2.6 B-13).
        List<EmailBlock> blocks =
        [
            new EmailParagraph(Greeting),
            new EmailParagraph($"{number} numaralı siparişinizin durumu yanlışlıkla güncellenmişti ve düzeltildi. Siparişinizin durumu: {StatusText(notice.Status)}."),
        ];
        if (notice.NewPaymentWindowEndsAt is { } end)
        {
            blocks.Add(new EmailParagraph($"Ödemeniz alınmış görünmüyor. Ödemeyi {LastPaymentDay(end)} tarihine kadar yapmanız gerekir."));
            blocks.AddRange(IbanBlock(order, number));
        }

        blocks.Add(Button(order));
        return blocks;
    }

    private static string StatusText(OrderStatus status) => status switch
    {
        OrderStatus.Placed => "Alındı",
        OrderStatus.Preparing => "Hazırlanıyor",
        OrderStatus.Shipped => "Kargoya verildi",
        OrderStatus.Delivered => "Teslim edildi",
        OrderStatus.DeliveryFailed => "Teslim edilemedi",
        OrderStatus.Cancelled => "İptal edildi",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "No name for this status."),
    };

    private List<EmailBlock> IbanRequested(OrderNoticeContext order, string number, RefundIbanRequestedNotice notice)
    {
        // No IBAN is written and none is accepted by reply (08 2.2.6 B-14; K-1251): the button goes to the order page, where the IBAN is entered.
        return
        [
            new EmailParagraph(Greeting),
            new EmailParagraph(IbanReasonSentence(notice.Reason)),
            new EmailParagraph($"{number} numaralı siparişinizin geri ödemesini yapabilmemiz için IBAN'ınızı sipariş sayfanızdan girmeniz gerekiyor."),
            new EmailButton(IbanButtonLabel, OrderLink(order)),
            new EmailParagraph("IBAN'ınızı bu e-postayı yanıtlayarak göndermeyin; yalnız sipariş sayfasından girin."),
        ];
    }

    private static string IbanReasonSentence(RefundIbanRequestReason reason) => reason switch
    {
        RefundIbanRequestReason.ReturnAfterIbanDeleted => "İade ettiğiniz ürün firmamıza ulaştı.",
        RefundIbanRequestReason.TransferFailed => "Geri ödeme havalesi girdiğiniz IBAN'a gerçekleşmedi.",
        RefundIbanRequestReason.CardRefundFailed => "Kartınıza yapılan iade gerçekleşmedi. Geri ödemeyi IBAN'ınıza havale yoluyla almayı seçebilirsiniz; IBAN girmek sizin seçiminizdir.",
        RefundIbanRequestReason.NoticeWithoutIban => "Firmaya başka bir yoldan ilettiğiniz cayma ya da fesih bildirimi kaydedildi; bildirim IBAN içermiyordu.",
        RefundIbanRequestReason.CancellationOrRemoval => "Ödenmiş bir kalem iptal edildi ya da siparişten çıkarıldı.",
        RefundIbanRequestReason.AdminRequest => "Firma, bir geri ödeme için IBAN'ınızı istiyor.",
        _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, "No sentence for this reason."),
    };

    private static List<EmailBlock> EmailChanged(OrderNoticeContext order, string number, OrderEmailChangedNotice notice)
    {
        // Goes to the old address, which may be someone else's: no new address, no name, no address of the order and no link or button (08 2.2.6 B-15).
        List<EmailBlock> blocks =
        [
            new EmailParagraph(Greeting),
            new EmailParagraph($"{number} numaralı siparişin e-posta adresi {DisplayFormat.Date(notice.ChangedAt)} tarihinde değiştirildi."),
            new EmailParagraph(NotYouSentence),
        ];
        blocks.AddRange(FirmContact(order));
        return blocks;
    }

    private List<EmailBlock> ReturnRejected(OrderNoticeContext order, string number, ReturnRejectedNotice notice)
    {
        if (!WithdrawalExemptions.IsConditional(notice.Reason))
        {
            throw new ArgumentException("A return is rejected for an opened protective package (08 2.2.6 B-16).", nameof(notice));
        }

        // The dispute ways are written without an amount: the monetary limit of the consumer arbitration board is not written (02 3.33.7; K-1250).
        List<EmailBlock> blocks =
        [
            new EmailParagraph(Greeting),
            new EmailParagraph($"{number} numaralı siparişinizde iade ettiğiniz {ItemText(notice.Item)}, {DisplayFormat.Number(notice.Item.Quantity)} adet kabul edilmedi."),
            new EmailParagraph("Ürünün koruyucu ambalajı açılmıştı; bu ürün için cayma hakkı ambalajın açılmasıyla sona erer."),
            new EmailParagraph("Bu nedenle geri ödeme yapılmayacaktır."),
            new EmailParagraph("Ürün, siparişteki teslimat adresinize firmamızın bedeliyle geri gönderilecektir."),
            new EmailParagraph("Bu karara itirazınız varsa tüketici hakem heyetine ya da — dava açmadan önce arabulucuya başvurmanız şartıyla — tüketici mahkemesine başvurabilirsiniz."),
        ];
        blocks.AddRange(FirmContact(order));
        blocks.Add(Button(order));
        return blocks;
    }

    private static string ItemText(NoticeItem item) => ItemText(new CancelledItem(item.ProductName, item.FirstOptionValue, item.SecondOptionValue, item.Quantity));

    /// <summary>An item and its quantity on a row of its own (08 2.2.6 item 4): "Ahşap Masa (Meşe, 120 cm), 2 adet".</summary>
    private static EmailKeyValue ItemRow(NoticeItem item) => new("Kalem", $"{ItemText(item)}, {DisplayFormat.Number(item.Quantity)} adet");

    /// <summary>An address as rows (08 2.2.6 B-9, B-10): the recipient's name, the open address, the district, the province and - when the address has one - the telephone.</summary>
    private static IEnumerable<EmailBlock> AddressRows(OrderNoticeContext order, NoticeAddress address)
    {
        if (order.ProvinceNames is null || !order.ProvinceNames.TryGetValue(address.ProvinceId, out string? province) || string.IsNullOrWhiteSpace(province))
        {
            throw new ArgumentException("A notice that writes an address is handed the province's name (06 4.3).", nameof(order));
        }

        yield return new EmailKeyValue("Alıcı adı", address.RecipientName);
        yield return new EmailKeyValue("Adres", address.AddressLine);
        yield return new EmailKeyValue("İlçe", address.District);
        yield return new EmailKeyValue("İl", province);
        if (!string.IsNullOrWhiteSpace(address.Phone))
        {
            yield return new EmailKeyValue("Telefon", DisplayFormat.Phone(address.Phone));
        }
    }
}
