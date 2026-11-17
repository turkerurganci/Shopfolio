using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.Playwright;

namespace Shopfolio.IntegrationTests.Browser;

/// <summary>
/// The validation-error state of the inventory's forms (T551 criterion 2, KK2; 04 2.3): each form is sent with one value that is not allowed (a required field emptied, an address, a number or an
/// IBAN that is not one) in each engine and each way; the answer is 422 with an invalid field on the page, and the table the form would write is the same as before. The list of the forms is
/// the cases below, and it is not a second list: <see cref="EveryFormWithFieldsOfTheWalk_HasACase_AndEveryCaseIsInTheWalk_KK2"/> reads the forms of the screen walk (<see cref="ScreenInventoryTests"/>)
/// from the pages themselves, so a form that gets a field and has no case, and a case whose form left the pages, break that test.
/// </summary>
public sealed class ValidationStateTests(BrowserHost host) : BrowserTestBase(host)
{
    /// <summary>What a flow case's preparation works with: the page, the host and a value that is unique to the run of the case.</summary>
    internal sealed record Run(WatchedPage Page, BrowserHost Host, string Unique);

    /// <summary>
    /// One form: the screen, the handler its address names, the slot (its id, or "@n" for the n-th form of the handler with no id, or empty for the only one), who opens it, the address, the table that must not change,
    /// a preparation after the page opens and a way to break the form where the generic one (<see cref="BreakGenericAsync"/>) is not enough. <paramref name="FromFlow"/> is a form the walk's pages do not show
    /// until a flow opens it (a re-verified account change, a cart). <paramref name="NoValidationState"/> names the product rule (04) that makes a form one with no invalid value at all (D-1712): its case observes that the
    /// form is accepted and shows no error, instead of a 422.
    /// </summary>
    internal sealed record FormCase(
        string Screen,
        string Handler,
        string Slot,
        string Form,
        Actor Actor,
        Func<BrowserHost, string> Path,
        string? Table = null,
        Func<Run, Task>? Prepare = null,
        Func<ILocator, Task>? Break = null,
        string? Button = null,
        bool FromFlow = false,
        string? NoValidationState = null,
        string? Watch = null)
    {
        public string Key => $"{Screen}|{Handler}|{Slot}";
    }

    private static string Order(BrowserHost h, string label) => h.Seed.Order(label).Id.ToString("D");

    private static readonly FormCase[] AllCases =
    [
        new("E-04", "AddCartItem", "", "sepete ekleme formu (adet)", Actor.Visitor, h => $"/urun/{h.Seed.Product("plain").Slug}"),
        new("E-10", "SendContactRequest", "", "iletişim formu", Actor.Visitor, _ => "/iletisim", "contact_requests"),
        new("E-12", "UpdateCartItem", "", "sepet satırı adet formu", Actor.Visitor, _ => "/sepet", Prepare: r => CartWithAProductAsync(r, "/sepet"), FromFlow: true),
        new("E-13", "PlaceOrder", "checkout-form", "ödeme adımı formu (siparişi onayla)", Actor.Visitor, _ => "/odeme", "orders", Prepare: r => CartWithAProductAsync(r, "/odeme"), Button: "css:button[data-js=submit-once]", FromFlow: true,
            // A change of the province, the method or the coupon sends the form to recalculate (checkout.js), which would race the send of this case; the wrong value is the contact address.
            Break: async form => await form.Locator("input[name=ContactEmail]").FillAsync("not-an-email")),
        new("E-15", "FindOrder", "", "sipariş takibi formu", Actor.Visitor, _ => "/siparis-takibi", "orders"),
        new("E-17", "CancelOrderItems", "", "iptal ekranı (üye): kalemler ve IBAN", Actor.Customer, h => $"/siparis/{h.Seed.Order("member-paid").Number}/iptal"),
        new("E-18", "SubmitWithdrawal", "", "cayma beyanı formu", Actor.Visitor, h => $"/siparis/erisim/{h.Seed.Order("delivered").AccessKey}/cayma"),
        new("E-19", "SubmitDefectClaim", "", "ayıp talebi formu", Actor.Visitor, h => $"/siparis/erisim/{h.Seed.Order("delivered").AccessKey}/ayip-talebi"),
        new("E-20", "Register", "registration-form", "kayıt formu", Actor.Visitor, _ => "/kayit", "customers"),
        new("E-22", "SignIn", "sign-in-form", "müşteri giriş formu", Actor.Visitor, _ => "/giris", "customers"),
        new("E-23", "RequestPasswordReset", "reset-request-form", "şifre sıfırlama isteği formu", Actor.Visitor, _ => "/sifre-sifirlama", "customers"),
        // The new password forms of the e-mailed reset links (the walk opens them with a valid link): a refused send writes neither the password nor the link's use, so what is watched is the account and its link rows together.
        new("E-23", "ResetPassword", "reset-set-form", "yeni şifre formu (geçerli sıfırlama bağlantısı)", Actor.Visitor, h => $"/sifre-sifirlama/{h.Links.PasswordReset}", "customers", Watch: "(SELECT l.*, c.password_hash FROM link_records l LEFT JOIN customers c ON c.id = l.customer_id)"),
        new("E-24", "Reauthenticate", "reauth-form", "müşterinin yeniden doğrulama formu", Actor.Customer, _ => "/hesap/yeniden-dogrulama?reauthenticatedOperation=EmailChange"),
        new("E-25", "ChangeName", "account-name-form", "hesap adı formu", Actor.Customer, _ => "/hesap", "customers"),
        new("E-25", "ChangeEmail", "account-email-form", "yeni e-posta formu (yeniden doğrulamadan sonra)", Actor.Visitor, _ => "/giris", "customers", Prepare: r => CustomerReopensAsync(r, "EmailChange"), FromFlow: true),
        new("E-25", "ChangePassword", "account-password-form", "yeni şifre formu (yeniden doğrulamadan sonra)", Actor.Visitor, _ => "/giris", "customers", Prepare: r => CustomerReopensAsync(r, "PasswordChange"), FromFlow: true),
        new("E-26", "CreateAddressBook", "address-form", "yeni adres formu", Actor.Customer, _ => "/hesap/adresler?create=true"),
        new("E-29", "SignIn", "", "panel giriş formu", Actor.Visitor, _ => "/panel/giris", "admins"),
        new("E-29", "RequestPasswordReset", "", "panel şifre sıfırlama isteği formu", Actor.Visitor, _ => "/panel/sifre-sifirlama", "admins"),
        new("E-29", "ResetPassword", "", "panel yeni şifre formu (geçerli sıfırlama bağlantısı)", Actor.Visitor, h => $"/panel/sifre-sifirlama/{h.Links.AdminPasswordReset}", "admins", Watch: "(SELECT l.*, a.password_hash FROM link_records l LEFT JOIN admins a ON a.id = l.admin_id)"),
        new("E-30", "AcceptAdminInvitation", "", "yönetici daveti formu (geçerli bağlantı)", Actor.Visitor, h => $"/panel/davet/{h.InvitationKey}", "admins"),
        new("E-33", "SaveProduct", "product-save", "ürün formu (yeni ürün)", Actor.Admin, _ => "/panel/urunler/yeni", "products", Button: "Kaydet"),
        new("E-34", "CreateCategory", "yeni-kategori", "yeni kategori formu", Actor.Admin, _ => "/panel/kategoriler", "categories"),
        new("E-34", "CreateCategory", "", "alt kategori formu", Actor.Admin, _ => "/panel/kategoriler", "categories"),
        new("E-34", "RenameCategory", "", "kategori adı formu", Actor.Admin, _ => "/panel/kategoriler", "categories"),
        // The list offers only parents the category may move under, so the wrong value is one the page does not offer: the category's own branch (UC-120: 422 in place; a parent that does not exist is 404).
        new("E-34", "MoveCategory", "", "kategori taşıma formu (kendi dalına taşıma)", Actor.Admin, _ => "/panel/kategoriler", "categories", Break: async form =>
            await form.Locator("select[name='Input.ParentCategoryId']").EvaluateAsync("(s) => { const id = s.form.querySelector('input[name=\"Input.CategoryId\"]').value; const o = document.createElement('option'); o.value = id; o.text = 'kendisi'; s.appendChild(o); s.value = id; }")),
        new("E-35", "SaveCoupon", "", "kupon formu (yeni kupon)", Actor.Admin, _ => "/panel/kuponlar/yeni", "coupons"),
        new("E-37", "CancelOrderByCompany", "cancel-order-by-company-form", "firma iptali formu", Actor.Admin, h => $"/panel/siparisler/{Order(h, "awaiting-transfer")}", "orders"),
        new("E-37", "CorrectAddress", "correct-shipping-address-form", "teslimat adresi düzeltme formu", Actor.Admin, h => $"/panel/siparisler/{Order(h, "awaiting-transfer")}", "orders"),
        new("E-37", "CorrectAddress", "correct-billing-address-form", "fatura adresi düzeltme formu", Actor.Admin, h => $"/panel/siparisler/{Order(h, "awaiting-transfer")}", "orders"),
        new("E-37", "CorrectOrderEmail", "correct-order-email-form", "sipariş e-postası düzeltme formu", Actor.Admin, h => $"/panel/siparisler/{Order(h, "awaiting-transfer")}", "orders"),
        new("E-37", "SaveInternalNote", "internal-note-form", "iç not formu", Actor.Admin, h => $"/panel/siparisler/{Order(h, "awaiting-transfer")}", "orders", NoValidationState: "04 7.2.15.1: iç not isteğe bağlı tek serbest metindir, sınırı yoktur"),
        new("E-37", "RecordWithdrawal", "record-withdrawal-form", "cayma kaydı formu", Actor.Admin, h => $"/panel/siparisler/{Order(h, "delivered")}", "orders"),
        new("E-37", "RequestRefundIban", "request-refund-iban-form", "iade IBAN'ı isteme formu", Actor.Admin, h => $"/panel/siparisler/{Order(h, "delivered")}", "orders"),
        new("E-37", "CorrectCarrier", "correct-carrier-form", "kargo firması düzeltme formu", Actor.Admin, h => $"/panel/siparisler/{Order(h, "delivered")}", "orders"),
        new("E-37", "MarkDelivered", "mark-delivered-form", "teslim edildi formu", Actor.Admin, h => $"/panel/siparisler/{Order(h, "shipped")}", "orders"),
        new("E-37", "MarkReshipped", "mark-reshipped-form", "yeniden gönderildi formu", Actor.Admin, h => $"/panel/siparisler/{Order(h, "delivery-failed")}", "orders"),
        new("E-41", "SaveContent", "content-save", "kurumsal içerik formu (yeni hizmet)", Actor.Admin, _ => "/panel/kurumsal-icerik/hizmet/yeni", "offerings", Button: "Kaydet"),
        new("E-42", "SaveHomepage", "", "ana sayfa ve menü formu", Actor.Admin, _ => "/panel/ana-sayfa-ve-menu", "company_settings"),
        new("E-43", "SaveBranding", "brand-save", "marka formu", Actor.Admin, _ => "/panel/marka", "company_settings", Button: "Kaydet"),
        new("E-44", "SaveCompany", "", "firma kimliği formu", Actor.Admin, _ => "/panel/firma-kimligi-ve-satis", "companies"),
        new("E-45", "SavePaymentMethods", "", "ödeme yöntemleri formu (havale açık, IBAN geçersiz)", Actor.Admin, _ => "/panel/odeme-yontemleri", "company_settings", async r => await r.Page.Page.Locator("#transfer-switch").CheckAsync()),
        new("E-46", "SaveShippingFee", "", "kargo ücreti formu", Actor.Admin, _ => "/panel/kargo-sureler-ve-varsayilanlar", "company_settings"),
        new("E-46", "SaveDeliveryProvinces", "", "teslimat illeri formu", Actor.Admin, _ => "/panel/kargo-sureler-ve-varsayilanlar", "company_delivery_provinces", NoValidationState: "04 5.4.5 / K-828: hiçbir il seçilmeden kayıt geçerlidir, vitrin 'hiçbir ile teslimat yapılmıyor' der"),
        new("E-46", "SavePeriods", "", "süreler formu", Actor.Admin, _ => "/panel/kargo-sureler-ve-varsayilanlar", "company_settings"),
        new("E-46", "SaveProductDefaults", "", "ürün varsayılanları formu", Actor.Admin, _ => "/panel/kargo-sureler-ve-varsayilanlar", "company_settings"),
        new("E-46", "SaveReturnAddress", "", "iade adresi formu", Actor.Admin, _ => "/panel/kargo-sureler-ve-varsayilanlar", "company_settings"),
        new("E-47", "SaveLegalTextDraft", "@0", "yasal metin taslağı formu (barındırma yerli)", Actor.Admin, _ => "/panel/yasal-metinler", "legal_text_versions"),
        new("E-47", "SaveLegalTextDraft", "@1", "yasal metin taslağı formu", Actor.Admin, _ => "/panel/yasal-metinler", "legal_text_versions"),
        new("E-49", "SendAdminInvitation", "@1", "yönetici daveti gönderme formu", Actor.Admin, _ => "/panel/yonetici-hesaplari", "admin_invitations"),
        new("E-50", "ChangeName", "change-name-form", "yöneticinin ad formu", Actor.Admin, _ => "/panel/hesabim", "admins"),
        new("E-50", "ChangeEmail", "change-email-form", "yöneticinin yeni e-posta formu (yeniden doğrulamadan sonra)", Actor.Visitor, _ => "/panel/giris", "admins", Prepare: r => AdminReopensAsync(r, "EmailChange"), FromFlow: true),
        new("E-50", "ChangePassword", "change-password-form", "yöneticinin yeni şifre formu (yeniden doğrulamadan sonra)", Actor.Visitor, _ => "/panel/giris", "admins", Prepare: r => AdminReopensAsync(r, "PasswordChange"), FromFlow: true),
        new("E-53", "ExportOrders", "", "sipariş dışa aktarma formu (bitiş başlangıçtan önce)", Actor.Admin, _ => "/panel/disa-aktarma", null, Break: async form =>
        {
            await form.Locator("input[name=StartDate]").FillAsync("2026-12-31");
            await form.Locator("input[name=EndDate]").FillAsync("2026-01-01");
        }),
        new("E-54", "Reauthenticate", "", "yöneticinin yeniden doğrulama formu", Actor.Admin, _ => "/panel/yeniden-dogrulama?reauthenticatedOperation=EmailChange"),
    ];

    private static readonly Dictionary<string, FormCase> ByKey = AllCases.ToDictionary(c => c.Key);

    public static TheoryData<string, ScriptPath, string> Cases()
    {
        TheoryData<string, ScriptPath, string> data = [];
        foreach (string engine in BrowserHost.Engines)
        {
            foreach (ScriptPath path in Enum.GetValues<ScriptPath>())
            {
                foreach (FormCase form in AllCases)
                {
                    data.Add(engine, path, form.Key);
                }
            }
        }

        return data;
    }

    // ---- the forms of a page (derived from the page) ----

    private static readonly HashSet<string> PerRowHandlers = ["CreateCategory", "RenameCategory", "MoveCategory"];

    /// <summary>The keys (<see cref="FormCase.Key"/>) of the post forms of the page that have a field to fill (a hidden-only form such as a sign-out or a confirm has no state that can be invalid).</summary>
    internal static async Task<List<string>> FormKeysAsync(IPage page, string screen)
    {
        string json = await page.EvaluateAsync<string>("() => JSON.stringify([...document.querySelectorAll('form')].filter(f => (f.getAttribute('method') || 'get').toLowerCase() === 'post').map(f => ({ action: f.getAttribute('action') || '', id: f.id, fields: [...f.querySelectorAll('input,select,textarea')].filter(e => e.type !== 'hidden' && e.type !== 'submit' && e.type !== 'button' && !e.disabled).length })))");
        List<(string Handler, string Id, int Fields)> forms = [];
        using (JsonDocument document = JsonDocument.Parse(json))
        {
            foreach (JsonElement form in document.RootElement.EnumerateArray())
            {
                Match handler = Regex.Match(form.GetProperty("action").GetString() ?? string.Empty, @"handler=([A-Za-z]+)", RegexOptions.None, TimeSpan.FromSeconds(2));
                forms.Add((handler.Success ? handler.Groups[1].Value : string.Empty, form.GetProperty("id").GetString() ?? string.Empty, form.GetProperty("fields").GetInt32()));
            }
        }

        List<string> keys = [];
        foreach (IGrouping<string, (string Handler, string Id, int Fields)> byHandler in forms.GroupBy(f => f.Handler))
        {
            List<(string Handler, string Id, int Fields)> withoutId = [.. byHandler.Where(f => f.Id.Length == 0)];
            foreach ((string handler, string id, int fields) in byHandler)
            {
                if (fields == 0 || handler.Length == 0)
                {
                    continue;
                }

                // The category tree draws one form of each kind per category, and how many categories there are depends on what the other tests left: its forms are one case, whatever their count.
                string slot = id.Length > 0 ? id : withoutId.Count > 1 && !PerRowHandlers.Contains(handler) ? "@" + withoutId.IndexOf((handler, id, fields)) : string.Empty;
                keys.Add($"{screen}|{handler}|{slot}");
            }
        }

        return keys;
    }

    [Fact]
    public async Task EveryFormWithFieldsOfTheWalk_HasACase_AndEveryCaseIsInTheWalk_KK2()
    {
        string engine = BrowserHost.Engines[0];
        HashSet<string> found = [];
        foreach (IGrouping<Actor, ScreenInventoryTests.Stop> group in new ScreenInventoryTests(Host).Stops().GroupBy(s => s.Actor))
        {
            await using IBrowserContext context = await Host.ContextAsync(engine, ScriptPath.Scriptless, group.Key);
            foreach (ScreenInventoryTests.Stop stop in group)
            {
                WatchedPage page = await NewPageAsync(context);
                await GoAsync(page, stop.Path);
                // A stop that sends the visitor on (the re-verification of an account that has nothing to confirm) shows the landing screen's forms: the landing screen has its own stop.
                if (new Uri(page.Page.Url).AbsolutePath == new Uri(Host.BaseUrl + stop.Path).AbsolutePath)
                {
                    foreach (string key in await FormKeysAsync(page.Page, stop.Screen))
                    {
                        found.Add(key);
                    }
                }

                await page.Page.CloseAsync();
            }
        }

        List<string> withoutCase = [.. found.Where(k => !ByKey.ContainsKey(k)).Order(StringComparer.Ordinal)];
        List<string> withoutForm = [.. AllCases.Where(c => !c.FromFlow && !found.Contains(c.Key)).Select(c => c.Key).Order(StringComparer.Ordinal)];
        Assert.True(found.Count >= 40, $"The walk found {found.Count} forms with fields; at least 40 are expected.");
        Assert.True(withoutCase.Count == 0, "Forms of the walk with a field and no validation case: " + string.Join(", ", withoutCase));
        Assert.True(withoutForm.Count == 0, "Validation cases whose form is not in the walk's pages: " + string.Join(", ", withoutForm));
    }

    /// <summary>The keys of the forms of the screen that the walk's pages do not show (a flow opens them); the audit sends them too.</summary>
    internal static IEnumerable<string> FlowKeysOf(string screen) => AllCases.Where(c => c.FromFlow && c.Screen == screen).Select(c => c.Key);

    /// <summary>The keys of the forms that can answer with an error (every case but the forms that take any value, <see cref="NoValidationStateOf"/>): the accessibility pass scans the answer of each (T552, 12 12.2 (1)).</summary>
    internal static IEnumerable<string> ErrorStateKeys() => AllCases.Where(c => c.NoValidationState is null).Select(c => c.Key);

    /// <summary>The reason a form has no invalid state at all (04 says it takes any value), or null when a value can be not allowed; the audit's error message heading has nothing to read on such a form.</summary>
    internal static string? NoValidationStateOf(string key) => ByKey[key].NoValidationState;

    // ---- breaking a form ----

    private static string InvalidValueOf(string type, string name)
    {
        if (type == "email")
        {
            return "not-an-email";
        }

        if (type == "number")
        {
            return "-1";
        }

        if (type == "tel" || Regex.IsMatch(name, "Phone", RegexOptions.None, TimeSpan.FromSeconds(2)))
        {
            return "abc";
        }

        if (Regex.IsMatch(name, "Iban", RegexOptions.None, TimeSpan.FromSeconds(2)))
        {
            return "TR00";
        }

        if (Regex.IsMatch(name, "Url|Link", RegexOptions.None, TimeSpan.FromSeconds(2)))
        {
            return "not a link";
        }

        if (Regex.IsMatch(name, "Fee|Amount|Threshold|Rate|Period|Quantity|Percent|Value|Color|Number", RegexOptions.None, TimeSpan.FromSeconds(2)))
        {
            return "abc";
        }

        return string.Empty;
    }

    /// <summary>The generic break: every text field takes a value that is not allowed (an emptied field when no kind of value fits), a select goes to its empty option when it has one, a date is emptied, a ticked box stays.</summary>
    private static async Task BreakGenericAsync(ILocator form)
    {
        foreach (ILocator input in await form.Locator("input:not([type=hidden]):not([type=submit]):not([type=button]):not([type=checkbox]):not([type=radio]):not([type=file]):not([type=search]):not([disabled]):not([readonly])").AllAsync())
        {
            // A field the form keeps hidden until a choice (an IBAN before the method is chosen) or fixed (the invitation's address) is not one a person can send a wrong value in.
            if (!await input.IsVisibleAsync())
            {
                continue;
            }

            string type = await input.GetAttributeAsync("type") ?? "text";
            string name = await input.GetAttributeAsync("name") ?? string.Empty;
            await input.FillAsync(InvalidValueOf(type, name));
        }

        foreach (ILocator area in await form.Locator("textarea:not([disabled]):not([readonly])").AllAsync())
        {
            if (!await area.IsVisibleAsync())
            {
                continue;
            }

            await area.FillAsync(string.Empty);
        }

        foreach (ILocator select in await form.Locator("select:not([disabled])").AllAsync())
        {
            if (await select.IsVisibleAsync() && await select.Locator("option[value='']").CountAsync() > 0)
            {
                await select.SelectOptionAsync(new SelectOptionValue { Value = string.Empty });
            }
        }
    }

    private static ILocator FormOf(IPage page, FormCase form)
    {
        if (form.Slot.StartsWith('@'))
        {
            return page.Locator($"form[action*='handler={form.Handler}']:not([id])").Nth(int.Parse(form.Slot[1..], System.Globalization.CultureInfo.InvariantCulture));
        }

        return form.Slot.Length > 0 ? page.Locator($"form#{form.Slot}") : page.Locator($"form[action*='handler={form.Handler}']:not([id])").First;
    }

    // ---- flows that open a form the walk's pages do not show ----

    private static async Task SignInAsync(Run run, string path, string email, string typed, string form)
    {
        await run.Page.Page.Locator($"{form} [name='Input.Email']").FillAsync(email);
        await run.Page.Page.Locator($"{form} [name='Input.Password']").FillAsync(typed);
        await NavigateAsync(run.Page, () => run.Page.Page.Locator($"{form} button[type=submit]").First.ClickAsync());
        _ = path;
    }

    private static async Task ReauthenticateAsync(Run run, string address, string operation)
    {
        await run.Page.Page.GotoAsync(run.Host.BaseUrl + address + "?reauthenticatedOperation=" + operation);
        await run.Page.Page.Locator("input[type=password]").First.FillAsync("Dogru-Sifre-1");
        await NavigateAsync(run.Page, () => run.Page.Page.Locator("form button[type=submit]").Last.ClickAsync());
    }

    private static async Task CartWithAProductAsync(Run run, string address)
    {
        await run.Page.Page.GotoAsync(run.Host.BaseUrl + $"/urun/{run.Host.Seed.Product("plain").Slug}");
        await NavigateAsync(run.Page, () => run.Page.Page.Locator("form.buy-form button[type=submit]").First.ClickAsync());
        await run.Page.Page.GotoAsync(run.Host.BaseUrl + address);
    }

    private static async Task CustomerReopensAsync(Run run, string operation)
    {
        string email = $"v-{run.Unique}@example.com";
        await BrowserSeed.NewCustomerWithPasswordAsync(run.Host.Dsn, email, "Dogru-Sifre-1");
        await SignInAsync(run, "/giris", email, "Dogru-Sifre-1", "#sign-in-form");
        await ReauthenticateAsync(run, "/hesap/yeniden-dogrulama", operation);
    }

    private static async Task AdminReopensAsync(Run run, string operation)
    {
        string email = $"v-{run.Unique}@example.com";
        await BrowserSeed.NewAdminWithPasswordAsync(run.Host.Dsn, email, "Dogru-Sifre-1");
        await SignInAsync(run, "/panel/giris", email, "Dogru-Sifre-1", "main form");
        await ReauthenticateAsync(run, "/panel/yeniden-dogrulama", operation);
    }

    private static int _unique;

    private async Task<string> FingerprintAsync(string? table) =>
        table is null ? string.Empty : await DataRecord.RowAsync(Host.Dsn, $"SELECT count(*) AS rows, md5(coalesce(string_agg(t::text, ',' ORDER BY t::text), '')) AS fingerprint FROM {table} t");

    /// <summary>The state a broken form's answer is in (what the answer drew); <paramref name="Inspection"/> is what the caller's inspection of the answering page returned.</summary>
    internal sealed record Probe(string Key, List<int> Posts, int Summary, int Invalid, int Messages, string Fingerprint, bool Unchanged, string Inspection, bool InspectionOk, WatchedPage Page);

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task TheFormSentWithAValueThatIsNotAllowed_Gives422WithAnInvalidField_AndWritesNothing_KK2(string engine, ScriptPath path, string key)
    {
        FormCase form = ByKey[key];
        await using IBrowserContext context = await Host.ContextAsync(engine, path, form.Actor);
        await ObserveProbeAsync(engine, path, form, context, null);
        Assert.Empty(Failures);
    }

    /// <summary>Sends the case's form with a value that is not allowed in the context and lets <paramref name="inspect"/> read the page that answers (the manual audit's error message heading, T552).</summary>
    internal async Task<Probe> ProbeAsync(string engine, ScriptPath path, string key, Func<WatchedPage, Task<(bool Ok, string Text)>> inspect)
    {
        FormCase form = ByKey[key];
        await using IBrowserContext context = await AccessibilityScanTests.AuditContextAsync(Host, engine, path, form.Actor);
        return await ObserveProbeAsync(engine, path, form, context, inspect);
    }

    private async Task<Probe> ObserveProbeAsync(string engine, ScriptPath path, FormCase form, IBrowserContext context, Func<WatchedPage, Task<(bool Ok, string Text)>>? inspect)
    {
        WatchedPage page = await NewPageAsync(context);
        List<int> posts = [];
        page.Page.Response += (_, r) =>
        {
            if (r.Request.Method == "POST" && r.Request.ResourceType == "document")
            {
                posts.Add(r.Status);
            }
        };
        await GoAsync(page, form.Path(Host));
        if (form.Prepare is not null)
        {
            await form.Prepare(new Run(page, Host, $"{Interlocked.Increment(ref _unique)}-{engine}-{path}".ToLowerInvariant()));
            posts.Clear();
        }

        ILocator target = FormOf(page.Page, form);
        string before = await FingerprintAsync(form.Watch ?? form.Table);
        // What is observed is the server's answer to the value, not the browser's own constraint bubble (which differs among the engines and stops the send for an e-mail or a number field): the form is sent as the scriptless browser of a client without the check would.
        // A form inside a closed disclosure (the category tree's actions) is opened first: a person opens it to type in it.
        await target.EvaluateAsync("(f) => { f.noValidate = true; for (let d = f.closest('details'); d; d = d.parentElement ? d.parentElement.closest('details') : null) { d.open = true; } }");
        await (form.Break ?? BreakGenericAsync)(target);
        ILocator button = form.Button is null
            ? target.Locator("button[type=submit]").First
            : form.Button.StartsWith("css:", StringComparison.Ordinal) ? target.Locator(form.Button[4..]).First : target.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = form.Button });
        // The answer's markup is what is read (the counts below), so the wait ends when the answering document is taken over and its markup parsed, not when every picture and script of it has finished: in Firefox the "load" of
        // the answer to a send did not come within the wait now and then (T552 CI 44135791843; D-1740 (b)), whereas the commit is the browser's own sign that the server has answered.
        await NavigateAsync(page, () => button.ClickAsync(), 30000, WaitUntilState.Commit);
        int summary = await page.Page.Locator("[data-js=error-summary]").CountAsync();
        int invalid = await page.Page.Locator("[aria-invalid=true]").CountAsync();
        // The order screen's forms (E-37) answer in the place of the form with the error message of the form (04 2.3.1 "in place") and have no field of their own to mark; the count of those messages is the other kind of sign.
        int messages = await page.Page.Locator("main .message--error").CountAsync();
        string after = await FingerprintAsync(form.Watch ?? form.Table);
        if (inspect is not null)
        {
            (bool inspectionOk, string inspection) = await inspect(page);
            return new Probe(form.Key, posts, summary, invalid, messages, after, before == after, inspection, inspectionOk, page);
        }

        string state = $"gönderim durumu {string.Join(",", posts)}; hata özeti {summary}; geçersiz alan {invalid}; hata iletisi {messages}; {form.Table ?? "tablo"} {(before == after ? "değişmedi" : "DEĞİŞTİ")}";
        if (form.NoValidationState is not null)
        {
            await ObserveAsync(
                form.Screen,
                path,
                engine,
                page,
                $"{form.Form}: doğrulama hâli yok ({form.NoValidationState}); izin verilmeyen değer yoktur, en boş hâliyle gönderilir",
                "gönderim kabul edilir (422 değil); hata gösterilmez; D-1712",
                state,
                posts.Count > 0 && posts[0] != 422 && invalid == 0 && messages == 0,
                after);
            Assert.Empty(Failures);
            return new Probe(form.Key, posts, summary, invalid, messages, after, before == after, string.Empty, true, page);
        }

        await ObserveAsync(
            form.Screen,
            path,
            engine,
            page,
            $"{form.Form}: bir değer izin verilmeyen biçimde gönderilir",
            "gönderim 422 döner; geçersiz alan (alanın kendi iletisiyle) ya da formun yerinde hata iletisi sayfadadır; kayıt değişmez",
            state,
            posts.Count > 0 && posts[^1] == 422 && (invalid > 0 || messages > 0) && before == after,
            after);
        Assert.Empty(Failures);
        return new Probe(form.Key, posts, summary, invalid, messages, after, before == after, string.Empty, true, page);
    }
}
