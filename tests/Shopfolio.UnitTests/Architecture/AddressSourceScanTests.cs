using System.Text.RegularExpressions;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The page address rules on the application source (T363 KK2, KK3, KK7; 02 3.30.2, 3.30.3, 3.30.7): no redirect from an old address, no platform share code, the
/// record's address written in one place, and the "Paylaş" button is one hidden button that the shared script shows - no external code. Each rule goes red on a deliberate
/// violation sample and stays quiet on the near-miss that is allowed.
/// </summary>
public sealed partial class AddressSourceScanTests
{
    private const string ShareView = "src/Shopfolio.Web/Pages/Shared/_ShareButton.cshtml";
    private const string CommonScript = "src/Shopfolio.Web/wwwroot/js/common.js";

    [GeneratedRegex(@"<button\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ButtonElement();

    [GeneratedRegex(@"https?://|//[a-z0-9.-]+\.[a-z]{2,}", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ExternalAddress();

    private static string Read(string relative) => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), relative));

    [Fact]
    public void ApplicationSource_HasNoRedirectFromAnOldAddress_NoPlatformShareCode_AndTheSlugIsAssignedInOnePlace()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (!AddressSourceScan.Applies(relative))
            {
                continue;
            }

            scanned.Add(relative);
            foreach ((string rule, int line) in AddressSourceScan.Scan(relative, File.ReadAllText(file)))
            {
                findings.Add($"{relative}:{line} {rule}");
            }
        }

        Assert.Contains(AddressSourceScan.ContentEntryFile, scanned);
        Assert.Contains(ShareView, scanned);
        Assert.Contains(CommonScript, scanned);
        Assert.Contains(scanned, f => f.Contains("/Infrastructure/Migrations/", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Theory]
    [InlineData("internal sealed class SlugHistory { }")]
    [InlineData("public string OldSlug { get; set; }")]
    [InlineData("string address = PreviousAddress;")]
    [InlineData("migrationBuilder.CreateTable(name: \"slug_history\");")]
    [InlineData("builder.HasColumnName(\"old_slug\");")]
    [InlineData("app.UseRewriter(options);")]
    [InlineData("options.AddRedirect(\"eski\", \"yeni\");")]
    [InlineData("services.AddSingleton<RedirectMap>();")]
    [InlineData("var redirects = new Dictionary<string, string>();")]
    [InlineData("const redirect = lookup(path);")]
    public void Scan_OfAFormerAddressOrARedirectMap_IsAFinding(string code)
    {
        Assert.Contains(AddressSourceScan.Scan("src/Shopfolio.Web/Modules/Catalog/X.cs", code), f => f.Rule == AddressSourceScan.RedirectFromOldAddress);
    }

    [Theory]
    [InlineData("return Redirect(\"/giris\");")]
    [InlineData("return RedirectToPage(\"/Panel/Orders\");")]
    [InlineData("// OldSlug is not kept: there is no redirect")]
    [InlineData("string slug = Slug.From(name, root);")]
    [InlineData("bool redirected = response.Headers.Location is not null;")]
    public void Scan_OfAnOrdinaryRedirectOrAComment_IsQuiet(string code)
    {
        Assert.DoesNotContain(AddressSourceScan.Scan("src/Shopfolio.Web/Modules/Catalog/X.cs", code), f => f.Rule == AddressSourceScan.RedirectFromOldAddress);
    }

    [Theory]
    [InlineData("var u = \"https://www.facebook.com/sharer/sharer.php?u=\" + link;")]
    [InlineData("var u = \"https://twitter.com/intent/tweet?url=\" + link;")]
    [InlineData("var u = \"https://x.com/intent/post?url=\" + link;")]
    [InlineData("var u = \"https://wa.me/?text=\" + link;")]
    [InlineData("var u = \"https://api.whatsapp.com/send?text=\" + link;")]
    [InlineData("var u = \"https://www.linkedin.com/sharing/share-offsite/?url=\" + link;")]
    [InlineData("var u = \"https://t.me/share/url?url=\" + link;")]
    [InlineData("<script src=\"//s7.addthis.com/js/300/addthis_widget.js\"></script>")]
    [InlineData("<div class=\"sharethis-inline-share-buttons\"></div>")]
    [InlineData("<script src=\"https://connect.facebook.net/tr_TR/sdk.js\"></script>")]
    public void Scan_OfAPlatformShareAddressOrWidget_IsAFinding(string code)
    {
        foreach (string path in new[] { "src/Shopfolio.Web/Pages/Shared/X.cshtml", "src/Shopfolio.Web/wwwroot/js/x.js", "src/Shopfolio.Web/Modules/Catalog/X.cs" })
        {
            Assert.Contains(AddressSourceScan.Scan(path, code), f => f.Rule == AddressSourceScan.PlatformShare);
        }
    }

    [Fact]
    public void Scan_OfTheDeviceMenuAndTheClipboard_IsQuiet()
    {
        const string code = "await navigator.share({ title: document.title, url: link }); await navigator.clipboard.writeText(link);";

        Assert.DoesNotContain(AddressSourceScan.Scan("src/Shopfolio.Web/wwwroot/js/common.js", code), f => f.Rule == AddressSourceScan.PlatformShare);
    }

    [Theory]
    [InlineData("page.Slug = slug;")]
    [InlineData("items[0].Slug = slug;")]
    [InlineData("(first ?? second).Slug =\n    slug;")]
    [InlineData("record . Slug = slug;")]
    public void Scan_OfAnAddressAssignedOutsideTheContentEntry_IsAFinding(string code)
    {
        Assert.Contains(AddressSourceScan.Scan("src/Shopfolio.Web/Modules/Catalog/X.cs", code), f => f.Rule == AddressSourceScan.SlugAssignedOutsideItsPlace);
        Assert.DoesNotContain(AddressSourceScan.Scan(AddressSourceScan.ContentEntryFile, code), f => f.Rule == AddressSourceScan.SlugAssignedOutsideItsPlace);
    }

    [Theory]
    [InlineData("var card = new CardRow { Id = p.Id, Slug = p.Slug };")]
    [InlineData("bool same = page.Slug == slug;")]
    [InlineData("Slug = slug;")]
    [InlineData("var s = p => p.Slug;")]
    public void Scan_OfAnInitializerAComparisonOrAProjection_IsQuiet(string code)
    {
        Assert.DoesNotContain(AddressSourceScan.Scan("src/Shopfolio.Web/Modules/Catalog/X.cs", code), f => f.Rule == AddressSourceScan.SlugAssignedOutsideItsPlace);
    }

    // ---- KK7: the one "Paylaş" button ----

    [Fact]
    public void TheShareView_IsOneHiddenButton_TheScriptShows_WithNoScriptAndNoExternalAddress()
    {
        string view = Read(ShareView);
        string markup = Regex.Replace(view, @"@\*.*?\*@", string.Empty, RegexOptions.Singleline | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));

        Assert.Equal(1, ButtonElement().Count(markup));
        Assert.Contains("data-share", markup, StringComparison.Ordinal);
        Assert.Contains("type=\"button\"", markup, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"<button\b[^>]*\bhidden\b", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)), markup);
        Assert.Contains(">Paylaş<", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("<script", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<a ", markup, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(ExternalAddress().Matches(markup));
        Assert.Empty(AddressSourceScan.Scan(ShareView, view));
    }

    [Fact]
    public void TheSharedScript_UsesTheDeviceMenuThenTheClipboard_ShowsTheButton_AndLoadsNothingFromOutside()
    {
        string script = Read(CommonScript);
        string code = string.Join('\n', script.ReplaceLineEndings("\n").Split('\n').Where(l => !l.TrimStart().StartsWith("//", StringComparison.Ordinal)));

        Assert.Contains("navigator.share", code, StringComparison.Ordinal);
        Assert.Contains("navigator.clipboard", code, StringComparison.Ordinal);
        Assert.Contains("button[data-share]", code, StringComparison.Ordinal);
        Assert.Contains("button.hidden = false", code, StringComparison.Ordinal);
        Assert.Contains("window.location.origin + window.location.pathname", code, StringComparison.Ordinal);
        Assert.Empty(ExternalAddress().Matches(code));
        Assert.DoesNotContain("import(", code, StringComparison.Ordinal);
        Assert.DoesNotContain("createElement", code, StringComparison.Ordinal);
        Assert.DoesNotContain("document.write", code, StringComparison.Ordinal);
        Assert.DoesNotContain("innerHTML", code, StringComparison.Ordinal);
        Assert.Empty(AddressSourceScan.Scan(CommonScript, script));
    }
}
