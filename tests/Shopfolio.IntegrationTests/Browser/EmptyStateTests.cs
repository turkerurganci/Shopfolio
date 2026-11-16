using Microsoft.Playwright;

namespace Shopfolio.IntegrationTests.Browser;

/// <summary>
/// The empty state of the panel's lists (T551 criterion 2, KK2 (b); 04 2.7.1.2): a list with no record says in one sentence what is not there and shows a way out - never an empty table and never an error. The inventory's walk
/// opens the lists with the seeded data, so the empty state needs an installation with no record: <see cref="BrowserHost.EmptyAsync"/> (its own database made from the template, one administrator who can sign in).
/// </summary>
public sealed class EmptyStateTests(BrowserHost host) : BrowserTestBase(host)
{
    /// <summary>The lists that have an empty state: the screen, its name and its address.</summary>
    internal static readonly (string Screen, string Name, string Path)[] Lists =
    [
        ("E-32", "ürün listesi", "/panel/urunler"),
        ("E-35", "kupon listesi", "/panel/kuponlar"),
        ("E-36", "sipariş listesi", "/panel/siparisler"),
        ("E-38", "talep listesi", "/panel/talepler"),
        ("E-52", "işlem izi", "/panel/islem-izi"),
    ];

    private static string Flat(string text) => System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(2)).Trim();

    public static TheoryData<string, ScriptPath, string> Cases()
    {
        TheoryData<string, ScriptPath, string> data = [];
        foreach (string engine in BrowserHost.Engines)
        {
            foreach (ScriptPath path in Enum.GetValues<ScriptPath>())
            {
                foreach ((string screen, _, _) in Lists)
                {
                    data.Add(engine, path, screen);
                }
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task TheListOfAnInstallationWithNoRecord_SaysWhatIsNotThere_AndShowsAWayOut_KK2(string engine, ScriptPath path, string screen)
    {
        (string _, string name, string address) = Lists.Single(l => l.Screen == screen);
        BrowserHost empty = await Host.EmptyAsync();
        await using IBrowserContext context = await empty.ContextAsync(engine, path, Actor.Admin);
        WatchedPage page = await NewPageAsync(context);
        IResponse? response = await page.Page.GotoAsync(empty.BaseUrl + address, new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 30000 });
        ILocator state = page.Page.Locator("main .empty-state");
        int states = await state.CountAsync();
        string sentence = states > 0 ? Flat(await state.First.Locator(".empty-state__sentence").InnerTextAsync()) : string.Empty;
        int exits = states > 0 ? await state.First.Locator("a[href]").CountAsync() : 0;
        int rows = await page.Page.Locator("main tbody tr, main .record-list li").CountAsync();
        int errors = await page.Page.Locator("main .message--error, [data-js=error-summary]").CountAsync();
        await ObserveAsync(
            screen,
            path,
            engine,
            page,
            $"{name}: kayıt yokken açılır",
            "200 döner; boş hâl cümlesi ve çıkış yolu görünür; satır yok; hata gösterilmez",
            $"durum {response?.Status}; boş hâl {states}; cümle \"{sentence}\"; çıkış bağlantısı {exits}; satır {rows}; hata {errors}; sayfa hatası {page.Errors.Count}",
            response?.Status == 200 && states == 1 && sentence.Length > 0 && exits == 1 && rows == 0 && errors == 0 && page.Errors.Count == 0);
        Assert.Empty(Failures);
    }
}
