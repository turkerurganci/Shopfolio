using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Ordering;

namespace Shopfolio.IntegrationTests.Disposal;

/// <summary>
/// The disposal regulator and its parts on a real PostgreSQL and a fake clock (T201; 05 5.2.8, 7.4.1; 06 9.3, 9.4.3): the nightly job's hour, a
/// missed night made up, every window read from the product constants, the temporary records, the audit trail's ten years, the log files by their
/// last write, the disposal record in the deleting transaction, and the rows that must stay. No test waits for real time; every row is seeded
/// relative to the harness's clock and every period is derived from the constant that owns it.
/// </summary>
[Collection(DisposalDefinition.Name)]
public sealed class DisposalTests : IAsyncLifetime
{
    private readonly TemplateDatabaseFixture _template;
    private readonly CollectionDatabaseFixture _collection;
    private readonly string _logRoot = Path.Combine(Path.GetTempPath(), "shopfolio-t201-" + Guid.NewGuid().ToString("N"));
    private DisposalHarness _harness = null!;

    public DisposalTests(TemplateDatabaseFixture template, CollectionDatabaseFixture collection)
    {
        _template = template;
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private string ApplicationDirectory => Path.Combine(_logRoot, "app");

    private string AccessDirectory => Path.Combine(_logRoot, "access");

    public async ValueTask InitializeAsync()
    {
        Directory.CreateDirectory(ApplicationDirectory);
        Directory.CreateDirectory(AccessDirectory);
        await using (NpgsqlConnection connection = new(_collection.Dsn))
        {
            await connection.OpenAsync(Ct);
            await using NpgsqlCommand truncate = new(
                "TRUNCATE sessions, recognized_browser_marks, link_records, attempt_records, carts, admin_invitations, audit_logs, disposal_records, heartbeat_records, jobs, customers, admins CASCADE",
                connection);
            await truncate.ExecuteNonQueryAsync(Ct);
        }

        _harness = new DisposalHarness(_collection.Dsn, new LogDirectories(ApplicationDirectory, AccessDirectory));
    }

    public async ValueTask DisposeAsync()
    {
        await _harness.DisposeAsync();
        if (Directory.Exists(_logRoot))
        {
            Directory.Delete(_logRoot, recursive: true);
        }
    }

    // ---- KK1, KK2: the nightly job's hour and the missed nights ----

    [Fact]
    public void TheNightlyJob_RunsTheQueueCleanupBeforeTheDisposal()
    {
        IReadOnlyList<string> names = _harness.StepNames;

        Assert.Equal(1, names.Count(n => n == "Disposal"));
        Assert.True(names.ToList().IndexOf("JobQueueCleanup") < names.ToList().IndexOf("Disposal"));
    }

    [Fact]
    public async Task TheNightlyJob_DoesNotRunAMinuteBefore0300InTurkey_AndRunsAt0300()
    {
        // 03:00 in Turkey is 00:00 UTC; the last success is the previous night's.
        DateTimeOffset due = new(2026, 11, 5, 0, 0, 0, TimeSpan.Zero);
        Assert.Equal(due, NightlyJobSchedule.LatestDueMoment(due));
        await SeedHeartbeatAsync(due.AddDays(-1));
        Guid customer = await SeedCustomerAsync("nightly@example.com");
        await _harness.SeedAsync(db => db.RecognizedBrowserMarks.Add(Mark(customer, due.AddHours(-12))));

        _harness.Time.SetUtcNow(due.AddSeconds(-1));
        bool early = await _harness.Nightly.RunIfDueAsync(Ct);
        long markedEarly = await CountAsync(db => db.RecognizedBrowserMarks.LongCountAsync(Ct));

        _harness.Time.SetUtcNow(due);
        bool atTheHour = await _harness.Nightly.RunIfDueAsync(Ct);
        long markedAfter = await CountAsync(db => db.RecognizedBrowserMarks.LongCountAsync(Ct));
        DateTimeOffset? succeeded = await _harness.ReadAsync(db => db.HeartbeatRecords.Select(h => h.NightlyJobLastSucceededAt).SingleAsync(Ct));

        Assert.False(early);
        Assert.Equal(1L, markedEarly);
        Assert.True(atTheHour);
        Assert.Equal(0L, markedAfter);
        Assert.Equal(due, succeeded);
    }

    [Fact]
    public async Task AMissedNight_IsMadeUp_WithBothNightsRecords_AndASecondRunChangesNothing()
    {
        DateTimeOffset night3 = new(2026, 11, 7, 0, 0, 0, TimeSpan.Zero);
        Guid customer = await SeedCustomerAsync("missed@example.com");
        await _harness.SeedAsync(db =>
        {
            db.RecognizedBrowserMarks.Add(Mark(customer, night3.AddDays(-2).AddHours(12)));
            db.RecognizedBrowserMarks.Add(Mark(customer, night3.AddDays(-1).AddHours(12)));
            db.RecognizedBrowserMarks.Add(Mark(customer, night3.AddDays(1)));
        });
        _harness.Time.SetUtcNow(night3);

        await _harness.RunStepAsync(Ct);
        List<DisposalRecord> first = await RecordsAsync();
        await _harness.RunStepAsync(Ct);
        List<DisposalRecord> second = await RecordsAsync();

        Assert.Equal(1L, await CountAsync(db => db.RecognizedBrowserMarks.LongCountAsync(Ct)));
        Assert.Equal(2, Assert.Single(first, r => r.DisposalRecordType == DisposalRecordType.RecognizedBrowserMark).Count);
        Assert.Equal(first.Count, second.Count);
    }

    [Fact]
    public async Task WhenNothingIsExpired_NoDisposalRecordIsWritten()
    {
        Guid customer = await SeedCustomerAsync("nothing@example.com");
        await _harness.SeedAsync(db => db.RecognizedBrowserMarks.Add(Mark(customer, _harness.Now.AddDays(1))));

        await _harness.RunStepAsync(Ct);

        Assert.Empty(await CountedRecordsAsync());
    }

    // ---- The windows of the temporary records ----

    [Fact]
    public async Task AttemptRecords_AreJudgedByTheirOwnLimitsWindow()
    {
        await _harness.SeedAsync(db =>
        {
            foreach ((AttemptLimit limit, int minutes) in AttemptRecordDisposalPart.WindowMinutes)
            {
                db.AttemptRecords.Add(Attempt(limit, "old-" + limit, _harness.Now - TimeSpan.FromMinutes(minutes + 1)));
                db.AttemptRecords.Add(Attempt(limit, "new-" + limit, _harness.Now - TimeSpan.FromMinutes(minutes - 1)));
            }
        });

        await _harness.RunStepAsync(Ct);

        List<string> left = await _harness.ReadAsync(db => db.AttemptRecords.Select(a => a.NormalizedValue).ToListAsync(Ct));
        string[] expected = [.. AttemptRecordDisposalPart.WindowMinutes.Keys.Select(l => "new-" + l)];
        Assert.Equal(expected.Order(StringComparer.Ordinal), left.Order(StringComparer.Ordinal));
        Assert.Equal(AttemptRecordDisposalPart.WindowMinutes.Count, (await RecordsAsync()).Where(r => r.DisposalRecordType == DisposalRecordType.AttemptRecord).Sum(r => r.Count));
    }

    [Fact]
    public void AttemptWindows_AreTheProductConstants()
    {
        Assert.Equal(ProductConstants.SignInAttemptWindowMinutes, AttemptRecordDisposalPart.WindowMinutes[AttemptLimit.L1]);
        Assert.Equal(ProductConstants.PasswordResetAttemptWindowMinutes, AttemptRecordDisposalPart.WindowMinutes[AttemptLimit.L2]);
        Assert.Equal(ProductConstants.RegistrationAttemptWindowMinutes, AttemptRecordDisposalPart.WindowMinutes[AttemptLimit.L3]);
        Assert.Equal(ProductConstants.ContactFormAttemptWindowMinutes, AttemptRecordDisposalPart.WindowMinutes[AttemptLimit.L4]);
        Assert.Equal(ProductConstants.GuestOrderLookupAttemptWindowMinutes, AttemptRecordDisposalPart.WindowMinutes[AttemptLimit.L5]);
        Assert.Equal(ProductConstants.InvalidCouponAttemptWindowMinutes, AttemptRecordDisposalPart.WindowMinutes[AttemptLimit.L6]);
        Assert.Equal(ProductConstants.VerificationResendAttemptWindowMinutes, AttemptRecordDisposalPart.WindowMinutes[AttemptLimit.L9]);
    }

    [Fact]
    public async Task Sessions_ShortOnesGoAfterTheirHours_LongOnesAfterTheirDays()
    {
        Guid customer = await SeedCustomerAsync("sessions@example.com");
        TimeSpan shortLife = TimeSpan.FromHours(ProductConstants.ShortSessionHours);
        TimeSpan longLife = TimeSpan.FromDays(ProductConstants.LongSessionDays);
        await _harness.SeedAsync(db =>
        {
            db.Sessions.Add(NewSession("short-old", customer, false, _harness.Now - shortLife - TimeSpan.FromMinutes(1)));
            db.Sessions.Add(NewSession("short-new", customer, false, _harness.Now - shortLife + TimeSpan.FromMinutes(1)));
            db.Sessions.Add(NewSession("long-old", customer, true, _harness.Now - longLife - TimeSpan.FromMinutes(1)));
            db.Sessions.Add(NewSession("long-new", customer, true, _harness.Now - longLife + TimeSpan.FromMinutes(1)));
            db.Sessions.Add(NewSession("long-after-short-life", customer, true, _harness.Now - shortLife - TimeSpan.FromHours(1)));
        });

        await _harness.RunStepAsync(Ct);

        List<string> left = await _harness.ReadAsync(db => db.Sessions.Select(s => s.TokenHash).ToListAsync(Ct));
        Assert.Equal(["long-after-short-life", "long-new", "short-new"], left.Order(StringComparer.Ordinal));
        Assert.Equal(2, (await RecordsAsync()).Single(r => r.DisposalRecordType == DisposalRecordType.Session).Count);
    }

    [Fact]
    public async Task RecognizedBrowserMarks_GoWhenExpired()
    {
        Guid customer = await SeedCustomerAsync("marks@example.com");
        await _harness.SeedAsync(db =>
        {
            db.RecognizedBrowserMarks.Add(Mark(customer, _harness.Now.AddSeconds(-1)));
            db.RecognizedBrowserMarks.Add(Mark(customer, _harness.Now.AddSeconds(1)));
        });

        await _harness.RunStepAsync(Ct);

        Assert.Equal(1L, await CountAsync(db => db.RecognizedBrowserMarks.LongCountAsync(Ct)));
    }

    [Fact]
    public async Task GuestCarts_GoAfterTheirIdleDays_AndAMembersCartStays()
    {
        Guid customer = await SeedCustomerAsync("cart@example.com");
        TimeSpan idle = TimeSpan.FromDays(DisposalTimings.GuestCartIdleDays);
        await _harness.SeedAsync(db =>
        {
            Cart old = Cart.ForGuest("guest-old", _harness.Now - idle - TimeSpan.FromDays(2));
            old.LastAccessedAt = _harness.Now - idle - TimeSpan.FromMinutes(1);
            Cart fresh = Cart.ForGuest("guest-new", _harness.Now - idle - TimeSpan.FromDays(2));
            fresh.LastAccessedAt = _harness.Now - idle + TimeSpan.FromMinutes(1);
            Cart member = Cart.ForCustomer(customer, _harness.Now - idle - TimeSpan.FromDays(2));
            db.Carts.AddRange(old, fresh, member);
        });

        await _harness.RunStepAsync(Ct);

        List<Cart> left = await _harness.ReadAsync(db => db.Carts.ToListAsync(Ct));
        Assert.Equal(2, left.Count);
        Assert.DoesNotContain(left, c => c.GuestCartKeyHash == "guest-old");
        Assert.Contains(left, c => c.CustomerId == customer);
        Assert.Equal(1, (await RecordsAsync()).Single(r => r.DisposalRecordType == DisposalRecordType.Cart).Count);
    }

    [Fact]
    public async Task AdminInvitations_AcceptedOnesAndThoseWhoseLinkRanOutGo_ThePendingOneStays()
    {
        Guid pending = Guid.Empty;
        await _harness.SeedAsync(db =>
        {
            AdminInvitation a = Invitation("accepted@example.com");
            a.AcceptedAt = _harness.Now.AddHours(-1);
            AdminInvitation e = Invitation("expired@example.com");
            AdminInvitation p = Invitation("pending@example.com");
            pending = p.Id;
            db.AdminInvitations.AddRange(a, e, p);
            db.LinkRecords.Add(LinkRecord.ForInvitation(a.Id, "link-a", _harness.Now.AddDays(1), _harness.Now.AddDays(-3)));
            db.LinkRecords.Add(LinkRecord.ForInvitation(e.Id, "link-e", _harness.Now.AddSeconds(-1), _harness.Now.AddDays(-3)));
            db.LinkRecords.Add(LinkRecord.ForInvitation(p.Id, "link-p", _harness.Now.AddDays(1), _harness.Now.AddDays(-1)));
        });

        await _harness.RunStepAsync(Ct);

        List<Guid> left = await _harness.ReadAsync(db => db.AdminInvitations.Select(i => i.Id).ToListAsync(Ct));
        Assert.Equal([pending], left);
        Assert.Equal(1L, await CountAsync(db => db.LinkRecords.LongCountAsync(Ct)));
        Assert.Equal(2, (await RecordsAsync()).Single(r => r.DisposalRecordType == DisposalRecordType.AdminInvitation).Count);
    }

    [Fact]
    public async Task ARevertLinksAddress_IsEmptiedWhenItsLifeIsOver_AndTheRowStays_AndAnotherLinkRowIsUntouched()
    {
        Guid customer = await SeedCustomerAsync("revert@example.com");
        await _harness.SeedAsync(db =>
        {
            LinkRecord over = LinkRecord.ForCustomer(LinkType.EmailChangeRevert, customer, "revert-over", _harness.Now.AddSeconds(-1), _harness.Now.AddDays(-5));
            over.PreviousEmail = "old@example.com";
            LinkRecord alive = LinkRecord.ForCustomer(LinkType.EmailChangeRevert, customer, "revert-alive", _harness.Now.AddDays(1), _harness.Now.AddDays(-1));
            alive.PreviousEmail = "older@example.com";
            LinkRecord verification = LinkRecord.ForCustomer(LinkType.EmailVerification, customer, "verification-over", _harness.Now.AddDays(-1), _harness.Now.AddDays(-4));
            db.LinkRecords.AddRange(over, alive, verification);
        });

        await _harness.RunStepAsync(Ct);

        Dictionary<string, string?> rows = await _harness.ReadAsync(db => db.LinkRecords.ToDictionaryAsync(l => l.TokenHash, l => l.PreviousEmail, Ct));
        Assert.Equal(3, rows.Count);
        Assert.Null(rows["revert-over"]);
        Assert.Equal("older@example.com", rows["revert-alive"]);
        Assert.Null(rows["verification-over"]);
        Assert.Equal(1, (await RecordsAsync()).Single(r => r.DisposalRecordType == DisposalRecordType.LinkRecord).Count);
    }

    [Fact]
    public async Task AFailedJobRow_IsLeftInPlace()
    {
        await _harness.SeedAsync(db =>
        {
            Job failed = Job.Enqueue(JobType.BounceQuery, null, null, _harness.Now.AddYears(-1));
            failed.JobState = JobState.Failed;
            failed.AttemptCount = JobTimings.DefaultRetrySchedule.Count + 1;
            failed.LastErrorSummary = "BounceQuery 57P01";
            db.Jobs.Add(failed);
        });

        await _harness.RunStepAsync(Ct);

        Job left = await _harness.ReadAsync(db => db.Jobs.SingleAsync(Ct));
        Assert.Equal(JobState.Failed, left.JobState);
        Assert.Empty(await CountedRecordsAsync());
    }

    // ---- The audit trail (Z-30) ----

    [Fact]
    public async Task AuditLog_ARowPastTenYearsGoes_TodaysStays_AndTheRecordCountsOne()
    {
        await _harness.SeedAsync(db =>
        {
            db.AuditLogs.Add(Audit(_harness.Now.AddYears(-12)));
            db.AuditLogs.Add(Audit(_harness.Now));
        });

        await _harness.RunStepAsync(Ct);

        Assert.Equal(1L, await CountAsync(db => db.AuditLogs.LongCountAsync(Ct)));
        Assert.Equal(1, (await RecordsAsync()).Single(r => r.DisposalRecordType == DisposalRecordType.AuditLog).Count);
    }

    [Fact]
    public async Task TheApplicationUser_CannotDeleteFromTheAuditTrailItself()
    {
        await _harness.SeedAsync(db => db.AuditLogs.Add(Audit(_harness.Now)));
        TestDatabaseUsers users = _template.Users;
        string appDsn = _template.DsnFor(_collection.DatabaseName, users.App, users.AppPassword, pooling: false);

        await using NpgsqlConnection connection = new(appDsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand delete = new("DELETE FROM audit_logs", connection);
        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => delete.ExecuteNonQueryAsync(Ct));

        Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, error.SqlState);
        Assert.Equal(1L, await CountAsync(db => db.AuditLogs.LongCountAsync(Ct)));
    }

    // ---- Log files ----

    [Fact]
    public async Task LogFiles_AreJudgedByTheirLastWrite_NotByTheDayInTheirName()
    {
        DateTimeOffset cutoff = LogFileRetention.Cutoff(_harness.Now);
        string expiredApp = WriteLog(ApplicationDirectory, "app-20200101.jsonl", cutoff.AddDays(-1));
        string expiredAudit = WriteLog(ApplicationDirectory, "audit-20200102.jsonl", cutoff.AddDays(-30));
        string expiredAccess = WriteLog(AccessDirectory, "access.log-20200103", cutoff.AddDays(-1));
        string freshWithOldName = WriteLog(ApplicationDirectory, "command-20150101.jsonl", cutoff.AddDays(1));
        string activeAccess = WriteLog(AccessDirectory, "access.log", cutoff.AddDays(-400));
        string foreign = WriteLog(ApplicationDirectory, "notes.txt", cutoff.AddDays(-400));

        await _harness.RunStepAsync(Ct);

        Assert.False(File.Exists(expiredApp));
        Assert.False(File.Exists(expiredAudit));
        Assert.False(File.Exists(expiredAccess));
        Assert.True(File.Exists(freshWithOldName));
        Assert.True(File.Exists(activeAccess));
        Assert.True(File.Exists(foreign));
        Assert.Equal(3, (await RecordsAsync()).Single(r => r.DisposalRecordType == DisposalRecordType.LogFile).Count);
    }

    [Fact]
    public async Task TheServerLogRotation_IsWrittenAsARange_NotTwiceForTheSameDays_AndFillsSkippedDays()
    {
        await _harness.RunStepAsync(Ct);
        DateOnly firstNewest = LogFileRetention.NewestExpiredDay(_harness.Now);
        await _harness.RunStepAsync(Ct);
        List<DisposalRecord> afterTwo = await RotationRowsAsync();

        _harness.Time.SetUtcNow(_harness.Now.AddDays(3));
        await _harness.RunStepAsync(Ct);
        List<DisposalRecord> afterSkip = await RotationRowsAsync();

        DisposalRecord only = Assert.Single(afterTwo);
        Assert.Null(only.Count);
        Assert.Equal(firstNewest, only.RangeStartDate);
        Assert.Equal(firstNewest, only.RangeEndDate);
        Assert.Equal(2, afterSkip.Count);
        DisposalRecord second = afterSkip.OrderBy(r => r.RangeStartDate).Last();
        Assert.Equal(firstNewest.AddDays(1), second.RangeStartDate);
        Assert.Equal(LogFileRetention.NewestExpiredDay(_harness.Now), second.RangeEndDate);
    }

    // ---- KK5: the record and the deletion are one transaction ----

    [Fact]
    public async Task WhenTheDisposalRecordIsRefused_TheDeletedRowsComeBack()
    {
        await _harness.SeedAsync(db => db.AttemptRecords.Add(Attempt(AttemptLimit.L1, "kept-by-rollback", _harness.Now.AddYears(-1))));

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => _harness.WithRunAsync(
            (run, _) => run.ChunksAsync(
                DisposalRecordType.AttemptRecord,
                async (db, max, cancel) =>
                {
                    int deleted = await db.AttemptRecords.ExecuteDeleteAsync(cancel);

                    // A count of zero breaks the table's rule (06 4.64): the whole chunk must be undone.
                    db.DisposalRecords.Add(DisposalRecord.Counted(DisposalRecordType.LogFile, 0, _harness.Now));
                    return deleted;
                },
                Ct)));

        Assert.Equal(1L, await CountAsync(db => db.AttemptRecords.LongCountAsync(Ct)));
        Assert.Empty(await CountedRecordsAsync());
    }

    // ---- Helpers ----

    private async Task<long> CountAsync(Func<AppDbContext, Task<long>> count) => await _harness.ReadAsync(count);

    private Task<List<DisposalRecord>> RecordsAsync() => _harness.ReadAsync(db => db.DisposalRecords.AsNoTracking().ToListAsync(Ct));

    /// <summary>The rows with a number: the server's log rotation row is written on every run (a range, no number) and is read on its own.</summary>
    private async Task<List<DisposalRecord>> CountedRecordsAsync() =>
        [.. (await RecordsAsync()).Where(r => r.DisposalRecordType != DisposalRecordType.ServerLogRotation)];

    private async Task<List<DisposalRecord>> RotationRowsAsync() =>
        [.. (await RecordsAsync()).Where(r => r.DisposalRecordType == DisposalRecordType.ServerLogRotation)];

    private async Task SeedHeartbeatAsync(DateTimeOffset lastSucceededAt)
    {
        await _harness.SeedAsync(db =>
        {
            HeartbeatRecord heartbeat = HeartbeatRecord.Create(lastSucceededAt);
            heartbeat.NightlyJobLastSucceededAt = lastSucceededAt;
            db.HeartbeatRecords.Add(heartbeat);
        });
    }

    private async Task<Guid> SeedCustomerAsync(string email)
    {
        Customer customer = Customer.Create("Ali", email, "digest-placeholder", _harness.Now, null, _harness.Now);
        await _harness.SeedAsync(db => db.Customers.Add(customer));
        return customer.Id;
    }

    private RecognizedBrowserMark Mark(Guid customerId, DateTimeOffset expiresAt)
    {
        return RecognizedBrowserMark.ForCustomer(customerId, "mark-" + Guid.NewGuid().ToString("N"), expiresAt, _harness.Now.AddDays(-30));
    }

    private static AttemptRecord Attempt(AttemptLimit limit, string value, DateTimeOffset at) =>
        AttemptRecord.Create(limit, limit == AttemptLimit.L4 ? AttemptAxis.Ip : limit == AttemptLimit.L6 ? AttemptAxis.Cart : AttemptAxis.Email, value, at);

    private static Session NewSession(string tokenHash, Guid customerId, bool isLongLived, DateTimeOffset lastActivityAt)
    {
        Session session = Session.ForCustomer(customerId, tokenHash, isLongLived, lastActivityAt);
        return session;
    }

    private AdminInvitation Invitation(string email) => AdminInvitation.Create(email, null, "Ali", _harness.Now.AddDays(-3));

    private static AuditLog Audit(DateTimeOffset at) =>
        AuditLog.Create(AuditOperation.SendAdminInvitation, Guid.NewGuid(), "Ali", "ali@example.com", null, null, null, at);

    private static string WriteLog(string directory, string name, DateTimeOffset lastWrite)
    {
        string path = Path.Combine(directory, name);
        File.WriteAllText(path, "{}");
        File.SetLastWriteTimeUtc(path, lastWrite.UtcDateTime);
        return path;
    }
}
