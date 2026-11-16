using Microsoft.Playwright;

namespace Shopfolio.IntegrationTests.Browser;

/// <summary>
/// The walk over the screen inventory (T551 criterion 2, KK2; 04 4.1, 4.2; 12 12.2 (1)): every screen of the inventory is opened in each engine and each way
/// - the script on and off - at its seeded address, and each line of the record names the screen, its address, the status the page answered and the browser's own
/// error messages. A screen with states (the empty list, the error, the unknown address) has one line for each state the walk reaches. The screens that need a
/// record the seed does not carry (a verification link, an invitation) are opened at their invalid-link state, which is a state of the screen (04 5.21).
/// </summary>
public sealed class ScreenInventoryTests(BrowserHost host) : BrowserTestBase(host)
{
    /// <summary>One address of the walk: the inventory id, the state it shows, who opens it and the status it must answer.</summary>
    internal sealed record Stop(string Screen, string State, Actor Actor, string Path, int Status = 200);

    /// <summary>The inventory's screens E-01...E-54 (E-48 is not a screen, 04 4.3) with the states the walk opens each in.</summary>
    internal IReadOnlyList<Stop> Stops()
    {
        BrowserSeedData seed = Host.Seed;
        SeededOrder awaiting = seed.Order("awaiting-transfer");
        SeededOrder paid = seed.Order("paid-open");
        SeededOrder delivered = seed.Order("delivered");
        SeededOrder closed = seed.Order("closed");
        SeededOrder member = seed.Order("member-paid");
        SeededOrder shipped = seed.Order("shipped");
        SeededOrder failed = seed.Order("delivery-failed");
        SeededOrder partial = seed.Order("partially-refunded");
        SeededOrder refunded = seed.Order("refunded");
        string plain = seed.Product("plain").Slug;
        string variants = seed.Product("variants").Slug;
        const string unknownKey = "T551-yok-baglanti-anahtari";
        List<Stop> stops =
        [
            // Customer side (04 4.1).
            new("E-01", "ana sayfa", Actor.Visitor, "/"),
            new("E-02", "kategori sayfası", Actor.Visitor, $"/kategori/{seed.CategorySlug}"),
            new("E-02", "kategori sayfası, bilinmeyen kategori (E-06 durumu)", Actor.Visitor, "/kategori/yok", 404),
            new("E-03", "arama sonuçları, sonuçlu", Actor.Visitor, "/arama?search=ürün"),
            new("E-03", "arama sonuçları, sonuçsuz", Actor.Visitor, "/arama?search=zzzzzz"),
            new("E-04", "ürün sayfası, tek varyant", Actor.Visitor, $"/urun/{plain}"),
            new("E-04", "ürün sayfası, iki seçenekli", Actor.Visitor, $"/urun/{variants}"),
            new("E-05", "arşivlenmiş ürün sayfası", Actor.Visitor, $"/urun/{seed.Product("archived").Slug}"),
            new("E-06", "sayfa bulunamadı", Actor.Visitor, "/urun/yok", 404),
            new("E-07", "içerik liste sayfası, hizmetler", Actor.Visitor, "/hizmetler"),
            new("E-07", "içerik liste sayfası, referanslar", Actor.Visitor, "/referanslar"),
            new("E-08", "içerik sayfası, hakkımızda", Actor.Visitor, "/hakkimizda"),
            new("E-08", "içerik sayfası, hizmet tanıtımı", Actor.Visitor, $"/hizmet/{seed.OfferingSlug}"),
            new("E-08", "içerik sayfası, referans iş", Actor.Visitor, $"/referans/{seed.PortfolioSlug}"),
            new("E-08", "içerik sayfası, genel sayfa", Actor.Visitor, $"/sayfa/{seed.PageSlug}"),
            new("E-09", "sık sorulan sorular", Actor.Visitor, "/sss"),
            new("E-10", "iletişim sayfası ve formu", Actor.Visitor, "/iletisim"),
            new("E-11", "yasal metin, aydınlatma metni", Actor.Visitor, "/aydinlatma-metni"),
            new("E-11", "yasal metin, çerez politikası", Actor.Visitor, "/cerez-politikasi"),
            new("E-11", "yasal metin, işlem rehberi", Actor.Visitor, "/islem-rehberi"),
            new("E-12", "sepet, boş", Actor.Visitor, "/sepet"),
            new("E-13", "ödeme adımı, boş sepetle sepete yönlenir", Actor.Visitor, "/odeme"),
            new("E-14", "sipariş teyit, havale bekleyen", Actor.Visitor, $"/siparis/erisim/{awaiting.AccessKey}/teyit"),
            new("E-15", "sipariş takibi girişi", Actor.Visitor, "/siparis-takibi"),
            new("E-16", "sipariş sayfası, misafir, havale bekleyen", Actor.Visitor, $"/siparis/erisim/{awaiting.AccessKey}"),
            new("E-16", "sipariş sayfası, misafir, hazırlanıyor", Actor.Visitor, $"/siparis/erisim/{paid.AccessKey}"),
            new("E-16", "sipariş sayfası, misafir, teslim edilmiş", Actor.Visitor, $"/siparis/erisim/{delivered.AccessKey}"),
            new("E-16", "sipariş sayfası, misafir, kapanmış", Actor.Visitor, $"/siparis/erisim/{closed.AccessKey}"),
            new("E-16", "sipariş sayfası, misafir, kargoda", Actor.Visitor, $"/siparis/erisim/{shipped.AccessKey}"),
            new("E-16", "sipariş sayfası, misafir, teslim edilemedi", Actor.Visitor, $"/siparis/erisim/{failed.AccessKey}"),
            new("E-16", "sipariş sayfası, misafir, kısmen iade edilmiş ödeme", Actor.Visitor, $"/siparis/erisim/{partial.AccessKey}"),
            new("E-16", "sipariş sayfası, misafir, iade edilmiş ödeme", Actor.Visitor, $"/siparis/erisim/{refunded.AccessKey}"),
            new("E-16", "sipariş sayfası, üye", Actor.Customer, $"/siparis/{member.Number}"),
            new("E-17", "iptal ekranı, misafir", Actor.Visitor, $"/siparis/erisim/{awaiting.AccessKey}/iptal"),
            new("E-17", "iptal ekranı, üye", Actor.Customer, $"/siparis/{member.Number}/iptal"),
            new("E-17", "gecikme feshi ekranı, üye", Actor.Customer, $"/siparis/{member.Number}/gecikme-feshi"),
            new("E-18", "cayma beyanı ekranı, teslim edilmiş", Actor.Visitor, $"/siparis/erisim/{delivered.AccessKey}/cayma"),
            new("E-19", "ayıp talebi ekranı, teslim edilmiş", Actor.Visitor, $"/siparis/erisim/{delivered.AccessKey}/ayip-talebi"),
            new("E-20", "kayıt ekranı", Actor.Visitor, "/kayit"),
            new("E-21", "doğrulama bağlantısı, geçersiz bağlantı", Actor.Visitor, $"/dogrulama/{unknownKey}"),
            new("E-22", "müşteri girişi", Actor.Visitor, "/giris"),
            new("E-23", "şifre sıfırlama, bağlantı isteme", Actor.Visitor, "/sifre-sifirlama"),
            new("E-23", "şifre sıfırlama, geçersiz bağlantı", Actor.Visitor, $"/sifre-sifirlama/{unknownKey}"),
            new("E-24", "yeniden doğrulama", Actor.Customer, "/hesap/yeniden-dogrulama"),
            new("E-25", "hesap, profil ve güvenlik", Actor.Customer, "/hesap"),
            new("E-26", "adres defteri", Actor.Customer, "/hesap/adresler"),
            new("E-27", "sipariş geçmişi", Actor.Customer, "/hesap/siparisler"),
            new("E-28", "e-posta değişikliğini geri alma, geçersiz bağlantı", Actor.Visitor, $"/geri-al/{unknownKey}"),

            // Panel (04 4.2).
            new("E-29", "panel girişi", Actor.Visitor, "/panel/giris"),
            new("E-29", "panel şifre sıfırlama, bağlantı isteme", Actor.Visitor, "/panel/sifre-sifirlama"),
            new("E-30", "yönetici daveti, geçersiz bağlantı", Actor.Visitor, $"/panel/davet/{unknownKey}"),
            new("E-31", "panel ana sayfası", Actor.Admin, "/panel"),
            new("E-32", "ürün listesi", Actor.Admin, "/panel/urunler"),
            new("E-33", "ürün formu, yeni ürün", Actor.Admin, "/panel/urunler/yeni"),
            new("E-33", "ürün formu, kayıtlı ürün", Actor.Admin, $"/panel/urunler/{seed.Product("variants").ProductId}"),
            new("E-34", "kategori ağacı", Actor.Admin, "/panel/kategoriler"),
            new("E-35", "kuponlar", Actor.Admin, "/panel/kuponlar"),
            new("E-36", "sipariş listesi", Actor.Admin, "/panel/siparisler"),
            new("E-37", "sipariş ayrıntısı, havale bekleyen", Actor.Admin, $"/panel/siparisler/{awaiting.Id}"),
            new("E-37", "sipariş ayrıntısı, teslim edilmiş", Actor.Admin, $"/panel/siparisler/{delivered.Id}"),
            new("E-37", "sipariş ayrıntısı, kapanmış", Actor.Admin, $"/panel/siparisler/{closed.Id}"),
            new("E-37", "sipariş ayrıntısı, kargoda", Actor.Admin, $"/panel/siparisler/{shipped.Id}"),
            new("E-37", "sipariş ayrıntısı, teslim edilemedi", Actor.Admin, $"/panel/siparisler/{failed.Id}"),
            new("E-37", "sipariş ayrıntısı, kısmen iade edilmiş ödeme", Actor.Admin, $"/panel/siparisler/{partial.Id}"),
            new("E-37", "sipariş ayrıntısı, iade edilmiş ödeme", Actor.Admin, $"/panel/siparisler/{refunded.Id}"),
            new("E-38", "talepler", Actor.Admin, "/panel/talepler"),
            new("E-39", "iletişim talebi ayrıntısı", Actor.Admin, $"/panel/talepler/{seed.ContactRequestId}"),
            new("E-40", "üye kaydı görünümü", Actor.Admin, "/panel/uyeler"),
            new("E-41", "kurumsal içerik", Actor.Admin, "/panel/kurumsal-icerik"),
            new("E-41", "kurumsal içerik, yeni hizmet", Actor.Admin, "/panel/kurumsal-icerik/hizmet/yeni"),
            new("E-41", "kurumsal içerik, kayıtlı hizmet", Actor.Admin, $"/panel/kurumsal-icerik/hizmet/{seed.OfferingId}"),
            new("E-42", "ana sayfa ve menü", Actor.Admin, "/panel/ana-sayfa-ve-menu"),
            new("E-43", "marka", Actor.Admin, "/panel/marka"),
            new("E-44", "firma kimliği ve satış", Actor.Admin, "/panel/firma-kimligi-ve-satis"),
            new("E-45", "ödeme yöntemleri", Actor.Admin, "/panel/odeme-yontemleri"),
            new("E-46", "kargo, süreler ve varsayılanlar", Actor.Admin, "/panel/kargo-sureler-ve-varsayilanlar"),
            new("E-47", "yasal metinler", Actor.Admin, "/panel/yasal-metinler"),
            new("E-49", "yönetici hesapları", Actor.Admin, "/panel/yonetici-hesaplari"),
            new("E-50", "yöneticinin kendi hesabı", Actor.Admin, "/panel/hesabim"),
            new("E-51", "satış özeti", Actor.Admin, "/panel/satis-ozeti"),
            new("E-52", "işlem izi", Actor.Admin, "/panel/islem-izi"),
            new("E-53", "dışa aktarma", Actor.Admin, "/panel/disa-aktarma"),
            new("E-54", "yeniden doğrulama", Actor.Admin, "/panel/yeniden-dogrulama"),
            new("E-54", "doğrulama bağlantısı, geçersiz bağlantı", Actor.Visitor, $"/panel/dogrulama/{unknownKey}"),
            new("E-54", "geri alma bağlantısı, geçersiz bağlantı", Actor.Visitor, $"/panel/geri-al/{unknownKey}"),
        ];
        return stops;
    }

    /// <summary>The screen ids of 04 4.1 and 4.2 (the inventory's own tables): the list is read from the document, so a screen the document adds and the walk lacks breaks this test.</summary>
    private static List<string> InventoryIds()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(System.IO.Path.Combine(directory.FullName, "Docs", "04_UI_SPECS.md")))
        {
            directory = directory.Parent;
        }

        string document = File.ReadAllText(System.IO.Path.Combine(directory?.FullName ?? throw new InvalidOperationException("Docs/04_UI_SPECS.md was not found above the test's folder."), "Docs", "04_UI_SPECS.md"));
        int start = document.IndexOf("### 4.1 ", StringComparison.Ordinal);
        int end = document.IndexOf("### 4.3 ", StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start, "04 4.1 and 4.2 were not found.");
        return [.. System.Text.RegularExpressions.Regex.Matches(document[start..end], @"^\| (E-\d{2}) \|", System.Text.RegularExpressions.RegexOptions.Multiline, TimeSpan.FromSeconds(2)).Select(m => m.Groups[1].Value).Distinct()];
    }

    [Fact]
    public void EveryScreenOfTheInventoryDocument_HasAStopInTheWalk_KK2()
    {
        List<string> ids = InventoryIds();
        HashSet<string> walked = [.. Stops().Select(s => s.Screen)];
        Assert.True(ids.Count >= 50, $"The inventory read from 04 has {ids.Count} screens; at least 50 are expected.");
        Assert.All(ids, id => Assert.Contains(id, walked));
        Assert.All(walked, id => Assert.Contains(id, ids));
    }

    [Theory]
    [MemberData(nameof(Ways))]
    public async Task EveryScreenOfTheInventory_OpensWithoutBrowserErrors_InTheWay_KK2(string engine, ScriptPath path)
    {
        foreach (IGrouping<Actor, Stop> group in Stops().GroupBy(s => s.Actor))
        {
            await using IBrowserContext context = await Host.ContextAsync(engine, path, group.Key);
            foreach (Stop stop in group)
            {
                WatchedPage page = await NewPageAsync(context);
                // The image address (/gorsel/{id}) has no serving endpoint yet (D-1711): the browser's own 404 line of such a request is the known state, counted and taken out; any other 404 stays a message.
                int missingImages = 0;
                page.Page.Response += (_, r) => missingImages += r.Status == 404 && new Uri(r.Url).AbsolutePath.StartsWith("/gorsel/", StringComparison.Ordinal) ? 1 : 0;
                IResponse? response = await GoAsync(page, stop.Path);
                int status = response?.Status ?? 0;
                string title = await page.Page.TitleAsync();
                string final = new Uri(page.Page.Url).PathAndQuery;
                // A redirect (the checkout of an empty cart to the cart, a sign-out to the sign-in) is a state of the screen; the page it lands on answers 200.
                bool statusOk = status == stop.Status;
                string expectedMessage = $"status of {stop.Status}";
                List<string> messages = [.. page.Errors.Where(e => stop.Status == 200 || !e.Contains(expectedMessage, StringComparison.Ordinal))];
                int known = 0;
                messages = [.. messages.Where(e => !(e.Contains("status of 404", StringComparison.Ordinal) && known++ < missingImages))];
                bool ok = statusOk && messages.Count == 0 && title.Length > 0;
                string observed = $"status {status}, address {final}, title \"{title}\", browser messages {messages.Count}, known image 404 {missingImages}" + (messages.Count > 0 ? ": " + string.Join(" / ", messages.Take(3)) : string.Empty);
                await ObserveAsync($"{stop.Screen}", path, engine, page, stop.State, $"status {stop.Status}, a title, no browser message", observed, ok);
                await page.Page.CloseAsync();
            }
        }

        Assert.Empty(Failures);
    }
}
