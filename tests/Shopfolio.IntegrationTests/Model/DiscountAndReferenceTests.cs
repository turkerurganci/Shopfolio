using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Lists;
using Shopfolio.Modules.Catalog.Contracts.Pricing;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the discount and reference price tests (T337): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(DiscountAndReferenceDefinition.Name)]
public sealed class DiscountAndReferenceDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "DiscountAndReference";
}

/// <summary>
/// The dated discount and the reference price through the real Catalog code on a real PostgreSQL (T337 criteria 1-8; KK1-KK8; 02 3.9.1-3.9.5, 5.11.1; 06 4.11, 4.12):
/// the writers join the test's transaction, the price history gets the rows each change owes, a refusal names the variant and leaves nothing behind - even when the
/// caller saves anyway -, the scheduler's source writes the range's bounds at their own moments, and the label and the card carry the running discount's declaration
/// with its dates. The clock is the test's; Turkey is UTC+3, so the moments come from <see cref="TurkeyTime"/>. The pure rules are the unit tests'
/// (DiscountRulesTests, ReferencePriceRulesTests).
/// </summary>
[Collection(DiscountAndReferenceDefinition.Name)]
public sealed class DiscountAndReferenceTests : IAsyncLifetime
{
    private static readonly DateOnly Today = new(2026, 11, 20);
    private static readonly DateTimeOffset Now = TurkeyTime.StartOf(Today).AddHours(9);
    private static readonly DateTimeOffset LongAgo = Now.AddDays(-60);
    private static readonly string[] SaveDiscountParameters = ["productId", "percentage", "startDate", "endDate", "ct"];

    private readonly CollectionDatabaseFixture _collection;
    private readonly MovableTime _time = new(Now);

    private int _counter;

    public DiscountAndReferenceTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE price_histories, discounts, variants, option_values, options, products CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: every variant on its own price ----

    [Fact]
    public async Task ADiscount_ComesDownToEveryVariantOnItsOwnPrice_AndItsAppliedPriceIsWrittenToTheHistory()
    {
        (Product product, Variant[] v) = await SeedAsync(ProductType.Physical, null, 1000m, 1250m);

        Assert.Same(DiscountSaveResult.Saved, await RunAsync(db => DiscountsOf(db).SaveAsync(product.Id, 20m, Today, Today.AddDays(5), Ct)));

        Assert.Equal((Now, 800m), (await HistoryAsync(v[0].Id))[^1]);
        Assert.Equal((Now, 1000m), (await HistoryAsync(v[1].Id))[^1]);
        DiscountView view = Assert.IsType<DiscountView>(await ReadAsync(db => DiscountsOf(db).GetAsync(product.Id, Ct)));
        Assert.Equal(DiscountState.Running, view.State);
        Assert.Equal(
            new[] { new DiscountVariantLine(v[0].Id, 1000m, 1000m, 800m), new DiscountVariantLine(v[1].Id, 1250m, 1250m, 1000m) },
            view.Variants.OrderBy(l => l.Price));
    }

    // ---- KK2: one hundred percent and a zero result ----

    [Fact]
    public async Task AHundredPercent_AndADiscountRoundedToZero_AreRefused_AndWriteNothingEvenWhenTheCallerSaves()
    {
        (Product product, Variant[] v) = await SeedAsync(ProductType.Physical, null, 10m, 0.01m);
        long rows = await CountAsync("price_histories");

        Assert.Same(DiscountSaveResult.InvalidPercentage, await SaveEvenIfRefusedAsync(db => DiscountsOf(db).SaveAsync(product.Id, 100m, Today, Today, Ct)));
        DiscountSaveResult zero = await SaveEvenIfRefusedAsync(db => DiscountsOf(db).SaveAsync(product.Id, 60m, Today, Today, Ct));

        Assert.Equal(("DiscountedPriceZero", v[1].Id, 0m), (zero.Reason, zero.VariantId, zero.DiscountedPrice));
        Assert.Equal(0L, await CountAsync("discounts"));
        Assert.Equal(rows, await CountAsync("price_histories"));
    }

    [Fact]
    public async Task WhileTheDiscountHasNotEnded_APriceItWouldBringToZeroIsRefused()
    {
        (Product product, Variant[] v) = await SeedAsync(ProductType.Physical, null, 10m);
        Assert.Same(DiscountSaveResult.Saved, await RunAsync(db => DiscountsOf(db).SaveAsync(product.Id, 60m, Today.AddDays(1), Today.AddDays(2), Ct)));

        // Not started yet, then running: 0,01 x 0,40 = 0,004 -> 0,00 is refused; 0,02 x 0,40 = 0,008 -> 0,01 is not.
        Assert.Same(VariantPriceResult.DiscountedPriceZero, await RunAsync(db => PricesOf(db).ChangeAsync(v[0].Id, 0.01m, Ct)));
        _time.Now = TurkeyTime.StartOf(Today.AddDays(1)).AddHours(1);
        Assert.Same(VariantPriceResult.DiscountedPriceZero, await RunAsync(db => PricesOf(db).ChangeAsync(v[0].Id, 0.01m, Ct)));
        Assert.Same(VariantPriceResult.Changed, await RunAsync(db => PricesOf(db).ChangeAsync(v[0].Id, 0.02m, Ct)));

        // Ended: the price is free again.
        _time.Now = TurkeyTime.EndOfRange(Today.AddDays(2));
        Assert.Same(VariantPriceResult.Changed, await RunAsync(db => PricesOf(db).ChangeAsync(v[0].Id, 0.01m, Ct)));
        Assert.Equal(0.01m, (await HistoryAsync(v[0].Id))[^1].Price);
    }

    // ---- KK3: 00.00 of the start day, the end day whole; the scheduler writes the bounds at their own moments ----

    [Fact]
    public async Task TheScheduler_OpensTheDiscountAtMidnightOfItsStartDay_AndClosesItAfterItsEndDay_EvenWhenLate()
    {
        (Product product, Variant[] v) = await SeedAsync(ProductType.Physical, null, 100m);
        DateOnly start = Today.AddDays(2);
        DateOnly end = Today.AddDays(3);
        Assert.Same(DiscountSaveResult.Saved, await RunAsync(db => DiscountsOf(db).SaveAsync(product.Id, 20m, start, end, Ct)));
        DiscountBoundarySource source = new();
        DateTimeOffset opens = TurkeyTime.StartOf(start);
        DateTimeOffset closes = TurkeyTime.EndOfRange(end);

        Assert.Empty(await FindDueAsync(source, opens.AddTicks(-1)));
        Assert.Equal(new[] { new DueRecord(product.Id, opens) }, await FindDueAsync(source, opens));

        // The scheduler comes six hours late: the row carries 00.00 of the start day.
        _time.Now = opens.AddHours(6);
        await ProcessAsync(source, product.Id);
        Assert.Equal((opens, 80m), (await HistoryAsync(v[0].Id))[^1]);
        Assert.Empty(await FindDueAsync(source, _time.Now));

        // 23.59 of the end day is still inside the range.
        _time.Now = new DateTimeOffset(end.Year, end.Month, end.Day, 23, 59, 0, TurkeyTime.Offset).ToUniversalTime();
        Assert.Equal(80m, Assert.IsType<DiscountLine>((await LabelAsync(v[0].Id)).Discount).DiscountedPrice);
        Assert.Empty(await FindDueAsync(source, _time.Now));

        _time.Now = closes;
        Assert.Equal(new[] { new DueRecord(product.Id, closes) }, await FindDueAsync(source, closes));
        await ProcessAsync(source, product.Id);
        Assert.Equal((closes, 100m), (await HistoryAsync(v[0].Id))[^1]);
        PriceLabel after = await LabelAsync(v[0].Id);
        Assert.Null(after.Discount);
        Assert.Equal(closes, after.PriceAppliesSince);
        Assert.Empty(await FindDueAsync(source, closes.AddDays(1)));
    }

    // ---- KK4: the dates on the product page and on the card ----

    [Fact]
    public async Task TheRunningDiscountsDates_AreOnTheLabelAndOnTheCard_TheCardDeclaringTheLowestPricedVariant()
    {
        (Product product, Variant[] v) = await SeedAsync(ProductType.Physical, null, 70m, 50m);
        Assert.Same(DiscountSaveResult.Saved, await RunAsync(db => DiscountsOf(db).SaveAsync(product.Id, 20m, Today, Today.AddDays(4), Ct)));

        PriceLabel label = await LabelAsync(v[0].Id);
        Assert.Equal(new DiscountLine(56m, 70m, 20m, Today, Today.AddDays(4)), label.Discount);
        Assert.Equal(70m, label.Price);

        ProductListItem card = Assert.Single(Assert.IsType<ProductListPage>(await ReadAsync(db => new ProductListReader(db, _time).SearchAsync(product.Name, null, 1, Ct))).Items);
        Assert.Equal(50m, card.LowestPrice);
        Assert.Equal(new DiscountLine(40m, 50m, 20m, Today, Today.AddDays(4)), card.Discount);

        // Before the start nothing is declared.
        _time.Now = TurkeyTime.StartOf(Today).AddTicks(-10);
        Assert.Null((await LabelAsync(v[0].Id)).Discount);
        Assert.Null(Assert.Single(Assert.IsType<ProductListPage>(await ReadAsync(db => new ProductListReader(db, _time).SearchAsync(product.Name, null, 1, Ct))).Items).Discount);
    }

    [Fact]
    public async Task TheUnitPrice_IsOfTheDiscountedPrice()
    {
        (Product product, Variant[] v) = await SeedAsync(ProductType.Physical, null, 45m);
        await ExecuteAsync($"UPDATE products SET measurement_unit = 'Gram' WHERE id = '{product.Id}'; UPDATE variants SET net_quantity = 250 WHERE id = '{v[0].Id}'");
        Assert.Same(DiscountSaveResult.Saved, await RunAsync(db => DiscountsOf(db).SaveAsync(product.Id, 20m, Today, Today, Ct)));

        // 45 x 0,80 = 36,00; 36 x 1000 / 250 = 144,00 per kilogram.
        Assert.Equal(new UnitPriceLine(MeasurementUnit.Kilogram, 144m), (await LabelAsync(v[0].Id)).UnitPrice);
    }

    [Fact]
    public async Task TheCardsDiscounts_AreReadInAFixedNumberOfQueries_WhateverThePageHolds()
    {
        string word = "Sayaç kampanya";
        await SeedDiscountedAsync(word + " 00");
        int one = await CountCommandsAsync(db => new ProductListReader(db, _time).SearchAsync(word, null, 1, Ct));

        for (int i = 1; i <= PageSizes.StorefrontProducts + 2; i++)
        {
            await SeedDiscountedAsync($"{word} {i:00}");
        }

        int full = await CountCommandsAsync(db => new ProductListReader(db, _time).SearchAsync(word, null, 1, Ct));
        Assert.Equal(one, full);
    }

    // ---- KK5: the reference cannot be entered ----

    [Fact]
    public async Task TheReference_HasNoColumn_AndTheSaveTakesNoReference()
    {
        Assert.Empty(await QueryAsync(
            "SELECT table_name || '.' || column_name FROM information_schema.columns WHERE table_name IN ('products', 'variants', 'discounts', 'price_histories') AND column_name LIKE '%reference%'"));
        Assert.Equal(
            SaveDiscountParameters,
            typeof(ICatalogModule).GetMethod(nameof(ICatalogModule.SaveDiscountAsync))!.GetParameters().Select(p => p.Name ?? string.Empty));
    }

    // ---- KK6: the window's lowest, an off-sale stretch, an earlier campaign, the product's rule ----

    [Fact]
    public async Task OnGoods_APriceSetWhileOffSale_IsInTheWindowAndBecomesTheReference()
    {
        (Product product, Variant[] v) = await SeedAsync(ProductType.Physical, null, 100m);

        // Eight days before the start the variant is taken off sale and its price is lowered; five days before it is back on sale at 100.
        _time.Now = Now.AddDays(-8);
        Assert.Same(VariantPriceResult.Changed, await RunAsync(async db =>
        {
            (await db.Variants.SingleAsync(x => x.Id == v[0].Id, Ct)).PublishStatus = PublishStatus.Draft;
            return await PricesOf(db).ChangeAsync(v[0].Id, 70m, Ct);
        }));
        _time.Now = Now.AddDays(-5);
        Assert.Same(VariantPriceResult.Changed, await RunAsync(async db =>
        {
            (await db.Variants.SingleAsync(x => x.Id == v[0].Id, Ct)).PublishStatus = PublishStatus.Published;
            return await PricesOf(db).ChangeAsync(v[0].Id, 100m, Ct);
        }));
        _time.Now = Now;

        DiscountSaveResult refused = await RunAsync(db => DiscountsOf(db).SaveAsync(product.Id, 25m, Today, Today, Ct));
        Assert.Equal(("NotBelowReference", v[0].Id, 75m, 70m), (refused.Reason, refused.VariantId, refused.DiscountedPrice, refused.ReferencePrice));
        Assert.Same(DiscountSaveResult.Saved, await RunAsync(db => DiscountsOf(db).SaveAsync(product.Id, 31m, Today, Today, Ct)));
    }

    [Fact]
    public async Task AnEarlierCampaignsDiscountedPrice_LowersTheReference()
    {
        (Product product, Variant[] v) = await SeedAsync(ProductType.Digital, null, 100m);
        await AddRowAsync(v[0].Id, 80m, Now.AddDays(-8));
        await AddRowAsync(v[0].Id, 100m, Now.AddDays(-4));

        DiscountSaveResult refused = await RunAsync(db => DiscountsOf(db).SaveAsync(product.Id, 15m, Today, Today, Ct));
        Assert.Equal(("NotBelowReference", 85m, 80m), (refused.Reason, refused.DiscountedPrice, refused.ReferencePrice));
        Assert.Same(DiscountSaveResult.Saved, await RunAsync(db => DiscountsOf(db).SaveAsync(product.Id, 21m, Today, Today, Ct)));
    }

    [Theory]
    [InlineData(ProductType.Physical, WithdrawalExemptionReason.ExpiryDated, "80")]
    [InlineData(ProductType.Physical, null, "80")]
    [InlineData(ProductType.Physical, WithdrawalExemptionReason.Perishable, "100")]
    [InlineData(ProductType.Service, null, "100")]
    public async Task TheReference_FollowsTheProductsRule(ProductType type, WithdrawalExemptionReason? reason, string expected)
    {
        (Product product, Variant[] v) = await SeedAsync(type, reason, 100m);
        await AddRowAsync(v[0].Id, 80m, Now.AddDays(-8));
        await AddRowAsync(v[0].Id, 100m, Now.AddDays(-4));
        decimal reference = decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture);

        // 15 % gives 85: below the price right before (100), not below the window's lowest (80).
        DiscountSaveResult result = await RunAsync(db => DiscountsOf(db).SaveAsync(product.Id, 15m, Today.AddDays(1), Today.AddDays(1), Ct));
        Assert.Equal(reference == 100m ? null : "NotBelowReference", result.Reason);
        if (!result.IsRejected)
        {
            DiscountView view = Assert.IsType<DiscountView>(await ReadAsync(db => DiscountsOf(db).GetAsync(product.Id, Ct)));
            Assert.Equal(reference, Assert.Single(view.Variants).ReferencePrice);
            Assert.Equal(DiscountState.NotStarted, view.State);
        }
        else
        {
            Assert.Equal(reference, result.ReferencePrice);
        }
    }

    // ---- KK7: a new variant's reference comes from its own history ----

    [Fact]
    public async Task ANewVariant_TakesItsReferenceFromItsOwnHistory_NotFromItsSiblings()
    {
        (Product product, Variant[] v) = await SeedAsync(ProductType.Physical, null, 50m, 100m);
        await ExecuteAsync($"UPDATE variants SET first_on_sale_at = '{Now.AddDays(-3):u}', created_at = '{Now.AddDays(-3):u}' WHERE id = '{v[1].Id}'");
        await ExecuteAsync($"DELETE FROM price_histories WHERE variant_id = '{v[1].Id}'");
        await AddRowAsync(v[1].Id, 100m, Now.AddDays(-3));

        Assert.Same(DiscountSaveResult.Saved, await RunAsync(db => DiscountsOf(db).SaveAsync(product.Id, 10m, Today, Today, Ct)));

        DiscountView view = Assert.IsType<DiscountView>(await ReadAsync(db => DiscountsOf(db).GetAsync(product.Id, Ct)));
        Assert.Equal(50m, view.Variants.Single(l => l.VariantId == v[0].Id).ReferencePrice);
        Assert.Equal(100m, view.Variants.Single(l => l.VariantId == v[1].Id).ReferencePrice);
    }

    // ---- KK8: a discount not below the reference is not saved; the earlier one stays ----

    [Fact]
    public async Task ARefusedDiscount_LeavesTheEarlierDiscountAndTheHistoryAsTheyWere()
    {
        (Product product, Variant[] v) = await SeedAsync(ProductType.Physical, null, 100m);
        Assert.Same(DiscountSaveResult.Saved, await RunAsync(db => DiscountsOf(db).SaveAsync(product.Id, 10m, Today.AddDays(5), Today.AddDays(6), Ct)));
        await AddRowAsync(v[0].Id, 80m, Now.AddDays(-8));
        await AddRowAsync(v[0].Id, 100m, Now.AddDays(-4));
        (Guid Id, decimal Percentage) before = await DiscountRowAsync(product.Id);
        long rows = await CountAsync("price_histories");

        DiscountSaveResult refused = await SaveEvenIfRefusedAsync(db => DiscountsOf(db).SaveAsync(product.Id, 15m, Today, Today, Ct));

        Assert.Equal("NotBelowReference", refused.Reason);
        Assert.Equal(before, await DiscountRowAsync(product.Id));
        Assert.Equal(rows, await CountAsync("price_histories"));
    }

    // ---- 06 4.12: a new definition replaces the row; the row is never updated ----

    [Fact]
    public async Task ANewDefinition_ReplacesTheRowOfTheSameProduct_AndAnUpdatedRowIsRefused()
    {
        (Product product, _) = await SeedAsync(ProductType.Physical, null, 100m);
        Assert.Same(DiscountSaveResult.Saved, await RunAsync(db => DiscountsOf(db).SaveAsync(product.Id, 10m, Today.AddDays(3), Today.AddDays(4), Ct)));
        (Guid first, _) = await DiscountRowAsync(product.Id);

        _time.Now = Now.AddHours(1);
        Assert.Same(DiscountSaveResult.Saved, await RunAsync(db => DiscountsOf(db).SaveAsync(product.Id, 20m, Today.AddDays(3), Today.AddDays(4), Ct)));

        (Guid second, decimal percentage) = await DiscountRowAsync(product.Id);
        Assert.NotEqual(first, second);
        Assert.Equal(20m, percentage);
        Assert.Equal(1L, await CountAsync("discounts"));
        await using (AppDbContext db = NewContext())
        {
            Assert.Equal(_time.Now, await db.Discounts.Where(d => d.Id == second).Select(d => d.CreatedAt).SingleAsync(Ct));
        }

        await using AppDbContext context = NewContext();
        (await context.Discounts.SingleAsync(d => d.ProductId == product.Id, Ct)).Percentage = 30m;
        InvalidOperationException refused = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));
        Assert.Contains("never updated", refused.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RemovingARunningDiscount_WritesTheOwnPrice_AndAnUnchangedAppliedPriceWritesNothing()
    {
        (Product product, Variant[] v) = await SeedAsync(ProductType.Physical, null, 100m);
        Assert.Same(DiscountSaveResult.Saved, await RunAsync(db => DiscountsOf(db).SaveAsync(product.Id, 20m, Today, Today.AddDays(3), Ct)));
        long rows = await CountAsync("price_histories");

        // The same rate with another end date applies the same price: no row. The same price: no row either.
        Assert.Same(DiscountSaveResult.Saved, await RunAsync(db => DiscountsOf(db).SaveAsync(product.Id, 20m, Today, Today.AddDays(5), Ct)));
        Assert.Same(VariantPriceResult.Unchanged, await RunAsync(db => PricesOf(db).ChangeAsync(v[0].Id, 100m, Ct)));
        Assert.Equal(rows, await CountAsync("price_histories"));

        _time.Now = Now.AddHours(2);
        Assert.Same(DiscountRemoveResult.Removed, await RunAsync(db => DiscountsOf(db).RemoveAsync(product.Id, Ct)));
        Assert.Equal((Now.AddHours(2), 100m), (await HistoryAsync(v[0].Id))[^1]);
        Assert.Null(await ReadAsync(db => DiscountsOf(db).GetAsync(product.Id, Ct)));
        Assert.Same(DiscountRemoveResult.NotFound, await RunAsync(db => DiscountsOf(db).RemoveAsync(product.Id, Ct)));
    }

    [Fact]
    public async Task APriceChangeDuringTheDiscount_WritesTheNewDiscountedPrice_AndTheStartItOwedFirst()
    {
        (Product product, Variant[] v) = await SeedAsync(ProductType.Physical, null, 100m);
        Assert.Same(DiscountSaveResult.Saved, await RunAsync(db => DiscountsOf(db).SaveAsync(product.Id, 20m, Today.AddDays(1), Today.AddDays(2), Ct)));

        // The scheduler has not written the start yet; the price change writes it first, at its own moment, then the new applied price.
        DateTimeOffset opens = TurkeyTime.StartOf(Today.AddDays(1));
        _time.Now = opens.AddHours(3);
        Assert.Same(VariantPriceResult.Changed, await RunAsync(db => PricesOf(db).ChangeAsync(v[0].Id, 150m, Ct)));

        Assert.Equal(new[] { (opens, 80m), (opens.AddHours(3), 120m) }, (await HistoryAsync(v[0].Id))[^2..]);
    }

    [Fact]
    public async Task TheWriters_JoinTheCallersTransaction_AndThrowOutsideOne()
    {
        (Product product, Variant[] v) = await SeedAsync(ProductType.Physical, null, 100m);
        await using AppDbContext db = NewContext();

        await Assert.ThrowsAsync<InvalidOperationException>(() => DiscountsOf(db).SaveAsync(product.Id, 10m, Today, Today, Ct));
        await Assert.ThrowsAsync<InvalidOperationException>(() => DiscountsOf(db).RemoveAsync(product.Id, Ct));
        await Assert.ThrowsAsync<InvalidOperationException>(() => PricesOf(db).ChangeAsync(v[0].Id, 90m, Ct));
    }

    // ---- helpers ----

    private Discounts DiscountsOf(AppDbContext db) => new(db, _time, new PriceHistoryRecorder(db, _time));

    private VariantPrices PricesOf(AppDbContext db) => new(db, _time, new PriceHistoryRecorder(db, _time));

    // A product with one variant per price, every variant published and on sale since long ago, each with its first history row.
    private async Task<(Product Product, Variant[] Variants)> SeedAsync(ProductType type, WithdrawalExemptionReason? reason, params decimal[] prices)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create("Ürün " + n, "urun-" + n, type, 20m, LongAgo);
        if (type == ProductType.Physical)
        {
            product.WithSeededOrigin();
        }
        else if (type == ProductType.Service)
        {
            product.FulfilmentPeriod = 5;
        }

        product.PublishStatus = PublishStatus.Published;
        product.WithdrawalExemptionReason = reason;
        Option option = Option.Create(product.Id, "Boy", 1, LongAgo);
        List<OptionValue> values = [];
        List<Variant> variants = [];
        for (int i = 0; i < prices.Length; i++)
        {
            OptionValue value = OptionValue.Create(option.Id, "Değer " + i, i + 1, LongAgo);
            Variant variant = Variant.Create(product.Id, $"IND-{n}-{i}", prices[i], LongAgo, type == ProductType.Physical ? 100 : null);
            variant.FirstOptionValueId = value.Id;
            variant.PublishStatus = PublishStatus.Published;
            variant.FirstOnSaleAt = LongAgo;
            values.Add(value);
            variants.Add(variant);
        }

        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Options.Add(option);
            c.OptionValues.AddRange(values);
            c.Variants.AddRange(variants);
            c.PriceHistories.AddRange(variants.Select(v => PriceHistory.Create(v.Id, v.Price, LongAgo, LongAgo)));
        });
        return (product, [.. variants]);
    }

    private async Task SeedDiscountedAsync(string name)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create(name, "sayac-" + n, ProductType.Digital, 20m, LongAgo.AddMinutes(n));
        product.PublishStatus = PublishStatus.Published;
        Option option = Option.Create(product.Id, "Boy", 1, LongAgo);
        OptionValue one = OptionValue.Create(option.Id, "Bir", 1, LongAgo);
        OptionValue two = OptionValue.Create(option.Id, "İki", 2, LongAgo);
        Variant a = Variant.Create(product.Id, $"SAYAC-{n}-A", 100m, LongAgo);
        Variant b = Variant.Create(product.Id, $"SAYAC-{n}-B", 120m, LongAgo);
        (a.FirstOptionValueId, b.FirstOptionValueId) = (one.Id, two.Id);
        foreach (Variant variant in (Variant[])[a, b])
        {
            variant.PublishStatus = PublishStatus.Published;
            variant.FirstOnSaleAt = LongAgo;
        }

        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Options.Add(option);
            c.OptionValues.AddRange(one, two);
            c.Variants.AddRange(a, b);
            c.PriceHistories.AddRange(PriceHistory.Create(a.Id, 100m, LongAgo, LongAgo), PriceHistory.Create(b.Id, 120m, LongAgo, LongAgo));
            c.Discounts.Add(Discount.Create(product.Id, 10m, Today, Today.AddDays(1), LongAgo));
        });
    }

    private async Task<T> RunAsync<T>(Func<AppDbContext, Task<T>> work)
        where T : IOperationResult
    {
        await using AppDbContext db = NewContext();
        await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync(Ct);
        T result = await work(db);
        if (result.IsRejected)
        {
            await transaction.RollbackAsync(Ct);
            return result;
        }

        await db.SaveChangesAsync(Ct);
        await transaction.CommitAsync(Ct);
        return result;
    }

    // The caller saves and commits whatever the result: a refusal must have left nothing in the unit of work.
    private async Task<T> SaveEvenIfRefusedAsync<T>(Func<AppDbContext, Task<T>> work)
    {
        await using AppDbContext db = NewContext();
        await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync(Ct);
        T result = await work(db);
        await db.SaveChangesAsync(Ct);
        await transaction.CommitAsync(Ct);
        return result;
    }

    private async Task<T> ReadAsync<T>(Func<AppDbContext, Task<T>> read)
    {
        await using AppDbContext db = NewContext();
        return await read(db);
    }

    private async Task<PriceLabel> LabelAsync(Guid variantId)
    {
        return Assert.IsType<PriceLabel>(await ReadAsync(db => new PriceLabelReader(db, _time).GetAsync(variantId, Ct)));
    }

    private async Task<List<(DateTimeOffset StartedAt, decimal Price)>> HistoryAsync(Guid variantId)
    {
        await using AppDbContext db = NewContext();
        return [.. (await db.PriceHistories.Where(h => h.VariantId == variantId).OrderBy(h => h.StartedAt).ThenBy(h => h.CreatedAt).Select(h => new { h.StartedAt, h.Price }).ToListAsync(Ct))
            .Select(h => (h.StartedAt, h.Price))];
    }

    private async Task<(Guid Id, decimal Percentage)> DiscountRowAsync(Guid productId)
    {
        await using AppDbContext db = NewContext();
        var row = await db.Discounts.Where(d => d.ProductId == productId).Select(d => new { d.Id, d.Percentage }).SingleAsync(Ct);
        return (row.Id, row.Percentage);
    }

    private Task AddRowAsync(Guid variantId, decimal price, DateTimeOffset startedAt)
        => SaveAsync(c => c.PriceHistories.Add(PriceHistory.Create(variantId, price, startedAt, startedAt)));

    private async Task<IReadOnlyList<DueRecord>> FindDueAsync(DiscountBoundarySource source, DateTimeOffset now)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await source.FindDueAsync(scope.ServiceProvider, now, Ct);
    }

    private async Task ProcessAsync(DiscountBoundarySource source, Guid productId)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        await source.ProcessAsync(scope.ServiceProvider, productId, Ct);
    }

    private ServiceProvider BuildProvider()
    {
        ServiceCollection services = new();
        services.AddSingleton<TimeProvider>(_time);
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddAppCache();
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddScoped<PriceHistoryRecorder>();
        return services.BuildServiceProvider();
    }

    private async Task<int> CountCommandsAsync(Func<AppDbContext, Task<ProductListPage?>> read)
    {
        CommandCounter counter = new();
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn).AddInterceptors(counter);
        await using AppDbContext context = new(builder.Options);
        Assert.NotNull(await read(context));
        return counter.Count;
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

    private async Task<long> CountAsync(string table)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new($"SELECT count(*) FROM {table}", connection);
        return (long)(await command.ExecuteScalarAsync(Ct))!;
    }

    private async Task<List<string>> QueryAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(Ct);
        List<string> rows = [];
        while (await reader.ReadAsync(Ct))
        {
            rows.Add(reader.GetString(0));
        }

        return rows;
    }

    private sealed class MovableTime(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;

        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class CommandCounter : Microsoft.EntityFrameworkCore.Diagnostics.DbCommandInterceptor
    {
        private int _count;

        public int Count => Volatile.Read(ref _count);

        public override ValueTask<Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<System.Data.Common.DbDataReader>> ReaderExecutingAsync(
            System.Data.Common.DbCommand command,
            Microsoft.EntityFrameworkCore.Diagnostics.CommandEventData eventData,
            Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<System.Data.Common.DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _count);
            return ValueTask.FromResult(result);
        }
    }
}
