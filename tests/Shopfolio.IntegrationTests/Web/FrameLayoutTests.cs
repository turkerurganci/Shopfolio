using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The layouts of the frame on the opened application (T540 KK3-KK9; 04 2.2, 2.9; 07 5.5.1; 09 16.1): the real storefront, panel and bare layouts drawn around one-line sample pages. The model
/// of the frame is T537's and is tested in <see cref="StorefrontFrameTests"/>; here the markup of the regions - their order, what each carries, who sees the strip and the band - is read from
/// the response. The collection database is the same as the model tests'.
/// </summary>
public sealed partial class StorefrontFrameTests
{
    private static readonly TimeSpan PatternTimeout = TimeSpan.FromSeconds(5);

    private static async Task<(HttpStatusCode Status, string Html, HttpResponseMessage Response)> GetHtmlAsync(HttpClient client, string path, params string[] cookies)
    {
        HttpRequestMessage request = new(HttpMethod.Get, path);

        // The sign-out form carries the forgery token, which needs the secure scheme the reverse proxy reports (05 6.3.3).
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        if (cookies.Length > 0)
        {
            request.Headers.Add("Cookie", string.Join("; ", cookies));
        }

        HttpResponseMessage response = await client.SendAsync(request, Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)), response);
    }

    private static int IndexOfRegion(string html, string marker)
    {
        int index = html.IndexOf(marker, StringComparison.Ordinal);
        Assert.True(index >= 0, $"The response has no region marked '{marker}'.");
        return index;
    }

    private async Task SeedPublishedAnnouncementAsync()
    {
        await using AppDbContext db = NewContext();
        Announcement announcement = Announcement.Create(Now);
        announcement.Text = "Duyuru metni";
        announcement.PublishStatus = PublishStatus.Published;
        db.Announcements.Add(announcement);
        await db.SaveChangesAsync(Ct);
    }

    // ---- KK5: the five regions in their order, for the administrator and for the customer ----

    [Fact]
    public async Task TheStorefrontLayout_DrawsTheFiveRegionsInOrder_AndTheStripOnlyWithAPanelSession_T540_KK5_KK9()
    {
        await SeedPublishedAnnouncementAsync();
        (string adminKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        (string customerKey, _) = await TestSessions.CustomerAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        (HttpStatusCode status, string html, _) = await GetHtmlAsync(client, "/sss/cerceve-ornegi", Admin(adminKey));

        Assert.Equal(HttpStatusCode.OK, status);
        int strip = IndexOfRegion(html, "class=\"admin-strip\"");
        int announcement = IndexOfRegion(html, "class=\"announcement\"");
        int header = IndexOfRegion(html, "<header class=\"site-header");
        int menu = IndexOfRegion(html, "class=\"site-header__menu\"");
        int content = IndexOfRegion(html, "id=\"sample-content\"");
        int footer = IndexOfRegion(html, "<footer class=\"site-footer\"");
        Assert.True(strip < announcement && announcement < header && header < menu && menu < content && content < footer);
        Assert.Contains("<a href=\"/panel\">Panele dön</a>", html, StringComparison.Ordinal);
        Assert.DoesNotContain("draft-band", html, StringComparison.Ordinal);

        // A visitor and a customer see no strip at all; the customer's cookie does not open it.
        string visitor = (await GetHtmlAsync(client, "/sss/cerceve-ornegi")).Html;
        string customer = (await GetHtmlAsync(client, "/sss/cerceve-ornegi", Customer(customerKey))).Html;
        Assert.DoesNotContain("admin-strip", visitor, StringComparison.Ordinal);
        Assert.DoesNotContain("Panele dön", customer, StringComparison.Ordinal);
        Assert.Contains("Duyuru metni", visitor, StringComparison.Ordinal);

        // The firm has not entered an ETBIS verification address: no band on any screen (VAL-A128).
        Assert.DoesNotContain("etbis-band", html, StringComparison.Ordinal);
        Assert.DoesNotContain("etbis-band", visitor, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheMenuAndTheAccountAreaAndTheSearchBox_AreInsideTheFoldOutPanel_OfTheHeader_T540_KK5()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string html = (await GetHtmlAsync(client, "/sss/cerceve-ornegi")).Html;

        int panelStart = IndexOfRegion(html, "id=\"site-header-panel\"");
        int search = IndexOfRegion(html, "site-header__search");
        int account = IndexOfRegion(html, "site-header__account");
        int toggle = IndexOfRegion(html, "data-js=\"menu-toggle\"");
        Assert.True(toggle < panelStart && panelStart < search && panelStart < account);
        Assert.Contains("aria-controls=\"site-header-panel\"", html, StringComparison.Ordinal);
        Assert.Matches(new Regex("<button[^>]*data-js=\"menu-toggle\"[^>]*\\shidden", RegexOptions.CultureInvariant, PatternTimeout), html);
        Assert.Contains("href=\"/giris?returnUrl=%2Fsss\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheProductsGroupOfTheMenu_IsAButtonWithAriaState_ThatControlsTheCategoryTree_T540_KK2_KK4()
    {
        await using (AppDbContext db = NewContext())
        {
            Category category = Category.Create("Mobilya", "mobilya", null, 1, Now);
            Product product = Product.Create("Masa", "masa", ProductType.Physical, 20m, Now).WithSeededOrigin();
            product.PublishStatus = PublishStatus.Published;
            db.Categories.Add(category);
            db.Products.Add(product);
            db.ProductCategories.Add(ProductCategory.Create(product.Id, category.Id, true, Now));
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string html = (await GetHtmlAsync(client, "/sss/cerceve-ornegi")).Html;

        // The group's tree is reachable by keyboard: a real button (hidden until common.js runs), expanded state and the id of the panel it controls.
        Match button = Regex.Match(html, "<button[^>]*data-js=\"menu-tree-toggle\"[^>]*>", RegexOptions.CultureInvariant, PatternTimeout);
        Assert.True(button.Success, "The Ürünler group has no toggle button.");
        Assert.Contains("type=\"button\"", button.Value, StringComparison.Ordinal);
        Assert.Contains("aria-expanded=\"false\"", button.Value, StringComparison.Ordinal);
        Assert.Contains("aria-controls=\"site-menu-tree\"", button.Value, StringComparison.Ordinal);
        Assert.Matches(new Regex("\\shidden[\\s>]", RegexOptions.CultureInvariant, PatternTimeout), button.Value);
        Assert.Contains("<div class=\"site-menu__panel\" id=\"site-menu-tree\">", html, StringComparison.Ordinal);
        Assert.Matches(new Regex("id=\"site-menu-tree\">\\s*<ul class=\"site-menu__tree\">\\s*<li>\\s*<a href=\"[^\"]*mobilya\">Mobilya</a>", RegexOptions.CultureInvariant, PatternTimeout), html);
    }

    // ---- KK6: the footer ----

    [Fact]
    public async Task TheFooter_IsWholeOnEveryScreen_TheContactHeadingLeadsToE10_AndOnlyPublishedLegalTextsAreLinked_T540_KK6()
    {
        await using (AppDbContext db = NewContext())
        {
            await db.Companies.ExecuteUpdateAsync(c => c.SetProperty(x => x.EtbisVerificationUrl, "https://etbis.example/dogrula"), Ct);
            db.LegalTextVersions.Add(LegalTextVersion.CreatePublished(LegalTextType.CookiePolicy, 1, Now, "metin", Now));
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        foreach (string path in new[] { "/sss/cerceve-ornegi", "/odeme?cizim=1" })
        {
            string html = (await GetHtmlAsync(client, path)).Html;
            Assert.Contains("<h2 class=\"site-footer__heading\" id=\"footer-contact-heading\"><a href=\"/iletisim\">İletişim</a></h2>", html, StringComparison.Ordinal);
            Assert.Contains("href=\"/cerez-politikasi\"", html, StringComparison.Ordinal);
            Assert.Contains("href=\"/islem-rehberi\"", html, StringComparison.Ordinal);
            Assert.DoesNotContain("href=\"/gizlilik-bildirimi\"", html, StringComparison.Ordinal);
            Assert.Contains("class=\"etbis-band\"", html, StringComparison.Ordinal);
            Assert.DoesNotContain("<details", html, StringComparison.Ordinal);
            Assert.DoesNotContain("data-js=\"footer", html, StringComparison.Ordinal);
        }
    }

    // ---- KK7: the payment step's header ----

    [Fact]
    public async Task ThePaymentStepHeader_CarriesTheBrandAndBackToTheCartOnly_T540_KK7()
    {
        await SeedPublishedAnnouncementAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string checkout = (await GetHtmlAsync(client, "/odeme?cizim=1")).Html;

        Assert.Contains("site-header--checkout", checkout, StringComparison.Ordinal);
        Assert.Contains("<a class=\"site-header__back\" href=\"/sepet\">Sepete dön</a>", checkout, StringComparison.Ordinal);
        Assert.DoesNotContain("site-header__menu", checkout, StringComparison.Ordinal);
        Assert.DoesNotContain("site-header__search", checkout, StringComparison.Ordinal);
        Assert.DoesNotContain("site-header__social", checkout, StringComparison.Ordinal);
        Assert.DoesNotContain("site-header__account", checkout, StringComparison.Ordinal);
        Assert.DoesNotContain("data-js=\"menu-toggle\"", checkout, StringComparison.Ordinal);
        Assert.DoesNotContain("class=\"announcement\"", checkout, StringComparison.Ordinal);
        Assert.Contains("<footer class=\"site-footer\"", checkout, StringComparison.Ordinal);
    }

    // ---- KK9: the draft band takes the strip's place ----

    [Fact]
    public async Task TheDraftBand_TakesTheStripsPlace_LeadsBackToTheForm_AndTheCustomerNeverSeesIt_T540_KK9()
    {
        (string adminKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        (string customerKey, _) = await TestSessions.CustomerAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string admin = (await GetHtmlAsync(client, "/iletisim/taslak-ornegi", Admin(adminKey))).Html;
        string customer = (await GetHtmlAsync(client, "/iletisim/taslak-ornegi", Customer(customerKey))).Html;
        string visitor = (await GetHtmlAsync(client, "/iletisim/taslak-ornegi")).Html;

        Assert.Contains("class=\"draft-band\"", admin, StringComparison.Ordinal);
        Assert.Contains("<a href=\"/panel/urunler/ornek-kayit\">Düzenlemeye dön</a>", admin, StringComparison.Ordinal);
        Assert.Contains("<span class=\"draft-band__label\">Taslak</span>", admin, StringComparison.Ordinal);
        Assert.DoesNotContain("admin-strip", admin, StringComparison.Ordinal);
        foreach (string html in new[] { customer, visitor })
        {
            Assert.DoesNotContain("draft-band", html, StringComparison.Ordinal);
            Assert.DoesNotContain("Düzenlemeye dön", html, StringComparison.Ordinal);
            Assert.DoesNotContain("admin-strip", html, StringComparison.Ordinal);
        }
    }

    // ---- The sign-in merge message (D-1132) ----

    [Fact]
    public async Task TheMergeMessage_IsDrawnWithTheProductNames_ThenGone_T540_D1132()
    {
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        List<Variant> variants = await SeedVariantsAsync(2);
        Guid cartId = await SeedCartAsync(customerId, null, variants[0].Id, variants[1].Id);
        await using (AppDbContext db = NewContext())
        {
            await db.CartItems.Where(i => i.CartId == cartId && i.VariantId == variants[0].Id).ExecuteUpdateAsync(i => i.SetProperty(x => x.IsMergeNoticePending, true), Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string first = (await GetHtmlAsync(client, "/sss/cerceve-ornegi", Customer(customerKey))).Html;
        string second = (await GetHtmlAsync(client, "/sss/cerceve-ornegi", Customer(customerKey))).Html;

        Assert.Contains("role=\"status\" data-js=\"merge-notice\"", first, StringComparison.Ordinal);
        Assert.Contains("Giriş yapmadan önce sepetinize eklediğiniz ürünler hesabınızın sepetine eklendi", first, StringComparison.Ordinal);
        Assert.Contains("Urun ", first, StringComparison.Ordinal);
        Assert.DoesNotContain("data-js=\"merge-notice\"", second, StringComparison.Ordinal);
    }

    // ---- KK3, KK4: one style block with the nonce; no style attribute, inline script or event attribute; the head ----

    [Fact]
    public async Task TheLayouts_HaveOneNoncedStyleBlock_NoInlineScriptNoStyleAttributeNoEventAttribute_AndTheStaticFilesUnderStatik_T540_KK3_KK4()
    {
        (string adminKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        foreach ((string path, string[] cookies) in new (string, string[])[]
        {
            ("/sss/cerceve-ornegi", []), ("/odeme?cizim=1", []), ("/iletisim/taslak-ornegi", [Admin(adminKey)]), ("/panel/urunler/x", [Admin(adminKey)]), ("/panel/davet/abc", []),
        })
        {
            (HttpStatusCode status, string html, HttpResponseMessage response) = await GetHtmlAsync(client, path, cookies);

            Assert.Equal(HttpStatusCode.OK, status);
            string csp = string.Join(";", response.Headers.GetValues("Content-Security-Policy"));
            string nonce = Regex.Match(csp, "'nonce-([^']+)'", RegexOptions.CultureInvariant, PatternTimeout).Groups[1].Value;
            Assert.NotEmpty(nonce);

            Assert.DoesNotMatch(new Regex("\\sstyle=", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, PatternTimeout), html);
            Assert.DoesNotMatch(new Regex("\\son[a-z]+=", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, PatternTimeout), html);
            Assert.DoesNotMatch(new Regex("<script(?![^>]*\\ssrc=)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, PatternTimeout), html);
            MatchCollection styles = Regex.Matches(html, "<style[^>]*>", RegexOptions.CultureInvariant, PatternTimeout);
            if (path.StartsWith("/panel", StringComparison.Ordinal))
            {
                Assert.Empty(styles);
                Assert.DoesNotContain("--brand-color", html, StringComparison.Ordinal);
            }
            else
            {
                Assert.Equal($"<style nonce=\"{nonce}\">", Assert.Single(styles).Value);
                Assert.Contains("--brand-color:#", html, StringComparison.Ordinal);
            }

            // Every style sheet and script is a fingerprinted or plain address under /statik, and the file answers.
            foreach (Match match in Regex.Matches(html, "(?:href|src)=\"(/statik/[^\"]+)\"", RegexOptions.CultureInvariant, PatternTimeout))
            {
                using HttpResponseMessage file = await client.GetAsync(match.Groups[1].Value, Ct);
                Assert.Equal(HttpStatusCode.OK, file.StatusCode);
            }

            Assert.Matches(new Regex("href=\"/statik/css/base\\.[a-z0-9]{10}\\.css\"", RegexOptions.CultureInvariant, PatternTimeout), html);
        }
    }

    [Fact]
    public async Task TheLayouts_CloseTheClosedPagesToSearchEngines_AndLoadTheStyleSheetsInTheDocumentedOrder_T540_KK4()
    {
        (string adminKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string panel = (await GetHtmlAsync(client, "/panel/urunler/x", Admin(adminKey))).Html;
        string bare = (await GetHtmlAsync(client, "/panel/davet/abc")).Html;
        string storefront = (await GetHtmlAsync(client, "/sss/cerceve-ornegi")).Html;
        string checkout = (await GetHtmlAsync(client, "/odeme?cizim=1")).Html;

        Assert.Contains("<meta name=\"robots\" content=\"noindex\">", panel, StringComparison.Ordinal);
        Assert.Contains("<meta name=\"robots\" content=\"noindex\">", bare, StringComparison.Ordinal);
        Assert.Contains("<meta name=\"robots\" content=\"noindex\">", checkout, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"robots\"", storefront, StringComparison.Ordinal);
        Assert.True(storefront.IndexOf("/css/base.", StringComparison.Ordinal) < storefront.IndexOf("/css/components.", StringComparison.Ordinal));
        Assert.True(storefront.IndexOf("/css/components.", StringComparison.Ordinal) < storefront.IndexOf("/css/storefront.", StringComparison.Ordinal));
        Assert.DoesNotContain("/css/admin.", storefront, StringComparison.Ordinal);
        Assert.True(panel.IndexOf("/css/components.", StringComparison.Ordinal) < panel.IndexOf("/css/admin.", StringComparison.Ordinal));
        Assert.DoesNotContain("/css/storefront.", panel, StringComparison.Ordinal);
    }

    // ---- KK8: the panel frame ----

    [Fact]
    public async Task ThePanelLayout_DrawsTheTopRowAndTheEightEntries_WithTheActiveGroupAndTheSalesIndicator_T540_KK8()
    {
        (string adminKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string html = (await GetHtmlAsync(client, "/panel/urunler/abc", Admin(adminKey))).Html;

        Assert.Contains("class=\"panel-top__sales panel-top__sales--closed\" href=\"/panel/firma-kimligi-ve-satis#satis-durumu\">Satış kapalı</a>", html, StringComparison.Ordinal);
        Assert.Contains("<a class=\"panel-top__site\" href=\"/\">Siteyi görüntüle</a>", html, StringComparison.Ordinal);
        Assert.Contains("href=\"/panel/hesabim\">Hesabım</a>", html, StringComparison.Ordinal);
        Assert.Contains("<form method=\"post\" action=\"/panel/cikis?handler=SignOut\">", html, StringComparison.Ordinal);
        Assert.Contains(">Çıkış</button>", html, StringComparison.Ordinal);
        Assert.Contains("class=\"panel-top__name u-single-line\"", html, StringComparison.Ordinal);

        string[] top = ["Ana sayfa", "Siparişler", "Talepler", "Üyeler"];
        string[] groups = ["Katalog", "İçerik ve site", "Raporlar", "Ayarlar"];
        foreach (string entry in top)
        {
            Assert.Contains($"<span>{entry}</span>", html, StringComparison.Ordinal);
        }

        MatchCollection groupMatches = Regex.Matches(html, "<h2 class=\"panel-menu__group-title\">([^<]+)</h2>", RegexOptions.CultureInvariant, PatternTimeout);
        Assert.Equal(groups, groupMatches.Select(m => m.Groups[1].Value).ToArray());
        Assert.Equal(8, top.Length + groups.Length);

        // The path is under Ürünler: that item is current, its group is the active one, and the others are not.
        Assert.Single(Regex.Matches(html, "data-js=\"menu-group\" data-active=\"true\"", RegexOptions.CultureInvariant, PatternTimeout));
        Assert.Single(Regex.Matches(html, "aria-current=\"page\"", RegexOptions.CultureInvariant, PatternTimeout));
        Assert.Matches(new Regex("href=\"/panel/urunler\" aria-current=\"page\">Ürünler</a>", RegexOptions.CultureInvariant, PatternTimeout), html);
        Assert.Equal(3, Regex.Count(html, "data-active=\"false\"", RegexOptions.CultureInvariant, PatternTimeout));
        Assert.Contains("aria-controls=\"panel-menu\"", html, StringComparison.Ordinal);
        Assert.Contains("id=\"panel-menu\"", html, StringComparison.Ordinal);

        // The panel has no brand colour, identity block, ETBIS band or storefront regions.
        foreach (string absent in new[] { "brand-color", "identity-block", "etbis-band", "site-footer", "site-header", "announcement" })
        {
            Assert.DoesNotContain(absent, html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task ThePanelMenu_MarksOrdersAndRequestsWhileWorkIsPending_AndTheSalesIndicatorFollowsTheGate_T540_KK8()
    {
        (string adminKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        await using WebApplicationFactory<Program> none = Open();
        using HttpClient noneClient = none.CreateClient();
        string without = (await GetHtmlAsync(noneClient, "/panel/urunler/abc", Admin(adminKey))).Html;
        Assert.DoesNotContain("pending-mark", without, StringComparison.Ordinal);

        await using WebApplicationFactory<Program> pending = Open().WithWebHostBuilder(b => b.ConfigureServices(s =>
        {
            s.RemoveAll<IPanelHome>();
            s.AddScoped<IPanelHome, PendingHome>();
        }));
        using HttpClient client = pending.CreateClient();
        string html = (await GetHtmlAsync(client, "/panel/urunler/abc", Admin(adminKey))).Html;

        Assert.Equal(2, Regex.Count(html, "<span class=\"pending-mark\">Bekleyen iş</span>", RegexOptions.CultureInvariant, PatternTimeout));
        Assert.Matches(new Regex("<span>Siparişler</span>\\s*<span class=\"pending-mark\">", RegexOptions.CultureInvariant, PatternTimeout), html);
        Assert.Matches(new Regex("<span>Talepler</span>\\s*<span class=\"pending-mark\">", RegexOptions.CultureInvariant, PatternTimeout), html);
    }

    private sealed class PendingHome : IPanelHome
    {
        public Task<PanelHome> GetAsync(CancellationToken ct) =>
            Task.FromResult(new PanelHome(
                [new PanelCounter(PanelCounterKind.ToShip, 2, default), new PanelCounter(PanelCounterKind.OpenRequest, 1, default)],
                false,
                false,
                null));
    }

    [Fact]
    public async Task TheScreensBeforeTheSession_HaveNoPanelFrame_OnlyTheProductsPanelTitle_T540_KK8()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string html = (await GetHtmlAsync(client, "/panel/davet/abc")).Html;

        Assert.Contains("<h1 class=\"panel-bare__title\">Shopfolio Panel</h1>", html, StringComparison.Ordinal);
        Assert.Contains("id=\"sample-content\"", html, StringComparison.Ordinal);
        foreach (string absent in new[] { "panel-top", "panel-menu", "Siteyi görüntüle", "Hesabım", "Çıkış", "brand-color", "site-header" })
        {
            Assert.DoesNotContain(absent, html, StringComparison.Ordinal);
        }
    }
}
