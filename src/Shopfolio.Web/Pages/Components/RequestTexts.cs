using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;

namespace Shopfolio.Pages.Components;

/// <summary>
/// The words of the panel's requests screens (T532; 04 9.10, 9.11): the two kinds, the three statuses of the filter, the five contact topics and the badge of a request row. The names are the source's word for
/// word (04 9.10.3, 9.11.3; 02 3.32.3, 5.10, 5.13); they are written once here and the list, the filter and the detail read them.
/// </summary>
public static class RequestTexts
{
    public const string ContactRequestKind = "İletişim talebi";

    public const string DefectClaimKind = "Ayıp talebi";

    /// <summary>The mark of a defect claim whose notice to the customer did not arrive (04 9.10.4; the text is the panel mark's).</summary>
    public static string EmailNotDeliveredMark => PanelMarks.TextOf(PanelMark.EmailNotDelivered);

    public static string KindOf(PanelRequestKind kind) => kind switch
    {
        PanelRequestKind.ContactRequest => ContactRequestKind,
        PanelRequestKind.DefectClaim => DefectClaimKind,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Not a request kind."),
    };

    /// <summary>The filter's status names (04 9.10.3): Açık, Kapatıldı, Çözüldü.</summary>
    public static string StatusOf(PanelRequestStatus status) => status switch
    {
        PanelRequestStatus.Open => "Açık",
        PanelRequestStatus.Closed => "Kapatıldı",
        PanelRequestStatus.Resolved => "Çözüldü",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Not a request status."),
    };

    /// <summary>The contact topic's name (02 3.32.3): Genel soru, Sipariş hakkında, Ürün hakkında, KVKK talebi, Diğer.</summary>
    public static string TopicOf(ContactTopic topic) => topic switch
    {
        ContactTopic.GeneralQuestion => "Genel soru",
        ContactTopic.AboutOrder => "Sipariş hakkında",
        ContactTopic.AboutProduct => "Ürün hakkında",
        ContactTopic.PersonalDataRequest => "KVKK talebi",
        ContactTopic.Other => "Diğer",
        _ => throw new ArgumentOutOfRangeException(nameof(topic), topic, "Not a contact topic."),
    };

    /// <summary>The status badge of a request row (04 9.10.4; 2.5.1.5, 2.5.1.6): Açık or Kapatıldı for a contact request, Açık or Çözüldü for a defect claim - the badges of <see cref="StatusBadges"/>.</summary>
    public static Badge BadgeOf(PanelRequestKind kind, PanelRequestStatus status) => (kind, status) switch
    {
        (PanelRequestKind.ContactRequest, PanelRequestStatus.Open) => StatusBadges.For(ContactRequestStatus.Open),
        (PanelRequestKind.ContactRequest, PanelRequestStatus.Closed) => StatusBadges.For(ContactRequestStatus.Closed),
        (PanelRequestKind.DefectClaim, PanelRequestStatus.Open) => StatusBadges.For(Shopfolio.Modules.AfterSales.Contracts.DefectClaimStatus.Open),
        (PanelRequestKind.DefectClaim, PanelRequestStatus.Resolved) => StatusBadges.For(Shopfolio.Modules.AfterSales.Contracts.DefectClaimStatus.Resolved),
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The status does not fit the kind of request (04 9.10.3)."),
    };
}
