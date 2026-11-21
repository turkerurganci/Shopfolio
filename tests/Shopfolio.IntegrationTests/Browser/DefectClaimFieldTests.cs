using Microsoft.Playwright;

namespace Shopfolio.IntegrationTests.Browser;

/// <summary>
/// The place of the defect claim's one units field on an order with several fielded items, seen in the browser (E-19; 04 2.12.1.8; K-1637 (c); T604, D-1412, D-1833). The order carries three items:
/// two with more than one unit (each row carries a field) and one with a single unit (no field). With no choice the field stands under the list on both ways. With the script a click on a fielded
/// item's choice moves the same field into that row and a click on the unfielded item's choice moves it back under the list; without the script nothing moves, and the send (refused for the empty
/// description) draws the field in the chosen item's row. The markup of both states is the integration test's (<c>AfterSalesScreenTests</c> D1833); this is the page in the browser.
/// </summary>
public sealed class DefectClaimFieldTests(BrowserHost host) : BrowserTestBase(host)
{
    private const string Holder = "[data-js='defect-claim-quantity']";

    [Theory]
    [MemberData(nameof(Ways))]
    public async Task SeveralFieldedItems_TheFieldStandsUnderTheListWithNoChoice_AndInTheChosenItemsRow_D1412_D1833(string engine, ScriptPath path)
    {
        (SeededOrder order, Guid[] items) = await BrowserSeed.NewOrderWithItemsAsync(Host.Dsn, "defect-claim-rows", 3, 2, 1);
        await using IBrowserContext context = await Host.ContextAsync(engine, path);
        WatchedPage page = await NewPageAsync(context);
        await GoAsync(page, $"/siparis/erisim/{order.AccessKey}/ayip-talebi");

        (int slot, int first, int second, int single) = await PlacesAsync(page, items);
        await ObserveAsync(
            "E-19", path, engine, page, "birden çok alanlı kalemli siparişte ayıp talebi ekranı açılır, kalem seçilmemiş", "tek adet alanı listenin altında durur, hiçbir kalemin satırında değil",
            $"liste altında {slot}; 1. kalem satırında {first}; 2. kalem satırında {second}; tek adetli kalemin satırında {single}", (slot, first, second, single) == (1, 0, 0, 0));

        await page.Page.Locator($"input[name='OrderItemId'][value='{items[1]}']").CheckAsync();
        (slot, first, second, single) = await PlacesAsync(page, items);
        bool moved = path == ScriptPath.Scripted;
        await ObserveAsync(
            "E-19", path, engine, page, "alanı olan 2. kalem seçilir",
            moved ? "alan 2. kalemin satırına taşınır" : "betik yok: alan yerinde (liste altında) kalır, gönderimde sunucu yerleştirir",
            $"liste altında {slot}; 1. kalem satırında {first}; 2. kalem satırında {second}; tek adetli kalemin satırında {single}",
            moved ? (slot, first, second, single) == (0, 0, 1, 0) : (slot, first, second, single) == (1, 0, 0, 0));

        if (moved)
        {
            await page.Page.Locator($"input[name='OrderItemId'][value='{items[2]}']").CheckAsync();
            (slot, first, second, single) = await PlacesAsync(page, items);
            await ObserveAsync(
                "E-19", path, engine, page, "alanı olmayan (tek adetli) kalem seçilir", "alan listenin altına döner",
                $"liste altında {slot}; 1. kalem satırında {first}; 2. kalem satırında {second}; tek adetli kalemin satırında {single}", (slot, first, second, single) == (1, 0, 0, 0));
        }
        else
        {
            // The description is left empty: the send is refused (422) and the page comes back with the sent choice, the field drawn by the server in that item's row.
            await NavigateAsync(page, () => page.Page.Locator("form[action*='SubmitDefectClaim'] button[type='submit']").ClickAsync());
            (slot, first, second, single) = await PlacesAsync(page, items);
            bool chosen = await page.Page.Locator($"input[name='OrderItemId'][value='{items[1]}']").IsCheckedAsync();
            await ObserveAsync(
                "E-19", path, engine, page, "2. kalem seçili, açıklama boş gönderilir", "ret sayfasında 2. kalem seçili ve alan onun satırında",
                $"2. kalem seçili {chosen}; liste altında {slot}; 1. kalem satırında {first}; 2. kalem satırında {second}; tek adetli kalemin satırında {single}",
                chosen && (slot, first, second, single) == (0, 0, 1, 0));
        }

        Assert.Empty(Failures);
    }

    private static async Task<(int Slot, int First, int Second, int Single)> PlacesAsync(WatchedPage page, Guid[] items) => (
        await page.Page.Locator($"[data-js='defect-claim-slot'] {Holder}").CountAsync(),
        await page.Page.Locator($"[data-item='{items[0]}'] {Holder}").CountAsync(),
        await page.Page.Locator($"[data-item='{items[1]}'] {Holder}").CountAsync(),
        await page.Page.Locator($"[data-item='{items[2]}'] {Holder}").CountAsync());
}
