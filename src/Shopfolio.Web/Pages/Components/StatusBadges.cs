using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Pages.Components;

/// <summary>The meaning a badge's colour helps with (04 2.5; 02 6.1.6): colour is only an aid - the badge always carries its text.</summary>
public enum BadgeTone
{
    Neutral,
    Info,
    Success,
    Warning,
    Danger,
}

/// <summary>One status badge: the text the source names the value with, and the tone that helps read it (04 2.5.1).</summary>
public sealed record Badge(string Text, BadgeTone Tone);

/// <summary>The two badges of an order, side by side: the shipment axis and the payment axis (04 2.5.1; 02 5.3.1; K-777). One badge never stands for both.</summary>
public sealed record OrderBadges(Badge Shipping, Badge Payment);

/// <summary>
/// The badge families of 04 2.5.1 (T542; OB-05): the order's shipment axis, the order's payment axis, the product and variant publish status, the corporate content publish status, the
/// defect claim's status and the contact request's status. The names are the source's, word for word (02 5.1-5.5, 5.10, 5.13); a consistency test reads them from 04 2.5.1. The account and
/// the invitation have no state machine (02 5.12.1; 03 1.10.7), so there is no badge family for them: the validity of an unverified registration or of an invitation is the screen's own row text.
/// A vitrin page shows no publish status badge (04 2.5.6); these functions are the panel's.
/// </summary>
public static class StatusBadges
{
    /// <summary>The shipment axis badge of an order (04 2.5.1.1).</summary>
    public static Badge For(OrderStatus status) => status switch
    {
        OrderStatus.Placed => new Badge("Alındı", BadgeTone.Info),
        OrderStatus.Preparing => new Badge("Hazırlanıyor", BadgeTone.Info),
        OrderStatus.Shipped => new Badge("Kargoya verildi", BadgeTone.Info),
        OrderStatus.Delivered => new Badge("Teslim edildi", BadgeTone.Success),
        OrderStatus.DeliveryFailed => new Badge("Teslim edilemedi", BadgeTone.Danger),
        OrderStatus.Cancelled => new Badge("İptal edildi", BadgeTone.Neutral),
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Not an order status."),
    };

    /// <summary>The payment axis badge of an order (04 2.5.1.2).</summary>
    public static Badge For(PaymentStatus status) => status switch
    {
        PaymentStatus.Pending => new Badge("Bekliyor", BadgeTone.Warning),
        PaymentStatus.Paid => new Badge("Ödendi", BadgeTone.Success),
        PaymentStatus.Failed => new Badge("Başarısız", BadgeTone.Danger),
        PaymentStatus.PartiallyRefunded => new Badge("Kısmen geri ödendi", BadgeTone.Neutral),
        PaymentStatus.Refunded => new Badge("Geri ödendi", BadgeTone.Neutral),
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Not a payment status."),
    };

    /// <summary>The two badges an order shows wherever it is shown: one for each axis (04 2.5.1; K-777).</summary>
    public static OrderBadges ForOrder(OrderStatus shipping, PaymentStatus payment) => new(For(shipping), For(payment));

    /// <summary>The publish status badge of a product and of a variant (04 2.5.1.3): Taslak, Yayında, Arşiv.</summary>
    public static Badge For(PublishStatus status) => status switch
    {
        PublishStatus.Draft => new Badge("Taslak", BadgeTone.Warning),
        PublishStatus.Published => new Badge("Yayında", BadgeTone.Success),
        PublishStatus.Archived => new Badge("Arşiv", BadgeTone.Neutral),
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Not a publish status."),
    };

    /// <summary>The publish status badge of a corporate content record (04 2.5.1.4): Taslak and Yayında only; the content has no archive (02 5.2), so Arşiv is refused.</summary>
    public static Badge ForCorporateContent(PublishStatus status) => status == PublishStatus.Archived
        ? throw new ArgumentOutOfRangeException(nameof(status), status, "A corporate content record has no archive status (02 5.2).")
        : For(status);

    /// <summary>The defect claim's badge (04 2.5.1.5): Açık, Çözüldü.</summary>
    public static Badge For(DefectClaimStatus status) => status switch
    {
        DefectClaimStatus.Open => new Badge("Açık", BadgeTone.Warning),
        DefectClaimStatus.Resolved => new Badge("Çözüldü", BadgeTone.Success),
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Not a defect claim status."),
    };

    /// <summary>The contact request's badge (04 2.5.1.6): Açık, Kapatıldı.</summary>
    public static Badge For(ContactRequestStatus status) => status switch
    {
        ContactRequestStatus.Open => new Badge("Açık", BadgeTone.Warning),
        ContactRequestStatus.Closed => new Badge("Kapatıldı", BadgeTone.Neutral),
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Not a contact request status."),
    };
}
