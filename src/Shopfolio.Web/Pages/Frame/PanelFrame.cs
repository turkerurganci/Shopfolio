using Shopfolio.Infrastructure.Security;

namespace Shopfolio.Pages.Frame;

/// <summary>
/// The panel's frame: the one view model every panel page response carries and no screen owns (T540; 07 5.5.1; 04 2.2.8). Like the storefront's it is built by a page filter from the request's
/// context (<see cref="PanelFrameBuilder"/>), never added by a page. The panel has no brand colour, no identity block and no ETBİS band (04 2.2.8; 04 2.1.3). The frame carries the sign-out form
/// and links only; it starts no operation (07 5.5.2).
/// </summary>
/// <param name="Sales">The sales state indicator of the top row, "Satış açık" or "Satış kapalı" (04 2.2.8; 02 3.1.5).</param>
/// <param name="SiteViewPath">The "Siteyi görüntüle" link to the storefront's home (04 2.2.8; 04 3.2.20).</param>
/// <param name="Admin">The administrator's name and what stands under it: "Hesabım" and "Çıkış" (04 2.2.8; 04 3.2.21, 3.2.22).</param>
/// <param name="Menu">The eight entries of the menu in the order of 04 3.2; the entries Siparişler and Talepler carry the pending work mark.</param>
public sealed record PanelFrame(PanelSalesIndicator Sales, string SiteViewPath, PanelAdminArea Admin, IReadOnlyList<PanelMenuEntry> Menu);

/// <summary>The sales state indicator (04 2.2.8): the text says the state; the link goes to the "Satış durumu" section of E-44 (04 3.2.19).</summary>
public sealed record PanelSalesIndicator(bool IsOpen, string Text, string Path);

/// <summary>The administrator area of the top row: the name (cut short by the layout when long, 04 2.2.8), "Hesabım" and the sign-out form (UC-109).</summary>
public sealed record PanelAdminArea(string Name, string AccountPath, PanelSignOutForm SignOut);

/// <summary>The panel's sign-out form (UC-109; 07 4.34): POST to <paramref name="Action"/>.</summary>
public sealed record PanelSignOutForm(string Action);

/// <summary>
/// One entry of the panel menu (04 2.2.8, 3.2). An entry either leads to a screen (<paramref name="Path"/>, no <paramref name="Items"/>) or is a group of screens (no path, items). The menu does not
/// repeat the counters: an entry shows one mark while there is pending work behind it, the numbers are on the home page (04 2.6.1). <paramref name="IsActive"/> says the current screen is this entry or
/// one of its items; a narrow screen opens the active group (04 2.2.8).
/// </summary>
public sealed record PanelMenuEntry(string Title, string? Path, bool HasPendingMark, bool IsActive, IReadOnlyList<PanelMenuItem> Items);

/// <summary>One screen under a menu group.</summary>
public sealed record PanelMenuItem(string Title, string Path, bool IsActive);

/// <summary>The addresses and texts of the panel frame (07 §3; 04 3.2): written here once.</summary>
internal static class PanelFramePaths
{
    public const string Home = "/panel";
    public const string Orders = "/panel/siparisler";
    public const string Requests = "/panel/talepler";
    public const string Products = "/panel/urunler";
    public const string Categories = "/panel/kategoriler";
    public const string Coupons = "/panel/kuponlar";
    public const string CorporateContent = "/panel/kurumsal-icerik";
    public const string HomeAndMenu = "/panel/ana-sayfa-ve-menu";
    public const string Brand = "/panel/marka";
    public const string Members = "/panel/uyeler";
    public const string SalesSummary = "/panel/satis-ozeti";
    public const string OperationTrail = "/panel/islem-izi";
    public const string Export = "/panel/disa-aktarma";
    public const string FirmIdentityAndSales = "/panel/firma-kimligi-ve-satis";
    public const string PaymentMethods = "/panel/odeme-yontemleri";
    public const string ShippingAndDefaults = "/panel/kargo-sureler-ve-varsayilanlar";
    public const string LegalTexts = "/panel/yasal-metinler";
    public const string AdminAccounts = "/panel/yonetici-hesaplari";

    /// <summary>E-50: the administrator's own account, "Hesabım" under the name.</summary>
    public const string MyAccount = "/panel/hesabim";

    /// <summary>UC-109: the sign-out form's action.</summary>
    public const string SignOut = "/panel/cikis?handler=SignOut";

    /// <summary>The "Satış durumu" section of E-44 (04 3.2.19).</summary>
    public const string SalesStateSection = "/panel/firma-kimligi-ve-satis#satis-durumu";

    public const string SalesOpenText = "Satış açık";
    public const string SalesClosedText = "Satış kapalı";

    /// <summary>
    /// The first path segments (after <c>/panel</c>) of the screens that open before a session and have no panel frame (04 2.2.9): the sign-in and reset (E-29), the invitation (E-30) and the two
    /// landing steps of E-54 - exactly the open pages of the panel (07 2.3.1), read from their one list. The re-verification step of E-54 (<c>/panel/yeniden-dogrulama</c>) has the frame.
    /// </summary>
    public static readonly IReadOnlyList<string> FramelessSegments =
        [.. SessionSchemes.OpenPanelAddresses.Select(address => address.Split('/')[2]).Distinct(StringComparer.Ordinal)];
}
