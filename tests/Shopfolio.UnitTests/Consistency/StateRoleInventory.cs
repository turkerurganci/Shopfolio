namespace Shopfolio.UnitTests.Consistency;

/// <summary>One screen as the code has it (T547; 04 4, 6.1.1): its id, the id of its block in 6.2 or 6.3, the page routes that carry it and - for a screen that no page route carries - the file of the shared view that draws it.</summary>
internal sealed record ScreenEntry(string Screen, string Block, IReadOnlyList<string> Routes, string? View = null);

/// <summary>A page of the web project that is no screen, with the reason (a handler that leaves at once; the document's own rule, 04 4.4).</summary>
internal sealed record NonScreenPage(string Route, string Reason);

/// <summary>
/// The code side of the state by role matrix (T547; 04 6.1 to 6.3; 09 21.4 item 1): the fifty-three screens with the pages that carry them, the pages that are no screen, and the words the code uses for the
/// things the matrix is about - the files that read a gate, the files that open a draft for the administrator, the names of the statuses. The comparison with the document is <see cref="StateRoleCheck"/>.
/// There is no list of known differences: a page without a screen, a screen without a page and a gate or draft read in a screen the document does not name each make the check report.
/// </summary>
internal static class StateRoleInventory
{
    private const string AccessRoot = "/siparis/erisim/{accessKey:regex(^[A-Za-z0-9_-]+\\z)}";
    private const string MemberRoot = "/siparis/{orderNumber:regex(^[0-9][0-9][0-9][0-9]-[A-Za-z0-9]+\\z)}";

    /// <summary>The fifty-three screens: E-01 to E-28 (6.2.n = E-n) and the panel's twenty-five (6.3.1 = E-29 to 6.3.19 = E-47, E-48 fell, 6.3.20 = E-49 to 6.3.25 = E-54).</summary>
    public static IReadOnlyList<ScreenEntry> Screens { get; } =
    [
        Customer(1, "/"),
        Customer(2, "/kategori/{slug}"),
        Customer(3, "/arama"),
        Customer(4, "/urun/{slug}"),
        Customer(5, "/urun/{slug}"),
        new ScreenEntry("E-06", "6.2.6", [], View: "Pages/Shared/_NotFound.cshtml"),
        Customer(7, "/hizmetler", "/referanslar"),
        Customer(8, "/hakkimizda", "/hizmet/{slug}", "/referans/{slug}", "/sayfa/{slug}"),
        Customer(9, "/sss"),
        Customer(10, "/iletisim"),
        Customer(11, "/aydinlatma-metni", "/cerez-politikasi", "/islem-rehberi"),
        Customer(12, "/sepet"),
        Customer(13, "/odeme"),
        Customer(14, AccessRoot + "/teyit", MemberRoot + "/teyit", "/odeme/donus/{accessKey:regex(^[A-Za-z0-9_-]+\\z)}"),
        Customer(15, "/siparis-takibi"),
        Customer(16, AccessRoot, MemberRoot),
        Customer(17, AccessRoot + "/iptal", MemberRoot + "/iptal", AccessRoot + "/gecikme-feshi", MemberRoot + "/gecikme-feshi"),
        Customer(18, AccessRoot + "/cayma", MemberRoot + "/cayma"),
        Customer(19, AccessRoot + "/ayip-talebi", MemberRoot + "/ayip-talebi"),
        Customer(20, "/kayit"),
        Customer(21, "/dogrulama/{key}"),
        Customer(22, "/giris", "/giris/google"),
        Customer(23, "/sifre-sifirlama/{key?}"),
        Customer(24, "/hesap/yeniden-dogrulama"),
        Customer(25, "/hesap", "/hesap/silindi"),
        Customer(26, "/hesap/adresler"),
        Customer(27, "/hesap/siparisler"),
        Customer(28, "/geri-al/{key}"),
        Panel(1, 29, "/panel/giris", "/panel/sifre-sifirlama/{key?}"),
        Panel(2, 30, "/panel/davet/{key}"),
        Panel(3, 31, "/panel"),
        Panel(4, 32, "/panel/urunler"),
        Panel(5, 33, "/panel/urunler/{id:guid}", "/panel/urunler/yeni"),
        Panel(6, 34, "/panel/kategoriler"),
        Panel(7, 35, "/panel/kuponlar/{key?}"),
        Panel(8, 36, "/panel/siparisler"),
        Panel(9, 37, "/panel/siparisler/{id:guid}"),
        Panel(10, 38, "/panel/talepler"),
        Panel(11, 39, "/panel/talepler/{id:guid}"),
        Panel(12, 40, "/panel/uyeler"),
        Panel(13, 41, "/panel/kurumsal-icerik", "/panel/kurumsal-icerik/{tip}/{id:guid}", "/panel/kurumsal-icerik/{tip}/yeni"),
        Panel(14, 42, "/panel/ana-sayfa-ve-menu"),
        Panel(15, 43, "/panel/marka"),
        Panel(16, 44, "/panel/firma-kimligi-ve-satis"),
        Panel(17, 45, "/panel/odeme-yontemleri"),
        Panel(18, 46, "/panel/kargo-sureler-ve-varsayilanlar"),
        Panel(19, 47, "/panel/yasal-metinler"),
        Panel(20, 49, "/panel/yonetici-hesaplari"),
        Panel(21, 50, "/panel/hesabim"),
        Panel(22, 51, "/panel/satis-ozeti"),
        Panel(23, 52, "/panel/islem-izi"),
        Panel(24, 53, "/panel/disa-aktarma"),
        Panel(25, 54, "/panel/yeniden-dogrulama", "/panel/dogrulama/{key}", "/panel/geri-al/{key}"),
    ];

    /// <summary>The pages that are no screen: the two sign-out addresses answer with a redirect and draw nothing (04 3.6.4).</summary>
    public static IReadOnlyList<NonScreenPage> NonScreenPages { get; } =
    [
        new NonScreenPage("/cikis", "the customer's sign-out handler redirects and draws nothing"),
        new NonScreenPage("/panel/cikis", "the panel's sign-out handler redirects and draws nothing"),
    ];

    /// <summary>The route two screens share: the product page draws E-05 (the archived product) at the address of E-04; the first screen is the page's.</summary>
    public static IReadOnlyList<string> SharedRoutes { get; } = ["/urun/{slug}"];

    /// <summary>The files that are no page but are one screen's code (the base class of the content record pages), by file relative to <c>src/Shopfolio.Web</c>.</summary>
    public static IReadOnlyDictionary<string, string> SupportFiles { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["Pages/Content/ContentRecordPages.cs"] = "E-08",
    };

    /// <summary>The words that mean the sales gate is read in a file (the operation asked, the refusal code, the storefront's closed message).</summary>
    public static IReadOnlyList<string> SalesGateTokens { get; } =
        ["SalesGateOperation.", "ErrorCode.SalesClosed", "SalesClosedMessages.StorefrontText", "SalesClosedMessages.ForStorefront"];

    /// <summary>The words that mean the gate of the data-collecting entries is read in a file.</summary>
    public static IReadOnlyList<string> DataGateTokens { get; } =
        ["DataCollectionEntry.", "DataCollectionClosed", "SalesClosedMessages.DataEntryText"];

    /// <summary>The words that mean a page opens a draft record for the administrator (the draft band and the administrator's reading of a list).</summary>
    public static IReadOnlyList<string> DraftTokens { get; } =
        ["SetDraftPreview(", "asAdministrator"];

    /// <summary>The screens the draft is the consequence on the administrator's row, with no draft read of their own: the not-found screen the draft's address no longer shows an administrator (6.2.6.2), and the list that never shows a draft (6.2.7.1).</summary>
    public static IReadOnlyList<string> DraftConsequenceScreens { get; } = ["E-06", "E-07"];

    /// <summary>The folders whose pages are shared frame and components, not a screen of their own.</summary>
    public static IReadOnlyList<string> SharedFolders { get; } = ["Pages/Frame/", "Pages/Components/", "Pages/Shared/"];

    private static ScreenEntry Customer(int number, params string[] routes) =>
        new("E-" + number.ToString("00", System.Globalization.CultureInfo.InvariantCulture), "6.2." + number.ToString(System.Globalization.CultureInfo.InvariantCulture), routes);

    private static ScreenEntry Panel(int block, int number, params string[] routes) =>
        new("E-" + number.ToString("00", System.Globalization.CultureInfo.InvariantCulture), "6.3." + block.ToString(System.Globalization.CultureInfo.InvariantCulture), routes);
}
