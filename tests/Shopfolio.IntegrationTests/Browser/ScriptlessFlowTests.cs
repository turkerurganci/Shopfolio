using System.Text.RegularExpressions;

using Microsoft.Playwright;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Components;

namespace Shopfolio.IntegrationTests.Browser;

/// <summary>
/// The second part of the scriptless path's observation (T551 criterion 3; 12 4.17, rows <c>VAL-B097</c>, <c>B099</c>, <c>B101</c>, <c>B102</c>, <c>B105</c>...<c>B112</c>): the payment
/// step, the product form, the account and the panel forms, each in both ways and each engine. A row whose evidence is a data record also reads the table row, masked
/// (<see cref="DataRecord"/>). The line of a row names its own step, what was expected and what was seen: no shared sentence stands for a row.
/// </summary>
public sealed class ScriptlessFlowTests(BrowserHost host) : BrowserTestBase(host)
{

    private static readonly Regex Spaces = new(@"\s+", RegexOptions.None, TimeSpan.FromSeconds(2));

    private static int _unique;

    /// <summary>A real 16 x 16 PNG (the upload's content check reads the bytes): signature, header, one deflated data chunk, end; each chunk with its CRC.</summary>
    private static byte[] MakePng()
    {
        const int size = 16;
        using MemoryStream raw = new();
        for (int y = 0; y < size; y++)
        {
            raw.WriteByte(0);
            for (int x = 0; x < size; x++)
            {
                raw.Write([(byte)(x * 16), (byte)(y * 16), 128]);
            }
        }

        using MemoryStream deflated = new();
        using (System.IO.Compression.ZLibStream z = new(deflated, System.IO.Compression.CompressionLevel.Optimal, leaveOpen: true))
        {
            z.Write(raw.ToArray());
        }

        using MemoryStream png = new();
        png.Write([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        byte[] header = [0, 0, 0, size, 0, 0, 0, size, 8, 2, 0, 0, 0];
        WriteChunk(png, "IHDR", header);
        WriteChunk(png, "IDAT", deflated.ToArray());
        WriteChunk(png, "IEND", []);
        return png.ToArray();
    }

    private static void WriteChunk(Stream stream, string type, byte[] data)
    {
        byte[] name = System.Text.Encoding.ASCII.GetBytes(type);
        byte[] length = new byte[4];
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(length, (uint)data.Length);
        stream.Write(length);
        stream.Write(name);
        stream.Write(data);
        uint crc = 0xFFFFFFFF;
        foreach (byte b in name.Concat(data))
        {
            crc ^= b;
            for (int k = 0; k < 8; k++)
            {
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320 : crc >> 1;
            }
        }

        byte[] check = new byte[4];
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(check, ~crc);
        stream.Write(check);
    }

    private static string Flat(string text) => Spaces.Replace(text, " ").Trim();

    private static string Way(ScriptPath path) => path == ScriptPath.Scripted ? "betikli" : "betiksiz";

    private static string Unique(string engine, ScriptPath path) => $"{engine[..2]}{(path == ScriptPath.Scripted ? "s" : "n")}{Interlocked.Increment(ref _unique)}";

    private static string PathOf(WatchedPage page) => new Uri(page.Page.Url).PathAndQuery;

    /// <summary>
    /// Runs the action; with the script on, waits until the page has been replaced (the script sends the form with the changed field, 09 16.2 item 1). Without the script a changed field
    /// sends nothing, so no page is waited for unless the action itself is a send (<paramref name="sends"/>).
    /// </summary>
    private static async Task ChangeAsync(WatchedPage page, ScriptPath path, Func<Task> act, bool sends = false)
    {
        if (path == ScriptPath.Scripted || sends)
        {
            await NavigateAsync(page, act, 20000);
        }
        else
        {
            await act();
        }
    }

    private async Task AddToCartAsync(WatchedPage page, string slug)
    {
        await GoAsync(page, $"/urun/{slug}");
        await NavigateAsync(page, () => page.Page.Locator("form.buy-form button[type=submit]").First.ClickAsync());
    }

    private static async Task<string> TextAsync(ILocator locator)
    {
        return await locator.CountAsync() > 0 ? Flat(await locator.First.InnerTextAsync()) : string.Empty;
    }

    // ---- the payment step ----

    private static async Task FillCheckoutAsync(WatchedPage page, ScriptPath path, string email, bool sameAsShipping, string recipient = "Ali Veli", bool decoy = false)
    {
        IPage p = page.Page;
        await p.Locator("[name=ContactEmail]").FillAsync(email);
        await ChangeAsync(page, path, () => p.Locator("select[name=ShippingProvinceId]").SelectOptionAsync(new SelectOptionValue { Label = "Ankara" }));
        await p.Locator("[name=ShippingRecipientName]").FillAsync(recipient);
        await p.Locator("[name=ShippingDistrict]").FillAsync("Çankaya");
        await p.Locator("[name=ShippingAddressLine]").FillAsync("Kızılay Mah. 2");
        await p.Locator("[name=ShippingPhone]").FillAsync("5321234567");
        ILocator same = p.Locator("input[type=checkbox][name=IsBillingSameAsShipping]");
        if (sameAsShipping)
        {
            if (!await same.IsCheckedAsync())
            {
                await same.CheckAsync();
            }

            if (decoy && path == ScriptPath.Scriptless)
            {
                // Without the script the billing fields stay on the page; what they hold is not read while the box is ticked (07 4.13.6).
                await p.Locator("[name=BillingRecipientName]").FillAsync("Baska Kisi");
                await p.Locator("[name=BillingDistrict]").FillAsync("Baska Ilce");
                await p.Locator("[name=BillingAddressLine]").FillAsync("Baska Cadde 9");
            }
        }
        else
        {
            if (await same.IsCheckedAsync())
            {
                await same.UncheckAsync();
            }

            await ChangeAsync(page, path, () => p.Locator("select[name=BillingProvinceId]").SelectOptionAsync(new SelectOptionValue { Label = "Ankara" }));
            await p.Locator("[name=BillingRecipientName]").FillAsync("Fatura Sahibi");
            await p.Locator("[name=BillingDistrict]").FillAsync("Keçiören");
            await p.Locator("[name=BillingAddressLine]").FillAsync("Fatura Cad. 5");
        }

        ILocator transfer = p.Locator("#method-banktransfer");
        if (await transfer.CountAsync() > 0 && !await transfer.IsCheckedAsync())
        {
            await ChangeAsync(page, path, () => transfer.CheckAsync());
        }

        ILocator boxes = p.Locator(".consents input[type=checkbox]");
        for (int i = 0; i < await boxes.CountAsync(); i++)
        {
            await boxes.Nth(i).CheckAsync();
        }
    }

    private static async Task<string> SubmitCheckoutAsync(WatchedPage page)
    {
        await page.Page.Locator(".checkout__confirm button.button--primary[data-js=submit-once]").ClickAsync();
        await page.Page.WaitForURLAsync(u => u.Contains("/teyit", StringComparison.Ordinal), new PageWaitForURLOptions { Timeout = 25000 });
        string title = await TextAsync(page.Page.Locator("#order-confirmation-title"));
        return title.Replace("Sipariş", string.Empty, StringComparison.Ordinal).Trim();
    }

    private async Task<string> PlacedOrderRowAsync(string number) => await DataRecord.RowAsync(
        Host.Dsn,
        "SELECT order_number, contact_email, order_status, (billing_recipient_name = shipping_recipient_name AND billing_district = shipping_district AND billing_address_line = shipping_address_line) AS billing_equals_shipping, "
        + "billing_recipient_name, billing_district, billing_address_line, (billing_recipient_name = 'Baska Kisi') AS decoy_taken FROM orders WHERE order_number = @n",
        ("n", number));

    /// <summary>VAL-B107 (KRITIK): "Teslimat adresimle aynı" - the billing form follows the box in both ways; ticked, the billing fields are the delivery's four and what was sent is not read; unticked, the typed values are written.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheSameAsDeliveryBox_DecidesTheBillingAddress_InBothWays_VALB107(string engine, ScriptPath path)
    {
        foreach (bool sameAsShipping in new[] { true, false })
        {
            string email = $"b107-{Unique(engine, path)}@example.com";
            await using IBrowserContext context = await Host.ContextAsync(engine, path);
            WatchedPage page = await NewPageAsync(context);
            await AddToCartAsync(page, Host.Seed.Product("plain").Slug);
            await GoAsync(page, "/odeme");
            await FillCheckoutAsync(page, path, email, sameAsShipping, decoy: true);
            bool billingShown = await page.Page.Locator("[name=BillingRecipientName]").IsVisibleAsync();
            string number = await SubmitCheckoutAsync(page);
            string row = await PlacedOrderRowAsync(number);
            bool ok = sameAsShipping
                ? row.Contains("billing_equals_shipping=True", StringComparison.Ordinal) && row.Contains("decoy_taken=False", StringComparison.Ordinal) && (path == ScriptPath.Scriptless || !billingShown)
                : row.Contains("billing_recipient_name=Fatura Sahibi", StringComparison.Ordinal) && row.Contains("billing_equals_shipping=False", StringComparison.Ordinal) && billingShown;
            await ObserveAsync(
                "VAL-B107",
                path,
                engine,
                page,
                sameAsShipping ? "kutu işaretli, fatura alanları" + (path == ScriptPath.Scriptless ? " (başka bir kişinin değerleriyle dolu) " : " ") + "sipariş onaylanır" : "kutu kaldırılır, fatura alanları doldurulur, sipariş onaylanır",
                sameAsShipping ? "fatura adresi teslimat adresinin dört alanıdır; gönderilen fatura alanları siparişe girmez; betikli yolda form kutuyla gizlenir" : "fatura formu açılır; girilen değerler (Fatura Sahibi, Keçiören) siparişe yazılır",
                $"fatura formu görünür: {billingShown}; sipariş {number}",
                ok,
                row);
            Assert.Empty(Failures);
        }
    }

    /// <summary>VAL-B109: the storefront's basic flow completes - browse, choose a product, the cart, the payment step's confirmation - in both ways.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheStorefrontFlow_Completes_InBothWays_VALB109(string engine, ScriptPath path)
    {
        string email = $"b109-{Unique(engine, path)}@example.com";
        await using IBrowserContext context = await Host.ContextAsync(engine, path);
        WatchedPage page = await NewPageAsync(context);
        await GoAsync(page, "/");
        await GoAsync(page, $"/kategori/{Host.Seed.CategorySlug}");
        await NavigateAsync(page, () => page.Page.Locator($"a[href='/urun/{Host.Seed.Product("plain").Slug}']").First.ClickAsync());
        await NavigateAsync(page, () => page.Page.Locator("form.buy-form button[type=submit]").First.ClickAsync());
        await GoAsync(page, "/sepet");
        string line = await TextAsync(page.Page.Locator(".cart-line"));
        await page.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Ödemeye geç" }).Or(page.Page.Locator("a[href='/odeme']")).First.ClickAsync();
        await page.Page.WaitForURLAsync(u => u.Contains("/odeme", StringComparison.Ordinal));
        await FillCheckoutAsync(page, path, email, true);
        string number = await SubmitCheckoutAsync(page);
        string row = await DataRecord.RowAsync(Host.Dsn, "SELECT order_number, contact_email, order_status, payment_status FROM orders WHERE order_number = @n", ("n", number));
        await ObserveAsync("VAL-B109", path, engine, page, "kategori, ürün, sepete ekle, sepet, ödeme adımı, onay", "sipariş açılır ve teyit ekranı gösterilir (sipariş numarası, havale bilgileri)", $"sepet satırı \"{line}\"; teyit ekranı {PathOf(page)}; sipariş {number}", row.StartsWith("order_number=", StringComparison.Ordinal) && !row.Contains("satır yok", StringComparison.Ordinal), row);
        Assert.Empty(Failures);
    }

    /// <summary>VAL-B099: the payment step's summary - the script sends the form with the coupon button's own handler (the same handler), the scriptless path sends it by the button; a changed summary opens no order.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheSummary_IsTheServers_AndAChangedSummaryOpensNoOrder_InBothWays_VALB099(string engine, ScriptPath path)
    {
        string email = $"b099-{Unique(engine, path)}@example.com";
        await using IBrowserContext context = await Host.ContextAsync(engine, path);
        WatchedPage page = await NewPageAsync(context);
        await AddToCartAsync(page, Host.Seed.Product("plain").Slug);
        await GoAsync(page, "/odeme");
        string before = await TextAsync(page.Page.Locator(".checkout__summary .checkout__total strong, aside .checkout__total strong"));
        WatchedPage second = await NewPageAsync(context);
        await GoAsync(second, "/odeme");
        await FillCheckoutAsync(page, path, email, true);
        // The summary changes under the first page: the coupon is applied on the second one (the cart's coupon is the server's state).
        await second.Page.Locator("input[name=CouponCode]").FillAsync(Host.Seed.CouponCode);
        await ChangeAsync(second, path, async () =>
        {
            if (path == ScriptPath.Scriptless)
            {
                await second.Page.Locator("button[data-js=apply-coupon]").ClickAsync();
            }
            else
            {
                await second.Page.Locator("input[name=CouponCode]").PressAsync("Tab");
            }
        }, sends: true);
        string applied = await TextAsync(second.Page.Locator(".coupon__applied"));
        string after = await TextAsync(second.Page.Locator("aside .checkout__total strong"));
        await ObserveAsync("VAL-B099", path, engine, second, path == ScriptPath.Scripted ? "kupon kodu yazılır ve alandan çıkılır (betik formu kupon düğmesiyle gönderir)" : "kupon kodu yazılır ve Uygula'ya basılır", "özet sunucudan gelir: kupon uygulanır, toplam düşer", $"kupon \"{applied}\"; toplam önce \"{before}\", sonra \"{after}\"", applied.Contains(Host.Seed.CouponCode, StringComparison.Ordinal) && before != after);
        // The cart changes under the first page (two units): its summary is stale now.
        await GoAsync(second, "/sepet");
        await second.Page.Locator("form.cart-line__quantity input[name=Quantity]").First.FillAsync("2");
        await NavigateAsync(second, () => second.Page.Locator("form.cart-line__quantity button[type=submit]").First.ClickAsync());
        await NavigateAsync(page, () => page.Page.Locator(".checkout__confirm button.button--primary[data-js=submit-once]").ClickAsync());
        long orders = await DataRecord.CountAsync(Host.Dsn, "SELECT count(*) FROM orders WHERE contact_email = @e", ("e", email));
        bool onStep = PathOf(page).StartsWith("/odeme", StringComparison.Ordinal);
        string warning = await TextAsync(page.Page.Locator(".checkout__confirm .message"));
        string shownTotal = await TextAsync(page.Page.Locator("aside .checkout__total strong"));
        await GoAsync(second, "/odeme");
        string currentTotal = await TextAsync(second.Page.Locator("aside .checkout__total strong"));
        await ObserveAsync(
            "VAL-B099",
            path,
            engine,
            page,
            "eski özetli sayfadan onay gönderilir (sepet ikinci sayfada iki adede çıkarılmıştı)",
            "özet değiştiyse sipariş oluşmaz ve güncel özet gösterilir (toplam sunucunun şimdiki toplamıdır, eski toplam değildir)",
            $"adres {PathOf(page)}; sayfadaki uyarı \"{warning}\"; gösterilen toplam \"{shownTotal}\", eski \"{after}\", sunucunun şimdiki \"{currentTotal}\"",
            onStep && orders == 0 && warning.Length > 0 && shownTotal.Length > 0 && shownTotal == currentTotal && shownTotal != after,
            $"orders (contact_email = maskeli): {orders} satır");

        // The delivery address and the payment method: with the script a change sends the form to the recalculation handler by itself; without it nothing is sent until "Hesapla", and the summary is recalculated there.
        WatchedPage fresh = await NewPageAsync(context);
        List<string> recalculations = [];
        fresh.Page.Request += (_, r) =>
        {
            if (r.Method == "POST" && r.Url.Contains("handler=Recalculate", StringComparison.Ordinal))
            {
                recalculations.Add(r.Url);
            }
        };
        await GoAsync(fresh, "/odeme");
        ILocator province = fresh.Page.Locator("select[name=ShippingProvinceId]");
        IReadOnlyList<string> options = await province.Locator("option").AllInnerTextsAsync();
        string chosenProvince = options.Count > 2 ? options[2].Trim() : options[^1].Trim();
        if (path == ScriptPath.Scripted)
        {
            await NavigateAsync(fresh, () => province.SelectOptionAsync(new SelectOptionValue { Label = chosenProvince }), 20000);
        }
        else
        {
            await province.SelectOptionAsync(new SelectOptionValue { Label = chosenProvince });
        }

        int afterProvince = recalculations.Count;
        string kept = await fresh.Page.Locator("select[name=ShippingProvinceId] option:checked").InnerTextAsync();
        if (path == ScriptPath.Scriptless)
        {
            await NavigateAsync(fresh, () => fresh.Page.Locator("button[data-js=recalculate]").ClickAsync());
        }

        string recalculatedTotal = await TextAsync(fresh.Page.Locator("aside .checkout__total strong"));
        string keptAfter = (await fresh.Page.Locator("select[name=ShippingProvinceId] option:checked").InnerTextAsync()).Trim();
        await ObserveAsync(
            "VAL-B099",
            path,
            engine,
            fresh,
            path == ScriptPath.Scripted ? $"teslimat ili \"{chosenProvince}\" seçilir" : $"teslimat ili \"{chosenProvince}\" seçilir, sonra Hesapla'ya basılır",
            path == ScriptPath.Scripted ? "il değişince form kendiliğinden hesap işleyicisine gönderilir ve özet sunucudan yenilenir" : "il değişince bir şey gönderilmez; özet Hesapla ile yenilenir",
            $"seçimden sonra giden hesap isteği {afterProvince}; toplam hesap isteği {recalculations.Count}; seçili il \"{kept.Trim()}\" -> \"{keptAfter}\"; toplam \"{recalculatedTotal}\"",
            (path == ScriptPath.Scripted ? afterProvince == 1 : afterProvince == 0 && recalculations.Count == 1) && keptAfter == chosenProvince && recalculatedTotal.Length > 0);

        // The payment method: the seed's installation has the bank transfer open only, so for this one observation the card line is opened too (two open methods) and the choice is changed by a real selection - no event is raised by hand.
        using IDisposable cardOpen = Host.OpenCard();
        WatchedPage methodPage = await NewPageAsync(context);
        List<string> methodRecalculations = [];
        methodPage.Page.Request += (_, r) =>
        {
            if (r.Method == "POST" && r.Url.Contains("handler=Recalculate", StringComparison.Ordinal))
            {
                methodRecalculations.Add(r.Url);
            }
        };
        await GoAsync(methodPage, "/odeme");
        ILocator card = methodPage.Page.Locator("#method-card");
        ILocator transfer = methodPage.Page.Locator("#method-banktransfer");
        bool twoOpen = await card.CountAsync() == 1 && await transfer.CountAsync() == 1 && !await card.IsDisabledAsync() && !await transfer.IsDisabledAsync();
        bool cardWasChosen = twoOpen && await card.IsCheckedAsync();
        if (path == ScriptPath.Scripted)
        {
            await NavigateAsync(methodPage, () => transfer.CheckAsync(), 20000);
        }
        else
        {
            await transfer.CheckAsync();
        }

        int afterMethod = methodRecalculations.Count;
        if (path == ScriptPath.Scriptless)
        {
            await NavigateAsync(methodPage, () => methodPage.Page.Locator("button[data-js=recalculate]").ClickAsync());
        }

        string methodTotal = await TextAsync(methodPage.Page.Locator("aside .checkout__total strong"));
        bool transferChecked = await methodPage.Page.Locator("#method-banktransfer").IsCheckedAsync();
        bool cardStillChecked = await methodPage.Page.Locator("#method-card").IsCheckedAsync();
        await ObserveAsync(
            "VAL-B099",
            path,
            engine,
            methodPage,
            path == ScriptPath.Scripted ? "iki açık yöntem varken ödeme yöntemi Kart'tan Havale'ye seçimle değiştirilir" : "iki açık yöntem varken ödeme yöntemi Kart'tan Havale'ye seçimle değiştirilir, sonra Hesapla'ya basılır",
            path == ScriptPath.Scripted ? "yöntem değişince form kendiliğinden hesap işleyicisine gönderilir ve seçim korunur" : "yöntem değişince bir şey gönderilmez; özet Hesapla ile yenilenir ve seçim korunur",
            $"iki açık yöntem {twoOpen}; önce kart seçili {cardWasChosen}; seçimden sonra giden hesap isteği {afterMethod}; toplam hesap isteği {methodRecalculations.Count}; havale seçili {transferChecked}, kart seçili {cardStillChecked}; toplam \"{methodTotal}\"",
            twoOpen && cardWasChosen && (path == ScriptPath.Scripted ? afterMethod == 1 : afterMethod == 0 && methodRecalculations.Count == 1) && transferChecked && !cardStillChecked && methodTotal.Length > 0);
        Assert.Empty(Failures);
    }

    /// <summary>VAL-B105: a double send - the script stops the second send and the button says it is going on; without it the order's second send goes to the existing order's confirmation and the contact request is written twice.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheDoubleSend_IsStoppedByTheScriptAndByTheOrdersKey_VALB105(string engine, ScriptPath path)
    {
        string email = $"b105-{Unique(engine, path)}@example.com";
        await using IBrowserContext context = await Host.ContextAsync(engine, path);
        WatchedPage page = await NewPageAsync(context);
        List<IRequest> sends = [];
        page.Page.Request += (_, r) =>
        {
            if (r.Method == "POST" && r.Url.Contains("handler=PlaceOrder", StringComparison.Ordinal))
            {
                sends.Add(r);
            }
        };
        await AddToCartAsync(page, Host.Seed.Product("plain").Slug);
        await GoAsync(page, "/odeme");
        await FillCheckoutAsync(page, path, email, true);
        string busy = string.Empty;
        bool labelSaysBusy = path == ScriptPath.Scriptless;
        if (path == ScriptPath.Scripted)
        {
            string original = await TextAsync(page.Page.Locator(".checkout__confirm button.button--primary"));
            string[] state = await page.Page.EvaluateAsync<string[]>("() => { const f = document.querySelector('form[data-js=checkout]'); const b = f.querySelector('.checkout__confirm button.button--primary'); f.requestSubmit(b); f.requestSubmit(b); return [b.textContent.trim(), f.getAttribute('aria-busy') ?? '']; }");
            busy = $"düğme \"{original}\" iken \"{state[0]}\", aria-busy \"{state[1]}\"";
            labelSaysBusy = state[0].Length > 0 && state[0] != original;
            await page.Page.WaitForURLAsync(u => u.Contains("/teyit", StringComparison.Ordinal), new PageWaitForURLOptions { Timeout = 25000 });
        }
        else
        {
            await page.Page.Locator(".checkout__confirm button.button--primary[data-js=submit-once]").ClickAsync();
            await page.Page.WaitForURLAsync(u => u.Contains("/teyit", StringComparison.Ordinal), new PageWaitForURLOptions { Timeout = 25000 });
        }

        string firstAddress = PathOf(page);
        // The same form sent once more with the same key (the second send of the page): the request is replayed as the browser would send it.
        IAPIResponse again = await ReplayAsync(context, sends[0]);
        string location = again.Headers.TryGetValue("location", out string? target) ? target : string.Empty;
        long orders = await DataRecord.CountAsync(Host.Dsn, "SELECT count(*) FROM orders WHERE contact_email = @e", ("e", email));
        bool stopped = path == ScriptPath.Scriptless || (sends.Count == 1 && busy.Contains("aria-busy \"true\"", StringComparison.Ordinal) && labelSaysBusy);
        await ObserveAsync(
            "VAL-B105",
            path,
            engine,
            page,
            path == ScriptPath.Scripted ? "ödeme adımında iki gönderim art arda (aynı anda requestSubmit), sonra aynı istek yeniden gönderilir" : "ödeme adımı onaylanır, sonra aynı istek (aynı onay anahtarı) yeniden gönderilir",
            "betikli yolda ikinci submit durdurulur ve düğme sürdüğünü söyler; ikinci gönderim var olan siparişin teyit ekranına gider; yeni sipariş oluşmaz",
            $"{(busy.Length > 0 ? busy + "; " : string.Empty)}ağa giden gönderim {sends.Count}; yeniden gönderim {again.Status} -> {location}; teyit ekranı {firstAddress}",
            stopped && orders == 1 && location.Contains("/teyit", StringComparison.Ordinal) && firstAddress.Split('?')[0] == new Uri(new Uri(Host.BaseUrl), location).AbsolutePath,
            $"orders (contact_email = maskeli): {orders} satır");

        // The contact request and the add to cart are not compared with the current state: a second send is a second record.
        string marker = $"b105-{Unique(engine, path)}-iletisim";
        await GoAsync(page, "/iletisim");
        List<IRequest> contactSends = [];
        page.Page.Request += (_, r) =>
        {
            if (r.Method == "POST" && r.Url.Contains("SendContactRequest", StringComparison.Ordinal))
            {
                contactSends.Add(r);
            }
        };
        await page.Page.Locator("[name='Input.Name']").FillAsync("Ayşe Yılmaz");
        await page.Page.Locator("[name='Input.Email']").FillAsync(email);
        await page.Page.Locator("[name='Input.ContactTopic']").SelectOptionAsync(new SelectOptionValue { Index = 1 });
        await page.Page.Locator("[name='Input.Message']").FillAsync(marker);
        string contactBusy = string.Empty;
        if (path == ScriptPath.Scripted)
        {
            string[] state = [];
            await NavigateAsync(page, async () => state = await page.Page.EvaluateAsync<string[]>("() => { const f = document.querySelector('form.contact-form'); const b = f.querySelector('button[type=submit]'); f.requestSubmit(b); f.requestSubmit(b); return [b.textContent.trim(), f.getAttribute('aria-busy') ?? '']; }"));
            contactBusy = $"düğme \"{state[0]}\", aria-busy \"{state[1]}\"";
        }
        else
        {
            await NavigateAsync(page, () => page.Page.Locator("form.contact-form button[type=submit]").ClickAsync());
            await ReplayAsync(context, contactSends[0]);
        }

        long written = await DataRecord.CountAsync(Host.Dsn, "SELECT count(*) FROM contact_requests WHERE message = @m", ("m", marker));
        bool contactOk = path == ScriptPath.Scripted ? written == 1 && contactSends.Count == 1 : written == 2;
        await ObserveAsync(
            "VAL-B105",
            path,
            engine,
            page,
            path == ScriptPath.Scripted ? "iletişim formu art arda iki kez gönderilir" : "iletişim formu gönderilir, aynı istek yeniden gönderilir",
            path == ScriptPath.Scripted ? "ikinci submit durdurulur: tek talep yazılır" : "güncel hâlle karşılaştırılmayan işlemde tekrar gönderim ikinci kez işlenir: iki talep",
            $"{(contactBusy.Length > 0 ? contactBusy + "; " : string.Empty)}ağa giden gönderim {contactSends.Count}; yazılan talep {written}",
            contactOk,
            $"contact_requests (message = {marker}): {written} satır");

        // The add to cart is not compared with the current state either (07 8.2.2): without the script the same send is processed again and the line holds two units.
        if (path == ScriptPath.Scriptless)
        {
            List<IRequest> cartSends = [];
            page.Page.Request += (_, r) =>
            {
                if (r.Method == "POST" && r.Url.Contains("AddCartItem", StringComparison.Ordinal))
                {
                    cartSends.Add(r);
                }
            };
            await GoAsync(page, $"/urun/{Host.Seed.Product("second").Slug}");
            await NavigateAsync(page, () => page.Page.Locator("form.buy-form button[type=submit]").First.ClickAsync());
            await ReplayAsync(context, cartSends[0]);
            await GoAsync(page, "/sepet");
            string quantity = await page.Page.Locator(".cart-line", new PageLocatorOptions { HasText = "İkinci ürün" }).Locator("input[name=Quantity]").First.InputValueAsync();
            await ObserveAsync(
                "VAL-B105",
                path,
                engine,
                page,
                "ürün sayfasında sepete ekle gönderilir, aynı istek yeniden gönderilir",
                "güncel hâlle karşılaştırılmayan işlemde tekrar gönderim ikinci kez işlenir: satırda iki adet",
                $"ağa giden gönderim {cartSends.Count}; sepet satırı adedi {quantity}",
                cartSends.Count == 1 && quantity == "2");
        }

        Assert.Empty(Failures);
    }

    private static async Task<IAPIResponse> ReplayAsync(IBrowserContext context, IRequest request)
    {
        Dictionary<string, string> original = await request.AllHeadersAsync();
        Dictionary<string, string> headers = new()
        {
            ["content-type"] = "application/x-www-form-urlencoded",
            ["x-forwarded-proto"] = "https",
            ["x-forwarded-for"] = original.GetValueOrDefault("x-forwarded-for", "203.0.113.9"),
        };
        return await context.APIRequest.PostAsync(request.Url, new APIRequestContextOptions { DataByte = request.PostDataBuffer, Headers = headers, MaxRedirects = 0, IgnoreHTTPSErrors = true });
    }

    /// <summary>VAL-B106: the copy button works with the script; without it the value is selectable text on the page and the shared address is the page's address.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheCopyButton_WorksWithTheScript_AndTheValueIsTextWithoutIt_VALB106(string engine, ScriptPath path)
    {
        await using IBrowserContext context = await Host.ContextAsync(engine, path);
        if (string.Equals(engine, "chromium", StringComparison.Ordinal))
        {
            // Only Chromium has the clipboard permissions; elsewhere a click is a user action and the copy is allowed by it (a refused grant leaves a WebKit context unusable).
            await context.GrantPermissionsAsync(["clipboard-read", "clipboard-write"]);
        }

        WatchedPage page = await NewPageAsync(context);
        await GoAsync(page, $"/siparis/erisim/{Host.Seed.Order("awaiting-transfer").AccessKey}/teyit");
        ILocator button = page.Page.Locator("button[data-copy]");
        bool visible = await button.CountAsync() > 0 && await button.First.IsVisibleAsync();
        string iban = await TextAsync(page.Page.Locator(".order-confirmation__iban"));
        if (path == ScriptPath.Scripted)
        {
            await button.First.ClickAsync();
            string result = await TextMatchingAsync(page.Page.Locator("[data-copy-result]"), "kopyaland", 5000);
            await ObserveAsync("VAL-B106", path, engine, page, "IBAN'ı kopyala düğmesine basılır", "düğme görünür; kopyalama çalışır ve sonuç cümlesi (IBAN kopyalandı) gösterilir", $"düğme görünür: {visible}; sonuç \"{result}\"", visible && result.Contains("kopyaland", StringComparison.Ordinal));
        }
        else
        {
            bool selectable = await page.Page.EvaluateAsync<bool>("() => { const e = document.querySelector('.order-confirmation__iban'); return !!e && getComputedStyle(e).userSelect !== 'none'; }", null);
            await ObserveAsync("VAL-B106", path, engine, page, "teyit ekranı betiksiz açılır", "düğme yoktur (gizli); IBAN sayfada seçilebilir metin olarak durur; paylaşılacak adres sayfanın adresidir", $"düğme görünür: {visible}; IBAN metni \"{DataRecord.Mask(iban)}\"; seçilebilir: {selectable}; adres {PathOf(page)}", !visible && selectable && iban.Contains("TR", StringComparison.Ordinal));
        }

        Assert.Empty(Failures);
    }

    private static async Task<string> TextMatchingAsync(ILocator locator, string pattern, int milliseconds)
    {
        try
        {
            await Assertions.Expect(locator.First).ToHaveTextAsync(
                new Regex(pattern, RegexOptions.None, TimeSpan.FromSeconds(2)),
                new LocatorAssertionsToHaveTextOptions { UseInnerText = true, Timeout = milliseconds });
        }
        catch (PlaywrightException)
        {
            // The time is over without a match: the text last read is returned and the row's observation records the difference.
        }

        return await TextAsync(locator);
    }

    // ---- the customer's own screens ----

    /// <summary>VAL-B097: the customer's item step - the first send shows the list the server computed and the result sentence, the same in both ways.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheItemStep_ShowsTheServersListAndSentence_InBothWays_VALB097(string engine, ScriptPath path)
    {
        SeededOrder order = await BrowserSeed.NewOrderAsync(Host.Dsn, $"b097-{engine}-{path}", OrderStatus.Delivered, PaymentStatus.Paid);
        await using IBrowserContext context = await Host.ContextAsync(engine, path);
        WatchedPage page = await NewPageAsync(context);
        await GoAsync(page, $"/siparis/erisim/{order.AccessKey}/cayma");
        await page.Page.Locator("input[type=checkbox][name='Items[0].OrderItemId']").CheckAsync();
        await page.Page.Locator("input[name='Items[0].Quantity']").FillAsync("1");
        await page.Page.Locator("input[name*=Iban i]").First.FillAsync("TR330006100519786457841326");
        await NavigateAsync(page, () => page.Page.Locator("form.operation-screen__form button[type=submit]").ClickAsync());
        string chosen = await TextAsync(page.Page.Locator("#withdrawal-chosen"));
        string sentence = await TextAsync(page.Page.Locator(".confirm--step"));
        await ObserveAsync("VAL-B097", path, engine, page, "iki adetli kalem seçilir, adet 1 yazılır, ilk gönderim", "son adımda sunucunun yazdığı kalem listesi (Kapı — 1 adet) ve sonuç cümlesi; iki yolda aynı", $"liste \"{chosen}\"; son adım \"{sentence[..Math.Min(sentence.Length, 160)]}\"", chosen.Contains("Kapı", StringComparison.Ordinal) && chosen.Contains("1 adet", StringComparison.Ordinal) && sentence.Length > 0);
        Assert.Empty(Failures);
    }

    /// <summary>VAL-B108: the address book's add and edit forms are opened by the buttons' own addresses (<c>create=true</c>, <c>addressBookId</c>), the save is the same handler.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheAddressBookForms_OpenByTheirAddresses_InBothWays_VALB108(string engine, ScriptPath path)
    {
        await using IBrowserContext context = await Host.ContextAsync(engine, path, Actor.Customer);
        WatchedPage page = await NewPageAsync(context);
        await GoAsync(page, "/hesap/adresler");
        await NavigateAsync(page, () => page.Page.Locator("a[href*='create=true']").First.ClickAsync());
        string addAddress = PathOf(page);
        bool addForm = await page.Page.Locator("#address-form").CountAsync() > 0;
        string name = $"Ev {Unique(engine, path)}";
        await page.Page.Locator("[name='Input.Name']").FillAsync(name);
        await page.Page.Locator("[name='Input.RecipientName']").FillAsync("Ali Veli");
        await page.Page.Locator("select[name='Input.ProvinceId']").SelectOptionAsync(new SelectOptionValue { Label = "Ankara" });
        await page.Page.Locator("[name='Input.District']").FillAsync("Çankaya");
        await page.Page.Locator("[name='Input.AddressLine']").FillAsync("Kızılay Mah. 2");
        await page.Page.Locator("[name='Input.Phone']").FillAsync("5321234567");
        await NavigateAsync(page, () => page.Page.Locator("#address-form button[type=submit]").ClickAsync());
        ILocator card = page.Page.Locator("li.address-card", new PageLocatorOptions { HasText = name });
        await NavigateAsync(page, () => card.Locator("a[href*='addressBookId=']").First.ClickAsync());
        string editAddress = PathOf(page);
        string editedName = await page.Page.Locator("[name='Input.Name']").InputValueAsync();
        string row = await DataRecord.RowAsync(Host.Dsn, "SELECT name, district, address_line FROM address_books WHERE name = @n", ("n", name));
        await ObserveAsync("VAL-B108", path, engine, page, "Yeni adres bağlantısı, kayıt, sonra Düzenle bağlantısı", "ekleme create=true, düzenleme addressBookId parametresiyle açılır; kayıt aynı işleyicide; form düzenlemede kayıtlı değerle dolu", $"ekleme adresi {addAddress} (form {addForm}); düzenleme adresi {editAddress}; düzenleme formunda ad \"{editedName}\"", addAddress.Contains("create=true", StringComparison.Ordinal) && addForm && editAddress.Contains("addressBookId=", StringComparison.Ordinal) && editedName == name, row);
        Assert.Empty(Failures);
    }

    /// <summary>VAL-B110: the account screens' basic flows complete by form sends - the registration, the sign-in, the password reset's request and the order tracking - and each shows its result page.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheAccountFlows_CompleteByFormSends_InBothWays_VALB110(string engine, ScriptPath path)
    {
        string id = Unique(engine, path);
        await using IBrowserContext context = await Host.ContextAsync(engine, path);
        WatchedPage page = await NewPageAsync(context);

        await GoAsync(page, "/kayit");
        await page.Page.Locator("[name=Name]").FillAsync("Ayşe Yılmaz");
        await page.Page.Locator("[name=Email]").FillAsync($"b110-{id}@example.com");
        await page.Page.Locator("[name=Password]").FillAsync("T551-Deneme-Sifre-91!");
        await NavigateAsync(page, () => page.Page.Locator("#registration-form button[type=submit]").ClickAsync());
        string registration = await TextAsync(page.Page.Locator("main"));
        bool registered = await page.Page.Locator("[data-js=error-summary], .field--invalid").CountAsync() == 0 && registration.Length > 0;
        await ObserveAsync("VAL-B110", path, engine, page, "kayıt formu doldurulur ve gönderilir", "kayıt gönderimi tamamlanır; sonuç sayfası (doğrulama bağlantısı gönderildi) gösterilir, alan hatası yoktur", $"adres {PathOf(page)}; sonuç \"{registration[..Math.Min(registration.Length, 140)]}\"", registered);

        await GoAsync(page, "/giris");
        await page.Page.Locator("[name='Input.Email']").FillAsync($"b110-yok-{id}@example.com");
        await page.Page.Locator("[name='Input.Password']").FillAsync("T551-Yanlis-Sifre-1!");
        await NavigateAsync(page, () => page.Page.Locator("#sign-in-form button[type=submit]").ClickAsync());
        string signIn = await TextAsync(page.Page.Locator("main"));
        await ObserveAsync("VAL-B110", path, engine, page, "giriş formu bilinmeyen hesapla gönderilir", "gönderim tamamlanır; sonuç sayfası aynı giriş ekranında nötr ret cümlesiyle gösterilir (hesabın var olup olmadığı söylenmez)", $"adres {PathOf(page)}; sonuç \"{signIn[..Math.Min(signIn.Length, 140)]}\"", PathOf(page).StartsWith("/giris", StringComparison.Ordinal) && signIn.Length > 0);

        // The sign-in with an account that exists: it completes, leaves the sign-in screen and the session is real (the account page opens).
        string member = $"b110-uye-{id}@example.com";
        await BrowserSeed.NewCustomerWithPasswordAsync(Host.Dsn, member, "T551-Giris-Sifre-77!");
        await using IBrowserContext memberContext = await Host.ContextAsync(engine, path);
        WatchedPage memberPage = await NewPageAsync(memberContext);
        await GoAsync(memberPage, "/giris");
        await memberPage.Page.Locator("[name='Input.Email']").FillAsync(member);
        await memberPage.Page.Locator("[name='Input.Password']").FillAsync("T551-Giris-Sifre-77!");
        await NavigateAsync(memberPage, () => memberPage.Page.Locator("#sign-in-form button[type=submit]").ClickAsync());
        string landed = PathOf(memberPage);
        long sessions = await DataRecord.CountAsync(Host.Dsn, "SELECT count(*) FROM sessions s JOIN customers c ON c.id = s.customer_id WHERE c.email = @e", ("e", member));
        await GoAsync(memberPage, "/hesap");
        string accountPath = PathOf(memberPage);
        await ObserveAsync("VAL-B110", path, engine, memberPage, "giriş formu kayıtlı hesabın e-postası ve şifresiyle gönderilir, sonra hesap sayfası açılır", "gönderim tamamlanır, giriş ekranından çıkılır, oturum açılır ve hesap sayfası gösterilir", $"gönderimden sonraki adres {landed}; oturum kaydı {sessions}; hesap sayfası adresi {accountPath}", !landed.StartsWith("/giris", StringComparison.Ordinal) && sessions == 1 && accountPath.StartsWith("/hesap", StringComparison.Ordinal));

        await GoAsync(page, "/sifre-sifirlama");
        await page.Page.Locator("[name='Input.Email']").FillAsync($"b110-yok-{id}@example.com");
        await NavigateAsync(page, () => page.Page.Locator("#reset-request-form button[type=submit]").ClickAsync());
        string reset = await TextAsync(page.Page.Locator("main"));
        await ObserveAsync("VAL-B110", path, engine, page, "şifre sıfırlama isteği gönderilir", "gönderim tamamlanır; sonuç sayfası bağlantının gönderildiğini söyler (hesap var olsa da olmasa da aynı cümle)", $"adres {PathOf(page)}; sonuç \"{reset[..Math.Min(reset.Length, 140)]}\"", reset.Length > 0 && await page.Page.Locator("[data-js=error-summary]").CountAsync() == 0);

        SeededOrder order = Host.Seed.Order("awaiting-transfer");
        await GoAsync(page, "/siparis-takibi");
        await page.Page.Locator("[name='Input.OrderNumber']").FillAsync(order.Number);
        await page.Page.Locator("[name='Input.ContactEmail']").FillAsync("alici@example.com");
        await NavigateAsync(page, () => page.Page.Locator("form button[type=submit]").Last.ClickAsync());
        string tracked = await TextMatchingAsync(page.Page.Locator("main"), Regex.Escape(order.Number), 15000);
        await ObserveAsync("VAL-B110", path, engine, page, "sipariş takibi: numara ve e-posta gönderilir", "sipariş sayfası açılır ve sipariş numarasını gösterir", $"adres {PathOf(page)}; sayfa \"{tracked[..Math.Min(tracked.Length, 120)]}\"", PathOf(page).StartsWith("/siparis/", StringComparison.Ordinal) && tracked.Contains(order.Number, StringComparison.Ordinal));
        Assert.DoesNotContain(Host.Record.Snapshot().Where(o => o.Row == "VAL-B110" && o.Engine == engine), o => o.Observed.Contains($"b110-{id}@", StringComparison.Ordinal) || o.Observed.Contains(order.AccessKey, StringComparison.Ordinal));
        Assert.Empty(Failures);
    }

    // ---- the panel's forms ----

    /// <summary>VAL-B111: the panel's forms are saved with the script off - the product, the category, the content, the order's internal note and a setting - each by its own handler, and the saved value is read back.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task ThePanelForms_AreSaved_InBothWays_VALB111(string engine, ScriptPath path)
    {
        string id = Unique(engine, path);
        SeededProduct product = await BrowserSeed.NewProductAsync(Host.Dsn, $"p{id}", ProductType.Physical);
        await using IBrowserContext context = await Host.ContextAsync(engine, path, Actor.Admin);
        WatchedPage page = await NewPageAsync(context);

        string productName = $"Kayıtlı ürün {id}";
        await GoAsync(page, $"/panel/urunler/{product.ProductId}");
        await page.Page.Locator("[name='Input.Name']").FillAsync(productName);
        await NavigateAsync(page, () => page.Page.Locator("#product-save").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Kaydet", Exact = true }).ClickAsync());
        string productRow = await DataRecord.RowAsync(Host.Dsn, "SELECT name FROM products WHERE id = @i", ("i", product.ProductId));
        await ObserveAsync("VAL-B111", path, engine, page, "ürün formunda ad değiştirilir ve Kaydet'e basılır", "ürün kendi işleyicisiyle kaydedilir; ad satırda yeni değerdir", $"adres {PathOf(page)}", productRow.Contains(productName, StringComparison.Ordinal), productRow);

        string categoryName = $"Kategori {id}";
        await GoAsync(page, "/panel/kategoriler");
        await page.Page.Locator($"#{CategoryFormId}").Locator("[name='Input.Name']").FillAsync(categoryName);
        await NavigateAsync(page, () => page.Page.Locator($"#{CategoryFormId}").GetByRole(AriaRole.Button).First.ClickAsync());
        string categoryRow = await DataRecord.RowAsync(Host.Dsn, "SELECT name FROM categories WHERE name = @n", ("n", categoryName));
        await ObserveAsync("VAL-B111", path, engine, page, "kategori ağacında yeni kategori adı yazılır ve eklenir", "kategori kendi işleyicisiyle kaydedilir", $"adres {PathOf(page)}", categoryRow.Contains(categoryName, StringComparison.Ordinal), categoryRow);

        string contentName = $"Bakım hizmeti {id}";
        Guid offeringId = await BrowserSeed.NewOfferingAsync(Host.Dsn, id);
        await GoAsync(page, $"/panel/kurumsal-icerik/hizmet/{offeringId}");
        await page.Page.Locator("[name='Input.Name']").FillAsync(contentName);
        await NavigateAsync(page, () => page.Page.Locator("#content-save").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Kaydet", Exact = true }).ClickAsync());
        string contentRow = await DataRecord.RowAsync(Host.Dsn, "SELECT name FROM offerings WHERE id = @i", ("i", offeringId));
        await ObserveAsync("VAL-B111", path, engine, page, "hizmet içeriğinin adı değiştirilir ve Kaydet'e basılır", "içerik kendi işleyicisiyle kaydedilir", $"adres {PathOf(page)}", contentRow.Contains(contentName, StringComparison.Ordinal), contentRow);

        SeededOrder order = Host.Seed.Order("paid-open");
        string note = $"İç not {id}";
        await GoAsync(page, $"/panel/siparisler/{order.Id}");
        await page.Page.Locator("#internal-note-text").FillAsync(note);
        await NavigateAsync(page, () => page.Page.Locator("#internal-note-form button[type=submit]").ClickAsync());
        string shownNote = await page.Page.Locator("#internal-note-text").InputValueAsync();
        await ObserveAsync("VAL-B111", path, engine, page, "sipariş ayrıntısında iç not yazılır ve kaydedilir", "sipariş işlemi kendi işleyicisiyle kaydedilir; not sayfada yeniden okunur", $"adres {PathOf(page)}; not alanında \"{shownNote}\"", shownNote == note);

        string window = path == ScriptPath.Scripted ? "3" : "1";
        await GoAsync(page, "/panel/kargo-sureler-ve-varsayilanlar");
        await page.Page.Locator("[name='Input.BankTransferPaymentWindow']").FillAsync(window);
        await NavigateAsync(page, () => page.Page.Locator("form:has([name='Input.BankTransferPaymentWindow'])").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Kaydet", Exact = true }).ClickAsync());
        string settingRow = await DataRecord.RowAsync(Host.Dsn, "SELECT bank_transfer_payment_window FROM company_settings LIMIT 1");
        await ObserveAsync("VAL-B111", path, engine, page, $"kargo ve süreler ekranında havale ödeme süresi {window} yazılır ve kaydedilir", "ayar kendi işleyicisiyle kaydedilir", $"adres {PathOf(page)}", settingRow.Contains($"bank_transfer_payment_window={window}", StringComparison.Ordinal), settingRow);
        Assert.Empty(Failures);
    }

    private const string CategoryFormId = "yeni-kategori";

    /// <summary>VAL-B102: the product form's option, value and variant rows - added in place with the script, the scriptless GET holds blank rows and a blank row is not read; the new option value enters the variant row's choice after the save.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheProductFormsRows_AreAddedInBothWays_AndABlankRowIsNotRead_VALB102(string engine, ScriptPath path)
    {
        string id = Unique(engine, path);
        SeededProduct product = await BrowserSeed.NewProductWithoutVariantAsync(Host.Dsn, $"p{id}", ProductType.Physical);
        await using IBrowserContext context = await Host.ContextAsync(engine, path, Actor.Admin);
        WatchedPage page = await NewPageAsync(context);
        await GoAsync(page, $"/panel/urunler/{product.ProductId}");
        int blankOptions = await page.Page.Locator("[name^='Input.Options['][name$='].Name']").CountAsync();
        int addedValues = 1;
        if (path == ScriptPath.Scripted)
        {
            await page.Page.Locator("button[data-template=product-option-template]").ClickAsync();
            await page.Page.Locator("button[data-js=row-add][data-list^=product-option-][data-list$=-values]").First.ClickAsync();
            addedValues = 2;
        }

        int rowsOnPage = await page.Page.Locator("[name^='Input.Options['][name$='].Name']").CountAsync();
        await page.Page.Locator("[name='Input.Options[0].Name']").FillAsync("Malzeme");
        await page.Page.Locator("[name='Input.Options[0].Values[0].Value']").FillAsync("Ahşap");
        if (path == ScriptPath.Scripted)
        {
            await page.Page.Locator("[name='Input.Options[0].Values[1].Value']").FillAsync("Metal");
        }

        await NavigateAsync(page, () => page.Page.Locator("#product-save").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Kaydet", Exact = true }).ClickAsync());
        string saveMessage = await TextAsync(page.Page.Locator("[data-js=error-summary], .message"));
        string saveAddress = PathOf(page);
        long options = await DataRecord.CountAsync(Host.Dsn, "SELECT count(*) FROM options WHERE product_id = @p", ("p", product.ProductId));
        long values = await DataRecord.CountAsync(Host.Dsn, "SELECT count(*) FROM option_values v JOIN options o ON o.id = v.option_id WHERE o.product_id = @p", ("p", product.ProductId));
        string optionRow = await DataRecord.RowAsync(Host.Dsn, "SELECT o.name AS option_name, v.value AS first_value FROM options o JOIN option_values v ON v.option_id = o.id WHERE o.product_id = @p ORDER BY v.sort_order LIMIT 1", ("p", product.ProductId));
        await GoAsync(page, $"/panel/urunler/{product.ProductId}");
        int inChoice = await page.Page.Locator("select[name*='FirstOptionValueId'] option", new PageLocatorOptions { HasText = "Ahşap" }).CountAsync();
        await ObserveAsync(
            "VAL-B102",
            path,
            engine,
            page,
            path == ScriptPath.Scripted ? "Seçenek ekle ve Değer ekle düğmeleriyle satırlar eklenir; ilk seçenek (Malzeme: Ahşap, Metal) doldurulur, eklenen ikinci seçenek boş bırakılır; Kaydet" : "GET'in boş seçenek ve değer satırı doldurulur (Malzeme: Ahşap); Kaydet",
            "eklenen satır aynı UC-114 gönderimiyle kaydedilir; boş bırakılan satır kayıt oluşturmaz; yeni seçenek değeri varyant satırının seçimine girer",
            $"sayfada seçenek satırı {blankOptions} -> {rowsOnPage}; kayıtlı seçenek {options}, değer {values}; varyant seçiminde Ahşap {inChoice} kez; kayıttan sonraki adres {saveAddress}; mesaj \"{saveMessage[..Math.Min(saveMessage.Length, 160)]}\"",
            options == 1 && values == addedValues && inChoice > 0,
            optionRow);

        // The variant row: added in place with the script (a second row is left blank), the GET's blank row without it; the filled row is saved with the new option value chosen, the blank one is not read.
        int variantRowsBefore = await page.Page.Locator("[name^='Input.Variants['][name$='].Price']").CountAsync();
        if (path == ScriptPath.Scripted)
        {
            await page.Page.Locator("button[data-js=row-add][data-template=product-variant-template]").ClickAsync();
        }

        int variantRowsAfter = await page.Page.Locator("[name^='Input.Variants['][name$='].Price']").CountAsync();
        string stockCode = $"B102{id}"[..Math.Min(20, $"B102{id}".Length)].ToUpperInvariant();
        await page.Page.Locator("select[name='Input.Variants[0].FirstOptionValueId']").SelectOptionAsync(new SelectOptionValue { Label = "Ahşap" });
        await page.Page.Locator("[name='Input.Variants[0].SKU']").FillAsync(stockCode);
        await page.Page.Locator("[name='Input.Variants[0].Price']").FillAsync("75");
        await page.Page.Locator("[name='Input.Variants[0].StockQuantity']").FillAsync("5");
        await NavigateAsync(page, () => page.Page.Locator("#product-save").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Kaydet", Exact = true }).ClickAsync());
        string variantMessage = await TextAsync(page.Page.Locator("[data-js=error-summary], .message"));
        long variants = await DataRecord.CountAsync(Host.Dsn, "SELECT count(*) FROM variants WHERE product_id = @p", ("p", product.ProductId));
        string variantRow = await DataRecord.RowAsync(
            Host.Dsn,
            "SELECT v.price, v.stock_quantity, ov.value AS first_option_value FROM variants v LEFT JOIN option_values ov ON ov.id = v.first_option_value_id WHERE v.product_id = @p",
            ("p", product.ProductId));
        await ObserveAsync(
            "VAL-B102",
            path,
            engine,
            page,
            path == ScriptPath.Scripted ? "Varyant ekle düğmesiyle ikinci varyant satırı eklenir; ilk satırda Ahşap seçilir, stok kodu, fiyat ve stok yazılır; ikinci satır boş bırakılır; Kaydet" : "GET'in boş varyant satırında Ahşap seçilir, stok kodu, fiyat ve stok yazılır; Kaydet",
            "varyant satırı aynı UC-114 gönderimiyle kaydedilir; boş bırakılan satır kayıt oluşturmaz; yeni seçenek değeri varyantın seçimidir",
            $"sayfada varyant satırı {variantRowsBefore} -> {variantRowsAfter}; kayıtlı varyant {variants}; mesaj \"{variantMessage[..Math.Min(variantMessage.Length, 160)]}\"",
            variants == 1 && variantRowsAfter == (path == ScriptPath.Scripted ? variantRowsBefore + 1 : variantRowsBefore) && variantRow.Contains("first_option_value=Ahşap", StringComparison.Ordinal),
            variantRow);
        Assert.Empty(Failures);
    }

    /// <summary>
    /// VAL-B101: the image upload of the product form - three files: within P-23, over P-23 by less than 1 MB, over the body ceiling. With the script on, the file over P-23 sends no request and the
    /// sentence is shown at the field, and a file within P-23 shows its progress; with the script off each send carries one image and the sent file gets its 422 field message or its 413. The expected
    /// texts are the ones 12 VAL-B101 gives; the sentence compared is the server's own (the limit comes to the page from the constant).
    /// </summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheImageUpload_ChecksTheSizeAtTheChoice_AndCarriesOneImagePerSend_InBothWays_VALB101(string engine, ScriptPath path)
    {
        string id = Unique(engine, path);
        SeededProduct product = await BrowserSeed.NewProductAsync(Host.Dsn, $"p{id}", ProductType.Physical);
        await using IBrowserContext context = await Host.ContextAsync(engine, path, Actor.Admin);
        WatchedPage page = await NewPageAsync(context);
        int imageMax = checked((int)ProductConstants.ImageMaxBytes);
        (string Label, string Name, byte[] Bytes, int Expected)[] files =
        [
            ("P-23'ü aşmayan", "kucuk.png", MakePng(), 200),
            ("P-23'ü bir megabayttan az aşan", "orta.png", new byte[imageMax + (100 * 1024)], 422),
            ("gövdeyi tavanın üstüne taşıyan", "buyuk.png", new byte[checked((int)RequestBodyLimits.ImageUploadMaxBytes) + (1024 * 1024)], 413),
        ];
        string sizeMessage = UploadChecks.SizeMessage(UploadSurface.ProductGallery);
        ILocator fileState = page.Page.Locator("#product-image-file + [data-js=file-state]");
        ILocator uploadButton = page.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Galeriye yükle" });

        // The send replaces the page, so the progress the script shows while it goes is kept by a script that runs in every page: the field's text and the upload button's aria-disabled, in the session.
        await context.AddInitScriptAsync("(() => { const note = () => { const s = document.querySelector('#product-image-file + [data-js=file-state]'); if (s && s.dataset.state === 'busy') { const b = document.querySelector('button[name=\"Input.ImageTarget\"]'); sessionStorage.setItem('t551Progress', s.textContent + '|' + (b ? b.getAttribute('aria-disabled') : '')); } }; new MutationObserver(note).observe(document, { subtree: true, childList: true, characterData: true, attributes: true }); })();");
        foreach ((string label, string name, byte[] bytes, int expected) in files)
        {
            await GoAsync(page, $"/panel/urunler/{product.ProductId}");
            int fileInputs = await page.Page.Locator("#product-image-file").CountAsync();
            string step = $"üründe {label} görsel ({bytes.Length / 1024} KB) seçilir ve Galeriye yükle'ye basılır";
            if (path == ScriptPath.Scripted && expected != 200)
            {
                int posts = 0;
                void CountPosts(object? sender, IRequest request)
                {
                    if (request.Method == "POST")
                    {
                        Interlocked.Increment(ref posts);
                    }
                }

                page.Page.Request += CountPosts;
                string addressBefore = PathOf(page);
                await page.Page.Locator("#product-image-file").SetInputFilesAsync(new FilePayload { Name = name, MimeType = "image/png", Buffer = bytes });
                string atChoice = await TextMatchingAsync(fileState, Regex.Escape(sizeMessage), 10000);
                await uploadButton.ClickAsync();
                await page.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                string afterClick = await TextAsync(fileState);
                page.Page.Request -= CountPosts;
                await ObserveAsync(
                    "VAL-B101",
                    path,
                    engine,
                    page,
                    step,
                    "aşan dosya için istek gitmez ve mesaj gösterilir (sunucunun boyut cümlesi alanın yanında durur; Galeriye yükle bir şey göndermez)",
                    $"gönderilen POST {posts}; seçimde mesaj \"{atChoice}\"; düğmeden sonra mesaj \"{afterClick}\"; adres değişmedi: {PathOf(page) == addressBefore}",
                    fileInputs == 1 && posts == 0 && atChoice == sizeMessage && afterClick == sizeMessage && PathOf(page) == addressBefore,
                    "uç yanıtı: yok (istek gitmedi)");
                continue;
            }

            await page.Page.Locator("#product-image-file").SetInputFilesAsync(new FilePayload { Name = name, MimeType = "image/png", Buffer = bytes });
            IResponse response = null!;
            await NavigateAsync(
                page,
                async () => response = await page.Page.RunAndWaitForResponseAsync(
                    () => uploadButton.ClickAsync(),
                    r => r.Request.Method == "POST",
                    new PageRunAndWaitForResponseOptions { Timeout = 60000 }),
                60000);
            string message = await TextAsync(page.Page.Locator(".field__message, .message, [data-js=error-summary]"));

            // The progress the script showed while the send went is what the init script kept in the session (the page that showed it was replaced).
            string progressNote = await page.Page.EvaluateAsync<string?>("() => { const note = sessionStorage.getItem('t551Progress'); sessionStorage.removeItem('t551Progress'); return note; }") ?? string.Empty;
            string progress = progressNote.Split('|')[0];
            bool held = progressNote.EndsWith("|true", StringComparison.Ordinal);
            bool sentWithProgress = path != ScriptPath.Scripted || (progress.Contains("yükleniyor", StringComparison.Ordinal) && progress.Contains(name, StringComparison.Ordinal) && held);
            await ObserveAsync(
                "VAL-B101",
                path,
                engine,
                page,
                step,
                $"her gönderim tek görsel taşır ({fileInputs} dosya alanı); sunucu {expected} yanıtı verir" + (expected == 200 ? " (kabul: sayfa yeniden çizilir, görsel forma girer)" : string.Empty) + (expected == 422 ? " ve alan mesajı gösterilir" : string.Empty) + (path == ScriptPath.Scripted ? "; gönderim sürerken ilerleme gösterilir (dosya adı, \"yükleniyor\"; Galeriye yükle aria-disabled)" : string.Empty),
                $"yanıt {response.Status}; mesaj \"{message[..Math.Min(message.Length, 120)]}\"" + (path == ScriptPath.Scripted ? $"; ilerleme \"{progress}\"; düğme aria-disabled: {held}" : string.Empty),
                fileInputs == 1 && response.Status == expected && (expected != 422 || message.Length > 0) && sentWithProgress,
                $"uç yanıtı: POST görsel yükleme -> {response.Status}");
        }

        Assert.Empty(Failures);
    }

    /// <summary>VAL-B112: the chunked upload of the digital file needs the script - it is not made without it - and the form's other fields are saved without it.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheChunkedUpload_NeedsTheScript_AndTheOtherFieldsAreSaved_VALB112(string engine, ScriptPath path)
    {
        string id = Unique(engine, path);
        SeededProduct product = await BrowserSeed.NewProductAsync(Host.Dsn, $"p{id}", ProductType.Digital);
        await using IBrowserContext context = await Host.ContextAsync(engine, path, Actor.Admin);
        WatchedPage page = await NewPageAsync(context);
        await GoAsync(page, $"/panel/urunler/{product.ProductId}");
        ILocator input = page.Page.Locator("#product-file-upload");
        int inputs = await input.CountAsync();
        await input.First.SetInputFilesAsync(new FilePayload { Name = "t551.zip", MimeType = "application/zip", Buffer = new byte[4096] });
        string state = string.Empty;
        string fileId = string.Empty;
        if (path == ScriptPath.Scripted)
        {
            state = await TextMatchingAsync(page.Page.Locator("[data-js=file-state]"), "^(?!.*(yükleniyor|Dosya yok))", 25000);
            fileId = await page.Page.Locator("#product-file-id").InputValueAsync();
        }
        else
        {
            state = await TextMatchingAsync(page.Page.Locator("[data-js=file-state]"), "Dosya yok", 5000);
            fileId = await page.Page.Locator("#product-file-id").InputValueAsync();
        }

        string productName = $"Dijital kayıt {id}";
        await page.Page.Locator("[name='Input.Name']").FillAsync(productName);
        bool uploaded = fileId.Length > 0;
        await ObserveAsync(
            "VAL-B112",
            path,
            engine,
            page,
            "dijital varyantlı ürün formunda dosya seçilir",
            path == ScriptPath.Scripted ? "betikli yolda parçalı yükleme çalışır: dosya durumu değişir, yükleme kimliği forma yazılır" : "betiksiz yolda yükleme yapılamaz: dosya durumu \"Dosya yok\" kalır, kimlik yazılmaz",
            $"dosya alanı {inputs}; durum \"{state}\"; kimlik yazıldı: {uploaded}",
            inputs == 1 && (path == ScriptPath.Scripted ? uploaded : !uploaded && state.Contains("Dosya yok", StringComparison.Ordinal)));
        await NavigateAsync(page, () => page.Page.Locator("#product-save").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Kaydet", Exact = true }).ClickAsync());
        string row = await DataRecord.RowAsync(Host.Dsn, "SELECT name FROM products WHERE id = @i", ("i", product.ProductId));
        await ObserveAsync("VAL-B112", path, engine, page, "yükleme dışındaki alan (ad) değiştirilip Kaydet'e basılır", "formun yükleme dışındaki alanları kaydedilebilir", $"adres {PathOf(page)}", row.Contains(productName, StringComparison.Ordinal), row);
        Assert.Empty(Failures);
    }
}
