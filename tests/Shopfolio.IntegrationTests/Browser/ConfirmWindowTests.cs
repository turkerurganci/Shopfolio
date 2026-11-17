using Microsoft.Playwright;

using Shopfolio.IntegrationTests.Web;

namespace Shopfolio.IntegrationTests.Browser;

/// <summary>
/// The confirmation windows of the inventory's screens (T551 criterion 2, KK2; 04 2.4, OB-04): each screen that confirms an operation is brought to its window in each engine and
/// each way, the window is open with its "Vazgeç", "Vazgeç" is pressed and the table the operation would write is the same as before (a fingerprint of the rows). The windows are drawn by the
/// server (the script only makes them modal), so the walk is the same in both ways.
/// </summary>
public sealed class ConfirmWindowTests(BrowserHost host) : BrowserTestBase(host)
{
    private static int _unique;

    private static string Unique() => Interlocked.Increment(ref _unique).ToString(System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>A fingerprint of the rows of the table the operation would write (an md5 of the rows' text).</summary>
    private Task<string> FingerprintAsync(string table, string? where = null, params (string Name, object Value)[] parameters) =>
        DataRecord.RowAsync(Host.Dsn, $"SELECT count(*) AS rows, md5(coalesce(string_agg(t::text, ',' ORDER BY t::text), '')) AS fingerprint FROM {table} t {(where is null ? string.Empty : "WHERE " + where)}", parameters);

    /// <summary>
    /// What the accessibility pass does with the window while it is open (T552, 12 12.2 (1), VAL-A776, VAL-A781): the scan and the focus reading of <see cref="AccessibilityStatesTests"/> run through the very walk of this class,
    /// so the list of windows is this class's own and no second list can drift from it. Null in this class's own tests.
    /// </summary>
    internal Func<string, string, WatchedPage, Task>? AtWindow { get; set; }

    /// <summary>The context of a walk: the plain one, or the one the scan can read the page in (the scriptless way cuts the page's script at the network) when the accessibility pass walks the windows.</summary>
    private Task<IBrowserContext> ContextAsync(string engine, ScriptPath path, Actor actor) =>
        AtWindow is null ? Host.ContextAsync(engine, path, actor) : AccessibilityScanTests.AuditContextAsync(Host, engine, path, actor);

    /// <summary>Opens the window with the trigger, observes it, presses "Vazgeç" and observes the page it returns to and the fingerprint.</summary>
    private async Task WindowAsync(string screen, string operation, ScriptPath path, string engine, WatchedPage page, string returnPrefix, Func<Task> trigger, Func<Task<string>> fingerprint)
    {
        string before = await fingerprint();
        await NavigateAsync(page, trigger);
        ILocator window = page.Page.Locator(".confirm");
        bool open = await window.CountAsync() == 1 && await window.IsVisibleAsync();
        string sentence = open ? Flat(await window.InnerTextAsync()) : string.Empty;
        string afterOpen = await fingerprint();
        await ObserveAsync(screen, path, engine, page, $"{operation}: işlem gönderilir", "onay penceresi açılır; işlemin adı, sonucu söyleyen cümle ve Vazgeç gösterilir; hiçbir şey yazılmaz", $"pencere açık {open}; metin \"{sentence[..Math.Min(sentence.Length, 140)]}\"; kayıt {(afterOpen == before ? "değişmedi" : "DEĞİŞTİ")}", open && sentence.Contains("Vazgeç", StringComparison.Ordinal) && afterOpen == before, before);
        if (AtWindow is not null && open)
        {
            await AtWindow(screen, operation, page);
        }

        await NavigateAsync(page, () => page.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Vazgeç", Exact = true }).ClickAsync());
        string back = new Uri(page.Page.Url).PathAndQuery;
        int windows = await page.Page.Locator(".confirm").CountAsync();
        string after = await fingerprint();
        await ObserveAsync(screen, path, engine, page, $"{operation}: Vazgeç'e basılır", "ekran kendi adresine döner, pencere kapanır, kayıt değişmez", $"adres {back}; pencere {windows}; kayıt {(after == before ? "değişmedi" : "DEĞİŞTİ")}", back.StartsWith(returnPrefix, StringComparison.Ordinal) && windows == 0 && after == before, after);
    }

    private static string Flat(string text) => System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(2)).Trim();

    /// <summary>E-44: the sales switch's confirmation.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheFirmScreen_ConfirmsClosingTheSales_KK2_E44(string engine, ScriptPath path)
    {
        await using IBrowserContext context = await ContextAsync(engine, path, Actor.Admin);
        WatchedPage page = await NewPageAsync(context);
        await GoAsync(page, "/panel/firma-kimligi-ve-satis");
        await WindowAsync("E-44", "satış geçici olarak kapatılır", path, engine, page, "/panel/firma-kimligi-ve-satis", () => page.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Satışı geçici olarak kapat" }).ClickAsync(), () => FingerprintAsync("company_settings"));
        Assert.Empty(Failures);
    }

    /// <summary>E-45: closing the bank transfer.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task ThePaymentMethodsScreen_ConfirmsClosingTheTransfer_KK2_E45(string engine, ScriptPath path)
    {
        await using IBrowserContext context = await ContextAsync(engine, path, Actor.Admin);
        WatchedPage page = await NewPageAsync(context);
        await GoAsync(page, "/panel/odeme-yontemleri");
        await page.Page.Locator("#transfer-switch").UncheckAsync();
        await WindowAsync("E-45", "havale kapatılır", path, engine, page, "/panel/odeme-yontemleri", () => page.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Kaydet", Exact = true }).ClickAsync(), () => FingerprintAsync("company_settings"));
        Assert.Empty(Failures);
    }

    /// <summary>E-46: changing the stored return address.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheShippingScreen_ConfirmsChangingTheReturnAddress_KK2_E46(string engine, ScriptPath path)
    {
        await DataRecord.RowAsync(Host.Dsn, "UPDATE company_settings SET return_recipient_name = 'İade Alıcısı', return_province_id = (SELECT id FROM provinces ORDER BY id LIMIT 1), return_district = 'Çankaya', return_address_line = 'İade Cad. 1' RETURNING 1 AS done");
        await using IBrowserContext context = await ContextAsync(engine, path, Actor.Admin);
        WatchedPage page = await NewPageAsync(context);
        await GoAsync(page, "/panel/kargo-sureler-ve-varsayilanlar");
        await page.Page.Locator("[name='Input.ReturnDistrict']").FillAsync("Keçiören");
        await WindowAsync("E-46", "iade adresi değiştirilir", path, engine, page, "/panel/kargo-sureler-ve-varsayilanlar", () => page.Page.Locator("form:has([name='Input.ReturnDistrict']) button[type=submit]").First.ClickAsync(), () => FingerprintAsync("company_settings"));
        Assert.Empty(Failures);
    }

    /// <summary>E-40: deleting a member.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheMembersScreen_ConfirmsDeletingAMember_KK2_E40(string engine, ScriptPath path)
    {
        await using Npgsql.NpgsqlConnection connection = new(Host.Dsn);
        await connection.OpenAsync(Ct);
        await using Npgsql.NpgsqlCommand command = new("SELECT email FROM customers WHERE id = @c", connection);
        command.Parameters.AddWithValue("c", Host.Seed.CustomerId);
        string address = (string)(await command.ExecuteScalarAsync(Ct))!;
        await using IBrowserContext context = await ContextAsync(engine, path, Actor.Admin);
        WatchedPage page = await NewPageAsync(context);
        await GoAsync(page, "/panel/uyeler?search=" + Uri.EscapeDataString(address));
        await WindowAsync("E-40", "üye hesabı silinir", path, engine, page, "/panel/uyeler", () => page.Page.Locator("#member-delete-button").ClickAsync(), () => FingerprintAsync("customers", "id = @c", ("c", Host.Seed.CustomerId)));
        Assert.Empty(Failures);
    }

    /// <summary>E-49: removing an administrator.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheAdminAccountsScreen_ConfirmsRemovingAnAdministrator_KK2_E49(string engine, ScriptPath path)
    {
        (_, Guid other) = await TestSessions.AdminAsync(Host.Dsn);
        await using IBrowserContext context = await ContextAsync(engine, path, Actor.Admin);
        WatchedPage page = await NewPageAsync(context);
        await GoAsync(page, "/panel/yonetici-hesaplari");
        await WindowAsync("E-49", "yönetici kaldırılır", path, engine, page, "/panel/yonetici-hesaplari", () => page.Page.Locator($"form:has(input[name='Input.AdminId'][value='{other:D}']) button[type=submit]").First.ClickAsync(), () => FingerprintAsync("admins", "id = @a", ("a", other)));
        Assert.Empty(Failures);
    }

    /// <summary>E-37: marking a transfer order paid.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheOrderScreen_ConfirmsMarkingTheTransferPaid_KK2_E37(string engine, ScriptPath path)
    {
        SeededOrder order = await BrowserSeed.NewOrderAsync(Host.Dsn, $"e37-{engine}-{path}-{Unique()}", Shopfolio.Modules.Ordering.Contracts.OrderStatus.Placed, Shopfolio.Modules.Ordering.Contracts.PaymentStatus.Pending);
        await using IBrowserContext context = await ContextAsync(engine, path, Actor.Admin);
        WatchedPage page = await NewPageAsync(context);
        await GoAsync(page, $"/panel/siparisler/{order.Id}");
        await WindowAsync("E-37", "havale siparişi ödendi işaretlenir", path, engine, page, $"/panel/siparisler/{order.Id}", () => page.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ödendi olarak işaretle" }).First.ClickAsync(), () => FingerprintAsync("orders", "id = @o", ("o", order.Id)));
        Assert.Empty(Failures);
    }

    /// <summary>E-33: deleting a product for good.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheProductForm_ConfirmsDeletingTheProduct_KK2_E33(string engine, ScriptPath path)
    {
        SeededProduct product = await BrowserSeed.NewProductAsync(Host.Dsn, $"w{Unique()}", Shopfolio.Modules.Catalog.Contracts.ProductType.Physical);
        await using IBrowserContext context = await ContextAsync(engine, path, Actor.Admin);
        WatchedPage page = await NewPageAsync(context);
        await GoAsync(page, $"/panel/urunler/{product.ProductId}");
        await WindowAsync("E-33", "ürün kalıcı olarak silinir", path, engine, page, $"/panel/urunler/{product.ProductId}", () => page.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Kalıcı olarak sil" }).First.ClickAsync(), () => FingerprintAsync("products", "id = @p", ("p", product.ProductId)));
        Assert.Empty(Failures);
    }

    /// <summary>E-41: deleting a content record for good.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheContentForm_ConfirmsDeletingTheRecord_KK2_E41(string engine, ScriptPath path)
    {
        Guid offering = await BrowserSeed.NewOfferingAsync(Host.Dsn, $"w{Unique()}");
        await using IBrowserContext context = await ContextAsync(engine, path, Actor.Admin);
        WatchedPage page = await NewPageAsync(context);
        await GoAsync(page, $"/panel/kurumsal-icerik/hizmet/{offering}");
        await WindowAsync("E-41", "hizmet kaydı kalıcı olarak silinir", path, engine, page, $"/panel/kurumsal-icerik/hizmet/{offering}", () => page.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Kalıcı olarak sil" }).First.ClickAsync(), () => FingerprintAsync("offerings", "id = @o", ("o", offering)));
        Assert.Empty(Failures);
    }

    /// <summary>E-25: the customer's account deletion - the step after the password check is the confirmation; "Vazgeç" leaves the account as it is.</summary>
    [Theory]
    [MemberData(nameof(Ways))]
    public async Task TheAccountScreen_ConfirmsDeletingTheAccount_KK2_E25(string engine, ScriptPath path)
    {
        string email = $"e25-{Unique()}-{engine[..2]}@example.com";
        const string typed = "T551-Giris-Sifre-77!";
        await BrowserSeed.NewCustomerWithPasswordAsync(Host.Dsn, email, typed);
        await using IBrowserContext context = await ContextAsync(engine, path, Actor.Visitor);
        WatchedPage page = await NewPageAsync(context);
        await GoAsync(page, "/giris");
        await page.Page.Locator("[name='Input.Email']").FillAsync(email);
        await page.Page.Locator("[name='Input.Password']").FillAsync(typed);
        await NavigateAsync(page, () => page.Page.Locator("#sign-in-form button[type=submit]").ClickAsync());
        await GoAsync(page, "/hesap");
        await NavigateAsync(page, () => page.Page.Locator("#account-delete-start").ClickAsync());
        await page.Page.Locator("input[type=password]").First.FillAsync(typed);
        await WindowAsync("E-25", "hesap silinir (şifre doğrulandıktan sonraki adım)", path, engine, page, "/hesap", () => page.Page.Locator("main form button[type=submit]").First.ClickAsync(), () => FingerprintAsync("customers", "email = @e", ("e", email)));
        Assert.Empty(Failures);
    }
}
