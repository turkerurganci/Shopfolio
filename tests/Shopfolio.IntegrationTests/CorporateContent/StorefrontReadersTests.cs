using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Categories;
using Shopfolio.Modules.Catalog.Contracts.Lists;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Settings;

namespace Shopfolio.IntegrationTests.CorporateContent;

/// <summary>The collection of the home page and menu tests (T361): one database copied from the template.</summary>
[CollectionDefinition(StorefrontReadersDefinition.Name)]
public sealed class StorefrontReadersDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "StorefrontReaders";
}

/// <summary>
/// The home page and the top menu on a real PostgreSQL (T361 criteria 1-6; KK1-KK6; 02 3.28.1-3.28.6): the two layouts that differ only in the order and weight of the same blocks, the
/// showcase of the newest published products that nobody picks, the corporate block, the flagged blocks in the firm's order, the menu skeleton with the firm's names and hidden types,
/// the general pages in the footer and the menu, and the categories that show only when a published product is in them or below. The cache is the module's own: a change drops it at
/// once (05 7.10.1).
/// </summary>
[Collection(StorefrontReadersDefinition.Name)]
public sealed class StorefrontReadersTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 8, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    private ServiceProvider _provider = null!;

    private Admin _admin = null!;

    private int _counter;

    public StorefrontReadersTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE audit_logs, cart_items, carts, order_items, orders, variants, products, categories, admins, about_pages, offerings, portfolio_items, faq_items, branches, custom_pages, announcements, images CASCADE");
        await ExecuteAsync(
            "UPDATE company_settings SET homepage_layout = 'ContentFirst', menu_name_products = 'Ürünler', menu_name_offerings = 'Hizmetlerimiz', "
            + "menu_name_portfolio_items = 'Referanslarımız', menu_name_about_page = 'Hakkımızda', menu_name_faq_items = 'SSS', menu_name_contact = 'İletişim'");
        _admin = Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", Now);
        await SaveAsync(c => c.Admins.Add(_admin));

        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddAppCache();
        services.AddScoped<AdvisoryLocks>();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IJobQueue, JobQueue>();
        services.AddFileStore();
        services.AddOptions<Shopfolio.Infrastructure.InstallationSettings.StatusThresholdOptions>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.IDiskUsageReader, Shopfolio.Infrastructure.Status.DriveDiskUsageReader>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.DiskUsageGuard>();
        services.AddSingleton<ImageProcessor>();
        services.AddCatalogModule();
        services.AddAfterSalesModule();
        services.AddPaymentModule();
        services.AddOrderingModule();
        services.AddCorporateContentModule();
        services.AddSettingsModule();
        services.AddScoped<IAuditActorSource>(_ => new FixedActor(_admin));
        services.AddScoped<IAuditTrail, AuditTrail>();
        _provider = services.BuildServiceProvider();
    }

    public async ValueTask DisposeAsync() => await _provider.DisposeAsync();

    // ---- KK1: the layout, the showcase, the blocks that are not there ----

    [Fact]
    public async Task AFreshSite_HasTheContentFirstLayout_AndOnlyTheContactItemInTheMenu_KK1()
    {
        HomepageView home = await HomeAsync();
        StorefrontMenu menu = await MenuAsync();

        Assert.Equal(HomepageLayout.ContentFirst, home.Layout);
        Assert.Equal([HomepageBlockKind.CorporateIntro], home.Blocks);
        Assert.Null(home.Intro);
        Assert.Null(home.Offerings);
        Assert.Null(home.PortfolioItems);
        Assert.Null(home.Showcase);
        Assert.Equal([MenuItemKind.Contact], menu.VisibleItems.Select(i => i.Kind));
        Assert.Equal(["Hakkımızda", "Hizmetlerimiz", "Referanslarımız", "SSS", "Ürünler"], menu.Items.Where(i => !i.IsVisible).Select(i => i.Name).Order(StringComparer.Ordinal));
        Assert.Empty(menu.Categories);
    }

    [Fact]
    public async Task TheShowcase_IsTheNewestPublishedProducts_NoDraftNoArchived_FourInContentFirst_EightInStoreFirst_KK1()
    {
        Guid category = await SeedCategoryAsync("Mobilya", null);
        List<Guid> published = [];
        for (int i = 1; i <= 10; i++)
        {
            published.Add(await SeedProductAsync($"Ürün {i}", PublishStatus.Published, category));
        }

        await SeedProductAsync("Taslak ürün", PublishStatus.Draft, category);
        await SeedProductAsync("Arşiv ürün", PublishStatus.Archived, category);

        HomepageView contentFirst = await HomeAsync();
        await SetLayoutAsync("StoreFirst");
        HomepageView storeFirst = await HomeAsync();

        Assert.NotNull(contentFirst.Showcase);
        Assert.Equal(["Ürün 10", "Ürün 9", "Ürün 8", "Ürün 7"], contentFirst.Showcase.Products.Select(p => p.Name));
        Assert.NotNull(storeFirst.Showcase);
        Assert.Equal(8, storeFirst.Showcase.Products.Count);
        Assert.Equal("Ürün 10", storeFirst.Showcase.Products[0].Name);
        Assert.Equal("Ürün 3", storeFirst.Showcase.Products[7].Name);
        Assert.DoesNotContain(storeFirst.Showcase.Products, p => p.Name is "Taslak ürün" or "Arşiv ürün");
        Assert.Equal("Ürünler", storeFirst.Showcase.Title);
        Assert.Equal(["Mobilya"], storeFirst.Showcase.CategoryLinks.Select(c => c.Name));
    }

    [Fact]
    public async Task ASiteWithoutAPublishedProduct_HasNoShowcaseBlock_EvenWithADraftAndAnArchivedOne_KK1()
    {
        Guid category = await SeedCategoryAsync("Mobilya", null);
        await SeedProductAsync("Taslak ürün", PublishStatus.Draft, category);
        await SeedProductAsync("Arşiv ürün", PublishStatus.Archived, category);

        HomepageView home = await HomeAsync();

        Assert.Null(home.Showcase);
        Assert.DoesNotContain(HomepageBlockKind.ProductShowcase, home.Blocks);
    }

    [Fact]
    public async Task TheLayout_OrdersTheBlocks_OnlyThroughTheirOrder_AndTheOtherBlocksStayTheSame_KK1()
    {
        Guid category = await SeedCategoryAsync("Mobilya", null);
        await SeedProductAsync("Masa", PublishStatus.Published, category);
        await SeedAboutAsync("Biz kimiz.", PublishStatus.Published);
        await SeedOfferingAsync("Montaj", 1, PublishStatus.Published, flagged: true);
        await SeedPortfolioAsync("Ofis", 1, PublishStatus.Published, flagged: true);

        HomepageView contentFirst = await HomeAsync();
        await SetLayoutAsync("StoreFirst");
        HomepageView storeFirst = await HomeAsync();

        Assert.Equal(
            [HomepageBlockKind.CorporateIntro, HomepageBlockKind.Offerings, HomepageBlockKind.PortfolioItems, HomepageBlockKind.ProductShowcase],
            contentFirst.Blocks);
        Assert.Equal(
            [HomepageBlockKind.ProductShowcase, HomepageBlockKind.CorporateIntro, HomepageBlockKind.Offerings, HomepageBlockKind.PortfolioItems],
            storeFirst.Blocks);

        // Both layouts hold the corporate introduction and the showcase together (02 3.28.1); the same set of blocks, the same content.
        Assert.Equal(contentFirst.Blocks.Order(), storeFirst.Blocks.Order());
        Assert.NotNull(contentFirst.Intro);
        Assert.NotNull(contentFirst.Showcase);
        Assert.NotNull(storeFirst.Intro);
        Assert.NotNull(storeFirst.Showcase);
        Assert.Equal(contentFirst.Offerings!.Cards.Select(c => c.Id), storeFirst.Offerings!.Cards.Select(c => c.Id));
    }

    [Fact]
    public async Task TheMenu_FollowsTheLayout_ContentTypesFirstOrProductsFirst_ContactAlwaysLast_KK1()
    {
        Guid category = await SeedCategoryAsync("Mobilya", null);
        await SeedProductAsync("Masa", PublishStatus.Published, category);
        await SeedAboutAsync("Biz kimiz.", PublishStatus.Published);
        await SeedOfferingAsync("Montaj", 1, PublishStatus.Published, flagged: false);
        await SeedPortfolioAsync("Ofis", 1, PublishStatus.Published, flagged: false);
        await SeedFaqAsync("İade var mı?", PublishStatus.Published);
        await SeedCustomPageAsync("Kampanya", 1, PublishStatus.Published, inMenu: true);

        StorefrontMenu contentFirst = await MenuAsync();
        await SetLayoutAsync("StoreFirst");
        StorefrontMenu storeFirst = await MenuAsync();

        Assert.Equal(
            [MenuItemKind.AboutPage, MenuItemKind.Offerings, MenuItemKind.PortfolioItems, MenuItemKind.FaqItems, MenuItemKind.Products, MenuItemKind.GeneralPage, MenuItemKind.Contact],
            contentFirst.Items.Select(i => i.Kind));
        Assert.Equal(
            [MenuItemKind.Products, MenuItemKind.Offerings, MenuItemKind.PortfolioItems, MenuItemKind.AboutPage, MenuItemKind.FaqItems, MenuItemKind.GeneralPage, MenuItemKind.Contact],
            storeFirst.Items.Select(i => i.Kind));
        Assert.Equal(
            ["/hakkimizda", "/hizmetler", "/referanslar", "/sss", null, "/sayfa/kampanya", "/iletisim"],
            contentFirst.Items.Select(i => i.Path));
    }

    [Fact]
    public async Task TheLayoutSetting_HoldsOnlyTwoValues_AndTheHomepageHasNoBannerCampaignOrProductMark_KK1()
    {
        Assert.Equal(["ContentFirst", "StoreFirst"], Enum.GetNames<HomepageLayout>());
        Assert.Equal(["CorporateIntro", "Offerings", "PortfolioItems", "ProductShowcase"], Enum.GetNames<HomepageBlockKind>());
        string[] forbidden = ["banner", "slider", "campaign", "homepage", "featured"];
        foreach (Type type in new[] { typeof(Product), typeof(Variant), typeof(ShowcaseProduct), typeof(HomepageShowcase) })
        {
            foreach (string name in type.GetProperties().Select(p => p.Name))
            {
                Assert.DoesNotContain(forbidden, word => name.Contains(word, StringComparison.OrdinalIgnoreCase));
            }
        }

        foreach (string name in typeof(HomepageView).GetProperties().Select(p => p.Name))
        {
            Assert.DoesNotContain(["Banner", "Slider", "Campaign"], word => name.Contains(word, StringComparison.OrdinalIgnoreCase));
        }

        await Task.CompletedTask;
    }

    // ---- KK2: the corporate block ----

    [Fact]
    public async Task TheCorporateBlock_ShowsTheShortIntroductionAndTheMainImage_AndLinksToTheAboutPage_KK2()
    {
        Guid about = await SeedAboutAsync("Yirmi yıllık atölyeyiz.", PublishStatus.Published);
        Guid first = await SeedImageAsync();
        Guid main = await SeedImageAsync();
        await SaveAsync(c =>
        {
            c.AboutPageImages.Add(AboutPageImage.Create(about, first, 1, false, Now));
            AboutPageImage mainLink = AboutPageImage.Create(about, main, 2, true, Now);
            mainLink.AltText = "Atölye";
            c.AboutPageImages.Add(mainLink);
        });

        HomepageView home = await HomeAsync();

        Assert.NotNull(home.Intro);
        Assert.Equal("Yirmi yıllık atölyeyiz.", home.Intro.ShortIntroduction);
        Assert.Equal("/hakkimizda", home.Intro.Path);
        Assert.Equal(main, home.Intro.MainImage?.ImageId);
        Assert.Equal("Atölye", home.Intro.MainImage?.AltText);
    }

    [Fact]
    public async Task AnUnpublishedAboutPage_GivesNoIntroduction_ButTheCorporateBlockStays_KK2()
    {
        await SeedAboutAsync("Taslak tanıtım.", PublishStatus.Draft);

        HomepageView home = await HomeAsync();

        Assert.Null(home.Intro);
        Assert.Contains(HomepageBlockKind.CorporateIntro, home.Blocks);
    }

    // ---- KK3: flagged records, in the firm's order ----

    [Fact]
    public async Task TheServiceAndReferenceBlocks_ShowOnlyPublishedFlaggedRecords_InTheManualOrder_AtMostThree_KK3()
    {
        await SeedOfferingAsync("D", 5, PublishStatus.Published, flagged: true);
        await SeedOfferingAsync("A", 2, PublishStatus.Published, flagged: true);
        await SeedOfferingAsync("B", 3, PublishStatus.Published, flagged: true);
        await SeedOfferingAsync("C", 4, PublishStatus.Published, flagged: true);
        await SeedOfferingAsync("İşaretsiz", 1, PublishStatus.Published, flagged: false);
        await SeedOfferingAsync("Taslak", 0, PublishStatus.Draft, flagged: true);
        await SeedPortfolioAsync("Y", 2, PublishStatus.Published, flagged: true);
        await SeedPortfolioAsync("X", 1, PublishStatus.Published, flagged: true);

        HomepageView home = await HomeAsync();

        Assert.NotNull(home.Offerings);
        Assert.Equal(["A", "B", "C"], home.Offerings.Cards.Select(c => c.Title));
        Assert.True(home.Offerings.HasMore);
        Assert.Equal("Hizmetlerimiz", home.Offerings.Title);
        Assert.Equal("/hizmetler", home.Offerings.ListPath);
        Assert.StartsWith("/hizmet/", home.Offerings.Cards[0].Path, StringComparison.Ordinal);
        Assert.NotNull(home.PortfolioItems);
        Assert.Equal(["X", "Y"], home.PortfolioItems.Cards.Select(c => c.Title));
        Assert.False(home.PortfolioItems.HasMore);
        Assert.StartsWith("/referans/", home.PortfolioItems.Cards[0].Path, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ABlockWithoutAFlaggedPublishedRecord_IsNotOnThePage_KK3()
    {
        await SeedOfferingAsync("Taslak", 1, PublishStatus.Draft, flagged: true);
        await SeedOfferingAsync("İşaretsiz", 2, PublishStatus.Published, flagged: false);
        await SeedPortfolioAsync("İşaretsiz", 1, PublishStatus.Published, flagged: false);

        HomepageView home = await HomeAsync();

        Assert.Null(home.Offerings);
        Assert.Null(home.PortfolioItems);
        Assert.DoesNotContain(HomepageBlockKind.Offerings, home.Blocks);
        Assert.DoesNotContain(HomepageBlockKind.PortfolioItems, home.Blocks);
    }

    [Fact]
    public async Task TheCard_CarriesTheMainImageOfTheRecord_KK3()
    {
        Guid offering = await SeedOfferingAsync("Montaj", 1, PublishStatus.Published, flagged: true);
        Guid image = await SeedImageAsync();
        await SaveAsync(c => c.OfferingImages.Add(OfferingImage.Create(offering, image, 1, true, Now)));

        HomepageView home = await HomeAsync();

        Assert.Equal(image, home.Offerings!.Cards.Single().MainImage?.ImageId);
    }

    // ---- KK4: the menu skeleton, the names, the hidden types ----

    [Fact]
    public async Task ARenamedMenuItem_ShowsTheNewNameAtOnce_AndTitlesTheHomepageBlock_KK4()
    {
        await SeedFaqAsync("İade var mı?", PublishStatus.Published);
        await SeedOfferingAsync("Montaj", 1, PublishStatus.Published, flagged: true);
        await SeedPortfolioAsync("Ofis", 1, PublishStatus.Published, flagged: true);
        StorefrontMenu before = await MenuAsync();

        await ExecuteAsync("UPDATE company_settings SET menu_name_portfolio_items = 'Projelerimiz', menu_name_faq_items = 'Sık sorulanlar'");
        StorefrontMenu after = await MenuAsync();
        HomepageView home = await HomeAsync();

        Assert.Equal("Referanslarımız", before.Items.Single(i => i.Kind == MenuItemKind.PortfolioItems).Name);
        Assert.Equal("Projelerimiz", after.Items.Single(i => i.Kind == MenuItemKind.PortfolioItems).Name);
        Assert.Equal("Sık sorulanlar", after.Items.Single(i => i.Kind == MenuItemKind.FaqItems).Name);
        Assert.Equal("Projelerimiz", home.PortfolioItems!.Title);
        Assert.Equal("/referanslar", after.Items.Single(i => i.Kind == MenuItemKind.PortfolioItems).Path);
    }

    [Fact]
    public async Task ATypeWithOnlyDrafts_IsHidden_WithItsReason_AndSoIsAnUnpublishedAboutPage_KK4()
    {
        await SeedOfferingAsync("Taslak hizmet", 1, PublishStatus.Draft, flagged: true);
        await SeedFaqAsync("Taslak soru", PublishStatus.Draft);
        await SeedAboutAsync("Taslak", PublishStatus.Draft);
        await SeedPortfolioAsync("Yayında", 1, PublishStatus.Published, flagged: false);

        StorefrontMenu menu = await MenuAsync();

        MenuItem offerings = menu.Items.Single(i => i.Kind == MenuItemKind.Offerings);
        MenuItem faq = menu.Items.Single(i => i.Kind == MenuItemKind.FaqItems);
        MenuItem about = menu.Items.Single(i => i.Kind == MenuItemKind.AboutPage);
        Assert.False(offerings.IsVisible);
        Assert.Equal(MenuHiddenReason.NoPublishedRecord, offerings.HiddenReason);
        Assert.False(faq.IsVisible);
        Assert.False(about.IsVisible);
        Assert.Equal(MenuHiddenReason.NoPublishedRecord, about.HiddenReason);
        Assert.True(menu.Items.Single(i => i.Kind == MenuItemKind.PortfolioItems).IsVisible);
        Assert.True(menu.Items.Single(i => i.Kind == MenuItemKind.Contact).IsVisible);
        Assert.DoesNotContain(menu.VisibleItems, i => i.Kind is MenuItemKind.Offerings or MenuItemKind.FaqItems or MenuItemKind.AboutPage);
    }

    // ---- KK5: general pages ----

    [Fact]
    public async Task AGeneralPage_IsInTheFooter_AndInTheTopMenuOnlyWhenMarked_BeforeContact_KK5()
    {
        await SeedCustomPageAsync("Kargo bilgisi", 2, PublishStatus.Published, inMenu: false);
        await SeedCustomPageAsync("Kampanya", 1, PublishStatus.Published, inMenu: true);
        await SeedCustomPageAsync("Taslak sayfa", 3, PublishStatus.Draft, inMenu: true);

        StorefrontMenu menu = await MenuAsync();

        Assert.Equal(["Kampanya", "Kargo bilgisi"], menu.FooterPages.Select(p => p.Title));
        Assert.Equal(["/sayfa/kampanya", "/sayfa/kargo-bilgisi"], menu.FooterPages.Select(p => p.Path));
        Assert.Equal([true, false], menu.FooterPages.Select(p => p.IsInMenu));
        MenuItem[] visible = [.. menu.VisibleItems];
        Assert.Equal(["Kampanya", "İletişim"], visible.Select(i => i.Name));
        Assert.Equal(MenuItemKind.GeneralPage, visible[0].Kind);
    }

    // ---- KK6: categories ----

    [Fact]
    public async Task ACategoryWithoutAPublishedProduct_IsNotInTheMenu_AParentWhoseOnlyProductsAreBelowIs_KK6()
    {
        Guid furniture = await SeedCategoryAsync("Mobilya", null);
        Guid tables = await SeedCategoryAsync("Masalar", furniture);
        Guid dining = await SeedCategoryAsync("Yemek masaları", tables);
        Guid chairs = await SeedCategoryAsync("Sandalyeler", furniture);
        Guid empty = await SeedCategoryAsync("Boş kategori", null);
        Guid draftsOnly = await SeedCategoryAsync("Yalnız taslak", null);
        await SeedProductAsync("Yemek masası", PublishStatus.Published, dining);
        await SeedProductAsync("Taslak sandalye", PublishStatus.Draft, chairs);
        await SeedProductAsync("Taslak", PublishStatus.Draft, draftsOnly);
        await SeedProductAsync("Arşiv", PublishStatus.Archived, draftsOnly);

        StorefrontMenu menu = await MenuAsync();

        Assert.Equal(["Mobilya", "Masalar", "Yemek masaları"], menu.Categories.Select(c => c.Name));
        Assert.Equal([1, 2, 3], menu.Categories.Select(c => c.Level));
        Assert.DoesNotContain(menu.Categories, c => c.Id == empty || c.Id == draftsOnly || c.Id == chairs);
        Assert.True(menu.Items.Single(i => i.Kind == MenuItemKind.Products).IsVisible);
    }

    [Fact]
    public async Task TheCategories_AreInTheAlphabeticalOrderOfTheirNames_AtEveryLevel_KK6()
    {
        Guid root = await SeedCategoryAsync("Giyim", null);
        Guid other = await SeedCategoryAsync("Ev", null);
        Guid shoes = await SeedCategoryAsync("Ayakkabı", root);
        Guid bags = await SeedCategoryAsync("Çanta", root);
        Guid wallets = await SeedCategoryAsync("Cüzdan", root);
        foreach (Guid category in new[] { other, shoes, bags, wallets })
        {
            await SeedProductAsync($"Ürün {category}", PublishStatus.Published, category);
        }

        StorefrontMenu menu = await MenuAsync();

        Assert.Equal(["Ev", "Giyim", "Ayakkabı", "Cüzdan", "Çanta"], menu.Categories.Select(c => c.Name));
    }

    [Fact]
    public async Task WithNoPublishedProduct_TheProductsItemIsHidden_WithItsReason_AndThereAreNoCategories_KK6()
    {
        Guid category = await SeedCategoryAsync("Mobilya", null);
        await SeedProductAsync("Taslak", PublishStatus.Draft, category);

        StorefrontMenu menu = await MenuAsync();

        MenuItem products = menu.Items.Single(i => i.Kind == MenuItemKind.Products);
        Assert.False(products.IsVisible);
        Assert.Equal(MenuHiddenReason.NoPublishedProduct, products.HiddenReason);
        Assert.Empty(menu.Categories);
    }

    [Fact]
    public async Task AnEmptyCategoryAddress_ShowsAnEmptyPage_AndAnUnknownAddressIsNotFound_KK6()
    {
        await SeedCategoryAsync("Boş kategori", null);
        Guid category = await SeedCategoryAsync("Mobilya", null);
        await SeedProductAsync("Masa", PublishStatus.Published, category);
        string emptySlug = await SlugOfAsync("Boş kategori");

        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        ICatalogModule catalog = scope.ServiceProvider.GetRequiredService<ICatalogModule>();
        ProductListPage? empty = await catalog.ListCategoryProductsAsync(emptySlug, null, 1, Ct);
        ProductListPage? unknown = await catalog.ListCategoryProductsAsync("yok-boyle-kategori", null, 1, Ct);

        Assert.NotNull(empty);
        Assert.Empty(empty.Items);
        Assert.Null(unknown);
    }

    // ---- The cache: a change drops it at once (05 7.10.1) ----

    [Fact]
    public async Task ThePublicationOfAProduct_DropsTheMenu_AndTheCategoryAndTheItemShowAtOnce_KK6()
    {
        Guid category = await SeedCategoryAsync("Mobilya", null);
        Guid product = await SeedProductAsync("Masa", PublishStatus.Draft, category);
        StorefrontMenu before = await MenuAsync();

        await using (AsyncServiceScope scope = _provider.CreateAsyncScope())
        {
            IProductTransitions transitions = scope.ServiceProvider.GetRequiredService<IProductTransitions>();
            PublishChangeResult result = await scope.ServiceProvider.GetRequiredService<ITransactionRunner>()
                .RunAsync(token => transitions.ChangeProductStatusAsync(product, PublishStatus.Published, 1, token), Ct);
            Assert.True(result.IsChanged);
        }

        StorefrontMenu after = await MenuAsync();
        HomepageView home = await HomeAsync();

        Assert.False(before.Items.Single(i => i.Kind == MenuItemKind.Products).IsVisible);
        Assert.Empty(before.Categories);
        Assert.True(after.Items.Single(i => i.Kind == MenuItemKind.Products).IsVisible);
        Assert.Equal(["Mobilya"], after.Categories.Select(c => c.Name));
        Assert.Equal(["Masa"], home.Showcase!.Products.Select(p => p.Name));
        Assert.Equal(["Mobilya"], home.Showcase.CategoryLinks.Select(c => c.Name));

        await using (AsyncServiceScope scope = _provider.CreateAsyncScope())
        {
            IProductTransitions transitions = scope.ServiceProvider.GetRequiredService<IProductTransitions>();
            PublishChangeResult result = await scope.ServiceProvider.GetRequiredService<ITransactionRunner>()
                .RunAsync(token => transitions.ChangeProductStatusAsync(product, PublishStatus.Draft, 2, token), Ct);
            Assert.True(result.IsChanged);
        }

        StorefrontMenu withdrawn = await MenuAsync();
        Assert.False(withdrawn.Items.Single(i => i.Kind == MenuItemKind.Products).IsVisible);
        Assert.Empty(withdrawn.Categories);
    }

    [Fact]
    public async Task TheCategoriesOfAProduct_DropTheMenu_WhenTheyChange_KK6()
    {
        Guid furniture = await SeedCategoryAsync("Mobilya", null);
        Guid garden = await SeedCategoryAsync("Bahçe", null);
        Guid product = await SeedProductAsync("Masa", PublishStatus.Published, furniture);
        StorefrontMenu before = await MenuAsync();

        await using (AsyncServiceScope scope = _provider.CreateAsyncScope())
        {
            ProductCategoryLinks links = scope.ServiceProvider.GetRequiredService<ProductCategoryLinks>();
            await scope.ServiceProvider.GetRequiredService<ITransactionRunner>()
                .RunAsync(token => links.SetAsync(product, [garden], garden, token), Ct);
        }

        StorefrontMenu after = await MenuAsync();

        Assert.Equal(["Mobilya"], before.Categories.Select(c => c.Name));
        Assert.Equal(["Bahçe"], after.Categories.Select(c => c.Name));
    }

    [Fact]
    public async Task ThePublicationOfAContentRecord_DropsTheMenuAndTheBlocks_AtOnce_KK3()
    {
        Guid offering = await SeedOfferingAsync("Montaj", 1, PublishStatus.Draft, flagged: true);
        HomepageView before = await HomeAsync();
        StorefrontMenu menuBefore = await MenuAsync();

        await using (AsyncServiceScope scope = _provider.CreateAsyncScope())
        {
            ContentPublishing publishing = scope.ServiceProvider.GetRequiredService<ContentPublishing>();
            ContentResult result = await scope.ServiceProvider.GetRequiredService<ITransactionRunner>()
                .RunAsync(token => publishing.PublishAsync(ContentRecordKind.Offering, offering, 1, token), Ct);
            Assert.Equal(ContentOutcome.Changed, result.Outcome);
        }

        HomepageView after = await HomeAsync();
        StorefrontMenu menuAfter = await MenuAsync();

        Assert.Null(before.Offerings);
        Assert.False(menuBefore.Items.Single(i => i.Kind == MenuItemKind.Offerings).IsVisible);
        Assert.Equal(["Montaj"], after.Offerings!.Cards.Select(c => c.Title));
        Assert.True(menuAfter.Items.Single(i => i.Kind == MenuItemKind.Offerings).IsVisible);
    }

    // ---- seeding and reading ----

    private async Task<HomepageView> HomeAsync()
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ICorporateContentModule>().GetHomepageAsync(Ct);
    }

    private async Task<StorefrontMenu> MenuAsync()
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ICorporateContentModule>().GetMenuAsync(Ct);
    }

    private Task SetLayoutAsync(string layout) => ExecuteAsync($"UPDATE company_settings SET homepage_layout = '{layout}'");

    private async Task<Guid> SeedCategoryAsync(string name, Guid? parent)
    {
        int level = 1;
        if (parent is Guid parentId)
        {
            await using AppDbContext reader = NewContext();
            level = await reader.Categories.Where(c => c.Id == parentId).Select(c => c.Level).SingleAsync(Ct) + 1;
        }

        int n = Interlocked.Increment(ref _counter);
        Category category = Category.Create(name, $"kategori-{n:D3}", parent, level, Now.AddMinutes(n));
        await SaveAsync(c => c.Categories.Add(category));
        return category.Id;
    }

    private async Task<string> SlugOfAsync(string name)
    {
        await using AppDbContext reader = NewContext();
        return await reader.Categories.Where(c => c.Name == name).Select(c => c.Slug).SingleAsync(Ct);
    }

    private async Task<Guid> SeedProductAsync(string name, PublishStatus status, Guid category)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create(name, $"urun-{n:D4}", ProductType.Physical, 20m, Now.AddMinutes(n)).WithSeededOrigin();
        product.PublishStatus = status;
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.ProductCategories.Add(ProductCategory.Create(product.Id, category, true, Now));
        });
        return product.Id;
    }

    private async Task<Guid> SeedImageAsync()
    {
        int n = Interlocked.Increment(ref _counter);
        Image image = Image.Create($"gorsel-{n:D4}", ImageFormat.Jpeg, 800, 600, Now);
        await SaveAsync(c => c.Images.Add(image));
        return image.Id;
    }

    private async Task<Guid> SeedAboutAsync(string shortIntroduction, PublishStatus status)
    {
        AboutPage page = AboutPage.Create(Now);
        page.ShortIntroduction = shortIntroduction;
        page.PublishStatus = status;
        await SaveAsync(c => c.AboutPages.Add(page));
        return page.Id;
    }

    private async Task<Guid> SeedOfferingAsync(string name, int order, PublishStatus status, bool flagged)
    {
        int n = Interlocked.Increment(ref _counter);
        Offering offering = Offering.Create(name, $"hizmet-{n:D3}", order, Now.AddMinutes(n));
        offering.PublishStatus = status;
        offering.IsShownOnHomepage = flagged;
        await SaveAsync(c => c.Offerings.Add(offering));
        return offering.Id;
    }

    private async Task<Guid> SeedPortfolioAsync(string title, int order, PublishStatus status, bool flagged)
    {
        int n = Interlocked.Increment(ref _counter);
        PortfolioItem item = PortfolioItem.Create(title, $"referans-{n:D3}", order, Now.AddMinutes(n));
        item.PublishStatus = status;
        item.IsShownOnHomepage = flagged;
        await SaveAsync(c => c.PortfolioItems.Add(item));
        return item.Id;
    }

    private async Task SeedFaqAsync(string question, PublishStatus status)
    {
        FaqItem item = FaqItem.Create(question, 1, Now);
        item.PublishStatus = status;
        await SaveAsync(c => c.FaqItems.Add(item));
    }

    private async Task SeedCustomPageAsync(string title, int order, PublishStatus status, bool inMenu)
    {
        string slug = Slug.From(title, "sayfa");
        CustomPage page = CustomPage.Create(title, slug, order, Now.AddMinutes(order));
        page.PublishStatus = status;
        page.IsShownInMenu = inMenu;
        await SaveAsync(c => c.CustomPages.Add(page));
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task SaveAsync(Action<AppDbContext> change)
    {
        await using AppDbContext context = NewContext();
        change(context);
        await context.SaveChangesAsync(Ct);
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private sealed class FixedActor(Admin admin) : IAuditActorSource
    {
        public Task<AuditActor?> CurrentAsync(CancellationToken ct) => Task.FromResult<AuditActor?>(new AuditActor(admin.Id, admin.Name, admin.Email));
    }
}
