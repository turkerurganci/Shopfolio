using System.Globalization;
using System.Net;
using System.Reflection;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Migrations;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the reservation and history tests (T110): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(StockReservationAndHistoryTablesDefinition.Name)]
public sealed class StockReservationAndHistoryTablesDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "StockReservationAndHistoryTables";
}

/// <summary>
/// The stock reservation and the two correction histories on a real PostgreSQL (T110 criteria 1-7; 06 4.14, 4.61, 4.62, 7.1, 7.2, 7.3, 7.5;
/// 09 10.6): the migration made the three tables; the database itself refuses what the rules refuse; a variant's deletion takes its reservations;
/// the order, coupon and province links are restrictive; an administrator's removal empties the history's link and keeps the row; and the
/// context refuses every change of a history row but the emptying of the administrator link.
/// </summary>
[Collection(StockReservationAndHistoryTablesDefinition.Name)]
public sealed class StockReservationAndHistoryTablesTests : IAsyncLifetime
{
    private const string CheckViolation = PostgresErrorCodes.CheckViolation;
    private const string UniqueViolation = PostgresErrorCodes.UniqueViolation;
    private const string ForeignKeyViolation = PostgresErrorCodes.ForeignKeyViolation;

    private static readonly string[] ThreeTables = ["order_address_histories", "order_email_histories", "stock_reservations"];

    private static readonly DateTimeOffset Now = new(2026, 11, 4, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    private int _orderCounter;

    public StockReservationAndHistoryTablesTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE order_address_histories, order_email_histories, stock_reservations, transition_records, order_items, orders, "
            + "admins, coupons, variants, products, legal_text_versions, provinces CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the migration ----

    [Fact]
    public async Task Migration_MakesTheThreeTables()
    {
        List<string> tables = await QueryAsync(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name = ANY(@names)",
            ("names", ThreeTables));

        Assert.Equal(ThreeTables, tables.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Columns_HaveTheTypesAndNullabilityOfTheFieldTables()
    {
        // 06 4.14, 4.61, 4.62 and 2.5.1 (the common Id and CreatedAt included; phone ten characters).
        string[] expected =
        [
            "order_address_histories.address_type text not null",
            "order_address_histories.admin_id uuid",
            "order_address_histories.admin_name text not null",
            "order_address_histories.created_at timestamp with time zone not null",
            "order_address_histories.id uuid not null",
            "order_address_histories.new_address_line text not null",
            "order_address_histories.new_district text not null",
            "order_address_histories.new_phone character varying(10)",
            "order_address_histories.new_province_id uuid not null",
            "order_address_histories.new_recipient_name text not null",
            "order_address_histories.old_address_line text not null",
            "order_address_histories.old_district text not null",
            "order_address_histories.old_phone character varying(10)",
            "order_address_histories.old_province_id uuid not null",
            "order_address_histories.old_recipient_name text not null",
            "order_address_histories.order_id uuid not null",
            "order_email_histories.admin_id uuid",
            "order_email_histories.admin_name text not null",
            "order_email_histories.created_at timestamp with time zone not null",
            "order_email_histories.id uuid not null",
            "order_email_histories.new_email text not null",
            "order_email_histories.old_email text not null",
            "order_email_histories.order_id uuid not null",
            "stock_reservations.coupon_id uuid",
            "stock_reservations.created_at timestamp with time zone not null",
            "stock_reservations.id uuid not null",
            "stock_reservations.order_id uuid not null",
            "stock_reservations.quantity integer not null",
            "stock_reservations.variant_id uuid",
        ];

        List<string> columns = await QueryAsync(
            "SELECT table_name || '.' || column_name || ' ' || data_type"
            + " || CASE WHEN character_maximum_length IS NOT NULL THEN '(' || character_maximum_length || ')' ELSE '' END"
            + " || CASE WHEN is_nullable = 'NO' THEN ' not null' ELSE '' END"
            + " FROM information_schema.columns WHERE table_schema = 'public' AND table_name = ANY(@names)",
            ("names", ThreeTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), columns.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ColumnDefaults_AreTheDefaultColumnOfTheFieldTables()
    {
        // 06 4.14, 4.61, 4.62: no "Default" cell is filled.
        List<string> defaults = await QueryAsync(
            "SELECT table_name || '.' || column_name FROM information_schema.columns WHERE table_schema = 'public' AND table_name = ANY(@names) AND column_default IS NOT NULL",
            ("names", ThreeTables));

        Assert.Empty(defaults);
    }

    [Fact]
    public async Task Indexes_AreTheDocumentedOnes()
    {
        // 06 7.1 (the filtered unique ones), 7.5 (the composite reads) and the foreign key indexes of 7.2.
        string[] expected =
        [
            "order_address_histories.ix_order_address_histories_admin_id | CREATE INDEX ix_order_address_histories_admin_id ON public.order_address_histories USING btree (admin_id)",
            "order_address_histories.ix_order_address_histories_new_province_id | CREATE INDEX ix_order_address_histories_new_province_id ON public.order_address_histories USING btree (new_province_id)",
            "order_address_histories.ix_order_address_histories_old_province_id | CREATE INDEX ix_order_address_histories_old_province_id ON public.order_address_histories USING btree (old_province_id)",
            "order_address_histories.ix_order_address_histories_order_id_address_type_created_at | CREATE INDEX ix_order_address_histories_order_id_address_type_created_at ON public.order_address_histories USING btree (order_id, address_type, created_at)",
            "order_address_histories.pk_order_address_histories | CREATE UNIQUE INDEX pk_order_address_histories ON public.order_address_histories USING btree (id)",
            "order_email_histories.ix_order_email_histories_admin_id | CREATE INDEX ix_order_email_histories_admin_id ON public.order_email_histories USING btree (admin_id)",
            "order_email_histories.ix_order_email_histories_order_id_created_at | CREATE INDEX ix_order_email_histories_order_id_created_at ON public.order_email_histories USING btree (order_id, created_at)",
            "order_email_histories.pk_order_email_histories | CREATE UNIQUE INDEX pk_order_email_histories ON public.order_email_histories USING btree (id)",
            "stock_reservations.ix_stock_reservations_coupon_id | CREATE INDEX ix_stock_reservations_coupon_id ON public.stock_reservations USING btree (coupon_id)",
            "stock_reservations.ix_stock_reservations_order_id | CREATE UNIQUE INDEX ix_stock_reservations_order_id ON public.stock_reservations USING btree (order_id) WHERE (coupon_id IS NOT NULL)",
            "stock_reservations.ix_stock_reservations_order_id_variant_id | CREATE UNIQUE INDEX ix_stock_reservations_order_id_variant_id ON public.stock_reservations USING btree (order_id, variant_id) WHERE (variant_id IS NOT NULL)",
            "stock_reservations.ix_stock_reservations_variant_id | CREATE INDEX ix_stock_reservations_variant_id ON public.stock_reservations USING btree (variant_id)",
            "stock_reservations.pk_stock_reservations | CREATE UNIQUE INDEX pk_stock_reservations ON public.stock_reservations USING btree (id)",
        ];

        List<string> indexes = await QueryAsync(
            "SELECT tablename || '.' || indexname || ' | ' || indexdef FROM pg_indexes WHERE schemaname = 'public' AND tablename = ANY(@names)",
            ("names", ThreeTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), indexes.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ForeignKeys_HaveTheDeleteBehaviorOfTheLinkTable()
    {
        // 06 7.2: the reservation's variant link cascades (c); the administrator links are emptied (n); every other link is restrictive (a).
        string[] expected =
        [
            "order_address_histories fk_order_address_histories_admins_admin_id n",
            "order_address_histories fk_order_address_histories_orders_order_id a",
            "order_address_histories fk_order_address_histories_provinces_new_province_id a",
            "order_address_histories fk_order_address_histories_provinces_old_province_id a",
            "order_email_histories fk_order_email_histories_admins_admin_id n",
            "order_email_histories fk_order_email_histories_orders_order_id a",
            "stock_reservations fk_stock_reservations_coupons_coupon_id a",
            "stock_reservations fk_stock_reservations_orders_order_id a",
            "stock_reservations fk_stock_reservations_variants_variant_id c",
        ];

        List<string> links = await QueryAsync(
            "SELECT conrelid::regclass::text || ' ' || conname || ' ' || confdeltype::text FROM pg_constraint WHERE contype = 'f' AND conrelid = ANY(ARRAY(SELECT x::regclass FROM unnest(@oids) x))",
            ("oids", ThreeTables.Select(t => "public." + t).ToArray()));

        Assert.Equal(expected.Order(StringComparer.Ordinal), links.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task CheckConstraints_AreTheDocumentedOnes()
    {
        // 06 7.3, the phone rule of 2.5.1 and the enum column's allowed values (06 2.9.2).
        string[] expected =
        [
            "order_address_histories.ck_order_address_histories_address_type",
            "order_address_histories.ck_order_address_histories_new_phone_ten_digits",
            "order_address_histories.ck_order_address_histories_old_phone_ten_digits",
            "order_address_histories.ck_order_address_histories_phones_by_address_type",
            "stock_reservations.ck_stock_reservations_exactly_one_of_variant_and_coupon",
            "stock_reservations.ck_stock_reservations_quantity_above_zero_one_if_coupon",
        ];

        List<string> checks = await QueryAsync(
            "SELECT conrelid::regclass::text || '.' || conname FROM pg_constraint WHERE contype = 'c' AND conrelid = ANY(ARRAY(SELECT x::regclass FROM unnest(@oids) x))",
            ("oids", ThreeTables.Select(t => "public." + t).ToArray()));

        Assert.Equal(expected.Order(StringComparer.Ordinal), checks.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Migration_HasNoDownMethod()
    {
        MethodInfo? down = typeof(CreateStockReservationAndHistoryTables)
            .GetMethod("Down", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);

        Assert.Null(down);
    }

    // ---- KK2, KK3: the reservation ----

    [Fact]
    public async Task ReservationWithNeitherAVariantNorACoupon_IsRefused_AndOneWithBoth_IsRefused()
    {
        Order order = await SeedOrderAsync();
        Variant variant = await SeedVariantAsync();
        Coupon coupon = await SeedCouponAsync();

        StockReservation neither = StockReservation.ForVariant(order.Id, variant.Id, 1, Now);
        neither.VariantId = null;
        StockReservation both = StockReservation.ForVariant(order.Id, variant.Id, 1, Now);
        both.CouponId = coupon.Id;

        Assert.Equal((CheckViolation, "ck_stock_reservations_exactly_one_of_variant_and_coupon"), await ViolationOfAsync(c => c.StockReservations.Add(neither)));
        Assert.Equal((CheckViolation, "ck_stock_reservations_exactly_one_of_variant_and_coupon"), await ViolationOfAsync(c => c.StockReservations.Add(both)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task VariantReservationWithoutAPositiveQuantity_IsRefused(int quantity)
    {
        Order order = await SeedOrderAsync();
        Variant variant = await SeedVariantAsync();

        Assert.Equal(
            (CheckViolation, "ck_stock_reservations_quantity_above_zero_one_if_coupon"),
            await ViolationOfAsync(c => c.StockReservations.Add(StockReservation.ForVariant(order.Id, variant.Id, quantity, Now))));
    }

    [Fact]
    public async Task CouponReservationOfTwo_IsRefused_AndVariantReservationOfTwo_IsAccepted()
    {
        Order order = await SeedOrderAsync();
        Variant variant = await SeedVariantAsync();
        Coupon coupon = await SeedCouponAsync();
        StockReservation couponRow = StockReservation.ForCoupon(order.Id, coupon.Id, Now);
        couponRow.Quantity = 2;

        Assert.Equal((CheckViolation, "ck_stock_reservations_quantity_above_zero_one_if_coupon"), await ViolationOfAsync(c => c.StockReservations.Add(couponRow)));

        await SaveAsync(c => c.StockReservations.Add(StockReservation.ForVariant(order.Id, variant.Id, 2, Now)));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM stock_reservations WHERE quantity = 2"));
    }

    [Fact]
    public async Task OrderWithAVariantRowAndACouponRow_IsAccepted()
    {
        Order order = await SeedOrderAsync();
        Variant variant = await SeedVariantAsync();
        Coupon coupon = await SeedCouponAsync();

        await SaveAsync(c => c.StockReservations.AddRange(StockReservation.ForVariant(order.Id, variant.Id, 3, Now), StockReservation.ForCoupon(order.Id, coupon.Id, Now)));

        Assert.Equal(2L, await ScalarAsync<long>($"SELECT count(*) FROM stock_reservations WHERE order_id = '{order.Id}'"));
    }

    [Fact]
    public async Task SecondReservationOfTheSameVariantInTheOrder_IsRefused_ButAnotherOrderOrVariantIsAccepted()
    {
        Order order = await SeedOrderAsync();
        Order other = await SeedOrderAsync();
        Variant variant = await SeedVariantAsync();
        Variant otherVariant = await SeedVariantAsync("KAPI-2");
        await SaveAsync(c => c.StockReservations.Add(StockReservation.ForVariant(order.Id, variant.Id, 1, Now)));

        Assert.Equal(
            (UniqueViolation, "ix_stock_reservations_order_id_variant_id"),
            await ViolationOfAsync(c => c.StockReservations.Add(StockReservation.ForVariant(order.Id, variant.Id, 2, Now))));

        await SaveAsync(c => c.StockReservations.AddRange(
            StockReservation.ForVariant(other.Id, variant.Id, 1, Now),
            StockReservation.ForVariant(order.Id, otherVariant.Id, 1, Now)));

        Assert.Equal(3L, await ScalarAsync<long>("SELECT count(*) FROM stock_reservations"));
    }

    [Fact]
    public async Task SecondCouponReservationInTheOrder_IsRefused_ButAnotherOrderIsAccepted()
    {
        Order order = await SeedOrderAsync();
        Order other = await SeedOrderAsync();
        Coupon coupon = await SeedCouponAsync();
        Coupon otherCoupon = Coupon.CreatePercentage("KIS10", 10m, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 5, Now);
        await SaveAsync(c => c.Coupons.Add(otherCoupon));
        await SaveAsync(c => c.StockReservations.Add(StockReservation.ForCoupon(order.Id, coupon.Id, Now)));

        Assert.Equal(
            (UniqueViolation, "ix_stock_reservations_order_id"),
            await ViolationOfAsync(c => c.StockReservations.Add(StockReservation.ForCoupon(order.Id, otherCoupon.Id, Now))));

        await SaveAsync(c => c.StockReservations.Add(StockReservation.ForCoupon(other.Id, coupon.Id, Now)));

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM stock_reservations"));
    }

    // ---- KK4: the reservation's links ----

    [Fact]
    public async Task DeletingTheVariant_TakesItsReservations_AndKeepsTheOthers()
    {
        Order order = await SeedOrderAsync();
        Coupon coupon = await SeedCouponAsync();
        Variant variant = await SeedVariantAsync();
        Variant kept = await SeedVariantAsync("KAPI-2");
        await SaveAsync(c => c.StockReservations.AddRange(
            StockReservation.ForVariant(order.Id, variant.Id, 1, Now),
            StockReservation.ForVariant(order.Id, kept.Id, 1, Now),
            StockReservation.ForCoupon(order.Id, coupon.Id, Now)));

        await ExecuteAsync($"DELETE FROM variants WHERE id = '{variant.Id}'");

        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM stock_reservations WHERE variant_id = '{variant.Id}'"));
        Assert.Equal(2L, await ScalarAsync<long>($"SELECT count(*) FROM stock_reservations WHERE order_id = '{order.Id}'"));
    }

    [Fact]
    public async Task DeletingACouponWithAReservation_IsRefusedByTheDatabase()
    {
        // D-54 (T110's share): the reservation's coupon link is restrictive (06 4.13, 4.14).
        Order order = await SeedOrderAsync();
        Coupon coupon = await SeedCouponAsync();
        await SaveAsync(c => c.StockReservations.Add(StockReservation.ForCoupon(order.Id, coupon.Id, Now)));

        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM coupons WHERE id = '{coupon.Id}'"));

        Assert.Equal((ForeignKeyViolation, "fk_stock_reservations_coupons_coupon_id"), (error.SqlState, error.ConstraintName));
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM coupons WHERE id = '{coupon.Id}'"));
    }

    [Fact]
    public async Task DeletingAnOrderWithARecord_IsRefusedByTheDatabase()
    {
        Order order = await SeedOrderAsync();
        Variant variant = await SeedVariantAsync();
        Admin admin = await SeedAdminAsync();
        Province province = await ScalarProvinceAsync();

        // Each table's row alone holds the order: the order's deletion names that table's link.
        foreach ((string table, string constraint) in new[]
        {
            ("stock_reservations", "fk_stock_reservations_orders_order_id"),
            ("order_email_histories", "fk_order_email_histories_orders_order_id"),
            ("order_address_histories", "fk_order_address_histories_orders_order_id"),
        })
        {
            await SeedRowAsync(table, order, variant, admin, province);

            PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM orders WHERE id = '{order.Id}'"));
            Assert.Equal((ForeignKeyViolation, constraint), (error.SqlState, error.ConstraintName));
            await ExecuteAsync($"DELETE FROM {table}");
        }
    }

    // ---- KK4, KK5: the histories ----

    [Fact]
    public async Task DeletingTheAdmin_KeepsTheHistoryRows_EmptiesTheirLinkAndKeepsTheName()
    {
        Order order = await SeedOrderAsync();
        Admin admin = await SeedAdminAsync();
        Province province = await ScalarProvinceAsync();
        await SaveAsync(c =>
        {
            c.OrderEmailHistories.Add(OrderEmailHistory.Create(order.Id, "eski@example.com", "yeni@example.com", admin.Id, admin.Name, Now));
            c.OrderAddressHistories.Add(NewAddressHistory(order.Id, AddressType.ShippingAddress, admin, province.Id, province.Id));
        });

        await ExecuteAsync($"DELETE FROM admins WHERE id = '{admin.Id}'");

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM order_email_histories WHERE admin_id IS NULL AND admin_name = 'Veli'"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM order_address_histories WHERE admin_id IS NULL AND admin_name = 'Veli'"));
    }

    [Fact]
    public async Task DeletingAProvinceOfTheAddressHistory_IsRefusedByTheDatabase_ForBothLinks()
    {
        Order order = await SeedOrderAsync();
        Admin admin = await SeedAdminAsync();
        // Two provinces the order itself does not hold, so only the history's two links are under test.
        Province oldProvince = Province.Create("35", "İzmir", Now);
        Province newProvince = Province.Create("34", "İstanbul", Now);
        await SaveAsync(c => c.Provinces.AddRange(oldProvince, newProvince));
        await SaveAsync(c => c.OrderAddressHistories.Add(NewAddressHistory(order.Id, AddressType.BillingAddress, admin, oldProvince.Id, newProvince.Id)));

        PostgresException newLink = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM provinces WHERE id = '{newProvince.Id}'"));
        PostgresException oldLink = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM provinces WHERE id = '{oldProvince.Id}'"));

        Assert.Equal((ForeignKeyViolation, "fk_order_address_histories_provinces_new_province_id"), (newLink.SqlState, newLink.ConstraintName));
        Assert.Equal((ForeignKeyViolation, "fk_order_address_histories_provinces_old_province_id"), (oldLink.SqlState, oldLink.ConstraintName));
    }

    [Fact]
    public async Task ShippingAddressRow_NeedsBothPhones_AndBillingAddressRow_NeedsNone()
    {
        Order order = await SeedOrderAsync();
        Admin admin = await SeedAdminAsync();
        Province province = await ScalarProvinceAsync();

        OrderAddressHistory shippingWithoutNewPhone = NewAddressHistory(order.Id, AddressType.ShippingAddress, admin, province.Id, province.Id);
        Set(shippingWithoutNewPhone, nameof(OrderAddressHistory.NewPhone), null);
        OrderAddressHistory shippingWithoutOldPhone = NewAddressHistory(order.Id, AddressType.ShippingAddress, admin, province.Id, province.Id);
        Set(shippingWithoutOldPhone, nameof(OrderAddressHistory.OldPhone), null);
        OrderAddressHistory billingWithOldPhone = NewAddressHistory(order.Id, AddressType.BillingAddress, admin, province.Id, province.Id);
        Set(billingWithOldPhone, nameof(OrderAddressHistory.OldPhone), "5321234567");
        OrderAddressHistory billingWithNewPhone = NewAddressHistory(order.Id, AddressType.BillingAddress, admin, province.Id, province.Id);
        Set(billingWithNewPhone, nameof(OrderAddressHistory.NewPhone), "5321234567");

        const string rule = "ck_order_address_histories_phones_by_address_type";
        Assert.Equal((CheckViolation, rule), await ViolationOfAsync(c => c.OrderAddressHistories.Add(shippingWithoutNewPhone)));
        Assert.Equal((CheckViolation, rule), await ViolationOfAsync(c => c.OrderAddressHistories.Add(shippingWithoutOldPhone)));
        Assert.Equal((CheckViolation, rule), await ViolationOfAsync(c => c.OrderAddressHistories.Add(billingWithOldPhone)));
        Assert.Equal((CheckViolation, rule), await ViolationOfAsync(c => c.OrderAddressHistories.Add(billingWithNewPhone)));

        await SaveAsync(c => c.OrderAddressHistories.AddRange(
            NewAddressHistory(order.Id, AddressType.ShippingAddress, admin, province.Id, province.Id),
            NewAddressHistory(order.Id, AddressType.BillingAddress, admin, province.Id, province.Id)));

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM order_address_histories"));
    }

    [Theory]
    [InlineData("532123456")]
    [InlineData("532123456a")]
    [InlineData("53212 3456")]
    public async Task ShippingAddressRow_WithAPhoneThatIsNotTenDigits_IsRefused(string phone)
    {
        Order order = await SeedOrderAsync();
        Admin admin = await SeedAdminAsync();
        Province province = await ScalarProvinceAsync();

        OrderAddressHistory oldBad = NewAddressHistory(order.Id, AddressType.ShippingAddress, admin, province.Id, province.Id);
        Set(oldBad, nameof(OrderAddressHistory.OldPhone), phone);
        OrderAddressHistory newBad = NewAddressHistory(order.Id, AddressType.ShippingAddress, admin, province.Id, province.Id);
        Set(newBad, nameof(OrderAddressHistory.NewPhone), phone);

        Assert.Equal((CheckViolation, "ck_order_address_histories_old_phone_ten_digits"), await ViolationOfAsync(c => c.OrderAddressHistories.Add(oldBad)));
        Assert.Equal((CheckViolation, "ck_order_address_histories_new_phone_ten_digits"), await ViolationOfAsync(c => c.OrderAddressHistories.Add(newBad)));
    }

    [Fact]
    public async Task AddressTypeOutsideTheEnum_IsRefused()
    {
        Order order = await SeedOrderAsync();
        Admin admin = await SeedAdminAsync();
        Province province = await ScalarProvinceAsync();
        await SaveAsync(c => c.OrderAddressHistories.Add(NewAddressHistory(order.Id, AddressType.BillingAddress, admin, province.Id, province.Id)));

        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync("UPDATE order_address_histories SET address_type = 'Other'"));

        Assert.Equal((CheckViolation, "ck_order_address_histories_address_type"), (error.SqlState, error.ConstraintName));
    }

    [Fact]
    public async Task SeveralCorrectionsOfTheSameOrderAndType_AreAccepted()
    {
        Order order = await SeedOrderAsync();
        Admin admin = await SeedAdminAsync();
        Province province = await ScalarProvinceAsync();

        await SaveAsync(c =>
        {
            c.OrderEmailHistories.AddRange(
                OrderEmailHistory.Create(order.Id, "a@example.com", "b@example.com", admin.Id, admin.Name, Now),
                OrderEmailHistory.Create(order.Id, "b@example.com", "c@example.com", admin.Id, admin.Name, Now.AddMinutes(1)));
            c.OrderAddressHistories.AddRange(
                NewAddressHistory(order.Id, AddressType.BillingAddress, admin, province.Id, province.Id),
                NewAddressHistory(order.Id, AddressType.BillingAddress, admin, province.Id, province.Id));
        });

        Assert.Equal(4L, await ScalarAsync<long>("SELECT (SELECT count(*) FROM order_email_histories) + (SELECT count(*) FROM order_address_histories)"));
    }

    // ---- KK6: the histories are only appended ----

    [Fact]
    public async Task ChangingAHistoryRowThroughTheContext_IsRefused_AndTheRowStays()
    {
        Order order = await SeedOrderAsync();
        Admin admin = await SeedAdminAsync();
        Province province = await ScalarProvinceAsync();
        OrderEmailHistory email = OrderEmailHistory.Create(order.Id, "eski@example.com", "yeni@example.com", admin.Id, admin.Name, Now);
        OrderAddressHistory address = NewAddressHistory(order.Id, AddressType.ShippingAddress, admin, province.Id, province.Id);
        await SaveAsync(c =>
        {
            c.OrderEmailHistories.Add(email);
            c.OrderAddressHistories.Add(address);
        });

        await using (AppDbContext context = NewContext())
        {
            OrderEmailHistory loaded = await context.OrderEmailHistories.SingleAsync(h => h.Id == email.Id, Ct);
            Set(loaded, nameof(OrderEmailHistory.NewEmail), "baska@example.com");
            InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));
            Assert.Contains("only appended", error.Message, StringComparison.Ordinal);
        }

        await using (AppDbContext context = NewContext())
        {
            OrderAddressHistory loaded = await context.OrderAddressHistories.SingleAsync(h => h.Id == address.Id, Ct);
            Set(loaded, nameof(OrderAddressHistory.NewDistrict), "Başka");
            InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));
            Assert.Contains("only appended", error.Message, StringComparison.Ordinal);
        }

        await using (AppDbContext context = NewContext())
        {
            OrderEmailHistory loaded = await context.OrderEmailHistories.SingleAsync(h => h.Id == email.Id, Ct);
            Set(loaded, nameof(OrderEmailHistory.AdminId), (Guid?)Guid.NewGuid());
            await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));
        }

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM order_email_histories WHERE new_email = 'yeni@example.com'"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM order_address_histories WHERE new_district = 'Çankaya'"));
    }

    [Fact]
    public async Task EmptyingTheAdminLinkOfAHistoryRowThroughTheContext_IsAccepted()
    {
        Order order = await SeedOrderAsync();
        Admin admin = await SeedAdminAsync();
        Province province = await ScalarProvinceAsync();
        OrderEmailHistory email = OrderEmailHistory.Create(order.Id, "eski@example.com", "yeni@example.com", admin.Id, admin.Name, Now);
        OrderAddressHistory address = NewAddressHistory(order.Id, AddressType.BillingAddress, admin, province.Id, province.Id);
        await SaveAsync(c =>
        {
            c.OrderEmailHistories.Add(email);
            c.OrderAddressHistories.Add(address);
        });

        await using (AppDbContext context = NewContext())
        {
            Set(await context.OrderEmailHistories.SingleAsync(h => h.Id == email.Id, Ct), nameof(OrderEmailHistory.AdminId), (Guid?)null);
            Set(await context.OrderAddressHistories.SingleAsync(h => h.Id == address.Id, Ct), nameof(OrderAddressHistory.AdminId), (Guid?)null);
            await context.SaveChangesAsync(Ct);
        }

        Assert.Equal(2L, await ScalarAsync<long>("SELECT (SELECT count(*) FROM order_email_histories WHERE admin_id IS NULL AND admin_name = 'Veli') + (SELECT count(*) FROM order_address_histories WHERE admin_id IS NULL AND admin_name = 'Veli')"));
    }

    [Fact]
    public async Task TheDisposalsExplicitDeleteOfTheHistoryAndReservationRows_IsAccepted()
    {
        // 06 4.14, 4.61, 4.62: the reservation ends by its own event and the histories go with the order's disposal, by an explicit delete.
        Order order = await SeedOrderAsync();
        Variant variant = await SeedVariantAsync();
        Admin admin = await SeedAdminAsync();
        Province province = await ScalarProvinceAsync();
        StockReservation reservation = StockReservation.ForVariant(order.Id, variant.Id, 1, Now);
        OrderEmailHistory email = OrderEmailHistory.Create(order.Id, "eski@example.com", "yeni@example.com", admin.Id, admin.Name, Now);
        OrderAddressHistory address = NewAddressHistory(order.Id, AddressType.BillingAddress, admin, province.Id, province.Id);
        await SaveAsync(c =>
        {
            c.StockReservations.Add(reservation);
            c.OrderEmailHistories.Add(email);
            c.OrderAddressHistories.Add(address);
        });

        await using (AppDbContext context = NewContext())
        {
            context.StockReservations.Remove(await context.StockReservations.SingleAsync(r => r.Id == reservation.Id, Ct));
            context.OrderEmailHistories.Remove(await context.OrderEmailHistories.SingleAsync(h => h.Id == email.Id, Ct));
            context.OrderAddressHistories.Remove(await context.OrderAddressHistories.SingleAsync(h => h.Id == address.Id, Ct));
            await context.SaveChangesAsync(Ct);
        }

        Assert.Equal(0L, await ScalarAsync<long>("SELECT (SELECT count(*) FROM stock_reservations) + (SELECT count(*) FROM order_email_histories) + (SELECT count(*) FROM order_address_histories)"));
    }

    // ---- helpers ----

    /// <summary>The history rows' setters are private (D-66); a test that needs a value the factory never makes sets it by name.</summary>
    private static void Set<TEntity>(TEntity entity, string property, object? value)
        where TEntity : class
    {
        typeof(TEntity).GetProperty(property)!.SetValue(entity, value);
    }

    private static OrderAddressHistory NewAddressHistory(Guid orderId, AddressType type, Admin admin, Guid oldProvinceId, Guid newProvinceId)
    {
        string? phone = type == AddressType.ShippingAddress ? "5321234567" : null;
        return OrderAddressHistory.Create(
            orderId,
            type,
            ("Ali Veli", phone, oldProvinceId, "Keçiören", "Eski Sokak 1"),
            ("Ayşe Veli", phone, newProvinceId, "Çankaya", "Yeni Sokak 2"),
            admin.Id,
            admin.Name,
            Now);
    }

    private async Task SeedRowAsync(string table, Order order, Variant variant, Admin admin, Province province)
    {
        switch (table)
        {
            case "stock_reservations":
                await SaveAsync(c => c.StockReservations.Add(StockReservation.ForVariant(order.Id, variant.Id, 1, Now)));
                break;
            case "order_email_histories":
                await SaveAsync(c => c.OrderEmailHistories.Add(OrderEmailHistory.Create(order.Id, "eski@example.com", "yeni@example.com", admin.Id, admin.Name, Now)));
                break;
            default:
                await SaveAsync(c => c.OrderAddressHistories.Add(NewAddressHistory(order.Id, AddressType.BillingAddress, admin, province.Id, province.Id)));
                break;
        }
    }

    private async Task<Province> ScalarProvinceAsync()
    {
        string id = await ScalarAsync<string>("SELECT id::text FROM provinces WHERE code = '06'");
        await using AppDbContext context = NewContext();
        return await context.Provinces.SingleAsync(p => p.Id == Guid.Parse(id), Ct);
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
        order.CompanyPhone = "3121234567";
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
        long count = await ScalarAsync<long>("SELECT count(*) FROM provinces");
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
        Admin admin = Admin.Create("Veli", "veli@example.com", "digest-placeholder", Now);
        await SaveAsync(c => c.Admins.Add(admin));
        return admin;
    }

    private async Task<Variant> SeedVariantAsync(string sku = "KAPI-1")
    {
        Product product = Product.Create("Kapı " + sku, "kapi-" + sku.ToLowerInvariant(), ProductType.Physical, 20m, Now).WithSeededOrigin();
        Variant variant = Variant.Create(product.Id, sku, 100m, Now, 100);
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
