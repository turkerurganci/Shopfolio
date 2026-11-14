using System.Text.RegularExpressions;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The script-free path and the script's place on the source (T539 KK2, KK4, KK10; 09 16.1 item 2, 16.2 items 1 and 3; 07 8.2; 10 KD-22): the scripts send an intermediate form only with
/// <c>requestSubmit(submitter)</c> (the same handler answers), never lock a button with <c>disabled</c>, call no other endpoint and carry nothing of an installable application; every page module
/// stands where its page's path says and is loaded by a view; the site has no manifest and no service worker. Each rule goes red on a deliberate violation (a text below) and stays quiet on the
/// near-miss that is allowed.
/// </summary>
public sealed partial class NoScriptPathSourceScanTests
{
    private const string Web = "src/Shopfolio.Web";

    /// <summary>The two scripts that predate the page-path rule (09 16.1 item 2): the shared script every page loads, and the product page's, written in T503 before the rule.</summary>
    private static readonly string[] RootScripts = ["common.js", "product.js"];

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BlockComment();

    [GeneratedRegex(@"(?m)^\s*//.*$|(?<=\s)//\s.*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LineComment();

    [GeneratedRegex(@"@\*.*?\*@", RegexOptions.Singleline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RazorComment();

    // A form sent from the script by the plain method: no submitter, so the handler of the pressed button is lost (09 16.2 item 1).
    [GeneratedRegex(@"\.\s*submit\s*\(\s*\)|\brequestSubmit\s*\(\s*\)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ScriptPlainSubmit();

    // A button locked with disabled: the handler value of a disabled button does not enter the form (09 16.2 item 3).
    [GeneratedRegex(@"\.\s*disabled\s*=\s*true\b|setAttribute\s*\(\s*['""]disabled['""]", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ScriptDisablesAButton();

    // What an installable application carries (10 KD-22; 02 3.34.1).
    [GeneratedRegex(@"\bserviceWorker\b|\bbeforeinstallprompt\b|\bappinstalled\b|\bcaches\s*\.|\bmanifest\b|\bwebmanifest\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ScriptApplicationParts();

    [GeneratedRegex(@"<link\b[^>]*\brel\s*=\s*[""']?(?:manifest|apple-touch-startup-image)\b|apple-mobile-web-app|\bserviceWorker\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ViewApplicationParts();

    [GeneratedRegex(@"StaticAsset\(\s*""(?<file>js/[^""]+)""\s*\)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ScriptReference();

    [GeneratedRegex(@"(?<=[a-z0-9])(?=[A-Z])", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex WordBoundary();

    private static string WebRoot => Path.Combine(RepositoryRoot.Find(), Web);

    private static string StripScriptComments(string text) => LineComment().Replace(BlockComment().Replace(text, string.Empty), string.Empty);

    private static IEnumerable<string> Scripts() => Directory.EnumerateFiles(Path.Combine(WebRoot, "wwwroot", "js"), "*.js", SearchOption.AllDirectories);

    private static IEnumerable<string> Views() => Directory.EnumerateFiles(Path.Combine(WebRoot, "Pages"), "*.cshtml", SearchOption.AllDirectories);

    private static string Relative(string root, string file) => Path.GetRelativePath(root, file).Replace('\\', '/');

    private static List<string> ScriptViolations(string script)
    {
        string text = StripScriptComments(script);
        return
        [
            .. ScriptPlainSubmit().Matches(text).Select(m => $"plain submit: {m.Value.Trim()}"),
            .. ScriptDisablesAButton().Matches(text).Select(m => $"disabled button: {m.Value.Trim()}"),
            .. ScriptApplicationParts().Matches(text).Select(m => $"installable-application part: {m.Value}"),
        ];
    }

    // ---- The scripts (09 16.2 items 1, 3; 10 KD-22) ----

    [Fact]
    public void TheScripts_SendFormsOnlyWithTheSubmitter_LockNoButtonWithDisabled_AndCarryNoPartOfAnInstallableApplication_KK2_KK4_KK10()
    {
        string[] scripts = [.. Scripts()];
        Assert.NotEmpty(scripts);
        foreach (string script in scripts)
        {
            Assert.Empty(ScriptViolations(File.ReadAllText(script)));
        }
    }

    [Theory]
    [InlineData("form.submit();")]
    [InlineData("choice.form.submit ( );")]
    [InlineData("form.requestSubmit();")]
    [InlineData("button.disabled = true;")]
    [InlineData("button.setAttribute('disabled', '');")]
    [InlineData("navigator.serviceWorker.register('/sw.js');")]
    [InlineData("window.addEventListener('beforeinstallprompt', show);")]
    [InlineData("const store = await caches.open('shell');")]
    [InlineData("link.href = '/site.webmanifest';")]
    public void AScriptViolation_IsFound_KK2_KK4_KK10(string line) => Assert.NotEmpty(ScriptViolations(line));

    [Theory]
    [InlineData("form.requestSubmit(button);")]
    [InlineData("form.requestSubmit(form.querySelector('button[type=\"submit\"]'));")]
    [InlineData("button.disabled = !variant.purchasable;")]
    [InlineData("// form.submit() is never used here")]
    [InlineData("/* serviceWorker and caches.open are named in a comment */")]
    [InlineData("form.setAttribute('aria-busy', 'true');")]
    [InlineData("const submitted = event.submitter;")]
    public void ANearMissOfAScriptRule_IsLeftAlone_KK2_KK4_KK10(string line) => Assert.Empty(ScriptViolations(line));

    // ---- The page modules (09 16.1 item 2) ----

    private static string KebabOf(string segment) => WordBoundary().Replace(segment, "-").ToLowerInvariant();

    [Fact]
    public void EveryPageModule_StandsWhereItsPagesPathSays_AndAViewLoadsIt_AndEveryLoadedScriptExists_KK2()
    {
        string pages = Path.Combine(WebRoot, "Pages");
        string wwwroot = Path.Combine(WebRoot, "wwwroot");

        // Where a page module may stand: the kebab-case of a page's own path under Pages (Pages/Panel/ProductForm.cshtml -> js/pages/panel/product-form.js).
        HashSet<string> pagePaths = [.. Views().Select(view => string.Join('/', Relative(pages, view)[..^".cshtml".Length].Split('/').Select(KebabOf)))];
        string[] modules = [.. Scripts().Select(script => Relative(Path.Combine(wwwroot, "js"), script)).Where(file => file.StartsWith("pages/", StringComparison.Ordinal))];
        Assert.NotEmpty(modules);
        foreach (string module in modules)
        {
            string pagePath = module["pages/".Length..^".js".Length];
            Assert.True(pagePaths.Contains(pagePath), $"The module js/{module} follows no page's path (09 16.1 item 2): there is no Pages/{pagePath} page.");
        }

        // The other scripts are the two named ones.
        string[] others = [.. Scripts().Select(script => Relative(Path.Combine(wwwroot, "js"), script)).Where(file => !file.StartsWith("pages/", StringComparison.Ordinal)).Order(StringComparer.Ordinal)];
        Assert.Equal([.. RootScripts.Order(StringComparer.Ordinal)], others);

        // Every module is loaded by a view, and a view loads only files that exist.
        List<string> loaded = [.. Views().SelectMany(view => ScriptReference().Matches(RazorComment().Replace(File.ReadAllText(view), string.Empty)).Select(m => m.Groups["file"].Value))];
        foreach (string file in loaded)
        {
            Assert.True(File.Exists(Path.Combine(wwwroot, file)), $"A view loads {file}, which does not exist.");
        }

        foreach (string module in modules)
        {
            Assert.Contains($"js/{module}", loaded);
        }
    }

    // ---- No installable application (10 KD-22; 02 3.34.1) ----

    [Fact]
    public void TheSite_HasNoManifestNoServiceWorkerFileAndNoViewThatLinksOne_KK10()
    {
        string wwwroot = Path.Combine(WebRoot, "wwwroot");
        string[] names = [.. Directory.EnumerateFiles(wwwroot, "*", SearchOption.AllDirectories).Select(Path.GetFileName).OfType<string>()];
        Assert.DoesNotContain(names, name => name.Contains("manifest", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(names, name => name.StartsWith("sw.", StringComparison.OrdinalIgnoreCase) || name.Contains("service-worker", StringComparison.OrdinalIgnoreCase) || name.Contains("serviceworker", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(names, name => name.Equals("offline.html", StringComparison.OrdinalIgnoreCase));

        foreach (string view in Views())
        {
            Assert.Empty(ViewApplicationParts().Matches(RazorComment().Replace(File.ReadAllText(view), string.Empty)).Select(m => m.Value));
        }
    }

    [Theory]
    [InlineData("<link rel=\"manifest\" href=\"/manifest.webmanifest\">")]
    [InlineData("<meta name=\"apple-mobile-web-app-capable\" content=\"yes\">")]
    [InlineData("<link rel=\"apple-touch-startup-image\" href=\"/x.png\">")]
    public void AViewThatCarriesAnInstallableApplicationPart_IsFound_KK10(string markup) => Assert.NotEmpty(ViewApplicationParts().Matches(markup));

    [Theory]
    [InlineData("<link rel=\"stylesheet\" href=\"/statik/css/base.css\">")]
    [InlineData("<link rel=\"icon\" href=\"/favicon.ico\">")]
    [InlineData("<link rel=\"apple-touch-icon\" href=\"/icon.png\">")]
    [InlineData("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">")]
    public void ANearMissOfTheApplicationRule_IsLeftAlone_KK10(string markup) => Assert.Empty(ViewApplicationParts().Matches(markup));
}
