using System.Text.Json;

using Microsoft.Playwright;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.IntegrationTests.Browser;

/// <summary>
/// The browser observations of the product page and the showcase card that the validation protocol (12, KP-4 to KP-6) and two backlog items ask for (T604; VAL-A010, VAL-A012, VAL-A016, VAL-A018, VAL-A020,
/// D-1811, D-1821). Every expected text is taken from the row's "Beklenen" column (the sentence of the row), not from what the page happened to draw; each product is made for its observation
/// (<see cref="BrowserSeed.NewShowcaseProductAsync"/>), so no other test's change reaches it. The answer of the address (status and body) is read from the response the browser got.
/// </summary>
public sealed class ProductPageObservationTests(BrowserHost host) : BrowserTestBase(host)
{
    private const int MediumWidth = 800;

    /// <summary>The state of the menu's "Diğer" item as the browser draws it: the entries on the row, the moved ones, the number of lines the row takes, the button and the drop-down.</summary>
    private const string MenuState = """
        () => {
          const item = document.querySelector('[data-js=menu-overflow]');
          const list = item.parentElement;
          const name = (e) => e.textContent.replace(/\s+/g, ' ').trim();
          const moved = [...item.querySelector('[data-js=menu-overflow-list]').children].map(name);
          const row = [...list.children].filter((e) => !e.hidden);
          const own = row.filter((e) => e !== item).map(name);
          const button = item.querySelector('[data-js=menu-overflow-toggle]');
          const panel = item.querySelector('.site-menu__panel');
          const style = getComputedStyle(panel);
          return JSON.stringify({
            shown: !item.hidden, own, moved, lines: new Set(row.map((e) => e.offsetTop)).size,
            expanded: button.getAttribute('aria-expanded'), panelShown: panel.getClientRects().length > 0 && style.display !== 'none' && style.visibility !== 'hidden',
            focusOnButton: document.activeElement === button });
        }
        """;

    /// <summary>The menu entries of the server's own markup (the page asked for again, read without the script's moves), in order.</summary>
    private const string ServerMenuNames = """
        async () => {
          const html = await (await fetch(location.href)).text();
          const doc = new DOMParser().parseFromString(html, 'text/html');
          const item = doc.querySelector('[data-js=menu-overflow]');
          return JSON.stringify([...item.parentElement.children].filter((e) => e !== item).map((e) => e.textContent.replace(/\s+/g, ' ').trim()));
        }
        """;

    /// <summary>Adds entries made like the first link entry of the menu, in front of the "Diğer" item.</summary>
    private const string InjectEntries = """
        (names) => {
          const item = document.querySelector('[data-js=menu-overflow]');
          const model = [...item.parentElement.children].find((e) => e.querySelector('a.site-menu__link'));
          for (const name of names) {
            const entry = model.cloneNode(true);
            const link = entry.querySelector('a');
            link.textContent = name;
            link.removeAttribute('aria-current');
            item.parentElement.insertBefore(entry, item);
          }
        }
        """;

    private static string Flat(string text) => System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(2)).Trim();

    /// <summary>A label that is one lower-case word, unique to the engine and the way (a product's name and address come from it, and the search finds it by it).</summary>
    private static string Label(string kind, string engine, ScriptPath path) => $"{kind}{engine}{(path == ScriptPath.Scripted ? "b" : "s")}";

    private static async Task<string> TextOfAsync(WatchedPage page, string selector)
    {
        ILocator locator = page.Page.Locator(selector);
        return await locator.CountAsync() > 0 ? Flat(await locator.First.InnerTextAsync()) : string.Empty;
    }

    private static async Task<bool> ShownAsync(WatchedPage page, string selector)
    {
        ILocator locator = page.Page.Locator(selector);
        return await locator.CountAsync() > 0 && await locator.First.IsVisibleAsync();
    }

    /// <summary>True when the picture of the selector has been loaded and drawn (its natural width is above zero); a lazy picture is scrolled to first.</summary>
    private static async Task<bool> PictureLoadedAsync(WatchedPage page, string selector)
    {
        ILocator picture = page.Page.Locator(selector).First;
        await picture.ScrollIntoViewIfNeededAsync();
        try
        {
            await page.Page.WaitForFunctionAsync("(s) => { const e = document.querySelector(s); return e !== null && e.complete && e.naturalWidth > 0; }", selector, new PageWaitForFunctionOptions { Timeout = 8000 });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    /// <summary>VAL-A010: a published product with several variants has one card in the category list, in the search result and on the home page.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task AProductWithSeveralVariants_HasOneCard_InTheCategoryListTheSearchAndTheHomePage_VALA010(string engine, ScriptPath path)
    {
        string label = Label("tekkart", engine, path);
        SeededProduct product = await BrowserSeed.NewShowcaseProductAsync(Host.Dsn, label, new BrowserSeed.ShowcaseSpec(Variants: 3, InCategory: true));
        await using IBrowserContext context = await Host.ContextAsync(engine, path);
        WatchedPage page = await NewPageAsync(context);
        (string Where, string Path)[] stops = [("kategori listesi", $"/kategori/{Host.Seed.CategorySlug}"), ("arama sonucu", $"/arama?search={label}"), ("ana sayfa vitrini", "/")];
        foreach ((string where, string address) in stops)
        {
            IResponse? response = await GoAsync(page, address);
            int cards = await page.Page.Locator($"a.product-card__link[href='/urun/{product.Slug}']").CountAsync();
            await ObserveAsync(
                "VAL-A010", path, engine, page, $"üç seçenekli yayında ürün, {where}", "ürün tek kartla görünür, seçenek başına kart yoktur",
                $"{address} yanıtı {response?.Status}; ürünün kartı {cards}", response?.Status == 200 && cards == 1);
        }

        Assert.Empty(Failures);
    }

    /// <summary>VAL-A012: the product page shows the product's pictures and its description.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheProductPage_ShowsThePicturesAndTheDescription_VALA012(string engine, ScriptPath path)
    {
        const string description = "Bu masa meşe ağacındandır ve iki yıl garantilidir.";
        SeededProduct product = await BrowserSeed.NewShowcaseProductAsync(Host.Dsn, Label("gorsel", engine, path), new BrowserSeed.ShowcaseSpec(ImageCount: 2, Description: $"<p>{description}</p>"));
        await using IBrowserContext context = await Host.ContextAsync(engine, path);
        WatchedPage page = await NewPageAsync(context);
        IResponse? response = await GoAsync(page, $"/urun/{product.Slug}");
        bool lead = await PictureLoadedAsync(page, "img.product__image");
        bool second = await PictureLoadedAsync(page, ".product__thumb img");
        int thumbs = await page.Page.Locator(".product__thumb img").CountAsync();
        string text = await TextOfAsync(page, ".product__description");
        await ObserveAsync(
            "VAL-A012", path, engine, page, "iki görselli ve açıklamalı yayında ürünün sayfası", "görseller ve açıklama ürün sayfasında yer alır",
            $"yanıt {response?.Status}; ana görsel yüklendi {lead}; öteki görseller {thumbs}, yüklendi {second}; açıklama \"{text}\"",
            response?.Status == 200 && lead && thumbs == 1 && second && text == description);
        Assert.Empty(Failures);
    }

    /// <summary>VAL-A016: a physical product made in Turkey shows the Ministry's logo beside the price and the panel has no field that closes it; one made elsewhere shows no logo.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task ADomesticPhysicalProduct_ShowsTheLogo_AnotherOneDoesNot_AndThePanelCannotCloseIt_VALA016(string engine, ScriptPath path)
    {
        SeededProduct domestic = await BrowserSeed.NewShowcaseProductAsync(Host.Dsn, Label("yerli", engine, path), new BrowserSeed.ShowcaseSpec(OriginCode: "TR"));
        SeededProduct foreign = await BrowserSeed.NewShowcaseProductAsync(Host.Dsn, Label("ithal", engine, path), new BrowserSeed.ShowcaseSpec(OriginCode: "DE"));
        await using IBrowserContext context = await Host.ContextAsync(engine, path);
        WatchedPage page = await NewPageAsync(context);

        IResponse? response = await GoAsync(page, $"/urun/{domestic.Slug}");
        bool logo = await ShownAsync(page, ".price-block img.origin-label__logo");
        bool logoLoaded = logo && await PictureLoadedAsync(page, ".price-block img.origin-label__logo");
        string alt = logo ? await page.Page.Locator(".price-block img.origin-label__logo").First.GetAttributeAsync("alt") ?? string.Empty : string.Empty;
        string place = await TextOfAsync(page, ".price-block .origin-label__text");
        await ObserveAsync(
            "VAL-A016", path, engine, page, "üretim yeri Türkiye olan fiziksel ürünün sayfası", "fiyatın yanında yerli üretim logosu vardır",
            $"yanıt {response?.Status}; logo görünür {logo}, yüklendi {logoLoaded}, alt metni \"{alt}\"; \"{place}\"", response?.Status == 200 && logo && logoLoaded && alt == "Yerli üretim logosu" && place == "Üretim yeri: Türkiye");

        response = await GoAsync(page, $"/urun/{foreign.Slug}");
        int logos = await page.Page.Locator("img.origin-label__logo").CountAsync();
        string foreignPlace = await TextOfAsync(page, ".price-block .origin-label__text");
        await ObserveAsync(
            "VAL-A016", path, engine, page, "üretim yeri Türkiye olmayan (Almanya) fiziksel ürünün sayfası", "logo yoktur",
            $"yanıt {response?.Status}; logo sayısı {logos}; \"{foreignPlace}\"", response?.Status == 200 && logos == 0 && foreignPlace == "Üretim yeri: Almanya");

        await using IBrowserContext adminContext = await Host.ContextAsync(engine, path, Actor.Admin);
        WatchedPage panel = await NewPageAsync(adminContext);
        response = await GoAsync(panel, $"/panel/urunler/{domestic.ProductId}");
        string controls = await panel.Page.EvaluateAsync<string>("() => JSON.stringify([...document.querySelectorAll('main form input, main form select, main form textarea')].filter((e) => e.type !== 'hidden').map((e) => (e.name + ' ' + ((e.labels && e.labels.length > 0) ? e.labels[0].textContent : '')).replace(/\\s+/g, ' ').trim()))");
        string[] names = JsonSerializer.Deserialize<string[]>(controls) ?? [];
        string[] closing = [.. names.Where(n => n.Contains("logo", StringComparison.OrdinalIgnoreCase) || n.Contains("yerli", StringComparison.OrdinalIgnoreCase))];
        bool originField = names.Any(n => n.Contains("Üretim yeri", StringComparison.OrdinalIgnoreCase) || n.Contains("OriginCountry", StringComparison.OrdinalIgnoreCase));
        await ObserveAsync(
            "VAL-A016", path, engine, panel, "yönetici, üretim yeri Türkiye olan ürünün panel formunu açar", "panelde logoyu kapatma seçeneği yoktur",
            $"yanıt {response?.Status}; formun {names.Length} denetimi, üretim yeri alanı {originField}; logo ya da yerli adlı denetim {closing.Length}", response?.Status == 200 && names.Length > 0 && originField && closing.Length == 0);
        Assert.Empty(Failures);
    }

    /// <summary>VAL-A018: a product with a running discount shows the reference price and the discount's two days on its page and on its card.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task ARunningDiscount_ShowsTheReferencePriceAndTheTwoDays_OnThePageAndTheCard_VALA018(string engine, ScriptPath path)
    {
        string label = Label("indirim", engine, path);
        SeededProduct product = await BrowserSeed.NewShowcaseProductAsync(Host.Dsn, label, new BrowserSeed.ShowcaseSpec(DiscountPercent: 10));
        string reference = DisplayFormat.Amount(100m);
        string dates = DisplayFormat.DateRange(BrowserSeed.TurkeyToday.AddDays(-1), BrowserSeed.TurkeyToday.AddDays(30));
        await using IBrowserContext context = await Host.ContextAsync(engine, path);
        WatchedPage page = await NewPageAsync(context);

        IResponse? response = await GoAsync(page, $"/urun/{product.Slug}");
        string body = response is null ? string.Empty : System.Net.WebUtility.HtmlDecode(await response.TextAsync());
        string price = await TextOfAsync(page, "[data-js=price]");
        string shownReference = await TextOfAsync(page, "[data-js=reference]");
        string mark = await TextOfAsync(page, "[data-js=mark]");
        string shownDates = await TextOfAsync(page, "[data-js=dates]");
        bool shown = await ShownAsync(page, "[data-js=reference]") && await ShownAsync(page, "[data-js=dates]");
        await ObserveAsync(
            "VAL-A018", path, engine, page, "indirimi sürmekte olan (yüzde 10, dün başladı, otuz gün sürer) yayında ürünün sayfası", "referans fiyat, indirimin başlangıç ve bitiş tarihi ürün sayfasında görünür",
            $"yanıt {response?.Status}, gövde referans fiyatı ve tarihleri taşır {body.Contains(reference, StringComparison.Ordinal) && body.Contains(dates, StringComparison.Ordinal)}; fiyat \"{price}\", referans \"{shownReference}\" ({(shown ? "görünür" : "gizli")}), işaret \"{mark}\", tarihler \"{shownDates}\"",
            response?.Status == 200 && shown && shownReference == reference && shownDates == dates && price == DisplayFormat.Amount(90m) && body.Contains(reference, StringComparison.Ordinal) && body.Contains(dates, StringComparison.Ordinal));

        response = await GoAsync(page, $"/arama?search={label}");
        string cardReference = await TextOfAsync(page, $"a.product-card__link[href='/urun/{product.Slug}'] .product-card__reference-price");
        string cardDates = await TextOfAsync(page, $"a.product-card__link[href='/urun/{product.Slug}'] .product-card__discount-dates");
        bool cardShown = await ShownAsync(page, ".product-card__reference-price") && await ShownAsync(page, ".product-card__discount-dates");
        await ObserveAsync(
            "VAL-A018", path, engine, page, "aynı ürünün vitrin kartı (arama sonucunda)", "vitrin kartında referans fiyat, indirimin başlangıç ve bitiş tarihi görünür",
            $"yanıt {response?.Status}; kartta referans \"{cardReference}\", tarihler \"{cardDates}\" ({(cardShown ? "görünür" : "gizli")})", response?.Status == 200 && cardShown && cardReference == reference && cardDates == dates);
        Assert.Empty(Failures);
    }

    /// <summary>VAL-A020: the fulfilment period of a service is written on the product page and in the order confirmation's summary (the head of the pre-information form) with the same value.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheFulfilmentPeriod_IsWrittenOnTheProductPageAndInTheConfirmation_WithTheSameValue_VALA020(string engine, ScriptPath path)
    {
        const int days = 3;
        SeededProduct service = await BrowserSeed.NewShowcaseProductAsync(Host.Dsn, Label("hizmet", engine, path), new BrowserSeed.ShowcaseSpec(Type: ProductType.Service, FulfilmentPeriod: days));
        string stored = await DataRecord.RowAsync(Host.Dsn, "SELECT fulfilment_period FROM products WHERE id = @id", ("id", service.ProductId));
        string value = DisplayFormat.Days(days);
        await using IBrowserContext context = await Host.ContextAsync(engine, path);
        WatchedPage page = await NewPageAsync(context);

        IResponse? response = await GoAsync(page, $"/urun/{service.Slug}");
        string line = await TextOfAsync(page, ".delivery__line");
        await ObserveAsync(
            "VAL-A020", path, engine, page, "ifa süresi üç gün girilmiş hizmet ürününün sayfası", "ifa süresi ürün sayfasında yazar",
            $"yanıt {response?.Status}; teslimat satırı \"{line}\"", response?.Status == 200 && line == $"Hizmet, ödeme onayından itibaren {value} içinde yerine getirilir", stored);

        await NavigateAsync(page, () => page.Page.Locator("form.buy-form button[type=submit]").First.ClickAsync());
        response = await GoAsync(page, "/odeme");
        string summary = await TextOfAsync(page, ".checkout__summary-text");
        bool sameValue = summary.Contains($"İfa süresi: {value} (ödemenin onaylandığı andan itibaren)", StringComparison.Ordinal);
        await ObserveAsync(
            "VAL-A020", path, engine, page, "hizmet sepette, ödeme adımının onay bölümü", "ifa süresi sepet onay bölümündeki Ön Bilgilendirme Formu'nun özetinde aynı değerle yazar",
            $"yanıt {response?.Status}; onay özeti \"{summary}\"", response?.Status == 200 && sameValue, stored);
        Assert.Empty(Failures);
    }

    /// <summary>D-1821: the product page's script writes the day the price applies since for the chosen variant (and hides the line for a variant with no date); without the script the server's answer to the selection form does the same.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheSinceLine_FollowsTheChosenVariant_D1821(string engine, ScriptPath path)
    {
        DateTimeOffset first = DateTimeOffset.UtcNow.AddDays(-30);
        DateTimeOffset second = DateTimeOffset.UtcNow.AddDays(-4);
        SeededProduct product = await BrowserSeed.NewShowcaseProductAsync(Host.Dsn, Label("tarih", engine, path), new BrowserSeed.ShowcaseSpec(Variants: 3, SinceDates: [first, second, null]));
        await using IBrowserContext context = await Host.ContextAsync(engine, path);
        WatchedPage page = await NewPageAsync(context);
        IResponse? response = await GoAsync(page, $"/urun/{product.Slug}");
        string line = $"Fiyatın geçerlilik başlangıcı: {DisplayFormat.Date(first)}";
        string shown = await TextOfAsync(page, "[data-js=since]");
        await ObserveAsync(
            "D-1821", path, engine, page, "üç seçenekli ürünün sayfası, ilk seçenek seçili", "ilk seçeneğin fiyat tarihi satırda yazar",
            $"yanıt {response?.Status}; satır \"{shown}\"", response?.Status == 200 && shown == line);

        string address = new Uri(page.Page.Url).PathAndQuery;
        (int place, DateTimeOffset? since)[] steps = [(2, second), (3, null)];
        foreach ((int place, DateTimeOffset? since) in steps)
        {
            await SelectAsync(page, path, place);
            shown = await TextOfAsync(page, "[data-js=since]");
            bool hidden = !await ShownAsync(page, "[data-js=since]");
            string expectedLine = since is { } date ? $"Fiyatın geçerlilik başlangıcı: {DisplayFormat.Date(date)}" : string.Empty;
            bool sameAddress = new Uri(page.Page.Url).AbsolutePath == new Uri(Host.BaseUrl + address).AbsolutePath;
            await ObserveAsync(
                "D-1821", path, engine, page, $"{place}. seçenek seçilir" + (path == ScriptPath.Scripted ? string.Empty : " ve seçim uygulanır"),
                since is null ? "tarihi olmayan seçenekte satır gizlenir" : "satır seçilen seçeneğin fiyat tarihini yazar",
                $"satır \"{shown}\" ({(hidden ? "gizli" : "görünür")}); sayfa {new Uri(page.Page.Url).PathAndQuery}", sameAddress && (since is null ? hidden : !hidden && shown == expectedLine));
        }

        Assert.Empty(Failures);
    }

    private static async Task SelectAsync(WatchedPage page, ScriptPath path, int place)
    {
        ILocator choice = page.Page.Locator("[data-js=option]").First;
        await choice.SelectOptionAsync(new SelectOptionValue { Value = place.ToString(System.Globalization.CultureInfo.InvariantCulture) });
        if (path == ScriptPath.Scriptless)
        {
            // Without the script the selection is a GET form: the server answers with the page of the chosen variant.
            await NavigateAsync(page, () => page.Page.Locator("[data-js=apply-selection]").ClickAsync());
        }
    }

    /// <summary>D-1811: on the medium class the menu's entries that do not fit the one row move, in their order, into the "Diğer" drop-down, which opens by click and Enter and closes on Escape; without the script the entries wrap and nothing is moved.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheMenuOverflow_MovesTheEntriesThatDoNotFit_IntoTheOtherDropDown_D1811(string engine, ScriptPath path)
    {
        await using IBrowserContext context = await Host.ContextAsync(engine, path, Actor.Visitor, MediumWidth);
        WatchedPage page = await NewPageAsync(context);
        await GoAsync(page, "/");
        if (path == ScriptPath.Scriptless)
        {
            string state = await page.Page.EvaluateAsync<string>(MenuState);
            using JsonDocument none = JsonDocument.Parse(state);
            int moved = none.RootElement.GetProperty("moved").GetArrayLength();
            bool shown = none.RootElement.GetProperty("shown").GetBoolean();
            await ObserveAsync("D-1811", path, engine, page, "orta sınıf genişliğinde ana sayfa, betik kapalı", "\"Diğer\" ögesi gizli kalır ve hiçbir giriş taşınmaz", $"\"Diğer\" görünür {shown}; taşınan {moved}", !shown && moved == 0);
            Assert.Empty(Failures);
            return;
        }

        await page.Page.EvaluateAsync("() => new Promise((r) => requestAnimationFrame(() => requestAnimationFrame(r)))");
        string[] original = JsonSerializer.Deserialize<string[]>(await page.Page.EvaluateAsync<string>(ServerMenuNames)) ?? [];
        string lengthened = string.Empty;
        using (JsonDocument natural = JsonDocument.Parse(await page.Page.EvaluateAsync<string>(MenuState)))
        {
            if (!natural.RootElement.GetProperty("shown").GetBoolean())
            {
                // The seeded menu's few entries fit the row at this width, so nothing would move: the page's own menu is lengthened by entries made like the existing ones, and the real script is
                // woken by the real resize event (the viewport is changed and set back). What is observed is the script's work on the browser's own layout.
                string[] extra = [.. Enumerable.Range(1, 8).Select(i => $"Ek giriş numarası {i}")];
                await page.Page.EvaluateAsync(InjectEntries, extra);
                original = [.. original, .. extra];
                lengthened = $"; menü {extra.Length} ek girişle uzatıldı (sunucunun girişleri sığıyordu; resize olayı gönderildi)";
                await page.Page.SetViewportSizeAsync(MediumWidth + 1, 900);
                await page.Page.EvaluateAsync("() => new Promise((r) => requestAnimationFrame(() => requestAnimationFrame(r)))");
                await page.Page.SetViewportSizeAsync(MediumWidth, 900);
                // WebKit may join the two changes into none; the event the script listens to is sent once more so the wake-up does not depend on the engine's own timing.
                await page.Page.EvaluateAsync("() => window.dispatchEvent(new Event('resize'))");
                await page.Page.WaitForFunctionAsync("() => !document.querySelector('[data-js=menu-overflow]').hidden", null, new PageWaitForFunctionOptions { Timeout = 8000 });
                await page.Page.EvaluateAsync("() => new Promise((r) => requestAnimationFrame(() => requestAnimationFrame(r)))");
            }
        }

        using JsonDocument settled = JsonDocument.Parse(await page.Page.EvaluateAsync<string>(MenuState));
        JsonElement root = settled.RootElement;
        string[] own = [.. root.GetProperty("own").EnumerateArray().Select(e => e.GetString() ?? string.Empty)];
        string[] moved2 = [.. root.GetProperty("moved").EnumerateArray().Select(e => e.GetString() ?? string.Empty)];
        string overflowButtonName = "Diğer";
        // The row holds the entries that stayed and the item itself; its own text ("Diğer") is not an entry of the server's markup.
        string[] stayed = [.. own.Where(n => n != overflowButtonName)];
        bool inOrder = original.SequenceEqual([.. stayed, .. moved2]);
        await ObserveAsync(
            "D-1811", path, engine, page, "orta sınıf genişliğinde ana sayfa, betik açık", "sığmayan girişler sırasıyla \"Diğer\" açılır listesine taşınır, satır tek satırdır, hiçbir giriş kaybolmaz",
            $"sunucunun girişleri {original.Length}; satırda kalan {stayed.Length}; taşınan {moved2.Length} ({string.Join(" | ", moved2)}); \"Diğer\" görünür {root.GetProperty("shown").GetBoolean()}; satır sayısı {root.GetProperty("lines").GetInt32()}; sıra korunur {inOrder}{lengthened}",
            root.GetProperty("shown").GetBoolean() && moved2.Length > 0 && root.GetProperty("lines").GetInt32() == 1 && inOrder);

        ILocator button = page.Page.Locator("[data-js=menu-overflow-toggle]");
        await button.ClickAsync();
        using JsonDocument opened = JsonDocument.Parse(await page.Page.EvaluateAsync<string>(MenuState));
        await ObserveAsync(
            "D-1811", path, engine, page, "\"Diğer\" düğmesine tıklanır", "açılır liste açılır, taşınan girişler görünür",
            $"aria-expanded {opened.RootElement.GetProperty("expanded").GetString()}; liste görünür {opened.RootElement.GetProperty("panelShown").GetBoolean()}",
            opened.RootElement.GetProperty("expanded").GetString() == "true" && opened.RootElement.GetProperty("panelShown").GetBoolean());

        await page.Page.Keyboard.PressAsync("Escape");
        using JsonDocument closed = JsonDocument.Parse(await page.Page.EvaluateAsync<string>(MenuState));
        await ObserveAsync(
            "D-1811", path, engine, page, "Escape'e basılır", "açılır liste kapanır ve odak \"Diğer\" düğmesine döner",
            $"aria-expanded {closed.RootElement.GetProperty("expanded").GetString()}; liste görünür {closed.RootElement.GetProperty("panelShown").GetBoolean()}; odak düğmede {closed.RootElement.GetProperty("focusOnButton").GetBoolean()}",
            closed.RootElement.GetProperty("expanded").GetString() == "false" && !closed.RootElement.GetProperty("panelShown").GetBoolean() && closed.RootElement.GetProperty("focusOnButton").GetBoolean());

        await page.Page.Keyboard.PressAsync("Enter");
        using JsonDocument again = JsonDocument.Parse(await page.Page.EvaluateAsync<string>(MenuState));
        await ObserveAsync(
            "D-1811", path, engine, page, "odak düğmedeyken Enter'a basılır", "açılır liste klavyeyle de açılır",
            $"aria-expanded {again.RootElement.GetProperty("expanded").GetString()}; liste görünür {again.RootElement.GetProperty("panelShown").GetBoolean()}",
            again.RootElement.GetProperty("expanded").GetString() == "true" && again.RootElement.GetProperty("panelShown").GetBoolean());
        Assert.Empty(Failures);
    }
}
