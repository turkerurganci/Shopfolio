using System.Text.RegularExpressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.UnitTests.Samples;
using Shopfolio.UnitTests.Samples.Good;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>
/// The enum rules on a real PostgreSQL (T102 KK2, KK3, KK4; 09 10.1 items 3 and 5, 12 item 8): an enum is stored as its code name, the
/// column carries the check of the allowed names, a subset field refuses a member outside it, and the list of the constraint equals the
/// members of the enum in both directions.
/// </summary>
public sealed partial class EnumDatabaseTests : IClassFixture<SampleDatabaseFixture>
{
    private static readonly DateTimeOffset Now = new(2026, 11, 3, 9, 0, 0, TimeSpan.Zero);

    private readonly SampleDatabaseFixture _database;

    public EnumDatabaseTests(SampleDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task EnumRecord_AfterSave_ShowsTheCodeNamesAsTextInTheDatabase()
    {
        SampleEnumRecord record = SampleEnumRecord.Create(Now);
        record.Status = PublishStatus.Archived;
        record.Method = PaymentMethod.BankTransfer;
        record.Transition = TransitionCode.S10;
        record.Dispatch = DispatchType.B16;
        await using (AppDbContext context = SampleModel.CreateGoodContext(_database.Dsn))
        {
            context.Set<SampleEnumRecord>().Add(record);
            await context.SaveChangesAsync(Ct);
        }

        string? status = await _database.ScalarAsync<string>("SELECT status FROM sample_enum_records WHERE id = @id", Ct, ("id", record.Id));
        string? method = await _database.ScalarAsync<string>("SELECT method FROM sample_enum_records WHERE id = @id", Ct, ("id", record.Id));
        string? transition = await _database.ScalarAsync<string>("SELECT transition FROM sample_enum_records WHERE id = @id", Ct, ("id", record.Id));
        string? dispatch = await _database.ScalarAsync<string>("SELECT dispatch FROM sample_enum_records WHERE id = @id", Ct, ("id", record.Id));
        string? type = await _database.ScalarAsync<string>(
            "SELECT data_type FROM information_schema.columns WHERE table_name = 'sample_enum_records' AND column_name = 'status'", Ct);

        Assert.Equal("Archived", status);
        Assert.Equal("BankTransfer", method);
        Assert.Equal("S10", transition);
        Assert.Equal("B16", dispatch);
        Assert.Equal("text", type);
    }

    [Fact]
    public async Task EnumRecord_ReadBack_HasTheSameValues_AndAnOptionalEnumStaysNull()
    {
        SampleEnumRecord record = SampleEnumRecord.Create(Now);
        record.Transition = TransitionCode.Correction;
        await using (AppDbContext context = SampleModel.CreateGoodContext(_database.Dsn))
        {
            context.Set<SampleEnumRecord>().Add(record);
            await context.SaveChangesAsync(Ct);
        }

        await using AppDbContext read = SampleModel.CreateGoodContext(_database.Dsn);
        SampleEnumRecord loaded = await read.Set<SampleEnumRecord>().SingleAsync(r => r.Id == record.Id, Ct);

        Assert.Equal(TransitionCode.Correction, loaded.Transition);
        Assert.Equal(PublishStatus.Archived, loaded.Status);
        Assert.Null(loaded.Method);
    }

    [Fact]
    public async Task QueryByAnEnumValue_FiltersOnTheStoredText()
    {
        SampleEnumRecord record = SampleEnumRecord.Create(Now);
        record.Dispatch = DispatchType.F6;
        await using (AppDbContext context = SampleModel.CreateGoodContext(_database.Dsn))
        {
            context.Set<SampleEnumRecord>().Add(record);
            await context.SaveChangesAsync(Ct);
        }

        await using AppDbContext read = SampleModel.CreateGoodContext(_database.Dsn);
        bool found = await read.Set<SampleEnumRecord>().AnyAsync(r => r.Id == record.Id && r.Dispatch == DispatchType.F6, Ct);

        Assert.True(found);
    }

    [Theory]
    [InlineData("status", "Bogus", "ck_sample_enum_records_status")]
    [InlineData("status", "archived", "ck_sample_enum_records_status")]
    [InlineData("status", "1", "ck_sample_enum_records_status")]
    [InlineData("method", "Cash", "ck_sample_enum_records_method")]
    [InlineData("transition", "S12", "ck_sample_enum_records_transition")]
    public async Task RawInsert_OfATextOutsideTheAllowedNames_BreaksTheCheckConstraint(string column, string text, string constraint)
    {
        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => InsertRawAsync(column, text));

        Assert.Equal(PostgresErrorCodes.CheckViolation, error.SqlState);
        Assert.Equal(constraint, error.ConstraintName);
    }

    [Fact]
    public async Task SubsetField_RefusesAMemberOutsideItsSubset_AndAcceptsTheMembersInside()
    {
        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => InsertRawAsync("subset_status", "Archived"));
        await InsertRawAsync("subset_status", "Published");

        Assert.Equal(PostgresErrorCodes.CheckViolation, error.SqlState);
        Assert.Equal("ck_sample_enum_records_subset_status", error.ConstraintName);
    }

    [Fact]
    public async Task SubsetField_WhenTheCodeSavesAMemberOutsideItsSubset_IsRefusedByTheDatabase()
    {
        SampleEnumRecord record = SampleEnumRecord.Create(Now);
        record.SubsetStatus = PublishStatus.Archived;
        await using AppDbContext context = SampleModel.CreateGoodContext(_database.Dsn);
        context.Set<SampleEnumRecord>().Add(record);

        DbUpdateException error = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(Ct));

        Assert.Equal("ck_sample_enum_records_subset_status", Assert.IsType<PostgresException>(error.InnerException).ConstraintName);
    }

    [Fact]
    public async Task RawInsert_OfAnUnknownText_IsFailedOnReadByTheConversion()
    {
        Guid id = EntityIds.New();
        await using AppDbContext context = SampleModel.CreateGoodContext(_database.Dsn);
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(Ct);
        await context.Database.ExecuteSqlRawAsync("ALTER TABLE sample_enum_records DROP CONSTRAINT ck_sample_enum_records_status", Ct);
        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO sample_enum_records (id, created_at, status, subset_status, transition, dispatch) "
            + "VALUES ({0}, {1}, 'Bogus', 'Draft', 'O1', 'B1')",
            [id, Now],
            Ct);

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Set<SampleEnumRecord>().SingleAsync(r => r.Id == id, Ct));

        Assert.Contains("'Bogus'", error.ToString(), StringComparison.Ordinal);
        await transaction.RollbackAsync(Ct);
    }

    [Fact]
    public async Task ConstraintLists_AreEqualToTheEnumMembersInBothDirections()
    {
        await using AppDbContext context = SampleModel.CreateGoodContext(_database.Dsn);
        IModel model = context.GetService<IDesignTimeModel>().Model;
        int compared = 0;
        foreach (IEntityType entityType in model.GetEntityTypes())
        {
            string table = entityType.GetTableName()!;
            StoreObjectIdentifier store = StoreObjectIdentifier.Table(table, entityType.GetSchema());
            foreach (IProperty property in entityType.GetProperties())
            {
                Type type = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                if (!type.IsEnum)
                {
                    continue;
                }

                string column = property.GetColumnName(store)!;
                string[] expected = property.FindAnnotation("Shopfolio:AllowedValues")?.Value as string[] ?? Enum.GetNames(type);
                string? definition = await _database.ScalarAsync<string>(
                    "SELECT pg_get_constraintdef(oid) FROM pg_constraint WHERE conname = @name AND contype = 'c'", Ct, ("name", $"ck_{table}_{column}"));

                Assert.NotNull(definition);
                string[] listed = QuotedName().Matches(definition).Select(m => m.Groups[1].Value).ToArray();
                Assert.Equal(expected.Order(StringComparer.Ordinal), listed.Order(StringComparer.Ordinal));
                compared++;
            }
        }

        // Five enum columns of the sample entities and the three of the application's own tables (companies.company_type,
        // company_settings.homepage_layout, legal_text_versions.legal_text_type; T103) and six of the catalog core tables (T104). The task that adds an enum column raises this.
        Assert.Equal(14, compared);
    }

    [Fact]
    public async Task EveryMemberOfAnEnum_IsWrittenAndReadBackWithTheMemberNameAsTheRawValue()
    {
        foreach (TransitionCode code in Enum.GetValues<TransitionCode>())
        {
            SampleEnumRecord record = SampleEnumRecord.Create(Now);
            record.Transition = code;
            await using (AppDbContext context = SampleModel.CreateGoodContext(_database.Dsn))
            {
                context.Set<SampleEnumRecord>().Add(record);
                await context.SaveChangesAsync(Ct);
            }

            string? raw = await _database.ScalarAsync<string>("SELECT transition FROM sample_enum_records WHERE id = @id", Ct, ("id", record.Id));
            await using AppDbContext read = SampleModel.CreateGoodContext(_database.Dsn);
            SampleEnumRecord loaded = await read.Set<SampleEnumRecord>().SingleAsync(r => r.Id == record.Id, Ct);

            Assert.Equal(code.ToString(), raw);
            Assert.Equal(code, loaded.Transition);
        }
    }

    private Task InsertRawAsync(string column, string text)
    {
        Dictionary<string, string> values = new(StringComparer.Ordinal)
        {
            ["status"] = "Draft",
            ["subset_status"] = "Draft",
            ["method"] = "Card",
            ["transition"] = "O1",
        };
        values[column] = text;
        return _database.ExecuteAsync(
            "INSERT INTO sample_enum_records (id, created_at, status, subset_status, method, transition, dispatch) "
            + "VALUES (@id, @at, @status, @subset, @method, @transition, 'B1')",
            Ct,
            ("id", EntityIds.New()),
            ("at", Now),
            ("status", values["status"]),
            ("subset", values["subset_status"]),
            ("method", values["method"]),
            ("transition", values["transition"]));
    }

    [GeneratedRegex("'([A-Za-z0-9]+)'")]
    private static partial Regex QuotedName();
}
