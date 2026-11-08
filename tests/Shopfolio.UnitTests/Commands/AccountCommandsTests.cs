using System.Text.RegularExpressions;

using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Commands;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.UnitTests.Fakes;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Commands;

/// <summary>
/// The questions of <c>end-sessions</c>, <c>invalidate-password</c> and <c>status</c> that need no database (T389; 05 8.5.3, 8.5.4, 8.5.6; 09 13.2 items 4, 5; KK4): a redirected input
/// ends with 2 before anything is read, written or opened; the commands take no option, so no personal value comes from the command line; the "no target" line holds the empty id
/// and nothing else; and the second provider number is read only by the status command (KK7: it is in no counter of the panel). The accounts themselves are the integration tests'.
/// </summary>
public sealed partial class AccountCommandsTests
{
    private const string Email = "ayse.yilmaz@example.com";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public static TheoryData<string> AccountCommands => [EndSessionsCommand.CommandName, InvalidatePasswordCommand.CommandName];

    private static IAdminCommand Command(string name) => AdminCommandCatalog.All.Single(c => c.Name == name);

    private static (CommandInput Input, RecordingWriter Writer) Input(FakeTerminal terminal)
    {
        RecordingWriter writer = new();
        ServiceCollection services = new();
        services.AddSingleton<IAuditLineWriter>(writer);
        ServiceProvider provider = services.BuildServiceProvider();
        return (new CommandInput(new Dictionary<string, object>(), terminal, provider), writer);
    }

    // ---- KK4: a redirected input ----

    [Theory]
    [MemberData(nameof(AccountCommands))]
    public async Task RedirectedInput_ExitsWithTwo_ReadsNothing_WritesNoLine_AndOpensNoDatabase(string name)
    {
        FakeTerminal terminal = new() { IsInputRedirected = true };
        terminal.Answer(Email);
        (CommandInput input, RecordingWriter writer) = Input(terminal);

        // The provider holds nothing but the writer: a command that reached for a module or the runner would throw instead of returning 2.
        CommandExit exit = await Command(name).ExecuteAsync(input, Ct);

        Assert.Equal(CommandExit.UsageError, exit);
        Assert.Equal(0, terminal.Reads);
        Assert.Empty(writer.Lines);
        Assert.DoesNotContain(Email, terminal.AllOutput, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(AccountCommands))]
    public void TheAccountCommands_TakeNoOption_SoNoPersonalValueCanComeFromTheCommandLine(string name)
    {
        Assert.Empty(Command(name).Options);
    }

    [Fact]
    public void TheCatalog_HoldsTheFourCommandsOfThisTask_UnderTheirTableNames()
    {
        string[] names = AdminCommandCatalog.All.Select(c => c.Name).ToArray();

        Assert.Contains(EndSessionsCommand.CommandName, names);
        Assert.Contains(InvalidatePasswordCommand.CommandName, names);
        Assert.Contains(StatusCommand.CommandName, names);
        Assert.Equal("end-sessions", EndSessionsCommand.CommandName);
        Assert.Equal("invalidate-password", InvalidatePasswordCommand.CommandName);
        Assert.Equal("status", StatusCommand.CommandName);
    }

    // ---- KK3: the "no target" line ----

    [Fact]
    public void TheNoTargetLine_HoldsTheCommandAndTheEmptyId_InTheShapeOfEveryCommandLine()
    {
        AuditLine none = AuditLine.AdminCommandWithoutTarget("end-sessions");
        AuditLine target = AuditLine.AdminCommand("end-sessions", Guid.NewGuid());

        Assert.Equal(Guid.Empty, none.Id);
        Assert.Equal(target.Template, none.Template);
        Assert.Equal(target.EventName, none.EventName);
        Assert.Equal("end-sessions", none.Name);
    }

    // ---- KK7: the second number is the installer's alone ----

    [Fact]
    public void RefundExposureCount_IsReadOnlyByTheStatusCommand_NotByAnyCounterOfThePanel()
    {
        string root = RepositoryRoot.Find();
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (relative.Contains("/obj/", StringComparison.Ordinal) || relative.Contains("/bin/", StringComparison.Ordinal))
            {
                continue;
            }

            findings.AddRange(ExposureCountUses(relative, File.ReadAllText(file)));
        }

        Assert.Empty(findings);
    }

    [Fact]
    public void RefundExposureScan_OnSamples_ReportsEveryFormOutsideTheAllowedFiles()
    {
        const string panel = "src/Shopfolio.Web/Pages/Admin/Index.cshtml.cs";

        Assert.Single(ExposureCountUses(panel, "int n = await afterSales.CountCardOrdersWithRefundExposureAsync(ct);"));
        Assert.Single(ExposureCountUses(panel, "Func<CancellationToken, Task<int>> f = afterSales.CountCardOrdersWithRefundExposureAsync;"));
        Assert.Single(ExposureCountUses(panel, "string s = nameof(IAfterSalesModule.CountCardOrdersWithRefundExposureAsync);"));
        Assert.Single(ExposureCountUses(panel, "await afterSales\n    .CountCardOrdersWithRefundExposureAsync(ct);"));
        Assert.Empty(ExposureCountUses("src/Shopfolio.Web/Commands/StatusCommand.cs", "await m.CountCardOrdersWithRefundExposureAsync(ct);"));
        Assert.Empty(ExposureCountUses("src/Shopfolio.Web/Modules/AfterSales/AfterSalesModule.cs", "public async Task<int> CountCardOrdersWithRefundExposureAsync(CancellationToken ct)"));
        Assert.Empty(ExposureCountUses(panel, "int n = 1; // CountCardOrdersWithRefundExposureAsync is the installer's alone"));
    }

    private static readonly string[] _exposureAllowed =
    [
        "src/Shopfolio.Web/Commands/StatusCommand.cs",
        "src/Shopfolio.Web/Modules/AfterSales/AfterSalesModule.cs",
        "src/Shopfolio.Web/Modules/AfterSales/Contracts/IAfterSalesModule.cs",
    ];

    private static List<string> ExposureCountUses(string relativePath, string source)
    {
        if (_exposureAllowed.Contains(relativePath, StringComparer.Ordinal))
        {
            return [];
        }

        // A line comment is not a use; the name anywhere else - a call, a method group, a nameof, a call split over lines - is.
        string withoutComments = LineComment().Replace(source, string.Empty);
        return ExposureName().Matches(withoutComments).Select(m => $"{relativePath}: {m.Value}").ToList();
    }

    [GeneratedRegex(@"//[^\n]*", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LineComment();

    [GeneratedRegex(@"\bCountCardOrdersWithRefundExposureAsync\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ExposureName();

    // ---- KK6: the conditional lines (05 8.4.6, 8.5.6) ----

    [Fact]
    public void DescribeLine_AnOpenLine_IsOpen()
    {
        Assert.Equal("AÇIK", StatusCommand.DescribeLine(new ConditionalLine(ConditionalLine.CardPayment, [])));
    }

    [Fact]
    public void DescribeLine_AMissingKeyOfTheSettingsFile_ClosesTheLine_AndNamesTheKeysOnly()
    {
        string text = StatusCommand.DescribeLine(new ConditionalLine(ConditionalLine.GoogleSignIn, [GoogleOptions.ClientIdKey, GoogleOptions.ClientSecretKey]));

        Assert.Equal($"KAPALI (eksik: {GoogleOptions.ClientIdKey}, {GoogleOptions.ClientSecretKey})", text);
    }

    [Fact]
    public void DescribeLine_OnlyASecretFileKeyMissing_IsNotMeasurable_BecauseTheCommandProcessIsNotGivenTheSecret()
    {
        string card = StatusCommand.DescribeLine(new ConditionalLine(ConditionalLine.CardPayment, [PaymentOptions.ProviderKeysKey]));
        string google = StatusCommand.DescribeLine(new ConditionalLine(ConditionalLine.GoogleSignIn, [GoogleOptions.ClientSecretKey]));

        Assert.StartsWith("ölçülemez (eksik: " + PaymentOptions.ProviderKeysKey, card, StringComparison.Ordinal);
        Assert.StartsWith("ölçülemez (eksik: " + GoogleOptions.ClientSecretKey, google, StringComparison.Ordinal);
    }

    private sealed class RecordingWriter : IAuditLineWriter
    {
        public List<AuditLine> Lines { get; } = [];

        public void Write(AuditLine line) => Lines.Add(line);
    }
}
