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
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the item record tests (T111): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(ItemRecordTablesDefinition.Name)]
public sealed class ItemRecordTablesDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "ItemRecordTables";
}

/// <summary>
/// The eight item record tables on a real PostgreSQL (T111 criteria 1-8; 06 4.22-4.26, 4.28, 4.30, 4.31, 2.5.1, 7.1, 7.2, 7.3, 7.5; 09 10.6):
/// the migration made the eight tables with the money type of 06 2.5.1; the database itself refuses what the rules refuse — the amounts
/// (quantity and product amount above zero, 0 &lt;= coupon share &lt;= product amount, VAT not negative), the actor's fields, the receipt's
/// links, the return address, the IBAN's form and the second claim of an item; the item and the other restrictive links stop a deletion;
/// the administrator, IBAN and defect claim links are emptied; and the context refuses the deletion of the six permanent records.
/// </summary>
[Collection(ItemRecordTablesDefinition.Name)]
public sealed class ItemRecordTablesTests : IAsyncLifetime
{
    private const string CheckViolation = PostgresErrorCodes.CheckViolation;
    private const string UniqueViolation = PostgresErrorCodes.UniqueViolation;
    private const string ForeignKeyViolation = PostgresErrorCodes.ForeignKeyViolation;
    private const string NumericValueOutOfRange = PostgresErrorCodes.NumericValueOutOfRange;

    private const string Iban = "TR330006100519786457841326";
    private const string Phone = "3121234567";

    private static readonly string[] EightTables =
    [
        "cancellations", "defect_claims", "item_removals", "late_delivery_terminations", "not_returned_closures", "refund_ibans", "returns", "withdrawals",
    ];

    private static readonly DateTimeOffset Now = new(2026, 11, 4, 9, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly Today = new(2026, 11, 4);

    private static readonly ItemAmounts Amounts = new(100m, 10m, 15m);

    private readonly CollectionDatabaseFixture _collection;

    private int _orderCounter;

    public ItemRecordTablesTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    /// <summary>The four records that set amounts aside from their item (06 4.22-4.25).</summary>
    public static TheoryData<string> AmountTables => new() { "cancellations", "item_removals", "late_delivery_terminations", "withdrawals" };

    /// <summary>The two records that carry a return address (06 4.25, 4.30).</summary>
    public static TheoryData<string> AddressTables => new() { "withdrawals", "defect_claims" };

    /// <summary>The six records that are never deleted (06 4.22-4.26, 4.28).</summary>
    public static TheoryData<string> PermanentTables => new()
    {
        "cancellations", "item_removals", "late_delivery_terminations", "withdrawals", "returns", "not_returned_closures",
    };

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE returns, not_returned_closures, cancellations, item_removals, late_delivery_terminations, withdrawals, defect_claims, refund_ibans, "
            + "transition_records, order_items, orders, admins, legal_text_versions, provinces CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the migration ----

    [Fact]
    public async Task Migration_MakesTheEightTables()
    {
        List<string> tables = await QueryAsync(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name = ANY(@names)",
            ("names", EightTables));

        Assert.Equal(EightTables, tables.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Columns_HaveTheTypesAndNullabilityOfTheFieldTables()
    {
        // 06 4.22-4.26, 4.28, 4.30, 4.31 and 2.5.1 (the common Id and CreatedAt included): money numeric(12,2), quantity integer, date date,
        // moment timestamptz, phone ten characters, IBAN text, the status history jsonb.
        string[] expected =
        [
            "cancellations.actor_kind text not null",
            "cancellations.admin_id uuid",
            "cancellations.admin_name text",
            "cancellations.cancellation_reason text",
            "cancellations.coupon_share numeric(12,2) not null",
            "cancellations.created_at timestamp with time zone not null",
            "cancellations.id uuid not null",
            "cancellations.order_item_id uuid not null",
            "cancellations.product_amount numeric(12,2) not null",
            "cancellations.quantity integer not null",
            "cancellations.reason_note text",
            "cancellations.refund_iban_id uuid",
            "cancellations.transition_record_id uuid",
            "cancellations.vat_amount numeric(12,2) not null",
            "defect_claims.created_at timestamp with time zone not null",
            "defect_claims.defect_claim_status text not null",
            "defect_claims.description text not null",
            "defect_claims.id uuid not null",
            "defect_claims.order_item_id uuid not null",
            "defect_claims.quantity integer not null",
            "defect_claims.return_address_line text",
            "defect_claims.return_district text",
            "defect_claims.return_phone character varying(10)",
            "defect_claims.return_province_id uuid",
            "defect_claims.return_recipient_name text",
            "defect_claims.status_history jsonb not null",
            "item_removals.admin_id uuid",
            "item_removals.admin_name text",
            "item_removals.coupon_share numeric(12,2) not null",
            "item_removals.created_at timestamp with time zone not null",
            "item_removals.id uuid not null",
            "item_removals.order_item_id uuid not null",
            "item_removals.product_amount numeric(12,2) not null",
            "item_removals.quantity integer not null",
            "item_removals.vat_amount numeric(12,2) not null",
            "late_delivery_terminations.actor_kind text not null",
            "late_delivery_terminations.admin_id uuid",
            "late_delivery_terminations.admin_name text",
            "late_delivery_terminations.coupon_share numeric(12,2) not null",
            "late_delivery_terminations.created_at timestamp with time zone not null",
            "late_delivery_terminations.declaration_date date not null",
            "late_delivery_terminations.id uuid not null",
            "late_delivery_terminations.order_item_id uuid not null",
            "late_delivery_terminations.product_amount numeric(12,2) not null",
            "late_delivery_terminations.quantity integer not null",
            "late_delivery_terminations.refund_iban_id uuid",
            "late_delivery_terminations.vat_amount numeric(12,2) not null",
            "not_returned_closures.admin_id uuid",
            "not_returned_closures.admin_name text",
            "not_returned_closures.created_at timestamp with time zone not null",
            "not_returned_closures.id uuid not null",
            "not_returned_closures.order_item_id uuid not null",
            "not_returned_closures.quantity integer not null",
            "not_returned_closures.reopened_at timestamp with time zone",
            "not_returned_closures.withdrawal_id uuid not null",
            "refund_ibans.created_at timestamp with time zone not null",
            "refund_ibans.entered_at timestamp with time zone not null",
            "refund_ibans.iban text not null",
            "refund_ibans.id uuid not null",
            "refund_ibans.order_id uuid not null",
            "refund_ibans.scheduled_deletion_at timestamp with time zone",
            "returns.admin_id uuid",
            "returns.admin_name text",
            "returns.arrival_date date not null",
            "returns.created_at timestamp with time zone not null",
            "returns.defect_claim_id uuid",
            "returns.id uuid not null",
            "returns.late_delivery_termination_id uuid",
            "returns.order_item_id uuid not null",
            "returns.quantity integer not null",
            "returns.stock_added_at timestamp with time zone",
            "returns.withdrawal_id uuid",
            "withdrawals.actor_kind text not null",
            "withdrawals.admin_id uuid",
            "withdrawals.admin_name text",
            "withdrawals.coupon_share numeric(12,2) not null",
            "withdrawals.created_at timestamp with time zone not null",
            "withdrawals.declaration_date date not null",
            "withdrawals.id uuid not null",
            "withdrawals.is_delivered_before_declaration boolean",
            "withdrawals.order_item_id uuid not null",
            "withdrawals.product_amount numeric(12,2) not null",
            "withdrawals.quantity integer not null",
            "withdrawals.refund_iban_id uuid",
            "withdrawals.return_address_line text",
            "withdrawals.return_district text",
            "withdrawals.return_phone character varying(10)",
            "withdrawals.return_province_id uuid",
            "withdrawals.return_recipient_name text",
            "withdrawals.vat_amount numeric(12,2) not null",
        ];

        List<string> columns = await QueryAsync(
            "SELECT table_name || '.' || column_name || ' ' || data_type"
            + " || CASE WHEN character_maximum_length IS NOT NULL THEN '(' || character_maximum_length || ')' ELSE '' END"
            + " || CASE WHEN data_type = 'numeric' THEN '(' || numeric_precision || ',' || numeric_scale || ')' ELSE '' END"
            + " || CASE WHEN is_nullable = 'NO' THEN ' not null' ELSE '' END"
            + " FROM information_schema.columns WHERE table_schema = 'public' AND table_name = ANY(@names)",
            ("names", EightTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), columns.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ColumnDefaults_AreTheDefaultColumnOfTheFieldTables()
    {
        // 06 4.22-4.25: the coupon share defaults to 0; 06 4.30: the claim is born Open with an empty status history. No other cell is filled.
        string[] expected =
        [
            "cancellations.coupon_share = 0.0",
            "defect_claims.defect_claim_status = 'Open'::text",
            "defect_claims.status_history = '[]'::jsonb",
            "item_removals.coupon_share = 0.0",
            "late_delivery_terminations.coupon_share = 0.0",
            "withdrawals.coupon_share = 0.0",
        ];

        List<string> defaults = await QueryAsync(
            "SELECT table_name || '.' || column_name || ' = ' || column_default FROM information_schema.columns WHERE table_schema = 'public' AND table_name = ANY(@names) AND column_default IS NOT NULL",
            ("names", EightTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), defaults.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ARowWrittenWithoutTheDefaultedColumns_GetsTheDefaults()
    {
        (_, OrderItem item) = await SeedItemAsync();

        await ExecuteAsync(
            $"INSERT INTO cancellations (id, created_at, order_item_id, quantity, actor_kind, product_amount, vat_amount) VALUES ('{EntityIds.New()}', now(), '{item.Id}', 1, 'Buyer', 100, 15);"
            + $"INSERT INTO defect_claims (id, created_at, order_item_id, quantity, description) VALUES ('{EntityIds.New()}', now(), '{item.Id}', 1, 'Kırık');");

        Assert.Equal("0.00", await ScalarAsync<string>("SELECT coupon_share::text FROM cancellations"));
        Assert.Equal("Open []", await ScalarAsync<string>("SELECT defect_claim_status || ' ' || status_history::text FROM defect_claims"));
    }

    [Fact]
    public async Task Indexes_AreTheDocumentedOnes()
    {
        // 06 7.1 (one claim per item), 7.5 (the open claims, the scheduler's scan) and the foreign key indexes of 7.2.
        string[] expected =
        [
            "cancellations.ix_cancellations_admin_id | CREATE INDEX ix_cancellations_admin_id ON public.cancellations USING btree (admin_id)",
            "cancellations.ix_cancellations_order_item_id | CREATE INDEX ix_cancellations_order_item_id ON public.cancellations USING btree (order_item_id)",
            "cancellations.ix_cancellations_refund_iban_id | CREATE INDEX ix_cancellations_refund_iban_id ON public.cancellations USING btree (refund_iban_id)",
            "cancellations.ix_cancellations_transition_record_id | CREATE INDEX ix_cancellations_transition_record_id ON public.cancellations USING btree (transition_record_id)",
            "cancellations.pk_cancellations | CREATE UNIQUE INDEX pk_cancellations ON public.cancellations USING btree (id)",
            "defect_claims.ix_defect_claims_defect_claim_status | CREATE INDEX ix_defect_claims_defect_claim_status ON public.defect_claims USING btree (defect_claim_status) WHERE (defect_claim_status = 'Open'::text)",
            "defect_claims.ix_defect_claims_order_item_id | CREATE UNIQUE INDEX ix_defect_claims_order_item_id ON public.defect_claims USING btree (order_item_id)",
            "defect_claims.ix_defect_claims_return_province_id | CREATE INDEX ix_defect_claims_return_province_id ON public.defect_claims USING btree (return_province_id)",
            "defect_claims.pk_defect_claims | CREATE UNIQUE INDEX pk_defect_claims ON public.defect_claims USING btree (id)",
            "item_removals.ix_item_removals_admin_id | CREATE INDEX ix_item_removals_admin_id ON public.item_removals USING btree (admin_id)",
            "item_removals.ix_item_removals_order_item_id | CREATE INDEX ix_item_removals_order_item_id ON public.item_removals USING btree (order_item_id)",
            "item_removals.pk_item_removals | CREATE UNIQUE INDEX pk_item_removals ON public.item_removals USING btree (id)",
            "late_delivery_terminations.ix_late_delivery_terminations_admin_id | CREATE INDEX ix_late_delivery_terminations_admin_id ON public.late_delivery_terminations USING btree (admin_id)",
            "late_delivery_terminations.ix_late_delivery_terminations_order_item_id | CREATE INDEX ix_late_delivery_terminations_order_item_id ON public.late_delivery_terminations USING btree (order_item_id)",
            "late_delivery_terminations.ix_late_delivery_terminations_refund_iban_id | CREATE INDEX ix_late_delivery_terminations_refund_iban_id ON public.late_delivery_terminations USING btree (refund_iban_id)",
            "late_delivery_terminations.pk_late_delivery_terminations | CREATE UNIQUE INDEX pk_late_delivery_terminations ON public.late_delivery_terminations USING btree (id)",
            "not_returned_closures.ix_not_returned_closures_admin_id | CREATE INDEX ix_not_returned_closures_admin_id ON public.not_returned_closures USING btree (admin_id)",
            "not_returned_closures.ix_not_returned_closures_order_item_id | CREATE INDEX ix_not_returned_closures_order_item_id ON public.not_returned_closures USING btree (order_item_id)",
            "not_returned_closures.ix_not_returned_closures_withdrawal_id | CREATE INDEX ix_not_returned_closures_withdrawal_id ON public.not_returned_closures USING btree (withdrawal_id)",
            "not_returned_closures.pk_not_returned_closures | CREATE UNIQUE INDEX pk_not_returned_closures ON public.not_returned_closures USING btree (id)",
            "refund_ibans.ix_refund_ibans_order_id | CREATE INDEX ix_refund_ibans_order_id ON public.refund_ibans USING btree (order_id)",
            "refund_ibans.ix_refund_ibans_scheduled_deletion_at | CREATE INDEX ix_refund_ibans_scheduled_deletion_at ON public.refund_ibans USING btree (scheduled_deletion_at) WHERE (scheduled_deletion_at IS NOT NULL)",
            "refund_ibans.pk_refund_ibans | CREATE UNIQUE INDEX pk_refund_ibans ON public.refund_ibans USING btree (id)",
            "returns.ix_returns_admin_id | CREATE INDEX ix_returns_admin_id ON public.returns USING btree (admin_id)",
            "returns.ix_returns_defect_claim_id | CREATE INDEX ix_returns_defect_claim_id ON public.returns USING btree (defect_claim_id)",
            "returns.ix_returns_late_delivery_termination_id | CREATE INDEX ix_returns_late_delivery_termination_id ON public.returns USING btree (late_delivery_termination_id)",
            "returns.ix_returns_order_item_id | CREATE INDEX ix_returns_order_item_id ON public.returns USING btree (order_item_id)",
            "returns.ix_returns_withdrawal_id | CREATE INDEX ix_returns_withdrawal_id ON public.returns USING btree (withdrawal_id)",
            "returns.pk_returns | CREATE UNIQUE INDEX pk_returns ON public.returns USING btree (id)",
            "withdrawals.ix_withdrawals_admin_id | CREATE INDEX ix_withdrawals_admin_id ON public.withdrawals USING btree (admin_id)",
            "withdrawals.ix_withdrawals_order_item_id | CREATE INDEX ix_withdrawals_order_item_id ON public.withdrawals USING btree (order_item_id)",
            "withdrawals.ix_withdrawals_refund_iban_id | CREATE INDEX ix_withdrawals_refund_iban_id ON public.withdrawals USING btree (refund_iban_id)",
            "withdrawals.ix_withdrawals_return_province_id | CREATE INDEX ix_withdrawals_return_province_id ON public.withdrawals USING btree (return_province_id)",
            "withdrawals.pk_withdrawals | CREATE UNIQUE INDEX pk_withdrawals ON public.withdrawals USING btree (id)",
        ];

        List<string> indexes = await QueryAsync(
            "SELECT tablename || '.' || indexname || ' | ' || indexdef FROM pg_indexes WHERE schemaname = 'public' AND tablename = ANY(@names)",
            ("names", EightTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), indexes.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ForeignKeys_HaveTheDeleteBehaviorOfTheLinkTable()
    {
        // 06 7.2: the administrator, IBAN and defect claim links are emptied (n); every other link is restrictive (a). One name is cut to
        // the 63-byte limit by the framework (D-51): the receipt's termination link.
        string[] expected =
        [
            "cancellations fk_cancellations_admins_admin_id n",
            "cancellations fk_cancellations_order_items_order_item_id a",
            "cancellations fk_cancellations_refund_ibans_refund_iban_id n",
            "cancellations fk_cancellations_transition_records_transition_record_id a",
            "defect_claims fk_defect_claims_order_items_order_item_id a",
            "defect_claims fk_defect_claims_provinces_return_province_id a",
            "item_removals fk_item_removals_admins_admin_id n",
            "item_removals fk_item_removals_order_items_order_item_id a",
            "late_delivery_terminations fk_late_delivery_terminations_admins_admin_id n",
            "late_delivery_terminations fk_late_delivery_terminations_order_items_order_item_id a",
            "late_delivery_terminations fk_late_delivery_terminations_refund_ibans_refund_iban_id n",
            "not_returned_closures fk_not_returned_closures_admins_admin_id n",
            "not_returned_closures fk_not_returned_closures_order_items_order_item_id a",
            "not_returned_closures fk_not_returned_closures_withdrawals_withdrawal_id a",
            "refund_ibans fk_refund_ibans_orders_order_id a",
            "returns fk_returns_admins_admin_id n",
            "returns fk_returns_defect_claims_defect_claim_id n",
            "returns fk_returns_late_delivery_terminations_late_delivery_terminatio a",
            "returns fk_returns_order_items_order_item_id a",
            "returns fk_returns_withdrawals_withdrawal_id a",
            "withdrawals fk_withdrawals_admins_admin_id n",
            "withdrawals fk_withdrawals_order_items_order_item_id a",
            "withdrawals fk_withdrawals_provinces_return_province_id a",
            "withdrawals fk_withdrawals_refund_ibans_refund_iban_id n",
        ];

        List<string> links = await QueryAsync(
            "SELECT conrelid::regclass::text || ' ' || conname || ' ' || confdeltype::text FROM pg_constraint WHERE contype = 'f' AND conrelid = ANY(ARRAY(SELECT x::regclass FROM unnest(@oids) x))",
            ("oids", EightTables.Select(t => "public." + t).ToArray()));

        Assert.Equal(expected.Order(StringComparer.Ordinal), links.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task CheckConstraints_AreTheDocumentedOnes()
    {
        // 06 7.3, the phone and IBAN rules of 2.5.1 and the enum columns' allowed values (06 2.9.2).
        string[] expected =
        [
            "cancellations.ck_cancellations_actor_kind",
            "cancellations.ck_cancellations_buyer_without_admin",
            "cancellations.ck_cancellations_cancellation_reason",
            "cancellations.ck_cancellations_coupon_share_within_product",
            "cancellations.ck_cancellations_product_amount_above_zero",
            "cancellations.ck_cancellations_quantity_above_zero",
            "cancellations.ck_cancellations_reason_by_admin",
            "cancellations.ck_cancellations_reason_note_only_on_other",
            "cancellations.ck_cancellations_refund_iban_only_by_buyer",
            "cancellations.ck_cancellations_vat_amount_not_negative",
            "defect_claims.ck_defect_claims_defect_claim_status",
            "defect_claims.ck_defect_claims_quantity_above_zero",
            "defect_claims.ck_defect_claims_return_address_fields_together",
            "defect_claims.ck_defect_claims_return_phone_only_with_address",
            "defect_claims.ck_defect_claims_return_phone_ten_digits",
            "item_removals.ck_item_removals_coupon_share_within_product",
            "item_removals.ck_item_removals_product_amount_above_zero",
            "item_removals.ck_item_removals_quantity_above_zero",
            "item_removals.ck_item_removals_vat_amount_not_negative",
            "late_delivery_terminations.ck_late_delivery_terminations_actor_kind",
            "late_delivery_terminations.ck_late_delivery_terminations_buyer_without_admin",
            "late_delivery_terminations.ck_late_delivery_terminations_coupon_share_within_product",
            "late_delivery_terminations.ck_late_delivery_terminations_product_amount_above_zero",
            "late_delivery_terminations.ck_late_delivery_terminations_quantity_above_zero",
            "late_delivery_terminations.ck_late_delivery_terminations_vat_amount_not_negative",
            "not_returned_closures.ck_not_returned_closures_quantity_above_zero",
            "refund_ibans.ck_refund_ibans_iban_capitals_and_digits",
            "returns.ck_returns_at_most_one_link",
            "returns.ck_returns_quantity_above_zero",
            "withdrawals.ck_withdrawals_actor_kind",
            "withdrawals.ck_withdrawals_buyer_without_admin",
            "withdrawals.ck_withdrawals_coupon_share_within_product",
            "withdrawals.ck_withdrawals_product_amount_above_zero",
            "withdrawals.ck_withdrawals_quantity_above_zero",
            "withdrawals.ck_withdrawals_return_address_fields_together",
            "withdrawals.ck_withdrawals_return_phone_only_with_address",
            "withdrawals.ck_withdrawals_return_phone_ten_digits",
            "withdrawals.ck_withdrawals_vat_amount_not_negative",
        ];

        List<string> checks = await QueryAsync(
            "SELECT conrelid::regclass::text || '.' || conname FROM pg_constraint WHERE contype = 'c' AND conrelid = ANY(ARRAY(SELECT x::regclass FROM unnest(@oids) x))",
            ("oids", EightTables.Select(t => "public." + t).ToArray()));

        Assert.Equal(expected.Order(StringComparer.Ordinal), checks.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Migration_HasNoDownMethod()
    {
        MethodInfo? down = typeof(CreateItemRecordTables)
            .GetMethod("Down", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);

        Assert.Null(down);
    }

    // ---- KK2: the quantities and the amounts ----

    [Theory]
    [MemberData(nameof(AmountTables))]
    public async Task AmountRecord_WithAQuantityOfZero_IsRefused(string table)
    {
        (_, OrderItem item) = await SeedItemAsync();
        Admin admin = await SeedAdminAsync();

        Assert.Equal(
            (CheckViolation, $"ck_{table}_quantity_above_zero"),
            await ViolationOfAsync(c => c.Add(NewAmountRecord(table, item.Id, admin, Amounts, quantity: 0))));
    }

    [Theory]
    [MemberData(nameof(AmountTables))]
    public async Task AmountRecord_WithAProductAmountOfZero_IsRefused(string table)
    {
        (_, OrderItem item) = await SeedItemAsync();
        Admin admin = await SeedAdminAsync();

        Assert.Equal(
            (CheckViolation, $"ck_{table}_product_amount_above_zero"),
            await ViolationOfAsync(c => c.Add(NewAmountRecord(table, item.Id, admin, new ItemAmounts(0m, 0m, 0m)))));
    }

    [Theory]
    [MemberData(nameof(AmountTables))]
    public async Task AmountRecord_WithACouponShareAboveTheProductAmountOrBelowZero_IsRefused(string table)
    {
        (_, OrderItem item) = await SeedItemAsync();
        Admin admin = await SeedAdminAsync();
        string rule = $"ck_{table}_coupon_share_within_product";

        Assert.Equal((CheckViolation, rule), await ViolationOfAsync(c => c.Add(NewAmountRecord(table, item.Id, admin, new ItemAmounts(100m, 100.01m, 15m)))));
        Assert.Equal((CheckViolation, rule), await ViolationOfAsync(c => c.Add(NewAmountRecord(table, item.Id, admin, new ItemAmounts(100m, -0.01m, 15m)))));
    }

    [Theory]
    [MemberData(nameof(AmountTables))]
    public async Task AmountRecord_WithANegativeVat_IsRefused(string table)
    {
        (_, OrderItem item) = await SeedItemAsync();
        Admin admin = await SeedAdminAsync();

        Assert.Equal(
            (CheckViolation, $"ck_{table}_vat_amount_not_negative"),
            await ViolationOfAsync(c => c.Add(NewAmountRecord(table, item.Id, admin, new ItemAmounts(100m, 10m, -0.01m)))));
    }

    [Theory]
    [MemberData(nameof(AmountTables))]
    public async Task AmountRecord_AtTheLimitsOfTheRules_IsAccepted_AndKeepsItsKurus(string table)
    {
        // The coupon share may equal the product amount (the whole item paid by the coupon), the share and the VAT may be zero, and the
        // money type holds the kuruş up to 9.999.999.999,99 (06 2.5.1).
        (_, OrderItem item) = await SeedItemAsync();
        Admin admin = await SeedAdminAsync();

        await SaveAsync(c =>
        {
            c.Add(NewAmountRecord(table, item.Id, admin, new ItemAmounts(100m, 100m, 0m)));
            c.Add(NewAmountRecord(table, item.Id, admin, new ItemAmounts(0.01m, 0m, 0m)));
            c.Add(NewAmountRecord(table, item.Id, admin, new ItemAmounts(9_999_999_999.99m, 1234.56m, 0.01m)));
        });

        List<string> rows = await QueryAsync($"SELECT product_amount::text || ' ' || coupon_share::text || ' ' || vat_amount::text FROM {table}");
        Assert.Equal(["0.01 0.00 0.00", "100.00 100.00 0.00", "9999999999.99 1234.56 0.01"], rows.Order(StringComparer.Ordinal));
    }

    [Theory]
    [MemberData(nameof(AmountTables))]
    public async Task AmountRecord_AboveTheMoneyType_IsRefused(string table)
    {
        (_, OrderItem item) = await SeedItemAsync();
        Admin admin = await SeedAdminAsync();

        Assert.Equal(
            (NumericValueOutOfRange, null),
            await ViolationOfAsync(c => c.Add(NewAmountRecord(table, item.Id, admin, new ItemAmounts(10_000_000_000m, 0m, 0m)))));
    }

    [Fact]
    public async Task ReceiptClosureOrClaimWithAQuantityOfZero_IsRefused()
    {
        (_, OrderItem item) = await SeedItemAsync();
        Admin admin = await SeedAdminAsync();
        Withdrawal withdrawal = await SeedWithdrawalAsync(item.Id);

        Assert.Equal(
            (CheckViolation, "ck_returns_quantity_above_zero"),
            await ViolationOfAsync(c => c.Returns.Add(Return.ForWithdrawal(item.Id, withdrawal.Id, 0, Today, admin.Id, admin.Name, Now))));
        Assert.Equal(
            (CheckViolation, "ck_not_returned_closures_quantity_above_zero"),
            await ViolationOfAsync(c => c.NotReturnedClosures.Add(NotReturnedClosure.Create(item.Id, withdrawal.Id, 0, admin.Id, admin.Name, Now))));
        Assert.Equal(
            (CheckViolation, "ck_defect_claims_quantity_above_zero"),
            await ViolationOfAsync(c => c.DefectClaims.Add(DefectClaim.Create(item.Id, 0, "Kırık", null, Now))));
    }

    // ---- KK3: the actor's fields ----

    [Fact]
    public async Task CustomerCancellation_WithAnAdministratorField_IsRefused()
    {
        (_, OrderItem item) = await SeedItemAsync();
        Admin admin = await SeedAdminAsync();

        Cancellation withAdmin = Cancellation.ByBuyer(item.Id, 1, Amounts, null, Now);
        withAdmin.AdminId = admin.Id;
        Cancellation withName = Cancellation.ByBuyer(item.Id, 1, Amounts, null, Now);
        withName.AdminName = "Veli";

        Assert.Equal((CheckViolation, "ck_cancellations_buyer_without_admin"), await ViolationOfAsync(c => c.Cancellations.Add(withAdmin)));
        Assert.Equal((CheckViolation, "ck_cancellations_buyer_without_admin"), await ViolationOfAsync(c => c.Cancellations.Add(withName)));
    }

    [Fact]
    public async Task CancellationReason_IsFilledOnTheAdministratorsRecord_AndOnlyThere()
    {
        (_, OrderItem item) = await SeedItemAsync();
        Admin admin = await SeedAdminAsync();

        Cancellation customerWithReason = Cancellation.ByBuyer(item.Id, 1, Amounts, null, Now);
        customerWithReason.CancellationReason = CancellationReason.OutOfStock;
        Cancellation adminWithoutReason = Cancellation.ByAdmin(item.Id, 1, admin.Id, admin.Name, CancellationReason.OutOfStock, null, Amounts, Now);
        adminWithoutReason.CancellationReason = null;

        Assert.Equal((CheckViolation, "ck_cancellations_reason_by_admin"), await ViolationOfAsync(c => c.Cancellations.Add(customerWithReason)));
        Assert.Equal((CheckViolation, "ck_cancellations_reason_by_admin"), await ViolationOfAsync(c => c.Cancellations.Add(adminWithoutReason)));
    }

    [Fact]
    public async Task ReasonNote_IsFilledOnlyOnTheOtherReason()
    {
        (_, OrderItem item) = await SeedItemAsync();
        Admin admin = await SeedAdminAsync();

        Cancellation customerWithNote = Cancellation.ByBuyer(item.Id, 1, Amounts, null, Now);
        customerWithNote.ReasonNote = "Not";
        Cancellation stockWithNote = Cancellation.ByAdmin(item.Id, 1, admin.Id, admin.Name, CancellationReason.OutOfStock, "Not", Amounts, Now);

        Assert.Equal((CheckViolation, "ck_cancellations_reason_note_only_on_other"), await ViolationOfAsync(c => c.Cancellations.Add(customerWithNote)));
        Assert.Equal((CheckViolation, "ck_cancellations_reason_note_only_on_other"), await ViolationOfAsync(c => c.Cancellations.Add(stockWithNote)));

        await SaveAsync(c => c.Cancellations.AddRange(
            Cancellation.ByAdmin(item.Id, 1, admin.Id, admin.Name, CancellationReason.Other, "Müşteri aradı", Amounts, Now),
            Cancellation.ByAdmin(item.Id, 1, admin.Id, admin.Name, CancellationReason.Other, null, Amounts, Now),
            Cancellation.ByAdmin(item.Id, 1, admin.Id, admin.Name, CancellationReason.OutOfStock, null, Amounts, Now)));

        Assert.Equal(3L, await ScalarAsync<long>("SELECT count(*) FROM cancellations"));
    }

    [Fact]
    public async Task RefundIban_IsLinkedOnlyToTheCustomersCancellation()
    {
        (Order order, OrderItem item) = await SeedItemAsync();
        Admin admin = await SeedAdminAsync();
        RefundIban iban = RefundIban.Create(order.Id, Iban, Now);
        await SaveAsync(c => c.RefundIbans.Add(iban));

        Cancellation adminWithIban = Cancellation.ByAdmin(item.Id, 1, admin.Id, admin.Name, CancellationReason.OutOfStock, null, Amounts, Now);
        adminWithIban.RefundIbanId = iban.Id;

        Assert.Equal((CheckViolation, "ck_cancellations_refund_iban_only_by_buyer"), await ViolationOfAsync(c => c.Cancellations.Add(adminWithIban)));

        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(item.Id, 1, Amounts, iban.Id, Now)));

        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM cancellations WHERE refund_iban_id = '{iban.Id}' AND actor_kind = 'Buyer'"));
    }

    [Fact]
    public async Task CustomerTerminationOrWithdrawal_WithAnAdministratorField_IsRefused()
    {
        (_, OrderItem item) = await SeedItemAsync();
        Admin admin = await SeedAdminAsync();

        LateDeliveryTermination terminationWithAdmin = LateDeliveryTermination.ByBuyer(item.Id, 1, Today, Amounts, null, Now);
        terminationWithAdmin.AdminId = admin.Id;
        LateDeliveryTermination terminationWithName = LateDeliveryTermination.ByBuyer(item.Id, 1, Today, Amounts, null, Now);
        terminationWithName.AdminName = "Veli";
        Withdrawal withdrawalWithAdmin = Withdrawal.ByBuyer(item.Id, 1, Today, null, Amounts, null, Now);
        withdrawalWithAdmin.AdminId = admin.Id;
        Withdrawal withdrawalWithName = Withdrawal.ByBuyer(item.Id, 1, Today, null, Amounts, null, Now);
        withdrawalWithName.AdminName = "Veli";

        Assert.Equal((CheckViolation, "ck_late_delivery_terminations_buyer_without_admin"), await ViolationOfAsync(c => c.LateDeliveryTerminations.Add(terminationWithAdmin)));
        Assert.Equal((CheckViolation, "ck_late_delivery_terminations_buyer_without_admin"), await ViolationOfAsync(c => c.LateDeliveryTerminations.Add(terminationWithName)));
        Assert.Equal((CheckViolation, "ck_withdrawals_buyer_without_admin"), await ViolationOfAsync(c => c.Withdrawals.Add(withdrawalWithAdmin)));
        Assert.Equal((CheckViolation, "ck_withdrawals_buyer_without_admin"), await ViolationOfAsync(c => c.Withdrawals.Add(withdrawalWithName)));
    }

    [Fact]
    public async Task RecordsOfBothActors_AreAccepted_AndAnEmptiedAdministratorLinkIsAccepted()
    {
        (Order order, OrderItem item) = await SeedItemAsync();
        Admin admin = await SeedAdminAsync();
        RefundIban iban = RefundIban.Create(order.Id, Iban, Now);
        await SaveAsync(c => c.RefundIbans.Add(iban));

        Cancellation emptied = Cancellation.ByAdmin(item.Id, 1, admin.Id, admin.Name, CancellationReason.OutOfStock, null, Amounts, Now);
        emptied.AdminId = null;
        emptied.AdminName = null;

        await SaveAsync(c =>
        {
            c.Cancellations.AddRange(Cancellation.ByBuyer(item.Id, 1, Amounts, null, Now), emptied);
            c.LateDeliveryTerminations.AddRange(
                LateDeliveryTermination.ByBuyer(item.Id, 1, Today, Amounts, iban.Id, Now),
                LateDeliveryTermination.ByAdmin(item.Id, 1, admin.Id, admin.Name, Today, Amounts, iban.Id, Now));
            c.Withdrawals.AddRange(
                Withdrawal.ByBuyer(item.Id, 1, Today, null, Amounts, iban.Id, Now),
                Withdrawal.ByAdmin(item.Id, 1, admin.Id, admin.Name, Today, null, Amounts, iban.Id, Now));
        });

        Assert.Equal(6L, await ScalarAsync<long>(
            "SELECT (SELECT count(*) FROM cancellations) + (SELECT count(*) FROM late_delivery_terminations) + (SELECT count(*) FROM withdrawals)"));
    }

    [Fact]
    public async Task SystemActor_IsRefusedOnTheThreeRecordsWithAnActor()
    {
        // 06 4.22, 4.24, 4.25: the actor is a customer or an administrator; the column's allowed values are the subset (06 2.9.2).
        (_, OrderItem item) = await SeedItemAsync();

        Cancellation cancellation = Cancellation.ByBuyer(item.Id, 1, Amounts, null, Now);
        cancellation.ActorKind = ActorKind.System;
        LateDeliveryTermination termination = LateDeliveryTermination.ByBuyer(item.Id, 1, Today, Amounts, null, Now);
        termination.ActorKind = ActorKind.System;
        Withdrawal withdrawal = Withdrawal.ByBuyer(item.Id, 1, Today, null, Amounts, null, Now);
        withdrawal.ActorKind = ActorKind.System;

        Assert.Equal((CheckViolation, "ck_cancellations_actor_kind"), await ViolationOfAsync(c => c.Cancellations.Add(cancellation)));
        Assert.Equal((CheckViolation, "ck_late_delivery_terminations_actor_kind"), await ViolationOfAsync(c => c.LateDeliveryTerminations.Add(termination)));
        Assert.Equal((CheckViolation, "ck_withdrawals_actor_kind"), await ViolationOfAsync(c => c.Withdrawals.Add(withdrawal)));
    }

    // ---- KK4: the receipt's links ----

    [Fact]
    public async Task ReceiptWithTwoOrThreeLinks_IsRefused_AndOneWithOneOrNoneIsAccepted()
    {
        (_, OrderItem item) = await SeedItemAsync();
        Admin admin = await SeedAdminAsync();
        Withdrawal withdrawal = await SeedWithdrawalAsync(item.Id);
        LateDeliveryTermination termination = LateDeliveryTermination.ByBuyer(item.Id, 1, Today, Amounts, null, Now);
        DefectClaim claim = DefectClaim.Create(item.Id, 1, "Kırık", null, Now);
        await SaveAsync(c =>
        {
            c.LateDeliveryTerminations.Add(termination);
            c.DefectClaims.Add(claim);
        });

        Return withdrawalAndTermination = Return.ForWithdrawal(item.Id, withdrawal.Id, 1, Today, admin.Id, admin.Name, Now);
        withdrawalAndTermination.LateDeliveryTerminationId = termination.Id;
        Return withdrawalAndClaim = Return.ForWithdrawal(item.Id, withdrawal.Id, 1, Today, admin.Id, admin.Name, Now);
        withdrawalAndClaim.DefectClaimId = claim.Id;
        Return terminationAndClaim = Return.ForLateDeliveryTermination(item.Id, termination.Id, 1, Today, admin.Id, admin.Name, Now);
        terminationAndClaim.DefectClaimId = claim.Id;
        Return allThree = Return.ForWithdrawal(item.Id, withdrawal.Id, 1, Today, admin.Id, admin.Name, Now);
        allThree.LateDeliveryTerminationId = termination.Id;
        allThree.DefectClaimId = claim.Id;

        const string rule = "ck_returns_at_most_one_link";
        Assert.Equal((CheckViolation, rule), await ViolationOfAsync(c => c.Returns.Add(withdrawalAndTermination)));
        Assert.Equal((CheckViolation, rule), await ViolationOfAsync(c => c.Returns.Add(withdrawalAndClaim)));
        Assert.Equal((CheckViolation, rule), await ViolationOfAsync(c => c.Returns.Add(terminationAndClaim)));
        Assert.Equal((CheckViolation, rule), await ViolationOfAsync(c => c.Returns.Add(allThree)));

        // The database holds "at most one": the claim's link is emptied when the claim is deleted, so a row without a link is kept (06 4.26).
        Return withoutLink = Return.ForDefectClaim(item.Id, claim.Id, 1, Today, admin.Id, admin.Name, Now);
        withoutLink.DefectClaimId = null;
        await SaveAsync(c => c.Returns.AddRange(
            Return.ForWithdrawal(item.Id, withdrawal.Id, 1, Today, admin.Id, admin.Name, Now),
            Return.ForLateDeliveryTermination(item.Id, termination.Id, 1, Today, admin.Id, admin.Name, Now),
            Return.ForDefectClaim(item.Id, claim.Id, 1, Today, admin.Id, admin.Name, Now),
            withoutLink));

        Assert.Equal(4L, await ScalarAsync<long>("SELECT count(*) FROM returns"));
    }

    // ---- KK5: one claim per item; the return address ----

    [Fact]
    public async Task SecondClaimOfTheSameItem_IsRefused_ButAClaimOfAnotherItemIsAccepted()
    {
        (_, OrderItem item) = await SeedItemAsync();
        (_, OrderItem other) = await SeedItemAsync();
        await SaveAsync(c => c.DefectClaims.Add(DefectClaim.Create(item.Id, 1, "Kırık", null, Now)));

        Assert.Equal(
            (UniqueViolation, "ix_defect_claims_order_item_id"),
            await ViolationOfAsync(c => c.DefectClaims.Add(DefectClaim.Create(item.Id, 2, "Yine kırık", null, Now))));

        await SaveAsync(c => c.DefectClaims.Add(DefectClaim.Create(other.Id, 1, "Çizik", null, Now)));

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM defect_claims"));
    }

    [Theory]
    [MemberData(nameof(AddressTables))]
    public async Task ReturnAddress_WithOneOfTheFourFieldsMissing_IsRefused(string table)
    {
        (_, OrderItem item) = await SeedItemAsync();
        Province province = await ProvinceAsync();
        string rule = $"ck_{table}_return_address_fields_together";

        for (int missing = 0; missing < 4; missing++)
        {
            object record = NewAddressRecord(table, item.Id, new ReturnAddress("Firma Depo", province.Id, "Çankaya", "Depo Sokak 1", Phone));
            SetAddress(
                record,
                missing == 0 ? null : "Firma Depo",
                missing == 1 ? null : province.Id,
                missing == 2 ? null : "Çankaya",
                missing == 3 ? null : "Depo Sokak 1",
                Phone);

            Assert.Equal((CheckViolation, rule), await ViolationOfAsync(c => c.Add(record)));
        }
    }

    [Theory]
    [MemberData(nameof(AddressTables))]
    public async Task ReturnPhone_WithoutTheAddress_IsRefused(string table)
    {
        (_, OrderItem item) = await SeedItemAsync();
        object record = NewAddressRecord(table, item.Id, null);
        SetAddress(record, null, null, null, null, Phone);

        Assert.Equal((CheckViolation, $"ck_{table}_return_phone_only_with_address"), await ViolationOfAsync(c => c.Add(record)));
    }

    [Theory]
    [MemberData(nameof(AddressTables))]
    public async Task ReturnPhone_ThatIsNotTenDigits_IsRefused(string table)
    {
        (_, OrderItem item) = await SeedItemAsync();
        Province province = await ProvinceAsync();

        foreach (string phone in new[] { "312123456", "312123456a", "31212 3456" })
        {
            object record = NewAddressRecord(table, item.Id, new ReturnAddress("Firma Depo", province.Id, "Çankaya", "Depo Sokak 1", phone));
            Assert.Equal((CheckViolation, $"ck_{table}_return_phone_ten_digits"), await ViolationOfAsync(c => c.Add(record)));
        }
    }

    [Theory]
    [MemberData(nameof(AddressTables))]
    public async Task ReturnAddress_FilledWithOrWithoutAPhone_OrEmpty_IsAccepted(string table)
    {
        // A defect claim is one per item: each accepted claim takes its own item.
        Province province = await ProvinceAsync();
        ReturnAddress?[] addresses =
        [
            new ReturnAddress("Firma Depo", province.Id, "Çankaya", "Depo Sokak 1", Phone),
            new ReturnAddress("Firma Depo", province.Id, "Çankaya", "Depo Sokak 1", null),
            null,
        ];

        foreach (ReturnAddress? address in addresses)
        {
            (_, OrderItem item) = await SeedItemAsync();
            await SaveAsync(c => c.Add(NewAddressRecord(table, item.Id, address)));
        }

        Assert.Equal(3L, await ScalarAsync<long>($"SELECT count(*) FROM {table}"));
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM {table} WHERE return_phone = '{Phone}'"));
    }

    // ---- KK6: the IBAN ----

    [Theory]
    [InlineData("tr330006100519786457841326")]
    [InlineData("TR33 0006 1005 1978 6457 8413 26")]
    [InlineData("TR33-0006100519786457841326")]
    public async Task RefundIban_NotInCapitalsAndDigits_IsRefused(string iban)
    {
        Order order = await SeedOrderAsync();

        Assert.Equal(
            (CheckViolation, "ck_refund_ibans_iban_capitals_and_digits"),
            await ViolationOfAsync(c => c.RefundIbans.Add(RefundIban.Create(order.Id, iban, Now))));
    }

    [Fact]
    public async Task RefundIban_InCapitalsAndDigits_IsAccepted_AndItsCorrectionIsWrittenInPlace()
    {
        Order order = await SeedOrderAsync();
        RefundIban iban = RefundIban.Create(order.Id, Iban, Now);
        await SaveAsync(c => c.RefundIbans.Add(iban));

        await using (AppDbContext context = NewContext())
        {
            RefundIban loaded = await context.RefundIbans.SingleAsync(i => i.Id == iban.Id, Ct);
            loaded.Iban = "TR320010009999901234567890";
            loaded.EnteredAt = Now.AddHours(1);
            await context.SaveChangesAsync(Ct);
        }

        Assert.Equal("TR320010009999901234567890", await ScalarAsync<string>($"SELECT iban FROM refund_ibans WHERE id = '{iban.Id}'"));
    }

    [Fact]
    public async Task DeletingTheIban_EmptiesTheLinksOfTheThreeRecords()
    {
        // 06 4.31: the IBAN is deleted by its event; the incoming links are emptied (06 7.2; 2.2.5).
        (Order order, OrderItem item) = await SeedItemAsync();
        RefundIban iban = RefundIban.Create(order.Id, Iban, Now);
        await SaveAsync(c => c.RefundIbans.Add(iban));
        await SaveAsync(c =>
        {
            c.Cancellations.Add(Cancellation.ByBuyer(item.Id, 1, Amounts, iban.Id, Now));
            c.LateDeliveryTerminations.Add(LateDeliveryTermination.ByBuyer(item.Id, 1, Today, Amounts, iban.Id, Now));
            c.Withdrawals.Add(Withdrawal.ByBuyer(item.Id, 1, Today, null, Amounts, iban.Id, Now));
        });

        await using (AppDbContext context = NewContext())
        {
            context.RefundIbans.Remove(await context.RefundIbans.SingleAsync(i => i.Id == iban.Id, Ct));
            await context.SaveChangesAsync(Ct);
        }

        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM refund_ibans"));
        Assert.Equal(3L, await ScalarAsync<long>(
            "SELECT (SELECT count(*) FROM cancellations WHERE refund_iban_id IS NULL) + (SELECT count(*) FROM late_delivery_terminations WHERE refund_iban_id IS NULL)"
            + " + (SELECT count(*) FROM withdrawals WHERE refund_iban_id IS NULL)"));
    }

    // ---- KK7: the links and the deletion ----

    [Fact]
    public async Task DeletingAnItemWithARecord_IsRefusedByTheDatabase()
    {
        // Each table's row alone holds its item: the item's deletion names that table's link. The closure's withdrawal belongs to another
        // item, and the receipt is written without a link, so only the record under test holds the item.
        Admin admin = await SeedAdminAsync();
        (_, OrderItem withdrawalItem) = await SeedItemAsync();
        Withdrawal withdrawal = await SeedWithdrawalAsync(withdrawalItem.Id);

        foreach ((string table, string constraint) in new[]
        {
            ("cancellations", "fk_cancellations_order_items_order_item_id"),
            ("item_removals", "fk_item_removals_order_items_order_item_id"),
            ("late_delivery_terminations", "fk_late_delivery_terminations_order_items_order_item_id"),
            ("withdrawals", "fk_withdrawals_order_items_order_item_id"),
            ("returns", "fk_returns_order_items_order_item_id"),
            ("not_returned_closures", "fk_not_returned_closures_order_items_order_item_id"),
            ("defect_claims", "fk_defect_claims_order_items_order_item_id"),
        })
        {
            (_, OrderItem item) = await SeedItemAsync();
            await SaveAsync(c => c.Add(NewItemLinkedRecord(table, item.Id, admin, withdrawal.Id)));

            PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM order_items WHERE id = '{item.Id}'"));

            Assert.Equal((ForeignKeyViolation, constraint), (error.SqlState, error.ConstraintName));
            Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM order_items WHERE id = '{item.Id}'"));
        }
    }

    [Fact]
    public async Task DeletingAnOrderWithAnIban_IsRefusedByTheDatabase()
    {
        Order order = await SeedOrderAsync();
        await SaveAsync(c => c.RefundIbans.Add(RefundIban.Create(order.Id, Iban, Now)));

        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM orders WHERE id = '{order.Id}'"));

        Assert.Equal((ForeignKeyViolation, "fk_refund_ibans_orders_order_id"), (error.SqlState, error.ConstraintName));
    }

    [Fact]
    public async Task DeletingAWithdrawalOrATerminationOrATransitionWithARecord_IsRefusedByTheDatabase()
    {
        (Order order, OrderItem item) = await SeedItemAsync();
        Admin admin = await SeedAdminAsync();
        Withdrawal received = await SeedWithdrawalAsync(item.Id);
        Withdrawal closed = await SeedWithdrawalAsync(item.Id);
        LateDeliveryTermination termination = LateDeliveryTermination.ByBuyer(item.Id, 1, Today, Amounts, null, Now);
        TransitionRecord transition = TransitionRecord.Create(order.Id, TransitionCode.S3, ActorKind.Buyer, null, null, null, null, null, null, null, Now);
        Cancellation cancellation = Cancellation.ByBuyer(item.Id, 1, Amounts, null, Now);
        cancellation.TransitionRecordId = transition.Id;
        await SaveAsync(c =>
        {
            c.LateDeliveryTerminations.Add(termination);
            c.TransitionRecords.Add(transition);
        });
        await SaveAsync(c =>
        {
            c.Returns.AddRange(
                Return.ForWithdrawal(item.Id, received.Id, 1, Today, admin.Id, admin.Name, Now),
                Return.ForLateDeliveryTermination(item.Id, termination.Id, 1, Today, admin.Id, admin.Name, Now));
            c.NotReturnedClosures.Add(NotReturnedClosure.Create(item.Id, closed.Id, 1, admin.Id, admin.Name, Now));
            c.Cancellations.Add(cancellation);
        });

        foreach ((string sql, string constraint) in new[]
        {
            ($"DELETE FROM withdrawals WHERE id = '{received.Id}'", "fk_returns_withdrawals_withdrawal_id"),
            ($"DELETE FROM withdrawals WHERE id = '{closed.Id}'", "fk_not_returned_closures_withdrawals_withdrawal_id"),
            ($"DELETE FROM late_delivery_terminations WHERE id = '{termination.Id}'", "fk_returns_late_delivery_terminations_late_delivery_terminatio"),
            ($"DELETE FROM transition_records WHERE id = '{transition.Id}'", "fk_cancellations_transition_records_transition_record_id"),
        })
        {
            PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(sql));
            Assert.Equal((ForeignKeyViolation, constraint), (error.SqlState, error.ConstraintName));
        }
    }

    [Fact]
    public async Task DeletingAProvinceOfAReturnAddress_IsRefusedByTheDatabase()
    {
        (_, OrderItem item) = await SeedItemAsync();
        (_, OrderItem claimItem) = await SeedItemAsync();
        // Two provinces the order does not hold, so only the records' links are under test.
        Province withdrawalProvince = Province.Create("35", "İzmir", Now);
        Province claimProvince = Province.Create("34", "İstanbul", Now);
        await SaveAsync(c => c.Provinces.AddRange(withdrawalProvince, claimProvince));
        await SaveAsync(c =>
        {
            c.Withdrawals.Add(Withdrawal.ByBuyer(item.Id, 1, Today, new ReturnAddress("Firma Depo", withdrawalProvince.Id, "Bornova", "Depo Sokak 1", null), Amounts, null, Now));
            c.DefectClaims.Add(DefectClaim.Create(claimItem.Id, 1, "Kırık", new ReturnAddress("Firma Depo", claimProvince.Id, "Kadıköy", "Depo Sokak 2", null), Now));
        });

        PostgresException withdrawalLink = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM provinces WHERE id = '{withdrawalProvince.Id}'"));
        PostgresException claimLink = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM provinces WHERE id = '{claimProvince.Id}'"));

        Assert.Equal((ForeignKeyViolation, "fk_withdrawals_provinces_return_province_id"), (withdrawalLink.SqlState, withdrawalLink.ConstraintName));
        Assert.Equal((ForeignKeyViolation, "fk_defect_claims_provinces_return_province_id"), (claimLink.SqlState, claimLink.ConstraintName));
    }

    [Fact]
    public async Task DeletingTheAdmin_KeepsTheRecords_EmptiesTheirLinkAndKeepsTheName()
    {
        (_, OrderItem item) = await SeedItemAsync();
        Admin admin = await SeedAdminAsync();
        Withdrawal withdrawal = Withdrawal.ByAdmin(item.Id, 1, admin.Id, admin.Name, Today, null, Amounts, null, Now);
        await SaveAsync(c => c.Withdrawals.Add(withdrawal));
        await SaveAsync(c =>
        {
            c.Cancellations.Add(Cancellation.ByAdmin(item.Id, 1, admin.Id, admin.Name, CancellationReason.OutOfStock, null, Amounts, Now));
            c.ItemRemovals.Add(ItemRemoval.Create(item.Id, 1, admin.Id, admin.Name, Amounts, Now));
            c.LateDeliveryTerminations.Add(LateDeliveryTermination.ByAdmin(item.Id, 1, admin.Id, admin.Name, Today, Amounts, null, Now));
            c.Returns.Add(Return.ForWithdrawal(item.Id, withdrawal.Id, 1, Today, admin.Id, admin.Name, Now));
            c.NotReturnedClosures.Add(NotReturnedClosure.Create(item.Id, withdrawal.Id, 1, admin.Id, admin.Name, Now));
        });

        await ExecuteAsync($"DELETE FROM admins WHERE id = '{admin.Id}'");

        foreach (string table in new[] { "cancellations", "item_removals", "late_delivery_terminations", "withdrawals", "returns", "not_returned_closures" })
        {
            Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM {table} WHERE admin_id IS NULL AND admin_name = 'Veli'"));
        }
    }

    [Fact]
    public async Task DeletingTheClaim_KeepsTheReceipt_AndEmptiesItsLink()
    {
        // 06 4.30: the claim is deleted by its retention, by the disposal's explicit delete; the receipt's link to it is emptied (06 4.26).
        (_, OrderItem item) = await SeedItemAsync();
        Admin admin = await SeedAdminAsync();
        DefectClaim claim = DefectClaim.Create(item.Id, 2, "Kırık", null, Now);
        await SaveAsync(c => c.DefectClaims.Add(claim));
        await SaveAsync(c => c.Returns.Add(Return.ForDefectClaim(item.Id, claim.Id, 2, Today, admin.Id, admin.Name, Now)));

        await using (AppDbContext context = NewContext())
        {
            context.DefectClaims.Remove(await context.DefectClaims.SingleAsync(d => d.Id == claim.Id, Ct));
            await context.SaveChangesAsync(Ct);
        }

        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM defect_claims"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM returns WHERE defect_claim_id IS NULL AND quantity = 2"));
    }

    [Theory]
    [MemberData(nameof(PermanentTables))]
    public async Task DeletingAPermanentRecordThroughTheContext_IsRefused_AndTheRowStays(string table)
    {
        Admin admin = await SeedAdminAsync();
        (_, OrderItem item) = await SeedItemAsync();
        Withdrawal withdrawal = await SeedWithdrawalAsync(item.Id);
        object record = table == "withdrawals" ? withdrawal : NewItemLinkedRecord(table, item.Id, admin, withdrawal.Id);
        if (record is not Withdrawal)
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
    public async Task ChangingARecordInPlace_IsAccepted()
    {
        // The records are not "only appended": the receipt's arrival date is corrected in place and its stock step is written once; the
        // closure is reopened; the disposal empties the administrator's link and name (06 4.26, 4.28; K-1030 (1)).
        (_, OrderItem item) = await SeedItemAsync();
        Admin admin = await SeedAdminAsync();
        Withdrawal withdrawal = await SeedWithdrawalAsync(item.Id);
        Return received = Return.ForWithdrawal(item.Id, withdrawal.Id, 1, Today, admin.Id, admin.Name, Now);
        NotReturnedClosure closure = NotReturnedClosure.Create(item.Id, withdrawal.Id, 1, admin.Id, admin.Name, Now);
        await SaveAsync(c =>
        {
            c.Returns.Add(received);
            c.NotReturnedClosures.Add(closure);
        });

        await using (AppDbContext context = NewContext())
        {
            Return loadedReturn = await context.Returns.SingleAsync(r => r.Id == received.Id, Ct);
            loadedReturn.ArrivalDate = Today.AddDays(-1);
            loadedReturn.StockAddedAt = Now.AddHours(1);
            NotReturnedClosure loadedClosure = await context.NotReturnedClosures.SingleAsync(r => r.Id == closure.Id, Ct);
            loadedClosure.ReopenedAt = Now.AddHours(1);
            loadedClosure.AdminId = null;
            loadedClosure.AdminName = null;
            await context.SaveChangesAsync(Ct);
        }

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM returns WHERE arrival_date = '2026-11-03' AND stock_added_at IS NOT NULL"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM not_returned_closures WHERE reopened_at IS NOT NULL AND admin_id IS NULL AND admin_name IS NULL"));
    }

    // ---- helpers ----

    private static object NewAmountRecord(string table, Guid itemId, Admin admin, ItemAmounts amounts, int quantity = 1)
    {
        return table switch
        {
            "cancellations" => Cancellation.ByBuyer(itemId, quantity, amounts, null, Now),
            "item_removals" => ItemRemoval.Create(itemId, quantity, admin.Id, admin.Name, amounts, Now),
            "late_delivery_terminations" => LateDeliveryTermination.ByAdmin(itemId, quantity, admin.Id, admin.Name, Today, amounts, null, Now),
            "withdrawals" => Withdrawal.ByBuyer(itemId, quantity, Today, null, amounts, null, Now),
            _ => throw new ArgumentOutOfRangeException(nameof(table), table, null),
        };
    }

    private static object NewAddressRecord(string table, Guid itemId, ReturnAddress? address)
    {
        return table switch
        {
            "withdrawals" => Withdrawal.ByBuyer(itemId, 1, Today, address, Amounts, null, Now),
            "defect_claims" => DefectClaim.Create(itemId, 1, "Kırık", address, Now),
            _ => throw new ArgumentOutOfRangeException(nameof(table), table, null),
        };
    }

    private static void SetAddress(object record, string? recipientName, Guid? provinceId, string? district, string? addressLine, string? phone)
    {
        switch (record)
        {
            case Withdrawal w:
                (w.ReturnRecipientName, w.ReturnProvinceId, w.ReturnDistrict, w.ReturnAddressLine, w.ReturnPhone) = (recipientName, provinceId, district, addressLine, phone);
                break;
            case DefectClaim d:
                (d.ReturnRecipientName, d.ReturnProvinceId, d.ReturnDistrict, d.ReturnAddressLine, d.ReturnPhone) = (recipientName, provinceId, district, addressLine, phone);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(record));
        }
    }

    /// <summary>A record of the table that holds the item; the receipt carries no link and the closure the given withdrawal.</summary>
    private static object NewItemLinkedRecord(string table, Guid itemId, Admin admin, Guid withdrawalId)
    {
        switch (table)
        {
            case "returns":
                Return received = Return.ForWithdrawal(itemId, withdrawalId, 1, Today, admin.Id, admin.Name, Now);
                received.WithdrawalId = null;
                return received;
            case "not_returned_closures":
                return NotReturnedClosure.Create(itemId, withdrawalId, 1, admin.Id, admin.Name, Now);
            case "defect_claims":
                return DefectClaim.Create(itemId, 1, "Kırık", null, Now);
            default:
                return NewAmountRecord(table, itemId, admin, Amounts);
        }
    }

    private async Task<Withdrawal> SeedWithdrawalAsync(Guid itemId)
    {
        Withdrawal withdrawal = Withdrawal.ByBuyer(itemId, 1, Today, null, Amounts, null, Now);
        await SaveAsync(c => c.Withdrawals.Add(withdrawal));
        return withdrawal;
    }

    /// <summary>Seeds one more order with one physical item of three units.</summary>
    private async Task<(Order Order, OrderItem Item)> SeedItemAsync()
    {
        Order order = await SeedOrderAsync();
        OrderItem item = OrderItem.Create(order.Id, null, null, ProductType.Physical, "Kapı", 3, 100m, 20m, 50m, Now);
        await SaveAsync(c => c.OrderItems.Add(item));
        return (order, item);
    }

    private async Task<Province> ProvinceAsync()
    {
        References refs = await GetReferencesAsync();
        await using AppDbContext context = NewContext();
        return await context.Provinces.SingleAsync(p => p.Id == refs.ProvinceId, Ct);
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
}
