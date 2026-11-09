using System.Text;
using System.Text.RegularExpressions;

using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Commands;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.LogRead;
using Shopfolio.UnitTests.Architecture;
using Shopfolio.UnitTests.Fakes;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Commands;

/// <summary>
/// The installer's commands of T387 (05 8.5.5, 8.5.9, 8.5.12, 9.3; 09 13.2 items 3, 4, 5, 7): the <c>--help</c> list and the <c>05 8.5</c> table are equal both ways (KK9); the day and the
/// amount options are read without the culture; an IP or an e-mail never comes as an argument and a redirected input ends with 2 before any line (KK3); the read command's line carries
/// no filter value and waits for the disk (KK5); the CSV form; the panel has no screen of the three sources (KK6) and the Commands folder names no provider or e-mail gateway (KK9).
/// </summary>
public sealed partial class SetupCommandsTests : IDisposable
{
    private const string Email = "ayse.yilmaz@example.com";

    private readonly string _directory = Path.Combine(Path.GetTempPath(), "shopfolio-t387-" + Guid.NewGuid().ToString("N"));

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    // ---- KK9: --help equals the 05 8.5 table, both ways ----

    [Fact]
    public async Task HelpList_And_The05Table_AreEqualBothWays_InTheTablesOrder()
    {
        FakeTerminal terminal = new();

        int exit = await Refusing(terminal).ExecuteAsync(["--help"], Ct);

        Assert.Equal((int)CommandExit.Success, exit);
        List<string> listed = [.. HelpCommandLine().Matches(terminal.Output).Select(m => m.Groups["name"].Value)];

        string root = RepositoryRoot.Find();
        List<(string Name, string Row)> guideRows = [.. GuideRow().Matches(File.ReadAllText(Path.Combine(root, "Docs", "09_CODING_GUIDELINES.md")))
            .Select(m => (m.Groups["name"].Value, m.Groups["row"].Value))];
        string architecture = File.ReadAllText(Path.Combine(root, "Docs", "05_TECHNICAL_ARCHITECTURE.md"));
        int start = architecture.IndexOf("### 8.5 ", StringComparison.Ordinal);
        int end = architecture.IndexOf("### 8.6 ", start, StringComparison.Ordinal);
        List<string> tableRows = [.. ArchitectureRow().Matches(architecture[start..end]).Select(m => m.Groups["row"].Value)];

        Assert.Equal(9, tableRows.Count);
        Assert.Equal(tableRows, guideRows.Select(r => r.Row));
        Assert.Equal(guideRows.Select(r => r.Name), listed);
        Assert.Equal(AdminCommandCatalog.All.Select(c => c.Name), listed);
    }

    // ---- The day and the amount options ----

    [Theory]
    [InlineData("2026-10-29", 2026, 10, 29)]
    [InlineData("2028-02-29", 2028, 2, 29)]
    public void Date_IsAnIsoDayOnly(string text, int year, int month, int day)
    {
        Assert.Equal(new DateOnly(year, month, day), CommandDispatcher.ParseDate(text));
    }

    [Theory]
    [InlineData("2026-10-29T00:00:00Z")]
    [InlineData("29.10.2026")]
    [InlineData("2026-1-5")]
    [InlineData("2026-13-01")]
    [InlineData("2027-02-29")]
    [InlineData("٢٠٢٦-١٠-٢٩")]
    [InlineData(" 2026-10-29")]
    public void Date_InAnyOtherForm_IsRefused(string text)
    {
        Assert.Null(CommandDispatcher.ParseDate(text));
    }

    [Theory]
    [InlineData("1250", 125000)]
    [InlineData("1250.5", 125050)]
    [InlineData("1250.50", 125050)]
    [InlineData("0.01", 1)]
    [InlineData("9999999999.99", 999999999999)]
    public void Amount_IsDottedDecimal_InWholeKurus(string text, long kurus)
    {
        Assert.Equal(kurus, CommandDispatcher.ParseAmountInKurus(text));
    }

    [Theory]
    [InlineData("1250,50")]
    [InlineData("1.250,50")]
    [InlineData("1,250.50")]
    [InlineData("-10")]
    [InlineData("+10")]
    [InlineData("10.123")]
    [InlineData("10.")]
    [InlineData(".5")]
    [InlineData("12345678901")]
    [InlineData("1e3")]
    public void Amount_InAnyOtherForm_IsRefused(string text)
    {
        Assert.Null(CommandDispatcher.ParseAmountInKurus(text));
    }

    [Theory]
    [InlineData("34.56", 3456)]
    [InlineData("0.10", 10)]
    [InlineData("0.01", 1)]
    [InlineData("1234567.89", 123456789)]
    public void ToKurus_TheTableOf09_14_2(string amount, long kurus)
    {
        Assert.Equal(kurus, PaymentAmounts.ToKurus(decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void ToKurus_RefusesAThirdPlace_ANegativeAmount_AndAnOverflow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PaymentAmounts.ToKurus(10.123m));
        Assert.Throws<ArgumentOutOfRangeException>(() => PaymentAmounts.ToKurus(-1m));
        Assert.Throws<OverflowException>(() => PaymentAmounts.ToKurus(decimal.MaxValue / 10));
    }

    [Theory]
    [InlineData("--from", "2026-10-29T00:00:00Z")]
    [InlineData("--to", "29.10.2026")]
    public async Task ReadLogs_DayInAnotherForm_IsAUsageError_AndTheValueIsNotRepeated(string option, string value)
    {
        FakeTerminal terminal = new();
        List<string> args = ["read-logs", "--source", "login", "--from", "2026-10-01", "--to", "2026-10-31"];
        args[args.IndexOf(option) + 1] = value;

        int exit = await Refusing(terminal).ExecuteAsync([.. args], Ct);

        Assert.Equal((int)CommandExit.UsageError, exit);
        Assert.DoesNotContain(value, terminal.AllOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ApplyPaymentResult_AmountWithAComma_IsAUsageError()
    {
        FakeTerminal terminal = new();

        int exit = await Refusing(terminal).ExecuteAsync(["apply-payment-result", "--order-number", "2026-7K4M9P", "--result", "paid", "--amount", "1250,50"], Ct);

        Assert.Equal((int)CommandExit.UsageError, exit);
        Assert.DoesNotContain("1250,50", terminal.AllOutput, StringComparison.Ordinal);
    }

    // ---- KK3: the filter; an IP or an e-mail never comes as an argument ----

    [Theory]
    [InlineData("System", "ip:10.0.0.1")]
    [InlineData("System", "email:ayse.yilmaz@example.com")]
    [InlineData("Login", "ip:2001:db8::1")]
    [InlineData("System", "event:ayse@example.com")]
    [InlineData("System", "request:10.0.0.1")]
    [InlineData("System", "request:3232235777")]
    [InlineData("System", "request:a.b")]
    [InlineData("System", "account:not-a-guid")]
    [InlineData("System", "account:")]
    [InlineData("System", "unknown:x")]
    [InlineData("System", "")]
    [InlineData("Login", "account:0f8fad5b-d9cb-469f-a165-70867728950e")]
    [InlineData("Login", "request:abc")]
    [InlineData("Login", "event:Bogus")]
    [InlineData("Login", "event:0")]
    [InlineData("Login", "event:succeeded")]
    [InlineData("Disposal", "ip")]
    [InlineData("Disposal", "email")]
    [InlineData("Disposal", "request:abc")]
    [InlineData("Disposal", "event:Bogus")]
    public void Filter_ThatCarriesPersonalData_OrDoesNotFitTheSource_IsRefused(string source, string text)
    {
        Assert.Null(ReadLogsCommand.ParseFilter(text, Enum.Parse<LogSource>(source)));
    }

    [Theory]
    [InlineData("Login", null, "None", null)]
    [InlineData("Login", "ip", "Ip", null)]
    [InlineData("Login", "email", "Email", null)]
    [InlineData("Login", "event:Failed", "Event", "Failed")]
    [InlineData("Disposal", "event:Order", "Event", "Order")]
    [InlineData("System", "event:AdminCommand", "Event", "AdminCommand")]
    [InlineData("System", "request:0af7651916cd43dd8448eb211c80319c", "Request", "0af7651916cd43dd8448eb211c80319c")]
    [InlineData("System", "account:0F8FAD5B-D9CB-469F-A165-70867728950E", "Account", "0f8fad5b-d9cb-469f-a165-70867728950e")]
    [InlineData("System", "ip", "Ip", null)]
    [InlineData("System", "email", "Email", null)]
    public void Filter_OfEachSource_IsRead(string source, string? text, string kind, string? value)
    {
        Assert.Equal(new ReadLogsCommand.ReadFilter(Enum.Parse<LogFilterKind>(kind), value), ReadLogsCommand.ParseFilter(text, Enum.Parse<LogSource>(source)));
    }

    [Theory]
    [InlineData("ip:10.0.0.1", "10.0.0.1")]
    [InlineData("email:ayse.yilmaz@example.com", Email)]
    [InlineData("request:10.0.0.1", "10.0.0.1")]
    public async Task ReadLogs_IpOrEmailAsAnArgument_IsAUsageError_WritesNoLine_AndDoesNotRepeatIt(string filter, string personal)
    {
        FakeTerminal terminal = new();
        (CommandInput input, RecordingWriter writer) = ReadInput(terminal, "system", filter);

        CommandExit exit = await new ReadLogsCommand().ExecuteAsync(input, Ct);

        Assert.Equal(CommandExit.UsageError, exit);
        Assert.Empty(writer.Lines);
        Assert.Equal(0, terminal.Reads);
        Assert.DoesNotContain(personal, terminal.AllOutput, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("login", "ip")]
    [InlineData("login", "email")]
    [InlineData("system", "ip")]
    [InlineData("system", "email")]
    public async Task ReadLogs_RedirectedInput_WithAnIpOrEmailFilter_ExitsWithTwo_ReadsNothing_AndWritesNoLine(string source, string filter)
    {
        FakeTerminal terminal = new() { IsInputRedirected = true };
        terminal.Answer(Email);
        (CommandInput input, RecordingWriter writer) = ReadInput(terminal, source, filter);

        // The provider holds nothing but the writer: a command that reached for a module or the database would throw instead of returning 2.
        CommandExit exit = await new ReadLogsCommand().ExecuteAsync(input, Ct);

        Assert.Equal(CommandExit.UsageError, exit);
        Assert.Equal(0, terminal.Reads);
        Assert.Empty(writer.Lines);
    }

    [Fact]
    public async Task ReadLogs_FromAfterTo_IsAUsageError_AndWritesNoLine()
    {
        FakeTerminal terminal = new();
        (CommandInput input, RecordingWriter writer) = ReadInput(terminal, "login", null, new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 1));

        Assert.Equal(CommandExit.UsageError, await new ReadLogsCommand().ExecuteAsync(input, Ct));
        Assert.Empty(writer.Lines);
    }

    [Fact]
    public async Task ReadLogs_UnknownSource_IsAUsageError()
    {
        FakeTerminal terminal = new();
        (CommandInput input, RecordingWriter writer) = ReadInput(terminal, "journal", null);

        Assert.Equal(CommandExit.UsageError, await new ReadLogsCommand().ExecuteAsync(input, Ct));
        Assert.Empty(writer.Lines);
    }

    [Fact]
    public void ReadLogs_IpAndEmailFilters_HaveNoValueInTheOption_AndNoOptionIsNamedForPersonalData()
    {
        IAdminCommand command = AdminCommandCatalog.All.Single(c => c.Name == ReadLogsCommand.CommandName);

        Assert.Equal(["source", "from", "to", "filter"], command.Options.Select(o => o.Name));
        Assert.DoesNotContain(command.Options, o => o.Name is "ip" or "email");
        Assert.Equal(CommandOptionKind.Date, command.Options.Single(o => o.Name == "from").Kind);
        Assert.Equal(CommandOptionKind.Date, command.Options.Single(o => o.Name == "to").Kind);
    }

    // ---- job and apply-payment-result: usage errors before any line ----

    [Theory]
    [InlineData("not-a-guid", "requeue")]
    [InlineData("0f8fad5b-d9cb-469f-a165-70867728950e", "delete")]
    [InlineData("0f8fad5b-d9cb-469f-a165-70867728950e", "Requeue")]
    public async Task Job_BadIdOrAction_IsAUsageError_AndWritesNoLine(string id, string action)
    {
        FakeTerminal terminal = new();
        RecordingWriter writer = new();
        CommandInput input = new(new Dictionary<string, object> { ["id"] = id, ["action"] = action }, terminal, Services(writer));

        Assert.Equal(CommandExit.UsageError, await new JobCommand().ExecuteAsync(input, Ct));
        Assert.Empty(writer.Lines);
    }

    [Theory]
    [InlineData("paid", null)]
    [InlineData("none", 125000L)]
    [InlineData("maybe", null)]
    [InlineData("Paid", 125000L)]
    public async Task ApplyPaymentResult_ResultAndAmountThatDoNotFit_AreAUsageError_AndWriteNoLine(string result, long? amount)
    {
        FakeTerminal terminal = new();
        RecordingWriter writer = new();
        Dictionary<string, object> values = new() { ["order-number"] = "2026-7K4M9P", ["result"] = result };
        if (amount is { } kurus)
        {
            values["amount"] = kurus;
        }

        CommandInput input = new(values, terminal, Services(writer));

        Assert.Equal(CommandExit.UsageError, await new ApplyPaymentResultCommand().ExecuteAsync(input, Ct));
        Assert.Empty(writer.Lines);
    }

    // ---- KK5: the read command's line ----

    [Fact]
    public void ReadLogsLine_CarriesTheKindNotTheValue_AndWaitsForTheDisk()
    {
        Guid account = Guid.NewGuid();
        AuditLine line = AuditLine.ReadLogs(ReadLogsCommand.CommandName, account, LogSource.System, LogFilterKind.Account, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31));

        Assert.True(line.FlushToDisk);
        Assert.False(AuditLine.AdminCommand("job", account).FlushToDisk);
        Assert.Equal(["LogSource", "FilterKind", "FromDay", "ToDay"], line.ClosedValues.Select(v => v.Key));
        Assert.All(LogTemplateSourceScan.Placeholders(line.Template), name =>
        {
            Assert.False(LogTemplateSourceScan.IsPersonalDataName(name), name);
            Assert.False(LogTemplateSourceScan.IsRequestFieldName(name), name);
        });

        AuditLogWriter writer = new(_directory, "command", new ManualTimeProvider(new DateTimeOffset(2026, 11, 9, 9, 0, 0, TimeSpan.Zero)));
        writer.Write(line);
        string written = File.ReadAllText(Directory.GetFiles(_directory).Single());

        Assert.Contains("\"logSource\":\"System\"", written, StringComparison.Ordinal);
        Assert.Contains("\"filterKind\":\"Account\"", written, StringComparison.Ordinal);
        Assert.Contains("\"fromDay\":\"2026-10-01\"", written, StringComparison.Ordinal);
        Assert.Contains("\"toDay\":\"2026-10-31\"", written, StringComparison.Ordinal);
        Assert.Contains(account.ToString(), written, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadLogsLine_HasNoParameterForAFilterValue()
    {
        // The factory takes typed values only: an enum, a day and an id. A string parameter other than the command name would let a value in.
        System.Reflection.ParameterInfo[] parameters = typeof(AuditLine).GetMethod(nameof(AuditLine.ReadLogs))!.GetParameters();

        Assert.Single(parameters, p => p.ParameterType == typeof(string));
        Assert.Equal("command", parameters.Single(p => p.ParameterType == typeof(string)).Name);
    }

    // ---- The CSV form and the system log line ----

    [Fact]
    public void Csv_HasSemicolons_CrLf_Quotes_AndNeutralizesFormulas()
    {
        using StringWriter output = new();

        LogCsv.WriteRow(output, "a;b", "say \"x\"", "=1+1", "-2", "@x", "plain");

        Assert.Equal("\"a;b\";\"say \"\"x\"\"\";'=1+1;'-2;'@x;plain\r\n", output.ToString());
        Assert.Equal([0xEF, 0xBB, 0xBF], LogCsv.Encoding.GetPreamble());
    }

    [Fact]
    public void SystemLine_ReadsTheApplicationsTime_AndTheProxysSeconds_AndMatchesTheFilter()
    {
        string account = Guid.NewGuid().ToString("D");
        LogReader.SystemLine app = LogReader.SystemLine.TryParse($$"""{"time":"2026-10-05T10:00:00.000Z","event":"AdminCommand","accountId":"{{account}}","requestId":"r-1"}""")!;
        LogReader.SystemLine proxy = LogReader.SystemLine.TryParse("""{"ts":1759658400.5,"remote_ip":"10.0.0.1","request_id":"r-2"}""")!;

        Assert.Equal(new DateTimeOffset(2026, 10, 5, 10, 0, 0, TimeSpan.Zero), app.Time);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(1759658400500), proxy.Time);
        Assert.True(app.Matches(Request(LogFilterKind.Event, "AdminCommand")));
        Assert.False(app.Matches(Request(LogFilterKind.Event, "JobStarted")));
        Assert.True(app.Matches(Request(LogFilterKind.Account, account)));
        Assert.True(app.Matches(Request(LogFilterKind.Request, "r-1")));
        Assert.True(proxy.Matches(Request(LogFilterKind.Ip, "10.0.0.1")));
        Assert.False(proxy.Matches(Request(LogFilterKind.Request, "r-1")));
        Assert.Null(LogReader.SystemLine.TryParse("not json"));
        Assert.Null(LogReader.SystemLine.TryParse("""{"message":"no time"}"""));
        Assert.Null(LogReader.SystemLine.TryParse("[1,2]"));
    }

    // ---- KK2: the server's text logs (05 9.1.4, 9.3.1) ----

    [Fact]
    public void ServerTextLine_ReadsTheDatabasesPgBackRestsAndTheBackupToolsTimes_AndCarriesTheTimeToTheLinesAfter()
    {
        DateTimeOffset database = ServerTextLine.TimeOf("2026-10-05 10:00:00.123 UTC [57] LOG:  checkpoint starting: time", null)!.Value;
        DateTimeOffset offsetZone = ServerTextLine.TimeOf("2026-10-05 13:00:00.123 +03 [57] LOG:  database system is ready", null)!.Value;
        DateTimeOffset pgBackRest = ServerTextLine.TimeOf("2026-10-05 01:30:00.000 P00   INFO: backup command begin 2.54.0", null)!.Value;
        DateTimeOffset round = ServerTextLine.TimeOf("file-backup: round starts at 2026-10-05T01:30:00Z", null)!.Value;

        Assert.Equal(new DateTimeOffset(2026, 10, 5, 10, 0, 0, 123, TimeSpan.Zero), database);
        Assert.Equal(new DateTimeOffset(2026, 10, 5, 10, 0, 0, 123, TimeSpan.Zero), offsetZone);
        Assert.Equal(new DateTimeOffset(2026, 10, 5, 1, 30, 0, TimeSpan.Zero), pgBackRest);
        Assert.Equal(new DateTimeOffset(2026, 10, 5, 1, 30, 0, TimeSpan.Zero), round);
        Assert.Equal(round, ServerTextLine.TimeOf("file-backup: step backup starts", round));
        Assert.Null(ServerTextLine.TimeOf("file-backup: step backup starts", null));
        Assert.Null(ServerTextLine.TimeOf("2026-13-45 99:00:00 broken", null));
    }

    [Theory]
    [InlineData("connection received: host=10.0.0.1 port=50122", "10.0.0.1", true)]
    [InlineData("Accepted publickey for root from 10.0.0.1 port 22.", "10.0.0.1", true)]
    [InlineData("failed from 10.0.0.1.", "10.0.0.1", true)]
    [InlineData("connection received: host=10.0.0.15 port=50122", "10.0.0.1", false)]
    [InlineData("connection received: host=110.0.0.1 port=50122", "10.0.0.1", false)]
    [InlineData("connection received: host=5.10.0.0.1", "10.0.0.1", false)]
    [InlineData("request 7f3c2a10-aaaa-bbbb-cccc-000000000001 done", "7f3c2a10-aaaa-bbbb-cccc-000000000001", true)]
    [InlineData("request 7f3c2a10-aaaa-bbbb-cccc-0000000000012 done", "7f3c2a10-aaaa-bbbb-cccc-000000000001", false)]
    public void ServerTextLine_MatchesTheFilterValueAsAWholeToken(string line, string value, bool expected)
    {
        Assert.Equal(expected, ServerTextLine.ContainsToken(line, value));
    }

    // ---- KK8: the payment result command's line (05 8.5.12) ----

    [Fact]
    public void PaymentResultLine_CarriesTheOrdersIdAndTheResultAsClosedValues_AndWaitsForTheDisk()
    {
        Guid order = Guid.NewGuid();
        AuditLine paid = AuditLine.PaymentResult(ApplyPaymentResultCommand.CommandName, order, InstallerPaymentResult.Paid, 12_050);
        AuditLine none = AuditLine.PaymentResult(ApplyPaymentResultCommand.CommandName, order, InstallerPaymentResult.None, null);

        Assert.True(paid.FlushToDisk);
        Assert.Equal("OrderId", paid.IdKey);
        Assert.Equal(["PaymentResult", "AmountInKurus"], paid.ClosedValues.Select(v => v.Key));
        Assert.Equal(["Paid", "12050"], paid.ClosedValues.Select(v => v.Value));
        Assert.Equal(["None", string.Empty], none.ClosedValues.Select(v => v.Value));
        Assert.All(LogTemplateSourceScan.Placeholders(paid.Template), name =>
        {
            Assert.False(LogTemplateSourceScan.IsPersonalDataName(name), name);
            Assert.False(LogTemplateSourceScan.IsRequestFieldName(name), name);
        });
        System.Reflection.ParameterInfo[] parameters = typeof(AuditLine).GetMethod(nameof(AuditLine.PaymentResult))!.GetParameters();
        Assert.Equal("command", Assert.Single(parameters, p => p.ParameterType == typeof(string)).Name);

        AuditLogWriter writer = new(_directory, "command", new ManualTimeProvider(new DateTimeOffset(2026, 11, 9, 9, 0, 0, TimeSpan.Zero)));
        writer.Write(paid);
        string written = File.ReadAllText(Directory.GetFiles(_directory).Single());

        Assert.Contains(order.ToString(), written, StringComparison.Ordinal);
        Assert.Contains("\"paymentResult\":\"Paid\"", written, StringComparison.Ordinal);
        Assert.Contains("\"amountInKurus\":\"12050\"", written, StringComparison.Ordinal);
        Assert.Contains("\"command\":\"apply-payment-result\"", written, StringComparison.Ordinal);
    }

    // ---- KK6, KK9: source scans ----

    [Fact]
    public void Panel_HasNoScreenOfTheThreeSources()
    {
        string root = RepositoryRoot.Find();
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src", "Shopfolio.Web", "Pages"), "*.*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".cs", StringComparison.Ordinal) || f.EndsWith(".cshtml", StringComparison.Ordinal)))
        {
            string text = StripComments(File.ReadAllText(file));
            if (ThreeSources().IsMatch(text))
            {
                findings.Add(Path.GetRelativePath(root, file));
            }
        }

        Assert.Empty(findings);
    }

    [Fact]
    public void CommandsFolder_NamesNoProviderGateway_NoEmailGateway_AndNoJobRunner()
    {
        string root = RepositoryRoot.Find();
        string[] files = Directory.GetFiles(Path.Combine(root, "src", "Shopfolio.Web", "Commands"), "*.cs", SearchOption.AllDirectories);
        List<string> findings = [.. files.Where(f => OutsideCalls().IsMatch(StripComments(File.ReadAllText(f)))).Select(f => Path.GetRelativePath(root, f))];

        Assert.Contains(files, f => f.EndsWith("ApplyPaymentResultCommand.cs", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void Scans_OnViolationSamples_ReportEachForm()
    {
        Assert.Matches(ThreeSources(), "_db.LoginRecords.ToList();");
        Assert.Matches(ThreeSources(), "@inject Shopfolio.Infrastructure.LogRead.LogReader reader");
        Assert.Matches(ThreeSources(), "db.Set<DisposalRecord>()");
        Assert.DoesNotMatch(ThreeSources(), StripComments("// LoginRecords in a comment"));
        Assert.Matches(OutsideCalls(), "IPaymentGateway gateway");
        Assert.Matches(OutsideCalls(), "services.GetRequiredService<IEmailGateway>()");
        Assert.Matches(OutsideCalls(), "JobRunnerService runner");
        Assert.Matches(OutsideCalls(), "IGoogleIdentityGateway g");
    }

    // ---- helpers ----

    private static LogReadRequest Request(LogFilterKind kind, string value) => new(LogSource.System, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31), kind, value);

    private static CommandDispatcher Refusing(FakeTerminal terminal) =>
        new(AdminCommandCatalog.All, terminal, () => throw new InvalidOperationException("The services must not be made for a command line that is refused."));

    private static ServiceProvider Services(RecordingWriter writer)
    {
        ServiceCollection services = new();
        services.AddSingleton<IAuditLineWriter>(writer);
        return services.BuildServiceProvider();
    }

    private static (CommandInput Input, RecordingWriter Writer) ReadInput(FakeTerminal terminal, string source, string? filter, DateOnly? from = null, DateOnly? to = null)
    {
        RecordingWriter writer = new();
        Dictionary<string, object> values = new()
        {
            ["source"] = source,
            ["from"] = from ?? new DateOnly(2026, 10, 1),
            ["to"] = to ?? new DateOnly(2026, 10, 31),
        };
        if (filter is not null)
        {
            values["filter"] = filter;
        }

        return (new CommandInput(values, terminal, Services(writer)), writer);
    }

    private static string StripComments(string text) => LineComment().Replace(text, string.Empty);

    [GeneratedRegex(@"^  (?<name>[a-z]+(?:-[a-z]+)*)  ", RegexOptions.Multiline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex HelpCommandLine();

    [GeneratedRegex(@"^\s*\| `(?<name>[a-z]+(?:-[a-z]+)*)` \| (?<row>8\.5\.\d+) \|", RegexOptions.Multiline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex GuideRow();

    [GeneratedRegex(@"^\| (?<row>8\.5\.\d+) \| [^|]+ \|", RegexOptions.Multiline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ArchitectureRow();

    [GeneratedRegex(@"\b(?:LoginRecords?|DisposalRecords?|LogReader|LogRead|LogReadRequest)\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ThreeSources();

    [GeneratedRegex(@"\b(?:IPaymentGateway|IEmailGateway|IGoogleIdentityGateway|JobRunner\w*|IJobHandler\w*)\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex OutsideCalls();

    [GeneratedRegex(@"(?m)//.*$|/\*[\s\S]*?\*/|@\*[\s\S]*?\*@", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LineComment();

    private sealed class RecordingWriter : IAuditLineWriter
    {
        public List<AuditLine> Lines { get; } = [];

        public void Write(AuditLine line) => Lines.Add(line);
    }
}
