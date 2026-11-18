using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Web;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Browser;

/// <summary>An order of the seed: its number, its access key (a guest's page address) and the pair of states it was seeded in.</summary>
public sealed record SeededOrder(string Label, Guid Id, string Number, string AccessKey, OrderStatus Shipping, PaymentStatus Payment);

/// <summary>A product of the seed with its place in the storefront.</summary>
public sealed record SeededProduct(string Label, Guid ProductId, string Slug, IReadOnlyList<Guid> VariantIds);

/// <summary>What the tool seeds (T551 criterion 2): the customer, the administrator, the catalog and the orders; <see cref="ForRecord"/> is the part the record keeps (no secret).</summary>
public sealed class BrowserSeedData
{
    public string CustomerKey { get; init; } = string.Empty;

    public Guid CustomerId { get; init; }

    public string AdminKey { get; init; } = string.Empty;

    public Guid AdminId { get; init; }

    public Guid ProvinceId { get; init; }

    public string CouponCode { get; init; } = string.Empty;

    public Guid ContactRequestId { get; init; }

    public string CategorySlug { get; init; } = string.Empty;

    /// <summary>The category no published product is in: its page is the empty category page (04 5.2.9).</summary>
    public string EmptyCategorySlug { get; init; } = string.Empty;

    /// <summary>The session key of the bare customer: a second member with no address and no order, so the empty states of the address book (E-26) and the order history (E-27) do not depend on what another test saved (D-1717). Never written to the record.</summary>
    public string BareCustomerKey { get; init; } = string.Empty;

    public string OfferingSlug { get; init; } = string.Empty;

    public Guid OfferingId { get; init; }

    public string PortfolioSlug { get; init; } = string.Empty;

    public string PageSlug { get; init; } = string.Empty;

    public IReadOnlyList<SeededProduct> Products { get; init; } = [];

    public IReadOnlyList<SeededOrder> Orders { get; init; } = [];

    public SeededProduct Product(string label) => Products.Single(p => p.Label == label);

    public SeededOrder Order(string label) => Orders.Single(o => o.Label == label);

    /// <summary>The seed as the record keeps it: the labels, the states and the counts; never a session key or an access key.</summary>
    public object ForRecord() => new
    {
        customers = 2,
        admins = 1,
        coupon = CouponCode,
        products = Products.Select(p => new { p.Label, p.Slug, variants = p.VariantIds.Count }),
        orders = Orders.Select(o => new { o.Label, o.Number, shipping = o.Shipping.ToString(), payment = o.Payment.ToString() }),
    };
}

/// <summary>
/// The tool's seeded data (T551 criterion 2; 12 12.2 (1)): a customer, an administrator, a catalog with a plain product, a product with two option
/// dimensions and a service, a coupon and an order in each state pair the screens draw differently. The rows are made once for the run, in the way
/// the other integration tests seed (<see cref="TestSessions"/>), so a screen that needs more of a kind makes it itself.
/// </summary>
internal static class BrowserSeed
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    /// <summary>The state pairs of the orders: (label, shipping, payment, card). One order of each is enough for the screens' hâls; the full matrix is T547's.</summary>
    private static readonly (string Label, OrderStatus Shipping, PaymentStatus Payment, bool Card)[] States =
    [
        ("awaiting-transfer", OrderStatus.Placed, PaymentStatus.Pending, false),
        ("paid-open", OrderStatus.Preparing, PaymentStatus.Paid, false),
        ("shipped", OrderStatus.Shipped, PaymentStatus.Paid, false),
        ("delivered", OrderStatus.Delivered, PaymentStatus.Paid, false),
        ("closed", OrderStatus.Cancelled, PaymentStatus.Failed, false),
        ("delivery-failed", OrderStatus.DeliveryFailed, PaymentStatus.Paid, false),
        ("partially-refunded", OrderStatus.Delivered, PaymentStatus.PartiallyRefunded, false),
        ("refunded", OrderStatus.Cancelled, PaymentStatus.Refunded, false),
    ];

    /// <summary>A real 1 x 1 PNG; the seeded images (VAL-B100) all show it, so the pages load them without a browser error.</summary>
    private const string PixelPng = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==";

    private static string _imagesDirectory = string.Empty;

    /// <summary>An image row whose source copy is on disk (the key is the fingerprint's shape, 64 hexadecimal digits).</summary>
    private static Image SeededImage(string name)
    {
        string key = Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("t551-" + name)));
        Directory.CreateDirectory(_imagesDirectory);
        File.WriteAllBytes(Path.Combine(_imagesDirectory, key), Convert.FromBase64String(PixelPng));
        return Image.Create(key, ImageFormat.Png, 1, 1, Now);
    }

    public static async Task<BrowserSeedData> RunAsync(string dsn, string imagesDirectory)
    {
        _imagesDirectory = imagesDirectory;
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(dsn, longLived: true);
        (string adminKey, Guid adminId) = await TestSessions.AdminAsync(dsn, longLived: true);
        // The bare customer: no address, no order; only the empty-state observations sign in as it (D-1717).
        (string bareCustomerKey, _) = await TestSessions.CustomerAsync(dsn, longLived: true);
        List<SeededOrder> orders = [];
        foreach ((string label, OrderStatus shipping, PaymentStatus payment, bool card) in States)
        {
            // The first guest order also makes the province and the legal texts every order points at.
            (Guid id, string number, string accessKey) = await TestSessions.OrderAsync(dsn, null);
            await AddItemsAndStateAsync(dsn, id, shipping, payment, card);
            orders.Add(new SeededOrder(label, id, number, accessKey, shipping, payment));
        }

        // The customer's own order: its page is the member address (04 5.16).
        (Guid memberId, string memberNumber, string memberKey) = await TestSessions.OrderAsync(dsn, customerId);
        await AddItemsAndStateAsync(dsn, memberId, OrderStatus.Preparing, PaymentStatus.Paid, false);
        orders.Add(new SeededOrder("member-paid", memberId, memberNumber, memberKey, OrderStatus.Preparing, PaymentStatus.Paid));

        await using AppDbContext db = NewContext(dsn);
        Guid province = await db.Provinces.Select(p => p.Id).FirstAsync();
        List<SeededProduct> products =
        [
            await ProductAsync(dsn, "plain", "Sade ürün", ProductType.Physical, [(100m, null, null)]),
            await ProductAsync(dsn, "variants", "Renkli ve bedenli ürün", ProductType.Physical, [(100m, 1, 1), (120m, 1, 2), (140m, 2, 1), (160m, 2, 2)]),
            await ProductAsync(dsn, "second", "İkinci ürün", ProductType.Physical, [(50m, null, null)]),
            await ProductAsync(dsn, "archived", "Arşivli ürün", ProductType.Physical, [(10m, null, null)], PublishStatus.Archived),
        ];

        const string coupon = "T551KUPON";
        db.Coupons.Add(Coupon.CreatePercentage(coupon, 10m, new DateOnly(2026, 1, 1), new DateOnly(2030, 12, 31), 100, Now));
        ContactRequest contact = ContactRequest.Create("Ayşe Yılmaz", "ayse@example.com", ContactTopic.GeneralQuestion, "Kargo süresi nedir?", Now);
        db.ContactRequests.Add(contact);
        Guid contactId = contact.Id;

        // The corporate content (04 5.7, 5.8): the fixed About page, one record of each of the three types, all published; the published cookie policy.
        AboutPage about = AboutPage.Create(Now);
        about.ShortIntroduction = "Biz bir aile firmasıyız.";
        about.SetLongText("<p>Hikayemiz uzun.</p>");
        about.PublishStatus = PublishStatus.Published;
        Offering offering = Offering.Create("Bakım hizmeti", "t551-hizmet", 1, Now);
        offering.PublishStatus = PublishStatus.Published;
        offering.SetText("<p>Yılda iki bakım.</p>");
        PortfolioItem portfolio = PortfolioItem.Create("Otel projesi", "t551-referans", 1, Now);
        portfolio.PublishStatus = PublishStatus.Published;
        portfolio.SetText("<p>Otel işi.</p>");
        CustomPage page = CustomPage.Create("Kargo bilgisi", "t551-sayfa", 1, Now);
        page.PublishStatus = PublishStatus.Published;
        page.SetText("<p>Kargo üç gündür.</p>");
        db.AboutPages.Add(about);
        db.Offerings.Add(offering);
        db.PortfolioItems.Add(portfolio);
        db.CustomPages.Add(page);
        Category category = Category.Create("Mobilya", "t551-kategori", null, 1, Now);
        Category emptyCategory = Category.Create("Boş kategori", "t551-bos-kategori", null, 1, Now);
        db.Categories.Add(category);
        db.Categories.Add(emptyCategory);
        foreach (SeededProduct product in products.Where(p => p.Label is not ("second" or "archived")))
        {
            db.ProductCategories.Add(ProductCategory.Create(product.ProductId, category.Id, true, Now));
        }

        // The firm (04 5.13): the settings with the bank transfer open and the one delivery province, so the payment step opens for a cart of the seed.
        CompanySettings? settings = await db.CompanySettings.FirstOrDefaultAsync(CancellationToken.None);
        bool newSettings = settings is null;
        settings ??= CompanySettings.Create(Now);
        settings.BankTransferPaymentWindow = 2;
        settings.DefaultShippingPeriod = 2;
        settings.DownloadAllowance = 1;
        settings.IsBankTransferEnabled = true;
        settings.BankTransferIban = "TR330006100519786457841326";
        settings.HomepageLayout = HomepageLayout.StoreFirst;
        settings.MenuNameProducts = "Ürünler";
        settings.MenuNameOfferings = "Hizmetler";
        settings.MenuNamePortfolioItems = "Çalışmalar";
        settings.MenuNameAboutPage = "Hakkımızda";
        settings.MenuNameFaqItems = "Sıkça Sorulanlar";
        settings.MenuNameContact = "İletişim";
        Company? company = await db.Companies.FirstOrDefaultAsync(CancellationToken.None);
        bool newCompany = company is null;
        company ??= Company.Create(Now);
        company.CompanyType = CompanyType.Tradesman;
        company.LegalName = "Örnek Ticaret";
        company.TaxIdNumber = "1234567890";
        company.ProvinceId = province;
        company.District = "Çankaya";
        company.AddressLine = "Kızılay 2";
        company.Phone = "3121234567";
        company.Email = "firma@example.com";
        company.KepAddress = "firma@hs01.kep.tr";
        company.ChamberName = "Ankara Esnaf Odası";
        company.ProfessionalConductRules = "Meslek kuralları";
        if (!await db.CompanyDeliveryProvinces.AnyAsync(CancellationToken.None))
        {
            db.CompanyDeliveryProvinces.Add(CompanyDeliveryProvince.Create(province, Now));
        }

        if (newSettings)
        {
            db.CompanySettings.Add(settings);
        }

        if (newCompany)
        {
            db.Companies.Add(company);
        }

        await db.Database.ExecuteSqlAsync($"INSERT INTO legal_text_versions (id, created_at, legal_text_type, version_number, published_at, text, hosting_location, version) VALUES ({EntityIds.New()}, now(), 'CookiePolicy', 1, now(), '<p>Çerez politikası yayında.</p>', NULL, 1)", CancellationToken.None);
        await db.SaveChangesAsync(CancellationToken.None);

        return new BrowserSeedData
        {
            CustomerKey = customerKey,
            CustomerId = customerId,
            AdminKey = adminKey,
            AdminId = adminId,
            ProvinceId = province,
            CouponCode = coupon,
            ContactRequestId = contactId,
            CategorySlug = category.Slug,
            EmptyCategorySlug = emptyCategory.Slug,
            BareCustomerKey = bareCustomerKey,
            OfferingSlug = offering.Slug,
            OfferingId = offering.Id,
            PortfolioSlug = portfolio.Slug,
            PageSlug = page.Slug,
            Products = products,
            Orders = orders,
        };
    }

    /// <summary>One more guest order in the given pair of states, for a row that changes the order (the confirmed step writes it).</summary>
    public static async Task<SeededOrder> NewOrderAsync(string dsn, string label, OrderStatus shipping, PaymentStatus payment, bool card = false)
    {
        (Guid id, string number, string accessKey) = await TestSessions.OrderAsync(dsn, null);
        await AddItemsAndStateAsync(dsn, id, shipping, payment, card);
        return new SeededOrder(label, id, number, accessKey, shipping, payment);
    }

    /// <summary>A verified customer with a known password (the sign-in of VAL-B110 needs an account that can sign in).</summary>
    public static async Task NewCustomerWithPasswordAsync(string dsn, string email, string password)
    {
        Customer customer = Customer.Create("Giriş Müşterisi", email, new PasswordService().Hash(password), Now, null, Now);
        await using AppDbContext db = NewContext(dsn);
        db.Customers.Add(customer);
        await db.SaveChangesAsync(CancellationToken.None);
    }

    /// <summary>An administrator with a known password (the sign-in and the re-verification of the panel's account screen need an account that can sign in).</summary>
    public static async Task NewAdminWithPasswordAsync(string dsn, string email, string typed)
    {
        Shopfolio.Modules.Administration.Admin admin = Shopfolio.Modules.Administration.Admin.Create("Giriş Yöneticisi", email, new PasswordService().Hash(typed), Now);
        await using AppDbContext db = NewContext(dsn);
        db.Admins.Add(admin);
        await db.SaveChangesAsync(CancellationToken.None);
    }

    /// <summary>A valid, unused administrator invitation: the address key of its link (the invitation screen's form opens only for such a link, 04 5.30).</summary>
    public static async Task<string> NewInvitationKeyAsync(string dsn, LinkKeys keys)
    {
        Shopfolio.Modules.Administration.AdminInvitation invitation = Shopfolio.Modules.Administration.AdminInvitation.Create($"davetli-{Guid.NewGuid():N}@example.com", null, "Gönderen", Now);
        LinkRecord link = LinkRecord.ForInvitation(keys, invitation.Id, Now.AddDays(1), Now);
        await using AppDbContext db = NewContext(dsn);
        db.AdminInvitations.Add(invitation);
        db.LinkRecords.Add(link);
        await db.SaveChangesAsync(CancellationToken.None);
        return keys.Derive(link.Id);
    }

    /// <summary>An administrator whose password reset link is valid (the panel's new password form opens only for such a link, 04 9.1.5): the account has a password; the returned key is the one the e-mail would carry. <paramref name="typed"/> is the password before the reset.</summary>
    public static async Task<string> NewAdminResetKeyAsync(string dsn, LinkKeys keys, string email, string typed)
    {
        Shopfolio.Modules.Administration.Admin admin = Shopfolio.Modules.Administration.Admin.Create("Sıfırlama Yöneticisi", email, new PasswordService().Hash(typed), Now);
        LinkRecord link = LinkRecord.ForAdmin(keys, LinkType.PasswordReset, admin.Id, Now.AddDays(1), Now);
        await using AppDbContext db = NewContext(dsn);
        db.Admins.Add(admin);
        db.LinkRecords.Add(link);
        await db.SaveChangesAsync(CancellationToken.None);
        return keys.Derive(link.Id);
    }

    /// <summary>The address keys of the valid links behind the e-mailed link screens (04 4.1, 4.2): each opens its screen's own, valid state (the button or the form of the link), not the invalid-link state.</summary>
    internal sealed record SeededLinks(string Registration, string NewAddress, string GoogleConfirmation, string PasswordReset, string EmailRevert, string AdminPasswordReset, string AdminNewAddress, string AdminEmailRevert);

    /// <summary>One valid link of every kind the screen inventory opens by a key in its address, each with the record it belongs to (an unverified registration, a pending address change, a verified account).</summary>
    public static async Task<SeededLinks> NewLinksAsync(string dsn, LinkKeys keys)
    {
        string unique = Guid.NewGuid().ToString("N");
        string digest = new PasswordService().Hash("T551-Baglanti-Sifre-1!");
        Customer pending = Customer.Create("Bekleyen Kayıt", $"bekleyen-{unique}@example.com", digest, null, null, Now);
        Customer changing = Customer.Create("Adres Değiştiren", $"degisen-{unique}@example.com", digest, Now, null, Now);
        Customer google = Customer.Create("Google Müşterisi", $"google-{unique}@example.com", null, Now, null, Now);
        Customer reverting = Customer.Create("Geri Alan", $"geri-{unique}@example.com", digest, Now, null, Now);
        Customer resetting = Customer.Create("Sıfırlayan", $"sifirlayan-{unique}@example.com", digest, Now, null, Now);
        Shopfolio.Modules.Administration.Admin adminResetting = Shopfolio.Modules.Administration.Admin.Create("Sıfırlayan Yönetici", $"yonetici-sifirlayan-{unique}@example.com", digest, Now);
        Shopfolio.Modules.Administration.Admin adminChanging = Shopfolio.Modules.Administration.Admin.Create("Adres Değiştiren Yönetici", $"yonetici-degisen-{unique}@example.com", digest, Now);
        Shopfolio.Modules.Administration.Admin adminReverting = Shopfolio.Modules.Administration.Admin.Create("Geri Alan Yönetici", $"yonetici-geri-{unique}@example.com", digest, Now);
        DateTimeOffset end = Now.AddDays(1);
        LinkRecord registration = LinkRecord.ForCustomer(keys, LinkType.EmailVerification, pending.Id, end, Now);
        LinkRecord newAddress = LinkRecord.ForCustomer(keys, LinkType.NewEmailVerification, changing.Id, end, Now);
        LinkRecord confirmation = LinkRecord.ForCustomer(keys, LinkType.GoogleLinkConfirmation, google.Id, end, Now);
        LinkRecord revert = LinkRecord.ForCustomer(keys, LinkType.EmailChangeRevert, reverting.Id, end, Now);
        revert.PreviousEmail = $"onceki-{unique}@example.com";
        LinkRecord reset = LinkRecord.ForCustomer(keys, LinkType.PasswordReset, resetting.Id, end, Now);
        LinkRecord adminReset = LinkRecord.ForAdmin(keys, LinkType.PasswordReset, adminResetting.Id, end, Now);
        LinkRecord adminNewAddress = LinkRecord.ForAdmin(keys, LinkType.NewEmailVerification, adminChanging.Id, end, Now);
        LinkRecord adminRevert = LinkRecord.ForAdmin(keys, LinkType.EmailChangeRevert, adminReverting.Id, end, Now);
        adminRevert.PreviousEmail = $"yonetici-onceki-{unique}@example.com";
        await using AppDbContext db = NewContext(dsn);
        db.Customers.AddRange(pending, changing, google, reverting, resetting);
        db.Admins.AddRange(adminResetting, adminChanging, adminReverting);
        await db.SaveChangesAsync(CancellationToken.None);
        db.PendingEmailChanges.Add(PendingEmailChange.ForCustomer(changing.Id, $"yeni-{unique}@example.com", Now));
        db.PendingEmailChanges.Add(PendingEmailChange.ForAdmin(adminChanging.Id, $"yonetici-yeni-{unique}@example.com", Now));
        db.LinkRecords.AddRange(registration, newAddress, confirmation, revert, reset, adminReset, adminNewAddress, adminRevert);
        await db.SaveChangesAsync(CancellationToken.None);
        return new SeededLinks(keys.Derive(registration.Id), keys.Derive(newAddress.Id), keys.Derive(confirmation.Id), keys.Derive(reset.Id), keys.Derive(revert.Id), keys.Derive(adminReset.Id), keys.Derive(adminNewAddress.Id), keys.Derive(adminRevert.Id));
    }

    private static async Task AddItemsAndStateAsync(string dsn, Guid orderId, OrderStatus shipping, PaymentStatus payment, bool card)
    {
        await using AppDbContext db = NewContext(dsn);
        db.OrderItems.Add(OrderItem.Create(orderId, null, null, ProductType.Physical, "Kapı", 2, 50m, 20m, 16.67m, Now));
        await db.SaveChangesAsync(CancellationToken.None);
        string shippingName = shipping.ToString();
        string paymentName = payment.ToString();
        await db.Database.ExecuteSqlAsync($"UPDATE orders SET order_status = {shippingName}, payment_status = {paymentName} WHERE id = {orderId}", CancellationToken.None);
        if (card)
        {
            await db.Database.ExecuteSqlAsync($"UPDATE orders SET payment_method = 'Card', bank_transfer_iban = NULL WHERE id = {orderId}", CancellationToken.None);
        }
    }

    /// <summary>One more published product with one variant, for a row that changes the product (each observation has its own).</summary>
    public static Task<SeededProduct> NewProductAsync(string dsn, string label, ProductType type) => ProductAsync(dsn, label, "Ürün " + label, type, [(60m, null, null)], PublishStatus.Draft);

    /// <summary>A draft product with no variant yet: its first option can be saved (an existing variant row would have to bind an option value that does not exist before the save).</summary>
    public static Task<SeededProduct> NewProductWithoutVariantAsync(string dsn, string label, ProductType type) => ProductAsync(dsn, label, "Ürün " + label, type, [], PublishStatus.Draft);

    /// <summary>A published product; with first and second places the variants take one value each of two options (the place being the value's order).</summary>
    private static async Task<SeededProduct> ProductAsync(string dsn, string label, string name, ProductType type, (decimal Price, int? First, int? Second)[] specs, PublishStatus status = PublishStatus.Published)
    {
        string slug = $"t551-{label}";
        Product product = Product.Create(name, slug, type, 20m, Now).WithSeededOrigin();
        product.PublishStatus = status;
        Option? first = specs.Any(s => s.First is not null) ? Option.Create(product.Id, "Renk", 1, Now) : null;
        Option? second = specs.Any(s => s.Second is not null) ? Option.Create(product.Id, "Beden", 2, Now) : null;
        Dictionary<int, OptionValue> firstValues = [];
        Dictionary<int, OptionValue> secondValues = [];
        List<Variant> variants = [];
        List<Image> variantImages = [];
        int index = 0;
        foreach ((decimal price, int? firstPlace, int? secondPlace) in specs)
        {
            Variant variant = Variant.Create(product.Id, $"T551-{label}-{index}"[..Math.Min(20, $"T551-{label}-{index}".Length)], price, Now.AddSeconds(index));
            variant.PublishStatus = status;
            variant.StockQuantity = type == ProductType.Digital ? null : (label == "variants" && index == 3 ? 0 : 50);
            if (first is not null && firstPlace is int a)
            {
                if (!firstValues.TryGetValue(a, out OptionValue? value))
                {
                    firstValues[a] = value = OptionValue.Create(first.Id, $"Renk {a}", a, Now);
                }

                variant.FirstOptionValueId = value.Id;
            }

            if (second is not null && secondPlace is int b)
            {
                if (!secondValues.TryGetValue(b, out OptionValue? value))
                {
                    secondValues[b] = value = OptionValue.Create(second.Id, $"Beden {b}", b, Now);
                }

                variant.SecondOptionValueId = value.Id;
            }

            if (label == "variants" && index is 0 or 2)
            {
                // Two of the variants carry an image of their own: the script and the server both change the page's lead image to it (VAL-B100).
                Image own = SeededImage($"{label}-{index}");
                variantImages.Add(own);
                variant.ImageId = own.Id;
            }

            variants.Add(variant);
            index++;
        }

        await using AppDbContext db = NewContext(dsn);
        db.Products.Add(product);
        if (first is not null)
        {
            db.Options.Add(first);
            db.OptionValues.AddRange(firstValues.Values);
        }

        if (second is not null)
        {
            db.Options.Add(second);
            db.OptionValues.AddRange(secondValues.Values);
        }

        if (label == "variants")
        {
            Image lead = SeededImage($"{label}-lead");
            db.Images.Add(lead);
            db.ProductImages.Add(ProductImage.Create(product.Id, lead.Id, 1, true, Now));
        }

        db.Images.AddRange(variantImages);
        db.Variants.AddRange(variants);
        await db.SaveChangesAsync(CancellationToken.None);
        return new SeededProduct(label, product.Id, slug, [.. variants.Select(v => v.Id)]);
    }

    /// <summary>One more offering for a row that saves the content form (the saved form changes the record; the shared offering of the screen inventory stays as seeded).</summary>
    public static async Task<Guid> NewOfferingAsync(string dsn, string label)
    {
        Offering offering = Offering.Create("Bakım hizmeti " + label, "t551-hizmet-" + label, 2, Now);
        offering.PublishStatus = PublishStatus.Published;
        offering.SetText("<p>Yılda iki bakım.</p>");
        await using AppDbContext db = NewContext(dsn);
        db.Offerings.Add(offering);
        await db.SaveChangesAsync(CancellationToken.None);
        return offering.Id;
    }

    private static AppDbContext NewContext(string dsn)
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, dsn);
        return new AppDbContext(builder.Options);
    }
}
