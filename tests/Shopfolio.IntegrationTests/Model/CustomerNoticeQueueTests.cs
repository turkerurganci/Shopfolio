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

/// <summary>The collection of the customer notice queue tests (T306): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(CustomerNoticeQueueDefinition.Name)]
public sealed class CustomerNoticeQueueDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "CustomerNoticeQueue";
}

/// <summary>
/// The dispatch rows of the customer notices B-1…B-8 on a real PostgreSQL, written as the application user (T306 criteria 1, 3, 5; KK1, KK3, KK5; 08 2.2.6; 06 4.58):
/// the row is written in the caller's transaction and goes with it, every one of the eight types satisfies the table's order link and related record rule, the row
/// carries the order link and the event's values and no body, the B-1 after a correction carries the first e-mail's row id, B-3 is written once for a window - again
/// for a restarted window, never for a window that has run out - and the body is made from the stored row.
/// </summary>
[Collection(CustomerNoticeQueueDefinition.Name)]
public sealed class CustomerNoticeQueueTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset WindowEnd = new(2026, 11, 10, 21, 0, 0, TimeSpan.Zero);

    private readonly TemplateDatabaseFixture _template;
    private readonly CollectionDatabaseFixture _collection;
    private readonly ManualTimeProvider _time = new(Now);

    private int _orderCounter;

    public CustomerNoticeQueueTests(TemplateDatabaseFixture template, CollectionDatabaseFixture collection)
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

    // ---- the row ----

    [Fact]
    public async Task B1_IsOneQueuedRowToTheOrdersEmail_WithTheOrderLink_NoRelatedRecord_AndNoStoredBody()
    {
        Guid orderId = await SeedOrderAsync();

        int written = await RunAsync(queue => queue.QueueAsync(new OrderReceivedNotice(orderId), "  musteri@example.com ", Ct));

        Assert.Equal(1, written);
        DispatchRecord row = await ReadSingleAsync();
        Assert.Equal(DispatchType.B1, row.DispatchType);
        Assert.Equal("musteri@example.com", row.RecipientEmail);
        Assert.Equal(orderId, row.OrderId);
        Assert.Null(row.RelatedRecordType);
        Assert.Null(row.RelatedRecordId);
        Assert.Null(row.OriginalDispatchId);
        Assert.Null(row.EventValues);
        Assert.Equal(DispatchState.Queued, row.DispatchState);
        Assert.Equal(Now, row.CreatedAt);
        Assert.Equal(0, row.AttemptCount);
    }

    [Fact]
    public async Task TheB1AfterACorrection_IsANewRowToTheNewAddress_CarryingTheFirstRowsId()
    {
        Guid orderId = await SeedOrderAsync();
        await RunAsync(queue => queue.QueueAsync(new OrderReceivedNotice(orderId), "eski@example.com", Ct));
        Guid first = (await ReadAllAsync()).Single().Id;

        await RunAsync(queue => queue.QueueAsync(new OrderReceivedNotice(orderId, first), "yeni@example.com", Ct));

        List<DispatchRecord> rows = await ReadAllAsync();
        Assert.Equal(2, rows.Count);
        DispatchRecord correction = rows.Single(r => r.Id != first);
        Assert.Equal(first, correction.OriginalDispatchId);
        Assert.Equal("yeni@example.com", correction.RecipientEmail);
        Assert.Equal(orderId, correction.OrderId);
    }

    [Fact]
    public async Task EveryOfTheEightTypes_SatisfiesTheTablesOrderLinkAndRelatedRecordRule()
    {
        Guid orderId = await SeedOrderAsync();
        CustomerNotice[] notices =
        [
            new OrderReceivedNotice(orderId),
            new TransferChosenNotice(orderId, WindowEnd),
            new TransferReminderNotice(orderId, WindowEnd),
            new PaymentConfirmedNotice(orderId),
            new ShippedNotice(orderId, "YurticiKargo", null, "YT1", false),
            new DeliveryFailedNotice(orderId),
            new CancelledNotice(orderId, ActorKind.Admin, CancellationReason.OutOfStock, null, null, new CancelledItem("Masa", "Meşe", null, 1)),
            new RefundProcessedNotice(orderId, RefundType.Withdrawal, 120m, PaymentMethod.BankTransfer),
        ];

        foreach (CustomerNotice notice in notices)
        {
            await RunAsync(queue => queue.QueueAsync(notice, "musteri@example.com", Ct));
        }

        List<DispatchRecord> rows = await ReadAllAsync();
        Assert.Equal(
            [DispatchType.B1, DispatchType.B2, DispatchType.B3, DispatchType.B4, DispatchType.B5, DispatchType.B6, DispatchType.B7, DispatchType.B8],
            [.. rows.Select(r => r.DispatchType).Order()]);
        Assert.All(rows, row =>
        {
            Assert.Equal(orderId, row.OrderId);
            Assert.Null(row.RelatedRecordType);
        });
    }

    [Fact]
    public async Task TheRow_GoesWithTheTransaction_WhenTheEventIsRefused()
    {
        Guid orderId = await SeedOrderAsync();

        await RunAsync(queue => queue.QueueAsync(new PaymentConfirmedNotice(orderId), "musteri@example.com", Ct), rejected: true);

        Assert.Equal(0, await CountAsync());
    }

    [Fact]
    public async Task WithoutATransaction_NoRowIsWritten()
    {
        Guid orderId = await SeedOrderAsync();
        await using AppDbContext db = NewContext();
        CustomerNoticeQueue queue = new(db, _time);

        await Assert.ThrowsAsync<InvalidOperationException>(() => queue.QueueAsync(new PaymentConfirmedNotice(orderId), "musteri@example.com", Ct));
        Assert.Equal(0, await CountAsync());
    }

    [Fact]
    public async Task ANoticeThatBreaksItsRule_OrAnEmptyRecipient_WritesNoRow()
    {
        Guid orderId = await SeedOrderAsync();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            RunAsync(queue => queue.QueueAsync(new CancelledNotice(orderId, ActorKind.System, null, null, TransitionReason.Closure, null), "musteri@example.com", Ct)));
        await Assert.ThrowsAnyAsync<ArgumentException>(() => RunAsync(queue => queue.QueueAsync(new PaymentConfirmedNotice(orderId), " ", Ct)));

        Assert.Equal(0, await CountAsync());
    }

    // ---- KK5: B-3 once for a window ----

    [Fact]
    public async Task B3_IsWrittenOnceForAWindow_AndTheSecondCallWritesNothing()
    {
        Guid orderId = await SeedOrderAsync();

        int first = await RunAsync(queue => queue.QueueAsync(new TransferReminderNotice(orderId, WindowEnd), "musteri@example.com", Ct));
        int second = await RunAsync(queue => queue.QueueAsync(new TransferReminderNotice(orderId, WindowEnd), "musteri@example.com", Ct));

        Assert.Equal(1, first);
        Assert.Equal(0, second);
        Assert.Equal(1, await CountAsync());
    }

    [Fact]
    public async Task B3_IsWrittenAgainForAWindowThatRestarted_AfterThePaidMarkWasCorrected()
    {
        Guid orderId = await SeedOrderAsync();
        DateTimeOffset restartedEnd = WindowEnd.AddDays(7);
        await RunAsync(queue => queue.QueueAsync(new TransferReminderNotice(orderId, WindowEnd), "musteri@example.com", Ct));

        int again = await RunAsync(queue => queue.QueueAsync(new TransferReminderNotice(orderId, restartedEnd), "musteri@example.com", Ct));
        int repeated = await RunAsync(queue => queue.QueueAsync(new TransferReminderNotice(orderId, restartedEnd), "musteri@example.com", Ct));

        Assert.Equal(1, again);
        Assert.Equal(0, repeated);
        List<DispatchRecord> rows = await ReadAllAsync();
        Assert.Equal(2, rows.Count);
        Assert.Equal([WindowEnd, restartedEnd], [.. rows.Select(r => CustomerNoticeValues.ReadWindowEnd(r.EventValues)!.Value).Order()]);
    }

    [Fact]
    public async Task B3_IsNotWritten_ForAWindowThatHasRunOut_TheReminderDueInAnOutage()
    {
        Guid orderId = await SeedOrderAsync();
        _time.SetUtcNow(WindowEnd.AddMinutes(1));

        int written = await RunAsync(queue => queue.QueueAsync(new TransferReminderNotice(orderId, WindowEnd), "musteri@example.com", Ct));
        int stillRunning = await RunAsync(queue => queue.QueueAsync(new TransferReminderNotice(orderId, WindowEnd.AddDays(3)), "musteri@example.com", Ct));

        Assert.Equal(0, written);
        Assert.Equal(1, stillRunning);
        Assert.Equal(1, await CountAsync());
    }

    [Fact]
    public async Task B3_OfAnotherOrder_IsItsOwn_AndB2OfTheSameOrderDoesNotCountAsAReminder()
    {
        Guid one = await SeedOrderAsync();
        Guid two = await SeedOrderAsync();
        await RunAsync(queue => queue.QueueAsync(new TransferChosenNotice(one, WindowEnd), "bir@example.com", Ct));

        int first = await RunAsync(queue => queue.QueueAsync(new TransferReminderNotice(one, WindowEnd), "bir@example.com", Ct));
        int other = await RunAsync(queue => queue.QueueAsync(new TransferReminderNotice(two, WindowEnd), "iki@example.com", Ct));

        Assert.Equal(1, first);
        Assert.Equal(1, other);
        Assert.Equal(3, await CountAsync());
    }

    // ---- the body is made from the stored row ----

    [Fact]
    public async Task TheBody_IsMadeFromTheStoredRow_TheSameAsFromTheNotice()
    {
        Guid orderId = await SeedOrderAsync();
        CustomerNotice[] notices =
        [
            new ShippedNotice(orderId, "ArasKargo", null, "A-9", false),
            new CancelledNotice(orderId, ActorKind.Admin, CancellationReason.Other, "açıklama", null, new CancelledItem("Masa", "Meşe", "120 cm", 2)),
            new RefundProcessedNotice(orderId, RefundType.AmountBased, 266.67m, PaymentMethod.Card),
            new TransferChosenNotice(orderId, WindowEnd),
        ];
        foreach (CustomerNotice notice in notices)
        {
            await RunAsync(queue => queue.QueueAsync(notice, "musteri@example.com", Ct));
        }

        CustomerNoticeTemplates templates = new(Links(), CarrierCatalog.Default);
        OrderNoticeContext context = new("2026-ABC123", "anahtar", true, false, "TR330006100519786457841326", false, "form", "sözleşme", "firma@example.com", "3121234567", null);
        List<DispatchRecord> rows = await ReadAllAsync();
        Assert.Equal(notices.Length, rows.Count);
        foreach (CustomerNotice notice in notices)
        {
            DispatchRecord row = rows.Single(r => r.DispatchType == CustomerNoticeValues.For(notice).Type);
            CustomerNotice read = CustomerNoticeValues.Read(row.DispatchType, row.OrderId!.Value, row.EventValues)
                ?? throw new InvalidOperationException("The stored row did not read back as a notice.");

            Assert.Equal(await ComposeAsync(templates.Compose(context, notice)), await ComposeAsync(templates.Compose(context, read)));
        }
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

    private async Task<DispatchRecord> ReadSingleAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.DispatchRecords.AsNoTracking().SingleAsync(Ct);
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
