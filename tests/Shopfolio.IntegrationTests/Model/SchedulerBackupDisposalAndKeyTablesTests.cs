using System.Globalization;
using System.Reflection;

using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Backup;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Migrations;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the scheduler, backup, disposal and key ring tests (T118): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(SchedulerBackupDisposalAndKeyTablesDefinition.Name)]
public sealed class SchedulerBackupDisposalAndKeyTablesDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "SchedulerBackupDisposalAndKeyTables";
}

/// <summary>
/// The job, the disposal record, the heartbeat and outage records, the backup status record and the data protection key on a real PostgreSQL
/// (T118 criteria 1-8; 06 4.59, 4.64-4.68, 2.3.2, 7.1-7.5; 09 10.6, 10.8 item 3): the migration made the six tables; the database itself refuses
/// a second pending job of a type for one record (the empty related record counting as equal), a related record that does not fit the type, a
/// completion moment that does not fit the state, a negative attempt count, a second heartbeat or backup status row, an outage of two minutes or
/// less, a disposal row whose number and dates do not fit its type and a range that ends before it starts; the key ring is the package's schema,
/// closed to the read-only user; the backup status user updates only the two moments; and a missing role setting rolls the migration back.
/// </summary>
[Collection(SchedulerBackupDisposalAndKeyTablesDefinition.Name)]
public sealed class SchedulerBackupDisposalAndKeyTablesTests : IAsyncLifetime
{
    private const string CheckViolation = PostgresErrorCodes.CheckViolation;
    private const string UniqueViolation = PostgresErrorCodes.UniqueViolation;
    private const string InsufficientPrivilege = PostgresErrorCodes.InsufficientPrivilege;

    /// <summary>The statements of the database container's backup job (deploy/images/database/database-job.sh), as it writes the result.</summary>
    private const string SucceededStatement = "UPDATE public.backup_status_records SET last_succeeded_at = now()";

    private const string FailedStatement = "UPDATE public.backup_status_records SET last_failed_at = now()";

    private static readonly string[] SixTables = ["backup_status_records", "data_protection_keys", "disposal_records", "heartbeat_records", "jobs", "outage_records"];

    private static readonly DateTimeOffset Now = new(2026, 11, 4, 12, 0, 0, TimeSpan.Zero);

    private readonly TemplateDatabaseFixture _template;
    private readonly CollectionDatabaseFixture _collection;

    public SchedulerBackupDisposalAndKeyTablesTests(TemplateDatabaseFixture template, CollectionDatabaseFixture collection)
    {
        _template = template;
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private TestDatabaseUsers Users => _template.Users;

    private string AppDsn => _template.DsnFor(_collection.DatabaseName, Users.App, Users.AppPassword, pooling: false);

    private string ReaderDsn => _template.DsnFor(_collection.DatabaseName, Users.Reader, Users.ReaderPassword, pooling: false);

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(_collection.Dsn, "TRUNCATE jobs, disposal_records, heartbeat_records, outage_records, backup_status_records, data_protection_keys");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the migration ----

    [Fact]
    public async Task Migration_MakesTheSixTables()
    {
        List<string> tables = await QueryAsync(
            _collection.Dsn,
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name = ANY(@names)",
            ("names", SixTables));

        Assert.Equal(SixTables, tables.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Columns_HaveTheTypesAndNullabilityOfTheFieldTables()
    {
        // 06 4.59, 4.64-4.68 (the common Id and CreatedAt of the 4 introduction included, except on the key ring: 06 4.68); the enum columns are text.
        string[] expected =
        [
            "backup_status_records.created_at timestamp with time zone not null",
            "backup_status_records.id uuid not null",
            "backup_status_records.last_failed_at timestamp with time zone",
            "backup_status_records.last_succeeded_at timestamp with time zone",
            "data_protection_keys.friendly_name text",
            "data_protection_keys.id integer not null",
            "data_protection_keys.xml text",
            "disposal_records.count integer",
            "disposal_records.created_at timestamp with time zone not null",
            "disposal_records.disposal_record_type text not null",
            "disposal_records.id uuid not null",
            "disposal_records.range_end_date date",
            "disposal_records.range_start_date date",
            "heartbeat_records.created_at timestamp with time zone not null",
            "heartbeat_records.id uuid not null",
            "heartbeat_records.last_heartbeat_at timestamp with time zone",
            "heartbeat_records.nightly_job_last_attempted_at timestamp with time zone",
            "heartbeat_records.nightly_job_last_succeeded_at timestamp with time zone",
            "jobs.attempt_count integer not null",
            "jobs.completed_at timestamp with time zone",
            "jobs.created_at timestamp with time zone not null",
            "jobs.id uuid not null",
            "jobs.job_state text not null",
            "jobs.job_type text not null",
            "jobs.last_error_summary text",
            "jobs.next_attempt_at timestamp with time zone",
            "jobs.related_record_id uuid",
            "outage_records.created_at timestamp with time zone not null",
            "outage_records.ended_at timestamp with time zone not null",
            "outage_records.id uuid not null",
            "outage_records.started_at timestamp with time zone not null",
        ];

        List<string> columns = await QueryAsync(
            _collection.Dsn,
            """
            SELECT c.relname || '.' || a.attname || ' ' || format_type(a.atttypid, a.atttypmod) || CASE WHEN a.attnotnull THEN ' not null' ELSE '' END
            FROM pg_attribute a JOIN pg_class c ON c.oid = a.attrelid JOIN pg_namespace n ON n.oid = c.relnamespace
            WHERE n.nspname = 'public' AND c.relname = ANY(@names) AND a.attnum > 0 AND NOT a.attisdropped
            """,
            ("names", SixTables));

        Assert.Equal(expected, columns.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task DataProtectionKeys_KeyIsAnIntegerTheDatabaseGenerates_AndThereIsNoCreatedAt()
    {
        // 06 4.68: the package's key is an integer the database makes (an identity column), and the table has no creation time (06 2.8.4's one exception).
        List<string> key = await QueryAsync(
            _collection.Dsn,
            "SELECT format_type(atttypid, atttypmod) || ' ' || attidentity::text FROM pg_attribute WHERE attrelid = 'public.data_protection_keys'::regclass AND attname = 'id'");
        long createdAt = await ScalarAsync<long>(
            "SELECT count(*) FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'data_protection_keys' AND column_name = 'created_at'");

        Assert.Equal("integer d", Assert.Single(key));
        Assert.Equal(0L, createdAt);
    }

    [Fact]
    public void DataProtectionKey_InTheModel_IsThePackagesEntityUnderTheConventionsNames()
    {
        // 09 5.2 item 4 (bilgim tarihli, checked here by the task that binds the package): the package's context interface names the set
        // DataProtectionKeys and the naming convention makes data_protection_keys, friendly_name and xml with no name given by hand.
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        using AppDbContext context = new(builder.Options);
        IEntityType entity = ModelRules.ModelOf(context).FindEntityType(typeof(DataProtectionKey))!; // '!': the context implements the package's interface.
        StoreObjectIdentifier table = StoreObjectIdentifier.Table("data_protection_keys", null);

        Assert.IsAssignableFrom<IDataProtectionKeyContext>(context);
        Assert.Equal("data_protection_keys", entity.GetTableName());
        Assert.Equal(["friendly_name", "id", "xml"], entity.GetProperties().Select(p => p.GetColumnName(table)!).Order(StringComparer.Ordinal));
        Assert.Equal(typeof(int), Assert.Single(entity.FindPrimaryKey()!.Properties).ClrType);
    }

    [Fact]
    public async Task Columns_HaveTheDatabaseDefaultsOfTheDefaultColumns()
    {
        // The "Varsayılan" column: the job's state is Pending and its attempt count 0 — as database defaults; no other column has one (the key
        // ring's identity is no column default).
        string[] expected = ["jobs.attempt_count 0", "jobs.job_state 'Pending'::text"];

        List<string> defaults = await QueryAsync(
            _collection.Dsn,
            "SELECT table_name || '.' || column_name || ' ' || column_default FROM information_schema.columns"
            + " WHERE table_schema = 'public' AND table_name = ANY(@names) AND column_default IS NOT NULL",
            ("names", SixTables));

        Assert.Equal(expected, defaults.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Indexes_AreTheDocumentedOnes()
    {
        // 06 4.59 and 7.1, 7.5 (the filtered unique index with equal empty values, the queue scan, the failed rows, the clean-up), 4.64 (CreatedAt),
        // 4.65 and 4.67 (the single-row index only), 4.66 and 4.68 (the primary key only, 7.5.2).
        string[] expected =
        [
            "backup_status_records.ix_backup_status_records_single_row | CREATE UNIQUE INDEX ix_backup_status_records_single_row ON public.backup_status_records USING btree ((true))",
            "backup_status_records.pk_backup_status_records | CREATE UNIQUE INDEX pk_backup_status_records ON public.backup_status_records USING btree (id)",
            "data_protection_keys.pk_data_protection_keys | CREATE UNIQUE INDEX pk_data_protection_keys ON public.data_protection_keys USING btree (id)",
            "disposal_records.ix_disposal_records_created_at | CREATE INDEX ix_disposal_records_created_at ON public.disposal_records USING btree (created_at)",
            "disposal_records.pk_disposal_records | CREATE UNIQUE INDEX pk_disposal_records ON public.disposal_records USING btree (id)",
            "heartbeat_records.ix_heartbeat_records_single_row | CREATE UNIQUE INDEX ix_heartbeat_records_single_row ON public.heartbeat_records USING btree ((true))",
            "heartbeat_records.pk_heartbeat_records | CREATE UNIQUE INDEX pk_heartbeat_records ON public.heartbeat_records USING btree (id)",
            "jobs.ix_jobs_completed_at | CREATE INDEX ix_jobs_completed_at ON public.jobs USING btree (completed_at) WHERE (job_state = 'Completed'::text)",
            "jobs.ix_jobs_job_state | CREATE INDEX ix_jobs_job_state ON public.jobs USING btree (job_state) WHERE (job_state = 'Failed'::text)",
            "jobs.ix_jobs_job_state_next_attempt_at | CREATE INDEX ix_jobs_job_state_next_attempt_at ON public.jobs USING btree (job_state, next_attempt_at) WHERE (job_state = 'Pending'::text)",
            "jobs.ix_jobs_job_type_related_record_id | CREATE UNIQUE INDEX ix_jobs_job_type_related_record_id ON public.jobs USING btree (job_type, related_record_id) NULLS NOT DISTINCT WHERE (job_state = 'Pending'::text)",
            "jobs.pk_jobs | CREATE UNIQUE INDEX pk_jobs ON public.jobs USING btree (id)",
            "outage_records.pk_outage_records | CREATE UNIQUE INDEX pk_outage_records ON public.outage_records USING btree (id)",
        ];

        List<string> indexes = await QueryAsync(
            _collection.Dsn,
            "SELECT tablename || '.' || indexname || ' | ' || indexdef FROM pg_indexes WHERE schemaname = 'public' AND tablename = ANY(@names)",
            ("names", SixTables));

        Assert.Equal(expected, indexes.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task SixTables_HaveNoForeignKey()
    {
        // 06 4.59 (the related record is a value, 2.2.6), 4.64-4.68: no table of the six has a link.
        List<string> links = await QueryAsync(
            _collection.Dsn,
            "SELECT conrelid::regclass::text || ' ' || conname FROM pg_constraint WHERE contype = 'f' AND (conrelid = ANY(ARRAY(SELECT x::regclass FROM unnest(@oids) x)) OR confrelid = ANY(ARRAY(SELECT x::regclass FROM unnest(@oids) x)))",
            ("oids", SixTables.Select(t => "public." + t).ToArray()));

        Assert.Empty(links);
    }

    [Fact]
    public async Task CheckConstraints_AreTheDocumentedOnes()
    {
        // 06 7.3 and the enum columns' allowed values (06 2.9.2); the key ring has none (06 4.68), nor have the two single-row records.
        string[] expected =
        [
            "disposal_records.ck_disposal_records_count_and_range_by_type",
            "disposal_records.ck_disposal_records_disposal_record_type",
            "disposal_records.ck_disposal_records_range_end_not_before_start",
            "jobs.ck_jobs_attempt_count_not_negative",
            "jobs.ck_jobs_completed_at_with_state",
            "jobs.ck_jobs_job_state",
            "jobs.ck_jobs_job_type",
            "jobs.ck_jobs_related_record_by_type",
            "outage_records.ck_outage_records_longer_than_two_minutes",
        ];

        List<string> checks = await QueryAsync(
            _collection.Dsn,
            "SELECT conrelid::regclass::text || '.' || conname FROM pg_constraint WHERE contype = 'c' AND conrelid = ANY(ARRAY(SELECT x::regclass FROM unnest(@oids) x))",
            ("oids", SixTables.Select(t => "public." + t).ToArray()));

        Assert.Equal(expected, checks.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task NewJob_WithoutStateAndCount_TakesTheDatabaseDefaults()
    {
        await ExecuteAsync(_collection.Dsn, $"INSERT INTO jobs (id, created_at, job_type, related_record_id) VALUES ('{EntityIds.New()}', now(), 'CardRefund', '{EntityIds.New()}')");

        Assert.Equal("Pending 0", Assert.Single(await QueryAsync(_collection.Dsn, "SELECT job_state || ' ' || attempt_count FROM jobs")));
    }

    // ---- KK2: one pending job of a type per record ----

    [Fact]
    public async Task SecondPendingJob_OfTheSameTypeForTheSameRecord_IsRefused()
    {
        Guid record = EntityIds.New();
        await SaveAsync(c => c.Jobs.Add(Job.Enqueue(JobType.CardRefund, record, null, Now)));

        Assert.Equal(
            (UniqueViolation, "ix_jobs_job_type_related_record_id"),
            await ViolationOfAsync(c => c.Jobs.Add(Job.Enqueue(JobType.CardRefund, record, Now.AddMinutes(5), Now))));
    }

    [Fact]
    public async Task SecondPendingBounceQuery_WithoutRelatedRecord_IsRefused_BecauseEmptyValuesCountAsEqual()
    {
        await SaveAsync(c => c.Jobs.Add(Job.Enqueue(JobType.BounceQuery, null, null, Now)));

        Assert.Equal(
            (UniqueViolation, "ix_jobs_job_type_related_record_id"),
            await ViolationOfAsync(c => c.Jobs.Add(Job.Enqueue(JobType.BounceQuery, null, null, Now))));
    }

    [Fact]
    public async Task PendingJob_NextToACompletedAndAFailedOneOfTheSameRecord_OrOfAnotherType_IsAccepted()
    {
        Guid record = EntityIds.New();
        Job completed = Job.Enqueue(JobType.FinalPaymentQuery, record, null, Now);
        completed.JobState = JobState.Completed;
        completed.CompletedAt = Now;
        Job failed = Job.Enqueue(JobType.FinalPaymentQuery, record, null, Now);
        failed.JobState = JobState.Failed;
        failed.AttemptCount = 6;
        await SaveAsync(c => c.Jobs.AddRange(completed, failed));

        await SaveAsync(c => c.Jobs.AddRange(Job.Enqueue(JobType.FinalPaymentQuery, record, null, Now), Job.Enqueue(JobType.CardRefund, record, null, Now)));

        Assert.Equal(4L, await ScalarAsync<long>("SELECT count(*) FROM jobs"));
    }

    [Theory]
    [InlineData(JobType.CardRefund)]
    [InlineData(JobType.FinalPaymentQuery)]
    [InlineData(JobType.ImageDerivative)]
    [InlineData(JobType.InboundNotificationProcessing)]
    [InlineData(JobType.RefundOutcomeQuery)]
    public async Task JobWithoutRelatedRecord_OfATypeOtherThanTheBounceQuery_IsRefused(JobType jobType)
    {
        Assert.Equal(
            (CheckViolation, "ck_jobs_related_record_by_type"),
            await ViolationOfAsync(c => c.Jobs.Add(Job.Enqueue(jobType, null, null, Now))));
    }

    [Fact]
    public async Task BounceQuery_WithARelatedRecord_IsRefused()
    {
        Assert.Equal(
            (CheckViolation, "ck_jobs_related_record_by_type"),
            await ViolationOfAsync(c => c.Jobs.Add(Job.Enqueue(JobType.BounceQuery, EntityIds.New(), null, Now))));
    }

    // ---- KK3: the completion moment and the attempt count ----

    [Theory]
    [InlineData(JobState.Pending, true)]
    [InlineData(JobState.Failed, true)]
    [InlineData(JobState.Completed, false)]
    public async Task JobWhoseCompletionMomentDoesNotFitItsState_IsRefused(JobState state, bool withCompletedAt)
    {
        Job job = Job.Enqueue(JobType.ImageDerivative, EntityIds.New(), null, Now);
        job.JobState = state;
        job.CompletedAt = withCompletedAt ? Now : null;

        Assert.Equal((CheckViolation, "ck_jobs_completed_at_with_state"), await ViolationOfAsync(c => c.Jobs.Add(job)));
    }

    [Fact]
    public async Task CompletedJobWithItsMoment_IsAccepted()
    {
        Job job = Job.Enqueue(JobType.ImageDerivative, EntityIds.New(), null, Now);
        job.JobState = JobState.Completed;
        job.CompletedAt = Now;

        await SaveAsync(c => c.Jobs.Add(job));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM jobs WHERE job_state = 'Completed' AND completed_at IS NOT NULL"));
    }

    [Fact]
    public async Task JobWithANegativeAttemptCount_IsRefused_AndZeroIsAccepted()
    {
        Job negative = Job.Enqueue(JobType.RefundOutcomeQuery, EntityIds.New(), null, Now);
        negative.AttemptCount = -1;

        Assert.Equal((CheckViolation, "ck_jobs_attempt_count_not_negative"), await ViolationOfAsync(c => c.Jobs.Add(negative)));
        await SaveAsync(c => c.Jobs.Add(Job.Enqueue(JobType.RefundOutcomeQuery, EntityIds.New(), null, Now)));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT sum(attempt_count) FROM jobs"));
    }

    // ---- KK4: the single-row records ----

    [Fact]
    public async Task SecondHeartbeatRecord_IsRefused()
    {
        await SaveAsync(c => c.HeartbeatRecords.Add(HeartbeatRecord.Create(Now)));

        Assert.Equal(
            (UniqueViolation, "ix_heartbeat_records_single_row"),
            await ViolationOfAsync(c => c.HeartbeatRecords.Add(HeartbeatRecord.Create(Now))));
    }

    [Fact]
    public async Task SecondBackupStatusRecord_IsRefused()
    {
        await SaveAsync(c => c.BackupStatusRecords.Add(BackupStatusRecord.Create(Now)));

        Assert.Equal(
            (UniqueViolation, "ix_backup_status_records_single_row"),
            await ViolationOfAsync(c => c.BackupStatusRecords.Add(BackupStatusRecord.Create(Now))));
    }

    // ---- KK5: the outage is longer than two minutes ----

    [Theory]
    [InlineData(120)]
    [InlineData(60)]
    [InlineData(0)]
    [InlineData(-30)]
    public async Task OutageOfTwoMinutesOrLess_IsRefused(int seconds)
    {
        OutageRecord outage = OutageRecord.Create(Now, Now.AddSeconds(seconds), Now);

        Assert.Equal((CheckViolation, "ck_outage_records_longer_than_two_minutes"), await ViolationOfAsync(c => c.OutageRecords.Add(outage)));
    }

    [Fact]
    public async Task OutageLongerThanTwoMinutes_IsAccepted()
    {
        await SaveAsync(c => c.OutageRecords.Add(OutageRecord.Create(Now, Now.AddMinutes(2).AddMilliseconds(1), Now)));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM outage_records"));
    }

    // ---- KK6: the disposal record's number and dates ----

    [Theory]
    [InlineData("Order", null, null, null)]
    [InlineData("Order", 0, null, null)]
    [InlineData("LogFile", -1, null, null)]
    [InlineData("Customer", 1, "2026-11-01", null)]
    [InlineData("Customer", 1, null, "2026-11-01")]
    [InlineData("ServerLogRotation", 3, "2026-11-01", "2026-11-03")]
    [InlineData("ServerLogRotation", null, null, "2026-11-03")]
    [InlineData("ServerLogRotation", null, "2026-11-01", null)]
    public async Task DisposalRowWhoseNumberAndDatesDoNotFitItsType_IsRefused(string type, int? count, string? start, string? end)
    {
        (string state, string? constraint) = await SqlViolationOfAsync(
            "INSERT INTO disposal_records (id, created_at, disposal_record_type, count, range_start_date, range_end_date) VALUES (@id, now(), @type, @count, @start, @end)",
            ("id", EntityIds.New()),
            ("type", type),
            ("count", (object?)count ?? DBNull.Value),
            ("start", DateOrNull(start)),
            ("end", DateOrNull(end)));

        Assert.Equal((CheckViolation, "ck_disposal_records_count_and_range_by_type"), (state, constraint));

        static object DateOrNull(string? text) => text is null ? DBNull.Value : DateOnly.Parse(text, CultureInfo.InvariantCulture);
    }

    [Fact]
    public async Task ServerLogRotation_WhoseRangeEndsBeforeItStarts_IsRefused()
    {
        DisposalRecord rotation = DisposalRecord.ServerLogRotation(new DateOnly(2026, 11, 3), new DateOnly(2026, 11, 2), Now);

        Assert.Equal((CheckViolation, "ck_disposal_records_range_end_not_before_start"), await ViolationOfAsync(c => c.DisposalRecords.Add(rotation)));
    }

    [Fact]
    public async Task CountedRowAndOneDayRotation_AreAccepted()
    {
        await SaveAsync(c => c.DisposalRecords.AddRange(
            DisposalRecord.Counted(DisposalRecordType.Order, 1, Now),
            DisposalRecord.ServerLogRotation(new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 2), Now)));

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM disposal_records"));
    }

    // ---- KK7: the privileges of the key ring and of the backup status record ----

    [Fact]
    public async Task Reader_ReadingTheKeyRing_IsRefused_ButItReadsTheBackupStatus()
    {
        PostgresException keys = await Assert.ThrowsAsync<PostgresException>(() => QueryAsync(ReaderDsn, "SELECT xml FROM data_protection_keys"));
        List<string> status = await QueryAsync(ReaderDsn, "SELECT count(*) FROM backup_status_records");

        Assert.Equal(InsufficientPrivilege, keys.SqlState);
        Assert.Equal("0", Assert.Single(status));
    }

    [Fact]
    public async Task ApplicationUser_WritesAndReadsTheKeyRing_ThroughTheIdentityKeyWithoutASequencePrivilege()
    {
        // 09 10.8 item 7 (bilgim tarihli): the identity column's insert needs no privilege on its sequence.
        await ExecuteAsync(AppDsn, "INSERT INTO data_protection_keys (friendly_name, xml) VALUES ('key-1', '<key/>')");

        Assert.Equal("key-1 <key/>", Assert.Single(await QueryAsync(AppDsn, "SELECT friendly_name || ' ' || xml FROM data_protection_keys")));
    }

    [Fact]
    public async Task BackupStatusUser_UpdatesTheTwoMomentsWithTheJobsStatements()
    {
        await SaveAsync(c => c.BackupStatusRecords.Add(BackupStatusRecord.Create(Now)));

        await AsBackupStatusAsync(SucceededStatement);
        await AsBackupStatusAsync(FailedStatement);

        Assert.Equal("true true", Assert.Single(await QueryAsync(
            _collection.Dsn,
            "SELECT (last_succeeded_at IS NOT NULL)::text || ' ' || (last_failed_at IS NOT NULL)::text FROM backup_status_records")));
    }

    [Fact]
    public async Task BackupJob_WritesTheResultWithExactlyTheStatementsTheUserMayRun()
    {
        // The statements above are the job's own (D-39 (a)): a change in the script that the privilege would refuse fails here.
        string script = await File.ReadAllTextAsync(
            Path.Combine(RepositoryRoot.Find(), "deploy", "images", "database", "database-job.sh"), TestContext.Current.CancellationToken);

        Assert.Contains($"\"{SucceededStatement}\"", script, StringComparison.Ordinal);
        Assert.Contains($"\"{FailedStatement}\"", script, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("UPDATE public.backup_status_records SET created_at = now()")]
    [InlineData("UPDATE public.backup_status_records SET id = gen_random_uuid()")]
    [InlineData("SELECT last_succeeded_at FROM public.backup_status_records")]
    [InlineData("UPDATE public.backup_status_records SET last_succeeded_at = now() WHERE id IS NOT NULL")]
    [InlineData("INSERT INTO public.backup_status_records (id, created_at) VALUES (gen_random_uuid(), now())")]
    [InlineData("DELETE FROM public.backup_status_records")]
    [InlineData("UPDATE public.heartbeat_records SET last_heartbeat_at = now()")]
    [InlineData("SELECT count(*) FROM public.data_protection_keys")]
    public async Task BackupStatusUser_AnythingButTheTwoMoments_IsRefused(string statement)
    {
        await SaveAsync(c => c.BackupStatusRecords.Add(BackupStatusRecord.Create(Now)));

        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => AsBackupStatusAsync(statement));

        Assert.Equal(InsufficientPrivilege, error.SqlState);
    }

    // ---- KK7: the role settings ----

    [Theory]
    [InlineData("shopfolio.reader_user")]
    [InlineData("shopfolio.backup_status_user")]
    public async Task Migration_WithoutARoleSettingItReads_IsRolledBack_AndWithItGrantsTheRightUsers(string setting)
    {
        string database = "shopfolio_test_" + Convert.ToHexStringLower(System.Security.Cryptography.RandomNumberGenerator.GetBytes(6));
        string admin = _template.DsnFor(_collection.DatabaseName);
        Dictionary<string, string> variables = Users.Variables(database);
        string migratorDsn = _template.DsnFor(database, Users.Migrator, Users.MigratorPassword, pooling: false);
        string previous = typeof(CreateDispatchAndChannelStateTables).GetCustomAttribute<MigrationAttribute>()!.Id;
        string value = setting == "shopfolio.reader_user" ? Users.Reader : Users.BackupStatus;
        try
        {
            await BootstrapScript.RunAsync(admin, BootstrapScript.DatabaseFile, variables, Ct);
            await BootstrapScript.RunAsync(admin, BootstrapScript.DatabaseAccessFile, variables, Ct);
            await BootstrapScript.RunAsync(_template.DsnFor(database), BootstrapScript.SchemaFile, variables, Ct);
            await MigrateAsync(migratorDsn, previous);

            // The setting is gone: the new migration fails in its privilege statement and leaves nothing.
            await ExecuteAsync(admin, $"ALTER ROLE {Users.Migrator} IN DATABASE {database} RESET {setting}");
            PostgresException failure = await Assert.ThrowsAnyAsync<PostgresException>(() => MigrateAsync(migratorDsn, null));
            Assert.Equal(PostgresErrorCodes.UndefinedObject, failure.SqlState);
            string dsn = _template.DsnFor(database);
            Assert.Equal(0L, await ScalarAsync<long>(dsn, "SELECT count(*) FROM pg_tables WHERE schemaname = 'public' AND tablename = ANY(@names)", ("names", SixTables)));
            Assert.Equal(previous, await ScalarAsync<string>(dsn, "SELECT max(migration_id) FROM \"__EFMigrationsHistory\""));

            // The setting is back: the migration runs and the privileges are the right users'.
            await ExecuteAsync(admin, $"ALTER ROLE {Users.Migrator} IN DATABASE {database} SET {setting} = '{value}'");
            await MigrateAsync(migratorDsn, null);
            Assert.Equal(
                "false|true|true|true|false|false",
                await ScalarAsync<string>(
                    dsn,
                    $"SELECT has_table_privilege('{Users.Reader}', 'public.data_protection_keys', 'SELECT')::text || '|' || "
                    + $"has_table_privilege('{Users.App}', 'public.data_protection_keys', 'SELECT')::text || '|' || "
                    + $"has_column_privilege('{Users.BackupStatus}', 'public.backup_status_records', 'last_succeeded_at', 'UPDATE')::text || '|' || "
                    + $"has_column_privilege('{Users.BackupStatus}', 'public.backup_status_records', 'last_failed_at', 'UPDATE')::text || '|' || "
                    + $"has_column_privilege('{Users.BackupStatus}', 'public.backup_status_records', 'created_at', 'UPDATE')::text || '|' || "
                    + $"has_any_column_privilege('{Users.BackupStatus}', 'public.backup_status_records', 'SELECT')::text"));
        }
        finally
        {
            NpgsqlConnection.ClearAllPools();
            await ExecuteAsync(admin, $"DROP DATABASE IF EXISTS {database} WITH (FORCE)");
        }
    }

    // ---- KK8 and criterion 8: one topic, no Down ----

    [Fact]
    public void Migration_HasNoDownMethod()
    {
        MethodInfo? down = typeof(CreateSchedulerBackupDisposalAndKeyTables)
            .GetMethod("Down", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);

        Assert.Null(down);
    }

    // ---- helpers ----

    private static async Task MigrateAsync(string dsn, string? target)
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, dsn);
        await using AppDbContext context = new(builder.Options);
        IMigrator migrator = ((IInfrastructure<IServiceProvider>)context).Instance.GetService(typeof(IMigrator)) as IMigrator
            ?? throw new InvalidOperationException("The migrator service is not available.");
        await migrator.MigrateAsync(target, Ct);
    }

    /// <summary>
    /// Runs one statement as the backup status user. The user has no password and connects only through the local socket (pg_hba.conf), so
    /// the test acts as it with SET ROLE on the administrator's connection (the way DatabaseUsersTests does).
    /// </summary>
    private async Task AsBackupStatusAsync(string statement)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using (NpgsqlCommand role = new($"SET ROLE {Users.BackupStatus}", connection))
        {
            await role.ExecuteNonQueryAsync(Ct);
        }

        await using NpgsqlCommand command = new(statement, connection);
        await command.ExecuteNonQueryAsync(Ct);
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
    private async Task<(string SqlState, string? Constraint)> SqlViolationOfAsync(string sql, params (string Name, object Value)[] parameters)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        foreach ((string name, object value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => command.ExecuteNonQueryAsync(Ct));
        return (error.SqlState, error.ConstraintName);
    }

    private static async Task ExecuteAsync(string dsn, string sql)
    {
        await using NpgsqlConnection connection = new(dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private Task<T> ScalarAsync<T>(string sql) => ScalarAsync<T>(_collection.Dsn, sql);

    private static async Task<T> ScalarAsync<T>(string dsn, string sql, params (string Name, object Value)[] parameters)
    {
        await using NpgsqlConnection connection = new(dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        foreach ((string name, object value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        return (T)(await command.ExecuteScalarAsync(Ct))!;
    }

    private static async Task<List<string>> QueryAsync(string dsn, string sql, params (string Name, object Value)[] parameters)
    {
        await using NpgsqlConnection connection = new(dsn);
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
