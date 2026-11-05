using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the image library scan over the application source and shows on a deliberate violation sample that each rule goes red (T223 KK1;
/// 09 15.6 item 1; 09 19.2 item 3). The scan reads files and never writes one.
/// </summary>
public sealed class ImagingLibraryScanTests
{
    private const string SamplePath = "Architecture/ViolationSamples/Imaging/LibraryLeak.cs";

    private static string Sample() => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", SamplePath));

    [Fact]
    public void ApplicationSource_NamesTheLibraryOnlyInTheImagingFolder_AndNeverARejectedOne()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.*", SearchOption.AllDirectories)
                     .Where(f => f.EndsWith(".cs", StringComparison.Ordinal) || f.EndsWith(".cshtml", StringComparison.Ordinal)))
        {
            string path = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (path.Contains("/obj/", StringComparison.Ordinal) || path.Contains("/bin/", StringComparison.Ordinal))
            {
                continue;
            }

            scanned.Add(path);
            foreach ((string rule, int line) in ImagingLibraryScan.Scan(path, File.ReadAllText(file)))
            {
                findings.Add($"{path}:{line} {rule}");
            }
        }

        Assert.Contains(ImagingLibraryScan.ImagingFolder + "ImageProcessor.cs", scanned);
        Assert.Contains(scanned, f => f.StartsWith("src/Shopfolio.Web/Modules/", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void NamedFolder_Exists_AndTheProcessorNamesTheLibrary()
    {
        string root = RepositoryRoot.Find();

        Assert.True(Directory.Exists(Path.Combine(root, ImagingLibraryScan.ImagingFolder)));
        Assert.Contains("using SkiaSharp;", File.ReadAllText(Path.Combine(root, ImagingLibraryScan.ImagingFolder, "ImageProcessor.cs")), StringComparison.Ordinal);
    }

    [Fact]
    public void Scan_OnViolationSampleInAModule_ReportsTheLibraryAndBothRejectedOnesOnTheirLines()
    {
        IReadOnlyList<(string Rule, int Line)> findings = ImagingLibraryScan.Scan("src/Shopfolio.Web/Modules/Catalog/Leak.cs", Sample());

        Assert.Equal(
            [
                (ImagingLibraryScan.LibraryOutsideImaging, 3),
                (ImagingLibraryScan.RejectedLibrary, 4),
                (ImagingLibraryScan.RejectedLibrary, 8),
            ],
            findings);
    }

    [Fact]
    public void Scan_OnViolationSampleInTheImagingFolder_AllowsTheLibrary_ButNotARejectedOne()
    {
        IReadOnlyList<(string Rule, int Line)> findings = ImagingLibraryScan.Scan(ImagingLibraryScan.ImagingFolder + "Leak.cs", Sample());

        Assert.Equal([(ImagingLibraryScan.RejectedLibrary, 4), (ImagingLibraryScan.RejectedLibrary, 8)], findings);
    }

    [Fact]
    public void Scan_OnAFolderThatOnlyStartsLikeTheImagingFolder_StillReportsTheLibrary()
    {
        IReadOnlyList<(string Rule, int Line)> findings = ImagingLibraryScan.Scan("src/Shopfolio.Web/Infrastructure/ImagingExtras/Leak.cs", Sample());

        Assert.Contains(findings, f => f.Rule == ImagingLibraryScan.LibraryOutsideImaging && f.Line == 3);
    }
}
