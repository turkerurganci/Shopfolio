using System.Text.RegularExpressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Commands;
using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Backup;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.UnitTests.Fakes;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Commands;

/// <summary>
/// The two commands of a restore from backup (T388; 05 8.5.10, 8.5.11; 09 13.2 items 3 and 7): the restore point is an instant with its offset and anything else is a usage error (KK2);
/// the order side's table list is the model's list less the tables that are not the order side, so a new table must be classified (KK3); the check only reads (KK1); the log reader keeps the
/// deletion lines from the restore moment and counts what it cannot read (KK6).
/// </summary>
public sealed partial class RestoreCommandsTests : IDisposable
{
    private static readonly DateTimeOffset RestorePoint = new(2026, 11, 5, 11, 30, 0, TimeSpan.Zero);

    // The tables that are not the order side (05 8.5.11): the catalog, the content, the settings, the accounts and the working records. The check never counts them: their writes go back with the restore.
    private static readonly string[] _notOrderSide =
    [
        "about_page_images", "about_pages", "address_books", "admin_invitations", "admins", "announcements", "attempt_records", "backup_status_records", "branches", "brandings",
        "cart_items", "carts", "categories", "channel_state_records", "companies", "company_delivery_provinces", "company_settings", "countries", "coupons", "custom_page_images",
        "custom_pages", "customers", "data_protection_keys", "digital_files", "discounts", "disposal_records", "faq_item_images", "faq_items", "heartbeat_records", "images", "jobs",
        "link_records", "login_records", "offering_images", "offering_products", "offerings", "option_values", "options", "outage_records", "pending_email_changes",
        "portfolio_item_images", "portfolio_item_products", "portfolio_items", "price_histories", "product_categories", "product_images", "products", "provinces",
        "recognized_browser_marks", "sessions", "social_links", "stock_reservations", "variants",
    ];

    private readonly string _directory = Path.Combine(Path.GetTempPath(), "shopfolio-t388-" + Guid.NewGuid().ToString("N"));

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    // --- KK2: the restore point is an instant with an offset ---

    [Theory]
    [InlineData("rollback-check", "2026-11-05")]
    [InlineData("rollback-check", "2026-11-05T14:30:00")]
    [InlineData("rollback-check", "yesterday")]
    [InlineData("reapply-deletions", "2026-11-05")]
    [InlineData("reapply-deletions", "2026-11-05T14:30:00")]
    [InlineData("reapply-deletions", "05.11.2026 14:30")]
    public async Task RestorePoint_WithoutAnOffset_OrADateAlone_IsAUsageError_AndNothingIsOpened(string command, string value)
    {
        FakeTerminal terminal = new();

        int exit = await Real(terminal).ExecuteAsync([command, "--restore-point", value], Ct);

        Assert.Equal((int)CommandExit.UsageError, exit);
        Assert.DoesNotContain(value, terminal.AllOutput, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("rollback-check")]
    [InlineData("reapply-deletions")]
    public async Task RestorePoint_IsRequired(string command)
    {
        FakeTerminal terminal = new();

        int exit = await Real(terminal).ExecuteAsync([command], Ct);

        Assert.Equal((int)CommandExit.UsageError, exit);
        Assert.Contains("--restore-point", terminal.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Help_ListsBothCommandsWithTheRestorePointOption()
    {
        FakeTerminal terminal = new();

        int exit = await Real(terminal).ExecuteAsync(["--help"], Ct);

        Assert.Equal((int)CommandExit.Success, exit);
        Assert.Contains("rollback-check", terminal.Output, StringComparison.Ordinal);
        Assert.Contains("reapply-deletions", terminal.Output, StringComparison.Ordinal);
        Assert.Equal(2, Regex.Count(terminal.Output, "--restore-point", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));
    }

    // --- KK3: every table is classified ---

    [Fact]
    public void EveryTableOfTheModel_IsEitherCountedByTheCheck_OrListedAsNotTheOrderSide()
    {
        IServiceCollection services = CommandHost.CreateServices(
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Database:ConnectionString"] = "Host=127.0.0.1;Database=unused;Username=unused" }).Build(),
            new FakeTerminal());
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        string[] model = [.. scope.ServiceProvider.GetRequiredService<AppDbContext>().Model.GetEntityTypes().Select(t => t.GetTableName()).OfType<string>().Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];

        string[] classified = [.. RollbackAudit.Tables.Concat(_notOrderSide).Order(StringComparer.Ordinal)];

        Assert.Equal(model, classified);
        Assert.Empty(RollbackAudit.Tables.Intersect(_notOrderSide, StringComparer.Ordinal));
    }

    [Fact]
    public void TheCheck_CountsTheTablesOf02_3_34_8_AndTheInboundNotificationRecord()
    {
        // 02 3.34.8 and 05 8.5.11: orders, payments and transition record, item-level records, contact requests, frozen legal texts, checkbox acceptances, the sent record, the trail and the inbound record.
        string[] required =
        [
            "orders", "order_items", "payment_records", "transition_records", "cancellations", "item_removals", "late_delivery_terminations", "withdrawals", "returns", "return_rejections",
            "not_returned_closures", "defect_claims", "contact_requests", "legal_text_versions", "checkbox_acceptances", "dispatch_records", "audit_logs", "inbound_notification_records",
        ];

        Assert.All(required, table => Assert.Contains(table, RollbackAudit.Tables));
    }

    [Theory]
    [InlineData(true, 0, true)]
    [InlineData(true, 3, false)]
    [InlineData(false, 0, false)]
    public void ARollbackIsAllowedOnlyWhenCommitMomentsAreKeptAndTheCountIsZero(bool recorded, long rows, bool allowed)
    {
        RollbackCount count = new(recorded, recorded ? [new RollbackTableCount("orders", rows)] : []);

        Assert.Equal(allowed, count.AllowsRollback);
    }

    // --- KK1: the check only reads ---

    [Fact]
    public void TheCheck_HasNoWritingCall_AndTheScanSeesOneInASample()
    {
        string root = RepositoryRoot.Find();
        string[] files =
        [
            Path.Combine(root, "src", "Shopfolio.Web", "Infrastructure", "Backup", "RollbackAudit.cs"),
            Path.Combine(root, "src", "Shopfolio.Web", "Commands", "RollbackCheckCommand.cs"),
        ];

        foreach (string file in files)
        {
            Assert.Empty(WritingCalls().Matches(StripComments(File.ReadAllText(file))));
        }

        Assert.NotEmpty(WritingCalls().Matches("await db.SaveChangesAsync(ct);"));
        Assert.NotEmpty(WritingCalls().Matches("await db.Orders.ExecuteDeleteAsync(ct);"));
        Assert.NotEmpty(WritingCalls().Matches("db.Database.ExecuteSqlRawAsync(\"UPDATE orders SET version = 1\");"));
        Assert.NotEmpty(WritingCalls().Matches("var s = \"delete from orders\";"));
    }

    // --- KK6: the log reader ---

    [Fact]
    public void LogReader_KeepsTheDeletionLinesFromTheRestoreMomentOn_InTheOrderTheyWereWritten()
    {
        Guid before = Guid.NewGuid();
        Guid first = Guid.NewGuid();
        Guid second = Guid.NewGuid();
        Guid nextDay = Guid.NewGuid();
        Write(RestorePoint.AddMinutes(-1), AuditLine.InstantDeletion(InstantDeletionKinds.CustomerAccount, before));
        Write(RestorePoint, AuditLine.InstantDeletion(InstantDeletionKinds.CustomerAccount, first));
        Write(RestorePoint.AddHours(2), AuditLine.InstantDeletion(InstantDeletionKinds.AdminAccount, second));
        Write(RestorePoint.AddDays(1), AuditLine.InstantDeletion(InstantDeletionKinds.CustomerAccount, nextDay));

        InstantDeletionReading reading = InstantDeletionLog.Read(_directory, RestorePoint, RestorePoint.AddDays(1).AddHours(1));

        Assert.Equal(0, reading.UnreadableLines);
        Assert.Equal(
            [(InstantDeletionKinds.CustomerAccount, first), (InstantDeletionKinds.AdminAccount, second), (InstantDeletionKinds.CustomerAccount, nextDay)],
            reading.Entries.Select(e => (e.Kind, e.RecordId)));
        Assert.Equal(RestorePoint, reading.Entries[0].At);
    }

    [Fact]
    public void LogReader_ReadsTheTurkeyDayOfTheLine_NotTheUtcDay()
    {
        // 21:30 UTC on the 5th is 00:30 on the 6th in Turkey: the line is in the file of the 6th and the read must reach it.
        Guid late = Guid.NewGuid();
        DateTimeOffset moment = new(2026, 11, 5, 21, 30, 0, TimeSpan.Zero);
        Write(moment, AuditLine.InstantDeletion(InstantDeletionKinds.CustomerAccount, late));

        InstantDeletionReading reading = InstantDeletionLog.Read(_directory, moment.AddHours(-1), moment.AddHours(1));

        Assert.Equal(late, Assert.Single(reading.Entries).RecordId);
    }

    [Fact]
    public void LogReader_IgnoresOtherEvents_AndCountsLinesItCannotRead_WithoutKeepingTheirText()
    {
        Guid kept = Guid.NewGuid();
        Write(RestorePoint.AddMinutes(1), AuditLine.AdminCommand("create-admin", Guid.NewGuid()));
        Write(RestorePoint.AddMinutes(2), AuditLine.InstantDeletion(InstantDeletionKinds.CustomerAccount, kept));
        string path = Directory.GetFiles(_directory, "audit-*.jsonl").Single();
        File.AppendAllText(path, "not json at all\n");
        File.AppendAllText(path, "{\"event\":\"InstantDeletion\",\"time\":\"2026-11-05T12:00:00.000Z\",\"kind\":\"CustomerAccount\"}\n");
        File.AppendAllText(path, "[1,2]\n");
        File.AppendAllText(path, "{\"event\":\"InstantDeletion\",\"time\":123,\"kind\":\"CustomerAccount\",\"recordId\":\"" + Guid.NewGuid() + "\"}\n");
        File.AppendAllText(path, "{\"event\":\"InstantDeletion\",\"time\":\"2026-11-05T12:00:00.000Z\",\"kind\":\"CustomerAccount\",\"recordId\":5}\n");

        InstantDeletionReading reading = InstantDeletionLog.Read(_directory, RestorePoint, RestorePoint.AddHours(3));

        Assert.Equal(kept, Assert.Single(reading.Entries).RecordId);
        Assert.Equal(5, reading.UnreadableLines);
    }

    [Fact]
    public void LogReader_WithNoFile_FindsNothing()
    {
        InstantDeletionReading reading = InstantDeletionLog.Read(_directory, RestorePoint, RestorePoint.AddDays(2));

        Assert.Empty(reading.Entries);
        Assert.Equal(0, reading.UnreadableLines);
    }

    [Fact]
    public void LogReader_DoesNotReadTheCommandFile()
    {
        LoggingSetup.CreateAuditLineWriter(new LoggingOptions { Directory = _directory }, AuditFile.Command, new ManualTimeProvider(RestorePoint.AddMinutes(1)))
            .Write(AuditLine.InstantDeletion(InstantDeletionKinds.CustomerAccount, Guid.NewGuid()));

        Assert.Empty(InstantDeletionLog.Read(_directory, RestorePoint, RestorePoint.AddHours(1)).Entries);
    }

    [Fact]
    public void TheKindList_IsClosed_AndHasTheTwoInstantDeletions()
    {
        Assert.Equal(["CustomerAccount", "AdminAccount"], InstantDeletionKinds.All);
    }

    // ---- helpers ----

    private static CommandDispatcher Real(FakeTerminal terminal) =>
        new(AdminCommandCatalog.All, terminal, () => throw new InvalidOperationException("The services must not be made for a command line that is refused."));

    private static string StripComments(string text) => LineComment().Replace(text, string.Empty);

    private void Write(DateTimeOffset at, AuditLine line)
    {
        LoggingSetup.CreateAuditLineWriter(new LoggingOptions { Directory = _directory }, AuditFile.Audit, new ManualTimeProvider(at)).Write(line);
    }

    [GeneratedRegex(@"\bSaveChanges(?:Async)?\b|\bExecute(?:Update|Delete)(?:Async)?\b|\bExecuteSql\w*\b|\b(?:insert\s+into|update\s+\S+\s+set|delete\s+from)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex WritingCalls();

    [GeneratedRegex(@"^\s*//.*$", RegexOptions.Multiline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LineComment();
}
