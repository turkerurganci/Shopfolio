using Microsoft.AspNetCore.Authentication;

using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Pages.Frame;

/// <summary>
/// Builds the panel frame from the request's context (T540; 07 5.5.1; 04 2.2.8, 3.2): the sales gate's result now, the administrator's name, and the pending work behind the Siparişler and Talepler
/// entries. Everything is read in this request and nothing is kept (the gate is computed every time, 05 4.1.5); the pending marks come from the panel home's counters (<see cref="IPanelHome"/>), so
/// the menu's mark and the home's number can never differ. Reads go through the modules' open surfaces (09 6 item 2).
/// </summary>
internal sealed class PanelFrameBuilder
{
    private readonly ISettingsModule _settings;
    private readonly IAdministrationModule _administration;
    private readonly IPanelHome _home;

    public PanelFrameBuilder(ISettingsModule settings, IAdministrationModule administration, IPanelHome home)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(administration);
        ArgumentNullException.ThrowIfNull(home);
        _settings = settings;
        _administration = administration;
        _home = home;
    }

    /// <summary>
    /// True when the request is one of the screens that have no panel frame (04 2.2.9): <c>/panel/giris</c>, <c>/panel/sifre-sifirlama[/…]</c>, <c>/panel/davet/…</c>, <c>/panel/dogrulama/…</c> and
    /// <c>/panel/geri-al/…</c>. They are the open pages of the panel (<see cref="SessionSchemes.OpenPanelAddresses"/>) and show only the product's panel title.
    /// </summary>
    public static bool IsFramelessScreen(HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string[] segments = (request.Path.Value ?? string.Empty).Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length >= 2
            && string.Equals(segments[0], SessionSchemes.PanelSegment, StringComparison.OrdinalIgnoreCase)
            && PanelFramePaths.FramelessSegments.Contains(segments[1], StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>The panel frame of this request, or null when the request holds no administrator session (the closed door answers such a request before the page runs).</summary>
    public async Task<PanelFrame?> BuildAsync(HttpContext context, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(context);
        AuthenticateResult session = await context.AuthenticateAsync(SessionSchemes.AdminScheme);
        if (!session.Succeeded || session.Principal?.AccountId() is not Guid adminId)
        {
            return null;
        }

        string name = await _administration.GetAdminNameAsync(adminId, ct) ?? string.Empty;
        SalesGate gate = await _settings.GetSalesGateAsync(ct);
        PanelHome home = await _home.GetAsync(ct);
        bool ordersPending = home.Counters.Any(c => c.Kind != PanelCounterKind.OpenRequest && c.Count > 0);
        bool requestsPending = home.Counters.Any(c => c.Kind == PanelCounterKind.OpenRequest && c.Count > 0);

        return new PanelFrame(
            gate.ShowsClosedNotice
                ? new PanelSalesIndicator(false, PanelFramePaths.SalesClosedText, PanelFramePaths.SalesStateSection)
                : new PanelSalesIndicator(true, PanelFramePaths.SalesOpenText, PanelFramePaths.SalesStateSection),
            "/",
            new PanelAdminArea(name, PanelFramePaths.MyAccount, new PanelSignOutForm(PanelFramePaths.SignOut)),
            MenuOf(context.Request.Path.Value ?? string.Empty, ordersPending, requestsPending));
    }

    /// <summary>The eight entries of 04 3.2, in order; the screen at <paramref name="currentPath"/> marks its entry (and its group) active.</summary>
    internal static IReadOnlyList<PanelMenuEntry> MenuOf(string currentPath, bool ordersPending, bool requestsPending) =>
    [
        Screen("Ana sayfa", PanelFramePaths.Home, false, currentPath, exactOnly: true),
        Screen("Siparişler", PanelFramePaths.Orders, ordersPending, currentPath),
        Screen("Talepler", PanelFramePaths.Requests, requestsPending, currentPath),
        Group("Katalog", currentPath, ("Ürünler", PanelFramePaths.Products), ("Kategoriler", PanelFramePaths.Categories), ("Kuponlar", PanelFramePaths.Coupons)),
        Group("İçerik ve site", currentPath, ("Kurumsal içerik", PanelFramePaths.CorporateContent), ("Ana sayfa ve menü", PanelFramePaths.HomeAndMenu), ("Marka", PanelFramePaths.Brand)),
        Screen("Üyeler", PanelFramePaths.Members, false, currentPath),
        Group("Raporlar", currentPath, ("Satış özeti", PanelFramePaths.SalesSummary), ("İşlem izi", PanelFramePaths.OperationTrail), ("Dışa aktarma", PanelFramePaths.Export)),
        Group(
            "Ayarlar",
            currentPath,
            ("Firma kimliği ve satış", PanelFramePaths.FirmIdentityAndSales),
            ("Ödeme yöntemleri", PanelFramePaths.PaymentMethods),
            ("Kargo, süreler ve varsayılanlar", PanelFramePaths.ShippingAndDefaults),
            ("Yasal metinler", PanelFramePaths.LegalTexts),
            ("Yönetici hesapları", PanelFramePaths.AdminAccounts)),
    ];

    private static PanelMenuEntry Screen(string title, string path, bool pending, string currentPath, bool exactOnly = false) =>
        new(title, path, pending, IsCurrent(currentPath, path, exactOnly), []);

    private static PanelMenuEntry Group(string title, string currentPath, params (string Title, string Path)[] items)
    {
        PanelMenuItem[] children = [.. items.Select(item => new PanelMenuItem(item.Title, item.Path, IsCurrent(currentPath, item.Path, exactOnly: false)))];
        return new PanelMenuEntry(title, null, false, children.Any(c => c.IsActive), children);
    }

    // The screen itself and the records under it (/panel/siparisler/{key}); the home entry matches the home only, every screen is under /panel.
    private static bool IsCurrent(string currentPath, string path, bool exactOnly)
    {
        string current = currentPath.TrimEnd('/');
        return string.Equals(current, path, StringComparison.OrdinalIgnoreCase)
            || (!exactOnly && current.StartsWith(path + "/", StringComparison.OrdinalIgnoreCase));
    }
}
