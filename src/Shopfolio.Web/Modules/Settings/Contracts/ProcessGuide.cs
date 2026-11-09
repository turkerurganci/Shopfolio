namespace Shopfolio.Modules.Settings.Contracts;

/// <summary>One titled paragraph of the process guide: the title and the text, both fixed.</summary>
public sealed record ProcessGuideSection(string Title, string Text);

/// <summary>
/// The "İşlem rehberi" page and the fixed "how the order is placed" text of the confirmation step (T356; 02 3.24.3, 3.24.8; 04 5.11.5; K-547, K-626). One text lives in one place: the
/// four sections below are what the confirmation step shows above the boxes, what the Pre-Information Form carries as its own part (the generated form takes it through the
/// <c>HowTheOrderIsPlaced</c> value at the version's making) and what the page shows. It is the product's own fixed text: no company setting, no edited field, not a condition of the
/// sales gate - the page shows while sales are closed too, and its link stands in the footer of every page, the home page included (<see cref="IsInFooterOfEveryPage"/>).
/// </summary>
public static class ProcessGuide
{
    /// <summary>The page's heading and the footer link's text (04 3.1.19).</summary>
    public const string Title = "İşlem rehberi";

    /// <summary>The page's first path segment (07 2.1.2, UC-18); the address rules list it among the fixed pages.</summary>
    public const string PageSegment = "islem-rehberi";

    /// <summary>The footer carries the link on every page next to the legal text links; unlike the two firm-edited texts it never waits for a publication (04 5.11.9).</summary>
    public const bool IsInFooterOfEveryPage = true;

    /// <summary>Sales being closed or the gate being open changes nothing: the page is not a condition of the sales gate and shows in every state (02 3.24.8; K-626).</summary>
    public const bool ShowsWhileSalesAreClosed = true;

    /// <summary>The four pieces of information of 02 3.24.3 and 3.24.8, in the order the spec lists them.</summary>
    public static IReadOnlyList<ProcessGuideSection> Sections { get; } =
    [
        new(
            "Sözleşmenin kurulması için izlenecek adımlar",
            "Sepetinizi oluşturun; ödeme adımında iletişim, teslimat ve fatura bilgilerinizi yazın, ödeme yönteminizi seçin. Onay bölümünde sipariş özetini, Ön Bilgilendirme Formu'nu ve "
            + "Mesafeli Satış Sözleşmesi'ni okuyun, gereken onay kutularını işaretleyin ve \"Siparişi onayla — ödeme yükümlülüğü doğar\" düğmesine basın. Sözleşme siparişiniz onaylandığı "
            + "anda kurulur ve ödeme yükümlülüğünüz de bu anda doğar."),
        new(
            "Sözleşme metninin saklanması",
            "Onayladığınız Ön Bilgilendirme Formu ve Mesafeli Satış Sözleşmesi, sipariş sayfanızda yasal saklama süresi boyunca erişilebilir kalır; sipariş sayfasından her zaman yeniden okuyabilirsiniz."),
        new(
            "Veri giriş hatalarının düzeltilmesi",
            "Yazdığınız bilgilerdeki hataları siparişi onaylamadan önce, onay özetinden ilgili bölüme dönerek düzeltebilirsiniz."),
        new(
            "Gizlilik kuralları ve uyuşmazlık yolları",
            "Kişisel verilerinizin nasıl işlendiği, sitenin altbilgisindeki ve onay adımındaki aydınlatma metni bağlantısında yazar. Uyuşmazlıklarda tüketici hakem heyetine ya da — dava açılmadan "
            + "önce arabulucuya başvurma şartıyla — tüketici mahkemesine başvurabilirsiniz."),
    ];

    /// <summary>
    /// The text as one plain block (each section a numbered title line and its text line, a blank line between): the value of the form's <c>HowTheOrderIsPlaced</c> token and the
    /// body of the step's fixed text.
    /// </summary>
    public static string Text { get; } = string.Join("\n\n", Sections.Select((s, i) => $"{i + 1}. {s.Title}\n{s.Text}"));
}
