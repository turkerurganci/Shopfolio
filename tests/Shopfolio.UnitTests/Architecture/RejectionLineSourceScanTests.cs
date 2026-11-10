using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The rejection line is written in two places only (T213 criterion 8, KK8; 09 8.2 item 3, 18.2 item 4; D-132), over the real files and on a deliberate
/// violation sample (09 19.2 item 3): the scan finds a refusal that writes a line of its own in every quoting form of the template, and it spares exactly
/// the two writers, by path.
/// </summary>
public sealed class RejectionLineSourceScanTests
{
    [Fact]
    public void TheApplication_WritesTheRejectionEventAndTheCodeFieldInTheTwoWritersOnly()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file);
            if (!RejectionLineSourceScan.AppliesTo(relative) || relative.Replace('\\', '/').Contains("/obj/", StringComparison.Ordinal) || relative.Replace('\\', '/').Contains("/bin/", StringComparison.Ordinal))
            {
                continue;
            }

            scanned.Add(relative.Replace('\\', '/'));
            findings.AddRange(RejectionLineSourceScan.Scan(relative, File.ReadAllText(file)).Select(f => $"{relative}:{f.Line} {f.Rule}"));
        }

        Assert.Contains(RejectionLineSourceScan.WriterFile, scanned);
        Assert.Contains(RejectionLineSourceScan.LayerFile, scanned);
        Assert.Empty(findings);
    }

    [Fact]
    public void TheTwoWriters_StillHoldTheFormsTheScanSpares_SoTheScanIsNotEmpty()
    {
        string root = RepositoryRoot.Find();
        string writer = File.ReadAllText(Path.Combine(root, RejectionLineSourceScan.WriterFile));
        string layer = File.ReadAllText(Path.Combine(root, RejectionLineSourceScan.LayerFile));

        string[] writerFindings = [.. RejectionLineSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Other/Copy.cs", writer).Select(f => f.Rule)];
        string[] layerFindings = [.. RejectionLineSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Other/Copy.cs", layer).Select(f => f.Rule)];

        Assert.Contains(RejectionLineSourceScan.EventOutsideTheWriter, writerFindings);
        Assert.Contains(RejectionLineSourceScan.CodeFieldOutsideTheTwoWriters, writerFindings);
        Assert.Contains(RejectionLineSourceScan.CodeFieldOutsideTheTwoWriters, layerFindings);
    }

    [Theory]
    [InlineData("src/Shopfolio.Web/Infrastructure/Other/RejectionLog.cs")]
    [InlineData("src/Shopfolio.Web/Infrastructure/Errors/RejectionLog.cs.bak")]
    [InlineData("src/Shopfolio.Web/Infrastructure/Errors/rejectionlog.cs")]
    [InlineData("src/Shopfolio.Web/Infrastructure/Errors/Sub/RejectionLog.cs")]
    [InlineData("src/Shopfolio.Web/Infrastructure/Errors/UnexpectedErrorLayers.cs")]
    public void TheExemption_IsTheExactPathOnly(string relativePath)
    {
        IReadOnlyList<(string Rule, int Line)> findings = RejectionLineSourceScan.Scan(relativePath, "log.LogWarning(\"{Event} {Code}\", LogEvents.ValidationRejected, c);");

        Assert.NotEmpty(findings);
    }

    [Fact]
    public void TheExemption_IsPerRule_TheLayerMayWriteTheCodeButNotTheRejectionEvent()
    {
        const string line = "log.LogError(\"{Event} {Code}\", LogEvents.ValidationRejected, c);";

        Assert.Equal(
            [RejectionLineSourceScan.EventOutsideTheWriter],
            RejectionLineSourceScan.Scan(RejectionLineSourceScan.LayerFile, line).Select(f => f.Rule));
        Assert.Empty(RejectionLineSourceScan.Scan(RejectionLineSourceScan.WriterFile, line));
    }

    [Fact]
    public void OnViolationSample_EachFormIsReported_AndTheNearNamesAreNot()
    {
        IReadOnlyList<(string Rule, int Line)> findings = RejectionLineSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Forms/Sample.cs", Sample("RejectionLineElsewhere.cs"));

        Assert.Equal(
            [
                (RejectionLineSourceScan.EventOutsideTheWriter, 9),
                (RejectionLineSourceScan.CodeFieldOutsideTheTwoWriters, 10),
                (RejectionLineSourceScan.CodeFieldOutsideTheTwoWriters, 11),
                (RejectionLineSourceScan.CodeFieldOutsideTheTwoWriters, 12),
                (RejectionLineSourceScan.CodeFieldOutsideTheTwoWriters, 13),
                (RejectionLineSourceScan.CodeFieldOutsideTheTwoWriters, 14),
                (RejectionLineSourceScan.CodeFieldOutsideTheTwoWriters, 15),
                (RejectionLineSourceScan.CodeFieldOutsideTheTwoWriters, 18),
                (RejectionLineSourceScan.ScopeProperty, 20),
                (RejectionLineSourceScan.ScopeProperty, 21),
                (RejectionLineSourceScan.CodeFieldOutsideTheTwoWriters, 24),
                (RejectionLineSourceScan.CodeFieldOutsideTheTwoWriters, 27),
            ],
            findings);
    }

    [Fact]
    public void TheHoleOfAnInterpolatedString_IsNoTemplateProperty_ButThePascalCaseTemplateOnTheSameLineIs()
    {
        const string path = "src/Shopfolio.Web/Infrastructure/Forms/Sample.cs";

        Assert.Empty(RejectionLineSourceScan.Scan(path, "throw new ArgumentException($\"'{code}' is not known.\", nameof(code));"));
        Assert.Single(RejectionLineSourceScan.Scan(path, "log.LogWarning($\"{x}\" + \"{Event} {Code}\", e, c);"));
        Assert.Single(RejectionLineSourceScan.Scan(path, "log.LogWarning(\"{Event} {FIELDS}\", e, c);"));
    }

    // --- T411 criterion 5 (KK5): the same widening as the template scan ---

    [Fact]
    public void OnTheWiderFormsSample_EachFormIsReported_AndTheNearNamesTheHoleAndTheCommentAreNot()
    {
        IReadOnlyList<(string Rule, int Line)> findings = RejectionLineSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Forms/Sample.cs", Sample("RejectionLineForms.cs"));

        Assert.Equal(
            [
                (RejectionLineSourceScan.CodeFieldOutsideTheTwoWriters, 6),
                (RejectionLineSourceScan.CodeFieldOutsideTheTwoWriters, 12),
                (RejectionLineSourceScan.ErrorCodeArgument, 13),
                (RejectionLineSourceScan.ErrorCodeArgument, 14),
                (RejectionLineSourceScan.CodeFieldOutsideTheTwoWriters, 15),
                (RejectionLineSourceScan.ScopeProperty, 16),
                (RejectionLineSourceScan.EventOutsideTheWriter, 17),
            ],
            findings);
    }

    [Fact]
    public void OnTheViewSample_EachFormIsReportedLineByLine_AndTheTextOfTheViewIsNot()
    {
        IReadOnlyList<(string Rule, int Line)> findings = RejectionLineSourceScan.Scan("src/Shopfolio.Web/Pages/Sample.cshtml", Sample("RejectionLineView.cshtml"));

        Assert.Equal(
            [
                (RejectionLineSourceScan.CodeFieldOutsideTheTwoWriters, 4),
                (RejectionLineSourceScan.ErrorCodeArgument, 5),
                (RejectionLineSourceScan.EventOutsideTheWriter, 6),
                (RejectionLineSourceScan.ScopeProperty, 7),
            ],
            findings);
    }

    [Theory]
    [InlineData("log.LogWarning(\"see https://x {Event} {Code}\", e, c);")]
    [InlineData("log.LogWarning($\"x {{Code}} {y}\", e, c);")]
    [InlineData("log.LogWarning(@\"a \"\"b\"\" {Fields}\", e, c);")]
    [InlineData("log.LogWarning(\"\"\"{Code}\"\"\", e, c);")]
    [InlineData("log.LogWarning(\"\"\"\n  {Event}\n  {ErrorCode}\n  \"\"\", e, c);")]
    [InlineData("log.LogWarning(\"{Event} {ErrorCode}\", e, c);")]
    [InlineData("log.LogWarning(\"{Event} {RejectionCode}\", e, c);")]
    [InlineData("const string T = \"{Event} {Code}\";")]
    [InlineData("[LoggerMessage(LogLevel.Warning, \"{Event} {Code}\")]")]
    [InlineData("[LoggerMessageAttribute(Level = LogLevel.Warning, Message = \"{Event} {Fields}\")]")]
    [InlineData("log.LogWarning(\"{Event} {Reason}\", e, ErrorCode.NotFound);")]
    [InlineData("log.LogWarning(message: \"{Event} {Reason}\", args: [e, ErrorCode.NotFound]);")]
    public void EachForm_OutsideTheTwoWriters_IsReported(string source)
    {
        Assert.NotEmpty(RejectionLineSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Forms/Sample.cs", source));
    }

    [Theory]
    [InlineData("// log.LogWarning(\"{Event} {Code}\", e, ErrorCode.NotFound);")]
    [InlineData("/* {Code} {Fields} ValidationRejected */")]
    [InlineData("log.LogWarning(\"{Event} {CodePoint} {Fieldset} {StatusCode}\", e, 1, 2, 3);")]
    [InlineData("log.LogWarning($\"{code} {fields}\", e);")]
    [InlineData("var code = ErrorCode.NotFound;")]
    public void ACommentTheHoleOfAString_AndTheNearNames_AreNotReported(string source)
    {
        Assert.Empty(RejectionLineSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Forms/Sample.cs", source));
    }

    [Theory]
    [InlineData("Code", true)]
    [InlineData("FIELDS", true)]
    [InlineData("ErrorCode", true)]
    [InlineData("LastRejectionCode", true)]
    [InlineData("CodePoint", false)]
    [InlineData("Fieldset", false)]
    [InlineData("StatusCode", false)]
    public void IsCodeOrFieldsName_IsTheRuleOfTheNames(string name, bool expected)
    {
        Assert.Equal(expected, RejectionLineSourceScan.IsCodeOrFieldsName(name));
    }

    [Fact]
    public void Scan_AppliesToTheApplicationSourceOnly()
    {
        Assert.True(RejectionLineSourceScan.AppliesTo("src/Shopfolio.Web/Infrastructure/Errors/RejectionLog.cs"));
        Assert.True(RejectionLineSourceScan.AppliesTo(@"src\Shopfolio.Web\Pages\Cart.cshtml.cs"));
        Assert.True(RejectionLineSourceScan.AppliesTo("src/Shopfolio.Web/Pages/Cart.cshtml"));
        Assert.True(RejectionLineSourceScan.AppliesTo("src/Shopfolio.Web/Pages/Cart.razor"));
        Assert.False(RejectionLineSourceScan.AppliesTo("src/Shopfolio.Web/Pages/site.css"));
        Assert.False(RejectionLineSourceScan.AppliesTo("tests/Shopfolio.IntegrationTests/Logging/SystemLogsTests.cs"));
    }

    private static string Sample(string name) =>
        File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "Logging", name));
}
