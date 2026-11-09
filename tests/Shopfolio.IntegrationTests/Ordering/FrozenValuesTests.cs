using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>The collection of the frozen values' tests (T355): one database copied from the template.</summary>
[CollectionDefinition(FrozenValuesDefinition.Name)]
public sealed class FrozenValuesDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "FrozenValues";
}

/// <summary>
/// The values an order freezes at its birth, on a real PostgreSQL with the real Catalog read (T355 KK1-KK6; 02 3.23.1-3.23.5): the item keeps its price, discount, coupon share, VAT and
/// description when the product changes; the type, exemption, fulfilment period and download allowance stay and a new order takes the new ones; the item is flat; the firm's identity,
/// the versions and the IBAN stay, and the documents' parties are read from the first row of a corrected value's history; the moment of freezing is the order's; the brand is not
/// frozen. The pure rules are the unit tests' (OrderFreezeTests).
/// </summary>
[Collection(FrozenValuesDefinition.Name)]
public sealed class FrozenValuesTests : IAsyncLifetime
{
    private static readonly DateOnly Today = new(2026, 11, 20);
    private static readonly DateTimeOffset Now = TurkeyTime.StartOf(Today).AddHours(9);
    private static readonly DateTimeOffset LongAgo = Now.AddDays(-60);

    private readonly CollectionDatabaseFixture _collection;
    private readonly MovableTime _time = new(Now);

    private int _counter;

    public FrozenValuesTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE checkbox_acceptance_order_items, checkbox_acceptances, order_email_histories, order_address_histories, order_items, orders, admins, digital_files, product_images, images, price_histories, discounts, "
            + "variants, option_values, options, products, legal_text_versions, provinces CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the item keeps the values that produced the paid amount and the product's description ----

    [Fact]
    public async Task AnItem_KeepsItsPriceDiscountCouponShareVatAndDescription_WhenTheProductChangesLater_KK1()
    {
        Seeded seeded = await SeedProductAsync(ProductType.Physical, price: 1000m, discountPercentage: 20m);
        OrderItemSource source = await SourceAsync(seeded.VariantId);
        Assert.Equal(new DiscountLine(800m, 1000m, 20m, Today, Today.AddDays(5)), source.Discount);

        Guid orderId = await SaveOrderAsync(PaymentMethod.Card, null, (source, 2, 30m));
        OrderItem frozen = await ItemOfAsync(orderId);

        // Every value that produced the paid amount, and the description of that moment (02 3.23.1).
        ItemAmounts amounts = ItemPricing.Price(new ItemPriceInput(800m, 2, 30m, 20m));
        Assert.Equal(1000m, frozen.UnitPrice);
        Assert.Equal(20m, frozen.DiscountPercentage);
        Assert.Equal(800m, frozen.DiscountedUnitPrice);
        Assert.Equal(1000m, frozen.ReferencePrice);
        Assert.Equal(30m, frozen.CouponShare);
        Assert.Equal(20m, frozen.VatRate);
        Assert.Equal(amounts.Vat, frozen.VatAmount);
        Assert.Equal(seeded.ProductName, frozen.ProductName);
        Assert.Equal("Kırmızı", frozen.FirstOptionValue);
        Assert.Equal("M", frozen.SecondOptionValue);
        Assert.Equal(seeded.ImageId, frozen.ImageId);

        // The firm changes everything; the live read sees it, the item does not.
        await ExecuteAsync(
            $"UPDATE products SET name = 'Yeni ad', vat_rate = 10 WHERE id = '{seeded.ProductId}'; "
            + $"UPDATE variants SET price = 1500, image_id = NULL WHERE id = '{seeded.VariantId}'; "
            + $"UPDATE option_values SET value = 'Mavi' WHERE value = 'Kırmızı'; "
            + $"DELETE FROM discounts WHERE product_id = '{seeded.ProductId}'");
        OrderItemSource changed = await SourceAsync(seeded.VariantId);
        Assert.Equal(("Yeni ad", 1500m, 10m, "Mavi", (DiscountLine?)null), (changed.ProductName, changed.UnitPrice, changed.VatRate, changed.FirstOptionValue, changed.Discount));

        OrderItem after = await ItemOfAsync(orderId);
        Assert.Equal(
            (frozen.ProductName, frozen.UnitPrice, frozen.VatRate, frozen.VatAmount, frozen.DiscountPercentage, frozen.DiscountedUnitPrice, frozen.ReferencePrice, frozen.CouponShare, frozen.FirstOptionValue, frozen.ImageId),
            (after.ProductName, after.UnitPrice, after.VatRate, after.VatAmount, after.DiscountPercentage, after.DiscountedUnitPrice, after.ReferencePrice, after.CouponShare, after.FirstOptionValue, after.ImageId));
    }

    // ---- KK2: the type, the exemption, the fulfilment period and the download allowance freeze; a new order takes the new values ----

    [Fact]
    public async Task TheDeliveryLineAndWithdrawalAttributes_Freeze_AndOnlyANewOrderTakesTheirChange_KK2()
    {
        Seeded physical = await SeedProductAsync(ProductType.Physical, 100m, exemption: WithdrawalExemptionReason.MadeToOrder);
        Seeded service = await SeedProductAsync(ProductType.Service, 200m, fulfilmentPeriod: 5);
        Seeded digital = await SeedProductAsync(ProductType.Digital, 50m);
        Guid fileId = await SeedFileAsync(digital.VariantId);

        Guid first = await SaveOrderAsync(
            PaymentMethod.Card, null, downloadAllowance: 3, (await SourceAsync(physical.VariantId), 1, 0m), (await SourceAsync(service.VariantId), 1, 0m), (await SourceAsync(digital.VariantId), 1, 0m));
        await ExecuteAsync(
            $"UPDATE products SET withdrawal_exemption_reason = NULL WHERE id = '{physical.ProductId}'; "
            + $"UPDATE products SET fulfilment_period = 9 WHERE id = '{service.ProductId}'");

        // The P-8 setting is now 7: the new order freezes 7 and the changed product values; the first order keeps its own.
        Guid second = await SaveOrderAsync(
            PaymentMethod.Card, null, downloadAllowance: 7, (await SourceAsync(physical.VariantId), 1, 0m), (await SourceAsync(service.VariantId), 1, 0m), (await SourceAsync(digital.VariantId), 1, 0m));

        Dictionary<ProductType, OrderItem> before = (await ItemsOfAsync(first)).ToDictionary(i => i.ProductType);
        Dictionary<ProductType, OrderItem> later = (await ItemsOfAsync(second)).ToDictionary(i => i.ProductType);
        Assert.Equal(WithdrawalExemptionReason.MadeToOrder, before[ProductType.Physical].WithdrawalExemptionReason);
        Assert.Null(later[ProductType.Physical].WithdrawalExemptionReason);
        Assert.Equal(5, before[ProductType.Service].FulfilmentPeriod);
        Assert.Equal(9, later[ProductType.Service].FulfilmentPeriod);
        Assert.Equal(3, before[ProductType.Digital].DownloadAllowance);
        Assert.Equal(7, later[ProductType.Digital].DownloadAllowance);
        Assert.Equal(fileId, before[ProductType.Digital].DigitalFileId);

        // The type-bound fields stay on their own type: the database's checks hold them (K-1049).
        Assert.Null(before[ProductType.Service].WithdrawalExemptionReason);
        Assert.Null(before[ProductType.Physical].DownloadAllowance);
        Assert.Null(before[ProductType.Digital].FulfilmentPeriod);

        // The buttons read the frozen values, not the product (K-598): both orders shipped and paid, the product no longer exempt - the first order's withdrawal stays closed
        // by its frozen exemption, the second order's is open; the download button counts the allowance each order froze.
        await ExecuteAsync($"UPDATE orders SET order_status = 'Shipped', payment_status = 'Paid' WHERE id IN ('{first}', '{second}')");
        OrderOperationsView beforeView = Assert.IsType<OrderOperationsView>(await ReadOperationsAsync(first));
        OrderOperationsView laterView = Assert.IsType<OrderOperationsView>(await ReadOperationsAsync(second));
        Assert.False(beforeView.IsOpen(OrderOperation.WithdrawPhysicalItem, before[ProductType.Physical].Id));
        Assert.True(laterView.IsOpen(OrderOperation.WithdrawPhysicalItem, later[ProductType.Physical].Id));
        Assert.True(beforeView.IsOpen(OrderOperation.DownloadDigitalFile, before[ProductType.Digital].Id));
        Assert.Equal(
            OperationDecision.Closed,
            (await CheckOperationAsync(first, OrderOperation.WithdrawPhysicalItem, before[ProductType.Physical].Id, 1)).Decision);

        // KK4's boxes: the ticked digital box's record keeps the text shown at confirmation and the item it covers (02 3.24.6); the record is never deleted.
        CheckboxAcceptance box = CheckboxAcceptance.Create(first, CheckboxType.Digital, "İndirme, ödemem onaylandığı anda açılacak", Now);
        await SaveAsync(c =>
        {
            c.CheckboxAcceptances.Add(box);
            c.CheckboxAcceptanceOrderItems.Add(CheckboxAcceptanceOrderItem.Create(box.Id, before[ProductType.Digital].Id, Now));
        });
        await using AppDbContext db = NewContext();
        Assert.Equal("İndirme, ödemem onaylandığı anda açılacak", (await db.CheckboxAcceptances.AsNoTracking().SingleAsync(b => b.OrderId == first, Ct)).ShownText);
        Assert.Equal(before[ProductType.Digital].Id, (await db.CheckboxAcceptanceOrderItems.AsNoTracking().SingleAsync(l => l.CheckboxAcceptanceId == box.Id, Ct)).OrderItemId);
    }

    // ---- KK3: the frozen values are flat in the item; no grouping table between the order and its items ----

    [Fact]
    public async Task TheFrozenValues_LiveFlatInTheItem_AndTheOrderCarriesOnlyTheCartLevelValues_KK3()
    {
        await using AppDbContext db = NewContext();
        IEntityType item = db.Model.FindEntityType(typeof(OrderItem))!;
        IEntityType order = db.Model.FindEntityType(typeof(Order))!;
        string[] flat =
        [
            nameof(OrderItem.UnitPrice), nameof(OrderItem.VatRate), nameof(OrderItem.VatAmount), nameof(OrderItem.DiscountPercentage), nameof(OrderItem.DiscountedUnitPrice),
            nameof(OrderItem.ReferencePrice), nameof(OrderItem.CouponShare), nameof(OrderItem.ProductName), nameof(OrderItem.FirstOptionValue), nameof(OrderItem.SecondOptionValue),
            nameof(OrderItem.ImageId), nameof(OrderItem.ProductType), nameof(OrderItem.WithdrawalExemptionReason), nameof(OrderItem.FulfilmentPeriod), nameof(OrderItem.DownloadAllowance),
        ];
        Assert.All(flat, name => Assert.NotNull(item.FindProperty(name)));

        // The item points straight at its order; no entity stands between them.
        IForeignKey toOrder = Assert.Single(item.GetForeignKeys(), fk => fk.PrincipalEntityType == order);
        Assert.Equal(nameof(OrderItem.OrderId), Assert.Single(toOrder.Properties).Name);
        Assert.DoesNotContain(
            db.Model.GetEntityTypes(),
            e => e != item && e.GetForeignKeys().Any(fk => fk.PrincipalEntityType == order) && item.GetForeignKeys().Any(fk => fk.PrincipalEntityType == e));

        // The order carries the cart-level values only: the coupon code, the shipping fee and the total - no product-level value.
        Assert.NotNull(order.FindProperty(nameof(Order.CouponCode)));
        Assert.NotNull(order.FindProperty(nameof(Order.ShippingFee)));
        Assert.NotNull(order.FindProperty(nameof(Order.Total)));
        Assert.All(
            [nameof(OrderItem.UnitPrice), nameof(OrderItem.VatRate), nameof(OrderItem.DiscountPercentage), nameof(OrderItem.ReferencePrice), nameof(OrderItem.ProductName)],
            name => Assert.Null(order.FindProperty(name)));
    }

    // ---- KK4: addresses, identity, versions, boxes, IBAN freeze; the documents show the confirmation's parties after a correction ----

    [Fact]
    public async Task TheIdentityVersionsAndIban_Freeze_AndACorrectedAddressOrEmail_LeavesTheDocumentsPartiesAsConfirmed_KK4()
    {
        Seeded seeded = await SeedProductAsync(ProductType.Physical, 100m);
        Guid orderId = await SaveOrderAsync(PaymentMethod.BankTransfer, "TR330006100519786457841326", (await SourceAsync(seeded.VariantId), 1, 0m));

        Order frozen = await OrderAsync(orderId);
        Assert.Equal(CompanyType.Tradesman, frozen.CompanyType);
        Assert.Equal("Örnek Ticaret", frozen.CompanyLegalName);
        Assert.Equal("1234567890", frozen.CompanyTaxIdNumber);
        Assert.Null(frozen.CompanyMersisNumber);
        Assert.Equal("TR330006100519786457841326", frozen.BankTransferIban);

        // The firm corrects the shipping address and the guest's e-mail: the order shows the new values, the documents the confirmed ones (K-1027).
        Admin admin = Admin.Create("Veli", $"veli-{EntityIds.New():N}@example.com", "digest-placeholder", Now);
        Guid province = frozen.ShippingProvinceId!.Value;
        await SaveAsync(c =>
        {
            c.Admins.Add(admin);
            c.OrderAddressHistories.Add(OrderAddressHistory.Create(
                orderId, AddressType.ShippingAddress, ("Ali Veli", "5321234567", province, "Çankaya", "Atatürk Bulvarı 1"), ("Ali Veli", "5329999999", province, "Keçiören", "Yeni Sokak 2"), admin.Id, admin.Name, Now.AddHours(1)));
            c.OrderAddressHistories.Add(OrderAddressHistory.Create(
                orderId, AddressType.ShippingAddress, ("Ali Veli", "5329999999", province, "Keçiören", "Yeni Sokak 2"), ("Ali Veli", "5328888888", province, "Mamak", "Son Sokak 3"), admin.Id, admin.Name, Now.AddHours(2)));
            c.OrderEmailHistories.Add(OrderEmailHistory.Create(orderId, "Ali@Example.com", "yeni@example.com", admin.Id, admin.Name, Now.AddHours(1)));
        });
        await ExecuteAsync(
            $"UPDATE orders SET shipping_phone = '5328888888', shipping_district = 'Mamak', shipping_address_line = 'Son Sokak 3', "
            + $"contact_email = 'yeni@example.com', normalized_contact_email = 'yeni@example.com' WHERE id = '{orderId}'");

        ConfirmationParties parties = Assert.IsType<ConfirmationParties>(await ReadPartiesAsync(orderId));
        Assert.Equal("Ali@Example.com", parties.ContactEmail);
        Assert.Equal(new PartyAddress("Ali Veli", "5321234567", province, "Çankaya", "Atatürk Bulvarı 1"), parties.Shipping);
        Assert.Equal(new PartyAddress("Ali Veli", null, province, "Çankaya", "Atatürk Bulvarı 1"), parties.Billing);
        Assert.Null(await ReadPartiesAsync(Guid.NewGuid()));

        // The firm's identity is not corrected: a later change of the firm leaves the order's copy alone.
        Order after = await OrderAsync(orderId);
        Assert.Equal((frozen.CompanyLegalName, frozen.PreInformationFormVersionId, frozen.DistanceSalesContractVersionId, frozen.PrivacyNoticeVersionId), (after.CompanyLegalName, after.PreInformationFormVersionId, after.DistanceSalesContractVersionId, after.PrivacyNoticeVersionId));
    }

    // ---- KK5: the moment of freezing is the order's; the promise stays ----

    [Fact]
    public async Task TheMomentOfFreezing_IsTheOrders_AndADiscountEndingLater_DoesNotReachTheItem_KK5()
    {
        Seeded seeded = await SeedProductAsync(ProductType.Physical, 1000m, discountPercentage: 20m);
        Guid orderId = await SaveOrderAsync(PaymentMethod.Card, null, (await SourceAsync(seeded.VariantId), 1, 0m));

        // The discount's range is over a week later: a read now has no discount, the item keeps the one of its moment.
        _time.Now = Now.AddDays(7);
        Assert.Null((await SourceAsync(seeded.VariantId)).Discount);
        OrderItem item = await ItemOfAsync(orderId);
        Order order = await OrderAsync(orderId);
        Assert.Equal(800m, item.DiscountedUnitPrice);
        Assert.Equal(Now, order.CreatedAt);
        Assert.Equal(Now, item.CreatedAt);
        Assert.Equal(2, order.ShippingPeriod);
        Assert.Equal(800m, order.Total);
    }

    // ---- KK6: the brand settings are not frozen ----

    [Fact]
    public void TheOrderAndItsItems_HaveNoBrandColumn_SoAPastOrderShowsTheCurrentBrand_KK6()
    {
        using AppDbContext db = NewContext();
        string[] brand = ["brand", "logo", "color", "colour", "favicon", "site_icon"];
        IEnumerable<string> columns = new[] { typeof(Order), typeof(OrderItem) }
            .SelectMany(t => db.Model.FindEntityType(t)!.GetProperties())
            .Select(p => p.GetColumnName());
        Assert.DoesNotContain(columns, c => brand.Any(b => c.Contains(b, StringComparison.OrdinalIgnoreCase)));
    }

    private sealed record Seeded(Guid ProductId, Guid VariantId, string ProductName, Guid? ImageId);

    private async Task<Seeded> SeedProductAsync(
        ProductType type, decimal price, decimal? discountPercentage = null, WithdrawalExemptionReason? exemption = null, int? fulfilmentPeriod = null)
    {
        int n = Interlocked.Increment(ref _counter);
        string name = $"Ürün {n}";
        Product product = Product.Create(name, $"urun-{n}", type, 20m, LongAgo);
        if (type == ProductType.Physical)
        {
            product.WithSeededOrigin();
        }

        product.PublishStatus = PublishStatus.Published;
        product.WithdrawalExemptionReason = exemption;
        product.FulfilmentPeriod = fulfilmentPeriod;
        Option color = Option.Create(product.Id, "Renk", 1, LongAgo);
        Option size = Option.Create(product.Id, "Beden", 2, LongAgo);
        OptionValue red = OptionValue.Create(color.Id, "Kırmızı", 1, LongAgo);
        OptionValue medium = OptionValue.Create(size.Id, "M", 1, LongAgo);
        Image image = Image.Create(EntityIds.NewStorageKey(), ImageFormat.Jpeg, 100, 100, LongAgo);
        Variant variant = Variant.Create(product.Id, $"DON-{n}", price, LongAgo, type == ProductType.Physical ? 100 : null);
        variant.FirstOptionValueId = red.Id;
        variant.SecondOptionValueId = medium.Id;
        variant.ImageId = image.Id;
        variant.PublishStatus = PublishStatus.Published;
        variant.FirstOnSaleAt = LongAgo;
        await SaveAsync(c =>
        {
            c.Images.Add(image);
            c.Products.Add(product);
            c.Options.AddRange(color, size);
            c.OptionValues.AddRange(red, medium);
            c.Variants.Add(variant);
            c.PriceHistories.Add(PriceHistory.Create(variant.Id, price, LongAgo, LongAgo));
            if (discountPercentage is decimal percentage)
            {
                c.Discounts.Add(Discount.Create(product.Id, percentage, Today, Today.AddDays(5), LongAgo));
            }
        });
        return new Seeded(product.Id, variant.Id, name, image.Id);
    }

    private async Task<Guid> SeedFileAsync(Guid variantId)
    {
        DigitalFile file = DigitalFile.Create(EntityIds.NewStorageKey(), "kitap.pdf", 100, LongAgo);
        file.UploadCompletedAt = LongAgo;
        file.VariantId = variantId;
        file.AttachedAt = LongAgo;
        await SaveAsync(c => c.DigitalFiles.Add(file));
        return file.Id;
    }

    private Task<Guid> SaveOrderAsync(PaymentMethod method, string? iban, params (OrderItemSource Source, int Quantity, decimal CouponShare)[] lines)
        => SaveOrderAsync(method, iban, downloadAllowance: 3, lines);

    // The confirmation's freezing, as the order's birth will call it: the items from the live read, the identity, the versions and the IBAN of this moment, before the first save.
    private async Task<Guid> SaveOrderAsync(PaymentMethod method, string? iban, int downloadAllowance, params (OrderItemSource Source, int Quantity, decimal CouponShare)[] lines)
    {
        int n = Interlocked.Increment(ref _counter);
        DateTimeOffset now = _time.GetUtcNow();
        Province province = Province.Create($"{n:D2}", $"İl {n}", now);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, now, "metin", now);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, now, "metin", now);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, now, "metin", now).WithHosting();
        List<ItemAmounts> amounts = [.. lines.Select(l => ItemPricing.Price(new ItemPriceInput(l.Source.Discount?.DiscountedPrice ?? l.Source.UnitPrice, l.Quantity, l.CouponShare, l.Source.VatRate)))];
        Order order = Order.Create(
            $"2026-{n:D6}", $"access-{n}-{Guid.NewGuid():N}", $"confirm-{n}-{Guid.NewGuid():N}", method, true, "Ali@Example.com", ItemPricing.SumOrderTotal(amounts, 0m), 0m, now.AddHours(1), now);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        bool hasPhysical = lines.Any(l => l.Source.ProductType == ProductType.Physical);
        order.FreezeAddresses(
            hasPhysical ? ("Ali Veli", "5321234567", province.Id, "Çankaya", "Atatürk Bulvarı 1") : null,
            ("Ali Veli", province.Id, "Çankaya", "Atatürk Bulvarı 1"));
        CompanyIdentity identity = new(
            1, CompanyType.Tradesman, "Örnek Ticaret", "1234567890", "0123456789012345", null, province.Id, "Çankaya", "Kızılay 2", "3121234567", "firma@example.com",
            "firma@hs01.kep.tr", "Ankara Esnaf Odası", null, "Meslek kuralları", null);
        OrderFreeze.FreezeLegalIdentity(order, identity, new LegalTextVersionIds(form.Id, contract.Id, notice.Id), iban);
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
        order.FreezeShippingFeeVatBreakdown([]);
        order.FreezeShippingPromise(hasPhysical ? 2 : null);
        List<OrderItem> items = [.. lines.Select((l, i) => OrderFreeze.FreezeItem(order.Id, l.Source, l.Quantity, amounts[i], downloadAllowance, now))];
        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.LegalTextVersions.AddRange(form, contract, notice);
            c.Orders.Add(order);
            c.OrderItems.AddRange(items);
        });
        return order.Id;
    }

    private async Task<OrderItemSource> SourceAsync(Guid variantId)
    {
        await using AppDbContext db = NewContext();
        return Assert.Single(await new OrderItemSourceReader(db, _time, new ProductGallery(db, _time), new DigitalFileReader(db)).ListAsync([variantId], Ct));
    }

    private async Task<ConfirmationParties?> ReadPartiesAsync(Guid orderId)
    {
        await using ServiceProvider provider = OrderingTestServices.Build(_collection.Dsn);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IOrderingModule>().GetConfirmationPartiesAsync(orderId, Ct);
    }

    private async Task<OrderOperationsView?> ReadOperationsAsync(Guid orderId)
    {
        await using ServiceProvider provider = OrderingTestServices.Build(_collection.Dsn);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IOrderOperations>().ReadAsync(orderId, ActorKind.Buyer, Ct);
    }

    private async Task<OperationVerdict> CheckOperationAsync(Guid orderId, OrderOperation operation, Guid itemId, int quantity)
    {
        await using ServiceProvider provider = OrderingTestServices.Build(_collection.Dsn);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IOrderOperations>().CheckAsync(orderId, operation, itemId, quantity, Ct);
    }

    private async Task<OrderItem> ItemOfAsync(Guid orderId) => Assert.Single(await ItemsOfAsync(orderId));

    private async Task<List<OrderItem>> ItemsOfAsync(Guid orderId)
    {
        await using AppDbContext db = NewContext();
        return await db.OrderItems.AsNoTracking().Where(i => i.OrderId == orderId).ToListAsync(Ct);
    }

    private async Task<Order> OrderAsync(Guid orderId)
    {
        await using AppDbContext db = NewContext();
        return await db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
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

    private sealed class MovableTime(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;

        public override DateTimeOffset GetUtcNow() => Now;
    }
}
