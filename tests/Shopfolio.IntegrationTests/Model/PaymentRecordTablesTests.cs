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
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the payment and refund record tests (T112): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(PaymentRecordTablesDefinition.Name)]
public sealed class PaymentRecordTablesDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "PaymentRecordTables";
}

/// <summary>
/// The four payment side tables on a real PostgreSQL (T112 criteria 1-7; 06 4.32-4.35, 2.5.1, 7.1, 7.2, 7.3, 7.5; 09 10.6): the migration
/// made the four tables with the money type of 06 2.5.1; the database itself refuses what the rules refuse — the payment record without
/// a transaction id and a transition, the second payment of a transaction, the refund's links against its type, the second shipping fee
/// and the second refund of a payment, the refund record's outcome moment against its state and the second open or successful request,
/// the notification without an id and the same notification twice; the restrictive links stop a deletion, the defect claim and the
/// administrator links are emptied; and the context refuses the deletion of the three permanent records.
/// </summary>
[Collection(PaymentRecordTablesDefinition.Name)]
public sealed class PaymentRecordTablesTests : IAsyncLifetime
{
    private const string CheckViolation = PostgresErrorCodes.CheckViolation;
    private const string UniqueViolation = PostgresErrorCodes.UniqueViolation;
    private const string ForeignKeyViolation = PostgresErrorCodes.ForeignKeyViolation;
    private const string NumericValueOutOfRange = PostgresErrorCodes.NumericValueOutOfRange;

    private const string Phone = "3121234567";

    private static readonly string[] FourTables = ["inbound_notification_records", "payment_records", "refund_records", "refunds"];

    private static readonly DateTimeOffset Now = new(2026, 11, 4, 9, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly Today = new(2026, 11, 4);

    private static readonly ItemAmounts Amounts = new(100m, 10m, 15m);

    private readonly CollectionDatabaseFixture _collection;

    private int _orderCounter;

    private int _transactionCounter;

    public PaymentRecordTablesTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    /// <summary>Every refund type (06 5.6.7).</summary>
    public static TheoryData<RefundType> AllTypes => new(Enum.GetValues<RefundType>());

    /// <summary>The types other than the amount-based refund that carry an item (06 4.33).</summary>
    public static TheoryData<RefundType> ItemTypesOtherThanAmountBased => new()
    {
        RefundType.Cancellation, RefundType.ItemRemoval, RefundType.LateDeliveryTermination, RefundType.Withdrawal, RefundType.DefectClaimRescission,
    };

    /// <summary>The four types whose refund may carry the shipping fee (06 4.33).</summary>
    public static TheoryData<RefundType> ShippingFeeTypes => new()
    {
        RefundType.Cancellation, RefundType.ItemRemoval, RefundType.LateDeliveryTermination, RefundType.Withdrawal,
    };

    /// <summary>The three types whose refund never carries the shipping fee (06 4.33).</summary>
    public static TheoryData<RefundType> NoShippingFeeTypes => new()
    {
        RefundType.DefectClaimRescission, RefundType.AmountBased, RefundType.LatePayment,
    };

    /// <summary>The open states and the outcome states of a refund record (06 4.34).</summary>
    public static TheoryData<RefundRecordState> OpenStates => new() { RefundRecordState.Sending, RefundRecordState.OutcomePending };

    public static TheoryData<RefundRecordState> OutcomeStates => new() { RefundRecordState.Succeeded, RefundRecordState.Failed };

    /// <summary>The three records that are never deleted (06 4.32-4.34).</summary>
    public static TheoryData<string> PermanentTables => new() { "payment_records", "refunds", "refund_records" };

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE inbound_notification_records, refund_records, refunds, payment_records, returns, not_returned_closures, cancellations, item_removals, "
            + "late_delivery_terminations, withdrawals, defect_claims, transition_records, order_items, orders, admins, legal_text_versions, provinces CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the migration ----

    [Fact]
    public async Task Migration_MakesTheFourTables()
    {
        List<string> tables = await QueryAsync(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name = ANY(@names)",
            ("names", FourTables));

        Assert.Equal(FourTables, tables.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Columns_HaveTheTypesAndNullabilityOfTheFieldTables()
    {
        // 06 4.32-4.35 and 2.5.1 (the common Id and CreatedAt included): money numeric(12,2), quantity integer, moment timestamptz, the
        // provider's values text, the enums text.
        string[] expected =
        [
            "inbound_notification_records.amount numeric(12,2)",
            "inbound_notification_records.created_at timestamp with time zone not null",
            "inbound_notification_records.event_type text not null",
            "inbound_notification_records.id uuid not null",
            "inbound_notification_records.order_id uuid",
            "inbound_notification_records.order_reference text",
            "inbound_notification_records.outcome text not null",
            "inbound_notification_records.processed_at timestamp with time zone",
            "inbound_notification_records.provider_notification_id text",
            "inbound_notification_records.provider_transaction_id text",
            "inbound_notification_records.refund_record_id uuid",
            "inbound_notification_records.unprocessed_reason text",
            "payment_records.created_at timestamp with time zone not null",
            "payment_records.id uuid not null",
            "payment_records.order_id uuid not null",
            "payment_records.provider_transaction_id text",
            "payment_records.transition_record_id uuid",
            "refund_records.created_at timestamp with time zone not null",
            "refund_records.id uuid not null",
            "refund_records.is_marked_failed_by_admin boolean not null",
            "refund_records.late_success_at timestamp with time zone",
            "refund_records.outcome_at timestamp with time zone",
            "refund_records.refund_id uuid not null",
            "refund_records.state text not null",
            "refunds.actor_kind text not null",
            "refunds.admin_id uuid",
            "refunds.admin_name text",
            "refunds.amount numeric(12,2) not null",
            "refunds.cancellation_id uuid",
            "refunds.coupon_share numeric(12,2)",
            "refunds.created_at timestamp with time zone not null",
            "refunds.defect_claim_id uuid",
            "refunds.id uuid not null",
            "refunds.is_shipping_fee_included boolean not null",
            "refunds.item_removal_id uuid",
            "refunds.late_delivery_termination_id uuid",
            "refunds.not_returned_closure_id uuid",
            "refunds.order_id uuid not null",
            "refunds.order_item_id uuid",
            "refunds.payment_method text not null",
            "refunds.payment_record_id uuid",
            "refunds.quantity integer",
            "refunds.refund_type text not null",
            "refunds.return_id uuid",
            "refunds.sent_at timestamp with time zone",
            "refunds.vat_amount numeric(12,2)",
            "refunds.voided_at timestamp with time zone",
            "refunds.withdrawal_id uuid",
        ];

        List<string> columns = await QueryAsync(
            "SELECT table_name || '.' || column_name || ' ' || data_type"
            + " || CASE WHEN character_maximum_length IS NOT NULL THEN '(' || character_maximum_length || ')' ELSE '' END"
            + " || CASE WHEN data_type = 'numeric' THEN '(' || numeric_precision || ',' || numeric_scale || ')' ELSE '' END"
            + " || CASE WHEN is_nullable = 'NO' THEN ' not null' ELSE '' END"
            + " FROM information_schema.columns WHERE table_schema = 'public' AND table_name = ANY(@names)",
            ("names", FourTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), columns.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ColumnDefaults_AreTheDefaultColumnOfTheFieldTables()
    {
        // 06 4.33: the shipping fee mark defaults to false; 06 4.34: the refund record is born Sending and unmarked. No other cell is filled.
        string[] expected =
        [
            "refund_records.is_marked_failed_by_admin = false",
            "refund_records.state = 'Sending'::text",
            "refunds.is_shipping_fee_included = false",
        ];

        List<string> defaults = await QueryAsync(
            "SELECT table_name || '.' || column_name || ' = ' || column_default FROM information_schema.columns WHERE table_schema = 'public' AND table_name = ANY(@names) AND column_default IS NOT NULL",
            ("names", FourTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), defaults.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ARowWrittenWithoutTheDefaultedColumns_GetsTheDefaults()
    {
        Order order = await SeedOrderAsync();
        Guid paymentId = EntityIds.New();
        Guid refundId = EntityIds.New();

        await ExecuteAsync(
            $"INSERT INTO payment_records (id, created_at, order_id, provider_transaction_id) VALUES ('{paymentId}', now(), '{order.Id}', 'tx-default');"
            + "INSERT INTO refunds (id, created_at, order_id, refund_type, payment_method, amount, payment_record_id, actor_kind)"
            + $" VALUES ('{refundId}', now(), '{order.Id}', 'LatePayment', 'Card', 120, '{paymentId}', 'System');"
            + $"INSERT INTO refund_records (id, created_at, refund_id) VALUES ('{EntityIds.New()}', now(), '{refundId}');");

        Assert.False(await ScalarAsync<bool>("SELECT is_shipping_fee_included FROM refunds"));
        Assert.Equal("Sending false", await ScalarAsync<string>("SELECT state || ' ' || is_marked_failed_by_admin::text FROM refund_records"));
    }

    [Fact]
    public async Task Indexes_AreTheDocumentedOnes()
    {
        // 06 7.1 (the filtered unique indexes), 7.5 (the disposal's scan) and the foreign key indexes of 7.2. Two or three indexes on the same
        // column take the framework's numbered name (D-55); one name is cut to the 63-byte limit by the framework (D-51).
        string[] expected =
        [
            "inbound_notification_records.ix_inbound_notification_records_created_at | CREATE INDEX ix_inbound_notification_records_created_at ON public.inbound_notification_records USING btree (created_at)",
            "inbound_notification_records.ix_inbound_notification_records_order_id | CREATE INDEX ix_inbound_notification_records_order_id ON public.inbound_notification_records USING btree (order_id)",
            "inbound_notification_records.ix_inbound_notification_records_provider_notification_id | CREATE UNIQUE INDEX ix_inbound_notification_records_provider_notification_id ON public.inbound_notification_records USING btree (provider_notification_id) WHERE (provider_notification_id IS NOT NULL)",
            "inbound_notification_records.ix_inbound_notification_records_provider_transaction_id_event_ | CREATE UNIQUE INDEX ix_inbound_notification_records_provider_transaction_id_event_ ON public.inbound_notification_records USING btree (provider_transaction_id, event_type, outcome) WHERE (provider_notification_id IS NULL)",
            "inbound_notification_records.ix_inbound_notification_records_refund_record_id | CREATE INDEX ix_inbound_notification_records_refund_record_id ON public.inbound_notification_records USING btree (refund_record_id)",
            "inbound_notification_records.pk_inbound_notification_records | CREATE UNIQUE INDEX pk_inbound_notification_records ON public.inbound_notification_records USING btree (id)",
            "payment_records.ix_payment_records_order_id | CREATE INDEX ix_payment_records_order_id ON public.payment_records USING btree (order_id)",
            "payment_records.ix_payment_records_provider_transaction_id | CREATE UNIQUE INDEX ix_payment_records_provider_transaction_id ON public.payment_records USING btree (provider_transaction_id) WHERE (provider_transaction_id IS NOT NULL)",
            "payment_records.ix_payment_records_transition_record_id | CREATE INDEX ix_payment_records_transition_record_id ON public.payment_records USING btree (transition_record_id)",
            "payment_records.pk_payment_records | CREATE UNIQUE INDEX pk_payment_records ON public.payment_records USING btree (id)",
            "refund_records.ix_refund_records_refund_id | CREATE INDEX ix_refund_records_refund_id ON public.refund_records USING btree (refund_id)",
            "refund_records.ix_refund_records_refund_id1 | CREATE UNIQUE INDEX ix_refund_records_refund_id1 ON public.refund_records USING btree (refund_id) WHERE (state = ANY (ARRAY['Sending'::text, 'OutcomePending'::text]))",
            "refund_records.ix_refund_records_refund_id2 | CREATE UNIQUE INDEX ix_refund_records_refund_id2 ON public.refund_records USING btree (refund_id) WHERE (state = 'Succeeded'::text)",
            "refund_records.pk_refund_records | CREATE UNIQUE INDEX pk_refund_records ON public.refund_records USING btree (id)",
            "refunds.ix_refunds_admin_id | CREATE INDEX ix_refunds_admin_id ON public.refunds USING btree (admin_id)",
            "refunds.ix_refunds_cancellation_id | CREATE INDEX ix_refunds_cancellation_id ON public.refunds USING btree (cancellation_id)",
            "refunds.ix_refunds_defect_claim_id | CREATE INDEX ix_refunds_defect_claim_id ON public.refunds USING btree (defect_claim_id)",
            "refunds.ix_refunds_item_removal_id | CREATE INDEX ix_refunds_item_removal_id ON public.refunds USING btree (item_removal_id)",
            "refunds.ix_refunds_late_delivery_termination_id | CREATE INDEX ix_refunds_late_delivery_termination_id ON public.refunds USING btree (late_delivery_termination_id)",
            "refunds.ix_refunds_not_returned_closure_id | CREATE INDEX ix_refunds_not_returned_closure_id ON public.refunds USING btree (not_returned_closure_id)",
            "refunds.ix_refunds_order_id | CREATE INDEX ix_refunds_order_id ON public.refunds USING btree (order_id)",
            "refunds.ix_refunds_order_id1 | CREATE UNIQUE INDEX ix_refunds_order_id1 ON public.refunds USING btree (order_id) WHERE (is_shipping_fee_included AND (voided_at IS NULL))",
            "refunds.ix_refunds_order_item_id | CREATE INDEX ix_refunds_order_item_id ON public.refunds USING btree (order_item_id)",
            "refunds.ix_refunds_payment_record_id | CREATE UNIQUE INDEX ix_refunds_payment_record_id ON public.refunds USING btree (payment_record_id) WHERE (payment_record_id IS NOT NULL)",
            "refunds.ix_refunds_return_id | CREATE INDEX ix_refunds_return_id ON public.refunds USING btree (return_id)",
            "refunds.ix_refunds_withdrawal_id | CREATE INDEX ix_refunds_withdrawal_id ON public.refunds USING btree (withdrawal_id)",
            "refunds.pk_refunds | CREATE UNIQUE INDEX pk_refunds ON public.refunds USING btree (id)",
        ];

        List<string> indexes = await QueryAsync(
            "SELECT tablename || '.' || indexname || ' | ' || indexdef FROM pg_indexes WHERE schemaname = 'public' AND tablename = ANY(@names)",
            ("names", FourTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), indexes.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ForeignKeys_HaveTheDeleteBehaviorOfTheLinkTable()
    {
        // 06 7.2: the refund's defect claim and administrator links are emptied (n); every other link is restrictive (a). One name is cut to
        // the 63-byte limit by the framework (D-51): the refund's termination link.
        string[] expected =
        [
            "inbound_notification_records fk_inbound_notification_records_orders_order_id a",
            "inbound_notification_records fk_inbound_notification_records_refund_records_refund_record_id a",
            "payment_records fk_payment_records_orders_order_id a",
            "payment_records fk_payment_records_transition_records_transition_record_id a",
            "refund_records fk_refund_records_refunds_refund_id a",
            "refunds fk_refunds_admins_admin_id n",
            "refunds fk_refunds_cancellations_cancellation_id a",
            "refunds fk_refunds_defect_claims_defect_claim_id n",
            "refunds fk_refunds_item_removals_item_removal_id a",
            "refunds fk_refunds_late_delivery_terminations_late_delivery_terminatio a",
            "refunds fk_refunds_not_returned_closures_not_returned_closure_id a",
            "refunds fk_refunds_order_items_order_item_id a",
            "refunds fk_refunds_orders_order_id a",
            "refunds fk_refunds_payment_records_payment_record_id a",
            "refunds fk_refunds_returns_return_id a",
            "refunds fk_refunds_withdrawals_withdrawal_id a",
        ];

        List<string> links = await QueryAsync(
            "SELECT conrelid::regclass::text || ' ' || conname || ' ' || confdeltype::text FROM pg_constraint WHERE contype = 'f' AND conrelid = ANY(ARRAY(SELECT x::regclass FROM unnest(@oids) x))",
            ("oids", FourTables.Select(t => "public." + t).ToArray()));

        Assert.Equal(expected.Order(StringComparer.Ordinal), links.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task CheckConstraints_AreTheDocumentedOnes()
    {
        // 06 7.3 and the enum columns' allowed values (06 2.9.2).
        string[] expected =
        [
            "inbound_notification_records.ck_inbound_notification_records_amount_not_negative",
            "inbound_notification_records.ck_inbound_notification_records_notification_or_transaction",
            "inbound_notification_records.ck_inbound_notification_records_reason_only_when_processed",
            "inbound_notification_records.ck_inbound_notification_records_unprocessed_reason",
            "payment_records.ck_payment_records_transaction_or_transition",
            "refund_records.ck_refund_records_admin_mark_is_failed",
            "refund_records.ck_refund_records_late_success_only_when_marked",
            "refund_records.ck_refund_records_outcome_at_with_outcome",
            "refund_records.ck_refund_records_state",
            "refunds.ck_refunds_actor_kind",
            "refunds.ck_refunds_amount_above_zero",
            "refunds.ck_refunds_coupon_share_not_negative",
            "refunds.ck_refunds_links_by_type",
            "refunds.ck_refunds_not_returned_closure_only_amount_based",
            "refunds.ck_refunds_not_returned_closure_with_item",
            "refunds.ck_refunds_payment_method",
            "refunds.ck_refunds_quantity_above_zero",
            "refunds.ck_refunds_refund_type",
            "refunds.ck_refunds_sent_or_voided",
            "refunds.ck_refunds_shipping_fee_only_on_item_records",
            "refunds.ck_refunds_system_without_admin",
            "refunds.ck_refunds_vat_amount_not_negative",
        ];

        List<string> checks = await QueryAsync(
            "SELECT conrelid::regclass::text || '.' || conname FROM pg_constraint WHERE contype = 'c' AND conrelid = ANY(ARRAY(SELECT x::regclass FROM unnest(@oids) x))",
            ("oids", FourTables.Select(t => "public." + t).ToArray()));

        Assert.Equal(expected.Order(StringComparer.Ordinal), checks.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task EnumColumns_AllowTheDocumentedValues()
    {
        // 06 4.33: the refund's actor is the system or an administrator; the other enum columns allow every member (06 5.3.5, 5.4.1, 5.6.6, 5.6.7).
        string[] expected =
        [
            "ck_inbound_notification_records_unprocessed_reason | CHECK ((unprocessed_reason = ANY (ARRAY['AmountMismatch'::text, 'OrderNotFound'::text, 'RefundMarkedFailed'::text, 'UnrecognizedNotification'::text])))",
            "ck_refund_records_state | CHECK ((state = ANY (ARRAY['Sending'::text, 'OutcomePending'::text, 'Succeeded'::text, 'Failed'::text])))",
            "ck_refunds_actor_kind | CHECK ((actor_kind = ANY (ARRAY['System'::text, 'Admin'::text])))",
            "ck_refunds_payment_method | CHECK ((payment_method = ANY (ARRAY['Card'::text, 'BankTransfer'::text])))",
            "ck_refunds_refund_type | CHECK ((refund_type = ANY (ARRAY['Cancellation'::text, 'ItemRemoval'::text, 'LateDeliveryTermination'::text, 'Withdrawal'::text, 'DefectClaimRescission'::text, 'AmountBased'::text, 'LatePayment'::text])))",
        ];

        List<string> checks = await QueryAsync(
            "SELECT conname || ' | ' || pg_get_constraintdef(oid) FROM pg_constraint WHERE contype = 'c' AND conname = ANY(@names)",
            ("names", expected.Select(e => e[..e.IndexOf(' ', StringComparison.Ordinal)]).ToArray()));

        Assert.Equal(expected.Order(StringComparer.Ordinal), checks.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Migration_HasNoDownMethod()
    {
        MethodInfo? down = typeof(CreatePaymentAndRefundTables)
            .GetMethod("Down", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);

        Assert.Null(down);
    }

    // ---- KK2: the payment record ----

    [Fact]
    public async Task SecondPaymentWithTheSameTransactionId_IsRefused()
    {
        Order order = await SeedOrderAsync();
        Order other = await SeedOrderAsync();
        await SaveAsync(c => c.PaymentRecords.Add(PaymentRecord.ForCardPayment(order.Id, "tx-1", null, Now)));

        Assert.Equal(
            (UniqueViolation, "ix_payment_records_provider_transaction_id"),
            await ViolationOfAsync(c => c.PaymentRecords.Add(PaymentRecord.ForCardPayment(other.Id, "tx-1", null, Now))));
    }

    [Fact]
    public async Task PaymentsWithoutATransactionId_AreNotUniqueByIt()
    {
        // The filter keeps the empty rows out: two bank transfer marks (no transaction id) are accepted.
        Order order = await SeedOrderAsync();
        TransitionRecord first = await SeedTransitionAsync(order.Id);
        TransitionRecord second = await SeedTransitionAsync(order.Id);

        await SaveAsync(c => c.PaymentRecords.AddRange(
            PaymentRecord.ForBankTransferMark(order.Id, first.Id, Now),
            PaymentRecord.ForBankTransferMark(order.Id, second.Id, Now),
            PaymentRecord.ForCardPayment(order.Id, "tx-2", null, Now)));

        Assert.Equal(3L, await ScalarAsync<long>("SELECT count(*) FROM payment_records"));
    }

    [Fact]
    public async Task PaymentWithoutATransactionIdAndATransition_IsRefused()
    {
        Order order = await SeedOrderAsync();
        PaymentRecord payment = PaymentRecord.ForCardPayment(order.Id, "tx-3", null, Now);
        payment.ProviderTransactionId = null;

        Assert.Equal((CheckViolation, "ck_payment_records_transaction_or_transition"), await ViolationOfAsync(c => c.PaymentRecords.Add(payment)));
    }

    [Fact]
    public async Task CardPaymentWithItsTransition_IsAccepted()
    {
        Order order = await SeedOrderAsync();
        TransitionRecord transition = await SeedTransitionAsync(order.Id);

        await SaveAsync(c => c.PaymentRecords.Add(PaymentRecord.ForCardPayment(order.Id, "tx-4", transition.Id, Now)));

        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM payment_records WHERE transition_record_id = '{transition.Id}' AND provider_transaction_id = 'tx-4'"));
    }

    // ---- KK3: the refund ----

    [Theory]
    [MemberData(nameof(AllTypes))]
    public async Task RefundOfEachType_WithTheLinksOfItsType_IsAccepted(RefundType type)
    {
        Links links = await SeedLinksAsync();

        await SaveAsync(c => c.Refunds.Add(NewRefund(type, links)));

        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM refunds WHERE refund_type = '{type}'"));
    }

    [Fact]
    public async Task RefundWithTheOptionalLinksEmpty_IsAccepted()
    {
        // The withdrawal before delivery has no receipt; the rescission of a digital item has no receipt and the deleted claim's link is
        // emptied; the amount-based refund may carry no item (06 4.33's type table).
        Links links = await SeedLinksAsync();
        Refund withdrawal = NewRefund(RefundType.Withdrawal, links);
        withdrawal.ReturnId = null;
        Refund rescission = NewRefund(RefundType.DefectClaimRescission, links);
        (rescission.ReturnId, rescission.DefectClaimId) = (null, null);
        Refund amountBased = NewRefund(RefundType.AmountBased, links);
        (amountBased.NotReturnedClosureId, amountBased.OrderItemId) = (null, null);

        await SaveAsync(c => c.Refunds.AddRange(withdrawal, rescission, amountBased));

        Assert.Equal(3L, await ScalarAsync<long>("SELECT count(*) FROM refunds"));
    }

    [Theory]
    [MemberData(nameof(AllTypes))]
    public async Task RefundWithoutTheRecordLinkOfItsType_IsRefused(RefundType type)
    {
        Links links = await SeedLinksAsync();
        Refund refund = NewRefund(type, links);
        Action breakTheLink = type switch
        {
            RefundType.Cancellation => () => refund.CancellationId = null,
            RefundType.ItemRemoval => () => refund.ItemRemovalId = null,
            RefundType.LateDeliveryTermination => () => refund.LateDeliveryTerminationId = null,
            RefundType.Withdrawal => () => refund.WithdrawalId = null,
            RefundType.DefectClaimRescission => () => refund.CouponShare = null,
            RefundType.AmountBased => () => refund.Quantity = 1,
            RefundType.LatePayment => () => refund.PaymentRecordId = null,
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };
        breakTheLink();

        Assert.Equal((CheckViolation, "ck_refunds_links_by_type"), await ViolationOfAsync(c => c.Refunds.Add(refund)));
    }

    [Theory]
    [MemberData(nameof(ItemTypesOtherThanAmountBased))]
    public async Task RefundOfAnItemRecordType_WithoutItsItemOrUnits_IsRefused(RefundType type)
    {
        Links links = await SeedLinksAsync();
        Refund withoutItem = NewRefund(type, links);
        withoutItem.OrderItemId = null;
        Refund withoutUnits = NewRefund(type, links);
        withoutUnits.Quantity = null;

        Assert.Equal((CheckViolation, "ck_refunds_links_by_type"), await ViolationOfAsync(c => c.Refunds.Add(withoutItem)));
        Assert.Equal((CheckViolation, "ck_refunds_links_by_type"), await ViolationOfAsync(c => c.Refunds.Add(withoutUnits)));
    }

    [Fact]
    public async Task RefundWithALinkItsTypeDoesNotName_IsRefused()
    {
        // "The links the type does not name are empty" (06 4.33): a cancellation's refund with a removal, a withdrawal's with a defect claim,
        // a removal's with a coupon share, an amount-based with a cancellation, a late payment's with an item.
        Links links = await SeedLinksAsync();
        Refund cancellation = NewRefund(RefundType.Cancellation, links);
        cancellation.ItemRemovalId = links.ItemRemovalId;
        Refund withdrawal = NewRefund(RefundType.Withdrawal, links);
        withdrawal.DefectClaimId = links.DefectClaimId;
        Refund removal = NewRefund(RefundType.ItemRemoval, links);
        removal.CouponShare = 1m;
        Refund amountBased = NewRefund(RefundType.AmountBased, links);
        amountBased.CancellationId = links.CancellationId;
        Refund latePayment = NewRefund(RefundType.LatePayment, links);
        latePayment.OrderItemId = links.ItemId;

        foreach (Refund refund in new[] { cancellation, withdrawal, removal, amountBased, latePayment })
        {
            Assert.Equal((CheckViolation, "ck_refunds_links_by_type"), await ViolationOfAsync(c => c.Refunds.Add(refund)));
        }
    }

    [Theory]
    [MemberData(nameof(ItemTypesOtherThanAmountBased))]
    public async Task ClosureLinkOnATypeOtherThanAmountBased_IsRefused(RefundType type)
    {
        Links links = await SeedLinksAsync();
        Refund refund = NewRefund(type, links);
        refund.NotReturnedClosureId = links.ClosureId;

        Assert.Equal((CheckViolation, "ck_refunds_not_returned_closure_only_amount_based"), await ViolationOfAsync(c => c.Refunds.Add(refund)));
    }

    [Fact]
    public async Task ClosureLinkOnTheLatePayment_IsRefused()
    {
        Links links = await SeedLinksAsync();
        Refund refund = NewRefund(RefundType.LatePayment, links);
        refund.NotReturnedClosureId = links.ClosureId;

        (string state, string? constraint) = await ViolationOfAsync(c => c.Refunds.Add(refund));

        Assert.Equal(CheckViolation, state);
        Assert.StartsWith("ck_refunds_not_returned_closure_", constraint, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AmountBasedRefundOfAClosure_WithoutTheItem_IsRefused()
    {
        Links links = await SeedLinksAsync();
        Refund refund = NewRefund(RefundType.AmountBased, links);
        refund.OrderItemId = null;

        Assert.Equal((CheckViolation, "ck_refunds_not_returned_closure_with_item"), await ViolationOfAsync(c => c.Refunds.Add(refund)));
    }

    [Theory]
    [MemberData(nameof(ShippingFeeTypes))]
    public async Task ShippingFee_OnACancellationRemovalTerminationOrWithdrawal_IsAccepted(RefundType type)
    {
        Links links = await SeedLinksAsync();
        Refund refund = NewRefund(type, links);
        refund.IsShippingFeeIncluded = true;

        await SaveAsync(c => c.Refunds.Add(refund));

        Assert.True(await ScalarAsync<bool>("SELECT is_shipping_fee_included FROM refunds"));
    }

    [Theory]
    [MemberData(nameof(NoShippingFeeTypes))]
    public async Task ShippingFee_OnARescissionAmountBasedOrLatePayment_IsRefused(RefundType type)
    {
        Links links = await SeedLinksAsync();
        Refund refund = NewRefund(type, links);
        refund.IsShippingFeeIncluded = true;

        Assert.Equal((CheckViolation, "ck_refunds_shipping_fee_only_on_item_records"), await ViolationOfAsync(c => c.Refunds.Add(refund)));
    }

    [Fact]
    public async Task SecondShippingFeeRefundOfTheOrder_IsRefused()
    {
        Links links = await SeedLinksAsync();
        Refund first = NewRefund(RefundType.Cancellation, links);
        first.IsShippingFeeIncluded = true;
        await SaveAsync(c => c.Refunds.Add(first));
        Refund second = NewRefund(RefundType.Withdrawal, links);
        second.IsShippingFeeIncluded = true;

        Assert.Equal((UniqueViolation, "ix_refunds_order_id1"), await ViolationOfAsync(c => c.Refunds.Add(second)));
    }

    [Fact]
    public async Task ShippingFeeRefund_AfterTheFirstIsVoided_OrOnAnotherOrder_IsAccepted()
    {
        // The filter reads the refunds that are not voided (06 7.1): the voided refund's fee is paid back again; another order has its own.
        Links links = await SeedLinksAsync();
        Links otherOrder = await SeedLinksAsync();
        Refund voided = NewRefund(RefundType.Cancellation, links);
        (voided.IsShippingFeeIncluded, voided.VoidedAt) = (true, Now);
        Refund again = NewRefund(RefundType.Cancellation, links);
        again.IsShippingFeeIncluded = true;
        Refund other = NewRefund(RefundType.Cancellation, otherOrder);
        other.IsShippingFeeIncluded = true;
        Refund withoutFee = NewRefund(RefundType.ItemRemoval, links);

        await SaveAsync(c => c.Refunds.AddRange(voided, again, other, withoutFee));

        Assert.Equal(3L, await ScalarAsync<long>("SELECT count(*) FROM refunds WHERE is_shipping_fee_included"));
    }

    [Fact]
    public async Task SecondRefundOfTheSamePaymentRecord_IsRefused()
    {
        Links links = await SeedLinksAsync();
        await SaveAsync(c => c.Refunds.Add(NewRefund(RefundType.LatePayment, links)));

        Assert.Equal(
            (UniqueViolation, "ix_refunds_payment_record_id"),
            await ViolationOfAsync(c => c.Refunds.Add(NewRefund(RefundType.LatePayment, links))));
    }

    [Fact]
    public async Task RefundBothSentAndVoided_IsRefused()
    {
        Links links = await SeedLinksAsync();
        Refund refund = NewRefund(RefundType.Cancellation, links);
        (refund.SentAt, refund.VoidedAt) = (Now, Now);

        Assert.Equal((CheckViolation, "ck_refunds_sent_or_voided"), await ViolationOfAsync(c => c.Refunds.Add(refund)));
    }

    [Fact]
    public async Task RefundAmounts_OutsideTheirRules_AreRefused()
    {
        // 06 4.33: Amount > 0; Quantity > 0; CouponShare >= 0; VatAmount >= 0.
        Links links = await SeedLinksAsync();
        Refund zeroAmount = NewRefund(RefundType.Cancellation, links);
        zeroAmount.Amount = 0m;
        Refund zeroUnits = NewRefund(RefundType.Cancellation, links);
        zeroUnits.Quantity = 0;
        Refund negativeShare = NewRefund(RefundType.DefectClaimRescission, links);
        negativeShare.CouponShare = -0.01m;
        Refund negativeVat = NewRefund(RefundType.DefectClaimRescission, links);
        negativeVat.VatAmount = -0.01m;

        Assert.Equal((CheckViolation, "ck_refunds_amount_above_zero"), await ViolationOfAsync(c => c.Refunds.Add(zeroAmount)));
        Assert.Equal((CheckViolation, "ck_refunds_quantity_above_zero"), await ViolationOfAsync(c => c.Refunds.Add(zeroUnits)));
        Assert.Equal((CheckViolation, "ck_refunds_coupon_share_not_negative"), await ViolationOfAsync(c => c.Refunds.Add(negativeShare)));
        Assert.Equal((CheckViolation, "ck_refunds_vat_amount_not_negative"), await ViolationOfAsync(c => c.Refunds.Add(negativeVat)));
    }

    [Fact]
    public async Task RefundAmounts_AtTheLimitsOfTheRules_AreAccepted_AndKeepTheirKurus()
    {
        Links links = await SeedLinksAsync();
        Refund refund = NewRefund(RefundType.DefectClaimRescission, links);
        (refund.Amount, refund.CouponShare, refund.VatAmount) = (0.01m, 0m, 0m);
        Refund largest = NewRefund(RefundType.AmountBased, links);
        largest.Amount = 9_999_999_999.99m;

        await SaveAsync(c => c.Refunds.AddRange(refund, largest));

        Assert.Equal(
            "0.01 0.00 0.00",
            await ScalarAsync<string>($"SELECT amount::text || ' ' || coupon_share::text || ' ' || vat_amount::text FROM refunds WHERE id = '{refund.Id}'"));
        Assert.Equal("9999999999.99", await ScalarAsync<string>($"SELECT amount::text FROM refunds WHERE id = '{largest.Id}'"));
    }

    [Fact]
    public async Task RefundAmount_AboveTheMoneyType_IsRefused()
    {
        Links links = await SeedLinksAsync();
        Refund refund = NewRefund(RefundType.AmountBased, links);
        refund.Amount = 10_000_000_000m;

        Assert.Equal((NumericValueOutOfRange, null), await ViolationOfAsync(c => c.Refunds.Add(refund)));
    }

    [Fact]
    public async Task SystemRefund_WithAnAdministratorField_IsRefused()
    {
        Links links = await SeedLinksAsync();
        Refund withLink = NewRefund(RefundType.LatePayment, links);
        withLink.AdminId = links.AdminId;
        Refund withName = NewRefund(RefundType.Cancellation, links);
        withName.AdminName = "Veli";

        Assert.Equal((CheckViolation, "ck_refunds_system_without_admin"), await ViolationOfAsync(c => c.Refunds.Add(withLink)));
        Assert.Equal((CheckViolation, "ck_refunds_system_without_admin"), await ViolationOfAsync(c => c.Refunds.Add(withName)));
    }

    [Fact]
    public async Task RefundByACustomer_IsRefused()
    {
        Links links = await SeedLinksAsync();
        Refund refund = NewRefund(RefundType.Cancellation, links);
        refund.ActorKind = ActorKind.Buyer;

        Assert.Equal((CheckViolation, "ck_refunds_actor_kind"), await ViolationOfAsync(c => c.Refunds.Add(refund)));
    }

    // ---- KK4: the refund record ----

    [Theory]
    [MemberData(nameof(OpenStates))]
    public async Task OpenRefundRecord_WithAnOutcomeMoment_IsRefused(RefundRecordState state)
    {
        Refund refund = await SeedRefundAsync();
        RefundRecord record = RefundRecord.Sending(refund.Id, Now);
        (record.State, record.OutcomeAt) = (state, Now);

        Assert.Equal((CheckViolation, "ck_refund_records_outcome_at_with_outcome"), await ViolationOfAsync(c => c.RefundRecords.Add(record)));
    }

    [Theory]
    [MemberData(nameof(OutcomeStates))]
    public async Task RefundRecordWithAnOutcome_WithoutTheOutcomeMoment_IsRefused(RefundRecordState state)
    {
        Refund refund = await SeedRefundAsync();
        RefundRecord record = RefundRecord.Sending(refund.Id, Now);
        record.State = state;

        Assert.Equal((CheckViolation, "ck_refund_records_outcome_at_with_outcome"), await ViolationOfAsync(c => c.RefundRecords.Add(record)));
    }

    [Fact]
    public async Task AdminMark_OnARecordThatIsNotFailed_IsRefused()
    {
        Refund refund = await SeedRefundAsync();
        RefundRecord pending = RefundRecord.Sending(refund.Id, Now);
        (pending.State, pending.IsMarkedFailedByAdmin) = (RefundRecordState.OutcomePending, true);
        RefundRecord succeeded = RefundRecord.Sending(refund.Id, Now);
        (succeeded.State, succeeded.OutcomeAt, succeeded.IsMarkedFailedByAdmin) = (RefundRecordState.Succeeded, Now, true);

        Assert.Equal((CheckViolation, "ck_refund_records_admin_mark_is_failed"), await ViolationOfAsync(c => c.RefundRecords.Add(pending)));
        Assert.Equal((CheckViolation, "ck_refund_records_admin_mark_is_failed"), await ViolationOfAsync(c => c.RefundRecords.Add(succeeded)));
    }

    [Fact]
    public async Task LateSuccess_OnARecordWithoutTheAdminMark_IsRefused()
    {
        Refund refund = await SeedRefundAsync();
        RefundRecord record = RefundRecord.Sending(refund.Id, Now);
        (record.State, record.OutcomeAt, record.LateSuccessAt) = (RefundRecordState.Failed, Now, Now);

        Assert.Equal((CheckViolation, "ck_refund_records_late_success_only_when_marked"), await ViolationOfAsync(c => c.RefundRecords.Add(record)));
    }

    [Fact]
    public async Task MarkedRecordWithALateSuccess_IsAccepted()
    {
        Refund refund = await SeedRefundAsync();
        RefundRecord record = RefundRecord.Sending(refund.Id, Now);
        (record.State, record.OutcomeAt, record.IsMarkedFailedByAdmin, record.LateSuccessAt) = (RefundRecordState.Failed, Now, true, Now.AddHours(1));

        await SaveAsync(c => c.RefundRecords.Add(record));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM refund_records WHERE is_marked_failed_by_admin AND late_success_at IS NOT NULL"));
    }

    [Theory]
    [MemberData(nameof(OpenStates))]
    public async Task SecondOpenRequestOfTheRefund_IsRefused(RefundRecordState secondState)
    {
        Refund refund = await SeedRefundAsync();
        await SaveAsync(c => c.RefundRecords.Add(RefundRecord.Sending(refund.Id, Now)));
        RefundRecord second = RefundRecord.Sending(refund.Id, Now);
        second.State = secondState;

        Assert.Equal((UniqueViolation, "ix_refund_records_refund_id1"), await ViolationOfAsync(c => c.RefundRecords.Add(second)));
    }

    [Fact]
    public async Task SecondSuccessfulRequestOfTheRefund_IsRefused()
    {
        Refund refund = await SeedRefundAsync();
        await SaveAsync(c => c.RefundRecords.Add(Outcome(refund.Id, RefundRecordState.Succeeded)));

        Assert.Equal(
            (UniqueViolation, "ix_refund_records_refund_id2"),
            await ViolationOfAsync(c => c.RefundRecords.Add(Outcome(refund.Id, RefundRecordState.Succeeded))));
    }

    [Fact]
    public async Task FailedRequests_AndOneOpenAndOneSuccessfulPerRefund_AreAccepted()
    {
        // The retries after failures are new records of the same refund; another refund has its own open request (06 4.34).
        Refund refund = await SeedRefundAsync();
        Refund other = await SeedRefundAsync();

        await SaveAsync(c => c.RefundRecords.AddRange(
            Outcome(refund.Id, RefundRecordState.Failed),
            Outcome(refund.Id, RefundRecordState.Failed),
            RefundRecord.Sending(refund.Id, Now),
            Outcome(refund.Id, RefundRecordState.Succeeded),
            RefundRecord.Sending(other.Id, Now)));

        Assert.Equal(5L, await ScalarAsync<long>("SELECT count(*) FROM refund_records"));
    }

    // ---- KK5: the inbound notification record ----

    [Fact]
    public async Task NotificationWithoutAnyId_IsRefused()
    {
        InboundNotificationRecord notification = InboundNotificationRecord.Create(null, null, "payment", "success", "2026-000001", 120m, Now);

        Assert.Equal(
            (CheckViolation, "ck_inbound_notification_records_notification_or_transaction"),
            await ViolationOfAsync(c => c.InboundNotificationRecords.Add(notification)));
    }

    [Fact]
    public async Task SameNotificationId_IsRefused()
    {
        await SaveAsync(c => c.InboundNotificationRecords.Add(InboundNotificationRecord.Create("n-1", "tx-1", "payment", "success", null, 120m, Now)));

        Assert.Equal(
            (UniqueViolation, "ix_inbound_notification_records_provider_notification_id"),
            await ViolationOfAsync(c => c.InboundNotificationRecords.Add(InboundNotificationRecord.Create("n-1", "tx-2", "refund", "failure", null, null, Now))));
    }

    [Fact]
    public async Task SameNotificationWithoutAnId_IsRefused()
    {
        await SaveAsync(c => c.InboundNotificationRecords.Add(InboundNotificationRecord.Create(null, "tx-1", "payment", "success", null, 120m, Now)));

        Assert.Equal(
            (UniqueViolation, "ix_inbound_notification_records_provider_transaction_id_event_"),
            await ViolationOfAsync(c => c.InboundNotificationRecords.Add(InboundNotificationRecord.Create(null, "tx-1", "payment", "success", null, 120m, Now))));
    }

    [Fact]
    public async Task OtherEventsOfTheSameTransaction_AndNotificationsWithIds_AreAccepted()
    {
        // A failed attempt then a successful one, and a refund's outcome, are separate notifications (06 4.35); the rows with an id are unique
        // by the id only.
        await SaveAsync(c => c.InboundNotificationRecords.AddRange(
            InboundNotificationRecord.Create(null, "tx-1", "payment", "failure", null, 120m, Now),
            InboundNotificationRecord.Create(null, "tx-1", "payment", "success", null, 120m, Now),
            InboundNotificationRecord.Create(null, "tx-1", "refund", "success", null, 120m, Now),
            InboundNotificationRecord.Create("n-1", "tx-1", "payment", "success", null, 120m, Now),
            InboundNotificationRecord.Create("n-2", "tx-1", "payment", "success", null, 120m, Now)));

        Assert.Equal(5L, await ScalarAsync<long>("SELECT count(*) FROM inbound_notification_records"));
    }

    [Fact]
    public async Task NotificationAmount_BelowZero_IsRefused_AndZeroOrEmptyIsAccepted()
    {
        Assert.Equal(
            (CheckViolation, "ck_inbound_notification_records_amount_not_negative"),
            await ViolationOfAsync(c => c.InboundNotificationRecords.Add(InboundNotificationRecord.Create("n-1", null, "payment", "success", null, -0.01m, Now))));

        await SaveAsync(c => c.InboundNotificationRecords.AddRange(
            InboundNotificationRecord.Create("n-2", null, "payment", "success", null, 0m, Now),
            InboundNotificationRecord.Create("n-3", null, "payment", "success", null, null, Now)));

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM inbound_notification_records"));
    }

    [Fact]
    public async Task UnprocessedReason_WithoutTheProcessingMoment_IsRefused()
    {
        InboundNotificationRecord notification = InboundNotificationRecord.Create("n-1", null, "payment", "success", "2026-999999", 120m, Now);
        notification.UnprocessedReason = UnprocessedNotificationReason.OrderNotFound;

        Assert.Equal(
            (CheckViolation, "ck_inbound_notification_records_reason_only_when_processed"),
            await ViolationOfAsync(c => c.InboundNotificationRecords.Add(notification)));
    }

    [Fact]
    public async Task ProcessedNotification_LinkedToItsOrderAndRefundRecord_IsAccepted()
    {
        Refund refund = await SeedRefundAsync();
        RefundRecord record = RefundRecord.Sending(refund.Id, Now);
        await SaveAsync(c => c.RefundRecords.Add(record));
        InboundNotificationRecord notification = InboundNotificationRecord.Create("n-1", "tx-9", "refund", "success", "2026-000001", 120m, Now);
        (notification.OrderId, notification.RefundRecordId, notification.ProcessedAt) = (refund.OrderId, record.Id, Now);
        InboundNotificationRecord unprocessed = InboundNotificationRecord.Create("n-2", null, "payment", "success", "2026-999999", 120m, Now);
        (unprocessed.ProcessedAt, unprocessed.UnprocessedReason) = (Now, UnprocessedNotificationReason.OrderNotFound);

        await SaveAsync(c => c.InboundNotificationRecords.AddRange(notification, unprocessed));

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM inbound_notification_records WHERE processed_at IS NOT NULL"));
    }

    // ---- KK6: the money type and the deletion behavior ----

    [Fact]
    public async Task MoneyColumns_AreNumeric12And2()
    {
        // 06 2.5.1: every amount of the four tables is numeric(12,2).
        string[] expected =
        [
            "inbound_notification_records.amount numeric(12,2)",
            "refunds.amount numeric(12,2)",
            "refunds.coupon_share numeric(12,2)",
            "refunds.vat_amount numeric(12,2)",
        ];

        List<string> columns = await QueryAsync(
            "SELECT table_name || '.' || column_name || ' ' || data_type || '(' || numeric_precision || ',' || numeric_scale || ')'"
            + " FROM information_schema.columns WHERE table_schema = 'public' AND table_name = ANY(@names) AND data_type = 'numeric'",
            ("names", FourTables));

        Assert.Equal(expected, columns.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task DeletingTheDefectClaim_KeepsTheRefund_AndEmptiesItsLink()
    {
        // 06 4.30: the claim is deleted by its retention, by the disposal's explicit delete; the refund's link to it is emptied (06 4.33).
        Links links = await SeedLinksAsync();
        Refund refund = NewRefund(RefundType.DefectClaimRescission, links);
        await SaveAsync(c => c.Refunds.Add(refund));

        await ExecuteAsync($"DELETE FROM defect_claims WHERE id = '{links.DefectClaimId}'");

        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM refunds WHERE id = '{refund.Id}' AND defect_claim_id IS NULL"));
    }

    [Fact]
    public async Task DeletingTheAdmin_KeepsTheRefund_EmptiesItsLinkAndKeepsTheName()
    {
        Links links = await SeedLinksAsync();
        Refund refund = Refund.ByAdmin(links.OrderId, RefundType.AmountBased, PaymentMethod.BankTransfer, 50m, links.AdminId, "Veli", Now);
        refund.SentAt = Now;
        await SaveAsync(c => c.Refunds.Add(refund));

        await ExecuteAsync($"DELETE FROM admins WHERE id = '{links.AdminId}'");

        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM refunds WHERE id = '{refund.Id}' AND admin_id IS NULL AND admin_name = 'Veli'"));
    }

    [Fact]
    public async Task DeletingALinkedPrincipal_IsRefusedByTheDatabase()
    {
        // Every link but the defect claim's and the administrator's is restrictive (06 7.2): each principal below is held only by the row
        // under test, so its deletion names that row's link.
        Links links = await SeedLinksAsync();
        Order bare = await SeedOrderAsync();
        (_, OrderItem bareItem) = await SeedItemAsync();
        Refund latePayment = NewRefund(RefundType.LatePayment, links);
        Refund cancellation = NewRefund(RefundType.Cancellation, links);
        Refund removal = NewRefund(RefundType.ItemRemoval, links);
        Refund termination = NewRefund(RefundType.LateDeliveryTermination, links);
        Refund withdrawal = NewRefund(RefundType.Withdrawal, links);
        withdrawal.ReturnId = null;
        Refund rescission = NewRefund(RefundType.DefectClaimRescission, links);
        rescission.DefectClaimId = null;
        Refund closure = NewRefund(RefundType.AmountBased, links);
        Refund ofBareOrder = Refund.BySystem(bare.Id, RefundType.AmountBased, PaymentMethod.Card, 10m, Now);
        Refund ofBareItem = NewRefund(RefundType.AmountBased, links);
        (ofBareItem.NotReturnedClosureId, ofBareItem.OrderItemId) = (null, bareItem.Id);
        await SaveAsync(c => c.Refunds.AddRange(latePayment, cancellation, removal, termination, withdrawal, rescission, closure, ofBareOrder, ofBareItem));
        RefundRecord record = RefundRecord.Sending(cancellation.Id, Now);
        await SaveAsync(c => c.RefundRecords.Add(record));
        Order notifiedOrder = await SeedOrderAsync();
        InboundNotificationRecord notification = InboundNotificationRecord.Create("n-1", null, "refund", "success", null, null, Now);
        notification.RefundRecordId = record.Id;
        InboundNotificationRecord orderNotification = InboundNotificationRecord.Create("n-2", null, "payment", "success", null, null, Now);
        orderNotification.OrderId = notifiedOrder.Id;
        await SaveAsync(c => c.InboundNotificationRecords.AddRange(notification, orderNotification));

        foreach ((string statement, string constraint) in new[]
        {
            ($"DELETE FROM orders WHERE id = '{bare.Id}'", "fk_refunds_orders_order_id"),
            ($"DELETE FROM order_items WHERE id = '{bareItem.Id}'", "fk_refunds_order_items_order_item_id"),
            ($"DELETE FROM payment_records WHERE id = '{links.PaymentRecordId}'", "fk_refunds_payment_records_payment_record_id"),
            ($"DELETE FROM cancellations WHERE id = '{links.CancellationId}'", "fk_refunds_cancellations_cancellation_id"),
            ($"DELETE FROM item_removals WHERE id = '{links.ItemRemovalId}'", "fk_refunds_item_removals_item_removal_id"),
            ($"DELETE FROM late_delivery_terminations WHERE id = '{links.TerminationId}'", "fk_refunds_late_delivery_terminations_late_delivery_terminatio"),
            ($"DELETE FROM not_returned_closures WHERE id = '{links.ClosureId}'", "fk_refunds_not_returned_closures_not_returned_closure_id"),
            ($"DELETE FROM returns WHERE id = '{links.ReturnId}'", "fk_refunds_returns_return_id"),
            ($"DELETE FROM refunds WHERE id = '{cancellation.Id}'", "fk_refund_records_refunds_refund_id"),
            ($"DELETE FROM refund_records WHERE id = '{record.Id}'", "fk_inbound_notification_records_refund_records_refund_record_id"),
            ($"DELETE FROM orders WHERE id = '{notifiedOrder.Id}'", "fk_inbound_notification_records_orders_order_id"),
        })
        {
            PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(statement));
            Assert.Equal((ForeignKeyViolation, constraint), (error.SqlState, error.ConstraintName));
        }
    }

    [Fact]
    public async Task DeletingThePaymentRecordsPrincipals_IsRefusedByTheDatabase()
    {
        Order order = await SeedOrderAsync();
        Order other = await SeedOrderAsync();
        TransitionRecord transition = await SeedTransitionAsync(other.Id);
        await SaveAsync(c => c.PaymentRecords.AddRange(
            PaymentRecord.ForCardPayment(order.Id, "tx-1", null, Now),
            PaymentRecord.ForBankTransferMark(other.Id, transition.Id, Now)));

        PostgresException ofOrder = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM orders WHERE id = '{order.Id}'"));
        PostgresException ofTransition = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM transition_records WHERE id = '{transition.Id}'"));

        Assert.Equal((ForeignKeyViolation, "fk_payment_records_orders_order_id"), (ofOrder.SqlState, ofOrder.ConstraintName));
        Assert.Equal((ForeignKeyViolation, "fk_payment_records_transition_records_transition_record_id"), (ofTransition.SqlState, ofTransition.ConstraintName));
    }

    [Fact]
    public async Task DeletingTheWithdrawalOfARefund_IsRefusedByTheDatabase()
    {
        // The withdrawal is also held by the closure and the receipt of the seeded links; a withdrawal of its own isolates the refund's link.
        Links links = await SeedLinksAsync();
        Withdrawal own = Withdrawal.ByBuyer(links.ItemId, 1, Today, null, Amounts, null, Now);
        await SaveAsync(c => c.Withdrawals.Add(own));
        Refund refund = NewRefund(RefundType.Withdrawal, links);
        (refund.WithdrawalId, refund.ReturnId) = (own.Id, null);
        await SaveAsync(c => c.Refunds.Add(refund));

        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM withdrawals WHERE id = '{own.Id}'"));

        Assert.Equal((ForeignKeyViolation, "fk_refunds_withdrawals_withdrawal_id"), (error.SqlState, error.ConstraintName));
    }

    [Theory]
    [MemberData(nameof(PermanentTables))]
    public async Task DeletingAPermanentRecordThroughTheContext_IsRefused_AndTheRowStays(string table)
    {
        Refund refund = await SeedRefundAsync();
        object record = table switch
        {
            "payment_records" => PaymentRecord.ForCardPayment(refund.OrderId, "tx-del", null, Now),
            "refunds" => refund,
            "refund_records" => RefundRecord.Sending(refund.Id, Now),
            _ => throw new ArgumentOutOfRangeException(nameof(table)),
        };
        if (record is not Refund)
        {
            await SaveAsync(c => c.Add(record));
        }

        long before = await ScalarAsync<long>($"SELECT count(*) FROM {table}");
        await using (AppDbContext context = NewContext())
        {
            context.Remove(record);
            InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));
            Assert.Contains("never deleted", error.Message, StringComparison.Ordinal);
        }

        Assert.Equal(before, await ScalarAsync<long>($"SELECT count(*) FROM {table}"));
    }

    [Fact]
    public async Task ChangingARecordInPlace_AndDeletingANotification_AreAccepted()
    {
        // The guard refuses only the deletion: the refund is sent and its record gets its outcome in place (06 4.33, 4.34); the notification
        // is deleted by the disposal by its retention (06 4.35).
        Refund refund = await SeedRefundAsync();
        RefundRecord record = RefundRecord.Sending(refund.Id, Now);
        InboundNotificationRecord notification = InboundNotificationRecord.Create("n-1", null, "payment", "success", null, null, Now);
        await SaveAsync(c =>
        {
            c.RefundRecords.Add(record);
            c.InboundNotificationRecords.Add(notification);
        });

        await using (AppDbContext context = NewContext())
        {
            RefundRecord loaded = await context.RefundRecords.SingleAsync(r => r.Id == record.Id, Ct);
            (loaded.State, loaded.OutcomeAt) = (RefundRecordState.Succeeded, Now);
            Refund loadedRefund = await context.Refunds.SingleAsync(r => r.Id == refund.Id, Ct);
            loadedRefund.SentAt = Now;
            context.InboundNotificationRecords.Remove(await context.InboundNotificationRecords.SingleAsync(n => n.Id == notification.Id, Ct));
            await context.SaveChangesAsync(Ct);
        }

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM refund_records WHERE state = 'Succeeded' AND outcome_at IS NOT NULL"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM refunds WHERE sent_at IS NOT NULL"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM inbound_notification_records"));
    }

    // ---- helpers ----

    private static RefundRecord Outcome(Guid refundId, RefundRecordState state)
    {
        RefundRecord record = RefundRecord.Sending(refundId, Now);
        (record.State, record.OutcomeAt) = (state, Now);
        return record;
    }

    /// <summary>A refund of the type with exactly the links its type names (06 4.33's type table), the optional ones filled.</summary>
    private static Refund NewRefund(RefundType type, Links links)
    {
        Refund refund = type == RefundType.LatePayment
            ? Refund.ForLatePayment(links.OrderId, links.PaymentRecordId, 120m, Now)
            : Refund.BySystem(links.OrderId, type, PaymentMethod.Card, 50m, Now);
        switch (type)
        {
            case RefundType.Cancellation:
                (refund.CancellationId, refund.OrderItemId, refund.Quantity) = (links.CancellationId, links.ItemId, 1);
                break;
            case RefundType.ItemRemoval:
                (refund.ItemRemovalId, refund.OrderItemId, refund.Quantity) = (links.ItemRemovalId, links.ItemId, 1);
                break;
            case RefundType.LateDeliveryTermination:
                (refund.LateDeliveryTerminationId, refund.OrderItemId, refund.Quantity) = (links.TerminationId, links.ItemId, 1);
                break;
            case RefundType.Withdrawal:
                (refund.WithdrawalId, refund.ReturnId, refund.OrderItemId, refund.Quantity) = (links.WithdrawalId, links.ReturnId, links.ItemId, 1);
                break;
            case RefundType.DefectClaimRescission:
                (refund.OrderItemId, refund.Quantity, refund.CouponShare, refund.VatAmount) = (links.ItemId, 1, 5m, 7.5m);
                (refund.DefectClaimId, refund.ReturnId) = (links.DefectClaimId, links.ReturnId);
                break;
            case RefundType.AmountBased:
                (refund.NotReturnedClosureId, refund.OrderItemId) = (links.ClosureId, links.ItemId);
                break;
            case RefundType.LatePayment:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type));
        }

        return refund;
    }

    /// <summary>Seeds one order with one item, one record of each item record table, a late payment and an administrator.</summary>
    private async Task<Links> SeedLinksAsync()
    {
        (Order order, OrderItem item) = await SeedItemAsync();
        Admin admin = await SeedAdminAsync();
        Cancellation cancellation = Cancellation.ByBuyer(item.Id, 1, Amounts, null, Now);
        ItemRemoval removal = ItemRemoval.Create(item.Id, 1, admin.Id, admin.Name, Amounts, Now);
        LateDeliveryTermination termination = LateDeliveryTermination.ByAdmin(item.Id, 1, admin.Id, admin.Name, Today, Amounts, null, Now);
        Withdrawal withdrawal = Withdrawal.ByBuyer(item.Id, 1, Today, null, Amounts, null, Now);
        DefectClaim claim = DefectClaim.Create(item.Id, 1, "Kırık", null, Now);
        await SaveAsync(c =>
        {
            c.Cancellations.Add(cancellation);
            c.ItemRemovals.Add(removal);
            c.LateDeliveryTerminations.Add(termination);
            c.Withdrawals.Add(withdrawal);
            c.DefectClaims.Add(claim);
        });
        Return received = Return.ForDefectClaim(item.Id, claim.Id, 1, Today, admin.Id, admin.Name, Now);
        NotReturnedClosure closure = NotReturnedClosure.Create(item.Id, withdrawal.Id, 1, admin.Id, admin.Name, Now);
        PaymentRecord payment = PaymentRecord.ForCardPayment(order.Id, $"tx-late-{Interlocked.Increment(ref _transactionCounter)}", null, Now);
        await SaveAsync(c =>
        {
            c.Returns.Add(received);
            c.NotReturnedClosures.Add(closure);
            c.PaymentRecords.Add(payment);
        });

        return new Links(
            order.Id, item.Id, admin.Id, cancellation.Id, removal.Id, termination.Id, withdrawal.Id, received.Id, claim.Id, closure.Id, payment.Id);
    }

    /// <summary>Seeds one card refund of a cancellation, for the refund record tests.</summary>
    private async Task<Refund> SeedRefundAsync()
    {
        Links links = await SeedLinksAsync();
        Refund refund = NewRefund(RefundType.Cancellation, links);
        await SaveAsync(c => c.Refunds.Add(refund));
        return refund;
    }

    private async Task<TransitionRecord> SeedTransitionAsync(Guid orderId)
    {
        TransitionRecord transition = TransitionRecord.Create(orderId, TransitionCode.O1, ActorKind.System, null, null, null, null, null, null, null, Now);
        await SaveAsync(c => c.TransitionRecords.Add(transition));
        return transition;
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

    private async Task<Admin> SeedAdminAsync()
    {
        Admin admin = Admin.Create("Veli", $"veli-{EntityIds.New():N}@example.com", "digest-placeholder", Now);
        await SaveAsync(c => c.Admins.Add(admin));
        return admin;
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

    /// <summary>The seeded principals a refund may link to: the order, its item, an administrator, one record of each item record table and a late payment.</summary>
    private sealed record Links(
        Guid OrderId,
        Guid ItemId,
        Guid AdminId,
        Guid CancellationId,
        Guid ItemRemovalId,
        Guid TerminationId,
        Guid WithdrawalId,
        Guid ReturnId,
        Guid DefectClaimId,
        Guid ClosureId,
        Guid PaymentRecordId);
}
