using System.Net;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Disposal;

/// <summary>
/// The other time-bound records' parts of the disposal on a real PostgreSQL and a fake clock (T302; 06 9.1 Z-29, Z-33, Z-34, Z-40, Z-41, Z-43, Z-44; 06 9.3): the
/// contact request by topic, the sign-in and disposal records, the dispatch record's two arms, the inbound notification record, the defect claim, the digital
/// files and their orphan files. Every date is seeded relative to the harness's clock; no test waits for real time.
/// </summary>
[Collection(DisposalDefinition.Name)]
public sealed class RecordDisposalTests : IAsyncLifetime
{
    private readonly CollectionDatabaseFixture _collection;
    private readonly string _logRoot = Path.Combine(Path.GetTempPath(), "shopfolio-t302-" + Guid.NewGuid().ToString("N"));
    private DisposalHarness _harness = null!;
    private Guid _provinceId;
    private Guid _formId;
    private Guid _contractId;
    private Guid _noticeId;
    private int _orderCounter;

    public RecordDisposalTests(TemplateDatabaseFixture template, CollectionDatabaseFixture collection)
    {
        ArgumentNullException.ThrowIfNull(template);
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    /// <summary>A paid order created in 2015 is past Z-28 at the harness's clock.</summary>
    private static DateTimeOffset PastPaidPeriod => new(2015, 6, 1, 9, 0, 0, TimeSpan.Zero);

    public async ValueTask InitializeAsync()
    {
        string app = Path.Combine(_logRoot, "app");
        string access = Path.Combine(_logRoot, "access");
        Directory.CreateDirectory(app);
        Directory.CreateDirectory(access);
        await ExecuteAsync(
            "TRUNCATE contact_requests, dispatch_records, inbound_notification_records, login_records, disposal_records, orders, customers, admins, digital_files, legal_text_versions, provinces CASCADE");
        _harness = new DisposalHarness(_collection.Dsn, new LogDirectories(app, access));

        Province province = Province.Create("06", "Ankara", _harness.Now);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, 1, _harness.Now, "metin", _harness.Now);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, 1, _harness.Now, "metin", _harness.Now);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, 1, _harness.Now, "metin", _harness.Now).WithHosting();
        await _harness.SeedAsync(db =>
        {
            db.Provinces.Add(province);
            db.LegalTextVersions.AddRange(form, contract, notice);
        });
        _provinceId = province.Id;
        _formId = form.Id;
        _contractId = contract.Id;
        _noticeId = notice.Id;
    }

    public async ValueTask DisposeAsync()
    {
        await _harness.DisposeAsync();
        if (Directory.Exists(_logRoot))
        {
            Directory.Delete(_logRoot, recursive: true);
        }
    }

    private DateTimeOffset Ago(int days) => _harness.Now - TimeSpan.FromDays(days);

    // ---- KK1: chunks are transactions of their own; a chunk that did nothing writes no row ----

    [Fact]
    public async Task AFailingChunk_DoesNotRollBackTheChunkBefore()
    {
        ContactRequest first = await SeedRequestAsync(ContactTopic.GeneralQuestion, closedAt: Ago(3 * 365));
        int calls = 0;

        await Assert.ThrowsAsync<InvalidOperationException>(() => _harness.WithRunAsync((run, _) => run.ChunksAsync(
            DisposalRecordType.ContactRequest,
            async (db, _, cancel) =>
            {
                calls++;
                if (calls > 1)
                {
                    throw new InvalidOperationException("second chunk fails");
                }

                await db.ContactRequests.Where(c => c.Id == first.Id).ExecuteDeleteAsync(cancel);
                return DisposalTimings.PartSize;
            },
            Ct)));

        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM contact_requests"));
        Assert.Equal(DisposalTimings.PartSize, await DisposalCountAsync(DisposalRecordType.ContactRequest));
    }

    [Fact]
    public async Task APartThatFindsNothing_WritesNoDisposalRow()
    {
        await SeedRequestAsync(ContactTopic.GeneralQuestion, closedAt: Ago(30));
        await _harness.SeedAsync(db => db.DigitalFiles.Add(DigitalFile.Create(EntityIds.NewStorageKey(), "a.pdf", 10, Ago(0))));

        await _harness.RunStepAsync(Ct);

        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM disposal_records WHERE disposal_record_type IN ('ContactRequest', 'DigitalFile', 'DispatchRecord', 'DefectClaim', 'InboundNotificationRecord')"));
    }

    // ---- KK3: the contact request, by topic, open ones too ----

    [Fact]
    public async Task AContactRequest_GoesAtTwoYearsFromItsLastClosing_AndAnAboutOrderRequestAtTenYears()
    {
        ContactRequest orderKept = await SeedRequestAsync(ContactTopic.AboutOrder, closedAt: Ago(3 * 365));
        ContactRequest orderGone = await SeedRequestAsync(ContactTopic.AboutOrder, closedAt: Ago(11 * 365));
        ContactRequest generalGone = await SeedRequestAsync(ContactTopic.GeneralQuestion, closedAt: Ago(3 * 365));
        ContactRequest generalKept = await SeedRequestAsync(ContactTopic.GeneralQuestion, closedAt: Ago(365));
        ContactRequest openNeverClosed = await SeedRequestAsync(ContactTopic.AboutProduct, closedAt: null, createdAt: Ago(3 * 365));
        ContactRequest reopened = await SeedRequestAsync(ContactTopic.Other, closedAt: Ago(3 * 365), open: true);
        ContactRequest closedRecently = await SeedRequestAsync(ContactTopic.PersonalDataRequest, closedAt: Ago(30), createdAt: Ago(5 * 365));

        await _harness.RunPartAsync<ContactRequestDisposalPart>(Ct);
        await _harness.RunPartAsync<ContactRequestDisposalPart>(Ct);

        HashSet<Guid> left = [.. await _harness.ReadAsync(db => db.ContactRequests.Select(c => c.Id).ToListAsync(Ct))];
        Assert.Equal(new HashSet<Guid> { orderKept.Id, generalKept.Id, closedRecently.Id }, left);
        Assert.DoesNotContain(orderGone.Id, left);
        Assert.DoesNotContain(generalGone.Id, left);
        Assert.DoesNotContain(openNeverClosed.Id, left);
        Assert.DoesNotContain(reopened.Id, left);
        Assert.Equal(4, await DisposalCountAsync(DisposalRecordType.ContactRequest));
    }

    // ---- KK4: the sign-in record and the disposal record ----

    [Fact]
    public async Task ASignInRecord_GoesAtSixMonths_AndADisposalRecordAtThreeYears_WithItsOwnRowForTheDeletion()
    {
        LoginRecord old = LoginRecord.Create(IPAddress.Parse("203.0.113.9"), AccountType.Customer, "a@example.com", LoginOutcome.Succeeded, Ago(200));
        LoginRecord recent = LoginRecord.Create(IPAddress.Parse("203.0.113.9"), AccountType.Customer, "b@example.com", LoginOutcome.Failed, Ago(150));
        DisposalRecord oldRecord = DisposalRecord.Counted(DisposalRecordType.Order, 3, Ago(4 * 365));
        DisposalRecord recentRecord = DisposalRecord.Counted(DisposalRecordType.Order, 2, Ago(2 * 365));
        await _harness.SeedAsync(db =>
        {
            db.LoginRecords.AddRange(old, recent);
            db.DisposalRecords.AddRange(oldRecord, recentRecord);
        });

        await _harness.RunPartAsync<LoginRecordDisposalPart>(Ct);
        await _harness.RunPartAsync<DisposalRecordDisposalPart>(Ct);

        Assert.Equal([recent.Id], await _harness.ReadAsync(db => db.LoginRecords.Select(l => l.Id).ToListAsync(Ct)));
        Assert.Equal(1, await DisposalCountAsync(DisposalRecordType.LoginRecord));
        Assert.Equal(1, await DisposalCountAsync(DisposalRecordType.DisposalRecord));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM disposal_records WHERE id = @id", ("id", oldRecord.Id)));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM disposal_records WHERE id = @id", ("id", recentRecord.Id)));
    }

    // ---- KK2: the dispatch record's two arms ----

    [Fact]
    public async Task ADispatchRecordOfNoOrder_GoesAtOneYear()
    {
        DispatchRecord gone = DispatchRecord.Queue(DispatchType.F5, "a@example.com", null, null, null, Ago(2 * 365));
        DispatchRecord kept = DispatchRecord.Queue(DispatchType.F5, "b@example.com", null, null, null, Ago(200));
        await _harness.SeedAsync(db => db.DispatchRecords.AddRange(gone, kept));

        await _harness.RunPartAsync<DispatchRecordDisposalPart>(Ct);

        Assert.Equal([kept.Id], await _harness.ReadAsync(db => db.DispatchRecords.Select(d => d.Id).ToListAsync(Ct)));
        Assert.Equal(1, await DisposalCountAsync(DisposalRecordType.DispatchRecord));
    }

    [Fact]
    public async Task ADispatchRecordOfAnOrder_GoesAtTenYears_OnBothArms()
    {
        Order paid = await SeedPaidOrderAsync(PastPaidPeriod);
        Order unpaid = await SeedFailedOrderAsync(Ago(30));
        DispatchRecord paidGone = await SeedDispatchAsync(paid.Id, Ago(11 * 365));
        DispatchRecord paidKept = await SeedDispatchAsync(paid.Id, Ago(5 * 365));
        DispatchRecord unpaidGone = await SeedDispatchAsync(unpaid.Id, Ago(11 * 365));

        await _harness.RunPartAsync<DispatchRecordDisposalPart>(Ct);

        Assert.Equal([paidKept.Id], await _harness.ReadAsync(db => db.DispatchRecords.Select(d => d.Id).ToListAsync(Ct)));
        Assert.DoesNotContain(paidGone.Id, await _harness.ReadAsync(db => db.DispatchRecords.Select(d => d.Id).ToListAsync(Ct)));
        Assert.DoesNotContain(unpaidGone.Id, await _harness.ReadAsync(db => db.DispatchRecords.Select(d => d.Id).ToListAsync(Ct)));
    }

    [Fact]
    public async Task OnTheUnpaidArm_TheAddressAndTheEventValuesGoAtThreeYears_AndAPaidOrderKeepsThem()
    {
        Order paid = await SeedPaidOrderAsync(PastPaidPeriod);
        Order unpaid = await SeedFailedOrderAsync(Ago(30));
        Order latePaid = await SeedFailedOrderAsync(Ago(30));
        PaymentRecord late = PaymentRecord.ForCardPayment(latePaid.Id, "txn-" + Guid.NewGuid().ToString("N"), null, _harness.Now);
        await _harness.SeedAsync(db => db.PaymentRecords.Add(late));
        DispatchRecord latePaidOld = await SeedDispatchAsync(latePaid.Id, Ago(4 * 365));
        DispatchRecord paidOld = await SeedDispatchAsync(paid.Id, Ago(4 * 365));
        DispatchRecord unpaidOld = await SeedDispatchAsync(unpaid.Id, Ago(4 * 365));
        DispatchRecord unpaidRecent = await SeedDispatchAsync(unpaid.Id, Ago(2 * 365));

        await _harness.RunPartAsync<DispatchRecordDisposalPart>(Ct);

        Assert.NotNull(await RecipientOfAsync(paidOld.Id));
        Assert.NotNull(await RecipientOfAsync(latePaidOld.Id));
        Assert.Null(await RecipientOfAsync(unpaidOld.Id));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records WHERE id = @id AND event_values IS NOT NULL", ("id", unpaidOld.Id)));
        Assert.NotNull(await RecipientOfAsync(unpaidRecent.Id));
        Assert.Equal(1, await DisposalCountAsync(DisposalRecordType.DispatchRecord));
    }

    // ---- KK2, KK6: the inbound notification record waits for the order ----

    [Fact]
    public async Task AnInboundNotification_OfNoOrder_GoesAtTenYears()
    {
        InboundNotificationRecord gone = await SeedNotificationAsync(null, Ago(11 * 365));
        InboundNotificationRecord kept = await SeedNotificationAsync(null, Ago(9 * 365));

        await _harness.RunPartAsync<InboundNotificationDisposalPart>(Ct);

        Assert.Equal([kept.Id], await _harness.ReadAsync(db => db.InboundNotificationRecords.Select(n => n.Id).ToListAsync(Ct)));
        Assert.NotEqual(gone.Id, kept.Id);
        Assert.Equal(1, await DisposalCountAsync(DisposalRecordType.InboundNotificationRecord));
    }

    [Fact]
    public async Task AnInboundNotification_OfAPaidOrder_StaysUntilTheOrderIsDisposed()
    {
        Order paid = await SeedPaidOrderAsync(PastPaidPeriod);
        InboundNotificationRecord row = await SeedNotificationAsync(paid.Id, Ago(11 * 365));

        await _harness.RunPartAsync<InboundNotificationDisposalPart>(Ct);
        long before = await ScalarAsync<long>("SELECT count(*) FROM inbound_notification_records WHERE id = @id", ("id", row.Id));
        await _harness.RunOrderPartAsync(null, Ct);
        await _harness.RunPartAsync<InboundNotificationDisposalPart>(Ct);
        long after = await ScalarAsync<long>("SELECT count(*) FROM inbound_notification_records WHERE id = @id", ("id", row.Id));

        Assert.Equal(1L, before);
        Assert.Equal(0L, after);
    }

    [Fact]
    public async Task AnInboundNotification_OfAnUnpaidOrder_GoesTenYearsFromTheMomentTheOrderFailed()
    {
        Order failedLongAgo = await SeedFailedOrderAsync(Ago(11 * 365));
        Order failedRecently = await SeedFailedOrderAsync(Ago(9 * 365));
        InboundNotificationRecord gone = await SeedNotificationAsync(failedLongAgo.Id, Ago(11 * 365));
        InboundNotificationRecord kept = await SeedNotificationAsync(failedRecently.Id, Ago(9 * 365));

        await _harness.RunPartAsync<InboundNotificationDisposalPart>(Ct);

        Assert.Equal([kept.Id], await _harness.ReadAsync(db => db.InboundNotificationRecords.Select(n => n.Id).ToListAsync(Ct)));
        Assert.NotEqual(gone.Id, kept.Id);
    }

    // ---- KK6: the defect claim goes alone, after the order ----

    [Fact]
    public async Task ADefectClaim_GoesTenYearsFromItsLastChange_OnlyAfterItsOrderIsDisposed_AndTheLinksAreEmptied()
    {
        (Order disposed, OrderItem disposedItem) = await SeedOrderWithItemAsync(PastPaidPeriod);
        (Order standing, OrderItem standingItem) = await SeedOrderWithItemAsync(Ago(10));
        await _harness.RunOrderPartAsync(null, Ct);
        Assert.Null(await ScalarOrNullAsync<string>("SELECT contact_email FROM orders WHERE id = @id", ("id", disposed.Id)));
        Assert.NotNull(await ScalarOrNullAsync<string>("SELECT contact_email FROM orders WHERE id = @id", ("id", standing.Id)));
        DefectClaim gone = await SeedClaimAsync(disposedItem.Id, PastPaidPeriod, """[{"At":"2016-05-05T10:00:00+00:00"}]""");
        DefectClaim neverChanged = await SeedClaimAsync(await SeedExtraItemAsync(disposed.Id), PastPaidPeriod, "[]");
        DefectClaim recentChange = await SeedClaimAsync(await SeedExtraItemAsync(disposed.Id), PastPaidPeriod, $$"""[{"At":"{{Ago(400):O}}"}]""");
        DefectClaim orderNotDisposed = await SeedClaimAsync(standingItem.Id, PastPaidPeriod, "[]");
        Admin admin = Admin.Create("Veli", $"veli-{EntityIds.New():N}@example.com", "digest-placeholder", _harness.Now);
        await _harness.SeedAsync(db => db.Admins.Add(admin));
        Return received = Return.ForDefectClaim(disposedItem.Id, gone.Id, 1, new DateOnly(2026, 11, 5), admin.Id, admin.Name, PastPaidPeriod);
        await _harness.SeedAsync(db => db.Returns.Add(received));
        int versionBefore = await ScalarAsync<int>("SELECT version FROM orders WHERE id = @id", ("id", disposed.Id));

        await _harness.RunPartAsync<DefectClaimDisposalPart>(Ct);

        HashSet<Guid> left = [.. await _harness.ReadAsync(db => db.DefectClaims.Select(c => c.Id).ToListAsync(Ct))];
        Assert.Equal(new HashSet<Guid> { recentChange.Id, orderNotDisposed.Id }, left);
        Assert.DoesNotContain(neverChanged.Id, left);
        Assert.Equal(2, await DisposalCountAsync(DisposalRecordType.DefectClaim));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM returns WHERE id = @id AND defect_claim_id IS NULL", ("id", received.Id)));
        Assert.Equal(versionBefore, await ScalarAsync<int>("SELECT version FROM orders WHERE id = @id", ("id", disposed.Id)));
    }

    // ---- KK7, KK8, KK9: the digital files ----

    [Fact]
    public async Task ADigitalFile_GoesWhenDetachedOrNeverAttached_NotWhenAnOrderItemLinksToIt_AndAnIncompleteUploadGoesWithoutARow()
    {
        DigitalFile unattached = DigitalFile.Create(EntityIds.NewStorageKey(), "a.pdf", 10, Ago(3));
        unattached.UploadCompletedAt = Ago(3);
        DigitalFile young = DigitalFile.Create(EntityIds.NewStorageKey(), "b.pdf", 10, _harness.Now - TimeSpan.FromHours(1));
        young.UploadCompletedAt = _harness.Now - TimeSpan.FromHours(1);
        DigitalFile linked = DigitalFile.Create(EntityIds.NewStorageKey(), "c.pdf", 10, Ago(3));
        linked.UploadCompletedAt = Ago(3);
        linked.AttachedAt = Ago(3);
        linked.DetachedAt = Ago(2);
        DigitalFile incomplete = DigitalFile.Create(EntityIds.NewStorageKey(), "d.pdf", 10, Ago(3));
        DigitalFile incompleteYoung = DigitalFile.Create(EntityIds.NewStorageKey(), "e.pdf", 10, _harness.Now - TimeSpan.FromHours(1));
        await _harness.SeedAsync(db => db.DigitalFiles.AddRange(unattached, young, linked, incomplete, incompleteYoung));
        Order order = await SeedPaidOrderAsync(Ago(10));
        OrderItem item = OrderItem.Create(order.Id, null, null, ProductType.Digital, "Kitap", 1, 100m, 20m, 16.67m, Ago(10));
        item.DownloadAllowance = 3;
        item.DigitalFileId = linked.Id;
        await _harness.SeedAsync(db => db.OrderItems.Add(item));

        await _harness.RunPartAsync<IncompleteDigitalUploadPart>(Ct);
        await _harness.RunPartAsync<DigitalFileDisposalPart>(Ct);
        await _harness.RunPartAsync<DigitalFileDisposalPart>(Ct);

        HashSet<Guid> left = [.. await _harness.ReadAsync(db => db.DigitalFiles.Select(d => d.Id).ToListAsync(Ct))];
        Assert.Equal(new HashSet<Guid> { young.Id, linked.Id, incompleteYoung.Id }, left);
        Assert.Equal(1, await DisposalCountAsync(DisposalRecordType.DigitalFile));
    }

    [Fact]
    public async Task AnOrphanDigitalFile_GoesAfterItsRowWasRecorded_AndAFileOfARowStays()
    {
        DigitalFile gone = DigitalFile.Create(EntityIds.NewStorageKey(), "a.pdf", 10, Ago(3));
        gone.UploadCompletedAt = Ago(3);
        DigitalFile standing = DigitalFile.Create(EntityIds.NewStorageKey(), "b.pdf", 10, _harness.Now - TimeSpan.FromHours(1));
        standing.UploadCompletedAt = _harness.Now - TimeSpan.FromHours(1);
        await _harness.SeedAsync(db => db.DigitalFiles.AddRange(gone, standing));
        string orphanKey = EntityIds.NewStorageKey();
        string youngOrphanKey = EntityIds.NewStorageKey();
        _harness.Files.Seed(FileArea.DigitalFile, gone.StorageKey, [1], Ago(3));
        _harness.Files.Seed(FileArea.DigitalFile, standing.StorageKey, [2], Ago(3));
        _harness.Files.Seed(FileArea.DigitalFile, orphanKey, [3], Ago(3));
        _harness.Files.Seed(FileArea.DigitalFile, youngOrphanKey, [4], _harness.Now - TimeSpan.FromHours(1));

        await _harness.RunPartAsync<DigitalFileDisposalPart>(Ct);
        await _harness.RunPartAsync<OrphanDigitalFilePart>(Ct);

        Assert.Equal(new HashSet<string> { standing.StorageKey, youngOrphanKey }, [.. _harness.Files.KeysOf(FileArea.DigitalFile)]);
        Assert.Equal(1, await DisposalCountAsync(DisposalRecordType.DigitalFile));
        Assert.Equal(2, await DisposalCountAsync(DisposalRecordType.OrphanFile));
    }

    // ---- seeds ----

    private async Task<ContactRequest> SeedRequestAsync(ContactTopic topic, DateTimeOffset? closedAt, DateTimeOffset? createdAt = null, bool open = false)
    {
        ContactRequest request = ContactRequest.Create("Ali", "ali@example.com", topic, "Merhaba", createdAt ?? closedAt?.AddDays(-1) ?? _harness.Now);
        if (closedAt is { } closed)
        {
            request.LastClosedAt = closed;
            request.ContactRequestStatus = open ? ContactRequestStatus.Open : ContactRequestStatus.Closed;
        }

        await _harness.SeedAsync(db => db.ContactRequests.Add(request));
        return request;
    }

    private async Task<DispatchRecord> SeedDispatchAsync(Guid orderId, DateTimeOffset at)
    {
        DispatchRecord record = DispatchRecord.Queue(DispatchType.B1, "ali@example.com", orderId, null, null, at);
        record.EventValues = """{"a":1}""";
        await _harness.SeedAsync(db => db.DispatchRecords.Add(record));
        return record;
    }

    private async Task<InboundNotificationRecord> SeedNotificationAsync(Guid? orderId, DateTimeOffset at)
    {
        InboundNotificationRecord record = InboundNotificationRecord.Create("n-" + Guid.NewGuid().ToString("N"), null, "payment", "processed", null, 120m, at);
        record.OrderId = orderId;
        await _harness.SeedAsync(db => db.InboundNotificationRecords.Add(record));
        return record;
    }

    private async Task<string?> RecipientOfAsync(Guid dispatchId) =>
        await _harness.ReadAsync(db => db.DispatchRecords.Where(d => d.Id == dispatchId).Select(d => d.RecipientEmail).SingleAsync(Ct));

    private async Task<(Order Order, OrderItem Item)> SeedOrderWithItemAsync(DateTimeOffset createdAt)
    {
        Order order = await SeedPaidOrderAsync(createdAt);
        return (order, await AddItemAsync(order.Id, createdAt));
    }

    private async Task<Guid> SeedExtraItemAsync(Guid orderId) => (await AddItemAsync(orderId, PastPaidPeriod)).Id;

    private async Task<OrderItem> AddItemAsync(Guid orderId, DateTimeOffset at)
    {
        OrderItem item = OrderItem.Create(orderId, null, null, ProductType.Physical, "Kalem", 1, 100m, 20m, 16.67m, at);
        await _harness.SeedAsync(db => db.OrderItems.Add(item));
        return item;
    }

    private async Task<DefectClaim> SeedClaimAsync(Guid itemId, DateTimeOffset createdAt, string history)
    {
        DefectClaim claim = DefectClaim.Create(itemId, 1, "Kusurlu", null, createdAt);
        foreach (DefectClaimHistoryEntry entry in PayloadJson.Read<List<DefectClaimHistoryEntry>>(history)!)
        {
            claim.ApplyStatus(entry.Status, entry.ActorKind, entry.At, entry.Quantity);
        }

        await _harness.SeedAsync(db => db.DefectClaims.Add(claim));
        return claim;
    }

    private async Task<Order> SeedOrderAsync(DateTimeOffset createdAt)
    {
        int n = Interlocked.Increment(ref _orderCounter);
        Order order = Order.Create(
            $"2026-{n:D6}", $"access-{n}-{Guid.NewGuid():N}", $"confirm-{n}-{Guid.NewGuid():N}", PaymentMethod.Card, true, "Ali@Example.com", 120m, 0m,
            createdAt.AddHours(1), createdAt);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.ShippingRecipientName = "Ali Veli";
        order.ShippingPhone = "5321234567";
        order.ShippingProvinceId = _provinceId;
        order.ShippingDistrict = "Çankaya";
        order.ShippingAddressLine = "Atatürk Bulvarı 1";
        order.BillingRecipientName = "Ali Veli";
        order.BillingProvinceId = _provinceId;
        order.BillingDistrict = "Çankaya";
        order.BillingAddressLine = "Atatürk Bulvarı 1";
        order.CompanyType = CompanyType.Tradesman;
        order.CompanyLegalName = "Örnek Ticaret";
        order.CompanyTaxIdNumber = "1234567890";
        order.CompanyProvinceId = _provinceId;
        order.CompanyDistrict = "Çankaya";
        order.CompanyAddressLine = "Kızılay 2";
        order.CompanyPhone = "3121234567";
        order.CompanyEmail = "firma@example.com";
        order.CompanyKepAddress = "firma@hs01.kep.tr";
        order.PreInformationFormVersionId = _formId;
        order.DistanceSalesContractVersionId = _contractId;
        order.PrivacyNoticeVersionId = _noticeId;
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
        await _harness.SeedAsync(db => db.Orders.Add(order));
        return order;
    }

    /// <summary>A paid order: an uncorrected card payment record with its confirming transition, and the payment axis set the way the transition unit would.</summary>
    private async Task<Order> SeedPaidOrderAsync(DateTimeOffset createdAt)
    {
        Order order = await SeedOrderAsync(createdAt);
        TransitionRecord confirming = TransitionRecord.Create(order.Id, TransitionCode.O1, ActorKind.System, null, null, null, null, null, null, null, createdAt);
        await _harness.SeedAsync(db => db.TransitionRecords.Add(confirming));
        PaymentRecord payment = PaymentRecord.ForCardPayment(order.Id, "txn-" + Guid.NewGuid().ToString("N"), confirming.Id, _harness.Now);
        await _harness.SeedAsync(db => db.PaymentRecords.Add(payment));
        await ExecuteAsync("UPDATE orders SET payment_status = 'Paid' WHERE id = '" + order.Id + "'");
        return order;
    }

    /// <summary>An order that failed at <paramref name="failedAt"/>: Failed and Cancelled, with the Ö2 transition; no payment record.</summary>
    private async Task<Order> SeedFailedOrderAsync(DateTimeOffset failedAt, DateTimeOffset? createdAt = null)
    {
        Order order = await SeedOrderAsync(createdAt ?? failedAt - TimeSpan.FromHours(2));
        TransitionRecord failed = TransitionRecord.Create(
            order.Id, TransitionCode.O2, ActorKind.System, null, null, TransitionReason.CardPaymentFailed, null, null, null, null, failedAt);
        await _harness.SeedAsync(db => db.TransitionRecords.Add(failed));
        await ExecuteAsync("UPDATE orders SET payment_status = 'Failed', order_status = 'Cancelled' WHERE id = '" + order.Id + "'");
        return order;
    }

    // ---- reads ----

    private Task<int> DisposalCountAsync(DisposalRecordType type) =>
        ScalarAsync<int>("SELECT coalesce(sum(count), 0)::int FROM disposal_records WHERE disposal_record_type = @type", ("type", type.ToString()));

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<T?> ScalarOrNullAsync<T>(string sql, params (string Name, object Value)[] parameters)
        where T : class
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        foreach ((string name, object value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        object? result = await command.ExecuteScalarAsync(Ct);
        return result is null or DBNull ? null : (T)result;
    }

    private async Task<T> ScalarAsync<T>(string sql, params (string Name, object Value)[] parameters)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        foreach ((string name, object value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        return (T)(await command.ExecuteScalarAsync(Ct))!;
    }
}
