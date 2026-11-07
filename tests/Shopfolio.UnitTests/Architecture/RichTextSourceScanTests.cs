using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the rich text scan over the application source and shows on a deliberate violation sample that each rule goes red (T339 KK8; 09 15.3 item 7; 09 19.2 item 3). The scan reads
/// files and never writes one.
/// </summary>
public sealed class RichTextSourceScanTests
{
    private const string SamplePath = "Architecture/ViolationSamples/RichText/RawMarkupLeak.cs";

    private static string Sample() => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", SamplePath));

    [Fact]
    public void ApplicationSource_NamesTheCleanerOnlyInItsClass_AndPrintsRawMarkupOnlyInTheDisplayHelper()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.*", SearchOption.AllDirectories)
                     .Where(f => f.EndsWith(".cs", StringComparison.Ordinal) || f.EndsWith(".cshtml", StringComparison.Ordinal) || f.EndsWith(".razor", StringComparison.Ordinal)))
        {
            string path = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (path.Contains("/obj/", StringComparison.Ordinal) || path.Contains("/bin/", StringComparison.Ordinal))
            {
                continue;
            }

            scanned.Add(path);
            foreach ((string rule, int line) in RichTextSourceScan.Scan(path, File.ReadAllText(file)))
            {
                findings.Add($"{path}:{line} {rule}");
            }
        }

        Assert.Contains(RichTextSourceScan.SanitizerFile, scanned);
        Assert.Contains(RichTextSourceScan.DisplayFile, scanned);
        Assert.Contains(RichTextSourceScan.SearchHeadFile, scanned);
        Assert.Contains(scanned, f => f.StartsWith("src/Shopfolio.Web/Modules/", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void TheNamedFiles_Exist_AndTheSanitizerNamesTheLibraryAndTheDisplayBuildsTheHtmlString()
    {
        string root = RepositoryRoot.Find();

        Assert.Contains("using Ganss.Xss;", File.ReadAllText(Path.Combine(root, RichTextSourceScan.SanitizerFile)), StringComparison.Ordinal);
        Assert.Contains("new HtmlString(", File.ReadAllText(Path.Combine(root, RichTextSourceScan.DisplayFile)), StringComparison.Ordinal);
        Assert.Contains("new HtmlString(", File.ReadAllText(Path.Combine(root, RichTextSourceScan.SearchHeadFile)), StringComparison.Ordinal);
    }

    [Fact]
    public void TheSearchHeadException_IsTheTwoHeadBuildersOfTodayAndGrowsByNothing_UntilD357MovesThem()
    {
        string text = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), RichTextSourceScan.SearchHeadFile));

        // D-357: the two places that return the head as markup. A third one is a new raw-markup place and needs its own decision, not this file's exception.
        Assert.Equal(2, System.Text.RegularExpressions.Regex.Count(text, @"\bHtmlString\b", System.Text.RegularExpressions.RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public void Scan_OnViolationSampleInAModule_ReportsEveryRuleOnItsLine()
    {
        IReadOnlyList<(string Rule, int Line)> findings = RichTextSourceScan.Scan("src/Shopfolio.Web/Modules/Catalog/Leak.cs", Sample());

        Assert.Equal(
            [
                (RichTextSourceScan.CleanerOutsideItsClass, 3),
                (RichTextSourceScan.CleanerOutsideItsClass, 6),
                (RichTextSourceScan.HtmlStringOutsideItsPlaces, 7),
                (RichTextSourceScan.RawMarkupOutsideTheDisplay, 8),
                (RichTextSourceScan.RawMarkupOutsideTheDisplay, 9),
                (RichTextSourceScan.RawMarkupOutsideTheDisplay, 10),
                (RichTextSourceScan.RawMarkupOutsideTheDisplay, 11),
            ],
            findings);
    }

    [Fact]
    public void Scan_OnTheSanitizersFile_AllowsTheCleaner_ButNotRawMarkup()
    {
        IReadOnlyList<(string Rule, int Line)> findings = RichTextSourceScan.Scan(RichTextSourceScan.SanitizerFile, Sample());

        Assert.DoesNotContain(findings, f => f.Rule == RichTextSourceScan.CleanerOutsideItsClass);
        Assert.Equal([7, 8, 9, 10, 11], findings.Select(f => f.Line));
    }

    [Fact]
    public void Scan_OnTheDisplayFile_AllowsRawMarkupAndHtmlString_ButNotTheCleaner()
    {
        IReadOnlyList<(string Rule, int Line)> findings = RichTextSourceScan.Scan(RichTextSourceScan.DisplayFile, Sample());

        Assert.Equal([(RichTextSourceScan.CleanerOutsideItsClass, 3), (RichTextSourceScan.CleanerOutsideItsClass, 6)], findings);
    }

    [Fact]
    public void Scan_OnTheSearchHeadFile_AllowsHtmlString_ButNotTheOtherRawMarkupOrTheCleaner()
    {
        IReadOnlyList<(string Rule, int Line)> findings = RichTextSourceScan.Scan(RichTextSourceScan.SearchHeadFile, Sample());

        Assert.DoesNotContain(findings, f => f.Rule == RichTextSourceScan.HtmlStringOutsideItsPlaces);
        Assert.Equal([3, 6, 8, 9, 10, 11], findings.Select(f => f.Line));
    }

    [Fact]
    public void Scan_OnAFolderThatOnlyStartsLikeTheSecurityFolder_StillReportsEverything()
    {
        IReadOnlyList<(string Rule, int Line)> findings = RichTextSourceScan.Scan("src/Shopfolio.Web/Infrastructure/SecurityExtras/RichText.cs", Sample());

        Assert.Equal(7, findings.Count);
    }

    [Fact]
    public void Scan_AFullLineCommentThatSaysTheNames_IsNotAFinding()
    {
        IReadOnlyList<(string Rule, int Line)> findings = RichTextSourceScan.Scan(
            "src/Shopfolio.Web/Modules/Catalog/Note.cs",
            "// Html.Raw, HtmlSanitizer and new HtmlString( are named here in a comment\n/// <see cref=\"HtmlString\"/>\ninternal sealed class Note { }\n");

        Assert.Empty(findings);
    }
}
