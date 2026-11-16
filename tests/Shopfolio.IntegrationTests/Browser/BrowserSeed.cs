using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Web;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

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
        customers = 1,
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
    ];

    public static async Task<BrowserSeedData> RunAsync(string dsn)
    {
        (string customerKey, Guid customerId) = await TestSessions.CustomerAsync(dsn, longLived: true);
        (string adminKey, Guid adminId) = await TestSessions.AdminAsync(dsn, longLived: true);
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
        db.Categories.Add(category);
        foreach (SeededProduct product in products.Where(p => p.Label != "second"))
        {
            db.ProductCategories.Add(ProductCategory.Create(product.ProductId, category.Id, true, Now));
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
            OfferingSlug = offering.Slug,
            OfferingId = offering.Id,
            PortfolioSlug = portfolio.Slug,
            PageSlug = page.Slug,
            Products = products,
            Orders = orders,
        };
    }

    /// <summary>One more guest order in the given pair of states, for a row that changes the order (the confirmed step writes it).</summary>
    public static async Task<SeededOrder> NewOrderAsync(string dsn, string label, OrderStatus shipping, PaymentStatus payment)
    {
        (Guid id, string number, string accessKey) = await TestSessions.OrderAsync(dsn, null);
        await AddItemsAndStateAsync(dsn, id, shipping, payment, false);
        return new SeededOrder(label, id, number, accessKey, shipping, payment);
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

    /// <summary>A published product; with first and second places the variants take one value each of two options (the place being the value's order).</summary>
    private static async Task<SeededProduct> ProductAsync(string dsn, string label, string name, ProductType type, (decimal Price, int? First, int? Second)[] specs)
    {
        string slug = $"t551-{label}";
        Product product = Product.Create(name, slug, type, 20m, Now).WithSeededOrigin();
        product.PublishStatus = PublishStatus.Published;
        Option? first = specs.Any(s => s.First is not null) ? Option.Create(product.Id, "Renk", 1, Now) : null;
        Option? second = specs.Any(s => s.Second is not null) ? Option.Create(product.Id, "Beden", 2, Now) : null;
        Dictionary<int, OptionValue> firstValues = [];
        Dictionary<int, OptionValue> secondValues = [];
        List<Variant> variants = [];
        int index = 0;
        foreach ((decimal price, int? firstPlace, int? secondPlace) in specs)
        {
            Variant variant = Variant.Create(product.Id, $"T551-{label}-{index}"[..Math.Min(20, $"T551-{label}-{index}".Length)], price, Now.AddSeconds(index));
            variant.PublishStatus = PublishStatus.Published;
            variant.StockQuantity = 50;
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

        db.Variants.AddRange(variants);
        await db.SaveChangesAsync(CancellationToken.None);
        return new SeededProduct(label, product.Id, slug, [.. variants.Select(v => v.Id)]);
    }

    private static AppDbContext NewContext(string dsn)
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, dsn);
        return new AppDbContext(builder.Options);
    }
}
