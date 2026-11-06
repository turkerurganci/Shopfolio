using System.Reflection;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The search engine data pattern's rules on the application source and on the application's types (T219 KK2, KK6, KK7; 09 9.4 items 2 and 3; 09 19.2 item 3; 05 3.7):
/// the three users of the common folding function reach it, no second folding or address pattern exists, no type has a search engine field or a typed address, the
/// sitemap and robots names stay in the reserved list, and the robots marks are written only by the pattern. Each rule goes red on a deliberate violation sample.
/// </summary>
public sealed partial class SearchEngineSourceScanTests
{
    private const string SamplesFolder = "Architecture/ViolationSamples/SearchEngine";

    private static string Sample(string file) => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", SamplesFolder, file));

    [GeneratedRegex(@"^(?:Meta(?:Title|Description|Keywords)|Seo[A-Z_]\w*|Og(?:Image|Title|Description|Url)|Canonical\w*|RobotsMeta\w*|Slug)$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SearchEngineFieldName();

    [Fact]
    public void ApplicationSource_HasOneFolding_OneAddressPattern_NoSearchEngineField_NoProtocolFileName_AndTheRobotsMarksOnlyInThePattern()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (!SearchEngineSourceScan.Applies(relative))
            {
                continue;
            }

            scanned.Add(relative);
            foreach ((string rule, int line) in SearchEngineSourceScan.Scan(relative, File.ReadAllText(file)))
            {
                findings.Add($"{relative}:{line} {rule}");
            }
        }

        Assert.Contains(SearchEngineSourceScan.NormalizerFile, scanned);
        Assert.Contains(SearchEngineSourceScan.SlugFile, scanned);
        Assert.Contains(SearchEngineSourceScan.AddressRulesFile, scanned);
        Assert.Contains(scanned, f => f.StartsWith(SearchEngineSourceScan.SearchEngineFolder, StringComparison.Ordinal));
        Assert.Contains(SearchEngineSourceScan.RobotsExceptions[0], scanned);
        Assert.Empty(findings);
    }

    [Fact]
    public void TheThreeUsersOfTheFolding_AddressSearchTermAndSearchedName_CallTheCommonFunction()
    {
        string root = RepositoryRoot.Find();

        foreach (string user in SearchEngineSourceScan.SimplifyUsers)
        {
            Assert.True(SearchEngineSourceScan.CallsSimplify(File.ReadAllText(Path.Combine(root, user))), user);
        }
    }

    [Fact]
    public void TheCommonFolding_IsTheSameResultForTheAddressTheSearchTermAndTheStockCodeSuggestion()
    {
        string[] names = ["Çiçekli Şal", "ÇELİK KAPI", "Ağaç Masa", "Iğdır Işığı", "Café Crème", "ıİiI"];

        foreach (string name in names)
        {
            string folded = TextNormalizer.Simplify(name);
            string address = Slug.From(name, AddressRules.ProductPrefix);

            // The address is the folded name with every run of other characters made one hyphen.
            Assert.Equal(Regex.Replace(folded, "[^a-z0-9]+", "-").Trim('-'), address);
            // The search word reaches the form the searched column has: the same folded text between the wildcards.
            Assert.Equal("%" + folded + "%", SearchTerm.ToLikePattern(name));
            // The searched column of the product is written with the same function.
            Assert.Equal(folded, Shopfolio.Modules.Catalog.Product.Create(name, "x", Shopfolio.Modules.Catalog.Contracts.ProductType.Physical, 20m, DateTimeOffset.UnixEpoch).NameSearchForm);
            // The suggested stock code starts from the address.
            Assert.StartsWith(address[..Math.Min(address.Length, StockCodeSuggestion.AddressLength)].TrimEnd('-').ToUpperInvariant(), StockCodeSuggestion.Suggest(address, 1, []), StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ViolationSample_Code_EachRuleReportsItsLines_AndNotTheCommentsOrTheNeighbours()
    {
        IReadOnlyList<(string Rule, int Line)> findings = SearchEngineSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Samples/SearchEngineMisuse.cs", Sample("SearchEngineMisuse.cs"));

        Assert.Equal(
            [
                (SearchEngineSourceScan.FoldOutsideNormalizer, 6),
                (SearchEngineSourceScan.FoldOutsideNormalizer, 8),
                (SearchEngineSourceScan.FoldOutsideNormalizer, 10),
                (SearchEngineSourceScan.AddressPatternOutsideSlug, 12),
                (SearchEngineSourceScan.AddressPatternOutsideSlug, 14),
                (SearchEngineSourceScan.FoldOutsideNormalizer, 18),
                (SearchEngineSourceScan.SearchEngineField, 23),
                (SearchEngineSourceScan.SearchEngineField, 25),
                (SearchEngineSourceScan.SearchEngineField, 27),
                (SearchEngineSourceScan.SearchEngineField, 29),
                (SearchEngineSourceScan.SearchEngineField, 31),
                (SearchEngineSourceScan.SearchEngineField, 33),
                (SearchEngineSourceScan.ProtocolFileOutsideRules, 43),
                (SearchEngineSourceScan.ProtocolFileOutsideRules, 44),
                (SearchEngineSourceScan.RobotsOutsideSearchEngine, 47),
                (SearchEngineSourceScan.RobotsOutsideSearchEngine, 49),
                (SearchEngineSourceScan.RobotsOutsideSearchEngine, 51),
                (SearchEngineSourceScan.ProtocolFileOutsideRules, 53),
            ],
            findings);
    }

    [Fact]
    public void ViolationSample_Razor_ASearchEngineFieldATypedAddressAndTheRobotsMarkAreReported_AndTheRazorCommentIsNot()
    {
        IReadOnlyList<(string Rule, int Line)> findings = SearchEngineSourceScan.Scan("src/Shopfolio.Web/Pages/Panel/Sample.cshtml", Sample("SeoFields.cshtml"));

        Assert.Equal(
            [
                (SearchEngineSourceScan.SearchEngineField, 3),
                (SearchEngineSourceScan.SearchEngineField, 4),
                (SearchEngineSourceScan.SlugTyped, 5),
                (SearchEngineSourceScan.RobotsOutsideSearchEngine, 7),
            ],
            findings);
    }

    [Fact]
    public void TheRulesExceptions_AreOneFileEach_AndTheSamePatternInsideTheirFileIsNotReported()
    {
        Assert.Empty(SearchEngineSourceScan.Scan(SearchEngineSourceScan.NormalizerFile, "var f = s.Normalize(NormalizationForm.FormD);"));
        Assert.Empty(SearchEngineSourceScan.Scan(SearchEngineSourceScan.SlugFile, "var r = new Regex(\"[^a-z0-9]+\");"));
        Assert.Empty(SearchEngineSourceScan.Scan(SearchEngineSourceScan.AddressRulesFile, "string[] f = [\"sitemap.xml\", \"robots.txt\"];"));
        Assert.Empty(SearchEngineSourceScan.Scan(SearchEngineSourceScan.SearchEngineFolder + "X.cs", "r.Headers[\"X-Robots-Tag\"] = \"noindex\";"));
        Assert.Empty(SearchEngineSourceScan.Scan(SearchEngineSourceScan.RobotsExceptions[0], "r.Headers[\"X-Robots-Tag\"] = \"noindex\";"));
        Assert.Single(SearchEngineSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Forms/Other.cs", "r.Headers[\"X-Robots-Tag\"] = \"noindex\";"));
    }

    [Fact]
    public void TheRobotsException_IsNotAFolder_SoANewFileNextToItIsReported()
    {
        Assert.Single(SearchEngineSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Forms/CsvFileResult2.cs", "r.Headers.Append(\"x-robots-tag\", \"noindex\");"));
    }

    [Fact]
    public void TheFoldingRule_SeesEveryCallFormOfTheDecomposition()
    {
        string[] forms =
        [
            "s.Normalize(NormalizationForm.FormD)",
            "s.Normalize( System.Text.NormalizationForm.FormKD )",
            "s.Normalize\n(\n)",
            "var f = NormalizationForm.FormC;",
            "string.Compare(a, b, CultureInfo.InvariantCulture, CompareOptions.IgnoreNonSpace)",
            "x.Replace(\"ğ\", \"g\")",
            "x.Replace('İ', 'i')",
        ];

        foreach (string form in forms)
        {
            Assert.Contains(SearchEngineSourceScan.Scan("src/Shopfolio.Web/Modules/Catalog/X.cs", form), f => f.Rule == SearchEngineSourceScan.FoldOutsideNormalizer);
        }
    }

    [Fact]
    public void TheSearchEngineFieldRule_SeesIdentifiersColumnsAndFormFieldsInEveryNamingStyle_AndNotOrdinaryWords()
    {
        string[] hits = ["MetaTitle", "MetaDescription", "SeoSlug", "Seo_Title", "OgImage", "CanonicalPath", "\"meta_title\"", "\"seo-title\"", "\"og_image\"", "\"canonical_url\""];
        string[] clean = ["Seoul", "Metadata", "Meta", "OpenGraphType", "Keywords", "Description", "Montage", "Organization", "Organic"];

        foreach (string hit in hits)
        {
            Assert.Contains(SearchEngineSourceScan.Scan("src/Shopfolio.Web/Modules/Catalog/X.cs", $"var x = {hit};"), f => f.Rule == SearchEngineSourceScan.SearchEngineField);
        }

        foreach (string word in clean)
        {
            Assert.DoesNotContain(SearchEngineSourceScan.Scan("src/Shopfolio.Web/Modules/Catalog/X.cs", $"var x = {word};"), f => f.Rule == SearchEngineSourceScan.SearchEngineField);
        }
    }

    [Fact]
    public void ApplicationTypes_HaveNoSearchEngineFieldAndNoFormInputHoldsAnAddress()
    {
        const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        Assembly application = typeof(Program).Assembly;
        List<string> members = [];
        foreach (Type type in application.GetTypes())
        {
            foreach (string name in type.GetMembers(all).Where(m => m.MemberType is MemberTypes.Property or MemberTypes.Field).Select(m => m.Name))
            {
                bool slugOfARecord = string.Equals(name, "Slug", StringComparison.Ordinal);
                bool seo = SearchEngineFieldName().IsMatch(name) && !slugOfARecord;
                bool slugOnAnInput = slugOfARecord && typeof(IFormInput).IsAssignableFrom(type);
                if (seo || slugOnAnInput)
                {
                    members.Add($"{type.FullName}.{name}");
                }
            }
        }

        Assert.Empty(members);
    }

    [Fact]
    public void TheApplicationsEndpoints_HaveNoSitemapAndNoRobotsFileYet_TheNamesStayReserved()
    {
        Assert.Equal(["sitemap.xml", "robots.txt"], AddressRules.ProtocolFiles);
        Assert.All(AddressRules.ProtocolFiles, name => Assert.True(AddressRules.IsClosedFirstSegment(name)));
    }
}
