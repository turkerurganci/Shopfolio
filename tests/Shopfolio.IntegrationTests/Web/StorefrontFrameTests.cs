using System.Net;
using System.Text.Json;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the storefront frame's tests (T537): one database copied from the template.</summary>
[CollectionDefinition(StorefrontFrameTestsDefinition.Name)]
public sealed class StorefrontFrameTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "StorefrontFrameTests";
}

/// <summary>
/// The storefront frame on the opened application with two test-only pages (T537 KK1-KK9; 07 5.5.1, 4.34.3; 04 2.2, 2.9): the frame enters the response of a page that does not add it, comes
/// from the request's context, and says who the browser is (visitor, member, administrator), what the cart holds, which strip and band apply, what the footer carries and what the gates say
/// now. Sessions, carts, texts and the firm's settings are real rows of the collection database; the frame is printed as JSON by the sample page and read back here.
/// </summary>
[Collection(StorefrontFrameTestsDefinition.Name)]
public sealed partial class StorefrontFrameTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 10, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _database;

    private int _counter;

    public StorefrontFrameTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await using AppDbContext db = NewContext();
        db.CartItems.RemoveRange(await db.CartItems.ToListAsync(Ct));
        db.Carts.RemoveRange(await db.Carts.ToListAsync(Ct));
        db.Announcements.RemoveRange(await db.Announcements.ToListAsync(Ct));
        db.SocialLinks.RemoveRange(await db.SocialLinks.ToListAsync(Ct));
        await db.SaveChangesAsync(Ct);
        await db.Database.ExecuteSqlRawAsync("TRUNCATE legal_text_versions CASCADE", Ct);
        await db.CompanySettings.ExecuteUpdateAsync(s => s.SetProperty(c => c.WhatsAppNumber, (string?)null), Ct);
        await db.Companies.ExecuteUpdateAsync(c => c.SetProperty(x => x.EtbisVerificationUrl, (string?)null), Ct);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(FrameSampleModel).Assembly)));

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
    }

    private static async Task<JsonElement> FrameOfAsync(HttpClient client, string path, params string[] cookies)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, path);
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        if (cookies.Length > 0)
        {
            request.Headers.Add("Cookie", string.Join("; ", cookies));
        }

        using HttpResponseMessage response = await client.SendAsync(request, Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync(Ct);
        return JsonDocument.Parse(body.Trim()).RootElement.Clone();
    }

    private static string Customer(string key) => $"{CookieNames.Session}={key}";

    private static string Admin(string key) => $"{CookieNames.AdminSession}={key}";

    private static string GuestCart(string key) => $"{CookieNames.Cart}={key}";

    // ---- KK1, KK2, KK6: the frame enters the page that does not add it; the fallback chain; the footer ----

    [Fact]
    public async Task APageThatDrawsItself_GetsTheFrame_WithTheBrandFallbackAndTheWholeFooter_T537_KK1_KK2_KK6()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        JsonElement frame = await FrameOfAsync(client, "/hakkimizda");

        // The model of the page holds no frame (FrameSampleModel has none); the response still carries it. No logo and no name: the domain, as text.
        Assert.Equal(JsonValueKind.Null, frame.GetProperty("Brand").GetProperty("LogoId").ValueKind);
        Assert.False(string.IsNullOrWhiteSpace(frame.GetProperty("Brand").GetProperty("BrandName").GetString()));
        Assert.NotEqual(JsonValueKind.Null, frame.GetProperty("Menu").ValueKind);
        Assert.Equal("/arama", frame.GetProperty("Search").GetProperty("Path").GetString());
        Assert.False(frame.GetProperty("IsCheckoutStep").GetBoolean());

        // The footer: the contact heading's link, the identity set, no ETBIS band while the field is empty, and the process guide that waits for nothing.
        JsonElement footer = frame.GetProperty("Footer");
        Assert.Equal("/iletisim", footer.GetProperty("ContactPath").GetString());
        Assert.Equal(JsonValueKind.Null, footer.GetProperty("EtbisVerificationUrl").ValueKind);
        Assert.Equal(JsonValueKind.Object, footer.GetProperty("Identity").ValueKind);
        Assert.Equal(["/islem-rehberi"], footer.GetProperty("LegalLinks").EnumerateArray().Select(l => l.GetProperty("Path").GetString()!).ToArray());
    }

    [Fact]
    public async Task TheFooter_CarriesTheEtbisBandWhenTheFieldIsFilled_AndThePublishedLegalLinksOnly_T537_KK6()
    {
        await using (AppDbContext db = NewContext())
        {
            await db.Companies.ExecuteUpdateAsync(c => c.SetProperty(x => x.EtbisVerificationUrl, "https://etbis.example/dogrula"), Ct);
            db.LegalTextVersions.Add(LegalTextVersion.CreatePublished(LegalTextType.CookiePolicy, 1, Now, "metin", Now));
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        JsonElement footer = (await FrameOfAsync(client, "/hakkimizda")).GetProperty("Footer");

        Assert.Equal("https://etbis.example/dogrula", footer.GetProperty("EtbisVerificationUrl").GetString());
        // The cookie policy has a published version, the privacy notice has none: only the first and the guide are linked.
        Assert.Equal(["/cerez-politikasi", "/islem-rehberi"], footer.GetProperty("LegalLinks").EnumerateArray().Select(l => l.GetProperty("Path").GetString()!).ToArray());
    }

    [Fact]
    public async Task ThePlatformSignature_FollowsTheFirmsSwitch_T537_KK6()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();
        bool shown;
        await using (AsyncServiceScope scope = factory.Services.CreateAsyncScope())
        {
            shown = (await scope.ServiceProvider.GetRequiredService<ICorporateContentModule>().GetBrandIdentityAsync(Ct)).ShowsPlatformSignature;
        }

        JsonElement frame = await FrameOfAsync(client, "/hakkimizda");

        Assert.Equal(shown, frame.GetProperty("Footer").GetProperty("ShowsPlatformSignature").GetBoolean());
    }

    // ---- KK4: the three account states; the administrator's session is no session ----

    [Fact]
    public async Task TheAccountArea_HasThreeStates_AndAnAdministratorIsNoMember_T537_KK4()
    {
        (string customerKey, _) = await TestSessions.CustomerAsync(_database.Dsn);
        (string adminKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        JsonElement visitor = (await FrameOfAsync(client, "/hakkimizda")).GetProperty("Account");
        Assert.Equal(JsonValueKind.Null, visitor.GetProperty("MemberName").ValueKind);
        Assert.Equal("/giris?returnUrl=%2Fhakkimizda", visitor.GetProperty("SignInPath").GetString());
        Assert.Equal("/siparis-takibi", visitor.GetProperty("OrderTrackingPath").GetString());
        Assert.Equal(JsonValueKind.Null, visitor.GetProperty("SignOut").ValueKind);

        JsonElement member = (await FrameOfAsync(client, "/hakkimizda?a=1", Customer(customerKey))).GetProperty("Account");
        Assert.Equal("Test musteri", member.GetProperty("MemberName").GetString());
        Assert.Equal("/cikis", member.GetProperty("SignOut").GetProperty("Path").GetString());
        Assert.Equal("/hakkimizda?a=1", member.GetProperty("SignOut").GetProperty("ReturnUrl").GetString());
        Assert.Equal("/hesap", member.GetProperty("AccountPath").GetString());
        Assert.Equal("/hesap/siparisler", member.GetProperty("OrdersPath").GetString());

        // The administrator's cookie alone: the account area is the visitor's, and the strip is there.
        JsonElement admin = await FrameOfAsync(client, "/hakkimizda", Admin(adminKey));
        Assert.Equal(JsonValueKind.Null, admin.GetProperty("Account").GetProperty("MemberName").ValueKind);
        Assert.Equal("/panel", admin.GetProperty("AdminStrip").GetProperty("PanelPath").GetString());

        // Both cookies: the member is shown and the strip as well; the customer never gets the strip.
        JsonElement both = await FrameOfAsync(client, "/hakkimizda", Customer(customerKey), Admin(adminKey));
        Assert.Equal("Test musteri", both.GetProperty("Account").GetProperty("MemberName").GetString());
        Assert.NotEqual(JsonValueKind.Null, both.GetProperty("AdminStrip").ValueKind);
        JsonElement customerOnly = await FrameOfAsync(client, "/hakkimizda", Customer(customerKey));
        Assert.Equal(JsonValueKind.Null, customerOnly.GetProperty("AdminStrip").ValueKind);
    }

    [Fact]
    public async Task AnExpiredSession_IsAVisitor_T537_KK4()
    {
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        await using (AppDbContext db = NewContext())
        {
            await db.Sessions.Where(s => s.CustomerId == customerId).ExecuteUpdateAsync(s => s.SetProperty(x => x.LastActivityAt, Now.AddYears(-5)), Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        JsonElement account = (await FrameOfAsync(client, "/hakkimizda", Customer(customerKey))).GetProperty("Account");

        Assert.Equal(JsonValueKind.Null, account.GetProperty("MemberName").ValueKind);
    }

    // ---- KK3: the draft announcement is the administrator's only ----

    [Fact]
    public async Task ADraftAnnouncement_ComesWithItsMarkToTheAdministratorOnly_AndAPublishedOneToEveryone_T537_KK3()
    {
        (string adminKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        Announcement draft = Announcement.Create(Now);
        draft.Text = "Taslak duyuru";
        await using (AppDbContext db = NewContext())
        {
            db.Announcements.Add(draft);
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        Assert.Equal(JsonValueKind.Null, (await FrameOfAsync(client, "/hakkimizda")).GetProperty("Announcement").ValueKind);
        JsonElement seen = (await FrameOfAsync(client, "/hakkimizda", Admin(adminKey))).GetProperty("Announcement");
        Assert.Equal("Taslak duyuru", seen.GetProperty("Text").GetString());
        Assert.True(seen.GetProperty("IsDraft").GetBoolean());

        await using (AppDbContext db = NewContext())
        {
            await db.Announcements.ExecuteUpdateAsync(a => a.SetProperty(x => x.PublishStatus, Shopfolio.Modules.Catalog.Contracts.PublishStatus.Published), Ct);
        }

        JsonElement published = (await FrameOfAsync(client, "/hakkimizda")).GetProperty("Announcement");
        Assert.Equal("Taslak duyuru", published.GetProperty("Text").GetString());
        Assert.False(published.GetProperty("IsDraft").GetBoolean());
    }

    // ---- KK8: the reduced frame of the payment step ----

    [Fact]
    public async Task ThePaymentStep_HasNoMenuNoSearchAndNoAnnouncement_AndTheFooterStaysWhole_T537_KK8()
    {
        await using (AppDbContext db = NewContext())
        {
            Announcement announcement = Announcement.Create(Now);
            announcement.Text = "Duyuru";
            announcement.PublishStatus = Shopfolio.Modules.Catalog.Contracts.PublishStatus.Published;
            db.Announcements.Add(announcement);
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        JsonElement checkout = await FrameOfAsync(client, "/odeme");
        JsonElement page = await FrameOfAsync(client, "/hakkimizda");

        Assert.True(checkout.GetProperty("IsCheckoutStep").GetBoolean());
        Assert.Equal(JsonValueKind.Null, checkout.GetProperty("Menu").ValueKind);
        Assert.Equal(JsonValueKind.Null, checkout.GetProperty("Search").ValueKind);
        Assert.Equal(JsonValueKind.Null, checkout.GetProperty("Announcement").ValueKind);
        Assert.Equal("/sepet", checkout.GetProperty("Cart").GetProperty("Path").GetString());
        Assert.Equal(page.GetProperty("Footer").ToString(), checkout.GetProperty("Footer").ToString());
        Assert.NotEqual(JsonValueKind.Null, page.GetProperty("Announcement").ValueKind);
    }

    // ---- KK7: the gates are computed in every request, never stored ----

    [Fact]
    public async Task TheGates_AreComputedInEveryRequest_T537_KK7()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        JsonElement closed = (await FrameOfAsync(client, "/hakkimizda")).GetProperty("Sales");
        Assert.True(closed.GetProperty("IsSalesClosed").GetBoolean());
        Assert.False(closed.GetProperty("IsDataCollectionOpen").GetBoolean());

        Assert.False(closed.GetProperty("IsTemporarilyClosed").GetBoolean());

        // The same application, the next request: the firm's own switch is read afresh, then read again after it is turned off.
        await using (AppDbContext db = NewContext())
        {
            await db.CompanySettings.ExecuteUpdateAsync(s => s.SetProperty(c => c.IsSalesTemporarilyClosed, true), Ct);
        }

        Assert.True((await FrameOfAsync(client, "/hakkimizda")).GetProperty("Sales").GetProperty("IsTemporarilyClosed").GetBoolean());

        await using (AppDbContext db = NewContext())
        {
            await db.CompanySettings.ExecuteUpdateAsync(s => s.SetProperty(c => c.IsSalesTemporarilyClosed, false), Ct);
        }

        Assert.False((await FrameOfAsync(client, "/hakkimizda")).GetProperty("Sales").GetProperty("IsTemporarilyClosed").GetBoolean());
    }

    // ---- KK5: the cart count of this browser; the links ----

    [Fact]
    public async Task TheCartCount_IsThisBrowsersCart_AndTheSocialLinksAndTheWhatsAppNumberAreInTheModel_T537_KK5()
    {
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        (string adminKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        string guestKey = $"guest-{Guid.NewGuid():N}";
        List<Variant> variants = await SeedVariantsAsync(3);
        await SeedCartAsync(customerId, null, variants[0].Id, variants[1].Id);
        await SeedCartAsync(null, guestKey, variants[2].Id);
        await using (AppDbContext db = NewContext())
        {
            db.SocialLinks.Add(SocialLink.Create(SocialPlatform.YouTube, "https://youtube.example/k", Now));
            db.SocialLinks.Add(SocialLink.Create(SocialPlatform.Instagram, "https://instagram.example/k", Now));
            await db.SaveChangesAsync(Ct);
            await db.CompanySettings.ExecuteUpdateAsync(s => s.SetProperty(c => c.WhatsAppNumber, "5321234567"), Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        Assert.Equal(0, (await FrameOfAsync(client, "/hakkimizda")).GetProperty("Cart").GetProperty("ItemCount").GetInt32());
        Assert.Equal(1, (await FrameOfAsync(client, "/hakkimizda", GuestCart(guestKey))).GetProperty("Cart").GetProperty("ItemCount").GetInt32());
        Assert.Equal(2, (await FrameOfAsync(client, "/hakkimizda", Customer(customerKey))).GetProperty("Cart").GetProperty("ItemCount").GetInt32());

        // The administrator's session changes neither the cart nor the account area: the guest cart of the same browser is still counted.
        JsonElement admin = await FrameOfAsync(client, "/hakkimizda", Admin(adminKey), GuestCart(guestKey));
        Assert.Equal(1, admin.GetProperty("Cart").GetProperty("ItemCount").GetInt32());

        JsonElement contact = admin.GetProperty("Contact");
        Assert.Equal("5321234567", contact.GetProperty("WhatsAppNumber").GetString());
        Assert.Equal(["Instagram", "YouTube"], contact.GetProperty("SocialLinks").EnumerateArray().Select(l => l.GetProperty("Platform").GetString()!).ToArray());
    }

    // ---- KK9: the merge message is told once ----

    [Fact]
    public async Task TheMergeMessage_IsToldOnceInTheRequestThatReadsIt_AndARedirectReadsNoMark_T537_KK9()
    {
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        List<Variant> variants = await SeedVariantsAsync(2);
        Guid cartId = await SeedCartAsync(customerId, null, variants[0].Id, variants[1].Id);
        await using (AppDbContext db = NewContext())
        {
            await db.CartItems.Where(i => i.CartId == cartId && i.VariantId == variants[0].Id).ExecuteUpdateAsync(i => i.SetProperty(x => x.IsMergeNoticePending, true), Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        // A redirect answer is no page: no frame, and the mark stays.
        using (HttpRequestMessage post = new(HttpMethod.Post, "/hakkimizda?handler=Leave"))
        {
            post.Headers.Add("Cookie", Customer(customerKey));
            post.Content = new FormUrlEncodedContent([]);
            using HttpResponseMessage redirected = await client.SendAsync(post, Ct);
            Assert.True(redirected.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.SeeOther or HttpStatusCode.Found);
        }

        await using (AppDbContext db = NewContext())
        {
            Assert.True(await db.CartItems.AnyAsync(i => i.CartId == cartId && i.IsMergeNoticePending, Ct));
        }

        JsonElement first = await FrameOfAsync(client, "/hakkimizda", Customer(customerKey));
        JsonElement second = await FrameOfAsync(client, "/hakkimizda", Customer(customerKey));

        Assert.Single(first.GetProperty("MergeAddedProductNames").EnumerateArray());
        Assert.Equal(2, first.GetProperty("Cart").GetProperty("ItemCount").GetInt32());
        Assert.Empty(second.GetProperty("MergeAddedProductNames").EnumerateArray());
        Assert.Equal(2, second.GetProperty("Cart").GetProperty("ItemCount").GetInt32());
    }

    // ---- The read ends of the modules the frame uses (T537) ----

    [Fact]
    public async Task TheCartBadge_CountsLiveLinesOnly_AndReadsZeroWithoutACart_AndTheMemberNameIsRead_T537_KK5()
    {
        (string _, Guid customerId) = await TestSessions.CustomerAsync(_database.Dsn);
        List<Variant> variants = await SeedVariantsAsync(3);
        Guid cartId = await SeedCartAsync(customerId, null, variants[0].Id, variants[1].Id, variants[2].Id);
        await using (AppDbContext db = NewContext())
        {
            // A line whose product left the showcase: the link is empty and the name is kept; it is not live.
            await db.CartItems.Where(i => i.CartId == cartId && i.VariantId == variants[2].Id).ExecuteUpdateAsync(i => i.SetProperty(x => x.VariantId, (Guid?)null).SetProperty(x => x.RemovedProductName, "Kalkan urun"), Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        IOrderingModule ordering = scope.ServiceProvider.GetRequiredService<IOrderingModule>();

        CartBadge member = await ordering.ReadCartBadgeAsync(CartOwner.ForMember(customerId), Ct);
        CartBadge noCart = await ordering.ReadCartBadgeAsync(CartOwner.ForMember(Guid.NewGuid()), Ct);
        CartBadge unknownGuest = await ordering.ReadCartBadgeAsync(CartOwner.ForGuest($"guest-{Guid.NewGuid():N}"), Ct);

        Assert.Equal(2, member.ItemCount);
        Assert.False(member.HasMergeNotice);
        Assert.Equal(CartBadge.Empty, noCart);
        Assert.Equal(CartBadge.Empty, unknownGuest);
        Assert.False(string.IsNullOrWhiteSpace(await scope.ServiceProvider.GetRequiredService<IMembershipModule>().GetMemberNameAsync(customerId, Ct)));
        Assert.Null(await scope.ServiceProvider.GetRequiredService<IMembershipModule>().GetMemberNameAsync(Guid.NewGuid(), Ct));
    }

    private async Task<List<Variant>> SeedVariantsAsync(int count)
    {
        List<Variant> variants = [];
        await using AppDbContext db = NewContext();
        for (int i = 0; i < count; i++)
        {
            int n = Interlocked.Increment(ref _counter);
            Product product = Product.Create($"Urun {n}", $"urun-{Guid.NewGuid():N}", ProductType.Physical, 20m, Now.AddMinutes(n)).WithSeededOrigin();
            product.PublishStatus = PublishStatus.Published;
            Variant variant = Variant.Create(product.Id, $"SKU-{Guid.NewGuid():N}"[..20], 100m, Now.AddMinutes(n));
            variant.PublishStatus = PublishStatus.Published;
            variant.StockQuantity = 10;
            db.Products.Add(product);
            db.Variants.Add(variant);
            variants.Add(variant);
        }

        await db.SaveChangesAsync(Ct);
        return variants;
    }

    private async Task<Guid> SeedCartAsync(Guid? customerId, string? guestKey, params Guid[] variantIds)
    {
        Cart cart = customerId is Guid id ? Cart.ForCustomer(id, Now) : Cart.ForGuest(SecretKey.Hash(guestKey!), Now);
        await using AppDbContext db = NewContext();
        db.Carts.Add(cart);
        foreach (Guid variantId in variantIds)
        {
            db.CartItems.Add(CartItem.Create(cart.Id, variantId, 1, 100m, Now));
        }

        await db.SaveChangesAsync(Ct);
        return cart.Id;
    }
}
