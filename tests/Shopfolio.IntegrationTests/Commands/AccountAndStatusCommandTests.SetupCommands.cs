using System.Net;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Commands;

/// <summary>
/// The installer's commands of T387 on a real PostgreSQL (05 8.5.5, 8.5.9, 8.5.12; 09 13.2 items 3, 5, 7): <c>job</c> requeues or closes a failed row and the job queue condition clears
/// (KK1); <c>read-logs</c> reads each source by range and filter into a CSV in the export directory and its own line is on the disk before the read, so the read finds it (KK2, KK5);
/// a piped input with an IP filter ends with 2 and no line in a real process (KK3); <c>apply-payment-result</c> refuses an unmarked order and an amount that is not the total with 3
/// and no write, and applies a paid or a "none" result through the one transition path (KK7, KK8).
/// </summary>
public sealed partial class AccountAndStatusCommandTests
{
    private readonly string _exportDirectory = Path.Combine(Path.GetTempPath(), "shopfolio-t387-export-" + Guid.NewGuid().ToString("N"));

    // ---- KK1: job ----

    [Theory]
    [InlineData("requeue")]
    [InlineData("close")]
    public async Task Job_FailedRow_IsRequeuedOrClosed_AndTheJobQueueConditionClears(string action)
    {
        Guid jobId = await SeedFailedJobAsync();
        FakeTerminal before = new() { IsInputRedirected = false };
        await RunAsync(before, "status");
        Assert.Contains("  JobQueue: AÇIK", before.Output, StringComparison.Ordinal);

        int exit = await RunAsync(new FakeTerminal(), "job", "--id", jobId.ToString(), "--action", action);

        Assert.Equal(0, exit);
        string row = await ScalarAsync<string>($"SELECT job_state || '|' || attempt_count || '|' || coalesce(next_attempt_at::text, '-') || '|' || (completed_at IS NULL) FROM jobs WHERE id = '{jobId}'");
        Assert.Equal(action == "requeue" ? "Pending|0|-|True" : $"Completed|{JobTimings.DefaultRetrySchedule.Count + 1}|-|False", row.Replace("true", "True", StringComparison.Ordinal).Replace("false", "False", StringComparison.Ordinal));

        if (action == "requeue")
        {
            // A requeued row has no next attempt (09 13.2 item 7), so it is due at once and its age is its creation: the condition stays open until the running application's runner
            // takes it - the command does not run it (05 8.5.7). The failed row itself is gone; the runner's work is done here by hand.
            FakeTerminal requeued = new() { IsInputRedirected = false };
            await RunAsync(requeued, "status");
            Assert.Contains("Başarısız iş satırı: 0", requeued.Output, StringComparison.Ordinal);
            await ExecuteAsync($"UPDATE jobs SET job_state = 'Completed', completed_at = now() WHERE id = '{jobId}'");
        }

        FakeTerminal after = new() { IsInputRedirected = false };
        await RunAsync(after, "status");
        Assert.DoesNotContain("  JobQueue: AÇIK", after.Output, StringComparison.Ordinal);
        Assert.Contains("job", ReadShared(Assert.Single(Directory.GetFiles(_logDirectory, "command-*.jsonl"))), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Job_RowThatIsNotFailed_OrIsGone_IsRefusedWithThree()
    {
        Guid jobId = await SeedFailedJobAsync();
        await ExecuteAsync($"UPDATE jobs SET job_state = 'Pending' WHERE id = '{jobId}'");

        Assert.Equal(3, await RunAsync(new FakeTerminal(), "job", "--id", jobId.ToString(), "--action", "close"));
        Assert.Equal(3, await RunAsync(new FakeTerminal(), "job", "--id", Guid.NewGuid().ToString(), "--action", "requeue"));
    }

    // ---- KK2, KK5: read-logs ----

    [Fact]
    public async Task ReadLogs_System_ByEvent_FindsItsOwnLine_WrittenBeforeTheRead_AndLeavesOutOtherDaysAndEvents()
    {
        Directory.CreateDirectory(_logDirectory);
        string today = TurkeyTime.DateOf(DateTimeOffset.UtcNow).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
        await File.WriteAllLinesAsync(
            Path.Combine(_logDirectory, $"app-{today}.jsonl"),
            [
                $$"""{"time":"{{DateTimeOffset.UtcNow.AddMinutes(-1):O}}","event":"JobStarted","message":"other event"}""",
                """{"time":"2020-01-01T10:00:00Z","event":"AdminCommand","message":"another day"}""",
                "not a json line",
            ],
            Ct);
        string day = TurkeyTime.DateOf(DateTimeOffset.UtcNow).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        FakeTerminal terminal = new();

        int exit = await RunAsync(terminal, "read-logs", "--source", "system", "--from", day, "--to", day, "--filter", "event:AdminCommand");

        Assert.Equal(0, exit);
        string csv = await File.ReadAllTextAsync(Assert.Single(Directory.GetFiles(_exportDirectory, "*.csv")), Ct);
        Assert.Contains("read-logs", csv, StringComparison.Ordinal);
        Assert.Contains("\"\"logSource\"\":\"\"System\"\"", csv, StringComparison.Ordinal);
        Assert.DoesNotContain("other event", csv, StringComparison.Ordinal);
        Assert.DoesNotContain("another day", csv, StringComparison.Ordinal);
        Assert.Contains("Satır: 1", terminal.Output, StringComparison.Ordinal);
        Assert.Contains("Okunamayan satır (JSON ya da zaman yok): 1", terminal.Output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ReadLogs_Login_ByEvent_AndDisposal_ByRange_WriteTheRowsOfTheRangeOnly()
    {
        await ExecuteAsync("TRUNCATE disposal_records");
        DateTimeOffset now = DateTimeOffset.UtcNow;
        await using (AppDbContext db = NewContext())
        {
            db.LoginRecords.Add(LoginRecord.Create(IPAddress.Parse("198.51.100.7"), AccountType.Admin, "Kayit.Bir@Example.com", LoginOutcome.Failed, now));
            db.LoginRecords.Add(LoginRecord.Create(IPAddress.Parse("198.51.100.8"), AccountType.Customer, "kayit.iki@example.com", LoginOutcome.Succeeded, now));
            db.LoginRecords.Add(LoginRecord.Create(IPAddress.Parse("198.51.100.9"), AccountType.Admin, "eski@example.com", LoginOutcome.Failed, now.AddDays(-40)));
            await db.SaveChangesAsync(Ct);
        }

        string day = TurkeyTime.DateOf(now).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        FakeTerminal login = new();
        Assert.Equal(0, await RunAsync(login, "read-logs", "--source", "login", "--from", day, "--to", day, "--filter", "event:Failed"));
        string csv = await File.ReadAllTextAsync(Assert.Single(Directory.GetFiles(_exportDirectory, "*.csv")), Ct);
        Assert.Contains("198.51.100.7", csv, StringComparison.Ordinal);
        Assert.Contains("kayit.bir@example.com", csv, StringComparison.Ordinal);
        Assert.DoesNotContain("198.51.100.8", csv, StringComparison.Ordinal);
        Assert.DoesNotContain("198.51.100.9", csv, StringComparison.Ordinal);

        FakeTerminal disposal = new();
        Assert.Equal(0, await RunAsync(disposal, "read-logs", "--source", "disposal", "--from", day, "--to", day));
        Assert.Contains("Satır: 0", disposal.Output, StringComparison.Ordinal);
        Assert.Equal(2, Directory.GetFiles(_exportDirectory, "*.csv").Length);
    }

    [Fact]
    public async Task ReadLogs_Login_ByEmailFromTheTerminal_ReadsTheNormalizedAddress_AndTheLineHoldsNoValue()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        await using (AppDbContext db = NewContext())
        {
            db.LoginRecords.Add(LoginRecord.Create(IPAddress.Parse("198.51.100.7"), AccountType.Admin, EmailA, LoginOutcome.Failed, now));
            db.LoginRecords.Add(LoginRecord.Create(IPAddress.Parse("198.51.100.8"), AccountType.Admin, EmailB, LoginOutcome.Failed, now));
            await db.SaveChangesAsync(Ct);
        }

        string day = TurkeyTime.DateOf(now).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(0, await RunAsync(Interactive("  AYSE.Yilmaz@Example.com "), "read-logs", "--source", "login", "--from", day, "--to", day, "--filter", "email"));

        string csv = await File.ReadAllTextAsync(Assert.Single(Directory.GetFiles(_exportDirectory, "*.csv")), Ct);
        Assert.Contains("198.51.100.7", csv, StringComparison.Ordinal);
        Assert.DoesNotContain("198.51.100.8", csv, StringComparison.Ordinal);
        string line = ReadShared(Assert.Single(Directory.GetFiles(_logDirectory, "command-*.jsonl")));
        Assert.DoesNotContain("@", line, StringComparison.Ordinal);
        Assert.Contains("\"filterKind\":\"Email\"", line, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ReadLogs_RealProcess_PipedInput_WithAnIpFilter_ExitsWithTwo_AndWritesNoLine()
    {
        (int exit, string output) = await RunProcessAsync(["admin", "read-logs", "--source", "login", "--from", "2026-10-01", "--to", "2026-10-31", "--filter", "ip"], "198.51.100.7\n");

        Assert.Equal(2, exit);
        Assert.DoesNotContain("198.51.100.7", output, StringComparison.Ordinal);
        Assert.False(Directory.Exists(_logDirectory) && Directory.GetFiles(_logDirectory, "command-*.jsonl").Length > 0);
    }

    // ---- KK7, KK8: apply-payment-result ----

    [Fact]
    public async Task ApplyPaymentResult_UnmarkedOrder_AndAnAmountThatIsNotTheTotal_AreRefusedWithThree_AndWriteNothing()
    {
        Guid running = await SeedCardOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);
        Guid marked = await SeedCardOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);
        await ExecuteAsync($"UPDATE orders SET payment_window_ends_at = now() - interval '1 hour' WHERE id = '{marked}'");
        Guid transfer = await SeedCardOrderAsync(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.BankTransfer);
        await ExecuteAsync($"UPDATE orders SET payment_window_ends_at = now() - interval '1 hour' WHERE id = '{transfer}'");
        string before = await SnapshotAsync();

        Assert.Equal(3, await RunAsync(new FakeTerminal(), "apply-payment-result", "--order-number", await NumberOfAsync(running), "--result", "none"));
        Assert.Equal(3, await RunAsync(new FakeTerminal(), "apply-payment-result", "--order-number", await NumberOfAsync(transfer), "--result", "none"));
        Assert.Equal(3, await RunAsync(new FakeTerminal(), "apply-payment-result", "--order-number", await NumberOfAsync(marked), "--result", "paid", "--amount", "119.99"));
        Assert.Equal(3, await RunAsync(new FakeTerminal(), "apply-payment-result", "--order-number", "2026-000000", "--result", "none"));

        Assert.Equal(before, await SnapshotAsync());
    }

    [Fact]
    public async Task ApplyPaymentResult_Paid_WithTheTotal_MakesTheOrderPaid_ThroughTheTransitionPath()
    {
        Guid marked = await SeedCardOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);
        await ExecuteAsync($"UPDATE orders SET payment_window_ends_at = now() - interval '1 hour' WHERE id = '{marked}'");

        int exit = await RunAsync(new FakeTerminal(), "apply-payment-result", "--order-number", await NumberOfAsync(marked), "--result", "paid", "--amount", "120");

        Assert.Equal(0, exit);
        Assert.Equal("Paid", await ScalarAsync<string>($"SELECT payment_status FROM orders WHERE id = '{marked}'"));
        Assert.True(await ScalarAsync<long>("SELECT count(*) FROM dispatch_records") + await ScalarAsync<long>("SELECT count(*) FROM jobs") > 0);
    }

    [Fact]
    public async Task ApplyPaymentResult_None_CancelsTheOrder_ThroughTheTransitionPath()
    {
        Guid marked = await SeedCardOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);
        await ExecuteAsync($"UPDATE orders SET payment_window_ends_at = now() - interval '1 hour' WHERE id = '{marked}'");

        int exit = await RunAsync(new FakeTerminal(), "apply-payment-result", "--order-number", await NumberOfAsync(marked), "--result", "none");

        Assert.Equal(0, exit);
        Assert.Equal("Cancelled", await ScalarAsync<string>($"SELECT order_status FROM orders WHERE id = '{marked}'"));
        Assert.True(await ScalarAsync<long>("SELECT count(*) FROM dispatch_records") + await ScalarAsync<long>("SELECT count(*) FROM jobs") > 0);
    }

    private Task<string> NumberOfAsync(Guid orderId) => ScalarAsync<string>($"SELECT order_number FROM orders WHERE id = '{orderId}'");

    private async Task<Guid> SeedFailedJobAsync()
    {
        await using AppDbContext db = NewContext();
        Job failed = Job.Enqueue(JobType.CardRefund, EntityIds.New(), null, DateTimeOffset.UtcNow.AddHours(-2));
        failed.JobState = JobState.Failed;
        failed.AttemptCount = JobTimings.DefaultRetrySchedule.Count + 1;
        failed.LastErrorSummary = nameof(InvalidOperationException);
        db.Jobs.Add(failed);
        await db.SaveChangesAsync(Ct);
        return failed.Id;
    }
}
