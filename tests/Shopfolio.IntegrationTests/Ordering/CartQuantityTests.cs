using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The cart's quantity rules on a real PostgreSQL (T347 KK1-KK6; 02 3.16.8-3.16.12): the quantity that the stock less the reserved units does not hold is refused without a number, the sign-in merge
/// and the quantity increase go through the one rule and the quantity is never lowered, the service capacity follows the stock, the firm's per-order limit covers all variants of a product and wins
/// over the stock, a limit lowered later keeps the lines out of the order, the cart has no line ceiling, and the paid order takes only its own lines out of the cart.
/// </summary>
[Collection(CartTestsDefinition.Name)]
public sealed class CartQuantityTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 8, 9, 0, 0, TimeSpan.Zero);
    private static readonly ShippingFeeSection NoShippingTerms = new(0m, null, null, 1);

    private readonly CollectionDatabaseFixture _collection;

    private int _counter;

    public CartQuantityTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE dispatch_records, jobs, refund_records, refunds, payment_records, cancellations, internal_notes, transition_records, cart_items, carts, "
            + "order_items, orders, variants, products, customers, legal_text_versions, provinces CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: more than the stock is not added; the reserved units are taken off; no number is told ----

    [Fact]
    public async Task AQuantityAboveTheStock_IsRefused_TheCartIsNotChanged_AndNoCartIsLeftBehind_KK1()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Masa", 1, stock: 5);
        string key = NewKey();

        CartAddResult first = await AddAsync(CartOwner.ForGuest(key), variants[0].Id, 6);

        Assert.Equal(CartAddOutcome.NoStock, first.Outcome);
        Assert.Null(first.CartId);
        await using (AppDbContext reader = NewContext())
        {
            Assert.Equal(0, await reader.Carts.CountAsync(Ct));
        }

        Assert.Equal(CartAddOutcome.Added, (await AddAsync(CartOwner.ForGuest(key), variants[0].Id, 5)).Outcome);
        Assert.Equal(CartAddOutcome.NoStock, (await AddAsync(CartOwner.ForGuest(key), variants[0].Id, 1)).Outcome);
        Assert.Equal(5, Assert.Single((await OpenAsync(CartOwner.ForGuest(key))).Lines).Quantity);
    }

    [Fact]
    public async Task TheReservedUnits_AreTakenOffTheStock_ThatTheAddIsJudgedBy_KK1()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Masa", 1, stock: 5);
        await ExecuteAsync($"UPDATE variants SET reserved_quantity = 3 WHERE id = '{variants[0].Id}'");
        CartOwner owner = CartOwner.ForGuest(NewKey());

        CartAddResult over = await AddAsync(owner, variants[0].Id, 3);
        CartAddResult within = await AddAsync(owner, variants[0].Id, 2);

        Assert.Equal(CartAddOutcome.NoStock, over.Outcome);
        Assert.Equal(CartAddOutcome.Added, within.Outcome);
        Assert.Equal(2, Assert.Single((await OpenAsync(owner)).Lines).Quantity);
    }

    // ---- KK2: the merge and the increase go through the same rule; the quantity is never lowered ----

    [Fact]
    public async Task TheMerge_KeepsALineAboveTheStock_WithTheSameMessage_AndTheQuantityIsNotLowered_KK2()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Masa", 2, stock: 5);
        Guid customerId = await SeedCustomerAsync();
        string key = NewKey();
        await SeedCartAsync(null, key, (variants[0].Id, 5, 100m), (variants[1].Id, 2, 100m));
        await SeedCartAsync(customerId, null, (variants[0].Id, 3, 100m));

        // The guest holds 5 of the first variant; the stock then falls under that quantity before the sign-in.
        await ExecuteAsync($"UPDATE variants SET stock_quantity = 4 WHERE id = '{variants[0].Id}'");
        CartMergeResult merge = await MergeAsync(customerId, key);
        CartView view = await OpenAsync(CartOwner.ForMember(customerId));

        Assert.True(merge.WasMerged);
        CartLine over = view.Lines.Single(l => l.VariantId == variants[0].Id);
        Assert.Equal(5, over.Quantity);
        Assert.Equal(CartQuantityProblem.NoStock, over.QuantityProblem);
        Assert.False(over.EntersOrder);
        Assert.Equal(2, view.Lines.Single(l => l.VariantId == variants[1].Id).Quantity);
        Assert.True(view.CanOpenPayment);
        Assert.Equal(200m, view.OrderableSubtotal);
    }

    [Fact]
    public async Task AQuantityIncrease_GoesThroughTheSameRule_AndAfterTheStockFell_TheLineStaysUntilTheQuantityIsLowered_KK2()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Masa", 1, stock: 10);
        Guid customerId = await SeedCustomerAsync();
        CartOwner owner = CartOwner.ForMember(customerId);
        await AddAsync(owner, variants[0].Id, 4);
        await ExecuteAsync($"UPDATE variants SET stock_quantity = 3 WHERE id = '{variants[0].Id}'");

        CartView view = await OpenAsync(owner);
        CartAddResult more = await AddAsync(owner, variants[0].Id, 1);

        CartLine line = Assert.Single(view.Lines);
        Assert.Equal((4, CartQuantityProblem.NoStock, false), (line.Quantity, line.QuantityProblem, line.EntersOrder));
        Assert.Equal(CartAddOutcome.NoStock, more.Outcome);
        Assert.False(view.CanOpenPayment);

        // The stock returns: the same line, with the same quantity, enters the order again.
        await ExecuteAsync($"UPDATE variants SET stock_quantity = 4 WHERE id = '{variants[0].Id}'");
        CartLine back = Assert.Single((await OpenAsync(owner)).Lines);
        Assert.Equal((4, CartQuantityProblem.None, true), (back.Quantity, back.QuantityProblem, back.EntersOrder));
    }

    [Fact]
    public async Task TheServiceCapacity_FollowsTheSameRule_KK2()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Danışmanlık", 1, stock: 2, type: ProductType.Service);
        CartOwner owner = CartOwner.ForGuest(NewKey());

        CartAddResult over = await AddAsync(owner, variants[0].Id, 3);
        await AddAsync(owner, variants[0].Id, 2);
        await ExecuteAsync($"UPDATE variants SET stock_quantity = 1 WHERE id = '{variants[0].Id}'");
        CartLine line = Assert.Single((await OpenAsync(owner)).Lines);

        Assert.Equal(CartAddOutcome.NoStock, over.Outcome);
        Assert.Equal((2, CartQuantityProblem.NoStock, false), (line.Quantity, line.QuantityProblem, line.EntersOrder));
    }

    [Fact]
    public async Task AServiceWithoutCapacity_IsNeverHeldBack_KK2()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Danışmanlık", 1, stock: null, type: ProductType.Service);

        CartAddResult result = await AddAsync(CartOwner.ForGuest(NewKey()), variants[0].Id, 500);

        Assert.Equal(CartAddOutcome.Added, result.Outcome);
    }

    // ---- KK3: the per-order limit covers all variants together and wins over the stock ----

    [Fact]
    public async Task TheLimit_CoversTheSumOfAllVariants_AndIsToldAsANumber_KK3()
    {
        (Product product, List<Variant> variants) = await SeedProductAsync("Lamba", 2, stock: 10);
        await SetLimitAsync(product.Id, 2);
        CartOwner owner = CartOwner.ForGuest(NewKey());

        Assert.Equal(CartAddOutcome.Added, (await AddAsync(owner, variants[0].Id, 1)).Outcome);
        Assert.Equal(CartAddOutcome.Added, (await AddAsync(owner, variants[1].Id, 1)).Outcome);
        CartAddResult third = await AddAsync(owner, variants[0].Id, 1);

        Assert.Equal(CartAddOutcome.OverOrderLimit, third.Outcome);
        Assert.Equal(2, third.OrderLimit);
        CartView view = await OpenAsync(owner);
        Assert.Equal([1, 1], [.. view.Lines.Select(l => l.Quantity)]);
        Assert.All(view.Lines, l => Assert.True(l.EntersOrder));
    }

    [Fact]
    public async Task WhenTheLimitAndTheStockAreBothExceeded_OnlyTheLimitIsTold_KK3()
    {
        (Product product, List<Variant> variants) = await SeedProductAsync("Lamba", 1, stock: 3);
        await SetLimitAsync(product.Id, 2);

        CartAddResult result = await AddAsync(CartOwner.ForGuest(NewKey()), variants[0].Id, 9);

        Assert.Equal(CartAddOutcome.OverOrderLimit, result.Outcome);
        Assert.Equal(2, result.OrderLimit);
    }

    [Fact]
    public async Task WithAnEmptyLimit_TheOnlyLimitIsTheStock_AndALimitOfAnotherProductDoesNotCount_KK3()
    {
        (_, List<Variant> free) = await SeedProductAsync("Masa", 1, stock: 4);
        (Product limited, List<Variant> other) = await SeedProductAsync("Lamba", 1, stock: 4);
        await SetLimitAsync(limited.Id, 1);
        CartOwner owner = CartOwner.ForGuest(NewKey());

        Assert.Equal(CartAddOutcome.Added, (await AddAsync(owner, free[0].Id, 4)).Outcome);
        Assert.Equal(CartAddOutcome.Added, (await AddAsync(owner, other[0].Id, 1)).Outcome);
        Assert.Equal(CartAddOutcome.NoStock, (await AddAsync(owner, free[0].Id, 1)).Outcome);
    }

    [Fact]
    public async Task TheSignInMerge_IsJudgedByTheSameLimit_AndKeepsTheLines_KK3()
    {
        (Product product, List<Variant> variants) = await SeedProductAsync("Lamba", 2, stock: 10);
        await SetLimitAsync(product.Id, 2);
        Guid customerId = await SeedCustomerAsync();
        string key = NewKey();
        await SeedCartAsync(null, key, (variants[0].Id, 2, 100m));
        await SeedCartAsync(customerId, null, (variants[1].Id, 1, 100m));

        await MergeAsync(customerId, key);
        CartView view = await OpenAsync(CartOwner.ForMember(customerId));

        Assert.Equal([2, 1], view.Lines.Select(l => l.Quantity).OrderByDescending(q => q));
        Assert.All(view.Lines, l => Assert.Equal((CartQuantityProblem.OverOrderLimit, 2, false), (l.QuantityProblem, l.OrderLimit, l.EntersOrder)));
    }

    // ---- KK4: a limit lowered later keeps the lines in the cart and out of the order ----

    [Fact]
    public async Task ALoweredLimit_KeepsTheLinesInTheCart_OutOfTheOrder_AndOutOfTheAmounts_KK4()
    {
        (Product product, List<Variant> variants) = await SeedProductAsync("Lamba", 2, stock: 10);
        (_, List<Variant> table) = await SeedProductAsync("Masa", 1, stock: 10);
        Guid customerId = await SeedCustomerAsync();
        await SeedCartAsync(customerId, null, (variants[0].Id, 2, 100m), (variants[1].Id, 1, 100m), (table[0].Id, 1, 100m));

        Assert.All((await OpenAsync(CartOwner.ForMember(customerId))).Lines, l => Assert.True(l.EntersOrder));
        await SetLimitAsync(product.Id, 2);
        CartView view = await OpenAsync(CartOwner.ForMember(customerId));

        Assert.Equal(3, view.Lines.Count);
        Assert.Equal([false, false, true], [.. view.Lines.OrderBy(l => l.ProductName == "Masa").ThenBy(l => l.VariantId).Select(l => l.EntersOrder)]);
        Assert.Equal(CartQuantityProblem.None, view.Lines.Single(l => l.VariantId == table[0].Id).QuantityProblem);
        Assert.Equal(100m, view.OrderableSubtotal);

        // The quantity of one line goes down by the customer's hand; the total is within the limit and both enter again.
        await ExecuteAsync($"UPDATE cart_items SET quantity = 1 WHERE variant_id = '{variants[0].Id}'");
        Assert.All((await OpenAsync(CartOwner.ForMember(customerId))).Lines, l => Assert.True(l.EntersOrder));
    }

    // ---- KK5: no ceiling on the number of lines ----

    [Fact]
    public async Task TheFiftyFirstLine_IsAdded_TheCartHasNoCeiling_KK5()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Masa", 51, stock: 10);
        Guid customerId = await SeedCustomerAsync();
        await SeedCartAsync(customerId, null, [.. variants.Take(50).Select(v => (v.Id, 1, 100m))]);

        CartAddResult added = await AddAsync(CartOwner.ForMember(customerId), variants[50].Id, 1);
        CartView view = await OpenAsync(CartOwner.ForMember(customerId));

        Assert.Equal(CartAddOutcome.Added, added.Outcome);
        Assert.Equal(51, view.Lines.Count);
        Assert.All(view.Lines, l => Assert.True(l.EntersOrder));
        Assert.Equal(5100m, view.OrderableSubtotal);
    }

    // ---- KK6: the paid order takes only its own lines out of the cart ----

    [Fact]
    public async Task APaidCardOrder_TakesTheOrdersVariantOutWhole_IncludingWhatWasAddedWhilePaying_AndLeavesTheOtherProduct_KK6()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Masa", 2, stock: 10);
        Guid customerId = await SeedCustomerAsync();
        Guid cartId = await SeedCartAsync(customerId, null, (variants[0].Id, 1, 100m));
        Guid orderId = await SeedPendingOrderAsync(cartId, variants[0].Id);

        // While the payment is pending, one more of the same variant and another product are added.
        await AddAsync(CartOwner.ForMember(customerId), variants[0].Id, 2);
        await AddAsync(CartOwner.ForMember(customerId), variants[1].Id, 2);
        PaymentTransitionResult result = await PayAsync(orderId);

        Assert.False(result.IsRejected);
        CartView view = await OpenAsync(CartOwner.ForMember(customerId));
        Assert.Equal([(variants[1].Id, 2)], view.Lines.Select(l => (l.VariantId, l.Quantity)));
    }

    [Fact]
    public async Task APaidOrder_OfAGuestCartThatMergedAtSignIn_TakesNothingOutOfTheAccountsCart_KK6()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Masa", 1, stock: 10);
        Guid customerId = await SeedCustomerAsync();
        string key = NewKey();
        Guid guestCartId = await SeedCartAsync(null, key, (variants[0].Id, 1, 100m));
        Guid orderId = await SeedPendingOrderAsync(guestCartId, variants[0].Id);
        await SeedCartAsync(customerId, null, (variants[0].Id, 3, 100m));

        await MergeAsync(customerId, key);
        PaymentTransitionResult result = await PayAsync(orderId);

        Assert.False(result.IsRejected);
        CartLine line = Assert.Single((await OpenAsync(CartOwner.ForMember(customerId))).Lines);
        Assert.Equal((variants[0].Id, 3), (line.VariantId, line.Quantity));
    }

    [Fact]
    public async Task APaymentThatIsAlreadyApplied_TakesNothingOut_KK6()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Masa", 1, stock: 10);
        Guid customerId = await SeedCustomerAsync();
        Guid cartId = await SeedCartAsync(customerId, null, (variants[0].Id, 1, 100m));
        Guid orderId = await SeedPendingOrderAsync(cartId, variants[0].Id);
        await PayAsync(orderId);
        await AddAsync(CartOwner.ForMember(customerId), variants[0].Id, 2);

        PaymentTransitionResult again = await PayAsync(orderId);

        Assert.True(again.IsAlreadyApplied);
        Assert.Equal(2, Assert.Single((await OpenAsync(CartOwner.ForMember(customerId))).Lines).Quantity);
    }

    // ---- D-590 (T346 test gap): the told-once mark of an item taken off the showcase moves with the merge ----

    [Fact]
    public async Task TheMerge_CarriesTheTakenOffMark_AndTheMembersFirstOpenTellsTheNameOnce_D590()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Masa", 2, stock: 10);
        Guid customerId = await SeedCustomerAsync();
        string key = NewKey();
        await SeedCartAsync(null, key, (variants[0].Id, 1, 100m), (variants[1].Id, 1, 100m));
        await ExecuteAsync($"UPDATE cart_items SET removed_product_name = 'Eski Masa' WHERE variant_id = '{variants[0].Id}'");

        await MergeAsync(customerId, key);
        CartView first = await OpenAsync(CartOwner.ForMember(customerId));
        CartView second = await OpenAsync(CartOwner.ForMember(customerId));

        Assert.Equal(["Eski Masa"], [.. first.RemovedProductNames]);
        Assert.Empty(second.RemovedProductNames);
        Assert.Equal(variants[1].Id, Assert.Single(second.Lines).VariantId);
    }

    // ---- seeds and helpers ----

    private static string NewKey() => $"key-{Guid.NewGuid():N}";

    private async Task SetLimitAsync(Guid productId, int limit)
    {
        await ExecuteAsync($"UPDATE products SET max_quantity_per_order = {limit} WHERE id = '{productId}'");
    }

    private async Task<(Product Product, List<Variant> Variants)> SeedProductAsync(string name, int variantCount, int? stock, ProductType type = ProductType.Physical)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create(name, $"urun-{n:D4}", type, 20m, Now.AddMinutes(n));
        if (type == ProductType.Service)
        {
            // A service has no place of origin and is published with a fulfilment period (P-44).
            product.FulfilmentPeriod = 3;
        }
        else
        {
            product = product.WithSeededOrigin();
        }

        product.PublishStatus = PublishStatus.Published;
        List<Variant> variants = [];
        for (int i = 0; i < variantCount; i++)
        {
            Variant variant = Variant.Create(product.Id, $"SKU-{n:D4}-{i}", 100m, Now.AddMinutes(n));
            variant.PublishStatus = PublishStatus.Published;
            variant.StockQuantity = stock;
            variants.Add(variant);
        }

        Option? option = variantCount > 1 ? Option.Create(product.Id, "Boy", 1, Now) : null;
        List<OptionValue> values = [];
        if (option is not null)
        {
            for (int i = 0; i < variants.Count; i++)
            {
                OptionValue value = OptionValue.Create(option.Id, $"Değer {i}", i + 1, Now);
                variants[i].FirstOptionValueId = value.Id;
                values.Add(value);
            }
        }

        await SaveAsync(c =>
        {
            c.Products.Add(product);
            if (option is not null)
            {
                c.Options.Add(option);
                c.OptionValues.AddRange(values);
            }

            c.Variants.AddRange(variants);
        });
        return (product, variants);
    }

    private async Task<Guid> SeedCustomerAsync()
    {
        int n = Interlocked.Increment(ref _counter);
        Customer customer = Customer.Create($"Ali {n}", $"ali-{n}-{Guid.NewGuid():N}@example.com", "digest-placeholder", Now, null, Now);
        await SaveAsync(c => c.Customers.Add(customer));
        return customer.Id;
    }

    private async Task<Guid> SeedCartAsync(Guid? customerId, string? guestKey, params (Guid VariantId, int Quantity, decimal Price)[] items)
    {
        Cart cart = customerId is Guid id ? Cart.ForCustomer(id, Now) : Cart.ForGuest(Shopfolio.Infrastructure.Security.SecretKey.Hash(guestKey!), Now);
        await SaveAsync(c =>
        {
            c.Carts.Add(cart);
            foreach ((Guid variantId, int quantity, decimal price) in items)
            {
                c.CartItems.Add(CartItem.Create(cart.Id, variantId, quantity, price, Now));
            }
        });
        return cart.Id;
    }

    // An unpaid card order born from the cart, with one item of the variant; the states are set around the entity as the transition tests do.
    private async Task<Guid> SeedPendingOrderAsync(Guid? cartId, Guid variantId)
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n:D2}", $"İl {n}", Now);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Now, "metin", Now);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Now, "metin", Now);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Now, "metin", Now).WithHosting();
        Order order = Order.Create(
            $"2026-{n:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", PaymentMethod.Card, true, "Ali@Example.com", 120m, 0m, Now.AddHours(1), Now);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = cartId;
        order.BillingRecipientName = "Ali Veli";
        order.BillingProvinceId = province.Id;
        order.BillingDistrict = "Çankaya";
        order.BillingAddressLine = "Atatürk Bulvarı 1";
        order.CompanyType = CompanyType.Tradesman;
        order.CompanyLegalName = "Örnek Ticaret";
        order.CompanyTaxIdNumber = "1234567890";
        order.CompanyProvinceId = province.Id;
        order.CompanyDistrict = "Çankaya";
        order.CompanyAddressLine = "Kızılay 2";
        order.CompanyPhone = "3121234567";
        order.CompanyEmail = "firma@example.com";
        order.CompanyKepAddress = "firma@hs01.kep.tr";
        order.PreInformationFormVersionId = form.Id;
        order.DistanceSalesContractVersionId = contract.Id;
        order.PrivacyNoticeVersionId = notice.Id;
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
        Guid productId = await ProductOfAsync(variantId);
        OrderItem item = OrderItem.Create(order.Id, productId, variantId, ProductType.Physical, "Masa", 1, 100m, 20m, 16.67m, Now);
        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.LegalTextVersions.AddRange(form, contract, notice);
            c.Orders.Add(order);
            c.OrderItems.Add(item);
        });
        await ExecuteAsync($"UPDATE orders SET order_status = 'Placed', payment_status = 'Pending' WHERE id = '{order.Id}'");
        return order.Id;
    }

    private async Task<Guid> ProductOfAsync(Guid variantId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.Variants.AsNoTracking().Where(v => v.Id == variantId).Select(v => v.ProductId).SingleAsync(Ct);
    }

    // T349: the cart reads its shipping terms; no fee, no threshold and no minimum keep these quantity cases free of them.
    private Task<CartView> OpenAsync(CartOwner owner) => RunAsync((ordering, token) => ordering.OpenCartAsync(owner, NoShippingTerms, token));

    private Task<CartAddResult> AddAsync(CartOwner owner, Guid variantId, int quantity) => RunAsync((ordering, token) => ordering.AddToCartAsync(owner, variantId, quantity, token));

    private Task<CartMergeResult> MergeAsync(Guid customerId, string key) => RunAsync((ordering, token) => ordering.MergeGuestCartAsync(customerId, key, token));

    // The card provider's success for the order: the transition unit's one payment path, in its own transaction.
    private async Task<PaymentTransitionResult> PayAsync(Guid orderId, string transactionId = "trx-t347")
    {
        await using ServiceProvider provider = OrderingTestServices.Build(_collection.Dsn);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IOrderTransitions unit = scope.ServiceProvider.GetRequiredService<IOrderTransitions>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => unit.ApplyCardPaymentAsync(new CardPaymentSucceeded(orderId, transactionId), token), Ct);
    }

    // One request: its own scope and transaction; the owner of the transaction saves (09 6 item 3, 10.2 item 5).
    private async Task<T> RunAsync<T>(Func<IOrderingModule, CancellationToken, Task<T>> work)
    {
        await using ServiceProvider provider = OrderingTestServices.Build(_collection.Dsn);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IOrderingModule ordering = scope.ServiceProvider.GetRequiredService<IOrderingModule>();
        Boxed<T> boxed = await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(async token => new Boxed<T>(await work(ordering, token)), Ct);
        return boxed.Value;
    }

    private sealed record Boxed<T>(T Value) : IOperationResult
    {
        public bool IsRejected => false;
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
}
