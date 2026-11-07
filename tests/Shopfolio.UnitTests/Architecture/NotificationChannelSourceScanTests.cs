using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the notification channel scan over the application source and shows on a deliberate violation sample that each rule goes red in every shape (T328 KK2, KK4, KK5;
/// 03 7; 02 9.1, 9.5; 09 19.2 item 3). The scan reads files and never writes one.
/// </summary>
public sealed class NotificationChannelSourceScanTests
{
    private const string Sample = "Architecture/ViolationSamples/NotificationChannel/Misuse.cs";
    private const string AnyModuleFile = "src/Shopfolio.Web/Modules/Ordering/Misuse.cs";

    private static string SampleText() => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", Sample));

    [Fact]
    public void ApplicationSource_HasNoOtherChannel_NoMarketing_NoSwitch_AndNoEmailOutsideTheDispatchRows()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (!NotificationChannelSourceScan.InScope(relative))
            {
                continue;
            }

            scanned.Add(relative);
            foreach ((string rule, int line) in NotificationChannelSourceScan.Scan(relative, File.ReadAllText(file)))
            {
                findings.Add($"{relative}:{line} {rule}");
            }
        }

        // The scan really reached the files whose names the two exact lists carry and the module's queues - a folder that is not read would pass silently.
        Assert.All(NotificationChannelSourceScan.GatewayFiles, f => Assert.Contains(f, scanned));
        Assert.All(NotificationChannelSourceScan.QueueFiles, f => Assert.Contains(f, scanned));
        Assert.Empty(findings);
    }

    [Fact]
    public void TheTwoExactLists_NameFilesThatReallyUseWhatTheyAreAllowed()
    {
        string root = RepositoryRoot.Find();

        // An allowed file that no longer names what it is allowed to name is a stale entry: the list stays exact.
        Assert.All(NotificationChannelSourceScan.GatewayFiles, f => Assert.Contains("IEmailGateway", File.ReadAllText(Path.Combine(root, f)), StringComparison.Ordinal));
        Assert.All(
            NotificationChannelSourceScan.QueueFiles,
            f => Assert.Matches(@"DispatchRecord\s*\.\s*Queue|new\s+DispatchRecord", File.ReadAllText(Path.Combine(root, f))));
    }

    [Fact]
    public void Scan_OnTheViolationSample_ReportsEveryShapeOfEveryRule()
    {
        IReadOnlyList<(string Rule, int Line)> findings = NotificationChannelSourceScan.Scan(AnyModuleFile, SampleText());

        Assert.Equal(
            [
                (NotificationChannelSourceScan.UnknownChannel, 6),
                (NotificationChannelSourceScan.UnknownChannel, 7),
                (NotificationChannelSourceScan.UnknownChannel, 8),
                (NotificationChannelSourceScan.MarketingMessage, 9),
                (NotificationChannelSourceScan.MarketingMessage, 10),
                (NotificationChannelSourceScan.MarketingMessage, 11),
                (NotificationChannelSourceScan.MarketingMessage, 12),
                (NotificationChannelSourceScan.TransactionSwitch, 13),
                (NotificationChannelSourceScan.TransactionSwitch, 14),
                (NotificationChannelSourceScan.TransactionSwitch, 15),
                (NotificationChannelSourceScan.MarketingMessage, 16),
                (NotificationChannelSourceScan.TransactionSwitch, 17),
                (NotificationChannelSourceScan.UnknownChannel, 18),
                (NotificationChannelSourceScan.MarketingMessage, 19),
                (NotificationChannelSourceScan.GatewayOutsideTheSender, 20),
                (NotificationChannelSourceScan.DispatchRowOutsideTheQueues, 21),
                (NotificationChannelSourceScan.DispatchRowOutsideTheQueues, 22),
                (NotificationChannelSourceScan.DispatchRowOutsideTheQueues, 23),
            ],
            findings);
    }

    [Fact]
    public void Scan_TheWordsOfAFilesPath_AreScannedToo()
    {
        Assert.Contains(
            (NotificationChannelSourceScan.UnknownChannel, 0),
            NotificationChannelSourceScan.Scan("src/Shopfolio.Web/Modules/Notifications/SmsSender.cs", "namespace A;"));
        Assert.Contains(
            (NotificationChannelSourceScan.TransactionSwitch, 0),
            NotificationChannelSourceScan.Scan("src/Shopfolio.Web/Pages/bildirim-tercihleri.razor", "<p></p>"));
    }

    [Fact]
    public void Scan_AllowedFiles_MayNameTheGatewayAndMakeTheRows_ButNoOtherFileMay()
    {
        const string text = "IEmailGateway g; var r = DispatchRecord.Queue(a);";

        Assert.Empty(NotificationChannelSourceScan.Scan(NotificationChannelSourceScan.GatewayFiles[3], "IEmailGateway g;"));
        Assert.Empty(NotificationChannelSourceScan.Scan(NotificationChannelSourceScan.QueueFiles[0], "var r = DispatchRecord.Queue(a);"));
        Assert.Equal(
            [(NotificationChannelSourceScan.GatewayOutsideTheSender, 1)],
            NotificationChannelSourceScan.Scan(NotificationChannelSourceScan.QueueFiles[0], text));
        Assert.Equal(
            [(NotificationChannelSourceScan.DispatchRowOutsideTheQueues, 1)],
            NotificationChannelSourceScan.Scan(NotificationChannelSourceScan.GatewayFiles[3], text));
    }

    [Fact]
    public void Scan_AFileOutsideTheSourceFolderOrOfAnotherKind_IsNotRead()
    {
        Assert.Empty(NotificationChannelSourceScan.Scan("tests/Shopfolio.UnitTests/Foo.cs", SampleText()));
        Assert.Empty(NotificationChannelSourceScan.Scan("src/Shopfolio.Web/obj/Foo.cs", SampleText()));
        Assert.Empty(NotificationChannelSourceScan.Scan("src/Shopfolio.Web/Foo.txt", SampleText()));
    }
}
