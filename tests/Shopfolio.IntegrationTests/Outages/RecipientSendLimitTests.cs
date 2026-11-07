using System.Net;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Outages;

/// <summary>The collection of the per-recipient sending limit tests (T304): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(RecipientSendLimitDefinition.Name)]
public sealed class RecipientSendLimitDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "RecipientSendLimit";
}

/// <summary>
/// The per-recipient sending limit on a real PostgreSQL (T304 criteria 1-9; KK1-KK9; 08 2.2.4; 02 9.1.2, 9.1.6; 06 4.58): the counter is read from
/// the dispatch record, never stored; the twenty-first customer notification of an address in the 24 hours before its own moment is written
/// "undelivered" with the recipient error at its first attempt, the fake service is not called and the channel state and the order are not
/// touched; the order's "did not arrive" mark is read from the row; a resending is exempt but counted; a retry does not ask again; the thinned
/// security line carries the count and no address. The clocks are fake clocks; the rows are written at the moments the test chooses.
/// </summary>
[Collection(RecipientSendLimitDefinition.Name)]
public sealed class RecipientSendLimitTests : IAsyncLifetime
{
    private const string Ali = "ali@example.com";

    private static readonly DateTimeOffset Now = new(2026, 11, 6, 12, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    private Guid _order;

    private Guid _otherOrder;

    public RecipientSendLimitTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE dispatch_records, contact_requests, order_items, orders, legal_text_versions, provinces CASCADE");
        (Guid first, Guid second) = await SeedOrdersAsync();
        _order = first;
        _otherOrder = second;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: only the customer notices are limited and counted ----

    [Fact]
    public async Task FirmAndAccountEmails_AreNotLimited_AndNotCounted()
    {
        await AddEarlierAsync(RecipientSendLimit.Threshold - 1, Ali);
        Guid firm = await AddAsync(DispatchType.F1, Ali, Now.AddMinutes(-1), orderId: null, related: DispatchRelatedRecordType.Order);
        Guid reset = await AddAsync(DispatchType.PasswordResetLink, Ali, Now.AddMinutes(-1), orderId: null, related: DispatchRelatedRecordType.Customer);
        for (int i = 0; i < 5; i++)
        {
            await AddAsync(DispatchType.F5, Ali, Now.AddMinutes(-2), orderId: null);
        }

        Assert.Equal(RecipientLimitVerdict.NotLimited, await AskAsync(firm));
        Assert.Equal(RecipientLimitVerdict.NotLimited, await AskAsync(reset));
        Guid customer = await AddAsync(DispatchType.B2, Ali, Now, _order);

        Assert.Equal(RecipientLimitVerdict.Allowed, await AskAsync(customer));
        Assert.Equal(DispatchState.Queued, (await RowAsync(firm)).DispatchState);
    }

    // ---- KK2: the counter is read from the record, not stored ----

    [Fact]
    public async Task TheCounter_IsReadFromTheRecord_SoRemovingARowChangesTheAnswerWithNothingElseToReset()
    {
        List<Guid> earlier = await AddEarlierAsync(RecipientSendLimit.Threshold, Ali);
        Guid over = await AddAsync(DispatchType.B1, Ali, Now, _order);
        Assert.Equal(RecipientLimitVerdict.Refused, await AskAsync(over));

        // Two earlier rows are removed: the refused row is still counted, so eighteen and it make nineteen, and the next row passes.
        await ExecuteAsync($"DELETE FROM dispatch_records WHERE id IN ('{earlier[0]}', '{earlier[1]}')");
        Guid again = await AddAsync(DispatchType.B1, Ali, Now.AddSeconds(1), _order);

        Assert.Equal(RecipientLimitVerdict.Allowed, await AskAsync(again));
        Assert.Empty(await NamesAsync("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND (table_name ILIKE '%limit%' OR table_name ILIKE '%counter%')"));
        Assert.Empty(await NamesAsync("SELECT column_name FROM information_schema.columns WHERE table_name IN ('dispatch_records', 'channel_state_records') AND (column_name ILIKE '%limit%' OR column_name ILIKE '%counter%' OR column_name ILIKE '%sent_count%')"));
    }

    // ---- KK3: the recipient is compared without regard to case ----

    [Fact]
    public async Task AddressesThatDifferInCase_AreOneRecipient_EvenOnTwoOrders()
    {
        for (int i = 0; i < 10; i++)
        {
            await AddAsync(DispatchType.B1, "ALI@Example.com", Now.AddMinutes(-1 - i), _order);
            await AddAsync(DispatchType.B2, "ali@example.com", Now.AddMinutes(-20 - i), _otherOrder);
        }

        Guid twentyFirst = await AddAsync(DispatchType.B3, "ALİ@EXAMPLE.COM", Now, _order);

        Assert.Equal(RecipientLimitVerdict.Refused, await AskAsync(twentyFirst));
    }

    // ---- KK4: the twentieth passes, the twenty-first is over; equal moments are ordered by the id ----

    [Fact]
    public async Task OfTwentyOneRowsMadeAtTheSameMoment_TheOneWithTheLargestIdIsOver_WhateverTheOrderOfAsking()
    {
        Guid[] ids = [.. Enumerable.Range(1, 21).Select(i => new Guid($"00000000-0000-7000-8000-{i:D12}"))];
        foreach (Guid id in ids)
        {
            await AddAsync(DispatchType.B2, Ali, Now, _order, id: id);
        }

        // The last by id is asked first and is over; the rest still pass, though the queue is "full" when they are asked.
        Assert.Equal(RecipientLimitVerdict.Refused, await AskAsync(ids[20]));
        foreach (Guid id in ids.Take(20).Reverse())
        {
            Assert.Equal(RecipientLimitVerdict.Allowed, await AskAsync(id));
        }

        Assert.Equal(DispatchState.Undelivered, (await RowAsync(ids[20])).DispatchState);
        foreach (Guid id in ids.Take(20))
        {
            Assert.Equal(DispatchState.Queued, (await RowAsync(id)).DispatchState);
        }
    }

    // ---- KK5: a refused row is counted; the window slides ----

    [Fact]
    public async Task ARefusedRow_IsCounted_AndTheCountFallsWhenTheWindowSlides()
    {
        await AddEarlierAsync(RecipientSendLimit.Threshold - 1, Ali);
        Guid refusedByCount = await AddAsync(DispatchType.B1, Ali, Now.AddMinutes(-1) + TimeSpan.FromSeconds(30), _order, state: DispatchState.Undelivered, outcome: DispatchOutcome.RecipientError);
        Guid next = await AddAsync(DispatchType.B2, Ali, Now, _order);

        // Nineteen earlier rows and one "undelivered" row: twenty earlier rows, so the next row is over although none of them was sent.
        Assert.Equal(RecipientLimitVerdict.Refused, await AskAsync(next));
        Assert.NotEqual(Guid.Empty, refusedByCount);

        // A row made when the window has moved past the older rows: only the eight nearest of the nineteen earlier rows, the "undelivered"
        // row and the refused row are inside the window, ten in all.
        Guid later = await AddAsync(DispatchType.B2, Ali, Now.AddMinutes(-5) + RecipientSendLimit.Window + TimeSpan.FromSeconds(1), _order);

        Assert.Equal(RecipientLimitVerdict.Allowed, await AskAsync(later));
    }

    // ---- KK6, KK7: the refusal ----

    [Fact]
    public async Task AFreshRowOverTheLimit_IsUndelivered_WithTheRecipientError_NoServiceCall_ChannelStateAndOrderUntouched()
    {
        await AddEarlierAsync(RecipientSendLimit.Threshold, Ali);
        Guid over = await AddAsync(DispatchType.B1, Ali, Now, _order);
        string channelBefore = await TextAsync("SELECT row_to_json(c)::text FROM channel_state_records c");
        string orderBefore = await TextAsync($"SELECT row_to_json(o)::text FROM orders o WHERE id = '{_order}'");
        string otherBefore = await TextAsync($"SELECT row_to_json(d)::text FROM dispatch_records d WHERE id = '{await FirstEarlierIdAsync()}'");
        FakeEmailGateway gateway = new();

        RecipientLimitVerdict verdict = await SendLikeTheSenderAsync(over, gateway);

        Assert.Equal(RecipientLimitVerdict.Refused, verdict);
        DispatchRecord row = await RowAsync(over);
        Assert.Equal(DispatchState.Undelivered, row.DispatchState);
        Assert.Equal(DispatchOutcome.RecipientError, row.LastOutcome);
        Assert.Null(row.NextAttemptAt);
        Assert.Null(row.SentAt);
        Assert.Empty(gateway.Calls);
        Assert.Equal(channelBefore, await TextAsync("SELECT row_to_json(c)::text FROM channel_state_records c"));
        Assert.Equal(orderBefore, await TextAsync($"SELECT row_to_json(o)::text FROM orders o WHERE id = '{_order}'"));
        Assert.Equal(otherBefore, await TextAsync($"SELECT row_to_json(d)::text FROM dispatch_records d WHERE id = '{await FirstEarlierIdAsync()}'"));

        // The order's mark is read from the row; the other order has none.
        await using AppDbContext db = NewContext();
        Assert.True(await DispatchMarks.HasMarkAsync(db, _order, Ct));
        Assert.False(await DispatchMarks.HasMarkAsync(db, _otherOrder, Ct));
    }

    [Fact]
    public async Task ARowUnderTheLimit_GoesToTheService_AfterTheAsk()
    {
        await AddEarlierAsync(RecipientSendLimit.Threshold - 1, Ali);
        Guid twentieth = await AddAsync(DispatchType.B1, Ali, Now, _order);
        FakeEmailGateway gateway = new();

        RecipientLimitVerdict verdict = await SendLikeTheSenderAsync(twentieth, gateway);

        Assert.Equal(RecipientLimitVerdict.Allowed, verdict);
        Assert.Single(gateway.Calls);
        Assert.Equal(DispatchState.Sent, (await RowAsync(twentieth)).DispatchState);
    }

    // ---- KK8: a resending and the B-1 after a correction are exempt but counted ----

    [Fact]
    public async Task AResending_IsExempt_GoesToTheService_AndIsCountedByTheLaterRows()
    {
        await AddEarlierAsync(RecipientSendLimit.Threshold, Ali);
        Guid resend = await AddAsync(DispatchType.B1, Ali, Now, _order, original: Guid.NewGuid());
        Guid later = await AddAsync(DispatchType.B2, Ali, Now.AddSeconds(1), _order);
        FakeEmailGateway gateway = new();

        Assert.Equal(RecipientLimitVerdict.Exempt, await SendLikeTheSenderAsync(resend, gateway));

        Assert.Single(gateway.Calls);
        Assert.Equal(DispatchState.Sent, (await RowAsync(resend)).DispatchState);
        Assert.Equal(RecipientLimitVerdict.Refused, await AskAsync(later));
    }

    [Fact]
    public async Task TheResendingCounts_SoWithItNineteenEarlierRowsLetOneMoreRowPassAndTheNextIsOver()
    {
        await AddEarlierAsync(RecipientSendLimit.Threshold - 2, Ali);
        await AddAsync(DispatchType.B1, Ali, Now.AddMinutes(-1), _order, original: Guid.NewGuid());
        Guid twentieth = await AddAsync(DispatchType.B2, Ali, Now, _order);
        Guid twentyFirst = await AddAsync(DispatchType.B2, Ali, Now.AddSeconds(1), _order);

        // Eighteen rows and the resending are nineteen earlier rows for the first; the first, still queued, makes twenty for the second.
        Assert.Equal(RecipientLimitVerdict.Allowed, await AskAsync(twentieth));
        Assert.Equal(RecipientLimitVerdict.Refused, await AskAsync(twentyFirst));
    }

    // ---- KK9: asked once; the line has the count and no address ----

    [Fact]
    public async Task ARetry_DoesNotAskTheLimitAgain()
    {
        await AddEarlierAsync(RecipientSendLimit.Threshold, Ali);
        Guid retried = await AddAsync(DispatchType.B1, Ali, Now, _order, attempts: 1, nextAttemptAt: Now.AddMinutes(5), outcome: DispatchOutcome.ChannelError);

        Assert.Equal(RecipientLimitVerdict.NotFirstAttempt, await AskAsync(retried));

        DispatchRecord row = await RowAsync(retried);
        Assert.Equal(DispatchState.Queued, row.DispatchState);
        Assert.Equal(1, row.AttemptCount);
    }

    [Fact]
    public async Task TheSecurityLine_IsWrittenOnTheFirstRefusal_ThenCounted_AndCarriesNoAddress()
    {
        await AddEarlierAsync(RecipientSendLimit.Threshold, Ali);
        List<Guid> over = [];
        for (int i = 0; i < 3; i++)
        {
            over.Add(await AddAsync(DispatchType.B1, Ali, Now.AddSeconds(i), _order));
        }

        ManualTimeProvider time = new(Now);
        CollectingSink sink = new();
        using Logger root = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Sink(sink).CreateLogger();
        using SerilogLoggerFactory factory = new(root);
        RecipientLimitLog log = new(time, new Microsoft.Extensions.Logging.Logger<RecipientLimitLog>(factory));

        foreach (Guid id in over)
        {
            Assert.Equal(RecipientLimitVerdict.Refused, await AskAsync(id, log));
        }

        string line = Format(Assert.Single(sink.Events));
        Assert.Contains(Shopfolio.Infrastructure.Logging.LogEvents.RecipientLimitExceededSampled, line, StringComparison.Ordinal);
        Assert.DoesNotContain("example.com", line, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Ali, line, StringComparison.OrdinalIgnoreCase);

        time.Advance(RecipientSendLimit.LogWindow);
        Guid afterWindow = await AddAsync(DispatchType.B1, Ali, Now.AddSeconds(10), _order);
        Assert.Equal(RecipientLimitVerdict.Refused, await AskAsync(afterWindow, log));

        Assert.Equal(2, sink.Events.Count);
        Assert.Equal(2, System.Text.Json.JsonDocument.Parse(Format(sink.Events[1])).RootElement.GetProperty("suppressedCount").GetInt32());
    }

    private static string Format(LogEvent logEvent)
    {
        using StringWriter writer = new(System.Globalization.CultureInfo.InvariantCulture);
        new ApplicationLogFormatter().Format(logEvent, writer);
        return writer.ToString();
    }

    private sealed class CollectingSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = [];

        public void Emit(LogEvent logEvent) => Events.Add(logEvent);
    }

    // ---- helpers ----

    private async Task<RecipientLimitVerdict> AskAsync(Guid id, RecipientLimitLog? log = null)
    {
        await using AppDbContext db = NewContext();
        return await NewCheck(db, log).AskAsync(id, Ct);
    }

    // The sender of T305/T385/T407, in short: ask the limit; call the service (and write the result) only when the row is not refused.
    private async Task<RecipientLimitVerdict> SendLikeTheSenderAsync(Guid id, FakeEmailGateway gateway)
    {
        RecipientLimitVerdict verdict = await AskAsync(id);
        if (verdict != RecipientLimitVerdict.Refused)
        {
            await using AppDbContext db = NewContext();
            DispatchAttempts attempts = new(db, NewRunner(db), gateway, new ManualTimeProvider(Now));
            await attempts.SendAsync(id, new EmailMessage(Ali, "Örnek Mağaza", null, "Konu", "<p>metin</p>", "metin"), Ct);
        }

        return verdict;
    }

    private static RecipientLimitCheck NewCheck(AppDbContext db, RecipientLimitLog? log)
    {
        ManualTimeProvider time = new(Now);
        RecipientLimitLog used = log ?? new RecipientLimitLog(time, Microsoft.Extensions.Logging.Abstractions.NullLogger<RecipientLimitLog>.Instance);
        return new RecipientLimitCheck(db, NewRunner(db), used);
    }

    private static TransactionRunner NewRunner(AppDbContext db) =>
        new(db, new JobEnqueueTracker(), new JobSignal(), new CacheInvalidations(db), new AppCache());

    // "Count" rows of one address made one minute apart before Now (the oldest first in the id order), each in a state that does not matter.
    private async Task<List<Guid>> AddEarlierAsync(int count, string recipient)
    {
        List<Guid> ids = [];
        for (int i = 0; i < count; i++)
        {
            ids.Add(await AddAsync(DispatchType.B1, recipient, Now.AddMinutes(-(i + 1) * 0.5 - 0.5), _order, state: DispatchState.Sent));
        }

        return ids;
    }

    private async Task<Guid> FirstEarlierIdAsync() =>
        Guid.Parse(await TextAsync("SELECT id::text FROM dispatch_records ORDER BY created_at, id LIMIT 1"));

    private async Task<Guid> AddAsync(
        DispatchType type,
        string recipient,
        DateTimeOffset createdAt,
        Guid? orderId,
        DispatchRelatedRecordType? related = null,
        Guid? id = null,
        Guid? original = null,
        DispatchState state = DispatchState.Queued,
        DispatchOutcome? outcome = null,
        int attempts = 0,
        DateTimeOffset? nextAttemptAt = null)
    {
        DispatchRecord row = DispatchRecord.Queue(type, recipient, orderId, related, related is null ? null : Guid.NewGuid(), createdAt);
        if (id is not null)
        {
            row.Id = id.Value;
        }

        row.OriginalDispatchId = original;
        row.DispatchState = state;
        row.LastOutcome = state == DispatchState.Sent ? DispatchOutcome.Sent : outcome;
        row.AttemptCount = state == DispatchState.Sent ? 1 : attempts;
        row.SentAt = state == DispatchState.Sent ? createdAt : null;
        row.NextAttemptAt = state == DispatchState.Queued ? nextAttemptAt : null;
        await using AppDbContext db = NewContext();
        db.DispatchRecords.Add(row);
        await db.SaveChangesAsync(Ct);
        return row.Id;
    }

    private async Task<DispatchRecord> RowAsync(Guid id)
    {
        await using AppDbContext db = NewContext();
        return await db.DispatchRecords.AsNoTracking().SingleAsync(d => d.Id == id, Ct);
    }

    private async Task<(Guid First, Guid Second)> SeedOrdersAsync()
    {
        Province province = Province.Create("06", "Ankara", Now);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, 1, Now, "metin", Now);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, 1, Now, "metin", Now);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, 1, Now, "metin", Now).WithHosting();
        Order first = NewOrder(1, province, form, contract, notice);
        Order second = NewOrder(2, province, form, contract, notice);
        await using AppDbContext db = NewContext();
        db.Provinces.Add(province);
        db.LegalTextVersions.AddRange(form, contract, notice);
        db.Orders.AddRange(first, second);
        await db.SaveChangesAsync(Ct);
        return (first.Id, second.Id);
    }

    private static Order NewOrder(int n, Province province, LegalTextVersion form, LegalTextVersion contract, LegalTextVersion notice)
    {
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
        order.PrivacyNoticeVersionId = notice.Id;
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
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

    private async Task<string> TextAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return (string)(await command.ExecuteScalarAsync(Ct))!;
    }

    private async Task<List<string>> NamesAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        List<string> names = [];
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(Ct);
        while (await reader.ReadAsync(Ct))
        {
            names.Add(reader.GetString(0));
        }

        return names;
    }
}
