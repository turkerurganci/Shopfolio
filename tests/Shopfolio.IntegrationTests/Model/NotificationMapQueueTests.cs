using System.Net;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.EmailComposition;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.SearchEngine;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the notification map queue tests (T328): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(NotificationMapQueueDefinition.Name)]
public sealed class NotificationMapQueueDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "NotificationMapQueue";
}

/// <summary>
/// The notification map against the real queues on a real PostgreSQL, written as the application user (T328 criteria 1, 3; KK1, KK6; 03 7.1; 02 9.3.1, 9.5): every type a
/// queue writes has a trigger row in the map and belongs to the path it was queued through, and the address of the stored row is the one the map names for the type -
/// the order's contact e-mail for the customer notices (B-15 the address before the correction), the firm's contact e-mail for F-1…F-4, each administrator's own address
/// for F-5 and F-6. The account e-mails are written by <c>AccountEmailQueueTests</c>.
/// </summary>
[Collection(NotificationMapQueueDefinition.Name)]
public sealed class NotificationMapQueueTests : IAsyncLifetime
{
    private const string OrderEmail = "musteri@example.com";
    private const string OldOrderEmail = "eski@example.com";
    private const string FirmEmail = "firma@example.com";
    private const string WholeIban = "TR330006100519786457841326";

    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset WindowEnd = new(2026, 11, 10, 21, 0, 0, TimeSpan.Zero);
    private static readonly Guid RecordId = Guid.Parse("018f0000-0000-7000-8000-0000000000b2");
    private static readonly Guid ProvinceId = Guid.Parse("018f0000-0000-7000-8000-0000000000c3");
    private static readonly NoticeItem Table = new("Ahşap Masa", "Meşe", "120 cm", 2);
    private static readonly NoticeAddress Address = new("Ali Veli", "Yeni Sok. 7", "Konak", ProvinceId, "5321234567");

    private readonly TemplateDatabaseFixture _template;
    private readonly CollectionDatabaseFixture _collection;
    private readonly ManualTimeProvider _time = new(Now);

    public NotificationMapQueueTests(TemplateDatabaseFixture template, CollectionDatabaseFixture collection)
    {
        _template = template;
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private string AppDsn => _template.DsnFor(_collection.DatabaseName, _template.Users.App, _template.Users.AppPassword, pooling: false);

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE dispatch_records, order_items, orders, legal_text_versions, provinces, admins CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task TheCustomerNotices_AreWrittenToTheAddressTheMapNames_AndEveryTypeIsOfTheCustomerPath()
    {
        Guid orderId = await SeedOrderAsync();
        CustomerNotice[] notices =
        [
            new DeclarationReceivedNotice(orderId, DispatchRelatedRecordType.Withdrawal, RecordId, false, new DateOnly(2026, 11, 4), null, [Table], null),
            new DeclarationReceivedNotice(orderId, DispatchRelatedRecordType.DefectClaim, RecordId, true, null, Now, [Table], null),
            new AddressCorrectedNotice(orderId, AddressType.ShippingAddress, Address),
            new ItemRemovedNotice(orderId, Table),
            new CarrierCorrectedNotice(orderId, "YurticiKargo", null, "YT1", false),
            new StatusCorrectedNotice(orderId, OrderStatus.Placed, WindowEnd),
            new RefundIbanRequestedNotice(orderId, RefundIbanRequestReason.TransferFailed),
            new OrderEmailChangedNotice(orderId, Now),
            new ReturnRejectedNotice(orderId, Table, WithdrawalExemptionReason.HygieneSensitive),
        ];

        foreach (CustomerNotice notice in notices)
        {
            // B-15 goes to the address the order had before the correction; the queue is given that address by the caller.
            bool toTheOldAddress = notice is OrderEmailChangedNotice;
            await RunAsync(db => NewCustomerQueue(db).QueueAsync(notice, toTheOldAddress ? OldOrderEmail : OrderEmail, Ct));
        }

        List<DispatchRecord> rows = await ReadAllAsync();
        Assert.Equal(notices.Length, rows.Count);
        Assert.All(rows, row =>
        {
            Assert.Equal(NoticeFamily.Customer, NotificationMap.FamilyOf(row.DispatchType));
            IReadOnlyList<NoticeAddressee> addressees = NotificationMap.AddresseesOf(row.DispatchType);
            Assert.NotEmpty(addressees);
            bool oldAddress = addressees.Contains(NoticeAddressee.OrderContactBeforeCorrection);
            Assert.Equal(oldAddress ? OldOrderEmail : OrderEmail, row.RecipientEmail);
        });
        Assert.Equal(DispatchType.B15, Assert.Single(rows, r => r.RecipientEmail == OldOrderEmail).DispatchType);
    }

    [Fact]
    public async Task TheFirmNoticesF1ToF4_AreWrittenToTheFirmContactEmail()
    {
        Guid orderId = Guid.Parse("018f0000-0000-7000-8000-0000000000a1");
        CompanyNotice[] notices =
        [
            new NewOrderNotice(orderId, "SP-1", PaymentMethod.Card, 10m, Now),
            new NewContactRequestNotice(RecordId, ContactTopic.Other, Now),
            new NewClaimNotice(DispatchRelatedRecordType.Withdrawal, RecordId, orderId, "SP-1", false, Now),
            new CardRefundNotice(RecordId, orderId, "SP-1", RefundType.LatePayment, 10m, Now),
        ];

        foreach (CompanyNotice notice in notices)
        {
            await RunAsync(db => NewCompanyQueue(db).QueueCompanyAsync(notice, Ct));
        }

        List<DispatchRecord> rows = await ReadAllAsync();
        Assert.Equal([DispatchType.F1, DispatchType.F2, DispatchType.F3, DispatchType.F4], [.. rows.Select(r => r.DispatchType).Order()]);
        Assert.All(rows, row =>
        {
            Assert.Equal(NoticeFamily.Firm, NotificationMap.FamilyOf(row.DispatchType));
            Assert.Equal([NoticeAddressee.FirmContact], NotificationMap.AddresseesOf(row.DispatchType));
            Assert.Equal(FirmEmail, row.RecipientEmail);
        });
    }

    [Fact]
    public async Task TheAdministratorNoticesF5AndF6_AreWrittenToEachAdministratorsOwnAddress()
    {
        string[] admins = ["ali@example.com", "veli@example.com"];

        await RunAsync(db => Task.FromResult(NewCompanyQueue(db).QueueAdmins(IbanChangedNotice.For(WholeIban, "Ali Yılmaz", false, Now), admins)));
        await RunAsync(db => Task.FromResult(NewCompanyQueue(db).QueueAdmins(new AdminRemovedNotice(RecordId, "Ali", "Veli", Now), admins)));

        List<DispatchRecord> rows = await ReadAllAsync();
        Assert.Equal(admins.Length * 2, rows.Count);
        Assert.Equal([DispatchType.F5, DispatchType.F6], [.. rows.Select(r => r.DispatchType).Distinct().Order()]);
        Assert.All(rows, row =>
        {
            Assert.Equal(NoticeFamily.Administrators, NotificationMap.FamilyOf(row.DispatchType));
            Assert.Equal([NoticeAddressee.EachAdministrator], NotificationMap.AddresseesOf(row.DispatchType));
            Assert.Contains(row.RecipientEmail, admins);
        });
        Assert.All(
            rows.GroupBy(r => r.DispatchType),
            group => Assert.Equal(admins.Order(StringComparer.Ordinal), group.Select(r => r.RecipientEmail!).Order(StringComparer.Ordinal)));
    }

    private CustomerNoticeQueue NewCustomerQueue(AppDbContext db) => new(db, _time);

    private CompanyNoticeQueue NewCompanyQueue(AppDbContext db) => new(db, new FixedContactEmail(FirmEmail), _time);

    private static TransactionRunner NewRunner(AppDbContext db) => new(db, new JobEnqueueTracker(), new JobSignal(), new CacheInvalidations(db), new AppCache());

    private async Task<int> RunAsync(Func<AppDbContext, Task<int>> work)
    {
        await using AppDbContext db = NewContext();
        int written = 0;
        await NewRunner(db).RunAsync(
            async cancel =>
            {
                written = await work(db);
                return new Outcome(false);
            },
            Ct);
        return written;
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, AppDsn);
        return new AppDbContext(builder.Options);
    }

    private AppDbContext NewOwnerContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task<List<DispatchRecord>> ReadAllAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.DispatchRecords.AsNoTracking().OrderBy(d => d.CreatedAt).ThenBy(d => d.Id).ToListAsync(Ct);
    }

    /// <summary>An order of the real model: the dispatch row's order link is a foreign key to it.</summary>
    private async Task<Guid> SeedOrderAsync()
    {
        await using AppDbContext db = NewOwnerContext();
        Province province = Province.Create("06", "Ankara", Now);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, 1, Now, "metin", Now);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, 1, Now, "metin", Now);
        LegalTextVersion privacy = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, 1, Now, "metin", Now).WithHosting();
        db.Provinces.Add(province);
        db.LegalTextVersions.AddRange(form, contract, privacy);
        await db.SaveChangesAsync(Ct);

        Order order = Order.Create(
            "2026-000001", "access-placeholder-1", "confirm-placeholder-1", PaymentMethod.Card, true, "Ali@Example.com", 120m, 0m, Now.AddHours(1), Now);
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
        order.CompanyEmail = FirmEmail;
        order.CompanyKepAddress = "firma@hs01.kep.tr";
        order.PreInformationFormVersionId = form.Id;
        order.DistanceSalesContractVersionId = contract.Id;
        order.PrivacyNoticeVersionId = privacy.Id;
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
        db.Orders.Add(order);
        await db.SaveChangesAsync(Ct);
        return order.Id;
    }

    private async Task ExecuteAsync(string sql)
    {
        await using Npgsql.NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using Npgsql.NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private sealed class FixedContactEmail : ICompanyContactEmail
    {
        private readonly string? _email;

        public FixedContactEmail(string? email) => _email = email;

        public Task<string?> GetAsync(CancellationToken ct) => Task.FromResult(_email);
    }

    private sealed record Outcome(bool Rejected) : IOperationResult
    {
        public bool IsRejected => Rejected;
    }
}
