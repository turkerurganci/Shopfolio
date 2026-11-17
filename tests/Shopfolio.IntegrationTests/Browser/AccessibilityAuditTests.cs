using System.Text.Json;

using Microsoft.Playwright;

namespace Shopfolio.IntegrationTests.Browser;

/// <summary>
/// The manual audit record of the level A accessibility pass (T552 criterion 2, KK2; 12 12.2 (3), VAL-A791): for every screen of the inventory walk, in each engine and each way, five headings each carry a result - the keyboard,
/// the focus order and its visibility, the 200 percent zoom, the accessible names, the error message. The procedures are the ones a person does by hand, done by the tool so that the record is the same on every run: the Tab key is
/// pressed until every control has been reached (or the focus has been caught), the focused control's own style is read at each stop, the page is drawn at half the desktop width (the layout of a 200 percent zoom), the name of every
/// control is computed from the page, and each form of the screen is sent with a value that is not allowed and the page that answers is read. A heading with nothing to check on the screen (no form, so no error message) says so with its reason.
/// </summary>
public sealed class AccessibilityAuditTests(BrowserHost host) : BrowserTestBase(host)
{
    internal const string Keyboard = "klavye";
    internal const string FocusOrder = "odak sırası ve görünürlüğü";
    internal const string Zoom = "%200 yakınlaştırma";
    internal const string Name = "erişilebilir ad";
    internal const string Error = "hata mesajı";

    /// <summary>The five headings of the record in the order they are written (VAL-A791).</summary>
    internal static readonly string[] Headings = [Keyboard, FocusOrder, Zoom, Name, Error];

    /// <summary>The width of a 200 percent zoom of the desktop width: the page is laid out as the browser lays it out at that zoom.</summary>
    internal const int ZoomedWidth = BrowserHost.DesktopWidth / 2;

    private static string Step(string heading, ScreenInventoryTests.Stop stop) => $"elle denetim, {heading}: {stop.State}";

    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    private static List<string> Strings(JsonElement element, string name) => [.. element.GetProperty(name).EnumerateArray().Select(e => e.GetString() ?? string.Empty)];

    /// <summary>The error message heading's reading of the page that answered a form sent with a value that is not allowed (04 2.3, 2.12.1.1; WCAG 3.3.1).</summary>
    internal const string ErrorState = """
        () => {
          const summary = document.querySelector('[data-js="error-summary"]');
          const invalid = [...document.querySelectorAll('[aria-invalid="true"]')];
          const messages = [...document.querySelectorAll('main .message--error')];
          const problems = [];
          if (summary) {
            if (!summary.getAttribute('role')) { problems.push('the summary has no role'); }
            const links = [...summary.querySelectorAll('a[href^="#"]')];
            if (links.length === 0) { problems.push('the summary links to no field'); }
            for (const a of links) {
              if (!document.getElementById(a.getAttribute('href').slice(1))) { problems.push('a summary link names no field: ' + a.getAttribute('href')); }
              if (!(a.textContent || '').trim()) { problems.push('a summary link has no text'); }
            }
          }
          for (const f of invalid) {
            const ids = (f.getAttribute('aria-describedby') || '').split(/\s+/).filter(Boolean);
            const texts = ids.map((id) => document.getElementById(id)).filter(Boolean).map((n) => (n.textContent || '').replace(/\s+/g, ' ').trim()).filter((t) => t);
            const d = (f.name || f.id);
            if (texts.length === 0) { problems.push('an invalid field has no message bound to it: ' + d); }
            else if (!texts.some((t) => /\p{L}{3,}/u.test(t))) { problems.push('the message of ' + d + ' has no words'); }
            if (!(f.labels && f.labels.length) && !f.getAttribute('aria-label') && !f.getAttribute('aria-labelledby')) { problems.push('an invalid field has no name: ' + d); }
          }
          for (const m of messages) {
            if (!(m.textContent || '').replace(/\s+/g, ' ').trim()) { problems.push('an empty error message'); }
            if (!m.querySelector('.message__symbol')) { problems.push('an error message has no symbol beside its colour'); }
          }
          const first = invalid[0]; const active = document.activeElement;
          if (summary && first && active !== first && !(summary === active || summary.contains(active))) { problems.push('the focus is not on the first invalid field or the summary: ' + (active ? active.tagName : 'none')); }
          return JSON.stringify({ problems, summary: !!summary, invalid: invalid.length, messages: messages.length });
        }
        """;

    internal static async Task<(bool Ok, string Text)> InspectErrorAsync(WatchedPage page)
    {
        using JsonDocument document = JsonDocument.Parse(await page.Page.EvaluateAsync<string>(ErrorState));
        List<string> problems = Strings(document.RootElement, "problems");
        string text = $"hata özeti {document.RootElement.GetProperty("summary").GetBoolean()}, geçersiz alan {document.RootElement.GetProperty("invalid").GetInt32()}, hata iletisi {document.RootElement.GetProperty("messages").GetInt32()}" + (problems.Count > 0 ? "; sorun: " + string.Join(" / ", problems.Take(4)) : string.Empty);
        return (problems.Count == 0, text);
    }

    [Theory]
    [MemberData(nameof(Ways))]
    public async Task EveryScreenOfTheInventory_HasAResultUnderEachOfTheFiveHeadings_InTheWay_KK2(string engine, ScriptPath path)
    {
        IReadOnlyList<ScreenInventoryTests.Stop> stops = new ScreenInventoryTests(Host).Stops();
        Dictionary<string, HashSet<string>> formKeys = [];
        Dictionary<string, string> titles = [];
        foreach (IGrouping<Actor, ScreenInventoryTests.Stop> group in stops.GroupBy(s => s.Actor))
        {
            await using IBrowserContext context = await AccessibilityScanTests.AuditContextAsync(Host, engine, path, group.Key);
            await using IBrowserContext narrow = await AccessibilityScanTests.AuditContextAsync(Host, engine, path, group.Key, ZoomedWidth);
            foreach (ScreenInventoryTests.Stop stop in group)
            {
                WatchedPage page = await NewPageAsync(context);
                await GoAsync(page, stop.Path);
                string where = new Uri(page.Page.Url).AbsolutePath;
                if (where == new Uri(Host.BaseUrl + stop.Path).AbsolutePath)
                {
                    if (!formKeys.TryGetValue(stop.Screen, out HashSet<string>? keys))
                    {
                        formKeys[stop.Screen] = keys = [];
                    }

                    keys.UnionWith(await ValidationStateTests.FormKeysAsync(page.Page, stop.Screen));
                }

                // The keyboard and the focus order: the Tab key until the controls the page offers have all been reached.
                using (JsonDocument install = JsonDocument.Parse(await page.Page.EvaluateAsync<string>(AuditScripts.Install)))
                {
                    int expected = install.RootElement.GetProperty("expected").GetInt32();
                    int positive = install.RootElement.GetProperty("positiveTabindex").GetInt32();
                    // The Tab key goes on until the focus has come round to the first control again (the browser also focuses what the tool does not list, such as a scrolling region); the cap is a trap's end.
                    for (int i = 0; i < Math.Max(expected * 2 + 10, 40); i++)
                    {
                        await page.Page.Keyboard.PressAsync("Tab");
                        if (i >= expected && i % 3 == 0 && await page.Page.EvaluateAsync<bool>("() => window.__a11y.wrapped"))
                        {
                            break;
                        }
                    }

                    using JsonDocument summary = JsonDocument.Parse(await page.Page.EvaluateAsync<string>(AuditScripts.Summarize));
                    JsonElement s = summary.RootElement;
                    List<string> missing = Strings(s, "missing");
                    bool trap = s.GetProperty("trap").GetBoolean();
                    await ObserveAsync(
                        stop.Screen, path, engine, page, Step(Keyboard, stop),
                        "Tab ile sayfanın bütün denetimlerine ulaşılır; klavye tuzağı yok (WCAG 2.1.1, 2.1.2)",
                        $"beklenen {expected}, ulaşılan {s.GetProperty("reached").GetInt32()}, tuzak {trap}" + (missing.Count > 0 ? "; ulaşılamayan: " + string.Join(", ", missing.Take(5)) : string.Empty),
                        missing.Count == 0 && !trap);
                    List<string> noIndicator = Strings(s, "noIndicator");
                    List<string> notInView = Strings(s, "notInView");
                    List<string> jumps = Strings(s, "jumps");
                    List<string> backwards = Strings(s, "backwards");
                    List<string> order = Strings(s, "order");
                    await ObserveAsync(
                        stop.Screen, path, engine, page, Step(FocusOrder, stop),
                        "odak belge sırasını ve görsel sırayı izler (pozitif tabindex yok), her odaklanan denetim görünür bir belirteç taşır ve görünümdedir (WCAG 2.4.3, 2.4.7)",
                        $"odak sırası {order.Count} durak; pozitif tabindex {positive}; belirteçsiz {noIndicator.Count}; görünüm dışı {notInView.Count}; yukarı sıçrama {jumps.Count}; geri dönüş {backwards.Count}"
                            + (noIndicator.Count + notInView.Count + jumps.Count + backwards.Count > 0 ? ": " + string.Join(" / ", noIndicator.Concat(notInView).Concat(jumps).Concat(backwards).Take(4)) : string.Empty)
                            + "; adlar: " + string.Join(" > ", order.Take(12)) + (order.Count > 12 ? " > …" : string.Empty),
                        positive == 0 && noIndicator.Count == 0 && notInView.Count == 0 && jumps.Count == 0 && backwards.Count == 0);
                }

                // The accessible names.
                using (JsonDocument names = JsonDocument.Parse(await page.Page.EvaluateAsync<string>(AuditScripts.Names)))
                {
                    JsonElement n = names.RootElement;
                    List<string> empty = Strings(n, "empty");
                    List<string> meaningless = Strings(n, "meaningless");
                    List<string> labelNotInName = Strings(n, "labelNotInName");
                    List<string> images = Strings(n, "images");
                    await ObserveAsync(
                        stop.Screen, path, engine, page, Step(Name, stop),
                        "her denetimin ve görselin erişilebilir adı vardır ve bir şey söyler; görünür etiket adın içindedir (WCAG 1.1.1, 2.4.4, 2.5.3, 4.1.2)",
                        $"denetim {n.GetProperty("controls").GetInt32()}; adsız {empty.Count}; anlamsız ad {meaningless.Count}; etiketi adda olmayan {labelNotInName.Count}; alt metni olmayan görsel {images.Count}"
                            + (empty.Count + meaningless.Count + labelNotInName.Count + images.Count > 0 ? ": " + string.Join(" / ", empty.Concat(meaningless).Concat(labelNotInName).Concat(images).Take(4)) : string.Empty),
                        empty.Count + meaningless.Count + labelNotInName.Count + images.Count == 0);
                }

                // The page-level facts of the criteria the tool's rules do not cover.
                using (JsonDocument facts = JsonDocument.Parse(await page.Page.EvaluateAsync<string>(AuditScripts.Facts)))
                {
                    JsonElement f = facts.RootElement;
                    string title = f.GetProperty("title").GetString() ?? string.Empty;
                    List<string> sensory = Strings(f, "sensory");
                    bool ok = f.GetProperty("lang").GetString() == "tr" && title.Length > 0 && f.GetProperty("media").GetInt32() == 0 && f.GetProperty("refresh").GetInt32() == 0 && f.GetProperty("infinite").GetInt32() == 0 && sensory.Count == 0 && !f.GetProperty("skipped").GetBoolean();
                    // A stop that answers 404 or sends the visitor on (the empty cart's checkout, a re-verification with nothing to confirm) shows another screen's page: that page has its own stop, so its title is not compared here.
                    bool ownPage = stop.Status == 200 && where == new Uri(Host.BaseUrl + stop.Path).AbsolutePath;
                    if (ownPage && titles.TryGetValue(title, out string? other) && other != stop.Screen && title.Length > 0)
                    {
                        ok = false;
                        title += $" (aynı başlık {other} ekranında da)";
                    }

                    if (ownPage)
                    {
                        titles.TryAdd(title, stop.Screen);
                    }

                    await ObserveAsync(
                        stop.Screen, path, engine, page, $"sayfa olguları: {stop.State}",
                        "dil tr; ayırt edici başlık; ses, görüntü, kendiliğinden yenileme ve sonsuz hareket yok; yönergede duyusal tarif yok; başlık sırası atlamasız (WCAG 3.1.1, 2.4.2, 1.2.x, 2.2.1, 2.2.2, 1.3.3, 1.3.1)",
                        $"dil {f.GetProperty("lang").GetString()}; başlık «{title}»; ortam {f.GetProperty("media").GetInt32()}; yenileme {f.GetProperty("refresh").GetInt32()}; sonsuz hareket {f.GetProperty("infinite").GetInt32()}; duyusal tarif {sensory.Count}; başlık atlaması {f.GetProperty("skipped").GetBoolean()}" + (sensory.Count > 0 ? ": " + string.Join(" / ", sensory.Take(3)) : string.Empty),
                        ok);
                }

                await page.Page.CloseAsync();

                // The 200 percent zoom: the same address at half the desktop width.
                WatchedPage zoomed = await NewPageAsync(narrow);
                await GoAsync(zoomed, stop.Path);
                using (JsonDocument reflow = JsonDocument.Parse(await zoomed.Page.EvaluateAsync<string>(AuditScripts.Reflow)))
                {
                    JsonElement r = reflow.RootElement;
                    int over = r.GetProperty("over").GetInt32();
                    List<string> past = Strings(r, "past");
                    List<string> clipped = Strings(r, "clipped");
                    await ObserveAsync(
                        stop.Screen, path, engine, zoomed, Step(Zoom, stop),
                        $"{ZoomedWidth} piksel genişlikte (%200) sayfa yatay kaydırılmaz, içerik sağ kenarı aşmaz ve kırpılmaz (WCAG 1.4.4, 1.4.10)",
                        $"yatay taşma {over} piksel; sağ kenarı aşan {past.Count}; metni kırpılan {clipped.Count}" + (past.Count + clipped.Count > 0 ? ": " + string.Join(" / ", past.Concat(clipped).Take(4)) : string.Empty),
                        over <= 0 && past.Count == 0 && clipped.Count == 0);
                }

                await zoomed.Page.CloseAsync();
            }
        }

        // The error message heading: each form of the screen is sent with a value that is not allowed and the page that answers is read; a screen with no form with a field has nothing to send.
        ValidationStateTests validation = new(Host);
        foreach (string screen in stops.Select(s => s.Screen).Distinct())
        {
            HashSet<string> keys = formKeys.TryGetValue(screen, out HashSet<string>? found) ? [.. found] : [];
            keys.UnionWith(ValidationStateTests.FlowKeysOf(screen));
            ScreenInventoryTests.Stop first = stops.First(s => s.Screen == screen);
            if (keys.Count == 0)
            {
                await using IBrowserContext context = await AccessibilityScanTests.AuditContextAsync(Host, engine, path, first.Actor);
                WatchedPage page = await NewPageAsync(context);
                await GoAsync(page, first.Path);
                await ObserveAsync(screen, path, engine, page, $"elle denetim, {Error}: ekranın alanlı gönderim formu yok", "uygulanmaz: gönderilecek alanı olan form yok, hata iletisi doğmaz (WCAG 3.3.1)", "alanlı form yok", true);
                await page.Page.CloseAsync();
                continue;
            }

            foreach (string key in keys.Order(StringComparer.Ordinal))
            {
                ValidationStateTests.Probe probe = await validation.ProbeAsync(engine, path, key, InspectErrorAsync);
                bool sent = probe.Posts.Count > 0;
                await ObserveAsync(
                    screen, path, engine, probe.Page, $"elle denetim, {Error}: {key}",
                    "hatalı alan adıyla ve hatanın metniyle, alana bağlı ve renk dışında bir işaretle gösterilir; odak ilk hatalı alanda ya da özette (WCAG 3.3.1)",
                    probe.Inspection + $"; gönderim {string.Join(",", probe.Posts)}",
                    sent && probe.InspectionOk);
                await probe.Page.Page.CloseAsync();
            }
        }

        // Every screen has a result under each of the five headings in this engine and this way: the record has no blank.
        IReadOnlyList<Observation> record = Host.Record.Snapshot();
        string wayName = path == ScriptPath.Scripted ? "betikli" : "betiksiz";
        List<string> blanks = [];
        foreach (string screen in stops.Select(s => s.Screen).Distinct())
        {
            foreach (string heading in Headings)
            {
                if (!record.Any(o => o.Row == screen && o.Engine == engine && o.Path == wayName && o.Step.StartsWith($"elle denetim, {heading}", StringComparison.Ordinal)) && !(heading == Error && record.Any(o => o.Row == screen && o.Engine == engine && o.Path == wayName && o.Step.StartsWith($"elle denetim, {Error}", StringComparison.Ordinal))))
                {
                    blanks.Add($"{screen} / {heading}");
                }
            }
        }

        Assert.True(blanks.Count == 0, "Screens with a heading that has no result: " + string.Join(", ", blanks.Take(10)));
        Assert.Empty(Failures);
    }
}
