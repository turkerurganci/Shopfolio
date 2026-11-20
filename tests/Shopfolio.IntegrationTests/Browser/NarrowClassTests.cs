using System.Text.Json;

using Microsoft.Playwright;

namespace Shopfolio.IntegrationTests.Browser;

/// <summary>
/// The rows that only a browser can read at their width (D-1228, D-1418, D-1456; T604 A4): the contact page and the FAQ page in the three classes (VAL-B037, VAL-B067, VAL-A853, VAL-A120, VAL-A121, VAL-A122), the
/// re-authentication screen at 320 (VAL-B078) and the account screen's inner navigation and its keyboard walk (04 5.25.18). Each test reads the layout the page really has (the boxes of the elements in the
/// browser), not the markup: a column is a set of boxes one above the other, a tab row is a set of boxes on one line, "never folded" is a block whose rows are shown whole and not clipped.
/// </summary>
public sealed class NarrowClassTests(BrowserHost host) : BrowserTestBase(host)
{
    /// <summary>The three classes (04 2.1.2): narrow, medium and wide; the wide class begins at 1024.</summary>
    private static readonly int[] Classes = [320, 768, 1280];

    private const int WideFrom = 1024;

    /// <summary>The account screen's navigation does not stand at the left of the sections in the wide class (04 5.25.18; D-1790, src: T604 U); false once the page is fixed, and the observation then asks for the constant's removal.</summary>
    private const bool KnownNavigationNotAtTheLeft = true;

    /// <summary>The boxes of every element each selector matches, in document order: place, size, whether it is shown, whether it clips its content, its text and address, whether a closed or hidden ancestor holds it.</summary>
    private const string BoxesScript = """
        (sels) => JSON.stringify(sels.map((s) => [...document.querySelectorAll(s)].map((e) => {
          const cs = getComputedStyle(e); const r = e.getBoundingClientRect();
          return { id: e.id || '', x: r.left, y: r.top + scrollY, w: r.width, h: r.height,
            vis: cs.display !== 'none' && cs.visibility !== 'hidden' && r.width > 0 && r.height > 0,
            clip: (cs.overflowX !== 'visible' && e.scrollWidth > e.clientWidth + 1) || cs.textOverflow === 'ellipsis',
            text: (e.textContent || '').replace(/\s+/g, ' ').trim().slice(0, 160), href: e.getAttribute('href') || '', target: e.getAttribute('target') || '',
            hidden: !!e.closest('[hidden],details:not([open]),[inert],[aria-hidden=true]') };
        })))
        """;

    /// <summary>The controls of the page's main part a keyboard reaches, in document order (a control that is disabled, out of the tab order or not drawn is not one).</summary>
    private const string Focusables = "const T = () => [...document.querySelectorAll('main a[href], main button, main input:not([type=hidden]), main select, main textarea')].filter((e) => !e.disabled && e.tabIndex >= 0 && e.getClientRects().length > 0 && getComputedStyle(e).visibility !== 'hidden');";

    private const string ReadFocus = "const R = () => { const e = document.activeElement; const r = e.getBoundingClientRect(); return JSON.stringify({ index: T().indexOf(e), inView: r.width > 0 && r.top >= 0 && r.left >= 0 && r.bottom <= innerHeight && r.right <= innerWidth + 1 }); };";

    private sealed record Box(string Id, double X, double Y, double W, double H, bool Visible, bool Clipped, string Text, string Href, string Target, bool Hidden)
    {
        public double Right => X + W;

        public double Bottom => Y + H;

        /// <summary>Shown whole: drawn, not under a closed or hidden ancestor, not clipped.</summary>
        public bool Whole => Visible && !Hidden && !Clipped;
    }

    private static async Task<List<List<Box>>> BoxesAsync(IPage page, params string[] selectors)
    {
        using JsonDocument all = JsonDocument.Parse(await page.EvaluateAsync<string>(BoxesScript, selectors));
        List<List<Box>> result = [];
        foreach (JsonElement group in all.RootElement.EnumerateArray())
        {
            result.Add([.. group.EnumerateArray().Select(e => new Box(
                e.GetProperty("id").GetString() ?? string.Empty,
                e.GetProperty("x").GetDouble(),
                e.GetProperty("y").GetDouble(),
                e.GetProperty("w").GetDouble(),
                e.GetProperty("h").GetDouble(),
                e.GetProperty("vis").GetBoolean(),
                e.GetProperty("clip").GetBoolean(),
                e.GetProperty("text").GetString() ?? string.Empty,
                e.GetProperty("href").GetString() ?? string.Empty,
                e.GetProperty("target").GetString() ?? string.Empty,
                e.GetProperty("hidden").GetBoolean()))]);
        }

        return result;
    }

    /// <summary>One column: the boxes, taken in the order of their place from the top, never share a line (each begins where the one before it ended).</summary>
    private static bool Stacked(IEnumerable<Box> boxes)
    {
        List<Box> ordered = [.. boxes.OrderBy(b => b.Y)];
        for (int i = 1; i < ordered.Count; i++)
        {
            if (ordered[i].Y < ordered[i - 1].Bottom - 1)
            {
                return false;
            }
        }

        return true;
    }

    private static bool SameLeft(IEnumerable<Box> boxes) => boxes.Select(b => b.X).Distinct().All(x => Math.Abs(x - boxes.First().X) <= 1);

    /// <summary>The reflow reading (WCAG 1.4.10): the page's horizontal overflow and the elements that reach past its right edge or clip their text.</summary>
    private static async Task<(int Over, int Past, int Clipped)> ReflowAsync(IPage page)
    {
        using JsonDocument reflow = JsonDocument.Parse(await page.EvaluateAsync<string>(AuditScripts.Reflow));
        JsonElement root = reflow.RootElement;

        // The one cut the documents allow: the long brand name of the narrow class is cut on one line and stands whole in the tab title and the footer (04 8.3.2 row 1; the header's single-line brand).
        int clipped = root.GetProperty("clipped").EnumerateArray().Count(e => e.GetString()?.Contains("u-single-line", StringComparison.Ordinal) != true);
        return (root.GetProperty("over").GetInt32(), root.GetProperty("past").GetArrayLength(), clipped);
    }

    private static string Class(int width) => width >= WideFrom ? "geniş" : width >= 768 ? "orta" : "dar";

    private static string Digits(string text) => new([.. text.Where(char.IsAsciiDigit)]);

    private async Task<WatchedPage> OpenAsync(IBrowserContext context, string path)
    {
        WatchedPage page = await NewPageAsync(context);
        await GoAsync(page, path);
        await WaitForStylesAsync(page);
        return page;
    }

    /// <summary>The contact page (E-10) in each class: the two columns of the wide class and the one column of the others (VAL-B037), the identity block whole and equal to the footer's (VAL-A853), the contact details (VAL-A120), the branches' map links (VAL-A121) and the form that can be filled (VAL-A122).</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheContactPage_ShowsItsContentInTheClassOfEachWidth_D1228(string engine, ScriptPath path)
    {
        foreach (int width in Classes)
        {
            await using IBrowserContext context = await AccessibilityScanTests.AuditContextAsync(Host, engine, path, Actor.Visitor, width);
            WatchedPage page = await OpenAsync(context, "/iletisim");
            List<List<Box>> boxes = await BoxesAsync(page.Page, ".contact__identity", ".contact__form", ".contact__branches", ".contact__identity .identity-block__row", ".site-footer .identity-block__row", ".branch", ".branch__map", "#footer-contact-heading a", ".contact__identity details, .contact__identity button, .contact__identity [aria-expanded]", ".site-footer__identity .identity-block");
            Box identity = boxes[0][0];
            Box form = boxes[1][0];
            Box branches = boxes[2][0];
            List<Box> pageRows = boxes[3];
            List<Box> footerRows = boxes[4];
            (int over, int past, int clipped) = await ReflowAsync(page.Page);
            string label = Class(width);

            // VAL-B037: the class's layout; the identity block is whole in every class and is behind no link or fold.
            bool wide = width >= WideFrom;
            bool layout = wide
                ? identity.Right <= form.X + 1 && branches.Right <= form.X + 1 && Math.Abs(branches.X - identity.X) <= 1 && branches.Y >= identity.Bottom - 1
                : identity.Bottom <= form.Y + 1 && form.Bottom <= branches.Y + 1 && Math.Abs(form.X - identity.X) <= 1 && Math.Abs(branches.X - identity.X) <= 1;
            bool whole = pageRows.Count > 0 && pageRows.All(r => r.Whole) && boxes[8].Count == 0;
            await ObserveAsync(
                "VAL-B037", path, engine, page, $"{label} sınıf ({width}): iletişim sayfasının yerleşimi",
                wide ? "solda kimlik bloğu ve şubeler, sağda form; kimlik bloğu tam ve bir bağlantının ya da katlanmanın arkasında değil; yatay kaydırma yok" : "tek sütun: kimlik bloğu, form, şubeler sırasıyla; kimlik bloğu tam ve katlanmamış; yatay kaydırma yok",
                $"kimlik x {identity.X:0}-{identity.Right:0} y {identity.Y:0}-{identity.Bottom:0}; form x {form.X:0}-{form.Right:0} y {form.Y:0}-{form.Bottom:0}; şubeler x {branches.X:0}-{branches.Right:0} y {branches.Y:0}; kimlik satırı {pageRows.Count}, tam {pageRows.Count(r => r.Whole)}, katlayan/bağlantı {boxes[8].Count}; yatay taşma {over}, sağı aşan {past}, kırpılan {clipped}",
                layout && whole && over <= 0 && past == 0 && clipped == 0);

            // VAL-A853: the footer's block and the page's block are the same fields, both whole, the footer's under the heading "İletişim" that leads to the contact page.
            List<string> pageTexts = [.. pageRows.Select(r => r.Text)];
            List<string> footerTexts = [.. footerRows.Select(r => r.Text)];
            Box? heading = boxes[7].FirstOrDefault();
            bool same = pageTexts.Count > 0 && pageTexts.SequenceEqual(footerTexts);
            bool footerWhole = footerRows.Count > 0 && footerRows.All(r => r.Whole);
            bool headingOk = heading is { Whole: true, Href: "/iletisim", Text: "İletişim" } && boxes[9].Count == 1;
            await ObserveAsync(
                "VAL-A853", path, engine, page, $"{label} sınıf ({width}): kimlik bloğu altbilgide ve iletişim sayfasında",
                "iki yerde aynı alanlar tam görünür, kısaltılmaz; altbilgide \"İletişim\" başlığı altında durur ve başlık iletişim sayfasına götürür",
                $"sayfada {pageTexts.Count} alan, altbilgide {footerTexts.Count} alan, eşit {same}; altbilgi tam {footerWhole}; başlık \"{heading?.Text}\" {heading?.Href}; başlığın altındaki blok {boxes[9].Count}",
                same && footerWhole && headingOk && pageRows.All(r => r.Whole));

            // VAL-A120: the contact details the firm entered (the phone and the e-mail address of the seed) are in the block.
            string blockText = string.Join(" | ", pageTexts);
            bool phone = pageRows.Any(r => Digits(r.Text).EndsWith("3121234567", StringComparison.Ordinal));
            bool mail = pageRows.Any(r => r.Text.Contains("firma@example.com", StringComparison.Ordinal));
            await ObserveAsync(
                "VAL-A120", path, engine, page, $"{label} sınıf ({width}): firmanın iletişim bilgileri",
                "sayfa girilen telefonu, e-posta adresini ve adresi gösterir",
                $"telefon {phone}; e-posta {mail}; adres satırı {pageRows.Any(r => r.Text.Contains("Kızılay 2", StringComparison.Ordinal))}",
                phone && mail && pageRows.Any(r => r.Text.Contains("Kızılay 2", StringComparison.Ordinal)));

            // VAL-A121: each branch carries "Haritada aç" and the link opens that branch's own address.
            List<Box> maps = boxes[6];
            bool eachBranch = boxes[5].Count >= 1 && maps.Count == boxes[5].Count && maps.All(m => m.Whole && m.Text == "Haritada aç" && m.Href == BrowserSeed.BranchMapUrl && m.Target == "_blank");
            await ObserveAsync(
                "VAL-A121", path, engine, page, $"{label} sınıf ({width}): şubenin haritada aç bağlantısı",
                "her şube \"Haritada aç\" bağlantısıyla durur; bağlantı o şubenin konumunu (kayıtlı adresi) yeni sekmede açar",
                $"şube {boxes[5].Count}; bağlantı {maps.Count}; ilki \"{maps.FirstOrDefault()?.Text}\" {maps.FirstOrDefault()?.Href} {maps.FirstOrDefault()?.Target}; şube adı görünür {boxes[5].Any(b => b.Text.Contains(BrowserSeed.BranchName, StringComparison.Ordinal))}",
                eachBranch && boxes[5].Any(b => b.Text.Contains(BrowserSeed.BranchName, StringComparison.Ordinal)));

            // VAL-A122: the form is shown and can be filled (it is not sent: a sent request is a record of the seed).
            string[] names = ["Input.Name", "Input.Email", "Input.Phone", "Input.Message"];
            string[] values = ["Deneme Kişi", "deneme@example.com", "5321234567", "Mesaj metni"];
            List<string> read = [];
            for (int i = 0; i < names.Length; i++)
            {
                ILocator field = page.Page.Locator($"#contact-form [name='{names[i]}']");
                await field.FillAsync(values[i]);
                read.Add(await field.InputValueAsync());
            }

            ILocator topic = page.Page.Locator("#contact-form [name='Input.ContactTopic']");
            string topicTag = await topic.First.EvaluateAsync<string>("e => e.tagName");
            if (topicTag == "SELECT")
            {
                await topic.SelectOptionAsync(new SelectOptionValue { Index = 1 });
            }

            bool filled = read.SequenceEqual(values);
            Box formBox = (await BoxesAsync(page.Page, "#contact-form button[type=submit]"))[0][0];
            await ObserveAsync(
                "VAL-A122", path, engine, page, $"{label} sınıf ({width}): iletişim formu doldurulur",
                "form sayfada görünür; ad, e-posta, telefon, konu ve mesaj alanları doldurulur ve girilen değer alanda durur; Gönder düğmesi görünür",
                $"geri okunan {string.Join(" / ", read)}; konu alanı {topicTag}; Gönder görünür {formBox.Whole} ({formBox.Text})",
                filled && formBox.Whole && formBox.Text == "Gönder" && form.Visible);
            await page.Page.CloseAsync();
        }

        Assert.Empty(Failures);
    }

    /// <summary>The FAQ page (E-09; VAL-B067): one column in every class, no question and no answer hidden.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheFaqPage_IsOneColumnWithNoQuestionHidden_D1228_VALB067(string engine, ScriptPath path)
    {
        foreach (int width in Classes)
        {
            await using IBrowserContext context = await AccessibilityScanTests.AuditContextAsync(Host, engine, path, Actor.Visitor, width);
            WatchedPage page = await OpenAsync(context, "/sss");
            List<List<Box>> boxes = await BoxesAsync(page.Page, ".faq__item", ".faq__question", ".faq__answer");
            (int over, int past, int clipped) = await ReflowAsync(page.Page);
            List<Box> items = boxes[0];
            bool every = items.Count == BrowserSeed.FaqSeed.Length
                && boxes[1].Count == items.Count && boxes[2].Count == items.Count
                && boxes[1].Zip(BrowserSeed.FaqSeed).All(pair => pair.First.Whole && pair.First.Text == pair.Second.Question)
                && boxes[2].Zip(BrowserSeed.FaqSeed).All(pair => pair.First.Whole && pair.First.H > 0 && pair.First.Text == System.Text.RegularExpressions.Regex.Replace(pair.Second.Answer, "<[^>]+>", string.Empty));
            bool column = Stacked(items) && SameLeft(items);
            await ObserveAsync(
                "VAL-B067", path, engine, page, $"{Class(width)} sınıf ({width}): sık sorulan sorular",
                "sayfa tek sütundur; yayındaki her soru ve cevabı tam görünür, hiçbiri gizlenmez; yatay kaydırma yok",
                $"soru {items.Count} (beklenen {BrowserSeed.FaqSeed.Length}); tek sütun {column}; her soru ve cevap tam {every}; yatay taşma {over}, sağı aşan {past}, kırpılan {clipped}",
                every && column && over <= 0 && past == 0 && clipped == 0);
            await page.Page.CloseAsync();
        }

        Assert.Empty(Failures);
    }

    /// <summary>The re-authentication screen (E-24; VAL-B078): one column in every class, the content hidden by none.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheReauthenticationScreen_IsOneColumnWithNothingHidden_D1418_VALB078(string engine, ScriptPath path)
    {
        foreach (int width in Classes)
        {
            await using IBrowserContext context = await AccessibilityScanTests.AuditContextAsync(Host, engine, path, Actor.Customer, width);
            WatchedPage page = await OpenAsync(context, "/hesap/yeniden-dogrulama?reauthenticatedOperation=EmailChange");
            List<List<Box>> boxes = await BoxesAsync(page.Page, "h1", "#reauth-operation", "#reauth-form", "#reauth-google", "#reauth-set-password", "#reauth-cancel", "#reauth-form input:not([type=hidden])", "#reauth-form button, #reauth-google button");
            (int over, int past, int clipped) = await ReflowAsync(page.Page);
            List<Box> blocks = [.. boxes[0], .. boxes[1], .. boxes[2], .. boxes[3], .. boxes[4], .. boxes[5]];
            List<Box> controls = [.. boxes[6], .. boxes[7]];
            bool column = blocks.Count >= 4 && Stacked(blocks);
            bool inside = blocks.Concat(controls).All(b => b.X >= 0 && b.Right <= width + 1);
            bool shown = boxes[0].Count == 1 && boxes[1].Count == 1 && boxes[5].Count == 1 && blocks.Concat(controls).All(b => b.Whole) && controls.Count >= 1;
            await ObserveAsync(
                "VAL-B078", path, engine, page, $"{Class(width)} sınıf ({width}): yeniden doğrulama ekranı",
                "sayfa tek sütundur; başlık, işlem cümlesi, yöntemler ve Vazgeç bağlantısı tam görünür, hiçbiri gizlenmez; yatay kaydırma yok",
                $"adres {new Uri(page.Page.Url).PathAndQuery}; blok {blocks.Count}, denetim {controls.Count}; tek sütun {column}; hepsi sınır içinde {inside}; hepsi görünür {shown}; yatay taşma {over}, sağı aşan {past}, kırpılan {clipped}",
                new Uri(page.Page.Url).AbsolutePath == "/hesap/yeniden-dogrulama" && column && inside && shown && over <= 0 && past == 0 && clipped == 0);
            await page.Page.CloseAsync();
        }

        Assert.Empty(Failures);
    }

    /// <summary>
    /// The account screen (E-25; 04 5.25.18): the inner navigation is a row of tabs at the head in the narrow and medium classes and a vertical list in the wide class, the "Hesabı sil" section is the last in every class,
    /// and the controls of the main part are reached one after the other in the order of the document by the keyboard, each in view.
    /// </summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheAccountScreen_KeepsItsNavigationAndItsLastSectionInEveryClass_D1456(string engine, ScriptPath path)
    {
        foreach (int width in Classes)
        {
            await using IBrowserContext context = await AccessibilityScanTests.AuditContextAsync(Host, engine, path, Actor.Customer, width);
            WatchedPage page = await OpenAsync(context, "/hesap");
            List<List<Box>> boxes = await BoxesAsync(page.Page, ".account-nav", ".account-nav__link", ".account-section", "#account-delete", "main");
            (int over, int past, int clipped) = await ReflowAsync(page.Page);
            Box nav = boxes[0][0];
            List<Box> links = boxes[1];
            List<Box> sections = boxes[2];
            bool wide = width >= WideFrom;
            // A row of tabs: the links sit side by side (the row wraps when the width is short, so at least the first two share a line, left to right) and the navigation stands at the head, before the sections.
            bool tabs = links.Count >= 2 && links.All(l => l.Whole && l.X >= 0 && l.Right <= width + 1) && Math.Abs(links[1].Y - links[0].Y) <= 4 && links[1].X > links[0].X && nav.Bottom <= sections[0].Y + 1;
            bool list = links.Count >= 2 && links.All(l => l.Whole) && SameLeft(links) && links.Skip(1).Zip(links).All(pair => pair.First.Y >= pair.Second.Bottom - 1);
            await ObserveAsync(
                "E-25", path, engine, page, $"{Class(width)} sınıf ({width}): iç gezinme",
                wide ? "iç gezinme dikey listedir (bağlantılar alt alta, aynı hizada); yatay kaydırma yok" : "iç gezinme yatay sekmelerdir (bağlantılar yan yana) ve bölümlerden önce, ekranın başındadır; yatay kaydırma yok",
                $"bağlantı {links.Count}; yatay sekme {tabs}; dikey liste {list}; gezinme x {nav.X:0}-{nav.Right:0} y {nav.Y:0}-{nav.Bottom:0}; ilk bölüm x {sections[0].X:0} y {sections[0].Y:0}; yatay taşma {over}, sağı aşan {past}, kırpılan {clipped}",
                (wide ? list : tabs) && over <= 0 && past == 0 && clipped == 0);

            if (wide)
            {
                // 04 5.25.18: in the wide class the navigation stands at the left and the sections at the right. The account screen's page does not put its content in the layout the address and order screens use
                // (account-layout), so the list is vertical but stands above the sections at the full width: the known departure D-1790 (src, T604 U). Two ways: when the page is fixed this observation fails
                // and the constant is removed, as the title measure's known list is (D-1788).
                bool atTheLeft = nav.Right <= sections[0].X + 1;
                await ObserveAsync(
                    "E-25", path, engine, page, $"{Class(width)} sınıf ({width}): iç gezinme solda, bölümler sağda (bilinen sapma D-1790)",
                    $"belge (04 5.25.18): gezinme solda, bölümler sağda; bu sürümde sayfa yerleşim sarmalayıcısını (account-layout) kullanmaz, gezinme bölümlerin üstünde tam genişliktedir ve sapma {KnownNavigationNotAtTheLeft}",
                    $"gezinme x {nav.X:0}-{nav.Right:0} y {nav.Y:0}-{nav.Bottom:0}; ilk bölüm x {sections[0].X:0} y {sections[0].Y:0}; solda {atTheLeft}",
                    atTheLeft != KnownNavigationNotAtTheLeft);
            }

            Box delete = boxes[3][0];
            bool last = sections[^1].Id == "account-delete" && sections.All(s => s.Id == "account-delete" || s.Y < delete.Y) && delete.Whole;
            await ObserveAsync(
                "E-25", path, engine, page, $"{Class(width)} sınıf ({width}): Hesabı sil bölümü",
                "Hesabı sil bölümü her sınıfta bölümlerin en altındadır ve tam görünür",
                $"bölüm {sections.Count}; sonuncusu \"{sections[^1].Id}\"; silme bölümü y {delete.Y:0} (öteki bölümlerin en altı {sections.Where(s => s.Id != "account-delete").Max(s => s.Y):0})",
                last);

            await WalkWithTheKeyboardAsync(engine, path, page, width);
            await page.Page.CloseAsync();
        }

        Assert.Empty(Failures);
    }

    /// <summary>
    /// The keyboard walk of the main part: from the first control the Tab key reaches the next ones one after the other in the order of the document, each in view (Chromium and Firefox). WebKit's Tab key does not take a link to
    /// its focus on every host (see <see cref="AccessibilityAuditTests"/>), so there the controls are focused one by one in the order of the document and each is read in view.
    /// </summary>
    private async Task WalkWithTheKeyboardAsync(string engine, ScriptPath path, WatchedPage page, int width)
    {
        using JsonDocument targets = JsonDocument.Parse(await page.Page.EvaluateAsync<string>($"() => {{ {Focusables} return JSON.stringify(T().map((e) => e.id || e.getAttribute('name') || e.getAttribute('href') || e.tagName)); }}"));
        List<string> expected = [.. targets.RootElement.EnumerateArray().Select(e => e.GetString() ?? string.Empty)];
        List<int> reached = [];
        List<string> outOfView = [];
        bool byTab = engine is "chromium" or "firefox";
        for (int i = 0; i < expected.Count; i++)
        {
            if (i == 0 || !byTab)
            {
                await page.Page.EvaluateAsync($"(i) => {{ {Focusables} T()[i].focus(); }}", i);
            }
            else
            {
                await page.Page.Keyboard.PressAsync("Tab");
            }

            using JsonDocument read = JsonDocument.Parse(await page.Page.EvaluateAsync<string>($"() => {{ {Focusables} {ReadFocus} return R(); }}"));
            reached.Add(read.RootElement.GetProperty("index").GetInt32());
            if (!read.RootElement.GetProperty("inView").GetBoolean())
            {
                outOfView.Add(expected[i]);
            }
        }

        bool inOrder = reached.SequenceEqual(Enumerable.Range(0, expected.Count));
        await ObserveAsync(
            "E-25", path, engine, page, $"{Class(width)} sınıf ({width}): klavyeyle gezinme",
            "ekranın ana bölümünün denetimlerine klavyeyle belge sırasıyla, tek tek ulaşılır ve odaktaki denetim görünür alandadır (WCAG 2.1.1, 2.4.3)",
            $"denetim {expected.Count} ({(byTab ? "Tab tuşu" : "belge sırasıyla odak")}); ulaşılan sıra {string.Join(",", reached)}; görünür alanın dışında {outOfView.Count}" + (outOfView.Count > 0 ? ": " + string.Join(" / ", outOfView.Take(4)) : string.Empty),
            expected.Count >= 6 && inOrder && outOfView.Count == 0);
    }
}
