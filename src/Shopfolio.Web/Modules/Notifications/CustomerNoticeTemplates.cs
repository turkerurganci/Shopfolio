using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.EmailComposition;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>
/// What the body of a customer notice reads from the order when it is made (08 2.2.6 items 2-4; 05 4.4.2): the order's number, the access key and the two flags
/// that choose the main button's address, the frozen IBAN (B-2, B-3), whether an item is digital (B-4), the two frozen legal texts already filled with the values of
/// the confirmation moment (B-1; 09 11.4 item 4), the firm's contact (B-6, B-7) and, on a re-sent e-mail, the creation moment of the first e-mail of the group.
/// </summary>
internal sealed record OrderNoticeContext(
    string OrderNumber,
    string? AccessKey,
    bool IsGuestOrder,
    bool IsAccountDeleted,
    string? BankTransferIban,
    bool HasDigitalItem,
    string? PreInformationFormText,
    string? DistanceSalesContractText,
    string FirmEmail,
    string FirmPhone,
    DateTimeOffset? ResendOf,
    IReadOnlyDictionary<Guid, string>? ProvinceNames = null);

/// <summary>
/// The templates of the customer notices B-1…B-8 (08 2.2.6; 09 11.4 item 1): from the order's values and the event's values each notice makes the
/// <see cref="EmailContent"/> the common pattern writes (HTML and plain text from one model, T305). The skeleton is shared - the re-send sentence, "Merhaba,"
/// without a name (the recipient of the delivery and the buyer may be different people), the event's sentence with the order number, the notice's sections, the
/// main button with its address under it - and the footer is the frame's. Nothing here reads a request, a table or the clock: every value is handed in.
/// </summary>
internal sealed partial class CustomerNoticeTemplates
{
    /// <summary>The label of the main button (08 2.2.6 item 3).</summary>
    public const string OrderButtonLabel = "Siparişi görüntüle";

    private const string Greeting = "Merhaba,";
    private const string TrackingLabel = "Takip et";

    private readonly EmailLinks _links;
    private readonly CarrierCatalog _carriers;

    public CustomerNoticeTemplates(EmailLinks links, CarrierCatalog carriers)
    {
        ArgumentNullException.ThrowIfNull(links);
        ArgumentNullException.ThrowIfNull(carriers);
        _links = links;
        _carriers = carriers;
    }

    /// <summary>The content of a notice B-1…B-8 for an order.</summary>
    public EmailContent Compose(OrderNoticeContext order, CustomerNotice notice)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(notice);
        ArgumentException.ThrowIfNullOrWhiteSpace(order.OrderNumber);

        string number = order.OrderNumber;
        return notice switch
        {
            OrderReceivedNotice received => Make(order, "Siparişiniz alındı", false, Received(order, number), received.FirstEmailSentAt),
            TransferChosenNotice chosen => Make(order, "Havale/EFT bilgileriniz", true, TransferChosen(order, number, chosen)),
            TransferReminderNotice reminder => Make(order, "Havale ödemeniz bekleniyor", true, TransferReminder(order, number, reminder)),
            PaymentConfirmedNotice => Make(order, "Ödemeniz onaylandı", false, Confirmed(order, number)),
            ShippedNotice shipped => Make(order, "Siparişiniz kargoya verildi", true, Shipped(order, number, shipped)),
            DeliveryFailedNotice => Make(order, "Siparişiniz teslim edilemedi", false, DeliveryFailed(order, number)),
            CancelledNotice cancelled => Make(
                order,
                cancelled.Item is null ? "Siparişiniz iptal edildi" : "Siparişinizdeki bir kalem iptal edildi",
                false,
                Cancelled(order, number, cancelled)),
            RefundProcessedNotice refund => Make(order, "Geri ödemeniz işlendi", false, Refunded(order, number, refund)),
            DeclarationReceivedNotice declaration => Make(order, DeclarationTitle(declaration), false, Declaration(order, number, declaration)),
            AddressCorrectedNotice address => Make(order, "Siparişinizin adresi düzeltildi", true, AddressCorrected(order, number, address)),
            ItemRemovedNotice removed => Make(order, "Siparişinizdeki bir kalem değişti", false, ItemRemoved(order, number, removed)),
            CarrierCorrectedNotice carrier => Make(order, "Kargo bilginiz düzeltildi", true, CarrierCorrected(order, number, carrier)),
            StatusCorrectedNotice status => Make(order, "Siparişinizin durumu düzeltildi", true, StatusCorrected(order, number, status)),
            RefundIbanRequestedNotice request => Make(order, "Geri ödemeniz için IBAN bilgisi gerekli", false, IbanRequested(order, number, request)),
            OrderEmailChangedNotice changed => Make(order, "Siparişinizin e-posta adresi değiştirildi", false, EmailChanged(order, number, changed)),
            ReturnRejectedNotice rejected => Make(order, "İade ettiğiniz ürün kabul edilmedi", false, ReturnRejected(order, number, rejected)),
            _ => throw new ArgumentException($"{notice.GetType().Name} is not a customer notice of B-1…B-16.", nameof(notice)),
        };
    }

    /// <summary>The main button's address (08 2.2.6 item 3): the access key address of a guest order and of an order whose account is deleted, the account's order address of a member's.</summary>
    public Uri OrderLink(OrderNoticeContext order)
    {
        ArgumentNullException.ThrowIfNull(order);
        if (order.IsGuestOrder || order.IsAccountDeleted)
        {
            if (string.IsNullOrWhiteSpace(order.AccessKey))
            {
                throw new ArgumentException("A guest order, and an order whose account is deleted, has an access key (02 3.22.5).", nameof(order));
            }

            return _links.GuestOrder(order.AccessKey);
        }

        return _links.MemberOrder(order.OrderNumber);
    }

    private static EmailContent Make(OrderNoticeContext order, string title, bool carriesCurrentInfoNote, List<EmailBlock> blocks, DateTimeOffset? eventResendOf = null)
    {
        // A B-1 correction's moment comes from the row's own event values (it outlives the first row); the group's first row is the source of the other notices'.
        EmailResend? resend = (eventResendOf ?? order.ResendOf) is { } at ? new EmailResend(at, carriesCurrentInfoNote) : null;
        return new EmailContent(title, order.OrderNumber, blocks, resend);
    }

    private List<EmailBlock> Received(OrderNoticeContext order, string number)
    {
        if (string.IsNullOrWhiteSpace(order.PreInformationFormText) || string.IsNullOrWhiteSpace(order.DistanceSalesContractText))
        {
            throw new ArgumentException("B-1 carries the two frozen legal texts in full (08 2.2.6 B-1).", nameof(order));
        }

        // The two texts follow the main button, each its own section in this order; no attachment exists (08 2.2.6, the placement of the two texts).
        return
        [
            new EmailParagraph(Greeting),
            new EmailParagraph($"Siparişiniz alındı. Sipariş numaranız: {number}."),
            Button(order),
            new EmailLegalText("Ön Bilgilendirme Formu", order.PreInformationFormText),
            new EmailLegalText("Mesafeli Satış Sözleşmesi", order.DistanceSalesContractText),
        ];
    }

    private List<EmailBlock> TransferChosen(OrderNoticeContext order, string number, TransferChosenNotice notice)
    {
        List<EmailBlock> blocks =
        [
            new EmailParagraph(Greeting),
            new EmailParagraph($"{number} numaralı siparişiniz alındı. Ödemeyi aşağıdaki hesaba havale/EFT ile yapabilirsiniz."),
        ];
        blocks.AddRange(IbanBlock(order, number));
        blocks.Add(new EmailParagraph($"Ödemenizi {LastPaymentDay(notice.PaymentWindowEndsAt)} tarihine kadar yapmanız gerekir; aksi hâlde sipariş iptal edilir."));
        blocks.Add(Button(order));
        return blocks;
    }

    private List<EmailBlock> TransferReminder(OrderNoticeContext order, string number, TransferReminderNotice notice)
    {
        List<EmailBlock> blocks =
        [
            new EmailParagraph(Greeting),
            new EmailParagraph(
                $"{number} numaralı siparişiniz için havale ödemenizi henüz alamadık. Ödemeyi {LastPaymentDay(notice.PaymentWindowEndsAt)} tarihine kadar yapmazsanız siparişiniz iptal edilir."),
        ];
        blocks.AddRange(IbanBlock(order, number));
        blocks.Add(Button(order));
        blocks.Add(new EmailParagraph("Ödemeyi yaptıysanız bu e-postayı dikkate almayın; havale firmaya ulaştığında siparişiniz güncellenir."));
        return blocks;
    }

    private List<EmailBlock> Confirmed(OrderNoticeContext order, string number)
    {
        List<EmailBlock> blocks = [new EmailParagraph(Greeting), new EmailParagraph($"{number} numaralı siparişinizin ödemesi onaylandı.")];
        if (order.HasDigitalItem)
        {
            // The download is on the order page: no download address is written, the button goes to that page (02 3.12.1).
            blocks.Add(new EmailParagraph("Dijital ürününüz indirmeye hazır. Dosyayı sipariş sayfanızdan indirebilirsiniz."));
        }

        blocks.Add(Button(order));
        return blocks;
    }

    private List<EmailBlock> Shipped(OrderNoticeContext order, string number, ShippedNotice notice)
    {
        List<EmailBlock> blocks = [new EmailParagraph(Greeting), new EmailParagraph($"{number} numaralı siparişiniz kargoya verildi.")];
        blocks.AddRange(ShippingBlocks(notice));
        blocks.Add(Button(order));
        return blocks;
    }

    /// <summary>The shipping block of B-5 and, with the corrected values, of B-12 (08 2.2.6 B-5, B-12): the company and the number with the tracking link, or the own-vehicle sentence.</summary>
    private List<EmailBlock> ShippingBlocks(ShippedNotice notice)
    {
        List<EmailBlock> blocks = [];
        if (notice.IsOwnVehicleDelivery)
        {
            blocks.Add(new EmailParagraph("Siparişiniz firmamızın kendi aracıyla teslim edilecektir."));
            return blocks;
        }

        ShippingDisplay display = _carriers.Describe(notice.Carrier, notice.OtherCarrierName, notice.TrackingNumber, false);
        if (display.CarrierName is not null)
        {
            blocks.Add(new EmailKeyValue("Kargo şirketi", display.CarrierName));
        }

        if (display.TrackingNumber is not null)
        {
            blocks.Add(new EmailKeyValue("Takip numarası", display.TrackingNumber));
        }

        if (display.Link is not null)
        {
            // The carrier's own tracking page: another site, so it carries noreferrer (08 2.2.5 item 4, 2.4).
            blocks.Add(new EmailButton(TrackingLabel, new Uri(display.Link.Href, UriKind.Absolute), isExternal: true));
        }

        return blocks;
    }

    private List<EmailBlock> DeliveryFailed(OrderNoticeContext order, string number)
    {
        List<EmailBlock> blocks =
        [
            new EmailParagraph(Greeting),
            new EmailParagraph($"{number} numaralı siparişinizin gönderisi teslim edilemedi ve firmamıza geri döndü. Teslimat için firmayla iletişime geçebilirsiniz."),
        ];
        blocks.AddRange(FirmContact(order));
        blocks.Add(Button(order));
        return blocks;
    }

    private List<EmailBlock> Cancelled(OrderNoticeContext order, string number, CancelledNotice notice)
    {
        string item = notice.Item is null ? string.Empty : $"{ItemText(notice.Item)}, {DisplayFormat.Number(notice.Item.Quantity)} adet";
        List<EmailBlock> blocks = [new EmailParagraph(Greeting)];
        switch (notice.Actor)
        {
            case ActorKind.Buyer:
                blocks.Add(new EmailParagraph(notice.Item is null
                    ? $"Talebiniz üzerine {number} numaralı siparişiniz iptal edildi."
                    : $"Talebiniz üzerine {number} numaralı siparişinizdeki {item} iptal edildi."));
                break;
            case ActorKind.Admin:
                blocks.Add(new EmailParagraph(notice.Item is null
                    ? $"{number} numaralı siparişiniz firmamız tarafından iptal edildi."
                    : $"{number} numaralı siparişinizdeki {item} firmamız tarafından iptal edildi."));
                blocks.Add(new EmailParagraph("Sebep: " + AdministratorReasonText(notice)));
                // The notice of the Distance Sales Regulation art. 16/4: the same text for every reason, stock included (08 2.2.6 B-7).
                blocks.Add(new EmailParagraph("Ödediğiniz tutar, teslimat masrafları dahil, bu bildirimden itibaren en geç on dört gün içinde iade edilir."));
                break;
            case ActorKind.System:
                blocks.Add(new EmailParagraph($"{number} numaralı siparişiniz kendiliğinden iptal edildi."));
                blocks.Add(new EmailParagraph("Sebep: " + SystemReasonText(notice)));
                break;
            default:
                throw new ArgumentException("Unknown actor of a cancellation.", nameof(notice));
        }

        blocks.AddRange(FirmContact(order));
        blocks.Add(Button(order));
        return blocks;
    }

    private List<EmailBlock> Refunded(OrderNoticeContext order, string number, RefundProcessedNotice notice)
    {
        List<EmailBlock> blocks = [new EmailParagraph(Greeting)];
        if (notice.RefundType == RefundType.LatePayment)
        {
            blocks.Add(new EmailParagraph($"İptal edilmiş {number} numaralı siparişinize gelen ödeme, tutarın tamamı olarak iade edildi."));
            blocks.Add(EmailKeyValue.Amount("İade edilen tutar", notice.Amount));
        }
        else
        {
            blocks.Add(new EmailParagraph($"{number} numaralı siparişiniz için {RefundReason(notice.RefundType)} nedeniyle {DisplayFormat.Amount(notice.Amount)} geri ödeme işlendi."));
        }

        // The refund goes back the way the payment came; the IBAN of a transfer refund is never written (08 2.2.6 B-8).
        blocks.Add(new EmailParagraph(notice.Method == PaymentMethod.Card
            ? "Tutar, ödeme yaptığınız karta iade edildi."
            : "Tutar, bildirdiğiniz IBAN'a havale edildi."));
        blocks.Add(Button(order));
        return blocks;
    }

    private static string RefundReason(RefundType type) => type switch
    {
        RefundType.Cancellation => "iptal",
        RefundType.ItemRemoval => "kalem çıkarma",
        RefundType.LateDeliveryTermination => "gecikme nedeniyle fesih",
        RefundType.Withdrawal => "cayma",
        RefundType.DefectClaimRescission => "ayıp talebi",
        RefundType.AmountBased => "kısmi geri ödeme",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "No reason sentence for this refund type."),
    };

    private static string AdministratorReasonText(CancelledNotice notice) => notice.AdministratorReason switch
    {
        CancellationReason.OutOfStock => "stokta bulunamadı",
        CancellationReason.DamagedOrUnsellable => "ürün hasarlı ya da satılamaz durumda",
        CancellationReason.CannotShipToAddress => "teslimat adresine gönderim yapılamıyor",
        CancellationReason.UndeliveredBuyerCaused => "gönderi teslim edilemedi — müşteri kaynaklı (adres hatalı ya da alıcı bulunamadı)",
        CancellationReason.AgreedWithBuyer => "müşteriyle anlaşıldı (müşteri talebi)",
        CancellationReason.Other when !string.IsNullOrWhiteSpace(notice.ReasonNote) => notice.ReasonNote,
        _ => throw new ArgumentException("An administrator's cancellation names its reason (08 2.2.6 B-7).", nameof(notice)),
    };

    private static string SystemReasonText(CancelledNotice notice) => notice.SystemReason switch
    {
        TransitionReason.PaymentWindowExpired => "ödeme süresi dolduğu için",
        TransitionReason.CardPaymentFailed => "kart ödemeniz tamamlanamadığı için",
        TransitionReason.PaymentPageNotOpened => "ödeme sayfası açılamadığı için",
        TransitionReason.SameCartReorder => "aynı sepetten yeni bir sipariş verdiğiniz için",
        _ => throw new ArgumentException("A system cancellation names one of its four reasons (08 2.2.6 B-7).", nameof(notice)),
    };

    /// <summary>An item as it is written in a notice (08 2.2.6 item 4): the product name and, on a variant, the option values in brackets - "Ahşap Masa (Meşe, 120 cm)".</summary>
    private static string ItemText(CancelledItem item)
    {
        string[] options = [.. new[] { item.FirstOptionValue, item.SecondOptionValue }.Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => o!)];
        return options.Length == 0 ? item.ProductName : $"{item.ProductName} ({string.Join(", ", options)})";
    }

    /// <summary>The IBAN block of B-2 and B-3 (08 2.2.6 B-2): the order's frozen IBAN in groups of four and the order number to write in the transfer's description.</summary>
    private static IEnumerable<EmailBlock> IbanBlock(OrderNoticeContext order, string number)
    {
        if (string.IsNullOrWhiteSpace(order.BankTransferIban))
        {
            throw new ArgumentException("A transfer notice carries the IBAN the order froze (02 3.21.5).", nameof(order));
        }

        yield return new EmailKeyValue("IBAN", DisplayFormat.Iban(order.BankTransferIban));
        yield return new EmailKeyValue("Havale açıklamasına yazılacak sipariş numarası", number);
    }

    /// <summary>The last payment day: a window ends at the end of its last day, so the day is the one that holds the instant just before the end (<c>TransferSchedule</c>).</summary>
    private static string LastPaymentDay(DateTimeOffset windowEndsAt) => DisplayFormat.Date(TurkeyTime.DateOf(windowEndsAt.AddTicks(-1)));

    /// <summary>The firm's contact in the text itself (08 2.2.6 item 6): the contact e-mail and the telephone; the reply goes to the same address.</summary>
    private static IEnumerable<EmailBlock> FirmContact(OrderNoticeContext order)
    {
        if (string.IsNullOrWhiteSpace(order.FirmEmail) || string.IsNullOrWhiteSpace(order.FirmPhone))
        {
            throw new ArgumentException("A notice that sends the customer to the firm carries the firm's e-mail and telephone (08 2.2.6 item 6).", nameof(order));
        }

        yield return new EmailKeyValue("Firma e-postası", order.FirmEmail);
        yield return new EmailKeyValue("Firma telefonu", DisplayFormat.Phone(order.FirmPhone));
    }

    private EmailButton Button(OrderNoticeContext order) => new(OrderButtonLabel, OrderLink(order));
}
