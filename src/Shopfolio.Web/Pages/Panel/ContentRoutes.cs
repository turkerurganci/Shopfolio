using Shopfolio.Modules.CorporateContent.Contracts;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// The closed list of the corporate content's <c>{tip}</c> address segments and the names the panel gives the types (T534; 07 3.1.1; 04 9.13.3): one place for the address of a record's form and for
/// what a group or a "Yeni" button is called. A segment outside the list is no type.
/// </summary>
public static class ContentRoutes
{
    /// <summary>The address of a record's form without its key: <c>/panel/kurumsal-icerik/{tip}</c>.</summary>
    public const string Root = "/panel/kurumsal-icerik";

    /// <summary>The key segment of the new record's form.</summary>
    public const string NewSegment = "yeni";

    private static readonly (PanelContentType Type, string Segment, string GroupName, string NewLabel, string EmptyText)[] Table =
    [
        (PanelContentType.AboutPage, "hakkimizda", "Hakkımızda", "Hakkımızda'yı düzenle", "Hakkımızda henüz yazılmadı."),
        (PanelContentType.Offering, "hizmet", "Hizmetlerimiz", "Yeni hizmet", "Henüz hizmet tanıtımı yok."),
        (PanelContentType.PortfolioItem, "referans", "Referanslarımız", "Yeni referans", "Henüz referans iş yok."),
        (PanelContentType.FaqItem, "sss", "SSS", "Yeni soru", "Henüz soru yok."),
        (PanelContentType.Branch, "sube", "Şubeler", "Yeni şube", "Henüz şube yok."),
        (PanelContentType.CustomPage, "sayfa", "Genel sayfalar", "Yeni sayfa", "Henüz genel sayfa yok."),
        (PanelContentType.Announcement, "duyuru", "Duyuru", "Duyuru yaz", "Duyuru henüz yazılmadı."),
    ];

    /// <summary>The type of an address segment; false for a segment outside the closed list (404).</summary>
    public static bool TryParse(string? segment, out PanelContentType type)
    {
        foreach ((PanelContentType candidate, string name, _, _, _) in Table)
        {
            if (string.Equals(name, segment, StringComparison.Ordinal))
            {
                type = candidate;
                return true;
            }
        }

        type = default;
        return false;
    }

    public static string SegmentOf(PanelContentType type) => Table.Single(row => row.Type == type).Segment;

    public static string GroupNameOf(PanelContentType type) => Table.Single(row => row.Type == type).GroupName;

    public static string NewLabelOf(PanelContentType type) => Table.Single(row => row.Type == type).NewLabel;

    public static string EmptyTextOf(PanelContentType type) => Table.Single(row => row.Type == type).EmptyText;

    /// <summary>The address of a record's form: <c>/panel/kurumsal-icerik/{tip}/{iç anahtar}</c>.</summary>
    public static string FormPath(PanelContentType type, Guid id) => $"{Root}/{SegmentOf(type)}/{id:D}";

    /// <summary>The address of the new record's form.</summary>
    public static string NewPath(PanelContentType type) => $"{Root}/{SegmentOf(type)}/{NewSegment}";

    /// <summary>True for the two types that are one record: Hakkımızda and the announcement (02 3.27.3, 3.27.21).</summary>
    public static bool IsSingle(PanelContentType type) => type is PanelContentType.AboutPage or PanelContentType.Announcement;
}
