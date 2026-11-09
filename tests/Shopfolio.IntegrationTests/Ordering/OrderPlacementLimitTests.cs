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
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// L-7 and L-8 at the confirmation's one gate, on a real PostgreSQL (T377 criteria 3-6, KK3-KK6; 02 8.2.2, 8.2.3, 8.2.4, 11.2 P-37, P-46): L-7 counts the unpaid orders the system
/// cancelled because the window ran out or the card failed, in the sliding window, and nothing else (not the customer's or the firm's cancellation, not a new order of the same
/// cart, not an order whose payment page was never opened); both limits count two axes separately and the e-mail axis only a signed-in member's orders; L-8 closes the bank transfer
/// only and leaves the card open; no account type is exempt.
/// </summary>
[Collection(OrderTransitionsDefinition.Name)]
public sealed class OrderPlacementLimitTests : IAsyncLifetime
{
    private const string Email = "Ali@Example.com";
    private const string Ip = "203.0.113.5";

    private static readonly DateTimeOffset Start = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;
    private readonly ManualTimeProvider _clock = new(Start);
    private int _counter;

    public OrderPlacementLimitTests(CollectionDatabaseFixture collection)
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

    // ---- KK4: only the two automatic reasons count ----

    [Fact]
    public async Task OnlyTheWindowThatRanOutAndTheCardThatFailed_AreCounted_KK4()
    {
        foreach (TransitionReason reason in Enum.GetValues<TransitionReason>())
        {
            await CancelAsync(await SeedAsync(isGuest: false, ip: Ip), ActorKind.System, reason);
        }

        // The customer's and the firm's cancellation of an unpaid order: no automatic reason (the firm's carries its own reason).
        await CancelAsync(await SeedAsync(isGuest: false, ip: Ip), ActorKind.Buyer, reason: null);
        await CancelAsync(await SeedAsync(isGuest: false, ip: Ip), ActorKind.Admin, reason: null);

        Assert.Equal(UnpaidCancellationLimit.CountedReasons.Count, await CountByIpAsync(Ip));
        Assert.Equal(UnpaidCancellationLimit.CountedReasons.Count, await CountByEmailAsync(Email));
        Assert.Equal(
            [TransitionReason.PaymentPageNotOpened, TransitionReason.SameCartReorder, TransitionReason.Closure],
            Enum.GetValues<TransitionReason>().Except(UnpaidCancellationLimit.CountedReasons).Order());
    }

    [Theory]
    [InlineData(TransitionReason.SameCartReorder)]
    [InlineData(TransitionReason.PaymentPageNotOpened)]
    public async Task AnyNumberOfTheLeftOutCancellations_NeverFillsTheLimit_KK4(TransitionReason reason)
    {
        for (int i = 0; i < UnpaidCancellationLimit.Max + 3; i++)
        {
            await CancelAsync(await SeedAsync(isGuest: false, ip: Ip), ActorKind.System, reason);
        }

        Assert.Equal(0, await CountByIpAsync(Ip));
        Assert.Null(await ExceededAsync(PaymentMethod.Card, Ip, Email));
        Assert.Null(await ExceededAsync(PaymentMethod.BankTransfer, Ip, Email));
    }

    [Fact]
    public async Task TheWindowSlides_ARowExactlyOneWindowOldStillCounts_AndOlderOnesDoNot_KK4()
    {
        TimeSpan window = L7Window();
        await CancelAsync(await SeedAsync(isGuest: false, ip: Ip), ActorKind.System, TransitionReason.PaymentWindowExpired, at: Start - window);
        await CancelAsync(await SeedAsync(isGuest: false, ip: Ip), ActorKind.System, TransitionReason.CardPaymentFailed, at: Start - window - TimeSpan.FromSeconds(1));

        Assert.Equal(1, await CountByIpAsync(Ip));

        _clock.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(0, await CountByIpAsync(Ip));
    }

    [Fact]
    public async Task TheLimitIsFullExactlyAtTheMaximum_AndItStopsEveryPaymentMethod_KK4_KK5()
    {
        for (int i = 0; i < UnpaidCancellationLimit.Max - 1; i++)
        {
            await CancelAsync(await SeedAsync(isGuest: true, ip: Ip), ActorKind.System, TransitionReason.CardPaymentFailed);
        }

        Assert.Null(await ExceededAsync(PaymentMethod.Card, Ip, null));

        await CancelAsync(await SeedAsync(isGuest: true, ip: Ip), ActorKind.System, TransitionReason.PaymentWindowExpired);

        Assert.Equal(AttemptLimit.L7, await ExceededAsync(PaymentMethod.Card, Ip, null));
        Assert.Equal(AttemptLimit.L7, await ExceededAsync(PaymentMethod.BankTransfer, Ip, null));
        Assert.Null(await ExceededAsync(PaymentMethod.Card, "203.0.113.6", null));
    }

    // ---- KK5: the e-mail axis counts only a signed-in member's orders, in L-7 and in L-8 ----

    [Fact]
    public async Task L7_EmailAxisCountsOnlyAMembersOrders_AGuestOrderIsOnTheIpAxisOnly_KK5()
    {
        for (int i = 0; i < UnpaidCancellationLimit.Max; i++)
        {
            await CancelAsync(await SeedAsync(isGuest: true, ip: $"198.51.100.{i + 1}", email: "ali@example.com"), ActorKind.System, TransitionReason.PaymentWindowExpired);
        }

        Assert.Equal(0, await CountByEmailAsync("ali@example.com"));

        for (int i = 0; i < UnpaidCancellationLimit.Max; i++)
        {
            await CancelAsync(await SeedAsync(isGuest: false, ip: $"198.51.101.{i + 1}", email: "veli@example.com"), ActorKind.System, TransitionReason.CardPaymentFailed);
        }

        Assert.Equal(UnpaidCancellationLimit.Max, await CountByEmailAsync("VELI@example.com"));
        Assert.Equal(AttemptLimit.L7, await ExceededAsync(PaymentMethod.Card, "192.0.2.77", "veli@example.com"));

        // The same address typed by a guest is not stopped by the member's count, whatever the guest's address.
        Assert.Null(await ExceededAsync(PaymentMethod.Card, "192.0.2.78", null));
    }

    [Fact]
    public async Task L8_EmailAxisCountsOnlyAMembersOrders_AGuestOrderIsOnTheIpAxisOnly_KK5()
    {
        for (int i = 0; i < OpenUnpaidOrderLimit.Ceiling; i++)
        {
            await SeedAsync(isGuest: true, ip: $"198.51.100.{i + 1}", email: "ali@example.com");
        }

        Assert.Null(await ExceededAsync(PaymentMethod.BankTransfer, "192.0.2.77", "ali@example.com"));

        for (int i = 0; i < OpenUnpaidOrderLimit.Ceiling; i++)
        {
            await SeedAsync(isGuest: false, ip: $"198.51.101.{i + 1}", email: "veli@example.com");
        }

        Assert.Equal(AttemptLimit.L8, await ExceededAsync(PaymentMethod.BankTransfer, "192.0.2.77", "veli@example.com"));
    }

    // ---- KK1, KK5: two axes, each on its own; whichever is full stops the order ----

    [Fact]
    public async Task TheTwoAxesAreSeparate_TheIpAxisStopsWithoutTheEmailAxis_AndTheEmailAxisWithoutTheIpAxis_KK1()
    {
        // IP axis full: three open orders of three different members from one address.
        for (int i = 0; i < OpenUnpaidOrderLimit.Ceiling; i++)
        {
            await SeedAsync(isGuest: false, ip: Ip, email: $"kisi{i}@example.com");
        }

        Assert.Equal(AttemptLimit.L8, await ExceededAsync(PaymentMethod.BankTransfer, Ip, "yeni@example.com"));
        Assert.Equal(AttemptLimit.L8, await ExceededAsync(PaymentMethod.BankTransfer, Ip, null));
        Assert.Null(await ExceededAsync(PaymentMethod.BankTransfer, "203.0.113.99", "yeni@example.com"));

        // E-mail axis full: three open orders of one member from three different addresses.
        for (int i = 0; i < OpenUnpaidOrderLimit.Ceiling; i++)
        {
            await SeedAsync(isGuest: false, ip: $"198.51.100.{i + 1}", email: "tek@example.com");
        }

        Assert.Equal(AttemptLimit.L8, await ExceededAsync(PaymentMethod.BankTransfer, "203.0.113.99", "tek@example.com"));
        Assert.Null(await ExceededAsync(PaymentMethod.BankTransfer, "198.51.100.1", "baska@example.com"));
        Assert.Null(await ExceededAsync(PaymentMethod.BankTransfer, "203.0.113.99", null));
    }

    // ---- KK5: L-8 closes the bank transfer only ----

    [Fact]
    public async Task AFullOpenCeiling_ClosesTheBankTransferOnly_AndTheCardStaysOpen_UntilAnOpenOrderIsPaidOrCancelled_KK5()
    {
        Guid first = (await SeedAsync(isGuest: false, ip: Ip)).Id;
        for (int i = 1; i < OpenUnpaidOrderLimit.Ceiling; i++)
        {
            await SeedAsync(isGuest: false, ip: Ip);
        }

        Assert.Equal(AttemptLimit.L8, await ExceededAsync(PaymentMethod.BankTransfer, Ip, Email));
        Assert.Null(await ExceededAsync(PaymentMethod.Card, Ip, Email));
        Assert.Null(await ExceededAsync(PaymentMethod.Card, Ip, null));

        await ExecuteAsync($"UPDATE orders SET payment_status = '{PaymentStatus.Paid}' WHERE id = '{first}'");

        Assert.Null(await ExceededAsync(PaymentMethod.BankTransfer, Ip, Email));
    }

    [Fact]
    public async Task AnOpenCardOrder_CountsToTheCeiling_ButNeverStopsTheCard_KK5()
    {
        for (int i = 0; i < OpenUnpaidOrderLimit.Ceiling; i++)
        {
            await SeedAsync(isGuest: true, ip: Ip, method: PaymentMethod.Card);
        }

        await using AppDbContext db = NewContext();
        Assert.Equal(OpenUnpaidOrderLimit.Ceiling, await new OpenUnpaidOrderLimit(db).CountByIpAsync(IPAddress.Parse(Ip), Ct));
        Assert.Equal(AttemptLimit.L8, await ExceededAsync(PaymentMethod.BankTransfer, Ip, null));
        Assert.Null(await ExceededAsync(PaymentMethod.Card, Ip, null));
    }

    [Fact]
    public async Task L7_IsAskedBeforeL8_AndStopsTheCardToo_KK5()
    {
        for (int i = 0; i < OpenUnpaidOrderLimit.Ceiling; i++)
        {
            await SeedAsync(isGuest: true, ip: Ip);
        }

        for (int i = 0; i < UnpaidCancellationLimit.Max; i++)
        {
            await CancelAsync(await SeedAsync(isGuest: true, ip: Ip), ActorKind.System, TransitionReason.CardPaymentFailed);
        }

        Assert.Equal(AttemptLimit.L7, await ExceededAsync(PaymentMethod.BankTransfer, Ip, null));
        Assert.Equal(AttemptLimit.L7, await ExceededAsync(PaymentMethod.Card, Ip, null));
    }

    [Fact]
    public async Task NoAddressAndNoMember_CountsNothing_AndStopsNothing_KK5()
    {
        await using AppDbContext db = NewContext();
        OrderPlacementLimits limits = new(new UnpaidCancellationLimit(db, _clock), new OpenUnpaidOrderLimit(db));

        Assert.Null(await limits.FindExceededAsync(PaymentMethod.BankTransfer, null, null, Ct));
        Assert.Equal(0, await new UnpaidCancellationLimit(db, _clock).CountByIpAsync(null, Ct));
    }

    // ---- KK6: the gate takes no account type; the administrator is under the same numbers ----

    [Fact]
    public void TheGateAsksForNoAccountType_SoNoAccountCanBeExempt_KK6()
    {
        System.Reflection.MethodInfo method = typeof(OrderPlacementLimits).GetMethod(nameof(OrderPlacementLimits.FindExceededAsync))!;

        Assert.Equal(
            [typeof(PaymentMethod), typeof(IPAddress), typeof(string), typeof(CancellationToken)],
            method.GetParameters().Select(p => p.ParameterType));
        Assert.Equal(UnpaidCancellationLimit.Max, AttemptLimits.OrderCountedOf(AttemptLimit.L7).Max);
        Assert.Equal(OpenUnpaidOrderLimit.Ceiling, AttemptLimits.OrderCountedOf(AttemptLimit.L8).Max);
    }

    // ---- helpers ----

    private static TimeSpan L7Window() => AttemptLimits.OrderCountedOf(AttemptLimit.L7).Window!.Value;

    private async Task<int> CountByIpAsync(string address)
    {
        await using AppDbContext db = NewContext();
        return await new UnpaidCancellationLimit(db, _clock).CountByIpAsync(IPAddress.Parse(address), Ct);
    }

    private async Task<int> CountByEmailAsync(string email)
    {
        await using AppDbContext db = NewContext();
        return await new UnpaidCancellationLimit(db, _clock).CountByEmailAsync(email, Ct);
    }

    private async Task<AttemptLimit?> ExceededAsync(PaymentMethod method, string address, string? memberEmail)
    {
        await using AppDbContext db = NewContext();
        OrderPlacementLimits limits = new(new UnpaidCancellationLimit(db, _clock), new OpenUnpaidOrderLimit(db));
        return await limits.FindExceededAsync(method, IPAddress.Parse(address), memberEmail, Ct);
    }

    // The system's (or the customer's, or the firm's) cancellation of an unpaid order: the order's payment fails and the transition record names who and why.
    private async Task CancelAsync(Order order, ActorKind actor, TransitionReason? reason, DateTimeOffset? at = null)
    {
        await ExecuteAsync($"UPDATE orders SET order_status = '{OrderStatus.Cancelled}', payment_status = '{PaymentStatus.Failed}' WHERE id = '{order.Id}'");
        TransitionRecord record = TransitionRecord.Create(order.Id, TransitionCode.S3, actor, null, null, reason, null, null, null, null, at ?? Start.AddHours(-1));
        await using AppDbContext db = NewContext();
        db.TransitionRecords.Add(record);
        await db.SaveChangesAsync(Ct);
    }

    private async Task<Order> SeedAsync(bool isGuest, string? ip, string email = Email, PaymentMethod method = PaymentMethod.BankTransfer)
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n % 90:D2}", $"İl {n}", Start);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Start, "metin", Start);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Start, "metin", Start);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Start, "metin", Start).WithHosting();
        Order order = Order.Create(
            $"2026-{n + 377000:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", method, isGuest, email, 120m, 0m, Start.AddDays(2), Start.AddDays(-1));
        order.IpAddress = ip is null ? null : IPAddress.Parse(ip);
        order.CartId = EntityIds.New();
        order.BankTransferIban = method == PaymentMethod.BankTransfer ? "TR330006100519786457841326" : null;
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
