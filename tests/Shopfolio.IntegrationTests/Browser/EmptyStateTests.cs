using System.Text.RegularExpressions;

using Microsoft.Playwright;

namespace Shopfolio.IntegrationTests.Browser;

/// <summary>One screen the document gives an empty state to: the screen, the place of the document that says so and the words of that place.</summary>
internal sealed record DerivedEmptyScreen(string Screen, string Source, string Quote);

/// <summary>
/// The list of the screens that have an empty state, read from <c>Docs/04_UI_SPECS.md</c> (T551 criterion 2, KK2 (c); 12 12.2 (1)): the tests keep no second list of their own. Four places of the document say it:
/// the "boş hâl satırı" row of 3.3.12 (the storefront) and of 3.4.26 (the panel); the "Boş / yükleniyor / hata durumları" item of every screen (the item that says "ekranın boş hâli yoktur" does not count);
/// and the "Boş hâl satırı" rows of the state tables 6.2.x (the storefront) and 6.3.x (the panel).
/// </summary>
internal static class EmptyStateInventory
{
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(2);

    /// <summary>Reads the document and returns one entry for each screen and place, in the order of the screen ids.</summary>
    public static List<DerivedEmptyScreen> Derive(string document)
    {
        string[] lines = [.. document.Split('\n').Select(l => l.TrimEnd('\r'))];
        // The section of a screen: "5.2" is E-02 (the storefront's screens), "9.6" is E-34 (the panel's screens); 6.2.N and 6.3.N follow the same numbers (6.2.2 is 5.2's table, 6.3.6 is 9.6's).
        Dictionary<string, string> screenOfSection = [];
        foreach (string line in lines)
        {
            Match heading = Regex.Match(line, @"^### ((?:5|9)\.\d+) (E-\d{2}) — ", RegexOptions.None, Limit);
            if (heading.Success)
            {
                screenOfSection[heading.Groups[1].Value] = heading.Groups[2].Value;
            }
        }

        List<DerivedEmptyScreen> found = [];
        string? screen = null;
        bool inItem = false;
        foreach (string line in lines)
        {
            Match heading = Regex.Match(line, @"^### ((?:5|9)\.\d+) (E-\d{2}) — ", RegexOptions.None, Limit);
            if (heading.Success)
            {
                screen = heading.Groups[2].Value;
                inItem = false;
                continue;
            }

            if (line.StartsWith('#'))
            {
                screen = null;
                inItem = false;
                continue;
            }

            if (screen is null)
            {
                continue;
            }

            // The item is its own line ("- **5.2.9 Boş / yükleniyor / hata durumları.** ...") or a header line followed by the indented sub-items.
            if (line.Contains("Boş / yükleniyor / hata durumları", StringComparison.Ordinal))
            {
                inItem = true;
            }
            else if (!line.StartsWith("  ", StringComparison.Ordinal))
            {
                inItem = false;
            }

            if (inItem && SaysThereIsAnEmptyState(line))
            {
                Match number = Regex.Match(line, @"\*\*(\d+\.\d+\.\d+)", RegexOptions.None, Limit);
                found.Add(new DerivedEmptyScreen(screen, number.Success ? number.Groups[1].Value : "madde", Shorten(line)));
            }
        }

        foreach (string line in lines)
        {
            Match storefront = Regex.Match(line, @"^\| 3\.3\.12 \|.*boş hâl satırı — ((?:E-\d{2}(?:, )?)+)", RegexOptions.None, Limit);
            if (storefront.Success)
            {
                found.AddRange(Regex.Matches(storefront.Groups[1].Value, @"E-\d{2}", RegexOptions.None, Limit).Select(m => new DerivedEmptyScreen(m.Value, "3.3.12", Shorten(line))));
            }

            Match panel = Regex.Match(line, @"^\| 3\.4\.26 \| ((?:E-\d{2}(?: · )?)+) — boş hâl satırı", RegexOptions.None, Limit);
            if (panel.Success)
            {
                found.AddRange(Regex.Matches(panel.Groups[1].Value, @"E-\d{2}", RegexOptions.None, Limit).Select(m => new DerivedEmptyScreen(m.Value, "3.4.26", Shorten(line))));
            }

            Match row = Regex.Match(line, @"^\| (6\.([23])\.(\d+)\.\d+) \|", RegexOptions.None, Limit);
            if (row.Success && line.Contains("boş hâl satırı", StringComparison.OrdinalIgnoreCase))
            {
                string section = (row.Groups[2].Value == "2" ? "5." : "9.") + row.Groups[3].Value;
                found.Add(new DerivedEmptyScreen(screenOfSection[section], row.Groups[1].Value, Shorten(line)));
            }
        }

        return [.. found.OrderBy(f => f.Screen, StringComparer.Ordinal).ThenBy(f => f.Source, StringComparer.Ordinal)];
    }

    private static bool SaysThereIsAnEmptyState(string line)
    {
        // "Ekranın boş hâli yoktur" and "Ekrana özgü boş hâl yoktur" say the screen has none.
        string rest = Regex.Replace(line, @"boş hâl(?:i)? yoktur", string.Empty, RegexOptions.IgnoreCase, Limit);
        return rest.Contains("boş hâl", StringComparison.OrdinalIgnoreCase);
    }

    private static string Shorten(string line) => line.Length <= 140 ? line : line[..140] + "...";
}

/// <summary>Which application a stop opens: the seeded one, or the installation with no record at all.</summary>
internal enum EmptyOn
{
    Seeded,
    NoRecord,
}

/// <summary>
/// One address at which a screen shows its empty state (the address, who opens it, the application it is opened on) and what is measured there: the sentence and the way out.
/// <paramref name="Says"/> is a pattern the sentence must match (what is not there); <paramref name="ExitHref"/> is the address the way out leads to (null: the document names no way out).
/// </summary>
internal sealed record EmptyStop(
    string Screen,
    string State,
    EmptyOn On,
    Actor Actor,
    string Address,
    string Says,
    string? ExitHref,
    string? ExitLabel,
    bool IsFilterState = false,
    int Count = 1,
    string Sentences = ".empty-state__sentence, .empty-state__message",
    string Exits = ".empty-state a[href]",
    string Rows = "tbody tr:not(:has(td[colspan])), .record-list li, .content-row, .product-card, .order-history tbody tr")
{
    public string Key => Screen + " " + State;
}

/// <summary>
/// The empty state of every screen the document gives one (T551 criterion 2, KK2 (c); 04 2.7.1.2): the list of screens is read from <c>Docs/04_UI_SPECS.md</c> (<see cref="EmptyStateInventory"/>), every one of them has
/// a stop here and the stop measures the sentence that says what is not there and the way out, which is followed. The walk of the inventory opens the screens with the seeded data, so the empty state is opened
/// on an installation with no record (<see cref="BrowserHost.EmptyAsync"/>: its own database, one administrator), on a category no published product is in and as a member with no address and no order
/// (<see cref="BrowserSeedData.BareCustomerKey"/>), so no empty state waits for what another test has saved (D-1717).
/// </summary>
public sealed class EmptyStateTests(BrowserHost host) : BrowserTestBase(host)
{
    private static string Flat(string text) => Regex.Replace(text, @"\s+", " ", RegexOptions.None, TimeSpan.FromSeconds(2)).Trim();

    /// <summary>The stops: the empty state of each screen of <see cref="EmptyStateInventory"/> and, where the document says so, its filter's empty state (04 6.2.x, 6.3.x).</summary>
    internal static IReadOnlyList<EmptyStop> Stops(BrowserSeedData seed) =>
    [
        new("E-02", "boş kategori", EmptyOn.Seeded, Actor.Visitor, $"/kategori/{seed.EmptyCategorySlug}", "ürün yok", "/", "Ana sayfaya dön"),
        new("E-02", "süzgecin sonucu boş", EmptyOn.Seeded, Actor.Visitor, $"/kategori/{seed.CategorySlug}?minPrice=99999", "süzgece uyan ürün yok", $"/kategori/{seed.CategorySlug}", "Süzgeci kaldır", IsFilterState: true),
        new("E-03", "sonuçsuz arama", EmptyOn.NoRecord, Actor.Visitor, "/arama?search=zzzzzz", "bulunamadı", "/", "Ana sayfaya dön"),
        new("E-03", "süzgecin sonucu boş", EmptyOn.Seeded, Actor.Visitor, "/arama?search=ürün&minPrice=99999", "süzgece uyan ürün yok", "/arama", "Süzgeci kaldır", IsFilterState: true),
        new("E-07", "hizmetler, yayında kayıt yok", EmptyOn.NoRecord, Actor.Visitor, "/hizmetler", "yayında kayıt yok", "/", "Ana sayfaya dön"),
        new("E-07", "referanslar, yayında kayıt yok", EmptyOn.NoRecord, Actor.Visitor, "/referanslar", "yayında kayıt yok", "/", "Ana sayfaya dön"),
        new("E-09", "yayında soru yok", EmptyOn.NoRecord, Actor.Visitor, "/sss", "yayında soru yok", "/", "Ana sayfaya dön"),
        new("E-12", "boş sepet", EmptyOn.NoRecord, Actor.Visitor, "/sepet", "boş", "/", "Ürünlere dön"),
        new("E-26", "adresi olmayan defter", EmptyOn.Seeded, Actor.BareCustomer, "/hesap/adresler", "adres", "/hesap/adresler", "Adres ekle"),
        new("E-27", "siparişi olmayan üye", EmptyOn.Seeded, Actor.BareCustomer, "/hesap/siparisler", "sipariş", "/", "Ürünlere dön"),
        new("E-32", "ürün yok", EmptyOn.NoRecord, Actor.Admin, "/panel/urunler", "ürün yok", "/panel/urunler/yeni", "Yeni ürün"),
        new("E-32", "aramanın sonucu boş", EmptyOn.NoRecord, Actor.Admin, "/panel/urunler?search=zzzzzz", "süzgece uyan ürün yok", "/panel/urunler", "Aramayı ve süzgeci kaldır", IsFilterState: true),
        new("E-34", "kategori yok", EmptyOn.NoRecord, Actor.Admin, "/panel/kategoriler", "kategori yok", "/panel/kategoriler", "Yeni kategori"),
        new("E-35", "kupon yok", EmptyOn.NoRecord, Actor.Admin, "/panel/kuponlar", "kupon yok", "/panel/kuponlar/yeni", "Yeni kupon"),
        new("E-36", "sipariş yok", EmptyOn.NoRecord, Actor.Admin, "/panel/siparisler", "sipariş yok", "/panel", "Ana sayfaya dön"),
        new("E-36", "aramanın sonucu boş", EmptyOn.NoRecord, Actor.Admin, "/panel/siparisler?search=zzzzzz", "süzgece uyan sipariş yok", "/panel/siparisler", "Aramayı ve süzgeci kaldır", IsFilterState: true),
        new("E-38", "talep yok", EmptyOn.NoRecord, Actor.Admin, "/panel/talepler", "talep yok", "/panel", "Ana sayfaya dön"),
        new("E-38", "süzgecin sonucu boş", EmptyOn.NoRecord, Actor.Admin, "/panel/talepler?requestStatus=Closed", "süzgece uyan talep yok", "/panel/talepler", "Süzgeci kaldır", IsFilterState: true),
        new("E-41", "yedi grubun hepsi boş", EmptyOn.NoRecord, Actor.Admin, "/panel/kurumsal-icerik", "henüz", "/panel/kurumsal-icerik/", null, Count: 7, Sentences: ".content-group__empty", Exits: "section.content-group a.button[href$='/yeni']", Rows: ".content-row"),
        new("E-51", "ilk kurulum, dağılım tabloları", EmptyOn.NoRecord, Actor.Admin, "/panel/satis-ozeti", "yok", null, null, Count: 2, Sentences: "#summary-top-sellers td[colspan], #summary-payment-methods td[colspan]", Exits: ".empty-state a[href]", Rows: "#summary-top-sellers tbody tr:not(:has(td[colspan])), #summary-payment-methods tbody tr:not(:has(td[colspan]))"),
        new("E-52", "işlem izi kaydı yok", EmptyOn.NoRecord, Actor.Admin, "/panel/islem-izi", "kaydı yok", "/panel", "Ana sayfaya dön"),
        new("E-52", "süzgecin sonucu boş", EmptyOn.NoRecord, Actor.Admin, "/panel/islem-izi?adminId={admin}", "süzgece uyan kayıt yok", "/panel/islem-izi", "Süzgeci kaldır", IsFilterState: true),
    ];

    public static TheoryData<string, ScriptPath, string> Cases(BrowserSeedData seed)
    {
        TheoryData<string, ScriptPath, string> data = [];
        foreach (string engine in BrowserHost.Engines)
        {
            foreach (ScriptPath path in Enum.GetValues<ScriptPath>())
            {
                foreach (EmptyStop stop in Stops(seed))
                {
                    data.Add(engine, path, stop.Key);
                }
            }
        }

        return data;
    }

    /// <summary>The cases are made before the seed exists, so they carry the stops' keys only; the stop itself is looked up by its key when the case runs.</summary>
    public static TheoryData<string, ScriptPath, string> AllCases() => Cases(new BrowserSeedData
    {
        EmptyCategorySlug = "-",
        CategorySlug = "-",
    });

    [Fact]
    public async Task EveryScreenWithAnEmptyStateInTheDocument_HasAStop_AndEveryStopIsOfSuchAScreen_KK2()
    {
        List<DerivedEmptyScreen> derived = EmptyStateInventory.Derive(ScreenInventoryTests.ReadScreenDocument());
        HashSet<string> screens = [.. derived.Select(d => d.Screen)];
        IReadOnlyList<EmptyStop> stops = Stops(Host.Seed);
        await WriteDerivationAsync(derived, stops);

        // The document gives many screens an empty state (the guard is a floor against a reading that finds none, not the list).
        Assert.True(screens.Count >= 10, $"The empty states read from 04 are on {screens.Count} screens; at least 10 are expected.");
        string[] withoutStop = [.. screens.Where(id => !stops.Any(s => s.Screen == id && !s.IsFilterState)).Order(StringComparer.Ordinal)];
        Assert.True(withoutStop.Length == 0, $"04 gives these screens an empty state and no stop observes it: {string.Join(", ", withoutStop)}.");
        string[] notInTheDocument = [.. stops.Select(s => s.Screen).Distinct().Where(id => !screens.Contains(id)).Order(StringComparer.Ordinal)];
        Assert.True(notInTheDocument.Length == 0, $"These screens have a stop and 04 gives them no empty state: {string.Join(", ", notInTheDocument)}.");
        Assert.Equal(stops.Count, stops.Select(s => s.Key).Distinct().Count());
    }

    /// <summary>The derivation as a table for the gate report: the screen, the place of 04 that gives it an empty state and the stops that observe it.</summary>
    private async Task WriteDerivationAsync(List<DerivedEmptyScreen> derived, IReadOnlyList<EmptyStop> stops)
    {
        List<string> table = ["| Ekran | 04 maddesi | Durak |", "|---|---|---|"];
        foreach (IGrouping<string, DerivedEmptyScreen> screen in derived.GroupBy(d => d.Screen))
        {
            string places = string.Join(", ", screen.Select(d => d.Source).Distinct());
            string observed = string.Join("; ", stops.Where(s => s.Screen == screen.Key).Select(s => $"{s.Address.Replace("{admin}", "<yönetici>", StringComparison.Ordinal)} ({s.State})"));
            table.Add($"| {screen.Key} | {places} | {observed} |");
        }

        Directory.CreateDirectory(Host.Record.Directory);
        await File.WriteAllLinesAsync(System.IO.Path.Combine(Host.Record.Directory, "empty-state-derivation.md"), table, Ct);
    }

    /// <summary>
    /// What the accessibility pass does with an empty state while it is open (T552, 12 12.2 (1), VAL-A776, VAL-A790): the scan of <see cref="AccessibilityStatesTests"/> runs through the very walk of this class,
    /// so the list of empty states is <see cref="Stops"/> and no second list can drift from it. Null in this class's own tests.
    /// </summary>
    internal Func<EmptyStop, ScriptPath, string, WatchedPage, Task>? AtEmptyState { get; set; }

    [Theory]
    [MemberData(nameof(AllCases))]
    public async Task TheScreenWithNothingToShow_SaysWhatIsNotThere_AndShowsAWayOut_KK2(string engine, ScriptPath path, string key) => Assert.Empty(await RunStopAsync(engine, path, key));

    /// <summary>Opens one stop in the engine and the way, measures it and follows its way out; the accessibility pass runs this with <see cref="AtEmptyState"/> set.</summary>
    internal async Task<IReadOnlyList<string>> RunStopAsync(string engine, ScriptPath path, string key)
    {
        EmptyStop stop = Stops(Host.Seed).Single(s => s.Key == key);
        BrowserHost on = stop.On == EmptyOn.NoRecord ? await Host.EmptyAsync() : Host;
        string address = stop.Address.Replace("{admin}", on.Seed.AdminId.ToString("D"), StringComparison.Ordinal);
        // The scan cannot run in a context with the script switched off: when the pass walks the states, the scriptless way is a context whose page script is cut at the network (AccessibilityScanTests.AuditContextAsync).
        await using IBrowserContext context = AtEmptyState is null ? await on.ContextAsync(engine, path, stop.Actor) : await AccessibilityScanTests.AuditContextAsync(on, engine, path, stop.Actor);
        WatchedPage page = await NewPageAsync(context);
        IResponse? response = await page.Page.GotoAsync(on.BaseUrl + address, new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 30000 });
        ILocator main = page.Page.Locator("main");
        ILocator sentences = main.Locator(stop.Sentences);
        int count = await sentences.CountAsync();
        List<string> said = [];
        for (int i = 0; i < count; i++)
        {
            said.Add(Flat(await sentences.Nth(i).InnerTextAsync()));
        }

        bool sentencesOk = count == stop.Count && said.All(s => s.Length > 0 && Regex.IsMatch(s, stop.Says, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2)));
        ILocator exits = main.Locator(stop.Exits);
        int exitCount = stop.ExitHref is null ? 0 : await exits.CountAsync();
        List<string> exitLines = [];
        bool exitsOk = true;
        if (stop.ExitHref is not null)
        {
            exitsOk = exitCount == stop.Count;
            for (int i = 0; i < exitCount; i++)
            {
                string href = await exits.Nth(i).GetAttributeAsync("href") ?? string.Empty;
                string label = Flat(await exits.Nth(i).InnerTextAsync());
                exitLines.Add($"\"{label}\" -> {href}");
                exitsOk &= label.Length > 0 && (stop.ExitLabel is null || label == stop.ExitLabel) && Leads(href, stop.ExitHref, stop.Count > 1);
            }
        }

        int rows = await main.Locator(stop.Rows).CountAsync();
        int errors = await main.Locator(".message--error, [data-js=error-summary]").CountAsync();
        bool ok = response?.Status == 200 && sentencesOk && exitsOk && rows == 0 && errors == 0 && OwnErrors(page, path).Count == 0;
        await ObserveAsync(
            stop.Screen,
            path,
            engine,
            page,
            $"{stop.State}: {address.Split('?')[0]} açılır",
            $"200 döner; boş hâl cümlesi (\"{stop.Says}\") {stop.Count} kez; çıkış {(stop.ExitHref is null ? "04'te tanımlı değil" : $"{stop.Count} bağlantı, {stop.ExitHref}")}; kayıt satırı yok; hata gösterilmez",
            $"durum {response?.Status}; cümle {count}: \"{string.Join("\" / \"", said.Take(3))}\"; çıkış {exitCount}: {string.Join(" / ", exitLines.Take(3))}; kayıt satırı {rows}; hata {errors}; sayfa hatası {OwnErrors(page, path).Count}",
            ok);

        if (AtEmptyState is not null)
        {
            await AtEmptyState(stop, path, engine, page);
        }

        if (stop.ExitHref is not null && exitCount > 0)
        {
            await FollowFirstExitAsync(stop, path, engine, page, exits.First, on);
        }

        return Failures;
    }

    /// <summary>
    /// The browser's own error messages of the page. When the accessibility pass walks the states the scriptless way cuts the page's script at the network (<see cref="AccessibilityScanTests.AuditContextAsync"/>),
    /// and the browser reports each cut script as a failed request with no status; those lines are the cut itself, not a fault of the page (a status such as 404 is still a fault).
    /// The exemption is that and nothing wider (D-1740 (a)): only on the scriptless way, only with the scan hook set, and no more lines than the script requests that failed at the network.
    /// </summary>
    private List<string> OwnErrors(WatchedPage page, ScriptPath path) =>
        OwnErrorsOf(page.Errors, page.ErrorUrls, page.CutScriptUrls, path, AtEmptyState is not null);

    /// <summary>The page's error lines without the lines of the cut script requests: at most <paramref name="cutScripts"/> lines of a failed load with no status, and only when the scan walks the states on the scriptless way.</summary>
    internal static List<string> OwnErrorsOf(IReadOnlyList<string> errors, IReadOnlyList<string> errorUrls, IReadOnlyList<string> cutScriptUrls, ScriptPath path, bool scanIsWalking)
    {
        if (!scanIsWalking || path != ScriptPath.Scriptless)
        {
            return [.. errors];
        }

        // By the address (D-1746 c): a line is the cut of a script when the console says it is about the address of a script request that failed at the network (each request explains one line). Only a line the engine gives no
        // address for falls back to the count, so another resource's status-less failure does not hide behind a cut script, and a cut script that gave no line does not free a line of another resource.
        List<string> pending = [.. cutScriptUrls];
        List<string> own = [];
        for (int i = 0; i < errors.Count; i++)
        {
            string error = errors[i];
            string url = i < errorUrls.Count ? errorUrls[i] : string.Empty;
            bool cut = false;
            if (pending.Count > 0 && error.StartsWith("console: Failed to load resource", StringComparison.Ordinal) && !error.Contains("status of", StringComparison.Ordinal))
            {
                if (url.Length > 0)
                {
                    cut = pending.Remove(url);
                }
                else
                {
                    pending.RemoveAt(0);
                    cut = true;
                }
            }

            if (!cut)
            {
                own.Add(error);
            }
        }

        return own;
    }

    [Fact]
    public void TheCutScriptExemption_IsOnlyOnTheScriptlessWayOfTheScan_AndOnlyAsManyLinesAsScriptsWereCut_KK2()
    {
        string[] twoCuts = ["console: Failed to load resource: net::ERR_FAILED", "console: Failed to load resource: net::ERR_FAILED"];

        // No address: the engine gave none for the lines, so the count decides (the earlier rule).
        string[] none = ["", ""];
        string[] cutUrls = ["https://h/js/a.js", "https://h/js/b.js"];
        // The exemption: both lines are the cut of two scripts.
        Assert.Empty(OwnErrorsOf(twoCuts, none, cutUrls, ScriptPath.Scriptless, true));
        // Not on the scripted way (a failed script there is a fault), not without the scan walking the states.
        Assert.Equal(2, OwnErrorsOf(twoCuts, none, cutUrls, ScriptPath.Scripted, true).Count);
        Assert.Equal(2, OwnErrorsOf(twoCuts, none, cutUrls, ScriptPath.Scriptless, false).Count);
        // Not for another resource: a third line has no cut script behind it, a status line is a fault, a page error is a fault.
        Assert.Single(OwnErrorsOf([.. twoCuts, "console: Failed to load resource: net::ERR_FAILED"], [.. none, ""], cutUrls, ScriptPath.Scriptless, true));
        Assert.Single(OwnErrorsOf(["console: Failed to load resource: the server responded with a status of 404 ()"], [""], cutUrls[..1], ScriptPath.Scriptless, true));
        Assert.Single(OwnErrorsOf(["page: boom"], [""], cutUrls[..1], ScriptPath.Scriptless, true));
        Assert.Single(OwnErrorsOf(twoCuts, none, cutUrls[..1], ScriptPath.Scriptless, true));
    }

    /// <summary>The exemption goes by the address when the engine gives one (D-1746 c): a status-less failure of another resource stays a fault even while a cut script's line is pending, and a cut script's own line goes.</summary>
    [Fact]
    public void TheCutScriptExemption_GoesByTheAddress_WhenTheEngineGivesOne_KK2()
    {
        string[] lines = ["console: Failed to load resource: net::ERR_FAILED", "console: Failed to load resource: net::ERR_FAILED"];
        string[] cut = ["https://h/js/a.js"];

        // The first line is about the cut script, the second about a stylesheet: only the first goes, though one script was cut and two lines have no status.
        List<string> own = OwnErrorsOf(lines, ["https://h/js/a.js", "https://h/css/site.css"], cut, ScriptPath.Scriptless, true);
        Assert.Single(own);
        // The same two lines in the other order: still the stylesheet's line stays (the order does not decide).
        Assert.Single(OwnErrorsOf(lines, ["https://h/css/site.css", "https://h/js/a.js"], cut, ScriptPath.Scriptless, true));
        // A line about an address no request cut is a fault even when the count would have allowed it.
        Assert.Equal(2, OwnErrorsOf(lines, ["https://h/js/other.js", "https://h/css/site.css"], cut, ScriptPath.Scriptless, true).Count);
    }

    [Theory]
    [InlineData("https://h/js/a.js:12:5", "https://h/js/a.js")]
    [InlineData("https://h/js/a.js:0:0", "https://h/js/a.js")]
    [InlineData("https://h/js/a.js", "https://h/js/a.js")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void UrlOfLocation_DropsTheLineAndTheColumn_KK2(string? location, string expected)
    {
        Assert.Equal(expected, WatchedPage.UrlOfLocation(location));
    }

    /// <summary>The way out leads to the address the document names; the first of a group leads to a record form under it.</summary>
    private static bool Leads(string href, string expected, bool underIt) =>
        href == expected
        || href.StartsWith(expected + "?", StringComparison.Ordinal)
        || href.StartsWith(expected + "#", StringComparison.Ordinal)
        || (underIt && href.StartsWith(expected, StringComparison.Ordinal));

    /// <summary>Follows the way out as the user does (a click) and observes that it lands on a page that answers and shows no error; the observation has no picture of its own, the empty state's line has it.</summary>
    private async Task FollowFirstExitAsync(EmptyStop stop, ScriptPath path, string engine, WatchedPage page, ILocator exit, BrowserHost on)
    {
        string href = await exit.GetAttributeAsync("href") ?? string.Empty;
        string target = new Uri(new Uri(on.BaseUrl), href).GetLeftPart(UriPartial.Path);
        int before = OwnErrors(page, path).Count;
        int missingImages = 0;
        IResponse? response = null;
        int hash = href.IndexOf('#', StringComparison.Ordinal);
        bool ok;
        string observed;
        if (hash >= 0)
        {
            // A way out that leads to a place on the same page (the button that opens the first record's form): the place must be on the page.
            string id = href[(hash + 1)..];
            int places = await page.Page.Locator($"[id='{id}']").CountAsync();
            ok = places == 1;
            observed = $"{href} sayfada {places} yer";
        }
        else
        {
            page.Page.Response += (_, r) =>
            {
                response = r.Url.StartsWith(target, StringComparison.Ordinal) && r.Request.IsNavigationRequest ? r : response;
                missingImages += r.Status == 404 && new Uri(r.Url).AbsolutePath.StartsWith("/gorsel/", StringComparison.Ordinal) ? 1 : 0;
            };
            await NavigateAsync(page, () => exit.ClickAsync());
            string landed = new Uri(page.Page.Url).PathAndQuery;
            string title = await page.Page.TitleAsync();
            // The image address is served (D-1711): every browser message counts, an image's 404 too.
            int unknown = OwnErrors(page, path).Count - before;
            ok = (response?.Status ?? 0) is >= 200 and < 400 && page.Page.Url.StartsWith(target, StringComparison.Ordinal) && title.Length > 0 && unknown == 0 && missingImages == 0;
            observed = $"tıklanınca {landed}; durum {response?.Status}; başlık \"{title}\"; tarayıcı iletisi {unknown}";
        }

        Host.Record.Add(new Observation(stop.Screen, path == ScriptPath.Scripted ? "betikli" : "betiksiz", engine, page.Page.ViewportSize?.Width ?? 0, $"{stop.State}: çıkış bağlantısı izlenir", $"çıkış {target.Replace(on.BaseUrl, string.Empty, StringComparison.Ordinal)} adresine götürür; sayfa yanıt verir ve hata göstermez", DataRecord.Mask(observed), ok, null));
        if (!ok)
        {
            Failures.Add($"{stop.Screen} {engine} {path} - {stop.State} çıkış: {DataRecord.Mask(observed)}");
        }
    }
}
