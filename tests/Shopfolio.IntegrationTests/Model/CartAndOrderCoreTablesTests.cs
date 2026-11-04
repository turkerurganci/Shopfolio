using System.Globalization;
using System.Net;
using System.Reflection;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Migrations;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the cart and order core tests (T109): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(CartAndOrderCoreTablesDefinition.Name)]
public sealed class CartAndOrderCoreTablesDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "CartAndOrderCoreTables";
}

/// <summary>
/// The eight cart and order tables on a real PostgreSQL (T109 criteria 1-8; 06 4.15-4.21, 7.1, 7.2, 7.3, 7.5; 09 10.6): the migration made the
/// tables; the database itself refuses what the rules refuse; the account's deletion takes the member's cart with its items and empties the
/// order's account link; a variant's deletion empties the order item's link and is stopped by an unmarked cart item; the order side's links
/// are restrictive; and the context refuses the deletion of the permanent rows and every change of a transition record but the disposal's.
/// </summary>
[Collection(CartAndOrderCoreTablesDefinition.Name)]
public sealed class CartAndOrderCoreTablesTests : IAsyncLifetime
{
    private const string CheckViolation = PostgresErrorCodes.CheckViolation;
    private const string UniqueViolation = PostgresErrorCodes.UniqueViolation;
    private const string ForeignKeyViolation = PostgresErrorCodes.ForeignKeyViolation;

    private static readonly string[] EightTables =
    [
        "cart_items",
        "carts",
        "checkbox_acceptance_order_items",
        "checkbox_acceptances",
        "internal_notes",
        "order_items",
        "orders",
        "transition_records",
    ];

    private static readonly DateTimeOffset Now = new(2026, 11, 4, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    private int _orderCounter;

    public CartAndOrderCoreTablesTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE checkbox_acceptance_order_items, checkbox_acceptances, internal_notes, transition_records, order_items, orders, cart_items, carts, "
            + "customers, admins, coupons, digital_files, images, variants, products, legal_text_versions, provinces CASCADE");
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
        // 06 4.15-4.21 and 2.5.1 (the common Id and CreatedAt included; money numeric(12,2), percent numeric(5,2), phone ten characters).
        string[] expected =
        [
            "cart_items.cart_id uuid not null",
            "cart_items.created_at timestamp with time zone not null",
            "cart_items.first_added_unit_price numeric(12,2) not null",
            "cart_items.id uuid not null",
            "cart_items.is_merge_notice_pending boolean not null",
            "cart_items.quantity integer not null",
            "cart_items.removed_product_name text",
            "cart_items.variant_id uuid",
            "carts.created_at timestamp with time zone not null",
            "carts.customer_id uuid",
            "carts.guest_cart_key_hash text",
            "carts.id uuid not null",
            "carts.is_payment_unavailable_notice_pending boolean not null",
            "carts.last_accessed_at timestamp with time zone",
            "checkbox_acceptance_order_items.checkbox_acceptance_id uuid not null",
            "checkbox_acceptance_order_items.created_at timestamp with time zone not null",
            "checkbox_acceptance_order_items.id uuid not null",
            "checkbox_acceptance_order_items.order_item_id uuid not null",
            "checkbox_acceptances.checkbox_type text not null",
            "checkbox_acceptances.created_at timestamp with time zone not null",
            "checkbox_acceptances.id uuid not null",
            "checkbox_acceptances.order_id uuid not null",
            "checkbox_acceptances.shown_text text not null",
            "internal_notes.admin_id uuid",
            "internal_notes.admin_name text not null",
            "internal_notes.created_at timestamp with time zone not null",
            "internal_notes.id uuid not null",
            "internal_notes.order_id uuid not null",
            "internal_notes.text text not null",
            "order_items.coupon_share numeric(12,2) not null",
            "order_items.created_at timestamp with time zone not null",
            "order_items.delivery_date date",
            "order_items.delivery_marked_at timestamp with time zone",
            "order_items.digital_file_id uuid",
            "order_items.discount_percentage numeric(5,2)",
            "order_items.discounted_unit_price numeric(12,2)",
            "order_items.download_allowance integer",
            "order_items.download_count integer not null",
            "order_items.first_option_value text",
            "order_items.fulfilment_period integer",
            "order_items.id uuid not null",
            "order_items.image_id uuid",
            "order_items.order_id uuid not null",
            "order_items.product_id uuid",
            "order_items.product_name character varying(150) not null",
            "order_items.product_type text not null",
            "order_items.quantity integer not null",
            "order_items.reference_price numeric(12,2)",
            "order_items.second_option_value text",
            "order_items.unit_price numeric(12,2) not null",
            "order_items.variant_id uuid",
            "order_items.vat_amount numeric(12,2) not null",
            "order_items.vat_rate numeric(5,2) not null",
            "order_items.withdrawal_exemption_reason text",
            "orders.bank_transfer_iban text",
            "orders.billing_address_line text",
            "orders.billing_district text",
            "orders.billing_province_id uuid",
            "orders.billing_recipient_name text",
            "orders.carrier text",
            "orders.cart_id uuid",
            "orders.company_address_line text not null",
            "orders.company_district text not null",
            "orders.company_email text not null",
            "orders.company_kep_address text not null",
            "orders.company_legal_name text not null",
            "orders.company_mersis_number text",
            "orders.company_phone character varying(10) not null",
            "orders.company_province_id uuid not null",
            "orders.company_tax_id_number text",
            "orders.company_trade_registry_number text",
            "orders.company_type text not null",
            "orders.confirmation_key text not null",
            "orders.contact_email text",
            "orders.coupon_code text",
            "orders.coupon_id uuid",
            "orders.created_at timestamp with time zone not null",
            "orders.customer_id uuid",
            "orders.distance_sales_contract_text text not null",
            "orders.distance_sales_contract_version_id uuid not null",
            "orders.id uuid not null",
            "orders.ip_address inet",
            "orders.is_account_deleted boolean not null",
            "orders.is_guest_order boolean not null",
            "orders.is_own_vehicle_delivery boolean not null",
            "orders.normalized_contact_email text",
            "orders.order_access_key text",
            "orders.order_number text not null",
            "orders.order_status text not null",
            "orders.other_carrier_name text",
            "orders.payment_method text not null",
            "orders.payment_reminder_at timestamp with time zone",
            "orders.payment_status text not null",
            "orders.payment_window_ends_at timestamp with time zone not null",
            "orders.pre_information_form_text text not null",
            "orders.pre_information_form_version_id uuid not null",
            "orders.privacy_notice_version_id uuid not null",
            "orders.shipping_address_line text",
            "orders.shipping_district text",
            "orders.shipping_fee numeric(12,2) not null",
            "orders.shipping_fee_vat_breakdown jsonb",
            "orders.shipping_period integer",
            "orders.shipping_phone character varying(10)",
            "orders.shipping_province_id uuid",
            "orders.shipping_recipient_name text",
            "orders.total numeric(12,2) not null",
            "orders.tracking_number text",
            "orders.version integer not null",
            "transition_records.actor_kind text not null",
            "transition_records.admin_id uuid",
            "transition_records.admin_name text",
            "transition_records.cancellation_reason text",
            "transition_records.corrected_transition_record_id uuid",
            "transition_records.correction_reason text",
            "transition_records.created_at timestamp with time zone not null",
            "transition_records.id uuid not null",
            "transition_records.order_id uuid not null",
            "transition_records.reason_note text",
            "transition_records.transition_code text not null",
            "transition_records.transition_reason text",
        ];

        List<string> columns = await QueryAsync(
            """
            SELECT c.relname || '.' || a.attname || ' ' || format_type(a.atttypid, a.atttypmod) || CASE WHEN a.attnotnull THEN ' not null' ELSE '' END
            FROM pg_attribute a JOIN pg_class c ON c.oid = a.attrelid JOIN pg_namespace n ON n.oid = c.relnamespace
            WHERE n.nspname = 'public' AND c.relname = ANY(@names) AND a.attnum > 0 AND NOT a.attisdropped
            """,
            ("names", EightTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), columns.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ColumnDefaults_AreTheDefaultColumnOfTheFieldTables()
    {
        // 06 4.15-4.18: the filled "Default" cells; every other cell of the eight tables is "-".
        string[] expected =
        [
            "cart_items.is_merge_notice_pending = false",
            "carts.is_payment_unavailable_notice_pending = false",
            "order_items.coupon_share = 0.0",
            "order_items.download_count = 0",
            "orders.is_account_deleted = false",
            "orders.is_own_vehicle_delivery = false",
            "orders.order_status = 'Placed'::text",
            "orders.payment_status = 'Pending'::text",
            "orders.version = 1",
        ];

        List<string> defaults = await QueryAsync(
            "SELECT table_name || '.' || column_name || ' = ' || column_default FROM information_schema.columns WHERE table_schema = 'public' AND table_name = ANY(@names) AND column_default IS NOT NULL",
            ("names", EightTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), defaults.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Indexes_AreTheDocumentedOnes()
    {
        // 06 7.1 (the unique ones), 7.5 (the filtered and composite reads) and the foreign key indexes of 7.2. Two names are cut to 63 bytes by
        // the framework (D-51): the pair index of the checkbox acceptance's items.
        string[] expected =
        [
            "cart_items.ix_cart_items_cart_id | CREATE INDEX ix_cart_items_cart_id ON public.cart_items USING btree (cart_id)",
            "cart_items.ix_cart_items_cart_id_variant_id | CREATE UNIQUE INDEX ix_cart_items_cart_id_variant_id ON public.cart_items USING btree (cart_id, variant_id) WHERE (removed_product_name IS NULL)",
            "cart_items.ix_cart_items_variant_id | CREATE INDEX ix_cart_items_variant_id ON public.cart_items USING btree (variant_id)",
            "cart_items.pk_cart_items | CREATE UNIQUE INDEX pk_cart_items ON public.cart_items USING btree (id)",
            "carts.ix_carts_customer_id | CREATE UNIQUE INDEX ix_carts_customer_id ON public.carts USING btree (customer_id)",
            "carts.ix_carts_guest_cart_key_hash | CREATE UNIQUE INDEX ix_carts_guest_cart_key_hash ON public.carts USING btree (guest_cart_key_hash)",
            "carts.ix_carts_last_accessed_at | CREATE INDEX ix_carts_last_accessed_at ON public.carts USING btree (last_accessed_at) WHERE (guest_cart_key_hash IS NOT NULL)",
            "carts.pk_carts | CREATE UNIQUE INDEX pk_carts ON public.carts USING btree (id)",
            "checkbox_acceptance_order_items.ix_checkbox_acceptance_order_items_checkbox_acceptance_id_orde | CREATE UNIQUE INDEX ix_checkbox_acceptance_order_items_checkbox_acceptance_id_orde ON public.checkbox_acceptance_order_items USING btree (checkbox_acceptance_id, order_item_id)",
            "checkbox_acceptance_order_items.ix_checkbox_acceptance_order_items_order_item_id | CREATE INDEX ix_checkbox_acceptance_order_items_order_item_id ON public.checkbox_acceptance_order_items USING btree (order_item_id)",
            "checkbox_acceptance_order_items.pk_checkbox_acceptance_order_items | CREATE UNIQUE INDEX pk_checkbox_acceptance_order_items ON public.checkbox_acceptance_order_items USING btree (id)",
            "checkbox_acceptances.ix_checkbox_acceptances_order_id_checkbox_type | CREATE UNIQUE INDEX ix_checkbox_acceptances_order_id_checkbox_type ON public.checkbox_acceptances USING btree (order_id, checkbox_type)",
            "checkbox_acceptances.pk_checkbox_acceptances | CREATE UNIQUE INDEX pk_checkbox_acceptances ON public.checkbox_acceptances USING btree (id)",
            "internal_notes.ix_internal_notes_admin_id | CREATE INDEX ix_internal_notes_admin_id ON public.internal_notes USING btree (admin_id)",
            "internal_notes.ix_internal_notes_order_id | CREATE UNIQUE INDEX ix_internal_notes_order_id ON public.internal_notes USING btree (order_id)",
            "internal_notes.pk_internal_notes | CREATE UNIQUE INDEX pk_internal_notes ON public.internal_notes USING btree (id)",
            "order_items.ix_order_items_digital_file_id | CREATE INDEX ix_order_items_digital_file_id ON public.order_items USING btree (digital_file_id)",
            "order_items.ix_order_items_image_id | CREATE INDEX ix_order_items_image_id ON public.order_items USING btree (image_id)",
            "order_items.ix_order_items_order_id | CREATE INDEX ix_order_items_order_id ON public.order_items USING btree (order_id)",
            "order_items.ix_order_items_product_id | CREATE INDEX ix_order_items_product_id ON public.order_items USING btree (product_id)",
            "order_items.ix_order_items_variant_id | CREATE INDEX ix_order_items_variant_id ON public.order_items USING btree (variant_id)",
            "order_items.pk_order_items | CREATE UNIQUE INDEX pk_order_items ON public.order_items USING btree (id)",
            "orders.ix_orders_billing_province_id | CREATE INDEX ix_orders_billing_province_id ON public.orders USING btree (billing_province_id)",
            "orders.ix_orders_cart_id | CREATE INDEX ix_orders_cart_id ON public.orders USING btree (cart_id)",
            "orders.ix_orders_company_province_id | CREATE INDEX ix_orders_company_province_id ON public.orders USING btree (company_province_id)",
            "orders.ix_orders_confirmation_key | CREATE UNIQUE INDEX ix_orders_confirmation_key ON public.orders USING btree (confirmation_key)",
            "orders.ix_orders_coupon_id | CREATE INDEX ix_orders_coupon_id ON public.orders USING btree (coupon_id)",
            "orders.ix_orders_customer_id | CREATE INDEX ix_orders_customer_id ON public.orders USING btree (customer_id)",
            "orders.ix_orders_distance_sales_contract_version_id | CREATE INDEX ix_orders_distance_sales_contract_version_id ON public.orders USING btree (distance_sales_contract_version_id)",
            "orders.ix_orders_ip_address_created_at | CREATE INDEX ix_orders_ip_address_created_at ON public.orders USING btree (ip_address, created_at)",
            "orders.ix_orders_normalized_contact_email | CREATE INDEX ix_orders_normalized_contact_email ON public.orders USING btree (normalized_contact_email)",
            "orders.ix_orders_order_access_key | CREATE UNIQUE INDEX ix_orders_order_access_key ON public.orders USING btree (order_access_key)",
            "orders.ix_orders_order_number | CREATE UNIQUE INDEX ix_orders_order_number ON public.orders USING btree (order_number)",
            "orders.ix_orders_order_status_payment_status_created_at | CREATE INDEX ix_orders_order_status_payment_status_created_at ON public.orders USING btree (order_status, payment_status, created_at)",
            "orders.ix_orders_payment_reminder_at | CREATE INDEX ix_orders_payment_reminder_at ON public.orders USING btree (payment_reminder_at) WHERE (payment_status = 'Pending'::text)",
            "orders.ix_orders_payment_window_ends_at | CREATE INDEX ix_orders_payment_window_ends_at ON public.orders USING btree (payment_window_ends_at) WHERE (payment_status = 'Pending'::text)",
            "orders.ix_orders_pre_information_form_version_id | CREATE INDEX ix_orders_pre_information_form_version_id ON public.orders USING btree (pre_information_form_version_id)",
            "orders.ix_orders_privacy_notice_version_id | CREATE INDEX ix_orders_privacy_notice_version_id ON public.orders USING btree (privacy_notice_version_id)",
            "orders.ix_orders_shipping_province_id | CREATE INDEX ix_orders_shipping_province_id ON public.orders USING btree (shipping_province_id)",
            "orders.pk_orders | CREATE UNIQUE INDEX pk_orders ON public.orders USING btree (id)",
            "transition_records.ix_transition_records_admin_id | CREATE INDEX ix_transition_records_admin_id ON public.transition_records USING btree (admin_id)",
            "transition_records.ix_transition_records_corrected_transition_record_id | CREATE INDEX ix_transition_records_corrected_transition_record_id ON public.transition_records USING btree (corrected_transition_record_id)",
            "transition_records.ix_transition_records_order_id_created_at | CREATE INDEX ix_transition_records_order_id_created_at ON public.transition_records USING btree (order_id, created_at)",
            "transition_records.pk_transition_records | CREATE UNIQUE INDEX pk_transition_records ON public.transition_records USING btree (id)",
        ];

        List<string> indexes = await QueryAsync(
            "SELECT tablename || '.' || indexname || ' | ' || indexdef FROM pg_indexes WHERE schemaname = 'public' AND tablename = ANY(@names)",
            ("names", EightTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), indexes.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ForeignKeys_HaveTheDeleteBehaviorOfTheLinkTable()
    {
        // 06 7.2: the member's cart and the cart's item cascade (c); the account, administrator, product and variant links are emptied (n);
        // every other link is restrictive (a). Order.CartId is a value, not a link (06 7.2.3). Three names are cut to 63 bytes (D-51).
        string[] expected =
        [
            "cart_items fk_cart_items_carts_cart_id c",
            "cart_items fk_cart_items_variants_variant_id n",
            "carts fk_carts_customers_customer_id c",
            "checkbox_acceptance_order_items fk_checkbox_acceptance_order_items_checkbox_acceptances_checkb a",
            "checkbox_acceptance_order_items fk_checkbox_acceptance_order_items_order_items_order_item_id a",
            "checkbox_acceptances fk_checkbox_acceptances_orders_order_id a",
            "internal_notes fk_internal_notes_admins_admin_id n",
            "internal_notes fk_internal_notes_orders_order_id a",
            "order_items fk_order_items_digital_files_digital_file_id a",
            "order_items fk_order_items_images_image_id a",
            "order_items fk_order_items_orders_order_id a",
            "order_items fk_order_items_products_product_id n",
            "order_items fk_order_items_variants_variant_id n",
            "orders fk_orders_coupons_coupon_id a",
            "orders fk_orders_customers_customer_id n",
            "orders fk_orders_legal_text_versions_distance_sales_contract_version_ a",
            "orders fk_orders_legal_text_versions_pre_information_form_version_id a",
            "orders fk_orders_legal_text_versions_privacy_notice_version_id a",
            "orders fk_orders_provinces_billing_province_id a",
            "orders fk_orders_provinces_company_province_id a",
            "orders fk_orders_provinces_shipping_province_id a",
            "transition_records fk_transition_records_admins_admin_id n",
            "transition_records fk_transition_records_orders_order_id a",
            "transition_records fk_transition_records_transition_records_corrected_transition_ a",
        ];

        List<string> links = await QueryAsync(
            "SELECT conrelid::regclass::text || ' ' || conname || ' ' || confdeltype::text FROM pg_constraint WHERE contype = 'f' AND conrelid = ANY(ARRAY(SELECT x::regclass FROM unnest(@oids) x))",
            ("oids", EightTables.Select(t => "public." + t).ToArray()));

        Assert.Equal(expected.Order(StringComparer.Ordinal), links.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task CheckConstraints_AreTheDocumentedOnes()
    {
        // 06 7.3, the type rules of 2.5.1 (phone, VAT rate) and the enum columns' allowed values (06 2.9.2).
        string[] expected =
        [
            "cart_items.ck_cart_items_first_added_unit_price_above_zero",
            "cart_items.ck_cart_items_quantity_above_zero",
            "cart_items.ck_cart_items_removed_name_without_variant",
            "carts.ck_carts_exactly_one_owner",
            "carts.ck_carts_last_accessed_at_only_for_guest",
            "checkbox_acceptances.ck_checkbox_acceptances_checkbox_type",
            "order_items.ck_order_items_coupon_share_not_negative",
            "order_items.ck_order_items_delivery_mark_and_date_together",
            "order_items.ck_order_items_digital_file_only_digital",
            "order_items.ck_order_items_discount_fields_together",
            "order_items.ck_order_items_discount_percentage_range",
            "order_items.ck_order_items_discounted_unit_price_range",
            "order_items.ck_order_items_download_allowance_only_digital",
            "order_items.ck_order_items_download_count_within_allowance",
            "order_items.ck_order_items_fulfilment_period_only_service",
            "order_items.ck_order_items_product_type",
            "order_items.ck_order_items_quantity_above_zero_one_if_digital",
            "order_items.ck_order_items_reference_price_above_zero",
            "order_items.ck_order_items_second_option_needs_first",
            "order_items.ck_order_items_unit_price_above_zero",
            "order_items.ck_order_items_vat_amount_not_negative",
            "order_items.ck_order_items_vat_rate_range",
            "order_items.ck_order_items_withdrawal_exemption_only_physical",
            "order_items.ck_order_items_withdrawal_exemption_reason",
            "orders.ck_orders_at_most_one_carrier",
            "orders.ck_orders_card_without_payment_reminder",
            "orders.ck_orders_company_phone_ten_digits",
            "orders.ck_orders_company_type",
            "orders.ck_orders_coupon_and_code_together",
            "orders.ck_orders_deleted_account_without_link",
            "orders.ck_orders_disposal_fields_together",
            "orders.ck_orders_iban_only_on_bank_transfer",
            "orders.ck_orders_order_status",
            "orders.ck_orders_own_vehicle_without_carrier",
            "orders.ck_orders_payment_method",
            "orders.ck_orders_payment_status",
            "orders.ck_orders_shipping_address_all_or_none",
            "orders.ck_orders_shipping_fee_and_breakdown",
            "orders.ck_orders_shipping_period_not_negative",
            "orders.ck_orders_shipping_phone_ten_digits",
            "orders.ck_orders_status_pair_allowed",
            "orders.ck_orders_total_above_zero",
            "orders.ck_orders_tracking_number_with_carrier",
            "transition_records.ck_transition_records_actor_kind",
            "transition_records.ck_transition_records_at_most_one_reason",
            "transition_records.ck_transition_records_auto_cancellation_only_system_s3_o2",
            "transition_records.ck_transition_records_cancellation_reason",
            "transition_records.ck_transition_records_cancellation_reason_by_admin",
            "transition_records.ck_transition_records_closure_only_s3_s4_s9",
            "transition_records.ck_transition_records_correction_fields_only_on_correction",
            "transition_records.ck_transition_records_correction_reason",
            "transition_records.ck_transition_records_reason_only_on_cancellation_codes",
            "transition_records.ck_transition_records_transition_code",
            "transition_records.ck_transition_records_transition_reason",
        ];

        List<string> checks = await QueryAsync(
            "SELECT conrelid::regclass::text || '.' || conname FROM pg_constraint WHERE contype = 'c' AND conrelid = ANY(ARRAY(SELECT x::regclass FROM unnest(@oids) x))",
            ("oids", EightTables.Select(t => "public." + t).ToArray()));

        Assert.Equal(expected.Order(StringComparer.Ordinal), checks.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Migration_HasNoDownMethod()
    {
        MethodInfo? down = typeof(CreateCartAndOrderCoreTables)
            .GetMethod("Down", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);

        Assert.Null(down);
    }

    // ---- KK2: the cart's owner ----

    [Fact]
    public async Task Cart_WithoutAnOwner_IsRefused()
    {
        Cart cart = Cart.ForGuest(Digest("g1"), Now);
        cart.GuestCartKeyHash = null;
        cart.LastAccessedAt = null;

        Assert.Equal((CheckViolation, "ck_carts_exactly_one_owner"), await ViolationOfAsync(c => c.Carts.Add(cart)));
    }

    [Fact]
    public async Task Cart_WithBothOwners_IsRefused()
    {
        Customer customer = await SeedCustomerAsync();
        Cart cart = Cart.ForGuest(Digest("g1"), Now);
        cart.CustomerId = customer.Id;

        Assert.Equal((CheckViolation, "ck_carts_exactly_one_owner"), await ViolationOfAsync(c => c.Carts.Add(cart)));
    }

    [Fact]
    public async Task GuestCart_WithoutTheLastAccessMoment_IsRefused_AndMemberCartWithIt_IsRefused()
    {
        Customer customer = await SeedCustomerAsync();
        Cart guest = Cart.ForGuest(Digest("g1"), Now);
        guest.LastAccessedAt = null;
        Cart member = Cart.ForCustomer(customer.Id, Now);
        member.LastAccessedAt = Now;

        Assert.Equal((CheckViolation, "ck_carts_last_accessed_at_only_for_guest"), await ViolationOfAsync(c => c.Carts.Add(guest)));
        Assert.Equal((CheckViolation, "ck_carts_last_accessed_at_only_for_guest"), await ViolationOfAsync(c => c.Carts.Add(member)));
    }

    [Fact]
    public async Task SecondCartOfTheMember_IsRefused()
    {
        Customer customer = await SeedCustomerAsync();
        await SaveAsync(c => c.Carts.Add(Cart.ForCustomer(customer.Id, Now)));

        Assert.Equal((UniqueViolation, "ix_carts_customer_id"), await ViolationOfAsync(c => c.Carts.Add(Cart.ForCustomer(customer.Id, Now))));
    }

    [Fact]
    public async Task SecondCartWithTheSameGuestDigest_IsRefused()
    {
        await SaveAsync(c => c.Carts.Add(Cart.ForGuest(Digest("same"), Now)));

        Assert.Equal((UniqueViolation, "ix_carts_guest_cart_key_hash"), await ViolationOfAsync(c => c.Carts.Add(Cart.ForGuest(Digest("same"), Now))));
    }

    [Fact]
    public async Task MemberCartAndGuestCart_AreAccepted_WithTheNoticeOff()
    {
        Customer customer = await SeedCustomerAsync();

        await SaveAsync(c => c.Carts.AddRange(Cart.ForCustomer(customer.Id, Now), Cart.ForGuest(Digest("g1"), Now)));

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM carts WHERE NOT is_payment_unavailable_notice_pending"));
    }

    // ---- KK3: the cart item ----

    [Fact]
    public async Task UnmarkedItemWithoutAVariant_IsRefused()
    {
        Cart cart = await SeedGuestCartAsync();
        Variant variant = await SeedVariantAsync();
        CartItem item = CartItem.Create(cart.Id, variant.Id, 1, 100m, Now);
        item.VariantId = null;

        Assert.Equal((CheckViolation, "ck_cart_items_removed_name_without_variant"), await ViolationOfAsync(c => c.CartItems.Add(item)));
    }

    [Fact]
    public async Task SecondUnmarkedItemOfTheSameVariant_IsRefused_ButAMarkedOneIsAccepted()
    {
        Cart cart = await SeedGuestCartAsync();
        Variant variant = await SeedVariantAsync();
        await SaveAsync(c => c.CartItems.Add(CartItem.Create(cart.Id, variant.Id, 1, 100m, Now)));

        Assert.Equal(
            (UniqueViolation, "ix_cart_items_cart_id_variant_id"),
            await ViolationOfAsync(c => c.CartItems.Add(CartItem.Create(cart.Id, variant.Id, 2, 100m, Now))));

        CartItem marked = CartItem.Create(cart.Id, variant.Id, 2, 100m, Now);
        marked.RemovedProductName = "Kapı";
        await SaveAsync(c => c.CartItems.Add(marked));

        Assert.Equal(2L, await ScalarAsync<long>($"SELECT count(*) FROM cart_items WHERE cart_id = '{cart.Id}'"));
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(1, 0)]
    public async Task CartItem_WithoutAQuantityOrAPrice_IsRefused(int quantity, int price)
    {
        Cart cart = await SeedGuestCartAsync();
        Variant variant = await SeedVariantAsync();
        string expected = quantity == 0 ? "ck_cart_items_quantity_above_zero" : "ck_cart_items_first_added_unit_price_above_zero";

        Assert.Equal((CheckViolation, expected), await ViolationOfAsync(c => c.CartItems.Add(CartItem.Create(cart.Id, variant.Id, quantity, price, Now))));
    }

    // ---- KK4: the order ----

    [Fact]
    public async Task ValidOrder_IsAccepted_WithTheDefaultsOfTheFieldTable()
    {
        References refs = await SeedReferencesAsync();

        await SaveAsync(c => c.Orders.Add(NewOrder(refs)));

        Assert.Equal(
            "Placed Pending False False 1",
            await ScalarAsync<string>("SELECT order_status || ' ' || payment_status || ' ' || initcap(is_account_deleted::text) || ' ' || initcap(is_own_vehicle_delivery::text) || ' ' || version FROM orders"));
    }

    [Fact]
    public async Task FailedPaymentWithAPlacedOrder_IsRefused()
    {
        References refs = await SeedReferencesAsync();
        Order order = NewOrder(refs);

        Assert.Equal(
            (CheckViolation, "ck_orders_status_pair_allowed"),
            await ViolationOfAsync(c =>
            {
                c.Orders.Add(order);
                c.Entry(order).Property(o => o.PaymentStatus).CurrentValue = PaymentStatus.Failed;
            }));
    }

    [Fact]
    public async Task PendingPaymentWithAPreparingOrder_IsRefused()
    {
        References refs = await SeedReferencesAsync();
        Order order = NewOrder(refs);

        Assert.Equal(
            (CheckViolation, "ck_orders_status_pair_allowed"),
            await ViolationOfAsync(c =>
            {
                c.Orders.Add(order);
                c.Entry(order).Property(o => o.OrderStatus).CurrentValue = OrderStatus.Preparing;
            }));
    }

    [Fact]
    public async Task PaidPreparingOrder_AndFailedCancelledOrder_AreAccepted()
    {
        References refs = await SeedReferencesAsync();
        Order paid = NewOrder(refs);
        Order failed = NewOrder(refs);

        await SaveAsync(c =>
        {
            c.Orders.AddRange(paid, failed);
            c.Entry(paid).Property(o => o.PaymentStatus).CurrentValue = PaymentStatus.Paid;
            c.Entry(paid).Property(o => o.OrderStatus).CurrentValue = OrderStatus.Preparing;
            c.Entry(failed).Property(o => o.PaymentStatus).CurrentValue = PaymentStatus.Failed;
            c.Entry(failed).Property(o => o.OrderStatus).CurrentValue = OrderStatus.Cancelled;
        });

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM orders"));
    }

    [Fact]
    public async Task EmptyEmailWithAFilledAccessKey_IsRefused()
    {
        References refs = await SeedReferencesAsync();
        Order order = NewOrder(refs);
        order.SetContactEmail(null);

        Assert.Equal((CheckViolation, "ck_orders_disposal_fields_together"), await ViolationOfAsync(c => c.Orders.Add(order)));
    }

    [Fact]
    public async Task FilledEmailWithoutTheBillingAddress_IsRefused()
    {
        References refs = await SeedReferencesAsync();
        Order order = NewOrder(refs);
        order.BillingDistrict = null;

        Assert.Equal((CheckViolation, "ck_orders_disposal_fields_together"), await ViolationOfAsync(c => c.Orders.Add(order)));
    }

    [Fact]
    public async Task DisposedOrder_WithEveryPersonalFieldEmpty_IsAccepted()
    {
        References refs = await SeedReferencesAsync();
        Order order = NewOrder(refs);
        EmptyDisposalFields(order);

        await SaveAsync(c => c.Orders.Add(order));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM orders WHERE contact_email IS NULL AND order_access_key IS NULL"));
    }

    [Fact]
    public async Task DeletedAccountWithAFilledAccountLink_IsRefused()
    {
        References refs = await SeedReferencesAsync();
        Customer customer = await SeedCustomerAsync();
        Order order = NewOrder(refs);
        order.CustomerId = customer.Id;
        order.IsAccountDeleted = true;

        Assert.Equal((CheckViolation, "ck_orders_deleted_account_without_link"), await ViolationOfAsync(c => c.Orders.Add(order)));
    }

    [Fact]
    public async Task BankTransferOrderWithoutAnIban_IsRefused_AndCardOrderWithOne_IsRefused()
    {
        References refs = await SeedReferencesAsync();
        Order transfer = NewOrder(refs, PaymentMethod.BankTransfer);
        transfer.BankTransferIban = null;
        Order card = NewOrder(refs);
        card.BankTransferIban = "TR000000000000000000000000";

        Assert.Equal((CheckViolation, "ck_orders_iban_only_on_bank_transfer"), await ViolationOfAsync(c => c.Orders.Add(transfer)));
        Assert.Equal((CheckViolation, "ck_orders_iban_only_on_bank_transfer"), await ViolationOfAsync(c => c.Orders.Add(card)));
    }

    [Fact]
    public async Task CardOrderWithAPaymentReminder_IsRefused()
    {
        References refs = await SeedReferencesAsync();
        Order order = NewOrder(refs);
        order.PaymentReminderAt = Now.AddHours(1);

        Assert.Equal((CheckViolation, "ck_orders_card_without_payment_reminder"), await ViolationOfAsync(c => c.Orders.Add(order)));
    }

    [Fact]
    public async Task CouponWithoutItsCode_IsRefused()
    {
        References refs = await SeedReferencesAsync();
        Coupon coupon = await SeedCouponAsync();
        Order order = NewOrder(refs);
        order.CouponId = coupon.Id;

        Assert.Equal((CheckViolation, "ck_orders_coupon_and_code_together"), await ViolationOfAsync(c => c.Orders.Add(order)));
    }

    [Fact]
    public async Task PartialShippingAddress_IsRefused()
    {
        References refs = await SeedReferencesAsync();
        Order order = NewOrder(refs);
        order.ShippingRecipientName = "Ali Veli";

        Assert.Equal((CheckViolation, "ck_orders_shipping_address_all_or_none"), await ViolationOfAsync(c => c.Orders.Add(order)));
    }

    [Fact]
    public async Task TrackingNumberWithoutACarrier_IsRefused_AndOwnVehicleWithACarrier_IsRefused()
    {
        References refs = await SeedReferencesAsync();
        Order tracking = NewOrder(refs);
        tracking.TrackingNumber = "123";
        Order ownVehicle = NewOrder(refs);
        ownVehicle.IsOwnVehicleDelivery = true;
        ownVehicle.Carrier = "Yurtici";
        ownVehicle.TrackingNumber = "123";

        Assert.Equal((CheckViolation, "ck_orders_tracking_number_with_carrier"), await ViolationOfAsync(c => c.Orders.Add(tracking)));
        Assert.Equal((CheckViolation, "ck_orders_own_vehicle_without_carrier"), await ViolationOfAsync(c => c.Orders.Add(ownVehicle)));
    }

    [Fact]
    public async Task ShippingFeeWithoutItsBreakdown_IsRefused()
    {
        References refs = await SeedReferencesAsync();
        Order order = NewOrder(refs);
        order.ShippingFee = 50m;

        Assert.Equal((CheckViolation, "ck_orders_shipping_fee_and_breakdown"), await ViolationOfAsync(c => c.Orders.Add(order)));
    }

    [Fact]
    public async Task SecondOrderWithTheSameNumberAccessKeyOrConfirmationKey_IsRefused()
    {
        References refs = await SeedReferencesAsync();
        Order first = NewOrder(refs);
        await SaveAsync(c => c.Orders.Add(first));

        Order sameNumber = NewOrder(refs);
        sameNumber.OrderNumber = first.OrderNumber;
        Order sameAccess = NewOrder(refs);
        sameAccess.OrderAccessKey = first.OrderAccessKey;
        Order sameConfirmation = NewOrder(refs);
        sameConfirmation.ConfirmationKey = first.ConfirmationKey;

        Assert.Equal((UniqueViolation, "ix_orders_order_number"), await ViolationOfAsync(c => c.Orders.Add(sameNumber)));
        Assert.Equal((UniqueViolation, "ix_orders_order_access_key"), await ViolationOfAsync(c => c.Orders.Add(sameAccess)));
        Assert.Equal((UniqueViolation, "ix_orders_confirmation_key"), await ViolationOfAsync(c => c.Orders.Add(sameConfirmation)));
    }

    [Fact]
    public async Task TwoOrdersWithTheSameContactEmail_AreAccepted()
    {
        References refs = await SeedReferencesAsync();

        await SaveAsync(c => c.Orders.AddRange(NewOrder(refs), NewOrder(refs)));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(DISTINCT normalized_contact_email) FROM orders"));
    }

    [Fact]
    public void Order_WritesTheNormalizedEmailWithTheEmail_AndTheDocumentsOnce()
    {
        Order order = Order.Create("2026-ABCD", "access", "confirm", PaymentMethod.Card, true, "Ali@Example.COM", 10m, 0m, Now.AddHours(1), Now);
        order.FreezeDocuments("form", "contract");

        Assert.Equal("ali@example.com", order.NormalizedContactEmail);
        Assert.Throws<InvalidOperationException>(() => order.FreezeDocuments("form 2", "contract 2"));
    }

    [Fact]
    public void Order_StatusEmailDocumentAndVersionFields_HaveNoPublicSetter()
    {
        string[] writable =
        [
            .. typeof(Order).GetProperties()
                .Where(p => p.SetMethod is { IsPublic: true })
                .Select(p => p.Name)
                .Where(n => n is "OrderStatus" or "PaymentStatus" or "ContactEmail" or "NormalizedContactEmail" or "PreInformationFormText"
                    or "DistanceSalesContractText" or "Version"),
        ];

        Assert.Empty(writable);
    }

    // ---- KK5: the order item ----

    [Fact]
    public async Task ItemWithZeroQuantity_IsRefused()
    {
        Order order = await SeedOrderAsync();
        OrderItem item = NewItem(order, ProductType.Physical);
        item.Quantity = 0;

        Assert.Equal((CheckViolation, "ck_order_items_quantity_above_zero_one_if_digital"), await ViolationOfAsync(c => c.OrderItems.Add(item)));
    }

    [Fact]
    public async Task DigitalItemWithTwoUnits_IsRefused()
    {
        Order order = await SeedOrderAsync();
        OrderItem item = NewItem(order, ProductType.Digital);
        item.Quantity = 2;

        Assert.Equal((CheckViolation, "ck_order_items_quantity_above_zero_one_if_digital"), await ViolationOfAsync(c => c.OrderItems.Add(item)));
    }

    [Fact]
    public async Task DownloadCountAboveTheAllowance_IsRefused()
    {
        Order order = await SeedOrderAsync();
        OrderItem item = NewItem(order, ProductType.Digital);

        Assert.Equal(
            (CheckViolation, "ck_order_items_download_count_within_allowance"),
            await ViolationOfAsync(c =>
            {
                c.OrderItems.Add(item);
                c.Entry(item).Property(i => i.DownloadCount).CurrentValue = 4;
            }));
    }

    [Fact]
    public async Task DownloadCountOnAPhysicalItem_IsRefused()
    {
        Order order = await SeedOrderAsync();
        OrderItem item = NewItem(order, ProductType.Physical);

        Assert.Equal(
            (CheckViolation, "ck_order_items_download_count_within_allowance"),
            await ViolationOfAsync(c =>
            {
                c.OrderItems.Add(item);
                c.Entry(item).Property(i => i.DownloadCount).CurrentValue = 1;
            }));
    }

    [Fact]
    public async Task FieldsOfAnotherType_AreRefused()
    {
        Order order = await SeedOrderAsync();
        OrderItem physicalWithAllowance = NewItem(order, ProductType.Physical);
        physicalWithAllowance.DownloadAllowance = 3;
        OrderItem serviceWithoutPeriod = NewItem(order, ProductType.Service);
        serviceWithoutPeriod.FulfilmentPeriod = null;
        OrderItem digitalWithExemption = NewItem(order, ProductType.Digital);
        digitalWithExemption.WithdrawalExemptionReason = WithdrawalExemptionReason.SealedMedia;
        OrderItem serviceWithFile = NewItem(order, ProductType.Service);
        DigitalFile file = await SeedDigitalFileAsync();
        serviceWithFile.DigitalFileId = file.Id;

        Assert.Equal((CheckViolation, "ck_order_items_download_allowance_only_digital"), await ViolationOfAsync(c => c.OrderItems.Add(physicalWithAllowance)));
        Assert.Equal((CheckViolation, "ck_order_items_fulfilment_period_only_service"), await ViolationOfAsync(c => c.OrderItems.Add(serviceWithoutPeriod)));
        Assert.Equal((CheckViolation, "ck_order_items_withdrawal_exemption_only_physical"), await ViolationOfAsync(c => c.OrderItems.Add(digitalWithExemption)));
        Assert.Equal((CheckViolation, "ck_order_items_digital_file_only_digital"), await ViolationOfAsync(c => c.OrderItems.Add(serviceWithFile)));
    }

    [Fact]
    public async Task PartialDiscountAndDiscountedPriceAboveTheUnitPrice_AreRefused()
    {
        Order order = await SeedOrderAsync();
        OrderItem partial = NewItem(order, ProductType.Physical);
        partial.DiscountPercentage = 10m;
        OrderItem above = NewItem(order, ProductType.Physical);
        above.DiscountPercentage = 10m;
        above.DiscountedUnitPrice = above.UnitPrice;
        above.ReferencePrice = 120m;

        Assert.Equal((CheckViolation, "ck_order_items_discount_fields_together"), await ViolationOfAsync(c => c.OrderItems.Add(partial)));
        Assert.Equal((CheckViolation, "ck_order_items_discounted_unit_price_range"), await ViolationOfAsync(c => c.OrderItems.Add(above)));
    }

    [Fact]
    public async Task ItemsOfTheThreeTypes_AreAccepted_WithTheDefaultsOfTheFieldTable()
    {
        Order order = await SeedOrderAsync();
        OrderItem physical = NewItem(order, ProductType.Physical);
        physical.WithdrawalExemptionReason = WithdrawalExemptionReason.MadeToOrder;
        physical.FirstOptionValue = "Kırmızı";
        physical.DeliveryMarkedAt = Now;
        physical.DeliveryDate = new DateOnly(2026, 11, 4);

        await SaveAsync(c => c.OrderItems.AddRange(physical, NewItem(order, ProductType.Digital), NewItem(order, ProductType.Service)));

        Assert.Equal(3L, await ScalarAsync<long>("SELECT count(*) FROM order_items WHERE coupon_share = 0 AND download_count = 0"));
    }

    // ---- KK6: the transition record ----

    [Fact]
    public async Task RecordWithTwoReasons_IsRefused()
    {
        Order order = await SeedOrderAsync();
        Admin admin = await SeedAdminAsync();
        TransitionRecord record = TransitionRecord.Create(
            order.Id, TransitionCode.S3, ActorKind.Admin, admin.Id, admin.Name, TransitionReason.Closure, CancellationReason.OutOfStock, null, null, null, Now);

        Assert.Equal((CheckViolation, "ck_transition_records_at_most_one_reason"), await ViolationOfAsync(c => c.TransitionRecords.Add(record)));
    }

    [Fact]
    public async Task CorrectionReasonOnANonCorrectionRow_IsRefused()
    {
        Order order = await SeedOrderAsync();
        Admin admin = await SeedAdminAsync();
        TransitionRecord record = TransitionRecord.Create(
            order.Id, TransitionCode.S2, ActorKind.Admin, admin.Id, admin.Name, null, null, CorrectionReason.WrongOrder, null, null, Now);

        Assert.Equal((CheckViolation, "ck_transition_records_correction_fields_only_on_correction"), await ViolationOfAsync(c => c.TransitionRecords.Add(record)));
    }

    [Fact]
    public async Task CorrectionRowWithoutItsCorrectedRow_IsRefused_AndOneByTheBuyer_IsRefused()
    {
        Order order = await SeedOrderAsync();
        Admin admin = await SeedAdminAsync();
        TransitionRecord first = TransitionRecord.Create(order.Id, TransitionCode.S1, ActorKind.System, null, null, null, null, null, null, null, Now);
        await SaveAsync(c => c.TransitionRecords.Add(first));
        TransitionRecord withoutTarget = TransitionRecord.Create(
            order.Id, TransitionCode.Correction, ActorKind.Admin, admin.Id, admin.Name, null, null, CorrectionReason.WrongOrder, null, null, Now);
        TransitionRecord byBuyer = TransitionRecord.Create(
            order.Id, TransitionCode.Correction, ActorKind.Buyer, null, null, null, null, CorrectionReason.WrongOrder, null, first.Id, Now);

        Assert.Equal((CheckViolation, "ck_transition_records_correction_fields_only_on_correction"), await ViolationOfAsync(c => c.TransitionRecords.Add(withoutTarget)));
        Assert.Equal((CheckViolation, "ck_transition_records_correction_fields_only_on_correction"), await ViolationOfAsync(c => c.TransitionRecords.Add(byBuyer)));
    }

    [Fact]
    public async Task CorrectionRowByTheAdministrator_IsAccepted()
    {
        Order order = await SeedOrderAsync();
        Admin admin = await SeedAdminAsync();
        TransitionRecord first = TransitionRecord.Create(order.Id, TransitionCode.S1, ActorKind.System, null, null, null, null, null, null, null, Now);
        await SaveAsync(c => c.TransitionRecords.Add(first));

        await SaveAsync(c => c.TransitionRecords.Add(TransitionRecord.Create(
            order.Id, TransitionCode.Correction, ActorKind.Admin, admin.Id, admin.Name, null, null, CorrectionReason.Other, "not", first.Id, Now.AddMinutes(1))));

        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM transition_records WHERE corrected_transition_record_id = '{first.Id}'"));
    }

    [Fact]
    public async Task ReasonsOutsideTheirCodesAndActors_AreRefused()
    {
        Order order = await SeedOrderAsync();
        Admin admin = await SeedAdminAsync();
        TransitionRecord reasonOnS2 = TransitionRecord.Create(
            order.Id, TransitionCode.S2, ActorKind.Admin, admin.Id, admin.Name, null, CancellationReason.OutOfStock, null, null, null, Now);
        TransitionRecord autoByAdmin = TransitionRecord.Create(
            order.Id, TransitionCode.S3, ActorKind.Admin, admin.Id, admin.Name, TransitionReason.PaymentWindowExpired, null, null, null, null, Now);
        TransitionRecord closureOnO2 = TransitionRecord.Create(
            order.Id, TransitionCode.O2, ActorKind.System, null, null, TransitionReason.Closure, null, null, null, null, Now);
        TransitionRecord cancellationBySystem = TransitionRecord.Create(
            order.Id, TransitionCode.S4, ActorKind.System, null, null, null, CancellationReason.OutOfStock, null, null, null, Now);

        Assert.Equal((CheckViolation, "ck_transition_records_reason_only_on_cancellation_codes"), await ViolationOfAsync(c => c.TransitionRecords.Add(reasonOnS2)));
        Assert.Equal((CheckViolation, "ck_transition_records_auto_cancellation_only_system_s3_o2"), await ViolationOfAsync(c => c.TransitionRecords.Add(autoByAdmin)));
        Assert.Equal((CheckViolation, "ck_transition_records_closure_only_s3_s4_s9"), await ViolationOfAsync(c => c.TransitionRecords.Add(closureOnO2)));
        Assert.Equal((CheckViolation, "ck_transition_records_cancellation_reason_by_admin"), await ViolationOfAsync(c => c.TransitionRecords.Add(cancellationBySystem)));
    }

    [Fact]
    public void TransitionRecord_HasNoPublicSetter()
    {
        PropertyInfo[] writable = [.. typeof(TransitionRecord).GetProperties().Where(p => p.SetMethod is { IsPublic: true })];

        Assert.Empty(writable);
    }

    // ---- KK7: the note, the acceptances and the order's deletion ----

    [Fact]
    public async Task SecondInternalNoteOfTheOrder_IsRefused()
    {
        Order order = await SeedOrderAsync();
        Admin admin = await SeedAdminAsync();
        await SaveAsync(c => c.InternalNotes.Add(InternalNote.Create(order.Id, "bir", admin.Id, admin.Name, Now)));

        Assert.Equal(
            (UniqueViolation, "ix_internal_notes_order_id"),
            await ViolationOfAsync(c => c.InternalNotes.Add(InternalNote.Create(order.Id, "iki", admin.Id, admin.Name, Now))));
    }

    [Fact]
    public async Task SecondAcceptanceOfTheSameType_IsRefused_ButTheOtherTypeIsAccepted()
    {
        Order order = await SeedOrderAsync();
        await SaveAsync(c => c.CheckboxAcceptances.Add(CheckboxAcceptance.Create(order.Id, CheckboxType.Digital, "metin", Now)));

        Assert.Equal(
            (UniqueViolation, "ix_checkbox_acceptances_order_id_checkbox_type"),
            await ViolationOfAsync(c => c.CheckboxAcceptances.Add(CheckboxAcceptance.Create(order.Id, CheckboxType.Digital, "metin", Now))));

        await SaveAsync(c => c.CheckboxAcceptances.Add(CheckboxAcceptance.Create(order.Id, CheckboxType.Service, "metin", Now)));
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM checkbox_acceptances"));
    }

    [Fact]
    public async Task SecondLinkOfTheSameAcceptanceAndItem_IsRefused()
    {
        Order order = await SeedOrderAsync();
        OrderItem item = NewItem(order, ProductType.Digital);
        CheckboxAcceptance acceptance = CheckboxAcceptance.Create(order.Id, CheckboxType.Digital, "metin", Now);
        await SaveAsync(c =>
        {
            c.OrderItems.Add(item);
            c.CheckboxAcceptances.Add(acceptance);
            c.CheckboxAcceptanceOrderItems.Add(CheckboxAcceptanceOrderItem.Create(acceptance.Id, item.Id, Now));
        });

        Assert.Equal(
            (UniqueViolation, "ix_checkbox_acceptance_order_items_checkbox_acceptance_id_orde"),
            await ViolationOfAsync(c => c.CheckboxAcceptanceOrderItems.Add(CheckboxAcceptanceOrderItem.Create(acceptance.Id, item.Id, Now))));
    }

    [Fact]
    public async Task DeletingAnOrderWithAnItem_IsRefusedByTheDatabase()
    {
        Order order = await SeedOrderAsync();
        await SaveAsync(c => c.OrderItems.Add(NewItem(order, ProductType.Physical)));

        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM orders WHERE id = '{order.Id}'"));

        Assert.Equal((ForeignKeyViolation, "fk_order_items_orders_order_id"), (error.SqlState, error.ConstraintName));
    }

    [Theory]
    [InlineData("transition_records")]
    [InlineData("internal_notes")]
    [InlineData("checkbox_acceptances")]
    public async Task DeletingAnOrderWithItsRecords_IsRefusedByTheDatabase(string table)
    {
        Order order = await SeedOrderAsync();
        Admin admin = await SeedAdminAsync();
        await SaveAsync(c =>
        {
            switch (table)
            {
                case "transition_records":
                    c.TransitionRecords.Add(TransitionRecord.Create(order.Id, TransitionCode.S1, ActorKind.System, null, null, null, null, null, null, null, Now));
                    break;
                case "internal_notes":
                    c.InternalNotes.Add(InternalNote.Create(order.Id, "not", admin.Id, admin.Name, Now));
                    break;
                default:
                    c.CheckboxAcceptances.Add(CheckboxAcceptance.Create(order.Id, CheckboxType.Service, "metin", Now));
                    break;
            }
        });

        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM orders WHERE id = '{order.Id}'"));

        Assert.Equal((ForeignKeyViolation, $"fk_{table}_orders_order_id"), (error.SqlState, error.ConstraintName));
    }

    [Fact]
    public async Task DeletingTheCustomer_TakesTheCartAndItsItems_AndEmptiesTheOrdersAccountLink()
    {
        References refs = await SeedReferencesAsync();
        Customer customer = await SeedCustomerAsync();
        Variant variant = await SeedVariantAsync();
        Cart memberCart = Cart.ForCustomer(customer.Id, Now);
        Cart guestCart = Cart.ForGuest(Digest("g1"), Now);
        Order order = NewOrder(refs);
        order.CustomerId = customer.Id;
        await SaveAsync(c =>
        {
            c.Carts.AddRange(memberCart, guestCart);
            c.CartItems.Add(CartItem.Create(memberCart.Id, variant.Id, 1, 100m, Now));
            c.CartItems.Add(CartItem.Create(guestCart.Id, variant.Id, 1, 100m, Now));
            c.Orders.Add(order);
        });

        await ExecuteAsync($"DELETE FROM customers WHERE id = '{customer.Id}'");

        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM carts WHERE id = '{memberCart.Id}'"));
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM cart_items WHERE cart_id = '{memberCart.Id}'"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM cart_items"));
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM orders WHERE id = '{order.Id}' AND customer_id IS NULL"));
    }

    [Fact]
    public async Task DeletingTheVariant_KeepsTheOrderItem_AndEmptiesItsLink()
    {
        Order order = await SeedOrderAsync();
        Variant variant = await SeedVariantAsync();
        OrderItem item = NewItem(order, ProductType.Physical);
        item.ProductId = variant.ProductId;
        item.VariantId = variant.Id;
        await SaveAsync(c => c.OrderItems.Add(item));

        await ExecuteAsync($"DELETE FROM variants WHERE id = '{variant.Id}'");

        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM order_items WHERE id = '{item.Id}' AND variant_id IS NULL AND product_id IS NOT NULL"));

        await ExecuteAsync($"DELETE FROM products WHERE id = '{variant.ProductId}'");

        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM order_items WHERE id = '{item.Id}' AND product_id IS NULL"));
    }

    [Fact]
    public async Task DeletingTheVariant_IsStoppedByAnUnmarkedCartItem_AndEmptiesAMarkedOne()
    {
        Cart cart = await SeedGuestCartAsync();
        Variant variant = await SeedVariantAsync();
        CartItem item = CartItem.Create(cart.Id, variant.Id, 1, 100m, Now);
        await SaveAsync(c => c.CartItems.Add(item));

        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM variants WHERE id = '{variant.Id}'"));
        Assert.Equal((CheckViolation, "ck_cart_items_removed_name_without_variant"), (error.SqlState, error.ConstraintName));

        await ExecuteAsync($"UPDATE cart_items SET removed_product_name = 'Kapı' WHERE id = '{item.Id}'");
        await ExecuteAsync($"DELETE FROM variants WHERE id = '{variant.Id}'");

        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM cart_items WHERE id = '{item.Id}' AND variant_id IS NULL"));
    }

    [Fact]
    public async Task DeletingTheAdmin_EmptiesTheLinkOfTheTransitionRecordAndTheNote_AndKeepsTheNames()
    {
        Order order = await SeedOrderAsync();
        Admin admin = await SeedAdminAsync();
        await SaveAsync(c =>
        {
            c.TransitionRecords.Add(TransitionRecord.Create(order.Id, TransitionCode.S2, ActorKind.Admin, admin.Id, admin.Name, null, null, null, null, null, Now));
            c.InternalNotes.Add(InternalNote.Create(order.Id, "not", admin.Id, admin.Name, Now));
        });

        await ExecuteAsync($"DELETE FROM admins WHERE id = '{admin.Id}'");

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM transition_records WHERE admin_id IS NULL AND admin_name = 'Veli'"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM internal_notes WHERE admin_id IS NULL AND admin_name = 'Veli'"));
    }

    [Fact]
    public async Task DeletingACouponLinkedToAnOrder_IsRefusedByTheDatabase()
    {
        // D-54 (T109's share): the order's coupon link is restrictive (06 4.13, 4.17).
        References refs = await SeedReferencesAsync();
        Coupon coupon = await SeedCouponAsync();
        Order order = NewOrder(refs);
        order.CouponId = coupon.Id;
        order.CouponCode = coupon.Code;
        await SaveAsync(c => c.Orders.Add(order));

        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM coupons WHERE id = '{coupon.Id}'"));

        Assert.Equal((ForeignKeyViolation, "fk_orders_coupons_coupon_id"), (error.SqlState, error.ConstraintName));
    }

    [Fact]
    public async Task DeletingALegalTextVersionOrAProvinceOfAnOrder_IsRefusedByTheDatabase()
    {
        References refs = await SeedReferencesAsync();
        await SaveAsync(c => c.Orders.Add(NewOrder(refs)));

        PostgresException version = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM legal_text_versions WHERE id = '{refs.PrivacyNoticeId}'"));
        PostgresException province = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM provinces WHERE id = '{refs.ProvinceId}'"));

        Assert.Equal((ForeignKeyViolation, "fk_orders_legal_text_versions_privacy_notice_version_id"), (version.SqlState, version.ConstraintName));
        Assert.Equal(ForeignKeyViolation, province.SqlState);
        Assert.StartsWith("fk_orders_provinces_", province.ConstraintName, StringComparison.Ordinal);
    }

    // ---- KK8 (guard): the permanent rows and the appended record ----

    [Fact]
    public async Task DeletingThePermanentRowsThroughTheContext_IsRefused_AndTheRowsStay()
    {
        Order order = await SeedOrderAsync();
        OrderItem item = NewItem(order, ProductType.Digital);
        TransitionRecord record = TransitionRecord.Create(order.Id, TransitionCode.S1, ActorKind.System, null, null, null, null, null, null, null, Now);
        CheckboxAcceptance acceptance = CheckboxAcceptance.Create(order.Id, CheckboxType.Digital, "metin", Now);
        CheckboxAcceptanceOrderItem link = CheckboxAcceptanceOrderItem.Create(acceptance.Id, item.Id, Now);
        await SaveAsync(c =>
        {
            c.OrderItems.Add(item);
            c.TransitionRecords.Add(record);
            c.CheckboxAcceptances.Add(acceptance);
            c.CheckboxAcceptanceOrderItems.Add(link);
        });

        await AssertDeletionRefusedAsync(c => c.Orders.SingleAsync(o => o.Id == order.Id, Ct));
        await AssertDeletionRefusedAsync(c => c.OrderItems.SingleAsync(i => i.Id == item.Id, Ct));
        await AssertDeletionRefusedAsync(c => c.TransitionRecords.SingleAsync(t => t.Id == record.Id, Ct));
        await AssertDeletionRefusedAsync(c => c.CheckboxAcceptances.SingleAsync(a => a.Id == acceptance.Id, Ct));
        await AssertDeletionRefusedAsync(c => c.CheckboxAcceptanceOrderItems.SingleAsync(l => l.Id == link.Id, Ct));

        Assert.Equal(
            "1 1 1 1 1",
            await ScalarAsync<string>(
                "SELECT (SELECT count(*) FROM orders) || ' ' || (SELECT count(*) FROM order_items) || ' ' || (SELECT count(*) FROM transition_records) "
                + "|| ' ' || (SELECT count(*) FROM checkbox_acceptances) || ' ' || (SELECT count(*) FROM checkbox_acceptance_order_items)"));
    }

    [Fact]
    public async Task ChangingATransitionRecordThroughTheContext_IsRefused_AndTheRowStays()
    {
        Order order = await SeedOrderAsync();
        TransitionRecord record = TransitionRecord.Create(order.Id, TransitionCode.S1, ActorKind.System, null, null, null, null, null, null, null, Now);
        await SaveAsync(c => c.TransitionRecords.Add(record));

        await using (AppDbContext context = NewContext())
        {
            TransitionRecord loaded = await context.TransitionRecords.SingleAsync(t => t.Id == record.Id, Ct);
            context.Entry(loaded).Property(t => t.TransitionCode).CurrentValue = TransitionCode.S2;
            InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));
            Assert.Contains("only appended", error.Message, StringComparison.Ordinal);
        }

        Assert.Equal("S1", await ScalarAsync<string>($"SELECT transition_code FROM transition_records WHERE id = '{record.Id}'"));
    }

    [Fact]
    public async Task ErasingTheAdministratorsDataOfATransitionRecord_IsAccepted_ButFillingANoteIsRefused()
    {
        Order order = await SeedOrderAsync();
        Admin admin = await SeedAdminAsync();
        TransitionRecord record = TransitionRecord.Create(
            order.Id, TransitionCode.S4, ActorKind.Admin, admin.Id, admin.Name, null, CancellationReason.Other, null, "açıklama", null, Now);
        TransitionRecord other = TransitionRecord.Create(order.Id, TransitionCode.S2, ActorKind.Admin, admin.Id, admin.Name, null, null, null, null, null, Now);
        await SaveAsync(c => c.TransitionRecords.AddRange(record, other));

        await using (AppDbContext context = NewContext())
        {
            TransitionRecord loaded = await context.TransitionRecords.SingleAsync(t => t.Id == record.Id, Ct);
            loaded.ErasePersonalData();
            await context.SaveChangesAsync(Ct);
        }

        await using (AppDbContext context = NewContext())
        {
            TransitionRecord loaded = await context.TransitionRecords.SingleAsync(t => t.Id == other.Id, Ct);
            context.Entry(loaded).Property(t => t.ReasonNote).CurrentValue = "yeni";
            await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));
        }

        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM transition_records WHERE id = '{record.Id}' AND num_nonnulls(admin_id, admin_name, reason_note) = 0"));
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM transition_records WHERE id = '{other.Id}' AND reason_note IS NULL"));
    }

    [Fact]
    public async Task CartCartItemAndInternalNote_CanBeDeletedThroughTheContext()
    {
        Cart cart = await SeedGuestCartAsync();
        Variant variant = await SeedVariantAsync();
        CartItem item = CartItem.Create(cart.Id, variant.Id, 1, 100m, Now);
        Order order = await SeedOrderAsync();
        Admin admin = await SeedAdminAsync();
        InternalNote note = InternalNote.Create(order.Id, "not", admin.Id, admin.Name, Now);
        await SaveAsync(c =>
        {
            c.CartItems.Add(item);
            c.InternalNotes.Add(note);
        });

        await using (AppDbContext context = NewContext())
        {
            context.CartItems.Remove(await context.CartItems.SingleAsync(i => i.Id == item.Id, Ct));
            context.Carts.Remove(await context.Carts.SingleAsync(c => c.Id == cart.Id, Ct));
            context.InternalNotes.Remove(await context.InternalNotes.SingleAsync(n => n.Id == note.Id, Ct));
            await context.SaveChangesAsync(Ct);
        }

        Assert.Equal(0L, await ScalarAsync<long>("SELECT (SELECT count(*) FROM carts) + (SELECT count(*) FROM cart_items) + (SELECT count(*) FROM internal_notes)"));
    }

    // ---- helpers ----

    /// <summary>A placeholder digest; the tests never hold a real key or a real digest of one.</summary>
    private static string Digest(string tag) => $"digest-placeholder-{tag}";

    /// <summary>Empties the fields the disposal empties (06 4.17; 5.6.10).</summary>
    private static void EmptyDisposalFields(Order order)
    {
        order.SetContactEmail(null);
        order.OrderAccessKey = null;
        order.IpAddress = null;
        order.CartId = null;
        order.CustomerId = null;
        order.BillingRecipientName = null;
        order.BillingProvinceId = null;
        order.BillingDistrict = null;
        order.BillingAddressLine = null;
    }

    private static OrderItem NewItem(Order order, ProductType productType)
    {
        OrderItem item = OrderItem.Create(order.Id, null, null, productType, "Ürün", 1, 100m, 20m, 16.67m, Now);
        if (productType == ProductType.Digital)
        {
            item.DownloadAllowance = 3;
        }
        else if (productType == ProductType.Service)
        {
            item.FulfilmentPeriod = 5;
        }

        return item;
    }

    private Order NewOrder(References refs, PaymentMethod paymentMethod = PaymentMethod.Card)
    {
        int n = Interlocked.Increment(ref _orderCounter);
        Order order = Order.Create(
            $"2026-{n:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", paymentMethod, true, "Ali@Example.com", 120m, 0m, Now.AddHours(1), Now);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.BillingRecipientName = "Ali Veli";
        order.BillingProvinceId = refs.ProvinceId;
        order.BillingDistrict = "Çankaya";
        order.BillingAddressLine = "Atatürk Bulvarı 1";
        order.BankTransferIban = paymentMethod == PaymentMethod.BankTransfer ? "TR000000000000000000000000" : null;
        order.CompanyType = CompanyType.Tradesman;
        order.CompanyLegalName = "Örnek Ticaret";
        order.CompanyTaxIdNumber = "1234567890";
        order.CompanyProvinceId = refs.ProvinceId;
        order.CompanyDistrict = "Çankaya";
        order.CompanyAddressLine = "Kızılay 2";
        order.CompanyPhone = "3121234567";
        order.CompanyEmail = "firma@example.com";
        order.CompanyKepAddress = "firma@hs01.kep.tr";
        order.PreInformationFormVersionId = refs.PreInformationFormId;
        order.DistanceSalesContractVersionId = refs.DistanceSalesContractId;
        order.PrivacyNoticeVersionId = refs.PrivacyNoticeId;
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
        return order;
    }

    private async Task<Order> SeedOrderAsync()
    {
        References refs = await SeedReferencesAsync();
        Order order = NewOrder(refs);
        await SaveAsync(c => c.Orders.Add(order));
        return order;
    }

    private async Task<References> SeedReferencesAsync()
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

    private async Task<Customer> SeedCustomerAsync()
    {
        Customer customer = Customer.Create("Ali", "ali@example.com", "digest-placeholder", Now, null, Now);
        await SaveAsync(c => c.Customers.Add(customer));
        return customer;
    }

    private async Task<Admin> SeedAdminAsync()
    {
        Admin admin = Admin.Create("Veli", "veli@example.com", "digest-placeholder", Now);
        await SaveAsync(c => c.Admins.Add(admin));
        return admin;
    }

    private async Task<Cart> SeedGuestCartAsync()
    {
        Cart cart = Cart.ForGuest(Digest("guest"), Now);
        await SaveAsync(c => c.Carts.Add(cart));
        return cart;
    }

    private async Task<Variant> SeedVariantAsync()
    {
        Product product = Product.Create("Kapı", "kapi", ProductType.Physical, 20m, Now);
        Variant variant = Variant.Create(product.Id, "KAPI-1", 100m, Now);
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Variants.Add(variant);
        });
        return variant;
    }

    private async Task<Coupon> SeedCouponAsync()
    {
        Coupon coupon = Coupon.CreatePercentage("YAZ10", 10m, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 5, Now);
        await SaveAsync(c => c.Coupons.Add(coupon));
        return coupon;
    }

    private async Task<DigitalFile> SeedDigitalFileAsync()
    {
        DigitalFile file = DigitalFile.Create(EntityIds.NewStorageKey(), "kitap.pdf", 1024, Now);
        await SaveAsync(c => c.DigitalFiles.Add(file));
        return file;
    }

    private async Task AssertDeletionRefusedAsync<T>(Func<AppDbContext, Task<T>> load)
        where T : class
    {
        await using AppDbContext context = NewContext();
        T entity = await load(context);
        context.Remove(entity);
        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));
        Assert.Contains("never deleted", error.Message, StringComparison.Ordinal);
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
