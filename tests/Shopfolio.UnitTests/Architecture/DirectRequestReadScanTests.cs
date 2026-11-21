using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// A page model reads the request only through the handler's parameters and the bound input, except the reads on the allowed list (D-1614; 07 5.3.1; 09 3.4),
/// over the real page files and on a deliberate violation sample. The list is held both ways: a read that is not on it breaks the test, and an entry that no
/// page reads (or reads a different number of times) breaks it too.
/// </summary>
public sealed class DirectRequestReadScanTests
{
    private const string AnyPage = "src/Shopfolio.Web/Pages/Account/Sample.cshtml.cs";

    [Fact]
    public void ThePageModels_ReadTheRequestDirectly_OnlyWhereTheAllowedListNamesIt_D1614()
    {
        string root = RepositoryRoot.Find();
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src", "Shopfolio.Web", "Pages"), "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            findings.AddRange(DirectRequestReadScan.Scan(relative, File.ReadAllText(file)).Select(f => $"{relative}:{f.Line} {f.Rule} {f.Read}"));
        }

        Assert.Empty(findings);
    }

    [Fact]
    public void EveryAllowedRead_IsStillReadAsManyTimesAsTheListSays_SoTheListHoldsNoStaleEntry_D1614()
    {
        string root = RepositoryRoot.Find();
        List<string> differing = [];
        foreach (IGrouping<string, (string File, string Read, int Count, string Reason)> file in DirectRequestReadScan.Allowed.GroupBy(a => a.File, StringComparer.Ordinal))
        {
            Assert.True(DirectRequestReadScan.AppliesTo(file.Key), file.Key);
            Assert.All(file, entry => Assert.False(string.IsNullOrWhiteSpace(entry.Reason), $"{entry.File} {entry.Read}: no reason"));
            IReadOnlyList<(string Read, int Line)> reads = DirectRequestReadScan.Reads(File.ReadAllText(Path.Combine(root, file.Key)));
            foreach (IGrouping<string, (string File, string Read, int Count, string Reason)> entry in file.GroupBy(a => a.Read, StringComparer.Ordinal))
            {
                int listed = entry.Sum(e => e.Count);
                int found = reads.Count(r => r.Read == entry.Key);
                if (listed != found)
                {
                    differing.Add($"{file.Key} {entry.Key}: listed {listed}, read {found}");
                }
            }
        }

        Assert.Empty(differing);
    }

    [Fact]
    public void OnTheSample_EveryDirectReadIsReported_AndTheNearFormsAreNot_D1614()
    {
        IReadOnlyList<(string Rule, int Line, string Read)> findings = DirectRequestReadScan.Scan(AnyPage, Sample());

        Assert.Equal(
            [
                (DirectRequestReadScan.UnlistedRead, 7, "Form[\"Input.Name\"]"),
                (DirectRequestReadScan.UnlistedRead, 8, "Query[\"page\"]"),
                (DirectRequestReadScan.UnlistedRead, 9, "Form.ContainsKey"),
                (DirectRequestReadScan.UnlistedRead, 10, "Query.TryGetValue"),
                (DirectRequestReadScan.UnlistedRead, 11, "ReadFormAsync"),
                (DirectRequestReadScan.UnlistedRead, 12, "Form[nameof(Input.Name)]"),
            ],
            findings);
    }

    [Fact]
    public void AnAllowedRead_IsOnlyAllowedInItsOwnFile_AndOnlyAsOftenAsTheListSays_D1614()
    {
        const string cart = "src/Shopfolio.Web/Pages/Storefront/Cart.cshtml.cs";
        string text = "a = Request.Form[nameof(UpdateCartItemInput.Quantity)];\nb = Request.Form[nameof(UpdateCartItemInput.Quantity)];\nc = Request.Form[nameof(UpdateCartItemInput.Quantity)];";

        IReadOnlyList<(string Rule, int Line, string Read)> third = DirectRequestReadScan.Scan(cart, text);
        IReadOnlyList<(string Rule, int Line, string Read)> otherFile = DirectRequestReadScan.Scan("src/Shopfolio.Web/Pages/Storefront/Other.cshtml.cs", "a = Request.Form[nameof(UpdateCartItemInput.Quantity)];");

        Assert.Equal([(DirectRequestReadScan.UnlistedRead, 3, "Form[nameof(UpdateCartItemInput.Quantity)]")], third);
        Assert.Single(otherFile);
    }

    [Theory]
    [InlineData("src/Shopfolio.Web/Pages/Account/A.cshtml.cs", true)]
    [InlineData("src\\Shopfolio.Web\\Pages\\Account\\A.cshtml.cs", true)]
    [InlineData("src/Shopfolio.Web/Pages/Account/A.cshtml", false)]
    [InlineData("src/Shopfolio.Web/Infrastructure/A.cs", false)]
    public void TheScan_AppliesToThePageModelFilesOnly(string path, bool applies)
    {
        Assert.Equal(applies, DirectRequestReadScan.AppliesTo(path));
    }

    private static string Sample() =>
        File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "RequestReads", "DirectRequestReads.cs"));
}
