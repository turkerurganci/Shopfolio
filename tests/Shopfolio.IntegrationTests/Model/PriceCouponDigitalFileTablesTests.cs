using System.Reflection;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the price, coupon and digital file tests (T105): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(PriceCouponDigitalFileTablesDefinition.Name)]
public sealed class PriceCouponDigitalFileTablesDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "PriceCouponDigitalFileTables";
}

/// <summary>
/// The structure of the price history, the discount, the coupon and the digital file on a real PostgreSQL (T105 criteria 1-7; 06 4.11-4.13, 4.70;
/// 06 2.2.3; 09 10.1, 10.6): the migration made the four tables, the database itself refuses what the rules refuse - unique indexes, filtered
/// unique indexes and check constraints - a variant deletion takes its price history, a product deletion its discount, both empty the link of a
/// digital file, and the context refuses an update of a price history row and the deletion of a coupon.
/// </summary>
[Collection(PriceCouponDigitalFileTablesDefinition.Name)]
public sealed class PriceCouponDigitalFileTablesTests : IAsyncLifetime
{
    private const string CheckViolation = PostgresErrorCodes.CheckViolation;
    private const string UniqueViolation = PostgresErrorCodes.UniqueViolation;

    private static readonly string[] FourTables = ["coupons", "digital_files", "discounts", "price_histories"];

    private static readonly DateTimeOffset Now = new(2026, 11, 4, 9, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly Day1 = new(2026, 11, 10);

    private static readonly DateOnly Day2 = new(2026, 11, 20);

    private readonly CollectionDatabaseFixture _collection;

    public PriceCouponDigitalFileTablesTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE price_histories, discounts, coupons, digital_files, variants, products CASCADE");
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
        // 06 4.11-4.13, 4.70 and 2.5.1: money numeric(12,2), percent numeric(5,2), dates date, the byte size bigint.
        string[] expected =
        [
            "coupons.amount numeric(12,2)",
            "coupons.code text not null",
            "coupons.created_at timestamp with time zone not null",
            "coupons.end_date date not null",
            "coupons.id uuid not null",
            "coupons.minimum_basket_amount numeric(12,2)",
            "coupons.normalized_code text not null",
            "coupons.percentage numeric(5,2)",
            "coupons.reserved_count integer not null",
            "coupons.start_date date not null",
            "coupons.total_usage_limit integer not null",
            "coupons.used_count integer not null",
            "coupons.version integer not null",
            "digital_files.attached_at timestamp with time zone",
            "digital_files.created_at timestamp with time zone not null",
            "digital_files.detached_at timestamp with time zone",
            "digital_files.file_name text not null",
            "digital_files.id uuid not null",
            "digital_files.product_id uuid",
            "digital_files.size_bytes bigint not null",
            "digital_files.storage_key text not null",
            "digital_files.upload_completed_at timestamp with time zone",
            "digital_files.variant_id uuid",
            "discounts.created_at timestamp with time zone not null",
            "discounts.end_date date not null",
            "discounts.id uuid not null",
            "discounts.percentage numeric(5,2) not null",
            "discounts.product_id uuid not null",
            "discounts.start_date date not null",
            "price_histories.created_at timestamp with time zone not null",
            "price_histories.id uuid not null",
            "price_histories.price numeric(12,2) not null",
            "price_histories.started_at timestamp with time zone not null",
            "price_histories.variant_id uuid not null",
        ];

        List<string> columns = await QueryAsync(
            """
            SELECT c.relname || '.' || a.attname || ' ' || format_type(a.atttypid, a.atttypmod) || CASE WHEN a.attnotnull THEN ' not null' ELSE '' END
            FROM pg_attribute a JOIN pg_class c ON c.oid = a.attrelid JOIN pg_namespace n ON n.oid = c.relnamespace
            WHERE n.nspname = 'public' AND c.relname = ANY(@names) AND a.attnum > 0 AND NOT a.attisdropped
            """,
            ("names", FourTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), columns.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ColumnsWithADefault_AreExactlyTheThreeOfTheFieldTables_WithTheirDocumentedValues()
    {
        // 06 4 "Default" is the database's default: only the coupon's two right counters (0) and its version (1) carry one.
        List<string> defaults = await QueryAsync(
            "SELECT table_name || '.' || column_name || '=' || column_default FROM information_schema.columns " +
            "WHERE table_schema = 'public' AND table_name = ANY(@names) AND column_default IS NOT NULL",
            ("names", FourTables));

        Assert.Equal(["coupons.reserved_count=0", "coupons.used_count=0", "coupons.version=1"], defaults.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task RawCouponInsertWithoutTheDefaultedColumns_GetsTheDocumentedDefaults()
    {
        Guid id = EntityIds.New();
        await ExecuteAsync(
            "INSERT INTO coupons (id, created_at, code, normalized_code, percentage, start_date, end_date, total_usage_limit) " +
            $"VALUES ('{id}', now(), 'YAZ', 'yaz', 10, '2026-11-10', '2026-11-20', 5)");

        Assert.Equal(0, await ScalarAsync<int>($"SELECT used_count FROM coupons WHERE id = '{id}'"));
        Assert.Equal(0, await ScalarAsync<int>($"SELECT reserved_count FROM coupons WHERE id = '{id}'"));
        Assert.Equal(1, await ScalarAsync<int>($"SELECT version FROM coupons WHERE id = '{id}'"));
    }

    [Fact]
    public async Task DeleteActionsOfTheFourTables_AreCascadeAndSetNullAsDocumented_AndNeverRestrict()
    {
        List<string> actions = await QueryAsync(
            "SELECT conname || '=' || confdeltype FROM pg_constraint WHERE contype = 'f' AND conrelid::regclass::text = ANY(@names)",
            ("names", FourTables));

        Assert.Equal(
            [
                "fk_digital_files_products_product_id=n",
                "fk_digital_files_variants_variant_id=n",
                "fk_discounts_products_product_id=c",
                "fk_price_histories_variants_variant_id=c",
            ],
            actions.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task EveryForeignKeyColumnOfTheFourTables_StartsAnUnfilteredIndex()
    {
        // 06 7.2: the emptying and the cascade read the link through an index that holds every row, so a filtered index does not count.
        List<string> unindexed = await QueryAsync(
            """
            SELECT c.conrelid::regclass::text || '.' || c.conname
            FROM pg_constraint c
            WHERE c.contype = 'f' AND c.conrelid::regclass::text = ANY(@tables)
              AND NOT EXISTS (
                SELECT 1 FROM pg_index i
                WHERE i.indrelid = c.conrelid AND i.indpred IS NULL AND (i.indkey::text || ' ') LIKE (array_to_string(c.conkey, ' ') || ' %'))
            """,
            ("tables", FourTables));

        Assert.Empty(unindexed);
    }

    [Fact]
    public async Task Indexes_AreTheDocumentedOnes()
    {
        List<string> indexes = await QueryAsync(
            "SELECT indexdef FROM pg_indexes WHERE schemaname = 'public' AND tablename = ANY(@names) AND indexname NOT LIKE 'pk_%'",
            ("names", FourTables));

        Assert.Equal(
            [
                "CREATE INDEX ix_digital_files_created_at ON public.digital_files USING btree (created_at) WHERE (upload_completed_at IS NULL)",
                "CREATE INDEX ix_digital_files_product_id ON public.digital_files USING btree (product_id)",
                "CREATE INDEX ix_digital_files_variant_id ON public.digital_files USING btree (variant_id)",
                "CREATE INDEX ix_price_histories_variant_id_started_at ON public.price_histories USING btree (variant_id, started_at)",
                "CREATE UNIQUE INDEX ix_coupons_normalized_code ON public.coupons USING btree (normalized_code)",
                "CREATE UNIQUE INDEX ix_digital_files_product_id1 ON public.digital_files USING btree (product_id) WHERE (detached_at IS NULL)",
                "CREATE UNIQUE INDEX ix_digital_files_storage_key ON public.digital_files USING btree (storage_key)",
                "CREATE UNIQUE INDEX ix_digital_files_variant_id1 ON public.digital_files USING btree (variant_id) WHERE (detached_at IS NULL)",
                "CREATE UNIQUE INDEX ix_discounts_product_id ON public.discounts USING btree (product_id)",
            ],
            indexes.Order(StringComparer.Ordinal));
    }

    // ---- KK2: price history ----

    [Theory]
    [InlineData("0")]
    [InlineData("-0.01")]
    public async Task PriceHistoryWithAPriceNotAboveZero_IsRefused(string price)
    {
        Variant variant = await SeedVariantAsync();

        Assert.Equal(
            (CheckViolation, "ck_price_histories_price_above_zero"),
            await ViolationOfAsync(c => c.PriceHistories.Add(PriceHistory.Create(variant.Id, decimal.Parse(price, System.Globalization.CultureInfo.InvariantCulture), Now, Now))));
    }

    [Fact]
    public async Task PriceHistoryRows_AreAppended()
    {
        Variant variant = await SeedVariantAsync();
        await SaveAsync(c => c.PriceHistories.Add(PriceHistory.Create(variant.Id, 100m, Now, Now)));
        await SaveAsync(c => c.PriceHistories.Add(PriceHistory.Create(variant.Id, 80m, Now.AddDays(1), Now.AddDays(1))));

        Assert.Equal(2L, await ScalarAsync<long>($"SELECT count(*) FROM price_histories WHERE variant_id = '{variant.Id}'"));
    }

    [Fact]
    public void PriceHistory_HasNoPublicSetter()
    {
        PropertyInfo[] writable = [.. typeof(PriceHistory).GetProperties().Where(p => p.SetMethod is { IsPublic: true })];

        Assert.Empty(writable);
    }

    [Fact]
    public async Task UpdatingAPriceHistoryRowThroughTheContext_IsRefused_AndTheRowStays()
    {
        Variant variant = await SeedVariantAsync();
        PriceHistory row = PriceHistory.Create(variant.Id, 100m, Now, Now);
        await SaveAsync(c => c.PriceHistories.Add(row));

        await using (AppDbContext context = NewContext())
        {
            PriceHistory loaded = await context.PriceHistories.SingleAsync(p => p.Id == row.Id, Ct);
            context.Entry(loaded).Property(p => p.Price).CurrentValue = 90m;
            InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));
            Assert.Contains("never updated", error.Message, StringComparison.Ordinal);
        }

        Assert.Equal(100m, await ScalarAsync<decimal>($"SELECT price FROM price_histories WHERE id = '{row.Id}'"));
    }

    [Fact]
    public async Task DeletingAVariant_TakesItsPriceHistory_AlsoWhenTheContextTracksTheRows()
    {
        Variant first = await SeedVariantAsync("A");
        Variant second = await SeedVariantAsync("B");
        await SaveAsync(c =>
        {
            c.PriceHistories.Add(PriceHistory.Create(first.Id, 100m, Now, Now));
            c.PriceHistories.Add(PriceHistory.Create(second.Id, 100m, Now, Now));
        });

        await ExecuteAsync($"DELETE FROM variants WHERE id = '{first.Id}'");
        await using (AppDbContext context = NewContext())
        {
            _ = await context.PriceHistories.Where(p => p.VariantId == second.Id).ToListAsync(Ct);
            context.Variants.Remove(await context.Variants.SingleAsync(v => v.Id == second.Id, Ct));
            await context.SaveChangesAsync(Ct);
        }

        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM price_histories"));
    }

    // ---- KK3: discount ----

    [Fact]
    public async Task SecondDiscountOfAProduct_IsRefused()
    {
        Product product = await SeedProductAsync();
        await SaveAsync(c => c.Discounts.Add(Discount.Create(product.Id, 10m, Day1, Day2, Now)));

        Assert.Equal((UniqueViolation, "ix_discounts_product_id"), await ViolationOfAsync(c => c.Discounts.Add(Discount.Create(product.Id, 20m, Day1, Day2, Now))));
    }

    [Theory]
    [InlineData("100")]
    [InlineData("0")]
    [InlineData("-5")]
    public async Task DiscountPercentageOutsideTheRange_IsRefused(string percentage)
    {
        Product product = await SeedProductAsync();

        Assert.Equal(
            (CheckViolation, "ck_discounts_percentage_range"),
            await ViolationOfAsync(c => c.Discounts.Add(Discount.Create(product.Id, Dec(percentage), Day1, Day2, Now))));
    }

    [Fact]
    public async Task DiscountOfNinetyNinePointNinetyNine_EndingOnItsStartDay_IsAccepted()
    {
        Product product = await SeedProductAsync();

        await SaveAsync(c => c.Discounts.Add(Discount.Create(product.Id, 99.99m, Day1, Day1, Now)));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM discounts"));
    }

    [Fact]
    public async Task DiscountEndingBeforeItStarts_IsRefused()
    {
        Product product = await SeedProductAsync();

        Assert.Equal((CheckViolation, "ck_discounts_end_not_before_start"), await ViolationOfAsync(c => c.Discounts.Add(Discount.Create(product.Id, 10m, Day2, Day1, Now))));
    }

    [Fact]
    public async Task DeletingAProduct_TakesItsDiscount()
    {
        Product product = await SeedProductAsync();
        await SaveAsync(c => c.Discounts.Add(Discount.Create(product.Id, 10m, Day1, Day2, Now)));

        await ExecuteAsync($"DELETE FROM products WHERE id = '{product.Id}'");

        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM discounts"));
    }

    // ---- KK4: coupon ----

    [Fact]
    public async Task ValidPercentageAndAmountCoupons_AreAccepted()
    {
        Coupon percentage = Coupon.CreatePercentage("YAZ10", 99.99m, Day1, Day1, 0, Now);
        percentage.MinimumBasketAmount = 0m;
        Coupon amount = Coupon.CreateAmount("İNDİRİM50", 50m, 50.01m, Day1, Day2, 1, Now);

        await SaveAsync(c =>
        {
            c.Coupons.Add(percentage);
            c.Coupons.Add(amount);
        });

        Assert.Equal("indirim50", await ScalarAsync<string>($"SELECT normalized_code FROM coupons WHERE id = '{amount.Id}'"));
        Assert.Equal("İNDİRİM50", await ScalarAsync<string>($"SELECT code FROM coupons WHERE id = '{amount.Id}'"));
    }

    [Fact]
    public async Task SecondCouponWithTheSameNormalizedCode_IsRefused()
    {
        await SaveAsync(c => c.Coupons.Add(Coupon.CreatePercentage("YAZ10", 10m, Day1, Day2, 5, Now)));

        Assert.Equal(
            (UniqueViolation, "ix_coupons_normalized_code"),
            await ViolationOfAsync(c => c.Coupons.Add(Coupon.CreatePercentage(" yaz10 ", 20m, Day1, Day2, 5, Now))));
    }

    [Fact]
    public async Task CouponWithBothPercentageAndAmount_IsRefused()
    {
        Coupon coupon = Coupon.CreateAmount("IKISI", 50m, 100m, Day1, Day2, 5, Now);
        coupon.Percentage = 10m;

        Assert.Equal((CheckViolation, "ck_coupons_exactly_one_value"), await ViolationOfAsync(c => c.Coupons.Add(coupon)));
    }

    [Fact]
    public async Task CouponWithNeitherPercentageNorAmount_IsRefused()
    {
        Coupon coupon = Coupon.CreatePercentage("HICBIRI", 10m, Day1, Day2, 5, Now);
        coupon.Percentage = null;

        Assert.Equal((CheckViolation, "ck_coupons_exactly_one_value"), await ViolationOfAsync(c => c.Coupons.Add(coupon)));
    }

    [Theory]
    [InlineData("100")]
    [InlineData("0")]
    public async Task CouponPercentageOutsideTheRange_IsRefused(string percentage)
    {
        Assert.Equal(
            (CheckViolation, "ck_coupons_percentage_range"),
            await ViolationOfAsync(c => c.Coupons.Add(Coupon.CreatePercentage("YUZDE", Dec(percentage), Day1, Day2, 5, Now))));
    }

    [Fact]
    public async Task CouponEndingBeforeItStarts_IsRefused()
    {
        Assert.Equal(
            (CheckViolation, "ck_coupons_end_not_before_start"),
            await ViolationOfAsync(c => c.Coupons.Add(Coupon.CreatePercentage("TERS", 10m, Day2, Day1, 5, Now))));
    }

    [Fact]
    public async Task AmountCouponWithoutAMinimumBasketAmount_IsRefused()
    {
        Coupon coupon = Coupon.CreateAmount("ASGARISIZ", 50m, 100m, Day1, Day2, 5, Now);
        coupon.MinimumBasketAmount = null;

        Assert.Equal((CheckViolation, "ck_coupons_amount_needs_larger_minimum_basket"), await ViolationOfAsync(c => c.Coupons.Add(coupon)));
    }

    [Fact]
    public async Task AmountCouponWithAMinimumBasketAmountEqualToTheAmount_IsRefused()
    {
        Assert.Equal(
            (CheckViolation, "ck_coupons_amount_needs_larger_minimum_basket"),
            await ViolationOfAsync(c => c.Coupons.Add(Coupon.CreateAmount("ESIT", 50m, 50m, Day1, Day2, 5, Now))));
    }

    [Fact]
    public async Task AmountCouponWithAZeroAmount_IsRefused()
    {
        Assert.Equal(
            (CheckViolation, "ck_coupons_amount_above_zero"),
            await ViolationOfAsync(c => c.Coupons.Add(Coupon.CreateAmount("SIFIR", 0m, 10m, Day1, Day2, 5, Now))));
    }

    [Fact]
    public async Task PercentageCouponWithANegativeMinimumBasketAmount_IsRefused()
    {
        Coupon coupon = Coupon.CreatePercentage("EKSIASGARI", 10m, Day1, Day2, 5, Now);
        coupon.MinimumBasketAmount = -0.01m;

        Assert.Equal((CheckViolation, "ck_coupons_minimum_basket_amount_not_negative"), await ViolationOfAsync(c => c.Coupons.Add(coupon)));
    }

    [Fact]
    public async Task CouponWithANegativeTotalUsageLimit_IsRefused()
    {
        Assert.Equal(
            (CheckViolation, "ck_coupons_usage_counts_not_negative"),
            await ViolationOfAsync(c => c.Coupons.Add(Coupon.CreatePercentage("EKSI", 10m, Day1, Day2, -1, Now))));
    }

    [Theory]
    [InlineData("used_count")]
    [InlineData("reserved_count")]
    public async Task CouponWithANegativeRightCounter_IsRefused(string column)
    {
        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(
            $"INSERT INTO coupons (id, created_at, code, normalized_code, percentage, start_date, end_date, total_usage_limit, {column}) " +
            $"VALUES ('{EntityIds.New()}', now(), 'EKSI', 'eksi', 10, '2026-11-10', '2026-11-20', 5, -1)"));

        Assert.Equal((CheckViolation, "ck_coupons_usage_counts_not_negative"), (error.SqlState, error.ConstraintName));
    }

    // ---- KK5: a coupon is never deleted ----

    [Fact]
    public async Task DeletingACouponThroughTheContext_IsRefused_ButItsFormCanBeChanged()
    {
        Coupon coupon = Coupon.CreatePercentage("KALICI", 10m, Day1, Day2, 5, Now);
        await SaveAsync(c => c.Coupons.Add(coupon));

        await using (AppDbContext context = NewContext())
        {
            context.Coupons.Remove(await context.Coupons.SingleAsync(c => c.Id == coupon.Id, Ct));
            InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));
            Assert.Contains("never deleted", error.Message, StringComparison.Ordinal);
        }

        await using (AppDbContext context = NewContext())
        {
            Coupon loaded = await context.Coupons.SingleAsync(c => c.Id == coupon.Id, Ct);
            loaded.EndDate = Day1;
            loaded.BumpVersion();
            await context.SaveChangesAsync(Ct);
        }

        Assert.Equal(2, await ScalarAsync<int>($"SELECT version FROM coupons WHERE id = '{coupon.Id}'"));
    }

    [Fact]
    public async Task EveryLinkToTheCoupons_IsRestrictive()
    {
        // 06 4.13: the incoming links (the order's coupon link, the coupon reservation) are restrictive. They arrive with their own tables
        // (T109, T110); the rule is held here for every link that exists.
        List<string> actions = await QueryAsync(
            "SELECT conname || '=' || confdeltype FROM pg_constraint WHERE contype = 'f' AND confrelid = 'public.coupons'::regclass");

        Assert.All(actions, a => Assert.EndsWith("=a", a, StringComparison.Ordinal));
    }

    // ---- KK6: digital file ----

    [Fact]
    public async Task IncompleteUploadWithAProductLink_IsRefused()
    {
        Product product = await SeedProductAsync();
        DigitalFile file = NewFile();
        file.ProductId = product.Id;

        Assert.Equal((CheckViolation, "ck_digital_files_incomplete_upload_unattached"), await ViolationOfAsync(c => c.DigitalFiles.Add(file)));
    }

    [Fact]
    public async Task IncompleteUploadWithAnAttachMoment_IsRefused()
    {
        DigitalFile file = NewFile();
        file.AttachedAt = Now;

        Assert.Equal((CheckViolation, "ck_digital_files_incomplete_upload_unattached"), await ViolationOfAsync(c => c.DigitalFiles.Add(file)));
    }

    [Fact]
    public async Task FileLinkedToBothAProductAndAVariant_IsRefused()
    {
        Variant variant = await SeedVariantAsync();
        DigitalFile file = Attached(NewFile());
        file.ProductId = variant.ProductId;
        file.VariantId = variant.Id;

        Assert.Equal((CheckViolation, "ck_digital_files_at_most_one_owner"), await ViolationOfAsync(c => c.DigitalFiles.Add(file)));
    }

    [Fact]
    public async Task DetachedWithoutAnAttachMoment_IsRefused()
    {
        DigitalFile file = NewFile();
        file.UploadCompletedAt = Now;
        file.DetachedAt = Now;

        Assert.Equal((CheckViolation, "ck_digital_files_detached_needs_attached"), await ViolationOfAsync(c => c.DigitalFiles.Add(file)));
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    public async Task FileSizeNotAboveZero_IsRefused(long size)
    {
        DigitalFile file = DigitalFile.Create(EntityIds.NewStorageKey(), "kitap.pdf", size, Now);

        Assert.Equal((CheckViolation, "ck_digital_files_size_above_zero"), await ViolationOfAsync(c => c.DigitalFiles.Add(file)));
    }

    [Fact]
    public async Task FileSizeBeyondTheIntegerRange_IsStored()
    {
        DigitalFile file = DigitalFile.Create(EntityIds.NewStorageKey(), "video.mp4", 5_000_000_000L, Now);

        await SaveAsync(c => c.DigitalFiles.Add(file));

        Assert.Equal(5_000_000_000L, await ScalarAsync<long>($"SELECT size_bytes FROM digital_files WHERE id = '{file.Id}'"));
    }

    [Fact]
    public async Task SecondFileWithTheSameStorageKey_IsRefused()
    {
        await SaveAsync(c => c.DigitalFiles.Add(DigitalFile.Create("ayni-anahtar", "a.pdf", 10, Now)));

        Assert.Equal(
            (UniqueViolation, "ix_digital_files_storage_key"),
            await ViolationOfAsync(c => c.DigitalFiles.Add(DigitalFile.Create("ayni-anahtar", "b.pdf", 10, Now))));
    }

    [Fact]
    public async Task SecondCurrentFileOfAProduct_IsRefused_ButADetachedOneIsNot()
    {
        Product product = await SeedProductAsync();
        DigitalFile detached = Attached(NewFile());
        detached.ProductId = product.Id;
        detached.DetachedAt = Now;
        DigitalFile current = Attached(NewFile());
        current.ProductId = product.Id;
        await SaveAsync(c =>
        {
            c.DigitalFiles.Add(detached);
            c.DigitalFiles.Add(current);
        });

        DigitalFile second = Attached(NewFile());
        second.ProductId = product.Id;

        Assert.Equal((UniqueViolation, "ix_digital_files_product_id1"), await ViolationOfAsync(c => c.DigitalFiles.Add(second)));
    }

    [Fact]
    public async Task SecondCurrentFileOfAVariant_IsRefused_ButADetachedOneIsNot()
    {
        Variant variant = await SeedVariantAsync();
        DigitalFile detached = Attached(NewFile());
        detached.VariantId = variant.Id;
        detached.DetachedAt = Now;
        DigitalFile current = Attached(NewFile());
        current.VariantId = variant.Id;
        await SaveAsync(c =>
        {
            c.DigitalFiles.Add(detached);
            c.DigitalFiles.Add(current);
        });

        DigitalFile second = Attached(NewFile());
        second.VariantId = variant.Id;

        Assert.Equal((UniqueViolation, "ix_digital_files_variant_id1"), await ViolationOfAsync(c => c.DigitalFiles.Add(second)));
    }

    [Fact]
    public async Task DeletingAProductOrAVariant_KeepsTheFileRows_WithTheirLinksEmptied()
    {
        Variant variant = await SeedVariantAsync();
        Product other = await SeedProductAsync("diger");
        DigitalFile productFile = Attached(NewFile());
        productFile.ProductId = other.Id;
        DigitalFile variantFile = Attached(NewFile());
        variantFile.VariantId = variant.Id;
        await SaveAsync(c =>
        {
            c.DigitalFiles.Add(productFile);
            c.DigitalFiles.Add(variantFile);
        });

        await ExecuteAsync($"DELETE FROM products WHERE id = '{other.Id}'");
        await ExecuteAsync($"DELETE FROM variants WHERE id = '{variant.Id}'");

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM digital_files WHERE product_id IS NULL AND variant_id IS NULL AND attached_at IS NOT NULL"));
    }

    private static decimal Dec(string value) => decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);

    private static DigitalFile NewFile() => DigitalFile.Create(EntityIds.NewStorageKey(), "kitap.pdf", 1024, Now);

    private static DigitalFile Attached(DigitalFile file)
    {
        file.UploadCompletedAt = Now;
        file.AttachedAt = Now;
        return file;
    }

    private async Task<Product> SeedProductAsync(string slug = "dijital-kitap")
    {
        Product product = Product.Create("Ürün " + slug, slug, ProductType.Digital, 20m, Now);
        await SaveAsync(c => c.Products.Add(product));
        return product;
    }

    private async Task<Variant> SeedVariantAsync(string sku = "KITAP-1")
    {
        Product product = await SeedProductAsync("urun-" + sku.ToLowerInvariant());
        Variant variant = Variant.Create(product.Id, sku, 100m, Now);
        await SaveAsync(c => c.Variants.Add(variant));
        return variant;
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
            rows.Add(Convert.ToString(reader.GetValue(0), System.Globalization.CultureInfo.InvariantCulture)!);
        }

        return rows;
    }
}
