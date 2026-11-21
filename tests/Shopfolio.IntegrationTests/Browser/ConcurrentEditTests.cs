using Microsoft.Playwright;

using Shopfolio.Infrastructure.Forms;

namespace Shopfolio.IntegrationTests.Browser;

/// <summary>
/// The warning of two administrators editing one record, seen in two browser contexts (VAL-A371; 02 3.31.1; 04 2.10.1; K-800; T604 A1, D-1753): the product form is open in both contexts at the same version, the first administrator
/// saves, the second one saves a different name; the second sees the warning in the page (the message, the record's own link and the one "Yine de kaydet" button), the first one's value stays in the database until the second
/// confirms, and the confirmation is a save like any other. The uç yanıtı (409) and the data are the integration tests' (<c>ProductFormPageTests</c>, <c>ContentPanelPageTests</c>); this is the page in the browser.
/// </summary>
public sealed class ConcurrentEditTests(BrowserHost host) : BrowserTestBase(host)
{
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TwoAdministratorsOnTheSameProduct_TheSecondOneSeesTheWarningInThePage_AndNothingIsOverwrittenBeforeTheConfirmation_VALA371_D1753(string engine, ScriptPath path)
    {
        string id = Guid.NewGuid().ToString("N")[..8];
        SeededProduct product = await BrowserSeed.NewProductAsync(Host.Dsn, $"c{id}", Shopfolio.Modules.Catalog.Contracts.ProductType.Physical);
        await using IBrowserContext firstContext = await Host.ContextAsync(engine, path, Actor.Admin);
        await using IBrowserContext secondContext = await Host.ContextAsync(engine, path, Actor.SecondAdmin);
        WatchedPage first = await NewPageAsync(firstContext);
        WatchedPage second = await NewPageAsync(secondContext);
        string address = $"/panel/urunler/{product.ProductId}";
        await GoAsync(first, address);
        await GoAsync(second, address);
        string firstName = $"Birinci yönetici {id}";
        string secondName = $"İkinci yönetici {id}";
        const string nameSql = "SELECT name FROM products WHERE id = @i";

        // Both have the form open at the same version; the first one saves.
        await first.Page.Locator("[name='Input.Name']").FillAsync(firstName);
        await NavigateAsync(first, () => first.Page.Locator("#product-save").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = ConflictControls.SaveLabel, Exact = true }).ClickAsync());
        string afterFirst = await DataRecord.ScalarAsync(Host.Dsn, nameSql, ("i", product.ProductId));
        await ObserveAsync(
            "VAL-A371", path, engine, first, "ilk yönetici adı değiştirir ve kaydeder", "kayıt ilk yöneticinin adıyla kaydedilir",
            $"adres {PathOf(first)}; veritabanında ad \"{afterFirst}\"", afterFirst == firstName);

        // The second one, still on the old version, saves a different name: the page warns and does not write.
        await second.Page.Locator("[name='Input.Name']").FillAsync(secondName);
        await NavigateAsync(second, () => second.Page.Locator("#product-save").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = ConflictControls.SaveLabel, Exact = true }).ClickAsync());
        string warned = await second.Page.Locator("#product-save").InnerTextAsync();
        string kept = await second.Page.Locator("[name='Input.Name']").InputValueAsync();
        ILocator overwrite = second.Page.Locator("#product-save").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = ConflictControls.OverwriteLabel, Exact = true });
        ILocator current = second.Page.Locator("#product-save").GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = ConflictControls.OpenCurrentLabel, Exact = true });
        string afterSecond = await DataRecord.ScalarAsync(Host.Dsn, nameSql, ("i", product.ProductId));
        bool message = warned.Contains(ConflictControls.RecordMessage, StringComparison.Ordinal);
        bool overwriteShown = await overwrite.CountAsync() == 1 && await overwrite.IsVisibleAsync();
        bool currentShown = await current.CountAsync() == 1 && await current.IsVisibleAsync();
        await ObserveAsync(
            "VAL-A371", path, engine, second, "ikinci yönetici eski sürümle başka bir ad kaydeder",
            "sayfada kaydın o arada başka yöneticice kaydedildiği uyarısı, \"Güncel hâli aç\" bağlantısı ve tek \"Yine de kaydet\" düğmesi gösterilir; ikincinin yazdığı ad formda durur; veritabanında ilkinin adı kalır (üzerine yazılmaz)",
            $"uyarı metni var {message}; Yine de kaydet düğmesi {overwriteShown}; Güncel hâli aç bağlantısı {currentShown}; formdaki ad \"{kept}\"; veritabanında ad \"{afterSecond}\"; adres {PathOf(second)}",
            message && overwriteShown && currentShown && kept == secondName && afterSecond == firstName);

        // The confirmation is a save like any other: the second one's value is written.
        await NavigateAsync(second, () => overwrite.ClickAsync());
        string afterConfirm = await DataRecord.ScalarAsync(Host.Dsn, nameSql, ("i", product.ProductId));
        bool warningGone = !(await second.Page.Locator("#product-save").InnerTextAsync()).Contains(ConflictControls.RecordMessage, StringComparison.Ordinal);
        await ObserveAsync(
            "VAL-A371", path, engine, second, "ikinci yönetici \"Yine de kaydet\"e basar", "ikincinin adı kaydedilir ve uyarı kalkar",
            $"veritabanında ad \"{afterConfirm}\"; uyarı kalktı {warningGone}; adres {PathOf(second)}",
            afterConfirm == secondName && warningGone);

        Assert.Empty(Failures);
    }

    private static string PathOf(WatchedPage page) => new Uri(page.Page.Url).PathAndQuery;
}
