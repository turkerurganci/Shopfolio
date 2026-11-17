using System.Text.Json;

using Microsoft.Playwright;

namespace Shopfolio.IntegrationTests.Browser;

/// <summary>
/// The level A criteria that no single screen answers and no rule of the tool covers (T552 criterion 3, KK3): a changed value changes no context (WCAG 3.2.2, VAL-A784), the help is at the same place on every screen (3.2.6, VAL-A341)
/// and what the visitor typed is not asked for again (3.3.7, VAL-A342). Each is observed in the browser, in the engines and the ways the criterion has (the change of a value only has a script to observe in the scripted way).
/// </summary>
public sealed class AccessibilityCriteriaTests(BrowserHost host) : BrowserTestBase(host)
{
    public static TheoryData<string> ScriptedEngines()
    {
        TheoryData<string> data = [];
        foreach (string engine in BrowserHost.Engines)
        {
            data.Add(engine);
        }

        return data;
    }

    private static async Task<string> HeadingAsync(WatchedPage page) => await page.Page.Locator("main h1, h1").First.InnerTextAsync();

    // ---- 3.2.2 ----

    /// <summary>
    /// Changes a value the script reacts to and reads what the visitor is left with: the same address (a screen's own update is not another screen) and the same heading. The form is sent on the change, so what is observed is the
    /// answer: the screen that was open, with its own figures changed. A change the page answers in place (the variant of a product that can be bought) sends nothing and navigates nowhere.
    /// </summary>
    private async Task ChangeAsync(string engine, WatchedPage page, string screen, string what, Func<Task> change, string expectedChange, Func<Task<string>> read, bool mayStayInPlace = false)
    {
        string pathBefore = new Uri(page.Page.Url).AbsolutePath;
        string headingBefore = await HeadingAsync(page);
        string valueBefore = await read();
        bool navigated = true;
        if (mayStayInPlace)
        {
            try
            {
                await NavigateAsync(page, change, 4000);
            }
            catch (TimeoutException)
            {
                navigated = false;
            }
            catch (PlaywrightException e) when (e.Message.Contains("Timeout", StringComparison.Ordinal))
            {
                navigated = false;
            }
        }
        else
        {
            await NavigateAsync(page, change, 20000);
        }

        string pathAfter = new Uri(page.Page.Url).AbsolutePath;
        string headingAfter = await HeadingAsync(page);
        string valueAfter = await read();
        await ObserveAsync(
            "VAL-A784", ScriptPath.Scripted, engine, page, $"{screen}: {what}",
            "değer değişince sayfa başka bir ekrana götürmez: adres ve başlık aynı kalır; ekranın kendi değerleri güncellenir (WCAG 3.2.2)",
            $"adres {pathBefore} -> {pathAfter}; başlık «{headingBefore}» -> «{headingAfter}»; {expectedChange}: «{valueBefore}» -> «{valueAfter}»; sayfa yenilendi {navigated}",
            pathBefore == pathAfter && headingBefore == headingAfter && valueBefore != valueAfter);
    }

    /// <summary>
    /// The notice before the change (VAL-A784, "değişim önceden söylenmiştir"): the form that holds the field says, visibly and in its own text, that a change of a value updates the page by itself - measured on the page the
    /// visitor sees before touching the field, with the text the one component writes (<see cref="Shopfolio.Pages.Components.ChangeNotice.Text"/>).
    /// </summary>
    private async Task NoticeAsync(string engine, WatchedPage page, string screen, ILocator field)
    {
        string told = await field.EvaluateAsync<string>("(el) => { const n = el.closest('form')?.querySelector('[data-js=\"change-notice\"]'); return n && !n.hidden && n.getClientRects().length > 0 ? n.textContent.trim() : ''; }");
        await ObserveAsync(
            "VAL-A784", ScriptPath.Scripted, engine, page, $"{screen}: değişim önceden söylenir",
            "değer değişince kendiliğinden gönderilen formda değişiklikten önce görünen kısa bilgilendirme vardır (WCAG 3.2.2)",
            $"formdaki bilgilendirme: «{told}»", told == Shopfolio.Pages.Components.ChangeNotice.Text);
    }

    [Theory]
    [MemberData(nameof(ScriptedEngines))]
    public async Task AChangedValue_OnTheScreensThatReactToIt_LeavesTheSameScreenWithItsOwnFiguresChanged_VALA784(string engine)
    {
        AccessibilityScanTests scan = new(Host);

        // E-04: the variant of a product with two options is chosen in place (the price and the stock line follow).
        await using IBrowserContext visitor = await Host.ContextAsync(engine, ScriptPath.Scripted, Actor.Visitor);
        WatchedPage product = await NewPageAsync(visitor);
        await GoAsync(product, $"/urun/{Host.Seed.Product("variants").Slug}");
        ILocator select = product.Page.Locator("[data-js='variant-form'] select").First;
        string[] values = await select.EvaluateAsync<string[]>("(s) => [...s.options].map((o) => o.value)");
        string current = await select.InputValueAsync();
        string other = values.First(v => v != current);
        await NoticeAsync(engine, product, "E-04", select);
        await ChangeAsync(engine, product, "E-04", "seçenek değiştirilir", () => select.SelectOptionAsync(other), "seçilen seçenek", async () => await select.InputValueAsync(), mayStayInPlace: true);
        await product.Page.CloseAsync();

        // E-12: the quantity of a cart line (the cart answers with the amount the server counted).
        WatchedPage cart = await NewPageAsync(visitor);
        await scan.FillCartAsync(cart);
        await GoAsync(cart, "/sepet");
        ILocator quantity = cart.Page.Locator("form[data-js='submit-once'] input[name='Quantity']").First;
        await NoticeAsync(engine, cart, "E-12", quantity);
        await ChangeAsync(engine, cart, "E-12", "adet değiştirilir", async () =>
        {
            await quantity.FillAsync("2");
            await quantity.PressAsync("Tab");
        }, "satır tutarı", async () => (await cart.Page.Locator(".cart-line__total").First.InnerTextAsync()).Replace('\n', ' '));
        await cart.Page.CloseAsync();

        // E-13: the delivery province of the payment step (the summary is recalculated).
        WatchedPage checkout = await NewPageAsync(visitor);
        await GoAsync(checkout, "/odeme");
        ILocator province = checkout.Page.Locator("select[name=ShippingProvinceId]");
        await NoticeAsync(engine, checkout, "E-13", province);
        await ChangeAsync(engine, checkout, "E-13", "teslimat ili değiştirilir", () => province.SelectOptionAsync(new SelectOptionValue { Label = "Ankara" }), "seçilen il", async () => await province.EvaluateAsync<string>("(s) => s.options[s.selectedIndex].text"));
        await checkout.Page.CloseAsync();

        // E-51: the period of the sales summary (the figures of the chosen period are drawn).
        await using IBrowserContext admin = await Host.ContextAsync(engine, ScriptPath.Scripted, Actor.Admin);
        WatchedPage summary = await NewPageAsync(admin);
        await GoAsync(summary, "/panel/satis-ozeti");
        ILocator period = summary.Page.Locator("select[name=period]");
        string[] periods = await period.EvaluateAsync<string[]>("(s) => [...s.options].map((o) => o.value)");
        string chosen = await period.InputValueAsync();
        await NoticeAsync(engine, summary, "E-51", period);
        await ChangeAsync(engine, summary, "E-51", "dönem değiştirilir", () => period.SelectOptionAsync(periods.First(v => v != chosen && v.Length > 0)), "seçilen dönem", async () => await period.InputValueAsync());
        await summary.Page.CloseAsync();

        Assert.Empty(Failures);
    }

    // ---- 3.2.6 ----

    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheHelpLink_IsAtTheSamePlaceOnEveryScreenThatHasIt_VALA341(string engine, ScriptPath path)
    {
        AccessibilityScanTests scan = new(Host);
        Dictionary<string, List<string>> where = [];
        WatchedPage? last = null;
        foreach (IGrouping<(Actor Actor, bool Prepared), ScreenInventoryTests.Stop> group in AccessibilityScanTests.ContextsOf(scan.AuditStops()))
        {
            await using IBrowserContext context = await AccessibilityScanTests.AuditContextAsync(Host, engine, path, group.Key.Actor);
            await AccessibilityScanTests.PrepareAsync(context, group);
            foreach (ScreenInventoryTests.Stop stop in group)
            {
                WatchedPage page = await NewPageAsync(context);
                await GoAsync(page, stop.Path);
                using JsonDocument facts = JsonDocument.Parse(await page.Page.EvaluateAsync<string>(AuditScripts.Facts));
                string frame = stop.Path.StartsWith("/panel", StringComparison.Ordinal) ? "panel" : "vitrin";
                string help = "yardım bağlantısı altbilgide sıra " + string.Join(",", facts.RootElement.GetProperty("help").EnumerateArray().Select(e => e.GetInt32()));
                if (!where.TryGetValue(frame + ": " + help, out List<string>? screens))
                {
                    where[frame + ": " + help] = screens = [];
                }

                screens.Add(stop.Screen);
                if (last is not null)
                {
                    await last.Page.CloseAsync();
                }

                last = page;
            }
        }

        // The storefront screens (and the member's, which are the storefront's frame) hold the link at one place; the panel's frame is one more frame with its own, and it too holds it at one place or nowhere on every panel screen.
        IEnumerable<string> storefront = where.Keys.Where(k => k.StartsWith("vitrin:", StringComparison.Ordinal));
        IEnumerable<string> panel = where.Keys.Where(k => k.StartsWith("panel:", StringComparison.Ordinal));
        Assert.True(last is not null);
        string observed = string.Join("; ", where.Select(w => $"{w.Key} ({w.Value.Distinct().Count()} ekran" + (w.Value.Distinct().Count() <= 5 ? ": " + string.Join(",", w.Value.Distinct()) : string.Empty) + ")"));
        await ObserveAsync(
            "VAL-A341", path, engine, last!, "yardım bağlantısı (iletişim sayfası) her ekranda aynı yerde",
            "yardım mekanizması (altbilgideki iletişim bağlantısı) birden çok ekranda tekrarlanıyorsa her çerçevede aynı göreli yerdedir (WCAG 3.2.6)",
            observed,
            storefront.Count() == 1 && panel.Count() == 1 && !storefront.Single().EndsWith("sıra ", StringComparison.Ordinal));
        Assert.Empty(Failures);
    }

    // ---- 3.3.7 ----

    [Theory]
    [MemberData(nameof(Ways))]
    public async Task WhatTheVisitorTyped_IsKeptWhenTheVisitorLeavesTheStepAndComesBack_VALA342(string engine, ScriptPath path)
    {
        AccessibilityScanTests scan = new(Host);
        await using IBrowserContext context = await Host.ContextAsync(engine, path);
        WatchedPage page = await NewPageAsync(context);
        await scan.FillCartAsync(page);
        await GoAsync(page, "/odeme");
        ILocator same = page.Page.Locator("input[type=checkbox][name=IsBillingSameAsShipping]");
        bool sameChecked = await same.IsCheckedAsync();
        string email = $"a342-{engine}-{path}@example.com".ToLowerInvariant();
        await page.Page.Locator("[name=ContactEmail]").FillAsync(email);
        await page.Page.Locator("[name=ShippingRecipientName]").FillAsync("Ali Veli");
        await page.Page.Locator("[name=ShippingDistrict]").FillAsync("Çankaya");
        await page.Page.Locator("[name=ShippingAddressLine]").FillAsync("Kızılay Mah. 2");
        await page.Page.Locator("[name=ShippingPhone]").FillAsync("5321234567");

        // Leaving by the sign-in link (the form is sent with it, 04 5.13.5) and coming back to the step.
        await NavigateAsync(page, () => page.Page.Locator("button[name=LeaveTarget][value=login]").First.ClickAsync());
        string leftTo = new Uri(page.Page.Url).AbsolutePath;
        await GoAsync(page, "/odeme");
        string[] fields = ["ContactEmail", "ShippingRecipientName", "ShippingDistrict", "ShippingAddressLine", "ShippingPhone"];
        string[] expected = [email, "Ali Veli", "Çankaya", "Kızılay Mah. 2", "5321234567"];
        List<string> lost = [];
        for (int i = 0; i < fields.Length; i++)
        {
            string value = await page.Page.Locator($"[name={fields[i]}]").InputValueAsync();
            if (value != expected[i])
            {
                lost.Add($"{fields[i]} «{value}»");
            }
        }

        await ObserveAsync(
            "VAL-A342", path, engine, page, "ödeme adımından girişe gidilir ve geri dönülür",
            "aynı süreçte daha önce yazılan bilgi yeniden istenmez: dönüşte alanlar yazılanı taşır; fatura adresi kutusu varsayılan olarak işaretlidir (WCAG 3.3.7)",
            $"girişe gidilen adres {leftTo}; fatura adresi aynı kutusu işaretli {sameChecked}; boşalan alan {lost.Count}" + (lost.Count > 0 ? ": " + string.Join(", ", lost) : string.Empty),
            sameChecked && lost.Count == 0 && leftTo.StartsWith("/giris", StringComparison.Ordinal));
        Assert.Empty(Failures);
    }

    // ---- 2.1.1, 2.1.2 (VAL-A345) ----

    private const int MaxTabs = 300;

    /// <summary>The element that holds the focus: its place in the document, tag, id, name and text (two elements are told apart by the place).</summary>
    private const string FocusedElement = "() => { const a = document.activeElement; if (!a || a === document.body) { return 'body'; } return [...document.querySelectorAll('*')].indexOf(a) + ':' + a.tagName + '#' + a.id + '[' + (a.getAttribute('name') || '') + ']' + (a.textContent || '').replace(/\\s+/g, ' ').trim().slice(0, 24); }";

    private static async Task<bool> FocusIsOnAsync(IPage page, string selector) => await page.EvaluateAsync<bool>("(s) => !!document.activeElement && document.activeElement.matches(s)", selector);

    /// <summary>
    /// Takes the focus to the element the way a person without a pointer does: the Tab key, from where the focus is, until it is on the element. A focus that does not leave an element after two presses, or that does not reach the element in
    /// <see cref="MaxTabs"/> presses, is a keyboard trap or an element the keyboard cannot reach. WebKit (Playwright 26.4 on Windows) does not take a link to its Tab order, so there the focus is set on the element and the keys (Enter, Space, the arrows) that work it are still the keyboard's.
    /// </summary>
    private static async Task<(bool Reached, int Tabs, string Problem)> ReachAsync(IPage page, string engine, string selector)
    {
        if (engine == "webkit")
        {
            ILocator target = page.Locator(selector).First;
            if (await target.CountAsync() == 0)
            {
                return (false, 0, $"{selector} is not on the page");
            }

            await target.FocusAsync();
            return (await FocusIsOnAsync(page, selector), 0, "the element does not take the focus");
        }

        string previous = string.Empty;
        int stuck = 0;
        for (int tabs = 0; tabs <= MaxTabs; tabs++)
        {
            if (await FocusIsOnAsync(page, selector))
            {
                return (true, tabs, string.Empty);
            }

            await page.Keyboard.PressAsync("Tab");
            string now = await page.EvaluateAsync<string>(FocusedElement);
            stuck = now == previous ? stuck + 1 : 0;
            if (stuck >= 2)
            {
                return (false, tabs, $"keyboard trap: the focus does not leave {now}");
            }

            previous = now;
        }

        return (false, MaxTabs, $"{selector} is not reached in {MaxTabs} Tab presses");
    }

    /// <summary>One step of the walk: the focus is taken to the element by the keyboard, the action is done by keys, and the step is written down.</summary>
    private static async Task StepAsync(List<string> steps, List<string> problems, IPage page, string engine, string what, string selector, Func<Task> act)
    {
        (bool reached, int tabs, string problem) = await ReachAsync(page, engine, selector);
        if (!reached)
        {
            problems.Add($"{what}: {problem}");
            return;
        }

        await act();
        steps.Add($"{what} ({tabs} Tab)");
    }

    /// <summary>
    /// VAL-A345: from the cart to the order confirmation with the keyboard alone (WCAG 2.1.1, 2.1.2). Every control of the walk is reached by the Tab key and worked by keys: the link of the cart, the fields typed, the province chosen with the arrows,
    /// the consents ticked with the space, the confirm button pressed with Enter. Distinguishing: an element the Tab key does not reach (tabindex -1) or a trap ends the walk with the problem named, and the observation fails.
    /// </summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheStorefrontFlow_FromTheCartToTheOrderConfirmation_IsDoneByTheKeyboardAlone_VALA345(string engine, ScriptPath path)
    {
        await using IBrowserContext context = await Host.ContextAsync(engine, path);
        WatchedPage page = await NewPageAsync(context);
        await new AccessibilityScanTests(Host).FillCartAsync(page);
        await GoAsync(page, "/sepet");
        IPage p = page.Page;
        List<string> steps = [];
        List<string> problems = [];
        string email = $"a345-{engine}-{path}@example.com".ToLowerInvariant();

        // Without the script a changed field sends nothing; with it the page is replaced after the change (09 16.2 item 1), so the wait is for the page.
        async Task ChangedAsync(Func<Task> act)
        {
            if (path == ScriptPath.Scripted)
            {
                await NavigateAsync(page, act, 20000);
            }
            else
            {
                await act();
            }
        }

        await StepAsync(steps, problems, p, engine, "sepetten ödemeye geç bağlantısı", "main a[href='/odeme']", () => NavigateAsync(page, () => p.Keyboard.PressAsync("Enter")));
        if (problems.Count == 0 && !new Uri(p.Url).AbsolutePath.StartsWith("/odeme", StringComparison.Ordinal))
        {
            problems.Add($"Enter on the link did not open the payment step ({new Uri(p.Url).AbsolutePath})");
        }

        (string Name, string Text)[] typed =
        [
            ("ContactEmail", email),
            ("ShippingRecipientName", "Ali Veli"),
            ("ShippingDistrict", "Çankaya"),
            ("ShippingAddressLine", "Kızılay Mah. 2"),
            ("ShippingPhone", "5321234567"),
        ];
        if (problems.Count == 0)
        {
            await StepAsync(steps, problems, p, engine, "e-posta yazılır", "[name=ContactEmail]", () => p.Keyboard.TypeAsync(email));

            // The province: the arrows move the choice one place at a time; with the script every move sends the form, and the focus is taken back by the keyboard.
            for (int move = 0; move < 100 && problems.Count == 0; move++)
            {
                string chosen = await p.Locator("select[name=ShippingProvinceId]").EvaluateAsync<string>("(s) => s.selectedOptions.length ? s.selectedOptions[0].textContent.trim() : ''");
                if (chosen == "Ankara")
                {
                    steps.Add($"il oklarla seçildi ({move} ok)");
                    break;
                }

                await StepAsync(steps, problems, p, engine, "il seçilir", "select[name=ShippingProvinceId]", () => ChangedAsync(() => p.Keyboard.PressAsync("ArrowDown")));
                steps.RemoveAll(s => s.StartsWith("il seçilir", StringComparison.Ordinal));
            }

            foreach ((string name, string text) in typed.Skip(1))
            {
                await StepAsync(steps, problems, p, engine, $"{name} yazılır", $"[name={name}]", () => p.Keyboard.TypeAsync(text));
            }

            ILocator transfer = p.Locator("#method-banktransfer");
            if (await transfer.CountAsync() > 0 && !await transfer.IsCheckedAsync())
            {
                await StepAsync(steps, problems, p, engine, "havale yöntemi boşlukla seçilir", "#method-banktransfer", () => ChangedAsync(() => p.Keyboard.PressAsync("Space")));
            }

            int consents = await p.Locator(".consents input[type=checkbox]").CountAsync();
            for (int i = 0; i < consents; i++)
            {
                int index = i;
                // The box is marked so that the focus can be asked "is it on this one" by a selector; the mark is no way to it for the visitor.
                await p.Locator(".consents input[type=checkbox]").Nth(index).EvaluateAsync("(el) => { document.querySelectorAll('[data-walk-target]').forEach((e) => e.removeAttribute('data-walk-target')); el.setAttribute('data-walk-target', ''); }");
                const string box = "[data-walk-target]";
                await StepAsync(steps, problems, p, engine, $"onay kutusu {index + 1} boşlukla işaretlenir", box, async () =>
                {
                    if (!await p.Locator(box).IsCheckedAsync())
                    {
                        await p.Keyboard.PressAsync("Space");
                    }
                });
            }
        }

        string confirm = ".checkout__confirm button.button--primary";
        if (problems.Count == 0)
        {
            await StepAsync(steps, problems, p, engine, "siparişi onayla düğmesi", confirm, async () =>
            {
                await p.Keyboard.PressAsync("Enter");
                await p.WaitForURLAsync(u => u.Contains("/teyit", StringComparison.Ordinal), new PageWaitForURLOptions { Timeout = 25000 });
            });
        }

        bool onConfirmation = new Uri(p.Url).AbsolutePath.StartsWith("/siparis", StringComparison.Ordinal) || p.Url.Contains("/teyit", StringComparison.Ordinal);
        string title = onConfirmation && await p.Locator("#order-confirmation-title").CountAsync() > 0 ? (await p.Locator("#order-confirmation-title").InnerTextAsync()).Trim() : string.Empty;
        await ObserveAsync(
            "VAL-A345", path, engine, page, "sepetten sipariş onayına yalnız klavyeyle",
            "sepetten sipariş onayına kadar her adım klavyeyle (Tab, yazı, oklar, boşluk, Enter) tamamlanır ve klavye tuzağı yoktur (WCAG 2.1.1, 2.1.2)",
            $"{steps.Count} adım yapıldı" + (engine == "webkit" ? " (WebKit: odak öğeye verilir, eylem tuşlarla)" : string.Empty) + $"; son adres {new Uri(p.Url).AbsolutePath}; onay başlığı «{title}»" + (problems.Count > 0 ? "; sorun: " + string.Join(" / ", problems) : string.Empty),
            problems.Count == 0 && title.StartsWith("Sipariş", StringComparison.Ordinal));
        Assert.Empty(Failures);
    }

    // ---- VAL-A350 ----

    /// <summary>The words by which a page would claim accessibility conformance: the word of accessibility, the name of the standard, the words of a statement.</summary>
    internal const string ConformanceClaim = "(erişilebilirlik|erisilebilirlik|accessibility|wcag|a11y|uyumluluk beyan|uyumluluk bildirim|conformance)";

    /// <summary>
    /// VAL-A350: the product gives no statement of accessibility conformance (02 12.4.1): no screen of the site or the panel, in the text a person reads or in a link's name or address, claims one. Read in the browser on every screen of the
    /// pass (<see cref="AccessibilityScanTests.AuditStops"/>), the visitor's and the administrator's.
    /// </summary>
    [Theory]
    [MemberData(nameof(ScriptedEngines))]
    public async Task TheSiteAndThePanel_GiveNoStatementOfAccessibilityConformance_VALA350(string engine)
    {
        AccessibilityScanTests scan = new(Host);
        const string read = "(pattern) => { const re = new RegExp(pattern, 'i'); const hits = []; const text = (document.body.innerText || '').toLocaleLowerCase('tr'); const m = text.match(re); if (m) { hits.push('metin: ' + m[0]); } for (const a of document.querySelectorAll('a[href], [aria-label], [title]')) { const t = [a.getAttribute('href'), a.getAttribute('aria-label'), a.getAttribute('title'), a.textContent].join(' ').toLocaleLowerCase('tr'); const k = t.match(re); if (k) { hits.push('bağlantı/ad: ' + k[0]); } } return hits.slice(0, 3); }";
        int screensRead = 0;
        List<string> claims = [];
        WatchedPage? last = null;
        foreach (IGrouping<(Actor Actor, bool Prepared), ScreenInventoryTests.Stop> group in AccessibilityScanTests.ContextsOf(scan.AuditStops()))
        {
            await using IBrowserContext context = await AccessibilityScanTests.AuditContextAsync(Host, engine, ScriptPath.Scripted, group.Key.Actor);
            await AccessibilityScanTests.PrepareAsync(context, group);
            foreach (ScreenInventoryTests.Stop stop in group)
            {
                WatchedPage page = await NewPageAsync(context);
                await GoAsync(page, stop.Path);
                string[] hits = await page.Page.EvaluateAsync<string[]>(read, ConformanceClaim);
                screensRead++;
                if (hits.Length > 0)
                {
                    claims.Add($"{stop.Screen} ({stop.Path}): {string.Join(", ", hits)}");
                }

                if (last is not null)
                {
                    await last.Page.CloseAsync();
                }

                last = page;
            }
        }

        Assert.True(last is not null && screensRead >= 50, $"{screensRead} screens were read; at least 50 are expected.");
        await ObserveAsync(
            "VAL-A350", ScriptPath.Scripted, engine, last!, "sitede ve panelde uyumluluk beyanı aranır",
            "sitede ve panelde erişilebilirlik uyumluluk beyanı yoktur: hiçbir ekranın metninde, bağlantı adında ya da adresinde ölçüt adı, erişilebilirlik sözcüğü ya da beyan ifadesi geçmez (02 §12.4.1)",
            $"{screensRead} ekran okundu; beyan arayan ifadeyle eşleşen {claims.Count}" + (claims.Count > 0 ? ": " + string.Join(" / ", claims.Take(4)) : string.Empty),
            claims.Count == 0);
        Assert.Empty(Failures);
    }
}
