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
        await ChangeAsync(engine, product, "E-04", "seçenek değiştirilir", () => select.SelectOptionAsync(other), "seçilen seçenek", async () => await select.InputValueAsync(), mayStayInPlace: true);
        await product.Page.CloseAsync();

        // E-12: the quantity of a cart line (the cart answers with the amount the server counted).
        WatchedPage cart = await NewPageAsync(visitor);
        await scan.FillCartAsync(cart);
        await GoAsync(cart, "/sepet");
        ILocator quantity = cart.Page.Locator("form[data-js='submit-once'] input[name='Quantity']").First;
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
        await ChangeAsync(engine, checkout, "E-13", "teslimat ili değiştirilir", () => province.SelectOptionAsync(new SelectOptionValue { Label = "Ankara" }), "seçilen il", async () => await province.EvaluateAsync<string>("(s) => s.options[s.selectedIndex].text"));
        await checkout.Page.CloseAsync();

        // E-51: the period of the sales summary (the figures of the chosen period are drawn).
        await using IBrowserContext admin = await Host.ContextAsync(engine, ScriptPath.Scripted, Actor.Admin);
        WatchedPage summary = await NewPageAsync(admin);
        await GoAsync(summary, "/panel/satis-ozeti");
        ILocator period = summary.Page.Locator("select[name=period]");
        string[] periods = await period.EvaluateAsync<string[]>("(s) => [...s.options].map((o) => o.value)");
        string chosen = await period.InputValueAsync();
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
}
