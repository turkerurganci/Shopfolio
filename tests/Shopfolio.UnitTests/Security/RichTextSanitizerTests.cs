using System.Text.Json;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Security;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Security;

/// <summary>
/// The closed text format set's cleaning (T339 KK8; 09 15.3 items 2-5, 7; 06 2.10; 02 3.11.6): an attack set cleaned of every tag, attribute and dangerous scheme, the five
/// formats and the two carriers passing unchanged, relative, schemeless and network-path links falling while their text stays, every link carrying its <c>rel</c>, the second cleaning
/// giving the first one's output, and the allow list being a superset of the snapshot file (it only grows, 06 2.10.2).
/// </summary>
public sealed class RichTextSanitizerTests
{
    private static readonly Regex AnyTag = new("<(?!/?(?:strong|em|ul|li|a|h3|p|br)\\b)[a-zA-Z!/?]", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));

    private static readonly string[] AttackTexts =
    [
        "<script>alert(1)</script>",
        "<SCRIPT SRC=//evil.example/x.js></SCRIPT>",
        "<iframe src=\"https://evil.example\"></iframe>",
        "<img src=x onerror=alert(1)>",
        "<svg onload=alert(1)><circle/></svg>",
        "<style>p{background:url(javascript:alert(1))}</style>",
        "<p style=\"color:red\" onclick=\"alert(1)\">metin</p>",
        "<a href=\"javascript:alert(1)\">tıkla</a>",
        "<a href=\"JaVaScRiPt:alert(1)\">tıkla</a>",
        "<a href=\"&#106;avascript:alert(1)\">tıkla</a>",
        "<a href=\"java\nscript:alert(1)\">tıkla</a>",
        "<a href=\"java\tscript:alert(1)\">tıkla</a>",
        "<a href=\" javascript:alert(1)\">tıkla</a>",
        "<a href=\"data:text/html,<script>alert(1)</script>\">tıkla</a>",
        "<a href=\"vbscript:msgbox(1)\">tıkla</a>",
        "<a href=\"x\" target=\"_blank\" class=\"y\" id=\"z\">tıkla</a>",
        "<p><b onmouseover=alert(1)>kalın",
        "<<script>alert(1)//<</script>",
        "<div><span>iç içe <em>kapanmamış",
        "<math><annotation-xml encoding=\"text/html\"><img src=x onerror=alert(1)></annotation-xml></math>",
        "<object data=\"x\"></object><embed src=\"x\">",
        "<!-- yorum --><p>metin</p>",
        "<form action=\"https://evil.example\"><input name=a></form>",
    ];

    public static TheoryData<string> Attacks => [.. AttackTexts];

    public static TheoryData<string> LinksThatFall =>
    [
        "/x", "x.com", "//evil.example", "//evil.example/yol", "../yukari", "?a=1", "#bolum", "ftp://sunucu.example/x", "file:///c:/x", "tel:+905550000000",
    ];

    [Theory]
    [MemberData(nameof(Attacks))]
    public void AnAttack_LeavesNoTagNoAttributeAndNoDangerousScheme(string attack)
    {
        string cleaned = RichTextSanitizer.Sanitize(attack);

        Assert.DoesNotMatch(AnyTag, cleaned);
        string lower = cleaned.ToLowerInvariant();
        Assert.DoesNotContain("javascript:", lower, StringComparison.Ordinal);
        Assert.DoesNotContain("vbscript:", lower, StringComparison.Ordinal);
        Assert.DoesNotContain("data:", lower, StringComparison.Ordinal);
        Assert.DoesNotContain("onerror", lower, StringComparison.Ordinal);
        Assert.DoesNotContain("onload", lower, StringComparison.Ordinal);
        Assert.DoesNotContain("onclick", lower, StringComparison.Ordinal);
        Assert.DoesNotContain("style", lower, StringComparison.Ordinal);
        Assert.DoesNotContain("class=", lower, StringComparison.Ordinal);
        Assert.DoesNotContain("target", lower, StringComparison.Ordinal);
        Assert.DoesNotContain("alert(1)", lower, StringComparison.Ordinal);
        Assert.Equal(cleaned, RichTextSanitizer.Sanitize(cleaned));
    }

    [Fact]
    public void TheFiveFormatsTheParagraphAndTheLineBreak_PassUnchanged()
    {
        const string text = "<h3>Başlık</h3><p><strong>kalın</strong> ve <em>italik</em><br>ikinci satır</p><ul><li>bir</li><li>iki</li></ul>";

        Assert.Equal(text, RichTextSanitizer.Sanitize(text));
    }

    [Fact]
    public void AFormatOutsideTheSet_Falls_AndItsTextStays()
    {
        string cleaned = RichTextSanitizer.Sanitize("<h1>Büyük</h1><h2>Orta</h2><b>kalın</b> <i>eğik</i> <u>çizgili</u> <font color=\"red\">renkli</font> <span class=\"x\">aralık</span><div>blok</div>");

        Assert.Equal("BüyükOrtakalın eğik çizgili renkli aralıkblok", cleaned);
    }

    [Fact]
    public void ParagraphsAndLineBreaks_AreTheCarriersOfTheText_AndAreKept()
    {
        const string text = "<p>ilk paragraf</p><p>ikinci<br>satır sonu<br>üçüncü satır</p>";

        string cleaned = RichTextSanitizer.Sanitize(text);

        Assert.Equal(text, cleaned);
        Assert.Equal(2, Regex.Count(cleaned, "<p>", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));
        Assert.Equal(2, Regex.Count(cleaned, "<br>", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public void ABareLineFeedInPlainText_IsKept_SoAPlainTextKeepsItsLines()
    {
        string cleaned = RichTextSanitizer.Sanitize("birinci satır\nikinci satır");

        Assert.Equal("birinci satır\nikinci satır", cleaned);
    }

    [Theory]
    [InlineData("https://ornek.example/yol?a=1&b=2")]
    [InlineData("http://ornek.example")]
    [InlineData("mailto:ad@ornek.example")]
    public void AnAbsoluteHttpHttpsOrMailtoLink_Stays_WithItsRel(string href)
    {
        string cleaned = RichTextSanitizer.Sanitize($"<a href=\"{href}\">bağlantı</a>");

        Assert.Contains("href=\"", cleaned, StringComparison.Ordinal);
        Assert.Contains("rel=\"noopener noreferrer\"", cleaned, StringComparison.Ordinal);
        Assert.EndsWith(">bağlantı</a>", cleaned, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(LinksThatFall))]
    public void ARelativeSchemelessNetworkPathOrOtherSchemeLink_LosesItsAddress_AndTheTextStaysInTheTag(string href)
    {
        string cleaned = RichTextSanitizer.Sanitize($"<a href=\"{href}\">bağlantı</a>");

        Assert.DoesNotContain("href", cleaned, StringComparison.Ordinal);
        Assert.Contains(">bağlantı</a>", cleaned, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryLink_CarriesTheRel_AndAGivenRelOrTargetIsNotKept()
    {
        string cleaned = RichTextSanitizer.Sanitize("<a href=\"https://a.example\" rel=\"opener\" target=\"_blank\">bir</a> <a href=\"mailto:x@y.example\">iki</a> <a>üç</a>");

        Assert.Equal(3, Regex.Count(cleaned, "rel=\"noopener noreferrer\"", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));
        Assert.DoesNotContain("opener\"", cleaned.Replace("noopener noreferrer", string.Empty, StringComparison.Ordinal), StringComparison.Ordinal);
        Assert.DoesNotContain("target", cleaned, StringComparison.Ordinal);
    }

    [Fact]
    public void HrefOnAnotherTag_IsNotKept()
    {
        string cleaned = RichTextSanitizer.Sanitize("<p href=\"https://a.example\">metin</p><li href=\"https://b.example\">madde</li>");

        Assert.DoesNotContain("href", cleaned, StringComparison.Ordinal);
        Assert.Contains("metin", cleaned, StringComparison.Ordinal);
    }

    [Fact]
    public void CodeAndEmbeddedDocuments_FallWithTheirContent_ButTheTextAroundThemStays()
    {
        string cleaned = RichTextSanitizer.Sanitize("önce<script>kod()</script>sonra<style>p{}</style><iframe>içerik</iframe> son");

        Assert.Equal("öncesonra son", cleaned);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(" \n\t ")]
    public void NullEmptyAndWhiteSpaceOnly_AreTheEmptyText(string? input)
    {
        Assert.Equal(string.Empty, RichTextSanitizer.Sanitize(input));
    }

    [Fact]
    public void ATextThatIsOnlyCode_BecomesTheEmptyText()
    {
        Assert.Equal(string.Empty, RichTextSanitizer.Sanitize("<script>alert(1)</script>"));
    }

    [Fact]
    public void TheSecondCleaning_GivesTheFirstOnesOutput_ForTheWholeAttackSetAndTheFormats()
    {
        foreach (string text in AttackTexts.Concat(
            ["<p>a &amp; b &lt;c&gt;</p>", "<ul><li><a href=\"https://x.example\">y</a></li></ul>", "ç ğ ı ö ş ü İ"]))
        {
            string first = RichTextSanitizer.Sanitize(text);
            Assert.Equal(first, RichTextSanitizer.Sanitize(first));
        }
    }

    [Fact]
    public void ATextWithMarkupCharactersAsText_KeepsThemEscaped()
    {
        Assert.Equal("<p>a &amp; b &lt;c&gt;</p>", RichTextSanitizer.Sanitize("<p>a &amp; b &lt;c&gt;</p>"));
    }

    [Fact]
    public async Task TwoThreadsCleaningAtOnce_GiveTheSameResultAsOne()
    {
        const string text = "<p onclick=\"x\"><strong>kalın</strong> <a href=\"https://a.example\">bağ</a></p><script>x</script>";
        string expected = RichTextSanitizer.Sanitize(text);

        string[] results = await Task.WhenAll(Enumerable.Range(0, 16).Select(_ => Task.Run(() =>
        {
            string last = string.Empty;
            for (int i = 0; i < 50; i++)
            {
                last = RichTextSanitizer.Sanitize(text);
            }

            return last;
        }, TestContext.Current.CancellationToken)));

        Assert.All(results, r => Assert.Equal(expected, r));
    }

    [Fact]
    public void TheDisplay_FiltersTheStoredValueAgain_AndDoesNotRewriteIt()
    {
        const string stored = "<p>metin</p><script>x()</script><img src=x onerror=alert(1)>";

        string shown = RichText.Render(stored).ToString()!;

        Assert.Equal("<p>metin</p>", shown);
        Assert.Equal(string.Empty, RichText.Render(null).ToString());
    }

    [Fact]
    public void TheAllowList_IsASupersetOfTheSnapshotFile_SoItOnlyGrows()
    {
        string path = Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Security", "rich-text-allowlist.json");
        using JsonDocument snapshot = JsonDocument.Parse(File.ReadAllText(path));

        static HashSet<string> Read(JsonElement root, string name) => [.. root.GetProperty(name).EnumerateArray().Select(e => e.GetString()!)];

        Assert.Subset(RichTextSanitizer.AllowedTags.ToHashSet(StringComparer.Ordinal), Read(snapshot.RootElement, "tags"));
        Assert.Subset(RichTextSanitizer.AllowedAttributes.ToHashSet(StringComparer.Ordinal), Read(snapshot.RootElement, "attributes"));
        Assert.Subset(RichTextSanitizer.AllowedSchemes.ToHashSet(StringComparer.Ordinal), Read(snapshot.RootElement, "schemes"));
    }

    [Fact]
    public void TheAllowListToday_IsExactlyTheDocumentsSet()
    {
        Assert.Equal(["a", "br", "em", "h3", "li", "p", "strong", "ul"], RichTextSanitizer.AllowedTags.Order(StringComparer.Ordinal));
        Assert.Equal(["href"], RichTextSanitizer.AllowedAttributes);
        Assert.Equal(["http", "https", "mailto"], RichTextSanitizer.AllowedSchemes.Order(StringComparer.Ordinal));
    }
}
