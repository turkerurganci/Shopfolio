using System.Text.RegularExpressions;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The front-end rules of the frame on the source (T540 KK1-KK4; 09 16.1 items 1-9; 04 2.1): the scripts load nothing and write no markup from text, the views hold no event attribute, inline
/// script, style attribute or embedded content, the style sheets import nothing and write no colour outside the base file, every width condition is one of the two limits the base file defines,
/// every class is a BEM name, and the brand colour is a filled background only. Each rule goes red on a deliberate violation (a text below) and stays quiet on the near-miss that is allowed.
/// </summary>
public sealed partial class FrameSourceScanTests
{
    private const string Web = "src/Shopfolio.Web";

    private static readonly string[] StyleFiles = ["base.css", "components.css", "storefront.css", "admin.css"];

    private static readonly string[] FrameViews =
    [
        "_StorefrontLayout", "_StorefrontHeader", "_StorefrontFooter", "_MenuTree", "_AdminStrip", "_DraftBand", "_PanelLayout", "_PanelBareLayout",

        // The list components (T545) are shared views of the two frames: the same view rules apply (no event attribute, inline script, style attribute or embedded content; BEM class names).
        "_ProductCard", "_ContentCard", "_ProductGrid", "_ContentGrid", "_Pager", "_EmptyState", "_ListSearch", "_PanelTable",
    ];

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BlockComment();

    [GeneratedRegex(@"(?m)^\s*//.*$|(?<=\s)//\s.*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LineComment();

    [GeneratedRegex(@"@\*.*?\*@", RegexOptions.Singleline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RazorComment();

    [GeneratedRegex(@"(?m)^\s*import\b|\bimport\s*\(|\bimport\s+[\w{*]", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ScriptImport();

    [GeneratedRegex(@"\b(?:innerHTML|outerHTML|insertAdjacentHTML|insertAdjacentText|document\s*\.\s*write(?:ln)?|eval|setAttribute\s*\(\s*['""]on)\b|new\s+Function\b|\bsrcdoc\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ScriptMarkupFromText();

    [GeneratedRegex(@"https?://|\bfetch\s*\(|XMLHttpRequest|\bWebSocket\b|sendBeacon|\bimportScripts\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ScriptNetwork();

    [GeneratedRegex(@"\son[a-z]+\s*=", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex EventAttribute();

    [GeneratedRegex(@"<script\b(?![^>]*\ssrc\s*=)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex InlineScript();

    [GeneratedRegex(@"\sstyle\s*=", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex StyleAttribute();

    [GeneratedRegex(@"<(?:iframe|embed|object|frame|applet)\b|https?://", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex EmbeddedContent();

    [GeneratedRegex(@"@import\b|url\s*\(\s*['""]?(?:https?:)?//", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex StyleImport();

    [GeneratedRegex(@"#[0-9a-fA-F]{3,8}\b|\b(?:rgba?|hsla?|hwb|lab|lch|oklab|oklch|color)\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ColourLiteral();

    [GeneratedRegex(@"@media\b([^{]*)\{", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex MediaRule();

    [GeneratedRegex(@"--bp-(?:medium|wide)\s*:\s*(\d+)px", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BreakpointDefinition();

    [GeneratedRegex(@"^\(min-width:\s*(\d+)px\)$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex MinWidthCondition();

    [GeneratedRegex(@"\{[^{}]*\}", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex DeclarationBlock();

    [GeneratedRegex(@"\.([A-Za-z_][\w-]*)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ClassSelector();

    [GeneratedRegex(@"\bclass\s*=\s*""([^""]*)""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ClassAttribute();

    [GeneratedRegex(@"^(?:u-[a-z0-9]+(?:-[a-z0-9]+)*|[a-z][a-z0-9]*(?:-[a-z0-9]+)*(?:__[a-z0-9]+(?:-[a-z0-9]+)*)?(?:--[a-z0-9]+(?:-[a-z0-9]+)*)?)$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BemName();

    [GeneratedRegex(@"([\w-]+)\s*:\s*[^;{}]*var\(\s*--brand-color\s*\)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BrandColourUse();

    [GeneratedRegex(@"([\w-]+)\s*:\s*[^;{}]*var\(\s*--brand-text-color\s*\)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BrandTextColourUse();

    private static string ReadWeb(string relative) => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), Web, relative));

    private static string StripScriptComments(string text) => LineComment().Replace(BlockComment().Replace(text, string.Empty), string.Empty);

    private static string StripStyleComments(string text) => BlockComment().Replace(text, string.Empty);

    private static string FrameView(string name) => RazorComment().Replace(ReadWeb($"Pages/Shared/{name}.cshtml"), string.Empty);

    // ---- The scripts (09 16.1 items 3, 9) ----

    private static List<string> ScriptViolations(string script)
    {
        string text = StripScriptComments(script);
        return [.. ScriptImport().Matches(text).Select(m => $"import: {m.Value.Trim()}"), .. ScriptMarkupFromText().Matches(text).Select(m => $"markup from text: {m.Value}"), .. ScriptNetwork().Matches(text).Select(m => $"network: {m.Value}")];
    }

    [Fact]
    public void TheScripts_ImportNothing_WriteNoMarkupFromText_AndLoadNothingFromTheNetwork_KK4()
    {
        string[] scripts = [.. Directory.EnumerateFiles(Path.Combine(RepositoryRoot.Find(), Web, "wwwroot", "js"), "*.js")];
        Assert.NotEmpty(scripts);
        foreach (string script in scripts)
        {
            Assert.Empty(ScriptViolations(File.ReadAllText(script)));
        }
    }

    [Theory]
    [InlineData("import { a } from './other.js';")]
    [InlineData("const m = await import('./other.js');")]
    [InlineData("element.innerHTML = text;")]
    [InlineData("element.outerHTML = text;")]
    [InlineData("element.insertAdjacentHTML('beforeend', text);")]
    [InlineData("document.write(text);")]
    [InlineData("eval(text);")]
    [InlineData("const f = new Function(text);")]
    [InlineData("fetch('/x');")]
    [InlineData("const s = 'https://tracker.example/p.js';")]
    [InlineData("navigator.sendBeacon('/m');")]
    public void AScriptViolation_IsFound_KK4(string line) => Assert.NotEmpty(ScriptViolations(line));

    [Theory]
    [InlineData("// we never use innerHTML or import here")]
    [InlineData("/* eval and fetch(x) are named in a comment */")]
    [InlineData("element.textContent = text; // not innerHTML")]
    [InlineData("button.dataset.importance = 'high';")]
    [InlineData("const reportCount = 1;")]
    public void ANearMissOfAScriptRule_IsLeftAlone_KK4(string line) => Assert.Empty(ScriptViolations(line));

    // ---- The views (09 16.1 items 4, 6; 04 2.1.4) ----

    private static List<string> ViewViolations(string view) =>
    [
        .. EventAttribute().Matches(view).Select(m => $"event attribute: {m.Value.Trim()}"),
        .. InlineScript().Matches(view).Select(m => $"inline script: {m.Value}"),
        .. StyleAttribute().Matches(view).Select(m => $"style attribute: {m.Value.Trim()}"),
        .. EmbeddedContent().Matches(view).Select(m => $"embedded or external: {m.Value}"),
    ];

    [Fact]
    public void TheCutTextLine_IsUsedByTheTwoNamedExceptionsOnly_AndNoStyleSheetCutsTextOtherwise_KK2()
    {
        // 04 8.3.2: text is never cut except the long brand name of the narrow header and the administrator's name in the panel's top row.
        string[] users = [.. FrameViews.Where(name => FrameView(name).Contains("u-single-line", StringComparison.Ordinal)).Order(StringComparer.Ordinal)];
        Assert.Equal(["_PanelLayout", "_StorefrontHeader"], users);

        foreach (string file in StyleFiles)
        {
            foreach (string rule in StripStyleComments(ReadWeb($"wwwroot/css/{file}")).Split('}'))
            {
                if (rule.Contains("text-overflow", StringComparison.Ordinal))
                {
                    Assert.Contains("u-single-line", rule, StringComparison.Ordinal);
                }
            }
        }
    }

    [Fact]
    public void TheFrameViews_HaveNoEventAttributeNoInlineScriptNoStyleAttributeAndNoEmbeddedContent_KK4()
    {
        foreach (string name in FrameViews)
        {
            Assert.Empty(ViewViolations(FrameView(name)));
        }
    }

    [Theory]
    [InlineData("<button onclick=\"go()\">x</button>")]
    [InlineData("<a href=\"/x\" onmouseover=\"go()\">x</a>")]
    [InlineData("<script>go();</script>")]
    [InlineData("<p style=\"color:red\">x</p>")]
    [InlineData("<iframe src=\"/x\"></iframe>")]
    [InlineData("<script src=\"https://cdn.example/x.js\"></script>")]
    [InlineData("<embed src=\"/x\">")]
    public void AViewViolation_IsFound_KK4(string markup) => Assert.NotEmpty(ViewViolations(markup));

    [Theory]
    [InlineData("<script type=\"module\" src=\"/statik/js/common.js\"></script>")]
    [InlineData("<button data-js=\"menu-toggle\" aria-controls=\"x\">Menü</button>")]
    [InlineData("<style nonce=\"abc\">:root{--x:1}</style>")]
    [InlineData("<a href=\"/iletisim\">İletişim</a>")]
    [InlineData("<input autocomplete=\"off\" name=\"q\">")]
    public void ANearMissOfAViewRule_IsLeftAlone_KK4(string markup) => Assert.Empty(ViewViolations(markup));

    // ---- The style sheets (09 16.1 items 1, 7; 04 2.1.1, 2.1.3) ----

    private static List<string> StyleViolations(string css, bool colourLiteralsAllowed)
    {
        string text = StripStyleComments(css);
        List<string> found = [.. StyleImport().Matches(text).Select(m => $"import or external: {m.Value}")];
        if (!colourLiteralsAllowed)
        {
            found.AddRange(ColourLiteral().Matches(text).Select(m => $"colour literal: {m.Value}"));
        }

        return found;
    }

    [Fact]
    public void TheStyleSheets_ImportNothing_AndOnlyTheBaseFileWritesColourValues_KK3()
    {
        foreach (string file in StyleFiles)
        {
            Assert.Empty(StyleViolations(ReadWeb($"wwwroot/css/{file}"), colourLiteralsAllowed: file == "base.css"));
        }
    }

    [Theory]
    [InlineData("@import url('other.css');")]
    [InlineData("a { background: url(https://x.example/a.png); }")]
    [InlineData("a { color: #112233; }")]
    [InlineData("a { color: #fff; }")]
    [InlineData("a { color: rgb(1, 2, 3); }")]
    [InlineData("a { color: rgba(1, 2, 3, 0.5); }")]
    [InlineData("a { color: hsl(10 20% 30%); }")]
    public void AStyleViolation_IsFound_KK3(string css) => Assert.NotEmpty(StyleViolations(css, colourLiteralsAllowed: false));

    [Theory]
    [InlineData("#content { color: var(--color-text); }")]
    [InlineData("/* #112233 stays in a comment */ a { color: var(--color-link); }")]
    [InlineData("a { background: var(--brand-color); }")]
    [InlineData("a { margin: 0.25rem; }")]
    public void ANearMissOfAStyleRule_IsLeftAlone_KK3(string css) => Assert.Empty(StyleViolations(css, colourLiteralsAllowed: false));

    private static HashSet<int> DefinedLimits(string baseCss) =>
        [.. BreakpointDefinition().Matches(StripStyleComments(baseCss)).Select(m => int.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture))];

    private const string ReducedMotionCondition = "(prefers-reduced-motion: reduce)";

    private static string MediaRuleOf(string condition) => "@media " + condition + " { a { margin: 0; } }";

    private static List<string> WidthConditionViolations(string css, HashSet<int> limits)
    {
        List<string> found = [];
        foreach (Match media in MediaRule().Matches(StripStyleComments(css)))
        {
            string condition = media.Groups[1].Value.Trim();
            if (condition == ReducedMotionCondition)
            {
                // The one condition that is not a width: the user's own choice to see no motion (04 2.7.2); it carries no breakpoint.
                continue;
            }

            Match min = MinWidthCondition().Match(condition);
            if (!min.Success || !limits.Contains(int.Parse(min.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture)))
            {
                found.Add($"@media {condition}");
            }
        }

        return found;
    }

    [Fact]
    public void TheBaseFile_DefinesTheTwoLimitsOnce_AndEveryWidthConditionOfEveryFileIsOneOfThem_KK1()
    {
        HashSet<int> limits = DefinedLimits(ReadWeb("wwwroot/css/base.css"));
        Assert.Equal([768, 1024], limits.Order());

        int conditions = 0;
        foreach (string file in StyleFiles)
        {
            string css = ReadWeb($"wwwroot/css/{file}");
            Assert.Empty(WidthConditionViolations(css, limits));
            conditions += MediaRule().Count(StripStyleComments(css));
            if (file != "base.css")
            {
                Assert.Empty(BreakpointDefinition().Matches(StripStyleComments(css)));
            }
        }

        Assert.True(conditions > 0);
    }

    [Theory]
    [InlineData("(min-width: 800px)")]
    [InlineData("(max-width: 768px)")]
    [InlineData("(min-width: 768px) and (max-width: 1023px)")]
    [InlineData("screen and (min-width: 1024px)")]
    [InlineData("(prefers-reduced-motion: reduce) and (max-width: 500px)")]
    [InlineData("(prefers-reduced-motion: no-preference)")]
    public void AWidthConditionOutsideTheTwoLimits_IsFound_KK1(string condition)
    {
        Assert.NotEmpty(WidthConditionViolations(MediaRuleOf(condition), [768, 1024]));
    }

    [Theory]
    [InlineData("(min-width: 768px)")]
    [InlineData("(min-width: 1024px)")]
    [InlineData("(prefers-reduced-motion: reduce)")]
    public void AWidthConditionAtALimit_IsLeftAlone_KK1(string condition)
    {
        Assert.Empty(WidthConditionViolations(MediaRuleOf(condition), [768, 1024]));
    }

    // ---- The content column keeps a limited width in both frames (04 2.1.1; 05 3.8.2) ----

    private static readonly (string File, string Selector)[] ContentColumns = [("base.css", ".container"), ("admin.css", ".panel-content")];

    private static List<string> UnlimitedContentViolations(string css, string selector)
    {
        // A rule of the selector (alone, inside a media block or not) that sets max-width to the one shared variable and centres the column.
        List<string> rules = [.. StripStyleComments(css).Split('}').Where(rule => rule.Contains('{', StringComparison.Ordinal) && Regex.IsMatch(rule[..rule.LastIndexOf('{')], @"(?:^|[\s,{])" + Regex.Escape(selector) + @"\s*$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)))];
        if (!rules.Any(rule => Regex.IsMatch(rule, @"max-width:\s*var\(--content-max-width\)", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5))))
        {
            return [$"{selector} has no max-width of the shared content width"];
        }

        return rules.Any(rule => Regex.IsMatch(rule, @"margin-inline:\s*auto", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5))) ? [] : [$"{selector} is not centred"];
    }

    [Fact]
    public void TheVariableOfTheContentWidth_IsDefinedOnce_AndTheStorefrontAndThePanelColumnsBothLimitAndCentreTheirContent_KK1()
    {
        Assert.Single(Regex.Matches(StripStyleComments(ReadWeb("wwwroot/css/base.css")), @"--content-max-width\s*:", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));
        foreach ((string file, string selector) in ContentColumns)
        {
            Assert.Empty(UnlimitedContentViolations(ReadWeb($"wwwroot/css/{file}"), selector));
        }
    }

    [Theory]
    [InlineData(".panel-content { flex: 1 1 auto; padding: 1rem; }")]
    [InlineData(".panel-content { padding: 2rem; }")]
    [InlineData(".panel-content { max-width: 80rem; margin-inline: auto; }")]
    [InlineData(".panel-content { max-width: var(--content-max-width); }")]
    public void AColumnWithoutALimitedAndCentredWidth_IsFound_KK1(string rule)
    {
        Assert.NotEmpty(UnlimitedContentViolations(rule, ".panel-content"));
        Assert.NotEmpty(UnlimitedContentViolations("@media (min-width: 1024px) { " + rule + " }", ".panel-content"));
    }

    [Theory]
    [InlineData(".panel-content { max-width: var(--content-max-width); margin-inline: auto; }")]
    [InlineData(".panel-content__title { margin: 0; } .panel-content { max-width: var(--content-max-width); margin-inline: auto; }")]
    public void AColumnWithALimitedAndCentredWidth_IsLeftAlone_KK1(string rule)
    {
        Assert.Empty(UnlimitedContentViolations(rule, ".panel-content"));
        Assert.Empty(UnlimitedContentViolations("@media (min-width: 1024px) { " + rule + " }", ".panel-content"));
    }

    // ---- The menu's category tree is reachable by keyboard (04 2.1.4 item 3; 2.1.2) ----

    private static List<string> TreeTriggerViolations(string view, string script)
    {
        List<string> found = [];
        if (view.Contains("site-menu__group-title", StringComparison.Ordinal))
        {
            // The first button after the title: the title's own trigger, not the menu toggle that comes earlier in the header.
            Match button = Regex.Match(view, @"site-menu__group-title[\s\S]*?(?<tag><button\b[^>]*\btype=""button""[^>]*>)", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));
            string tag = button.Groups["tag"].Value;
            if (!button.Success || !tag.Contains("aria-expanded=", StringComparison.Ordinal) || !tag.Contains("aria-controls=", StringComparison.Ordinal))
            {
                found.Add("the group title has no <button type=\"button\"> carrying aria-expanded and aria-controls");
            }
        }

        if (!script.Contains("\"Escape\"", StringComparison.Ordinal) || !script.Contains("addEventListener(\"click\"", StringComparison.Ordinal))
        {
            found.Add("the script binds no click and no Escape");
        }

        return found;
    }

    [Fact]
    public void TheMenuGroupTitle_HasAFocusableTrigger_ThatTheScriptOpensByClickAndClosesOnEscape_KK4()
    {
        Assert.Empty(TreeTriggerViolations(FrameView("_StorefrontHeader"), ReadWeb("wwwroot/js/common.js")));

        // Nothing opens the tree by hover or focus alone any more: that was the only way and keyboard users could not take it.
        string css = StripStyleComments(ReadWeb("wwwroot/css/storefront.css"));
        Assert.DoesNotContain(".site-menu__item--group:hover", css, StringComparison.Ordinal);
        Assert.DoesNotContain(".site-menu__item--group:focus-within", css, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("<li><span class=\"site-menu__group-title\">Ürünler</span><ul></ul></li>")]
    [InlineData("<li><span class=\"site-menu__group-title\">Ürünler</span><a href=\"#\" aria-expanded=\"false\" aria-controls=\"x\">x</a></li>")]
    [InlineData("<li><span class=\"site-menu__group-title\">Ürünler</span><button type=\"button\">x</button></li>")]
    public void AGroupTitleWithoutAFocusableTrigger_IsFound_KK4(string view) => Assert.NotEmpty(TreeTriggerViolations(view, "x.addEventListener(\"click\", f); if (e.key === \"Escape\") {}"));

    [Fact]
    public void ATriggerThatTheScriptNeverBinds_IsFound_KK4() =>
        Assert.NotEmpty(TreeTriggerViolations("<span class=\"site-menu__group-title\">x</span><button type=\"button\" aria-expanded=\"false\" aria-controls=\"t\">x</button>", "const a = 1;"));

    [Fact]
    public void AGroupTitleWithItsTrigger_IsLeftAlone_KK4() =>
        Assert.Empty(TreeTriggerViolations("<span class=\"site-menu__group-title\">x</span><button type=\"button\" class=\"b\" aria-controls=\"t\" aria-expanded=\"false\" hidden>x</button>", "b.addEventListener(\"click\", f); if (e.key === \"Escape\") {}"));

    // ---- The class names (09 16.1 item 5) ----

    private static List<string> ClassNameViolations(IEnumerable<string> names) => [.. names.Where(name => !BemName().IsMatch(name))];

    private static IEnumerable<string> StyleClasses(string css) =>
        ClassSelector().Matches(DeclarationBlock().Replace(DeclarationBlock().Replace(DeclarationBlock().Replace(StripStyleComments(css), string.Empty), string.Empty), string.Empty)).Select(m => m.Groups[1].Value);

    private static IEnumerable<string> ViewClasses(string view) =>
        ClassAttribute().Matches(view).Where(m => !m.Groups[1].Value.Contains('@', StringComparison.Ordinal)).SelectMany(m => m.Groups[1].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries));

    [Fact]
    public void EveryClassOfTheFrame_IsABemName_InTheStyleSheetsAndInTheViews_KK1()
    {
        foreach (string file in StyleFiles)
        {
            string[] classes = [.. StyleClasses(ReadWeb($"wwwroot/css/{file}"))];
            Assert.Equal<string>([], ClassNameViolations(classes));
        }

        foreach (string name in FrameViews)
        {
            string[] classes = [.. ViewClasses(FrameView(name)).Distinct(StringComparer.Ordinal)];
            Assert.Equal<string>([], ClassNameViolations(classes));
        }
    }

    [Theory]
    [InlineData("camelCase")]
    [InlineData("Block")]
    [InlineData("block_element")]
    [InlineData("block__Element")]
    [InlineData("block__element__deeper")]
    [InlineData("block--modifier--again")]
    [InlineData("block-")]
    public void ANameThatIsNotBem_IsFound_KK1(string name) => Assert.Single(ClassNameViolations([name]));

    [Theory]
    [InlineData("block")]
    [InlineData("block-name")]
    [InlineData("block__element")]
    [InlineData("block--modifier")]
    [InlineData("block-name__element-name--modifier-name")]
    [InlineData("u-single-line")]
    public void ABemName_IsLeftAlone_KK1(string name) => Assert.Empty(ClassNameViolations([name]));

    // ---- The brand colour is a filled background (04 2.1.3) ----

    private static readonly string[] FillProperties = ["background", "background-color", "--button-primary-bg"];

    private static readonly string[] FillTextProperties = ["color", "--button-primary-text"];

    private static List<string> BrandColourViolations(string css)
    {
        string text = StripStyleComments(css);
        return
        [
            .. BrandColourUse().Matches(text).Where(m => !FillProperties.Contains(m.Groups[1].Value)).Select(m => $"brand colour as {m.Groups[1].Value}"),
            .. BrandTextColourUse().Matches(text).Where(m => !FillTextProperties.Contains(m.Groups[1].Value)).Select(m => $"brand text colour as {m.Groups[1].Value}"),
        ];
    }

    [Fact]
    public void TheBrandColour_IsAFilledBackgroundOnTheStorefrontOnly_AndTheComponentsAndThePanelNeverUseIt_KK3()
    {
        Assert.Empty(BrandColourViolations(ReadWeb("wwwroot/css/storefront.css")));
        Assert.Contains("var(--brand-color)", ReadWeb("wwwroot/css/storefront.css"), StringComparison.Ordinal);
        foreach (string file in new[] { "components.css", "admin.css" })
        {
            string text = StripStyleComments(ReadWeb($"wwwroot/css/{file}"));
            Assert.DoesNotContain("--brand-color", text, StringComparison.Ordinal);
            Assert.DoesNotContain("--brand-text-color", text, StringComparison.Ordinal);
        }
    }

    [Theory]
    [InlineData("a { color: var(--brand-color); }")]
    [InlineData("a { border: 1px solid var(--brand-color); }")]
    [InlineData("a { border-color: var(--brand-color); }")]
    [InlineData("a { outline: 2px solid var(--brand-color); }")]
    [InlineData("a { fill: var(--brand-color); }")]
    [InlineData("a { text-decoration-color: var(--brand-color); }")]
    [InlineData("a { background: var(--brand-color); border-color: var(--brand-text-color); }")]
    public void TheBrandColourOutsideAFilledBackground_IsFound_KK3(string css) => Assert.NotEmpty(BrandColourViolations(css));

    [Theory]
    [InlineData("a { background: var(--brand-color); color: var(--brand-text-color); }")]
    [InlineData(".storefront { --button-primary-bg: var(--brand-color); --button-primary-text: var(--brand-text-color); }")]
    [InlineData("a { background-color: var(--brand-color); }")]
    public void TheBrandColourAsAFilledBackground_IsLeftAlone_KK3(string css) => Assert.Empty(BrandColourViolations(css));

    // ---- A shared view that writes a tag helper declares the helpers it needs (T541; the framed error page drew a raw <partial> tag) ----

    [GeneratedRegex(@"<partial\b|<(?:form|input|a|select|textarea|label|environment|cache)\b[^>]*\basp-[a-z-]+", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex TagHelperUse();

    [GeneratedRegex(@"@addTagHelper\s+\*\s*,\s*Microsoft\.AspNetCore\.Mvc\.TagHelpers\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex TagHelperDeclaration();

    private static bool UsesTagHelpersWithoutDeclaring(string view)
    {
        string text = RazorComment().Replace(view, string.Empty);
        return TagHelperUse().IsMatch(text) && !TagHelperDeclaration().IsMatch(view);
    }

    [Fact]
    public void EverySharedView_ThatWritesATagHelper_DeclaresTheHelpers_KK1()
    {
        string directory = Path.Combine(RepositoryRoot.Find(), Web, "Pages", "Shared");
        string[] views = Directory.GetFiles(directory, "*.cshtml");

        Assert.NotEmpty(views);
        Assert.Empty(views.Where(path => UsesTagHelpersWithoutDeclaring(File.ReadAllText(path))).Select(Path.GetFileName));
    }

    [Theory]
    [InlineData("<partial name=\"_Message\" model=\"x\" />")]
    [InlineData("<form method=\"post\" asp-antiforgery=\"true\"></form>")]
    [InlineData("<a asp-page=\"/Index\">x</a>")]
    public void ASharedViewWithATagHelperAndNoDeclaration_IsFound_KK1(string view) => Assert.True(UsesTagHelpersWithoutDeclaring(view));

    [Theory]
    [InlineData("@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers\n<partial name=\"_Message\" model=\"x\" />")]
    [InlineData("<p>no helper here</p>")]
    [InlineData("@* <partial name=\"_Message\" /> stays in a comment *@\n<p>text</p>")]
    public void ASharedViewWithADeclarationOrNoTagHelper_IsLeftAlone_KK1(string view) => Assert.False(UsesTagHelpersWithoutDeclaring(view));
}
