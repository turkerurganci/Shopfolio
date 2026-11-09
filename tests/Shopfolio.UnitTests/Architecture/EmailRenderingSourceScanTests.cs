using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the e-mail rendering scan over the application source and shows on a deliberate violation sample that each rule goes red in every shape (T305 KK2, KK5,
/// KK6; 09 11.4 items 2 and 7; 09 19.2 item 3). The scan reads files and never writes one.
/// </summary>
public sealed class EmailRenderingSourceScanTests
{
    private const string Sample = "Architecture/ViolationSamples/EmailRendering/Misuse.cs";
    private const string DocumentsSample = "Architecture/ViolationSamples/EmailRendering/DocumentsMisuse.cs";
    private const string InFolder = EmailRenderingSourceScan.Folder + "Misuse.cs";

    private static string SampleText() => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", Sample));

    [Fact]
    public void ApplicationSource_OfTheEmailNamespace_HasNoRawMarkupAndNoRequestHeader()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".cs", StringComparison.Ordinal) || f.EndsWith(".razor", StringComparison.Ordinal)))
        {
            string relative = Path.GetRelativePath(root, file);
            string path = relative.Replace('\\', '/');
            if (path.Contains("/obj/", StringComparison.Ordinal) || path.Contains("/bin/", StringComparison.Ordinal))
            {
                continue;
            }

            string text = File.ReadAllText(file);
            if (EmailRenderingSourceScan.InScope(path, text))
            {
                scanned.Add(path);
            }

            foreach ((string rule, int line) in EmailRenderingSourceScan.Scan(relative, text))
            {
                findings.Add($"{path}:{line} {rule}");
            }
        }

        // The scan really reached the frame component and the composer - a folder that is not read would pass silently.
        Assert.Contains(EmailRenderingSourceScan.Folder + "EmailFrame.razor", scanned);
        Assert.Contains(EmailRenderingSourceScan.Folder + "EmailComposer.cs", scanned);
        Assert.Contains(EmailRenderingSourceScan.Folder + "EmailTextWriter.cs", scanned);
        Assert.Contains(EmailRenderingSourceScan.DocumentsFolder + "OrderDocuments.cs", scanned);
        Assert.Contains(EmailRenderingSourceScan.DocumentsFolder + "ConfirmationSummaryText.cs", scanned);
        Assert.Empty(findings);
    }

    [Fact]
    public void Scan_OnTheViolationSample_ReportsEveryShapeOfEveryRule()
    {
        IReadOnlyList<(string Rule, int Line)> findings = EmailRenderingSourceScan.Scan(InFolder, SampleText());

        Assert.Equal(
            [
                (EmailRenderingSourceScan.RawMarkup, 7),
                (EmailRenderingSourceScan.RawMarkup, 8),
                (EmailRenderingSourceScan.RawMarkup, 9),
                (EmailRenderingSourceScan.RawMarkup, 10),
                (EmailRenderingSourceScan.RawMarkup, 11), // Html.Raw split over lines 11-12: reported at its first line
                (EmailRenderingSourceScan.RawMarkup, 13),
                (EmailRenderingSourceScan.RawMarkup, 14),
                (EmailRenderingSourceScan.RawMarkup, 15),
                (EmailRenderingSourceScan.RequestHeaderOrObject, 16),
                (EmailRenderingSourceScan.RequestHeaderOrObject, 17),
                (EmailRenderingSourceScan.RequestHeaderOrObject, 18),
                (EmailRenderingSourceScan.RequestHeaderOrObject, 19),
                (EmailRenderingSourceScan.RequestHeaderOrObject, 20),
                (EmailRenderingSourceScan.RequestHeaderOrObject, 21),
            ],
            findings);
    }

    [Fact]
    public void Scan_TheOrderDocumentsFolderAndNamespace_AreReadWithTheSameRules()
    {
        string sample = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", DocumentsSample));
        (string Rule, int Line)[] expected =
        [
            (EmailRenderingSourceScan.RawMarkup, 7),
            (EmailRenderingSourceScan.RawMarkup, 8),
            (EmailRenderingSourceScan.RawMarkup, 9),
            (EmailRenderingSourceScan.RequestHeaderOrObject, 10),
            (EmailRenderingSourceScan.RequestHeaderOrObject, 11),
        ];

        // By its folder, and - anywhere else in the source - by the namespace it declares.
        Assert.Equal(expected, EmailRenderingSourceScan.Scan(EmailRenderingSourceScan.DocumentsFolder + "DocumentsMisuse.cs", sample.Replace("Shopfolio.Modules.Ordering.Documents", "Other", StringComparison.Ordinal)));
        Assert.Equal(expected, EmailRenderingSourceScan.Scan("src/Shopfolio.Web/Modules/Ordering/Misuse.cs", sample));
        Assert.Empty(EmailRenderingSourceScan.Scan("src/Shopfolio.Web/Modules/Ordering/Misuse.cs", sample.Replace("Documents", "Other", StringComparison.Ordinal)));
    }

    [Fact]
    public void Scan_AFileOutsideTheFolder_IsReadOnlyWhenItDeclaresTheEmailNamespace()
    {
        string sample = SampleText();

        Assert.NotEmpty(EmailRenderingSourceScan.Scan("src/Shopfolio.Web/Modules/Notifications/Misuse.cs", sample));
        Assert.Empty(EmailRenderingSourceScan.Scan("src/Shopfolio.Web/Modules/Notifications/Misuse.cs", sample.Replace("EmailComposition", "Other", StringComparison.Ordinal)));
    }

    [Fact]
    public void Scan_ARazorFile_IsReadToo_AndRazorAndLineCommentsAreNotFindings()
    {
        const string razor = """
            @* MarkupString, Html.Raw and Request.Host in a Razor comment are not findings. *@
            @* A comment over
               several lines: HttpContext *@
            <p>@((MarkupString)Model.Text)</p>
            // HtmlString in a line comment
            <p>@Html.Raw(Model.Text)</p>
            """;

        IReadOnlyList<(string Rule, int Line)> findings = EmailRenderingSourceScan.Scan(EmailRenderingSourceScan.Folder + "Other.razor", razor);

        Assert.Equal([(EmailRenderingSourceScan.RawMarkup, 4), (EmailRenderingSourceScan.RawMarkup, 6)], findings);
    }

    [Fact]
    public void Scan_TheEncodingNames_AreNotFindings()
    {
        const string text = """
            using System.Text.Encodings.Web;
            var encoded = HtmlEncoder.Default.Encode(value);
            var line = WebUtility.HtmlEncode(value);
            var hostName = options.Domain;
            """;

        Assert.Empty(EmailRenderingSourceScan.Scan(EmailRenderingSourceScan.Folder + "Fine.cs", text));
    }
}
