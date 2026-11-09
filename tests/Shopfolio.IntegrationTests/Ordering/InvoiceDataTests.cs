using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>The collection of the invoice data's tests (T357): one database copied from the template.</summary>
[CollectionDefinition(InvoiceDataDefinition.Name)]
public sealed class InvoiceDataDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "InvoiceData";
}

/// <summary>
/// The invoice data on a real PostgreSQL (T357 KK2, KK4; 02 3.25.1, 3.25.3): every field comes from the order's frozen values after the product changed, the fee's VAT is split over the
/// physical items' rates, the hand-over moments follow the item types and a corrected shipment does not count; and when the firm changes its identity, an earlier order's invoice data
/// keeps the identity of its confirmation while the panel shows the new one. The pure composition is the unit tests' (InvoiceDataTests); the absence of an invoice surface is the
/// architecture scan's (InvoiceSurfaceTests).
/// </summary>
[Collection(InvoiceDataDefinition.Name)]
public sealed class InvoiceDataTests : IAsyncLifetime
{
    private const string Domain = "magaza.example.com";
    private static readonly DateOnly Today = new(2026, 11, 20);
    private static readonly DateTimeOffset Now = TurkeyTime.StartOf(Today).AddHours(9);
    private static readonly DateTimeOffset LongAgo = Now.AddDays(-60);

    private static int _versionNumber = 1000;

    private readonly CollectionDatabaseFixture _collection;

    private int _counter;

    public InvoiceDataTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        // The company row and the provinces stay (the identity is read from the company row); only this collection's own records go.
        await ExecuteAsync(
            "TRUNCATE transition_records, checkbox_acceptance_order_items, checkbox_acceptances, order_email_histories, order_address_histories, order_items, orders, digital_files, "
            + "product_images, images, price_histories, discounts, variants, option_values, options, products CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK2: every field from the order's frozen values, the fee's VAT split over the rates ----

    [Fact]
    public async Task TheInvoiceData_GivesEveryFieldFromTheFrozenValues_AfterTheProductChanged_WithTheFeesVatSplit_KK2()
    {
        Guid province = await ProvinceAsync();
        CompanyIdentity identity = await SetCompanyAsync(province, "Örnek Ticaret", "Kızılay 2");
        Guid shirt = await SeedVariantAsync(ProductType.Physical, 500m, 20m);
        Guid book = await SeedVariantAsync(ProductType.Physical, 200m, 10m);
        Guid setup = await SeedVariantAsync(ProductType.Service, 300m, 20m, fulfilmentPeriod: 5);
        Guid ebook = await SeedVariantAsync(ProductType.Digital, 100m, 20m);
        CarrierEntry carrier = CarrierCatalog.Default.Active[0];

        (Guid orderId, List<OrderItem> items) = await SaveOrderAsync(
            identity, province, shippingFee: 40m, o => { o.Carrier = carrier.Code; o.TrackingNumber = "123456"; }, (shirt, 2, 20m), (book, 1, 0m), (setup, 1, 0m), (ebook, 1, 0m));
        OrderItem service = items.Single(i => i.ProductType == ProductType.Service);

        // The payment approval, a shipment, a re-sending that was corrected (it sent nothing) and the service's "completed" mark.
        DateTimeOffset approvedAt = Now.AddHours(1), shippedAt = Now.AddDays(1), resentAt = Now.AddDays(3), completedAt = Now.AddDays(4);
        TransitionRecord resent = Record(orderId, TransitionCode.S8, ActorKind.Admin, null, resentAt);
        await SaveAsync(c => c.TransitionRecords.AddRange(
            Record(orderId, TransitionCode.O1, ActorKind.System, null, approvedAt),
            Record(orderId, TransitionCode.S5, ActorKind.Admin, null, shippedAt),
            resent,
            Record(orderId, TransitionCode.Correction, ActorKind.Admin, resent.Id, resentAt.AddHours(1))));
        await ExecuteAsync($"UPDATE order_items SET delivery_marked_at = '{completedAt:O}', delivery_date = '{TurkeyTime.DateOf(completedAt):yyyy-MM-dd}' WHERE id = '{service.Id}'");

        // The product changes afterwards: its VAT rate and price reach only new orders.
        await ExecuteAsync("UPDATE products SET vat_rate = 1; UPDATE variants SET price = price + 7;");

        InvoiceData data = Assert.Single(await ReadDirectAsync([orderId, Guid.NewGuid()]));
        Order order = await OrderAsync(orderId);

        Assert.Equal((order.OrderNumber, order.Total, 40m, Domain, PaymentMethod.Card), (data.OrderNumber, data.Total, data.ShippingFee, data.Domain, data.PaymentMethod));
        Assert.Equal(
            ItemPricing.SplitShippingFee(40m, [new ShippingBasisLine(ProductType.Physical, 20m, 1000m), new ShippingBasisLine(ProductType.Physical, 10m, 200m)]),
            data.ShippingFeeVatBreakdown);
        Assert.Equal(2, data.ShippingFeeVatBreakdown.Count);
        Assert.Equal(new PartyAddress("Ali Veli", null, province, "Çankaya", "Atatürk Bulvarı 1"), data.BillingAddress);
        Assert.Equal("Örnek Ticaret", data.Company.CompanyLegalName);
        Assert.Equal((approvedAt, carrier.Name), (data.PaymentConfirmedAt!.Value, data.CarrierName));

        Assert.Equal(items.Count, data.Items.Count);
        foreach (OrderItem item in items)
        {
            InvoiceItem line = data.Items.Single(l => l.OrderItemId == item.Id);
            Assert.Equal(
                (item.ProductName, item.Quantity, item.UnitPrice, item.DiscountedUnitPrice, item.CouponShare, item.VatRate, item.VatAmount),
                (line.ProductName, line.Quantity, line.UnitPrice, line.DiscountedUnitPrice, line.CouponShare, line.VatRate, line.VatAmount));
            Assert.NotEqual(1m, line.VatRate);
        }

        Assert.Equal(20m, data.Items.Single(l => l.OrderItemId == items[0].Id).CouponShare);
        Assert.Equal(shippedAt, data.Items.Single(l => l.ProductType == ProductType.Physical && l.Quantity == 2).ShippedAt);
        Assert.Equal(completedAt, data.Items.Single(l => l.ProductType == ProductType.Service).ShippedAt);
        Assert.Equal(approvedAt, data.Items.Single(l => l.ProductType == ProductType.Digital).ShippedAt);
    }

    [Fact]
    public async Task AnEmptyRequest_ReadsNothing_AndAnUnpaidUnshippedOrder_HasNoMoments_KK2()
    {
        Guid province = await ProvinceAsync();
        CompanyIdentity identity = await SetCompanyAsync(province, "Örnek Ticaret", "Kızılay 2");
        Guid shirt = await SeedVariantAsync(ProductType.Physical, 500m, 20m);
        (Guid orderId, _) = await SaveOrderAsync(identity, province, shippingFee: 0m, _ => { }, (shirt, 1, 0m));

        Assert.Empty(await ReadDirectAsync([]));
        InvoiceData data = Assert.Single(await ReadDirectAsync([orderId]));
        Assert.Empty(data.ShippingFeeVatBreakdown);
        Assert.Null(data.PaymentConfirmedAt);
        Assert.Null(data.CarrierName);
        Assert.Null(Assert.Single(data.Items).ShippedAt);
    }

    // ---- KK4: the firm changes its identity; the earlier order keeps the old one ----

    [Fact]
    public async Task WhenTheFirmChangesItsIdentity_AnEarlierOrdersInvoiceData_GivesTheOldIdentity_KK4()
    {
        Guid province = await ProvinceAsync();
        Guid shirt = await SeedVariantAsync(ProductType.Physical, 500m, 20m);
        CompanyIdentity before = await SetCompanyAsync(province, "Eski Unvan", "Eski Sokak 1");
        (Guid earlier, _) = await SaveOrderAsync(before, province, shippingFee: 0m, _ => { }, (shirt, 1, 0m));

        CompanyIdentity after = await SetCompanyAsync(province, "Yeni Unvan", "Yeni Sokak 9");
        (Guid later, _) = await SaveOrderAsync(after, province, shippingFee: 0m, _ => { }, (shirt, 1, 0m));

        IReadOnlyList<InvoiceData> data = await ReadThroughModuleAsync([later, earlier]);
        InvoiceData old = data.Single(d => d.OrderId == earlier);
        InvoiceData current = data.Single(d => d.OrderId == later);

        Assert.Equal(("Eski Unvan", "Eski Sokak 1"), (old.Company.CompanyLegalName, old.Company.CompanyAddressLine));
        Assert.Equal(("Yeni Unvan", "Yeni Sokak 9"), (current.Company.CompanyLegalName, current.Company.CompanyAddressLine));
        Assert.Equal(("Yeni Unvan", "Yeni Sokak 9"), (after.LegalName, after.AddressLine));
        Assert.Equal([earlier, later], data.Select(d => d.OrderId));
    }

    private static TransitionRecord Record(Guid orderId, TransitionCode code, ActorKind actor, Guid? corrected, DateTimeOffset at)
        => TransitionRecord.Create(orderId, code, actor, null, null, null, null, corrected is null ? null : CorrectionReason.WrongOrder, null, corrected, at);

    private async Task<Guid> ProvinceAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.Provinces.AsNoTracking().OrderBy(p => p.Code).Select(p => p.Id).FirstAsync(Ct);
    }

    // The firm's identity as the panel keeps it, read back through the Settings module - the value the confirmation freezes.
    private async Task<CompanyIdentity> SetCompanyAsync(Guid province, string legalName, string addressLine)
    {
        await ExecuteAsync(
            $"UPDATE companies SET company_type = 'Tradesman', legal_name = '{legalName}', tax_id_number = '1111111111', province_id = '{province}', district = 'Çankaya', "
            + $"address_line = '{addressLine}', phone = '3121234567', email = 'firma@example.com', kep_address = 'firma@hs01.kep.tr'");
        await using ServiceProvider provider = OrderingTestServices.Build(_collection.Dsn);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ISettingsModule>().GetCompanyIdentityAsync(Ct);
    }

    private async Task<Guid> SeedVariantAsync(ProductType type, decimal price, decimal vatRate, int? fulfilmentPeriod = null)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create($"Ürün {n}", $"fatura-urun-{n}", type, vatRate, LongAgo);
        if (type == ProductType.Physical)
        {
            product.WithSeededOrigin();
        }

        product.PublishStatus = PublishStatus.Published;
        product.FulfilmentPeriod = fulfilmentPeriod;
        Variant variant = Variant.Create(product.Id, $"FAT-{n}", price, LongAgo, type == ProductType.Physical ? 100 : null);
        variant.PublishStatus = PublishStatus.Published;
        variant.FirstOnSaleAt = LongAgo;
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Variants.Add(variant);
            c.PriceHistories.Add(PriceHistory.Create(variant.Id, price, LongAgo, LongAgo));
        });
        return variant.Id;
    }

    // The confirmation's freezing, as the order's birth calls it: the items from the live read, the identity of this moment, the fee and its VAT split, before the first save.
    private async Task<(Guid OrderId, List<OrderItem> Items)> SaveOrderAsync(
        CompanyIdentity identity, Guid province, decimal shippingFee, Action<Order> shipping, params (Guid VariantId, int Quantity, decimal CouponShare)[] lines)
    {
        int n = Interlocked.Increment(ref _counter);
        List<OrderItemSource> sources = [];
        foreach ((Guid variantId, _, _) in lines)
        {
            sources.Add(await SourceAsync(variantId));
        }

        List<ItemAmounts> amounts = [.. lines.Select((l, i) => ItemPricing.Price(new ItemPriceInput(sources[i].UnitPrice, l.Quantity, l.CouponShare, sources[i].VatRate)))];
        Order order = Order.Create(
            $"2026-F{n:D5}", $"access-{n}-{Guid.NewGuid():N}", $"confirm-{n}-{Guid.NewGuid():N}", PaymentMethod.Card, true, "ali@example.com",
            ItemPricing.SumOrderTotal(amounts, shippingFee), shippingFee, Now.AddHours(1), Now.AddMinutes(n));
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        bool hasPhysical = sources.Any(s => s.ProductType == ProductType.Physical);
        order.FreezeAddresses(
            hasPhysical ? ("Ali Veli", "5321234567", province, "Çankaya", "Atatürk Bulvarı 1") : null,
            ("Ali Veli", province, "Çankaya", "Atatürk Bulvarı 1"));
        OrderFreeze.FreezeLegalIdentity(order, identity, new LegalTextVersionIds(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), null);
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
        order.FreezeShippingFeeVatBreakdown(ItemPricing.SplitShippingFee(
            shippingFee, [.. lines.Select((l, i) => new ShippingBasisLine(sources[i].ProductType, sources[i].VatRate, sources[i].UnitPrice * l.Quantity))]));
        order.FreezeShippingPromise(hasPhysical ? 2 : null);
        shipping(order);
        List<OrderItem> items = [.. lines.Select((l, i) => OrderFreeze.FreezeItem(order.Id, sources[i], l.Quantity, amounts[i], 3, order.CreatedAt))];
        await SaveWithVersionsAsync(order, items);
        return (order.Id, items);
    }

    // The legal text versions the order points to: real rows, as the order's foreign keys require; the version numbers are unique over the collection's tests (the rows stay).
    private async Task SaveWithVersionsAsync(Order order, List<OrderItem> items)
    {
        int n = Interlocked.Increment(ref _versionNumber);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, order.CreatedAt, "metin", order.CreatedAt);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, order.CreatedAt, "metin", order.CreatedAt);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, order.CreatedAt, "metin", order.CreatedAt).WithHosting();
        order.PreInformationFormVersionId = form.Id;
        order.DistanceSalesContractVersionId = contract.Id;
        order.PrivacyNoticeVersionId = notice.Id;
        await SaveAsync(c =>
        {
            c.LegalTextVersions.AddRange(form, contract, notice);
            c.Orders.Add(order);
            c.OrderItems.AddRange(items);
        });
    }

    private async Task<OrderItemSource> SourceAsync(Guid variantId)
    {
        await using AppDbContext db = NewContext();
        TimeProvider time = new FixedTime(Now);
        return Assert.Single(await new OrderItemSourceReader(db, time, new ProductGallery(db, time), new DigitalFileReader(db)).ListAsync([variantId], Ct));
    }

    private async Task<IReadOnlyList<InvoiceData>> ReadDirectAsync(IReadOnlyCollection<Guid> orderIds)
    {
        await using AppDbContext db = NewContext();
        return await new InvoiceDataReader(db, Options.Create(new SiteOptions { Domain = Domain })).ListAsync(orderIds, Ct);
    }

    private async Task<IReadOnlyList<InvoiceData>> ReadThroughModuleAsync(IReadOnlyCollection<Guid> orderIds)
    {
        await using ServiceProvider provider = OrderingTestServices.Build(_collection.Dsn);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IOrderingModule>().ListInvoiceDataAsync(orderIds, Ct);
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

    private sealed class FixedTime(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
