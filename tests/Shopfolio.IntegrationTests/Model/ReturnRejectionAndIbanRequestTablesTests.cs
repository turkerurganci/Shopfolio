using System.Globalization;
using System.Net;
using System.Reflection;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Migrations;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the return rejection and IBAN request tests (T113): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(ReturnRejectionAndIbanRequestTablesDefinition.Name)]
public sealed class ReturnRejectionAndIbanRequestTablesDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "ReturnRejectionAndIbanRequestTables";
}

/// <summary>
/// The return rejection and IBAN request tables on a real PostgreSQL (T113 criteria 1-7; 06 4.27, 4.29, 2.2.5, 7.1, 7.2, 7.3; 09 10.6):
/// the migration made the two tables; the database itself refuses what the rules refuse — the rejection without units, the request both
/// fulfilled and closed, the second open request of an item and of an itemless refund, the IBAN on an unfulfilled request, the refund link
/// against the reason and the itemless request of another reason; the restrictive links stop a deletion, the administrator and IBAN links
/// are emptied; the context refuses the rejection's deletion and lets the disposal delete the request.
/// </summary>
[Collection(ReturnRejectionAndIbanRequestTablesDefinition.Name)]
public sealed class ReturnRejectionAndIbanRequestTablesTests : IAsyncLifetime
{
    private const string CheckViolation = PostgresErrorCodes.CheckViolation;
    private const string UniqueViolation = PostgresErrorCodes.UniqueViolation;
    private const string ForeignKeyViolation = PostgresErrorCodes.ForeignKeyViolation;

    private const string Phone = "3121234567";

    private const string AdminName = "Veli";

    private static readonly string[] TwoTables = ["refund_iban_requests", "return_rejections"];

    private static readonly DateTimeOffset Now = new(2026, 11, 4, 9, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly Today = new(2026, 11, 4);

    private static readonly ItemAmounts Amounts = new(100m, 10m, 15m);

    private readonly CollectionDatabaseFixture _collection;

    private int _orderCounter;

    private int _transactionCounter;

    private Guid _adminId;

    public ReturnRejectionAndIbanRequestTablesTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    /// <summary>Every reason of a request (06 5.6.4).</summary>
    public static TheoryData<RefundIbanRequestReason> AllReasons => new(Enum.GetValues<RefundIbanRequestReason>());

    /// <summary>The reasons whose request on an item carries no refund link (06 4.29): every reason but the failed card refund.</summary>
    public static TheoryData<RefundIbanRequestReason> ItemReasonsWithoutRefund => new(
        Enum.GetValues<RefundIbanRequestReason>().Where(r => r != RefundIbanRequestReason.CardRefundFailed));

    /// <summary>The two reasons an itemless request may have (06 4.29; K-1075, K-1131).</summary>
    public static TheoryData<RefundIbanRequestReason> ItemlessReasons => new() { RefundIbanRequestReason.CardRefundFailed, RefundIbanRequestReason.TransferFailed };

    /// <summary>The four reasons an itemless request may not have (06 4.29).</summary>
    public static TheoryData<RefundIbanRequestReason> NotItemlessReasons => new(
        Enum.GetValues<RefundIbanRequestReason>().Where(r => r is not (RefundIbanRequestReason.CardRefundFailed or RefundIbanRequestReason.TransferFailed)));

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE refund_iban_requests, return_rejections, refund_records, refunds, payment_records, withdrawals, refund_ibans, order_items, orders, "
            + "admins, legal_text_versions, provinces CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the migration ----

    [Fact]
    public async Task Migration_MakesTheTwoTables()
    {
        List<string> tables = await QueryAsync(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name = ANY(@names)",
            ("names", TwoTables));

        Assert.Equal(TwoTables, tables.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Columns_HaveTheTypesAndNullabilityOfTheFieldTables()
    {
        // 06 4.27 and 4.29 (the common Id and CreatedAt included): quantity integer, arrival date date, moment timestamptz, the
        // administrator's name text — required on the request, optional on the rejection —, the reason text.
        string[] expected =
        [
            "refund_iban_requests.admin_id uuid",
            "refund_iban_requests.admin_name text not null",
            "refund_iban_requests.closed_at timestamp with time zone",
            "refund_iban_requests.created_at timestamp with time zone not null",
            "refund_iban_requests.fulfilled_at timestamp with time zone",
            "refund_iban_requests.id uuid not null",
            "refund_iban_requests.order_item_id uuid",
            "refund_iban_requests.reason text not null",
            "refund_iban_requests.refund_iban_id uuid",
            "refund_iban_requests.refund_id uuid",
            "return_rejections.admin_id uuid",
            "return_rejections.admin_name text",
            "return_rejections.arrival_date date not null",
            "return_rejections.created_at timestamp with time zone not null",
            "return_rejections.id uuid not null",
            "return_rejections.order_item_id uuid not null",
            "return_rejections.quantity integer not null",
            "return_rejections.withdrawal_id uuid not null",
        ];

        List<string> columns = await QueryAsync(
            "SELECT table_name || '.' || column_name || ' ' || data_type"
            + " || CASE WHEN character_maximum_length IS NOT NULL THEN '(' || character_maximum_length || ')' ELSE '' END"
            + " || CASE WHEN is_nullable = 'NO' THEN ' not null' ELSE '' END"
            + " FROM information_schema.columns WHERE table_schema = 'public' AND table_name = ANY(@names)",
            ("names", TwoTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), columns.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Columns_HaveNoDefault()
    {
        // 06 4.27 and 4.29: no cell of the "Varsayılan" column is filled.
        List<string> defaults = await QueryAsync(
            "SELECT table_name || '.' || column_name FROM information_schema.columns WHERE table_schema = 'public' AND table_name = ANY(@names) AND column_default IS NOT NULL",
            ("names", TwoTables));

        Assert.Empty(defaults);
    }

    [Fact]
    public async Task Indexes_AreTheDocumentedOnes()
    {
        // 06 7.1 (the two filtered unique indexes of the open requests) and the foreign key indexes of 7.2. Two indexes on the same column
        // take the framework's numbered name (D-55).
        string[] expected =
        [
            "refund_iban_requests.ix_refund_iban_requests_admin_id | CREATE INDEX ix_refund_iban_requests_admin_id ON public.refund_iban_requests USING btree (admin_id)",
            "refund_iban_requests.ix_refund_iban_requests_order_item_id | CREATE INDEX ix_refund_iban_requests_order_item_id ON public.refund_iban_requests USING btree (order_item_id)",
            "refund_iban_requests.ix_refund_iban_requests_order_item_id1 | CREATE UNIQUE INDEX ix_refund_iban_requests_order_item_id1 ON public.refund_iban_requests USING btree (order_item_id) WHERE ((fulfilled_at IS NULL) AND (closed_at IS NULL))",
            "refund_iban_requests.ix_refund_iban_requests_refund_iban_id | CREATE INDEX ix_refund_iban_requests_refund_iban_id ON public.refund_iban_requests USING btree (refund_iban_id)",
            "refund_iban_requests.ix_refund_iban_requests_refund_id | CREATE INDEX ix_refund_iban_requests_refund_id ON public.refund_iban_requests USING btree (refund_id)",
            "refund_iban_requests.ix_refund_iban_requests_refund_id1 | CREATE UNIQUE INDEX ix_refund_iban_requests_refund_id1 ON public.refund_iban_requests USING btree (refund_id) WHERE ((order_item_id IS NULL) AND (fulfilled_at IS NULL) AND (closed_at IS NULL))",
            "refund_iban_requests.pk_refund_iban_requests | CREATE UNIQUE INDEX pk_refund_iban_requests ON public.refund_iban_requests USING btree (id)",
            "return_rejections.ix_return_rejections_admin_id | CREATE INDEX ix_return_rejections_admin_id ON public.return_rejections USING btree (admin_id)",
            "return_rejections.ix_return_rejections_order_item_id | CREATE INDEX ix_return_rejections_order_item_id ON public.return_rejections USING btree (order_item_id)",
            "return_rejections.ix_return_rejections_withdrawal_id | CREATE INDEX ix_return_rejections_withdrawal_id ON public.return_rejections USING btree (withdrawal_id)",
            "return_rejections.pk_return_rejections | CREATE UNIQUE INDEX pk_return_rejections ON public.return_rejections USING btree (id)",
        ];

        List<string> indexes = await QueryAsync(
            "SELECT tablename || '.' || indexname || ' | ' || indexdef FROM pg_indexes WHERE schemaname = 'public' AND tablename = ANY(@names)",
            ("names", TwoTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), indexes.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ForeignKeys_HaveTheDeleteBehaviorOfTheLinkTable()
    {
        // 06 7.2: the administrator links and the request's IBAN link are emptied (n); the item, withdrawal and refund links are
        // restrictive (a).
        string[] expected =
        [
            "refund_iban_requests fk_refund_iban_requests_admins_admin_id n",
            "refund_iban_requests fk_refund_iban_requests_order_items_order_item_id a",
            "refund_iban_requests fk_refund_iban_requests_refund_ibans_refund_iban_id n",
            "refund_iban_requests fk_refund_iban_requests_refunds_refund_id a",
            "return_rejections fk_return_rejections_admins_admin_id n",
            "return_rejections fk_return_rejections_order_items_order_item_id a",
            "return_rejections fk_return_rejections_withdrawals_withdrawal_id a",
        ];

        List<string> links = await QueryAsync(
            "SELECT conrelid::regclass::text || ' ' || conname || ' ' || confdeltype::text FROM pg_constraint WHERE contype = 'f' AND conrelid = ANY(ARRAY(SELECT x::regclass FROM unnest(@oids) x))",
            ("oids", TwoTables.Select(t => "public." + t).ToArray()));

        Assert.Equal(expected.Order(StringComparer.Ordinal), links.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task CheckConstraints_AreTheDocumentedOnes()
    {
        // 06 7.3 and the enum column's allowed values (06 2.9.2).
        string[] expected =
        [
            "refund_iban_requests.ck_refund_iban_requests_fulfilled_or_closed | CHECK ((num_nonnulls(fulfilled_at, closed_at) <= 1))",
            "refund_iban_requests.ck_refund_iban_requests_iban_only_when_fulfilled | CHECK (((refund_iban_id IS NULL) OR (fulfilled_at IS NOT NULL)))",
            "refund_iban_requests.ck_refund_iban_requests_itemless_on_refund_failure | CHECK (((order_item_id IS NOT NULL) OR ((reason = ANY (ARRAY['CardRefundFailed'::text, 'TransferFailed'::text])) AND (refund_id IS NOT NULL))))",
            "refund_iban_requests.ck_refund_iban_requests_reason | CHECK ((reason = ANY (ARRAY['ReturnAfterIbanDeleted'::text, 'TransferFailed'::text, 'CardRefundFailed'::text, 'NoticeWithoutIban'::text, 'CancellationOrRemoval'::text, 'AdminRequest'::text])))",
            "refund_iban_requests.ck_refund_iban_requests_refund_by_reason | CHECK (((refund_id IS NOT NULL) = ((reason = 'CardRefundFailed'::text) OR ((reason = 'TransferFailed'::text) AND (order_item_id IS NULL)))))",
            "return_rejections.ck_return_rejections_quantity_above_zero | CHECK ((quantity > 0))",
        ];

        List<string> checks = await QueryAsync(
            "SELECT conrelid::regclass::text || '.' || conname || ' | ' || pg_get_constraintdef(oid) FROM pg_constraint WHERE contype = 'c' AND conrelid = ANY(ARRAY(SELECT x::regclass FROM unnest(@oids) x))",
            ("oids", TwoTables.Select(t => "public." + t).ToArray()));

        Assert.Equal(expected.Order(StringComparer.Ordinal), checks.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Migration_HasNoDownMethod()
    {
        MethodInfo? down = typeof(CreateReturnRejectionAndIbanRequestTables)
            .GetMethod("Down", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);

        Assert.Null(down);
    }

    // ---- KK2: the return rejection ----

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task RejectionWithoutUnits_IsRefused(int quantity)
    {
        Seed seed = await SeedAsync();

        Assert.Equal(
            (CheckViolation, "ck_return_rejections_quantity_above_zero"),
            await ViolationOfAsync(c => c.ReturnRejections.Add(Rejection(seed, quantity))));
    }

    [Fact]
    public async Task RejectionsOfTheSameDeclaration_AreAccepted_AndKeepTheirArrivalDate()
    {
        // A rejection is made with the received units; each partial arrival is its own row (06 4.27, 4.26).
        Seed seed = await SeedAsync();
        await SaveAsync(c => c.ReturnRejections.AddRange(Rejection(seed, 1), Rejection(seed, 2)));

        Assert.Equal(
            "2 3 2026-11-03",
            await ScalarAsync<string>("SELECT count(*) || ' ' || sum(quantity) || ' ' || max(arrival_date)::text FROM return_rejections"));
    }

    // ---- KK3: fulfilled, closed and the open requests ----

    [Fact]
    public async Task RequestBothFulfilledAndClosed_IsRefused()
    {
        Seed seed = await SeedAsync();
        RefundIbanRequest request = ItemRequest(seed.ItemId, RefundIbanRequestReason.AdminRequest);
        (request.FulfilledAt, request.ClosedAt) = (Now, Now);

        Assert.Equal((CheckViolation, "ck_refund_iban_requests_fulfilled_or_closed"), await ViolationOfAsync(c => c.RefundIbanRequests.Add(request)));
    }

    [Fact]
    public async Task SecondOpenRequestOfTheItem_IsRefused()
    {
        Seed seed = await SeedAsync();
        await SaveAsync(c => c.RefundIbanRequests.Add(ItemRequest(seed.ItemId, RefundIbanRequestReason.ReturnAfterIbanDeleted)));

        Assert.Equal(
            (UniqueViolation, "ix_refund_iban_requests_order_item_id1"),
            await ViolationOfAsync(c => c.RefundIbanRequests.Add(ItemRequest(seed.ItemId, RefundIbanRequestReason.AdminRequest))));
    }

    [Fact]
    public async Task SecondOpenItemlessRequestOfTheRefund_IsRefused()
    {
        Seed seed = await SeedAsync();
        await SaveAsync(c => c.RefundIbanRequests.Add(ItemlessRequest(seed.LatePaymentRefundId, RefundIbanRequestReason.CardRefundFailed)));

        Assert.Equal(
            (UniqueViolation, "ix_refund_iban_requests_refund_id1"),
            await ViolationOfAsync(c => c.RefundIbanRequests.Add(ItemlessRequest(seed.LatePaymentRefundId, RefundIbanRequestReason.TransferFailed))));
    }

    [Fact]
    public async Task FulfilledAndClosedRequests_DoNotHoldTheItem_AndAnotherItemHasItsOwnOpenRequest()
    {
        Seed seed = await SeedAsync();
        RefundIbanRequest fulfilled = ItemRequest(seed.ItemId, RefundIbanRequestReason.AdminRequest);
        fulfilled.FulfilledAt = Now;
        RefundIbanRequest closed = ItemRequest(seed.ItemId, RefundIbanRequestReason.CancellationOrRemoval);
        closed.ClosedAt = Now;

        await SaveAsync(c => c.RefundIbanRequests.AddRange(
            fulfilled,
            closed,
            ItemRequest(seed.ItemId, RefundIbanRequestReason.ReturnAfterIbanDeleted),
            ItemRequest(seed.OtherItemId, RefundIbanRequestReason.ReturnAfterIbanDeleted)));

        Assert.Equal(4L, await ScalarAsync<long>("SELECT count(*) FROM refund_iban_requests"));
    }

    [Fact]
    public async Task OpenItemRequestsOnTheSameRefund_AreNotUniqueByTheRefund()
    {
        // The refund's filtered unique index holds only the itemless requests (06 7.1): the transfer path of the same refund may be open
        // on two items and next to the itemless request.
        Seed seed = await SeedAsync();

        await SaveAsync(c => c.RefundIbanRequests.AddRange(
            ItemRequest(seed.ItemId, RefundIbanRequestReason.CardRefundFailed, seed.ItemRefundId),
            ItemRequest(seed.OtherItemId, RefundIbanRequestReason.CardRefundFailed, seed.ItemRefundId),
            ItemlessRequest(seed.ItemRefundId, RefundIbanRequestReason.CardRefundFailed)));

        Assert.Equal(3L, await ScalarAsync<long>($"SELECT count(*) FROM refund_iban_requests WHERE refund_id = '{seed.ItemRefundId}'"));
    }

    [Fact]
    public async Task NewItemlessRequestOfTheRefund_AfterTheFirstIsFulfilled_IsAccepted()
    {
        // K-1131: the transfer of the late payment's refund failed after the IBAN was entered; the new request is itemless, on the same refund.
        Seed seed = await SeedAsync();
        RefundIbanRequest first = ItemlessRequest(seed.LatePaymentRefundId, RefundIbanRequestReason.CardRefundFailed);
        first.FulfilledAt = Now;
        await SaveAsync(c => c.RefundIbanRequests.Add(first));

        await SaveAsync(c => c.RefundIbanRequests.Add(ItemlessRequest(seed.LatePaymentRefundId, RefundIbanRequestReason.TransferFailed)));

        Assert.Equal(2L, await ScalarAsync<long>($"SELECT count(*) FROM refund_iban_requests WHERE refund_id = '{seed.LatePaymentRefundId}' AND order_item_id IS NULL"));
    }

    // ---- KK4: the IBAN link ----

    [Fact]
    public async Task OpenRequestWithAnIban_IsRefused()
    {
        Seed seed = await SeedAsync();
        RefundIbanRequest request = ItemRequest(seed.ItemId, RefundIbanRequestReason.AdminRequest);
        request.RefundIbanId = seed.RefundIbanId;

        Assert.Equal((CheckViolation, "ck_refund_iban_requests_iban_only_when_fulfilled"), await ViolationOfAsync(c => c.RefundIbanRequests.Add(request)));
    }

    [Fact]
    public async Task ClosedRequestWithAnIban_IsRefused()
    {
        Seed seed = await SeedAsync();
        RefundIbanRequest request = ItemRequest(seed.ItemId, RefundIbanRequestReason.AdminRequest);
        (request.ClosedAt, request.RefundIbanId) = (Now, seed.RefundIbanId);

        Assert.Equal((CheckViolation, "ck_refund_iban_requests_iban_only_when_fulfilled"), await ViolationOfAsync(c => c.RefundIbanRequests.Add(request)));
    }

    [Fact]
    public async Task FulfilledRequestWithItsIban_AndWithItsIbanDeleted_AreAccepted()
    {
        Seed seed = await SeedAsync();
        RefundIbanRequest withIban = ItemRequest(seed.ItemId, RefundIbanRequestReason.AdminRequest);
        (withIban.FulfilledAt, withIban.RefundIbanId) = (Now, seed.RefundIbanId);
        RefundIbanRequest withoutIban = ItemRequest(seed.OtherItemId, RefundIbanRequestReason.AdminRequest);
        withoutIban.FulfilledAt = Now;

        await SaveAsync(c => c.RefundIbanRequests.AddRange(withIban, withoutIban));

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM refund_iban_requests WHERE fulfilled_at IS NOT NULL"));
    }

    // ---- KK5: the refund link by reason and the itemless request ----

    [Theory]
    [MemberData(nameof(AllReasons))]
    public async Task ItemRequestOfEachReason_WithTheRefundLinkOfItsReason_IsAccepted(RefundIbanRequestReason reason)
    {
        Seed seed = await SeedAsync();
        Guid? refundId = reason == RefundIbanRequestReason.CardRefundFailed ? seed.ItemRefundId : null;

        await SaveAsync(c => c.RefundIbanRequests.Add(ItemRequest(seed.ItemId, reason, refundId)));

        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM refund_iban_requests WHERE reason = '{reason}'"));
    }

    [Theory]
    [MemberData(nameof(ItemReasonsWithoutRefund))]
    public async Task ItemRequestOfAnotherReason_WithARefundLink_IsRefused(RefundIbanRequestReason reason)
    {
        Seed seed = await SeedAsync();

        Assert.Equal(
            (CheckViolation, "ck_refund_iban_requests_refund_by_reason"),
            await ViolationOfAsync(c => c.RefundIbanRequests.Add(ItemRequest(seed.ItemId, reason, seed.ItemRefundId))));
    }

    [Fact]
    public async Task ItemRequestOfAFailedCardRefund_WithoutTheRefundLink_IsRefused()
    {
        Seed seed = await SeedAsync();

        Assert.Equal(
            (CheckViolation, "ck_refund_iban_requests_refund_by_reason"),
            await ViolationOfAsync(c => c.RefundIbanRequests.Add(ItemRequest(seed.ItemId, RefundIbanRequestReason.CardRefundFailed))));
    }

    [Theory]
    [MemberData(nameof(ItemlessReasons))]
    public async Task ItemlessRequestOfAFailedRefund_WithTheRefundLink_IsAccepted(RefundIbanRequestReason reason)
    {
        Seed seed = await SeedAsync();

        await SaveAsync(c => c.RefundIbanRequests.Add(ItemlessRequest(seed.LatePaymentRefundId, reason)));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM refund_iban_requests WHERE order_item_id IS NULL"));
    }

    [Theory]
    [MemberData(nameof(ItemlessReasons))]
    public async Task ItemlessRequestOfAFailedRefund_WithoutTheRefundLink_IsRefused(RefundIbanRequestReason reason)
    {
        // Both rules refuse the row; PostgreSQL tests the check constraints in name order and names the first.
        Seed seed = await SeedAsync();
        RefundIbanRequest request = ItemlessRequest(seed.LatePaymentRefundId, reason);
        request.RefundId = null;

        Assert.Equal((CheckViolation, "ck_refund_iban_requests_itemless_on_refund_failure"), await ViolationOfAsync(c => c.RefundIbanRequests.Add(request)));
    }

    [Theory]
    [MemberData(nameof(NotItemlessReasons))]
    public async Task ItemlessRequestOfAnotherReason_IsRefused_WithOrWithoutTheRefundLink(RefundIbanRequestReason reason)
    {
        Seed seed = await SeedAsync();
        RefundIbanRequest withRefund = ItemlessRequest(seed.LatePaymentRefundId, reason);
        RefundIbanRequest withoutRefund = ItemlessRequest(seed.LatePaymentRefundId, reason);
        withoutRefund.RefundId = null;

        Assert.Equal((CheckViolation, "ck_refund_iban_requests_itemless_on_refund_failure"), await ViolationOfAsync(c => c.RefundIbanRequests.Add(withRefund)));
        Assert.Equal((CheckViolation, "ck_refund_iban_requests_itemless_on_refund_failure"), await ViolationOfAsync(c => c.RefundIbanRequests.Add(withoutRefund)));
    }

    [Fact]
    public async Task UnknownReason_IsRefused()
    {
        Seed seed = await SeedAsync();

        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(
            "INSERT INTO refund_iban_requests (id, created_at, order_item_id, reason, admin_name)"
            + $" VALUES ('{EntityIds.New()}', now(), '{seed.ItemId}', 'Other', '{AdminName}')"));

        Assert.Equal((CheckViolation, "ck_refund_iban_requests_reason"), (error.SqlState, error.ConstraintName));
    }

    // ---- KK6: the deletion behavior ----

    [Fact]
    public async Task DeletingTheIban_KeepsTheRequest_AndEmptiesItsLink()
    {
        // 06 4.31: the IBAN is deleted by its event; the request's link to it is emptied (06 4.29).
        Seed seed = await SeedAsync();
        RefundIbanRequest request = ItemRequest(seed.ItemId, RefundIbanRequestReason.AdminRequest);
        (request.FulfilledAt, request.RefundIbanId) = (Now, seed.RefundIbanId);
        await SaveAsync(c => c.RefundIbanRequests.Add(request));

        await ExecuteAsync($"DELETE FROM refund_ibans WHERE id = '{seed.RefundIbanId}'");

        Assert.Equal(
            1L,
            await ScalarAsync<long>($"SELECT count(*) FROM refund_iban_requests WHERE id = '{request.Id}' AND refund_iban_id IS NULL AND fulfilled_at IS NOT NULL"));
    }

    [Fact]
    public async Task DeletingTheAdmin_KeepsTheRejectionAndTheRequest_EmptiesTheirLinksAndKeepsTheNames()
    {
        Seed seed = await SeedAsync();
        ReturnRejection rejection = Rejection(seed, 1);
        RefundIbanRequest request = ItemRequest(seed.ItemId, RefundIbanRequestReason.AdminRequest);
        await SaveAsync(c =>
        {
            c.ReturnRejections.Add(rejection);
            c.RefundIbanRequests.Add(request);
        });

        await ExecuteAsync($"DELETE FROM admins WHERE id = '{seed.AdminId}'");

        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM return_rejections WHERE id = '{rejection.Id}' AND admin_id IS NULL AND admin_name = '{AdminName}'"));
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM refund_iban_requests WHERE id = '{request.Id}' AND admin_id IS NULL AND admin_name = '{AdminName}'"));
    }

    [Fact]
    public async Task DeletingALinkedPrincipal_IsRefusedByTheDatabase()
    {
        // The item, withdrawal and refund links are restrictive (06 7.2): each principal below is held only by the row under test, so its
        // deletion names that row's link. The rejection's item is not its withdrawal's item here — the pairing is the writer's rule.
        Seed seed = await SeedAsync();
        (_, OrderItem rejectedItem) = await SeedItemAsync();
        (_, OrderItem requestedItem) = await SeedItemAsync();
        Refund ownRefund = Refund.BySystem(seed.OrderId, RefundType.AmountBased, PaymentMethod.Card, 10m, Now);
        await SaveAsync(c => c.Refunds.Add(ownRefund));
        ReturnRejection rejection = Rejection(seed, 1);
        rejection.OrderItemId = rejectedItem.Id;
        await SaveAsync(c =>
        {
            c.ReturnRejections.Add(rejection);
            c.RefundIbanRequests.AddRange(
                ItemRequest(requestedItem.Id, RefundIbanRequestReason.AdminRequest),
                ItemlessRequest(ownRefund.Id, RefundIbanRequestReason.CardRefundFailed));
        });

        foreach ((string statement, string constraint) in new[]
        {
            ($"DELETE FROM order_items WHERE id = '{rejectedItem.Id}'", "fk_return_rejections_order_items_order_item_id"),
            ($"DELETE FROM withdrawals WHERE id = '{seed.WithdrawalId}'", "fk_return_rejections_withdrawals_withdrawal_id"),
            ($"DELETE FROM order_items WHERE id = '{requestedItem.Id}'", "fk_refund_iban_requests_order_items_order_item_id"),
            ($"DELETE FROM refunds WHERE id = '{ownRefund.Id}'", "fk_refund_iban_requests_refunds_refund_id"),
        })
        {
            PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(statement));
            Assert.Equal((ForeignKeyViolation, constraint), (error.SqlState, error.ConstraintName));
        }
    }

    [Fact]
    public async Task DeletingARejectionThroughTheContext_IsRefused_AndTheRowStays()
    {
        Seed seed = await SeedAsync();
        ReturnRejection rejection = Rejection(seed, 1);
        await SaveAsync(c => c.ReturnRejections.Add(rejection));

        await using (AppDbContext context = NewContext())
        {
            context.Remove(rejection);
            InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));
            Assert.Contains("never deleted", error.Message, StringComparison.Ordinal);
        }

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM return_rejections"));
    }

    [Fact]
    public async Task FulfillingARequestInPlace_AndDeletingItThroughTheContext_AreAccepted()
    {
        // The customer's IBAN fulfils the request in place (06 4.29); the disposal deletes the request explicitly (06 2.2.5, 4.17).
        Seed seed = await SeedAsync();
        RefundIbanRequest request = ItemRequest(seed.ItemId, RefundIbanRequestReason.AdminRequest);
        await SaveAsync(c => c.RefundIbanRequests.Add(request));

        await using (AppDbContext context = NewContext())
        {
            RefundIbanRequest loaded = await context.RefundIbanRequests.SingleAsync(r => r.Id == request.Id, Ct);
            (loaded.FulfilledAt, loaded.RefundIbanId) = (Now, seed.RefundIbanId);
            await context.SaveChangesAsync(Ct);
        }

        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM refund_iban_requests WHERE refund_iban_id = '{seed.RefundIbanId}'"));

        await using (AppDbContext context = NewContext())
        {
            context.RefundIbanRequests.Remove(await context.RefundIbanRequests.SingleAsync(r => r.Id == request.Id, Ct));
            await context.SaveChangesAsync(Ct);
        }

        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM refund_iban_requests"));
    }

    // ---- helpers ----

    private static ReturnRejection Rejection(Seed seed, int quantity) =>
        ReturnRejection.Create(seed.ItemId, seed.WithdrawalId, quantity, Today.AddDays(-1), seed.AdminId, AdminName, Now);

    /// <summary>A request on the item by the seeded administrator.</summary>
    private RefundIbanRequest ItemRequest(Guid itemId, RefundIbanRequestReason reason, Guid? refundId = null) =>
        RefundIbanRequest.ForItem(itemId, reason, refundId, _adminId, AdminName, Now);

    /// <summary>An itemless request on the refund by the seeded administrator.</summary>
    private RefundIbanRequest ItemlessRequest(Guid refundId, RefundIbanRequestReason reason) =>
        RefundIbanRequest.ForRefund(refundId, reason, _adminId, AdminName, Now);

    /// <summary>
    /// Seeds one order with two physical items, an administrator, a withdrawal of the first item, a card refund of the first item, a late
    /// payment's refund and a refund IBAN.
    /// </summary>
    private async Task<Seed> SeedAsync()
    {
        (Order order, OrderItem item) = await SeedItemAsync();
        OrderItem other = OrderItem.Create(order.Id, null, null, ProductType.Physical, "Pencere", 2, 100m, 20m, 50m, Now);
        Admin admin = Admin.Create(AdminName, $"veli-{EntityIds.New():N}@example.com", "digest-placeholder", Now);
        RefundIban iban = RefundIban.Create(order.Id, "TR000000000000000000000000", Now);
        PaymentRecord payment = PaymentRecord.ForCardPayment(order.Id, $"tx-late-{Interlocked.Increment(ref _transactionCounter)}", null, Now);
        await SaveAsync(c =>
        {
            c.OrderItems.Add(other);
            c.Admins.Add(admin);
            c.RefundIbans.Add(iban);
            c.PaymentRecords.Add(payment);
        });

        Withdrawal withdrawal = Withdrawal.ByBuyer(item.Id, 3, Today.AddDays(-5), null, Amounts, null, Now);
        Refund itemRefund = Refund.BySystem(order.Id, RefundType.AmountBased, PaymentMethod.Card, 50m, Now);
        itemRefund.OrderItemId = item.Id;
        Refund latePayment = Refund.ForLatePayment(order.Id, payment.Id, 120m, Now);
        await SaveAsync(c =>
        {
            c.Withdrawals.Add(withdrawal);
            c.Refunds.AddRange(itemRefund, latePayment);
        });

        _adminId = admin.Id;
        return new Seed(order.Id, item.Id, other.Id, admin.Id, withdrawal.Id, itemRefund.Id, latePayment.Id, iban.Id);
    }

    /// <summary>Seeds one more order with one physical item of three units.</summary>
    private async Task<(Order Order, OrderItem Item)> SeedItemAsync()
    {
        Order order = await SeedOrderAsync();
        OrderItem item = OrderItem.Create(order.Id, null, null, ProductType.Physical, "Kapı", 3, 100m, 20m, 50m, Now);
        await SaveAsync(c => c.OrderItems.Add(item));
        return (order, item);
    }

    private Order NewOrder(References refs)
    {
        int n = Interlocked.Increment(ref _orderCounter);
        Order order = Order.Create(
            $"2026-{n:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", PaymentMethod.Card, true, "Ali@Example.com", 120m, 0m, Now.AddHours(1), Now);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.BillingRecipientName = "Ali Veli";
        order.BillingProvinceId = refs.ProvinceId;
        order.BillingDistrict = "Çankaya";
        order.BillingAddressLine = "Atatürk Bulvarı 1";
        order.CompanyType = CompanyType.Tradesman;
        order.CompanyLegalName = "Örnek Ticaret";
        order.CompanyTaxIdNumber = "1234567890";
        order.CompanyProvinceId = refs.ProvinceId;
        order.CompanyDistrict = "Çankaya";
        order.CompanyAddressLine = "Kızılay 2";
        order.CompanyPhone = Phone;
        order.CompanyEmail = "firma@example.com";
        order.CompanyKepAddress = "firma@hs01.kep.tr";
        order.PreInformationFormVersionId = refs.PreInformationFormId;
        order.DistanceSalesContractVersionId = refs.DistanceSalesContractId;
        order.PrivacyNoticeVersionId = refs.PrivacyNoticeId;
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
        return order;
    }

    /// <summary>Seeds one more order; the references (province, legal texts) are made on the first call and reused after.</summary>
    private async Task<Order> SeedOrderAsync()
    {
        References refs = await GetReferencesAsync();
        Order order = NewOrder(refs);
        await SaveAsync(c => c.Orders.Add(order));
        return order;
    }

    private async Task<References> GetReferencesAsync()
    {
        long count = await ScalarAsync<long>("SELECT count(*) FROM provinces WHERE code = '06'");
        if (count == 0)
        {
            Province province = Province.Create("06", "Ankara", Now);
            LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, 1, Now, "metin", Now);
            LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, 1, Now, "metin", Now);
            LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, 1, Now, "metin", Now).WithHosting();
            await SaveAsync(c =>
            {
                c.Provinces.Add(province);
                c.LegalTextVersions.AddRange(form, contract, notice);
            });
            return new References(province.Id, form.Id, contract.Id, notice.Id);
        }

        await using AppDbContext context = NewContext();
        Province existing = await context.Provinces.SingleAsync(p => p.Code == "06", Ct);
        Guid Version(LegalTextType type) => context.LegalTextVersions.Single(v => v.LegalTextType == type).Id;
        return new References(existing.Id, Version(LegalTextType.PreInformationForm), Version(LegalTextType.DistanceSalesContract), Version(LegalTextType.PrivacyNotice));
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

    /// <summary>Saves the change and returns the SQLSTATE and the constraint name of the database error it raises.</summary>
    private async Task<(string SqlState, string? Constraint)> ViolationOfAsync(Action<AppDbContext> change)
    {
        await using AppDbContext context = NewContext();
        change(context);

        DbUpdateException error = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(Ct));

        PostgresException inner = Assert.IsType<PostgresException>(error.InnerException);
        return (inner.SqlState, inner.ConstraintName);
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<T> ScalarAsync<T>(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return (T)(await command.ExecuteScalarAsync(Ct))!;
    }

    private async Task<List<string>> QueryAsync(string sql, params (string Name, object Value)[] parameters)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        foreach ((string name, object value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        List<string> rows = [];
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(Ct);
        while (await reader.ReadAsync(Ct))
        {
            rows.Add(Convert.ToString(reader.GetValue(0), CultureInfo.InvariantCulture)!);
        }

        return rows;
    }

    private sealed record References(Guid ProvinceId, Guid PreInformationFormId, Guid DistanceSalesContractId, Guid PrivacyNoticeId);

    /// <summary>
    /// The seeded principals: the order, its two items, an administrator, a withdrawal of the first item, a card refund of the first item,
    /// a late payment's refund (no item) and a refund IBAN.
    /// </summary>
    private sealed record Seed(
        Guid OrderId,
        Guid ItemId,
        Guid OtherItemId,
        Guid AdminId,
        Guid WithdrawalId,
        Guid ItemRefundId,
        Guid LatePaymentRefundId,
        Guid RefundIbanId);
}
