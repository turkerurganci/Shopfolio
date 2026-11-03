using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.UnitTests.Samples;
using Shopfolio.UnitTests.Samples.Good;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>
/// The data model rules on a real PostgreSQL (T101 KK2, KK3, KK5, KK7, KK8; 09 5.2 item 5, 10.1 item 5): the sample tables the model
/// produces are read back from the catalog, and the sample records are written and read through the one context.
/// </summary>
public sealed class SampleDatabaseTests : IClassFixture<SampleDatabaseFixture>
{
    private static readonly DateTimeOffset Now = new(2026, 11, 3, 9, 0, 0, TimeSpan.Zero);

    private readonly SampleDatabaseFixture _database;

    public SampleDatabaseTests(SampleDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task SampleRecord_AfterSave_HasAVersion7Key_AndTheDatabaseMadeNone()
    {
        SampleOwner owner = SampleOwner.Create("KEY-" + EntityIds.NewStorageKey(), Now);
        await using (AppDbContext context = SampleModel.CreateGoodContext(_database.Dsn))
        {
            context.Set<SampleOwner>().Add(owner);
            await context.SaveChangesAsync(Ct);
        }

        int? version = await _database.ScalarAsync<int>("SELECT uuid_extract_version(id) FROM sample_owners WHERE id = @id", Ct, ("id", owner.Id));
        string? idDefault = await _database.ScalarAsync<string>(
            "SELECT column_default FROM information_schema.columns WHERE table_name = 'sample_owners' AND column_name = 'id'", Ct);

        Assert.Equal(7, version);
        Assert.Null(idDefault);
    }

    [Fact]
    public async Task Catalog_NamesAreDerivedFromTheRule_AndFitThePatternAndTheLength()
    {
        string[] names = await ReadColumnAsync(
            """
            SELECT conname FROM pg_constraint WHERE conrelid IN ('sample_owners'::regclass, 'sample_items'::regclass)
            UNION ALL SELECT indexname FROM pg_indexes WHERE tablename IN ('sample_owners', 'sample_items')
            UNION ALL SELECT column_name FROM information_schema.columns WHERE table_name IN ('sample_owners', 'sample_items')
            UNION ALL SELECT table_name FROM information_schema.tables WHERE table_name IN ('sample_owners', 'sample_items')
            """);

        Assert.Contains("pk_sample_owners", names);
        Assert.Contains("ix_sample_owners_normalized_code", names);
        Assert.Contains("ck_sample_owners_price_not_negative", names);
        Assert.Contains(names, n => n.StartsWith("fk_sample_items_", StringComparison.Ordinal));
        Assert.All(names, n => Assert.Matches("^[a-z][a-z0-9_]*$", n));
        Assert.All(names, n => Assert.True(System.Text.Encoding.UTF8.GetByteCount(n) <= 63));
    }

    [Fact]
    public async Task CheckRule_RefusesAValueThatBreaksIt()
    {
        SampleOwner owner = SampleOwner.Create("CHK-" + EntityIds.NewStorageKey(), Now);
        owner.Price = -1m;
        await using AppDbContext context = SampleModel.CreateGoodContext(_database.Dsn);
        context.Set<SampleOwner>().Add(owner);

        DbUpdateException error = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(Ct));

        PostgresException inner = Assert.IsType<PostgresException>(error.InnerException);
        Assert.Equal("ck_sample_owners_price_not_negative", inner.ConstraintName);
    }

    [Fact]
    public async Task ForeignKeys_InTheCatalog_AreCascadeSetNullOrNoActionAndNeverRestrict()
    {
        string[] actions = await ReadColumnAsync(
            "SELECT a.attname || ':' || c.confdeltype::text FROM pg_constraint c JOIN pg_attribute a ON a.attrelid = c.conrelid AND a.attnum = c.conkey[1] "
            + "WHERE c.contype = 'f' AND c.conrelid = 'sample_items'::regclass ORDER BY 1");

        Assert.Equal(["emptied_owner_id:n", "owner_id:c", "restrictive_owner_id:a", "unwritten_owner_id:a"], actions);
    }

    [Fact]
    public async Task DeletingAnOwner_FollowsTheDeleteBehaviorOfEachRelationship()
    {
        (SampleOwner cascading, SampleOwner emptying, SampleOwner restrictive, SampleOwner unwritten, SampleOwner holder, SampleItem item) = await SaveItemWithFourOwnersAsync();

        await _database.ExecuteAsync("DELETE FROM sample_owners WHERE id = @id", Ct, ("id", emptying.Id));
        Guid? emptied = await _database.ScalarAsync<Guid?>("SELECT emptied_owner_id FROM sample_items WHERE id = @id", Ct, ("id", item.Id));
        Assert.Null(emptied);

        foreach (SampleOwner blocked in new[] { restrictive, unwritten })
        {
            PostgresException error = await Assert.ThrowsAsync<PostgresException>(
                () => _database.ExecuteAsync("DELETE FROM sample_owners WHERE id = @id", Ct, ("id", blocked.Id)));
            Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, error.SqlState);
        }

        await _database.ExecuteAsync("DELETE FROM sample_owners WHERE id = @id", Ct, ("id", cascading.Id));
        long left = await _database.ScalarAsync<long>("SELECT count(*) FROM sample_items WHERE id = @id", Ct, ("id", item.Id));
        Assert.Equal(0, left);
        Assert.NotEqual(Guid.Empty, holder.Id);
    }

    [Fact]
    public async Task Update_WithAnOlderVersion_AffectsNoRow()
    {
        SampleOwner owner = SampleOwner.Create("VER-" + EntityIds.NewStorageKey(), Now);
        await using (AppDbContext create = SampleModel.CreateGoodContext(_database.Dsn))
        {
            create.Set<SampleOwner>().Add(owner);
            await create.SaveChangesAsync(Ct);
        }

        await using AppDbContext first = SampleModel.CreateGoodContext(_database.Dsn);
        await using AppDbContext second = SampleModel.CreateGoodContext(_database.Dsn);
        SampleOwner loadedByFirst = await first.Set<SampleOwner>().SingleAsync(o => o.Id == owner.Id, Ct);
        SampleOwner loadedBySecond = await second.Set<SampleOwner>().SingleAsync(o => o.Id == owner.Id, Ct);

        loadedByFirst.Name = "First";
        loadedByFirst.BumpVersion();
        await first.SaveChangesAsync(Ct);

        loadedBySecond.Name = "Second";
        loadedBySecond.BumpVersion();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync(Ct));

        string? name = await _database.ScalarAsync<string>("SELECT name FROM sample_owners WHERE id = @id", Ct, ("id", owner.Id));
        int? version = await _database.ScalarAsync<int>("SELECT version FROM sample_owners WHERE id = @id", Ct, ("id", owner.Id));
        Assert.Equal("First", name);
        Assert.Equal(2, version);
    }

    [Fact]
    public async Task EmptyText_OptionalColumnGetsNull_RequiredColumnIsRefused()
    {
        SampleOwner optional = SampleOwner.Create("EMP-" + EntityIds.NewStorageKey(), Now);
        optional.Note = string.Empty;
        await using (AppDbContext context = SampleModel.CreateGoodContext(_database.Dsn))
        {
            context.Set<SampleOwner>().Add(optional);
            await context.SaveChangesAsync(Ct);
        }

        bool isNull = await _database.ScalarAsync<bool>("SELECT note IS NULL FROM sample_owners WHERE id = @id", Ct, ("id", optional.Id));
        Assert.True(isNull);

        SampleOwner required = SampleOwner.Create("EMR-" + EntityIds.NewStorageKey(), Now);
        required.Name = string.Empty;
        await using AppDbContext refusing = SampleModel.CreateGoodContext(_database.Dsn);
        refusing.Set<SampleOwner>().Add(required);
        await Assert.ThrowsAnyAsync<Exception>(() => refusing.SaveChangesAsync(Ct));
        long written = await _database.ScalarAsync<long>("SELECT count(*) FROM sample_owners WHERE id = @id", Ct, ("id", required.Id));
        Assert.Equal(0, written);
    }

    [Fact]
    public async Task NewRecordWithoutAKey_IsRefusedBeforeSql_AndNothingIsWritten()
    {
        long before = await _database.ScalarAsync<long>("SELECT count(*) FROM sample_owners", Ct);
        await using AppDbContext context = SampleModel.CreateGoodContext(_database.Dsn);
        context.Set<SampleOwner>().Add(new SampleOwner { CreatedAt = Now, Code = "NOKEY", NormalizedCode = "nokey" });

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));

        long after = await _database.ScalarAsync<long>("SELECT count(*) FROM sample_owners", Ct);
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task IdentityColumn_UniqueIndexFindsTheNormalizedForm()
    {
        string unique = EntityIds.NewStorageKey();
        SampleOwner first = SampleOwner.Create("İSTANBUL-" + unique, Now);
        await using (AppDbContext context = SampleModel.CreateGoodContext(_database.Dsn))
        {
            context.Set<SampleOwner>().Add(first);
            await context.SaveChangesAsync(Ct);
        }

        SampleOwner second = SampleOwner.Create("ISTANBUL-" + unique.ToUpperInvariant(), Now);
        await using AppDbContext duplicate = SampleModel.CreateGoodContext(_database.Dsn);
        duplicate.Set<SampleOwner>().Add(second);

        DbUpdateException error = await Assert.ThrowsAsync<DbUpdateException>(() => duplicate.SaveChangesAsync(Ct));

        Assert.Equal("ix_sample_owners_normalized_code", Assert.IsType<PostgresException>(error.InnerException).ConstraintName);
    }

    [Fact]
    public async Task ColumnTypes_InTheCatalog_AreTheTypesOfTheTable()
    {
        string[] columns = await ReadColumnAsync(
            "SELECT column_name || ':' || pg_catalog.format_type(a.atttypid, a.atttypmod) FROM information_schema.columns c "
            + "JOIN pg_attribute a ON a.attrelid = 'sample_owners'::regclass AND a.attname = c.column_name WHERE c.table_name = 'sample_owners' ORDER BY 1");

        Assert.Contains("id:uuid", columns);
        Assert.Contains("created_at:timestamp with time zone", columns);
        Assert.Contains("start_date:date", columns);
        Assert.Contains("price:numeric(12,2)", columns);
        Assert.Contains("vat_rate:numeric(5,2)", columns);
        Assert.Contains("net_quantity:numeric(12,3)", columns);
        Assert.Contains("count:integer", columns);
        Assert.Contains("byte_size:bigint", columns);
        Assert.Contains("is_active:boolean", columns);
        Assert.Contains("client_address:inet", columns);
        Assert.Contains("payload:jsonb", columns);
        Assert.Contains("long_text:text", columns);
        Assert.Contains("code:character varying(50)", columns);
    }

    private async Task<(SampleOwner Cascading, SampleOwner Emptying, SampleOwner Restrictive, SampleOwner Unwritten, SampleOwner Holder, SampleItem Item)> SaveItemWithFourOwnersAsync()
    {
        string suffix = EntityIds.NewStorageKey();
        SampleOwner[] owners = Enumerable.Range(0, 4).Select(i => SampleOwner.Create($"OWN{i}-{suffix}", Now)).ToArray();
        SampleItem item = SampleItem.Create(owners[0].Id, owners[1].Id, owners[2].Id, owners[3].Id, Now);
        await using AppDbContext context = SampleModel.CreateGoodContext(_database.Dsn);
        context.Set<SampleOwner>().AddRange(owners);
        context.Set<SampleItem>().Add(item);
        await context.SaveChangesAsync(Ct);
        return (owners[0], owners[1], owners[2], owners[3], owners[0], item);
    }

    private async Task<string[]> ReadColumnAsync(string sql)
    {
        List<string> values = [];
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(Ct);
        while (await reader.ReadAsync(Ct))
        {
            values.Add(reader.GetString(0));
        }

        return values.ToArray();
    }
}
