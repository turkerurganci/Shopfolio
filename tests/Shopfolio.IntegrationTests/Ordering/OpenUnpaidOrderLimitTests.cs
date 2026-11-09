using System.Net;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// L-8, the open unpaid order ceiling, on a real PostgreSQL (T376 criterion 8, KK8; 02 8.2, 8.2.2, 11.2 P-46): it counts the orders whose payment is Pending - no
/// window, so an old open order still counts - and the count falls when an open order is paid or cancelled; the ceiling is the product constant; the e-mail axis
/// counts only a signed-in member's orders, the IP axis counts every order, an IPv6 address by its /64 and an IPv4-mapped address as the IPv4 one.
/// </summary>
[Collection(OrderTransitionsDefinition.Name)]
public sealed class OpenUnpaidOrderLimitTests : IAsyncLifetime
{
    private const string Email = "Ali@Example.com";

    private static readonly DateTimeOffset Start = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;
    private int _counter;

    public OpenUnpaidOrderLimitTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE stock_reservations, dispatch_records, jobs, payment_records, cancellations, internal_notes, transition_records, order_items, orders, "
            + "coupons, variants, products, heartbeat_records, outage_records, legal_text_versions, provinces CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task TheCeiling_IsTheProductConstant_AndAnAxisIsFullExactlyAtIt_KK8()
    {
        Assert.Equal(ProductConstants.OpenUnpaidOrdersMax, OpenUnpaidOrderLimit.Ceiling);
        for (int i = 0; i < ProductConstants.OpenUnpaidOrdersMax - 1; i++)
        {
            await SeedAsync(isGuest: false, ip: "203.0.113.5");
        }

        Assert.False(await IsFullByIpAsync("203.0.113.5"));
        Assert.False(await IsFullByEmailAsync(Email));

        await SeedAsync(isGuest: false, ip: "203.0.113.5");

        Assert.True(await IsFullByIpAsync("203.0.113.5"));
        Assert.True(await IsFullByEmailAsync(Email));
    }

    [Fact]
    public async Task AnOpenOrderThatIsPaidOrCancelled_LeavesTheCount_AndNothingElseDoes_KK8()
    {
        Guid paid = (await SeedAsync(isGuest: false, ip: "203.0.113.5")).Id;
        Guid cancelled = (await SeedAsync(isGuest: false, ip: "203.0.113.5")).Id;
        await SeedAsync(isGuest: false, ip: "203.0.113.5");
        Assert.Equal(3, await CountByIpAsync("203.0.113.5"));
        Assert.Equal(3, await CountByEmailAsync(Email));

        await ExecuteAsync($"UPDATE orders SET payment_status = '{PaymentStatus.Paid}' WHERE id = '{paid}'");
        Assert.Equal(2, await CountByIpAsync("203.0.113.5"));
        Assert.Equal(2, await CountByEmailAsync(Email));
        Assert.False(await IsFullByIpAsync("203.0.113.5"));

        // The unpaid cancellation (S3 with Ö2) leaves the payment Failed; the order is no longer open.
        await ExecuteAsync($"UPDATE orders SET order_status = '{OrderStatus.Cancelled}', payment_status = '{PaymentStatus.Failed}' WHERE id = '{cancelled}'");
        Assert.Equal(1, await CountByIpAsync("203.0.113.5"));
        Assert.Equal(1, await CountByEmailAsync(Email));
    }

    [Fact]
    public async Task ThereIsNoWindow_AnOpenOrderOfAYearAgoStillCounts_KK8()
    {
        await SeedAsync(isGuest: false, ip: "203.0.113.5", createdAt: Start.AddDays(-400));

        Assert.Equal(1, await CountByIpAsync("203.0.113.5"));
        Assert.Equal(1, await CountByEmailAsync(Email));
    }

    [Fact]
    public async Task TheEmailAxis_CountsOnlyAMembersOrders_AndOneAddressWhateverItsCase_KK8()
    {
        await SeedAsync(isGuest: false, ip: "203.0.113.5", email: "ali@example.com");
        await SeedAsync(isGuest: true, ip: "203.0.113.6", email: "ali@example.com");
        await SeedAsync(isGuest: false, ip: "203.0.113.7", email: "veli@example.com");

        Assert.Equal(1, await CountByEmailAsync("ALI@EXAMPLE.COM"));
        Assert.Equal(1, await CountByEmailAsync("veli@example.com"));
        Assert.Equal(0, await CountByEmailAsync("yok@example.com"));
    }

    [Fact]
    public async Task TheIpAxis_CountsGuestAndMemberOrders_AndNoOtherAddress_KK8()
    {
        await SeedAsync(isGuest: true, ip: "203.0.113.5");
        await SeedAsync(isGuest: false, ip: "203.0.113.5");
        await SeedAsync(isGuest: true, ip: "203.0.113.6");

        Assert.Equal(2, await CountByIpAsync("203.0.113.5"));
        Assert.Equal(1, await CountByIpAsync("203.0.113.6"));
        Assert.Equal(0, await CountByIpAsync("203.0.113.7"));
    }

    [Fact]
    public async Task AnIpv6AddressIsItsSlash64_AndAnIpv4MappedAddressIsTheIpv4One_KK8()
    {
        await SeedAsync(isGuest: true, ip: "2001:db8:1:2::1");
        await SeedAsync(isGuest: true, ip: "2001:db8:1:2:ffff::9");
        await SeedAsync(isGuest: true, ip: "2001:db8:1:3::1");
        await SeedAsync(isGuest: true, ip: "::ffff:198.51.100.9");
        await SeedAsync(isGuest: true, ip: "198.51.100.9");

        Assert.Equal(2, await CountByIpAsync("2001:db8:1:2:abcd::5"));
        Assert.Equal(1, await CountByIpAsync("2001:db8:1:3::7"));
        Assert.Equal(2, await CountByIpAsync("198.51.100.9"));
        Assert.Equal(2, await CountByIpAsync("::ffff:198.51.100.9"));
    }

    [Fact]
    public async Task NoAddress_CountsNoOrder_BecauseALiveOrderAlwaysHoldsItsAddress_KK8()
    {
        await SeedAsync(isGuest: true, ip: "203.0.113.5");

        await using AppDbContext db = NewContext();
        Assert.Equal(0, await new OpenUnpaidOrderLimit(db).CountByIpAsync(null, Ct));
        Assert.False(await new OpenUnpaidOrderLimit(db).IsFullByIpAsync(null, Ct));
    }

    private async Task<int> CountByIpAsync(string address)
    {
        await using AppDbContext db = NewContext();
        return await new OpenUnpaidOrderLimit(db).CountByIpAsync(IPAddress.Parse(address), Ct);
    }

    private async Task<int> CountByEmailAsync(string email)
    {
        await using AppDbContext db = NewContext();
        return await new OpenUnpaidOrderLimit(db).CountByEmailAsync(email, Ct);
    }

    private async Task<bool> IsFullByIpAsync(string address)
    {
        await using AppDbContext db = NewContext();
        return await new OpenUnpaidOrderLimit(db).IsFullByIpAsync(IPAddress.Parse(address), Ct);
    }

    private async Task<bool> IsFullByEmailAsync(string email)
    {
        await using AppDbContext db = NewContext();
        return await new OpenUnpaidOrderLimit(db).IsFullByEmailAsync(email, Ct);
    }

    private async Task<Order> SeedAsync(bool isGuest, string? ip, string email = Email, DateTimeOffset? createdAt = null)
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n:D2}", $"İl {n}", Start);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Start, "metin", Start);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Start, "metin", Start);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Start, "metin", Start).WithHosting();
        Order order = Order.Create(
            $"2026-{n + 376000:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", PaymentMethod.BankTransfer, isGuest, email, 120m, 0m, Start.AddDays(2), createdAt ?? Start.AddDays(-1));
        order.IpAddress = ip is null ? null : IPAddress.Parse(ip);
        order.CartId = EntityIds.New();
        order.BankTransferIban = "TR330006100519786457841326";
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
        OrderItem item = OrderItem.Create(order.Id, null, null, ProductType.Physical, "Ürün", 1, 100m, 20m, 16.67m, Start);
        await using AppDbContext db = NewContext();
        db.Provinces.Add(province);
        db.LegalTextVersions.AddRange(form, contract, notice);
        db.Orders.Add(order);
        db.OrderItems.Add(item);
        await db.SaveChangesAsync(Ct);
        return order;
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }
}
