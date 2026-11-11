using Microsoft.AspNetCore.Http;

using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Catalog.Contracts.Categories;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Pages.Frame;

namespace Shopfolio.UnitTests.Pages;

/// <summary>The small rules of the frame's model and helpers (T540 KK3, KK5, KK8, KK9; 04 2.2.3, 2.2.8, 2.2.9, 2.9.2): the panel menu, the screens without the frame, the brand variables, the category tree, the draft band's link.</summary>
public sealed class PanelFrameModelTests
{
    private static readonly string[] EntryTitles =
        ["Ana sayfa", "Siparişler", "Talepler", "Katalog", "İçerik ve site", "Üyeler", "Raporlar", "Ayarlar"];

    [Fact]
    public void TheMenu_HasTheEightEntriesOfTheMenuMap_InOrder_KK8()
    {
        IReadOnlyList<PanelMenuEntry> menu = PanelFrameBuilder.MenuOf("/panel", false, false);

        Assert.Equal(EntryTitles, menu.Select(e => e.Title).ToArray());
        Assert.Equal(["Ürünler", "Kategoriler", "Kuponlar"], menu[3].Items.Select(i => i.Title).ToArray());
        Assert.Equal(["Kurumsal içerik", "Ana sayfa ve menü", "Marka"], menu[4].Items.Select(i => i.Title).ToArray());
        Assert.Equal(["Satış özeti", "İşlem izi", "Dışa aktarma"], menu[6].Items.Select(i => i.Title).ToArray());
        Assert.Equal(
            ["Firma kimliği ve satış", "Ödeme yöntemleri", "Kargo, süreler ve varsayılanlar", "Yasal metinler", "Yönetici hesapları"],
            menu[7].Items.Select(i => i.Title).ToArray());
        Assert.All(menu.Where(e => e.Items.Count == 0), e => Assert.NotNull(e.Path));
        Assert.All(menu.Where(e => e.Items.Count > 0), e => Assert.Null(e.Path));
    }

    [Theory]
    [InlineData("/panel", "Ana sayfa")]
    [InlineData("/panel/", "Ana sayfa")]
    [InlineData("/panel/siparisler", "Siparişler")]
    [InlineData("/panel/siparisler/abc-123", "Siparişler")]
    [InlineData("/panel/talepler/9", "Talepler")]
    [InlineData("/panel/uyeler", "Üyeler")]
    [InlineData("/panel/urunler/yeni", "Katalog")]
    [InlineData("/panel/kuponlar", "Katalog")]
    [InlineData("/panel/marka", "İçerik ve site")]
    [InlineData("/panel/islem-izi", "Raporlar")]
    [InlineData("/panel/yasal-metinler/gizlilik", "Ayarlar")]
    [InlineData("/PANEL/SIPARISLER", "Siparişler")]
    public void TheScreen_MarksItsOwnEntryActive_AndNoOtherEntry_KK8(string path, string active)
    {
        IReadOnlyList<PanelMenuEntry> menu = PanelFrameBuilder.MenuOf(path, false, false);

        Assert.Equal([active], menu.Where(e => e.IsActive).Select(e => e.Title).ToArray());
    }

    [Theory]
    [InlineData("/panel/ayar-olmayan")]
    [InlineData("/panel/siparisler-x")]
    [InlineData("/panel/urun")]
    [InlineData("/")]
    public void AScreenThatIsNoEntry_MarksNothingActive_KK8(string path) =>
        Assert.DoesNotContain(PanelFrameBuilder.MenuOf(path, false, false), e => e.IsActive);

    [Fact]
    public void TheActiveItemOfAGroup_IsMarkedAndItsSiblingsAreNot_KK8()
    {
        PanelMenuEntry group = PanelFrameBuilder.MenuOf("/panel/kategoriler/3", false, false)[3];

        Assert.True(group.IsActive);
        Assert.Equal(["Kategoriler"], group.Items.Where(i => i.IsActive).Select(i => i.Title).ToArray());
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void OnlyOrdersAndRequests_CarryThePendingMark_WhileWorkIsPending_KK8(bool orders, bool requests)
    {
        IReadOnlyList<PanelMenuEntry> menu = PanelFrameBuilder.MenuOf("/panel", orders, requests);

        Assert.Equal(orders, menu[1].HasPendingMark);
        Assert.Equal(requests, menu[2].HasPendingMark);
        Assert.DoesNotContain(menu.Where((_, index) => index is not (1 or 2)), e => e.HasPendingMark);
    }

    [Theory]
    [InlineData("/panel/giris", true)]
    [InlineData("/panel/sifre-sifirlama", true)]
    [InlineData("/panel/sifre-sifirlama/abc", true)]
    [InlineData("/panel/davet/abc", true)]
    [InlineData("/panel/dogrulama/abc", true)]
    [InlineData("/panel/geri-al/abc", true)]
    [InlineData("/PANEL/GIRIS", true)]
    [InlineData("/panel/yeniden-dogrulama", false)]
    [InlineData("/panel", false)]
    [InlineData("/panel/siparisler", false)]
    [InlineData("/panel/giris-x", false)]
    [InlineData("/giris", false)]
    [InlineData("/geri-al/abc", false)]
    public void TheScreensThatOpenBeforeASession_AreTheFramelessOnes_KK8(string path, bool frameless)
    {
        DefaultHttpContext context = new();
        context.Request.Path = path;

        Assert.Equal(frameless, PanelFrameBuilder.IsFramelessScreen(context.Request));
    }

    [Fact]
    public void TheFramelessSegments_AreExactlyTheOpenPanelPages_KK8()
    {
        string[] fromList = [.. SessionSchemes.OpenPanelAddresses.Select(a => a.Split('/')[2]).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];

        Assert.Equal(fromList, PanelFramePaths.FramelessSegments.Order(StringComparer.Ordinal).ToArray());
        Assert.DoesNotContain("yeniden-dogrulama", PanelFramePaths.FramelessSegments);
    }

    [Fact]
    public void TheBrandVariables_AreTwoColours_AndAValueThatIsNoColourFallsBackToTheDefault_KK3()
    {
        Assert.Equal(":root{--brand-color:#112233;--brand-text-color:#FFFFFF;}", FrameStyle.BrandVariables(Brand("#112233", "#FFFFFF")));
        Assert.Equal(":root{--brand-color:#1A56DB;--brand-text-color:#FFFFFF;}", FrameStyle.BrandVariables(Brand("red", "#FFFFFF")));
        Assert.Equal(":root{--brand-color:#1A56DB;--brand-text-color:#FFFFFF;}", FrameStyle.BrandVariables(Brand("#112233", "white")));
        Assert.Equal(":root{--brand-color:#1A56DB;--brand-text-color:#FFFFFF;}", FrameStyle.BrandVariables(Brand("#112233;} body{display:none", "#FFFFFF")));
        Assert.Equal(":root{--brand-color:#1A56DB;--brand-text-color:#FFFFFF;}", FrameStyle.BrandVariables(Brand("#12345", "#FFFFFF")));
        Assert.Equal(":root{--brand-color:#1A56DB;--brand-text-color:#FFFFFF;}", FrameStyle.BrandVariables(Brand("#1234567", "#FFFFFF")));
    }

    private static FrameBrand Brand(string color, string textColor) => new("Marka", null, null, "M", color, textColor);

    [Fact]
    public void TheImageAddressAndTheWhatsAppLink_AreBuiltFromSafeValuesOnly_KK5()
    {
        Guid id = Guid.Parse("11111111-2222-3333-4444-555555555555");

        Assert.Equal("/gorsel/11111111-2222-3333-4444-555555555555", FrameStyle.ImagePath(id));
        Assert.Equal("https://wa.me/905321234567", FrameStyle.WhatsAppLink("5321234567"));
        Assert.Null(FrameStyle.WhatsAppLink(null));
        Assert.Null(FrameStyle.WhatsAppLink("532123456"));
        Assert.Null(FrameStyle.WhatsAppLink("5321234567?text=x"));
        Assert.Null(FrameStyle.WhatsAppLink("+905321234567"));
    }

    [Fact]
    public void TheMenuTree_FollowsTheListsOrder_AndAnOrphanStandsAtTheRoot_KK5()
    {
        Guid a = Guid.NewGuid();
        Guid a1 = Guid.NewGuid();
        Guid a1x = Guid.NewGuid();
        Guid b = Guid.NewGuid();
        Guid orphan = Guid.NewGuid();
        MenuCategory[] list =
        [
            new(a, null, "Ayakkabi", "ayakkabi", 1),
            new(a1, a, "Bot", "bot", 2),
            new(a1x, a1, "Kar botu", "kar-botu", 3),
            new(b, null, "Canta", "canta", 1),
            new(orphan, Guid.NewGuid(), "Yetim", "yetim", 2),
        ];

        IReadOnlyList<MenuCategoryNode> tree = FrameMenuTree.Build(list);

        Assert.Equal(["Ayakkabi", "Canta", "Yetim"], tree.Select(n => n.Name).ToArray());
        Assert.Equal("/kategori/ayakkabi", tree[0].Path);
        Assert.Equal(["Bot"], tree[0].Children.Select(n => n.Name).ToArray());
        Assert.Equal(["Kar botu"], tree[0].Children[0].Children.Select(n => n.Name).ToArray());
        Assert.Equal("/kategori/kar-botu", tree[0].Children[0].Children[0].Path);
        Assert.Empty(tree[1].Children);
        Assert.Empty(FrameMenuTree.Build([]));
    }

    [Theory]
    [InlineData("/panel/urunler/abc")]
    [InlineData("/panel/icerik/sss/3?x=1")]
    public void TheDraftBand_TakesALocalPanelAddress_KK9(string path)
    {
        DefaultHttpContext context = new();

        context.SetDraftPreview(path);

        Assert.Equal(new DraftBand(path), context.Items[DraftBandAccess.ItemKey]);
    }

    [Theory]
    [InlineData("https://evil.example/panel/x")]
    [InlineData("//evil.example/panel/x")]
    [InlineData("/urunler/abc")]
    [InlineData("/panel")]
    [InlineData("panel/urunler/abc")]
    [InlineData("/panelx/abc")]
    [InlineData("/\\evil.example/panel/x")]
    public void TheDraftBand_RefusesAnAddressThatIsNotALocalPanelAddress_KK9(string path)
    {
        DefaultHttpContext context = new();

        Assert.Throws<ArgumentException>(() => context.SetDraftPreview(path));
        Assert.False(context.Items.ContainsKey(DraftBandAccess.ItemKey));
    }

    [Fact]
    public void TheDraftBand_IsReadOnlyWhileTheFrameCarriesTheAdministratorStrip_KK9()
    {
        DefaultHttpContext context = new();
        context.SetDraftPreview("/panel/urunler/abc");

        // No frame: nothing to take the place of.
        Assert.Null(context.GetDraftBand());
    }
}
