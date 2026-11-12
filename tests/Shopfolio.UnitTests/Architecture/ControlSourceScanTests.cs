using System.Text.RegularExpressions;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The source rules of the control components on the source (T544 KK3, KK4, KK6, KK7, KK8, KK10; 04 2.12.5-2.12.8; 09 3.4, 16.1): the customer's side uploads nothing, the confirmation boxes name no consent
/// the site does not ask, no limit is a number typed in a component, the coupon code is compared by the one normalising rule, the date component reads Turkey time from <c>TurkeyTime</c> only, and no box
/// comes ticked. Each rule goes red on a deliberate violation and stays quiet on the allowed near miss.
/// </summary>
public sealed class ControlSourceScanTests
{
    private const string Web = "src/Shopfolio.Web";

    private static readonly TimeSpan PatternTimeout = TimeSpan.FromSeconds(5);

    /// <summary>The upload component's own files: the only places under Pages that may name a file input (04 2.12.8.6).</summary>
    private static readonly string[] UploadComponentFiles =
    [
        "Pages/Components/UploadComponentModels.cs",
        "Pages/Shared/_GalleryUpload.cshtml",
        "Pages/Shared/_SingleUpload.cshtml",
        "Pages/Shared/_UploadStatus.cshtml",
    ];

    private static readonly string[] ConsentFiles =
    [
        "Pages/Components/ConsentModels.cs",
        "Pages/Shared/_ConsentBoxes.cshtml",
    ];

    private static readonly string[] ControlFiles =
    [
        "Pages/Components/DateModels.cs",
        "Pages/Components/CouponModels.cs",
        "Pages/Components/ConsentModels.cs",
        "Pages/Components/UploadComponentModels.cs",
        "Pages/Shared/_CouponField.cshtml",
        "Pages/Shared/_ConsentBoxes.cshtml",
        "Pages/Shared/_GalleryUpload.cshtml",
        "Pages/Shared/_SingleUpload.cshtml",
        "Pages/Shared/_UploadStatus.cshtml",
    ];

    private static string WebRoot() => Path.Combine(RepositoryRoot.Find(), Web);

    private static string ReadWeb(string relative) => File.ReadAllText(Path.Combine(WebRoot(), relative));

    /// <summary>The code of a file without its comments (C# line, XML and block comments; Razor and HTML comments): a comment may say what a component does not do.</summary>
    private static string Code(string text)
    {
        string result = Regex.Replace(text, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline | RegexOptions.CultureInvariant, PatternTimeout);
        result = Regex.Replace(result, @"@\*.*?\*@", string.Empty, RegexOptions.Singleline | RegexOptions.CultureInvariant, PatternTimeout);
        result = Regex.Replace(result, @"<!--.*?-->", string.Empty, RegexOptions.Singleline | RegexOptions.CultureInvariant, PatternTimeout);
        return Regex.Replace(result, @"(?<!:)//.*", string.Empty, RegexOptions.CultureInvariant, PatternTimeout);
    }

    private static string CodeOf(string relative) => Code(ReadWeb(relative));

    // ---- KK10: the customer's side uploads nothing ----

    private static List<string> UploadSurfaceHits(string source) =>
        [.. Regex.Matches(source, @"(type\s*=\s*""file""|\bIFormFile\w*|multipart/form-data|enctype\s*=)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, PatternTimeout).Select(m => m.Value)];

    [Fact]
    public void NoPageOutsideTheUploadComponent_NamesAFileInputOrAMultipartForm_KK10()
    {
        string pages = Path.Combine(WebRoot(), "Pages");
        HashSet<string> allowed = [.. UploadComponentFiles.Select(f => Path.GetFullPath(Path.Combine(WebRoot(), f)))];
        List<string> offenders = [];
        foreach (string file in Directory.EnumerateFiles(pages, "*.*", SearchOption.AllDirectories).Where(f => f.EndsWith(".cshtml", StringComparison.Ordinal) || f.EndsWith(".cs", StringComparison.Ordinal)))
        {
            if (!allowed.Contains(Path.GetFullPath(file)) && UploadSurfaceHits(Code(File.ReadAllText(file))).Count > 0)
            {
                offenders.Add(Path.GetRelativePath(pages, file));
            }
        }

        Assert.Empty(offenders);
    }

    [Fact]
    public void TheUploadComponent_StillDrawsTheFileInput_SoTheScanIsNotBlind_KK10()
    {
        Assert.NotEmpty(UploadSurfaceHits(CodeOf("Pages/Shared/_SingleUpload.cshtml")));
        Assert.NotEmpty(UploadSurfaceHits(CodeOf("Pages/Shared/_GalleryUpload.cshtml")));
    }

    [Theory]
    [InlineData("<input type=\"file\" name=\"Attachment\">")]
    [InlineData("<input type = \"file\">")]
    [InlineData("public IFormFile Attachment { get; set; }")]
    [InlineData("public IFormFileCollection Files { get; set; }")]
    [InlineData("<form method=\"post\" enctype=\"multipart/form-data\">")]
    public void AFileInputOnAPage_IsFound_KK10(string source)
    {
        Assert.NotEmpty(UploadSurfaceHits(source));
    }

    [Theory]
    [InlineData("<input type=\"text\" name=\"Subject\">")]
    [InlineData("<form method=\"post\" asp-page-handler=\"ApplyCoupon\">")]
    [InlineData("// the contact form takes no attachment (02 3.32.2)")]
    public void APageWithNoFileInput_IsQuiet_KK10(string source)
    {
        Assert.Empty(UploadSurfaceHits(Code(source)));
    }

    // ---- KK8: no consent the site does not ask ----

    private static List<string> ForbiddenConsents(string source) =>
        [.. Regex.Matches(source, @"(Kvkk|Marketing|Pazarlama|Newsletter|Bülten|\bAge\w*|Yaş|Birth|Doğum)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, PatternTimeout).Select(m => m.Value)];

    [Fact]
    public void TheConsentBoxes_HaveNoKvkkNoAgeAndNoMarketingBox_KK8()
    {
        foreach (string file in ConsentFiles)
        {
            Assert.Empty(ForbiddenConsents(CodeOf(file)));
        }
    }

    [Theory]
    [InlineData("new ConsentBoxView(\"MarketingConsent\", \"Kampanyalardan haberdar olmak istiyorum\")")]
    [InlineData("<input type=\"checkbox\" name=\"KvkkAccepted\">")]
    [InlineData("<label>18 yaşından büyüğüm</label>")]
    [InlineData("public bool AgeConfirmed { get; set; }")]
    [InlineData("<label>Pazarlama izni</label>")]
    public void AnExtraConsentBox_IsFound_KK8(string source)
    {
        Assert.NotEmpty(ForbiddenConsents(source));
    }

    [Theory]
    [InlineData("public string MessageId => Id + \"-message\";")]
    [InlineData("<input type=\"checkbox\" name=\"DistanceSalesAccepted\">")]
    public void TheTwoFixedBoxesAndTheMessage_AreQuiet_KK8(string source)
    {
        Assert.Empty(ForbiddenConsents(source));
    }

    // ---- KK7: no box comes ticked ----

    private static List<string> BareChecked(string source) =>
        [.. Regex.Matches(source, @"\schecked(?!\s*=\s*""@)(\s|=|>|/)", RegexOptions.CultureInvariant, PatternTimeout).Select(m => m.Value)];

    [Fact]
    public void TheConsentView_TicksNoBoxOnItsOwn_KK7()
    {
        string view = CodeOf("Pages/Shared/_ConsentBoxes.cshtml");

        Assert.Empty(BareChecked(view));
        Assert.Contains("checked=\"@box.IsTicked\"", view, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("<input type=\"checkbox\" name=\"X\" checked>")]
    [InlineData("<input type=\"checkbox\" name=\"X\" checked />")]
    [InlineData("<input type=\"checkbox\" name=\"X\" checked=\"checked\">")]
    [InlineData("<input type=\"checkbox\" name=\"X\" checked=\"true\">")]
    public void ABoxThatIsAlwaysTicked_IsFound_KK7(string source)
    {
        Assert.NotEmpty(BareChecked(source));
    }

    [Fact]
    public void ABoxTickedFromTheModel_IsQuiet_KK7()
    {
        Assert.Empty(BareChecked("<input type=\"checkbox\" name=\"X\" checked=\"@box.IsTicked\">"));
    }

    // ---- KK10, KK1: no limit is a number typed in a component ----

    private static List<string> TypedLimits(string source) =>
        [.. Regex.Matches(source, @"(maxlength=""\d|\d+\s*MB\b|\d+\s*görsel|[1-9]\d*L\s*\*\s*1024|[1-9]\d*\s*\*\s*1024\s*\*\s*1024)", RegexOptions.CultureInvariant, PatternTimeout).Select(m => m.Value)];

    [Fact]
    public void TheControlComponents_TypeNoLimit_KK10()
    {
        foreach (string file in ControlFiles)
        {
            Assert.Empty(TypedLimits(CodeOf(file)));
        }
    }

    [Theory]
    [InlineData("<input maxlength=\"150\">")]
    [InlineData("return \"Dosya en çok 10 MB olabilir.\";")]
    [InlineData("return \"En çok 20 görsel yüklenebilir.\";")]
    [InlineData("private const long Limit = 10L * 1024 * 1024;")]
    [InlineData("private const long Limit = 10 * 1024 * 1024;")]
    [InlineData("<textarea maxlength=\"500\"></textarea>")]
    public void ATypedLimit_IsFound_KK10(string source)
    {
        Assert.NotEmpty(TypedLimits(source));
    }

    [Theory]
    [InlineData("private const long BytesPerMegabyte = 1 << 20;")]
    [InlineData("<input maxlength=\"@Model.MaxLength\" min=\"@Model.Min\">")]
    [InlineData("return $\"En çok {UploadLimits.MaxCount(surface)} görsel yüklenebilir.\";")]
    public void ALimitFromTheConstants_IsQuiet_KK10(string source)
    {
        Assert.Empty(TypedLimits(source));
    }

    // ---- KK4, KK6: the coupon code is compared by the one normalising rule ----

    private static List<string> OwnCaseRules(string source) =>
        [.. Regex.Matches(source, @"(\.ToLower\w*\(|\.ToUpper\w*\(|OrdinalIgnoreCase|InvariantCultureIgnoreCase|CurrentCultureIgnoreCase|\.Normalize\()", RegexOptions.CultureInvariant, PatternTimeout).Select(m => m.Value)];

    [Fact]
    public void TheCouponComponent_HasNoCaseRuleOfItsOwn_AndUsesTheIdentityNormaliser_KK4()
    {
        string models = CodeOf("Pages/Components/CouponModels.cs");

        Assert.Empty(OwnCaseRules(models));
        Assert.Empty(OwnCaseRules(CodeOf("Pages/Shared/_CouponField.cshtml")));
        Assert.Contains("TextNormalizer.NormalizeIdentity(", models, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("return a.ToLower() == b.ToLower();")]
    [InlineData("return a.ToUpperInvariant() == b.ToUpperInvariant();")]
    [InlineData("return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);")]
    [InlineData("return string.Equals(a, b, StringComparison.InvariantCultureIgnoreCase);")]
    public void ACouponComparedByItsOwnCaseRule_IsFound_KK4(string source)
    {
        Assert.NotEmpty(OwnCaseRules(source));
    }

    [Fact]
    public void ACouponComparedByTheNormaliser_IsQuiet_KK4()
    {
        Assert.Empty(OwnCaseRules("return TextNormalizer.NormalizeIdentity(first) == TextNormalizer.NormalizeIdentity(second);"));
    }

    // ---- KK1, KK3: Turkey time comes from TurkeyTime only ----

    private static List<string> OwnClockRules(string source) =>
        [.. Regex.Matches(source, @"(DateTime(Offset)?\.(Now|UtcNow|Today)|TimeZoneInfo|\.ToLocalTime\(|\.ToUniversalTime\(|\.AddDays\(\s*1\s*\)|\.AddHours\()", RegexOptions.CultureInvariant, PatternTimeout).Select(m => m.Value)];

    [Fact]
    public void TheDateComponent_ReadsTurkeyTimeFromTurkeyTimeOnly_KK1()
    {
        string models = CodeOf("Pages/Components/DateModels.cs");

        Assert.Empty(OwnClockRules(models));
        Assert.Contains("TurkeyTime.", models, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("DateOnly today = DateOnly.FromDateTime(DateTime.Now);")]
    [InlineData("DateTime now = DateTime.UtcNow;")]
    [InlineData("TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(\"Turkey Standard Time\");")]
    [InlineData("DateTime local = instant.ToLocalTime();")]
    [InlineData("DateTime end = day.AddDays(1);")]
    public void ADateComponentWithItsOwnClock_IsFound_KK3(string source)
    {
        Assert.NotEmpty(OwnClockRules(source));
    }

    [Theory]
    [InlineData("return TurkeyTime.EndOfRange(end);")]
    [InlineData("DateOnly today = TurkeyTime.TodayOf(now);")]
    public void ADateComponentThatAsksTurkeyTime_IsQuiet_KK3(string source)
    {
        Assert.Empty(OwnClockRules(source));
    }
}
