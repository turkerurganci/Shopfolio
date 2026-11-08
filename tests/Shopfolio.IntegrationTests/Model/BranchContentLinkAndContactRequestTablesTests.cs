using System.Globalization;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Settings;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the branch, content-product link and contact request tests (T115): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(BranchContentLinkAndContactRequestTablesDefinition.Name)]
public sealed class BranchContentLinkAndContactRequestTablesDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "BranchContentLinkAndContactRequestTables";
}

/// <summary>
/// The branch, the two content-product links and the contact request on a real PostgreSQL (T115 criteria 1-7; 06 4.50, 4.53, 4.57, 2.2.5,
/// 2.5.1, 7.1-7.5; 09 10.6): the migration made the four tables; the database itself refuses a nine-digit phone, an alternative text without
/// an image, a third publish status, a second link of a product to a content record and a closed request without a closing moment; the
/// manual order repeats freely; the links go with their product and their content record; the branch's province and image links are
/// restrictive.
/// </summary>
[Collection(BranchContentLinkAndContactRequestTablesDefinition.Name)]
public sealed class BranchContentLinkAndContactRequestTablesTests : IAsyncLifetime
{
    private const string CheckViolation = PostgresErrorCodes.CheckViolation;
    private const string UniqueViolation = PostgresErrorCodes.UniqueViolation;
    private const string ForeignKeyViolation = PostgresErrorCodes.ForeignKeyViolation;

    private static readonly string[] FourTables = ["branches", "contact_requests", "offering_products", "portfolio_item_products"];

    private static readonly DateTimeOffset Now = new(2026, 11, 4, 11, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    public BranchContentLinkAndContactRequestTablesTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE branches, contact_requests, offering_products, portfolio_item_products, offerings, portfolio_items, products, images, provinces CASCADE");
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
        // 06 4.50, 4.53, 4.57 (the common Id and CreatedAt included); the lengths are P-27 and P-28 of 06 11.2; the phone is ten characters
        // (06 2.5.1); every address and every free text is text.
        string[] expected =
        [
            "branches.address_line text",
            "branches.created_at timestamp with time zone not null",
            "branches.district text",
            "branches.id uuid not null",
            "branches.image_alt_text text",
            "branches.image_id uuid",
            "branches.map_url text",
            "branches.name character varying(150) not null",
            "branches.phone character varying(10)",
            "branches.province_id uuid",
            "branches.publish_status text not null",
            "branches.sort_order integer not null",
            "branches.version integer not null",
            "branches.working_hours character varying(200)",
            "contact_requests.contact_request_status text not null",
            "contact_requests.contact_topic text not null",
            "contact_requests.created_at timestamp with time zone not null",
            "contact_requests.email text not null",
            "contact_requests.id uuid not null",
            "contact_requests.last_closed_at timestamp with time zone",
            "contact_requests.message text not null",
            "contact_requests.name text not null",
            "contact_requests.phone character varying(10)",
            "offering_products.created_at timestamp with time zone not null",
            "offering_products.id uuid not null",
            "offering_products.offering_id uuid not null",
            "offering_products.product_id uuid not null",
            "offering_products.sort_order integer not null",
            "portfolio_item_products.created_at timestamp with time zone not null",
            "portfolio_item_products.id uuid not null",
            "portfolio_item_products.portfolio_item_id uuid not null",
            "portfolio_item_products.product_id uuid not null",
            "portfolio_item_products.sort_order integer not null",
        ];

        List<string> columns = await QueryAsync(
            "SELECT table_name || '.' || column_name || ' ' || data_type"
            + " || CASE WHEN character_maximum_length IS NOT NULL THEN '(' || character_maximum_length || ')' ELSE '' END"
            + " || CASE WHEN is_nullable = 'NO' THEN ' not null' ELSE '' END"
            + " FROM information_schema.columns WHERE table_schema = 'public' AND table_name = ANY(@names)",
            ("names", FourTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), columns.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Columns_HaveTheDatabaseDefaultsOfTheDefaultColumn()
    {
        // The "Varsayılan" column of 06 4.50 and 4.57 is a database default, not only a default of the entity: the branch's status is Draft
        // and its version 1, the request's status is Open. No other column has a default.
        string[] expected =
        [
            "branches.publish_status 'Draft'::text",
            "branches.version 1",
            "contact_requests.contact_request_status 'Open'::text",
        ];

        List<string> defaults = await QueryAsync(
            "SELECT table_name || '.' || column_name || ' ' || column_default FROM information_schema.columns"
            + " WHERE table_schema = 'public' AND table_name = ANY(@names) AND column_default IS NOT NULL",
            ("names", FourTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), defaults.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Indexes_AreTheDocumentedOnes()
    {
        // 06 4.50, 4.53, 4.57 and 7.1, 7.4, 7.5.2: the branch's list index and its two link indexes; the two pair-unique indexes, whose first
        // column serves the content record's links, and the product indexes of the cascade; the request's list index and the expression index
        // of the disposal scan.
        string[] expected =
        [
            "branches.ix_branches_image_id | CREATE INDEX ix_branches_image_id ON public.branches USING btree (image_id)",
            "branches.ix_branches_province_id | CREATE INDEX ix_branches_province_id ON public.branches USING btree (province_id)",
            "branches.ix_branches_publish_status_sort_order | CREATE INDEX ix_branches_publish_status_sort_order ON public.branches USING btree (publish_status, sort_order)",
            "branches.pk_branches | CREATE UNIQUE INDEX pk_branches ON public.branches USING btree (id)",
            "contact_requests.ix_contact_requests_contact_request_status_created_at | CREATE INDEX ix_contact_requests_contact_request_status_created_at ON public.contact_requests USING btree (contact_request_status, created_at)",
            "contact_requests.ix_contact_requests_contact_topic_retention_start | CREATE INDEX ix_contact_requests_contact_topic_retention_start ON public.contact_requests USING btree (contact_topic, COALESCE(last_closed_at, created_at))",
            "contact_requests.pk_contact_requests | CREATE UNIQUE INDEX pk_contact_requests ON public.contact_requests USING btree (id)",
            "offering_products.ix_offering_products_offering_id_product_id | CREATE UNIQUE INDEX ix_offering_products_offering_id_product_id ON public.offering_products USING btree (offering_id, product_id)",
            "offering_products.ix_offering_products_product_id | CREATE INDEX ix_offering_products_product_id ON public.offering_products USING btree (product_id)",
            "offering_products.pk_offering_products | CREATE UNIQUE INDEX pk_offering_products ON public.offering_products USING btree (id)",
            "portfolio_item_products.ix_portfolio_item_products_portfolio_item_id_product_id | CREATE UNIQUE INDEX ix_portfolio_item_products_portfolio_item_id_product_id ON public.portfolio_item_products USING btree (portfolio_item_id, product_id)",
            "portfolio_item_products.ix_portfolio_item_products_product_id | CREATE INDEX ix_portfolio_item_products_product_id ON public.portfolio_item_products USING btree (product_id)",
            "portfolio_item_products.pk_portfolio_item_products | CREATE UNIQUE INDEX pk_portfolio_item_products ON public.portfolio_item_products USING btree (id)",
        ];

        List<string> indexes = await QueryAsync(
            "SELECT tablename || '.' || indexname || ' | ' || indexdef FROM pg_indexes WHERE schemaname = 'public' AND tablename = ANY(@names)",
            ("names", FourTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), indexes.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ForeignKeys_AreTheRestrictiveBranchLinksAndTheCascadingContentProductLinks()
    {
        // 06 4.50, 4.53, 7.2: the branch's province and image links are restrictive (a); each content-product link cascades (c) with its content
        // record and with its product; the contact request links nowhere.
        string[] expected =
        [
            "branches fk_branches_images_image_id a",
            "branches fk_branches_provinces_province_id a",
            "offering_products fk_offering_products_offerings_offering_id c",
            "offering_products fk_offering_products_products_product_id c",
            "portfolio_item_products fk_portfolio_item_products_portfolio_items_portfolio_item_id c",
            "portfolio_item_products fk_portfolio_item_products_products_product_id c",
        ];

        List<string> links = await QueryAsync(
            "SELECT conrelid::regclass::text || ' ' || conname || ' ' || confdeltype::text FROM pg_constraint WHERE contype = 'f' AND conrelid = ANY(ARRAY(SELECT x::regclass FROM unnest(@oids) x))",
            ("oids", FourTables.Select(t => "public." + t).ToArray()));

        Assert.Equal(expected.Order(StringComparer.Ordinal), links.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task CheckConstraints_AreTheDocumentedOnes()
    {
        // 06 2.5.1 (the phone), 2.9.2 (the two-valued status), 4.50 (the alternative text), 4.57 (the closing moment and the closed lists).
        string[] expected =
        [
            "branches.ck_branches_image_alt_text_only_with_image | CHECK (((image_id IS NOT NULL) OR (image_alt_text IS NULL)))",
            "branches.ck_branches_phone_ten_digits | CHECK (((phone)::text ~ '^[0-9]{10}$'::text))",
            "branches.ck_branches_publish_status | CHECK ((publish_status = ANY (ARRAY['Draft'::text, 'Published'::text])))",
            "contact_requests.ck_contact_requests_closed_has_last_closed_at | CHECK (((contact_request_status <> 'Closed'::text) OR (last_closed_at IS NOT NULL)))",
            "contact_requests.ck_contact_requests_contact_request_status | CHECK ((contact_request_status = ANY (ARRAY['Open'::text, 'Closed'::text])))",
            "contact_requests.ck_contact_requests_contact_topic | CHECK ((contact_topic = ANY (ARRAY['GeneralQuestion'::text, 'AboutOrder'::text, 'AboutProduct'::text, 'PersonalDataRequest'::text, 'Other'::text])))",
            "contact_requests.ck_contact_requests_phone_ten_digits | CHECK (((phone)::text ~ '^[0-9]{10}$'::text))",
        ];

        List<string> checks = await QueryAsync(
            "SELECT c.conrelid::regclass::text || '.' || c.conname || ' | ' || pg_get_constraintdef(c.oid) FROM pg_constraint c"
            + " WHERE c.contype = 'c' AND c.conrelid = ANY(ARRAY(SELECT x::regclass FROM unnest(@oids) x))",
            ("oids", FourTables.Select(t => "public." + t).ToArray()));

        Assert.Equal(expected.Order(StringComparer.Ordinal), checks.Order(StringComparer.Ordinal));
    }

    // ---- KK2: the phone is ten digits ----

    [Theory]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    [InlineData("053212345678")]
    [InlineData("+905321234567")]
    [InlineData("532 123 4567")]
    [InlineData("532123456a")]
    [InlineData("")]
    public async Task BranchPhone_NotTenDigits_IsRefused(string phone)
    {
        // The length check of the column (ten characters) or the digit rule refuses; the digit rule alone is the constraint under test for the
        // values that fit the length.
        PostgresException error = await Assert.ThrowsAsync<PostgresException>(
            () => ExecuteAsync("INSERT INTO branches (id, created_at, name, sort_order, phone) VALUES (gen_random_uuid(), now(), 'Merkez', 1, @phone)", ("phone", phone)));

        Assert.True(error.SqlState is CheckViolation or PostgresErrorCodes.StringDataRightTruncation, error.SqlState);
    }

    [Fact]
    public async Task BranchPhone_NineDigits_IsRefusedByTheDigitRule()
    {
        Assert.Equal(
            (CheckViolation, "ck_branches_phone_ten_digits"),
            await ViolationOfAsync(c => c.Branches.Add(NewBranch("Merkez", 1, phone: "532123456"))));
    }

    [Fact]
    public async Task ContactRequestPhone_NineDigits_IsRefusedByTheDigitRule()
    {
        ContactRequest request = NewRequest();
        request.Phone = "532123456";

        Assert.Equal((CheckViolation, "ck_contact_requests_phone_ten_digits"), await ViolationOfAsync(c => c.ContactRequests.Add(request)));
    }

    [Fact]
    public async Task Phone_TenDigitsOrEmpty_IsAccepted()
    {
        await SaveAsync(c =>
        {
            c.Branches.Add(NewBranch("Merkez", 1, phone: "5321234567"));
            c.Branches.Add(NewBranch("Şube", 2, phone: null));
            ContactRequest withPhone = NewRequest();
            withPhone.Phone = "5321234567";
            c.ContactRequests.Add(withPhone);
            c.ContactRequests.Add(NewRequest());
        });

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM branches"));
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM contact_requests"));
    }

    // ---- KK3: the branch ----

    [Fact]
    public async Task Branch_AltTextWithoutAnImage_IsRefused()
    {
        Branch branch = NewBranch("Merkez", 1);
        branch.ImageAltText = "Mağaza girişi";

        Assert.Equal((CheckViolation, "ck_branches_image_alt_text_only_with_image"), await ViolationOfAsync(c => c.Branches.Add(branch)));
    }

    [Fact]
    public async Task Branch_AltTextWithAnImage_AndAnImageWithoutAltText_AreAccepted()
    {
        Image first = Image.Create("branch-key-1", ImageFormat.Png, 100, 100, Now);
        Image second = Image.Create("branch-key-2", ImageFormat.Png, 100, 100, Now);
        await SaveAsync(c =>
        {
            c.Images.Add(first);
            c.Images.Add(second);
        });

        await SaveAsync(c =>
        {
            Branch withText = NewBranch("Merkez", 1);
            withText.ImageId = first.Id;
            withText.ImageAltText = "Mağaza girişi";
            Branch withoutText = NewBranch("Şube", 2);
            withoutText.ImageId = second.Id;
            c.Branches.Add(withText);
            c.Branches.Add(withoutText);
        });

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM branches"));
    }

    [Fact]
    public async Task Branch_PublishStatusThirdValueOfTheEnum_IsRefused()
    {
        Branch branch = NewBranch("Merkez", 1);
        branch.PublishStatus = PublishStatus.Archived;

        Assert.Equal((CheckViolation, "ck_branches_publish_status"), await ViolationOfAsync(c => c.Branches.Add(branch)));
    }

    [Fact]
    public async Task Branch_PublishStatusUnknownText_IsRefused()
    {
        Assert.Equal(
            (CheckViolation, "ck_branches_publish_status"),
            await ViolationOfSqlAsync("INSERT INTO branches (id, created_at, name, sort_order, publish_status) VALUES (gen_random_uuid(), now(), 'a', 1, 'Hidden')"));
    }

    [Theory]
    [InlineData("Draft")]
    [InlineData("Published")]
    public async Task Branch_PublishStatusDraftAndPublished_AreAccepted(string status)
    {
        await ExecuteAsync(
            "INSERT INTO branches (id, created_at, name, sort_order, publish_status) VALUES (gen_random_uuid(), now(), 'a', 1, @status)",
            ("status", status));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM branches"));
    }

    [Fact]
    public async Task Branch_OnlyTheNameAndTheOrderAreRequired_AndTheDatabaseFillsTheDefaults()
    {
        await ExecuteAsync("INSERT INTO branches (id, created_at, name, sort_order) VALUES (gen_random_uuid(), now(), 'Taslak şube', 3)");

        Assert.Equal("Draft", await ScalarAsync<string>("SELECT publish_status FROM branches"));
        Assert.Equal(1, await ScalarAsync<int>("SELECT version FROM branches"));
    }

    [Fact]
    public async Task Branch_SortOrder_SameValueInTwoRows_IsAccepted()
    {
        await SaveAsync(c =>
        {
            c.Branches.Add(NewBranch("Merkez", 1));
            c.Branches.Add(NewBranch("Şube", 1));
        });

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM branches WHERE sort_order = 1"));
    }

    [Fact]
    public async Task Branch_LongTextAndShortTextColumns_FollowTheirLengths()
    {
        string longValue = new('a', 100_000);
        await ExecuteAsync(
            "INSERT INTO branches (id, created_at, name, sort_order, district, address_line, map_url) VALUES (gen_random_uuid(), now(), @name, 1, @long, @long, @long)",
            ("name", new string('a', 150)),
            ("long", longValue));

        Assert.Equal(100_000, await ScalarAsync<int>("SELECT length(address_line) FROM branches"));

        PostgresException name = await Assert.ThrowsAsync<PostgresException>(
            () => ExecuteAsync("INSERT INTO branches (id, created_at, name, sort_order) VALUES (gen_random_uuid(), now(), @v, 1)", ("v", new string('a', 151))));
        PostgresException hours = await Assert.ThrowsAsync<PostgresException>(
            () => ExecuteAsync("INSERT INTO branches (id, created_at, name, sort_order, working_hours) VALUES (gen_random_uuid(), now(), 'a', 1, @v)", ("v", new string('a', 201))));

        Assert.Equal(PostgresErrorCodes.StringDataRightTruncation, name.SqlState);
        Assert.Equal(PostgresErrorCodes.StringDataRightTruncation, hours.SqlState);
    }

    [Fact]
    public async Task DeletingTheImageOfABranch_IsRefused_AndAReplacedImageIsLeftUnlinkedAndDeletable()
    {
        Image image = Image.Create("branch-image", ImageFormat.Png, 100, 100, Now);
        Image replacement = Image.Create("branch-image-2", ImageFormat.Png, 100, 100, Now);
        await SaveAsync(c =>
        {
            c.Images.Add(image);
            c.Images.Add(replacement);
        });
        Branch branch = NewBranch("Merkez", 1);
        branch.ImageId = image.Id;
        await SaveAsync(c => c.Branches.Add(branch));

        Assert.Equal(ForeignKeyViolation, (await ViolationOfSqlAsync("DELETE FROM images WHERE id = @id", ("id", image.Id))).SqlState);

        await ExecuteAsync("UPDATE branches SET image_id = @new WHERE id = @id", ("new", replacement.Id), ("id", branch.Id));
        await ExecuteAsync("DELETE FROM images WHERE id = @id", ("id", image.Id));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM images"));
    }

    [Fact]
    public async Task DeletingTheProvinceOfABranch_IsRefused()
    {
        Province province = Province.Create("06", "Ankara", Now);
        await SaveAsync(c => c.Provinces.Add(province));
        Branch branch = NewBranch("Merkez", 1);
        branch.ProvinceId = province.Id;
        await SaveAsync(c => c.Branches.Add(branch));

        Assert.Equal(ForeignKeyViolation, (await ViolationOfSqlAsync("DELETE FROM provinces WHERE id = @id", ("id", province.Id))).SqlState);
    }

    [Fact]
    public async Task BranchDeleted_LeavesItsImageAndProvince()
    {
        Province province = Province.Create("06", "Ankara", Now);
        Image image = Image.Create("branch-image", ImageFormat.Png, 100, 100, Now);
        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.Images.Add(image);
        });
        Branch branch = NewBranch("Merkez", 1);
        branch.ProvinceId = province.Id;
        branch.ImageId = image.Id;
        await SaveAsync(c => c.Branches.Add(branch));

        await ExecuteAsync("DELETE FROM branches WHERE id = @id", ("id", branch.Id));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM images"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM provinces"));
    }

    // ---- KK4: a product is linked to a content record once ----

    [Fact]
    public async Task OfferingProduct_SecondLinkOfTheSameProduct_IsRefused()
    {
        Offering offering = Offering.Create("Hizmet", "hizmet", 1, Now);
        Product product = NewProduct("kapi");
        await SaveAsync(c =>
        {
            c.Offerings.Add(offering);
            c.Products.Add(product);
        });
        await SaveAsync(c => c.OfferingProducts.Add(OfferingProduct.Create(offering.Id, product.Id, 1, Now)));

        Assert.Equal(
            (UniqueViolation, "ix_offering_products_offering_id_product_id"),
            await ViolationOfAsync(c => c.OfferingProducts.Add(OfferingProduct.Create(offering.Id, product.Id, 2, Now))));
    }

    [Fact]
    public async Task PortfolioItemProduct_SecondLinkOfTheSameProduct_IsRefused()
    {
        PortfolioItem item = PortfolioItem.Create("İş", "is", 1, Now);
        Product product = NewProduct("kapi");
        await SaveAsync(c =>
        {
            c.PortfolioItems.Add(item);
            c.Products.Add(product);
        });
        await SaveAsync(c => c.PortfolioItemProducts.Add(PortfolioItemProduct.Create(item.Id, product.Id, 1, Now)));

        Assert.Equal(
            (UniqueViolation, "ix_portfolio_item_products_portfolio_item_id_product_id"),
            await ViolationOfAsync(c => c.PortfolioItemProducts.Add(PortfolioItemProduct.Create(item.Id, product.Id, 2, Now))));
    }

    [Fact]
    public async Task ContentProductLinks_OneProductOnTwoRecords_AndTwoProductsOnOneRecord_AndTheSameOrder_AreAccepted()
    {
        Offering first = Offering.Create("Hizmet 1", "hizmet-1", 1, Now);
        Offering second = Offering.Create("Hizmet 2", "hizmet-2", 2, Now);
        PortfolioItem item = PortfolioItem.Create("İş", "is", 1, Now);
        Product a = NewProduct("a");
        Product b = NewProduct("b");
        await SaveAsync(c =>
        {
            c.Offerings.AddRange(first, second);
            c.PortfolioItems.Add(item);
            c.Products.AddRange(a, b);
        });

        await SaveAsync(c =>
        {
            c.OfferingProducts.Add(OfferingProduct.Create(first.Id, a.Id, 1, Now));
            c.OfferingProducts.Add(OfferingProduct.Create(first.Id, b.Id, 1, Now));
            c.OfferingProducts.Add(OfferingProduct.Create(second.Id, a.Id, 1, Now));
            c.PortfolioItemProducts.Add(PortfolioItemProduct.Create(item.Id, a.Id, 1, Now));
            c.PortfolioItemProducts.Add(PortfolioItemProduct.Create(item.Id, b.Id, 1, Now));
        });

        Assert.Equal(3L, await ScalarAsync<long>("SELECT count(*) FROM offering_products"));
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM portfolio_item_products"));
    }

    // ---- KK5: the links go with the product and with the content record ----

    [Fact]
    public async Task OfferingProduct_GoesWithTheProductAndWithTheOffering()
    {
        Offering offering = Offering.Create("Hizmet", "hizmet", 1, Now);
        Product kept = NewProduct("kalan");
        Product removed = NewProduct("silinen");
        await SaveAsync(c =>
        {
            c.Offerings.Add(offering);
            c.Products.AddRange(kept, removed);
        });
        await SaveAsync(c =>
        {
            c.OfferingProducts.Add(OfferingProduct.Create(offering.Id, kept.Id, 1, Now));
            c.OfferingProducts.Add(OfferingProduct.Create(offering.Id, removed.Id, 2, Now));
        });

        await ExecuteAsync("DELETE FROM products WHERE id = @id", ("id", removed.Id));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM offering_products"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM offerings"));

        await ExecuteAsync("DELETE FROM offerings WHERE id = @id", ("id", offering.Id));

        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM offering_products"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM products"));
    }

    [Fact]
    public async Task PortfolioItemProduct_GoesWithTheProductAndWithThePortfolioItem()
    {
        PortfolioItem item = PortfolioItem.Create("İş", "is", 1, Now);
        Product kept = NewProduct("kalan");
        Product removed = NewProduct("silinen");
        await SaveAsync(c =>
        {
            c.PortfolioItems.Add(item);
            c.Products.AddRange(kept, removed);
        });
        await SaveAsync(c =>
        {
            c.PortfolioItemProducts.Add(PortfolioItemProduct.Create(item.Id, kept.Id, 1, Now));
            c.PortfolioItemProducts.Add(PortfolioItemProduct.Create(item.Id, removed.Id, 2, Now));
        });

        await ExecuteAsync("DELETE FROM products WHERE id = @id", ("id", removed.Id));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM portfolio_item_products"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM portfolio_items"));

        await ExecuteAsync("DELETE FROM portfolio_items WHERE id = @id", ("id", item.Id));

        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM portfolio_item_products"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM products"));
    }

    [Fact]
    public async Task ContentProductLink_OfAnUnknownProductOrRecord_IsRefused()
    {
        Offering offering = Offering.Create("Hizmet", "hizmet", 1, Now);
        await SaveAsync(c => c.Offerings.Add(offering));

        Assert.Equal(
            ForeignKeyViolation,
            (await ViolationOfAsync(c => c.OfferingProducts.Add(OfferingProduct.Create(offering.Id, Guid.NewGuid(), 1, Now)))).SqlState);
    }

    // ---- KK6: the contact request ----

    [Fact]
    public async Task ContactRequest_ClosedWithoutALastClosedMoment_IsRefused()
    {
        ContactRequest request = NewRequest();
        request.ContactRequestStatus = ContactRequestStatus.Closed;

        Assert.Equal((CheckViolation, "ck_contact_requests_closed_has_last_closed_at"), await ViolationOfAsync(c => c.ContactRequests.Add(request)));
    }

    [Fact]
    public async Task ContactRequest_OpenOrClosedWithTheMoment_AreAccepted_AndReopeningKeepsTheMoment()
    {
        ContactRequest never = NewRequest();
        ContactRequest closed = NewRequest();
        closed.ContactRequestStatus = ContactRequestStatus.Closed;
        closed.LastClosedAt = Now;
        ContactRequest reopened = NewRequest();
        reopened.LastClosedAt = Now;
        await SaveAsync(c => c.ContactRequests.AddRange(never, closed, reopened));

        Assert.Equal(3L, await ScalarAsync<long>("SELECT count(*) FROM contact_requests"));
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM contact_requests WHERE last_closed_at IS NOT NULL"));
    }

    [Fact]
    public async Task ContactRequest_StatusAndTopicOutsideTheClosedLists_AreRefused()
    {
        Assert.Equal(
            (CheckViolation, "ck_contact_requests_contact_request_status"),
            await ViolationOfSqlAsync(
                "INSERT INTO contact_requests (id, created_at, name, email, contact_topic, message, contact_request_status) VALUES (gen_random_uuid(), now(), 'a', 'a@b.c', 'Other', 'm', 'Pending')"));
        Assert.Equal(
            (CheckViolation, "ck_contact_requests_contact_topic"),
            await ViolationOfSqlAsync(
                "INSERT INTO contact_requests (id, created_at, name, email, contact_topic, message) VALUES (gen_random_uuid(), now(), 'a', 'a@b.c', 'Complaint', 'm')"));
    }

    [Fact]
    public async Task ContactRequest_IsBornOpen_AndTakesALongMessage()
    {
        await ExecuteAsync(
            "INSERT INTO contact_requests (id, created_at, name, email, contact_topic, message) VALUES (gen_random_uuid(), now(), 'Ad', 'a@b.c', 'AboutOrder', @message)",
            ("message", new string('m', 100_000)));

        Assert.Equal("Open", await ScalarAsync<string>("SELECT contact_request_status FROM contact_requests"));
        Assert.Equal(100_000, await ScalarAsync<int>("SELECT length(message) FROM contact_requests"));
    }

    [Fact]
    public async Task ContactRequest_EveryTopic_IsAccepted()
    {
        await SaveAsync(c =>
        {
            foreach (ContactTopic topic in Enum.GetValues<ContactTopic>())
            {
                ContactRequest request = NewRequest();
                request.ContactTopic = topic;
                c.ContactRequests.Add(request);
            }
        });

        Assert.Equal((long)Enum.GetValues<ContactTopic>().Length, await ScalarAsync<long>("SELECT count(*) FROM contact_requests"));
    }

    // ---- helpers ----

    private static Branch NewBranch(string name, int sortOrder, string? phone = null)
    {
        Branch branch = Branch.Create(name, sortOrder, Now);
        branch.Phone = phone;
        return branch;
    }

    private static ContactRequest NewRequest() =>
        ContactRequest.Create("Ayşe", "ayse@example.com", ContactTopic.GeneralQuestion, "Merhaba", Now);

    private static Product NewProduct(string slug) =>
        Product.Create("Ürün " + slug, slug, ProductType.Physical, 20m, Now).WithSeededOrigin();

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

    /// <summary>Runs the statement and returns the SQLSTATE and the constraint name of the database error it raises.</summary>
    private async Task<(string SqlState, string? Constraint)> ViolationOfSqlAsync(string sql, params (string Name, object Value)[] parameters)
    {
        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(sql, parameters));
        return (error.SqlState, error.ConstraintName);
    }

    private async Task ExecuteAsync(string sql, params (string Name, object Value)[] parameters)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        foreach ((string name, object value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<T> ScalarAsync<T>(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        object? value = await command.ExecuteScalarAsync(Ct);
        return value is null or DBNull ? default! : (T)value;
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
}
