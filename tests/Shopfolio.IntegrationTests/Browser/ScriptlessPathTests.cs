using System.Text.RegularExpressions;

using Microsoft.Playwright;

using Npgsql;

namespace Shopfolio.IntegrationTests.Browser;

/// <summary>
/// The observation of the scriptless path's rows (T551 criterion 3; 12 4.17, rows <c>VAL-B096</c>...<c>VAL-B112</c>): each row is observed in both ways - the script on and
/// the script off - in each engine, and each line of the record carries the row, the way, the engine, the width, the step, what was expected and what was seen
/// with the screenshot (12 7.1; the T546 and T547 precedent: no shared sentence stands for a line). A row with a data record also reads the table row.
/// </summary>
public sealed class ScriptlessPathTests(BrowserHost host) : BrowserTestBase(host)
{
    private static readonly Regex Spaces = new(@"\s+", RegexOptions.None, TimeSpan.FromSeconds(2));

    private static string Flat(string text) => Spaces.Replace(text, " ").Trim();

    /// <summary>Waits until the locator's text matches the pattern or the time is over; returns the text last read.</summary>
    private static async Task<string> TextMatchingAsync(ILocator locator, string pattern, int milliseconds = 6000)
    {
        string last = string.Empty;
        DateTime end = DateTime.UtcNow.AddMilliseconds(milliseconds);
        while (DateTime.UtcNow < end)
        {
            if (await locator.CountAsync() > 0)
            {
                last = Flat(await locator.First.InnerTextAsync());
                if (Regex.IsMatch(last, pattern, RegexOptions.None, TimeSpan.FromSeconds(2)))
                {
                    return last;
                }
            }

            await Task.Delay(150, Ct);
        }

        return last;
    }

    private async Task AddToCartAsync(WatchedPage page, string slug)
    {
        await GoAsync(page, $"/urun/{slug}");
        await page.Page.Locator("form.buy-form button[type=submit]").First.ClickAsync();
        await SettleAsync(page);
    }

    /// <summary>VAL-B098: the cart's quantity change - in place by the script, by a 303 to the page without it - gives the server's amount in both ways.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheCartsQuantityChange_GivesTheServersAmount_InBothWays_VALB098(string engine, ScriptPath path)
    {
        await using IBrowserContext context = await Host.ContextAsync(engine, path);
        WatchedPage page = await NewPageAsync(context);
        await AddToCartAsync(page, Host.Seed.Product("plain").Slug);
        await GoAsync(page, "/sepet");
        ILocator quantity = page.Page.Locator("form.cart-line__quantity input[name=Quantity]").First;
        await quantity.FillAsync("3");
        await page.Page.Locator("form.cart-line__quantity button[type=submit]").First.ClickAsync();
        string total = await TextMatchingAsync(page.Page.Locator(".cart-line__total"), "300");
        await ObserveAsync("VAL-B098", path, engine, page, "sepette sade ürünün adedi 3 yapılır ve Güncelle'ye basılır", "kalem tutarı 3 x 100 = 300 (sunucunun hesabı); sayfada betiksiz yolda 303 sonrası aynı tutar", $"kalem tutarı: \"{total}\"; adres {new Uri(page.Page.Url).PathAndQuery}", total.Contains("300", StringComparison.Ordinal));
        Assert.Empty(Failures);
    }

    /// <summary>VAL-B100: the variant choice of the product page - in place by the script, by a GET form without it - gives the chosen variant's price.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheVariantChoice_ShowsTheChosenVariantsPrice_InBothWays_VALB100(string engine, ScriptPath path)
    {
        await using IBrowserContext context = await Host.ContextAsync(engine, path);
        WatchedPage page = await NewPageAsync(context);
        await GoAsync(page, $"/urun/{Host.Seed.Product("variants").Slug}");
        ILocator selects = page.Page.Locator("form.variant-form select[data-js=option]");
        await selects.Nth(0).SelectOptionAsync(new SelectOptionValue { Label = "Renk 2" });
        await selects.Nth(1).SelectOptionAsync(new SelectOptionValue { Label = "Beden 1" });
        if (path == ScriptPath.Scriptless)
        {
            await page.Page.Locator("button[data-js=apply-selection]").ClickAsync();
            await SettleAsync(page);
        }

        string price = await TextMatchingAsync(page.Page.Locator("[data-js=price]"), "140");
        string address = new Uri(page.Page.Url).PathAndQuery;
        bool queryOk = path == ScriptPath.Scripted || address.Contains('?', StringComparison.Ordinal);
        await ObserveAsync("VAL-B100", path, engine, page, "iki seçenekli üründe Renk 2 ve Beden 1 seçilir", "fiyat 140 (Renk 2, Beden 1); betiksiz yolda seçim ürün adresinin sorgu parametresindedir", $"fiyat: \"{price}\"; adres {address}", price.Contains("140", StringComparison.Ordinal) && queryOk);
        Assert.Empty(Failures);
    }

    /// <summary>VAL-B103: "Tümünü seç" and "Seçimi kaldır" of the delivery provinces exist on the scripted path only; the boxes are ticked one by one without them.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheProvinceBulkButtons_ExistOnlyWithTheScript_VALB103(string engine, ScriptPath path)
    {
        await using IBrowserContext context = await Host.ContextAsync(engine, path, Actor.Admin);
        WatchedPage page = await NewPageAsync(context);
        await GoAsync(page, "/panel/kargo-sureler-ve-varsayilanlar");
        ILocator all = page.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Tümünü seç" });
        ILocator none = page.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Seçimi kaldır" });
        int buttons = await all.CountAsync() + await none.CountAsync();
        ILocator boxes = page.Page.Locator("[data-js=province-boxes] input[type=checkbox]");
        int boxCount = await boxes.CountAsync();
        if (path == ScriptPath.Scripted)
        {
            await all.First.ClickAsync();
            int ticked = await boxes.EvaluateAllAsync<int>("els => els.filter(e => e.checked).length");
            await none.First.ClickAsync();
            int left = await boxes.EvaluateAllAsync<int>("els => els.filter(e => e.checked).length");
            await ObserveAsync("VAL-B103", path, engine, page, "Tümünü seç, sonra Seçimi kaldır", $"iki düğme var; Tümünü seç {boxCount} kutunun hepsini işaretler, Seçimi kaldır hepsini kaldırır", $"düğme sayısı {buttons}; işaretli {ticked}/{boxCount}; kaldırınca {left}", buttons == 2 && boxCount > 0 && ticked == boxCount && left == 0);
        }
        else
        {
            await ObserveAsync("VAL-B103", path, engine, page, "il kutularının sayfası betiksiz açılır", $"düğme yoktur; {boxCount} kutu tek tek işaretlenir", $"düğme sayısı {buttons}; kutu sayısı {boxCount}", buttons == 0 && boxCount > 0);
        }

        Assert.Empty(Failures);
    }

    /// <summary>VAL-B104: the sales summary's period choice goes by itself with the script and by the button without it; both give a GET with the chosen period.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheSalesSummarysPeriodChoice_IsAGet_InBothWays_VALB104(string engine, ScriptPath path)
    {
        await using IBrowserContext context = await Host.ContextAsync(engine, path, Actor.Admin);
        WatchedPage page = await NewPageAsync(context);
        await GoAsync(page, "/panel/satis-ozeti");
        ILocator period = page.Page.Locator("form.summary-period select").First;
        string[] values = await period.Locator("option").EvaluateAllAsync<string[]>("els => els.map(e => e.value)");
        string current = await period.InputValueAsync();
        string chosen = values.Last(v => v != current && v.Length > 0 && v != "Custom");
        await period.SelectOptionAsync(chosen);
        if (path == ScriptPath.Scriptless)
        {
            await page.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Göster" }).ClickAsync();
        }

        await page.Page.WaitForURLAsync(u => u.Contains('?', StringComparison.Ordinal), new PageWaitForURLOptions { Timeout = 8000 });
        string address = new Uri(page.Page.Url).PathAndQuery;
        string selected = await page.Page.Locator("form.summary-period select").First.InputValueAsync();
        await ObserveAsync("VAL-B104", path, engine, page, $"dönem seçeneği \"{chosen}\" seçilir" + (path == ScriptPath.Scriptless ? ", Göster'e basılır" : string.Empty), "özet seçilen dönemle GET'le yenilenir; seçili dönem korunur", $"adres {address}; seçili dönem \"{selected}\"", selected == chosen && address.Contains('=', StringComparison.Ordinal));
        Assert.Empty(Failures);
    }

    /// <summary>VAL-B096: the customer's last step - the first send of the operation only shows the confirmation, nothing is written; the confirmation button writes. The same in both ways.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheCustomersLastStep_WritesOnlyWithTheConfirmation_InBothWays_VALB096(string engine, ScriptPath path)
    {
        SeededOrder order = await BrowserSeed.NewOrderAsync(Host.Dsn, $"b096-{engine}-{path}", Shopfolio.Modules.Ordering.Contracts.OrderStatus.Placed, Shopfolio.Modules.Ordering.Contracts.PaymentStatus.Pending);
        await using IBrowserContext context = await Host.ContextAsync(engine, path);
        WatchedPage page = await NewPageAsync(context);
        await GoAsync(page, $"/siparis/erisim/{order.AccessKey}/iptal");
        await page.Page.Locator("form.operation-screen__form button[type=submit]").First.ClickAsync();
        await SettleAsync(page);
        bool confirmShown = await page.Page.Locator(".confirm--step").CountAsync() > 0;
        string beforeConfirm = await ReadStatusAsync(order.Id);
        await ObserveAsync("VAL-B096", path, engine, page, "iptal ekranında ilk gönderim (onaysız)", "aynı sayfa onay hâliyle gelir; sipariş durumu Placed kalır (bir şey yazılmaz)", $"onay bölümü {(confirmShown ? "var" : "yok")}; sipariş durumu {beforeConfirm}", confirmShown && beforeConfirm.StartsWith("Placed", StringComparison.Ordinal), await OrderRowAsync(order.Id));
        await page.Page.Locator(".confirm--step button[type=submit]").First.ClickAsync();
        await SettleAsync(page);
        string afterConfirm = await ReadStatusAsync(order.Id);
        for (int wait = 0; wait < 20 && !afterConfirm.StartsWith("Cancelled", StringComparison.Ordinal); wait++)
        {
            await Task.Delay(500, Ct);
            afterConfirm = await ReadStatusAsync(order.Id);
        }
        await ObserveAsync("VAL-B096", path, engine, page, "onay düğmesiyle gönderim", "sipariş iptal olur (Veri kaydı: orders.order_status Cancelled)", $"sipariş durumu {afterConfirm}; adres {new Uri(page.Page.Url).PathAndQuery}", afterConfirm.StartsWith("Cancelled", StringComparison.Ordinal), await OrderRowAsync(order.Id));
        Assert.Empty(Failures);
    }

    private Task<string> OrderRowAsync(Guid orderId) => DataRecord.RowAsync(Host.Dsn, "SELECT order_number, order_status, payment_status, contact_email FROM orders WHERE id = @id", ("id", orderId));

    private async Task<string> ReadStatusAsync(Guid orderId)
    {
        await using NpgsqlConnection connection = new(Host.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new("SELECT order_status || '/' || payment_status FROM orders WHERE id = @id", connection);
        command.Parameters.AddWithValue("id", orderId);
        return (string?)await command.ExecuteScalarAsync(Ct) ?? "yok";
    }
}
