using System.Net;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.LogRead;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
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
    public async Task ReadLogs_System_ReadsTheServersLogs_TheDatabasesTheBackupToolsAndTheJournal_ByRangeAndFilter()
    {
        string server = Path.Combine(Path.GetTempPath(), "shopfolio-t387-server-" + Guid.NewGuid().ToString("N"));
        try
        {
            DateTimeOffset now = DateTimeOffset.UtcNow.AddMinutes(-1);
            string stamp = now.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
            string iso = now.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", System.Globalization.CultureInfo.InvariantCulture);
            Directory.CreateDirectory(Path.Combine(server, "db", "server"));
            Directory.CreateDirectory(Path.Combine(server, "db", "pgbackrest"));
            Directory.CreateDirectory(Path.Combine(server, "backup"));
            Directory.CreateDirectory(Path.Combine(server, "journal", "machine"));
            await File.WriteAllLinesAsync(
                Path.Combine(server, "db", "server", "postgresql.log"),
                [$"{stamp} UTC [57] LOG:  connection received: host=203.0.113.7 port=50122", "2020-01-01 00:00:00.000 UTC [1] LOG:  an old day"],
                Ct);
            await File.WriteAllLinesAsync(Path.Combine(server, "db", "pgbackrest", "main-backup.log"), [$"{stamp} P00   INFO: backup command begin"], Ct);
            await File.WriteAllLinesAsync(
                Path.Combine(server, "backup", "file-backup.log"),
                ["a line before any time", $"file-backup: round starts at {iso}", "file-backup: step backup starts"],
                Ct);
            await File.WriteAllBytesAsync(
                Path.Combine(server, "journal", "machine", "system.journal"),
                JournalBytes(
                    compact: false,
                    (now, [("MESSAGE=Accepted publickey for root from 203.0.113.7 port 50122 ssh2", false), ("SYSLOG_IDENTIFIER=sshd", false), ("_PID=812", false), ("_SYSTEMD_UNIT=ssh.service", false)]),
                    (now, [("MESSAGE=a compressed message", true), ("SYSLOG_IDENTIFIER=kernel", false)]),
                    (new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero), [("MESSAGE=journal old day", false)])),
                Ct);
            await File.WriteAllBytesAsync(
                Path.Combine(server, "journal", "machine", "user-1000.journal"),
                JournalBytes(compact: true, (now, [("MESSAGE=compact entry", false), ("_COMM=dockerd", false)])),
                Ct);
            ServerLogDirectories directories = new(Path.Combine(server, "db"), Path.Combine(server, "backup"), Path.Combine(server, "journal"));
            void Adjust(IServiceCollection services)
            {
                services.RemoveAll<ServerLogDirectories>();
                services.AddSingleton(directories);
            }

            string day = TurkeyTime.DateOf(now).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            FakeTerminal all = new();
            Assert.Equal(0, await RunAsync(all, Adjust, "read-logs", "--source", "system", "--from", day, "--to", day));
            string csv = await File.ReadAllTextAsync(Assert.Single(Directory.GetFiles(_exportDirectory, "*.csv")), Ct);
            Assert.Contains("db/server/postgresql.log;", csv, StringComparison.Ordinal);
            Assert.Contains("connection received: host=203.0.113.7", csv, StringComparison.Ordinal);
            Assert.Contains("db/pgbackrest/main-backup.log;", csv, StringComparison.Ordinal);
            Assert.Contains("backup/file-backup.log;", csv, StringComparison.Ordinal);
            Assert.Contains("file-backup: step backup starts", csv, StringComparison.Ordinal);
            Assert.Contains("journal/machine/system.journal;", csv, StringComparison.Ordinal);
            Assert.Contains("ssh.service sshd[812]: Accepted publickey for root from 203.0.113.7 port 50122 ssh2", csv, StringComparison.Ordinal);
            Assert.Contains("journal/machine/user-1000.journal;", csv, StringComparison.Ordinal);
            Assert.Contains("dockerd: compact entry", csv, StringComparison.Ordinal);
            Assert.DoesNotContain("an old day", csv, StringComparison.Ordinal);
            Assert.DoesNotContain("journal old day", csv, StringComparison.Ordinal);
            Assert.DoesNotContain("a line before any time", csv, StringComparison.Ordinal);
            Assert.DoesNotContain("a compressed message", csv, StringComparison.Ordinal);
            // Server rows: the database 1, pgBackRest 1, the backup tool 2, the journal 2; and the command's own line in the application's directory.
            Assert.Contains("Satır: 7", all.Output, StringComparison.Ordinal);
            Assert.Contains("Okunamayan satır (JSON ya da zaman yok): 2", all.Output, StringComparison.Ordinal);
            // The files read are not changed (05 9.3.1).
            Assert.Equal(2, (await File.ReadAllLinesAsync(Path.Combine(server, "db", "server", "postgresql.log"), Ct)).Length);

            File.Delete(Assert.Single(Directory.GetFiles(_exportDirectory, "*.csv")));
            FakeTerminal byIp = new FakeTerminal { IsInputRedirected = false }.Answer("203.0.113.7");
            Assert.Equal(0, await RunAsync(byIp, Adjust, "read-logs", "--source", "system", "--from", day, "--to", day, "--filter", "ip"));
            csv = await File.ReadAllTextAsync(Assert.Single(Directory.GetFiles(_exportDirectory, "*.csv")), Ct);
            Assert.Contains("db/server/postgresql.log;", csv, StringComparison.Ordinal);
            Assert.Contains("journal/machine/system.journal;", csv, StringComparison.Ordinal);
            Assert.DoesNotContain("backup/file-backup.log;", csv, StringComparison.Ordinal);
            Assert.Contains("Satır: 2", byIp.Output, StringComparison.Ordinal);

            File.Delete(Assert.Single(Directory.GetFiles(_exportDirectory, "*.csv")));
            FakeTerminal byProgram = new();
            Assert.Equal(0, await RunAsync(byProgram, Adjust, "read-logs", "--source", "system", "--from", day, "--to", day, "--filter", "event:sshd"));
            csv = await File.ReadAllTextAsync(Assert.Single(Directory.GetFiles(_exportDirectory, "*.csv")), Ct);
            Assert.Contains("sshd[812]", csv, StringComparison.Ordinal);
            Assert.Contains("Satır: 1", byProgram.Output, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(server))
            {
                Directory.Delete(server, recursive: true);
            }
        }
    }

    // A systemd journal file (the documented "Journal File Format"): a header with the signature, the incompatible flags (compact: 16), the header's size and the tail object's
    // offset; then for each entry its data objects (FIELD=value; a compressed one flagged ZSTD) and the entry object with its realtime moment and the items pointing at them.
    private static byte[] JournalBytes(bool compact, params (DateTimeOffset Time, (string Field, bool Compressed)[] Fields)[] entries)
    {
        const int headerLength = 160;
        using MemoryStream stream = new();
        stream.Write(new byte[headerLength]);
        long tail = headerLength;
        foreach ((DateTimeOffset time, (string Field, bool Compressed)[] fields) in entries)
        {
            List<long> offsets = [];
            foreach ((string field, bool compressed) in fields)
            {
                byte[] payload = System.Text.Encoding.UTF8.GetBytes(field);
                int payloadStart = compact ? 72 : 64;
                tail = stream.Position;
                offsets.Add(tail);
                WriteObject(stream, 1, compressed ? (byte)4 : (byte)0, new byte[payloadStart - 16], payload);
            }

            int itemLength = compact ? 4 : 16;
            byte[] body = new byte[48 + (offsets.Count * itemLength)];
            System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(body.AsSpan(8), (ulong)(time.ToUnixTimeMilliseconds() * 1000));
            for (int i = 0; i < offsets.Count; i++)
            {
                if (compact)
                {
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(body.AsSpan(48 + (i * itemLength)), (uint)offsets[i]);
                }
                else
                {
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(body.AsSpan(48 + (i * itemLength)), (ulong)offsets[i]);
                }
            }

            tail = stream.Position;
            WriteObject(stream, 3, 0, body, []);
        }

        byte[] bytes = stream.ToArray();
        "LPKSHHRH"u8.CopyTo(bytes);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(12), compact ? 16u : 0u);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(88), headerLength);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(136), (ulong)tail);
        return bytes;
    }

    private static void WriteObject(MemoryStream stream, byte type, byte flags, byte[] fields, byte[] payload)
    {
        byte[] header = new byte[16];
        header[0] = type;
        header[1] = flags;
        System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(header.AsSpan(8), (ulong)(16 + fields.Length + payload.Length));
        stream.Write(header);
        stream.Write(fields);
        stream.Write(payload);
        while (stream.Position % 8 != 0)
        {
            stream.WriteByte(0);
        }
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
        Assert.Equal(3, await RunAsync(new FakeTerminal(), "apply-payment-result", "--order-number", await NumberOfAsync(running), "--result", "paid", "--amount", "120"));
        Assert.Equal(3, await RunAsync(new FakeTerminal(), "apply-payment-result", "--order-number", await NumberOfAsync(transfer), "--result", "none"));
        Assert.Equal(3, await RunAsync(new FakeTerminal(), "apply-payment-result", "--order-number", await NumberOfAsync(marked), "--result", "paid", "--amount", "119.99"));
        Assert.Equal(3, await RunAsync(new FakeTerminal(), "apply-payment-result", "--order-number", "2026-000000", "--result", "none"));

        Assert.Equal(before, await SnapshotAsync());
        // Every run wrote its line before the refusal: the order's id and the result; the unknown number with the empty id (05 8.5.12; 09 13.2 item 5).
        string[] lines = ReadShared(Assert.Single(Directory.GetFiles(_logDirectory, "command-*.jsonl"))).Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(5, lines.Length);
        Assert.Contains(running.ToString(), lines[1], StringComparison.Ordinal);
        Assert.Contains("\"paymentResult\":\"Paid\"", lines[1], StringComparison.Ordinal);
        Assert.Contains("\"amountInKurus\":\"12000\"", lines[1], StringComparison.Ordinal);
        Assert.Contains(Guid.Empty.ToString(), lines[4], StringComparison.Ordinal);
        Assert.Contains("\"paymentResult\":\"None\"", lines[4], StringComparison.Ordinal);
        Assert.DoesNotContain(await NumberOfAsync(running), lines[1], StringComparison.Ordinal);
    }

    [Fact]
    public async Task ApplyPaymentResult_Paid_WithTheTotal_MakesTheOrderPaid_ThroughTheTransitionPath()
    {
        Guid marked = await SeedCardOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);
        await ExecuteAsync($"UPDATE orders SET payment_window_ends_at = now() - interval '1 hour' WHERE id = '{marked}'");
        Guid variant = await SeedReservedVariantAsync(marked);

        int exit = await RunAsync(new FakeTerminal(), "apply-payment-result", "--order-number", await NumberOfAsync(marked), "--result", "paid", "--amount", "120");

        Assert.Equal(0, exit);
        // The reserved unit leaves the stock for good: stock 10 → 9, reserved 1 → 0, the reservation row is gone (06 4.14; 02 3.17.5).
        Assert.Equal("9/0/0", await StockOfAsync(variant));
        Assert.Equal("Paid", await ScalarAsync<string>($"SELECT payment_status FROM orders WHERE id = '{marked}'"));
        // The one path's own writes: its transition records (Ö1 with S1, D-389) and one B-4 row queued for the running application, nothing sent by the command (05 8.5.7).
        Assert.Equal("O1,S1", await ScalarAsync<string>($"SELECT string_agg(transition_code, ',' ORDER BY transition_code) FROM transition_records WHERE order_id = '{marked}'"));
        Assert.Equal("B4", await ScalarAsync<string>($"SELECT string_agg(dispatch_type, ',' ORDER BY dispatch_type) FROM dispatch_records WHERE order_id = '{marked}'"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM jobs WHERE job_state <> 'Pending'"));
        // The physical order is Preparing (02 5.5 Ö1 with S1) and the "payment outcome unknown" mark is gone: a second run is refused with 3 and writes nothing (VAL-C115).
        Assert.Equal("Preparing", await ScalarAsync<string>($"SELECT order_status FROM orders WHERE id = '{marked}'"));
        string after = await SnapshotAsync();
        Assert.Equal(3, await RunAsync(new FakeTerminal(), "apply-payment-result", "--order-number", await NumberOfAsync(marked), "--result", "paid", "--amount", "120"));
        Assert.Equal(after, await SnapshotAsync());
        // The line of the first run carries the order's id, the result and the amount in kuruş; written before the transaction (05 8.5.12).
        string line = ReadShared(Assert.Single(Directory.GetFiles(_logDirectory, "command-*.jsonl"))).Split('\n', StringSplitOptions.RemoveEmptyEntries)[0];
        Assert.Contains(marked.ToString(), line, StringComparison.Ordinal);
        Assert.Contains("\"command\":\"apply-payment-result\"", line, StringComparison.Ordinal);
        Assert.Contains("\"paymentResult\":\"Paid\"", line, StringComparison.Ordinal);
        Assert.Contains("\"amountInKurus\":\"12000\"", line, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ApplyPaymentResult_None_CancelsTheOrder_ThroughTheTransitionPath()
    {
        Guid marked = await SeedCardOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);
        await ExecuteAsync($"UPDATE orders SET payment_window_ends_at = now() - interval '1 hour' WHERE id = '{marked}'");
        Guid variant = await SeedReservedVariantAsync(marked);

        int exit = await RunAsync(new FakeTerminal(), "apply-payment-result", "--order-number", await NumberOfAsync(marked), "--result", "none");

        Assert.Equal(0, exit);
        // The reservation is freed: stock stays 10, reserved 1 → 0, the reservation row is gone (06 4.14 path 3).
        Assert.Equal("10/0/0", await StockOfAsync(variant));
        Assert.Equal("Cancelled", await ScalarAsync<string>($"SELECT order_status FROM orders WHERE id = '{marked}'"));
        // The system's cancellation by the payment window's rule: S3 with Ö2 and one B-7 row queued for the running application, nothing sent by the command (05 8.5.7).
        Assert.Equal("O2,S3", await ScalarAsync<string>($"SELECT string_agg(transition_code, ',' ORDER BY transition_code) FROM transition_records WHERE order_id = '{marked}'"));
        Assert.Equal("B7", await ScalarAsync<string>($"SELECT string_agg(dispatch_type, ',' ORDER BY dispatch_type) FROM dispatch_records WHERE order_id = '{marked}'"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM jobs WHERE job_state <> 'Pending'"));
        // The mark is gone with the cancellation: a second run is refused with 3 (VAL-C115).
        Assert.Equal(3, await RunAsync(new FakeTerminal(), "apply-payment-result", "--order-number", await NumberOfAsync(marked), "--result", "none"));
        string line = ReadShared(Assert.Single(Directory.GetFiles(_logDirectory, "command-*.jsonl"))).Split('\n', StringSplitOptions.RemoveEmptyEntries)[0];
        Assert.Contains(marked.ToString(), line, StringComparison.Ordinal);
        Assert.Contains("\"paymentResult\":\"None\"", line, StringComparison.Ordinal);
    }

    private Task<string> NumberOfAsync(Guid orderId) => ScalarAsync<string>($"SELECT order_number FROM orders WHERE id = '{orderId}'");

    // A physical variant with ten in stock and one unit reserved by the order (06 4.14): the payment commits the unit, the cancellation frees it.
    private async Task<Guid> SeedReservedVariantAsync(Guid orderId)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create("Kapı", $"kapi-t387-{n}", ProductType.Physical, 20m, now).WithSeededOrigin();
        Variant variant = Variant.Create(product.Id, $"KAPI-T387-{n}", 100m, now, 100);
        variant.StockQuantity = 10;
        await using (AppDbContext db = NewContext())
        {
            db.Products.Add(product);
            db.Variants.Add(variant);
            db.StockReservations.Add(StockReservation.ForVariant(orderId, variant.Id, 1, now));
            await db.SaveChangesAsync(Ct);
        }

        await ExecuteAsync($"UPDATE variants SET reserved_quantity = 1 WHERE id = '{variant.Id}'");
        return variant.Id;
    }

    private Task<string> StockOfAsync(Guid variantId) =>
        ScalarAsync<string>($"SELECT stock_quantity || '/' || reserved_quantity || '/' || (SELECT count(*) FROM stock_reservations WHERE variant_id = '{variantId}') FROM variants WHERE id = '{variantId}'");

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
