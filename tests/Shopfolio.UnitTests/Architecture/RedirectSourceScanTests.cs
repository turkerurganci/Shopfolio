using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the redirect scan over the application source and shows on a deliberate violation sample that each form goes red (T207 KK6; 05 6.5.6; 07 2.3.2;
/// D-118). The scan reads files and never writes one.
/// </summary>
public sealed class RedirectSourceScanTests
{
    [Fact]
    public void ApplicationSource_WritesRedirectsInTheTwoAllowedFilesOnly()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)))
        {
            string relative = Path.GetRelativePath(root, file);
            string path = relative.Replace('\\', '/');
            if (path.Contains("/obj/", StringComparison.Ordinal) || path.Contains("/bin/", StringComparison.Ordinal))
            {
                continue;
            }

            scanned.Add(path);
            foreach ((string rule, int line) in RedirectSourceScan.Scan(relative, File.ReadAllText(file)))
            {
                findings.Add($"{path}:{line} {rule}");
            }
        }

        Assert.Contains(scanned, f => f.EndsWith("Program.cs", StringComparison.Ordinal));
        Assert.Contains(RedirectSourceScan.FormResultsFile, scanned);
        Assert.Contains(RedirectSourceScan.HandlerFile, scanned);
        Assert.Empty(findings);
    }

    [Fact]
    public void Scan_OnViolationSample_ReportsEachFormAndNotTheCommentOrTheAutoRedirectProperty()
    {
        string sample = File.ReadAllText(Path.Combine(
            RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "Redirects", "RedirectMisuse.cs"));

        IReadOnlyList<(string Rule, int Line)> findings = RedirectSourceScan.Scan("src/Shopfolio.Web/Modules/Cart/Misuse.cs", sample);

        Assert.Equal(
            [
                (RedirectSourceScan.RedirectCall, 5),
                (RedirectSourceScan.RedirectCall, 6),
                (RedirectSourceScan.RedirectCall, 7),
                (RedirectSourceScan.RedirectCall, 8),
                (RedirectSourceScan.RedirectCall, 9),
                (RedirectSourceScan.RedirectCall, 10),
                (RedirectSourceScan.RedirectCall, 11),
                (RedirectSourceScan.RedirectCall, 12),
                (RedirectSourceScan.RedirectCall, 13),
                (RedirectSourceScan.LocationHeader, 15),
                (RedirectSourceScan.LocationHeader, 16),
                (RedirectSourceScan.LocationHeader, 17),
                (RedirectSourceScan.RedirectCall, 18),
            ],
            findings);
    }

    [Fact]
    public void Scan_InTheTwoAllowedFiles_FindsNothing_AndInAnyOtherFileFindsTheSameText()
    {
        const string text = "return Redirect(returnUrl); Response.Headers.Location = target;";

        Assert.Empty(RedirectSourceScan.Scan(RedirectSourceScan.FormResultsFile, text));
        Assert.Empty(RedirectSourceScan.Scan(@"src\Shopfolio.Web\Infrastructure\Security\SessionAuthenticationHandler.cs", text));
        Assert.Equal(
            [(RedirectSourceScan.LocationHeader, 1), (RedirectSourceScan.RedirectCall, 1)],
            RedirectSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Forms/FormResultsExtra.cs", text));
        Assert.NotEmpty(RedirectSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Security/Other/SessionAuthenticationHandler.cs", text));
        Assert.NotEmpty(RedirectSourceScan.Scan("src/Shopfolio.Web/Pages/Storefront/FormResults.cs", text));
    }

    [Fact]
    public void Scan_OfEachCallForm_IsAFinding()
    {
        string[] forms =
        [
            "return Redirect(x);",
            "return RedirectPermanent(x);",
            "return RedirectPreserveMethod(x);",
            "return RedirectPermanentPreserveMethod(x);",
            "return LocalRedirect(x);",
            "return LocalRedirectPermanent(x);",
            "return new RedirectResult(x);",
            "return new LocalRedirectResult(x);",
            "context.Response.Redirect(x);",
            "return Results.Redirect(x);",
            "return Results.LocalRedirect(x);",
            "return TypedResults.Redirect(x);",
            "return RedirectToPage(x);",
            "return RedirectToAction(x);",
            "return RedirectToRoute(x);",
            "return Redirect (x);",
            "return Redirect\r\n(x);",
            "properties.RedirectUri = x;",
            "return TypedResults.Redirect<string>(x);",
        ];

        foreach (string form in forms)
        {
            Assert.Equal([(RedirectSourceScan.RedirectCall, 1)], RedirectSourceScan.Scan("src/Shopfolio.Web/Misuse.cs", form));
        }
    }

    [Fact]
    public void Scan_OfEachLocationForm_IsAFinding_AndTheAutoRedirectSettingIsNot()
    {
        string[] forms =
        [
            "context.Response.Headers.Location = x;",
            "context.Response.Headers[HeaderNames.Location] = x;",
            "context.Response.Headers[\"Location\"] = x;",
            "context.Response.Headers.Append(\"location\", x);",
            "context.Response.Headers.Add(\"Location\", x);",
        ];

        foreach (string form in forms)
        {
            Assert.Equal([(RedirectSourceScan.LocationHeader, 1)], RedirectSourceScan.Scan("src/Shopfolio.Web/Misuse.cs", form));
        }

        Assert.Empty(RedirectSourceScan.Scan("src/Shopfolio.Web/Misuse.cs", "new SocketsHttpHandler { AllowAutoRedirect = false };"));
        Assert.Empty(RedirectSourceScan.Scan("src/Shopfolio.Web/Misuse.cs", "// return Redirect(x); Headers.Location = x;"));
    }
}
