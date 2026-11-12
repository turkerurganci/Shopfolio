using System.Globalization;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Pages.Orders;

/// <summary>
/// The words of the customer's operation screens E-18 and E-19 and of the reason they send the customer back with (04 5.18, 5.19; 07 6.3.3). The form of address is "siz" (K-785). The result
/// sentence and the confirm button of the withdrawal are K-785's; the periods are the module's values, never written here (K-785: "süreler bu dokümanda yazılmaz").
/// </summary>
public static class OrderScreenTexts
{
    /// <summary>07 6.3.3 <c>OperationClosed</c>: the operation is not open on the order, or it closed meanwhile (04 5.18.11, 5.19.10).</summary>
    public const string OperationClosedText = "Bu işlem şu anda yapılamıyor; işlem o arada kapanmış olabilir. Siparişinizin güncel hâli aşağıdadır. Süresi dolmuş bir işlem için iletişim formundan bize yazabilirsiniz.";

    /// <summary>07 6.3.3 <c>QuantityExceedsOpen</c>: the units asked are above the open units, or the open units differ from the ones shown (K-1202); nothing was applied.</summary>
    public const string QuantityExceedsOpenText = "İstediğiniz adet, ürünün şu anda işleme açık adedini aşıyor ya da açık adet o arada değişti; hiçbir ürün işlenmedi. Siparişinizin güncel hâli aşağıdadır.";

    public const string WithdrawalName = "Cayma beyanı";

    /// <summary>K-785: the confirm button of the withdrawal.</summary>
    public const string WithdrawalConfirmLabel = "Cayma beyanını gönder";

    public const string WithdrawalItemsLabel = "Cayılacak ürünler";

    /// <summary>04 5.18.4; K-509: the line of an item frozen with a conditional exemption.</summary>
    public const string PackagingConditionText = "Koruyucu ambalajı açılmamışsa cayabilirsiniz.";

    /// <summary>04 5.18.5; K-838: the shipped item not marked delivered.</summary>
    public const string CarrierReturnsText = "Ürünü henüz teslim almadıysanız ve teslim almazsanız kargo onu firmaya geri götürür. Teslim alırsanız aşağıdaki süre ve gönderme biçimi geçerlidir; süre beyanınızdan işler.";

    /// <summary>04 5.18.5; 02 7.4.2, 7.4.4; K-494: the way the goods go back.</summary>
    public const string SendingWayText = "Ürünü seçtiğiniz taşıyıcıyla iade adresine karşı ödemeli gönderin; gönderim masrafını teslim alırken firma öder.";

    /// <summary>04 5.18.6; 02 7.3.3; K-578, K-792: a service partly performed.</summary>
    public const string PartlyPerformedServiceText = "Kısmen ifa edilmiş hizmetten caymada, cayılan adetler için ödediğiniz bedelin tamamı geri ödenir.";

    /// <summary>04 5.18.6; 02 7.4.6; K-789: the shipping fee comes back only when every unit of the shipped physical items is withdrawn, earlier declarations included.</summary>
    public const string ShippingFeeText = "Kargoyla gönderilen ürünlerin bütün adetlerinden, önceki beyanlarınızla birlikte, caydığınızda kargo ücreti de geri ödenir.";

    public const string ReturnAddressTitle = "İade adresi";

    /// <summary>04 5.16.5; 02 3.1.7; K-665: the withdrawal's record on the item's row names the return address written into the declaration, not the firm's address of today.</summary>
    public static string RecordReturnAddress(string recipientName, string addressLine, string district, string provinceName) =>
        $"{ReturnAddressTitle}: {recipientName}, {addressLine}, {district} / {provinceName}";

    public const string DefectClaimName = "Ayıp talebi";

    public const string DefectClaimSubmitLabel = "Talebi gönder";

    public const string DefectClaimReopenLabel = "Talebi yeniden aç";

    public const string DefectClaimItemLabel = "Sorunlu ürün";

    public const string DefectClaimQuantityLabel = "Ayıplı adet";

    public const string DefectClaimDescriptionLabel = "Sorunun açıklaması";

    public const string CancelLabel = "Vazgeç";

    /// <summary>04 5.19.4 (2): the reopening is not a new claim.</summary>
    public const string ReopenIsSameClaimText = "Yeniden açma yeni bir talep açmaz; ayıbın geçmişi tek kayıtta kalır.";

    /// <summary>The badge of a resolved claim (04 5.19.4; 2.5.1.5).</summary>
    public const string ResolvedBadge = "Çözüldü";

    /// <summary>The reason's words of a return code, or null when the request carried no reason (07 6.3.3; a code off the list is ignored, 07 2.6.6).</summary>
    public static string? ReasonOf(NoticeCode? code) => code switch
    {
        NoticeCode.OperationClosed => OperationClosedText,
        NoticeCode.QuantityExceedsOpen => QuantityExceedsOpenText,
        _ => null,
    };

    /// <summary>04 5.18.5: the days to send the goods back from the declaration, with the last day.</summary>
    public static string SendingPeriod(int days, DateOnly lastDay) =>
        string.Create(CultureInfo.InvariantCulture, $"Ürünü beyanınızdan itibaren {DisplayFormat.Number(days)} gün içinde, en geç {DisplayFormat.Date(lastDay)} tarihine kadar gönderin.");

    /// <summary>04 5.18.6; 02 7.4.1, 7.4.5: the way and the start of the refund.</summary>
    public static string Refund(PaymentMethod way, bool fromGoodsArrival, int days) =>
        string.Create(CultureInfo.InvariantCulture, $"Geri ödeme {FromWhen(fromGoodsArrival)} itibaren {DisplayFormat.Number(days)} gün içinde {WayOf(way)} yapılır.");

    /// <summary>K-785: the result sentence of the withdrawal's last step; the number is the sum of the units chosen (K-795).</summary>
    public static string WithdrawalSentence(int units, PaymentMethod way, bool fromGoodsArrival, int days) =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"Seçtiğiniz {DisplayFormat.Number(units)} ürün için cayma beyanınız kaydedilecek; geri ödeme {FromWhen(fromGoodsArrival)} itibaren {DisplayFormat.Number(days)} gün içinde {WayOf(way)} yapılacak.");

    /// <summary>04 5.18.4; K-795: the line says the item's units open to the operation.</summary>
    public static string OpenUnits(int open) => string.Create(CultureInfo.InvariantCulture, $"İşleme açık adet: {DisplayFormat.Number(open)}");

    /// <summary>04 5.18.4: the last day of the item's window, when it has one.</summary>
    public static string WindowLastDay(DateOnly lastDay) => "Cayma hakkınızın son günü: " + DisplayFormat.Date(lastDay);

    /// <summary>04 5.19.4 (3); K-783: further information goes to the firm's e-mail.</summary>
    public static string MoreInformation(string? firmEmail) => firmEmail is null
        ? "Eklemek istediğiniz bilgiyi firmaya e-postayla iletebilirsiniz."
        : "Eklemek istediğiniz bilgiyi " + firmEmail + " adresine e-postayla iletebilirsiniz.";

    /// <summary>04 5.19.4 (1): the day the claim was opened.</summary>
    public static string OpenedOn(DateTimeOffset at) => "Açılış tarihi: " + DisplayFormat.Date(at);

    private static string FromWhen(bool fromGoodsArrival) => fromGoodsArrival ? "malın firmaya ulaşmasından" : "beyanınızdan";

    private static string WayOf(PaymentMethod way) => way == PaymentMethod.Card ? "kartınıza" : "IBAN'ınıza";
}
