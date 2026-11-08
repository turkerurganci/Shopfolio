using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the cookie scan over the application source and shows on a deliberate violation sample that each rule goes red (T224 KK3; 09 15.1 item 6;
/// 09 5.3 item 5; 09 19.2 item 3). The scan reads files and never writes one.
/// </summary>
public sealed class CookieSourceScanTests
{
    [Fact]
    public void ApplicationSource_WritesCookiesInBrowserCookiesAndNamesThemInCookieNamesOnly()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file);
            string path = relative.Replace('\\', '/');
            if (path.Contains("/obj/", StringComparison.Ordinal) || path.Contains("/bin/", StringComparison.Ordinal))
            {
                continue;
            }

            scanned.Add(path);
            foreach ((string rule, int line) in CookieSourceScan.Scan(relative, File.ReadAllText(file)))
            {
                findings.Add($"{path}:{line} {rule}");
            }
        }

        Assert.Contains(scanned, f => f.EndsWith("Program.cs", StringComparison.Ordinal));
        Assert.Contains(CookieSourceScan.BrowserCookiesFile, scanned);
        Assert.Contains(CookieSourceScan.DefaultsFile, scanned);
        Assert.Contains(CookieSourceScan.NamesFile, scanned);
        Assert.Contains(CookieSourceScan.HeaderFilterFile, scanned);
        Assert.Empty(findings);
    }

    [Fact]
    public void Scan_OnViolationSample_ReportsEachRuleAndNotTheCommentTheConstantOrTheRequestCookie()
    {
        string sample = File.ReadAllText(Path.Combine(
            RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "Cookies", "CookieMisuse.cs"));

        IReadOnlyList<(string Rule, int Line)> findings = CookieSourceScan.Scan("src/Shopfolio.Web/Modules/Cart/Misuse.cs", sample);

        Assert.Equal(
            [
                (CookieSourceScan.ResponseCookieWrite, 5),
                (CookieSourceScan.ResponseCookieWrite, 6),
                (CookieSourceScan.HostPrefixLiteral, 7),
                (CookieSourceScan.CookieAttributes, 8),
                (CookieSourceScan.ConsentBanner, 9),
                (CookieSourceScan.ResponseCookieWrite, 10),
                (CookieSourceScan.CookieAttributes, 10),
                (CookieSourceScan.RawSetCookieHeader, 13),
                (CookieSourceScan.RawSetCookieHeader, 14),
                (CookieSourceScan.RawSetCookieHeader, 15),
            ],
            findings);
    }

    [Fact]
    public void Scan_InEachAllowedFile_AllowsItsOneRuleAndNothingElse()
    {
        const string text = "Cookies.Append(\"__Host-a\", \"x\"); var o = new CookieOptions { SameSite = SameSiteMode.Lax };";

        Assert.Equal(
            [(CookieSourceScan.CookieAttributes, 1), (CookieSourceScan.HostPrefixLiteral, 1)],
            CookieSourceScan.Scan(CookieSourceScan.BrowserCookiesFile, text));
        Assert.Equal(
            [(CookieSourceScan.ResponseCookieWrite, 1), (CookieSourceScan.HostPrefixLiteral, 1)],
            CookieSourceScan.Scan(@"src\Shopfolio.Web\Infrastructure\Security\CookieDefaults.cs", text));
        Assert.Equal(
            [(CookieSourceScan.ResponseCookieWrite, 1), (CookieSourceScan.CookieAttributes, 1)],
            CookieSourceScan.Scan(CookieSourceScan.NamesFile, text));
    }

    [Fact]
    public void Scan_OfARawSetCookieHeaderWithANameOutsideTheList_IsAFindingEverywhereButInTheHeaderFilter()
    {
        string[] writes =
        [
            "context.Response.Headers.Append(\"Set-Cookie\", \"_ga=1\");",
            "context.Response.Headers.SetCookie = \"_ga=1\";",
            "context.Response.Headers[HeaderNames.SetCookie] = \"_ga=1\";",
            "context.Response.Headers[\"set-cookie\"] = \"_ga=1\";",
            "context.Response.Headers.Add(\"Set-Cookie\", new StringValues(\"_ga=1\"));",
        ];

        foreach (string write in writes)
        {
            Assert.Equal([(CookieSourceScan.RawSetCookieHeader, 1)], CookieSourceScan.Scan("src/Shopfolio.Web/Modules/Cart/Misuse.cs", write));
            Assert.Equal([(CookieSourceScan.RawSetCookieHeader, 1)], CookieSourceScan.Scan(CookieSourceScan.BrowserCookiesFile, write));
            Assert.Equal([(CookieSourceScan.RawSetCookieHeader, 1)], CookieSourceScan.Scan(CookieSourceScan.HeaderFilterFile, write));
        }
    }

    [Fact]
    public void TheHeaderFilterFile_MayOnlyRemoveTheHeader_D114()
    {
        const string removal = "removal.Response.Headers.Remove(HeaderNames.SetCookie);";
        const string split = "removal.Response.Headers\n    .Remove(\n        HeaderNames.SetCookie);\ncontext.Response.Headers.Append(\"Set-Cookie\", \"_ga=1\");";

        Assert.Empty(CookieSourceScan.Scan(CookieSourceScan.HeaderFilterFile, removal));
        Assert.Equal([(CookieSourceScan.RawSetCookieHeader, 4)], CookieSourceScan.Scan(CookieSourceScan.HeaderFilterFile, split));
        Assert.Equal([(CookieSourceScan.RawSetCookieHeader, 1)], CookieSourceScan.Scan("src/Shopfolio.Web/Modules/Cart/Misuse.cs", removal));
    }

    [Fact]
    public void AllowedFiles_Exist()
    {
        string root = RepositoryRoot.Find();

        Assert.True(File.Exists(Path.Combine(root, CookieSourceScan.BrowserCookiesFile)));
        Assert.True(File.Exists(Path.Combine(root, CookieSourceScan.DefaultsFile)));
        Assert.True(File.Exists(Path.Combine(root, CookieSourceScan.NamesFile)));
        Assert.True(File.Exists(Path.Combine(root, CookieSourceScan.HeaderFilterFile)));
    }
}
