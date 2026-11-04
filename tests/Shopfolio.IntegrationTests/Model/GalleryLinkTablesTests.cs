using System.Globalization;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the gallery link tests (T116): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(GalleryLinkTablesDefinition.Name)]
public sealed class GalleryLinkTablesDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "GalleryLinkTables";
}

/// <summary>
/// The five gallery links of the corporate content records on a real PostgreSQL (T116 criteria 1-6; 06 4.71, 2.2.5, 7.1-7.5, 9.2; 09 10.6):
/// the migration made the five tables of one shape; the database itself refuses a second link of the same image to a gallery and a second
/// main image of an owner; the manual order repeats freely; a link goes with its owner and its image link is restrictive, so a linked image
/// cannot be deleted. ProductImage (T104) is the product's counterpart and is tested in its own task.
/// </summary>
[Collection(GalleryLinkTablesDefinition.Name)]
public sealed class GalleryLinkTablesTests : IAsyncLifetime
{
    private const string UniqueViolation = PostgresErrorCodes.UniqueViolation;
    private const string ForeignKeyViolation = PostgresErrorCodes.ForeignKeyViolation;

    private static readonly DateTimeOffset Now = new(2026, 11, 4, 12, 0, 0, TimeSpan.Zero);

    private static readonly string[] FiveTables = ["about_page_images", "custom_page_images", "faq_item_images", "offering_images", "portfolio_item_images"];

    private static readonly Dictionary<string, Gallery> Galleries = new()
    {
        ["about_page_images"] = new(
            "about_page_images", "about_pages", "about_page_id",
            c =>
            {
                AboutPage owner = AboutPage.Create(Now);
                c.AboutPages.Add(owner);
                return owner.Id;
            },
            (owner, image, order, main) => AboutPageImage.Create(owner, image, order, main, Now)),
        ["offering_images"] = new(
            "offering_images", "offerings", "offering_id",
            c =>
            {
                Offering owner = Offering.Create("Hizmet", "hizmet-" + Guid.NewGuid().ToString("N"), 1, Now);
                c.Offerings.Add(owner);
                return owner.Id;
            },
            (owner, image, order, main) => OfferingImage.Create(owner, image, order, main, Now)),
        ["portfolio_item_images"] = new(
            "portfolio_item_images", "portfolio_items", "portfolio_item_id",
            c =>
            {
                PortfolioItem owner = PortfolioItem.Create("İş", "is-" + Guid.NewGuid().ToString("N"), 1, Now);
                c.PortfolioItems.Add(owner);
                return owner.Id;
            },
            (owner, image, order, main) => PortfolioItemImage.Create(owner, image, order, main, Now)),
        ["faq_item_images"] = new(
            "faq_item_images", "faq_items", "faq_item_id",
            c =>
            {
                FaqItem owner = FaqItem.Create("Soru?", 1, Now);
                c.FaqItems.Add(owner);
                return owner.Id;
            },
            (owner, image, order, main) => FaqItemImage.Create(owner, image, order, main, Now)),
        ["custom_page_images"] = new(
            "custom_page_images", "custom_pages", "custom_page_id",
            c =>
            {
                CustomPage owner = CustomPage.Create("Sayfa", "sayfa-" + Guid.NewGuid().ToString("N"), 1, Now);
                c.CustomPages.Add(owner);
                return owner.Id;
            },
            (owner, image, order, main) => CustomPageImage.Create(owner, image, order, main, Now)),
    };

    private readonly CollectionDatabaseFixture _collection;

    public GalleryLinkTablesTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    public static TheoryData<string> Tables => new(FiveTables);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE about_page_images, custom_page_images, faq_item_images, offering_images, portfolio_item_images,"
            + " about_pages, custom_pages, faq_items, offerings, portfolio_items, images CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the migration ----

    [Fact]
    public async Task Migration_MakesTheFiveTables()
    {
        List<string> tables = await QueryAsync(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name = ANY(@names)",
            ("names", FiveTables));

        Assert.Equal(FiveTables, tables.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Columns_HaveTheTypesAndNullabilityOfTheFieldTable()
    {
        // 06 4.71 (the common Id and CreatedAt included): the same seven columns in every table; the alternative text is free text.
        List<string> expected = [];
        foreach (Gallery g in Galleries.Values)
        {
            expected.AddRange(
            [
                g.Table + ".alt_text text",
                g.Table + ".created_at timestamp with time zone not null",
                g.Table + ".id uuid not null",
                g.Table + ".image_id uuid not null",
                g.Table + ".is_main boolean not null",
                g.Table + "." + g.OwnerColumn + " uuid not null",
                g.Table + ".sort_order integer not null",
            ]);
        }

        List<string> columns = await QueryAsync(
            "SELECT table_name || '.' || column_name || ' ' || data_type"
            + " || CASE WHEN character_maximum_length IS NOT NULL THEN '(' || character_maximum_length || ')' ELSE '' END"
            + " || CASE WHEN is_nullable = 'NO' THEN ' not null' ELSE '' END"
            + " FROM information_schema.columns WHERE table_schema = 'public' AND table_name = ANY(@names)",
            ("names", FiveTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), columns.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Columns_HaveTheDatabaseDefaultOfTheDefaultColumn()
    {
        // The "Varsayılan" column of 06 4.71: IsMain is false as a database default; no other column has a default.
        List<string> expected = FiveTables.Select(t => t + ".is_main false").ToList();

        List<string> defaults = await QueryAsync(
            "SELECT table_name || '.' || column_name || ' ' || column_default FROM information_schema.columns"
            + " WHERE table_schema = 'public' AND table_name = ANY(@names) AND column_default IS NOT NULL",
            ("names", FiveTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), defaults.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Indexes_AreTheDocumentedOnes()
    {
        // 06 4.71, 7.5.2: the pair-unique index (its first column serves the owner's gallery), the filtered unique index of the main image and
        // the image index of the restrictive link.
        List<string> expected = [];
        foreach (Gallery g in Galleries.Values)
        {
            expected.Add($"{g.Table}.ix_{g.Table}_image_id | CREATE INDEX ix_{g.Table}_image_id ON public.{g.Table} USING btree (image_id)");
            expected.Add($"{g.Table}.ix_{g.Table}_{g.OwnerColumn} | CREATE UNIQUE INDEX ix_{g.Table}_{g.OwnerColumn} ON public.{g.Table} USING btree ({g.OwnerColumn}) WHERE is_main");
            expected.Add($"{g.Table}.ix_{g.Table}_{g.OwnerColumn}_image_id | CREATE UNIQUE INDEX ix_{g.Table}_{g.OwnerColumn}_image_id ON public.{g.Table} USING btree ({g.OwnerColumn}, image_id)");
            expected.Add($"{g.Table}.pk_{g.Table} | CREATE UNIQUE INDEX pk_{g.Table} ON public.{g.Table} USING btree (id)");
        }

        List<string> indexes = await QueryAsync(
            "SELECT tablename || '.' || indexname || ' | ' || indexdef FROM pg_indexes WHERE schemaname = 'public' AND tablename = ANY(@names)",
            ("names", FiveTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), indexes.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ForeignKeys_CascadeWithTheOwner_AndAreRestrictiveToTheImage()
    {
        // 06 2.2.5, 4.71, 7.2: the owner link cascades (c); the image link is restrictive (a).
        List<string> expected = [];
        foreach (Gallery g in Galleries.Values)
        {
            expected.Add($"{g.Table} fk_{g.Table}_images_image_id a");
            expected.Add($"{g.Table} fk_{g.Table}_{g.OwnerTable}_{g.OwnerColumn} c");
        }

        List<string> links = await QueryAsync(
            "SELECT conrelid::regclass::text || ' ' || conname || ' ' || confdeltype::text FROM pg_constraint WHERE contype = 'f' AND conrelid = ANY(ARRAY(SELECT x::regclass FROM unnest(@oids) x))",
            ("oids", FiveTables.Select(t => "public." + t).ToArray()));

        Assert.Equal(expected.Order(StringComparer.Ordinal), links.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task CheckConstraints_AreNone()
    {
        // 06 4.71 has no check rule: the main mark is a filtered unique index, not a check.
        List<string> checks = await QueryAsync(
            "SELECT c.conname FROM pg_constraint c WHERE c.contype = 'c' AND c.conrelid = ANY(ARRAY(SELECT x::regclass FROM unnest(@oids) x))",
            ("oids", FiveTables.Select(t => "public." + t).ToArray()));

        Assert.Empty(checks);
    }

    [Fact]
    public async Task OnlyTheOwnerAndTheImageAndTheOrderAreRequired_AndTheDatabaseFillsIsMain()
    {
        foreach (Gallery g in Galleries.Values)
        {
            Guid owner = await SeedOwnerAsync(g);
            Guid image = await SeedImageAsync("minimal-" + g.Table);

            await ExecuteAsync(
                $"INSERT INTO {g.Table} (id, created_at, {g.OwnerColumn}, image_id, sort_order) VALUES (gen_random_uuid(), now(), @o, @i, 1)",
                ("o", owner),
                ("i", image));

            Assert.False(await ScalarAsync<bool>($"SELECT is_main FROM {g.Table} WHERE {g.OwnerColumn} = '{owner}'"));
        }
    }

    // ---- KK2: an image enters a gallery once ----

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task SecondLinkOfTheSameImageToAGallery_IsRefused(string table)
    {
        Gallery g = Galleries[table];
        Guid owner = await SeedOwnerAsync(g);
        Guid image = await SeedImageAsync("dup-" + table);
        await SaveAsync(c => c.Add(g.NewLink(owner, image, 1, true)));

        Assert.Equal(
            (UniqueViolation, $"ix_{table}_{g.OwnerColumn}_image_id"),
            await ViolationOfAsync(c => c.Add(g.NewLink(owner, image, 2, false))));
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task OneImage_OnTwoGalleries_AndTwoImagesOnOneGallery_AreAccepted(string table)
    {
        Gallery g = Galleries[table];
        Guid first = await SeedOwnerAsync(g);
        Guid? second = await SeedSecondOwnerAsync(g);
        Guid a = await SeedImageAsync("a-" + table);
        Guid b = await SeedImageAsync("b-" + table);

        await SaveAsync(c =>
        {
            c.Add(g.NewLink(first, a, 1, true));
            c.Add(g.NewLink(first, b, 2, false));
            if (second is not null)
            {
                c.Add(g.NewLink(second.Value, a, 1, true));
            }
        });

        Assert.Equal(second is null ? 2L : 3L, await ScalarAsync<long>($"SELECT count(*) FROM {table}"));
    }

    // ---- KK3: one main image per owner ----

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task SecondMainImageOfAnOwner_IsRefused(string table)
    {
        Gallery g = Galleries[table];
        Guid owner = await SeedOwnerAsync(g);
        Guid one = await SeedImageAsync("one-" + table);
        Guid two = await SeedImageAsync("two-" + table);
        await SaveAsync(c => c.Add(g.NewLink(owner, one, 1, true)));

        Assert.Equal(
            (UniqueViolation, $"ix_{table}_{g.OwnerColumn}"),
            await ViolationOfAsync(c => c.Add(g.NewLink(owner, two, 2, true))));
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task MainImage_OfTwoOwners_AndManyNonMainImagesOfOneOwner_AreAccepted(string table)
    {
        Gallery g = Galleries[table];
        Guid first = await SeedOwnerAsync(g);
        Guid? second = await SeedSecondOwnerAsync(g);
        Guid a = await SeedImageAsync("m-a-" + table);
        Guid b = await SeedImageAsync("m-b-" + table);
        Guid c3 = await SeedImageAsync("m-c-" + table);
        Guid d = await SeedImageAsync("m-d-" + table);

        await SaveAsync(c =>
        {
            c.Add(g.NewLink(first, a, 1, true));
            if (second is not null)
            {
                c.Add(g.NewLink(second.Value, b, 1, true));
            }

            c.Add(g.NewLink(first, c3, 2, false));
            c.Add(g.NewLink(first, d, 3, false));
        });

        Assert.Equal(second is null ? 3L : 4L, await ScalarAsync<long>($"SELECT count(*) FROM {table}"));
    }

    // ---- KK5: the manual order is not unique ----

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task SortOrder_SameValueInTwoRowsOfOneGallery_IsAccepted(string table)
    {
        Gallery g = Galleries[table];
        Guid owner = await SeedOwnerAsync(g);
        Guid one = await SeedImageAsync("o1-" + table);
        Guid two = await SeedImageAsync("o2-" + table);
        Guid three = await SeedImageAsync("o3-" + table);

        await SaveAsync(c =>
        {
            c.Add(g.NewLink(owner, one, 2, true));
            c.Add(g.NewLink(owner, two, 2, false));
            c.Add(g.NewLink(owner, three, 2, false));
        });

        Assert.Equal(3L, await ScalarAsync<long>($"SELECT count(*) FROM {table} WHERE sort_order = 2"));
    }

    // ---- KK4: the owner cascades, the image restricts ----

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task DeletingTheOwner_RemovesItsLinks_AndLeavesTheImagesAndOtherGalleries(string table)
    {
        Gallery g = Galleries[table];
        Guid removed = await SeedOwnerAsync(g);
        Guid? kept = await SeedSecondOwnerAsync(g);
        Guid a = await SeedImageAsync("c-a-" + table);
        Guid b = await SeedImageAsync("c-b-" + table);
        await SaveAsync(c =>
        {
            c.Add(g.NewLink(removed, a, 1, true));
            c.Add(g.NewLink(removed, b, 2, false));
            if (kept is not null)
            {
                c.Add(g.NewLink(kept.Value, a, 1, true));
            }
        });

        await ExecuteAsync($"DELETE FROM {g.OwnerTable} WHERE id = @id", ("id", removed));

        Assert.Equal(kept is null ? 0L : 1L, await ScalarAsync<long>($"SELECT count(*) FROM {table}"));
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM images"));
        Assert.Equal(kept is null ? 0L : 1L, await ScalarAsync<long>($"SELECT count(*) FROM {g.OwnerTable}"));
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task DeletingALinkedImage_IsRefused_AndAnUnlinkedImageIsDeletable(string table)
    {
        Gallery g = Galleries[table];
        Guid owner = await SeedOwnerAsync(g);
        Guid linked = await SeedImageAsync("l-" + table);
        Guid free = await SeedImageAsync("f-" + table);
        await SaveAsync(c => c.Add(g.NewLink(owner, linked, 1, true)));

        Assert.Equal(
            (ForeignKeyViolation, $"fk_{table}_images_image_id"),
            await ViolationOfSqlAsync("DELETE FROM images WHERE id = @id", ("id", linked)));

        await ExecuteAsync("DELETE FROM images WHERE id = @id", ("id", free));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM images"));

        // Once the link is removed (the removal from the gallery is an event), the image is deletable.
        await ExecuteAsync($"DELETE FROM {table} WHERE image_id = @id", ("id", linked));
        await ExecuteAsync("DELETE FROM images WHERE id = @id", ("id", linked));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM images"));
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task Link_OfAnUnknownOwnerOrImage_IsRefused(string table)
    {
        Gallery g = Galleries[table];
        Guid owner = await SeedOwnerAsync(g);
        Guid image = await SeedImageAsync("u-" + table);

        Assert.Equal(
            ForeignKeyViolation,
            (await ViolationOfAsync(c => c.Add(g.NewLink(owner, Guid.NewGuid(), 1, false)))).SqlState);
        Assert.Equal(
            ForeignKeyViolation,
            (await ViolationOfAsync(c => c.Add(g.NewLink(Guid.NewGuid(), image, 1, false)))).SqlState);
    }

    // ---- helpers ----

    private async Task<Guid> SeedOwnerAsync(Gallery gallery)
    {
        Guid id = Guid.Empty;
        await SaveAsync(c => id = gallery.AddOwner(c));
        return id;
    }

    /// <summary>A second owner of the same kind; the about page is one row at most (06 2.3.2), so it has none.</summary>
    private async Task<Guid?> SeedSecondOwnerAsync(Gallery gallery) => gallery.Table == "about_page_images" ? null : await SeedOwnerAsync(gallery);

    private async Task<Guid> SeedImageAsync(string storageKey)
    {
        Image image = Image.Create(storageKey, ImageFormat.Png, 100, 100, Now);
        await SaveAsync(c => c.Images.Add(image));
        return image.Id;
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

    /// <summary>One gallery link table: its owner's table and column, the way an owner is added and the way a link is made.</summary>
    private sealed record Gallery(string Table, string OwnerTable, string OwnerColumn, Func<AppDbContext, Guid> AddOwner, Func<Guid, Guid, int, bool, object> LinkFactory)
    {
        public object NewLink(Guid owner, Guid image, int order, bool isMain) => LinkFactory(owner, image, order, isMain);
    }
}
