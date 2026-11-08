using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The order number and the ways into the order page on a real PostgreSQL (T354 KK1-KK6; 02 3.22.1-3.22.5, 8.3.5): the number the confirmation is given and that a
/// taken or cancelled order's number is never given again, the three ways in - the member's history, the guest's number with the e-mail, the access key of the e-mail
/// link -, the key that is unguessable, has no life of its own and stops opening at its renewal and at the disposal, the member's address that opens for no one without
/// the account, and the four things of the page read from the order alone with no notice, job or e-mail in the database.
/// </summary>
[Collection(OrderTransitionsDefinition.Name)]
public sealed class OrderAccessTests : IAsyncLifetime
{
    // Monday 9 November 2026, 12:00 in Turkey.
    private static readonly DateTimeOffset Start = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;
    private ServiceProvider _provider = null!;
    private int _counter;

    public OrderAccessTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE dispatch_records, jobs, order_items, orders, customers, legal_text_versions, provinces CASCADE");
        _provider = BuildProvider();
    }

    public async ValueTask DisposeAsync() => await _provider.DisposeAsync();

    // ---- KK1, KK2: the number the order is given ----

    [Fact]
    public async Task Issue_GivesANumberOfTheTurkeyYear_AndAnAccessKey_KK1()
    {
        NewOrderIdentifiers inYear = await IssueAsync(Start);
        NewOrderIdentifiers atTheTurn = await IssueAsync(new DateTimeOffset(2026, 12, 31, 21, 30, 0, TimeSpan.Zero));

        Assert.StartsWith("2026-", inYear.OrderNumber, StringComparison.Ordinal);
        Assert.Equal(4 + 1 + ProductConstants.OrderNumberRandomBlockLength, inYear.OrderNumber.Length);
        Assert.All(inYear.OrderNumber[5..], c => Assert.Contains(c, OrderNumber.Alphabet));

        // 31 December 21:30 UTC is 1 January 00:30 in Turkey: the year of the order is the Turkey year (02 4.1.1).
        Assert.StartsWith("2027-", atTheTurn.OrderNumber, StringComparison.Ordinal);

        // Made by the one key function: 256 random bits as text, a different one every time.
        Assert.Equal(SecretKey.Create().Length, inYear.OrderAccessKey.Length);
        Assert.NotEqual(inYear.OrderAccessKey, atTheTurn.OrderAccessKey);
    }

    [Fact]
    public async Task Issue_WhenTheNumberIsTakenByAnOrder_MakesAnotherBeforeAnyOrderExists_KK2()
    {
        Order taken = await SeedAsync(number: "2026-AAAAAA");
        Queue<string> made = new(["2026-AAAAAA", "2026-AAAAAA", "2026-BBBBBB"]);

        NewOrderIdentifiers issued = await IssueAsync(Start, _ => made.Dequeue());

        Assert.Equal("2026-BBBBBB", issued.OrderNumber);
        Assert.Empty(made);
        await using AppDbContext reader = NewContext();
        Assert.Equal([taken.Id], await reader.Orders.Select(o => o.Id).ToListAsync(Ct));
    }

    [Fact]
    public async Task Issue_AfterTheOrderIsCancelled_StillSkipsItsNumber_KK2()
    {
        Order cancelled = await SeedAsync(number: "2026-CCCCCC");
        await ExecuteAsync($"UPDATE orders SET order_status = 'Cancelled', payment_status = 'Failed' WHERE id = '{cancelled.Id}'");
        Queue<string> made = new(["2026-CCCCCC", "2026-DDDDDD"]);

        NewOrderIdentifiers issued = await IssueAsync(Start, _ => made.Dequeue());

        Assert.Equal("2026-DDDDDD", issued.OrderNumber);
        await using AppDbContext reader = NewContext();
        Assert.Equal(OrderStatus.Cancelled, (await reader.Orders.SingleAsync(o => o.Id == cancelled.Id, Ct)).OrderStatus);
    }

    [Fact]
    public async Task Issue_SkipsTheNumberOfAnOrderTheCallerAddedAndHasNotSavedYet_KK2()
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Orders.Add(Order.Create("2026-EEEEEE", SecretKey.Create(), SecretKey.Create(), PaymentMethod.Card, true, "ali@example.com", 100m, 0m, Start.AddHours(1), Start));
        Queue<string> made = new(["2026-EEEEEE", "2026-FFFFFF"]);

        NewOrderIdentifiers issued = await scope.ServiceProvider.GetRequiredService<OrderIdentifiers>().IssueAsync(Start, _ => made.Dequeue(), Ct);

        Assert.Equal("2026-FFFFFF", issued.OrderNumber);
    }

    [Fact]
    public async Task TheUniqueIndexes_AreTheOnesTheRetryNames_AndTheDatabaseBreaksThemOnAClash_KK2()
    {
        Order first = await SeedAsync(number: "2026-GGGGGG");

        string numberIndex = await ClashAsync(() => SeedAsync(number: first.OrderNumber));
        string keyIndex = await ClashAsync(() => SeedAsync(number: "2026-HHHHHH", accessKey: first.OrderAccessKey));

        Assert.Equal("ix_orders_order_number", numberIndex);
        Assert.Equal("ix_orders_order_access_key", keyIndex);
        Assert.Equal([numberIndex, keyIndex], OrderIdentifiers.UniqueIndexes);
    }

    // ---- KK3: the three ways in ----

    [Fact]
    public async Task AMemberOpensTheOrderFromTheHistory_AnotherCustomerAndAGuestOrderDoNot_KK3()
    {
        Customer owner = await SeedCustomerAsync();
        Customer other = await SeedCustomerAsync();
        Order own = await SeedAsync(customerId: owner.Id);
        Order guest = await SeedAsync();

        Assert.Equal(own.Id, await WithModuleAsync(m => m.FindMemberOrderAsync(own.OrderNumber.ToLowerInvariant(), owner.Id, Ct)));
        Assert.Null(await WithModuleAsync(m => m.FindMemberOrderAsync(own.OrderNumber, other.Id, Ct)));
        Assert.Null(await WithModuleAsync(m => m.FindMemberOrderAsync(guest.OrderNumber, owner.Id, Ct)));
        Assert.Null(await WithModuleAsync(m => m.FindMemberOrderAsync("2026-ZZZZZZ", owner.Id, Ct)));
    }

    [Fact]
    public async Task AGuestOpensTheOrderWithTheNumberAndTheEmail_WhateverTheLetterCase_KK3()
    {
        Order order = await SeedAsync(email: "Ali.Veli@Example.com");

        OrderAccessLookup? exact = await WithModuleAsync(m => m.FindOrderByNumberAndEmailAsync(order.OrderNumber, "Ali.Veli@Example.com", Ct));
        OrderAccessLookup? folded = await WithModuleAsync(m => m.FindOrderByNumberAndEmailAsync(" " + order.OrderNumber.ToLowerInvariant() + " ", "  ALI.VELI@EXAMPLE.COM ", Ct));

        Assert.Equal(new OrderAccessLookup(order.Id, order.OrderAccessKey!), exact);
        Assert.Equal(exact, folded);
    }

    [Fact]
    public async Task TheGuestsLookup_AnswersNullForEveryWrongPart_AndTheNumberAloneOpensNothing_KK3_KK5()
    {
        Order order = await SeedAsync(email: "ali@example.com");
        await SeedAsync(email: "veli@example.com");

        Assert.Null(await WithModuleAsync(m => m.FindOrderByNumberAndEmailAsync(order.OrderNumber, "veli@example.com", Ct)));
        Assert.Null(await WithModuleAsync(m => m.FindOrderByNumberAndEmailAsync(order.OrderNumber, "ali@example.org", Ct)));
        Assert.Null(await WithModuleAsync(m => m.FindOrderByNumberAndEmailAsync("2026-ZZZZZZ", "ali@example.com", Ct)));
        Assert.Null(await WithModuleAsync(m => m.FindOrderByNumberAndEmailAsync(order.OrderNumber, string.Empty, Ct)));
        Assert.Null(await WithModuleAsync(m => m.FindOrderByNumberAndEmailAsync(order.OrderNumber, "   ", Ct)));
        Assert.Null(await WithModuleAsync(m => m.FindOrderByNumberAndEmailAsync(string.Empty, "ali@example.com", Ct)));
    }

    [Fact]
    public async Task TheGuestsLookup_MatchesTheFrozenEmail_OfADeletedAccountAndOfAnAccountWhoseAddressChanged_KK3()
    {
        Customer customer = await SeedCustomerAsync();
        Order deleted = await SeedAsync(email: "silinen@example.com");
        Order changed = await SeedAsync(email: "eski@example.com", customerId: customer.Id);
        await ExecuteAsync($"UPDATE orders SET is_account_deleted = true WHERE id = '{deleted.Id}'");
        await ExecuteAsync($"UPDATE customers SET email = 'yeni@example.com', normalized_email = 'yeni@example.com' WHERE id = '{customer.Id}'");

        Assert.Equal(deleted.Id, (await WithModuleAsync(m => m.FindOrderByNumberAndEmailAsync(deleted.OrderNumber, "silinen@example.com", Ct)))!.OrderId);
        Assert.Equal(changed.Id, (await WithModuleAsync(m => m.FindOrderByNumberAndEmailAsync(changed.OrderNumber, "eski@example.com", Ct)))!.OrderId);

        // The account's new address is not the order's: the frozen one is what matches.
        Assert.Null(await WithModuleAsync(m => m.FindOrderByNumberAndEmailAsync(changed.OrderNumber, "yeni@example.com", Ct)));
    }

    [Fact]
    public async Task TheAccessKeyOpensTheOrderOfAGuestAndOfAMember_AndNothingElse_KK3_KK4()
    {
        Customer member = await SeedCustomerAsync();
        Order guest = await SeedAsync();
        Order own = await SeedAsync(customerId: member.Id);

        Assert.Equal(guest.Id, await WithModuleAsync(m => m.FindOrderByAccessKeyAsync(guest.OrderAccessKey!, Ct)));
        Assert.Equal(own.Id, await WithModuleAsync(m => m.FindOrderByAccessKeyAsync(own.OrderAccessKey!, Ct)));
        Assert.Null(await WithModuleAsync(m => m.FindOrderByAccessKeyAsync(SecretKey.Create(), Ct)));
        Assert.Null(await WithModuleAsync(m => m.FindOrderByAccessKeyAsync(string.Empty, Ct)));

        // The number is no key and a key is no number.
        Assert.Null(await WithModuleAsync(m => m.FindOrderByAccessKeyAsync(guest.OrderNumber, Ct)));
        Assert.Null(await WithModuleAsync(m => m.FindMemberOrderAsync(guest.OrderAccessKey!, member.Id, Ct)));
    }

    [Fact]
    public async Task TheThreeWays_OpenTheSameOrder_KK3()
    {
        Customer member = await SeedCustomerAsync();
        Order order = await SeedAsync(email: "ali@example.com", customerId: member.Id);

        Guid? fromHistory = await WithModuleAsync(m => m.FindMemberOrderAsync(order.OrderNumber, member.Id, Ct));
        OrderAccessLookup? fromEmail = await WithModuleAsync(m => m.FindOrderByNumberAndEmailAsync(order.OrderNumber, "ali@example.com", Ct));
        Guid? fromLink = await WithModuleAsync(m => m.FindOrderByAccessKeyAsync(fromEmail!.AccessKey, Ct));

        Assert.Equal([order.Id, order.Id, order.Id], [fromHistory, fromEmail!.OrderId, fromLink]);
    }

    // ---- KK4: the key ----

    [Fact]
    public async Task TheAccessKeyOpensWithoutAskingForTheEmail_AndHasNoLifeOfItsOwn_KK4()
    {
        // An order ten years old: nothing in the way in reads a clock, so the key opens it as on the first day.
        Order old = await SeedAsync(createdAt: Start.AddYears(-10));

        Assert.Equal(old.Id, await WithModuleAsync(m => m.FindOrderByAccessKeyAsync(old.OrderAccessKey!, Ct)));
    }

    [Fact]
    public async Task TheAccessKeyIsCaseSensitive_AndTwoOrdersHaveTwoKeys_KK4()
    {
        Order first = await SeedAsync();
        Order second = await SeedAsync();
        string key = first.OrderAccessKey!;
        string changed = key.Any(char.IsAsciiLetterLower) ? key.ToUpperInvariant() : key.ToLowerInvariant();

        Assert.NotEqual(first.OrderAccessKey, second.OrderAccessKey);
        Assert.NotEqual(key, changed);
        Assert.Null(await WithModuleAsync(m => m.FindOrderByAccessKeyAsync(changed, Ct)));
    }

    [Fact]
    public async Task WhenTheKeyIsRenewed_TheOldOneStopsOpening_AndTheNewOneOpens_KK4()
    {
        Order order = await SeedAsync(email: "ali@example.com");
        string oldKey = order.OrderAccessKey!;
        string newKey;
        await using (AppDbContext context = NewContext())
        {
            Order loaded = await context.Orders.SingleAsync(o => o.Id == order.Id, Ct);
            newKey = loaded.RenewAccessKey();
            loaded.BumpVersion();
            await context.SaveChangesAsync(Ct);
        }

        Assert.Null(await WithModuleAsync(m => m.FindOrderByAccessKeyAsync(oldKey, Ct)));
        Assert.Equal(order.Id, await WithModuleAsync(m => m.FindOrderByAccessKeyAsync(newKey, Ct)));

        // The guest's lookup hands out the new key.
        Assert.Equal(newKey, (await WithModuleAsync(m => m.FindOrderByNumberAndEmailAsync(order.OrderNumber, "ali@example.com", Ct)))!.AccessKey);
    }

    [Fact]
    public async Task WhenThePersonalDataIsDisposed_TheKeyIsEmpty_AndNoWayOpensTheOrder_KK4()
    {
        Order order = await SeedAsync(email: "ali@example.com");
        string key = order.OrderAccessKey!;
        await using (AppDbContext context = NewContext())
        {
            Order loaded = await context.Orders.SingleAsync(o => o.Id == order.Id, Ct);
            loaded.ErasePersonalData();
            loaded.BumpVersion();
            await context.SaveChangesAsync(Ct);
        }

        Assert.Null(await WithModuleAsync(m => m.FindOrderByAccessKeyAsync(key, Ct)));
        Assert.Null(await WithModuleAsync(m => m.FindOrderByNumberAndEmailAsync(order.OrderNumber, "ali@example.com", Ct)));
        Assert.Null(await WithModuleAsync(m => m.FindOrderByNumberAndEmailAsync(order.OrderNumber, string.Empty, Ct)));
    }

    // ---- KK6: what the page carries, from the order alone ----

    [Fact]
    public async Task ThePageSummary_CarriesTheStatus_TheTrackingLink_TheDownloadButtons_AndTheFrozenIban_KK6()
    {
        Order order = await SeedAsync(
            method: PaymentMethod.BankTransfer, email: "ali@example.com", configure: o =>
            {
                o.BankTransferIban = "TR330006100519786457841326";
                o.ApplyCorrection(OrderStatus.Shipped, PaymentStatus.Paid);
                o.RecordShipment("YurticiKargo", null, "YK 123", isOwnVehicleDelivery: false);
            });
        OrderItem open = await AddItemAsync(order.Id, ProductType.Digital, "Kitap", allowance: 3, count: 1);
        OrderItem used = await AddItemAsync(order.Id, ProductType.Digital, "Müzik", allowance: 2, count: 2);
        await AddItemAsync(order.Id, ProductType.Physical, "Kupa", allowance: null, count: 0);

        // The buyer reaches the page by the link: no e-mail, notice or job stands between (02 6.1.1).
        OrderAccessLookup access = (await WithModuleAsync(m => m.FindOrderByNumberAndEmailAsync(order.OrderNumber, "ali@example.com", Ct)))!;
        OrderPageSummary summary = (await WithModuleAsync(m => m.GetOrderPageSummaryAsync(access.OrderId, Ct)))!;

        Assert.Equal(order.OrderNumber, summary.OrderNumber);
        Assert.Equal(OrderStatus.Shipped, summary.OrderStatus);
        Assert.Equal(PaymentStatus.Paid, summary.PaymentStatus);
        Assert.Equal("Yurtiçi Kargo", summary.Shipping.CarrierName);
        Assert.Equal("YK 123", summary.Shipping.TrackingNumber);
        Assert.Equal(CarrierCatalog.Default.BuildLink("YurticiKargo", "YK 123"), summary.Shipping.Link);
        Assert.Equal("TR330006100519786457841326", summary.TransferIban);
        Assert.Equal(
            [new OrderDownloadButton(open.Id, "Kitap", true), new OrderDownloadButton(used.Id, "Müzik", false)],
            summary.Downloads);

        await using AppDbContext reader = NewContext();
        Assert.False(await reader.DispatchRecords.AnyAsync(Ct));
        Assert.False(await reader.Jobs.AnyAsync(Ct));
    }

    [Fact]
    public async Task ThePageSummary_OfAnUnpaidCardOrder_HasNoDownloadButton_NoIban_AndNoLink_KK6()
    {
        Order order = await SeedAsync(method: PaymentMethod.Card);
        await AddItemAsync(order.Id, ProductType.Digital, "Kitap", allowance: 3, count: 0);

        OrderPageSummary summary = (await WithModuleAsync(m => m.GetOrderPageSummaryAsync(order.Id, Ct)))!;

        Assert.Equal(PaymentStatus.Pending, summary.PaymentStatus);
        Assert.Empty(summary.Downloads);
        Assert.Null(summary.TransferIban);
        Assert.Null(summary.Shipping.Link);
        Assert.Null(summary.Shipping.TrackingNumber);
    }

    [Fact]
    public async Task ThePageSummary_OfAMissingOrder_IsNull_KK6()
    {
        Assert.Null(await WithModuleAsync(m => m.GetOrderPageSummaryAsync(Guid.NewGuid(), Ct)));
    }

    // ---- helpers ----

    private async Task<NewOrderIdentifiers> IssueAsync(DateTimeOffset now, Func<int, string>? makeNumber = null)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        OrderIdentifiers identifiers = scope.ServiceProvider.GetRequiredService<OrderIdentifiers>();
        return makeNumber is null ? await identifiers.IssueAsync(now, Ct) : await identifiers.IssueAsync(now, makeNumber, Ct);
    }

    private async Task<T> WithModuleAsync<T>(Func<IOrderingModule, Task<T>> call)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<IOrderingModule>());
    }

    /// <summary>Runs a seed that breaks a unique index and returns the name of the index the database names.</summary>
    private static async Task<string> ClashAsync(Func<Task> seed)
    {
        DbUpdateException error = await Assert.ThrowsAsync<DbUpdateException>(seed);
        PostgresException postgres = Assert.IsType<PostgresException>(error.InnerException);
        Assert.Equal(PostgresErrorCodes.UniqueViolation, postgres.SqlState);
        return postgres.ConstraintName!;
    }

    private async Task<Customer> SeedCustomerAsync()
    {
        int n = Interlocked.Increment(ref _counter);
        Customer customer = Customer.Create($"Müşteri {n}", $"musteri{n}@example.com", "digest-placeholder", Start, null, Start);
        await SaveAsync(c => c.Customers.Add(customer));
        return customer;
    }

    private async Task<OrderItem> AddItemAsync(Guid orderId, ProductType type, string name, int? allowance, int count)
    {
        OrderItem item = OrderItem.Create(orderId, null, null, type, name, 1, 100m, 20m, 16.67m, Start);
        item.DownloadAllowance = allowance;
        await SaveAsync(c => c.OrderItems.Add(item));
        if (count > 0)
        {
            await ExecuteAsync($"UPDATE order_items SET download_count = {count} WHERE id = '{item.Id}'");
        }

        return item;
    }

    private async Task<Order> SeedAsync(
        string? number = null,
        string? accessKey = null,
        string email = "alici@example.com",
        Guid? customerId = null,
        PaymentMethod method = PaymentMethod.Card,
        DateTimeOffset? createdAt = null,
        Action<Order>? configure = null)
    {
        int n = Interlocked.Increment(ref _counter);
        DateTimeOffset at = createdAt ?? Start;
        Province province = Province.Create($"{n % 100:D2}", $"İl {n}", Start);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Start, "metin", Start);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Start, "metin", Start);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Start, "metin", Start).WithHosting();
        Order order = Order.Create(
            number ?? OrderNumber.Create(2026), accessKey ?? SecretKey.Create(), SecretKey.Create(), method, customerId is null, email, 120m, 0m, at.AddHours(1), at);
        order.CustomerId = customerId;
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
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
        order.BankTransferIban = method == PaymentMethod.BankTransfer ? "TR330006100519786457841326" : null;
        configure?.Invoke(order);

        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.LegalTextVersions.AddRange(form, contract, notice);
            c.Orders.Add(order);
        });
        return order;
    }

    private ServiceProvider BuildProvider()
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddSingleton(NpgsqlDataSource.Create(_collection.Dsn));
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
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.None));
        services.AddCatalogModule();
        services.AddAfterSalesModule();
        services.AddNoticeDependencies();
        services.AddOrderingModule();
        services.AddPaymentModule();
        return services.BuildServiceProvider();
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
