using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

/// <summary>
/// The dispatch rows of the customer notices B-9…B-16 on a real PostgreSQL, written as the application user (T307 criteria 1, 8, 10; 08 2.2.6; 06 4.58): every one of the
/// eight types satisfies the table's order link and related record rule (B-9 names its withdrawal, termination or claim as a value, the other seven name none), the
/// row carries the order link and the event's values and no body, B-15 goes to the address it is given (the old one), the row goes with the transaction of the event, and
/// the body is made from the stored row the same as from the notice. Shares the database of the B-1…B-8 tests.
/// </summary>
[Collection(CustomerNoticeQueueDefinition.Name)]
public sealed class CustomerNoticeAfterOrderQueueTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset WindowEnd = new(2026, 11, 10, 21, 0, 0, TimeSpan.Zero);
    private static readonly Guid RecordId = Guid.Parse("018f0000-0000-7000-8000-0000000000b2");
    private static readonly Guid ProvinceId = Guid.Parse("018f0000-0000-7000-8000-0000000000c3");
    private static readonly NoticeItem Table = new("Ahşap Masa", "Meşe", "120 cm", 2);
    private static readonly NoticeAddress Address = new("Ali Veli", "Yeni Sok. 7", "Konak", ProvinceId, "5321234567");

    private readonly TemplateDatabaseFixture _template;
    private readonly CollectionDatabaseFixture _collection;
    private readonly ManualTimeProvider _time = new(Now);

    private int _orderCounter;

    public CustomerNoticeAfterOrderQueueTests(TemplateDatabaseFixture template, CollectionDatabaseFixture collection)
    {
        _template = template;
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private string AppDsn => _template.DsnFor(_collection.DatabaseName, _template.Users.App, _template.Users.AppPassword, pooling: false);

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE dispatch_records, order_items, orders, legal_text_versions, provinces CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static CustomerNotice[] AllNine(Guid orderId) =>
    [
        new DeclarationReceivedNotice(orderId, DispatchRelatedRecordType.Withdrawal, RecordId, false, new DateOnly(2026, 11, 4), null, [Table], Address with { Phone = null }),
        new DeclarationReceivedNotice(orderId, DispatchRelatedRecordType.LateDeliveryTermination, RecordId, false, new DateOnly(2026, 11, 4), null, [Table], null),
        new DeclarationReceivedNotice(orderId, DispatchRelatedRecordType.DefectClaim, RecordId, false, null, Now, [Table], Address),
        new DeclarationReceivedNotice(orderId, DispatchRelatedRecordType.DefectClaim, RecordId, true, null, Now, [Table], null),
        new AddressCorrectedNotice(orderId, AddressType.ShippingAddress, Address),
        new ItemRemovedNotice(orderId, Table),
        new CarrierCorrectedNotice(orderId, "YurticiKargo", null, "YT1", false),
        new StatusCorrectedNotice(orderId, OrderStatus.Placed, WindowEnd),
        new RefundIbanRequestedNotice(orderId, RefundIbanRequestReason.TransferFailed),
    ];

    [Fact]
    public async Task EveryOfTheEightTypes_SatisfiesTheTablesOrderLinkAndRelatedRecordRule()
    {
        Guid orderId = await SeedOrderAsync();
        CustomerNotice[] notices =
        [
            .. AllNine(orderId),
            new OrderEmailChangedNotice(orderId, Now),
            new ReturnRejectedNotice(orderId, Table, WithdrawalExemptionReason.HygieneSensitive),
        ];

        foreach (CustomerNotice notice in notices)
        {
            await RunAsync(queue => queue.QueueAsync(notice, "musteri@example.com", Ct));
        }

        List<DispatchRecord> rows = await ReadAllAsync();
        Assert.Equal(notices.Length, rows.Count);
        Assert.Equal(
            [DispatchType.B9, DispatchType.B10, DispatchType.B11, DispatchType.B12, DispatchType.B13, DispatchType.B14, DispatchType.B15, DispatchType.B16],
            [.. rows.Select(r => r.DispatchType).Distinct().Order()]);
        Assert.All(rows, row =>
        {
            Assert.Equal(orderId, row.OrderId);
            Assert.Null(row.OriginalDispatchId);
            Assert.Equal(DispatchState.Queued, row.DispatchState);
            Assert.NotNull(row.EventValues);
        });
    }

    [Fact]
    public async Task B9_NamesItsRecord_AsAValue_ByTheKindOfTheDeclaration()
    {
        Guid orderId = await SeedOrderAsync();
        foreach (CustomerNotice notice in AllNine(orderId).Take(4))
        {
            await RunAsync(queue => queue.QueueAsync(notice, "musteri@example.com", Ct));
        }

        List<DispatchRecord> rows = await ReadAllAsync();
        Assert.Equal(4, rows.Count);
        Assert.All(rows, row =>
        {
            Assert.Equal(DispatchType.B9, row.DispatchType);
            Assert.Equal(RecordId, row.RelatedRecordId);
        });
        Assert.Equal(
            [DispatchRelatedRecordType.Withdrawal, DispatchRelatedRecordType.LateDeliveryTermination, DispatchRelatedRecordType.DefectClaim, DispatchRelatedRecordType.DefectClaim],
            [.. rows.Select(r => r.RelatedRecordType!.Value).OrderBy(t => t)]);
    }

    [Fact]
    public async Task TheOtherSevenTypes_WriteNoRelatedRecord()
    {
        Guid orderId = await SeedOrderAsync();
        foreach (CustomerNotice notice in AllNine(orderId).Skip(4))
        {
            await RunAsync(queue => queue.QueueAsync(notice, "musteri@example.com", Ct));
        }

        List<DispatchRecord> rows = await ReadAllAsync();
        Assert.Equal(5, rows.Count);
        Assert.All(rows, row =>
        {
            Assert.Null(row.RelatedRecordType);
            Assert.Null(row.RelatedRecordId);
        });
    }

    [Fact]
    public async Task B15_GoesToTheAddressItIsGiven_TheOldOne_AndB1OfTheCorrectionToTheNewOne()
    {
        Guid orderId = await SeedOrderAsync();

        await RunAsync(queue => queue.QueueAsync(new OrderEmailChangedNotice(orderId, Now), "eski@example.com", Ct));
        await RunAsync(queue => queue.QueueAsync(new OrderReceivedNotice(orderId), "yeni@example.com", Ct));

        List<DispatchRecord> rows = await ReadAllAsync();
        Assert.Equal("eski@example.com", rows.Single(r => r.DispatchType == DispatchType.B15).RecipientEmail);
        Assert.Equal("yeni@example.com", rows.Single(r => r.DispatchType == DispatchType.B1).RecipientEmail);
        string values = rows.Single(r => r.DispatchType == DispatchType.B15).EventValues!;
        Assert.DoesNotContain("yeni@example.com", values, StringComparison.Ordinal);
        Assert.DoesNotContain("eski@example.com", values, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ASecondEventOfTheSameType_IsANewRow_NeverMergedOrDeduplicated()
    {
        Guid orderId = await SeedOrderAsync();
        CarrierCorrectedNotice first = new(orderId, "YurticiKargo", null, "YT1", false);
        CarrierCorrectedNotice second = new(orderId, "ArasKargo", null, "AR2", false);

        await RunAsync(queue => queue.QueueAsync(first, "musteri@example.com", Ct));
        await RunAsync(queue => queue.QueueAsync(second, "musteri@example.com", Ct));
        await RunAsync(queue => queue.QueueAsync(second, "musteri@example.com", Ct));

        List<DispatchRecord> rows = await ReadAllAsync();
        Assert.Equal(3, rows.Count);
        Assert.All(rows, row => Assert.Equal(DispatchType.B12, row.DispatchType));
        Assert.Contains("YT1", rows[0].EventValues, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheRow_GoesWithTheTransaction_WhenTheEventIsRefused()
    {
        Guid orderId = await SeedOrderAsync();

        await RunAsync(queue => queue.QueueAsync(AllNine(orderId)[0], "musteri@example.com", Ct), rejected: true);

        Assert.Equal(0, await CountAsync());
    }

    [Fact]
    public async Task WithoutATransaction_AndForANoticeThatBreaksItsRule_NoRowIsWritten()
    {
        Guid orderId = await SeedOrderAsync();
        await using (AppDbContext db = NewContext())
        {
            CustomerNoticeQueue queue = new(db, _time);
            await Assert.ThrowsAsync<InvalidOperationException>(() => queue.QueueAsync(AllNine(orderId)[0], "musteri@example.com", Ct));
        }

        await Assert.ThrowsAsync<ArgumentException>(() =>
            RunAsync(queue => queue.QueueAsync(new ReturnRejectedNotice(orderId, Table, WithdrawalExemptionReason.Perishable), "musteri@example.com", Ct)));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            RunAsync(queue => queue.QueueAsync(new DeclarationReceivedNotice(orderId, DispatchRelatedRecordType.Withdrawal, Guid.Empty, false, new DateOnly(2026, 11, 4), null, [Table], null), "musteri@example.com", Ct)));

        Assert.Equal(0, await CountAsync());
    }

    [Fact]
    public async Task TheBody_IsMadeFromTheStoredRow_TheSameAsFromTheNotice()
    {
        Guid orderId = await SeedOrderAsync();
        CustomerNotice[] notices =
        [
            .. AllNine(orderId),
            new OrderEmailChangedNotice(orderId, Now),
            new ReturnRejectedNotice(orderId, Table, WithdrawalExemptionReason.SealedMedia),
        ];
        foreach (CustomerNotice notice in notices)
        {
            await RunAsync(queue => queue.QueueAsync(notice, "musteri@example.com", Ct));
        }

        CustomerNoticeTemplates templates = new(Links(), CarrierCatalog.Default);
        Dictionary<Guid, string> provinces = new() { [ProvinceId] = "İzmir" };
        OrderNoticeContext context = new(
            "2026-ABC123", "anahtar", true, false, "TR330006100519786457841326", false, "form", "sözleşme", "firma@example.com", "3121234567", null, provinces);
        List<DispatchRecord> rows = await ReadAllAsync();
        Assert.Equal(notices.Length, rows.Count);

        // The jsonb column keeps the list in its own spelling, so the rows are not paired by their text: the bodies made from the rows are the bodies made from the notices.
        List<(string Subject, string Html, string Text)> fromNotices = [];
        foreach (CustomerNotice notice in notices)
        {
            fromNotices.Add(await ComposeAsync(templates.Compose(context, notice)));
        }

        List<(string Subject, string Html, string Text)> fromRows = [];
        foreach (DispatchRecord row in rows)
        {
            CustomerNotice read = CustomerNoticeValues.Read(row.DispatchType, row.OrderId!.Value, row.EventValues, row.RelatedRecordId)
                ?? throw new InvalidOperationException("The stored row did not read back as a notice.");
            fromRows.Add(await ComposeAsync(templates.Compose(context, read)));
        }

        Assert.Equal(
            [.. fromNotices.OrderBy(b => b.Text, StringComparer.Ordinal)],
            [.. fromRows.OrderBy(b => b.Text, StringComparer.Ordinal)]);
    }

    // ---- helpers ----

    private static EmailLinks Links() => new(new SiteAddress(Options.Create(new SiteOptions { Domain = "magaza.example.com" })));

    private static async Task<(string Subject, string Html, string Text)> ComposeAsync(EmailContent content)
    {
        using ServiceProvider services = new ServiceCollection().AddLogging().BuildServiceProvider();
        IOptions<SiteOptions> site = Options.Create(new SiteOptions { Domain = "magaza.example.com" });
        EmailComposer composer = new(Links(), site, services, services.GetRequiredService<ILoggerFactory>());
        EmailMessage message = await composer.ComposeAsync(
            "musteri@example.com",
            new EmailBranding("Örnek Mağaza", null, null),
            new EmailFirm("Örnek Ticaret", "firma@example.com"),
            content,
            Ct);
        return (message.Subject, message.HtmlBody, message.TextBody);
    }

    private static TransactionRunner NewRunner(AppDbContext db) => new(db, new JobEnqueueTracker(), new JobSignal(), new CacheInvalidations(db), new AppCache());

    private async Task<int> RunAsync(Func<CustomerNoticeQueue, Task<int>> work, bool rejected = false)
    {
        await using AppDbContext db = NewContext();
        CustomerNoticeQueue queue = new(db, _time);
        int written = 0;
        await NewRunner(db).RunAsync(
            async cancel =>
            {
                written = await work(queue);
                return new Outcome(rejected);
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

    private async Task<int> CountAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.DispatchRecords.CountAsync(Ct);
    }

    /// <summary>An order of the real model: the dispatch row's order link is a foreign key to it.</summary>
    private async Task<Guid> SeedOrderAsync()
    {
        await using AppDbContext db = NewOwnerContext();
        Province? province = await db.Provinces.SingleOrDefaultAsync(p => p.Code == "06", Ct);
        LegalTextVersion form;
        LegalTextVersion contract;
        LegalTextVersion privacy;
        if (province is null)
        {
            province = Province.Create("06", "Ankara", Now);
            form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, 1, Now, "metin", Now);
            contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, 1, Now, "metin", Now);
            privacy = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, 1, Now, "metin", Now).WithHosting();
            db.Provinces.Add(province);
            db.LegalTextVersions.AddRange(form, contract, privacy);
            await db.SaveChangesAsync(Ct);
        }
        else
        {
            form = await db.LegalTextVersions.SingleAsync(v => v.LegalTextType == LegalTextType.PreInformationForm, Ct);
            contract = await db.LegalTextVersions.SingleAsync(v => v.LegalTextType == LegalTextType.DistanceSalesContract, Ct);
            privacy = await db.LegalTextVersions.SingleAsync(v => v.LegalTextType == LegalTextType.PrivacyNotice, Ct);
        }

        int n = Interlocked.Increment(ref _orderCounter);
        Order order = Order.Create(
            $"2026-{n:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", PaymentMethod.Card, true, "Ali@Example.com", 120m, 0m, Now.AddHours(1), Now);
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

    private sealed record Outcome(bool Rejected) : IOperationResult
    {
        public bool IsRejected => Rejected;
    }
}
