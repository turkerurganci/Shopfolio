using System.Text.RegularExpressions;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The domestic production logo's source rules (T501 KK1, KK4, KK5; 08 4.3; 09 11.1 item 7; 04 5.4.18; K-610): the logo is an asset that comes with the code, so no setting, panel field or switch
/// names it; the code reads it only through the one contract class and the one component draws it; and no style rule in any width class hides, empties or clips the component. Every rule goes red
/// on a deliberate violation and stays quiet on the near miss that is allowed.
/// </summary>
public sealed partial class DomesticProductionLogoSourceScanTests
{
    private const string Web = "src/Shopfolio.Web";

    /// <summary>The only files that name the logo: the contract class, its endpoint, the one component, the wiring and the project file that embeds it.</summary>
    private static readonly string[] Owners =
    [
        "Modules/Catalog/Contracts/Pricing/DomesticProductionLogo.cs",
        "Endpoints/DomesticProductionLogoEndpoint.cs",
        "Pages/Shared/_OriginLabel.cshtml",
        "Program.cs",
        "Shopfolio.Web.csproj",
        "wwwroot/css/components.css",
    ];

    private static readonly string[] SourceExtensions = [".cs", ".cshtml", ".css", ".js", ".json", ".csproj", ".txt"];

    [GeneratedRegex(@"DomesticProduction|domestic-production|yerli-uretim|YerliUretim|MadeInTurkey|LocalProduction|origin-label", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LogoName();

    [GeneratedRegex(@"/\*.*?\*/|@\*.*?\*@", RegexOptions.Singleline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BlockComment();

    [GeneratedRegex(@"(?m)^\s*///?.*$|(?<=\s)//\s.*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LineComment();

    [GeneratedRegex(@"([^{}]+)\{([^{}]*)\}", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex StyleRule();

    [GeneratedRegex(@"(?:^|[\s>+~])(?:img|picture|\*)(?![\w-])", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex WholeImageSelector();

    [GeneratedRegex(
        @"display\s*:\s*none|visibility\s*:\s*(?:hidden|collapse)|content-visibility\s*:\s*hidden|opacity\s*:\s*0(?![.\d])|(?:^|[;\s])(?:width|height|max-width|max-height|font-size)\s*:\s*0(?:px|rem|em|%)?\s*(?:;|$|!)|\bclip(?:-path)?\s*:|transform\s*:[^;]*scale\(\s*0\s*\)|\bhidden\b",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
        matchTimeoutMilliseconds: 5000)]
    private static partial Regex HidingDeclaration();

    private static string Strip(string text) => LineComment().Replace(BlockComment().Replace(text, string.Empty), string.Empty);

    /// <summary>Names of the logo outside the files that own it (a setting, a panel field, a second view, a second reader).</summary>
    private static List<string> OwnerViolations(string relativePath, string text) =>
        Owners.Contains(relativePath, StringComparer.Ordinal) ? [] : [.. LogoName().Matches(Strip(text)).Select(m => $"{relativePath}: {m.Value}")];

    /// <summary>Style rules that hide, empty or clip the logo or its component, in any width class: the selector names the component or the whole image class, the declaration hides.</summary>
    private static List<string> HidingViolations(string css)
    {
        List<string> found = [];
        foreach (Match rule in StyleRule().Matches(Strip(css)))
        {
            string selector = rule.Groups[1].Value;
            bool targetsLogo = selector.Split(',').Any(s => s.Contains("origin-label", StringComparison.Ordinal) || WholeImageSelector().IsMatch(s.Trim()));
            if (targetsLogo && HidingDeclaration().Match(rule.Groups[2].Value) is { Success: true } hidden)
            {
                found.Add($"{selector.Trim()} {{ {hidden.Value.Trim()} }}");
            }
        }

        return found;
    }

    private static IEnumerable<(string Path, string Text)> WebSources()
    {
        string root = Path.Combine(RepositoryRoot.Find(), Web);
        foreach (string file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (relative.StartsWith("bin/", StringComparison.Ordinal) || relative.StartsWith("obj/", StringComparison.Ordinal) || !SourceExtensions.Contains(Path.GetExtension(file), StringComparer.Ordinal))
            {
                continue;
            }

            yield return (relative, File.ReadAllText(file));
        }
    }

    [Fact]
    public void OnlyTheContractTheEndpointTheComponentAndTheWiring_NameTheLogo_NoSettingNoPanelFieldNoSecondReader_KK1_KK5()
    {
        List<string> violations = [.. WebSources().SelectMany(source => OwnerViolations(source.Path, source.Text))];

        Assert.Equal<string>([], violations);
    }

    [Theory]
    [InlineData("Modules/Settings/CompanySettings.cs", "public bool DomesticProductionLogoEnabled { get; set; }")]
    [InlineData("appsettings.json", "{ \"MadeInTurkey\": { \"Logo\": \"x.png\" } }")]
    [InlineData("Pages/Storefront/Product.cshtml", "<img src=\"/statik/yerli-uretim-logosu.0123456789.jpg\" alt=\"x\">")]
    [InlineData("Pages/Panel/ProductForm.cshtml", "<input asp-for=\"Input.ShowLocalProductionLogo\">")]
    [InlineData("Modules/Catalog/ProductPageReader.cs", "var bytes = DomesticProductionLogo.Content;")]
    [InlineData("wwwroot/css/storefront.css", ".origin-label__logo { margin: 0 }")]
    public void ANameOfTheLogoOutsideItsOwners_IsFound_KK1_KK5(string path, string text) => Assert.NotEmpty(OwnerViolations(path, text));

    [Theory]
    [InlineData("Pages/Storefront/Product.cshtml", "<partial name=\"_OriginLabel\" model=\"label\" />")]
    [InlineData("Modules/Catalog/ProductPageReader.cs", "// the logo comes from DomesticProductionLogo (T501)")]
    [InlineData("Modules/Catalog/ProductPageReader.cs", "/// <see cref=\"DomesticProductionLogo\"/> decides it")]
    [InlineData("Pages/Storefront/Product.cshtml", "@* the origin-label component draws it *@")]
    [InlineData("Modules/CorporateContent/Branding.cs", "public Guid? LogoFileId { get; set; }")]
    [InlineData("Endpoints/DomesticProductionLogoEndpoint.cs", "DomesticProductionLogo.Address")]
    public void ANearMissOfTheOwnerRule_IsLeftAlone_KK1_KK5(string path, string text) => Assert.Empty(OwnerViolations(path, text));

    [Fact]
    public void NoStyleRuleOfAnyWidthClass_HidesEmptiesOrClipsTheLogoComponent_KK4()
    {
        string root = Path.Combine(RepositoryRoot.Find(), Web, "wwwroot", "css");
        string[] files = [.. Directory.EnumerateFiles(root, "*.css")];
        Assert.NotEmpty(files);

        foreach (string file in files)
        {
            Assert.Equal<string>([], HidingViolations(File.ReadAllText(file)));
        }

        // The component's own rule is in the shared components file, so the check above reads it.
        Assert.Contains(".origin-label__logo", File.ReadAllText(Path.Combine(root, "components.css")), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(".origin-label__logo { display: none; }")]
    [InlineData(".origin-label { display:none }")]
    [InlineData(".a, .origin-label__logo { visibility: hidden }")]
    [InlineData("@media (min-width: 768px)" + "{ .origin-label__logo { display: none; } }")]
    [InlineData(".origin-label__logo[hidden] { display: none }")]
    [InlineData(".origin-label__logo { opacity: 0 }")]
    [InlineData(".origin-label__logo { width: 0; }")]
    [InlineData(".origin-label__logo { height: 0px }")]
    [InlineData(".origin-label__logo { clip-path: inset(100%); }")]
    [InlineData(".origin-label__logo { display: none !important; }")]
    [InlineData("img { display: none }")]
    [InlineData(".price-block img { display: none }")]
    [InlineData("* { visibility: hidden }")]
    [InlineData(".origin-label__logo { transform: scale(0) }")]
    public void AStyleRuleThatHidesTheLogo_IsFound_KK4(string css) => Assert.NotEmpty(HidingViolations(css));

    [Theory]
    [InlineData(".origin-label { display: flex; flex-wrap: wrap; }")]
    [InlineData(".origin-label__logo { display: block; width: var(--origin-logo-width); height: auto; }")]
    [InlineData("@media (min-width: 768px)" + "{ .origin-label__logo { width: 10rem; } }")]
    [InlineData(".menu-toggle[hidden] { display: none; }")]
    [InlineData(".share-button { display: none }")]
    [InlineData("img { max-width: 100%; height: auto; }")]
    [InlineData(".origin-label__logo { opacity: 0.5 }")]
    [InlineData(".origin-label__logo { width: 10rem; }")]
    [InlineData("/* .origin-label__logo { display: none } */")]
    public void ANearMissOfTheHidingRule_IsLeftAlone_KK4(string css) => Assert.Empty(HidingViolations(css));

    [Fact]
    public void TheComponent_HasNoHiddenAttributeNoStyleAttributeAndNoEventAttribute_AndTheLogoIsAnImageWithAlternativeText_KK4()
    {
        string view = Strip(File.ReadAllText(Path.Combine(RepositoryRoot.Find(), Web, "Pages", "Shared", "_OriginLabel.cshtml")));

        Assert.DoesNotMatch(@"\s(?:hidden|style|on[a-z]+)\b", view);
        Assert.Matches("<img [^>]*\\balt=\"[^\"]+\"", view);
        Assert.Single(Regex.Matches(view, "<img\\b", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));
    }
}
