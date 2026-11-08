using System.Text;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Settings;

using DsnBuilder = Npgsql.NpgsqlConnectionStringBuilder;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the product core tests (T104): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(CatalogCoreTablesDefinition.Name)]
public sealed class CatalogCoreTablesDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "CatalogCoreTables";
}

/// <summary>
/// The structure of the product core on a real PostgreSQL (T104 criteria 1-8; 06 4.5-4.10, 4.69, 4.71, 4.72; 09 10.1, 10.6): the migration made the
/// eight tables, and the database itself refuses what the rules refuse - fences, unique indexes, check constraints and restrictive links - while a
/// product deletion takes its variants, options, category links and image links with it.
/// </summary>
[Collection(CatalogCoreTablesDefinition.Name)]
public sealed class CatalogCoreTablesTests : IAsyncLifetime
{
    private const string CheckViolation = PostgresErrorCodes.CheckViolation;
    private const string UniqueViolation = PostgresErrorCodes.UniqueViolation;
    private const string ForeignKeyViolation = PostgresErrorCodes.ForeignKeyViolation;

    private static readonly string[] EightTables =
        ["categories", "images", "option_values", "options", "product_categories", "product_images", "products", "variants"];

    private static readonly DateTimeOffset Now = new(2026, 11, 4, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    private Guid _originId;

    public CatalogCoreTablesTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE product_categories, product_images, variants, option_values, options, categories, images, products, countries CASCADE");

        // The countries are emptied above, so a physical product's required place of origin (02 3.8.5; T336) points at a row of this class's own.
        Country origin = Country.Create("ZZ", "Deneme Ülkesi", Now);
        _originId = origin.Id;
        await SaveAsync(c => c.Countries.Add(origin));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the migration ----

    [Fact]
    public async Task Migration_MakesTheEightTables()
    {
        List<string> tables = await QueryAsync(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name = ANY(@names) ORDER BY 1",
            ("names", EightTables));

        Assert.Equal(EightTables, tables);
    }

    [Fact]
    public async Task Migration_WhenPgTrgmIsNotInstalled_StopsWithAClearMessageAndKeepsTheEarlierMigrations()
    {
        // 09 10.6 items 8 and 11: the migration that makes the trigram index checks the extension first. A copy of the collection database is
        // taken back to the state before it (no extension, no product core tables, no history row) and the bundle is applied to it.
        string copy = "shopfolio_test_trgm_" + Convert.ToHexStringLower(System.Security.Cryptography.RandomNumberGenerator.GetBytes(4));
        DsnBuilder serverBuilder = new(_collection.Dsn) { Pooling = false };
        string originalDatabase = serverBuilder.Database!;
        serverBuilder.Database = "postgres";
        NpgsqlConnection.ClearAllPools();
        await using (NpgsqlConnection connection = new(serverBuilder.ConnectionString))
        {
            await connection.OpenAsync(Ct);
            await using NpgsqlCommand create = new($"CREATE DATABASE {copy} TEMPLATE {originalDatabase}", connection);
            await create.ExecuteNonQueryAsync(Ct);
        }

        try
        {
            serverBuilder.Database = copy;
            string copyDsn = serverBuilder.ConnectionString;
            await ExecuteOnAsync(
                copyDsn,
                "DROP EXTENSION pg_trgm CASCADE; "
                + "DROP TABLE product_categories, product_images, variants, option_values, options, categories, images, products CASCADE; "
                // The later migrations leave the history too: the bundle stops at the catalog core migration before it reaches them.
                + "DELETE FROM \"__EFMigrationsHistory\" WHERE migration_id >= "
                + "(SELECT migration_id FROM \"__EFMigrationsHistory\" WHERE migration_id LIKE '%CreateCatalogCoreTables')");

            using MigrationBundle bundle = await MigrationBundle.BuildAsync(Ct);
            InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => bundle.ApplyAsync(copyDsn, Ct));

            Assert.Contains("pg_trgm extension is not installed; the database creator user installs it", error.Message, StringComparison.Ordinal);
            Assert.EndsWith("_CreateSettingsTables", (await QueryOnAsync(copyDsn, "SELECT migration_id FROM \"__EFMigrationsHistory\"")).Single(), StringComparison.Ordinal);
            Assert.Empty(await QueryOnAsync(copyDsn, "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'products'"));
        }
        finally
        {
            NpgsqlConnection.ClearAllPools();
            serverBuilder.Database = "postgres";
            await using NpgsqlConnection connection = new(serverBuilder.ConnectionString);
            await connection.OpenAsync(Ct);
            await using NpgsqlCommand drop = new($"DROP DATABASE IF EXISTS {copy} WITH (FORCE)", connection);
            await drop.ExecuteNonQueryAsync(Ct);
        }
    }

    [Fact]
    public async Task ProductNameSearch_UsesTheTrigramIndex()
    {
        // 09 10.6 item 11: with sequential scans switched off the plan of the contains search on the search form is an index scan.
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using (NpgsqlCommand off = new("SET enable_seqscan = off", connection))
        {
            await off.ExecuteNonQueryAsync(Ct);
        }

        await using NpgsqlCommand explain = new("EXPLAIN SELECT id FROM products WHERE name_search_form LIKE '%celik%'", connection);
        StringBuilder plan = new();
        await using (NpgsqlDataReader reader = await explain.ExecuteReaderAsync(Ct))
        {
            while (await reader.ReadAsync(Ct))
            {
                plan.AppendLine(reader.GetString(0));
            }
        }

        Assert.Contains("ix_products_name_search_form", plan.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ValidProductWithVariantOptionsCategoryAndImage_IsAccepted()
    {
        Product product = NewProduct("celik-kapi");
        Option option = Option.Create(product.Id, "Renk", 1, Now);
        OptionValue value = OptionValue.Create(option.Id, "Kırmızı", 1, Now);
        Variant variant = Variant.Create(product.Id, "KAPI-1", 1500m, Now, 100);
        variant.FirstOptionValueId = value.Id;
        Category category = Category.Create("Kapılar", "kapilar", null, 1, Now);
        Image image = Image.Create("abc123", ImageFormat.Jpeg, 2560, 2560, Now);
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Options.Add(option);
            c.OptionValues.Add(value);
            c.Variants.Add(variant);
            c.Categories.Add(category);
            c.Images.Add(image);
        });
        await SaveAsync(c =>
        {
            c.ProductCategories.Add(ProductCategory.Create(product.Id, category.Id, true, Now));
            c.ProductImages.Add(ProductImage.Create(product.Id, image.Id, 1, true, Now));
        });

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM variants"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM product_images"));
    }

    // ---- 06 4 "Default": the database's own defaults (T104 correction 2) ----

    [Fact]
    public async Task ColumnsWithADefault_AreExactlyTheSixOfTheFieldTables_WithTheirDocumentedValues()
    {
        List<string> defaults = await QueryAsync(
            "SELECT table_name || '.' || column_name || '=' || column_default FROM information_schema.columns " +
            "WHERE table_schema = 'public' AND table_name = ANY(@names) AND column_default IS NOT NULL ORDER BY 1",
            ("names", EightTables));

        Assert.Equal(
            [
                "product_categories.is_primary_category=false",
                "product_images.is_main=false",
                "products.publish_status='Draft'::text",
                "products.version=1",
                "variants.publish_status='Draft'::text",
                "variants.reserved_quantity=0",
            ],
            defaults);
    }

    [Fact]
    public async Task RawInsertsWithoutTheDefaultedColumns_GetTheDocumentedDefaults()
    {
        Guid productId = Guid.CreateVersion7();
        Guid variantId = Guid.CreateVersion7();
        Category category = Category.Create("Kapılar", "kapilar", null, 1, Now);
        Image image = Image.Create("abc123", ImageFormat.Jpeg, 2560, 2560, Now);
        await SaveAsync(c =>
        {
            c.Categories.Add(category);
            c.Images.Add(image);
        });

        await ExecuteAsync(
            "INSERT INTO products (id, created_at, name, name_search_form, product_type, vat_rate, slug) " +
            $"VALUES ('{productId}', now(), 'Kapı', 'kapi', 'Physical', 20, 'kapi')");
        await ExecuteAsync(
            "INSERT INTO variants (id, created_at, product_id, sku, normalized_sku, price) " +
            $"VALUES ('{variantId}', now(), '{productId}', 'K-1', 'k-1', 10)");
        await ExecuteAsync(
            "INSERT INTO product_categories (id, created_at, product_id, category_id) " +
            $"VALUES ('{Guid.CreateVersion7()}', now(), '{productId}', '{category.Id}')");
        await ExecuteAsync(
            "INSERT INTO product_images (id, created_at, product_id, image_id, sort_order) " +
            $"VALUES ('{Guid.CreateVersion7()}', now(), '{productId}', '{image.Id}', 1)");

        Assert.Equal("Draft", await ScalarAsync<string>($"SELECT publish_status FROM products WHERE id = '{productId}'"));
        Assert.Equal(1, await ScalarAsync<int>($"SELECT version FROM products WHERE id = '{productId}'"));
        Assert.Equal("Draft", await ScalarAsync<string>($"SELECT publish_status FROM variants WHERE id = '{variantId}'"));
        Assert.Equal(0, await ScalarAsync<int>($"SELECT reserved_quantity FROM variants WHERE id = '{variantId}'"));
        Assert.False(await ScalarAsync<bool>($"SELECT is_primary_category FROM product_categories WHERE product_id = '{productId}'"));
        Assert.False(await ScalarAsync<bool>($"SELECT is_main FROM product_images WHERE product_id = '{productId}'"));
    }

    // ---- KK2: product fences, search form, trigram index ----

    [Fact]
    public async Task VatRateOfOneHundred_IsRefused_AndNinetyNinePointNinetyNineIsAccepted()
    {
        Product refused = NewProduct("a");
        refused.VatRate = 100m;
        Product accepted = NewProduct("b");
        accepted.VatRate = 99.99m;

        Assert.Equal((CheckViolation, "ck_products_vat_rate_range"), await ViolationOfAsync(c => c.Products.Add(refused)));
        await SaveAsync(c => c.Products.Add(accepted));
    }

    [Fact]
    public async Task NegativeVatRate_IsRefused()
    {
        Product product = NewProduct("a");
        product.VatRate = -0.01m;

        Assert.Equal((CheckViolation, "ck_products_vat_rate_range"), await ViolationOfAsync(c => c.Products.Add(product)));
    }

    [Theory]
    [InlineData(11)]
    [InlineData(-1)]
    public async Task ShippingPeriodOutsideTheFence_IsRefused(int days)
    {
        Product product = NewProduct("a");
        product.ShippingPeriod = days;
        Product stored = NewProduct("b");
        await SaveAsync(c => c.Products.Add(stored));

        // The save guard stops it in the application before any SQL is sent (T350 criterion 5; the P-6 fence).
        await using (AppDbContext context = NewContext())
        {
            context.Products.Add(product);
            await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));
        }

        // The column's check stands behind it for a write that does not pass through a save.
        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"UPDATE products SET shipping_period = {days} WHERE id = '{stored.Id}'"));
        Assert.Equal((CheckViolation, "ck_products_shipping_period_fence"), (error.SqlState, error.ConstraintName));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public async Task ShippingPeriodAtTheFenceEnds_IsAccepted(int days)
    {
        Product product = NewProduct("a");
        product.ShippingPeriod = days;

        await SaveAsync(c => c.Products.Add(product));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM products"));
    }

    [Fact]
    public async Task FulfilmentPeriodOfZero_IsRefused()
    {
        Product product = NewProduct("a");
        product.FulfilmentPeriod = 0;

        Assert.Equal((CheckViolation, "ck_products_fulfilment_period_fence"), await ViolationOfAsync(c => c.Products.Add(product)));
    }

    [Fact]
    public async Task MaxQuantityPerOrderOfZero_IsRefused_AndOneIsAccepted()
    {
        Product refused = NewProduct("a");
        refused.MaxQuantityPerOrder = 0;
        Product accepted = NewProduct("b");
        accepted.MaxQuantityPerOrder = 1;

        Assert.Equal((CheckViolation, "ck_products_max_quantity_per_order_fence"), await ViolationOfAsync(c => c.Products.Add(refused)));
        await SaveAsync(c => c.Products.Add(accepted));
    }

    [Fact]
    public async Task ProductNameLongerThanTheCeiling_IsRefused()
    {
        Product product = NewProduct("a");
        product.SetName(new string('a', ProductConstants.ProductNameMaxLength + 1));

        DbUpdateException error = await Assert.ThrowsAsync<DbUpdateException>(() => SaveAsync(c => c.Products.Add(product)));

        Assert.Equal(PostgresErrorCodes.StringDataRightTruncation, Assert.IsType<PostgresException>(error.InnerException).SqlState);
    }

    [Fact]
    public async Task NameAndItsSearchForm_AreWrittenTogether()
    {
        Product product = Product.Create("ÇELİK Kapı IŞIK", "celik-kapi", ProductType.Physical, 20m, Now);
        product.OriginCountryId = _originId;
        await SaveAsync(c => c.Products.Add(product));

        Assert.Equal("celik kapi isik", await ScalarAsync<string>($"SELECT name_search_form FROM products WHERE id = '{product.Id}'"));
        Assert.Equal("ÇELİK Kapı IŞIK", await ScalarAsync<string>($"SELECT name FROM products WHERE id = '{product.Id}'"));
    }

    [Fact]
    public async Task SearchFormOfAProduct_HasATrigramIndex()
    {
        string definition = await ScalarAsync<string>("SELECT indexdef FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'ix_products_name_search_form'");

        Assert.Contains("USING gin", definition, StringComparison.Ordinal);
        Assert.Contains("name_search_form gin_trgm_ops", definition, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ProductListIndexAndOriginCountryIndex_Exist()
    {
        List<string> indexes = await QueryAsync(
            "SELECT indexname FROM pg_indexes WHERE schemaname = 'public' AND tablename = 'products' ORDER BY 1");

        Assert.Equal(
            ["ix_products_name_search_form", "ix_products_origin_country_id", "ix_products_publish_status_created_at_id", "ix_products_slug", "pk_products"],
            indexes);
    }

    [Fact]
    public async Task EnumColumnsOfAProduct_RefuseAnUnknownCodeName()
    {
        Product product = NewProduct("a");
        await SaveAsync(c => c.Products.Add(product));

        PostgresException error = await Assert.ThrowsAsync<PostgresException>(
            () => ExecuteAsync($"UPDATE products SET product_type = 'Nonsense' WHERE id = '{product.Id}'"));

        Assert.Equal((CheckViolation, "ck_products_product_type"), (error.SqlState, error.ConstraintName));
    }

    [Fact]
    public async Task OriginCountry_MustExist_AndACountryInUseIsNotDeleted()
    {
        Country country = Country.Create("TR", "Türkiye", Now);
        Product orphan = NewProduct("a");
        orphan.OriginCountryId = Guid.NewGuid();
        Product product = NewProduct("b");
        product.OriginCountryId = country.Id;
        await SaveAsync(c => c.Countries.Add(country));

        Assert.Equal((ForeignKeyViolation, "fk_products_countries_origin_country_id"), await ViolationOfAsync(c => c.Products.Add(orphan)));
        await SaveAsync(c => c.Products.Add(product));
        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM countries WHERE id = '{country.Id}'"));
        Assert.Equal((ForeignKeyViolation, "fk_products_countries_origin_country_id"), (error.SqlState, error.ConstraintName));
    }

    // ---- KK3: variant ----

    [Fact]
    public async Task SameStockCodeInAnotherLetterCase_IsRefused()
    {
        Product product = NewProduct("a");
        Option option = Option.Create(product.Id, "Beden", 1, Now);
        OptionValue small = OptionValue.Create(option.Id, "S", 1, Now);
        OptionValue medium = OptionValue.Create(option.Id, "M", 2, Now);
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Options.Add(option);
            c.OptionValues.AddRange(small, medium);
        });
        Variant first = Variant.Create(product.Id, "Kapı-ISIK", 10m, Now, 100);
        first.FirstOptionValueId = small.Id;
        Variant second = Variant.Create(product.Id, "kapı-ısık", 10m, Now, 100);
        second.FirstOptionValueId = medium.Id;
        await SaveAsync(c => c.Variants.Add(first));

        Assert.Equal((UniqueViolation, "ix_variants_normalized_sku"), await ViolationOfAsync(c => c.Variants.Add(second)));
        Assert.Equal("Kapı-ISIK", await ScalarAsync<string>($"SELECT sku FROM variants WHERE id = '{first.Id}'"));
    }

    [Fact]
    public async Task ZeroPrice_IsRefused()
    {
        Product product = NewProduct("a");
        await SaveAsync(c => c.Products.Add(product));

        Assert.Equal((CheckViolation, "ck_variants_price_above_zero"), await ViolationOfAsync(c => c.Variants.Add(Variant.Create(product.Id, "A", 0m, Now, 100))));
    }

    [Fact]
    public async Task NetQuantityOfZero_IsRefused()
    {
        // A unit on the product, so the application's pairing guard lets the zero through and the database's own check is what refuses it.
        Product product = NewProduct("a");
        product.MeasurementUnit = MeasurementUnit.Kilogram;
        await SaveAsync(c => c.Products.Add(product));
        Variant variant = Variant.Create(product.Id, "A", 5m, Now, 100);
        variant.NetQuantity = 0m;

        Assert.Equal((CheckViolation, "ck_variants_net_quantity_above_zero"), await ViolationOfAsync(c => c.Variants.Add(variant)));
    }

    [Fact]
    public async Task NegativeStock_IsRefused()
    {
        Product product = NewProduct("a");
        await SaveAsync(c => c.Products.Add(product));
        Variant variant = Variant.Create(product.Id, "A", 5m, Now, 100);
        await SaveAsync(c => c.Variants.Add(variant));

        // A save that would write the negative number is refused by the stock guard first (T334); the constraint is the last defense of a write that bypasses it.
        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"UPDATE variants SET stock_quantity = -1 WHERE id = '{variant.Id}'"));

        Assert.Equal((CheckViolation, "ck_variants_stock_not_below_reserved"), (error.SqlState, error.ConstraintName));
    }

    [Fact]
    public async Task StockBelowTheReservedQuantity_IsRefused_AndEqualIsAccepted()
    {
        Product product = NewProduct("a");
        Variant variant = Variant.Create(product.Id, "A", 5m, Now, 100);
        variant.StockQuantity = 10;
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Variants.Add(variant);
        });
        await ExecuteAsync($"UPDATE variants SET reserved_quantity = 6 WHERE id = '{variant.Id}'");

        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"UPDATE variants SET stock_quantity = 5 WHERE id = '{variant.Id}'"));
        await ExecuteAsync($"UPDATE variants SET stock_quantity = 6 WHERE id = '{variant.Id}'");

        Assert.Equal((CheckViolation, "ck_variants_stock_not_below_reserved"), (error.SqlState, error.ConstraintName));
        Assert.Equal(6, await ScalarAsync<int>($"SELECT stock_quantity FROM variants WHERE id = '{variant.Id}'"));
    }

    [Fact]
    public async Task NegativeReservedQuantity_IsRefused_AndAnUnlimitedStockIsAccepted()
    {
        // An unlimited stock is a service without capacity (T334: a physical variant has a number).
        Product product = Product.Create("Ürün a", "a", ProductType.Service, 20m, Now);
        Variant variant = Variant.Create(product.Id, "A", 5m, Now);
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Variants.Add(variant);
        });

        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"UPDATE variants SET reserved_quantity = -1 WHERE id = '{variant.Id}'"));

        Assert.Equal((CheckViolation, "ck_variants_reserved_quantity_not_negative"), (error.SqlState, error.ConstraintName));
        Assert.Null(await ScalarOrNullAsync($"SELECT stock_quantity FROM variants WHERE id = '{variant.Id}'"));
    }

    [Fact]
    public async Task SecondVariantOfAProductWithoutOptions_IsRefused()
    {
        Product product = NewProduct("a");
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Variants.Add(Variant.Create(product.Id, "A", 5m, Now, 100));
        });

        Assert.Equal(
            (UniqueViolation, "ix_variants_product_id_first_option_value_id_second_option_val"),
            await ViolationOfAsync(c => c.Variants.Add(Variant.Create(product.Id, "B", 5m, Now, 100))));
    }

    [Fact]
    public async Task SecondValueWithoutTheFirst_IsRefused()
    {
        (Product product, _, OptionValue value) = await SeedOneOptionAsync();
        Variant variant = Variant.Create(product.Id, "A", 5m, Now, 100);
        variant.SecondOptionValueId = value.Id;

        Assert.Equal((CheckViolation, "ck_variants_second_value_needs_first"), await ViolationOfAsync(c => c.Variants.Add(variant)));
    }

    [Fact]
    public async Task ValueCombinationIsUniquePerProduct_AndAnotherValueIsAccepted()
    {
        (Product product, Option option, OptionValue red) = await SeedOneOptionAsync();
        OptionValue blue = OptionValue.Create(option.Id, "Mavi", 2, Now);
        await SaveAsync(c => c.OptionValues.Add(blue));
        Variant first = Variant.Create(product.Id, "A", 5m, Now, 100);
        first.FirstOptionValueId = red.Id;
        Variant duplicate = Variant.Create(product.Id, "B", 5m, Now, 100);
        duplicate.FirstOptionValueId = red.Id;
        Variant other = Variant.Create(product.Id, "C", 5m, Now, 100);
        other.FirstOptionValueId = blue.Id;
        await SaveAsync(c => c.Variants.Add(first));

        Assert.Equal(
            (UniqueViolation, "ix_variants_product_id_first_option_value_id_second_option_val"),
            await ViolationOfAsync(c => c.Variants.Add(duplicate)));
        await SaveAsync(c => c.Variants.Add(other));
    }

    [Fact]
    public async Task VariantImage_IsRestrictive()
    {
        Product product = NewProduct("a");
        Image image = Image.Create("k", ImageFormat.Png, 10, 10, Now);
        Variant variant = Variant.Create(product.Id, "A", 5m, Now, 100);
        variant.ImageId = image.Id;
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Images.Add(image);
            c.Variants.Add(variant);
        });

        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM images WHERE id = '{image.Id}'"));

        Assert.Equal((ForeignKeyViolation, "fk_variants_images_image_id"), (error.SqlState, error.ConstraintName));
    }

    // ---- KK4: option ----

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public async Task OptionOutsideOneAndTwo_IsRefused(int sortOrder)
    {
        Product product = NewProduct("a");
        await SaveAsync(c => c.Products.Add(product));

        Assert.Equal((CheckViolation, "ck_options_sort_order_fence"), await ViolationOfAsync(c => c.Options.Add(Option.Create(product.Id, "Üçüncü", sortOrder, Now))));
    }

    [Fact]
    public async Task SecondOptionWithTheSameOrder_IsRefused_AndTheTwoOrdersAreAccepted()
    {
        Product product = NewProduct("a");
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Options.Add(Option.Create(product.Id, "Renk", 1, Now));
            c.Options.Add(Option.Create(product.Id, "Beden", 2, Now));
        });

        Assert.Equal((UniqueViolation, "ix_options_product_id_sort_order"), await ViolationOfAsync(c => c.Options.Add(Option.Create(product.Id, "Başka", 2, Now))));
    }

    // ---- KK5: category ----

    [Theory]
    [InlineData(0, false)]
    [InlineData(4, true)]
    public async Task CategoryLevelOutsideOneAndThree_IsRefused(int level, bool withParent)
    {
        Category root = Category.Create("Kök", "kok", null, 1, Now);
        await SaveAsync(c => c.Categories.Add(root));
        Category category = Category.Create("X", "x", withParent ? root.Id : null, level, Now);

        Assert.Equal((CheckViolation, "ck_categories_level_fence"), await ViolationOfAsync(c => c.Categories.Add(category)));
    }

    [Fact]
    public async Task RootAboveLevelOne_AndChildAtLevelOne_AreRefused()
    {
        Category root = Category.Create("Kök", "kok", null, 1, Now);
        await SaveAsync(c => c.Categories.Add(root));

        Assert.Equal((CheckViolation, "ck_categories_level_follows_parent"), await ViolationOfAsync(c => c.Categories.Add(Category.Create("A", "a", null, 2, Now))));
        Assert.Equal((CheckViolation, "ck_categories_level_follows_parent"), await ViolationOfAsync(c => c.Categories.Add(Category.Create("B", "b", root.Id, 1, Now))));
    }

    [Fact]
    public async Task ThreeLevelTree_IsAccepted_AndAParentInUseIsNotDeleted()
    {
        Category one = Category.Create("Bir", "bir", null, 1, Now);
        Category two = Category.Create("İki", "iki", one.Id, 2, Now);
        Category three = Category.Create("Üç", "uc", two.Id, 3, Now);
        await SaveAsync(c => c.Categories.Add(one));
        await SaveAsync(c => c.Categories.Add(two));
        await SaveAsync(c => c.Categories.Add(three));

        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM categories WHERE id = '{one.Id}'"));

        Assert.Equal((ForeignKeyViolation, "fk_categories_categories_parent_category_id"), (error.SqlState, error.ConstraintName));
    }

    // ---- KK6: primary category, main image, slug ----

    [Fact]
    public async Task SecondPrimaryCategoryOfAProduct_IsRefused_ButASecondOrdinaryOneIsAccepted()
    {
        Product product = NewProduct("a");
        Category one = Category.Create("Bir", "bir", null, 1, Now);
        Category two = Category.Create("İki", "iki", null, 1, Now);
        Category three = Category.Create("Üç", "uc", null, 1, Now);
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Categories.AddRange(one, two, three);
        });
        await SaveAsync(c => c.ProductCategories.Add(ProductCategory.Create(product.Id, one.Id, true, Now)));

        Assert.Equal((UniqueViolation, "ix_product_categories_product_id"), await ViolationOfAsync(c => c.ProductCategories.Add(ProductCategory.Create(product.Id, two.Id, true, Now))));
        await SaveAsync(c => c.ProductCategories.Add(ProductCategory.Create(product.Id, two.Id, false, Now)));
        await SaveAsync(c => c.ProductCategories.Add(ProductCategory.Create(product.Id, three.Id, false, Now)));
    }

    [Fact]
    public async Task SameCategoryTwiceOnAProduct_IsRefused()
    {
        Product product = NewProduct("a");
        Category category = Category.Create("Bir", "bir", null, 1, Now);
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Categories.Add(category);
        });
        await SaveAsync(c => c.ProductCategories.Add(ProductCategory.Create(product.Id, category.Id, false, Now)));

        Assert.Equal(
            (UniqueViolation, "ix_product_categories_product_id_category_id"),
            await ViolationOfAsync(c => c.ProductCategories.Add(ProductCategory.Create(product.Id, category.Id, false, Now))));
    }

    [Fact]
    public async Task SecondMainImageOfAProduct_IsRefused_ButASecondOrdinaryOneIsAccepted()
    {
        Product product = NewProduct("a");
        Image one = Image.Create("k1", ImageFormat.Jpeg, 10, 10, Now);
        Image two = Image.Create("k2", ImageFormat.Jpeg, 10, 10, Now);
        Image three = Image.Create("k3", ImageFormat.Jpeg, 10, 10, Now);
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Images.AddRange(one, two, three);
        });
        await SaveAsync(c => c.ProductImages.Add(ProductImage.Create(product.Id, one.Id, 1, true, Now)));

        Assert.Equal((UniqueViolation, "ix_product_images_product_id"), await ViolationOfAsync(c => c.ProductImages.Add(ProductImage.Create(product.Id, two.Id, 2, true, Now))));
        await SaveAsync(c => c.ProductImages.Add(ProductImage.Create(product.Id, two.Id, 2, false, Now)));
        await SaveAsync(c => c.ProductImages.Add(ProductImage.Create(product.Id, three.Id, 2, false, Now)));
    }

    [Fact]
    public async Task SameImageTwiceInAGallery_IsRefused()
    {
        Product product = NewProduct("a");
        Image image = Image.Create("k1", ImageFormat.Jpeg, 10, 10, Now);
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Images.Add(image);
        });
        await SaveAsync(c => c.ProductImages.Add(ProductImage.Create(product.Id, image.Id, 1, true, Now)));

        Assert.Equal(
            (UniqueViolation, "ix_product_images_product_id_image_id"),
            await ViolationOfAsync(c => c.ProductImages.Add(ProductImage.Create(product.Id, image.Id, 2, false, Now))));
    }

    [Fact]
    public async Task SecondProductWithTheSameSlug_IsRefused_EvenInADifferentStatus()
    {
        await SaveAsync(c => c.Products.Add(NewProduct("celik-kapi")));
        Product archived = NewProduct("celik-kapi");
        archived.PublishStatus = PublishStatus.Archived;

        Assert.Equal((UniqueViolation, "ix_products_slug"), await ViolationOfAsync(c => c.Products.Add(archived)));
    }

    [Fact]
    public async Task SecondCategoryWithTheSameSlug_IsRefused()
    {
        await SaveAsync(c => c.Categories.Add(Category.Create("Bir", "ortak", null, 1, Now)));

        Assert.Equal((UniqueViolation, "ix_categories_slug"), await ViolationOfAsync(c => c.Categories.Add(Category.Create("İki", "ortak", null, 1, Now))));
    }

    [Theory]
    [InlineData("celik-kapi-")]
    [InlineData("-celik")]
    [InlineData("celik--kapi")]
    [InlineData("Celik")]
    [InlineData("çelik")]
    [InlineData("celik kapi")]
    public async Task MalformedSlug_IsRefused_ForAProductAndForACategory(string slug)
    {
        Product product = NewProduct("x");
        product.Slug = slug;

        Assert.Equal((CheckViolation, "ck_products_slug_format"), await ViolationOfAsync(c => c.Products.Add(product)));
        Assert.Equal((CheckViolation, "ck_categories_slug_format"), await ViolationOfAsync(c => c.Categories.Add(Category.Create("A", slug, null, 1, Now))));
    }

    [Theory]
    [InlineData("celik-kapi-2")]
    [InlineData("2024")]
    [InlineData("a")]
    public async Task WellFormedSlug_IsAccepted(string slug)
    {
        await SaveAsync(c => c.Products.Add(NewProduct(slug)));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM products"));
    }

    // ---- KK7: image fences and deletion ----

    [Theory]
    [InlineData(0, 10)]
    [InlineData(10, 0)]
    [InlineData(2561, 10)]
    [InlineData(10, 2561)]
    public async Task ImageDimensionOutsideTheLimits_IsRefused(int width, int height)
    {
        Assert.Equal((CheckViolation, "ck_images_dimensions_within_limits"), await ViolationOfAsync(c => c.Images.Add(Image.Create("k", ImageFormat.WebP, width, height, Now))));
    }

    [Fact]
    public async Task ImageStorageKey_IsNotUnique()
    {
        await SaveAsync(c => c.Images.Add(Image.Create("ayni", ImageFormat.Jpeg, 10, 10, Now)));
        await SaveAsync(c => c.Images.Add(Image.Create("ayni", ImageFormat.Jpeg, 10, 10, Now)));

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM images WHERE storage_key = 'ayni'"));
    }

    [Fact]
    public async Task DeletingAProduct_TakesVariantsOptionsValuesCategoryLinksAndImageLinks_ButNotTheCategoryOrTheImage()
    {
        Product product = NewProduct("a");
        Option option = Option.Create(product.Id, "Renk", 1, Now);
        OptionValue value = OptionValue.Create(option.Id, "Kırmızı", 1, Now);
        Variant variant = Variant.Create(product.Id, "A", 5m, Now, 100);
        variant.FirstOptionValueId = value.Id;
        Category category = Category.Create("Bir", "bir", null, 1, Now);
        Image image = Image.Create("k", ImageFormat.Jpeg, 10, 10, Now);
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Options.Add(option);
            c.OptionValues.Add(value);
            c.Variants.Add(variant);
            c.Categories.Add(category);
            c.Images.Add(image);
        });
        await SaveAsync(c =>
        {
            c.ProductCategories.Add(ProductCategory.Create(product.Id, category.Id, true, Now));
            c.ProductImages.Add(ProductImage.Create(product.Id, image.Id, 1, true, Now));
        });

        await ExecuteAsync($"DELETE FROM products WHERE id = '{product.Id}'");

        Assert.Equal(
            0L,
            await ScalarAsync<long>(
                "SELECT (SELECT count(*) FROM variants) + (SELECT count(*) FROM options) + (SELECT count(*) FROM option_values) "
                + "+ (SELECT count(*) FROM product_categories) + (SELECT count(*) FROM product_images)"));
        Assert.Equal(2L, await ScalarAsync<long>("SELECT (SELECT count(*) FROM categories) + (SELECT count(*) FROM images)"));
    }

    [Fact]
    public async Task CategoryHoldingAProduct_ImageInAGallery_AndOptionValueOfAVariant_AreNotDeleted()
    {
        (Product product, _, OptionValue value) = await SeedOneOptionAsync();
        Variant variant = Variant.Create(product.Id, "A", 5m, Now, 100);
        variant.FirstOptionValueId = value.Id;
        Category category = Category.Create("Bir", "bir", null, 1, Now);
        Image image = Image.Create("k", ImageFormat.Jpeg, 10, 10, Now);
        await SaveAsync(c =>
        {
            c.Variants.Add(variant);
            c.Categories.Add(category);
            c.Images.Add(image);
        });
        await SaveAsync(c =>
        {
            c.ProductCategories.Add(ProductCategory.Create(product.Id, category.Id, true, Now));
            c.ProductImages.Add(ProductImage.Create(product.Id, image.Id, 1, true, Now));
        });

        PostgresException categoryError = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM categories WHERE id = '{category.Id}'"));
        PostgresException imageError = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM images WHERE id = '{image.Id}'"));
        PostgresException valueError = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM option_values WHERE id = '{value.Id}'"));

        Assert.Equal((ForeignKeyViolation, "fk_product_categories_categories_category_id"), (categoryError.SqlState, categoryError.ConstraintName));
        Assert.Equal((ForeignKeyViolation, "fk_product_images_images_image_id"), (imageError.SqlState, imageError.ConstraintName));
        Assert.Equal((ForeignKeyViolation, "fk_variants_option_values_first_option_value_id"), (valueError.SqlState, valueError.ConstraintName));
    }

    [Fact]
    public async Task EveryForeignKeyColumnOfTheEightTables_StartsAnIndex()
    {
        // 06 7.2: the tables of this task have no single-row table; every foreign key column starts an index.
        List<string> unindexed = await QueryAsync(
            """
            SELECT c.conrelid::regclass::text || '.' || c.conname
            FROM pg_constraint c
            WHERE c.contype = 'f' AND c.conrelid::regclass::text = ANY(@tables)
              AND NOT EXISTS (
                SELECT 1 FROM pg_index i
                WHERE i.indrelid = c.conrelid AND (i.indkey::text || ' ') LIKE (array_to_string(c.conkey, ' ') || ' %'))
            ORDER BY 1
            """,
            ("tables", EightTables));

        Assert.Empty(unindexed);
    }

    private Product NewProduct(string slug)
    {
        Product product = Product.Create("Ürün " + slug, slug, ProductType.Physical, 20m, Now);
        product.OriginCountryId = _originId;
        return product;
    }

    private async Task<(Product Product, Option Option, OptionValue Value)> SeedOneOptionAsync()
    {
        Product product = NewProduct("secenekli");
        Option option = Option.Create(product.Id, "Renk", 1, Now);
        OptionValue value = OptionValue.Create(option.Id, "Kırmızı", 1, Now);
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Options.Add(option);
            c.OptionValues.Add(value);
        });
        return (product, option, value);
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

    private static async Task ExecuteOnAsync(string dsn, string sql)
    {
        await using NpgsqlConnection connection = new(dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private static async Task<List<string>> QueryOnAsync(string dsn, string sql)
    {
        await using NpgsqlConnection connection = new(dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        List<string> rows = [];
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(Ct);
        while (await reader.ReadAsync(Ct))
        {
            rows.Add(reader.GetString(0));
        }

        return rows;
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
        object? value = await ScalarOrNullAsync(sql);
        return (T)value!;
    }

    private async Task<object?> ScalarOrNullAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        object? value = await command.ExecuteScalarAsync(Ct);
        return value is DBNull ? null : value;
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
