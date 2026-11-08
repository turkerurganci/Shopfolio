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

/// <summary>The collection of the cart's tests (T346): one database copied from the template.</summary>
[CollectionDefinition(CartTestsDefinition.Name)]
public sealed class CartTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "CartTests";
}

/// <summary>
/// The cart on a real PostgreSQL (T346 KK1-KK7; 02 3.16.1-3.16.7): whose it is, that nothing empties it, the sign-in merge and what it tells, the live line and the reference price, the
/// sold-out line that stays out of the order, the item taken off the showcase told once, and the payment step that does not open without an orderable line. Two requests at once
/// (two merges, two adds) write the same member cart without an exception.
/// </summary>
[Collection(CartTestsDefinition.Name)]
public sealed class CartTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 8, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    private int _counter;

    public CartTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE cart_items, carts, order_items, orders, variants, products, customers, legal_text_versions, provinces CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the member's cart is the account's; the guest's is the browser's ----

    [Fact]
    public async Task AMembersCart_IsTheSameInEveryBrowser_AndTheGuestKeyIsEmptyAfterTheSignIn_KK1()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Masa", 2);
        Guid customerId = await SeedCustomerAsync();
        string guestKey = NewKey();

        await AddAsync(CartOwner.ForGuest(guestKey), variants[0].Id, 2);
        CartMergeResult merge = await MergeAsync(customerId, guestKey);

        Assert.True(merge.WasMerged);
        CartView first = await OpenAsync(CartOwner.ForMember(customerId));
        CartView second = await OpenAsync(CartOwner.ForMember(customerId));
        Assert.Equal(first.CartId, second.CartId);
        Assert.Equal([(variants[0].Id, 2)], second.Lines.Select(l => (l.VariantId, l.Quantity)));

        // The browser that was a guest now sees an empty cart, and the cart is not created by the read.
        CartView after = await OpenAsync(CartOwner.ForGuest(guestKey));
        Assert.Null(after.CartId);
        Assert.Empty(after.Lines);
        await using AppDbContext reader = NewContext();
        Assert.Equal(1, await reader.Carts.CountAsync(Ct));
    }

    [Fact]
    public async Task TwoGuestBrowsers_HaveTheirOwnCarts_AndAMemberAddsToTheAccountCart_KK1()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Masa", 1);
        Guid customerId = await SeedCustomerAsync();
        string keyA = NewKey();
        string keyB = NewKey();

        await AddAsync(CartOwner.ForGuest(keyA), variants[0].Id, 1);
        await AddAsync(CartOwner.ForMember(customerId), variants[0].Id, 3);

        Assert.Single((await OpenAsync(CartOwner.ForGuest(keyA))).Lines);
        Assert.Empty((await OpenAsync(CartOwner.ForGuest(keyB))).Lines);
        Assert.Equal(3, Assert.Single((await OpenAsync(CartOwner.ForMember(customerId))).Lines).Quantity);
    }

    // ---- KK2: the cart does not empty by itself ----

    [Fact]
    public async Task ACartThatWasLeftForTwoYears_AndAnOpenOrderOfTheCart_AreNotEmptied_KK2()
    {
        (Product product, List<Variant> variants) = await SeedProductAsync("Masa", 1);
        Guid customerId = await SeedCustomerAsync();
        Guid cartId = await SeedCartAsync(customerId, null, (variants[0].Id, 2, 100m));
        Guid guestCartId = await SeedCartAsync(null, "old-guest-" + NewKey(), (variants[0].Id, 1, 100m));
        await ExecuteAsync($"UPDATE carts SET last_accessed_at = '{Now.AddYears(-2):O}' WHERE id = '{guestCartId}'");
        await SeedOrderAsync(product.Id, variants[0].Id, cartId);

        CartView view = await OpenAsync(CartOwner.ForMember(customerId));

        Assert.Equal(2, Assert.Single(view.Lines).Quantity);
        await using AppDbContext reader = NewContext();
        Assert.Equal(1, await reader.CartItems.CountAsync(i => i.CartId == guestCartId, Ct));
        Assert.Equal(1, await reader.Orders.CountAsync(o => o.CartId == cartId, Ct));
    }

    [Fact]
    public async Task ASoldOutLine_AndALineOfAProductTakenOffTheShowcase_DoNotEmptyTheCartOnTheirOwn_KK2()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Masa", 2);
        Guid customerId = await SeedCustomerAsync();
        await SeedCartAsync(customerId, null, (variants[0].Id, 1, 100m), (variants[1].Id, 1, 100m));
        await ExecuteAsync($"UPDATE variants SET stock_quantity = 0 WHERE id = '{variants[0].Id}'");

        CartView view = await OpenAsync(CartOwner.ForMember(customerId));

        Assert.Equal(2, view.Lines.Count);
    }

    // ---- KK3: the larger quantity stays, whatever the way in ----

    [Fact]
    public async Task TheMerge_KeepsTheLargerQuantity_OfTheSameVariant_AndTheOtherVariantsStandApart_KK3()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Masa", 3);
        Guid customerId = await SeedCustomerAsync();
        string key = NewKey();
        await SeedCartAsync(customerId, null, (variants[0].Id, 5, 100m), (variants[1].Id, 1, 100m));
        await SeedCartAsync(null, key, (variants[0].Id, 2, 100m), (variants[1].Id, 4, 100m), (variants[2].Id, 1, 100m));

        await MergeAsync(customerId, key);

        CartView view = await OpenAsync(CartOwner.ForMember(customerId));
        Assert.Equal(
            new Dictionary<Guid, int> { [variants[0].Id] = 5, [variants[1].Id] = 4, [variants[2].Id] = 1 },
            view.Lines.ToDictionary(l => l.VariantId, l => l.Quantity));
        await using AppDbContext reader = NewContext();
        Assert.Equal(1, await reader.Carts.CountAsync(Ct));
    }

    [Fact]
    public async Task TheMerge_IsOneMethod_ForEveryWayIn_AndTheSecondCallWithTheSameKeyMergesNothing_KK3()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Masa", 1);
        Guid customerId = await SeedCustomerAsync();
        string key = NewKey();
        await SeedCartAsync(null, key, (variants[0].Id, 3, 100m));

        CartMergeResult first = await MergeAsync(customerId, key);
        CartMergeResult second = await MergeAsync(customerId, key);
        CartMergeResult unknown = await MergeAsync(customerId, NewKey());

        Assert.True(first.WasMerged);
        Assert.False(second.WasMerged);
        Assert.False(unknown.WasMerged);
        Assert.Equal(3, Assert.Single((await OpenAsync(CartOwner.ForMember(customerId))).Lines).Quantity);
    }

    [Fact]
    public async Task TwoMergesAtOnce_IntoTheSameMemberCart_BothLand_WithoutAnException_KK3()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Masa", 2);
        Guid customerId = await SeedCustomerAsync();
        string keyA = NewKey();
        string keyB = NewKey();
        await SeedCartAsync(null, keyA, (variants[0].Id, 2, 100m), (variants[1].Id, 1, 100m));
        await SeedCartAsync(null, keyB, (variants[0].Id, 6, 100m));

        // Two sign-ins of one account from two browsers, started together: each ends its own guest cart and writes the member side by conditional statements.
        await Task.WhenAll(MergeAsync(customerId, keyA), MergeAsync(customerId, keyB));

        CartView view = await OpenAsync(CartOwner.ForMember(customerId));
        Assert.Equal(
            new Dictionary<Guid, int> { [variants[0].Id] = 6, [variants[1].Id] = 1 },
            view.Lines.ToDictionary(l => l.VariantId, l => l.Quantity));
        await using AppDbContext reader = NewContext();
        Assert.Equal(1, await reader.Carts.CountAsync(Ct));
    }

    [Fact]
    public async Task TwoAddsAtOnce_OfTheSameVariant_ToTheSameMemberCart_AddUp_WithoutAnException_KK3()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Masa", 1);
        Guid customerId = await SeedCustomerAsync();

        await Task.WhenAll(
            AddAsync(CartOwner.ForMember(customerId), variants[0].Id, 2),
            AddAsync(CartOwner.ForMember(customerId), variants[0].Id, 3),
            AddAsync(CartOwner.ForMember(customerId), variants[0].Id, 1));

        CartView view = await OpenAsync(CartOwner.ForMember(customerId));
        Assert.Equal(6, Assert.Single(view.Lines).Quantity);
        await using AppDbContext reader = NewContext();
        Assert.Equal(1, await reader.Carts.CountAsync(Ct));
        Assert.Equal(1, await reader.CartItems.CountAsync(Ct));
    }

    // ---- KK4: what the customer did not see is told once ----

    [Fact]
    public async Task TheMerge_TellsOnceWhatTheGuestCartDidNotShow_ByName_KK4()
    {
        (_, List<Variant> chairs) = await SeedProductAsync("Sandalye", 1);
        (_, List<Variant> tables) = await SeedProductAsync("Masa", 1);
        (_, List<Variant> lamps) = await SeedProductAsync("Lamba", 1);
        Guid customerId = await SeedCustomerAsync();
        string key = NewKey();

        // The member's cart holds a chair the guest cart lacks, a table in a larger quantity, and a lamp in the same quantity.
        await SeedCartAsync(customerId, null, (chairs[0].Id, 1, 100m), (tables[0].Id, 4, 100m), (lamps[0].Id, 2, 100m));
        await SeedCartAsync(null, key, (tables[0].Id, 1, 100m), (lamps[0].Id, 2, 100m));

        CartMergeResult merge = await MergeAsync(customerId, key);
        CartView first = await OpenAsync(CartOwner.ForMember(customerId));
        CartView second = await OpenAsync(CartOwner.ForMember(customerId));

        Assert.Equal(2, merge.UnseenLines);
        Assert.Equal(["Masa", "Sandalye"], first.MergeAddedProductNames.Order(StringComparer.Ordinal));
        Assert.Empty(second.MergeAddedProductNames);
        Assert.Equal(3, second.Lines.Count);
    }

    [Fact]
    public async Task TheMerge_TellsNothing_WhenTheMemberCartAddedNothingTheGuestDidNotSee_KK4()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Masa", 2);
        Guid customerId = await SeedCustomerAsync();
        string key = NewKey();
        await SeedCartAsync(customerId, null, (variants[0].Id, 2, 100m));
        await SeedCartAsync(null, key, (variants[0].Id, 3, 100m), (variants[1].Id, 1, 100m));

        CartMergeResult merge = await MergeAsync(customerId, key);

        Assert.Equal(0, merge.UnseenLines);
        Assert.Empty((await OpenAsync(CartOwner.ForMember(customerId))).MergeAddedProductNames);

        // The guest cart of an empty member cart tells nothing either.
        Guid other = await SeedCustomerAsync();
        string otherKey = NewKey();
        await SeedCartAsync(null, otherKey, (variants[0].Id, 1, 100m));
        await MergeAsync(other, otherKey);
        Assert.Empty((await OpenAsync(CartOwner.ForMember(other))).MergeAddedProductNames);
    }

    // ---- KK5: the live line and the reference price ----

    [Fact]
    public async Task ThePriceMark_IsOneFlag_WithoutDirection_AndGoesWhenThePriceReturns_KK5()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Masa", 1);
        Guid customerId = await SeedCustomerAsync();
        CartOwner owner = CartOwner.ForMember(customerId);
        await AddAsync(owner, variants[0].Id, 1);

        Assert.False(Assert.Single((await OpenAsync(owner)).Lines).PriceChanged);

        await ExecuteAsync($"UPDATE variants SET price = 120 WHERE id = '{variants[0].Id}'");
        CartLine up = Assert.Single((await OpenAsync(owner)).Lines);
        Assert.True(up.PriceChanged);
        Assert.Equal(120m, up.UnitPrice);

        await ExecuteAsync($"UPDATE variants SET price = 80 WHERE id = '{variants[0].Id}'");
        CartLine down = Assert.Single((await OpenAsync(owner)).Lines);
        Assert.True(down.PriceChanged);
        Assert.Equal(80m, down.UnitPrice);

        await ExecuteAsync($"UPDATE variants SET price = 100 WHERE id = '{variants[0].Id}'");
        Assert.False(Assert.Single((await OpenAsync(owner)).Lines).PriceChanged);
    }

    [Fact]
    public async Task AQuantityIncrease_DoesNotMoveTheReferencePrice_KK5()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Masa", 1);
        Guid customerId = await SeedCustomerAsync();
        CartOwner owner = CartOwner.ForMember(customerId);
        await AddAsync(owner, variants[0].Id, 1);
        await ExecuteAsync($"UPDATE variants SET price = 130 WHERE id = '{variants[0].Id}'");

        await AddAsync(owner, variants[0].Id, 2);

        CartLine line = Assert.Single((await OpenAsync(owner)).Lines);
        Assert.Equal(3, line.Quantity);
        Assert.True(line.PriceChanged);
        await using AppDbContext reader = NewContext();
        Assert.Equal(100m, (await reader.CartItems.SingleAsync(Ct)).FirstAddedUnitPrice);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TheMerge_TakesTheReferencePriceOfTheLineAddedFirst_KK5(bool guestAddedFirst)
    {
        (_, List<Variant> variants) = await SeedProductAsync("Masa", 1);
        Guid customerId = await SeedCustomerAsync();
        string key = NewKey();
        DateTimeOffset early = Now.AddDays(-3);
        DateTimeOffset late = Now.AddDays(-1);
        await SeedCartAsync(customerId, null, (variants[0].Id, 2, guestAddedFirst ? 110m : 90m, guestAddedFirst ? late : early));
        await SeedCartAsync(null, key, (variants[0].Id, 1, guestAddedFirst ? 90m : 110m, guestAddedFirst ? early : late));

        await MergeAsync(customerId, key);

        await using AppDbContext reader = NewContext();
        CartItem item = await reader.CartItems.SingleAsync(Ct);
        Assert.Equal(90m, item.FirstAddedUnitPrice);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(early, item.CreatedAt);
    }

    // ---- KK6: the sold-out line stays and does not enter the order ----

    [Fact]
    public async Task ASoldOutLine_StaysInTheCart_DoesNotEnterTheOrder_AndEntersAgainWhenTheStockReturns_KK6()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Masa", 2);
        Guid customerId = await SeedCustomerAsync();
        CartOwner owner = CartOwner.ForMember(customerId);
        await AddAsync(owner, variants[0].Id, 2);
        await AddAsync(owner, variants[1].Id, 1);
        await ExecuteAsync($"UPDATE variants SET stock_quantity = 0 WHERE id = '{variants[0].Id}'");

        CartView soldOut = await OpenAsync(owner);

        CartLine gone = soldOut.Lines.Single(l => l.VariantId == variants[0].Id);
        Assert.True(gone.IsSoldOut);
        Assert.False(gone.EntersOrder);
        Assert.Equal([variants[1].Id], soldOut.OrderingLines.Select(l => l.VariantId));
        Assert.Equal(100m, soldOut.OrderableSubtotal);

        await ExecuteAsync($"UPDATE variants SET stock_quantity = 5 WHERE id = '{variants[0].Id}'");
        CartView back = await OpenAsync(owner);
        Assert.All(back.Lines, l => Assert.True(l.EntersOrder));
        Assert.Equal(300m, back.OrderableSubtotal);
    }

    [Fact]
    public async Task ASoldOutVariant_CannotBeAdded_AndAVariantNotOnTheShowcaseEither_KK6()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Masa", 2);
        Guid customerId = await SeedCustomerAsync();
        await ExecuteAsync($"UPDATE variants SET stock_quantity = 0 WHERE id = '{variants[0].Id}'");
        await ExecuteAsync($"UPDATE variants SET publish_status = 'Draft' WHERE id = '{variants[1].Id}'");

        CartAddResult soldOut = await AddAsync(CartOwner.ForMember(customerId), variants[0].Id, 1);
        CartAddResult draft = await AddAsync(CartOwner.ForMember(customerId), variants[1].Id, 1);
        CartAddResult missing = await AddAsync(CartOwner.ForMember(customerId), Guid.NewGuid(), 1);
        CartAddResult zero = await AddAsync(CartOwner.ForMember(customerId), variants[0].Id, 0);

        Assert.Equal(CartAddOutcome.SoldOut, soldOut.Outcome);
        Assert.Equal(CartAddOutcome.NotOnSale, draft.Outcome);
        Assert.Equal(CartAddOutcome.VariantNotFound, missing.Outcome);
        Assert.Equal(CartAddOutcome.InvalidQuantity, zero.Outcome);
        await using AppDbContext reader = NewContext();
        Assert.Equal(0, await reader.CartItems.CountAsync(Ct));
    }

    // ---- KK7: the item taken off the showcase is told once; no orderable line, no payment step ----

    [Fact]
    public async Task AnItemTakenOffTheShowcase_LeavesTheCart_AndIsToldOnce_KK7()
    {
        (Product product, List<Variant> variants) = await SeedProductAsync("Masa", 1);
        (_, List<Variant> others) = await SeedProductAsync("Sandalye", 1);
        Guid customerId = await SeedCustomerAsync();
        CartOwner owner = CartOwner.ForMember(customerId);
        await AddAsync(owner, variants[0].Id, 1);
        await AddAsync(owner, others[0].Id, 1);

        await RunAsync((ordering, token) => ordering.RemoveFromCartsAsync([variants[0].Id], product.Name, token));

        CartView first = await OpenAsync(owner);
        CartView second = await OpenAsync(owner);
        Assert.Equal(["Masa"], first.RemovedProductNames);
        Assert.Empty(second.RemovedProductNames);
        Assert.Equal([others[0].Id], first.Lines.Select(l => l.VariantId));
        Assert.True(first.CanOpenPayment);
    }

    [Fact]
    public async Task WithoutAnOrderableLine_ThePaymentStepDoesNotOpen_AndTheReasonIsInTheCart_KK7()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Masa", 2);
        Guid customerId = await SeedCustomerAsync();
        CartOwner owner = CartOwner.ForMember(customerId);

        CartView empty = await OpenAsync(owner);
        Assert.Equal(CartPaymentBlock.NoOrderableItem, empty.PaymentBlock);
        Assert.False(empty.CanOpenPayment);

        await AddAsync(owner, variants[0].Id, 2);
        await AddAsync(owner, variants[1].Id, 1);
        await ExecuteAsync("UPDATE variants SET stock_quantity = 0");

        CartView allSoldOut = await OpenAsync(owner);
        Assert.Equal(2, allSoldOut.Lines.Count);
        Assert.Equal(CartPaymentBlock.NoOrderableItem, allSoldOut.PaymentBlock);
        Assert.False(allSoldOut.CanOpenPayment);
        Assert.Equal(0m, allSoldOut.OrderableSubtotal);
    }

    [Fact]
    public async Task TheCartMethods_DoNotSaveOnTheirOwn_TheTransactionOwnerDoes_KK1()
    {
        (_, List<Variant> variants) = await SeedProductAsync("Masa", 1);
        Guid customerId = await SeedCustomerAsync();
        await AddAsync(CartOwner.ForMember(customerId), variants[0].Id, 1);

        // A read that changes nothing leaves the cart as it was; a merge that is rolled back leaves both carts.
        string key = NewKey();
        await SeedCartAsync(null, key, (variants[0].Id, 4, 100m));
        await using (ServiceProvider provider = OrderingTestServices.Build(_collection.Dsn))
        await using (AsyncServiceScope scope = provider.CreateAsyncScope())
        {
            AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await db.Database.BeginTransactionAsync(Ct);
            await scope.ServiceProvider.GetRequiredService<IOrderingModule>().MergeGuestCartAsync(customerId, key, Ct);
            await transaction.RollbackAsync(Ct);
        }

        await using AppDbContext reader = NewContext();
        Assert.Equal(2, await reader.Carts.CountAsync(Ct));
        Guid memberCartId = await reader.Carts.Where(c => c.CustomerId == customerId).Select(c => c.Id).SingleAsync(Ct);
        Assert.Equal(1, (await reader.CartItems.SingleAsync(i => i.CartId == memberCartId, Ct)).Quantity);
    }

    // ---- seeds and helpers ----

    private static string NewKey() => $"key-{Guid.NewGuid():N}";

    private async Task<(Product Product, List<Variant> Variants)> SeedProductAsync(string name, int variantCount)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create(name, $"urun-{n:D4}", ProductType.Physical, 20m, Now.AddMinutes(n)).WithSeededOrigin();
        product.PublishStatus = PublishStatus.Published;
        List<Variant> variants = [];
        for (int i = 0; i < variantCount; i++)
        {
            Variant variant = Variant.Create(product.Id, $"SKU-{n:D4}-{i}", 100m, Now.AddMinutes(n));
            variant.PublishStatus = PublishStatus.Published;
            variant.StockQuantity = 10;
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

    private Task<Guid> SeedCartAsync(Guid? customerId, string? guestKey, params (Guid VariantId, int Quantity, decimal Price)[] items)
        => SeedCartAsync(customerId, guestKey, [.. items.Select(i => (i.VariantId, i.Quantity, i.Price, Now))]);

    private async Task<Guid> SeedCartAsync(Guid? customerId, string? guestKey, params (Guid VariantId, int Quantity, decimal Price, DateTimeOffset At)[] items)
    {
        Cart cart = customerId is Guid id ? Cart.ForCustomer(id, Now) : Cart.ForGuest(Shopfolio.Infrastructure.Security.SecretKey.Hash(guestKey!), Now);
        await SaveAsync(c =>
        {
            c.Carts.Add(cart);
            foreach ((Guid variantId, int quantity, decimal price, DateTimeOffset at) in items)
            {
                c.CartItems.Add(CartItem.Create(cart.Id, variantId, quantity, price, at));
            }
        });
        return cart.Id;
    }

    private async Task SeedOrderAsync(Guid productId, Guid variantId, Guid cartId)
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
        OrderItem item = OrderItem.Create(order.Id, productId, variantId, ProductType.Physical, "Masa", 1, 100m, 20m, 16.67m, Now);
        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.LegalTextVersions.AddRange(form, contract, notice);
            c.Orders.Add(order);
            c.OrderItems.Add(item);
        });
    }

    private Task<CartView> OpenAsync(CartOwner owner) => RunAsync((ordering, token) => ordering.OpenCartAsync(owner, token));

    private Task<CartAddResult> AddAsync(CartOwner owner, Guid variantId, int quantity) => RunAsync((ordering, token) => ordering.AddToCartAsync(owner, variantId, quantity, token));

    private Task<CartMergeResult> MergeAsync(Guid customerId, string key) => RunAsync((ordering, token) => ordering.MergeGuestCartAsync(customerId, key, token));

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
