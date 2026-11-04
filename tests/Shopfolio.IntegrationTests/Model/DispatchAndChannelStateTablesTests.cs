using System.Globalization;
using System.Net;
using System.Reflection;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Migrations;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the dispatch and channel state tests (T117): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(DispatchAndChannelStateTablesDefinition.Name)]
public sealed class DispatchAndChannelStateTablesDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "DispatchAndChannelStateTables";
}

/// <summary>
/// The dispatch record and the channel state record on a real PostgreSQL (T117 criteria 1-6; 06 4.58, 4.60, 2.2.5, 2.2.6, 2.3.2, 7.1-7.5, 9.2;
/// 09 10.6): the migration made the two tables; the database itself refuses an order link or a related record that does not fit the type
/// (every type against every combination), a queued row with a sending moment, a sent row without one, a next attempt outside the queue, a
/// negative attempt count, a second channel state row and a first error moment that does not match the count; an undelivered row is accepted
/// with the sending moment and without it; the related record is a value, so its deletion leaves the dispatch row, and the order link is
/// restrictive.
/// </summary>
[Collection(DispatchAndChannelStateTablesDefinition.Name)]
public sealed class DispatchAndChannelStateTablesTests : IAsyncLifetime
{
    private const string CheckViolation = PostgresErrorCodes.CheckViolation;
    private const string UniqueViolation = PostgresErrorCodes.UniqueViolation;
    private const string ForeignKeyViolation = PostgresErrorCodes.ForeignKeyViolation;

    private const string Phone = "3121234567";

    private static readonly DateTimeOffset Now = new(2026, 11, 4, 12, 0, 0, TimeSpan.Zero);

    private static readonly string[] TwoTables = ["channel_state_records", "dispatch_records"];

    private static readonly string[] ClaimKinds = ["Withdrawal", "LateDeliveryTermination", "DefectClaim"];

    private static readonly string[] AllRelatedTypes = [.. Enum.GetNames<DispatchRelatedRecordType>()];

    private readonly CollectionDatabaseFixture _collection;

    private int _orderCounter;

    public DispatchAndChannelStateTablesTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    /// <summary>
    /// The table of 06 4.58 "Türe göre sipariş ve ilgili kayıt", written here on its own: for every type, whether the order link is filled and
    /// which related record types it accepts (the empty one is "boş").
    /// </summary>
    private static (bool WithOrder, string?[] RelatedTypes) Expected(string type)
    {
        string?[] none = [null];
        return type switch
        {
            "B9" => (true, ClaimKinds),
            _ when type is ['B', ..] => (true, none),
            "F1" => (false, ["Order"]),
            "F2" => (false, ["ContactRequest"]),
            "F3" => (false, ClaimKinds),
            "F4" => (false, ["Refund"]),
            "F5" => (false, none),
            "F6" => (false, ["AdminInvitation", "Admin"]),
            "EmailVerificationLink" or "AccountDeletedNotice" or "GoogleLinkConfirmationLink" => (false, ["Customer"]),
            "PasswordResetLink" or "NewEmailVerificationLink" or "EmailChangeNotice" => (false, ["Customer", "Admin"]),
            "AdminInvitation" => (false, ["AdminInvitation"]),
            _ => throw new InvalidOperationException($"No row of the type table for {type}."),
        };
    }

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE dispatch_records, channel_state_records, contact_requests, order_items, orders, legal_text_versions, provinces CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the migration ----

    [Fact]
    public async Task Migration_MakesTheTwoTables()
    {
        List<string> tables = await QueryAsync(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name = ANY(@names)",
            ("names", TwoTables));

        Assert.Equal(TwoTables, tables.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Columns_HaveTheTypesAndNullabilityOfTheFieldTables()
    {
        // 06 4.58 and 4.60 (the common Id and CreatedAt included); the enum columns are text, the event values are jsonb.
        string[] expected =
        [
            "channel_state_records.consecutive_channel_error_count integer not null",
            "channel_state_records.created_at timestamp with time zone not null",
            "channel_state_records.first_channel_error_at timestamp with time zone",
            "channel_state_records.id uuid not null",
            "channel_state_records.last_succeeded_at timestamp with time zone",
            "dispatch_records.attempt_count integer not null",
            "dispatch_records.created_at timestamp with time zone not null",
            "dispatch_records.dispatch_state text not null",
            "dispatch_records.dispatch_type text not null",
            "dispatch_records.event_values jsonb",
            "dispatch_records.id uuid not null",
            "dispatch_records.last_outcome text",
            "dispatch_records.next_attempt_at timestamp with time zone",
            "dispatch_records.order_id uuid",
            "dispatch_records.original_dispatch_id uuid",
            "dispatch_records.recipient_email text",
            "dispatch_records.related_record_id uuid",
            "dispatch_records.related_record_type text",
            "dispatch_records.sent_at timestamp with time zone",
        ];

        List<string> columns = await QueryAsync(
            "SELECT table_name || '.' || column_name || ' ' || data_type || CASE WHEN is_nullable = 'NO' THEN ' not null' ELSE '' END"
            + " FROM information_schema.columns WHERE table_schema = 'public' AND table_name = ANY(@names)",
            ("names", TwoTables));

        Assert.Equal(expected, columns.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Columns_HaveTheDatabaseDefaultsOfTheDefaultColumns()
    {
        // The "Varsayılan" column: the state is Queued, the counts are 0 — as database defaults; no other column has one.
        string[] expected =
        [
            "channel_state_records.consecutive_channel_error_count 0",
            "dispatch_records.attempt_count 0",
            "dispatch_records.dispatch_state 'Queued'::text",
        ];

        List<string> defaults = await QueryAsync(
            "SELECT table_name || '.' || column_name || ' ' || column_default FROM information_schema.columns"
            + " WHERE table_schema = 'public' AND table_name = ANY(@names) AND column_default IS NOT NULL",
            ("names", TwoTables));

        Assert.Equal(expected, defaults.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Indexes_AreTheDocumentedOnes()
    {
        // 06 4.58, 4.60, 7.5: the queue scan (filtered on the queued rows), the order history (its first column serves the order link, 7.5.2),
        // the related record, the type and the time scans; the single-row index of the channel state.
        string[] expected =
        [
            "channel_state_records.ix_channel_state_records_single_row | CREATE UNIQUE INDEX ix_channel_state_records_single_row ON public.channel_state_records USING btree ((true))",
            "channel_state_records.pk_channel_state_records | CREATE UNIQUE INDEX pk_channel_state_records ON public.channel_state_records USING btree (id)",
            "dispatch_records.ix_dispatch_records_created_at | CREATE INDEX ix_dispatch_records_created_at ON public.dispatch_records USING btree (created_at)",
            "dispatch_records.ix_dispatch_records_dispatch_state_next_attempt_at | CREATE INDEX ix_dispatch_records_dispatch_state_next_attempt_at ON public.dispatch_records USING btree (dispatch_state, next_attempt_at) WHERE (dispatch_state = 'Queued'::text)",
            "dispatch_records.ix_dispatch_records_dispatch_type_created_at | CREATE INDEX ix_dispatch_records_dispatch_type_created_at ON public.dispatch_records USING btree (dispatch_type, created_at)",
            "dispatch_records.ix_dispatch_records_order_id_created_at | CREATE INDEX ix_dispatch_records_order_id_created_at ON public.dispatch_records USING btree (order_id, created_at)",
            "dispatch_records.ix_dispatch_records_related_record_type_related_record_id_crea | CREATE INDEX ix_dispatch_records_related_record_type_related_record_id_crea ON public.dispatch_records USING btree (related_record_type, related_record_id, created_at)",
            "dispatch_records.pk_dispatch_records | CREATE UNIQUE INDEX pk_dispatch_records ON public.dispatch_records USING btree (id)",
        ];

        List<string> indexes = await QueryAsync(
            "SELECT tablename || '.' || indexname || ' | ' || indexdef FROM pg_indexes WHERE schemaname = 'public' AND tablename = ANY(@names)",
            ("names", TwoTables));

        Assert.Equal(expected, indexes.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ForeignKeys_AreTheRestrictiveOrderLinkOnly()
    {
        // 06 2.2.5, 2.2.6, 7.2: the order link is restrictive (a); the related record and the first e-mail are values, and the channel state
        // has no link.
        string[] expected = ["dispatch_records fk_dispatch_records_orders_order_id a"];

        List<string> links = await QueryAsync(
            "SELECT conrelid::regclass::text || ' ' || conname || ' ' || confdeltype::text FROM pg_constraint WHERE contype = 'f' AND conrelid = ANY(ARRAY(SELECT x::regclass FROM unnest(@oids) x))",
            ("oids", TwoTables.Select(t => "public." + t).ToArray()));

        Assert.Equal(expected, links);
    }

    [Fact]
    public async Task CheckConstraints_AreTheDocumentedOnes()
    {
        // 06 7.3 and the enum columns' allowed values (06 2.9.2).
        string[] expected =
        [
            "channel_state_records.ck_channel_state_records_error_count_not_negative",
            "channel_state_records.ck_channel_state_records_first_error_at_with_count",
            "dispatch_records.ck_dispatch_records_attempt_count_not_negative",
            "dispatch_records.ck_dispatch_records_dispatch_state",
            "dispatch_records.ck_dispatch_records_dispatch_type",
            "dispatch_records.ck_dispatch_records_last_outcome",
            "dispatch_records.ck_dispatch_records_next_attempt_only_when_queued",
            "dispatch_records.ck_dispatch_records_order_and_related_record_by_type",
            "dispatch_records.ck_dispatch_records_related_record_type",
            "dispatch_records.ck_dispatch_records_related_record_type_with_id",
            "dispatch_records.ck_dispatch_records_sent_at_with_state",
        ];

        List<string> checks = await QueryAsync(
            "SELECT conrelid::regclass::text || '.' || conname FROM pg_constraint WHERE contype = 'c' AND conrelid = ANY(ARRAY(SELECT x::regclass FROM unnest(@oids) x))",
            ("oids", TwoTables.Select(t => "public." + t).ToArray()));

        Assert.Equal(expected, checks.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Migration_HasNoDownMethod()
    {
        MethodInfo? down = typeof(CreateDispatchAndChannelStateTables)
            .GetMethod("Down", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);

        Assert.Null(down);
    }

    // ---- KK2: the order link and the related record by type ----

    [Fact]
    public void TypeTable_NamesEveryDispatchType()
    {
        // The test's own copy of the table covers every member of the enum, so a new type cannot pass unseen.
        foreach (string type in Enum.GetNames<DispatchType>())
        {
            (bool _, string?[] related) = Expected(type);
            Assert.NotEmpty(related);
        }
    }

    [Fact]
    public async Task EveryTypeAgainstEveryCombination_IsAcceptedExactlyWhereTheTypeTableAllows()
    {
        Guid orderId = await SeedOrderIdAsync();
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);

        List<string> wrong = [];
        foreach (string type in Enum.GetNames<DispatchType>())
        {
            (bool withOrder, string?[] allowed) = Expected(type);
            foreach (bool hasOrder in new[] { true, false })
            {
                foreach (string? related in new string?[] { null }.Concat(AllRelatedTypes))
                {
                    bool expected = hasOrder == withOrder && allowed.Contains(related);
                    string? error = await TryInsertAsync(connection, type, hasOrder ? orderId : null, related, related is null ? null : Guid.NewGuid());
                    if (expected != (error is null) || (!expected && error != "ck_dispatch_records_order_and_related_record_by_type"))
                    {
                        wrong.Add($"{type} order={hasOrder} related={related ?? "boş"} -> {error ?? "accepted"}");
                    }
                }
            }
        }

        Assert.Empty(wrong);
    }

    [Fact]
    public async Task RelatedRecordType_WithoutId_AndId_WithoutType_AreRefused()
    {
        Guid orderId = await SeedOrderIdAsync();
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);

        Assert.Equal("ck_dispatch_records_related_record_type_with_id", await TryInsertAsync(connection, "B9", orderId, "DefectClaim", null));
        Assert.Equal("ck_dispatch_records_related_record_type_with_id", await TryInsertAsync(connection, "B1", orderId, null, Guid.NewGuid()));
        Assert.Null(await TryInsertAsync(connection, "B9", orderId, "DefectClaim", Guid.NewGuid()));
        Assert.Null(await TryInsertAsync(connection, "B1", orderId, null, null));
    }

    [Fact]
    public async Task UnknownTypeOrRelatedTypeOrOutcome_IsRefused()
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);

        Assert.Equal("ck_dispatch_records_dispatch_type", await TryInsertAsync(connection, "B17", null, null, null));
        Assert.NotNull(await TryInsertAsync(connection, "F1", null, "Order2", Guid.NewGuid()));
        Assert.Equal("ck_dispatch_records_dispatch_state", await TryInsertAsync(connection, "F5", null, null, null, state: "Done"));
        Assert.Equal("ck_dispatch_records_last_outcome", await TryInsertAsync(connection, "F5", null, null, null, outcome: "Lost"));
        Assert.Null(await TryInsertAsync(connection, "F5", null, null, null, outcome: "RecipientError"));
    }

    // ---- KK3: the state, the moments and the count ----

    [Fact]
    public async Task SentAt_IsFilledOnSent_EmptyOnQueued_AndEitherOnUndelivered()
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        const string Rule = "ck_dispatch_records_sent_at_with_state";

        Assert.Equal(Rule, await TryInsertAsync(connection, "F5", null, null, null, state: "Queued", sentAt: true));
        Assert.Equal(Rule, await TryInsertAsync(connection, "F5", null, null, null, state: "Sent", sentAt: false));
        Assert.Null(await TryInsertAsync(connection, "F5", null, null, null, state: "Queued", sentAt: false));
        Assert.Null(await TryInsertAsync(connection, "F5", null, null, null, state: "Sent", sentAt: true));

        // K-1120: an undelivered row is the one that never went out (empty) or the one that went and came back (filled).
        Assert.Null(await TryInsertAsync(connection, "F5", null, null, null, state: "Undelivered", sentAt: true));
        Assert.Null(await TryInsertAsync(connection, "F5", null, null, null, state: "Undelivered", sentAt: false));
    }

    [Fact]
    public async Task NextAttemptAt_IsAllowedOnlyWhileQueued()
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        const string Rule = "ck_dispatch_records_next_attempt_only_when_queued";

        Assert.Null(await TryInsertAsync(connection, "F5", null, null, null, state: "Queued", nextAttempt: true));
        Assert.Null(await TryInsertAsync(connection, "F5", null, null, null, state: "Queued", nextAttempt: false));
        Assert.Equal(Rule, await TryInsertAsync(connection, "F5", null, null, null, state: "Sent", sentAt: true, nextAttempt: true));
        Assert.Equal(Rule, await TryInsertAsync(connection, "F5", null, null, null, state: "Undelivered", sentAt: false, nextAttempt: true));
        Assert.Equal(Rule, await TryInsertAsync(connection, "F5", null, null, null, state: "Undelivered", sentAt: true, nextAttempt: true));
        Assert.Null(await TryInsertAsync(connection, "F5", null, null, null, state: "Sent", sentAt: true, nextAttempt: false));
    }

    [Theory]
    [InlineData(-1, "ck_dispatch_records_attempt_count_not_negative")]
    [InlineData(0, null)]
    [InlineData(6, null)]
    public async Task AttemptCount_BelowZero_IsRefused(int count, string? expected)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);

        Assert.Equal(expected, await TryInsertAsync(connection, "F5", null, null, null, attempts: count));
    }

    [Fact]
    public async Task OnlyTheTypeAndTheCreationAreRequired_AndTheDatabaseFillsTheStateAndTheCount()
    {
        await ExecuteAsync("INSERT INTO dispatch_records (id, created_at, dispatch_type) VALUES (gen_random_uuid(), now(), 'F5')");

        Assert.Equal("Queued", await ScalarAsync<string>("SELECT dispatch_state FROM dispatch_records"));
        Assert.Equal(0, await ScalarAsync<int>("SELECT attempt_count FROM dispatch_records"));
    }

    // ---- KK4: the related record is a value; the order link restricts ----

    [Fact]
    public async Task DeletingTheRelatedRecord_LeavesTheDispatchRow_AndTheFirstEmailLikewise()
    {
        ContactRequest request = ContactRequest.Create("Ali", "ali@example.com", ContactTopic.GeneralQuestion, "Merhaba", Now);
        await SaveAsync(c => c.ContactRequests.Add(request));
        DispatchRecord first = DispatchRecord.Queue(DispatchType.F2, "firma@example.com", null, DispatchRelatedRecordType.ContactRequest, request.Id, Now);
        DispatchRecord again = DispatchRecord.Queue(DispatchType.F2, "firma@example.com", null, DispatchRelatedRecordType.ContactRequest, request.Id, Now);
        again.OriginalDispatchId = first.Id;
        await SaveAsync(c => c.DispatchRecords.AddRange(first, again));

        await ExecuteAsync($"DELETE FROM contact_requests WHERE id = '{request.Id}'");
        await ExecuteAsync($"DELETE FROM dispatch_records WHERE id = '{first.Id}'");

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM dispatch_records"));
        Assert.Equal(request.Id, await ScalarAsync<Guid>($"SELECT related_record_id FROM dispatch_records WHERE id = '{again.Id}'"));
        Assert.Equal(first.Id, await ScalarAsync<Guid>($"SELECT original_dispatch_id FROM dispatch_records WHERE id = '{again.Id}'"));
    }

    [Fact]
    public async Task OrderWithADispatchRecord_CannotBeDeleted_AndAnUnknownOrderIsRefused()
    {
        Guid orderId = await SeedOrderIdAsync();
        DispatchRecord record = DispatchRecord.Queue(DispatchType.B1, "ali@example.com", orderId, null, null, Now);
        await SaveAsync(c => c.DispatchRecords.Add(record));

        PostgresException refused = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync($"DELETE FROM orders WHERE id = '{orderId}'"));
        Assert.Equal((ForeignKeyViolation, "fk_dispatch_records_orders_order_id"), (refused.SqlState, refused.ConstraintName));

        (string SqlState, string? Constraint) unknown = await ViolationOfAsync(c => c.DispatchRecords.Add(DispatchRecord.Queue(DispatchType.B1, "ali@example.com", Guid.NewGuid(), null, null, Now)));
        Assert.Equal((ForeignKeyViolation, "fk_dispatch_records_orders_order_id"), unknown);

        await ExecuteAsync($"DELETE FROM dispatch_records WHERE id = '{record.Id}'");
        await ExecuteAsync($"DELETE FROM orders WHERE id = '{orderId}'");
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM orders"));
    }

    [Fact]
    public async Task DispatchRecord_RoundTripsThroughTheContext_WithTheEnumsAndTheEventValues()
    {
        DispatchRecord record = DispatchRecord.Queue(DispatchType.PasswordResetLink, "ali@example.com", null, DispatchRelatedRecordType.Admin, Guid.NewGuid(), Now);
        record.EventValues = """[{"name":"linkId","value":"abc"}]""";
        record.LastOutcome = DispatchOutcome.ChannelError;
        record.AttemptCount = 2;
        record.NextAttemptAt = Now.AddMinutes(5);
        await SaveAsync(c => c.DispatchRecords.Add(record));

        await using AppDbContext context = NewContext();
        DispatchRecord read = await context.DispatchRecords.SingleAsync(Ct);

        Assert.Equal(DispatchType.PasswordResetLink, read.DispatchType);
        Assert.Equal(DispatchRelatedRecordType.Admin, read.RelatedRecordType);
        Assert.Equal(DispatchState.Queued, read.DispatchState);
        Assert.Equal(DispatchOutcome.ChannelError, read.LastOutcome);
        Assert.Equal(2, read.AttemptCount);
        Assert.Equal(Now.AddMinutes(5), read.NextAttemptAt);
        Assert.Contains("linkId", read.EventValues, StringComparison.Ordinal);
    }

    // ---- KK5: the channel state ----

    [Fact]
    public async Task SecondChannelStateRow_IsRefused()
    {
        await SaveAsync(c => c.ChannelStateRecords.Add(ChannelStateRecord.Create(Now)));

        Assert.Equal(
            (UniqueViolation, "ix_channel_state_records_single_row"),
            await ViolationOfAsync(c => c.ChannelStateRecords.Add(ChannelStateRecord.Create(Now))));
    }

    [Fact]
    public async Task ChannelState_StartsAtZeroWithEmptyMoments_AndTheDatabaseFillsTheCount()
    {
        await ExecuteAsync("INSERT INTO channel_state_records (id, created_at) VALUES (gen_random_uuid(), now())");

        Assert.Equal(0, await ScalarAsync<int>("SELECT consecutive_channel_error_count FROM channel_state_records"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM channel_state_records WHERE last_succeeded_at IS NULL AND first_channel_error_at IS NULL"));
    }

    [Theory]
    [InlineData(-1, false, "ck_channel_state_records_error_count_not_negative")]
    [InlineData(-1, true, "ck_channel_state_records_error_count_not_negative")]
    [InlineData(0, true, "ck_channel_state_records_first_error_at_with_count")]
    [InlineData(3, false, "ck_channel_state_records_first_error_at_with_count")]
    [InlineData(0, false, null)]
    [InlineData(1, true, null)]
    [InlineData(5, true, null)]
    public async Task FirstErrorMoment_IsEmptyExactlyWhenTheCountIsZero(int count, bool withMoment, string? expected)
    {
        ChannelStateRecord row = ChannelStateRecord.Create(Now);
        row.ConsecutiveChannelErrorCount = count;
        row.FirstChannelErrorAt = withMoment ? Now : null;

        if (expected is null)
        {
            await SaveAsync(c => c.ChannelStateRecords.Add(row));
            Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM channel_state_records"));
        }
        else
        {
            Assert.Equal((CheckViolation, expected), await ViolationOfAsync(c => c.ChannelStateRecords.Add(row)));
        }
    }

    [Fact]
    public async Task ChannelState_CountIncrementIsOneUpdateOverTheRowsOwnValue_AndAResetEmptiesTheMoment()
    {
        await ExecuteAsync("INSERT INTO channel_state_records (id, created_at) VALUES (gen_random_uuid(), now())");

        // 06 4.60 (K-1072): the first error writes the moment with the count 0 -> 1; the next error only raises the count.
        await ExecuteAsync("UPDATE channel_state_records SET consecutive_channel_error_count = consecutive_channel_error_count + 1, first_channel_error_at = now()");
        await ExecuteAsync("UPDATE channel_state_records SET consecutive_channel_error_count = consecutive_channel_error_count + 1");
        Assert.Equal(2, await ScalarAsync<int>("SELECT consecutive_channel_error_count FROM channel_state_records"));

        await ExecuteAsync("UPDATE channel_state_records SET consecutive_channel_error_count = 0, first_channel_error_at = NULL, last_succeeded_at = now()");
        Assert.Equal(0, await ScalarAsync<int>("SELECT consecutive_channel_error_count FROM channel_state_records"));
    }

    // ---- helpers ----

    /// <summary>Inserts one dispatch row by SQL and returns the name of the constraint that refused it, or null when it was accepted.</summary>
    private static async Task<string?> TryInsertAsync(
        NpgsqlConnection connection,
        string type,
        Guid? orderId,
        string? relatedType,
        Guid? relatedId,
        string state = "Queued",
        string? outcome = null,
        bool sentAt = false,
        bool nextAttempt = false,
        int attempts = 0)
    {
        await using NpgsqlCommand command = new(
            "INSERT INTO dispatch_records (id, created_at, dispatch_type, order_id, related_record_type, related_record_id, dispatch_state, last_outcome, sent_at, next_attempt_at, attempt_count)"
            + " VALUES (gen_random_uuid(), now(), @type, @order, @rtype, @rid, @state, @outcome, CASE WHEN @sent THEN now() END, CASE WHEN @next THEN now() END, @attempts)",
            connection);
        command.Parameters.AddWithValue("type", type);
        command.Parameters.AddWithValue("order", NpgsqlTypes.NpgsqlDbType.Uuid, (object?)orderId ?? DBNull.Value);
        command.Parameters.AddWithValue("rtype", NpgsqlTypes.NpgsqlDbType.Text, (object?)relatedType ?? DBNull.Value);
        command.Parameters.AddWithValue("rid", NpgsqlTypes.NpgsqlDbType.Uuid, (object?)relatedId ?? DBNull.Value);
        command.Parameters.AddWithValue("state", state);
        command.Parameters.AddWithValue("outcome", NpgsqlTypes.NpgsqlDbType.Text, (object?)outcome ?? DBNull.Value);
        command.Parameters.AddWithValue("sent", sentAt);
        command.Parameters.AddWithValue("next", nextAttempt);
        command.Parameters.AddWithValue("attempts", attempts);

        try
        {
            await command.ExecuteNonQueryAsync(Ct);
            return null;
        }
        catch (PostgresException error) when (error.SqlState == CheckViolation)
        {
            return error.ConstraintName;
        }
    }

    private async Task<Guid> SeedOrderIdAsync()
    {
        References refs = await GetReferencesAsync();
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
        await SaveAsync(c => c.Orders.Add(order));
        return order.Id;
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
