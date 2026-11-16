using Microsoft.Playwright;

namespace Shopfolio.IntegrationTests.Browser;

/// <summary>
/// The validation-error state of the inventory's forms (T551 criterion 2, KK2; 04 2.3): each form is sent with its required fields empty (or one value that is not allowed) in each engine and
/// each way; the answer is 422 with the error summary and an invalid field on the page, and the table the form would write is the same as before.
/// </summary>
public sealed class ValidationStateTests(BrowserHost host) : BrowserTestBase(host)
{
    /// <summary>One form: the screen, the address, who opens it, a step before the send, the send's button and the table that must not change.</summary>
    internal sealed record FormCase(string Screen, string Form, Actor Actor, string Path, string Button, string Table, Func<IPage, Task>? Prepare = null);

    public static TheoryData<string, ScriptPath, string> Cases()
    {
        TheoryData<string, ScriptPath, string> data = [];
        foreach (string engine in BrowserHost.Engines)
        {
            foreach (ScriptPath path in Enum.GetValues<ScriptPath>())
            {
                foreach (string name in FormNames)
                {
                    data.Add(engine, path, name);
                }
            }
        }

        return data;
    }

    private static readonly string[] FormNames = ["E-10", "E-15", "E-20", "E-22", "E-23", "E-29", "E-34", "E-33", "E-41", "E-45"];

    private static FormCase CaseOf(string screen) => screen switch
    {
        "E-10" => new("E-10", "iletişim formu", Actor.Visitor, "/iletisim", "form.contact-form button[type=submit]", "contact_requests"),
        "E-15" => new("E-15", "sipariş takibi formu", Actor.Visitor, "/siparis-takibi", "form button[type=submit] >> nth=-1", "orders"),
        "E-20" => new("E-20", "kayıt formu", Actor.Visitor, "/kayit", "#registration-form button[type=submit]", "customers"),
        "E-22" => new("E-22", "müşteri giriş formu", Actor.Visitor, "/giris", "#sign-in-form button[type=submit]", "customers"),
        "E-23" => new("E-23", "şifre sıfırlama isteği formu", Actor.Visitor, "/sifre-sifirlama", "#reset-request-form button[type=submit]", "customers"),
        "E-29" => new("E-29", "panel giriş formu", Actor.Visitor, "/panel/giris", "main form button[type=submit]", "admins"),
        "E-34" => new("E-34", "yeni kategori formu", Actor.Admin, "/panel/kategoriler", "#yeni-kategori button[type=submit]", "categories"),
        "E-33" => new("E-33", "ürün formu (yeni ürün)", Actor.Admin, "/panel/urunler/yeni", "#product-save >> role=button[name='Kaydet']", "products"),
        "E-41" => new("E-41", "kurumsal içerik formu (yeni hizmet)", Actor.Admin, "/panel/kurumsal-icerik/hizmet/yeni", "#content-save >> role=button[name='Kaydet']", "offerings"),
        "E-45" => new("E-45", "ödeme yöntemleri formu (havale açık, IBAN boş)", Actor.Admin, "/panel/odeme-yontemleri", "main form >> role=button[name='Kaydet']", "company_settings", async page =>
        {
            await page.Locator("#transfer-switch").CheckAsync();
            await page.Locator("[name='Input.BankTransferIban']").FillAsync(string.Empty);
        }),
        _ => throw new ArgumentOutOfRangeException(nameof(screen), screen, "Unknown form."),
    };

    private async Task<string> FingerprintAsync(string table) =>
        await DataRecord.RowAsync(Host.Dsn, $"SELECT count(*) AS rows, md5(coalesce(string_agg(t::text, ',' ORDER BY t::text), '')) AS fingerprint FROM {table} t");

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task TheFormSentEmpty_Gives422WithTheErrorSummary_AndWritesNothing_KK2(string engine, ScriptPath path, string screen)
    {
        FormCase form = CaseOf(screen);
        await using IBrowserContext context = await Host.ContextAsync(engine, path, form.Actor);
        WatchedPage page = await NewPageAsync(context);
        List<int> posts = [];
        page.Page.Response += (_, r) =>
        {
            if (r.Request.Method == "POST" && r.Request.ResourceType == "document")
            {
                posts.Add(r.Status);
            }
        };
        await GoAsync(page, form.Path);
        if (form.Prepare is not null)
        {
            await form.Prepare(page.Page);
        }

        string before = await FingerprintAsync(form.Table);
        await NavigateAsync(page, () => page.Page.Locator(form.Button).ClickAsync());
        int summary = await page.Page.Locator("[data-js=error-summary]").CountAsync();
        int invalid = await page.Page.Locator("[aria-invalid=true]").CountAsync();
        string after = await FingerprintAsync(form.Table);
        await ObserveAsync(
            form.Screen,
            path,
            engine,
            page,
            $"{form.Form}: zorunlu alanlar boş gönderilir",
            "gönderim 422 döner; geçersiz alan (alanın kendi iletisiyle) sayfadadır, çok alanlı formlarda hata özeti de; kayıt değişmez",
            $"gönderim durumu {string.Join(",", posts)}; hata özeti {summary}; geçersiz alan {invalid}; {form.Table} {(before == after ? "değişmedi" : "DEĞİŞTİ")}",
            posts.Count > 0 && posts[0] == 422 && invalid > 0 && before == after,
            after);
        Assert.Empty(Failures);
    }
}
